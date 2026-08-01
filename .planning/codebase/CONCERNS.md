# Codebase Concerns

**Analysis Date:** 2026-08-01

## Tech Debt

**Zero Test Coverage Across Entire Stack:**
- Issue: Backend has `backend/tests/FleetOS.Tests/FleetOS.Tests.csproj` with xUnit, Moq, FluentAssertions, Bogus, and EF Core InMemory configured — but contains **zero test files**. Frontend has no test runner, no test config, and no test files at all. The entire 28,000+ line backend and 7,000+ line frontend are untested.
- Files: `backend/tests/FleetOS.Tests/FleetOS.Tests.csproj` (skeleton only), `frontend/package.json` (no test deps)
- Impact: Any refactor or bug fix risks regression with no safety net. The finance domain (transactions, month closing, profit calculation) is especially fragile — miscalculations could cause real monetary harm.
- Fix approach: Prioritize domain unit tests (Result pattern, entity state transitions like `FinancialMonth.Close()`, `Trip.Cancel()`, `VehicleIssueReport.Resolve()`), then handler tests for command handlers with InMemory DB, then frontend component tests for form validation and mutation flows.

**Massive `any` Usage in Frontend:**
- Issue: 29 instances of `any` type annotations across frontend components, bypassing TypeScript's safety net. Types are defined in `frontend/src/types/index.ts` but not used in the components that consume API data.
- Files: `frontend/src/pages/fleet/components/FuelLogFormModal.tsx`, `frontend/src/pages/finances/components/TransactionFormModal.tsx`, `frontend/src/pages/finances/components/FinanceSettings.tsx`, `frontend/src/pages/inventory/components/MovementFormModal.tsx`, `frontend/src/pages/inventory/components/StockBalanceList.tsx`, `frontend/src/pages/fleet/components/VehicleFormModal.tsx`
- Impact: Type mismatches between API response shape and component expectations will surface only at runtime. Renaming a DTO field on the backend breaks frontend silently.
- Fix approach: Replace `any` with proper typed interfaces from `frontend/src/types/index.ts`. Use `useQuery<Vehicle[]>` instead of `useQuery<any[]>`. Create API client functions with typed returns.

**Duplicated Command Handler Boilerplate:**
- Issue: Every finance command handler repeats the same pattern: resolve repository → create entity → add → commit → notify. The `FinanceCommandHandlers.cs` file (335 lines) has 8 handlers with near-identical structure. Each handler is ~35 lines of boilerplate around 3 lines of domain logic.
- Files: `backend/src/FleetOS.Application/Finance/Commands/FinanceCommandHandlers.cs`, `backend/src/FleetOS.Application/Finance/Queries/FinanceQueryHandlers.cs`
- Impact: Adding a new entity type requires copy-pasting 35+ lines. Missing the notification call in any handler causes stale UI. Bug fixes must be applied in 8+ places.
- Fix approach: Extract a generic `CrudCommandHandler<TAggregate, TCreateCommand>` base that encapsulates the resolve-create-commit-notify pipeline, or use MediatR pipeline behaviors for the notification step.

**Inline `window.confirm()` for Destructive Actions:**
- Issue: Destructive operations (delete vehicle, delete driver, cancel trip, delete maintenance, delete transaction, delete product) use native `window.confirm()` / `confirm()` instead of styled modals. This is inconsistent with the app's glass-panel design system and provides poor UX on mobile.
- Files: `frontend/src/pages/fleet/components/VehicleList.tsx:94`, `frontend/src/pages/drivers/components/DriverList.tsx:47`, `frontend/src/pages/trips/components/TripList.tsx:59,94`, `frontend/src/pages/maintenance/components/MaintenanceList.tsx:55`, `frontend/src/pages/finances/components/TransactionsList.tsx:124,134,146`, `frontend/src/pages/inventory/components/ProductsList.tsx:88`
- Impact: Inconsistent UX — the app has a well-designed `BaseModal` component but doesn't use it for confirmations. No undo support. No visual distinction between severity levels (delete vs cancel).
- Fix approach: Create a `ConfirmDialog` component using `BaseModal` with `variant="destructive"`. Replace all `window.confirm()` calls. Add undo toast for soft-delete operations.

**Console.log Statements in Production Frontend:**
- Issue: `useSignalR.ts` contains 4 `console.log` / `console.error` calls that will appear in production browser consoles, leaking internal connection URLs and debug state.
- Files: `frontend/src/hooks/useSignalR.ts:180,184,207,211`
- Impact: Information disclosure (hub URL), noisy developer console for end users.
- Fix approach: Replace with a structured logger (or conditional `import.meta.env.DEV` guards). SignalR's built-in logging can replace manual `console.log`.

**SignalR Singleton Connection Pattern:**
- Issue: `useSignalR.ts` manages a module-level singleton `connection` variable with manual state tracking (`connectionStarted`, `startPromise`, `connectionToken`). This pattern is fragile — if multiple components call `useSignalR()` simultaneously, race conditions can occur during connection setup. The singleton is never properly cleaned up on HMR (Hot Module Replacement).
- Files: `frontend/src/hooks/useSignalR.ts:20-23,148-216`
- Impact: Stale connections after token refresh, duplicate connections after HMR, potential memory leaks in long-running sessions.
- Fix approach: Use React context + ref pattern, or `useRef` to hold connection state. Cancel in-flight `start()` promises on unmount. Use AbortController for cleanup.

## Known Bugs

**SupabaseStorageService Blocks Constructor with Sync-over-Async:**
- Symptoms: API startup hangs or throws if Supabase is unreachable. `GetAwaiter().GetResult()` in the constructor blocks the DI thread.
- Files: `backend/src/FleetOS.Infrastructure/Services/SupabaseStorageService.cs:21`
- Trigger: Any deployment where `Supabase:Url` or `Supabase:ServiceKey` are misconfigured, or Supabase is temporarily down. The entire API fails to start.
- Workaround: The service is registered as `AddScoped` so it only initializes per-request, but the constructor still blocks. If Supabase env vars are empty, the constructor throws `InvalidOperationException` at first use.

**Hardcoded Owner Tax Rate in Dashboard:**
- Symptoms: "Lucro Real" (Real Profit) calculation on dashboard always applies a 27% tax rate regardless of actual tax situation or tenant configuration.
- Files: `backend/src/FleetOS.Infrastructure/Persistence/Repositories/DashboardRepository.cs:67`
- Trigger: All dashboard views. The `ownerTaxRate = 0.27m` is a magic number with no documentation of what tax it represents or why 27%.
- Workaround: None — the value is hardcoded. Must be made configurable per tenant or at minimum extracted to a configuration value.

**Global Exception Handler Misclassifies `InvalidOperationException`:**
- Symptoms: Any `InvalidOperationException` (including domain validation failures, missing configuration, etc.) returns HTTP 409 Conflict. This is incorrect — `InvalidOperationException` should often be 400 Bad Request or 422 Unprocessable Entity.
- Files: `backend/src/FleetOS.Api/Middleware/GlobalExceptionHandlerMiddleware.cs:35`
- Trigger: Any code path throwing `InvalidOperationException` — for example, `SupabaseStorageService` constructor, `JwtService` missing config, or domain entities with invalid state transitions.
- Workaround: Use custom exception types (e.g., `DomainValidationException`) or rely on the Result pattern's error codes for proper status code mapping. The `ErrorStatusCodeMapper` at `backend/src/FleetOS.Api/Errors/ErrorStatusCodeMapper.cs` exists but is not used for unhandled exceptions.

**Dashboard Issues Endpoint Polls at 3-Second Interval:**
- Symptoms: Excessive API calls and battery drain on mobile devices. Dashboard page fires `GET /VehicleIssues` every 3 seconds even when the user is idle.
- Files: `frontend/src/pages/dashboard/DashboardPage.tsx:101`
- Trigger: Any user viewing the dashboard page. The `refetchInterval: 3000` with `refetchIntervalInBackground: true` causes continuous polling.
- Workaround: Rely on SignalR `VehicleIssueCreated` events for real-time updates instead of polling. Increase interval to 30s+ or disable background refetch.

## Security Considerations

**Password Transmits in Plaintext Over Non-HTTPS Connections:**
- Risk: Driver and admin passwords travel as plain JSON body from browser to API. In development (HTTP) or misconfigured production, passwords are sniffable.
- Files: `backend/src/FleetOS.Application/Auth/Commands/Login/LoginCommand.cs:8` (Password field), `backend/src/FleetOS.Application/Operations/Drivers/Commands/CreateDriverCommand.cs:9` (Password field), `frontend/src/pages/auth/LoginPage.tsx`, `frontend/src/pages/drivers/components/DriverFormModal.tsx:11`
- Current mitigation: HTTPS redirect enabled in development (`Program.cs:68-71`). Production relies on Vercel/Render TLS termination.
- Recommendations: Enforce HTTPS in all environments. Consider client-side hashing (SHA-256) as defense-in-depth before BCrypt on server.

**Route Guards Are Client-Side Only:**
- Risk: Frontend route guards in `App.tsx` check only JWT expiry and role from Zustand store. They do not validate against the backend. A user with an expired-but-not-yet-removed token can see admin UI briefly.
- Files: `frontend/src/App.tsx:18-41` (`isTokenExpired`, `AdminRoute`, `DriverRoute`)
- Current mitigation: JWT expiry check (`isTokenExpired`) is implemented. Backend enforces authorization on every API call via `[Authorize]` attributes.
- Recommendations: This is acceptable for UX (fast redirects) since backend is the true authority. However, the `isTokenExpired` function should handle malformed tokens gracefully (currently catches all exceptions and returns `true`).

**SignalR Hub Accepts JWT via Query Parameter:**
- Risk: The JWT bearer token is passed as `access_token` query parameter for WebSocket connections. Query parameters are logged by web servers, proxies, and browser history.
- Files: `backend/src/FleetOS.Api/Extensions/ApiServiceExtensions.cs:113-123`
- Current mitigation: This is a documented SignalR limitation — browser WebSocket API doesn't support custom headers. Token expiry is short (60 min).
- Recommendations: Ensure access tokens used for SignalR have shorter expiry. Never log query parameters in production. Use the existing rate limiter to limit connection attempts.

**Hardcoded Default Seed Passwords:**
- Risk: `DbInitializer.cs` contains fallback passwords (`Admin@123456`, `Tenant@123456`) if environment variables are missing. A deployment without proper `.env` configuration will have predictable admin credentials.
- Files: `backend/src/FleetOS.Infrastructure/Persistence/DbInitializer.cs:40-43`
- Current mitigation: Passwords are read from `IConfiguration` first (environment variables). Fallbacks only apply in development seeding.
- Recommendations: Remove hardcoded fallback passwords entirely. Fail fast if seed credentials are missing in non-development environments. Add a startup check that warns if default passwords are detected.

**CORS Allows `AllowAnyHeader` + `AllowCredentials`:**
- Risk: `AllowAnyHeader()` combined with `AllowCredentials()` is overly permissive. Malicious origins could send unexpected headers.
- Files: `backend/src/FleetOS.Api/Extensions/ApiServiceExtensions.cs:63-67`
- Current mitigation: Origins are restricted via `Cors:AllowedOrigins` configuration.
- Recommendations: Restrict allowed headers to `Authorization`, `Content-Type`, `X-Correlation-Id`. Remove `AllowAnyHeader()`.

## Performance Bottlenecks

**Dashboard Executes 12+ Sequential Database Queries:**
- Problem: `DashboardRepository.GetSummaryAsync()` fires 12 separate `CountAsync` / `SumAsync` queries sequentially — one per KPI. Each round-trip to PostgreSQL adds ~1-5ms latency.
- Files: `backend/src/FleetOS.Infrastructure/Persistence/Repositories/DashboardRepository.cs:26-85`
- Cause: No query batching or parallel execution. All queries hit the same database context sequentially.
- Improvement path: Combine related counts into single queries using `GroupBy` or conditional aggregation. Alternatively, use `Task.WhenAll` with separate scoped contexts for parallel execution. Consider a cached summary that updates via SignalR notifications.

**`ToLower()` in LINQ Search Queries Defeats Database Indexes:**
- Problem: Search queries in `VehicleRepository`, `DriverRepository`, and `InventoryRepositories` call `.ToLower().Contains()` which translates to `LOWER(column) LIKE '%term%'` in SQL — a full table scan that bypasses all B-tree indexes.
- Files: `backend/src/FleetOS.Infrastructure/Persistence/Repositories/VehicleRepository.cs:78-82`, `backend/src/FleetOS.Infrastructure/Persistence/Repositories/DriverRepository.cs:64-68`, `backend/src/FleetOS.Infrastructure/Persistence/Repositories/InventoryRepositories.cs:88,151,175`
- Cause: Case-insensitive search without using PostgreSQL `citext` column type or functional indexes.
- Improvement path: Use `EF.Functions.ILike()` for PostgreSQL-native case-insensitive search, or migrate searchable columns to `citext` type. For large datasets, add trigram indexes (`pg_trgm`).

**AlertJob Loads All Tenants Then Queries Each Sequentially:**
- Problem: `AlertJob` fetches all tenants, then for each tenant runs a stock check query — N+1 pattern at the tenant level. With many tenants, this job takes increasingly long.
- Files: `backend/src/FleetOS.Infrastructure/BackgroundJobs/AlertJob.cs:47-96`
- Cause: Iterates tenants in a `foreach` loop with individual DbContext queries per tenant.
- Improvement path: Use a single SQL query with `GROUP BY tenant_id` to find all low-stock products across tenants at once. Or process tenants in parallel with `Task.WhenAll`.

**Frontend Dashboard Polls 3 Endpoints with 30s + 3s Intervals:**
- Problem: `DashboardPage.tsx` sets `refetchInterval: 30000` for summary and checklist report, plus `refetchInterval: 3000` for vehicle issues. Each refetch triggers 3 HTTP requests.
- Files: `frontend/src/pages/dashboard/DashboardPage.tsx:81,92,101`
- Cause: Polling used instead of relying exclusively on SignalR real-time events.
- Improvement path: Remove polling for data that has SignalR event handlers (trips, vehicles, transactions all invalidate via SignalR). Keep only the 30s summary poll as a fallback. Reduce issues polling to 30s+.

**File Upload Loads Entire File Into Memory:**
- Problem: `SupabaseStorageService.UploadFileAsync()` copies the entire upload stream into a `MemoryStream` before uploading to Supabase. Large receipt PDFs (up to 10MB per validation) double memory usage.
- Files: `backend/src/FleetOS.Infrastructure/Services/SupabaseStorageService.cs:27-29`
- Cause: Supabase Storage SDK requires `byte[]` for upload, forcing full materialization.
- Improvement path: Stream directly if the SDK supports it. For large files, consider chunked upload or multipart. At minimum, use `ArrayPool<byte>` to reduce GC pressure.

## Fragile Areas

**Finance Domain — Month Closing and Transaction State Machine:**
- Files: `backend/src/FleetOS.Domain/Finance/FinancialMonth.cs`, `backend/src/FleetOS.Domain/Finance/FinancialTransaction.cs`, `backend/src/FleetOS.Application/Finance/Commands/FinanceCommandHandlers.cs`
- Why fragile: Financial month has states (Open → Active → Closed) with irreversible transitions. Transactions can be Created → Paid → Cancelled. The `CloseFinancialMonthCommandHandler` allows closing a month without checking if all transactions are settled. A bug here could corrupt financial records.
- Safe modification: Always write tests first. Add state transition validation tests. Never modify without understanding the full state machine. Add audit logging for all financial mutations.
- Test coverage: Zero — no test files exist for any finance handler.

**Multi-Tenant Global Query Filters:**
- Files: `backend/src/FleetOS.Infrastructure/Persistence/FleetOsDbContext.cs:88-152`
- Why fragile: All 19 entity types have global query filters combining `DeletedAt == null && TenantId == _currentTenantId`. The `_currentTenantId` field is a mutable instance variable set via `SetTenantId()`. If any code path forgets to set the tenant ID before querying, the filter returns zero results silently (no error, just empty data). The `DbInitializer` works around this with `IgnoreQueryFilters()` but other code paths may have the same issue.
- Safe modification: Always verify tenant context is set before any DB operation. Add integration tests that verify tenant isolation. Never use `IgnoreQueryFilters()` without explicit documentation of why.
- Test coverage: Zero — no tests verify tenant isolation.

**SignalR Event Invalidation Cascade:**
- Files: `frontend/src/hooks/useSignalR.ts:25-146`
- Why fragile: A single `TripCreated` event invalidates 4 query keys. `VehicleCreated` invalidates 5. `FuelLogCreated` invalidates 6. If SignalR delivers events rapidly (bulk import), this causes a cascade of parallel API requests that can overwhelm the backend rate limiter (100 req/min per IP).
- Safe modification: Add debouncing to `invalidateQueries` calls. Batch invalidation with `queryClient.invalidateQueries` using broader query key patterns. Add a `useSignalR` throttle layer.
- Test coverage: Zero — no frontend tests exist.

**Entity Framework Migrations — 2400+ Line Snapshot:**
- Files: `backend/src/FleetOS.Infrastructure/Migrations/FleetOsDbContextModelSnapshot.cs` (2441 lines)
- Why fragile: The model snapshot is enormous due to 19+ entity types with complex relationships. Migration conflicts are common when multiple developers add migrations. The `InitialCore` migration alone is 950 lines. Re-running migrations on an existing database requires careful handling.
- Safe modification: Always coordinate migrations with the team. Never edit existing migrations. Use `dotnet ef migrations add` (never hand-edit). Test migrations on a copy of production data before deploying.
- Test coverage: Zero — no migration tests exist.

**CNP/CPF Hash Collision Risk:**
- Files: `backend/src/FleetOS.Application/Auth/Commands/Login/LoginCommandHandler.cs:105-110`
- Why fragile: CPF (Brazilian tax ID) is hashed with SHA-256 for storage, but CPFs have only 11 digits (10^11 ≈ 100 billion combinations). While SHA-256 prevents rainbow tables, the low entropy means brute-force is theoretically possible. CPFs are also not globally unique per tenant — the hash is scoped by tenant in the query.
- Safe modification: Ensure queries always include tenantId. Add a uniqueness constraint on `(TenantId, CpfHash)` in the database.
- Test coverage: Zero.

## Scaling Limits

**Background Job — Single-Process Alert Checking:**
- Current capacity: `AlertJob` runs every 6 hours on a single background thread, processing tenants sequentially.
- Limit: With 100+ tenants, the job could take minutes to complete, blocking the single background thread.
- Scaling path: Move to a distributed job scheduler (Hangfire, Quartz.NET) with tenant-level parallelism. Or extract to a separate worker service.

**No Pagination on Fuel Logs Default Query:**
- Current capacity: `FuelLogList.tsx` requests `pageSize: 100` by default.
- Limit: With many vehicles logging fuel daily, 100 records covers only a few days. Users with large fleets will hit the limit frequently.
- Scaling path: Implement proper pagination UI with page navigation. Consider infinite scroll for better UX.

## Dependencies at Risk

**`@microsoft/signalr` v10.0.0 — Bleeding Edge:**
- Risk: SignalR v10 is very new and may have breaking changes from v8/v9. The frontend uses it with WebSocket transport which requires specific server configuration.
- Impact: WebSocket transport failures fall back to SSE then Long Polling, adding latency.
- Migration plan: Pin to a stable version range. Add connection health monitoring.

**`html2canvas` + `jspdf` — Client-Side PDF Generation:**
- Risk: These libraries are known to have rendering inconsistencies across browsers and don't support CSS features like `backdrop-filter`, `grid`, or `gap` consistently.
- Impact: Any "export to PDF" feature using these will produce incorrect output for the app's glass-panel design.
- Migration plan: Consider server-side PDF generation or a more robust library like `@react-pdf/renderer`.

**`echarts` + `recharts` — Dual Charting Libraries:**
- Risk: Both `echarts` (via `echarts-for-react`) and `recharts` are installed as dependencies. This adds ~500KB+ to the bundle.
- Impact: Larger bundle size, longer load times, two different chart APIs to maintain.
- Migration plan: Standardize on one library. `recharts` is lighter and React-native; `echarts` has more features. Dashboard uses `recharts`.

## Missing Critical Features

**No Audit Trail for Financial Operations:**
- Problem: Financial transactions can be created, paid, cancelled, and deleted. There is no audit log recording who performed each action and when. If a transaction is deleted or cancelled, there is no record of the original data.
- Blocks: Financial compliance, dispute resolution, forensic analysis.
- Priority: High — this is a financial system.

**No Rate Limiting per User/Tenant:**
- Problem: Rate limiting is per-IP (`httpContext.Connection.RemoteIpAddress`), not per authenticated user or tenant. Behind a reverse proxy or load balancer, all users share the same IP, meaning one tenant can exhaust the rate limit for all others.
- Blocks: Multi-tenant scaling, fair resource allocation.
- Priority: Medium — affects production deployment on Render/Vercel where IP is shared.

## Test Coverage Gaps

**Entire Backend — Zero Tests:**
- What's not tested: Every command handler, query handler, domain entity, repository, service, and controller. Specifically:
  - Auth flow (login, refresh, logout, account lockout)
  - Finance domain (month state machine, transaction lifecycle, owner salary, profit calculation)
  - Multi-tenant isolation (global query filters)
  - Fleet operations (trip state machine, vehicle status transitions)
  - Inventory (stock balance calculations, movement validation)
- Files: All files under `backend/src/`
- Risk: Critical — financial calculations, auth bypass, and tenant data leakage could go undetected.
- Priority: Critical

**Entire Frontend — Zero Tests:**
- What's not tested: Every component, hook, store, and service. Specifically:
  - Auth flow (login, refresh token interceptor, route guards)
  - Form validation (Zod schemas)
  - API error handling (401 refresh flow)
  - SignalR connection lifecycle
  - Dashboard data display
- Files: All files under `frontend/src/`
- Risk: High — UI regressions, broken auth flows, and data display errors go undetected.
- Priority: High

**No Integration Tests:**
- What's not tested: API endpoint behavior, database query correctness, EF Core migration application, Docker container health.
- Files: `backend/tests/FleetOS.Tests/` (empty), no E2E test setup
- Risk: High — API contract changes between frontend and backend go undetected.
- Priority: High

---

*Concerns audit: 2026-08-01*
