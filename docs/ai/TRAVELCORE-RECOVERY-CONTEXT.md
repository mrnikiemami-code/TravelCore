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
- Post-P29 evolution tracks COMPLETE (incl. MODOPS / HOTIDX / HOMFEED retroactively accepted after forensic ledger)
- `TC-DEMOFEED-PLAN` authored · **DEFERRED** (Experience before Data)
- `TC-P30-PLAN` ACCEPTED (plan authored / locked)
- `TC-P30-WORKTREE-CLEANUP-01` ACCEPTED / hygiene complete
- `TC-P30-NORTHSTAR-ASSET-FORMAT-FIX-01` PASS · approved North Star normalized to real PNG (`447891c`)

## Current Important Locks

| Lock | Value |
|------|--------|
| Product order | **Experience → Data → Commercial** |
| DEMOFEED | Plan authored · execution **DEFERRED** until P30 experience foundation approved |
| No Page-First | Design System before pages |
| One Design System / Three Experiences | Public Marketplace · Admin Console · Agency Portal |
| North Star | `docs/product-experience/assets/travelcore-ui-ux-north-star.png` |
| Visual acceptance | Screenshot evidence + architect/user visual review required for major UI |
| Cursor PASS ≠ Architect ACCEPT | Mandatory |
| Pipeline Protocol | READY · USER opt-in PIPELINE |
| Pipeline Controller Mode | Mandatory · [`TRAVELCORE-PIPELINE-CONTROLLER.md`](TRAVELCORE-PIPELINE-CONTROLLER.md) |

## Runtime Roles

| Role | Actor |
|------|--------|
| Architect | ChatGPT |
| Implementation Agent | Cursor |
| Source of Truth | Repository recovery / SoT documents |
| Architect channel (transport) | `https://chatgpt.com/g/g-p-6a79dbc6468c8191a5e74afa2d82a8be-travelcore/c/6a8039a8-2014-83ed-be9f-813280b23bcb` |

## Current Authorized Work

At authoring of this recovery context document:

**`TC-PIPELINE-CONTROLLER-MODE-001`** (this governance task)

## Next Planned Work

**`TC-P30-T002`** — Product Experience Constitution + Visual Acceptance Protocol + Recovery Lock

Only after:

1. Architect ACCEPT of this controller task (when required)
2. A valid authorized `BEGIN_TRAVELCORE_CURSOR_TASK_V1` for T002

Do **not** auto-start T002 from this document.

## Open Blockers

**None** (working tree expected CLEAN after North Star format fix `447891c`)

## Rules

- No product execution without authorized envelope
- Never infer work from ROADMAP / deferred items / commits / unfinished ideas
- Never switch architect channel mid-pipeline
- On chat/session loss: run Recovery Packet · default **HUMAN** · PIPELINE only after fresh USER activation

## Update Policy

Update this document after:

- accepted gates
- phase transitions
- blocker open/close
- authorized next-task changes
- North Star / product-experience constitution changes

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-20 | Initial recovery context · `TC-PIPELINE-CONTROLLER-MODE-001` |
