# DATABASE_OVERVIEW.md

> **Projeto:** FleetOS – Enterprise Fleet Management Platform
> **Versão:** 1.0.0
> **Status:** Arquitetura Oficial do Banco de Dados
> **Objetivo:** Definir a estratégia de persistência do FleetOS.

---

# 1. Objetivo

Este documento define a arquitetura oficial do banco de dados do FleetOS.

Ele estabelece as convenções, padrões e princípios que deverão ser seguidos por todas as entidades, migrations, consultas e módulos da plataforma.

O objetivo é garantir consistência, desempenho, escalabilidade e facilidade de manutenção.

---

# 2. Banco de Dados

O MVP utilizará:

**PostgreSQL 16**

Hospedado inicialmente no:

* Supabase

A arquitetura não deverá depender de recursos exclusivos do Supabase, permitindo migração futura para qualquer ambiente PostgreSQL.

---

# 3. Filosofia

O banco será projetado para:

* Simplicidade.
* Escalabilidade.
* Multi-Tenant.
* Alto desempenho.
* Fácil manutenção.
* Evolução contínua.

A modelagem privilegiará clareza e consistência em vez de otimizações prematuras.

---

# 4. Estratégia Multi-Tenant

O FleetOS utilizará:

**Shared Database**

**Shared Schema**

O isolamento ocorrerá através de filtros lógicos.

Toda entidade operacional deverá conter:

* TenantId
* OrganizationId
* BusinessUnitId

Esses campos fazem parte do contexto organizacional e não poderão ser omitidos.

---

# 5. Organização do Banco

Será utilizado um único schema principal no MVP:

```text
public
```

Todos os módulos compartilharão este schema.

No futuro, novos schemas poderão ser introduzidos para requisitos específicos, sem alterar o modelo de domínio.

---

# 6. Convenções de Tabelas

Regras obrigatórias:

* Nome no singular.
* PascalCase.
* Sem prefixos.
* Sem abreviações desnecessárias.

Exemplos:

```text
User
Role
Driver
Vehicle
Trip
FuelRecord
Maintenance
InventoryItem
FinancialEntry
```

---

# 7. Convenções de Colunas

Todas as tabelas deverão seguir o mesmo padrão.

Exemplo mínimo:

```text
Id
TenantId
OrganizationId
BusinessUnitId

CreatedAt
CreatedBy

UpdatedAt
UpdatedBy

DeletedAt
DeletedBy

RowVersion
```

---

# 8. Chaves Primárias

Toda tabela utilizará:

```text
Id
```

Tipo:

```text
UUID (GUID)
```

Gerado na aplicação.

Não utilizar inteiros auto incrementais.

---

# 9. Chaves Estrangeiras

Sempre utilizar:

```text
<Entity>NameId
```

Exemplos:

```text
DriverId

VehicleId

TripId

UserId

OrganizationId
```

Nunca utilizar nomes ambíguos.

---

# 10. Auditoria

Todas as tabelas deverão possuir:

```text
CreatedAt

CreatedBy

UpdatedAt

UpdatedBy
```

Sempre preenchidos automaticamente.

---

# 11. Soft Delete

Nenhuma entidade operacional será removida fisicamente.

Será utilizado:

```text
DeletedAt

DeletedBy
```

As consultas utilizarão filtros globais para ignorar registros excluídos.

---

# 12. Controle de Concorrência

Todas as entidades possuirão:

```text
RowVersion
```

Utilizado para:

* Optimistic Concurrency
* Evitar sobrescritas
* Controle de conflitos

---

# 13. Índices

Todo índice deverá possuir justificativa arquitetural.

Prioridades:

* TenantId
* OrganizationId
* BusinessUnitId
* Campos utilizados em filtros
* Campos utilizados em ordenação
* Chaves estrangeiras

Evitar índices desnecessários.

---

# 14. Integridade Referencial

Todas as relações utilizarão Foreign Keys.

Não utilizar dados órfãos.

Toda exclusão lógica deverá respeitar as dependências entre entidades.

---

# 15. Normalização

Objetivo:

Terceira Forma Normal (3FN).

Desnormalizações somente serão permitidas quando documentadas e justificadas por requisitos de desempenho.

---

# 16. Paginação

Toda consulta de listagem deverá utilizar paginação.

Nunca retornar listas ilimitadas.

---

# 17. Ordenação

Toda consulta deverá possuir ordenação explícita.

Nunca depender da ordem física do banco.

---

# 18. Pesquisa

Sempre que possível utilizar:

* LIKE para pesquisas simples.
* ILIKE para buscas case insensitive.

A adoção de Full Text Search ficará para versões futuras.

---

# 19. Transações

Toda operação que modificar múltiplas entidades deverá ser executada dentro de uma transação.

Utilizar Unit of Work como padrão.

---

# 20. Migrations

Todas as alterações estruturais deverão ser realizadas através de Entity Framework Core Migrations.

Nunca alterar o banco manualmente em produção.

---

# 21. Performance

Boas práticas obrigatórias:

* Consultas assíncronas.
* Selecionar apenas colunas necessárias.
* Evitar SELECT *.
* Evitar consultas N+1.
* Utilizar projeções para listagens.
* Utilizar índices planejados.

---

# 22. Segurança

Nunca armazenar:

* Senhas em texto.
* Tokens.
* Chaves privadas.

Senhas deverão utilizar BCrypt.

---

# 23. Armazenamento de Arquivos

Arquivos não serão armazenados no banco.

Utilizar:

Supabase Storage.

O banco armazenará apenas:

* Caminho.
* Nome.
* Tipo.
* Tamanho.
* Hash.
* Data de envio.

---

# 24. Logs

Logs operacionais não serão armazenados nas tabelas de negócio.

Serão registrados por meio do sistema de auditoria e do Serilog.

---

# 25. Estratégia de Crescimento

O banco deverá suportar:

* Milhares de tenants.
* Milhões de registros.
* Crescimento modular.
* Inclusão de novos módulos sem impacto estrutural.

---

# 26. Convenções de Relacionamentos

Utilizar:

* 1:1 quando necessário.
* 1:N como padrão.
* N:N apenas por tabelas de junção explícitas.

Nunca utilizar relacionamentos implícitos.

---

# 27. Padrão para Tabelas de Junção

Exemplo:

```text
UserRole

RolePermission

TripDriver
```

Essas tabelas também deverão possuir auditoria e contexto organizacional quando fizer sentido.

---

# 28. Estratégia de Seed

O sistema possuirá seeds apenas para:

* Papéis padrão.
* Permissões padrão.
* Configurações iniciais.
* Tenant de desenvolvimento.

Nunca inserir dados operacionais via seed.

---

# 29. Ambiente de Desenvolvimento

Durante o desenvolvimento:

* Banco criado por migrations.
* Dados de teste gerados por seed controlada.
* Nenhuma alteração manual permitida.

---

# 30. Observabilidade

Toda consulta crítica deverá ser monitorada.

Métricas futuras:

* Tempo médio de execução.
* Consultas lentas.
* Locks.
* Deadlocks.
* Uso de índices.

Essas informações servirão para otimizações futuras.

---

# 31. Princípio Final

O banco de dados do FleetOS é parte da arquitetura do produto.

Toda decisão de modelagem deverá priorizar consistência, isolamento entre tenants, desempenho previsível e facilidade de evolução.

Nenhuma tabela, coluna ou relacionamento poderá ser criado sem seguir as convenções estabelecidas neste documento.
