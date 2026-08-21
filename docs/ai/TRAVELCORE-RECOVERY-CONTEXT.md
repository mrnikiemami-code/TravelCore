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

**P35 — Production Payment Provider Readiness** (`TC-P35-T003` Cursor **PASS** · **BLOCKED_PENDING_PROVIDER_RESEARCH**)

## Completed (recent)

- P35-T001 ACCEPTED — provider-agnostic readiness
- P35-T002 ACCEPTED — intake; business inputs later supplied
- P35-T003: Market split matrix (Iran ≠ UAE); Bank Mellat preference not treated as locked PSP

## Current Authorized Work

**None** — WAITING for Architect after T003 RESULT. Do not invent provider.

## Revision

| Date | Change |
|------|--------|
| 2026-08-21 | Sync after TC-P35-T003 market provider decision matrix |
| 2026-08-21 | Sync after TC-P35-T002 external decision intake |
| 2026-08-21 | Sync after TC-P35-T001 production provider readiness plan |
| 2026-08-21 | Sync after TC-P33-GATE review |
