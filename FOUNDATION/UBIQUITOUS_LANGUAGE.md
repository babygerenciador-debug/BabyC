# UBIQUITOUS_LANGUAGE.md

> **Projeto:** FleetOS – Enterprise Fleet Management Platform
> **Versão:** 1.0.0
> **Status:** Documento Oficial de Linguagem do Domínio
> **Objetivo:** Definir o vocabulário único do FleetOS.

---

# 1. Objetivo

Este documento estabelece a Linguagem Ubíqua (Ubiquitous Language) oficial do FleetOS.

Todos os desenvolvedores, Product Owners, designers, analistas e agentes de IA devem utilizar exatamente os termos definidos neste documento.

O objetivo é eliminar ambiguidades e garantir que todas as camadas do sistema utilizem o mesmo vocabulário.

---

# 2. Princípios

A linguagem do domínio deve obedecer às seguintes regras:

* Um conceito possui apenas um nome oficial.
* Um nome representa apenas um conceito.
* APIs, banco de dados, documentação e frontend devem utilizar os mesmos termos.
* Não utilizar sinônimos para entidades do domínio.
* Toda nova entidade deve ser adicionada neste documento antes de ser implementada.

---

# 3. Estrutura Organizacional

| Termo         | Definição                                                                 |
| ------------- | ------------------------------------------------------------------------- |
| Platform      | O produto FleetOS como plataforma SaaS.                                   |
| Tenant        | Cliente da plataforma FleetOS. É o limite máximo de isolamento dos dados. |
| Organization  | Empresa pertencente ao Tenant. Representa uma pessoa jurídica.            |
| Business Unit | Unidade operacional ou filial de uma Organization.                        |
| Workspace     | Espaço funcional preparado para evolução futura. Não faz parte do MVP.    |

---

# 4. Identidade e Segurança

| Termo          | Definição                                        |
| -------------- | ------------------------------------------------ |
| User           | Usuário autenticado da plataforma.               |
| Role           | Conjunto de permissões atribuídas a um usuário.  |
| Permission     | Ação autorizada dentro do sistema.               |
| Authentication | Processo de identificação do usuário.            |
| Authorization  | Processo de validação das permissões do usuário. |
| Refresh Token  | Token utilizado para renovar a autenticação.     |
| Session        | Sessão autenticada de um usuário.                |

---

# 5. Frota

| Termo            | Definição                                              |
| ---------------- | ------------------------------------------------------ |
| Driver           | Motorista autorizado a operar veículos da empresa.     |
| Vehicle          | Veículo pertencente à frota.                           |
| Vehicle Type     | Categoria do veículo (ônibus, micro-ônibus, van etc.). |
| Vehicle Document | Documento relacionado ao veículo.                      |
| Insurance        | Seguro do veículo.                                     |
| Maintenance      | Registro de manutenção preventiva ou corretiva.        |
| Fuel Record      | Registro de abastecimento do veículo.                  |

---

# 6. Operações

| Termo      | Definição                                                           |
| ---------- | ------------------------------------------------------------------- |
| Trip       | Viagem operacional realizada pela empresa.                          |
| Schedule   | Planejamento ou agenda de viagens.                                  |
| Checklist  | Lista de verificações executadas antes, durante ou após uma viagem. |
| Trip Event | Evento ocorrido durante uma viagem.                                 |

---

# 7. Estoque

| Termo          | Definição                                      |
| -------------- | ---------------------------------------------- |
| Product        | Item controlado no estoque.                    |
| Category       | Classificação de produtos.                     |
| Stock Movement | Entrada, saída ou ajuste de estoque.           |
| Inventory      | Conjunto de produtos controlados pela empresa. |

---

# 8. Financeiro

| Termo           | Definição              |
| --------------- | ---------------------- |
| Financial Entry | Lançamento financeiro. |
| Revenue         | Receita.               |
| Expense         | Despesa.               |
| Cost Center     | Centro de custo.       |
| Cash Flow       | Fluxo de caixa.        |

---

# 9. Dashboard e Relatórios

| Termo     | Definição                                                   |
| --------- | ----------------------------------------------------------- |
| Dashboard | Painel de indicadores do sistema.                           |
| KPI       | Indicador-chave de desempenho.                              |
| Widget    | Componente visual do dashboard.                             |
| Report    | Relatório gerado pelo sistema.                              |
| Filter    | Critério utilizado para restringir consultas e indicadores. |

---

# 10. Arquivos

| Termo        | Definição                                      |
| ------------ | ---------------------------------------------- |
| File Storage | Metadados de arquivos enviados ao sistema.     |
| Attachment   | Arquivo anexado a uma entidade.                |
| Document     | Arquivo relacionado a uma operação do sistema. |

---

# 11. Auditoria

| Termo          | Definição                                             |
| -------------- | ----------------------------------------------------- |
| Audit Log      | Registro imutável de ações realizadas no sistema.     |
| Event          | Acontecimento relevante do domínio.                   |
| Correlation ID | Identificador utilizado para rastrear uma requisição. |

---

# 12. Objetos de Valor (Value Objects)

| Termo         | Definição                                       |
| ------------- | ----------------------------------------------- |
| Email         | Endereço eletrônico válido e imutável.          |
| Phone         | Número de telefone padronizado.                 |
| CPF           | Cadastro de Pessoa Física.                      |
| CNPJ          | Cadastro Nacional da Pessoa Jurídica.           |
| Address       | Endereço estruturado.                           |
| Person Name   | Nome completo de uma pessoa.                    |
| Money         | Valor monetário com moeda e precisão definidas. |
| File Hash     | Hash criptográfico de um arquivo.               |
| File Path     | Caminho lógico do arquivo no armazenamento.     |
| License Plate | Placa do veículo.                               |
| Chassis       | Número do chassi do veículo.                    |

---

# 13. Termos Proibidos

Os seguintes termos não devem ser utilizados na documentação ou no código quando houver um termo oficial correspondente.

| Não utilizar                                | Utilizar        |
| ------------------------------------------- | --------------- |
| Cliente                                     | Tenant          |
| Empresa (quando se referir ao cliente SaaS) | Tenant          |
| Filial                                      | Business Unit   |
| Perfil                                      | Role            |
| Permissão de usuário                        | Permission      |
| Ônibus, Van, Carro (como entidade)          | Vehicle         |
| Abastecimento                               | Fuel Record     |
| Lançamento                                  | Financial Entry |
| Agenda de viagens                           | Schedule        |

Os termos em português podem aparecer apenas na interface destinada ao usuário final.

Na arquitetura, APIs e banco de dados, utilizar sempre os nomes oficiais em inglês.

---

# 14. Convenções de Idioma

## Backend

Idioma oficial:

**Inglês**

Exemplos:

* Driver
* Vehicle
* Trip
* FinancialEntry

---

## Frontend

Código:

**Inglês**

Interface do usuário:

**Português (MVP)**

A internacionalização será considerada em versões futuras.

---

## Banco de Dados

Todas as tabelas, colunas, índices e constraints utilizarão nomes em inglês.

---

## Documentação

A documentação arquitetural poderá ser escrita em português.

Os nomes das entidades permanecerão em inglês.

---

# 15. Convenções de APIs

Todos os endpoints utilizarão os nomes oficiais do domínio.

Exemplo:

```http
GET /api/v1/drivers
GET /api/v1/vehicles
GET /api/v1/trips
GET /api/v1/financial-entries
```

Nunca utilizar traduções ou abreviações.

---

# 16. Evolução da Linguagem

Sempre que uma nova entidade for criada, ela deverá:

1. Ser definida neste documento.
2. Receber um nome único.
3. Possuir uma descrição objetiva.
4. Ser utilizada de forma consistente em todas as camadas do sistema.

Nenhuma entidade poderá ser implementada sem antes fazer parte da Linguagem Ubíqua.

---

# 17. Glossário de Siglas

| Sigla   | Significado                              |
| ------- | ---------------------------------------- |
| DDD     | Domain-Driven Design                     |
| CQRS    | Command Query Responsibility Segregation |
| DTO     | Data Transfer Object                     |
| API     | Application Programming Interface        |
| JWT     | JSON Web Token                           |
| KPI     | Key Performance Indicator                |
| CRUD    | Create, Read, Update, Delete             |
| ADR     | Architecture Decision Record             |
| EF Core | Entity Framework Core                    |
| RBAC    | Role-Based Access Control                |

---

# 18. Princípio Final

A Linguagem Ubíqua é parte da arquitetura do FleetOS.

Toda comunicação entre negócio, documentação, banco de dados, backend, frontend e agentes de IA deverá utilizar exatamente os termos definidos neste documento.

Qualquer novo conceito introduzido no sistema deverá ser incorporado à Linguagem Ubíqua antes de sua implementação, garantindo consistência, clareza e evolução sustentável da plataforma.
