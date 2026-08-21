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

**P33 — Commercial Product Readiness Review** (`TC-P33-T004` Cursor **PASS** · awaiting Architect ACCEPT)

## Completed (recent)

- P32 GATE ACCEPTED WITH KNOWN LIMITATIONS
- P33-T001 … T003 ACCEPTED (planning)
- P33-T004 roadmap: `docs/plans/P33-tour-commerce-implementation-roadmap.md`

## Current Authorized Work

**None** — WAITING for Architect after T004 RESULT. Do not invent I1+ implementation.

## Revision

| Date | Change |
|------|--------|
| 2026-08-21 | Sync after TC-P33-T004 implementation roadmap |
