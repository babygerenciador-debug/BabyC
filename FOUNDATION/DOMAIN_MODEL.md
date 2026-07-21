# DOMAIN_MODEL.md

> **Projeto:** Baby Turismo Fleet Management System (BTFMS)
> **Versão:** 1.0.0
> **Documento Oficial do Domínio**
> **Baseado em Domain-Driven Design (DDD)**

---

# 1. Objetivo

Este documento define o modelo de domínio do Baby Turismo Fleet Management System (BTFMS).

O domínio representa o coração do sistema.

Toda regra de negócio deverá nascer neste documento antes de ser implementada.

Nenhuma entidade poderá ser criada sem estar documentada aqui.

---

# 2. Linguagem Ubíqua (Ubiquitous Language)

Todos os desenvolvedores e agentes deverão utilizar exatamente os mesmos termos.

| Termo         | Significado                                           |
| ------------- | ----------------------------------------------------- |
| Empresa       | Organização que utiliza o sistema                     |
| Usuário       | Pessoa autenticada na plataforma                      |
| Papel         | Conjunto de permissões (Role)                         |
| Permissão     | Ação autorizada                                       |
| Motorista     | Colaborador responsável pela condução do veículo      |
| Ônibus        | Veículo pertencente à frota                           |
| Viagem        | Serviço executado utilizando um ônibus e um motorista |
| Agenda        | Planejamento das viagens                              |
| Abastecimento | Registro de consumo de combustível                    |
| Manutenção    | Registro de manutenção preventiva ou corretiva        |
| Checklist     | Lista de verificação obrigatória                      |
| Documento     | Arquivo relacionado a uma entidade                    |
| Observação    | Comunicação operacional                               |
| Estoque       | Controle de itens utilizados pela empresa             |
| Produto       | Item armazenado em estoque                            |
| Dashboard     | Painel de indicadores                                 |
| Indicador     | Métrica calculada                                     |
| Relatório     | Documento analítico                                   |

Todos os documentos futuros deverão utilizar exatamente essa terminologia.

---

# 3. Visão Geral do Domínio

O domínio será dividido em Bounded Contexts.

```text
Empresa
│
├── Identidade
├── Operação
├── Frota
├── Financeiro
├── Estoque
├── Analytics
├── Configuração
└── Auditoria
```

Cada contexto possui autonomia e responsabilidades próprias.

---

# 4. Bounded Contexts

## 4.1 Identidade

Responsável por:

* Usuários
* Perfis
* Papéis
* Permissões
* Login
* Tokens
* Auditoria de acesso

### Entidades

* Empresa
* Usuário
* Papel
* Permissão
* RefreshToken

---

## 4.2 Operação

Responsável por:

* Motoristas
* Agenda
* Viagens
* Checklists
* Observações

### Entidades

* Motorista
* Viagem
* Agenda
* Checklist
* ChecklistResposta
* Observação

---

## 4.3 Frota

Responsável por:

* Ônibus
* Abastecimentos
* Manutenções
* Documentos

### Entidades

* Ônibus
* DocumentoVeículo
* Abastecimento
* Manutenção

---

## 4.4 Financeiro

Responsável por:

* Receitas
* Despesas
* Fluxo de Caixa
* Categorias
* Centros de Custo

---

## 4.5 Estoque

Responsável por:

* Produtos
* Entradas
* Saídas
* Movimentações
* Fornecedores

---

## 4.6 Analytics

Responsável por:

* Dashboards
* Widgets
* Indicadores
* Relatórios
* Exportações

---

## 4.7 Configuração

Responsável por:

* Dados da Empresa
* SMTP
* Preferências
* Temas
* Integrações

---

## 4.8 Auditoria

Responsável por:

* Logs
* Histórico
* Alterações
* Rastreabilidade

---

# 5. Entidades Raiz (Aggregate Roots)

As seguintes entidades serão raízes de agregados:

* Empresa
* Usuário
* Motorista
* Ônibus
* Viagem
* Abastecimento
* Manutenção
* Produto
* Receita
* Despesa
* Dashboard

Cada Aggregate Root controla a consistência de seu agregado.

---

# 6. Value Objects

Os seguintes conceitos serão modelados como Value Objects:

* CPF
* CNPJ
* E-mail
* Telefone
* Endereço
* Quilometragem
* Placa
* Chassi
* Renavam
* Dinheiro
* Período
* Coordenadas (futuro)

Value Objects são imutáveis e não possuem identidade própria.

---

# 7. Entidade Empresa

A Empresa representa o tenant do sistema.

Toda informação operacional pertence obrigatoriamente a uma Empresa.

Relacionamentos principais:

* Usuários
* Motoristas
* Ônibus
* Viagens
* Produtos
* Receitas
* Despesas
* Configurações

---

# 8. Entidade Usuário

Representa um colaborador autenticado.

Responsabilidades:

* autenticação
* autorização
* auditoria
* preferências

Relacionamentos:

* Empresa
* Papéis
* Permissões

---

# 9. Entidade Motorista

Representa um colaborador apto a executar viagens.

Responsabilidades:

* manter documentos
* participar de viagens
* registrar abastecimentos
* realizar checklists
* enviar observações

Regras:

* CNH válida para iniciar viagem.
* Apenas motoristas ativos podem ser vinculados a viagens.
* Um motorista não pode estar em duas viagens simultaneamente.

---

# 10. Entidade Ônibus

Representa um veículo da frota.

Relacionamentos:

* Empresa
* Viagens
* Abastecimentos
* Manutenções
* Documentos

Regras:

* Não pode realizar duas viagens simultaneamente.
* Deve possuir documentação válida.
* Deve estar em situação operacional.

---

# 11. Entidade Viagem

Representa um serviço prestado.

Relacionamentos:

* Empresa
* Motorista
* Ônibus
* Checklist
* Observações

Regras:

* Deve possuir exatamente um motorista responsável.
* Deve possuir exatamente um ônibus.
* Não pode iniciar sem checklist obrigatório.
* Não pode ser iniciada com documentos vencidos.

---

# 12. Entidade Abastecimento

Representa um abastecimento realizado em um veículo.

Relacionamentos:

* Ônibus
* Motorista
* Empresa

Regras:

* Quilometragem obrigatória.
* Litros obrigatórios.
* Valor obrigatório.
* Não permitir quilometragem inferior ao último abastecimento.

---

# 13. Entidade Manutenção

Representa uma intervenção no veículo.

Tipos:

* Preventiva
* Corretiva

Relacionamentos:

* Ônibus
* Empresa

---

# 14. Entidade Produto

Representa um item do estoque.

Responsabilidades:

* Controle de quantidade
* Controle mínimo
* Histórico de movimentações

---

# 15. Entidade Dashboard

Representa um painel personalizado do usuário.

Relacionamentos:

* Usuário
* Widgets

Cada usuário poderá possuir vários dashboards.

---

# 16. Eventos de Domínio

Exemplos:

* EmpresaCriada
* UsuárioCriado
* MotoristaCadastrado
* ViagemAgendada
* ViagemIniciada
* ViagemFinalizada
* ÔnibusCadastrado
* ManutençãoRegistrada
* AbastecimentoRegistrado
* ProdutoMovimentado
* EstoqueAbaixoDoMínimo

Esses eventos poderão ser utilizados futuramente para notificações e integrações.

---

# 17. Invariantes do Domínio

O domínio deve garantir, entre outras, as seguintes regras:

* Nenhuma viagem sem motorista.
* Nenhuma viagem sem ônibus.
* Nenhum ônibus em duas viagens simultâneas.
* Nenhum motorista em duas viagens simultâneas.
* Nenhuma viagem iniciada com checklist pendente.
* Nenhum abastecimento com quilometragem regressiva.
* Nenhum documento vencido pode ser ignorado.

Essas regras pertencem ao domínio, não ao frontend.

---

# 18. Objetos Compartilhados

Os seguintes objetos poderão ser reutilizados entre contextos:

* Documento
* Anexo
* Endereço
* Contato
* Arquivo
* Auditoria
* Paginação
* Filtro
* Ordenação

---

# 19. Evolução do Domínio

Novos módulos deverão ser adicionados preferencialmente como novos Bounded Contexts, evitando aumentar responsabilidades de contextos existentes.

Mudanças relevantes no domínio deverão gerar um ADR e atualização deste documento.

---

# 20. Princípio Final

O domínio é a principal fonte de verdade do BTFMS.

Banco de dados, API, frontend e testes devem refletir este modelo, nunca defini-lo.

Qualquer alteração nas regras de negócio deve começar neste documento antes de ser implementada em código.
