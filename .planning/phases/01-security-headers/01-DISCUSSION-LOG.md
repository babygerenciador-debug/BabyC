# Phase 1: Security Headers - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-01
**Phase:** 01-security-headers
**Areas discussed:** Middleware ordering, HSTS rollout strategy, Vercel headers approach

---

## Middleware Ordering

| Option | Description | Selected |
|--------|-------------|----------|
| Before UseRouting() | Security headers applied to all responses, including auth failures | ✓ |
| After UseAuth() | Headers only applied after authentication succeeds | |
| After UseRouting() | Headers applied after routing decisions | |

**User's choice:** Before UseRouting() (FIRST in pipeline)
**Notes:** Research indicates this is the recommended approach — ensures headers are present even on error responses (401, 403, 500).

---

## HSTS Rollout Strategy

| Option | Description | Selected |
|--------|-------------|----------|
| Incremental (300s → 1 year) | Start conservative, increase after validation | ✓ |
| Aggressive (1 year from start) | Immediate long-term enforcement | |
| No HSTS | Skip HSTS entirely | |

**User's choice:** Incremental rollout
**Notes:** Start with 300s (5 minutes), increase to 1 day, 1 week, 1 month, then 1 year. Enable preload only after 3 months of validation.

---

## Vercel Headers Approach

| Option | Description | Selected |
|--------|-------------|----------|
| vercel.json headers | Static headers applied at CDN edge, zero latency | ✓ |
| Edge Middleware | Per-request header generation via Edge Function | |

**User's choice:** vercel.json headers
**Notes:** For a static SPA, vercel.json is simpler and more performant. Hash-based CSP (Phase 2) will handle script trust without needing per-request nonces.

---

## The Agent's Discretion

- X-XSS-Protection value: Set to `0` (disabled) based on research. User did not explicitly discuss this, but research clearly indicates this is the correct approach (header is deprecated, setting it to `1; mode=block` introduces vulnerabilities in older browsers).
- NetEscapades package version: Selected v1.3.1 based on research (latest stable, 1.4M+ downloads, maintained by Andrew Lock).
- Permissions-Policy directives: Chose reasonable defaults (camera=self, microphone=none, geolocation=self) based on fleet management app needs.

## Deferred Ideas

None — all scope is within Phase 1 boundaries. CSP hardening is explicitly deferred to Phase 2.
