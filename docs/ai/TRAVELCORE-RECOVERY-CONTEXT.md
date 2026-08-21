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

**P32 — Commercial Demo Data & Media Enrichment** (`TC-P32-T003` Cursor **PASS WITH KNOWN LIMITATIONS** · awaiting Architect review)

## Completed

- **P00–P29** COMPLETE / ACCEPTED
- `TC-P30-GATE` **ACCEPTED WITH KNOWN LIMITATIONS**
- `TC-DEMOFEED-GATE` **ACCEPTED**
- `TC-P31-GATE` **ACCEPTED WITH KNOWN LIMITATIONS**
- `TC-P32-T001` **ACCEPTED WITH KNOWN LIMITATIONS** — media strategy + demo asset pack foundation
- `TC-P32-T002` Cursor **PASS** — DEMOFEED Hotel/Tour media enrichment via Media ownership
- `TC-P32-T003` Cursor **PASS WITH KNOWN LIMITATIONS** — live scenario validation evidence

## Current Important Locks

| Lock | Value |
|------|--------|
| Product order | **Experience → Data → Commercial** |
| P31 | **GATE ACCEPTED WITH KNOWN LIMITATIONS** |
| P32 | **ACTIVE** (`T003` Cursor PASS WITH KNOWN LIMITATIONS · Architect review) |
| Demo media pack | `docs/product-experience/assets/demo-media/` |
| Evidence | `docs/product-experience/evidence/P32-T003/` |
| Feeder | `tools/demofeed` · `enrich-media` · prefix `demofeed-*` |
| Cursor PASS ≠ Architect ACCEPT | Mandatory |
| Pipeline Controller | File-Based Task Pipeline V3 |

## Current Authorized Work

**None** — RESULT posted; WAITING for Architect `.task.md` / `.gate.md` after T003 review.

## Next Planned Work

Architect review / ACCEPT of **`TC-P32-T003`** only — then authorized next unit (do not invent GATE/T004).

## Open Blockers

1. **Destination media attach** — no Destination↔Media owner API.
2. **Place public hotel browse** — EF LINQ translation 500 on `/api/place/public/hotels` (blocks Hotel listing/detail success UI).

## Rules

- No product execution without authorized `.task.md` / `.gate.md`
- Never invent tasks from ROADMAP / deferred items
- Never exit PIPELINE mode while USER keeps it active — RESULT → WAITING MODE
- No scraping / competitor copy

## Revision

| Date | Change |
|------|--------|
| 2026-08-21 | Sync after Architect ACCEPT of TC-P31-GATE |
| 2026-08-21 | Sync after TC-P32-T001 media strategy + asset pack |
| 2026-08-21 | Sync after TC-P32-T002 enrich-media (Hotel/Tour) |
| 2026-08-21 | Sync after TC-P32-T003 live scenario validation |
