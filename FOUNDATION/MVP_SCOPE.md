# MVP_SCOPE.md

> **Projeto:** FleetOS – Enterprise Fleet Management Platform
> **Versão:** 1.0.0 (MVP)
> **Status:** Documento Oficial de Escopo
> **Objetivo:** Definir exatamente o que faz parte da primeira versão do produto.

---

# 1. Objetivo

Este documento define o escopo oficial do MVP (Minimum Viable Product) do FleetOS.

Seu propósito é garantir que o time de desenvolvimento e os agentes de IA implementem apenas as funcionalidades necessárias para validar o produto em ambiente real.

Qualquer funcionalidade fora deste documento será considerada **fora do escopo do MVP**.

---

# 2. Objetivos do MVP

O MVP deve ser capaz de:

* Gerenciar empresas de transporte.
* Controlar motoristas e veículos.
* Organizar viagens e agendas.
* Controlar abastecimentos.
* Gerenciar manutenções.
* Controlar estoque.
* Controlar fluxo financeiro.
* Apresentar dashboards gerenciais.
* Gerar relatórios.
* Operar com múltiplas empresas (Multi-Tenant).
* Ser implantado em produção sem dependências de APIs pagas.

---

# 3. Princípios do MVP

O MVP seguirá os seguintes princípios:

* Simplicidade acima de complexidade.
* Funcionalidade acima de perfeição.
* Arquitetura preparada para crescer.
* Nenhuma dependência obrigatória de serviços pagos.
* Interface moderna e responsiva.
* Código limpo e documentado.
* Escalabilidade planejada desde o início.

---

# 4. Tecnologias do MVP

## Backend

* ASP.NET Core 9
* Entity Framework Core
* FluentValidation
* MediatR (CQRS)
* AutoMapper
* Serilog

---

## Frontend

* React
* TypeScript
* Vite
* React Router
* TanStack Query
* React Hook Form
* Zod
* Apache ECharts
* React Grid Layout
* Tailwind CSS
* shadcn/ui

---

## Banco de Dados

* PostgreSQL (Supabase)

---

## Storage

* Supabase Storage

---

## Infraestrutura

* Docker
* Docker Compose
* Nginx

---

# 5. Arquitetura

O MVP utilizará:

* Clean Architecture
* Domain Driven Design
* CQRS
* Repository Pattern
* Unit of Work
* Multi-Tenant
* JWT Authentication

---

# 6. Funcionalidades Obrigatórias (P0)

## Plataforma

* Login
* Logout
* Refresh Token
* Controle de Usuários
* Controle de Papéis
* Controle de Permissões
* Auditoria
* Configurações da Empresa

---

## Multiempresa

* Tenant
* Empresa
* Filial
* Contexto Organizacional

---

## Motoristas

* Cadastro
* Edição
* Exclusão lógica
* Consulta
* Histórico
* Documentos
* CNH

---

## Veículos

* Cadastro
* Documentação
* Licenciamento
* Seguro
* Status Operacional

---

## Agenda

* Calendário
* Agendamento
* Situação da viagem

---

## Viagens

* Cadastro
* Motorista
* Veículo
* Origem
* Destino
* Checklist
* Observações
* Encerramento

---

## Abastecimento

* Registro
* Quilometragem
* Litros
* Valor
* Consumo Médio
* Histórico

---

## Manutenção

* Preventiva
* Corretiva
* Histórico
* Custos

---

## Estoque

* Produtos
* Categorias
* Entradas
* Saídas
* Movimentações
* Estoque mínimo

---

## Financeiro

* Receitas
* Despesas
* Categorias
* Fluxo de Caixa
* Centro de Custo

---

## Dashboard

* KPIs
* Cards
* Gráficos
* Indicadores
* Filtros por período
* Exportação

---

## Relatórios

* PDF
* Excel
* CSV

---

# 7. Funcionalidades Importantes (P1)

* Dashboard personalizável
* Favoritos
* Temas
* Notificações internas
* Upload de documentos
* Dashboard por perfil

---

# 8. Funcionalidades Futuras (P2)

* Aplicativo Mobile
* Portal do Motorista
* Portal do Cliente
* Dashboard em tempo real
* Assinaturas
* Billing
* White Label
* Marketplace
* API Pública

---

# 9. Funcionalidades Fora do MVP (P3)

Estas funcionalidades **não deverão ser implementadas** na primeira versão:

* Inteligência Artificial
* OCR
* Google Maps API
* GPS
* WhatsApp Business API
* Power BI
* Stripe
* Mercado Pago
* Telemetria
* IoT
* Machine Learning
* Integrações com ERPs
* Multi-região
* Multi-cloud
* Multi-idioma
* Multi-moeda
* Provisionamento automático de tenants

---

# 10. Critérios para Inclusão no MVP

Uma funcionalidade só poderá entrar no MVP se atender aos seguintes critérios:

* Resolve um problema operacional real.
* É utilizada com frequência pelos usuários.
* Não depende de serviços pagos.
* Pode ser implementada em tempo compatível com o cronograma.
* Possui impacto direto na operação da empresa.

---

# 11. Dependências Entre Módulos

| Módulo        | Depende de                   |
| ------------- | ---------------------------- |
| Usuários      | Tenant                       |
| Empresas      | Tenant                       |
| Filiais       | Empresa                      |
| Motoristas    | Empresa                      |
| Veículos      | Empresa                      |
| Agenda        | Motoristas, Veículos         |
| Viagens       | Agenda, Motoristas, Veículos |
| Abastecimento | Veículos                     |
| Manutenção    | Veículos                     |
| Estoque       | Empresa                      |
| Financeiro    | Empresa                      |
| Dashboard     | Todos os módulos             |
| Relatórios    | Todos os módulos             |

---

# 12. Critérios de Aceitação do MVP

O MVP será considerado concluído quando:

* Todos os módulos P0 estiverem implementados.
* Todos os testes críticos forem aprovados.
* A autenticação estiver funcional.
* O isolamento entre tenants estiver validado.
* Os dashboards estiverem operacionais.
* Os relatórios puderem ser exportados.
* O sistema puder ser implantado via Docker.

---

# 13. Métricas de Sucesso

O MVP será considerado validado se:

* A plataforma puder operar em uma empresa real.
* Os usuários conseguirem executar todas as operações principais sem auxílio técnico.
* O desempenho permanecer adequado com milhares de registros por tenant.
* Não houver vazamento de dados entre tenants.
* O tempo médio de resposta das operações críticas permanecer abaixo de 500 ms (em ambiente de produção dimensionado adequadamente).

---

# 14. Regras para Evolução

Novas funcionalidades somente poderão ser adicionadas após:

1. Validação do MVP em produção.
2. Aprovação do Product Owner.
3. Atualização da documentação.
4. Revisão do impacto arquitetural.

---

# 15. Princípio Final

O objetivo do MVP é validar o produto, não entregar todas as funcionalidades imaginadas para a plataforma.

Toda decisão de desenvolvimento deverá priorizar simplicidade, estabilidade e valor entregue ao usuário, preservando uma arquitetura preparada para evoluir sem necessidade de reescrita estrutural.
