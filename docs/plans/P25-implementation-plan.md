# P25 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P25-PLAN` |
| Phase | P25 — Notification |
| Status | PLAN ACCEPTED · **P25 IN_PROGRESS / PLAN authored** · no product execution |
| Baseline | `origin/main` |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

This document is the architecture plan for the Notification phase.

## Scope (plan-only)

- Planned notification channels: Email · SMS · In-app
- Provider abstraction posture (no provider execution in plan)
- Explicit separation from Booking/Payment execution ownership

## Preserved locked architecture

1. Modular Monolith: schema-per-module boundaries; no peer-schema FK.
2. No shared DbContext across modules.
3. No distributed transactions.
4. Notification delivery must not become an execution owner for Payment/Booking.

## Out-of-scope (explicitly not executed in this task)

- Module code implementation
- Schema/migration implementation
- API / Frontend / Dashboard surfaces
- Tests (except documentation validation)

## Next phase

- After this plan is accepted, product tasks may be created to implement notification delivery contracts.
