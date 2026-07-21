# Sistema de Gestão de Frota - Baby Turismo

## Funcionalidades Implementadas

### 1. Gestão de Viagens com Pagamento Integrado

#### Criação de Viagem
- Cadastro completo: motorista, veículo, origem, destino, datas
- **Valor da viagem** (campo obrigatório)
- **Status de pagamento**: Pendente ou Pago
- Observações opcionais

#### Fluxo de Pagamento
- **Viagem Pendente**: pode ser marcada como paga a qualquer momento
- **Viagem Paga**: cria automaticamente transação financeira (Revenue)
- **Atualização em tempo real**: dashboard financeiro atualiza automaticamente

#### Operações de Viagem
- **Iniciar viagem**: muda status para "Em Andamento"
- **Concluir viagem**: exige checklist, muda status para "Concluído"
- **Trocar veículo**: admin pode trocar o veículo de uma viagem ativa
- **Cancelar viagem**: cancela viagem agendada ou em andamento
- **Marcar como pago**: botão disponível apenas para viagens pendentes com valor > 0

### 2. Gestão Financeira

#### Transações Automáticas
- Ao pagar uma viagem, o sistema cria automaticamente:
  - Transação do tipo **Revenue** (Receita)
  - Descrição: "Viagem: {Origem} → {Destino}"
  - Valor: valor da viagem
  - Categoria: "Viagem" (Revenue)

#### Dashboard Financeiro
- **Receitas do mês**: soma de todas as transações Revenue
- **Despesas do mês**: soma de todas as transações Expense
- **Saldo do mês**: Receitas - Despesas
- **Veículos**: total e disponíveis
- **Viagens**: total do mês e em andamento

#### Categorias
- **Viagem** (Revenue): usada automaticamente para receitas de viagens
- **Combustível** (Expense): para despesas com combustível
- Personalizável: admin pode criar novas categorias

#### Contas a Pagar
- Interface para lançar despesas manuais
- Apenas tipo **Expense** (saídas)
- Receitas são geradas automaticamente pelo pagamento de viagens

### 3. Gestão de Frota

#### Veículos
- Cadastro: placa, apelido, marca, cor, capacidade, ano
- Status: Disponível, Em Viagem, Em Manutenção, Fora de Serviço
- Documentos: RENAVAM, ANTT, ARTESP, Seguro, Licenciamento
- Alertas de vencimento de documentos
- Alertas de abastecimento

#### Motoristas
- Cadastro: nome, email, CPF (últimos 4 dígitos)
- CNH: número, categoria, data de vencimento
- Validação automática de CNH vencida
- Associação com User (role: Driver)

### 4. Manutenção
- Registro de manutenções preventivas e corretivas
- Associação com veículo
- Custo, data, descrição, fornecedor
- URL da nota fiscal (opcional)

### 5. Abastecimento
- Registro de abastecimentos
- Associação com veículo e motorista (opcional)
- Quilometragem, litros, custo total
- URL do recibo (opcional)
- Observações

### 6. Gestão de Estoque
- Produtos com categoria
- Movimentações: Entrada, Saída, Transferência
- Saldo de estoque por localização (Principal ou Veículo)
- Estoque mínimo para alertas

### 7. Dashboard

#### KPIs em Tempo Real
- Total de veículos e disponíveis
- Viagens do mês e em andamento
- Estoque baixo
- Receitas, despesas e saldo do mês

#### Atualização Automática
- WebSocket via SignalR
- Qualquer mudança no sistema atualiza o dashboard automaticamente
- Sem necessidade de recarregar a página

## Arquitetura Técnica

### Backend (.NET 10)
- **Clean Architecture** com 4 camadas:
  - Domain: entidades de domínio e regras de negócio
  - Application: comandos, queries, handlers, validadores
  - Infrastructure: EF Core, repositórios, services
  - API: controllers, endpoints REST
- **CQRS** (Command Query Responsibility Segregation)
- **SignalR** para real-time
- **PostgreSQL** como banco de dados
- **Redis** para cache

### Frontend (React + TypeScript)
- **Vite** como bundler
- **TanStack Query** para gerenciamento de estado do servidor
- **React Hook Form** + **Zod** para formulários e validação
- **Lucide React** para ícones
- **CSS Modules** para estilização

### Banco de Dados
- **PostgreSQL 16** (Supabase)
- Migrations automatizadas via EF Core
- Soft delete (DeletedAt) em todas as entidades
- Auditoria: CreatedAt, CreatedBy, UpdatedAt, UpdatedBy

## Testes Realizados

### Fluxo Completo de Viagem
1. ✅ Criar viagem com valor R$ 1500 e status Pending
2. ✅ Dashboard antes de pagar: R$ 7800,00
3. ✅ Marcar viagem como paga (PATCH /trips/{id}/pay)
4. ✅ Transação financeira criada automaticamente
5. ✅ Dashboard após pagar: R$ 9300,00 (+R$ 1500)

### Operações de Viagem
- ✅ Iniciar viagem (Created → InProgress)
- ✅ Concluir viagem com checklist (InProgress → Completed)
- ✅ Trocar veículo de viagem ativa
- ✅ Cancelar viagem

### Integração Financeira
- ✅ Categorias criadas automaticamente
- ✅ Transações listadas corretamente
- ✅ Cálculo de saldo (Receitas - Despesas)
- ✅ Atualização em tempo real via SignalR

## Status do Sistema

### Containers
- ✅ API: healthy (porta 5000)
- ✅ Frontend: running
- ✅ Database: healthy (PostgreSQL 16)
- ✅ Redis: healthy
- ✅ Nginx: running (porta 80)

### Métricas Atuais
- **Veículos**: 2 (2 disponíveis)
- **Motoristas**: 1
- **Viagens**: 6 (0 em andamento)
- **Receitas**: R$ 9.300,00
- **Despesas**: R$ 800,00
- **Saldo**: R$ 8.500,00

## Acesso

- **Frontend**: http://localhost
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger

### Credenciais Padrão
- **Email**: admin@fleetos.io
- **Senha**: Admin@123456

## Próximos Passos (Opcionais)

1. **Autenticação de Motoristas**: portal separado para motoristas verem suas viagens
2. **Checklist Digital**: checklist estruturado com perguntas específicas
3. **Relatórios**: exportação de dados em PDF/Excel
4. **Notificações**: alertas de vencimento de documentos, manutenção, etc
5. **Geolocalização**: rastreamento de veículos em tempo real
6. **Integração com APIs externas**: consulta de placas, CNH, etc

## Comandos Úteis

### Docker
```bash
# Iniciar containers
docker compose up -d

# Ver logs
docker compose logs -f api

# Reconstruir
docker compose up -d --build

# Parar
docker compose down
```

### Database
```bash
# Acessar PostgreSQL
docker compose exec db psql -U postgres -d fleetos

# Ver tabelas
docker compose exec db psql -U postgres -d fleetos -c "\dt"

# Ver migrations aplicadas
docker compose exec db psql -U postgres -d fleetos -c "SELECT * FROM \"__EFMigrationsHistory\";"
```

### Testes Manuais
```bash
# Login
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"identifier":"admin@fleetos.io","password":"Admin@123456"}'

# Listar viagens
curl http://localhost:5000/api/v1/trips \
  -H "Authorization: Bearer {TOKEN}"

# Pagar viagem
curl -X PATCH http://localhost:5000/api/v1/trips/{ID}/pay \
  -H "Authorization: Bearer {TOKEN}"
```

---

**Última atualização**: 2026-07-21
**Versão**: 1.0.0
**Status**: ✅ Produção Ready
