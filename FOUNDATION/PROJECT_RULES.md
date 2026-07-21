# PROJECT_RULES.md

> **Versão:** 1.0.0
>
> **Projeto:** Baby Turismo Fleet Management System (BTFMS)
>
> **Status:** Obrigatório
>
> Este documento define os padrões obrigatórios para todo o projeto.
> Nenhum código poderá ser implementado sem respeitar estas regras.

---

# 1. Objetivo

Este documento garante que todo o projeto siga uma arquitetura consistente,
escalável e de fácil manutenção.

As regras aqui descritas possuem prioridade sobre qualquer sugestão de IA,
framework ou biblioteca.

Caso exista conflito entre uma recomendação externa e este documento,
este documento prevalece.

---

# 2. Filosofia do Projeto

O BTFMS NÃO é um CRUD.

O BTFMS é uma plataforma corporativa de gestão operacional.

Todas as decisões deverão priorizar:

- Escalabilidade
- Segurança
- Baixo Acoplamento
- Alta Coesão
- Legibilidade
- Testabilidade
- Performance
- Evolução Contínua

Nunca implementar soluções rápidas ("quick fixes") que prejudiquem a arquitetura.

---

# 3. Arquitetura Oficial

A arquitetura obrigatória será:

- Clean Architecture
- Domain Driven Design (DDD)
- CQRS
- SOLID
- Repository Pattern
- Unit of Work
- Dependency Injection
- Event Driven Architecture
- Vertical Slice Architecture (para Application Layer)

Arquiteturas alternativas somente poderão ser utilizadas após aprovação.

---

# 4. Tecnologias Oficiais

## Backend

- ASP.NET Core 10
- C#
- Entity Framework Core
- PostgreSQL
- Redis
- MediatR
- FluentValidation
- AutoMapper
- JWT
- Serilog

## Frontend

- React
- TypeScript
- Vite
- React Query
- React Hook Form
- React Router
- TailwindCSS
- Shadcn UI
- Apache ECharts
- Recharts
- React Grid Layout
- Framer Motion

---

# 5. Banco de Dados

Banco oficial:

PostgreSQL

Hospedagem:

Supabase

IMPORTANTE

O Supabase será utilizado apenas como:

- PostgreSQL
- Object Storage
- Backup

Nunca utilizar:

- Supabase Auth
- Supabase RPC
- Supabase Edge Functions
- Regras de negócio no banco

Toda regra de negócio deverá estar na API.

---

# 6. Comunicação

Fluxo obrigatório

Frontend

↓

API

↓

Application

↓

Domain

↓

Infrastructure

↓

PostgreSQL

Nunca permitir acesso direto do frontend ao banco.

---

# 7. Organização do Repositório

```text
backend/

frontend/

database/

docker/

docs/

scripts/

.github/

```

Nenhum arquivo fora da estrutura oficial.

---

# 8. Estrutura Backend

```text
src/

FleetOS.Api

FleetOS.Application

FleetOS.Domain

FleetOS.Infrastructure

FleetOS.Shared

tests/

FleetOS.Tests
```

---

# 9. Estrutura Frontend

```text
src/

app/

components/

features/

hooks/

services/

layouts/

pages/

routes/

contexts/

styles/

utils/

types/

assets/

```

Cada módulo deverá possuir sua própria pasta.

---

# 10. Convenções de Código

Todos os desenvolvedores e agentes deverão seguir:

- SOLID
- Clean Code
- DRY
- KISS
- YAGNI

Nunca duplicar regras de negócio.

---

# 11. Convenção de Nomes

## Classes

PascalCase

Exemplo

MotoristaService

---

## Interfaces

Prefixo I

Exemplo

IMotoristaRepository

---

## Métodos

PascalCase

CalcularMedia()

---

## Variáveis

camelCase

---

## Constantes

PascalCase

---

## Tabelas

snake_case

motoristas

viagens

abastecimentos

---

## Colunas

snake_case

created_at

updated_at

deleted_at

---

# 12. Padrões de API

Toda API deverá possuir:

- Versionamento
- Swagger
- JWT
- Paginação
- Filtros
- Ordenação
- Validação
- Tratamento global de exceções

Nunca retornar Exception diretamente.

---

# 13. DTOs

Nenhuma entidade poderá ser exposta diretamente.

Sempre utilizar:

Request DTO

Response DTO

ViewModel

---

# 14. Validação

Toda validação deverá utilizar:

FluentValidation

Nunca validar diretamente no Controller.

---

# 15. Controllers

Controllers deverão possuir apenas:

- Receber Request
- Chamar MediatR
- Retornar Response

Nenhuma regra de negócio.

---

# 16. Logging

Todos os módulos deverão registrar:

- Login
- Logout
- Alterações
- Exclusões
- Criações
- Erros
- Exceções
- Eventos importantes

Utilizar Serilog.

---

# 17. Auditoria

Toda alteração deverá registrar:

Usuário

Data

Hora

IP (quando disponível)

Valor antigo

Novo valor

Entidade alterada

---

# 18. Soft Delete

Nenhuma exclusão física será realizada.

Todas utilizarão:

deleted_at

deleted_by

---

# 19. Segurança

Obrigatório

JWT

Refresh Token

HTTPS

Rate Limit

CORS

Password Hash

BCrypt

Proteção contra SQL Injection

Proteção contra XSS

Proteção contra CSRF quando aplicável

---

# 20. Uploads

Todos os uploads deverão utilizar:

Supabase Storage

Organização:

/motoristas

/onibus

/documentos

/checklists

/comprovantes

/uploads

Nunca salvar arquivos no servidor da aplicação.

---

# 21. Frontend

O Frontend não poderá conter:

Regras de negócio

SQL

Autenticação direta

Consultas ao banco

Toda comunicação será feita via API.

---

# 22. Estado Global

Utilizar:

TanStack Query

Context API

Evitar estados globais desnecessários.

---

# 23. Dashboard Analytics

Não utilizar Power BI.

Todos os dashboards serão desenvolvidos utilizando:

Apache ECharts

Recharts

React Grid Layout

Widgets personalizáveis

Filtros

Exportação

Tempo real quando necessário

---

# 24. Performance

Sempre utilizar:

Lazy Loading

Code Splitting

Cache

Paginação

Compressão

Virtualização de listas grandes

---

# 25. Testes

Obrigatório:

Testes Unitários

Testes de Integração

Testes de Permissões

Testes de Regressão

Nenhuma funcionalidade crítica sem testes.

---

# 26. Git

Branch principal

main

Desenvolvimento

develop

Features

feature/nome

Correções

fix/nome

Hotfix

hotfix/nome

---

# 27. Commits

Seguir Conventional Commits.

Exemplo

feat:

fix:

refactor:

docs:

style:

test:

build:

---

# 28. Definition of Done

Uma funcionalidade somente será considerada pronta quando possuir:

✔ Documentação

✔ Testes

✔ Logs

✔ Tratamento de Erros

✔ Permissões

✔ Auditoria

✔ Validações

✔ Código Revisado

✔ Build sem erros

✔ Lint sem erros

✔ Cobertura mínima de testes definida para o projeto

---

# 29. Regras para Agentes de IA

Todo agente deverá:

Ler PROJECT_RULES.md antes de iniciar qualquer tarefa.

Nunca modificar arquitetura sem autorização.

Nunca criar dependências desnecessárias.

Nunca duplicar funcionalidades.

Nunca remover testes existentes.

Sempre documentar alterações arquiteturais.

Sempre seguir os padrões definidos neste documento.

Caso haja dúvida, o agente deverá solicitar esclarecimento em vez de assumir um comportamento.

---

# 30. Architecture Decision Records (ADR)

Toda decisão arquitetural relevante deverá gerar um ADR.

Exemplos:

- escolha de biblioteca
- mudança de arquitetura
- alteração de banco
- mudança de autenticação
- substituição de tecnologia

---

# 31. Documentação Obrigatória

Nenhum módulo poderá ser implementado sem documentação correspondente.

Cada módulo deverá possuir:

- Objetivo
- Responsabilidades
- Regras de Negócio
- Casos de Uso
- Fluxo
- Eventos
- Entidades
- DTOs
- Critérios de Aceite
- Testes

---

# 32. Roadmap

A ordem oficial de desenvolvimento será:

1. Documentação
2. Arquitetura
3. Domínio
4. Banco de Dados
5. Backend
6. Frontend
7. Testes
8. Deploy
9. Monitoramento
10. Evolução Contínua

---

# 33. Princípio Final

Todo código deve ser escrito como se este projeto fosse mantido pelos próximos 10 anos.

O objetivo não é apenas entregar funcionalidades, mas construir uma plataforma robusta, previsível, segura e preparada para evolução contínua.