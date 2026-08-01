# Domain Pitfalls — Security Hardening

**Domain:** Web application security (React SPA + ASP.NET Core API on Vercel/Render)
**Researched:** 2026-08-01

## Critical Pitfalls

Mistakes that cause rewrites or major issues.

### Pitfall 1: X-XSS-Protection Header Set to `1; mode=block`

**What goes wrong:** Setting `X-XSS-Protection: 1; mode=block` introduces XSS vulnerabilities in older browsers. Chrome removed this header's functionality in version 78 (2019). Firefox never implemented it. The header is deprecated by the W3C.

**Why it happens:** Developers copy outdated security header recommendations from old blog posts or Stack Overflow answers. Many "security header checklist" articles still recommend `1; mode=block`.

**Consequences:** In IE11 and older Safari, the XSS auditor can be exploited to block legitimate scripts or create new XSS vectors by manipulating the DOM in ways the auditor doesn't expect. Security scanners (like Mozilla Observatory) flag this as a finding.

**Prevention:** Set `X-XSS-Protection: 0` to explicitly disable the auditor. Rely on CSP for XSS protection instead. This is the recommendation from OWASP, MDN, and all major security frameworks.

**Detection:** Run Mozilla Observatory or SecurityHeaders.com scan. Any value other than `0` or absent is a problem.

### Pitfall 2: CSP Breaks Existing Functionality After Deployment

**What goes wrong:** Deploying a strict CSP that blocks legitimate scripts. Third-party analytics, chat widgets, A/B testing tools, or even first-party code using `eval()` or inline event handlers break. Users see a non-functional page with CSP violation errors in the console.

**Why it happens:** Developers deploy CSP in enforce mode without testing. They assume their code is CSP-compatible but haven't audited all third-party scripts. Some libraries (e.g., `styled-components`, legacy analytics) use inline scripts or `eval()` that break under strict CSP.

**Consequences:** Complete site outage or degraded functionality. Users cannot interact with the application. Support tickets flood in. Rollback required.

**Prevention:**
1. Deploy CSP in **report-only mode** first: `Content-Security-Policy-Report-Only`
2. Set up a reporting endpoint to collect violations
3. Monitor violations for 1-2 weeks before switching to enforce mode
4. Audit all third-party scripts for CSP compatibility
5. Use `vite-plugin-csp-guard` in dev mode to catch violations during development

**Detection:** Monitor `Content-Security-Policy-Report-Only` violations. Set up alerts for CSP violation reports.

### Pitfall 3: `unsafe-inline` in `script-src`

**What goes wrong:** Setting `script-src 'self' 'unsafe-inline'` defeats CSP's primary purpose. An attacker who finds an XSS vulnerability can inject inline scripts that execute because `'unsafe-inline'` allows them.

**Why it happens:** Developers add `'unsafe-inline'` because their app breaks without it. Inline event handlers (`onclick="..."`), inline `<script>` tags, or CSS-in-JS libraries that inject styles at runtime all require `'unsafe-inline'` in their respective directives.

**Consequences:** CSP provides no XSS protection. The header is present but useless. Security auditors flag it. If an XSS vulnerability exists, attackers can exploit it fully.

**Prevention:**
- For scripts: Use hash-based or nonce-based CSP. React 19 + Vite production builds don't need inline scripts.
- For styles: `'unsafe-inline'` in `style-src` is acceptable. Inline styles are a lower-risk vector.
- For third-party scripts: Use `strict-dynamic` to propagate trust from the entry script.

**Detection:** Audit CSP header. If `script-src` contains `'unsafe-inline'`, it's a finding.

### Pitfall 4: Rate Limiting Not Applied to Auth Endpoints

**What goes wrong:** Login, registration, and password reset endpoints have no rate limiting. Attackers can perform unlimited brute-force login attempts, credential stuffing, or account enumeration.

**Why it happens:** Developers focus on business logic and forget to add rate limiting. Or they add a global rate limiter but don't configure stricter limits for auth endpoints specifically.

**Consequences:** Account takeover via credential stuffing. Brute-force attacks succeed. Automated account creation. Password spray attacks.

**Prevention:** Apply a strict sliding-window rate limiter to all auth endpoints:
```csharp
options.AddPolicy("auth", ctx =>
    RateLimitPartition.GetSlidingWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
        }));
```

**Detection:** Monitor failed login attempts. If a single IP can make 100+ login attempts without being blocked, rate limiting is missing or misconfigured.

### Pitfall 5: Source Maps Shipped to Production

**What goes wrong:** JavaScript source maps (`.map` files) are deployed to production. Attackers download them and get the full, readable source code of the application — including API keys, business logic, and internal comments.

**Why it happens:** Developer sets `sourcemap: true` for debugging and forgets to disable it. Or the CI/CD pipeline doesn't verify that source maps are excluded from the production build.

**Consequences:** Full source code exposure. API keys embedded in code are stolen. Business logic is reverse-engineered. Internal URLs and endpoints are discovered.

**Prevention:**
1. Set `build.sourcemap: false` in `vite.config.ts` (this is the default for Vite production builds)
2. Verify no `.map` files in the deployment output
3. Upload source maps to error tracking services (Sentry, LogRocket) privately — never serve them publicly
4. Add a CI check that fails if `.map` files are found in the build output

**Detection:** Check deployment output for `.map` files. Use browser DevTools Network tab to verify no `.map` files are loaded.

## Moderate Pitfalls

### Pitfall 6: CORS `AllowAnyOrigin()` with `AllowCredentials()`

**What goes wrong:** Setting `AllowAnyOrigin()` combined with `AllowCredentials()` causes browsers to reject the response. The combination is explicitly forbidden by the CORS specification.

**Prevention:** Use `WithOrigins("https://app.vercel.app")` with `AllowCredentials()`. Never use `AllowAnyOrigin()` when credentials are involved.

### Pitfall 7: Security Headers Middleware Placed Too Late

**What goes wrong:** `UseSecurityHeaders()` is placed after `UseStaticFiles()` or `UseExceptionHandler()`. Static file responses and error responses don't get security headers.

**Prevention:** Place `UseSecurityHeaders()` as the **first** middleware in the pipeline. This ensures all responses, including 404s and 500s, get security headers.

### Pitfall 8: Rate Limiting by `X-Forwarded-For` Header

**What goes wrong:** Rate limiting by `X-Forwarded-For` header is spoofable. Attackers can set arbitrary `X-Forwarded-For` values to bypass rate limits.

**Prevention:** Use `httpContext.Connection.RemoteIpAddress` which is populated by Render's reverse proxy from the actual client IP. Render sets this correctly from the `X-Forwarded-For` header, but after the proxy has validated it.

### Pitfall 9: `VITE_` Prefix on Backend Secrets

**What goes wrong:** A backend secret (database password, JWT secret, API key) is prefixed with `VITE_`. Vite injects all `VITE_`-prefixed environment variables into the client bundle at build time. The secret is now visible to anyone who views the page source.

**Prevention:** Never prefix backend secrets with `VITE_`. Use a naming convention audit in CI to catch this. Backend secrets go in Render's environment variables, not in the Vite build.

### Pitfall 10: HSTS Set in `vercel.json` When Vercel Already Provides It

**What goes wrong:** Developer adds `Strict-Transport-Security` header in `vercel.json`. Vercel already sets this header automatically. Duplicate headers can cause issues with some HTTP clients and proxies.

**Prevention:** Do not set HSTS in `vercel.json`. Vercel handles it automatically for all deployments. Verify the header is present using browser DevTools or curl.

## Minor Pitfalls

### Pitfall 11: Using Deprecated `Feature-Policy` Instead of `Permissions-Policy`

**What goes wrong:** Setting `Feature-Policy` header instead of `Permissions-Policy`. `Feature-Policy` is deprecated and replaced by `Permissions-Policy`.

**Prevention:** Use `Permissions-Policy` header. NetEscapades handles this correctly — its `AddPermissionsPolicy()` method generates the correct header.

### Pitfall 12: Not Setting `object-src 'none'` in CSP

**What goes wrong:** Not setting `object-src 'none'` allows `<object>`, `<embed>`, and `<applet>` elements. These can be used for XSS via Flash, Java, or other plugins.

**Prevention:** Always set `object-src 'none'` in CSP. NetEscapades' `AddDefaultSecurityHeaders()` includes this by default.

### Pitfall 13: Missing `base-uri 'self'` in CSP

**What goes wrong:** Not setting `base-uri` allows attackers to inject a `<base>` tag that changes the base URL for all relative URLs on the page. This can redirect form submissions and script loads to attacker-controlled servers.

**Prevention:** Set `base-uri 'self'` in CSP. NetEscapades' defaults include this.

## Phase-Specific Warnings

| Phase Topic | Likely Pitfall | Mitigation |
|-------------|---------------|------------|
| Security headers | X-XSS-Protection set to `1; mode=block` | Set to `0`. Follow OWASP 2025 recommendations. |
| CSP implementation | Deploying enforce mode without testing | Use `Content-Security-Policy-Report-Only` first. Monitor violations for 1-2 weeks. |
| Rate limiting | No rate limiting on auth endpoints | Apply sliding-window limiter to `/api/auth/*` endpoints. 5 requests per minute per IP. |
| CORS | `AllowAnyOrigin()` with credentials | Use explicit origin lists. Test preflight requests in dev. |
| Bundle security | Source maps in production | `sourcemap: false` in Vite config. CI check for `.map` files. |

## Sources

- [OWASP HTTP Headers Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/HTTP_Headers_Cheat_Sheet.html)
- [MDN CSP Guide](https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/CSP) — 2026-03-22
- [NetEscapades.AspNetCore.SecurityHeaders](https://www.nuget.org/packages/NetEscapades.AspNetCore.SecurityHeaders) — v1.3.1
- [Google Security Blog: X-XSS-Protection Deprecation](https://developer.chrome.com/blog/x-xss-protection-deprecation)
- [ASP.NET Core Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0) — 2026-07-22
