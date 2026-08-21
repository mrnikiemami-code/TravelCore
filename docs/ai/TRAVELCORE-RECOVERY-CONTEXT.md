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

**P31 — Commercial Demo Experience** (`TC-P31-T003` · AWAITING_ARCHITECT_REVIEW)

## Completed

- **P00–P29** COMPLETE / ACCEPTED
- Post-P29 evolution tracks COMPLETE
- `TC-P30-GATE` **ACCEPTED WITH KNOWN LIMITATIONS** — P30 FOUNDATION ACCEPTED
- `TC-DEMOFEED-T002`…`T005` **DONE** · `TC-DEMOFEED-GATE` **ACCEPTED**
- `TC-P31-T001` **ACCEPTED** — plan `docs/plans/P31-commercial-demo-experience-plan.md`
- `TC-P31-T002` **ACCEPTED** — strategy `docs/plans/P31-demo-content-strategy.md`
- `TC-P31-T003` **Cursor PASS** — Home commercial upgrade + evidence `docs/product-experience/evidence/P31-T003/` (AWAITING_ARCHITECT_REVIEW)

## Current Important Locks

| Lock | Value |
|------|--------|
| Product order | **Experience → Data → Commercial** |
| P30 | **FOUNDATION ACCEPTED** |
| DEMOFEED | **GATE ACCEPTED** |
| P31 | **ACTIVE** (`T001`/`T002` ACCEPTED · `T003` review) |
| Feeder path | `tools/demofeed` — not an `ITravelCoreModule` |
| Demo identity | `demofeed-*` |
| Cursor PASS ≠ Architect ACCEPT | Mandatory |
| Pipeline Controller | File-Based Task Pipeline V3 |

## Current Authorized Work

**`TC-P31-T003`** Home upgrade complete (awaiting Architect ACCEPT/REWORK).

## Next Planned Work

Architect decision on T003.
Then **only** a new authorized `.task.md` / `.gate.md` — do not auto-implement T004+.

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
| 2026-08-20 | Initial · Controller Mode |
| 2026-08-20 | Sync after T002 / T003 |
| 2026-08-20 | Sync after `TC-P30-T004` Application Shells |
| 2026-08-20 | Sync after `TC-P30-T005` Public Home Experience |
| 2026-08-20 | Sync after `TC-P30-T005-VISUAL-CHECKPOINT-C` (REWORK_RECOMMENDED) |
| 2026-08-20 | Sync after `TC-P30-T005-REWORK` Public Home visual rework |
| 2026-08-21 | Sync after P30 GATE ACCEPT + DEMOFEED T002 activation |
| 2026-08-21 | Sync after DEMOFEED T003 Destination demo seed |
| 2026-08-21 | Sync after DEMOFEED T004 Place (Hotel) + Media cover seed |
| 2026-08-21 | Sync after DEMOFEED T005 Tour + Media cover seed |
| 2026-08-21 | Sync after DEMOFEED GATE Cursor review evidence |
| 2026-08-21 | Sync after P31-T001 commercial demo experience plan |
| 2026-08-21 | Sync after P31-T002 professional demo content strategy |
| 2026-08-21 | Sync after P31-T003 Home commercial upgrade |
