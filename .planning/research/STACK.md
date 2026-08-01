# Technology Stack — Security Hardening

**Project:** BabyC (React 19 + Vite frontend on Vercel / ASP.NET Core 10 backend on Render.com with PostgreSQL 16)
**Researched:** 2026-08-01
**Overall Confidence:** HIGH

---

## Recommended Stack

### Core Security Libraries

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| **NetEscapades.AspNetCore.SecurityHeaders** | 1.3.1 | ASP.NET Core middleware for security headers | De-facto standard. Maintained by Andrew Lock. Fluent API for CSP, HSTS, Permissions-Policy, COOP/COEP/CORP. 1.4M+ downloads. Supports per-endpoint policies and nonce-based CSP via TagHelpers. |
| **NetEscapades.AspNetCore.SecurityHeaders.TagHelpers** | 1.3.1 | Nonce & hash injection for `<script>` / `<style>` tags | Pairs with the main package. Generates per-request nonces for Razor views and auto-hashes inline content. Eliminates `unsafe-inline` safely. |
| **vite-plugin-csp-guard** | ^2.1.0 | Vite plugin for CSP nonce/hash injection during build | The only actively maintained Vite CSP plugin. Auto-generates nonces for `<script>` and `<link>` tags in `index.html`. Supports `dev` mode with HMR-compatible nonces. Avoids the broken `vite-plugin-csp` (unmaintained, last publish 2022). |
| **OWASP Secure Headers (reference)** | N/A | Header value reference | Not a library — use [OWASP Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/REST_Security_Cheat_Sheet.html#security-headers) as the canonical source for header values. |

### Frontend (Vercel-Side)

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| **Vercel `headers` in `vercel.json`** | Build Output API v3 | Static security headers on all responses | Vercel's native way to set headers. Applied at the CDN edge — zero latency. Does NOT support per-request nonce generation, so use for all headers except CSP `script-src`/`style-src`. |
| **React 19** | ^19.0 | UI framework | React 19 does not inject inline scripts by default. Compatible with strict nonce-based CSP out of the box. |
| **Vite 6** | ^6.0 | Build tool | Vite 6 produces clean ES module bundles. No `eval()` in production output. Source maps disabled by default in production (`build.sourcemap: false`). |

### Backend (Render.com-Side)

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| **ASP.NET Core 10** | .NET 10 (LTS) | Web API framework | Built-in rate limiting middleware (`Microsoft.AspNetCore.RateLimiting`). Built-in CORS middleware. Built-in HSTS via `UseHsts()`. No external dependencies for core security. |
| **PostgreSQL 16** | 16.x | Managed database on Render | Render's managed PostgreSQL. Connection strings provided via `DATABASE_URL` env var. SSL enforced by default. |

---

## 1. Content Security Policy (CSP) — React SPA on Vercel

### Strategy: Nonce-based strict CSP

**Do NOT use `unsafe-inline` or `unsafe-eval`.** Both defeat the primary purpose of CSP (XSS prevention). React 19 + Vite production builds do not require either.

### Why nonce over hash for a SPA

- React SPAs are served from a single `index.html`. The server (Vercel) cannot compute hashes at the edge for static files — hashes are fixed per-file but Vercel's static hosting doesn't inject CSP headers per-request.
- **Nonce requires per-request injection** — Vercel's edge functions or a middleware pattern must generate a unique nonce per HTML response.
- **Alternative: `strict-dynamic` + hash** — Hash the entry `<script>` tag in `index.html` at build time. The `strict-dynamic` keyword propagates trust to all scripts loaded by that entry script. This works well with Vite's static output because the hash is stable per build.

### Recommended approach: Hash-based CSP for Vercel static hosting

For a Vite SPA deployed as static files on Vercel, **use hash-based CSP with `strict-dynamic`**:

```
Content-Security-Policy:
  default-src 'self';
  script-src 'sha256-<HASH_OF_ENTRY_SCRIPT>' 'strict-dynamic';
  style-src 'self' 'unsafe-inline';
  object-src 'none';
  base-uri 'self';
  frame-ancestors 'none';
  upgrade-insecure-requests;
```

**Why `style-src` keeps `'unsafe-inline'`:** React's CSS-in-JS (if used) and inline style attributes require `'unsafe-inline'` for styles. This is acceptable — inline styles are a lower-risk XSS vector than inline scripts. CSS injection attacks require a separate exploitation chain.

**Why NOT nonce on Vercel:** Vercel serves static files from its CDN. Generating a per-request nonce requires an Edge Function to rewrite the HTML — this adds latency and complexity. Hash-based is simpler and equally secure for static SPAs.

### If you must use nonce (e.g., server-rendered HTML)

Use `vite-plugin-csp-guard` (v2.1+):

```ts
// vite.config.ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import csp from 'vite-plugin-csp-guard';

export default defineConfig({
  plugins: [
    react(),
    csp({
      policy: {
        'script-src': ["'self'", "'nonce-{nonce}'"],
        'style-src': ["'self'", "'unsafe-inline'"],
      },
    }),
  ],
});
```

This injects a `<meta http-equiv="Content-Security-Policy">` tag into `index.html` with a per-build nonce. For per-request nonces, you need a Vercel Edge Middleware.

### Remove `unsafe-eval`

React 19 production builds do **not** use `eval()`. Vite's production build output is pure ES modules. **Safe to remove `'unsafe-eval'` unconditionally.** If a third-party library requires `eval()`, replace the library — do not weaken CSP.

### Libraries that break strict CSP

| Library | Problem | Solution |
|---------|---------|----------|
| `styled-components` | Runtime CSS-in-JS uses `<style>` injection | Switch to CSS Modules, Tailwind, or static CSS. Or keep `'unsafe-inline'` in `style-src` only (acceptable). |
| `react-helmet` | Injects `<script>` / `<meta>` tags dynamically | Works with `strict-dynamic` — the entry script creates the DOM elements. |
| `new Function()` in analytics | Uses eval-like APIs | Replace with `gtag.js` (supports strict CSP) or self-host analytics scripts. |

---

## 2. Security Headers Configuration

### Vercel (`vercel.json`)

Add headers in `vercel.json`. These apply to all responses at the CDN edge:

```json
{
  "headers": [
    {
      "source": "/(.*)",
      "headers": [
        {
          "key": "X-Content-Type-Options",
          "value": "nosniff"
        },
        {
          "key": "X-Frame-Options",
          "value": "DENY"
        },
        {
          "key": "Referrer-Policy",
          "value": "strict-origin-when-cross-origin"
        },
        {
          "key": "Permissions-Policy",
          "value": "accelerometer=(), autoplay=(), camera=(), display-capture=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), payment=(), picture-in-picture=(), publickey-credentials-get=(), screen-wake-lock=(), sync-xhr=(), usb=(), web-share=(), xr-spatial-tracking=()"
        },
        {
          "key": "Cross-Origin-Opener-Policy",
          "value": "same-origin"
        },
        {
          "key": "Cross-Origin-Embedder-Policy",
          "value": "credentialless"
        },
        {
          "key": "Cross-Origin-Resource-Policy",
          "value": "same-site"
        },
        {
          "key": "X-XSS-Protection",
          "value": "0"
        }
      ]
    }
  ]
}
```

**Critical notes:**
- **`X-XSS-Protection: 0`** — Do NOT set `1; mode=block`. This header is deprecated and can introduce XSS vulnerabilities in older browsers. Chrome removed it in 78+. Setting `0` disables the auditor entirely.
- **HSTS is NOT set in `vercel.json`** — Vercel automatically applies `Strict-Transport-Security` on all deployments via its CDN. Do not duplicate it.
- **CSP is NOT set in `vercel.json`** — CSP requires per-request nonce or per-build hash. Set it via `<meta>` tag in `index.html` or via Edge Middleware.
- **`Permissions-Policy` uses the new syntax** — Empty parentheses `()` mean "blocked for all origins." The old syntax `none` is deprecated.

### ASP.NET Core (via NetEscapades)

```csharp
// Program.cs
builder.Services.AddSecurityHeaderPolicies()
    .SetDefaultPolicy(policy =>
    {
        policy.AddDefaultSecurityHeaders();
        policy.AddContentSecurityPolicy(builder =>
        {
            builder.AddUpgradeInsecureRequests();
            builder.AddDefaultSrc().Self();
            builder.AddScriptSrc().Self().WithNonce();
            builder.AddStyleSrc().Self().UnsafeInline(); // CSS-in-JS or inline styles
            builder.AddObjectSrc().None();
            builder.AddFrameAncestors().None();
            builder.AddBaseUri().Self();
        });
        policy.AddPermissionsPolicy(perms =>
        {
            perms.AddDefaultSecureDirectives();
            // Re-enable only what you need:
            perms.AddFullscreen().Self();
        });
        policy.AddCrossOriginOpenerPolicy(x => x.SameOrigin());
        policy.AddCrossOriginEmbedderPolicy(x => x.Credentialless());
        policy.AddCrossOriginResourcePolicy(x => x.SameSite());
        policy.RemoveServerHeader();
    });

var app = builder.Build();

// MUST be first in the pipeline
app.UseSecurityHeaders();

// Then the rest:
app.UseExceptionHandler("/Error");
app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
```

**Pipeline order matters.** `UseSecurityHeaders()` must be **first** so it wraps every response. If placed after `UseStaticFiles()`, static file responses won't get headers.

---

## 3. Render.com Security Considerations

### What Render handles automatically

- **HTTPS everywhere** — Automatic TLS via Let's Encrypt. All `*.onrender.com` domains get free SSL.
- **HSTS** — Render's reverse proxy adds `Strict-Transport-Security` automatically.
- **DDoS protection** — Cloudflare-based protection at the network edge.

### What YOU must configure

- **Security headers** — Render does NOT inject security headers on your behalf. Your ASP.NET Core middleware must set them.
- **CORS** — Must be configured in your ASP.NET Core app (see Section 7).
- **Rate limiting** — Must be configured in your ASP.NET Core app (see Section 6).
- **Database SSL** — Render PostgreSQL enforces SSL. Use `?sslmode=require` in your connection string.
- **Environment variables** — Set secrets via Render Dashboard → Environment. Never commit `.env` files.

### Render-specific concerns

| Concern | Risk | Mitigation |
|---------|------|------------|
| **Cold starts** | Render free/cheap tiers sleep after inactivity. First request after wake is slow. | Use at minimum the $7/mo Starter plan for production. Enable "Always On" if available. |
| **Ephemeral filesystem** | Render's filesystem is ephemeral between deploys. | Never write files to disk expecting persistence. Use PostgreSQL or object storage. |
| **No built-in WAF** | Render doesn't offer a WAF (unlike Vercel/AWS). | Use Cloudflare in front of Render for WAF + rate limiting at the edge. |
| **Instance IP changes** | Render instances can get new IPs on redeploy. | Don't whitelist specific IPs. Use API keys and CORS instead. |

### Recommended: Cloudflare in front of Render

For production, put Cloudflare between users and Render:
- **WAF rules** — Block SQL injection, XSS patterns at the edge.
- **Rate limiting** — Cloudflare's rate limiting ($200/mo add-on) protects before requests hit your server.
- **Bot management** — Block known bad bots.
- **Caching** — Cache static assets, reduce Render load.

---

## 4. Rate Limiting for Authentication Endpoints

### Strategy: Two-layer rate limiting

**Layer 1 — ASP.NET Core built-in rate limiter** (primary, most granular)
**Layer 2 — Cloudflare or Vercel Edge** (edge protection, defense-in-depth)

### ASP.NET Core Rate Limiting

```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    // Authentication-specific policy
    options.AddPolicy("auth", httpContext =>
    {
        // Partition by IP to prevent brute-force
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetSlidingWindowLimiter(ip, _ =>
            new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 5,           // 5 attempts
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,     // 10-second segments
                QueueLimit = 0,            // No queuing — reject immediately
            });
    });

    // General API policy
    options.AddPolicy("api", httpContext =>
    {
        var userId = httpContext.User.Identity?.Name ?? "anonymous";
        return RateLimitPartition.GetTokenBucketLimiter(userId, _ =>
            new TokenBucketRateLimiterOptions
            {
                TokenLimit = 100,
                TokensPerPeriod = 10,
                ReplenishmentPeriod = TimeSpan.FromSeconds(5),
                QueueLimit = 0,
            });
    });

    // Global fallback
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
    {
        var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1),
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        var lease = context.Lease;
        if (lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();
        }
        await Results.Problem(
            title: "Too Many Requests",
            statusCode: 429,
            detail: "Rate limit exceeded. Please try again later."
        ).ExecuteAsync(context.HttpContext, token);
    };
});

var app = builder.Build();
app.UseRouting();
app.UseRateLimiter(); // After UseRouting, before endpoint mapping
```

### Apply to auth endpoints

```csharp
// Minimal API
app.MapPost("/api/auth/login", LoginHandler)
   .RequireRateLimiting("auth");

app.MapPost("/api/auth/register", RegisterHandler)
   .RequireRateLimiting("auth");

app.MapPost("/api/auth/forgot-password", ForgotPasswordHandler)
   .RequireRateLimiting("auth");

// Controller-based
[HttpPost("login")]
[EnableRateLimiting("auth")]
public async Task<IActionResult> Login([FromBody] LoginDto dto) { ... }
```

### Algorithm choices

| Algorithm | Use For | Why |
|-----------|---------|-----|
| **Sliding Window** | Auth endpoints (login, register) | Smooth rate over time. Prevents burst attacks at window boundaries. |
| **Token Bucket** | General API endpoints | Allows bursts up to bucket size, then throttles. Good UX for legitimate users. |
| **Fixed Window** | Global per-IP fallback | Simple. Prevents one IP from consuming all server resources. |
| **Concurrency** | Long-running operations (file uploads) | Limits parallel requests, not total count. |

### What NOT to do

- **Do NOT use Redis-backed rate limiting** for a single Render instance — adds latency and complexity. Use in-memory rate limiting unless you have multiple backend instances behind a load balancer.
- **Do NOT rate limit by `X-Forwarded-For`** without validating it — spoofable. Use `httpContext.Connection.RemoteIpAddress` which Render's proxy populates correctly.
- **Do NOT return `200 OK` with a "slow down" message** — Always return `429 Too Many Requests`. Clients and proxies rely on the status code.

---

## 5. CORS Hardening

### ASP.NET Core CORS Configuration

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "https://your-app.vercel.app",
                "https://www.yourdomain.com"
            )
            .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
            .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
            .WithExposedHeaders("Retry-After", "X-Request-Id")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromHours(1)); // Cache preflight for 1h
    });

    // Stricter policy for sensitive endpoints
    options.AddPolicy("StrictApi", policy =>
    {
        policy.WithOrigins("https://www.yourdomain.com")
            .WithMethods("GET", "POST")
            .WithHeaders("Content-Type", "Authorization")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(5));
    });
});

var app = builder.Build();
app.UseCors("AllowFrontend"); // After UseRouting, before UseAuthorization
```

### CORS rules

| Rule | Why |
|------|-----|
| **Never use `AllowAnyOrigin()` + `AllowCredentials()`** | Browsers will reject this combination. It's a security footgun. |
| **Never use `*` for `Access-Control-Allow-Origin` with credentials** | Same as above. Always specify explicit origins. |
| **List exact origins, not wildcards** | `*.vercel.app` is tempting but allows any Vercel preview deployment. List each preview URL explicitly or use a staging subdomain. |
| **Restrict methods** | Don't allow `OPTIONS`, `HEAD`, `TRACE` if you don't need them. |
| **Restrict headers** | Only expose headers the frontend actually reads. `Retry-After` for rate limiting, `X-Request-Id` for debugging. |
| **Set `preflightMaxAge`** | Reduces OPTIONS requests. 1 hour is a good default. Browsers cache for 2 hours max. |

### Vercel Edge CORS (if backend is not on the same domain)

If you need CORS at the Vercel edge (e.g., proxying to Render):

```ts
// api/proxy/[...path].ts
export default function handler(req: NextRequest) {
  const origin = req.headers.get('origin');
  const allowedOrigins = ['https://www.yourdomain.com'];

  if (!allowedOrigins.includes(origin ?? '')) {
    return new Response('Forbidden', { status: 403 });
  }

  // ... proxy logic
  return new Response(body, {
    headers: {
      'Access-Control-Allow-Origin': origin,
      'Access-Control-Allow-Methods': 'GET, POST',
      'Access-Control-Allow-Credentials': 'true',
    },
  });
}
```

---

## 6. JS Bundle — Minimize Sensitive Information Exposure

### Vite Production Build

```ts
// vite.config.ts
export default defineConfig({
  build: {
    sourcemap: false,          // NEVER ship source maps to production
    minify: 'terser',          // Better minification than esbuild for security
    terserOptions: {
      compress: {
        drop_console: true,    // Remove console.* calls
        drop_debugger: true,   // Remove debugger statements
      },
      mangle: {
        properties: true,      // Shorten property names
      },
    },
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ['react', 'react-dom'],
        },
      },
    },
  },
});
```

### What to scrub from bundles

| Item | Risk | Solution |
|------|------|----------|
| **Source maps** | Full source code exposure | `sourcemap: false` (default in Vite production). Never set `sourcemap: true` in production. If needed for error tracking, upload to Sentry/LogRocket **privately** — do not serve them. |
| **`console.log` statements** | Information leakage | `drop_console: true` in Terser. Audit codebase for sensitive data in logs. |
| **API keys / secrets** | Credential theft | Use environment variables. Vite injects `import.meta.env.VITE_*` at build time. **Never put backend secrets in `VITE_*` vars.** |
| **Debug endpoints** | Attack surface | Use tree-shaking + conditional compilation. Remove dev-only code paths. |
| **Error messages** | Stack trace exposure | Wrap API calls in try/catch. Return generic error messages to users. Log details server-side only. |
| **Unused exports** | Code surface area | Vite's tree-shaking removes unused code. Audit `import` statements. Use `sideEffects: false` in `package.json` for library code. |

### Environment variable security in Vite

```bash
# .env (committed)
VITE_API_URL=https://api.yourdomain.com

# .env.local (NOT committed)
VITE_ANALYTICS_KEY=abc123

# NEVER do this:
# VITE_DATABASE_PASSWORD=secret  ← This is BUILT INTO THE BUNDLE
```

**Rule:** Only `VITE_`-prefixed variables are injected into the client bundle. Backend secrets go in Render's environment variables and are never prefixed with `VITE_`.

### Additional bundle protections

```ts
// vite.config.ts — prevent accidental secret leakage
import { defineConfig, loadEnv } from 'vite';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');

  // Verify no backend secrets are in VITE_ namespace
  const sensitive = ['DATABASE_URL', 'JWT_SECRET', 'API_KEY'];
  for (const key of sensitive) {
    if (key.startsWith('VITE_')) {
      throw new Error(`SECURITY: ${key} must not be prefixed with VITE_`);
    }
  }

  return { /* ... */ };
});
```

---

## Alternatives Considered

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| Security headers middleware | NetEscapades.AspNetCore.SecurityHeaders | Custom middleware | NetEscapades is battle-tested, handles edge cases (HSTS on HTTPS only, COOP/COEP/CORP). Custom middleware is error-prone. |
| CSP for Vite SPA | Hash-based with `strict-dynamic` | Nonce-based with Edge Middleware | Hash-based is simpler for static hosting. Nonce requires an Edge Function per request — adds latency and cost. |
| Rate limiting | ASP.NET Core built-in | AspNetCoreRateLimit (NuGet) | Built-in is maintained by Microsoft, supports partitioned limiters, and integrates with the middleware pipeline. The older `AspNetCoreRateLimit` package is effectively deprecated. |
| Vite CSP plugin | vite-plugin-csp-guard | vite-plugin-csp | `vite-plugin-csp` is unmaintained (last publish 2022). `vite-plugin-csp-guard` is actively maintained and supports Vite 5/6. |
| CORS | ASP.NET Core built-in `AddCors` | Custom CORS middleware | Built-in handles preflight caching, credentials, and per-policy configuration. Custom is unnecessary. |
| Edge rate limiting | Cloudflare | Vercel Edge Config | Vercel's rate limiting is limited to 1 req/sec on free tier. Cloudflare offers granular rate limiting with bot detection. |

---

## Installation

### Backend (ASP.NET Core)

```bash
dotnet add package NetEscapades.AspNetCore.SecurityHeaders --version 1.3.1
dotnet add package NetEscapades.AspNetCore.SecurityHeaders.TagHelpers --version 1.3.1
```

Rate limiting and CORS are built into ASP.NET Core 10 — no additional packages.

### Frontend (Vite)

```bash
npm install -D vite-plugin-csp-guard@^2.1.0 terser@^5.31.0
```

---

## Sources

- [MDN Content Security Policy Guide](https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/CSP) — Updated 2026-03-22
- [Vercel CDN Security Headers](https://vercel.com/docs/cdn-security/security-headers) — Updated 2026-03-05
- [NetEscapades.AspNetCore.SecurityHeaders on NuGet](https://www.nuget.org/packages/NetEscapades.AspNetCore.SecurityHeaders) — v1.3.1, updated 2025-12-20
- [ASP.NET Core Rate Limiting Middleware](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0) — Updated 2026-07-22
- [ASP.NET Core Middleware Pipeline](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-10.0) — Updated 2026-06-09
- [OWASP REST Security Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/REST_Security_Cheat_Sheet.html#security-headers)
- [Render Environment Variables](https://docs.render.com/environment-variables)
- [OWASP Content Security Policy Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Content_Security_Policy_Cheat_Sheet.html)
