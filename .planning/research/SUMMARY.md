# Research Summary: Security Hardening

**Domain:** Web application security — React SPA + ASP.NET Core API
**Researched:** 2026-08-01
**Overall confidence:** HIGH

## Executive Summary

The security hardening stack for a React 19 + Vite frontend on Vercel paired with an ASP.NET Core 10 backend on Render.com is well-defined with mature, battle-tested tooling. The primary recommendation is a **nonce-based or hash-based strict CSP** on the frontend (via `vite-plugin-csp-guard` or Vercel `vercel.json` headers), combined with **NetEscapades.AspNetCore.SecurityHeaders v1.3.1** on the backend for comprehensive header coverage.

Key findings: React 19 production builds do not require `unsafe-inline` or `unsafe-eval`, so a strict CSP is achievable without weakening security. Vercel handles HTTPS/HSTS automatically but does not inject security headers — these must be set in `vercel.json` or via Edge Middleware. ASP.NET Core 10 includes built-in rate limiting (`Microsoft.AspNetCore.RateLimiting`) which supports partitioned limiters by IP, user, or API key.

Render.com provides TLS and basic DDoS protection but does not inject security headers or offer a WAF. For production, placing Cloudflare in front of Render adds WAF, edge rate limiting, and bot management. The CORS configuration must use explicit origin lists (never `*` with credentials), and source maps must be disabled in production builds.

## Key Findings

**Stack:** NetEscapades.AspNetCore.SecurityHeaders 1.3.1 + vite-plugin-csp-guard 2.1+ + ASP.NET Core 10 built-in rate limiting
**Architecture:** Two-layer security — Vercel edge headers (static) + ASP.NET Core middleware (dynamic per-request). CSP via hash-based `strict-dynamic` for static SPA, nonce-based for any server-rendered HTML.
**Critical pitfall:** `X-XSS-Protection` header MUST be set to `0` (disabled), not `1; mode=block`. The XSS auditor was removed from Chrome 78+ and can introduce new XSS vectors in older browsers.

## Implications for Roadmap

Based on research, suggested phase structure:

1. **Core Security Headers** - Foundation layer; all subsequent phases depend on headers being correct
   - Addresses: HSTS, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, Permissions-Policy, COOP/COEP/CORP
   - Avoids: Missing headers that security scanners flag immediately

2. **CSP Implementation** - Most complex security change; affects all frontend code
   - Addresses: Strict CSP with hash-based or nonce-based policy, removal of unsafe-inline/unsafe-eval
   - Avoids: Breaking existing frontend functionality with overly strict policy

3. **Authentication Rate Limiting** - Protects against brute-force and credential stuffing
   - Addresses: Sliding window rate limiter on auth endpoints, token bucket for general API
   - Avoids: Account takeover via unlimited login attempts

4. **CORS Hardening** - Locks down cross-origin access
   - Addresses: Explicit origin allowlists, credential handling, preflight caching
   - Avoids: Overly permissive CORS that allows any origin to call the API

5. **Bundle Security** - Minimizes information exposure in production JS
   - Addresses: Source map disabling, console removal, secret scrubbing
   - Avoids: Accidental secret leakage via environment variables or debug code

**Phase ordering rationale:**
- Headers first because they're trivial to implement and provide immediate protection
- CSP second because it's the most likely to break functionality and needs testing
- Rate limiting third because auth endpoints are the most attacked
- CORS fourth because it requires coordination between frontend and backend deployments
- Bundle security last because it's a build-time concern, not a runtime security control

**Research flags for phases:**
- Phase 2 (CSP): Likely needs deeper research — test each third-party library for CSP compatibility
- Phase 3 (Rate limiting): Standard patterns, unlikely to need research

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | NetEscapades is the de-facto standard with 1.4M+ downloads. Vite plugin is actively maintained. |
| Features | HIGH | All recommendations based on official Microsoft/MDN/OWASP documentation from 2025-2026. |
| Architecture | HIGH | Two-layer approach (edge + backend) is the standard pattern for split-hosting deployments. |
| Pitfalls | HIGH | X-XSS-Protection deprecation is well-documented. CSP breaking changes are common but predictable. |

## Gaps to Address

- **Vercel Edge Middleware for per-request CSP**: If the app needs per-request nonce generation (e.g., for third-party scripts that can't use strict-dynamic), a Vercel Edge Middleware pattern needs to be prototyped.
- **Cloudflare WAF rules**: Specific Cloudflare WAF rule sets for ASP.NET Core + React have not been researched.
- **PostgreSQL connection security**: Render's managed PostgreSQL SSL enforcement is documented but connection pooling (PgBouncer) interaction with SSL needs validation.
- **Testing CSP in CI**: No research done on automated CSP violation testing in CI/CD pipelines (e.g., Playwright + CSP report collection).
