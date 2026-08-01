# Roadmap: FleetOS Security Remediation

## Overview

Remediate all 24 security findings from the HexStrike AI penetration test and internal security audit. The work progresses from foundational headers (immediate protection, low risk) through CSP hardening (most complex, highest breakage risk), authentication hardening (brute-force prevention), and finally input validation plus data protection (locking down file uploads, CORS, and information exposure). Each phase builds on the previous — headers create the foundation CSP enforces, auth hardening assumes headers are in place, and input validation closes the remaining attack surface.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

- [x] **Phase 1: Security Headers** - Backend middleware + Vercel config for HSTS, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, Permissions-Policy
- [ ] **Phase 2: CSP Hardening** - Remove unsafe-inline/unsafe-eval, move inline scripts external, hash-based CSP with Report-Only → enforce pipeline
- [ ] **Phase 3: Authentication Hardening** - Rate limiting on auth endpoints, JWT expiry validation in route guards, generic error messages
- [ ] **Phase 4: Input Validation & Data Protection** - File upload validation (magic bytes), CORS lockdown, CNH masking, production bundle cleanup

## Phase Details

### Phase 1: Security Headers
**Goal**: All endpoints serve consistent security headers that protect against clickjacking, MIME sniffing, protocol downgrade, and information leakage — providing immediate defensive coverage with zero risk of breaking existing functionality.
**Depends on**: Nothing (first phase)
**Requirements**: HDR-01, HDR-02, HDR-03, HDR-04
**Complexity**: Low
**Success Criteria** (what must be TRUE):
  1. Every backend API response includes HSTS, X-Frame-Options, X-Content-Type-Options, Referrer-Policy, and Permissions-Policy headers — verifiable via `curl -I` against any API endpoint
  2. Frontend static assets served by Vercel return the same security header set via vercel.json configuration
  3. X-XSS-Protection header is set to `0` (disabled) on all endpoints — the deprecated `1; mode=block` value is nowhere in the response
  4. HSTS max-age starts at 300 seconds and is verifiable in response headers — low initial value allows safe validation before increasing
**Plans**: 1
**Plan list**:
- [x] 01-01-PLAN.md — Backend security headers middleware (NetEscapades) + frontend Vercel header configuration

### Phase 2: CSP Hardening
**Goal**: A strict Content Security Policy eliminates the XSS attack surface by removing unsafe-inline and unsafe-eval from script-src, using hash-based policies for the static Vite SPA — deployed safely through Report-Only mode before enforcement.
**Depends on**: Phase 1 (headers foundation must be in place for CSP to be effective)
**Requirements**: CSP-01, CSP-02, CSP-03, CSP-04, CSP-05
**Complexity**: High
**Success Criteria** (what must be TRUE):
  1. No `unsafe-inline` keyword exists in script-src — inline theme detection script moved to external file
  2. No `unsafe-eval` keyword exists in script-src — no dynamic code execution (eval, new Function) in production build
  3. CSP initially deployed in Report-Only mode with violation reports being collected — zero functional breakage during testing period
  4. Hash-based CSP covers all static Vite-built SPA assets without requiring per-request nonce generation
  5. After Report-Only validation, CSP enforced with zero violations in browser console across all application pages
**Plans**: TBD

### Phase 3: Authentication Hardening
**Goal**: Authentication endpoints are protected against brute-force attacks and credential stuffing via rate limiting, JWT tokens are validated for expiry at the route guard level, and login failure responses prevent account enumeration.
**Depends on**: Phase 1 (security headers in place; rate limiting responses need consistent header coverage)
**Requirements**: AUTH-01, AUTH-02, AUTH-03, AUTH-04, AUTH-05
**Complexity**: Medium
**Success Criteria** (what must be TRUE):
  1. Login endpoint returns HTTP 429 after 5 requests per minute from the same IP address — sliding window algorithm resets after the window passes
  2. Registration and password-reset endpoints have independent rate limits that prevent bulk account creation or reset spam
  3. Frontend route guards decode the JWT `exp` claim and redirect unauthenticated users to login when the token has expired — no stale-token access to protected pages
  4. Login failure responses return identical generic error messages regardless of whether the account exists or the password is wrong — prevents username enumeration
  5. All API endpoints reject requests without a valid JWT — `[Authorize]` middleware verified active on all controllers
**Plans**: TBD

### Phase 4: Input Validation & Data Protection
**Goal**: User inputs are validated at both frontend and backend boundaries, file uploads are verified by content signature (not just extension), cross-origin access is locked to explicit allowlists, sensitive personal data (CNH) is masked in the UI, and the production bundle leaks no debug information or secrets.
**Depends on**: Phase 3 (authentication must be solid before tightening input validation — rate-limited endpoints need stable auth flow first)
**Requirements**: VAL-01, VAL-02, VAL-03, VAL-04, VAL-05, VAL-06, DATA-01, DATA-02, DATA-03, DATA-04
**Complexity**: Medium
**Success Criteria** (what must be TRUE):
  1. File uploads validated on frontend for MIME type and file size before sending — oversized or wrong-type files rejected with user-facing error in Portuguese
  2. Backend verifies file signatures by reading magic bytes — a renamed .exe with .jpg extension is rejected
  3. CORS policy uses explicit origin allowlists (never `AllowAnyOrigin` with `AllowCredentials`) and restricts methods to GET, POST, PUT, DELETE, PATCH and headers to Content-Type, Authorization
  4. CNH numbers displayed masked in driver list views (format `***.******-**`) with a reveal action for authorized users
  5. Production Vite build has source maps disabled, console.log statements stripped, and no backend secrets exposed via `VITE_`-prefixed environment variables
**Plans**: TBD
**UI hint**: yes

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3 → 4

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Security Headers | 1/1 | Complete | 2026-08-01 |
| 2. CSP Hardening | 0/TBD | Not started | - |
| 3. Authentication Hardening | 0/TBD | Not started | - |
| 4. Input Validation & Data Protection | 0/TBD | Not started | - |
