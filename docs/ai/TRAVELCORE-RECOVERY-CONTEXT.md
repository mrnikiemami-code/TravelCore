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

**P36 — Commercial UI/UX Final Polish** (`TC-P36-T004` Cursor **PASS** · Tours · **PARTIALLY_SELLABLE_VISUALLY**)

## Completed (recent)

- P36-T003 ACCEPTED — Hotels PARTIALLY_SELLABLE
- P36-T004: Tour listing/detail polish · friendly destination select · UUID leakage removed

## Current Authorized Work

**None** — WAITING for Architect after T004 RESULT. Do not auto-implement T005.

## Revision

| Date | Change |
|------|--------|
| 2026-08-21 | Sync after TC-P36-T004 Tour commercial polish |
| 2026-08-21 | Sync after TC-P36-T003 Hotel commercial polish |
| 2026-08-21 | Sync after TC-P36-T002 Home commercial redesign |
| 2026-08-21 | Sync after TC-P33-GATE review |
