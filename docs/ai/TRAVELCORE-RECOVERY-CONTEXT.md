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

**P30 — Product Experience Foundation**

## Completed

- **P00–P29** COMPLETE / ACCEPTED
- Post-P29 evolution tracks COMPLETE
- `TC-DEMOFEED-PLAN` authored · **DEFERRED**
- `TC-P30-PLAN` **ACCEPTED**
- `TC-P30-T002` PASS (Constitution)
- `TC-P30-T003` PASS (Design System 2.0) · architect ACCEPTED in channel
- `TC-P30-T004-ENVELOPE-CREATE` PASS (`80a5bb6`)
- `TC-P30-T004` **ACCEPTED WITH NOTES** (Application Shells)
- `TC-P30-T005-ENVELOPE-CREATE` PASS (`2f03718`)
- `TC-P30-T005` **PASS / AWAITING_ARCHITECT_REVIEW** (technical) · Visual Gate **PENDING / REWORK_RECOMMENDED** (`TC-P30-T005-VISUAL-CHECKPOINT-C`)

## Current Important Locks

| Lock | Value |
|------|--------|
| Product order | **Experience → Data → Commercial** |
| DEMOFEED | **DEFERRED** |
| No Page-First | Design System before pages · shells before full Home |
| One Design System / Three Experiences | Public · Admin · Agency |
| North Star | `docs/product-experience/assets/travelcore-ui-ux-north-star.png` |
| Constitution | `docs/product-experience/TRAVELCORE-PRODUCT-EXPERIENCE-CONSTITUTION.md` |
| Design System 2.0 | `docs/product-experience/DESIGN-SYSTEM-2.0.md` |
| Shells board | `/[locale]/dev/shells` |
| Public Home | `/[locale]` (T005 sellable foundation) |
| Cursor PASS ≠ Architect ACCEPT | Mandatory |
| Persistent T004 envelope | `docs/plans/TC-P30-T004-task-envelope.md` |
| Persistent T005 envelope | `docs/plans/TC-P30-T005-task-envelope.md` |

## Runtime Roles

| Role | Actor |
|------|--------|
| Architect | ChatGPT |
| Implementation Agent | Cursor |
| Source of Truth | Repository recovery / SoT documents |
| Architect channel | `https://chatgpt.com/g/g-p-6a79dbc6468c8191a5e74afa2d82a8be-travelcore/c/6a8039a8-2014-83ed-be9f-813280b23bcb` |

## Current Authorized Work

**`TC-P30-T005-VISUAL-CHECKPOINT-C`** — Visual Checkpoint C (this update · REWORK_RECOMMENDED)

## Next Planned Work

Architect decision on T005 visual rework vs ACCEPT.  
**`TC-P30-T006`** only with a separate valid authorized envelope.

Do **not** auto-start T006 / DEMOFEED from this document.

## Open Blockers

**None**

## Rules

- No product execution without authorized envelope
- Never invent tasks from ROADMAP / deferred items
- Never switch architect channel mid-pipeline
- Major UI: visual evidence vs North Star mandatory

## Revision

| Date | Change |
|------|--------|
| 2026-08-20 | Initial · Controller Mode |
| 2026-08-20 | Sync after T002 / T003 |
| 2026-08-20 | Sync after `TC-P30-T004` Application Shells |
| 2026-08-20 | Sync after `TC-P30-T005` Public Home Experience |
| 2026-08-20 | Sync after `TC-P30-T005-VISUAL-CHECKPOINT-C` (REWORK_RECOMMENDED) |
