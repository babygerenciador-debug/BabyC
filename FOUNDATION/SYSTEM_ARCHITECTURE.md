# SYSTEM_ARCHITECTURE.md

> **Projeto:** Baby Turismo Fleet Management System (BTFMS)
> **Versão:** 1.0.0
> **Status:** Arquitetura Oficial
> **Documento Obrigatório**

---

# 1. Objetivo

Este documento define a arquitetura oficial do Baby Turismo Fleet Management System (BTFMS).

Seu propósito é garantir que todas as implementações futuras mantenham uma arquitetura consistente, escalável, segura e preparada para evolução contínua.

Nenhum módulo poderá ser desenvolvido sem seguir esta arquitetura.

---

# 2. Visão da Plataforma

O BTFMS é uma plataforma web corporativa para gestão operacional de empresas de transporte.

A plataforma centraliza todos os processos da empresa em um único ambiente.

Entre eles:

* Gestão da frota
* Gestão de motoristas
* Gestão de viagens
* Agenda operacional
* Financeiro
* Estoque
* Manutenções
* Abastecimentos
* Analytics
* Relatórios
* Auditoria
* Notificações
* Controle documental

---

# 3. Objetivos Arquiteturais

A arquitetura deve atender aos seguintes objetivos:

## Escalabilidade

O sistema deve crescer sem exigir reescrita dos módulos existentes.

---

## Baixo Acoplamento

Cada módulo deve possuir responsabilidades bem definidas.

---

## Alta Coesão

Cada módulo deve resolver apenas um conjunto específico de problemas.

---

## Testabilidade

Toda regra de negócio deverá ser facilmente testável.

---

## Segurança

Toda comunicação deverá passar pela API.

O frontend jamais acessará diretamente o banco de dados.

---

## Evolução

Novos módulos poderão ser adicionados sem alterar os módulos existentes.

---

# 4. Arquitetura Geral

A arquitetura oficial segue o seguinte fluxo:

```text
Usuário

↓

React Application

↓

REST API (.NET)

↓

Application Layer

↓

Domain Layer

↓

Infrastructure Layer

↓

PostgreSQL

↓

Supabase Storage

↓

Redis
```

Toda comunicação deverá respeitar esse fluxo.

---

# 5. Tecnologias Oficiais

## Frontend

* React
* TypeScript
* Vite
* TanStack Query
* React Router
* React Hook Form
* Zod
* Tailwind CSS
* Shadcn UI
* Apache ECharts
* Recharts
* React Grid Layout
* DnD Kit
* Framer Motion

---

## Backend

* ASP.NET Core 10
* C#
* Entity Framework Core
* MediatR
* FluentValidation
* AutoMapper
* Serilog
* JWT
* BCrypt

---

## Banco

* PostgreSQL (Supabase)

---

## Cache

* Redis

---

## Storage

* Supabase Storage

---

## Containers

* Docker
* Docker Compose
* Nginx

---

# 6. Camadas da Aplicação

## Presentation Layer

Responsável por:

* Interface
* Navegação
* Componentes
* Consumo da API

Não possui regras de negócio.

---

## API Layer

Responsável por:

* Autenticação
* Autorização
* Controllers
* Middleware
* Versionamento
* Swagger

---

## Application Layer

Responsável por:

* Casos de Uso
* Commands
* Queries
* DTOs
* Validações
* Orquestração

Não conhece banco de dados.

---

## Domain Layer

Responsável por:

* Entidades
* Value Objects
* Domain Services
* Regras de Negócio
* Eventos de Domínio

É a camada mais importante do sistema.

Nunca depende de outras camadas.

---

## Infrastructure Layer

Responsável por:

* Entity Framework
* PostgreSQL
* Redis
* Storage
* Repositórios
* Serviços externos

Nunca conter regras de negócio.

---

# 7. Módulos

Cada módulo deverá ser independente.

## Core

* Usuários
* Autenticação
* Permissões
* Auditoria

---

## Operacional

* Motoristas
* Ônibus
* Agenda
* Viagens
* Checklists
* Observações

---

## Frota

* Manutenções
* Abastecimentos
* Documentos
* Pneus (futuro)

---

## Financeiro

* Receitas
* Despesas
* Fluxo de Caixa
* Centro de Custos

---

## Estoque

* Produtos
* Movimentações
* Fornecedores

---

## Analytics

* Dashboards
* KPIs
* Indicadores
* Relatórios
* Exportações

---

## Configuração

* Empresa
* Temas
* SMTP
* Preferências
* Integrações Futuras

---

# 8. Comunicação entre Módulos

Os módulos não poderão acessar diretamente o banco de outros módulos.

Toda comunicação ocorrerá por:

* Commands
* Queries
* Domain Events
* Interfaces

Nunca por dependências diretas.

---

# 9. Arquitetura de Analytics

O sistema NÃO utilizará Power BI.

Será implementado um motor próprio de Analytics.

Componentes:

* Widgets
* KPIs
* Indicadores
* Gráficos
* Filtros
* Exportação
* Dashboards Personalizados

Bibliotecas:

* Apache ECharts
* Recharts
* React Grid Layout

Todos os widgets serão configuráveis pelo usuário.

---

# 10. Autenticação

Fluxo:

1. Login
2. Geração de JWT
3. Geração de Refresh Token
4. Armazenamento seguro
5. Renovação automática

Toda autorização será baseada em papéis (RBAC) e permissões granulares.

---

# 11. Segurança

Obrigatório:

* HTTPS
* JWT
* BCrypt
* Rate Limiting
* CORS
* Audit Logs
* Soft Delete
* Validação de Entrada
* Sanitização de Dados

---

# 12. Banco de Dados

Banco oficial:

PostgreSQL

Responsabilidades:

* Persistência
* Integridade
* Transações
* Índices
* Constraints

Toda regra de negócio permanece na aplicação.

---

# 13. Storage

Arquivos serão armazenados exclusivamente no Supabase Storage.

Estrutura:

/motoristas

/onibus

/documentos

/checklists

/comprovantes

/uploads

Nenhum arquivo será salvo localmente na API.

---

# 14. Cache

Redis será utilizado para:

* Cache de consultas
* Sessões
* Tokens temporários
* Indicadores do Dashboard
* Filas futuras

---

# 15. Logs

Todos os eventos relevantes deverão gerar logs estruturados.

Exemplos:

* Login
* Logout
* Criação
* Alteração
* Exclusão
* Erros
* Exceções

Utilizar Serilog.

---

# 16. Auditoria

Toda alteração persistida deverá registrar:

* Usuário
* Data/Hora
* Entidade
* Operação
* Valores anteriores
* Novos valores

---

# 17. Integrações Futuras

A arquitetura deve permitir integração futura com:

* Google Maps
* WhatsApp Business
* E-mail
* Push Notifications
* OCR
* GPS
* Telemetria
* Aplicativo Mobile
* APIs governamentais (quando necessário)

Essas integrações deverão ser implementadas através de adaptadores na camada de Infrastructure.

---

# 18. Escalabilidade

O sistema deverá suportar:

* Novos módulos
* Novas empresas (multiempresa)
* Novos tipos de usuários
* Novos dashboards
* Novos relatórios
* Novas integrações

Sem alterar o núcleo do domínio.

---

# 19. Princípios Arquiteturais

Toda decisão deve respeitar:

* SOLID
* Clean Architecture
* Domain-Driven Design
* Separation of Concerns
* Dependency Inversion
* Composition over Inheritance
* Convention over Configuration

---

# 20. Critérios de Aceite Arquiteturais

Uma funcionalidade somente poderá ser considerada pronta quando:

* Respeitar esta arquitetura.
* Não introduzir acoplamento desnecessário.
* Não violar os limites entre camadas.
* Possuir testes.
* Possuir documentação.
* Utilizar os padrões oficiais do projeto.

---

# 21. Evolução da Arquitetura

Qualquer mudança estrutural deverá:

1. Ser documentada.
2. Gerar um ADR (Architecture Decision Record).
3. Ser aprovada antes da implementação.
4. Atualizar este documento.

---

# 22. Princípio Final

A arquitetura do BTFMS deve ser compreendida como um ativo estratégico do projeto.

Ela existe para permitir que a plataforma evolua por muitos anos sem perda de qualidade, previsibilidade ou desempenho.

Toda implementação deverá fortalecer a arquitetura, nunca enfraquecê-la.
