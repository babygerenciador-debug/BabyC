# BusinessUnit.md

> Projeto: FleetOS – Enterprise Fleet Management Platform
>
> Domínio: Core
>
> Aggregate Root: BusinessUnit
>
> Versão: 1.0.0 (MVP)
>
> Status: Especificação Oficial

---

# 1. Objetivo

A Business Unit representa a menor unidade organizacional operacional do FleetOS.

Ela define onde as operações realmente acontecem.

Toda movimentação operacional pertence obrigatoriamente a uma Business Unit.

Ela representa:

- Filial
- Garagem
- Centro Operacional
- Oficina
- Centro de Distribuição
- Escritório Regional

Dependendo do negócio do cliente.

---

# 2. Papel no Domínio

A Business Unit possui as seguintes responsabilidades:

- organizar operações
- controlar veículos
- controlar motoristas
- controlar viagens
- controlar estoque
- controlar financeiro
- controlar usuários locais
- controlar indicadores

É o principal ponto de segregação operacional dentro de uma Organization.

---

# 3. Aggregate

```
BusinessUnit
│
├── Drivers
├── Vehicles
├── Trips
├── Products
├── FinancialEntries
├── Users
├── Schedules
├── Checklists
└── Documents
```

---

# 4. Responsabilidades

A Business Unit é responsável por:

- armazenar informações operacionais
- identificar a unidade
- controlar recursos locais
- organizar equipes
- agrupar ativos
- centralizar indicadores locais

---

# 5. Estados

```
Creating
      │
      ▼
Active
      │
      ├────────────► Suspended
      │                   │
      ▼                   ▼
Archived
```

## Creating

Unidade sendo criada.

Ainda não possui operações.

---

## Active

Pode operar normalmente.

---

## Suspended

Operações bloqueadas.

Histórico preservado.

---

## Archived

Somente leitura.

Não recebe novas operações.

---

# 6. Lifecycle

```
Create

↓

Configure

↓

Activate

↓

Active

↓

Suspend

↓

Archive
```

---

# 7. Invariantes

## INV-BU-001

Toda Business Unit pertence exatamente a uma Organization.

Relacionada:

BR-0003

---

## INV-BU-002

Toda Business Unit pertence exatamente a um Tenant.

---

## INV-BU-003

Toda operação operacional pertence a uma Business Unit.

---

## INV-BU-004

Business Unit nunca poderá ser removida fisicamente.

Relacionada:

BR-0005

---

## INV-BU-005

Uma Business Unit arquivada não poderá receber novos registros.

---

# 8. Atributos

## Identificação

| Campo | Tipo |
|---------|------|
| Id | UUID |
| TenantId | UUID |
| OrganizationId | UUID |
| Name | string |
| Code | string |
| Status | enum |

---

## Endereço

| Campo | Tipo |
|---------|------|
| ZipCode | string |
| Street | string |
| Number | string |
| District | string |
| City | string |
| State | string |
| Country | string |

---

## Contato

| Campo | Tipo |
|---------|------|
| Email | string |
| Phone | string |

---

## Configuração

| Campo | Tipo |
|---------|------|
| IsHeadOffice | boolean |
| TimeZone | string |

---

## Auditoria

| Campo | Tipo |
|---------|------|
| CreatedAt | datetime |
| UpdatedAt | datetime |
| DeletedAt | datetime |

---

# 9. Value Objects

A Business Unit utilizará:

- BusinessUnitName
- BusinessUnitCode
- Address
- Email
- Phone
- TimeZone

Todos deverão ser imutáveis.

---

# 10. Entidades Relacionadas

## Driver

Relacionamento:

1:N

---

## Vehicle

Relacionamento:

1:N

---

## Trip

Relacionamento:

1:N

---

## Product

Relacionamento:

1:N

---

## FinancialEntry

Relacionamento:

1:N

---

## User

Relacionamento:

N:N

Um usuário poderá atuar em uma ou mais Business Units, conforme suas permissões.

---

# 11. Métodos do Domínio

O Aggregate deverá expor apenas comportamentos.

```
Rename()

Activate()

Suspend()

Archive()

UpdateAddress()

UpdateContact()

AssignUser()

RemoveUser()

TransferVehicle()

TransferDriver()
```

Nenhum atributo poderá ser alterado diretamente.

---

# 12. Eventos de Domínio

```
BusinessUnitCreated

BusinessUnitUpdated

BusinessUnitActivated

BusinessUnitSuspended

BusinessUnitArchived

UserAssigned

VehicleTransferred

DriverTransferred
```

---

# 13. Casos de Uso

- Criar Business Unit
- Atualizar Business Unit
- Suspender Business Unit
- Arquivar Business Unit
- Vincular usuário
- Transferir veículo
- Transferir motorista

---

# 14. Regras de Negócio

Relacionadas:

- BR-0003
- BR-0005
- BR-0006

Regras específicas:

BU-001 — Toda operação operacional deve estar vinculada a uma Business Unit.

BU-002 — O código da Business Unit deve ser único dentro da Organization.

BU-003 — Não é permitido arquivar uma Business Unit que possua viagens em andamento.

BU-004 — Não é permitido remover uma Business Unit com recursos ativos.

BU-005 — Uma Business Unit poderá ser marcada como matriz (`IsHeadOffice = true`), mas apenas uma por Organization.

---

# 15. Permissões

```
BusinessUnit.Read

BusinessUnit.Create

BusinessUnit.Update

BusinessUnit.Suspend

BusinessUnit.Archive

BusinessUnit.AssignUser

BusinessUnit.TransferAssets
```

---

# 16. Banco de Dados

Tabela principal

```
BusinessUnits
```

Relacionamentos

```
Tenant

↓

Organization

↓

BusinessUnit

├── Users

├── Drivers

├── Vehicles

├── Trips

├── Inventory

├── Finance
```

Todas as tabelas operacionais possuirão:

- TenantId
- OrganizationId
- BusinessUnitId

Essas três chaves formam o contexto organizacional da operação.

---

# 17. API

Endpoints previstos

```
GET    /api/v1/business-units

GET    /api/v1/business-units/{id}

POST   /api/v1/business-units

PUT    /api/v1/business-units/{id}

PATCH  /api/v1/business-units/{id}/activate

PATCH  /api/v1/business-units/{id}/suspend

PATCH  /api/v1/business-units/{id}/archive

POST   /api/v1/business-units/{id}/assign-user

POST   /api/v1/business-units/{id}/transfer-vehicle

POST   /api/v1/business-units/{id}/transfer-driver
```

---

# 18. Frontend

O módulo deverá possuir:

- Listagem
- Cadastro
- Edição
- Configuração
- Gestão de usuários
- Transferência de recursos
- Visualização de indicadores locais

Componentes:

- DataTable
- Formulário
- Modal de confirmação
- Cards de indicadores
- Filtros
- Paginação

---

# 19. Auditoria

Toda alteração deverá registrar:

- TenantId
- OrganizationId
- BusinessUnitId
- UserId
- Operação
- Data/Hora
- Valores alterados
- CorrelationId

---

# 20. Segurança

Toda requisição deverá validar:

- TenantId
- OrganizationId
- BusinessUnitId
- Usuário ativo
- RBAC
- Status da Business Unit

O frontend nunca poderá definir esses identificadores.

O contexto deverá ser resolvido pelo backend a partir da autenticação e da unidade selecionada pelo usuário.

---

# 21. Performance

Índices obrigatórios:

- Id
- TenantId
- OrganizationId
- Code
- Status

Todas as consultas deverão utilizar paginação.

Filtros serão executados diretamente no banco.

---

# 22. Testes Obrigatórios

## Unitários

- Criar Business Unit
- Alterar nome
- Alterar endereço
- Suspender
- Arquivar
- Vincular usuário

## Integração

- Criar unidade
- Garantir isolamento por Tenant
- Garantir isolamento por Organization
- Validar código duplicado
- Transferir ativos entre unidades

## Segurança

- Bloquear acesso entre Tenants
- Bloquear acesso entre Organizations
- Validar RBAC
- Validar contexto da Business Unit

---

# 23. Evolução Pós-MVP

O Aggregate foi projetado para suportar:

- hierarquia entre unidades
- regiões operacionais
- clusters logísticos
- centros de custo por unidade
- dashboards independentes
- metas por unidade
- aprovação de operações
- gestão de equipes
- multi-armazém
- integração com geolocalização

Essas funcionalidades não fazem parte do MVP, mas a modelagem foi preparada para suportá-las sem alterações estruturais.

---

# 24. Definition of Done

A Business Unit será considerada concluída quando possuir:

- documentação completa
- entidade de domínio
- configuração EF Core
- migration
- repository
- DTOs
- Commands
- Queries
- Validators
- Controllers
- Endpoints
- testes unitários
- testes de integração
- auditoria
- documentação sincronizada

---

# 25. Princípio Final

A Business Unit é o centro operacional do FleetOS.

Enquanto o Tenant representa o cliente da plataforma e a Organization representa a empresa jurídica, a Business Unit representa o local onde o negócio acontece.

Todos os módulos operacionais (Frota, Operações, Estoque, Financeiro e Dashboard) deverão executar suas atividades dentro do contexto de uma Business Unit, garantindo organização, escalabilidade, isolamento de dados e governança em ambientes multiempresa.