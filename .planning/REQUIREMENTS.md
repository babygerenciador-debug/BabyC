# Requirements: FleetOS Security Remediation

**Defined:** 2026-08-01
**Core Value:** The application must be secure against vulnerabilities identified in the pentest and internal audit before any new features are built.

## v1 Requirements

### Security Headers

- [x] **HDR-01**: Backend implements security headers middleware (HSTS, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, Permissions-Policy) using NetEscapades.AspNetCore.SecurityHeaders
- [x] **HDR-02**: Frontend sets security headers via vercel.json configuration
- [x] **HDR-03**: X-XSS-Protection set to `0` (disabled) on all endpoints — the header is deprecated and introduces vulnerabilities
- [x] **HDR-04**: HSTS configured with incremental rollout (start 300s max-age, increase after validation)

### CSP Hardening

- [ ] **CSP-01**: Remove `unsafe-inline` from script-src in Content Security Policy
- [ ] **CSP-02**: Remove `unsafe-eval` from script-src in Content Security Policy
- [ ] **CSP-03**: Move inline theme detection script (index.html) to external file
- [ ] **CSP-04**: Deploy CSP in Report-Only mode first, collect violations, then enforce
- [ ] **CSP-05**: Implement hash-based CSP for static Vite-built SPA assets

### Authentication Hardening

- [ ] **AUTH-01**: Rate limiting on login endpoint (sliding window, 5 requests/minute by IP)
- [ ] **AUTH-02**: Rate limiting on registration/password-reset endpoints
- [ ] **AUTH-03**: JWT expiry validation in frontend route guards (decode `exp` claim)
- [ ] **AUTH-04**: Generic error messages on login failures (prevent account enumeration)
- [ ] **AUTH-05**: Backend validates JWT on every API request via `[Authorize]` middleware (already exists — verify)

### Input Validation

- [ ] **VAL-01**: Frontend validates file type (MIME) and size before upload
- [ ] **VAL-02**: Backend verifies file signature (magic bytes) — not just extension/MIME
- [ ] **VAL-03**: Backend enforces maximum file size limit
- [ ] **VAL-04**: Backend generates random filenames for stored files
- [ ] **VAL-05**: CORS configured with explicit origin allowlists (never `AllowAnyOrigin` with credentials)
- [ ] **VAL-06**: CORS restricted to specific methods (GET, POST, PUT, DELETE, PATCH) and headers (Content-Type, Authorization)

### Data Protection

- [ ] **DATA-01**: CNH numbers masked in driver list views (show `***.******-**` with reveal option)
- [ ] **DATA-02**: Source maps disabled in production Vite build
- [ ] **DATA-03**: Console.log statements removed from production frontend bundle
- [ ] **DATA-04**: `VITE_`-prefixed environment variables audited — no backend secrets with this prefix

## v2 Requirements

### Enterprise Security

- **ENT-01**: Multi-factor authentication (MFA) for admin accounts
- **ENT-02**: Audit logging for security events (login, failed login, password change, role changes)
- **ENT-03**: Row-Level Security (RLS) in PostgreSQL for tenant isolation
- **ENT-04**: Encryption at rest for sensitive database columns (salary, CNH)
- **ENT-05**: CSP violation reporting endpoint with dashboard
- **ENT-06**: Dependency vulnerability scanning in CI/CD pipeline

## Out of Scope

| Feature | Reason |
|---------|--------|
| New feature development | Security remediation is the sole priority |
| Test coverage improvements | Important but separate from security scope |
| Code quality refactoring (any types, handler boilerplate) | Deferred to post-security phase |
| Supabase migration | Not a security requirement |
| OAuth/social login | Email/password sufficient for current needs |
| Mobile app | Web-first |
| Cloudflare WAF setup | Recommended but not blocking — can be added post-remediation |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| HDR-01 | Phase 1 | Complete |
| HDR-02 | Phase 1 | Complete |
| HDR-03 | Phase 1 | Complete |
| HDR-04 | Phase 1 | Complete |
| CSP-01 | Phase 2 | Pending |
| CSP-02 | Phase 2 | Pending |
| CSP-03 | Phase 2 | Pending |
| CSP-04 | Phase 2 | Pending |
| CSP-05 | Phase 2 | Pending |
| AUTH-01 | Phase 3 | Pending |
| AUTH-02 | Phase 3 | Pending |
| AUTH-03 | Phase 3 | Pending |
| AUTH-04 | Phase 3 | Pending |
| AUTH-05 | Phase 3 | Pending |
| VAL-01 | Phase 4 | Pending |
| VAL-02 | Phase 4 | Pending |
| VAL-03 | Phase 4 | Pending |
| VAL-04 | Phase 4 | Pending |
| VAL-05 | Phase 4 | Pending |
| VAL-06 | Phase 4 | Pending |
| DATA-01 | Phase 4 | Pending |
| DATA-02 | Phase 4 | Pending |
| DATA-03 | Phase 4 | Pending |
| DATA-04 | Phase 4 | Pending |

**Coverage:**
- v1 requirements: 24 total
- Mapped to phases: 24
- Unmapped: 0 ✓

---
*Requirements defined: 2026-08-01*
*Last updated: 2026-08-01 after initial definition*