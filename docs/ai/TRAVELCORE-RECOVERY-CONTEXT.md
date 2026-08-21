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

**P32 — Commercial Demo Data & Media Enrichment** (`TC-P32-GATE` Cursor **PASS WITH KNOWN LIMITATIONS** · awaiting Architect ACCEPT)

## Completed (Cursor)

- `TC-P32-T001` … `TC-P32-T009` (T006 = BLOCKED finding; T007–T009 Destination Cover path)
- `TC-P32-GATE` review artifact: `docs/product-experience/evidence/P32-GATE/GATE-REVIEW.md`

## Current Authorized Work

**None** — WAITING for Architect after GATE RESULT. Do not invent next phase.

## Open Limitations (accepted in GATE review)

- Destination Gallery deferred (Option A Cover only)
- Hotel gallery pack density < UI slots
- DemoFeed → Api media blob sync is runtime env
- Pricing/Booking engines intentionally absent

## Revision

| Date | Change |
|------|--------|
| 2026-08-21 | Sync after TC-P32-GATE Cursor PASS WITH KNOWN LIMITATIONS |
| 2026-08-21 | Sync after TC-P32-T005 hotel presentation completeness |
