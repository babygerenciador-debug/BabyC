# Technology Stack

**Analysis Date:** 2026-08-01

## Languages

**Primary:**
- C# 10.0 (.NET 10) - Backend API (`backend/src/`)
- TypeScript 6.0.2 - Frontend application (`frontend/src/`)

**Secondary:**
- SQL - PostgreSQL database queries (via EF Core)
- Dockerfile syntax - Container definitions
- Nginx configuration - Reverse proxy (`nginx/nginx.conf`)

## Runtime

**Environment:**
- .NET 10.0 Runtime (ASP.NET Core 10.0) - Backend API
- Node.js 22 Alpine - Frontend build process

**Package Manager:**
- NuGet (.NET) - Backend packages
- npm (Node.js) - Frontend packages
- Lockfile: `frontend/package-lock.json` present, backend uses `*.csproj` version pinning

## Frameworks

**Core:**
- ASP.NET Core 10.0 Web API - Backend REST API framework (`backend/src/FleetOS.Api/`)
- React 19.2.7 - Frontend UI library (`frontend/src/`)
- Vite 8.1.1 - Frontend build tool and dev server

**Testing:**
- xUnit 2.9.2 - Backend unit testing (`backend/tests/FleetOS.Tests/`)
- Moq 4.20.72 - Backend mocking framework
- FluentAssertions 6.12.2 - Backend assertion library
- Microsoft.EntityFrameworkCore.InMemory 10.0.4 - In-memory database for tests
- Bogus 35.6.1 - Test data generation
- No frontend testing framework detected

**Build/Dev:**
- Vite 8.1.1 - Frontend bundler, HMR dev server
- TypeScript 6.0.2 - Type checking (no emit, used with Vite)
- oxlint 1.71.0 - Frontend linting (React + TypeScript rules)
- dotnet CLI - Backend build, restore, publish
- Docker - Multi-stage builds for both frontend and backend

## Key Dependencies

**Critical (Backend):**
- Microsoft.EntityFrameworkCore 10.0.4 - ORM for database access (`backend/src/FleetOS.Infrastructure/`)
- Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1 - PostgreSQL provider for EF Core
- MediatR 12.* - CQRS/MediatR pattern implementation (`backend/src/FleetOS.Application/`, `backend/src/FleetOS.Domain/`)
- FluentValidation 11.* - Request validation
- Microsoft.AspNetCore.Authentication.JwtBearer 10.* - JWT authentication
- StackExchange.Redis 2.9.11 - Redis client for caching
- Supabase 1.1.1 - Supabase Storage SDK for file uploads
- BCrypt.Net-Next 4.0.3 - Password hashing
- Serilog 8.* - Structured logging (Console, File, CorrelationId enrichers)
- Swashbuckle.AspNetCore 7.* - Swagger/OpenAPI documentation
- AspNetCoreRateLimit 5.* - Rate limiting middleware
- CorrelationId 3.* - Request correlation ID tracking

**Critical (Frontend):**
- React 19.2.7 - UI component library
- React Router DOM 7.18.1 - Client-side routing
- Zustand 5.0.14 - Global state management (auth, theme)
- @tanstack/react-query 5.101.2 - Server state management, data fetching, caching
- Axios 1.18.1 - HTTP client for API calls
- Zod 3.25.76 - Schema validation
- React Hook Form 7.81.0 + @hookform/resolvers 3.10.0 - Form handling with Zod validation
- SignalR Client (@microsoft/signalr 10.0.0) - Real-time WebSocket communication
- ECharts 6.1.0 + echarts-for-react 3.0.6 - Charting library
- Recharts 3.9.2 - Alternative charting library
- Framer Motion 11.18.2 - Animation library
- React Grid Layout 2.2.3 - Draggable grid layouts
- @hello-pangea/dnd 18.0.1 - Drag and drop
- Lucide React 1.24.0 - Icon library
- Sonner 2.0.7 - Toast notifications
- date-fns 4.4.0 - Date manipulation
- html2canvas 1.4.1 + jspdf 4.2.1 - PDF generation from HTML
- Tailwind Merge 3.6.0 - Tailwind CSS class merging utility
- clsx 2.1.1 - Conditional className utility

**Infrastructure:**
- Docker Compose - Local multi-container orchestration (`docker-compose.yml`)
- Nginx 1.25 Alpine - Reverse proxy, static file serving, SSL termination (`nginx/nginx.conf`)
- Redis 7.2 Alpine - Caching layer (`docker-compose.yml`)
- PostgreSQL 15 - Primary database (`backend/docker-compose.yml`)

## Configuration

**Environment:**
- Backend: ASP.NET Core configuration system (appsettings.json, environment variables, user secrets)
  - `backend/src/FleetOS.Api/Program.cs` - Configuration loading
  - `.env.example` - Environment variable template
  - No `appsettings.json` committed (uses environment variables)
- Frontend: Vite environment variables
  - `frontend/vite.config.ts` - Build-time config
  - `VITE_API_URL` - API base URL
  - `VITE_APP_NAME`, `VITE_APP_VERSION` - App metadata
  - `frontend/vercel.json` - Production deployment config (Vercel)

**Build:**
- Backend: `backend/Dockerfile` - Multi-stage .NET build
  - Restore → Build → Publish → Runtime
- Frontend: `frontend/Dockerfile` - Multi-stage Node build
  - npm install → npm run build → Nginx static serving
- Configuration files:
  - `backend/FleetOS.sln` - Solution file
  - `backend/Directory.Build.props` - Shared MSBuild properties
  - `frontend/tsconfig.json` - TypeScript config with path aliases
  - `frontend/vite.config.ts` - Vite config with path aliases and proxy

**Deployment:**
- Backend: Render.com (Web Service, Docker image)
  - `backend/render.yaml` - Render deployment config
  - Health check: `/health`
  - Port: 8080
- Frontend: Vercel (static hosting)
  - `frontend/vercel.json` - Vercel config
  - SPA rewrite rules, security headers, cache control
- Local development: Docker Compose
  - `docker-compose.yml` - Full stack (API, frontend, Redis, Nginx)
  - `backend/docker-compose.yml` - Backend only (API, PostgreSQL, Redis)

## Platform Requirements

**Development:**
- .NET 10 SDK - Backend development
- Node.js 22+ - Frontend development
- Docker & Docker Compose - Local containerized environment
- PostgreSQL 15 (or Docker container) - Database
- Redis 7.2 (or Docker container) - Caching

**Production:**
- Backend hosting: Render.com (Docker runtime) or any ASP.NET Core 10 compatible host
- Frontend hosting: Vercel (static) or any static file server
- Database: Supabase PostgreSQL (managed) or PostgreSQL 15+
- Caching: Redis 7.2+ (optional, falls back to in-memory)
- Reverse proxy: Nginx 1.25+ (for SSL termination, routing)

**Container Images:**
- Backend runtime: `mcr.microsoft.com/dotnet/aspnet:10.0`
- Backend build: `mcr.microsoft.com/dotnet/sdk:10.0`
- Frontend build: `node:22-alpine`
- Frontend runtime: `nginx:1.25-alpine`
- Redis: `redis:7.2-alpine`
- PostgreSQL: `postgres:15`

---

*Stack analysis: 2026-08-01*
