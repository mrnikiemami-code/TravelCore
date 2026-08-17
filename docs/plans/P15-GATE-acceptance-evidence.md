# TC-P15-GATE — P15 Acceptance Evidence

**Task:** TC-P15-GATE — P15 Search Acceptance Gate  
**Baseline HEAD:** `b741bc5` (`TC-P15-T009` **ACCEPTED**)  
**Date:** 2026-08-17  
**Scope:** Gate / acceptance only — no new Search capability; **P16 not started** until Gate ACCEPT. Ceremonial Gate wait is **not** a pipeline stop.

## 1. Preconditions

| Check | Result |
|-------|--------|
| USER PIPELINE + continuity override | YES |
| Ceremonial GATE token | **Not required** |
| Architect Auto-Execute GATE | YES |
| T001–T007 + T009 ACCEPTED · T008 VACANT · R1–R7 RESOLVED | YES |
| Evidence pack | YES — [`P15-T009-hardening-and-evidence-pack.md`](P15-T009-hardening-and-evidence-pack.md) |
| Working tree at gate start | CLEAN (`b741bc5`) |

## 2. Checklist (architect GATE)

| # | Criterion | Result |
|---|-----------|--------|
| 1 | Search owns Retrieval + Discovery only (P15-R1) | **PASS** — T001 |
| 2 | Hybrid Read Model; SearchDocument ≠ domain entity (P15-R2) | **PASS** — T002 |
| 3 | Outbox + Async Projection Worker; retryable/idempotent (P15-R3) | **PASS** — T003 |
| 4 | Faceting aggregation owned by Search; meaning owned by Domain (P15-R4) | **PASS** — T004 |
| 5 | Ranking composition ≠ Recommendation / business policy (P15-R5) | **PASS** — T005 |
| 6 | AI readiness = structured attributable locale-aware facts (P15-R6) | **PASS** — T006 |
| 7 | Public Search API engine-neutral; locale-explicit (P15-R7) | **PASS** — T007 |
| 8 | T008 vacant (no invent) | **PASS** |
| 9 | Hardening / evidence | **PASS** — T009 |
| 10 | Search ≠ Tour/Content/Pricing/Agency SoT · ≠ SEO IndexPolicy · ≠ Booking/Payment | **PASS** |
| 11 | Forbidden engines absent (ES/OpenSearch/SQL FTS/pg_trgm/vector/LLM) | **PASS** |
| 12 | No new Search capability in Gate | **PASS** — evidence only |

## 3. Locked decisions

**P15-R1…R7 all RESOLVED** — see [`P15-implementation-plan.md`](P15-implementation-plan.md) open-decisions table.

## 4. Accepted product commits (P15)

| Task | Commit |
|------|--------|
| PLAN | `fba7a51` |
| T001 | `bea92a1` |
| T002 | `2b3c9d2` |
| T003 | `2631c4e` |
| T004 | `413d6fe` |
| T005 | `7b22225` |
| T006 | `edc176f` |
| T007 | `183d09d` |
| T008 | VACANT |
| T009 | `b741bc5` |

## 5. Ownership / architecture matrix

| Invariant | Result |
|-----------|--------|
| SearchDocument != Domain Entity | **PASS** |
| Search Read Model != Source of Truth | **PASS** |
| Domain transaction != synchronous Search write | **PASS** |
| Projection = Outbox + Async / Retryable / Idempotent | **PASS** |
| Search API != Search Engine API | **PASS** |
| Search API != Physical Search Engine API | **PASS** |
| Ranking != Recommendation | **PASS** |
| Faceting != Domain attribute ownership | **PASS** |
| AI Readiness != AI Infrastructure | **PASS** |

Carry-forward (ASCII): Search API != Search Engine API · SearchDocument != Domain Entity · Ranking != Recommendation · Faceting != Domain attribute ownership · AI Readiness != AI Infrastructure.

## 6. Public query contract

- `GET /api/search` · locale required · structured filters · continuation-ready · requested facets · `EmptySearchQueryService` stub.

## 7. Validation battery (gate re-run)

| Suite | Result | Detail |
|-------|--------|--------|
| `dotnet build TravelCore.sln` | **PASS** | 0 Error(s) · 0 Warning(s) |
| Search.UnitTests | **PASS** | **17** |
| ArchitectureTests | **PASS** | **195** |
| Host.IntegrationTests | **PASS** | **46** |
| Frontend `tsc --noEmit` (`src/frontend/web`) | **PASS** | clean |
| `git diff --check` | **PASS** | clean |
| Persistence.IntegrationTests | **PASS** | **25** (Docker/Testcontainers; Search schema lifecycle included) |

```text
dotnet build TravelCore.sln
dotnet test tests/Unit/TravelCore.Modules.Search.UnitTests
dotnet test tests/Architecture/TravelCore.ArchitectureTests
dotnet test tests/Integration/TravelCore.Host.IntegrationTests
dotnet test tests/Integration/TravelCore.Persistence.IntegrationTests
npx --yes tsc --noEmit  (src/frontend/web)
git diff --check
```

## 8. Explicit OUT / DEFER

- Physical Search engine (Elasticsearch/OpenSearch/SQL FTS) — **later**
- Real ranking/facet engines — **later**
- Booking / Payment — **later phases**
- Recommendation / personalization / embeddings / RAG / LLM — **not invented**
- P16 UGC — **after Gate ACCEPT only**

## 9. Architect STOP rules honored

| Rule | Honored |
|------|---------|
| No P16 product before Gate ACCEPT | YES |
| No inventing unlocked R# | YES (R1–R7 resolved) |
| No new Search capability in GATE | YES |
| No force-push / history rewrite | YES |

## 10. Gate outcome

Awaiting architect ACCEPT of this evidence commit. Ceremonial Gate wait is **not** a pipeline stop. P16 must not start until Gate ACCEPT + Auto-Execute PLAN.
