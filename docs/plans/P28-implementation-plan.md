# P28 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P28-PLAN` |
| Phase | P28 — Performance & Scale |
| Status | PLAN ACCEPTED · **P28 READY_FOR_GATE** · T002–T008 ACCEPTED · T009 evidence executed |
| Baseline | `ddbc0ba` (`docs: add P28 implementation plan`) |
| Authoritative sources | `docs/ROADMAP.md` § P28 · `docs/PROJECT-STATE.md` · `docs/architecture/02-technology-baseline.md` · `docs/architecture/04-module-boundaries.md` · `docs/architecture/05-dependency-rules.md` · `docs/architecture/07-data-architecture.md` · `docs/architecture/10-ui-constitution.md` §13 · `docs/architecture/15-future-architecture-transition-map.md` · `docs/architecture/22-observability-logging-and-correlation-foundation.md` · P06 Media · P15 Search · P27 Analytics · P26 SEO |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

This document is the architecture plan for the Performance & Scale phase.

> **Envelope note:** `TC-P28-PLAN` ACCEPTED · `TC-P28-T002`–`T008` ACCEPTED · `TC-P28-T009` implemented (evidence pack) · **READY_FOR_GATE** · **do not execute `TC-P28-GATE` until architect accepts `T009`**.

---

## 0. Next-phase resolve (from SoT)

| Question | Answer |
|----------|--------|
| Prior phase status | **P27 COMPLETE / ACCEPTED** (`TC-P27-GATE` `fb55c0a` / SoT `f20db63`) |
| Authoritative next phase | **P28 — Performance & Scale** |
| Declared status before this plan | **PLANNED / NOT_STARTED** |
| Dedicated Performance module in SoT today? | **NO** — performance themes exist in technology baseline and UI constitution only |
| Redis/cache implemented in product code? | **NO** — baseline declares Redis as non-SoR helper; no cache abstraction productized yet |
| Observability platform exists? | **YES** — `TravelCore.Observability` · platform telemetry separate from product analytics |
| Profile-before-optimize locked? | **YES** — ROADMAP P28: measure before optimizing; no unmeasured distributed complexity |

---

## 1. Phase purpose

P28 introduces **measured** performance and scale boundaries **after** meaningful product surfaces and platform foundations exist, without premature microservice extraction or cache-as-SoR anti-patterns.

Business purpose (from SoT):

- Improve perceived and measured performance on real public/admin surfaces
- Establish query/index, cache, CDN, rendering, and search-read performance posture
- Preserve modular ownership while enabling future scale work

Architecture objective:

- Introduce **platform-level performance abstractions** (measurement, cache, read projections, delivery posture) without breaking module boundaries
- Preserve **PostgreSQL as SoR** · **Redis != SoR** · **Cache != Source of Truth**
- Preserve **Search != ranking engine** · **Observability != Product Analytics**
- **Profile before optimize** — no Kafka/microservice/cache-everything without measured need

---

## 2. Preserved locked architecture

P28 must preserve:

1. Modular Monolith — schema-per-module; no peer-schema FK; no shared DbContext.
2. **EF Core for transactional writes/migrations**; **Dapper only for justified read projections**.
3. **Redis is helper/cache only** — never authoritative state.
4. **Search read performance != Search ranking engine** — P15 boundary unchanged.
5. **Observability != Product Analytics** — P27 boundary unchanged.
6. **SEO/Content/Destination/Booking/Payment ownership** unchanged.
7. **Public != Indexable** · **Search URL != SEO Landing** preserved.
8. P21–P27 ownership boundaries remain unchanged.
9. No distributed complexity (microservices · message bus product · multi-region) without measured operational need.

---

## 3. Current SoT baseline snapshot

- Technology baseline locks PostgreSQL SoR · Redis helper · Dapper for justified reads · Next.js Server Component first.
- UI constitution §13 declares Core Web Vitals targets (LCP/INP/CLS) as quality goals, not domain invariants.
- P06 Media delivered app-proxy delivery; WebP/AVIF pipeline **DEFERRED** (P06-R1).
- P15 Search boundary exists; hybrid read-model posture; ranking engine deferred.
- P26 SEO graph boundaries complete; sitemap/structured-data frameworks on P05 base.
- P27 Analytics downstream boundaries complete; no warehouse/streaming product.
- Observability foundation documented; platform telemetry separate from product analytics.
- No Redis client, cache abstraction, CDN integration, or load-test harness in product code today.

---

## 4. Decision inventory for P28 (open for architect locks)

| ID | Topic | Status |
|----|-------|--------|
| `P28-R1` | Measurement / profiling posture vs Observability | **RESOLVED** — profile-before-optimize · Observability owns platform telemetry · Performance measurement foundation (T003) |
| `P28-R2` | PostgreSQL query/index optimization boundary | **RESOLVED** — module-owned schema data access · no cross-schema DbContext shortcuts · measurement before query tuning (T005) |
| `P28-R3` | Read-model projection boundary (Dapper vs EF) | **RESOLVED** — evidence-based read optimization · Dapper justified by evidence only · EF write/migration owner preserved (T005) |
| `P28-R4` | Redis cache abstraction boundary | **RESOLVED** — cache != SoR · eligibility/invalidation/consistency policy architecture (T006) |
| `P28-R5` | CDN / static delivery boundary | **RESOLVED** — CDN posture boundary declared · vendor product DEFERRED (T008) |
| `P28-R6` | Frontend rendering / bundle / CWV boundary | **RESOLVED** — UI Constitution CWV posture · bundle platform DEFERRED (T008) |
| `P28-R7` | Search read performance boundary | **RESOLVED** — Search read latency posture · ranking engine DEFERRED (T008) |
| `P28-R8` | Load testing / deferred distributed scale posture | **RESOLVED** — scaling/infrastructure boundary · deferred microservice/mesh/multi-region (T007) |

---

## 5. Execution sequence

Proposed sequence after plan acceptance:

1. `TC-P28-PLAN` — P28 architecture implementation plan (**ACCEPTED** · `ddbc0ba`)
2. `TC-P28-T002` — performance foundation boundary (**ACCEPTED** · `38d9ca4`)
3. `TC-P28-T003` — measurement/observability interaction boundary (**ACCEPTED** · `4ac1876`)
4. `TC-P28-T004` — runtime performance boundary and module interaction model (**ACCEPTED** · `e2eee8a`)
5. `TC-P28-T005` — data access and read optimization boundary (**ACCEPTED** · `05d50c8`)
6. `TC-P28-T006` — caching boundary and cache policy architecture (**ACCEPTED** · `fce389d`)
7. `TC-P28-T007` — scaling and infrastructure boundary (**ACCEPTED** · `6edae65` / fix `46bf7ff`)
8. `TC-P28-T008` — operational hardening and deferred scope boundary (**ACCEPTED** · `13a424e`)
9. `TC-P28-T009` — evidence pack (**IMPLEMENTED / AWAITING_ARCHITECT_REVIEW** · **READY_FOR_GATE**)
10. `TC-P28-GATE` — acceptance gate (**NOT EXECUTED**)

> Note: `TC-P28-T001` is reserved in roadmap numbering for first product task after PLAN acceptance; this plan uses T002+ following established P25/P26/P27 progression where PLAN equals T001 authoring.

### Decision-to-task mapping (authoritative progression)

| Decision | Primary task | Notes |
|----------|--------------|-------|
| `P28-R1` | `TC-P28-T003` | Measurement/profiling posture; Observability separation |
| `P28-R2` | `TC-P28-T005` | Data access / query posture per module schema |
| `P28-R3` | `TC-P28-T005` | Read optimization / justified Dapper posture |
| `P28-R4` | `TC-P28-T006` | Redis cache abstraction; cache != SoR |
| `P28-R8` | `TC-P28-T007` | Scaling/infrastructure boundary · deferred distributed complexity |
| `P28-R5` | `TC-P28-T008` | CDN/static delivery posture (with Media/P06 foundation) |
| `P28-R6` | `TC-P28-T008` | Frontend rendering/bundle/CWV hardening |
| `P28-R7` | `TC-P28-T008` | Search read performance boundary hardening |

### TC-P28-GATE — Acceptance gate

- Purpose: final P28 acceptance evidence only; verify PLAN + T002–T009 accepted and P28-R1–R8 RESOLVED.
- Delivered: `docs/plans/P28-GATE-acceptance-evidence.md` · gate evidence architecture lock test · SoT sync marking **P28 COMPLETE**.
- Forbidden in this task: new performance product beyond accepted boundaries · microservice extraction · Kafka/bus product · next phase (P29) execution.

### TC-P28-T009 — Evidence pack

- Purpose: adversarial architecture review evidence and gate-readiness documentation without new product capability.
- Delivered: `docs/plans/P28-T009-hardening-and-evidence-pack.md` · evidence-pack architecture lock test · SoT sync · **READY_FOR_GATE**.
- Forbidden in this task: production CDN vendor lock-in · Redis cluster product · load-test infrastructure beyond boundary · GATE execution.

### TC-P28-T008 — Operational hardening and deferred performance scope

- Purpose: consolidate operational readiness, performance risk boundaries, and deferred optimization catalog; resolve R5/R6/R7.
- Delivered: `PerformanceOperationalBoundary` · `PerformanceDeferredScopeBoundary` · hardening guardrail tests · **P28-R5/R6/R7 RESOLVED**.
- Forbidden in this task: production optimization · benchmark claims without evidence · Redis/cache/CDN product · infrastructure deployment · API/frontend.

### TC-P28-T007 — Scaling and infrastructure boundary

- Purpose: define horizontal scaling principles, stateless assumptions, and infrastructure responsibility without cloud/K8s/sharding product.
- Delivered: `PerformanceScalingBoundary` · `PerformanceInfrastructureBoundary` · guardrail tests · **P28-R8 RESOLVED**.
- Forbidden in this task: cloud lock-in · Kubernetes · infrastructure provisioning · Redis/CDN · database sharding · API/frontend.

### TC-P28-T006 — Caching boundary and cache policy architecture

- Purpose: define cache ownership, eligibility, invalidation, and consistency boundaries without Redis/cache provider product.
- Delivered: `PerformanceCacheBoundary` · `PerformanceCachePolicyBoundary` · guardrail tests · **P28-R4 RESOLVED**.
- Forbidden in this task: Redis client · cache provider · distributed cache deployment · cache-as-authority · API/frontend.

### TC-P28-T005 — Data access and read optimization boundary

- Purpose: define measurement-gated data access and evidence-based read optimization without Dapper product or ORM replacement.
- Delivered: `PerformanceDataAccessBoundary` · `PerformanceReadOptimizationBoundary` · guardrail tests · **P28-R2/R3 RESOLVED**.
- Forbidden in this task: Dapper implementation without evidence · ORM replacement · query tuning product · schema migration · Redis/cache · API/frontend.

### TC-P28-T004 — Runtime performance boundary and module interaction model

- Purpose: define measurement-driven runtime boundary and domain interaction contracts without infrastructure product.
- Delivered: `PerformanceRuntimeBoundary` · `PerformanceModuleInteractionBoundary` · guardrail tests.
- Forbidden in this task: runtime cache/CDN hooks · database tuning · cross-module performance hooks · API/frontend · business ownership transfer.

### TC-P28-T003 — Measurement/observability interaction boundary

- Purpose: define profile-before-optimize measurement foundation and Observability separation without optimization product.
- Delivered: `PerformanceMeasurementBoundary` · `PerformanceObservabilityInteractionBoundary` · guardrail tests · **P28-R1 RESOLVED**.
- Forbidden in this task: APM vendor lock-in · OpenTelemetry product · benchmark harness · production tuning automation · Redis/cache/CDN · API/frontend.

### TC-P28-T002 — Performance foundation boundary

- Purpose: establish Platform-owned performance/scale foundation markers without Redis/cache/CDN product or premature optimization.
- Delivered: `TravelCore.Performance` · `PerformanceFoundationBoundary` · `PerformanceOwnershipBoundary` · guardrail tests.
- Forbidden in this task: Redis client · cache policy · CDN integration · database migration · API/frontend · module ownership changes.

---

## 6. Scope (IN)

1. Authoritative P28 plan + SoT alignment (plan-driven tasks only until architect locks R1–R8).
2. Measurement/profiling posture before optimization.
3. Module-owned PostgreSQL query/index boundaries.
4. Justified Dapper read projection boundaries.
5. Redis cache abstraction (non-SoR) with invalidation posture.
6. CDN/static delivery and frontend CWV/bundle boundaries.
7. Search read performance boundary (not ranking engine).
8. Load-test posture and deferred distributed-scale guardrails.
9. Architecture tests proving performance boundaries do not break module ownership.
10. Evidence pack + GATE.

---

## 7. Out of scope (explicitly NOT in P28 plan-driven early tasks)

- Product code beyond declared boundary scaffolding (until respective task envelopes)
- Microservice extraction or service mesh
- Kafka/RabbitMQ/event-bus scale-out product
- Multi-region active-active deployment product
- Search ranking engine or ML relevance tuning
- Analytics warehouse/BI/streaming (P27 deferred scope)
- WebP/AVIF conversion pipeline (P06-R1 DEFER)
- Production CDN vendor hard-coding in Domain modules
- Next phase P29 Production Hardening

---

## 8. Deferred scope

- Microservice extraction
- Dedicated search engine (Elasticsearch/OpenSearch) unless explicitly locked later
- Global CDN edge compute / edge functions product
- Auto-scaling orchestration product beyond boundary contracts
- Real-time stream processing for performance
- Cache-as-authority anti-pattern remediation product (must remain forbidden)

---

## 9. Blockers / conflicts

| Item | Status |
|------|--------|
| P27 GATE acceptance | **RESOLVED** — `TC-P27-GATE` · baseline `fb55c0a` |
| Meaningful public/commerce surfaces | **PARTIAL** — sufficient for boundary phase; full load-test scale deferred |
| Redis in technology baseline | **LOCKED** — helper only; not SoR |
| Dapper everywhere anti-pattern | **LOCKED** — forbidden by technology baseline |
| Observability vs Analytics separation | **LOCKED** — must preserve P27 boundary |
| P06 WebP/AVIF defer | **LOCKED** — image optimization boundary must not reopen P06-R1 defer |

---

## 10. Architecture constraints (locked)

1. **Profile before optimize** — no optimization task without measurement posture.
2. Performance abstractions live in **Platform** or explicit boundary contracts — not scattered tuning in Domain modules.
3. **Cache != SoR** · **Redis != authoritative state**.
4. **EF owns writes/migrations** · **Dapper only for justified reads**.
5. Module schemas remain isolated — no performance-driven peer-schema FK shortcuts.
6. **Search read performance != Search ranking** — P15 ownership preserved.
7. One task → one writer; evidence-based acceptance; GATE adds no new capability.

---

## 11. Validation strategy (phase-level)

- Plan tasks: `git diff --check` + docs coherence only.
- Product tasks (future): `dotnet build TravelCore.sln` + Performance/Architecture/Integration tests relevant to task scope.
- GATE: full P28 validation battery + clean working tree.

---

## 12. Done-when (plan-driven tasks T001–T003)

- `TC-P28-T001`–`T003` establish the authoritative P28 execution map with R1–R8 OPEN inventory, decision-to-task mapping, and task briefs through GATE.
- `P28-GATE` closes the phase after R1–R8 are RESOLVED and T004–T009 are accepted.
