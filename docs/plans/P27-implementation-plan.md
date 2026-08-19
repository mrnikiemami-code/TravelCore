# P27 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P27-PLAN` |
| Phase | P27 — Analytics + Product Intelligence |
| Status | PLAN ACCEPTED · **P27 IN_PROGRESS** · T006 provider abstraction boundary executed |
| Baseline | `59e50d0` (`feat(analytics): define T005 product event taxonomy boundary`) |
| Authoritative sources | `docs/ROADMAP.md` § P27 · `docs/PROJECT-STATE.md` · `docs/architecture/04-module-boundaries.md` · `docs/architecture/05-dependency-rules.md` · `docs/architecture/06-cross-module-communication.md` · `docs/architecture/07-data-architecture.md` · `docs/domain/module-ownership-matrix.md` · `docs/architecture/15-future-architecture-transition-map.md` · `docs/pages/09-page-state-and-composition-rules.md` §19 · P15 Search · P19 Booking · P20 Payment · P21 HotelBooking · P25 Notification · P26 SEO |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

This document is the architecture plan for the Analytics + Product Intelligence phase.

> **Envelope note:** `TC-P27-PLAN`–`T005` ACCEPTED · `TC-P27-T006` implemented (provider abstraction) · **do not execute `TC-P27-T007` until architect accepts `T006`**.

---

## 0. Next-phase resolve (from SoT)

| Question | Answer |
|----------|--------|
| Prior phase status | **P26 COMPLETE / ACCEPTED** (`TC-P26-GATE` `931ea19`) |
| Authoritative next phase | **P27 — Analytics + Product Intelligence** |
| Declared status before this plan | **PLANNED / NOT_STARTED** |
| Dedicated Analytics module/schema in SoT today? | **NO** — conceptual analytics intent exists in page archetypes only; no `analytics` schema/module |
| Analytics provider implemented? | **NO** — no provider abstraction or event pipeline in product code |
| Platform Observability exists? | **YES** — `TravelCore.Observability` platform infrastructure; **Observability != Product Analytics** |

---

## 1. Phase purpose

P27 introduces product analytics and intelligence boundaries **after** meaningful Search/Booking/Commerce surfaces exist, without scattering vendor-specific calls across domain modules or turning Analytics into a business-rule owner.

Business purpose (from SoT):

- Capture product-intent events such as SearchPerformed · SearchResultClicked · SearchNoResults · FilterApplied · TourViewed · HotelViewed · QuoteCreated · BookingStarted · BookingCompleted
- Enable future product intelligence while preserving modular ownership
- Keep analytics behind a reasonable abstraction — **no provider-specific calls scattered in Domain**

Architecture objective:

- Introduce **Analytics** as the downstream owner of event taxonomy, dispatch abstraction, and ingestion posture
- Preserve **Search/Booking/Payment/Tour/HotelBooking/SEO/Content/Destination** as fact publishers only
- Preserve **Platform Observability** (metrics/tracing/logging) as infrastructure separate from product analytics SoR
- No warehouse/BI/dashboard product in early boundary tasks

---

## 2. Preserved locked architecture

P27 must preserve:

1. Modular Monolith — schema-per-module; no peer-schema FK; no shared DbContext.
2. **Analytics owns event capture/dispatch mechanics**; business modules publish semantic product events only.
3. **Domain modules must not call Mixpanel/GA/Amplitude/etc. directly** in Domain or Infrastructure.
4. **Analytics != Booking/Payment/Search/SEO/Content/Destination/Notification** execution or editorial SoR.
5. **Observability != Product Analytics** — platform telemetry remains in Observability; product events in Analytics.
6. **PII minimization** — analytics references use opaque ids/contracts; Booking/Party remain PII SoR.
7. P21–P26 ownership boundaries remain unchanged.
8. Failed analytics dispatch must not rollback committed domain transactions.

---

## 3. Current SoT baseline snapshot

- P15 Search boundary exists; search events are conceptual in page docs, not implemented as analytics pipeline.
- P19 Booking · P20 Payment · P21 HotelBooking · P22 Flight · P23 DynamicPackage provide commerce facts; no analytics module consumes them yet.
- P25 Notification downstream posture exists; **Notification != Analytics**.
- P26 SEO/content graph boundaries complete; graph existence != analytics success.
- Page archetypes declare **conceptual analytics intent only** — not vendor taxonomy (`docs/pages/09-page-state-and-composition-rules.md` §19).
- Transition map has no dedicated Analytics section yet; ROADMAP P27 is authoritative for phase title and themes.

---

## 4. Decision inventory for P27 (open for architect locks)

| ID | Topic | Status |
|----|-------|--------|
| `P27-R1` | Analytics module ownership / schema posture vs domain modules / Observability | **RESOLVED** — independent Analytics module · schema `analytics` · **Analytics != Booking/Payment/Search/SEO/Content/Notification/Observability** · semantic consumption only · no peer-schema FK · T004 foundation only |
| `P27-R2` | Product event taxonomy boundary | **RESOLVED** — canonical event kinds owned by Analytics contracts · roadmap events covered · publishers emit semantic facts only · no vendor taxonomy · no event persistence in T005 |
| `P27-R3` | Provider abstraction / dispatch boundary | **RESOLVED** — provider-neutral dispatch contracts · no named production analytics vendor · zero-provider posture valid · T006 port only |
| `P27-R4` | Privacy / PII interaction boundary | **OPEN** — analytics must not become PII SoR · opaque resource/session references only · Booking/Party remain identity SoR |
| `P27-R5` | Consent / attribution interaction boundary | **OPEN** — analytics consent/attribution distinct from TripPlanner consent and Notification preferences · marketing vs product analytics separation preserved |
| `P27-R6` | Event ingestion / idempotency boundary | **OPEN** — downstream async ingestion · failed analytics must not fail domain transactions · idempotent event dispatch posture required |
| `P27-R7` | Public/admin operational boundary | **OPEN** — internal read/ops posture only until explicit product lock · no public analytics mutation/query API by default |
| `P27-R8` | Deferred/out-of-scope posture (warehouse, BI dashboards, ML, streaming) | **OPEN** — data warehouse · BI dashboards · ML recommendation · real-time streaming analytics · cross-vendor identity graph remain DEFERRED unless explicitly locked |

---

## 5. Execution sequence

Proposed sequence after plan acceptance:

1. `TC-P27-PLAN` — P27 architecture implementation plan (**ACCEPTED** · `f1e6f09`)
2. `TC-P27-T002` — plan-driven SoT alignment (**IMPLEMENTED / ACCEPTED**)
3. `TC-P27-T003` — plan decision inventory + execution sequence authoring (**IMPLEMENTED / ACCEPTED**)
4. `TC-P27-T004` — analytics module/schema foundation (**IMPLEMENTED / ACCEPTED**)
5. `TC-P27-T005` — product event taxonomy boundary (**IMPLEMENTED / ACCEPTED**)
6. `TC-P27-T006` — provider abstraction / dispatch boundary (**IMPLEMENTED / AWAITING_ARCHITECT_REVIEW**)
7. `TC-P27-T007` — event ingestion / publisher interaction boundary (**NOT EXECUTED**)
8. `TC-P27-T008` — hardening and guardrails (**NOT EXECUTED**)
9. `TC-P27-T009` — evidence pack (**NOT EXECUTED**)
10. `TC-P27-GATE` — acceptance gate (**NOT EXECUTED**)

> Note: `TC-P27-T001` is reserved in roadmap numbering for first product task after PLAN acceptance; this plan uses T002+ following established P25/P26 progression where PLAN equals T001 authoring.

### Decision-to-task mapping (authoritative progression)

| Decision | Primary task | Notes |
|----------|--------------|-------|
| `P27-R1` | `TC-P27-T004` | Independent Analytics module + schema `analytics`; Observability separation |
| `P27-R2` | `TC-P27-T005` | Canonical product event taxonomy boundary |
| `P27-R3` | `TC-P27-T006` | Provider-neutral dispatch abstraction |
| `P27-R4` | `TC-P27-T007` | Privacy/PII minimization + publisher reference semantics |
| `P27-R5` | `TC-P27-T008` | Consent/attribution interaction hardening vs TripPlanner/Notification |
| `P27-R6` | `TC-P27-T007` | Event ingestion/idempotency + non-blocking dispatch posture |
| `P27-R7` | `TC-P27-T008` | Public/admin operational boundary hardening |
| `P27-R8` | `TC-P27-T008` | Deferred/out-of-scope posture (warehouse, BI, ML, streaming) |

### TC-P27-GATE — Acceptance gate

- Purpose: final P27 acceptance evidence only; verify PLAN + T001–T009 accepted and P27-R1–R8 RESOLVED.
- Delivered: `docs/plans/P27-GATE-acceptance-evidence.md` · gate evidence architecture lock test · SoT sync marking **P27 COMPLETE**.
- Forbidden in this task: new Analytics capability · named production vendors · public analytics API/UI · warehouse/BI product · next phase (P28) execution.

### TC-P27-T009 — Evidence pack

- Purpose: adversarial architecture review evidence and gate-readiness documentation without new product capability.
- Delivered: `docs/plans/P27-T009-hardening-and-evidence-pack.md` · evidence-pack architecture lock test · SoT sync · **READY_FOR_GATE**.
- Forbidden in this task: named production vendors · warehouse/BI dashboards · ML recommendation · public analytics API/UI · Search/Booking/Payment ownership changes · GATE execution.

### TC-P27-T008 — Hardening and guardrails

- Purpose: consolidate accepted Analytics boundaries; resolve R5/R7/R8 posture; forbid deferred/public-ops product types.
- Delivered: consent/attribution interaction boundary · operational boundary · deferred-scope boundary · hardening guardrail tests.
- Forbidden in this task: consent persistence product · public analytics query/mutation API · warehouse connectors · evidence pack (T009) · GATE.

### TC-P27-T007 — Event ingestion / publisher interaction boundary

- Purpose: define Analytics-owned ingestion/idempotency posture and publisher interaction contracts without domain transaction coupling or PII SoR takeover.
- Delivered: semantic publisher boundary · idempotency boundary · non-blocking dispatch posture · opaque reference semantics · guardrail tests.
- Forbidden in this task: outbox consumer runtime at scale · event persistence tables beyond T004 foundation · provider execution · public API/UI · migrations beyond T004 schema foundation.

### TC-P27-T006 — Provider abstraction / dispatch boundary

- Purpose: define Analytics-owned provider-neutral dispatch contracts without named production adapters or vendor SDK in domain modules.
- Delivered: `AnalyticsProviderKey` · `AnalyticsProviderCapability` · `AnalyticsDispatchRequest`/`AnalyticsDispatchResult` · `IAnalyticsDispatchProvider` · `IAnalyticsProviderResolver` · `AnalyticsProviderTrustBoundary` · `AnalyticsProviderBoundary` · guardrail tests.
- Forbidden in this task: named production adapters (Mixpanel/GA4/Amplitude/Segment) · vendor SDK in Search/Booking modules · event taxonomy (T005) · ingestion runtime (T007) · API/frontend · migrations beyond T004 schema foundation.

### TC-P27-T005 — Product event taxonomy boundary

- Purpose: define Analytics-owned canonical product event kinds aligned with ROADMAP intent without Search ranking or domain workflow SoR takeover.
- Delivered: `AnalyticsProductEventKind` · `AnalyticsEventTaxonomyBoundary` · `AnalyticsEventReference` · `AnalyticsSemanticEventEnvelope` · guardrail tests · roadmap event coverage (SearchPerformed · SearchResultClicked · SearchNoResults · FilterApplied · TourViewed · HotelViewed · QuoteCreated · BookingStarted · BookingCompleted).
- Forbidden in this task: provider dispatch execution · event persistence beyond T004 foundation · Search ranking changes · public API/UI · migrations beyond T004 schema foundation.

### TC-P27-T004 — Analytics module/schema foundation

- Purpose: introduce independent Analytics module scaffolding with schema `analytics` only; preserve Observability != Product Analytics separation.
- Delivered: Contracts/Domain/Infrastructure · `AnalyticsOwnershipBoundary` · `AnalyticsPublisherBoundary` · `AnalyticsDbContext` · host registration · EnsureSchema migration · guardrail tests proving Analytics/Search/Booking/Payment/Observability separation.
- Forbidden in this task: vendor SDK · event taxonomy tables beyond foundation · provider adapters · public API/UI · peer-schema FK · shared DbContext.

### TC-P27-T003 — Plan decision inventory + execution sequence

- Purpose: expand the approved P27 plan from T002-aligned baseline into an executable decision inventory, decision-to-task mapping, and per-task briefs without adding product code.
- Delivered: decision-to-task mapping · task briefs T004–T009 + GATE · execution sequence updated · envelope note updated.
- Forbidden in this task: module code · schema/migration · API · frontend · analytics tables · product tests beyond docs validation.

---

## 6. Scope (IN)

1. Authoritative P27 plan + SoT alignment (plan-driven tasks only until architect locks R1–R8).
2. Analytics-owned event taxonomy and dispatch scaffolding (contracts/domain/infrastructure within Analytics module boundaries).
3. Provider-neutral dispatch abstraction without vendor SDK in domain modules.
4. Privacy/consent/idempotency guardrails without warehouse/BI product.
5. Architecture tests proving Analytics/Search/Booking/Payment/Observability separation.
6. Evidence pack + GATE.

---

## 7. Out of scope (explicitly NOT in P27 plan-driven early tasks)

- Product code beyond declared boundary scaffolding (until respective task envelopes)
- Named production analytics vendors (Mixpanel · GA4 · Amplitude · Segment runtime)
- Data warehouse / BI dashboard / ML recommendation product
- Real-time streaming pipeline / Kafka / RabbitMQ product analytics bus
- Search ranking engine changes
- SEO/Content/Destination ownership changes
- Notification delivery changes
- Frontend analytics SDK integration (unless explicitly locked later)
- Next phase P28 Performance

---

## 8. Deferred scope

- Full analytics warehouse and BI tooling
- Cross-device identity graph / attribution modeling at scale
- ML-based product intelligence / personalization engines
- Real-time streaming analytics operations
- Microservice extraction

---

## 9. Blockers / conflicts

| Item | Status |
|------|--------|
| P26 GATE acceptance | **RESOLVED** — `TC-P26-GATE` · baseline `931ea19` |
| Meaningful Search/Booking/Commerce surfaces | **PARTIAL** — sufficient for boundary phase; full scale analytics deferred |
| Observability vs Analytics separation | **LOCKED** — must preserve |
| Provider-specific calls in Domain | **LOCKED** — forbidden by ROADMAP P27 theme |

---

## 10. Architecture constraints (locked)

1. Create/extend **Analytics** module; do not embed analytics dispatch in Search/Booking/Payment/Tour/SEO modules.
2. Events reference publishable resources by semantic contracts (event kind + opaque ids), not peer-schema FK.
3. Domain modules publish facts/intent; Analytics owns taxonomy and dispatch orchestration only.
4. Platform Observability remains infrastructure telemetry; product analytics events remain Analytics-owned.
5. One task → one writer; evidence-based acceptance; GATE adds no new capability.

---

## 11. Validation strategy (phase-level)

- Plan tasks: `git diff --check` + docs coherence only.
- Product tasks (future): `dotnet build TravelCore.sln` + Analytics/Architecture/Integration tests relevant to task scope.
- GATE: full Analytics validation battery + clean working tree.

---

## 12. Done-when (plan-driven tasks T001–T003)

- `TC-P27-T001`–`T003` establish the authoritative P27 execution map with R1–R8 OPEN inventory, decision-to-task mapping, and task briefs through GATE.
- `P27-GATE` closes the phase after R1–R8 are RESOLVED and T004–T009 are accepted.
