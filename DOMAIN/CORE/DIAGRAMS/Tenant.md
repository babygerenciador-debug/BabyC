# Tenant.md

> Projeto: FleetOS – Enterprise Fleet Management Platform
>
> Domínio: Core
>
> Aggregate Root: Tenant
>
> Versão: 1.0.0 (MVP)
>
> Status: Especificação Oficial

---

# 1. Objetivo

O Tenant representa o cliente da plataforma FleetOS.

Todo dado existente no sistema pertence obrigatoriamente a um único Tenant.

O Tenant é o maior limite de isolamento da plataforma.

Nenhuma informação poderá ser compartilhada entre Tenants, exceto funcionalidades globais da plataforma administradas pelo Super Admin.

O Tenant é considerado o Aggregate Root principal do domínio Core.

---

# 2. Papel no Domínio

O Tenant é responsável por:

- controlar o ambiente do cliente
- definir configurações globais
- controlar usuários
- controlar organizações
- controlar limites do plano
- controlar recursos habilitados
- controlar armazenamento
- controlar auditoria
- controlar identidade visual
- controlar configurações regionais

Todos os demais Aggregates pertencem direta ou indiretamente a um Tenant.

---

# 3. Aggregate Root

```
Tenant
│
├── Organizations
├── Users
├── Roles
├── Settings
├── AuditLogs
├── FileStorage
├── Subscription
├── FeatureFlags
└── UsageMetrics
```

No MVP apenas parte dessas entidades será implementada.

As demais já fazem parte da arquitetura para garantir evolução futura.

---

# 4. Responsabilidades

O Aggregate Tenant possui as seguintes responsabilidades:

- Isolar dados.
- Armazenar informações cadastrais.
- Definir idioma.
- Definir timezone.
- Definir moeda.
- Definir configurações globais.
- Definir tema.
- Definir logo.
- Definir plano.
- Definir recursos disponíveis.
- Definir limites de utilização.
- Centralizar auditoria.

---

# 5. Estados

Um Tenant poderá assumir apenas um dos seguintes estados.

```
Provisioning
      │
      ▼
Active
      │
      ├──────────────► Suspended
      │                    │
      │                    ▼
      └──────────────► Archived
```

---

## Provisioning

Estado utilizado durante a criação do Tenant.

Ainda não possui usuários ativos.

Ainda não possui dados operacionais.

---

## Active

Tenant totalmente funcional.

Pode utilizar todos os módulos disponíveis no plano.

---

## Suspended

Tenant temporariamente bloqueado.

Características:

- login permitido apenas para administradores
- operações bloqueadas
- APIs retornam erro de acesso
- dados preservados

---

## Archived

Tenant descontinuado.

Características:

- somente leitura
- sem login operacional
- preservado para histórico

---

# 6. Lifecycle

```
CreateTenant
        │
Provisioning
        │
Configure
        │
Activate
        │
Active
        │
Suspend
        │
Suspended
        │
Archive
        ▼
Archived
```

---

# 7. Invariantes

As seguintes regras nunca poderão ser violadas.

## INV-001

Todo Tenant possui um identificador único.

---

## INV-002

Todo registro operacional pertence a um Tenant.

Relacionada:

BR-0001

---

## INV-003

Tenant nunca poderá ser removido fisicamente.

Relacionada:

BR-0005

---

## INV-004

Tenant sempre possuirá pelo menos um administrador.

Relacionada:

BR-0204

---

## INV-005

Tenant nunca poderá acessar dados de outro Tenant.

Relacionada:

BR-0002

---

# 8. Atributos

## Identificação

| Campo | Tipo |
|---------|------|
| Id | UUID |
| Name | string |
| Slug | string |
| Status | enum |
| CreatedAt | datetime |
| UpdatedAt | datetime |

---

## Configuração

| Campo | Tipo |
|---------|------|
| TimeZone | string |
| Language | string |
| Currency | string |
| DateFormat | string |

---

## Identidade Visual

| Campo | Tipo |
|---------|------|
| LogoUrl | string |
| PrimaryColor | string |
| SecondaryColor | string |

---

## Plano

| Campo | Tipo |
|---------|------|
| PlanName | string |
| MaxUsers | integer |
| MaxOrganizations | integer |
| MaxBusinessUnits | integer |
| MaxStorageMB | integer |

No MVP esses limites serão apenas informativos.

A validação poderá ser ativada futuramente.

---

# 9. Value Objects

O Aggregate utilizará os seguintes Value Objects.

```
TenantName

Slug

TimeZone

Language

Currency

Color

Logo

StorageLimit
```

Todos deverão ser imutáveis.

---

# 10. Entidades Internas

## Organization

Representa empresas pertencentes ao Tenant.

Relacionamento:

1:N

---

## User

Usuários autenticados.

Relacionamento:

1:N

---

## Role

Perfis de acesso.

Relacionamento:

1:N

---

## Setting

Configurações.

Relacionamento:

1:N

---

## AuditLog

Histórico de alterações.

Relacionamento:

1:N

---

## FileStorage

Metadados de arquivos.

Relacionamento:

1:N

---

# 11. Métodos do Domínio

O Aggregate deverá expor apenas comportamentos de negócio.

```
Activate()

Suspend()

Archive()

UpdateProfile()

UpdateTheme()

UpdateLanguage()

UpdateTimeZone()

AddOrganization()

RemoveOrganization()

UpdateSubscription()

EnableFeature()

DisableFeature()
```

Nenhum atributo deverá ser alterado diretamente.

---

# 12. Eventos de Domínio

O Aggregate poderá publicar os seguintes eventos.

```
TenantCreated

TenantActivated

TenantSuspended

TenantArchived

TenantUpdated

OrganizationAdded

FeatureEnabled

FeatureDisabled
```

Esses eventos serão utilizados futuramente para integração assíncrona.

---

# 13. Casos de Uso

O Aggregate participa dos seguintes casos de uso.

- Criar Tenant
- Atualizar Tenant
- Ativar Tenant
- Suspender Tenant
- Arquivar Tenant
- Atualizar Configurações
- Atualizar Identidade Visual
- Gerenciar Recursos

---

# 14. Regras de Negócio

Aplicáveis:

- BR-0001
- BR-0002
- BR-0003
- BR-0004
- BR-0005
- BR-0006
- BR-0204

---

# 15. Permissões

```
Tenant.Read

Tenant.Create

Tenant.Update

Tenant.Suspend

Tenant.Archive
```

Essas permissões serão atribuídas apenas a administradores da plataforma.

No MVP, apenas o Super Admin poderá criar novos Tenants.

---

# 16. Banco de Dados

Tabela:

```
Tenants
```

Relacionamentos:

```
Tenant

↓

Organizations

↓

BusinessUnits

↓

Users

↓

Fleet

↓

Trips

↓

Inventory

↓

Finance
```

Todas as tabelas conterão:

```
TenantId
```

Como chave de isolamento lógico.

---

# 17. API

Endpoints previstos.

```
GET /api/v1/tenants

GET /api/v1/tenants/{id}

POST /api/v1/tenants

PUT /api/v1/tenants/{id}

PATCH /api/v1/tenants/{id}/activate

PATCH /api/v1/tenants/{id}/suspend

PATCH /api/v1/tenants/{id}/archive
```

No MVP, apenas parte desses endpoints será disponibilizada.

---

# 18. Frontend

O módulo deverá possuir:

- listagem
- detalhes
- edição
- configurações
- identidade visual
- preferências regionais

A criação de Tenants ficará restrita ao painel administrativo da plataforma.

---

# 19. Auditoria

Toda alteração deverá registrar:

- usuário responsável
- data
- operação
- valores alterados
- CorrelationId

Nenhuma alteração poderá ocorrer sem auditoria.

---

# 20. Segurança

Toda consulta deverá utilizar Global Query Filters por TenantId.

Nenhuma API poderá aceitar TenantId enviado pelo frontend.

O Tenant deverá ser obtido exclusivamente a partir do contexto do usuário autenticado.

Isso evita ataques de escalonamento horizontal (Horizontal Privilege Escalation).

---

# 21. Performance

Índices obrigatórios:

- Id
- Slug
- Status

Todas as consultas deverão ser paginadas.

Utilizar projeções para listagens.

---

# 22. Testes Obrigatórios

## Unitários

- Criar Tenant.
- Ativar Tenant.
- Suspender Tenant.
- Arquivar Tenant.
- Atualizar configurações.

---

## Integração

- Criar Tenant.
- Criar Organization.
- Criar User.
- Aplicar filtros por Tenant.

---

## Segurança

- impedir acesso entre Tenants
- impedir alteração de TenantId
- validar permissões

---

# 23. Evolução Pós-MVP

O Aggregate foi projetado para suportar:

- múltiplos planos
- assinatura recorrente
- cobrança
- marketplace
- módulos premium
- feature flags
- armazenamento distribuído
- múltiplas organizações
- multi-idioma
- multi-moeda
- branding completo
- white label

Essas funcionalidades não fazem parte do MVP, mas a arquitetura foi preparada para incorporá-las sem mudanças estruturais.

---

# 24. Definition of Done

O Aggregate será considerado concluído quando possuir:

- documentação atualizada
- entidade de domínio
- configuração EF Core
- migration
- repository
- DTOs
- Commands
- Queries
- Validators
- Endpoints
- testes unitários
- testes de integração
- auditoria
- documentação sincronizada

---

# 25. Princípio Final

O Tenant é o principal Aggregate Root do FleetOS.

Toda funcionalidade da plataforma deverá existir dentro do contexto de um Tenant, respeitando os limites de isolamento, segurança e consistência definidos neste documento.

Nenhum módulo poderá ignorar ou contornar esse contexto.