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

**P32 — Commercial Demo Data & Media Enrichment** (`TC-P32-T004` Cursor **PASS** · awaiting Architect review)

## Completed

- **P00–P29** COMPLETE / ACCEPTED
- `TC-P30-GATE` **ACCEPTED WITH KNOWN LIMITATIONS**
- `TC-DEMOFEED-GATE` **ACCEPTED**
- `TC-P31-GATE` **ACCEPTED WITH KNOWN LIMITATIONS**
- `TC-P32-T001` **ACCEPTED WITH KNOWN LIMITATIONS**
- `TC-P32-T002` Cursor **PASS** — Hotel/Tour media enrichment
- `TC-P32-T003` Cursor **PASS WITH KNOWN LIMITATIONS** — live scenario validation
- `TC-P32-T004` Cursor **PASS** — Hotel public browse EF fix

## Current Important Locks

| Lock | Value |
|------|--------|
| Product order | **Experience → Data → Commercial** |
| P32 | **ACTIVE** (`T004` Cursor PASS · Architect review) |
| Evidence | `docs/product-experience/evidence/P32-T003/` · `P32-T004/` |
| Cursor PASS ≠ Architect ACCEPT | Mandatory |
| Pipeline Controller | File-Based Task Pipeline V3 |

## Current Authorized Work

**None** — RESULT posted; WAITING for Architect `.task.md` / `.gate.md`.

## Next Planned Work

Architect review of **`TC-P32-T004`** only — then authorized next unit (do not invent GATE).

## Open Blockers

1. Destination media attach — no Destination↔Media owner API.
2. Public hotel list `StarRating` omitted (owned Hotel join avoided).

## Rules

- No product execution without authorized `.task.md` / `.gate.md`
- Never invent tasks from ROADMAP / deferred items
- Never exit PIPELINE mode while USER keeps it active — RESULT → WAITING MODE
- No scraping / competitor copy

## Revision

| Date | Change |
|------|--------|
| 2026-08-21 | Sync after TC-P32-T003 live scenario validation |
| 2026-08-21 | Sync after TC-P32-T004 hotel public browse fix |
