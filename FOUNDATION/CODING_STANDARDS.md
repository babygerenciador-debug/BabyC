# CODING_STANDARDS.md

> **Projeto:** FleetOS – Enterprise Fleet Management Platform
> **Versão:** 1.0.0
> **Status:** Obrigatório
> **Objetivo:** Definir os padrões oficiais de desenvolvimento do FleetOS.

---

# 1. Objetivo

Este documento estabelece as convenções obrigatórias para todo o código do FleetOS.

Todos os desenvolvedores e agentes de IA devem seguir estas regras sem exceções.

Nenhum Pull Request poderá ser aprovado caso viole estas diretrizes.

---

# 2. Princípios Fundamentais

Todo código deve seguir os seguintes princípios:

* Simplicidade acima de complexidade.
* Legibilidade acima de otimização prematura.
* Composição acima de herança.
* Baixo acoplamento.
* Alta coesão.
* Código orientado ao domínio.
* Arquitetura antes de implementação.
* Convenção acima de configuração.

---

# 3. Arquitetura Obrigatória

O backend utilizará:

* Clean Architecture
* Domain Driven Design (DDD)
* CQRS
* Repository Pattern
* Unit of Work
* Dependency Injection
* SOLID
* Fluent Validation

O frontend utilizará:

* Component Based Architecture
* Feature Based Folder Structure
* Atomic UI (somente para componentes reutilizáveis)
* Composition Pattern
* Hooks

---

# 4. Estrutura do Repositório

```text
fleetos/

backend/
frontend/
docs/
docker/
scripts/
```

Não criar diretórios fora desse padrão sem aprovação arquitetural.

---

# 5. Convenções de Nome

## Classes

PascalCase

Exemplo:

```text
DriverService
VehicleRepository
TripAggregate
```

---

## Interfaces

Sempre iniciar com I.

```text
IDriverRepository
ITripService
```

---

## Métodos

PascalCase

```csharp
CreateDriver()

CalculateFuelAverage()

ScheduleTrip()
```

---

## Variáveis

camelCase

```csharp
driverName

totalDistance
```

---

## Constantes

PascalCase para constantes fortemente tipadas.

UPPER_SNAKE_CASE apenas quando exigido por integração externa.

---

## Arquivos

Mesmo nome da classe principal.

Nunca utilizar nomes genéricos.

Exemplo:

```text
DriverService.cs

TripController.cs
```

---

# 6. Organização do Backend

Cada módulo seguirá exatamente esta estrutura:

```text
Drivers/

Application/

Commands/

Queries/

DTOs/

Validators/

Domain/

Entities/

Events/

ValueObjects/

Repositories/

Infrastructure/

Persistence/

Controllers/

Tests/
```

Nenhum módulo poderá fugir dessa organização.

---

# 7. Organização do Frontend

Estrutura baseada em funcionalidades.

```text
src/

modules/

drivers/

pages/

components/

hooks/

services/

types/

validators/
```

Componentes compartilhados ficarão em:

```text
src/shared/
```

---

# 8. Regras para Entidades

Entidades devem conter apenas:

* Estado
* Comportamento do domínio
* Regras de negócio

Não devem:

* Acessar banco
* Conhecer HTTP
* Conhecer React
* Conhecer Entity Framework

---

# 9. Value Objects

Utilizar Value Objects para conceitos como:

* CPF
* CNPJ
* Email
* Phone
* Money
* Address
* Plate
* Chassis

Devem ser imutáveis.

---

# 10. Commands e Queries

Toda alteração de estado deve ser feita através de **Commands**.

Toda leitura deve ser feita através de **Queries**.

Exemplos:

Commands:

```text
CreateDriverCommand
UpdateVehicleCommand
ScheduleTripCommand
```

Queries:

```text
GetDriverByIdQuery
GetTripsByDateQuery
GetDashboardQuery
```

---

# 11. Validação

Toda validação será realizada com **FluentValidation**.

Não utilizar validações espalhadas por Controllers ou Services.

---

# 12. Tratamento de Erros

Nunca utilizar exceções para fluxo de negócio.

Erros de domínio devem retornar resultados controlados.

Exemplos:

* DriverNotFound
* VehicleUnavailable
* InvalidCNH
* TripAlreadyStarted

Exceções devem ser reservadas para falhas inesperadas.

---

# 13. Logging

Utilizar **Serilog**.

Todos os logs devem conter:

* CorrelationId
* TenantId
* UserId
* Timestamp
* Action
* Duration

Nunca registrar:

* Senhas
* Tokens
* Dados sensíveis

---

# 14. Multi-Tenant

Toda entidade persistida deve conter:

* TenantId
* OrganizationId
* BusinessUnitId

Filtros globais devem impedir acesso entre tenants.

---

# 15. API REST

Todos os endpoints seguirão RESTful.

Exemplo:

```http
GET    /api/drivers
GET    /api/drivers/{id}
POST   /api/drivers
PUT    /api/drivers/{id}
DELETE /api/drivers/{id}
```

Versionamento:

```text
/api/v1/
```

---

# 16. DTOs

Nunca expor entidades diretamente.

Sempre utilizar:

* Request DTO
* Response DTO

Mapeamento via AutoMapper.

---

# 17. Banco de Dados

Convenções:

* Tabelas no singular.
* Chaves primárias como `Id`.
* Chaves estrangeiras como `<Entidade>Id`.
* Soft Delete obrigatório (`DeletedAt`).
* Auditoria (`CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`).

Índices devem ser planejados antes da implementação.

---

# 18. Frontend

## Estado

* TanStack Query para dados do servidor.
* React Hook Form para formulários.
* Context API apenas para estado global simples.

Evitar estado duplicado.

---

## Componentes

Tipos:

* UI (apresentação)
* Feature (regra da funcionalidade)
* Layout
* Shared

Cada componente deve possuir responsabilidade única.

---

## Estilo

* Tailwind CSS
* shadcn/ui
* CSS customizado apenas quando necessário.

---

# 19. Testes

Cada módulo deve possuir:

* Testes unitários.
* Testes de integração.
* Testes de validação.
* Testes de autorização.

Nenhum módulo será considerado concluído sem testes.

---

# 20. Segurança

Obrigatório:

* JWT
* Refresh Token
* Hash de senha com BCrypt
* Validação de permissões
* Validação de Tenant
* Proteção contra Mass Assignment

Nunca confiar em dados enviados pelo cliente.

---

# 21. Performance

Boas práticas:

* Paginação obrigatória.
* Filtros no servidor.
* Projeções para listagens.
* Evitar consultas N+1.
* Consultas assíncronas.
* Cache apenas quando documentado.

---

# 22. Documentação

Todo módulo deve possuir:

* README
* DOMAIN
* DATABASE
* API
* FRONTEND
* TESTS

Toda alteração arquitetural deve atualizar a documentação correspondente.

---

# 23. Git

Branches:

```text
main
develop
feature/*
fix/*
hotfix/*
```

Commits devem seguir Conventional Commits.

Exemplos:

```text
feat(drivers): add driver registration

fix(trips): validate overlapping schedules

refactor(finance): simplify cash flow service

docs(api): update trip endpoints
```

---

# 24. Revisão de Código

Antes de aprovar um Pull Request, verificar:

* Arquitetura respeitada.
* Convenções atendidas.
* Testes passando.
* Documentação atualizada.
* Nenhum código morto.
* Nenhuma dependência desnecessária.
* Nenhuma duplicação evidente.

---

# 25. Checklist para Agentes de IA

Antes de concluir qualquer tarefa, o agente deve confirmar:

* Estrutura de pastas correta.
* Convenções de nomenclatura respeitadas.
* Regras de domínio implementadas.
* DTOs criados.
* Validators implementados.
* Endpoints documentados.
* Testes escritos.
* Documentação atualizada.
* Multi-Tenant respeitado.
* Permissões aplicadas.

Se qualquer item estiver pendente, a tarefa não deve ser considerada concluída.

---

# 26. Princípio Final

O FleetOS deve parecer um sistema desenvolvido por uma única equipe experiente, independentemente da quantidade de desenvolvedores ou agentes de IA envolvidos.

Toda decisão de implementação deve privilegiar consistência, clareza, testabilidade e evolução contínua da plataforma. Quando houver dúvida entre duas abordagens tecnicamente válidas, deve ser escolhida aquela que reduz a complexidade do código e facilita a manutenção de longo prazo.
