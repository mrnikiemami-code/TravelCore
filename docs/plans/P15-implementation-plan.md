# P15 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P15-PLAN` |
| Phase | P15 — Search & Discovery |
| Status | IN PROGRESS — PLAN ACCEPTED; P15-R1–R7 RESOLVED; T007 public query API delivered |
| Baseline | `608216d` (`docs: P14 acceptance gate evidence [TC-P14-GATE]` — **TC-P14-GATE** ACCEPTED; P14 COMPLETE) |
| Authoritative sources | `docs/ROADMAP.md` § P15 · P14 Gate ACCEPT · P14-R3/R5/R8 (Listing ≠ Landing · Related ≠ Recommendation · Filter ≠ Faceting) · P05 SEO · P08 Content · P09 Tour · P12 Pricing · P13 AgencyMarketplace |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P15** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

> **Envelope note:** Authored from **repository SoT** + architect P14 Gate ACCEPT continuity (auto-start P15 PLAN). Under PIPELINE continuity, ceremonial confirms and ceremonial Gate waits are **not required**. **No product code in PLAN task.**

---

## 1. Phase Purpose

P15 باید قابلیت **Search & Discovery** را معرفی کند بدون دزدیدن مالکیت Tour، Content، Pricing، AgencyMarketplace، یا SEO.

هدف (از Roadmap + Gate ACCEPT):

1. **Search = Retrieval + Discovery Owner** — Query · Ranking · Faceting · FTS/optimization.
2. **Tour = Fact Owner** — Catalog SoR باقی می‌ماند؛ Search کاتالوگ را تکرار نمی‌کند.
3. **Content = Editorial Owner** — Search ممکن است محتوای قابل‌کشف را ایندکس کند؛ مالک تحریر نیست.
4. **Pricing = Price Owner** — Search قیمت را مالک نمی‌شود؛ ممکن است فیلدهای نمایشی را از Pricing بخواند.
5. **AgencyMarketplace = Agency Offer Owner** — Search مالک Offer/publication نیست.
6. **SEO = Index Policy Owner** — Search مالک IndexPolicy/canonical نیست؛ Landing ≠ filtered listing (P14-R3) حفظ می‌شود.
7. Search پشت **abstraction** بماند تا موتور آینده بدون بازنویسی Domain ممکن شود (Roadmap: PostgreSQL FTS + `pg_trgm` اولیه؛ تعهد زودهنگام Elasticsearch ممنوع مگر قفل).

P14 تحویل داد: Public Experience presentation (Detail/Listing/Landing) + sticky ≠ Booking + Related/Content/Agency composition + Filter presentation only.

P15 اضافه می‌کند: **Search module** برای بازیابی/facet/ranking — **بدون** Booking، بدون Payment، بدون Recommendation engine، بدون Catalog duplication.

---

## 2. Starting Baseline

| Item | Value |
|------|--------|
| P14 Gate | `TC-P14-GATE` COMPLETE / ACCEPTED (`608216d`) |
| P14 evidence | [`P14-GATE-acceptance-evidence.md`](P14-GATE-acceptance-evidence.md) · [`P14-T009-hardening-and-evidence-pack.md`](P14-T009-hardening-and-evidence-pack.md) |
| P14 Plan | ACCEPTED · R1–R8 RESOLVED |
| Baseline HEAD | `608216d` |
| P00–P14 | COMPLETE |
| Public listing filters | Presentation/URL only (P14-R8); selection via Tour `related-published` |
| Related tours | Deterministic replaceable Tour public-read (P14-R5) — P15 may replace retrieval |
| Search module | Scaffolded (`search` schema · query/result contracts · P15-R1) |
| Booking / Payment | Modules do not exist |

---

## 3. Non-goals (explicit)

1. Booking engine / reservation / checkout / Payment.
2. Replacing PostgreSQL as source of truth for Tour/Content facts.
3. Premature hard commitment to Elasticsearch/OpenSearch as Domain SoR (abstraction first).
4. Recommendation / personalization / embeddings / AI ranking infrastructure.
5. Duplicating TourProduct as a second catalog SoR inside Search.
6. Moving IndexPolicy ownership out of SEO.
7. Stealing Price ownership from Pricing or Offer ownership from AgencyMarketplace.
8. Inventing unlocked R# closures — open decisions stay OPEN until architect lock.

---

## 4. Task sequence (proposed)

### TC-P15-PLAN — this document

### TC-P15-T001 — Search module scaffolding / ownership boundary
- Purpose: Independent Search module + ownership contracts (**P15-R1 RESOLVED**).
- Delivered: Contracts/Domain/Infrastructure scaffolding; schema `search`; query/result contracts; host registration; no peer FKs; PE remains presentation.
- Forbidden kept: no projection tables / indexing engine / Elasticsearch / FTS / ranking algorithm / faceting engine / Booking / Payment / Recommendation.

### TC-P15-T002 — Index / read-model strategy baseline
- Purpose: How Search represents discoverable documents (**P15-R2 RESOLVED** — Hybrid Read Model).
- Delivered: `SearchDocument` + `ISearchIndex` + projection-ready envelopes. Domain modules remain SoT. No concrete engine.
- Forbidden kept: Elasticsearch / OpenSearch / SQL FTS / `pg_trgm` / ranking / faceting / embedding / TourProduct clone / Pricing SoR copy.

### TC-P15-T003 — Synchronization strategy baseline
- Purpose: How facts flow into Search without Domain rewrite (**P15-R3 RESOLVED** — Transactional Outbox + Async Projection Worker).
- Delivered: `SearchProjectionEvent` · sync boundary · `ISearchProjectionWorker` · `ISearchProjectionIdempotencyStore` · skeleton worker. No broker/queue.
- Forbidden kept: RabbitMQ · Elasticsearch · OpenSearch · ranking · faceting · embeddings · peer FKs · Search write inside domain transaction.

### TC-P15-T004 — Search faceting ownership boundary
- Purpose: Facets owned by Search without domain ownership (**P15-R4 RESOLVED**).
- Delivered: `FacetDefinition` / `FacetValue` / `FacetResult` · `SearchFacetingBoundary`. Search owns Aggregation/Counting/Result composition; Domain owns attribute meaning/source facts; PE owns UI only.
- Forbidden kept: Facet engine · Elasticsearch aggregations · Ranking · Recommendation · AI model · Tour/Content facet tables · Pricing facet ownership.

### TC-P15-T005 — Ranking boundary baseline
- Purpose: Ranking owned by Search without business-policy invent (**P15-R5 RESOLVED**).
- Delivered: `RankingSignal` / `RankingContext` / `RankingResult` / `ISearchRanker` · `SearchRankingBoundary`. Deterministic explainable signals + stable tie-break. Ranking ≠ Recommendation.
- Forbidden kept: ML ranking · AI/embeddings/vector · personalization · commission/sponsorship · agency ranking · ES/OpenSearch · Booking/Payment · inventing R6/R7.

### TC-P15-T006 — AI / Search readiness (boundary only)
- Purpose: Structurally AI-consumable retrieval without LLM/vector invent (**P15-R6 RESOLVED**).
- Delivered: `SearchAiReadinessBoundary` · `SemanticRetrievalSnapshot` · `SearchFactProvenance` · optional semantic/eligibility/provenance fields on `SearchDocument` / projection envelope.
- Forbidden kept: embeddings · vector DB/search · RAG · LLM/prompt · AI-generated facts · Search-as-SoT · inventing R7.

### TC-P15-T007 — Query API / listing retrieval integration
- Purpose: Engine-neutral public Search query surface (**P15-R7 RESOLVED**).
- Delivered: `GET /api/search` · `SearchPublicQueryRequest/Response` · `ISearchQueryService` · `EmptySearchQueryService` stub · `SearchQueryApiBoundary`.
- Forbidden kept: ES/OpenSearch/SQL FTS · provider DSL · recommendation · embeddings · SEO IndexPolicy ownership · domain duplication.

### TC-P15-T008 — SEO Landing integration boundary
- Purpose: Search must not conflate Landing with filtered listing URLs.
- Expected: Preserve Listing ≠ SEO Landing; IndexPolicy stays SEO; no auto-index of every filter combo.

### TC-P15-T009 — Hardening + evidence

### TC-P15-GATE — Acceptance Gate
- Evidence only. Ceremonial Gate wait is **not** a pipeline stop.

---

## 5. Open decisions (must not invent)

| ID | Topic | Status | Notes |
|----|-------|--------|-------|
| **P15-R1** | Search ownership boundary | **RESOLVED** | Search = Discovery Owner. Owns query/result contracts and future read models. Does **not** own Tour/Content/Pricing/Agency facts or SEO IndexPolicy. Search is a Read Model / Projection (later), not SoT. No LLM/business rules inside Search. T001: no database projection, no indexing engine, no Elasticsearch, no FTS. |
| **P15-R2** | Index / read model | **RESOLVED** | Hybrid Read Model. Search owns `SearchDocument` + `ISearchIndex` abstraction. Domain modules remain SoT. No Elasticsearch/OpenSearch/SQL FTS/`pg_trgm` in T002. Search Document is not a domain entity. |
| **P15-R3** | Data synchronization strategy | **RESOLVED** | Transactional Outbox + Async Projection Worker. Search failure must not fail domain transaction. Projection retryable + idempotent. No RabbitMQ/real queue in T003. |
| **P15-R4** | Faceting ownership | **RESOLVED** | Search owns Aggregation / Counting / Result composition. Domain owns attribute meaning + source facts. PE owns filter UI only (P14-R8). No facet engine / ES aggregations / domain facet tables in T004. Structured fields remain available for future facets. |
| **P15-R5** | Ranking model | **RESOLVED** | Deterministic explainable signals + stable tie-break. Search owns ranking composition/ordering/metadata. Not business-policy authority. Ranking ≠ Recommendation. No ML/embeddings/personalization in T005. |
| **P15-R6** | AI / Search readiness | **RESOLVED** | Structured attributable locale-aware facts first. Semantic retrieval snapshot + provenance. No embeddings/vector/RAG/LLM. Search ≠ SoT. Consumer-neutral reusable contracts. |
| **P15-R7** | Query API contract | **RESOLVED** | Engine-neutral `GET /api/search`. Structured filters · continuation-ready pagination · explicit locale. Not SEO IndexPolicy. Empty stub execution allowed. No provider DSL. |

---

## 6. Architecture invariants (carry forward)

1. Tour = Fact Owner · Content = Editorial Owner · Pricing = Price Owner · AgencyMarketplace = Offer Owner · SEO = IndexPolicy Owner · Search = Retrieval/Discovery Owner.
2. Listing ≠ SEO Landing · Filtered URL ≠ SEO Landing ownership.
3. Filter UI (P14) ≠ Faceting Engine (P15).
4. Related Tours presentation ≠ Recommendation Engine.
5. PublicExperience remains presentation/composition only.
6. Published ≠ Bookable · Sticky Action ≠ Booking.
7. No Booking/Payment modules in P15 unless a later lock says otherwise.
8. Do not invent unlocked R# closures.

---

## 7. Repository safety

- Branch `main` · fast-forward push only · no force · CLEAN working tree before RESULT.
- One docs commit for PLAN (no product code).
- After PLAN ACCEPT, Auto-Execute first locked product task only when architect envelope names it.
