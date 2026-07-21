# BUSINESS_RULES.md

> **Projeto:** FleetOS – Enterprise Fleet Management Platform
> **Versão:** 1.0.0 (MVP)
> **Status:** Fonte Oficial das Regras de Negócio
> **Objetivo:** Centralizar todas as regras funcionais do domínio.

---

# 1. Objetivo

Este documento representa a fonte oficial das regras de negócio do FleetOS.

Toda funcionalidade implementada no sistema deverá respeitar as regras aqui definidas.

Nenhuma regra poderá existir apenas no código.

Sempre que uma nova regra surgir, ela deverá ser documentada neste arquivo antes de sua implementação.

---

# 2. Estrutura das Regras

Cada regra seguirá o padrão:

```text
ID
Categoria
Módulo
Prioridade
Descrição
Justificativa
Entidades
Casos de Uso
Eventos
Exceções
Critérios de Teste
```

---

# 3. Classificação

## Prioridade

| Código   | Significado                   |
| -------- | ----------------------------- |
| Critical | Impede operação do sistema    |
| High     | Impacta diretamente o negócio |
| Medium   | Regra operacional             |
| Low      | Regra complementar            |

---

## Categorias

* CORE
* SECURITY
* TENANT
* USER
* FLEET
* OPERATIONS
* INVENTORY
* FINANCE
* DASHBOARD

---

# 4. CORE

---

## BR-0001

**Categoria**

CORE

**Prioridade**

Critical

**Nome**

Todo registro pertence a um Tenant.

**Descrição**

Toda informação operacional armazenada no sistema deverá possuir um TenantId válido.

**Entidades**

Todas.

**Eventos**

Todos.

**Critérios de Teste**

* Não permitir salvar registro sem TenantId.
* Global Query Filter deve impedir acesso entre Tenants.

---

## BR-0002

**Categoria**

CORE

**Prioridade**

Critical

**Nome**

Todo usuário pertence a apenas um Tenant.

**Descrição**

Um usuário não poderá acessar dados de outro Tenant.

---

## BR-0003

**Categoria**

CORE

**Prioridade**

Critical

**Nome**

Toda Business Unit pertence a exatamente uma Organization.

---

## BR-0004

**Categoria**

CORE

**Prioridade**

High

**Nome**

Toda Organization pertence a exatamente um Tenant.

---

## BR-0005

**Categoria**

CORE

**Prioridade**

Critical

**Nome**

Soft Delete obrigatório.

Nenhuma entidade operacional será removida fisicamente.

---

## BR-0006

**Categoria**

CORE

**Prioridade**

Critical

**Nome**

Auditoria obrigatória.

Todas as alterações deverão registrar:

* Usuário
* Data
* Tenant
* Operação

---

# 5. SEGURANÇA

---

## BR-0101

Senha nunca poderá ser armazenada em texto.

---

## BR-0102

Toda API autenticada deverá validar JWT.

---

## BR-0103

Toda operação deverá validar permissões (RBAC).

---

## BR-0104

Refresh Token poderá ser revogado individualmente.

---

## BR-0105

Toda ação crítica deverá gerar AuditLog.

---

# 6. USUÁRIOS

---

## BR-0201

Email deve ser único dentro do Tenant.

---

## BR-0202

Usuário bloqueado não poderá autenticar.

---

## BR-0203

Usuário poderá possuir múltiplos papéis.

---

## BR-0204

Pelo menos um administrador deverá existir por Tenant.

---

# 7. FROTA

---

## BR-0301

Todo veículo pertence a uma Business Unit.

---

## BR-0302

Placa deve ser única dentro do Tenant.

---

## BR-0303

Veículo inativo não poderá ser utilizado.

---

## BR-0304

Veículo em manutenção não poderá ser vinculado a viagens.

---

## BR-0305

Veículo sem documentação obrigatória ficará indisponível.

---

## BR-0306

Toda manutenção deverá possuir responsável.

---

## BR-0307

Todo abastecimento deverá possuir:

* veículo
* motorista
* data
* quilometragem
* quantidade
* valor

---

# 8. MOTORISTAS

---

## BR-0401

Motorista deverá possuir CNH válida.

---

## BR-0402

Motorista com CNH vencida ficará indisponível.

---

## BR-0403

Motorista bloqueado não poderá iniciar viagens.

---

## BR-0404

Motorista desligado permanecerá apenas para histórico.

---

## BR-0405

CPF deverá ser único dentro do Tenant.

---

# 9. OPERAÇÕES

---

## BR-0501

Toda viagem deverá possuir pelo menos:

* um motorista
* um veículo

---

## BR-0502

Não permitir dois veículos iguais na mesma viagem.

---

## BR-0503

Não permitir dois motoristas iguais na mesma viagem.

---

## BR-0504

Veículo não poderá possuir viagens simultâneas.

---

## BR-0505

Motorista não poderá possuir viagens simultâneas.

---

## BR-0506

Viagem encerrada não poderá ser editada.

---

## BR-0507

Checklist obrigatório antes do início da viagem.

---

## BR-0508

Toda viagem deverá registrar horário de início e término.

---

# 10. ESTOQUE

---

## BR-0601

Estoque nunca poderá ficar negativo.

---

## BR-0602

Toda movimentação deverá possuir responsável.

---

## BR-0603

Toda movimentação deverá gerar histórico.

---

## BR-0604

Produtos poderão ser inativados, nunca excluídos.

---

# 11. FINANCEIRO

---

## BR-0701

Todo lançamento pertence a uma Business Unit.

---

## BR-0702

Receita e despesa nunca poderão compartilhar o mesmo tipo.

---

## BR-0703

Lançamentos confirmados não poderão ser removidos.

---

## BR-0704

Fluxo de caixa será calculado automaticamente.

---

## BR-0705

Toda despesa deverá possuir categoria.

---

# 12. DASHBOARD

---

## BR-0801

Indicadores serão calculados apenas com dados ativos.

---

## BR-0802

Dashboards deverão respeitar permissões do usuário.

---

## BR-0803

Filtros nunca poderão acessar dados de outro Tenant.

---

# 13. Regras de Auditoria

Toda alteração deverá registrar:

* Quem executou
* Quando executou
* Qual entidade foi alterada
* Valor anterior (quando aplicável)
* Novo valor
* Tenant
* CorrelationId

---

# 14. Regras de Performance

* Toda listagem será paginada.
* Nenhuma consulta retornará volume ilimitado.
* Todo filtro será executado no servidor.
* Consultas utilizarão projeções sempre que possível.

---

# 15. Regras de Evolução

Toda nova funcionalidade deverá responder às seguintes perguntas antes de ser implementada:

1. Existe uma regra semelhante?
2. Será necessário criar uma nova regra?
3. Qual módulo será afetado?
4. A regra impacta segurança?
5. A regra impacta auditoria?
6. A regra impacta o modelo multiempresa?
7. É necessário criar novos testes?

Se qualquer resposta indicar mudança de comportamento do sistema, este documento deverá ser atualizado antes da implementação.

---

# 16. Matriz de Rastreabilidade

Cada regra poderá ser referenciada pelos seguintes artefatos:

* Documentos de domínio (`Driver.md`, `Vehicle.md`, etc.).
* Casos de uso.
* APIs.
* Commands.
* Queries.
* Testes automatizados.
* ADRs.

Exemplo:

```text
BR-0504
↓
ScheduleTripCommand
↓
TripAggregate
↓
TripController
↓
TripTests
```

---

# 17. Princípio Final

O `BUSINESS_RULES.md` representa a verdade funcional do FleetOS.

Toda implementação deverá ser consequência destas regras.

Quando houver divergência entre código e documentação, a documentação será considerada a referência oficial até que uma decisão arquitetural (ADR) determine sua atualização.
