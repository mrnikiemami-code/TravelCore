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

**P31 — Commercial Demo Experience** (`TC-P31-GATE` **ACCEPTED WITH KNOWN LIMITATIONS**)

## Completed

- **P00–P29** COMPLETE / ACCEPTED
- Post-P29 evolution tracks COMPLETE
- `TC-P30-GATE` **ACCEPTED WITH KNOWN LIMITATIONS** — P30 FOUNDATION ACCEPTED
- `TC-DEMOFEED-T002`…`T005` **DONE** · `TC-DEMOFEED-GATE` **ACCEPTED**
- `TC-P31-T001` **ACCEPTED** — plan
- `TC-P31-T002` **ACCEPTED** — content strategy
- `TC-P31-T003`…`T005` **ACCEPTED WITH KNOWN LIMITATIONS** — Home / Hotel / Tour commercial polish
- `TC-P31-GATE` **ACCEPTED WITH KNOWN LIMITATIONS** — `docs/product-experience/evidence/P31-GATE/GATE-REVIEW.md`

## Current Important Locks

| Lock | Value |
|------|--------|
| Product order | **Experience → Data → Commercial** |
| P30 | **FOUNDATION ACCEPTED** |
| DEMOFEED | **GATE ACCEPTED** |
| P31 | **GATE ACCEPTED WITH KNOWN LIMITATIONS** |
| Feeder path | `tools/demofeed` — not an `ITravelCoreModule` |
| Demo identity | `demofeed-*` |
| Cursor PASS ≠ Architect ACCEPT | Mandatory |
| Pipeline Controller | File-Based Task Pipeline V3 |

## Current Authorized Work

**None** — waiting for next Architect `.task.md` / `.gate.md`.

## Next Planned Work

Architect discussed **P32 — Commercial Demo Data & Media Enrichment** but has **not** attached an authorized file yet.
Do **not** invent / auto-start `TC-P32-T001`.

## Open Blockers

**None**

## Rules

- No product execution without authorized `.task.md` / `.gate.md`
- Never invent tasks from ROADMAP / deferred items
- Never switch architect channel mid-pipeline
- Never exit PIPELINE mode while USER keeps it active — RESULT → WAITING MODE
- Major UI: visual evidence vs North Star mandatory

## Revision

| Date | Change |
|------|--------|
| 2026-08-21 | Sync after P31-T005 Tour commerce polish |
| 2026-08-21 | Sync after P31-GATE Cursor review evidence |
| 2026-08-21 | Sync after Architect ACCEPT of TC-P31-GATE |
