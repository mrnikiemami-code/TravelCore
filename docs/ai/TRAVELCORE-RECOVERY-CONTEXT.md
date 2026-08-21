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

**Post-P30 — DEMOFEED Data Enablement** (P30 FOUNDATION ACCEPTED)

## Completed

- **P00–P29** COMPLETE / ACCEPTED
- Post-P29 evolution tracks COMPLETE
- `TC-P30-PLAN` **ACCEPTED**
- `TC-P30-T002`…`T009` Architect **ACCEPTED** (known limitations where noted)
- `TC-P30-GATE` **ACCEPTED WITH KNOWN LIMITATIONS** — P30 FOUNDATION ACCEPTED (`7b34e33`)
- `TC-DEMOFEED-PLAN` authored · activated by `TC-DEMOFEED-ACTIVATE-001`
- `TC-DEMOFEED-T002` **DONE** — removable feeder host/boundary at `tools/demofeed`
- `TC-DEMOFEED-T003` **DONE** — Destination demo seed (`demofeed-*` via DestinationApplicationService)
- `TC-DEMOFEED-T004` **DONE** — Place (Hotel) + Media cover seed (`demofeed-hotel-*` via IPlaceService + IMediaUploadService)
- `TC-DEMOFEED-T005` **DONE** — TourProduct Package + Media cover seed (`demofeed-tour-*` via ITourProductService + Media)
- `TC-DEMOFEED-GATE` **Cursor PASS** — evidence `docs/plans/DEMOFEED-GATE-acceptance-evidence.md` (AWAITING_ARCHITECT_REVIEW)

## Current Important Locks

| Lock | Value |
|------|--------|
| Product order | **Experience → Data → Commercial** |
| P30 | **FOUNDATION ACCEPTED** |
| DEMOFEED | **GATE Cursor PASS** (`T002`–`T005` · Architect ACCEPT pending) |
| Feeder path | `tools/demofeed` — not an `ITravelCoreModule` · not in Api composition |
| Demo identity | code/slug prefix `demofeed-` |
| One Design System / Three Experiences | Public · Admin · Agency |
| North Star | `docs/product-experience/assets/travelcore-ui-ux-north-star.png` |
| Constitution | `docs/product-experience/TRAVELCORE-PRODUCT-EXPERIENCE-CONSTITUTION.md` |
| Design System 2.0 | `docs/product-experience/DESIGN-SYSTEM-2.0.md` |
| Cursor PASS ≠ Architect ACCEPT | Mandatory |
| Pipeline Controller | File-Based Task Pipeline V3 |

## Runtime Roles

| Role | Actor |
|------|--------|
| Architect | ChatGPT |
| Implementation Agent | Cursor |
| Source of Truth | Repository recovery / SoT documents |
| Architect channel | `https://chatgpt.com/g/g-p-6a79dbc6468c8191a5e74afa2d82a8be-travelcore/c/6a8039a8-2014-83ed-be9f-813280b23bcb` |

## Current Authorized Work

**`TC-DEMOFEED-GATE`** Cursor review complete (awaiting Architect ACCEPT/REWORK).

## Next Planned Work

Architect decision on GATE.
Then **only** a new authorized `.task.md` / `.gate.md` — do not invent experience re-review / purge / next phase.

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
