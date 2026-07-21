# 🚍 Baby Turismo Fleet Management System (BTFMS)

> Sistema profissional de gestão de frota, motoristas, viagens, manutenção, financeiro e analytics para empresas de transporte.

---

# Visão Geral

O Baby Turismo Fleet Management System (BTFMS) é uma plataforma web moderna desenvolvida para centralizar toda a operação de empresas de transporte em um único ambiente.

O objetivo principal do sistema é eliminar planilhas, reduzir falhas operacionais, automatizar processos e fornecer indicadores estratégicos em tempo real.

Ao contrário de ERPs tradicionais, o BTFMS foi projetado utilizando princípios modernos de arquitetura de software, permitindo evolução contínua, alta escalabilidade e integração futura com aplicações mobile, inteligência artificial e sistemas externos.

---

# Objetivos

O sistema deverá permitir:

- Gestão completa da frota
- Gestão de motoristas
- Gestão financeira
- Gestão de estoque
- Controle de abastecimentos
- Agenda Inteligente
- Dashboard Analítico
- Relatórios
- Controle documental
- Checklists
- Observações operacionais
- Sistema de notificações
- Auditoria completa
- Controle de permissões
- Upload de documentos
- Histórico completo das operações

---

# Tecnologias

## Backend

- ASP.NET Core 9
- C#
- Entity Framework Core
- PostgreSQL
- Redis
- Docker
- JWT
- FluentValidation
- MediatR
- AutoMapper
- Serilog

---

## Frontend

- React
- TypeScript
- Vite
- React Query
- React Router
- React Hook Form
- Zod
- TailwindCSS
- Shadcn UI
- DnD Kit
- React Grid Layout
- Apache ECharts
- Recharts
- Framer Motion

---

## Banco de Dados

PostgreSQL hospedado no Supabase.

O Supabase será utilizado apenas como infraestrutura.

Toda regra de negócio ficará exclusivamente na API.

---

# Arquitetura

O projeto utilizará:

- Clean Architecture
- Domain Driven Design (DDD)
- CQRS
- Repository Pattern
- Unit of Work
- SOLID
- Dependency Injection
- Event Driven Design

---

# Estrutura Geral

docs/
backend/
frontend/
database/
docker/

---

# Módulos

- Dashboard Analytics
- Motoristas
- Ônibus
- Agenda
- Viagens
- Abastecimentos
- Financeiro
- Estoque
- Manutenções
- Checklists
- Observações
- Notificações
- Configurações
- Auditoria
- Usuários
- Permissões

---

# Dashboard

O sistema NÃO utilizará Power BI.

Será desenvolvido um motor próprio de Analytics utilizando:

- Apache ECharts
- Recharts
- React Grid Layout
- DnD Kit

Os dashboards serão totalmente personalizáveis.

Cada usuário poderá:

- mover widgets
- criar dashboards
- salvar layouts
- criar filtros
- exportar relatórios

---

# Princípios

Todo código deverá seguir:

- Clean Code
- SOLID
- KISS
- DRY
- YAGNI

---

# Padrões

Todo desenvolvimento deverá respeitar:

- documentação
- testes
- arquitetura
- convenções
- nomenclaturas

Nenhuma funcionalidade será implementada sem documentação prévia.

---

# Roadmap

Fase 1

Documentação

↓

Fase 2

Arquitetura

↓

Fase 3

Banco de Dados

↓

Fase 4

Backend

↓

Fase 5

Frontend

↓

Fase 6

Testes

↓

Fase 7

Deploy

---

# Estrutura da Documentação

docs

00-PROJETO

01-REQUISITOS

02-ARQUITETURA

03-DOMINIO

04-BANCO

05-BACKEND

06-FRONTEND

07-MODULOS

08-INFRA

09-SEGURANCA

10-TESTES

11-IA

12-ADR

13-ANEXOS

---

# Filosofia do Projeto

Este projeto não deverá ser tratado como um CRUD.

Ele deverá ser tratado como uma plataforma corporativa de gestão operacional.

Toda decisão arquitetural deverá priorizar:

- manutenção
- escalabilidade
- legibilidade
- segurança
- desempenho
- baixo acoplamento

---

# Integrações Futuras

Google Maps

WhatsApp

E-mail

Push Notifications

OCR

Machine Learning

Aplicativo Mobile

GPS

Telemetria

---

# Licença

Projeto proprietário.

Todos os direitos reservados.
