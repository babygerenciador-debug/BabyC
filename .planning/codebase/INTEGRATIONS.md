# External Integrations

**Analysis Date:** 2026-08-01

## APIs & External Services

**Real-time Communication:**
- SignalR Hub - WebSocket-based real-time notifications
  - Endpoint: `/hubs/fleet` (`backend/src/FleetOS.Infrastructure/Hubs/FleetHub.cs`)
  - Client: `@microsoft/signalr` v10.0.0 (`frontend/src/services/api.ts`)
  - Auth: JWT Bearer token via `access_token` query param (WebSocket limitation)
  - Groups: Tenant-scoped (`Tenant_{tenantId}`)
  - Transport: WebSockets (proxied through Nginx with upgrade headers)

**File Storage:**
- Supabase Storage - Receipt uploads, logos, photos
  - SDK: `Supabase` v1.1.1 (C# client)
  - Service: `backend/src/FleetOS.Infrastructure/Services/SupabaseStorageService.cs`
  - Auth: Service role key (`SUPABASE_SERVICE_KEY`)
  - Bucket: Configurable (`SUPABASE_STORAGE_BUCKET`, default: `fleetos`)
  - URL: `SUPABASE_URL` (e.g., `https://[project-ref].supabase.co`)

## Data Storage

**Primary Database:**
- PostgreSQL 15 (Supabase-hosted in production, local Docker in dev)
  - Connection: `DATABASE_URL` / `ConnectionStrings__DefaultConnection`
  - ORM: Entity Framework Core 10.0.4 (`backend/src/FleetOS.Infrastructure/Persistence/FleetOsDbContext.cs`)
  - Provider: `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.1
  - Naming: Snake case convention (`EFCore.NamingConventions` 10.0.1)
  - Migrations: EF Core migrations (`backend/src/FleetOS.Infrastructure/Migrations/`)
  - Multi-tenancy: Global query filters on `TenantId` (tenant isolation)
  - Soft delete: Global query filter on `DeletedAt`

**Caching:**
- Redis 7.2 (with in-memory fallback)
  - Connection: `REDIS_CONNECTION_STRING` / `Redis:ConnectionString`
  - Client: `StackExchange.Redis` 2.9.11
  - Cache layer: `Microsoft.Extensions.Caching.StackExchangeRedis` 10.0.10
  - Instance prefix: `fleetos:`
  - Password: `REDIS_PASSWORD` / `Redis:Password`
  - Fallback: `DistributedMemoryCache` when Redis is unavailable
  - Config: Max memory 256MB, LRU eviction policy

**File Storage:**
- Supabase Storage (see APIs & External Services above)

## Authentication & Identity

**Auth Provider:**
- Custom JWT implementation
  - JWT Bearer authentication (`Microsoft.AspNetCore.Authentication.JwtBearer` 10.*)
  - Configuration: `backend/src/FleetOS.Api/Extensions/ApiServiceExtensions.cs`
  - Secret: `JWT_SECRET` (min 64 chars)
  - Issuer: `JWT_ISSUER` (default: `fleetos-api`)
  - Audience: `JWT_AUDIENCE` (default: `fleetos-clients`)
  - Access token expiry: `JWT_ACCESS_EXPIRY_MINUTES` (default: 60)
  - Refresh token expiry: `JWT_REFRESH_EXPIRY_DAYS` (default: 30)

**Password Hashing:**
- BCrypt via `BCrypt.Net-Next` 4.0.3
  - Service: `backend/src/FleetOS.Infrastructure/Services/BcryptPasswordService.cs`

**Token Refresh:**
- Refresh token flow implemented
  - Endpoint: `POST /api/v1/auth/refresh`
  - Token rotation with queue-based concurrent request handling
  - Frontend interceptor: `frontend/src/services/api.ts` (lines 96-118)

**Multi-Tenancy:**
- Tenant resolution middleware: `backend/src/FleetOS.Api/Extensions/MiddlewareExtensions.cs`
  - Resolves tenant from JWT claim `tenant_id`
  - Global query filters ensure data isolation
  - SignalR groups are tenant-scoped

**User Roles:**
- `SystemAdmin`, `TenantAdmin`, `Manager`, `Driver`
- Role-based route guards in frontend: `frontend/src/App.tsx` (`AdminRoute`, `DriverRoute`)

## Monitoring & Observability

**Logging:**
- Serilog structured logging
  - Packages: `Serilog.AspNetCore` 8.*, `Serilog.Sinks.Console` 6.*, `Serilog.Sinks.File` 6.*
  - Enrichers: `Environment`, `ThreadId`, `CorrelationId`
  - Console output: All environments
  - File output: Non-production only (ephemeral disk in Render)
  - File path: `logs/fleetos-{date}.log` (rolling daily, 30-day retention)
  - HTTP request logging: `UseSerilogRequestLogging()` with elapsed time

**Health Checks:**
- ASP.NET Core Health Checks
  - Endpoint: `/health` (mapped before rate limiter)
  - Docker healthcheck: `curl -f http://localhost:8080/health`
  - Used by Docker Compose and Render.com

**Correlation IDs:**
- `CorrelationId` package 3.*
  - Middleware: `UseCorrelationId()` in `Program.cs`
  - Propagated through HTTP requests for distributed tracing

## CI/CD & Deployment

**Hosting:**
- Backend: Render.com (Web Service, Docker image)
  - Config: `backend/render.yaml`
  - Repo: `https://github.com/babygerenciador-debug/BabyC`
  - Branch: `master`
  - Dockerfile: `backend/Dockerfile`
  - Health check path: `/health`
  - Port: 8080
- Frontend: Vercel (static hosting)
  - Config: `frontend/vercel.json`
  - Build: `npm run build` → `dist/`
  - Framework: Vite
  - SPA rewrite: `/(.*)` → `/index.html`

**CI Pipeline:**
- Not detected (no CI/CD config files like GitHub Actions, GitLab CI, etc.)

**Local Development:**
- Docker Compose orchestration
  - Full stack: `docker-compose.yml` (API, frontend, Redis, Nginx)
  - Backend only: `backend/docker-compose.yml` (API, PostgreSQL, Redis)

## Environment Configuration

**Required env vars (Backend):**
- `DATABASE_URL` / `ConnectionStrings__DefaultConnection` - PostgreSQL connection string
- `JWT_SECRET` - JWT signing secret (min 64 characters)
- `JWT_ISSUER` - JWT issuer (default: `fleetos-api`)
- `JWT_AUDIENCE` - JWT audience (default: `fleetos-clients`)
- `JWT_ACCESS_EXPIRY_MINUTES` - Access token TTL (default: 60)
- `JWT_REFRESH_EXPIRY_DAYS` - Refresh token TTL (default: 30)
- `REDIS_CONNECTION_STRING` / `Redis:ConnectionString` - Redis connection
- `REDIS_PASSWORD` / `Redis:Password` - Redis authentication
- `SUPABASE_URL` - Supabase project URL
- `SUPABASE_SERVICE_KEY` - Supabase service role key
- `SUPABASE_STORAGE_BUCKET` - Storage bucket name (default: `fleetos`)
- `CORS_ALLOWED_ORIGINS` / `Cors:AllowedOrigins` - Comma-separated allowed origins
- `ASPNETCORE_ENVIRONMENT` - Environment name (Development/Production)

**Required env vars (Frontend):**
- `VITE_API_URL` - Backend API base URL (e.g., `http://localhost:5000`)
- `VITE_APP_NAME` - Application name (default: `FleetOS`)
- `VITE_APP_VERSION` - Application version (default: `1.0.0`)

**Seed data env vars (first run):**
- `SEED_SYSTEM_ADMIN_EMAIL` - System admin email
- `SEED_SYSTEM_ADMIN_PASSWORD` - System admin password
- `SEED_TENANT_NAME` - Initial tenant name
- `SEED_TENANT_ADMIN_EMAIL` - Tenant admin email
- `SEED_TENANT_ADMIN_PASSWORD` - Tenant admin password

**Secrets location:**
- `.env.example` - Template with placeholder values
- `.env` - Local environment file (gitignored)
- Render.com dashboard - Production environment variables
- Vercel dashboard - Frontend environment variables
- Docker user secrets - `fleetos-api-secrets` (development)

## Webhooks & Callbacks

**Incoming:**
- None detected

**Outgoing:**
- None detected

## Background Jobs

**Hosted Services (ASP.NET Core `IHostedService`):**
- `AlertJob` - Fleet alert processing
  - Location: `backend/src/FleetOS.Infrastructure/BackgroundJobs/AlertJob.cs`
- `RefuelReminderJob` - Fuel reminder notifications
  - Location: `backend/src/FleetOS.Infrastructure/BackgroundJobs/RefuelReminderJob.cs`

## Real-time Communication Details

**SignalR Hub:**
- Hub class: `FleetHub` (`backend/src/FleetOS.Infrastructure/Hubs/FleetHub.cs`)
- Endpoint: `/hubs/fleet`
- Auth: `[Authorize]` attribute (JWT required)
- Groups: Automatic tenant-based grouping on connect/disconnect
- Keep-alive: 10 seconds (configured in `Program.cs`)
- Client timeout: 30 seconds
- Detailed errors: Enabled

**Frontend Client:**
- Library: `@microsoft/signalr` v10.0.0
- Connection: Via Vite proxy in dev (`/hubs` → backend), direct in production
- Auth: `access_token` query parameter (WebSocket doesn't support custom headers)

## Security Headers

**Nginx (`nginx/nginx.conf`):**
- `X-Frame-Options: SAMEORIGIN`
- `X-Content-Type-Options: nosniff`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `Strict-Transport-Security: max-age=31536000; includeSubDomains` (HTTPS server)
- `Content-Security-Policy` - Configured per environment

**Vercel (`frontend/vercel.json`):**
- Same header set as Nginx
- CSP allows: `'self'`, `unsafe-inline`, `unsafe-eval`, Google Fonts, Supabase

---

*Integration audit: 2026-08-01*
