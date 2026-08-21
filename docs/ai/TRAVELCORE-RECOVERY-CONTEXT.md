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

**P33 — Commercial Product Readiness Review** (`TC-P33-T007` Cursor **PASS** · I3 Public Booking Initiation UX · awaiting Architect ACCEPT)

## Completed (recent)

- P33-T005 I1 ACCEPTED (`enrich-commerce`)
- P33-T006 I2 ACCEPTED — Public Tour Commerce Composition
- P33-T007 I3: Tour Detail CTA → `/tours/{slug}/book` → `POST /api/booking/public/initiations` → Pending + access token (no Payment)

## Current Authorized Work

**None** — WAITING for Architect after T007 RESULT. Do not invent Payment / I4+.

## Revision

| Date | Change |
|------|--------|
| 2026-08-21 | Sync after TC-P33-T007 Public Booking Initiation UX |
| 2026-08-21 | Sync after TC-P33-T006 Public Tour Commerce Composition |
