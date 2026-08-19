# P27 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P27-PLAN` |
| Phase | P27 — Analytics + Product Intelligence |
| Status | PLAN ACCEPTED · **P27 IN_PROGRESS** · T002 plan-driven SoT alignment executed |
| Baseline | `f1e6f09` (`docs: add P27 implementation plan`) |
| Authoritative sources | `docs/ROADMAP.md` § P27 · `docs/PROJECT-STATE.md` · `docs/architecture/04-module-boundaries.md` · `docs/architecture/05-dependency-rules.md` · `docs/architecture/06-cross-module-communication.md` · `docs/architecture/07-data-architecture.md` · `docs/domain/module-ownership-matrix.md` · `docs/architecture/15-future-architecture-transition-map.md` · `docs/pages/09-page-state-and-composition-rules.md` §19 · P15 Search · P19 Booking · P20 Payment · P21 HotelBooking · P25 Notification · P26 SEO |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

This document is the architecture plan for the Analytics + Product Intelligence phase.

> **Envelope note:** `TC-P27-PLAN` ACCEPTED · `TC-P27-T002` implemented (plan-driven SoT alignment) · **do not execute `TC-P27-T003` until architect accepts `T002`**.

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
| `P27-R1` | Analytics module ownership / schema posture vs domain modules / Observability | **OPEN** — independent Analytics module candidate · schema `analytics` · **Analytics != Booking** · **Analytics != Payment** · **Analytics != Search ranking SoR** · **Analytics != SEO** · **Analytics != Content editorial** · **Analytics != Notification delivery** · **Analytics != Observability telemetry SoR** · semantic event consumption only · no peer-schema FK |
| `P27-R2` | Product event taxonomy boundary | **OPEN** — canonical event names/kinds owned by Analytics contracts · publishers emit semantic facts only · roadmap events: SearchPerformed · SearchResultClicked · SearchNoResults · FilterApplied · TourViewed · HotelViewed · QuoteCreated · BookingStarted · BookingCompleted |
| `P27-R3` | Provider abstraction / dispatch boundary | **OPEN** — provider-neutral dispatch contracts · no named production analytics vendor in early tasks · zero-provider posture valid until explicit lock |
| `P27-R4` | Privacy / PII interaction boundary | **OPEN** — analytics must not become PII SoR · opaque resource/session references only · Booking/Party remain identity SoR |
| `P27-R5` | Consent / attribution interaction boundary | **OPEN** — analytics consent/attribution distinct from TripPlanner consent and Notification preferences · marketing vs product analytics separation preserved |
| `P27-R6` | Event ingestion / idempotency boundary | **OPEN** — downstream async ingestion · failed analytics must not fail domain transactions · idempotent event dispatch posture required |
| `P27-R7` | Public/admin operational boundary | **OPEN** — internal read/ops posture only until explicit product lock · no public analytics mutation/query API by default |
| `P27-R8` | Deferred/out-of-scope posture (warehouse, BI dashboards, ML, streaming) | **OPEN** — data warehouse · BI dashboards · ML recommendation · real-time streaming analytics · cross-vendor identity graph remain DEFERRED unless explicitly locked |

---

## 5. Execution sequence

Proposed sequence after plan acceptance:

1. `TC-P27-PLAN` — P27 architecture implementation plan (**ACCEPTED** · `f1e6f09`)
2. `TC-P27-T002` — plan-driven SoT alignment (**IMPLEMENTED / AWAITING_ARCHITECT_REVIEW**)
3. `TC-P27-T003` — plan decision inventory + execution sequence authoring (**NOT EXECUTED**)
4. `TC-P27-T004` — analytics module/schema foundation (**NOT EXECUTED**)
5. `TC-P27-T005` — product event taxonomy boundary (**NOT EXECUTED**)
6. `TC-P27-T006` — provider abstraction / dispatch boundary (**NOT EXECUTED**)
7. `TC-P27-T007` — event ingestion / publisher interaction boundary (**NOT EXECUTED**)
8. `TC-P27-T008` — hardening and guardrails (**NOT EXECUTED**)
9. `TC-P27-T009` — evidence pack (**NOT EXECUTED**)
10. `TC-P27-GATE` — acceptance gate (**NOT EXECUTED**)

> Note: `TC-P27-T001` is reserved in roadmap numbering for first product task after PLAN acceptance; this plan uses T002+ following established P25/P26 progression where PLAN equals T001 authoring.

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

## 12. Done-when (PLAN task)

- `docs/plans/P27-implementation-plan.md` exists with sections 0–12, R1–R8 OPEN inventory, execution sequence, IN/OUT/DEFERRED, blockers.
- `docs/plans/P27-PLAN-task-envelope.md` captures architect envelope reference.
- `docs/PROJECT-STATE.md` and `docs/ROADMAP.md` declare P27 PLAN authored / NOT_STARTED product execution.
- No product code, migration, API, frontend, or package dependency changes in PLAN task.
