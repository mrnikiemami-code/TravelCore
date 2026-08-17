# TC-P15-T009 — Search hardening tests & evidence pack

**Task:** TC-P15-T009 — Search hardening and evidence pack  
**Product HEAD:** `183d09d` (`TC-P15-T007` **ACCEPTED**)  
**Date:** 2026-08-17  
**Scope:** Hardening + evidence **only** — no new product capability.  
**Forbidden in this task:** real search engine · ranking/facet engine · recommendation · personalization · embeddings · vector · RAG · LLM · Booking · Payment · Pricing/Catalog/SEO ownership changes.  
**Not this task:** `TC-P15-GATE` (evidence pack only; Gate is next).  
**Vacant slot:** `TC-P15-T008` has no independent product scope in the authoritative plan after R7 lock — do not invent filler work.

## 1. Mission checklist

| # | Verify | Result |
|---|--------|--------|
| 1 | Search owns Retrieval + Discovery only (P15-R1) | **PASS** — T001 |
| 2 | Hybrid Read Model; SearchDocument ≠ domain entity (P15-R2) | **PASS** — T002 |
| 3 | Outbox + Async Projection Worker; idempotent/retryable (P15-R3) | **PASS** — T003 |
| 4 | Faceting aggregation owned by Search; meaning owned by Domain (P15-R4) | **PASS** — T004 |
| 5 | Deterministic ranking composition; ≠ Recommendation / business policy (P15-R5) | **PASS** — T005 |
| 6 | AI readiness = structured attributable locale-aware facts; ≠ AI platform (P15-R6) | **PASS** — T006 |
| 7 | Public Search API engine-neutral; explicit locale; structured filters (P15-R7) | **PASS** — T007 |
| 8 | P15-R1…R7 all RESOLVED | **PASS** — plan open-decisions table |
| 9 | Forbidden engines absent (ES/OpenSearch/SQL FTS/pg_trgm/vector/LLM) | **PASS** — architecture guardrails |
| 10 | No new product capability in this task | **PASS** — evidence/docs + strengthened assertions only |

## 2. Accepted product commits (P15)

| Task | Commit | Essence |
|------|--------|---------|
| PLAN | `fba7a51` | Authoritative P15 plan |
| T001 | `bea92a1` | Search scaffolding / ownership — P15-R1 |
| T002 | `2b3c9d2` | Hybrid read-model / `ISearchIndex` — P15-R2 |
| T003 | `2631c4e` | Projection sync skeleton — P15-R3 |
| T004 | `413d6fe` | Faceting ownership contracts — P15-R4 |
| T005 | `7b22225` | Deterministic ranking contracts — P15-R5 |
| T006 | `edc176f` | AI-readiness / semantic retrieval — P15-R6 |
| T007 | `183d09d` | Public query API stub `GET /api/search` — P15-R7 **ACCEPTED** |

Architect acceptance of T001–T007 is as issued. T009 prepares gate evidence; it does **not** execute `TC-P15-GATE`.

## 3. Locked decisions (all RESOLVED)

| ID | Essence |
|----|---------|
| **P15-R1** | Search = Discovery Owner (`search` schema). Owns query/result contracts and future read models. Does not own Tour/Content/Pricing/Agency facts or SEO IndexPolicy. |
| **P15-R2** | Hybrid Read Model. `SearchDocument` + `ISearchIndex`. Domain modules remain SoT. No physical engine in T002. SearchDocument is not a domain entity. |
| **P15-R3** | Transactional Outbox + Async Projection Worker. Search failure must not fail domain transaction. Projection retryable + idempotent. No real queue in T003. |
| **P15-R4** | Search owns Aggregation / Counting / Result composition. Domain owns attribute meaning + source facts. PE owns filter UI only. |
| **P15-R5** | Deterministic explainable signals + stable tie-break. Ranking ≠ Recommendation. Not commercial/business-policy authority. |
| **P15-R6** | Structured attributable locale-aware facts first. Semantic retrieval + provenance. Search ≠ AI platform / vector store / LLM gateway. |
| **P15-R7** | Engine-neutral `GET /api/search`. Structured filters · continuation-ready pagination · explicit locale. Not SEO IndexPolicy. Empty stub allowed. |

## 4. Boundary / ownership matrix

| Concern | Owner | P15 posture |
|---------|-------|-------------|
| Query / Discovery / Retrieval contracts | **Search** | Engine-neutral |
| Tour catalog facts | **Tour** | Projected only; not Search SoT |
| Editorial Content facts | **Content** | Projected only |
| Price facts | **Pricing** | Not owned by Search |
| AgencyOffer facts | **AgencyMarketplace** | Not owned by Search |
| SEO IndexPolicy / Landing | **Seo** | Search API ≠ Landing |
| Filter UI | **PublicExperience** | Faceting calculation = Search |
| Ranking composition | **Search** | Not Recommendation / commission invent |
| Booking / Payment | **Out of P15** | Forbidden |

## 5. Invariant evidence (T001–T007)

### 5.1 Search ≠ peer SoT

- `SearchOwnershipBoundary`: OwnsTour/Content/Pricing/Agency/IndexPolicy = false.
- Architecture: Search projects must not project-reference peer business modules.

### 5.2 Read model ≠ domain model

- `SearchIndexBoundary.SearchDocumentIsDomainEntity = false`.
- No `DbSet<SearchDocument>`; no concrete `ISearchIndex` in Infrastructure.

### 5.3 Sync ≠ synchronous domain write

- `SearchProjectionSyncBoundary`: DomainTransactionIncludesSearchWrite = false; retryable + idempotent.
- Skeleton worker: duplicate events → `DuplicateSkipped`.

### 5.4 Faceting / Ranking / AI / Query API

- Faceting: aggregation ownership without facet engine / ES aggregations / domain facet tables.
- Ranking: `ISearchRanker` port; no ML/personalization/business invent.
- AI readiness: `SemanticRetrievalSnapshot` + provenance; no embeddings/vector/RAG/LLM.
- Query API: `GET /api/search` + `EmptySearchQueryService`; locale required; no provider DSL leak.

## 6. Guardrail / test surfaces

| Area | Evidence |
|------|----------|
| Unit | `TravelCore.Modules.Search.UnitTests` — ownership, contracts, projection idempotency, empty query stub |
| Architecture | `SearchBoundaryGuardrailTests` — peer refs, forbidden engines, faceting/ranking/AI/query boundaries |
| Host | `SearchPublicQueryHostTests` — locale required; empty stub; no lucene/shard leak |
| Persistence | Search schema migrate lifecycle (prior P15 tasks) |

## 7. Validation commands (this task)

```text
dotnet build TravelCore.sln
dotnet test tests/Unit/TravelCore.Modules.Search.UnitTests
dotnet test tests/Architecture/TravelCore.ArchitectureTests
dotnet test tests/Integration/TravelCore.Host.IntegrationTests
git diff --check
```

Frontend `tsc --noEmit`: N/A — no frontend TypeScript project in this repository root for Search surfaces.

## 8. Carry-forward invariants into GATE

- Listing ≠ SEO Landing · Filter UI ≠ Faceting · Related ≠ Recommendation · PublicExperience ≠ Search Owner · Search API ≠ Search Engine API · Search Read Model ≠ Domain Model · Ranking ≠ Business Priority · Search ≠ AI Platform.
