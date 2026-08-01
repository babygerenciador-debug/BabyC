<!-- refreshed: 2026-08-01 -->
# Architecture

**Analysis Date:** 2026-08-01

## System Overview

```text
┌──────────────────────────────────────────────────────────────────────┐
│                       Browser / Client                              │
│  React 19 + Vite SPA (fleetos_frontend)                             │
│  `frontend/src/`                                                     │
└──────────┬─────────────────────────────────────┬─────────────────────┘
           │ HTTPS (REST + SignalR WS)           │ SignalR WebSocket
           ▼                                     ▼
┌──────────────────────────────────────────────────────────────────────┐
│                    Nginx Reverse Proxy (`nginx/nginx.conf`)          │
│        TLS termination (port 443) + static frontend + /api → API    │
└──────────┬───────────────────────────────────────────────────────────┘
           ▼
┌──────────────────────────────────────────────────────────────────────┐
│                 ASP.NET Core 10 API (FleetOS.Api)                    │
│  `backend/src/FleetOS.Api/`                                         │
├────────────┬─────────────────┬──────────────────┬───────────────────┤
│ Middleware │ Controllers     │ Features/*       │ SignalR Hub       │
│ (auth,     │ `AuthController`│ domain-scoped    │ `/hubs/fleet`     │
│ tenant,    │ + feature       │ controllers      │ `Infrastructure/  │
│ rate-limit)│ controllers     │ (CQRS via        │  Hubs/FleetHub.cs`│
└──────┬─────┴────────┬────────┴────────┬─────────┴───────────────────┘
       │              │                 │
       │              ▼                 ▼
┌──────┴──────────────────────────────────────────────────────────────┐
│          Application Layer (`FleetOS.Application`)                  │
│   MediatR CQRS — Commands + Queries, FluentValidation, DTOs        │
│   `backend/src/FleetOS.Application/<Module>/Commands|Queries/`     │
└──────┬──────────────────────────────────────────────────────────────┘
       │ uses interfaces (DIP)
       ▼
┌──────────────────────────────────────────────────────────────────────┐
│             Domain Layer (`FleetOS.Domain`)                         │
│   Entities, Aggregates, Value Objects, Domain Events, repo interfaces│
│   `backend/src/FleetOS.Domain/<Module>/`                            │
└──────┬──────────────────────────────────────────────────────────────┘
       │ implemented by
       ▼
┌──────────────────────────────────────────────────────────────────────┐
│        Infrastructure Layer (`FleetOS.Infrastructure`)              │
│   EF Core + PostgreSQL, Redis, Supabase Storage, SignalR, Jobs     │
│   `backend/src/FleetOS.Infrastructure/Persistence|Services|Hubs`  │
└──────┬──────────────────────────────────────────────────────────────┘
       │
       ▼
┌──────────────────┬───────────────────┬──────────────────────────────┐
│ PostgreSQL       │ Redis             │ Supabase Storage             │
│ (Supabase)       │ (cache + pub/sub) │ (files / docs / photos)      │
└──────────────────┴───────────────────┴──────────────────────────────┘
```

## Component Responsibilities

| Component | Responsibility | File |
|-----------|----------------|------|
| Api project | HTTP entry point, middleware pipeline, controller routing, Swagger | `backend/src/FleetOS.Api/Program.cs` |
| BaseController | Mediator + TenantContext lazy resolution for all controllers | `backend/src/FleetOS.Api/Controllers/BaseController.cs` |
| Feature controllers | Thin CQRS facades per bounded context (Vehicles, Trips, Finance, …) | `backend/src/FleetOS.Api/Features/<Module>/*Controller.cs` |
| Application (Commands) | Use cases – mutates domain, calls UoW, emits notifications | `backend/src/FleetOS.Application/<Module>/Commands/` |
| Application (Queries) | Read-only use cases, returns DTOs | `backend/src/FleetOS.Application/<Module>/Queries/` |
| Domain | Entities, aggregates, value objects, domain events, repo interfaces | `backend/src/FleetOS.Domain/<Module>/` |
| Infrastructure | EF Core DbContext, repositories, external services (Supabase, JWT, SignalR, background jobs) | `backend/src/FleetOS.Infrastructure/` |
| Shared | `Result<T>`, `Error`, pagination primitives shared across layers | `backend/src/FleetOS.Shared/` |
| Frontend app | Routing + layout + providers | `frontend/src/App.tsx` |
| Frontend pages | Feature modules with co-located components | `frontend/src/pages/<feature>/` |
| Frontend services | Axios client + TanStack Query client | `frontend/src/services/api.ts`, `frontend/src/services/queryClient.ts` |
| Frontend stores | Zustand auth + theme | `frontend/src/store/useAuthStore.ts`, `frontend/src/store/useThemeStore.ts` |
| Realtime client | SignalR connection + cache invalidation | `frontend/src/hooks/useSignalR.ts` |

## Pattern Overview

**Overall:** Clean Architecture + DDD + CQRS over ASP.NET Core 10 (backend) and React 19 + Vite (frontend), orchestrated by MediatR on the server and TanStack Query + SignalR on the client.

**Key Characteristics:**
- **Vertical slice per module**: every bounded context (Fleet, Operations, Finance, Inventory, VehicleIssues, Notifications, Dashboard) has folders across all four backend projects (`Api/Features`, `Application`, `Domain`, `Infrastructure/Persistence`).
- **Dependency Inversion**: Domain defines repository interfaces (`IRepository<T>`, `IVehicleRepository`, …); Application consumes them; Infrastructure implements them.
- **Result monad** instead of exceptions for business flow: every Command/Query returns `Result` or `Result<T>` (`backend/src/FleetOS.Shared/Results/Result.cs`).
- **Automatic cross-cutting concerns**: audit fields via `AuditInterceptor`, tenant isolation via EF global query filters, validation via `ValidationBehavior` MediatR pipeline.
- **Realtime-first client**: SignalR hub broadcasts per-entity events; the frontend invalidates the matching TanStack Query keys.

## Layers

**Presentation (Frontend):**
- Purpose: UI, navigation, API consumption
- Location: `frontend/src/`
- Contains: React 19 components, pages per feature, Zustand stores, TanStack Query hooks
- Depends on: REST API at `/api/v1`, SignalR hub `/hubs/fleet`
- Used by: End users (admin/manager/driver roles)

**API Layer (`FleetOS.Api`):**
- Purpose: HTTP, auth, middleware, routing, Swagger, rate-limiting
- Location: `backend/src/FleetOS.Api/`
- Contains: `Program.cs`, `Controllers/`, `Features/`, `Middleware/`, `Extensions/`, `Services/`, `Errors/`
- Depends on: Application, Domain, Infrastructure (composition root)
- Used by: Frontend SPA, mobile clients (future)

**Application Layer (`FleetOS.Application`):**
- Purpose: Use cases (Commands + Queries), DTOs, validation, orchestration
- Location: `backend/src/FleetOS.Application/<Module>/{Commands,Queries}/`
- Contains: MediatR `IRequest<T>` records, handlers, FluentValidation validators, repository interfaces under `Common/Interfaces/`
- Depends on: Domain, Shared (never on Infrastructure)
- Used by: API layer via `ISender.Send(...)`

**Domain Layer (`FleetOS.Domain`):**
- Purpose: Entities, aggregates, value objects, domain events, repo interfaces
- Location: `backend/src/FleetOS.Domain/<Module>/`
- Contains: `Entity`, `AggregateRoot`, `ValueObject`, business rules, `IDomainEvent`, repository interfaces
- Depends on: Shared (for `Result`), MediatR.Contracts (for `INotification`)
- Used by: Application, Infrastructure (implements its interfaces)

**Infrastructure Layer (`FleetOS.Infrastructure`):**
- Purpose: Persistence, external services, SignalR hub, background jobs
- Location: `backend/src/FleetOS.Infrastructure/`
- Contains: `FleetOsDbContext`, EF configurations, repositories, `AuditInterceptor`, Supabase/BCrypt/JWT services, `FleetHub`, `AlertJob`, `RefuelReminderJob`
- Depends on: Application, Domain (implements their interfaces)
- Used by: API (DI registered in `Program.cs`)

## Data Flow

### Primary Request Path (CQRS write)

1. HTTP request hits `Program.cs` middleware pipeline: CorrelationId → TenantResolver → Auth → RateLimiter → `GlobalExceptionHandlerMiddleware` (`backend/src/FleetOS.Api/Program.cs:87-91`)
2. Routed to a feature controller (e.g. `VehiclesController.CreateVehicle` at `backend/src/FleetOS.Api/Features/Vehicles/VehiclesController.cs:14`) which dispatches a `CreateVehicleCommand` via MediatR
3. `ValidationBehavior<,>` pipeline (`backend/src/FleetOS.Application/Common/Behaviors/ValidationBehavior.cs`) runs FluentValidation validators; on failure returns `Result.Failure`
4. Handler (e.g. `CreateVehicleCommandHandler` at `backend/src/FleetOS.Application/Fleet/Vehicles/Commands/CreateVehicleCommandHandler.cs:42`) loads tenant from `ITenantContext`, enforces invariants, creates the `Vehicle` aggregate via its factory, persists via repository, commits via `IUnitOfWork`
5. `FleetOsDbContext.SaveChangesAsync` (`backend/src/FleetOS.Infrastructure/Persistence/FleetOsDbContext.cs:181`) dispatches domain events through `IPublisher` and the `AuditInterceptor` populates `CreatedBy/UpdatedBy` (`backend/src/FleetOS.Infrastructure/Persistence/Interceptors/AuditInterceptor.cs`)
6. Handler notifies SignalR hub via `IFleetNotificationService` (e.g. `NotifyVehicleCreatedAsync`) → `FleetHub` broadcasts to connected clients
7. Controller maps `Result<T>` to an HTTP response via `ResultExtensions.ToActionResult` (`backend/src/FleetOS.Api/Extensions/ResultExtensions.cs`)

### Realtime Fan-out Path

1. Backend publishes event (e.g. `VehicleCreated`) on `FleetHub` (`backend/src/FleetOS.Infrastructure/Hubs/FleetHub.cs`)
2. Frontend `useSignalR` hook (`frontend/src/hooks/useSignalR.ts:59-66`) receives the event and invalidates matching TanStack Query keys (`vehicles`, `vehicles-dropdown`, `dashboardSummary`, …)
3. React components using `useQuery` automatically refetch

### Authentication Flow

1. `POST /api/v1/auth/login` → `AuthController.Login` → `LoginCommandHandler` validates credentials (BCrypt), issues JWT + RefreshToken
2. Frontend `useAuthStore` (`frontend/src/store/useAuthStore.ts`) persists tokens in `sessionStorage` (Zustand `persist` middleware)
3. Axios interceptor (`frontend/src/services/api.ts:44`) attaches `Authorization: Bearer <token>`; on 401 it transparently refreshes via `/auth/refresh`
4. Backend JWT middleware (`backend/src/FleetOS.Api/Extensions/ApiServiceExtensions.cs:95-124`) validates token and also supports SignalR `access_token` query-param auth for WebSocket transport

**State Management:**
- Server: EF Core change tracker + global query filters scoped per-request by `TenantResolver` middleware from JWT claims (`tenant_id`, `organization_id`, `business_unit_id`)
- Client: Zustand stores (`useAuthStore`, `useThemeStore`) persisted to `sessionStorage`; TanStack Query server-state cache with 5-min `staleTime`; SignalR invalidates on mutations

## Key Abstractions

**`Result<T>` / `Error`** (shared result monad):
- Purpose: Replace exceptions for business-flow control, carry structured error codes
- Examples: `backend/src/FleetOS.Shared/Results/Result.cs`, `backend/src/FleetOS.Shared/Results/Error.cs`
- Pattern: `Result.Success(v)` / `Result.Failure<T>(Error.Validation(...))`; mapped to HTTP in `ResultExtensions.ToActionResult`

**`Entity` / `AggregateRoot` / `ValueObject`** (DDD building blocks):
- Purpose: Typed identity, multi-tenant context, audit fields, soft-delete, domain events
- Examples: `backend/src/FleetOS.Domain/Common/Entity.cs`, `AggregateRoot.cs`, `ValueObject.cs`
- Pattern: Entities expose private setters and mutate via intent-revealing methods; factories return `Result<T>` (e.g. `Vehicle.Create` at `backend/src/FleetOS.Domain/Fleet/Vehicles/Vehicle.cs:80`)

**`IRepository<T>` / `IUnitOfWork`** (persistence DIP):
- Purpose: Domain-defined persistence contracts implemented by EF Core
- Examples: `backend/src/FleetOS.Domain/Common/Interfaces/IRepository.cs`, `backend/src/FleetOS.Infrastructure/Persistence/Repositories/BaseRepository.cs`
- Pattern: Generic `IRepository<T>` + specialized interfaces (`IVehicleRepository`, `IDriverRepository`); DbContext implements `IUnitOfWork`

**MediatR Commands / Queries**:
- Purpose: One request = one handler, enabling CQRS and pipeline behaviors (validation)
- Examples: `CreateVehicleCommand` + handler, `GetVehiclesQuery` + handler
- Pattern: `record CreateVehicleCommand(...) : IRequest<Result<Guid>>`; handler is `internal sealed class`

**`ITenantContext` / `ICurrentUserService`**:
- Purpose: Per-request tenant + user resolution, DIP-compliant (defined in Domain, populated by API middleware)
- Examples: `backend/src/FleetOS.Domain/Common/Interfaces/IRepository.cs:24` (ITenantContext), `backend/src/FleetOS.Api/Extensions/MiddlewareExtensions.cs:56` (TenantContext impl)

**`IFleetNotificationService` (SignalR)**:
- Purpose: Abstracts realtime broadcasting so Application layer does not know about SignalR
- Example: `backend/src/FleetOS.Infrastructure/Services/FleetNotificationService.cs`

## Entry Points

**HTTP API:**
- Location: `backend/src/FleetOS.Api/Program.cs`
- Triggers: Browser / mobile HTTP requests
- Responsibilities: Middleware pipeline (Serilog → CORS → WebSockets → RateLimiter → Auth → CorrelationId → TenantResolver → GlobalExceptionHandler → MapControllers); runs `MigrateAndSeedAsync` on startup
- Route convention: `api/v1/[controller]` (BaseController) and `api/v1/<feature>` (feature controllers)

**SignalR Hub:**
- Location: `backend/src/FleetOS.Infrastructure/Hubs/FleetHub.cs`, mapped at `/hubs/fleet` in `Program.cs:79`
- Triggers: WebSocket upgrade from authenticated clients
- Responsibilities: Push per-entity events (`VehicleCreated`, `TripUpdated`, `StockUpdated`, …) to all connected tenant clients

**Background Jobs:**
- Location: `backend/src/FleetOS.Infrastructure/BackgroundJobs/AlertJob.cs`, `RefuelReminderJob.cs`
- Triggers: Hosted-service timer
- Responsibilities: Scheduled scans (e.g. fuel-alert thresholds) → emit notifications via `IFleetNotificationService`

**Frontend SPA:**
- Location: `frontend/src/main.tsx` → `frontend/src/App.tsx`
- Triggers: Browser load
- Responsibilities: BrowserRouter + React-Query provider + role-based route guards (`AdminRoute`, `DriverRoute`), `MainLayout` with sidebar nav

## Architectural Constraints

- **Multi-tenant isolation:** every tenant-scoped entity has an EF global query filter `x => x.DeletedAt == null && x.TenantId == _currentTenantId` set per-request by `TenantResolver` middleware from JWT claims (`backend/src/FleetOS.Infrastructure/Persistence/FleetOsDbContext.cs:89-151`). Bypassing this (e.g. `IgnoreQueryFilters`) requires explicit audit approval.
- **Soft delete only:** `AuditInterceptor` converts `EntityState.Deleted` into `SoftDelete(userId)` (sets `DeletedAt`). Hard deletes are forbidden for tenant-scoped entities.
- **Threading:** ASP.NET Core async request pipeline; EF Core queries are fully async (`*Async` methods with `CancellationToken`). SignalR hub calls are async. No background threads are spawned directly — use `IHostedService` (`AlertJob`, `RefuelReminderJob`).
- **Dependency direction:** Api → Application → Domain ← Infrastructure. Domain never references Infrastructure or Application. Application exposes interfaces (`Common/Interfaces/`) implemented by Infrastructure.
- **Global state:** `FleetOsDbContext._currentTenantId` is scoped per DI instance (scoped lifetime) — safe per request. Frontend has module-level mutable state for the SignalR connection in `frontend/src/hooks/useSignalR.ts:20-23` (singleton-per-tab by design).
- **Circular imports:** none enforced — project references are strictly one-directional (see csproj files). `FleetOS.Api` references all layers (composition root); other projects do not reference `Api`.
- **CQRS read/write split:** queries never mutate; commands never return data besides an `Id`/success. Handlers return `Result`/`Result<T>` exclusively.

## Anti-Patterns

### Bypassing `Result<T>` by throwing exceptions for business rules

**What happens:** Handler throws `InvalidOperationException` or similar for domain-rule violations.
**Why it's wrong:** Breaks the uniform `Result`-based flow, forces the `GlobalExceptionHandlerMiddleware` to map to 500/409, and defeats the `ValidationBehavior` pipeline.
**Do this instead:** Return `Result.Failure<T>(Error.BusinessRule(...))` or `Error.Validation(...)` — see `CreateVehicleCommandHandler` (`backend/src/FleetOS.Application/Fleet/Vehicles/Commands/CreateVehicleCommandHandler.cs:53-65`).

### Mutating entities via public setters from handlers

**What happens:** `vehicle.Status = ...` or `vehicle.Nickname = ...` called from a handler.
**Why it's wrong:** Circumvents invariants encapsulated in aggregate methods, bypasses audit (`UpdatedAt`) and domain-event emission.
**Do this instead:** Use intent-revealing methods on the aggregate (e.g. `vehicle.UpdateStatus(...)`, `vehicle.AssignDriver(...)`, `Vehicle.Create(...)` factory). See `backend/src/FleetOS.Domain/Fleet/Vehicles/Vehicle.cs:184-207`.

### Querying outside the tenant filter

**What happens:** Using `.IgnoreQueryFilters()` or raw SQL to read cross-tenant data.
**Why it's wrong:** Breaks multi-tenant isolation; creates security incidents.
**Do this instead:** Rely on the global filter. For the rare cross-tenant admin case, add a scoped `SetTenantId(Guid.Empty)` escape hatch with an audit log, and document it.

### Frontend calling the API without going through the `api` axios instance

**What happens:** `fetch('/api/v1/...')` or `axios.get(...)` in a component.
**Why it's wrong:** Bypasses the JWT attachment + 401-refresh interceptor in `frontend/src/services/api.ts`, breaking token renewal.
**Do this instead:** Always import `api` from `frontend/src/services/api.ts` and use it via TanStack Query's `queryFn`.

## Error Handling

**Strategy:** Uniform `Result<T>` return type at the use-case boundary; HTTP mapping centralized in `ResultExtensions.ToActionResult`; unhandled exceptions caught by `GlobalExceptionHandlerMiddleware` → `application/problem+json` (RFC 7807).

**Patterns:**
- Business failure: `Result.Failure(Error.Validation|NotFound|Conflict|BusinessRule(...))` → mapped via `ErrorStatusCodeMapper` (`backend/src/FleetOS.Api/Errors/ErrorStatusCodeMapper.cs`)
- Validation failure: `ValidationBehavior` short-circuits MediatR pipeline, returns `Result.Failure(Error.Validation(prop, msg))`
- Unhandled exception: `GlobalExceptionHandlerMiddleware` logs with Serilog, returns `ProblemDetails` with `correlationId` extension, hides message in prod (`backend/src/FleetOS.Api/Middleware/GlobalExceptionHandlerMiddleware.cs:50`)
- Frontend: axios response interceptor displays `toast.error` with `error.response.data.title|description` (`frontend/src/services/api.ts:76-85`)

## Cross-Cutting Concerns

**Logging:** Serilog (Console + rolling File in non-prod) with `LogContext`, MachineName and ThreadId enrichers; HTTP request logging via `UseSerilogRequestLogging`; per-handler `ILogger<T>` injected in command handlers.
**Validation:** FluentValidation validators auto-registered from `FleetOS.Application` assembly; run by `ValidationBehavior` MediatR pipeline before any handler.
**Authentication:** JWT Bearer + Refresh-Token rotation; BCrypt passwords; SignalR uses `access_token` query param (since browsers can't set WS headers) — configured in `ApiServiceExtensions.AddAuthServices`.
**Authorization:** Role-based via `[Authorize(Roles = "...")]` on controllers; role enum `UserRoleContext { SystemAdmin, TenantAdmin, Manager, Driver }` defined in `backend/src/FleetOS.Domain/Common/Interfaces/IRepository.cs:44`; frontend route guards in `frontend/src/App.tsx:27-41`.
**Tenant resolution:** `TenantResolver` middleware reads `tenant_id/organization_id/business_unit_id` claims → sets `FleetOsDbContext.SetTenantId(...)` → EF global query filters enforce isolation.
**Audit:** `AuditInterceptor` auto-fills `CreatedAt/CreatedBy/UpdatedAt/UpdatedBy` on save; converts hard deletes into soft deletes.
**Realtime:** SignalR hub `FleetHub` broadcasts CRUD events; frontend `useSignalR` invalidates TanStack Query keys.

---

*Architecture analysis: 2026-08-01*
