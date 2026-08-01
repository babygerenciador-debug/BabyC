# Codebase Structure

**Analysis Date:** 2026-08-01

## Directory Layout

```
BabyC/
├── backend/                        # ASP.NET Core 10 solution (FleetOS.sln)
│   ├── src/
│   │   ├── FleetOS.Api/            # HTTP host, middleware, controllers
│   │   ├── FleetOS.Application/    # CQRS commands/queries, DTOs, validators
│   │   ├── FleetOS.Domain/         # Entities, aggregates, value objects, events
│   │   ├── FleetOS.Infrastructure/ # EF Core, repositories, services, SignalR, jobs
│   │   └── FleetOS.Shared/         # Result<T>, Error, pagination
│   ├── tests/
│   │   └── FleetOS.Tests/          # xUnit test project (scaffolded; no tests yet)
│   ├── Dockerfile
│   ├── docker-compose.yml
│   ├── render.yaml
│   ├── run_migrations.sh
│   ├── Directory.Build.props
│   └── FleetOS.sln
├── frontend/                       # React 19 + Vite SPA
│   ├── src/
│   │   ├── App.tsx                 # Routing + role guards + providers
│   │   ├── main.tsx                # React root mount
│   │   ├── index.css               # Global CSS (Tailwind entry)
│   │   ├── components/             # Layout + shared components
│   │   ├── pages/                  # Feature modules (one folder per route)
│   │   ├── services/               # Axios client + TanStack Query client
│   │   ├── store/                  # Zustand stores (auth, theme)
│   │   ├── hooks/                  # useSignalR + custom hooks
│   │   ├── types/                  # Shared TypeScript types
│   │   ├── styles/                 # CSS modules / global styles
│   │   └── assets/                 # Images, fonts
│   ├── public/                     # Static assets served as-is (LOGO.png, etc.)
│   ├── index.html                  # SPA shell
│   ├── Dockerfile                  # Multi-stage: build → nginx
│   ├── nginx.conf                  # SPA fallback + /api proxy
│   ├── vercel.json                 # Vercel deployment config
│   ├── vite.config.ts              # Vite + React plugin
│   ├── tsconfig.json               # TS solution config
│   ├── tsconfig.app.json           # Frontend TS config
│   ├── tsconfig.node.json          # Vite config TS config
│   ├── .oxlintrc.json              # OxLint config
│   └── package.json
├── nginx/                          # Root reverse-proxy config (docker-compose)
│   ├── nginx.conf
│   └── ssl/                        # TLS certificates
├── DATABASE/                       # Database documentation (not SQL)
│   ├── DATABASE_OVERVIEW.md
│   └── ERD/
│       └── CORE_ERD.md
├── DOMAIN/                         # Domain documentation (portuguese)
│   └── CORE/
│       ├── CORE_CONTEXT.md
│       ├── BusinessUnit.md
│       └── DIAGRAMS/
│           ├── Tenant.md
│           └── Organization.md
├── FLEET/
│   └── User.md                     # User-module notes
├── FOUNDATION/                     # Project rules, architecture, coding standards
│   ├── SYSTEM_ARCHITECTURE.md
│   ├── DOMAIN_MODEL.md
│   ├── BUSINESS_RULES.md
│   ├── TENANT_MODEL.md
│   ├── CODING_STANDARDS.md
│   ├── PROJECT_RULES.md
│   ├── AI_DEVELOPMENT_GUIDE.md
│   ├── MVP_SCOPE.md
│   ├── UBIQUITOUS_LANGUAGE.md
│   └── README.md
├── docker-compose.yml              # api + frontend + redis + nginx (prod-like)
├── .env.example                    # Required env vars template
├── .gitignore
├── IMPLEMENTATION_ROADMAP.md
├── IMPLEMENTATION_SUMMARY.md
├── MODULE_TEMPLATE.md
├── SECURITY-AUDIT.md
└── UI-REVIEW.md
```

## Directory Purposes

**`backend/src/FleetOS.Api/`** — ASP.NET Core host
- Purpose: Composition root, middleware pipeline, HTTP endpoints
- Contains: `Program.cs`, `Controllers/` (base + auth), `Features/<module>/` (feature controllers), `Middleware/`, `Extensions/`, `Services/`, `Errors/`
- Key files: `Program.cs` (startup), `Controllers/BaseController.cs` (Mediator + TenantContext), `Extensions/MiddlewareExtensions.cs` (CorrelationId, TenantResolver)

**`backend/src/FleetOS.Application/`** — Use cases
- Purpose: CQRS commands/queries, validators, DTOs, repository interfaces
- Contains: One folder per module (`Auth/`, `Fleet/`, `Operations/`, `Finance/`, `Inventory/`, `VehicleIssues/`, `Notifications/`, `Dashboard/`) each with `Commands/` + `Queries/`
- Key files: `ApplicationServiceExtensions.cs`, `Common/Behaviors/ValidationBehavior.cs`, `Common/Interfaces/*`

**`backend/src/FleetOS.Domain/`** — Domain model
- Purpose: Entities, aggregates, value objects, domain events, repository interfaces
- Contains: `Common/` (Entity, AggregateRoot, ValueObject, interfaces), `Core/` (Tenants, Users), `Fleet/` (Vehicles, Fuel, Maintenance, VehicleIssues), `Operations/` (Drivers, Trips, Checklists), `Finance/`, `Inventory/`
- Key files: `Common/Entity.cs`, `Common/AggregateRoot.cs`, `Common/Interfaces/IRepository.cs`

**`backend/src/FleetOS.Infrastructure/`** — Adapters
- Purpose: EF Core persistence, external services, realtime, background jobs
- Contains: `Persistence/` (DbContext, configurations, repositories, interceptors, migrations), `Services/` (JWT, BCrypt, Supabase, notifications), `Hubs/` (SignalR), `BackgroundJobs/`
- Key files: `InfrastructureServiceExtensions.cs`, `Persistence/FleetOsDbContext.cs`, `Persistence/Interceptors/AuditInterceptor.cs`

**`backend/src/FleetOS.Shared/`** — Cross-cutting types
- Purpose: Types shared between all layers (no layer-specific deps)
- Contains: `Results/` (Result<T>, Error), `Pagination/` (PagedQuery, PagedResult)
- Key files: `Results/Result.cs`, `Results/Error.cs`

**`backend/tests/FleetOS.Tests/`** — Test project
- Purpose: xUnit tests (scaffolded; currently empty)
- Contains: `FleetOS.Tests.csproj` only

**`frontend/src/pages/`** — Feature modules
- Purpose: One folder per route, each with a `*Page.tsx` plus co-located components
- Contains: `auth/`, `dashboard/`, `drivers/`, `driver/` (portal), `fleet/`, `maintenance/`, `trips/`, `inventory/`, `finances/`
- Pattern: `<feature>/<Feature>Page.tsx` + `<feature>/components/<Widget>.tsx`

**`frontend/src/components/`** — Shared UI
- Purpose: Layout shell + reusable components
- Contains: `layout/MainLayout.tsx` (sidebar + topbar + notifications), `shared/BaseModal.tsx`

**`frontend/src/services/`** — API client
- Purpose: Centralized HTTP client and query cache
- Contains: `api.ts` (axios + interceptors), `queryClient.ts` (TanStack Query defaults)

**`frontend/src/store/`** — Zustand stores
- Purpose: Client state that outlives components
- Contains: `useAuthStore.ts` (JWT, user, theme; persisted to sessionStorage), `useThemeStore.ts` (dark/light)

**`frontend/src/hooks/`** — Custom hooks
- Purpose: Cross-cutting React behavior
- Contains: `useSignalR.ts` (singleton SignalR connection + cache invalidation)

**`FOUNDATION/`** — Project documentation
- Purpose: Source of truth for architecture, rules, domain model
- Key files: `SYSTEM_ARCHITECTURE.md`, `CODING_STANDARDS.md`, `BUSINESS_RULES.md`, `TENANT_MODEL.md`

**`DOMAIN/` + `DATABASE/`** — Reference docs
- Purpose: Domain context maps, ER diagrams
- Key files: `DOMAIN/CORE/CORE_CONTEXT.md`, `DATABASE/ERD/CORE_ERD.md`, `DATABASE/DATABASE_OVERVIEW.md`

## Key File Locations

**Entry Points:**
- `backend/src/FleetOS.Api/Program.cs`: API host startup, middleware pipeline, DI composition
- `backend/src/FleetOS.Infrastructure/Persistence/DbInitializer.cs`: auto-migration + seed on boot
- `frontend/src/main.tsx`: React DOM mount
- `frontend/src/App.tsx`: Router, providers, role guards
- `docker-compose.yml`: prod-like topology (api + frontend + redis + nginx)

**Configuration:**
- `backend/Directory.Build.props`: shared MSBuild properties
- `backend/src/FleetOS.Api/FleetOS.Api.csproj`: API deps
- `backend/src/FleetOS.Infrastructure/FleetOS.Infrastructure.csproj`: EF Core + Npgsql + Supabase + Redis
- `frontend/vite.config.ts`: Vite config, proxy rules
- `frontend/tsconfig.json`, `tsconfig.app.json`, `tsconfig.node.json`: TS solution
- `frontend/.oxlintrc.json`: OxLint rules
- `nginx/nginx.conf`: root reverse proxy (TLS termination + /api → backend, / → frontend)
- `.env.example`: required env-var names (JWT, DB, Redis, Supabase, Seed, CORS)
- `backend/render.yaml`: Render.com deployment manifest
- `frontend/vercel.json`: Vercel deployment config

**Core Logic (backend):**
- `backend/src/FleetOS.Domain/Common/Entity.cs`: base entity (Id, tenant context, audit, soft delete)
- `backend/src/FleetOS.Domain/Common/AggregateRoot.cs`: aggregate with domain events
- `backend/src/FleetOS.Domain/<Module>/`: entities per bounded context
- `backend/src/FleetOS.Application/<Module>/Commands|Queries/`: use cases
- `backend/src/FleetOS.Infrastructure/Persistence/FleetOsDbContext.cs`: EF Core + global filters
- `backend/src/FleetOS.Infrastructure/Persistence/Configurations/*.cs`: IEntityTypeConfiguration per entity
- `backend/src/FleetOS.Infrastructure/Persistence/Repositories/*.cs`: IRepository implementations

**Core Logic (frontend):**
- `frontend/src/services/api.ts`: axios client, JWT refresh, toast mapping
- `frontend/src/services/queryClient.ts`: TanStack defaults (5min staleTime, 1 retry)
- `frontend/src/store/useAuthStore.ts`: auth state + tenant branding
- `frontend/src/hooks/useSignalR.ts`: realtime + query invalidation
- `frontend/src/types/index.ts`: API type contracts (shared with backend DTOs)
- `frontend/src/components/layout/MainLayout.tsx`: shell with sidebar, topbar, notifications

**Testing:**
- `backend/tests/FleetOS.Tests/FleetOS.Tests.csproj`: xUnit project (no tests yet)
- No frontend test config exists (no vitest/jest)

## Naming Conventions

**Files (backend):**
- `PascalCase.cs` for classes/records/interfaces (e.g. `Vehicle.cs`, `CreateVehicleCommand.cs`, `IVehicleRepository.cs`)
- `*Controller.cs` for controllers (e.g. `VehiclesController.cs`, `AuthController.cs`)
- `*Command.cs` / `*CommandHandler.cs` / `*Validator.cs` for CQRS pieces
- `*Query.cs` / `*QueryHandler.cs` for reads
- `*Dto.cs` or `*Dtos.cs` for DTOs (e.g. `VehicleDto.cs`, `FinanceDtos.cs`)
- `*Configuration.cs` for EF type configs (e.g. `VehicleConfiguration.cs`)
- `*Repository.cs` for repo impls (e.g. `VehicleRepository.cs`)

**Files (frontend):**
- `PascalCase.tsx` for components (e.g. `FleetPage.tsx`, `VehicleFormModal.tsx`, `MainLayout.tsx`)
- `camelCase.ts` for hooks (e.g. `useSignalR.ts`)
- `<Feature>Page.tsx` is the route component for a page folder
- `<Widget>.tsx` for co-located components inside `<feature>/components/`
- `kebab-case` or `camelCase` for CSS (`MainLayout.css`, `index.css`)

**Directories:**
- Backend modules use **PascalCase** bounded-context names: `Fleet/`, `Operations/`, `Finance/`, `Inventory/`, `VehicleIssues/`, `Notifications/`, `Dashboard/`, `Auth/`
- Frontend pages use **lowercase** feature names matching routes: `fleet/`, `trips/`, `drivers/`, `driver/`, `maintenance/`, `inventory/`, `finances/`, `dashboard/`, `auth/`
- Backend layers: `Commands/`, `Queries/`, `Common/`, `Interfaces/`, `Persistence/`, `Configurations/`, `Repositories/`, `Interceptors/`
- Frontend folders: `components/`, `pages/`, `services/`, `store/`, `hooks/`, `types/`, `styles/`, `assets/`

## Where to Add New Code

**New backend module (e.g. `Tires/`):**
1. Domain: `backend/src/FleetOS.Domain/Tires/` — entities, value objects, repo interface (`ITireRepository`)
2. Application: `backend/src/FleetOS.Application/Tires/{Commands,Queries}/` — use cases + `ITireRepository` import + DTOs
3. Infrastructure:
   - `backend/src/FleetOS.Infrastructure/Persistence/Configurations/TireConfiguration.cs` (EF config)
   - `backend/src/FleetOS.Infrastructure/Persistence/Repositories/TireRepository.cs`
   - Register in `InfrastructureServiceExtensions.RegisterRepositories()` (`backend/src/FleetOS.Infrastructure/InfrastructureServiceExtensions.cs:116`)
   - Add `DbSet<Tire>` + global query filter in `FleetOsDbContext` (`backend/src/FleetOS.Infrastructure/Persistence/FleetOsDbContext.cs`)
4. API: `backend/src/FleetOS.Api/Features/Tires/TiresController.cs` extending `BaseController`
5. Migrations: `dotnet ef migrations add AddTires -p src/FleetOS.Infrastructure -s src/FleetOS.Api`
6. Realtime (optional): add methods to `IFleetNotificationService` + `FleetHub` + frontend invalidation in `useSignalR.ts`
7. Frontend: `frontend/src/pages/tires/{TiresPage.tsx, components/*}` and add route + sidebar link in `App.tsx` and `MainLayout.tsx`

**New command/query for existing module:**
- Add `CommandX.cs` + `CommandXHandler.cs` (+ optional `CommandXValidator.cs`) in `backend/src/FleetOS.Application/<Module>/Commands/`
- Add endpoint to the matching `backend/src/FleetOS.Api/Features/<Module>/<Module>Controller.cs`
- Return `Result<T>`; use `Result.ToActionResult(this)` for uniform HTTP mapping

**New frontend page:**
- Create folder `frontend/src/pages/<feature>/` with `<Feature>Page.tsx`
- Add route in `frontend/src/App.tsx` inside `<AdminRoute>` (or `<DriverRoute>`)
- Add sidebar item in `frontend/src/components/layout/MainLayout.tsx` → `navItems` array
- Use TanStack Query (`useQuery`/`useMutation`) + `api` from `frontend/src/services/api.ts`
- Define TS types in `frontend/src/types/index.ts` mirroring backend DTOs

**New shared type (cross-layer):**
- Put in `backend/src/FleetOS.Shared/` if needed by multiple layers
- Put in `frontend/src/types/index.ts` for the frontend

**Utilities / helpers:**
- Backend: `FleetOS.Shared` (cross-layer) or inside the relevant Application module
- Frontend: `frontend/src/components/shared/` for UI primitives; co-located inside `pages/<feature>/components/` for feature-scoped widgets

## Special Directories

**`backend/src/FleetOS.Infrastructure/Migrations/`**
- Purpose: EF Core migration files
- Generated: Yes (`dotnet ef migrations add`)
- Committed: Yes — required for `MigrateAndSeedAsync` on boot
- Convention: `<YYYYMMDDHHMMSS>_<Description>.cs` (+ `.Designer.cs`)

**`nginx/ssl/`**
- Purpose: TLS certificate + key for HTTPS termination
- Generated: No (provided by operator)
- Committed: Likely — contains cert files, NOT private keys (or they should be gitignored)

**`frontend/public/`**
- Purpose: Static assets served as-is by Vite / nginx
- Generated: No
- Committed: Yes (`LOGO.png`, favicons, etc.)

**`.planning/`**
- Purpose: GSD workflow state (plans, phases, roadmaps)
- Generated: Yes (by `/gsd-*` commands)
- Committed: Partially — state files yes, scratch no

**`FOUNDATION/`, `DOMAIN/`, `DATABASE/`, `FLEET/`**
- Purpose: Human-written project/domain documentation in Portuguese
- Generated: No
- Committed: Yes — these are the source of truth for the domain

---

*Structure analysis: 2026-08-01*
