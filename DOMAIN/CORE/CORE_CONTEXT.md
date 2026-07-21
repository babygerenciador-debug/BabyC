# CORE_CONTEXT.md

> Projeto: FleetOS – Enterprise Fleet Management Platform
>
> Domínio: Core
>
> Contexto: Core Domain
>
> Versão: 1.0.0 (MVP)
>
> Status: Especificação Oficial

---

# 1. Objetivo

O Core Context representa o coração do FleetOS.

Ele define:

- autenticação
- autorização
- isolamento multiempresa
- gerenciamento de usuários
- gerenciamento de empresas
- configurações
- auditoria
- identidade da plataforma

Todos os demais módulos dependem diretamente do Core.

Nenhum módulo poderá existir fora deste contexto.

---

# 2. Missão do Core

O Core possui quatro responsabilidades principais.

## Identidade

Quem é o usuário?

---

## Segurança

O que ele pode fazer?

---

## Organização

Onde os dados pertencem?

---

## Isolamento

Quais dados ele pode acessar?

---

# 3. Bounded Context

```
FleetOS

├── Core ⭐
│
├── Fleet
│
├── Operations
│
├── Inventory
│
├── Finance
│
└── Dashboard
```

O Core é o único contexto conhecido por todos os demais.

Os outros módulos nunca deverão depender entre si diretamente quando essa dependência puder ser resolvida pelo Core.

---

# 4. Aggregate Roots

O Core possui os seguintes Aggregates.

```
Tenant

Organization

BusinessUnit

User

Role

Permission

RefreshToken

Setting

AuditLog

FileStorage
```

Cada Aggregate possui responsabilidade única.

---

# 5. Relacionamento entre Aggregates

```
Tenant
│
├── Organization
│      │
│      ├── BusinessUnit
│      │
│      ├── Users
│      │
│      ├── Fleet
│      │
│      ├── Inventory
│      │
│      ├── Finance
│      │
│      └── Dashboard
│
└── Settings
```

Nenhum Aggregate poderá pertencer a mais de um Tenant.

---

# 6. Hierarquia do Domínio

```
Platform
    │
Tenant
    │
Organization
    │
BusinessUnit
    │
User
    │
Role
    │
Permission
```

Esta hierarquia deverá ser respeitada por todas as implementações.

---

# 7. Limites do Contexto

O Core é responsável por:

- Login
- JWT
- Refresh Token
- Usuários
- Papéis
- Permissões
- Tenant
- Organizations
- Business Units
- Configurações
- Auditoria
- Upload de arquivos

O Core NÃO é responsável por:

- veículos
- motoristas
- viagens
- estoque
- financeiro

Esses pertencem aos respectivos contextos.

---

# 8. Fluxo de Autenticação

```
Usuário

↓

Login

↓

JWT

↓

Middleware

↓

User Context

↓

Tenant Context

↓

Permission Context

↓

Controller

↓

Application

↓

Domain
```

Todo acesso autenticado deverá seguir este fluxo.

---

# 9. Fluxo de Autorização (RBAC)

```
User

↓

Roles

↓

Permissions

↓

Authorization Policy

↓

Endpoint
```

As permissões nunca serão verificadas diretamente pelo frontend.

Toda validação será realizada no backend.

---

# 10. Modelo Multi-Tenant

FleetOS utilizará:

**Shared Database + Shared Schema**

Cada tabela operacional possuirá obrigatoriamente:

```
TenantId
```

O isolamento será garantido por:

- Middleware
- User Context
- Global Query Filters
- Policies
- Auditoria

---

# 11. Escopo das Entidades

## Tenant

Escopo máximo do sistema.

---

## Organization

Escopo jurídico.

Representa uma empresa (CNPJ).

---

## BusinessUnit

Escopo operacional.

Representa uma unidade física, filial, garagem ou centro operacional.

---

## User

Pessoa autenticada.

---

## Role

Conjunto de permissões.

---

## Permission

Ação autorizada.

---

## AuditLog

Histórico imutável.

---

## FileStorage

Metadados dos arquivos.

---

# 12. Ownership (Propriedade)

Cada recurso possui exatamente um proprietário.

```
Tenant

↓

Organization

↓

BusinessUnit
```

Exemplo.

```
Trip

↓

BusinessUnit

↓

Organization

↓

Tenant
```

Nunca existirão recursos sem proprietário.

---

# 13. Eventos do Core

O Core publica:

```
TenantCreated

OrganizationCreated

BusinessUnitCreated

UserCreated

UserBlocked

RoleCreated

PermissionGranted

PermissionRevoked
```

O Core poderá consumir:

```
SubscriptionActivated

SubscriptionCanceled
```

No MVP esses eventos serão internos.

---

# 14. Comunicação entre Contextos

```
Fleet

↓

Solicita User Context

↓

Core

↓

Retorna:

Tenant

Organization

BusinessUnit

Permissions
```

Os módulos nunca acessarão diretamente tabelas de outros contextos.

A comunicação ocorrerá através de serviços da camada de aplicação.

---

# 15. Matriz de Dependências

| Contexto | Depende do Core |
|-----------|----------------:|
| Fleet | Sim |
| Operations | Sim |
| Inventory | Sim |
| Finance | Sim |
| Dashboard | Sim |

O Core não depende de nenhum outro contexto.

---

# 16. Segurança

Todas as operações deverão validar:

- JWT
- Tenant
- Usuário ativo
- Role
- Permission
- Status da Organization
- Status da Business Unit

Nenhuma API deverá confiar em informações enviadas pelo frontend para determinar o contexto do usuário.

---

# 17. Auditoria

Todas as operações críticas deverão registrar:

- TenantId
- OrganizationId
- BusinessUnitId
- UserId
- CorrelationId
- IP
- User Agent
- Data/Hora
- Operação

---

# 18. Convenções

Todos os módulos deverão utilizar os mesmos nomes definidos em:

- UBIQUITOUS_LANGUAGE.md

Todas as regras deverão respeitar:

- BUSINESS_RULES.md

Todos os Aggregates deverão seguir:

- MODULE_TEMPLATE.md

---

# 19. Evolução Pós-MVP

A arquitetura do Core foi preparada para suportar:

- múltiplos planos
- múltiplas organizações
- múltiplas filiais
- autenticação externa (OAuth2/OpenID Connect)
- SSO
- MFA
- White Label
- Feature Flags
- API Keys
- Marketplace de módulos
- Multi-idioma
- Multi-moeda

Essas funcionalidades não fazem parte do MVP, mas não exigirão mudanças estruturais.

---

# 20. Decisões Arquiteturais (ADR)

O Core adota as seguintes decisões:

- Shared Database + Shared Schema
- UUID como chave primária
- Soft Delete obrigatório
- Auditoria obrigatória
- JWT + Refresh Token
- RBAC para autorização
- DDD + Clean Architecture
- CQRS para separação de comandos e consultas
- Eventos de domínio para comunicação interna

---

# 21. Diagrama Geral do Core

```
Platform
    │
    ▼
Tenant
    │
    ▼
Organization
    │
    ▼
BusinessUnit
    │
    ├───────────────┐
    ▼               ▼
Users          Fleet
    │               │
    ▼               ▼
Roles        Operations
    │               │
    ▼               ▼
Permissions   Inventory
                    │
                    ▼
                Finance
                    │
                    ▼
                Dashboard
```

---

# 22. Definition of Done

O Core Context será considerado completo quando:

- Todos os Aggregates estiverem documentados.
- Todos os relacionamentos estiverem definidos.
- Todos os eventos estiverem especificados.
- Todas as regras de negócio estiverem rastreadas.
- Todos os diagramas estiverem atualizados.
- Toda a arquitetura estiver consistente.

---

# 23. Princípio Final

O Core Context é a fundação do FleetOS.

Nenhum módulo implementará autenticação, autorização, isolamento de dados ou gerenciamento organizacional de forma independente.

Todas essas responsabilidades pertencem exclusivamente ao Core.

Essa separação garante consistência, segurança, escalabilidade e evolução sustentável da plataforma, permitindo que novos módulos sejam adicionados sem alterar as bases arquiteturais do sistema.