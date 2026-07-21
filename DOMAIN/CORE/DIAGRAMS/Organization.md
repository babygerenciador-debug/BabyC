# Organization.md

> Projeto: FleetOS – Enterprise Fleet Management Platform
>
> Domínio: Core
>
> Aggregate Root: Organization
>
> Versão: 1.0.0 (MVP)
>
> Status: Especificação Oficial

---

# 1. Objetivo

A Organization representa uma empresa (Pessoa Jurídica) pertencente a um Tenant.

É a entidade responsável por agrupar todas as operações administrativas e operacionais relacionadas a um CNPJ.

Toda operação do FleetOS pertence obrigatoriamente a uma Organization.

---

# 2. Papel no Domínio

A Organization é responsável por:

- representar uma empresa jurídica
- armazenar dados cadastrais
- controlar filiais (Business Units)
- controlar veículos
- controlar motoristas
- controlar viagens
- controlar estoque
- controlar financeiro
- controlar documentos
- centralizar indicadores

Uma Organization nunca existe sem um Tenant.

---

# 3. Aggregate

```
Organization
│
├── BusinessUnits
├── Drivers
├── Vehicles
├── Trips
├── Products
├── FinancialEntries
├── Documents
└── Settings
```

---

# 4. Responsabilidades

O Aggregate Organization possui as seguintes responsabilidades:

- Identificar a empresa.
- Armazenar dados fiscais.
- Gerenciar filiais.
- Centralizar operações.
- Definir configurações operacionais.
- Controlar documentos da empresa.

---

# 5. Estados

Uma Organization poderá assumir os seguintes estados.

```
Creating
    │
    ▼
Active
    │
    ├────────► Suspended
    │               │
    ▼               ▼
Archived
```

## Creating

Empresa sendo cadastrada.

Ainda não possui operações.

---

## Active

Empresa operacional.

Pode utilizar todos os módulos disponíveis.

---

## Suspended

Empresa temporariamente bloqueada.

Não poderá criar novas operações.

Os dados permanecem preservados.

---

## Archived

Empresa arquivada.

Disponível apenas para consulta histórica.

---

# 6. Lifecycle

```
Create
   │
Configure
   │
Activate
   │
Active
   │
Suspend
   │
Archive
```

---

# 7. Invariantes

## INV-ORG-001

Toda Organization pertence exatamente a um Tenant.

Relacionada:

BR-0004

---

## INV-ORG-002

Toda Organization possui pelo menos uma Business Unit.

No MVP, essa Business Unit será criada automaticamente.

---

## INV-ORG-003

O CNPJ deve ser único dentro do Tenant.

---

## INV-ORG-004

Organization nunca poderá ser removida fisicamente.

Relacionada:

BR-0005

---

# 8. Atributos

## Identificação

| Campo | Tipo |
|---------|------|
| Id | UUID |
| TenantId | UUID |
| Name | string |
| TradeName | string |
| Slug | string |
| Status | enum |

---

## Dados Fiscais

| Campo | Tipo |
|---------|------|
| CNPJ | string |
| StateRegistration | string |
| MunicipalRegistration | string |

---

## Contato

| Campo | Tipo |
|---------|------|
| Email | string |
| Phone | string |
| Website | string |

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

## Auditoria

| Campo | Tipo |
|---------|------|
| CreatedAt | datetime |
| UpdatedAt | datetime |
| DeletedAt | datetime |

---

# 9. Value Objects

A Organization utilizará os seguintes Value Objects.

- OrganizationName
- TradeName
- CNPJ
- Email
- Phone
- Address
- ZipCode
- Website

Todos deverão ser imutáveis.

---

# 10. Entidades Internas

## BusinessUnit

Relacionamento:

1:N

Representa as filiais da empresa.

---

## Driver

Relacionamento:

1:N

Motoristas pertencentes à empresa.

---

## Vehicle

Relacionamento:

1:N

Veículos cadastrados.

---

## Product

Relacionamento:

1:N

Produtos do estoque.

---

## FinancialEntry

Relacionamento:

1:N

Lançamentos financeiros.

---

## Trip

Relacionamento:

1:N

Viagens realizadas.

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

CreateBusinessUnit()

DisableBusinessUnit()

UpdateFiscalData()
```

Nenhum atributo poderá ser alterado diretamente.

---

# 12. Eventos de Domínio

O Aggregate poderá emitir:

```
OrganizationCreated

OrganizationUpdated

OrganizationActivated

OrganizationSuspended

OrganizationArchived

BusinessUnitCreated
```

---

# 13. Casos de Uso

A Organization participa dos seguintes casos de uso.

- Criar empresa
- Atualizar empresa
- Alterar endereço
- Alterar contato
- Gerenciar filiais
- Arquivar empresa

---

# 14. Regras de Negócio

Relacionadas:

- BR-0003
- BR-0004
- BR-0005
- BR-0006

Regras específicas:

ORG-001 — O CNPJ deve ser válido conforme algoritmo oficial.

ORG-002 — Não é permitido cadastrar duas Organizations com o mesmo CNPJ dentro do mesmo Tenant.

ORG-003 — Toda Organization deve possuir ao menos uma Business Unit.

ORG-004 — Uma Organization arquivada não poderá receber novas operações.

---

# 15. Permissões

```
Organization.Read

Organization.Create

Organization.Update

Organization.Suspend

Organization.Archive
```

No MVP, apenas usuários administradores da empresa poderão gerenciar Organizations.

---

# 16. Banco de Dados

Tabela principal:

```
Organizations
```

Relacionamentos:

```
Tenant
│
└── Organization
      │
      ├── BusinessUnits
      ├── Drivers
      ├── Vehicles
      ├── Trips
      ├── Products
      └── FinancialEntries
```

Toda consulta deverá respeitar:

- TenantId
- Soft Delete
- Auditoria

---

# 17. API

Endpoints previstos.

```
GET    /api/v1/organizations

GET    /api/v1/organizations/{id}

POST   /api/v1/organizations

PUT    /api/v1/organizations/{id}

PATCH  /api/v1/organizations/{id}/activate

PATCH  /api/v1/organizations/{id}/suspend

PATCH  /api/v1/organizations/{id}/archive
```

---

# 18. Frontend

O módulo deverá possuir:

- Listagem
- Cadastro
- Edição
- Visualização
- Configuração
- Endereço
- Dados fiscais
- Contatos

Componentes:

- Tabela
- Formulário
- Modal de confirmação
- Upload de logo (evolução futura)

---

# 19. Auditoria

Toda alteração deverá registrar:

- Usuário
- Tenant
- Organization
- Data/Hora
- Operação
- Valores anteriores
- Valores novos
- CorrelationId

---

# 20. Segurança

Toda consulta deverá validar:

- TenantId
- Permissões RBAC
- Status da Organization

O frontend nunca poderá enviar TenantId manualmente.

OrganizationId deverá ser validado no backend.

---

# 21. Performance

Índices obrigatórios:

- Id
- TenantId
- CNPJ
- Slug
- Status

Filtros deverão ser executados no banco.

Listagens obrigatoriamente paginadas.

---

# 22. Testes Obrigatórios

## Unitários

- Criar Organization
- Alterar nome
- Alterar endereço
- Alterar contato
- Suspender
- Arquivar

---

## Integração

- Criar Business Unit automaticamente
- Garantir isolamento por Tenant
- Validar CNPJ duplicado
- Validar RBAC

---

## Segurança

- Bloquear acesso entre Tenants
- Bloquear criação sem Tenant
- Bloquear alteração de TenantId

---

# 23. Evolução Pós-MVP

O Aggregate foi projetado para suportar:

- múltiplos CNPJs por Tenant
- empresas coligadas
- grupos empresariais
- múltiplos regimes tributários
- múltiplos centros administrativos
- branding por Organization
- integrações fiscais futuras
- emissão de documentos fiscais

Essas funcionalidades não fazem parte do MVP, mas a arquitetura está preparada para suportá-las.

---

# 24. Definition of Done

O Aggregate será considerado concluído quando possuir:

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

A Organization representa a unidade jurídica da operação do cliente dentro do FleetOS.

Ela centraliza os recursos operacionais da empresa e organiza o domínio de forma independente do Tenant, permitindo que um único cliente gerencie múltiplas empresas de maneira segura, escalável e totalmente isolada.

Todo módulo operacional (Frota, Operações, Estoque, Financeiro e Dashboard) deverá estar vinculado a uma Organization e respeitar as regras de isolamento definidas pelo Tenant.