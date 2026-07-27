# Plano: Fechamento Mensal Financeiro

## Resumo
Criar sistema de meses financeiros: cada mês tem seu próprio salário, receitas e despesas são associadas ao mês aberto, meses fechados ficam congelados, e o usuário pode ver relatório mensal com exportação PDF.

---

## Fase 1 — Backend: Domain Entity

### 1.1 Nova entidade `FinancialMonth`
**Arquivo:** `backend/src/FleetOS.Domain/Finance/FinancialMonth.cs`

```csharp
public enum MonthStatus { Open = 1, Closed = 2 }

public sealed class FinancialMonth : AggregateRoot
{
    public int Year { get; private set; }
    public int MonthNumber { get; private set; }
    public decimal OwnerSalary { get; private set; }
    public MonthStatus Status { get; private set; }
    public DateTimeOffset OpenedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }

    // FK relationships
    private readonly List<FinancialTransaction> _transactions = [];
    public IReadOnlyList<FinancialTransaction> Transactions => _transactions.AsReadOnly();

    private FinancialMonth() { } // EF Core

    private FinancialMonth(Guid id, Guid tenantId, Guid orgId, Guid buId,
        int year, int month, decimal ownerSalary)
        : base(id, tenantId, orgId, buId)
    {
        Year = year;
        MonthNumber = month;
        OwnerSalary = ownerSalary;
        Status = MonthStatus.Open;
        OpenedAt = DateTimeOffset.UtcNow;
    }

    public static FinancialMonth Open(Guid tenantId, Guid orgId, Guid buId,
        int year, int month, decimal ownerSalary)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(month, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(month, 12);
        ArgumentOutOfRangeException.ThrowIfNegative(ownerSalary);

        return new FinancialMonth(Guid.NewGuid(), tenantId, orgId, buId,
            year, month, ownerSalary);
    }

    public void Close()
    {
        if (Status == MonthStatus.Closed)
            throw new InvalidOperationException("Month already closed");
        Status = MonthStatus.Closed;
        ClosedAt = DateTimeOffset.UtcNow;
    }
}
```

### 1.2 Atualizar `FinancialTransaction`
**Arquivo:** `backend/src/FleetOS.Domain/Finance/FinancialTransaction.cs`

Adicionar campo:
```csharp
public Guid FinancialMonthId { get; private set; }
```

Atualizar factory `Create()` para aceitar `financialMonthId`.

### 1.3 Manter `Tenant.OwnerSalary` como fallback/default
Ao abrir novo mês, pré-preenche com o valor do Tenant. Não remover.

---

## Fase 2 — Backend: EF Configuration

### 2.1 Nova config `FinancialMonthConfiguration`
**Arquivo:** `backend/src/FleetOS.Infrastructure/Persistence/Configurations/FinanceConfigurations.cs`

Adicionar:
```csharp
public sealed class FinancialMonthConfiguration : IEntityTypeConfiguration<FinancialMonth>
{
    public void Configure(EntityTypeBuilder<FinancialMonth> builder)
    {
        builder.ToTable("FinancialMonths");
        builder.HasKey(fm => fm.Id);
        builder.Property(fm => fm.Year).IsRequired();
        builder.Property(fm => fm.MonthNumber).IsRequired();
        builder.Property(fm => fm.OwnerSalary).HasPrecision(18, 2).IsRequired();
        builder.Property(fm => fm.Status).HasConversion<string>().IsRequired();
        builder.Property(fm => fm.OpenedAt).IsRequired();
        builder.Property(fm => fm.ClosedAt);

        // Query filter: soft delete + tenant
        builder.HasQueryFilter(fm => fm.DeletedAt == null && fm.TenantId == currentTenantId);

        // FK: FinancialMonth → FinancialTransactions
        builder.HasMany(fm => fm.Transactions)
               .WithOne()
               .HasForeignKey(ft => ft.FinancialMonthId)
               .OnDelete(DeleteBehavior.Restrict);

        // Unique: only one month per (tenant, year, month)
        builder.HasIndex(fm => new { fm.TenantId, fm.Year, fm.MonthNumber }).IsUnique();
    }
}
```

### 2.2 Atualizar `FinancialTransactionConfiguration`
Adicionar FK:
```csharp
builder.Property(ft => ft.FinancialMonthId).IsRequired();
// FK já configurada acima, mas pode adicionar índice
builder.HasIndex(ft => ft.FinancialMonthId);
```

### 2.3 Migration
```
dotnet ef migrations add AddFinancialMonth
```

---

## Fase 3 — Backend: Repository

### 3.1 Nova interface `IFinancialMonthRepository`
**Arquivo:** `backend/src/FleetOS.Application/Common/Interfaces/IFinanceRepositories.cs`

```csharp
public interface IFinancialMonthRepository
{
    Task<FinancialMonthDto?> GetCurrentOpenMonthAsync(Guid tenantId);
    Task<List<FinancialMonthDto>> GetAllMonthsAsync(Guid tenantId);
    Task<FinancialMonthDto?> GetByIdAsync(Guid id);
    Task<FinancialMonthReportDto> GetMonthReportAsync(Guid monthId);
    Task<bool> HasOpenMonthAsync(Guid tenantId);
}
```

### 3.2 Implementação `FinancialMonthRepository`
**Arquivo:** `backend/src/FleetOS.Infrastructure/Persistence/Repositories/FinanceRepositories.cs`

Métodos:
- `GetCurrentOpenMonthAsync`: retorna o mês com Status=Open mais recente
- `GetAllMonthsAsync`: lista todos os meses ordenados por ano/mês desc
- `GetByIdAsync`: busca por Id
- `GetMonthReportAsync`: calcula receitas, despesas, saldo, combustível do mês
- `HasOpenMonthAsync`: verifica se já existe mês aberto

### 3.3 Atualizar `IFinancialTransactionRepository`
Adicionar filtro por `FinancialMonthId` nos métodos de listagem e summary.

---

## Fase 4 — Backend: DTOs e Commands

### 4.1 Novos DTOs
**Arquivo:** `backend/src/FleetOS.Application/Finance/FinanceDtos.cs`

```csharp
public record FinancialMonthDto(
    Guid Id, int Year, int MonthNumber, decimal OwnerSalary,
    string Status, DateTimeOffset OpenedAt, DateTimeOffset? ClosedAt);

public record FinancialMonthReportDto(
    FinancialMonthDto Month,
    decimal OwnerSalary,
    decimal OwnerTaxAmount,
    decimal NetOwnerSalary,
    decimal TotalRevenues,
    decimal TotalExpenses,
    decimal FuelCost,
    decimal NetBalance,
    List<FinancialTransactionDto> Transactions);

public record OpenMonthCommand(int Year, int Month, decimal OwnerSalary);
```

### 4.2 Commands/Queries novos
**OpenMonth:** `POST /finance/months` → `OpenMonthCommand` → handler cria FinancialMonth
**CloseMonth:** `POST /finance/months/{id}/close` → `CloseMonthCommand` → handler fecha o mês
**GetMonths:** `GET /finance/months` → retorna lista
**GetMonthReport:** `GET /finance/months/{id}/report` → retorna `FinancialMonthReportDto`

---

## Fase 5 — Backend: Controllers

### 5.1 Novo controller `MonthsController`
**Arquivo:** `backend/src/FleetOS.Api/Features/Finance/MonthsController.cs`

```csharp
[Route("api/v1/finance/months")]
[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
public sealed class MonthsController : BaseController
{
    // GET /api/v1/finance/months
    [HttpGet]
    public async Task<ActionResult<List<FinancialMonthDto>>> GetAll() { ... }

    // POST /api/v1/finance/months
    [HttpPost]
    public async Task<ActionResult<FinancialMonthDto>> Open(OpenMonthCommand cmd) { ... }

    // POST /api/v1/finance/months/{id}/close
    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult> Close(Guid id) { ... }

    // GET /api/v1/finance/months/{id}/report
    [HttpGet("{id:guid}/report")]
    public async Task<ActionResult<FinancialMonthReportDto>> GetReport(Guid id) { ... }
}
```

### 5.2 Atualizar `TransactionsController`
- `POST /finance/transactions` → auto-associar ao mês aberto atual
- `GET /finance/transactions` → filtrar pelo mês aberto atual
- `GET /finance/transactions/summary` → aceitar `monthId` como parâmetro opcional

---

## Fase 6 — Backend: DI

### 6.1 Registrar repositório
**Arquivo:** `backend/src/FleetOS.Infrastructure/InfrastructureServiceExtensions.cs`

Adicionar:
```csharp
services.AddScoped<IFinancialMonthRepository, FinancialMonthRepository>();
```

---

## Fase 7 — Frontend: Dependências

### 7.1 Instalar pacotes PDF
```bash
npm install jspdf html2canvas
```

---

## Fase 8 — Frontend: Componentes

### 8.1 Seletor de Mês
**Arquivo:** `frontend/src/pages/finances/components/MonthSelector.tsx`

- Dropdown com todos os meses (label: "Mês Referência: Janeiro/2026")
- Indicador visual: aberto (verde) / fechado (cinza)
- Ao selecionar mês fechado: tela read-only do relatório + botão PDF
- Ao selecionar mês aberto: tela normal (transactions, etc)

### 8.2 Modal "Abrir Novo Mês"
**Arquivo:** `frontend/src/pages/finances/components/OpenMonthModal.tsx`

- Inputs: Mês (select 1-12), Ano (input), Salário do Proprietário
- Botão "Abrir Mês"
- Validação: não pode abrir mês que já existe

### 8.3 Modal "Fechar Mês"
**Arquivo:** `frontend/src/pages/finances/components/CloseMonthModal.tsx`

- Confirmação: "Tem certeza? Após fechar, não será possível adicionar transações."
- Botão "Fechar Mês"

### 8.4 Relatório Mensal com PDF
**Arquivo:** `frontend/src/pages/finances/components/MonthlyReport.tsx`

- Tabela: Receitas, Despesas, Combustível
- KPI Cards: Salário Líquido, Total Receitas, Total Despesas, Saldo Final
- Botão "Baixar PDF" → gera PDF com html2canvas + jspdf:
  - Captura o relatório como imagem
  - Gera PDF A4 com nome "relatorio-mes-{mes}-{ano}.pdf"
  - Faz download automático

### 8.5 Atualizar `FinancesPage.tsx`
- Adicionar `MonthSelector` no topo
- Ao abrir página: verificar se existe mês aberto. Se não, mostrar modal "Abrir Novo Mês"
- Se mês fechado selecionado: mostrar `MonthlyReport` (read-only)
- Se mês aberto: mostrar tabs normais (Dashboard, Transações, etc) filtradas pelo mês

### 8.6 Atualizar `CashFlowDashboard.tsx`
- Filtrar summary pelo mês atual (passar `monthId` no GET)
- Mudar auto-refresh para não recarregar meses fechados

### 8.7 Atualizar `TransactionsList.tsx`
- Filtrar transações pelo mês atual
- Se mês fechado: esconder botões de ação (Pay, Cancel, Delete)

---

## Fase 9 — Seed/Migration

### 9.1 Migração dos dados existentes
Na primeira execução após migration, o `DbInitializer` deve:
1. Verificar se existe FinancialMonth
2. Se não existir: criar um mês retroativamente para Janeiro/2026 (ou mês atual)
3. Associar todas as transações existentes a esse mês
4. Usar `Tenant.OwnerSalary` como salário do mês

### 9.2 Atualizar DbInitializer
Após criar o tenant, criar também o FinancialMonth inicial.

---

## Ordem de execução

1. Domain: `FinancialMonth.cs` + atualizar `FinancialTransaction.cs`
2. EF: Configurações + migration
3. Repository: Interface + implementação
4. DTOs e Commands
5. Controllers
6. DI registration
7. Frontend: npm install jspdf html2canvas
8. Frontend: MonthSelector
9. Frontend: OpenMonthModal
10. Frontend: CloseMonthModal
11. Frontend: MonthlyReport + PDF
12. Frontend: Atualizar FinancesPage, CashFlowDashboard, TransactionsList
13. DbInitializer: seed do mês inicial
14. Commit + push
