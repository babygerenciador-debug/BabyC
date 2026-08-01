# Phase 1 Context: Security Headers

**Phase:** 1
**Name:** Security Headers
**Date:** 2026-08-01

## Domain

Implement security headers middleware on the ASP.NET Core 10 backend and configure security headers on the Vercel frontend. This phase establishes the security header foundation that subsequent phases (CSP, auth hardening) build upon.

## Requirements

Phase requirements (from REQUIREMENTS.md):
- HDR-01: Backend implements security headers middleware (HSTS, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, Permissions-Policy) using NetEscapades.AspNetCore.SecurityHeaders
- HDR-02: Frontend sets security headers via vercel.json configuration
- HDR-03: X-XSS-Protection set to `0` (disabled) on all endpoints
- HDR-04: HSTS configured with incremental rollout (start 300s max-age, increase after validation)

## Decisions

### Backend Headers Implementation

**Decision:** Use `NetEscapades.AspNetCore.SecurityHeaders` v1.3.1 NuGet package.

**Rationale:** De-facto standard for ASP.NET Core security headers. Maintained by Andrew Lock, 1.4M+ downloads. Fluent API for configuring CSP, HSTS, Permissions-Policy, COOP/COEP/CORP. Supports per-endpoint policies.

**Implementation:**
- Add package: `dotnet add package NetEscapades.AspNetCore.SecurityHeaders --version 1.3.1`
- Configure in `Program.cs` BEFORE `UseRouting()`:
  ```csharp
  app.UseSecurityHeaders(policies =>
      policies.AddDefaultSecurityHeaders()
              .AddCustomHstsOptions(options =>
              {
                  options.MaxAge = 300; // Start conservative (5 minutes)
                  options.IncludeSubdomains = true;
                  options.Preload = false; // Enable after validation
              })
              .AddCustomHeader("X-XSS-Protection", "0") // Disabled, not 1; mode=block
              .AddCustomHeader("X-Frame-Options", "DENY")
              .AddCustomHeader("X-Content-Type-Options", "nosniff")
              .AddReferrerPolicy("strict-origin-when-cross-origin")
              .AddPermissionsPolicy(new PermissionsPolicyFeatureCollection
              {
                  { "camera", new[] { "self" } },
                  { "microphone", new[] { "none" } },
                  { "geolocation", new[] { "self" } }
              })
  );
  ```

### Middleware Ordering

**Decision:** Security headers middleware MUST be placed FIRST in the pipeline, before `UseRouting()`, `UseCors()`, `UseAuthentication()`, and `UseAuthorization()`.

**Rationale:** Security headers should be applied to ALL responses, including error responses (401, 403, 500). Placing it first ensures headers are present even if authentication fails or an exception occurs. This is the recommended order from the NetEscapades documentation and OWASP Secure Headers Project.

**Order in Program.cs:**
1. `UseSecurityHeaders()` ← FIRST
2. `UseCors()`
3. `UseRouting()`
4. `UseAuthentication()`
5. `UseAuthorization()`
6. `UseEndpoints()` / `MapControllers()`

### HSTS Rollout Strategy

**Decision:** Incremental rollout — start with 300s (5 minutes), then increase to 1 day, 1 week, 1 month, and finally 1 year after validation.

**Rationale:** HSTS with long max-age can permanently lock out users if certificate issues occur. Starting conservative allows validation that HTTPS is working correctly across all endpoints and clients before committing to long-term enforcement.

**Rollout plan:**
- Phase 1 (this phase): `max-age=300` (5 minutes)
- After 1 week of no issues: increase to `max-age=86400` (1 day)
- After 1 month: increase to `max-age=604800` (1 week)
- After 3 months: increase to `max-age=31536000` (1 year) and enable `preload`

**Validation criteria for each step:**
- No mixed-content warnings in browser console
- All API endpoints accessible via HTTPS
- No client reports of connection failures
- SSL Labs test passes with A+ rating

### X-XSS-Protection Header

**Decision:** Set `X-XSS-Protection: 0` (disabled) on all endpoints.

**Rationale:** The XSS Auditor was removed from Chrome 78+ and Edge 17+. In older browsers where it's still present, `X-XSS-Protection: 1; mode=block` can INTRODUCE XSS vulnerabilities by causing the browser to block legitimate content. Setting it to `0` explicitly disables the auditor, which is the recommended approach from MDN, OWASP, and Microsoft.

**Reference:** [MDN: X-XSS-Protection](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-XSS-Protection) — "This feature is obsolete. Although it may still work in some browsers, its use is discouraged since it can introduce security vulnerabilities."

### Frontend Headers (Vercel)

**Decision:** Use `vercel.json` `headers` configuration for static security headers on all frontend responses.

**Rationale:** Vercel's `vercel.json` headers are applied at the CDN edge with zero latency. For a static SPA, this is simpler and more performant than using Vercel Edge Middleware. Since the frontend is a static Vite build, per-request nonce generation is unnecessary — hash-based CSP (Phase 2) will handle script trust.

**Configuration in `vercel.json`:**
```json
{
  "headers": [
    {
      "source": "/(.*)",
      "headers": [
        { "key": "X-Frame-Options", "value": "DENY" },
        { "key": "X-Content-Type-Options", "value": "nosniff" },
        { "key": "Referrer-Policy", "value": "strict-origin-when-cross-origin" },
        { "key": "Permissions-Policy", "value": "camera=(self), microphone=(), geolocation=(self)" },
        { "key": "X-XSS-Protection", "value": "0" }
      ]
    }
  ]
}
```

**Note:** HSTS is handled automatically by Vercel's HTTPS termination. Do NOT set HSTS in `vercel.json` — Vercel manages this at the platform level.

### CSP Strategy (Deferred to Phase 2)

**Decision:** Content Security Policy hardening is deferred to Phase 2. This phase establishes the foundation (other security headers); Phase 2 implements strict CSP.

**Rationale:** CSP is the most complex security change and likely to break functionality. Separating it into Phase 2 allows focused testing and Report-Only mode deployment before enforcement.

## Canonical Refs

- `.planning/REQUIREMENTS.md` — Phase 1 requirements (HDR-01 through HDR-04)
- `.planning/research/STACK.md` — Security headers stack recommendations (lines 1-100)
- `.planning/research/ARCHITECTURE.md` — Middleware ordering and two-layer security architecture
- `.planning/research/PITFALLS.md` — X-XSS-Protection deprecation pitfall
- `backend/src/FleetOS.Api/Program.cs` — Backend middleware pipeline (target file for security headers)
- `frontend/vercel.json` — Vercel configuration (target file for frontend headers)

## Code Context

**Reusable assets:**
- Backend middleware pipeline exists in `backend/src/FleetOS.Api/Program.cs` — security headers middleware will be inserted here
- Frontend Vercel config exists in `frontend/vercel.json` — headers will be added here
- No existing security headers middleware — this is a new addition

**Integration points:**
- Backend: Insert `UseSecurityHeaders()` as the FIRST middleware in the pipeline
- Frontend: Add `headers` array to existing `vercel.json` structure

## Deferred Ideas

None — all scope is within Phase 1 boundaries.

---

*Context captured: 2026-08-01 after Phase 1 discussion*