# User.md

> Projeto: FleetOS – Enterprise Fleet Management Platform
>
> Domínio: Core
>
> Aggregate Root: User
>
> Versão: 1.0.0 (MVP)
>
> Status: Especificação Oficial

---

# 1. Objetivo

O User representa uma identidade autenticável dentro do FleetOS.

Ele é responsável por acessar a plataforma, executar operações, registrar auditoria e interagir com os módulos conforme suas permissões.

Todo usuário pertence obrigatoriamente a um Tenant.

No MVP, o usuário estará vinculado a uma Organization e poderá atuar em uma ou mais Business Units.

---

# 2. Papel no Domínio

O User possui as seguintes responsabilidades:

- autenticar na plataforma
- possuir credenciais de acesso
- executar operações
- registrar auditoria
- possuir permissões
- pertencer a uma organização
- acessar uma ou mais Business Units

O usuário nunca possui permissões diretamente.

Toda autorização ocorre através de Roles.

---

# 3. Aggregate

```
User
│
├── Roles
├── RefreshTokens
├── Sessions
├── AuditLogs
└── Preferences
```

---

# 4. Responsabilidades

O Aggregate User é responsável por:

- armazenar identidade
- armazenar credenciais
- controlar status da conta
- controlar autenticação
- controlar preferências
- controlar sessões
- controlar último acesso

---

# 5. Estados

```
Invited
    │
    ▼
Active
    │
    ├────────► Locked
    │
    ├────────► Disabled
    │
    ▼
Archived
```

## Invited

Usuário criado, porém ainda não ativou a conta.

---

## Active

Pode utilizar normalmente a plataforma.

---

## Locked

Conta temporariamente bloqueada.

Exemplo:

- muitas tentativas de login
- bloqueio administrativo

---

## Disabled

Conta desativada.

Não poderá autenticar.

---

## Archived

Conta arquivada.

Mantida apenas para histórico.

---

# 6. Lifecycle

```
Create

↓

Invite

↓

Activate

↓

Active

↓

Lock

↓

Unlock

↓

Disable

↓

Archive
```

---

# 7. Invariantes

## INV-USER-001

Todo usuário pertence exatamente a um Tenant.

---

## INV-USER-002

Todo usuário pertence a uma Organization.

---

## INV-USER-003

Todo usuário deve possuir pelo menos uma Role.

---

## INV-USER-004

Email deve ser único dentro do Tenant.

---

## INV-USER-005

Usuário arquivado não poderá autenticar.

---

## INV-USER-006

Senha nunca poderá ser armazenada em texto puro.

Sempre utilizar algoritmo seguro de hash (Argon2id recomendado; BCrypt como alternativa compatível).

---

# 8. Atributos

## Identificação

| Campo | Tipo |
|--------|------|
| Id | UUID |
| TenantId | UUID |
| OrganizationId | UUID |
| Name | string |
| Email | string |
| Status | enum |

---

## Credenciais

| Campo | Tipo |
|--------|------|
| PasswordHash | string |
| PasswordChangedAt | datetime |
| LastLoginAt | datetime |

---

## Segurança

| Campo | Tipo |
|--------|------|
| FailedLoginAttempts | integer |
| LockedUntil | datetime |
| EmailConfirmed | boolean |

---

## Preferências

| Campo | Tipo |
|--------|------|
| Language | string |
| Theme | string |
| TimeZone | string |

---

## Auditoria

| Campo | Tipo |
|--------|------|
| CreatedAt | datetime |
| UpdatedAt | datetime |
| DeletedAt | datetime |

---

# 9. Value Objects

O Aggregate utilizará:

- UserName
- Email
- PasswordHash
- Language
- TimeZone

Todos deverão ser imutáveis.

---

# 10. Entidades Relacionadas

## Role

Relacionamento:

N:N

Um usuário poderá possuir múltiplas Roles.

---

## BusinessUnit

Relacionamento:

N:N

No MVP, o usuário poderá acessar uma ou mais unidades.

---

## RefreshToken

Relacionamento:

1:N

Utilizado para renovação do JWT.

---

## AuditLog

Relacionamento:

1:N

Todas as operações do usuário deverão ser registradas.

---

# 11. Métodos do Domínio

```
Activate()

Disable()

Archive()

Lock()

Unlock()

ChangePassword()

ResetPassword()

AssignRole()

RemoveRole()

AssignBusinessUnit()

RemoveBusinessUnit()

UpdateProfile()

ConfirmEmail()
```

---

# 12. Eventos de Domínio

```
UserCreated

UserInvited

UserActivated

UserLocked

UserUnlocked

UserDisabled

PasswordChanged

RoleAssigned

RoleRemoved

BusinessUnitAssigned

BusinessUnitRemoved
```

---

# 13. Casos de Uso

- Criar usuário
- Convidar usuário
- Editar usuário
- Alterar senha
- Resetar senha
- Bloquear usuário
- Desbloquear usuário
- Alterar permissões
- Vincular Business Unit
- Arquivar usuário

---

# 14. Regras de Negócio

Relacionadas:

- BR-0001
- BR-0002
- BR-0006

Regras específicas:

USR-001 — O e-mail deve ser único dentro do Tenant.

USR-002 — O usuário deve possuir pelo menos uma Role ativa.

USR-003 — Usuário desativado não pode realizar login.

USR-004 — Senhas devem atender à política mínima definida em PROJECT_RULES.md.

USR-005 — Toda alteração de senha invalida todos os Refresh Tokens ativos.

USR-006 — Apenas administradores podem criar ou desativar usuários.

---

# 15. Permissões

```
User.Read

User.Create

User.Update

User.Delete

User.Disable

User.ResetPassword

User.AssignRole

User.AssignBusinessUnit
```

---

# 16. Banco de Dados

Tabela principal:

```
Users
```

Relacionamentos:

```
Tenant
    │
Organization
    │
User
    │
├── UserRoles
├── UserBusinessUnits
├── RefreshTokens
└── AuditLogs
```

---

# 17. API

Endpoints previstos

```
GET    /api/v1/users

GET    /api/v1/users/{id}

POST   /api/v1/users

PUT    /api/v1/users/{id}

PATCH  /api/v1/users/{id}/activate

PATCH  /api/v1/users/{id}/disable

PATCH  /api/v1/users/{id}/lock

PATCH  /api/v1/users/{id}/unlock

PATCH  /api/v1/users/{id}/change-password

POST   /api/v1/users/{id}/assign-role

POST   /api/v1/users/{id}/assign-business-unit
```

---

# 18. Frontend

O módulo deverá possuir:

- Listagem de usuários
- Cadastro
- Edição
- Alteração de senha
- Gestão de papéis
- Gestão de Business Units
- Filtros
- Pesquisa
- Paginação

Componentes:

- DataTable
- Formulários
- Select de Roles
- Select de Business Units
- Modal de confirmação
- Indicadores de status

---

# 19. Auditoria

Registrar:

- UserId
- TenantId
- OrganizationId
- Operação
- IP
- User Agent
- CorrelationId
- Data/Hora

Operações obrigatórias:

- Login
- Logout
- Alteração de senha
- Criação
- Edição
- Exclusão lógica
- Alteração de permissões

---

# 20. Segurança

- JWT obrigatório
- Refresh Token obrigatório
- Hash de senha obrigatório
- Nunca retornar PasswordHash
- Nunca registrar senha em logs
- Rate limit para login
- Bloqueio temporário após múltiplas tentativas inválidas
- Validação de permissões em todos os endpoints

---

# 21. Performance

Índices obrigatórios:

- Id
- TenantId
- OrganizationId
- Email
- Status

Consultas deverão utilizar projeções e paginação.

---

# 22. Testes Obrigatórios

## Unitários

- Criar usuário
- Alterar senha
- Alterar perfil
- Bloquear usuário
- Desbloquear usuário
- Arquivar usuário

## Integração

- Login
- Refresh Token
- Alteração de senha
- Associação de Roles
- Associação de Business Units

## Segurança

- Impedir acesso entre Tenants
- Validar RBAC
- Bloquear login inválido
- Invalidar Refresh Tokens após troca de senha

---

# 23. Evolução Pós-MVP

O Aggregate foi preparado para suportar:

- MFA (autenticação em dois fatores)
- Login via OAuth2/OpenID Connect
- SSO
- WebAuthn/Passkeys
- Avatar
- Assinatura eletrônica
- Delegação temporária de acesso
- Histórico de dispositivos
- Sessões simultâneas controladas

Essas funcionalidades não fazem parte do MVP.

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

O User representa a identidade operacional do FleetOS.

Toda ação realizada na plataforma deve estar vinculada a um usuário autenticado, autorizado e pertencente ao contexto correto de Tenant, Organization e Business Unit.

A autenticação identifica quem executa a ação, enquanto a autorização determina o que esse usuário pode fazer, garantindo segurança, rastreabilidade e governança em toda a plataforma.