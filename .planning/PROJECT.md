# FleetOS — Security Remediation

## What This Is

FleetOS is an enterprise fleet management platform (React 19 frontend on Vercel, .NET 10 backend on Render.com, PostgreSQL 16) for Baby Turismo. A penetration test using HexStrike AI MCP tools identified multiple security vulnerabilities — from information disclosure in the client-side bundle to missing security headers. This project focuses exclusively on remediating those findings plus pending items from the internal security audit.

## Core Value

The application must be secure against the vulnerabilities identified in the pentest and internal audit before any new features are built. Security hardening is the single priority.

## Requirements

### Validated

- JWT-based authentication with refresh token flow — existing
- Role-based access control (Admin/Driver) — existing
- Session storage for auth tokens (migrated from localStorage) — existing (C-03 fixed)
- Owner salary moved to backend tenant config — existing (C-01 fixed)
- HTTPS termination via Vercel (frontend) and Render (backend) — existing (C-02 resolved)
- BCrypt password hashing on backend — existing

### Active

- [ ] Harden CSP: remove `unsafe-inline` and `unsafe-eval` from script-src
- [ ] Add backend security headers (HSTS, X-Frame-Options, CSP, X-Content-Type-Options)
- [ ] Add X-XSS-Protection header on frontend and backend
- [ ] Reduce JS bundle info disclosure (minimize auth logic exposure in client code)
- [ ] Move inline JavaScript to external files with CSP nonces
- [ ] Harden route guards with JWT expiry validation (M-01)
- [ ] Add file type/size validation on frontend before upload (M-03)
- [ ] Mask CNH numbers in driver lists (L-01)
- [ ] Add rate limiting on authentication endpoints
- [ ] Harden CORS configuration for API endpoints

### Out of Scope

- New features (fleet management, finance, inventory modules) — not part of security remediation
- Test coverage improvements — important but separate from security scope
- Code quality refactoring (any types, console.log cleanup, handler boilerplate) — deferred to post-security phase
- Supabase migration — not a security requirement
- Mobile app development — web-first
- OAuth/social login — email/password sufficient for current needs

## Context

- Frontend: React 19 + Vite 8 + TypeScript 6, deployed on Vercel
- Backend: ASP.NET Core 10 + EF Core 10 + MediatR (CQRS), deployed on Render.com
- Database: PostgreSQL 16
- Real-time: SignalR for live updates
- File storage: Supabase Storage SDK
- Internal audit (`SECURITY-AUDIT.md`) identified 14 items; 4 critical/high already fixed
- External pentest (HexStrike AI) found: info disclosure in JS bundle, weak CSP, missing security headers
- Backend API at baby-c.onrender.com was unreachable during pentest (possibly not deployed)
- Zero test coverage across entire stack (known concern, out of scope for this project)

## Constraints

- **Tech stack**: Must remain React + .NET — no language/framework changes
- **Deployment**: Vercel (frontend) + Render.com (backend) — security headers must work within these platforms
- **No downtime**: Security fixes must not break existing functionality
- **Portuguese (pt-BR)**: UI language — all user-facing text in Portuguese

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Security remediation only | Pentest findings are blocking; features can wait | — Pending |
| CSP hardening over relaxation | `unsafe-inline`/`unsafe-eval` significantly increase XSS surface | — Pending |
| Backend headers in middleware | Consistent application across all API endpoints | — Pending |
| sessionStorage for auth (already done) | Token expires on tab close, reduces persistent XSS impact | ✓ Good |
| No httpOnly cookie migration now | Would require significant auth refactor; sessionStorage is adequate interim | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-08-01 after initialization*