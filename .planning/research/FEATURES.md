# Feature Landscape — Security Hardening

**Domain:** Web application security headers, CSP, rate limiting, CORS
**Researched:** 2026-08-01

## Table Stakes

Features every production web app needs. Missing = security scanner fails.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| HTTPS enforcement (HSTS) | Basic transport security | Low | Vercel + Render handle this automatically. Verify `Strict-Transport-Security` header is present. |
| X-Content-Type-Options: nosniff | Prevent MIME-type sniffing | Low | One-line header. Set in vercel.json and ASP.NET Core middleware. |
| X-Frame-Options: DENY | Prevent clickjacking | Low | Set in vercel.json and ASP.NET Core. Use `frame-ancestors 'none'` in CSP as the modern equivalent. |
| Referrer-Policy | Control referrer leakage | Low | `strict-origin-when-cross-origin` is the standard value. |
| CORS configuration | Cross-origin access control | Medium | Must be explicit. Never use `AllowAnyOrigin()` with credentials. |
| Source maps disabled in production | Prevent source code exposure | Low | `build.sourcemap: false` in Vite config. Verify no `.map` files in deployment. |
| No secrets in client bundle | Prevent credential theft | Medium | Audit all `VITE_` env vars. Never put backend secrets with `VITE_` prefix. |

## Differentiators

Features that set security posture apart. Not expected by default, but valued.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Strict CSP (no unsafe-inline/unsafe-eval) | XSS defense-in-depth | High | Hash-based with strict-dynamic for static SPA. Requires testing all third-party scripts. |
| Rate limiting on auth endpoints | Brute-force prevention | Medium | ASP.NET Core built-in sliding window limiter. Partition by IP. |
| Permissions-Policy header | Feature access control | Low | Block camera, microphone, geolocation, payment APIs. Reduces attack surface. |
| COOP/COEP/CORP headers | Cross-origin isolation | Low | Enables SharedArrayBuffer, prevents Spectre-class attacks. NetEscapades includes in defaults. |
| Cloudflare WAF in front of Render | Edge-level attack blocking | Medium | Requires Cloudflare subscription. Adds SQL injection and XSS pattern blocking. |
| CSP violation reporting | Detect XSS attempts | Medium | Use Reporting API with `report-to` directive. Send violations to backend endpoint or third-party service. |
| Per-endpoint security policies | Granular header control | Medium | NetEscapades supports named policies per endpoint. API-only endpoints get tighter CSP. |

## Anti-Features

Features to explicitly NOT build.

| Anti-Feature | Why Avoid | What to Do Instead |
|--------------|-----------|-------------------|
| X-XSS-Protection: 1; mode=block | Deprecated header. Removed from Chrome 78+. Can introduce XSS in older browsers. | Set `X-XSS-Protection: 0` to explicitly disable the auditor. Rely on CSP for XSS protection. |
| `unsafe-inline` in script-src | Defeats CSP's primary purpose (XSS prevention) | Use hash-based or nonce-based CSP. For inline styles, keep `unsafe-inline` in `style-src` only (acceptable risk). |
| `unsafe-eval` in script-src | Allows arbitrary code execution. React 19 + Vite don't need it. | Remove unconditionally. If a library requires it, replace the library. |
| `Access-Control-Allow-Origin: *` with credentials | Browsers reject this combination. Security footgun. | Use explicit origin lists: `WithOrigins("https://app.vercel.app")`. |
| Wildcard CORS origins | Allows any site to call your API | List exact origins. For preview deployments, use a staging subdomain. |
| Source maps in production | Exposes full source code to attackers | `sourcemap: false` in Vite. Upload to Sentry privately for error tracking. |
| Console.log in production bundles | Information leakage | `drop_console: true` in Terser config. Audit codebase for sensitive data in logs. |
| Redis-backed rate limiting (single instance) | Adds latency and complexity for no benefit | Use in-memory rate limiting unless running multiple backend instances. |

## Feature Dependencies

```
HTTPS (HSTS) → All other headers (headers only meaningful over HTTPS)
CSP → Third-party script audit (must test all scripts for CSP compatibility)
Rate limiting → Routing middleware (must be after UseRouting in pipeline)
CORS → Authentication (CORS headers must be set before auth checks)
```

## MVP Recommendation

Prioritize:
1. **Security headers (HSTS, X-Content-Type-Options, X-Frame-Options, Referrer-Policy)** — Trivial to implement, immediate protection
2. **CORS hardening** — Critical for API security, medium complexity
3. **Rate limiting on auth endpoints** — Prevents brute-force attacks
4. **Source maps disabled + console removal** — Build-time security, low complexity

Defer: **Strict CSP** — Most complex change. Requires testing all third-party scripts. Implement after core headers are in place.
Defer: **Cloudflare WAF** — Requires subscription. Add after validating core security controls work.

## Sources

- [MDN CSP Guide](https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/CSP) — 2026-03-22
- [OWASP REST Security Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/REST_Security_Cheat_Sheet.html)
- [Vercel Security Headers](https://vercel.com/docs/cdn-security/security-headers) — 2026-03-05
