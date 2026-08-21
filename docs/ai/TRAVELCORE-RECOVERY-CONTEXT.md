# TravelCore Recovery Context

| Field | Value |
|-------|--------|
| Document | `docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md` |
| Purpose | Fast durable position snapshot for new ChatGPT / Cursor sessions |
| Authority | Derived from repository SoT — update after gates / accepted tasks |
| Companion | [`../prompts/START-HERE-IF-CHATGPT-IS-LOST.md`](../prompts/START-HERE-IF-CHATGPT-IS-LOST.md) |
| Controller | [`TRAVELCORE-PIPELINE-CONTROLLER.md`](TRAVELCORE-PIPELINE-CONTROLLER.md) |

This file is a **fast recovery aid**. If it conflicts with `PROJECT-STATE.md` / accepted ADRs / Git evidence, those win — report `RECOVERY_CONFLICT` / `SOURCE_OF_TRUTH_CONFLICT`.

---

# Current Project Position

## Identity

| Field | Value |
|-------|--------|
| Project | TravelCore |
| Canonical repository | `mrnikami-code/TravelCore` |
| Architecture | Modular Monolith |
| Backend | .NET 10 / ASP.NET Core 10 Minimal API |
| Frontend | Next.js 16 / React 19 / TypeScript |

## Current Phase

**P34 — Payment & Confirmation Readiness** (`TC-P34-T005` Cursor **PASS** · public confirmed honesty awaiting Architect ACCEPT)

## Completed (recent)

- P33-GATE ACCEPTED WITH KNOWN LIMITATIONS
- P34-T001 ACCEPTED — readiness plan; Option B Architect-locked for next money-movement slice
- P34-T002: Sandbox provider design (docs only) — reuse `IPaymentProviderGateway`; keep `NamedProductionAdapterImplemented=false`
- P34-T003: Sandbox adapter + DI/eligibility gates implemented — `NamedProductionAdapterImplemented` still **false**
- P34-T004: Tour public booking/payment UX wired to sandbox; Option A when unavailable; ConfirmIfEligible observed after success outbox
- P34-T005: Public `confirmed` / `bookingConfirmed` now reflects BookingStatus (no hardcoded false)

## Current Authorized Work

**None** — WAITING for Architect after T005 RESULT. Do not invent further P34 tasks until authorized.

## Revision

| Date | Change |
|------|--------|
| 2026-08-21 | Sync after TC-P34-T005 public confirmed honesty |
| 2026-08-21 | Sync after TC-P34-T004 Tour sandbox UX |
| 2026-08-21 | Sync after TC-P34-T003 sandbox adapter implementation |
| 2026-08-21 | Sync after TC-P34-T002 sandbox design |
| 2026-08-21 | Sync after TC-P34-T001 readiness plan |
| 2026-08-21 | Sync after TC-P33-GATE review |
