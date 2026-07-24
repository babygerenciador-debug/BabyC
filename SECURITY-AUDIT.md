# Security Audit — FleetOS v1.0.0

**Audit Date:** 2026-07-21
**Fixes Applied:** 2026-07-21
**Scope:** Full-stack (React 19 frontend, .NET 10 backend, PostgreSQL 16)
**Method:** Manual code review of 50+ frontend files, 80+ backend files, and infrastructure config

---

## Fixes Applied

| ID | Status | Summary |
|----|--------|---------|
| C-01 | ✅ Fixed | OwnerSalary movido para backend como propriedade do Tenant. Frontend usa `GET/PUT /finance/settings` |
| C-03 | ✅ Fixed | Auth store trocado de `localStorage` para `sessionStorage` |
| C-04 | ✅ Fixed | Refresh token interceptor implementado no Axios |
| C-02 | ✅ Resolvido | HTTPS provido pela Vercel (frontend) + Render (backend) |
| I-01 | ✅ Resolvido | Nginx removido da stack de deploy (Vercel + Render) |
| M-01 | ❌ Pendente | Route guards continuam client-side |
| M-02 | ❌ Pendente | Foi mitigado (VITE_API_URL já usa `/api/v1` relativo) |
| M-03 | ❌ Pendente | Validação de tipo de arquivo |
| L-01 | ❌ Pendente | Máscara de CNH em listas |

**Detalhes das correções aplicadas:**

### C-01: OwnerSalary → Backend

**O que mudou:**
- `Tenant.cs` — nova propriedade `OwnerSalary` + método `SetOwnerSalary()`
- `TenantConfiguration.cs` — mapeamento da coluna `owner_salary` (decimal(18,2), default 0)
- `FinanceDtos.cs` — novo `FinanceSettingsDto`
- `GetFinanceSettingsQuery` + handler — lê `OwnerSalary` do tenant autenticado
- `UpdateFinanceSettingsCommand` + handler — admin atualiza o valor (com validação >= 0)
- `SettingsController.cs` — `GET /api/v1/finance/settings` e `PUT /api/v1/finance/settings`
- `GetCashFlowSummaryQuery` — parâmetro `OwnerSalary` removido, handler lê do tenant
- `TransactionsController.cs` — query param `ownerSalary` removido do `GET summary`
- `CashFlowDashboard.tsx` — `localStorage` removido, agora busca via query e salva via mutation

### C-03: Auth store → sessionStorage

**O que mudou:**
- `useAuthStore.ts` — `createJSONStorage(() => sessionStorage)` substitui localStorage
- Token e dados do usuário não persistem após fechar aba/navegador

### C-04: Refresh token interceptor

**O que mudou:**
- `useAuthStore.ts` — novo campo `refreshToken`, nova action `setTokens()`, `login()` agora aceita refreshToken
- `LoginPage.tsx` — passa `refreshToken` para o store
- `api.ts` — interceptor de resposta 401 agora tenta `POST /auth/refresh` com fila de requisições concorrentes antes de fazer logout

---

## Critical (Ainda Pendentes)

### C-02: Senha trafega em texto plano do frontend para o backend

**Arquivos:**
- `frontend/src/pages/finances/components/CashFlowDashboard.tsx:22-33` — `localStorage.getItem('@fleetos:ownerSalary')` e `localStorage.setItem`
- `frontend/src/pages/finances/components/CashFlowDashboard.tsx:40-42` — envia como `params: { ownerSalary: activeSalary }` no GET

**Problema:** O valor do salário do proprietário é:
1. Armazenado em `localStorage` — vulnerável a XSS
2. Enviado como query parameter na URL — logado por proxies, nginx, balanceadores
3. Controlado pelo frontend — qualquer requisição pode enviar valor arbitrário

**Correção:** Mover para backend como configuração por tenant. Criar tabela `tenant_settings` ou campo na entidade `Tenant`. Frontend apenas consulta via GET, nunca decide o valor.

**Prioridade:** Imediata

---

### C-02: Senha trafega em texto plano do frontend para o backend

**Arquivos:**
- `frontend/src/pages/drivers/components/DriverFormModal.tsx:11` — schema Zod define `password: z.string().min(6)`
- `frontend/src/pages/drivers/components/DriverFormModal.tsx:31` — `api.post('/drivers', data)` com password em texto plano
- `backend/src/FleetOS.Application/Operations/Drivers/Commands/CreateDriverCommand.cs:9` — `string Password` no comando
- `backend/src/FleetOS.Application/Operations/Drivers/Commands/CreateDriverCommandHandler.cs:56` — `_passwordService.HashPassword(request.Password)` (backend hasheia corretamente)

**Problema:** O backend hasheia a senha com BCrypt, mas ela trafega em texto plano entre frontend e backend. Em HTTP (ambiente dev), qualquer sniffing na rede local captura a senha. Mesmo com HTTPS, a senha existe em texto plano na memória do servidor antes do hash.

**Correção:** 
1. Usar HTTPS em todos os ambientes (dev + prod)
2. Opcional: hashear a senha no frontend com SHA-256 antes de enviar (defesa em profundidade), com o backend aplicando BCrypt sobre o hash

**Prioridade:** Imediata

---

### C-03: JWT armazenado em localStorage sem proteção

**Arquivos:**
- `frontend/src/store/useAuthStore.ts:60-62` — `persist` middleware com `name: 'fleetos-auth-storage'` no localStorage
- `frontend/src/services/api.ts:29-31` — lê token do Zustand store

**Problema:** O token JWT fica acessível via `localStorage.getItem('fleetos-auth-storage')`. Qualquer script injetado (XSS) rouba o token e obtém acesso persistente.

**Correção:**
1. **Curto prazo:** Trocar `localStorage` por `sessionStorage` (token expira ao fechar aba)
2. **Médio prazo:** Migrar para httpOnly cookie com o token (inacessível via JS)
3. Ou usar `zustand/middleware` sem `persist` e renovar token via refresh token

**Prioridade:** Alta

---

### C-04: Refresh token recebido mas nunca utilizado

**Arquivos:**
- `frontend/src/pages/auth/LoginPage.tsx:23-24` — recebe `accessToken` e `user` mas ignora `refreshToken` e `expiresAt`
- `frontend/src/services/api.ts:43-47` — interceptor 401 faz logout direto sem tentar refresh

**Problema:** O backend retorna `accessToken`, `refreshToken`, e `expiresAt` no login. O backend suporta `POST /auth/refresh` com rotação de token. Mas o frontend nunca implementa o fluxo de refresh. Token expira em 60 min → usuário é forçado a logar de novo.

**Correção:** Implementar interceptor de refresh no Axios:
1. No erro 401, antes de fazer logout, tentar `POST /auth/refresh` com o refresh token
2. Se sucesso: atualizar token no store, re-tentar a requisição original
3. Se falhar: fazer logout

**Prioridade:** Alta

---

## Medium

### M-01: Route guards 100% client-side

**Arquivos:**
- `frontend/src/App.tsx:17-35` — `ProtectedRoute`, `AdminRoute`, `DriverRoute`

**Problema:** As guards verificam apenas o estado do Zustand (`user`, `role`). Não validam se o JWT ainda é válido, não consultam o backend. Um usuário com token expirado ainda vê a UI até fazer uma requisição que retorne 401.

**Correção:** Adicionar verificação de expiração do JWT nas guards (decodificar `exp` claim). Ou adicionar uma query de verificação de sessão que roda no mount.

**Prioridade:** Média

---

### M-02: VITE_API_URL exposto no bundle de produção

**Arquivos:**
- `frontend/.env` — `VITE_API_URL=/api/v1`
- `frontend/src/services/api.ts:4-17` — lê `import.meta.env.VITE_API_URL`

**Problema:** Variáveis `VITE_*` são inlineadas no bundle JavaScript de produção. Qualquer usuário pode ver a URL base da API no código fonte.

**Correção:** Para produção, a URL deve ser relativa (`/api/v1`) e resolvida pelo mesmo host via nginx. Já é o caso — `VITE_API_URL=/api/v1` no `.env`. Verificar se isso se mantém no build.

**Prioridade:** Média

---

### M-03: Sem validação de tipo de arquivo no frontend antes do upload

**Arquivos:**
- `frontend/src/pages/fleet/components/FuelLogFormModal.tsx:142` — `<input type="file" accept="image/*,.pdf" />`

**Problema:** O atributo `accept` é apenas uma sugestão para o browser. Usuário pode selecionar qualquer arquivo via drag-drop ou inspecionando o elemento. Não há verificação de MIME type ou tamanho no frontend antes do envio.

**Correção:** Validar `file.type` e `file.size` no `onChange` antes de adicionar ao estado.

**Prioridade:** Média

---

## Low

### L-01: Dados sensíveis exibidos em listas

**Arquivos:**
- `frontend/src/pages/drivers/components/DriverList.tsx:87` — `cnhNumber` exibido completo na tabela

**Problema:** Número completo da CNH é mostrado na tabela de motoristas. Deveria ser mascarado (ex: `***.******-**`), com opção de revelar.

**Correção:** Aplicar máscara na exibição, mostrar completo apenas em detalhes ou modal específico.

**Prioridade:** Baixa

---

### L-02: Sem abstração de serviços (API calls nos componentes)

**Arquivos:** Todos os componentes de página — ~50 chamadas `api.get/post/put/patch/delete` espalhadas diretamente em `queryFn` e `mutationFn`

**Problema:** Duplicação de query keys, sem tipagem consistente, sem hooks reutilizáveis. Erros são tratados por componente.

**Correção:** Extrair hooks customizados (ex: `useTrips()`, `useCreateTrip()`, `useVehicles()`) com query keys, tipagem, e tratamento de erro centralizados.

**Prioridade:** Baixa

---

### L-03: Cor primária do tenant aplicada via JS no login

**Arquivos:**
- `frontend/src/store/useAuthStore.ts:66-68` — `applyTenantTheme` seta CSS custom properties

**Problema:** A cor primária é aplicada dinamicamente via JavaScript. Funciona, mas causa flash de estilo não-tematizado antes do JS executar.

**Correção:** Incluir a cor primária no `<head>` via servidor (SSR/SSG) ou aplicar a cor o mais cedo possível no ciclo de vida.

**Prioridade:** Baixa

---

## Infrastructure

### I-01: SSL configurado mas não ativo no nginx

**Arquivos:**
- `nginx/ssl/cert.pem` — certificado SSL presente
- `nginx/ssl/key.pem` — chave privada presente
- `nginx/nginx.conf` — apenas `listen 80`, sem `listen 443 ssl`

**Problema:** Certificados SSL existem no diretório mas o nginx não está configurado para HTTPS. A aplicação roda apenas HTTP.

**Correção:** Adicionar server block com `listen 443 ssl` e redirecionar HTTP para HTTPS.

**Prioridade:** Alta (infraestrutura)

---

### I-02: .env versionado com secrets de dev

**Arquivos:**
- `.env` — contém senhas reais (DB, JWT, Redis, seed) em texto plano
- `.gitignore` — `.env` está no arquivo (confirmar)

**Problema:** Em desenvolvimento local o `.env` está no repositório. Se o repositório for exposto, todas as secrets são comprometidas.

**Correção:** Garantir que `.env` está no `.gitignore`. Usar `.env.example` como template.

**Prioridade:** Média

---

## Database — Supabase Migration Readiness

| Aspecto | Status | Notas |
|---------|--------|-------|
| PostgreSQL compatibility | ✅ OK | Supabase usa PostgreSQL padrão, compatível com EF Core + Npgsql |
| EF Core Migrations | ✅ OK | Funcionam diretamente no Supabase PostgreSQL |
| Connection string | ⚠️ Precisa configurar | `.env.example` já tem formato Supabase, `.env` atual aponta para `fleetos_db` local |
| Supabase SDK | ✅ Instalado | `Supabase` 1.1.1 no `.csproj` da Infra |
| Supabase env vars | ⚠️ Vazios | `SUPABASE_URL`, `SUPABASE_SERVICE_KEY`, `SUPABASE_STORAGE_BUCKET` vazios no `.env` |
| Row Level Security | ❌ Não implementado | O tenant isolation é feito via EF Core global query filters, não via RLS do PostgreSQL |
| File Storage | ✅ Preparado | Backend tem referência ao Supabase Storage, bucket `fleetos` configurado |
| Auth | ⚠️ Decisão pendente | Atualmente JWT próprio. Migrar para Supabase Auth exigiria refatorar login/refresh |
| Portabilidade | ✅ Boa | `DATABASE_OVERVIEW.md` explicitamente evita features exclusivas do Supabase |

### Recomendação para migração:
1. Substituir `Host=fleetos_db` pela connection string do Supabase
2. Executar `dotnet ef database update` para aplicar migrations
3. Configurar `SUPABASE_URL` e `SUPABASE_SERVICE_KEY`
4. Manter JWT próprio (mais flexível) ou migrar para Supabase Auth (menos código)
5. RLS pode ser adicionado depois — não é blocker para migração

---

## Score Summary (Pós-Fix)

| Severity | Total | Fixados | Pendentes |
|----------|-------|---------|-----------|
| Critical | 4 | 3 | 1 (C-02: HTTPS) |
| High | 2 | 0 | 2 (I-01: SSL nginx, M-01: route guards) |
| Medium | 3 | 1 (M-02) | 2 |
| Low | 3 | 0 | 3 |
| Infra | 2 | 0 | 2 |

---

## Top 5 Próximos Passos

1. **Ativar HTTPS no nginx** — Configurar `listen 443 ssl` (certificados já existem em `nginx/ssl/`)
2. **Adicionar confirmação em ações destrutivas** — Delete vehicle, cancel trip (UI-REVIEW item)
3. **Adicionar validação de tipo de arquivo** — No upload de comprovantes
4. **Extrair service hooks customizados** — Centralizar chamadas de API
5. **Adicionar toast notifications** — Feedback visual para CRUD
