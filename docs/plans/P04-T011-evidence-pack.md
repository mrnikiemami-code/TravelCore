# P04 Evidence Pack — TC-P04-T011

**Task:** TC-P04-T011 — Phase hardening tests & evidence pack  
**Baseline HEAD:** `dc9d00d`  
**Date:** 2026-08-16  
**Scope:** Validation / evidence only — no product features; **gate not executed**.

## 1. Phase map (product commits)

| Task | Commit | Notes |
|------|--------|--------|
| PLAN | `9d264e6` | P04 plan accepted |
| T001 | `5de2ae1` | ReferenceData / Destination scaffolding |
| T002 | `3363cf1` | ReferenceData catalogs + persistence |
| T003 | `9176dbe` | Destination hierarchy domain + persistence (R1 kinds) |
| T004 | `9c30e77` (+ docs `da9730e`) | Translations FA/EN + geo |
| T005 | `3dabe6f` | Path / ancestors / descendants |
| T006 | `edc201f` (+ docs `124d57b`) | Localized slug hooks |
| T007 | `ba04618` (+ docs `76528e6`) | Access authz for Destination mutations |
| T008 | `81fd6ce` | Guided Admin Destination hierarchy workflow |
| T009 | `660d2c4` | Public Destination baseline + R3 noindex,follow |
| T010 | `dc9d00d` | ReferenceData Admin/read UX baseline |

## 2. Architectural invariants verified

- ReferenceData ≠ Destination (separate schemas / modules; ISO country catalog ≠ Destination Country node)
- DestinationKind closed (R1): Country · Region · City · Area
- Destination ≠ Place / Tour / Media / Content ownership
- Localized slug hooks ≠ SEO engine (P05)
- R3 RESOLVED: public Destination pages may exist; robots = **noindex, follow**
- Access-backed Destination mutations (`destination.destinations.write` / `Access.Destination.Destinations.Write`)
- Server Component First; Client islands allowlisted
- Job-based Admin IA (`catalog`) — not module-silo CRUD menus
- Raw IDs are not primary public/Admin UX

## 3. Validation battery (this task)

| Suite | Result |
|-------|--------|
| ArchitectureTests | 17 PASS |
| Destination.UnitTests | 7 PASS |
| ReferenceData.UnitTests | 4 PASS |
| Host.IntegrationTests | 11 PASS |
| Persistence.IntegrationTests | 16 PASS |
| Frontend `npm run quality` | PASS |
| `git diff --check` | PASS |
| HEAD == origin/main (baseline) | `dc9d00d` |

## 4. Host / authz evidence (representative)

- `DestinationAccessAuthorizationTests`: mutation 401 / 403 / 200 matrix
- Public Destination reads remain unauthenticated (T007/T009)
- Cookie transport `TravelCore.Identity` unchanged (no Bearer)

## 5. Frontend evidence

| Surface | Route |
|---------|--------|
| Admin Destination workflow | `/[locale]/admin/catalog/destinations` |
| Admin ReferenceData read | `/[locale]/admin/catalog/reference` |
| Public Destination | `/[locale]/destinations/[slug]` (noindex,follow) |
| Quality gates | allowlist + route/metadata assertions in `p02-quality-checks.mjs` |

## 6. Gate checklist preview (§16 plan)

| # | Criterion | Evidence posture |
|---|-----------|------------------|
| 1 | Separate schemas / no cross-schema writes | Architecture + Persistence tests |
| 2 | ReferenceData ≠ Destination | Architecture + ownership docs |
| 3 | Hierarchy kinds | R1 + Destination unit/persistence |
| 4 | Translations | T004 + unit/persistence |
| 5 | Hierarchy queries | T005 |
| 6 | Slug hooks without SEO engine | T006 + T009 (no canonical/hreflang/sitemap) |
| 7 | Admin Access-backed / no raw-ID UX | T007 + T008 |
| 8 | Public Destination only Destination (+ RD refs) | T009 |
| 9 | Server Component First | quality allowlist |
| 10 | P05+ engines absent | code review / routes |
| 11 | Evidence pack + green battery | **this document** |

## 7. Ready for gate?

**YES — evidence pack ready for `TC-P04-GATE`.**  
This task does **not** execute the gate and does **not** start P05.
