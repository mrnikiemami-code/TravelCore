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

**P35 — Production Payment Provider Readiness** (`TC-P35-T005` Cursor **PASS** · worksheet awaiting Architect/user answers)

## Completed (recent)

- P35-T004 ACCEPTED — Iran/UAE research shortlists
- P35-T005: Business provider selection worksheet (no vendor selected)

## Current Authorized Work

**None** — WAITING for Architect after T005 RESULT + user answers A–G.

## Revision

| Date | Change |
|------|--------|
| 2026-08-21 | Sync after TC-P35-T005 provider selection worksheet |
| 2026-08-21 | Sync after TC-P35-T004 Iran/UAE provider research |
| 2026-08-21 | Sync after TC-P35-T003 market provider decision matrix |
| 2026-08-21 | Sync after TC-P33-GATE review |
