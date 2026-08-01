---
phase: 01-security-headers
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - backend/src/FleetOS.Api/Program.cs
  - backend/src/FleetOS.Api/FleetOS.Api.csproj
  - backend/tests/FleetOS.Tests/FleetOS.Tests.csproj
  - backend/tests/FleetOS.Tests/SecurityHeadersIntegrationTest.cs
  - frontend/vercel.json
autonomous: true
requirements:
  - HDR-01
  - HDR-02
  - HDR-03
  - HDR-04

must_haves:
  truths:
    - "Every backend API response includes Strict-Transport-Security (max-age=300), X-Frame-Options (DENY), X-Content-Type-Options (nosniff), Referrer-Policy (strict-origin-when-cross-origin), and Permissions-Policy headers"
    - "Frontend static assets served by Vercel return X-Frame-Options (DENY), X-Content-Type-Options (nosniff), Referrer-Policy, Permissions-Policy, and X-XSS-Protection (0) headers"
    - "X-XSS-Protection header is set to 0 on both backend and frontend — the deprecated 1; mode=block value is nowhere in any response"
    - "HSTS max-age is 300 seconds on the backend — verifiable via curl -I against any API endpoint"
  artifacts:
    - path: backend/src/FleetOS.Api/Program.cs
      provides: Security headers middleware configuration via NetEscapades
      contains: UseSecurityHeaders
    - path: frontend/vercel.json
      provides: Static security headers at CDN edge
      contains: Permissions-Policy
    - path: backend/tests/FleetOS.Tests/SecurityHeadersIntegrationTest.cs
      provides: Automated header verification via test server
  key_links:
    - from: backend/src/FleetOS.Api/Program.cs
      to: NetEscapades.AspNetCore.SecurityHeaders NuGet package
      via: UseSecurityHeaders() middleware call — must be FIRST in pipeline before UseSerilogRequestLogging
      pattern: UseSecurityHeaders
    - from: frontend/vercel.json
      to: All frontend responses at Vercel CDN edge
      via: headers[].source=/(.*) matches all paths
      pattern: headers
---

<objective>
Add consistent security headers to both backend (ASP.NET Core middleware) and frontend (Vercel CDN edge) to protect against clickjacking, MIME sniffing, protocol downgrade, and information leakage — with zero risk of breaking existing functionality.

Purpose: Establish the security header foundation that Phase 2 (CSP), Phase 3 (auth hardening), and Phase 4 (input validation) build upon. Security headers are additive protections — they cannot break existing features.
Output: Both backend API responses and frontend static assets return the correct security header set, verified by automated integration test and vercel.json validation.
</objective>

<execution_context>
@/home/derick/.config/opencode/gsd-core/workflows/execute-plan.md
@/home/derick/.config/opencode/gsd-core/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/STATE.md

## Decision References (NON-NEGOTIABLE)
@.planning/phases/01-security-headers/01-CONTEXT.md

## Source Files to Read Before Implementation
@backend/src/FleetOS.Api/Program.cs
@backend/src/FleetOS.Api/FleetOS.Api.csproj
@frontend/vercel.json
@backend/tests/FleetOS.Tests/FleetOS.Tests.csproj

## Research References
@.planning/research/STACK.md
@.planning/research/ARCHITECTURE.md
</context>

<tasks>

<task type="tracer" tdd="true">
  <name>Task 1: Backend Security Headers Middleware (NetEscapades)</name>
  <files>
    backend/src/FleetOS.Api/FleetOS.Api.csproj,
    backend/src/FleetOS.Api/Program.cs,
    backend/tests/FleetOS.Tests/FleetOS.Tests.csproj,
    backend/tests/FleetOS.Tests/SecurityHeadersIntegrationTest.cs
  </files>
  <read_first>
    backend/src/FleetOS.Api/Program.cs — current middleware pipeline (insert UseSecurityHeaders FIRST, before line 52 UseSerilogRequestLogging).
    backend/src/FleetOS.Api/FleetOS.Api.csproj — add NetEscapades package reference here.
    backend/tests/FleetOS.Tests/FleetOS.Tests.csproj — add Microsoft.AspNetCore.Mvc.Testing package + FleetOS.Api project reference.
    .planning/phases/01-security-headers/01-CONTEXT.md — Decision "Backend Headers Implementation" (per D-01, D-03, D-04) and Decision "Middleware Ordering" (per D-02).
  </read_first>
  <behavior>
    - Test 1: GET request to any endpoint returns response header Strict-Transport-Security with value containing max-age=300
    - Test 2: GET request returns X-Frame-Options: DENY
    - Test 3: GET request returns X-Content-Type-Options: nosniff
    - Test 4: GET request returns Referrer-Policy: strict-origin-when-cross-origin
    - Test 5: GET request returns Permissions-Policy header with camera, microphone, geolocation directives
    - Test 6: GET request returns X-XSS-Protection: 0 (NOT 1; mode=block)
    - Test 7: Response does NOT contain X-XSS-Protection: 1 anywhere
  </behavior>
  <action>
    Implements HDR-01, HDR-03, HDR-04 per locked decisions D-01, D-02, D-03, D-04.

    **Step 1 — Package Legitimacy (MANDATORY gate):**
    NetEscapades.AspNetCore.SecurityHeaders v1.3.1 is listed as [ASSUMED] — no Package Legitimacy Audit table exists in RESEARCH.md. Before installing, verify on nuget.org:
    - Navigate to https://www.nuget.org/packages/NetEscapades.AspNetCore.SecurityHeaders/1.3.1
    - Confirm: author is "Andrew Lock", download count is 1M+, latest version matches
    - If any mismatch (wrong author, suspiciously low downloads, unexpected version), STOP and report to human

    **Step 2 — Install package:**
    Run `dotnet add backend/src/FleetOS.Api/FleetOS.Api.csproj package NetEscapades.AspNetCore.SecurityHeaders --version 1.3.1`

    **Step 3 — Register security header policies in DI:**
    In `backend/src/FleetOS.Api/Program.cs`, add `builder.Services.AddSecurityHeaderPolicies()` to the service registration chain (after line 39 `AddInfrastructureServices`). Configure the default policy with:
    - `AddDefaultSecurityHeaders()` — provides baseline X-Frame-Options, X-Content-Type-Options, Referrer-Policy
    - HSTS override: max-age=300, includeSubdomains=true, preload=false (per D-04 incremental rollout — start conservative, increase after 1 week validation)
    - `X-XSS-Protection: 0` (per D-03 — explicitly disabled, NOT 1; mode=block; the deprecated XSS Auditor introduces vulnerabilities in older browsers)
    - `X-Frame-Options: DENY` — prevents all framing (clickjacking protection)
    - `Permissions-Policy` — restrict camera=(self), microphone=(), geolocation=(self) (microphone fully blocked per fleet app requirements)
    - Do NOT configure CSP in this phase — deferred to Phase 2 per CONTEXT.md

    **Step 4 — Place UseSecurityHeaders() FIRST in middleware pipeline (per D-02):**
    Insert `app.UseSecurityHeaders()` at line 52 BEFORE `app.UseSerilogRequestLogging()`. This ensures ALL responses get security headers, including 401/403/500 error responses. The middleware must wrap the entire pipeline.

    Current pipeline order after insertion:
    1. `app.UseSecurityHeaders()` ← NEW, FIRST
    2. `app.UseSerilogRequestLogging()` ← existing
    3. Swagger (dev only) ← existing
    4. `app.UseHttpsRedirection()` ← existing
    5. `app.UseCors("AllowedOrigins")` ← existing
    6. ... rest unchanged

    **Step 5 — Create integration test (Nyquist automated verification):**
    Add `Microsoft.AspNetCore.Mvc.Testing` package to `backend/tests/FleetOS.Tests/FleetOS.Tests.csproj`. Add project reference to `FleetOS.Api`.
    Create `backend/tests/FleetOS.Tests/SecurityHeadersIntegrationTest.cs` using `WebApplicationFactory<Program>` to spin up a test server, send a GET request to `/health`, and assert all expected security headers are present with correct values.
    Also add `public partial class Program { }` at the bottom of `backend/src/FleetOS.Api/Program.cs` (after the finally block) to expose the Program type for the test's WebApplicationFactory generic parameter.
  </action>
  <verify>
    <automated>dotnet test backend/tests/FleetOS.Tests/ --filter "FullyQualifiedName~SecurityHeadersIntegrationTest" --verbosity normal</automated>
  </verify>
  <acceptance_criteria>
    - NetEscapades.AspNetCore.SecurityHeaders v1.3.1 is installed in FleetOS.Api.csproj
    - `builder.Services.AddSecurityHeaderPolicies()` is registered with all header policies configured
    - `app.UseSecurityHeaders()` is the FIRST middleware call in Program.cs (before UseSerilogRequestLogging on line 52)
    - HSTS header contains `max-age=300` (not 31536000 or any other value)
    - X-XSS-Protection header value is exactly `0` (not `1; mode=block`)
    - X-Frame-Options is `DENY` (not SAMEORIGIN)
    - Permissions-Policy header is present with camera, microphone, geolocation directives
    - Integration test `SecurityHeadersIntegrationTest` exists and passes, asserting all 6 header values
    - `public partial class Program { }` is added to bottom of Program.cs for test accessibility
  </acceptance_criteria>
  <done>
    All backend API responses include HSTS (max-age=300), X-Frame-Options (DENY), X-Content-Type-Options (nosniff), Referrer-Policy (strict-origin-when-cross-origin), Permissions-Policy, and X-XSS-Protection (0). Automated integration test passes confirming all headers.
  </done>
</task>

<task type="auto">
  <name>Task 2: Frontend Security Headers (Vercel Configuration)</name>
  <files>frontend/vercel.json</files>
  <read_first>
    frontend/vercel.json — existing headers config. Current state has X-Frame-Options: SAMEORIGIN (weak — needs DENY), has HSTS hardcoded at 1 year (must remove — Vercel handles HSTS at platform level), has full CSP with unsafe-inline and unsafe-eval (must remove — deferred to Phase 2).
    .planning/phases/01-security-headers/01-CONTEXT.md — Decision "Frontend Headers (Vercel)" for exact header values.
  </read_first>
  <action>
    Implements HDR-02, HDR-03 per locked decision D-03, D-05 (Vercel headers), D-06 (CSP deferred).

    Update the `headers` array in `frontend/vercel.json` for the `source: "/(.*)"` block:

    **REMOVE these headers:**
    - `Strict-Transport-Security` — Vercel automatically applies HSTS at the CDN/platform level. Do NOT duplicate it. If present in vercel.json, it can conflict with Vercel's automatic HSTS management.
    - `Content-Security-Policy` — CSP is deferred to Phase 2 (per CONTEXT.md decision). The current CSP contains `unsafe-inline` and `unsafe-eval` in script-src which are security vulnerabilities. Removing it now eliminates the weak CSP entirely; Phase 2 will implement a strict hash-based CSP.

    **MODIFY this header:**
    - `X-Frame-Options` — change value from `SAMEORIGIN` to `DENY` (per success criteria #3 and D-03 pattern; DENY is the strictest clickjacking protection)

    **ADD these headers:**
    - `X-XSS-Protection` with value `0` (per D-03 — disabled, the deprecated XSS Auditor can introduce XSS in older browsers)
    - `Permissions-Policy` with value `camera=(self), microphone=(), geolocation=(self)` (matching backend Permissions-Policy for consistency)

    **KEEP unchanged:**
    - `X-Content-Type-Options: nosniff` — already correct
    - `Referrer-Policy: strict-origin-when-cross-origin` — already correct
    - The `/assets/(.*)` cache-control block — unrelated to security headers

    **Validate:** Ensure the resulting JSON is valid (no trailing commas, correct bracket nesting). The headers array should have exactly 2 entries: the security headers block and the cache-control block.
  </action>
  <verify>
    <automated>node -e "const v=require('./frontend/vercel.json'); const h=v.headers[0].headers; const keys=h.map(x=>x.key); console.assert(keys.includes('X-Frame-Options'), 'missing X-Frame-Options'); console.assert(h.find(x=>x.key==='X-Frame-Options').value==='DENY', 'X-Frame-Options must be DENY'); console.assert(h.find(x=>x.key==='X-XSS-Protection').value==='0', 'X-XSS-Protection must be 0'); console.assert(!keys.includes('Strict-Transport-Security'), 'HSTS must NOT be in vercel.json'); console.assert(!keys.includes('Content-Security-Policy'), 'CSP must NOT be in vercel.json'); console.assert(keys.includes('Permissions-Policy'), 'missing Permissions-Policy'); console.log('vercel.json security headers: ALL CHECKS PASSED')"</automated>
  </verify>
  <acceptance_criteria>
    - X-Frame-Options value is `DENY` (not SAMEORIGIN)
    - X-XSS-Protection is present with value `0`
    - Permissions-Policy is present with camera, microphone, geolocation directives
    - X-Content-Type-Options remains `nosniff`
    - Referrer-Policy remains `strict-origin-when-cross-origin`
    - Strict-Transport-Security is NOT present (Vercel handles it at platform level)
    - Content-Security-Policy is NOT present (deferred to Phase 2)
    - JSON is valid and parseable
    - The `/assets/(.*)` cache-control block is preserved
  </acceptance_criteria>
  <done>
    vercel.json serves correct security headers (X-Frame-Options: DENY, X-XSS-Protection: 0, Permissions-Policy, X-Content-Type-Options: nosniff, Referrer-Policy) on all frontend responses, with HSTS and CSP removed (handled by Vercel platform and Phase 2 respectively).
  </done>
</task>

</tasks>

<threat_model>
## Trust Boundaries

| Boundary | Description |
|----------|-------------|
| Browser ← Backend API | Security headers traverse this boundary on every HTTP response |
| Browser ← Vercel CDN | Static security headers traverse this boundary on every frontend response |

## STRIDE Threat Register

| Threat ID | Category | Component | Severity | Disposition | Mitigation Plan |
|-----------|----------|-----------|----------|-------------|-----------------|
| T-01-01 | Spoofing | HSTS max-age too low | low | accept | Starting at 300s is intentional incremental rollout per D-04. Rollout plan documented in CONTEXT.md with validation criteria for each step. Risk: brief window for downgrade attack until max-age increases. |
| T-01-02 | Tampering | NuGet package supply chain | medium | mitigate | Verify NetEscapades.AspNetCore.SecurityHeaders on nuget.org before install — confirm author (Andrew Lock), download count (1M+), and version (1.3.1). Package is well-maintained with 1.4M+ downloads. |
| T-01-03 | Information Disclosure | Missing X-XSS-Protection: 0 | low | mitigate | Explicitly set X-XSS-Protection: 0 on both backend and frontend to disable the deprecated XSS Auditor, preventing it from introducing vulnerabilities in older browsers (per D-03). |
| T-01-04 | Elevation of Privilege | Weak X-Frame-Options (SAMEORIGIN) | low | mitigate | Change frontend X-Frame-Options from SAMEORIGIN to DENY to fully prevent clickjacking — no framing allowed from any origin. |
</threat_model>

<verification>
## End-to-End Header Verification

1. **Backend headers:** Start the API locally (`dotnet run --project backend/src/FleetOS.Api`), then verify with:
   ```bash
   curl -I http://localhost:5000/health
   ```
   Expected headers: `Strict-Transport-Security: max-age=300; includeSubdomains`, `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, `Permissions-Policy: camera=(self), microphone=(), geolocation=(self)`, `X-XSS-Protection: 0`

2. **Frontend headers:** After deploying to Vercel (or using `vercel dev` locally):
   ```bash
   curl -I https://your-app.vercel.app/
   ```
   Expected headers: `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, `Permissions-Policy: camera=(self), microphone=(), geolocation=(self)`, `X-XSS-Protection: 0`. Must NOT contain `Content-Security-Policy` or `Strict-Transport-Security`.

3. **Automated tests:** `dotnet test backend/tests/FleetOS.Tests/ --filter "FullyQualifiedName~SecurityHeadersIntegrationTest"`

4. **Negative check:** `curl -I` output must NOT contain `X-XSS-Protection: 1` anywhere on any endpoint.
</verification>

<success_criteria>
1. Every backend API response includes HSTS (max-age=300), X-Frame-Options (DENY), X-Content-Type-Options (nosniff), Referrer-Policy (strict-origin-when-cross-origin), and Permissions-Policy headers — verifiable via `curl -I` against `/health`
2. Frontend static assets served by Vercel return X-Frame-Options (DENY), X-Content-Type-Options (nosniff), Referrer-Policy, Permissions-Policy, and X-XSS-Protection (0) — verified via vercel.json validation
3. X-XSS-Protection header is set to `0` on both backend and frontend — the deprecated `1; mode=block` value is nowhere in any response
4. HSTS max-age is exactly 300 seconds on the backend — verifiable in response headers via curl
5. Automated integration test passes confirming all header values
</success_criteria>

<output>
Create `.planning/phases/01-security-headers/01-01-SUMMARY.md` when done
</output>
