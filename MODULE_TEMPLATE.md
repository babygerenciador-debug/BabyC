# MODULE_TEMPLATE.md

> **Projeto:** FleetOS – Enterprise Fleet Management Platform
> **Versão:** 1.0.0
> **Status:** Template Oficial para Desenvolvimento de Módulos
> **Objetivo:** Padronizar a criação de qualquer módulo do FleetOS.

---

# 1. Objetivo

Todo módulo do FleetOS deverá seguir este template.

O objetivo é garantir:

* Consistência arquitetural.
* Padronização do código.
* Facilidade de manutenção.
* Escalabilidade.
* Compatibilidade com desenvolvimento assistido por IA.

Nenhum módulo poderá ser implementado sem seguir esta estrutura.

---

# 2. Estrutura de Alto Nível

Todo módulo deverá ser composto pelas seguintes camadas:

```text
Business
    ↓
Domain
    ↓
Database
    ↓
Application
    ↓
API
    ↓
Frontend
    ↓
Tests
```

Cada camada possui responsabilidades específicas.

---

# 3. Estrutura de Documentação

Cada módulo deverá possuir sua própria pasta em `docs/08-MODULES`.

Exemplo para o módulo **Drivers**:

```text
DRIVERS/

README.md

DOMAIN.md

ERD.md

TABLES.md

BUSINESS_RULES.md

USE_CASES.md

API.md

FRONTEND.md

TESTS.md

CHECKLIST.md
```

A mesma estrutura será utilizada para qualquer módulo futuro.

---

# 4. README.md

Responsável por apresentar o módulo.

Deve conter:

* Objetivo.
* Escopo.
* Funcionalidades.
* Dependências.
* Fluxo principal.
* Relação com outros módulos.

---

# 5. DOMAIN.md

Representa o modelo de domínio.

Deve conter:

* Aggregate Root.
* Entidades.
* Value Objects.
* Eventos.
* Estados.
* Invariantes.
* Regras de negócio.
* Responsabilidades.
* Casos de uso relacionados.

---

# 6. ERD.md

Define a modelagem do banco.

Deve conter:

* Diagrama Mermaid.
* Relacionamentos.
* Cardinalidade.
* Chaves.
* Dependências.

---

# 7. TABLES.md

Especificação física das tabelas.

Para cada tabela documentar:

* Nome.
* Colunas.
* Tipos.
* Constraints.
* Índices.
* Chaves estrangeiras.
* Campos de auditoria.
* Soft Delete.

---

# 8. BUSINESS_RULES.md

Regras específicas do módulo.

Cada regra deverá possuir:

* ID.
* Prioridade.
* Descrição.
* Justificativa.
* Exceções.
* Critérios de teste.

Todas as regras devem complementar o `BUSINESS_RULES.md` global.

---

# 9. USE_CASES.md

Cada funcionalidade deverá possuir um caso de uso.

Estrutura:

* Objetivo.
* Atores.
* Pré-condições.
* Fluxo principal.
* Fluxos alternativos.
* Pós-condições.
* Regras aplicáveis.
* Eventos gerados.
* Permissões necessárias.

---

# 10. API.md

Especificação completa da API.

Cada endpoint deverá conter:

* Método HTTP.
* URL.
* Permissões.
* Request.
* Response.
* Possíveis erros.
* Regras de negócio relacionadas.
* Exemplos.

---

# 11. FRONTEND.md

Documentação da interface.

Para cada tela documentar:

* Objetivo.
* Componentes.
* Fluxo.
* Estados.
* Validações.
* Permissões.
* Responsividade.
* Integração com API.

---

# 12. TESTS.md

Plano de testes do módulo.

Dividir em:

* Testes unitários.
* Testes de integração.
* Testes de autorização.
* Testes de validação.
* Testes de performance (quando aplicável).

Cada regra de negócio deve possuir pelo menos um teste correspondente.

---

# 13. CHECKLIST.md

Documento utilizado para acompanhar a implementação.

Modelo:

```text
[ ] Documentação concluída

[ ] Entidades

[ ] Value Objects

[ ] Repositories

[ ] Commands

[ ] Queries

[ ] Validators

[ ] DTOs

[ ] Migrations

[ ] Controllers

[ ] Frontend

[ ] Testes Unitários

[ ] Testes Integração

[ ] Revisão

[ ] Documentação Atualizada
```

---

# 14. Estrutura do Backend

Todo módulo deverá utilizar a seguinte organização:

```text
Module/

Application/

Commands/

Queries/

DTOs/

Validators/

Handlers/

Mappings/

Domain/

Entities/

Events/

ValueObjects/

Repositories/

Specifications/

Infrastructure/

Persistence/

Configurations/

API/

Controllers/

Tests/
```

---

# 15. Estrutura do Frontend

Cada módulo seguirá o padrão:

```text
src/modules/

module/

pages/

components/

hooks/

services/

schemas/

types/

store/

utils/

routes/
```

---

# 16. Convenções

Todo módulo deverá:

* Respeitar o `CODING_STANDARDS.md`.
* Utilizar a `UBIQUITOUS_LANGUAGE.md`.
* Seguir o `SYSTEM_ARCHITECTURE.md`.
* Respeitar o `DATABASE_OVERVIEW.md`.
* Aplicar as regras do `BUSINESS_RULES.md`.

Nenhum módulo poderá criar padrões próprios.

---

# 17. Dependências

Antes da implementação, identificar:

* Quais módulos consome.
* Quais módulos publica eventos.
* Quais módulos serão impactados por alterações.

Dependências devem ser explícitas.

---

# 18. Eventos de Domínio

Cada módulo deverá listar:

Eventos emitidos.

Exemplo:

```text
DriverCreated

DriverBlocked

DriverLicenseExpired
```

Eventos consumidos.

Exemplo:

```text
TripScheduled

MaintenanceCompleted
```

---

# 19. Segurança

Todo módulo deverá documentar:

* Permissões.
* Papéis autorizados.
* Validações de Tenant.
* Validações de Organization.
* Validações de Business Unit.
* Auditoria obrigatória.

---

# 20. Observabilidade

Cada módulo deverá definir:

* Logs importantes.
* Métricas.
* Eventos auditáveis.
* Alertas futuros.

---

# 21. Escalabilidade

Cada módulo deve ser projetado considerando:

* Crescimento do volume de dados.
* Paginação.
* Consultas otimizadas.
* Processamento assíncrono quando necessário.
* Independência para futura extração como microserviço, sem exigir mudanças no domínio.

---

# 22. Critérios de Pronto (Definition of Done)

Um módulo será considerado concluído apenas quando:

* Toda a documentação estiver finalizada.
* Todas as regras de negócio estiverem implementadas.
* Todos os endpoints estiverem documentados.
* Todas as migrations estiverem criadas.
* Todos os testes obrigatórios estiverem aprovados.
* O frontend estiver integrado.
* A auditoria estiver funcionando.
* A documentação estiver sincronizada com o código.

---

# 23. Template de Entidade

Cada entidade principal deverá possuir um documento próprio contendo:

* Objetivo.
* Responsabilidades.
* Aggregate Root.
* Value Objects.
* Atributos.
* Estados.
* Métodos de domínio.
* Eventos.
* Regras de negócio.
* Relacionamentos.
* Permissões.
* Casos de uso.
* Validações.
* Auditoria.
* APIs relacionadas.
* Componentes de frontend.
* Testes obrigatórios.

---

# 24. Checklist para Agentes de IA

Antes de iniciar um módulo:

* Ler `PROJECT_RULES.md`.
* Ler `CODING_STANDARDS.md`.
* Ler `SYSTEM_ARCHITECTURE.md`.
* Ler `DATABASE_OVERVIEW.md`.
* Ler `UBIQUITOUS_LANGUAGE.md`.
* Ler `BUSINESS_RULES.md`.
* Ler este documento (`MODULE_TEMPLATE.md`).

Durante a implementação:

* Atualizar a documentação primeiro.
* Implementar seguindo DDD, CQRS e Clean Architecture.
* Manter compatibilidade Multi-Tenant.
* Garantir cobertura mínima de testes.

Antes de concluir:

* Validar checklist.
* Revisar documentação.
* Garantir consistência entre código e documentação.

---

# 25. Princípio Final

No FleetOS, módulos não são apenas conjuntos de tabelas ou telas.

Cada módulo representa um domínio de negócio claramente definido, com responsabilidades, regras, eventos e contratos bem estabelecidos.

Toda implementação deve nascer da documentação, e toda evolução do sistema deve preservar a consistência arquitetural da plataforma.

O objetivo é que qualquer desenvolvedor ou agente de IA consiga compreender, evoluir e manter o FleetOS utilizando a documentação como fonte primária de conhecimento, garantindo previsibilidade, qualidade e escalabilidade ao longo de toda a vida do produto.