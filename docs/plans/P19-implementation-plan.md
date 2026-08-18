# P19 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P19-PLAN` |
| Phase | P19 — Tour Booking |
| Status | PLAN ACCEPTED; **P19-R1–R4 RESOLVED**; P19-R5–R8 OPEN; T004 booker/passengers |
| Baseline | `73605aa` (`docs(tripplanner): add P18 acceptance gate evidence [TC-P18-GATE]`) |
| Authoritative sources | `docs/ROADMAP.md` § P19 · `docs/PROJECT-STATE.md` · `04-module-boundaries.md` · `05-dependency-rules.md` · `07-data-architecture.md` (schema `booking`) · `docs/domain/module-ownership-matrix.md` · `15-future-architecture-transition-map.md` § R Booking / § S Payment · ADR 0003 (Money) · ADR 0004 (NodaTime) · P09 Tour · P11 TourDeparture (R3 capacity definition · R7 passenger rules · R8 Published ≠ Bookable) · P12 Pricing (R3–R8 Quote/occupancy/public price) · P13 AgencyMarketplace · P14 PublicExperience (R2 Sticky Action ≠ Booking) · P15 Search · P17 Visa (R8 VisaApplication ≠ Booking) · P18 TripPlanner (Lead ≠ Booking) · P20 Payment (PLANNED) |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P19** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی و موجودی تصمیم را برای Cursor تعریف می‌کند.

> **Envelope note:** Authored from **repository SoT** after architect `TC-P18-GATE` ACCEPT (`73605aa`). Next phase is **explicitly** `P19 — Tour Booking` in `docs/ROADMAP.md` (not guessed). Under PIPELINE continuity, ceremonial confirms are **not required**. **No product code in PLAN task.** Open R# must stay OPEN until architect lock. **Do not implement T001 until this PLAN is ACCEPTED and P19-R1 is architecturally locked.**

---

## 0. Next-phase resolve (from SoT; no extra discovery task)

| Question | Answer from SoT |
|----------|-----------------|
| P18 completion | **COMPLETE / ACCEPTED** — Gate `73605aa` |
| Authoritative next phase ID | **P19** |
| Title / purpose | **Tour Booking** — traveler information · availability validation · Quote acceptance · reservation · Booking · status · price snapshot · cancellation foundation · confirmation (`docs/ROADMAP.md` § P19) |
| PLAN already existed? | **NO** — this document is the first P19 PLAN |
| SoT conflict? | **NO** — ROADMAP names P19 after P18; constitution already names Booking as a Commerce module; schema `booking` is listed in `07-data-architecture.md`. Capability themes in ROADMAP are **not** ownership transfers to Payment, Pricing, Tour catalog, Flight, or HotelBooking. |
| Dedicated module/schema in SoT today? | **YES (conceptual)** — Booking module exists in `04-module-boundaries.md` and ownership matrix; PostgreSQL schema `booking` is listed. **No Booking product code / DbContext / aggregates exist in the repository yet.** |
| Payment module today? | **NO product** — Payment is P20; constitution exists; do not implement Payment in P19. |
| Missing business fact blocking PLAN authorship? | **NO** — PLAN may enumerate R# and IN/OUT/DEFER without locking product semantics |
| Invented phase? | **NO** — P19 is already listed in ROADMAP |

---

## 1. Phase Purpose

P19 باید قابلیت **Tour Booking** را به‌عنوان دامنهٔ تراکنشی مستقل معرفی کند: رزرو اجرایی یک **TourDeparture** مشخص — **بدون** دزدیدن مالکیت Tour catalog، TourDeparture capacity *definition*، Pricing/Quote engine، Payment execution، Party/Identity master، AgencyMarketplace commercial allocation، VisaApplication، TripPlanner Lead، Search, SEO, or Notification delivery.

هدف (از Roadmap + accepted prior phases):

1. **Booking ≠ TourProduct ≠ TourDeparture** — Tour owns catalog + departure facts; Booking owns reservation/order state (`04-module-boundaries.md`, P09/P11).
2. **Booking ≠ Price ≠ Quote ≠ Payment** — Pricing owns Price/Quote; Booking owns accepted historical commercial snapshot; Payment owns money movement (P12, P20, ADR 0003).
3. **Capacity Definition ≠ Capacity Consumption ≠ Availability Projection** — P11-R3: TourDeparture owns Min/Max Pax **definition**; Booking later owns **consumption**. Exact hold/confirm/release semantics are **P19-R3 (OPEN)**.
4. **PlannerTravelerComposition ≠ BookingPassenger** — P18 traveler counts are preferences; P19 may collect actual traveler facts (P19-R4 OPEN).
5. **Lead ≠ Booking** — P18 Lead is follow-up request; conversion requires an explicit command (P19-R7 OPEN). Do not auto-convert.
6. **VisaApplication ≠ Booking** — P17-R8: visa case workflow is not a Booking. Booking must not auto-create visa applications.
7. **BookingStatus ≠ PaymentStatus ≠ TourDepartureStatus ≠ CatalogStatus ≠ SEO IndexPolicy**.
8. **PublicExperience = composition only** — checkout/booking UI is presentation; Booking remains transactional SoT. P14-R2 currently forbids fake Book Now; honest Book CTA is allowed only after Booking capability exists **and** P19-R8 locks it.
9. **Notification = delivery owner** — Booking may emit semantic events; it does not own SMTP/SMS/WhatsApp providers (transition map § V; still unimplemented).
10. **AI-readiness = structured transactional facts** — identifiers, status, snapshots, timestamps — **بدون** LLM booking agent / embeddings / vector / RAG / autonomous modification.

P18 تحویل داد: Trip Planner / Lead (anonymous-first intent + submitted Lead + `/plan`). Agency routing DEFERRED. Notification provider DEFERRED.

P19 اضافه می‌کند: **Tour Booking capability** — **بدون** Payment provider، بدون Flight/Hotel live inventory، بدون CRM، بدون generic workflow engine، بدون Search indexing of private bookings.

---

## 2. Starting Baseline

| Item | Value |
|------|--------|
| P18 Gate | `TC-P18-GATE` COMPLETE / ACCEPTED (`73605aa`) |
| P18 evidence | [`P18-GATE-acceptance-evidence.md`](P18-GATE-acceptance-evidence.md) · [`P18-T009-hardening-and-evidence-pack.md`](P18-T009-hardening-and-evidence-pack.md) |
| P18 Plan | ACCEPTED · R1–R8 RESOLVED · T001–T009 ACCEPTED |
| Baseline HEAD | `73605aa` |
| P00–P18 | COMPLETE |
| Booking module / schema | **Conceptual only** (`04-module-boundaries.md` · `07-data-architecture.md` schema `booking`) — no product code |
| Payment module | **Not implemented** (P20) |
| Notification module | **Not implemented** (architecture docs only) |
| TourDeparture | **P11 COMPLETE** — schedule, Min/Max Pax **definition**, passenger occupancy **rules**, Published ≠ Bookable |
| Pricing | **P12 COMPLETE** — Price on logical TourDeparture target; Quote = Pricing calculation snapshot; public price summary; no Booking/Payment |
| Public Tour UX | **P14 COMPLETE** — Sticky Action ≠ Booking; no Book Now / Checkout |
| Existing Booking APIs | **NONE** — architecture tests currently forbid `/api/booking` on sticky chrome |

---

## 3. Scope classification (IN / OUT / DEFER)

Classifications below are **planning inventory**, not architect locks. Dependent product tasks must not treat IN as permission to invent unlocked R# closures.

| Concept | Classification | Notes |
|---------|----------------|-------|
| Independent Booking module + schema `booking` (SoT candidate) | **IN (candidate)** | Constitution + data-architecture; exact scaffolding locked by **P19-R1** |
| TourDeparture as initial booking target | **IN (candidate)** | P12-R3 buyable target is TourDeparture; **P19-R1** must confirm |
| Booking aggregate + lifecycle | **IN (candidate)** | **P19-R2** |
| Capacity consumption / hold / expiry / concurrency | **IN (candidate)** | Definition stays TourDeparture (P11-R3); consumption **P19-R3** |
| Booker / BookingPassenger / contact snapshot / PII minimization | **IN (candidate)** | **P19-R4** — no passport upload by default |
| Quote consumption + historical monetary snapshot | **IN (candidate)** | Quote remains Pricing-owned; snapshot **P19-R5** |
| Confirmation / cancellation foundation + Payment *interaction contracts* | **IN (candidate)** | Payment execution **P20**; orchestration boundary **P19-R6** |
| Direct vs agency booking boundary | **IN (candidate as boundary)** | Commission/settlement **OUT**; **P19-R7** |
| Public booking composition + authorization + private reads | **IN (candidate)** | PE composes; Booking SoT; **P19-R8** |
| Customer booking detail / list (authorized) | **IN (candidate)** | Not SEO-indexed |
| Ops/admin booking read (minimal) | **IN (candidate)** | Not a BI platform |
| Idempotent public write commands | **IN (candidate)** | Create/reserve/confirm/cancel — exact keys **OPEN** |
| Transactional outbox in `booking` schema | **IN (candidate)** | Per-module outbox (`07-data-architecture.md`); no second event bus |
| Payment provider / attempts / webhooks / refunds execution | **OUT** | P20 |
| Price/Quote engine / occupancy price calculation | **OUT** | P12 remains owner |
| TourProduct / TourDeparture catalog mutation | **OUT** | Tour remains owner |
| Live Flight booking | **OUT** | P22 |
| HotelBooking live inventory | **OUT** | P21 · Hotel Catalog ≠ HotelBooking |
| VisaApplication / applicant case | **OUT** | P17-R8 deferred outside P17; not P19 |
| TripPlanner Lead engine / CRM pipeline | **OUT** | P18 remains Lead SoT |
| Search indexing of Booking/passenger facts | **OUT** | Booking is not Search content |
| SEO-indexed booking pages | **OUT** | SEO owns IndexPolicy; private transaction pages must not be product landings |
| Notification provider (SMTP/SMS/push) | **OUT / DEFER** | Delivery owner unimplemented |
| Agency settlement / commission engine | **OUT** | Not Booking owner in constitution |
| Generic cart / multi-item checkout | **OUT / DEFER** | P19 is TourDeparture booking, not a mall cart |
| Complex amendment / rebooking / waitlist / overbooking policy | **DEFER** unless R# later locks a minimal slice |
| Loyalty / promo engine / insurance workflow | **OUT** | |
| LLM booking agent / embeddings / RAG | **OUT** | |
| Distributed DB transaction Booking+Payment+Notification | **OUT** | Forbidden by `05-dependency-rules.md` |

---

## 4. Explicit non-goals

P19 must not silently become:

- Payment
- Pricing / Quote engine
- Tour catalog / TourDeparture definition authority
- Flight booking
- Hotel booking
- Visa application
- CRM / sales pipeline
- generic workflow / BPM engine
- Search
- SEO
- Notification provider
- Inventory module for generic SKU stock (Tour capacity ≠ product stock)

---

## 5. Locked facts from prior phases (carry forward; not reopened)

These are **already accepted**. PLAN does not re-decide them.

| Fact | Source |
|------|--------|
| Booking is an independent Commerce module | `04-module-boundaries.md` · ownership matrix |
| Schema name `booking` listed | `07-data-architecture.md` |
| Booking may hold TourProductId · TourDepartureId · QuoteSnapshot · traveler snapshot **without** owning foreign aggregates | `05-dependency-rules.md` |
| Booking may depend on Party, Tour **contracts**, Pricing Quote **contracts/snapshots** | `05-dependency-rules.md` |
| No BookingDbContext + PaymentDbContext + NotificationDbContext in one optional transaction | `05-dependency-rules.md` |
| Price ≠ Quote ≠ Booking ≠ Payment | P12 · `07-data-architecture.md` |
| Accepted historical commercial facts live in Booking snapshot; live Price changes do not rewrite history | ownership matrix · transition map § R |
| Payment does not mutate Booking DbContext; Payment success is an event Booking may react to | `04-module-boundaries.md` Payment |
| TourDeparture owns Min/Max Pax **capacity definition**; Booking owns **consumption later** | **P11-R3** |
| TourDeparturePassengerRule exists; Booking owns **actual travellers later** | **P11-R7** |
| TourDepartureStatus Published ≠ Bookable (no Booking invented in P11) | **P11-R8** |
| Buyable Price initial target = logical TourDeparture | **P12-R3** |
| Quote owned by Pricing; calculation snapshot; no Booking ownership in P12 | **P12-R4** |
| Pricing owns occupancy *categories*; no Booking passenger entity in P12 | **P12-R5** |
| Sticky Action ≠ Booking; Book Now / Pay Now / Reserve Seat / Checkout forbidden while capability absent | **P14-R2** |
| Lead ≠ Booking · PlannerTravelerComposition ≠ BookingPassenger | **P18-R2 / P18-R4** |
| Visa ≠ VisaApplication · VisaApplication ≠ Booking | **P17-R8** |
| Money = Amount + CurrencyCode; IRR canonical; Toman display-only | ADR 0003 |
| Temporal model = NodaTime + IANA; do not store local departure dates as UTC instants | ADR 0004 · P11-R2 |
| No peer-schema FK; logical ids only | constitution |
| Per-module outbox inside owner schema | `07-data-architecture.md` |

---

## 6. Investigation map (questions for R#; do not invent answers)

### 6.1 Booking target (feeds P19-R1)

Evaluate whether a Tour Booking **must** reference:

- TourDeparture (expected candidate; P12-R3 / P11)
- TourProduct (catalog context vs executable target)
- Pricing Quote
- AgencyOffer
- direct supplier/agency context

Do not guess. Do not clone TourDeparture as a mutable catalog inside Booking.

### 6.2 Capacity / hold / concurrency (P19-R3 RESOLVED)

Locked: Tour owns capacity **definition**; Booking owns **consumption**. Temporary `CapacityHold` is required. Hold lifecycle: Active / Consumed / Released / Expired. **CapacityHoldStatus != BookingStatus**. **Pending != CapacityHeld**. **Consumed != BookingConfirmed**. **Expired Hold != Expired Booking**. Explicit `ExpiresAt` (no hardcoded timeout). Overbooking prevention is atomic, server-side, database-backed. Process-local locking is not correctness. Idempotent hold required. A Booking row may exist without an Active hold. Confirmation orchestration remains R6. Availability Projection owner remains outside this lock. Frontend availability is never the correctness boundary.

### 6.3 Lifecycle (P19-R2 RESOLVED; still feeds P19-R6)

Locked BookingStatus set: **Pending** · **Confirmed** · **Cancelled**.

- **Pending** — aggregate exists; does **not** imply capacity held, payment pending, or quote valid.
- **Confirmed** — final successful Booking state. **Confirmed != PaymentSucceeded**. **Confirmed != CapacityHeld**. Unrestricted `Confirm()` is **not** implemented (preconditions remain R3/R5/R6).
- **Cancelled** — no longer active. **Cancelled != Refunded**. **Cancelled != CapacityReleased**.

Allowed: Create → Pending; Pending → Cancelled (`CancelPending`). Forbidden: generic `SetStatus()`; Confirmed → Cancelled (R6); Cancelled → Pending; extra statuses (Expired, AwaitingPayment, Paid, Refunded, Held, Reserved).

Confirmation facts still deferred to R3/R5/R6: capacity secured · valid commercial snapshot · required passenger data · payment success **if required** · departure still bookable. Avoid generic workflow infrastructure.

### 6.4 Identity / passengers / PII (feeds P19-R4)

Evaluate:

- Booker ≠ Identity Account entity · Booker ≠ Party master clone
- BookingPassenger ≠ Party Person Master · transaction-time facts survive later Party edits
- Adult/Child/Infant vs name / birth date / gender / nationality / passport
- **Do not assume passport/document upload**
- anonymous vs authenticated booking (do **not** inherit P18 anonymous-first automatically)
- BookingContactSnapshot ≠ Party
- logging redaction · API exposure · retention · authorization

### 6.5 Pricing / Quote (feeds P19-R5)

Evaluate:

- Must Booking require a valid Quote?
- Snapshot of Quote values vs live Price
- Quote expiry before confirmation
- taxes/fees/components preservation
- QuoteId logical only?
- which monetary data becomes authoritative **after** confirmation

Reuse ADR 0003. Do not invent alternate amount types.

### 6.6 Payment interaction (feeds P19-R6) — no Payment product in P19

Evaluate:

- payment required vs optional for confirmation
- initiation / success / failure events
- refunds after cancellation (request vs execution)
- recovery: payment succeeded / confirmation delayed; hold without payment; duplicate success; redelivery

Payment remains P20. P19 may define **contracts/events only**.

### 6.7 Agency / Lead / Visa / PE (feeds P19-R7 / P19-R8)

Evaluate:

- AgencyOffer reference optional? Booking owner remains Booking (expected)
- direct vs agency-mediated vs both; **no settlement/commission engine**
- Lead → Booking conversion only via explicit command
- Visa requirements may be **surfaced** logically; no VisaApplication create
- Checkout UI vs Booking API vs Payment ownership
- whether unfinished draft is persisted
- object-level authorization; no public listing of arbitrary customer bookings
- booking routes must not be SEO product surfaces

---

## 7. Task sequence (proposed)

Do **not** execute any product task until PLAN ACCEPT **and** the matching R# is locked.

### TC-P19-PLAN — this document

### TC-P19-T001 — Booking module scaffolding / ownership / target boundary

- Purpose: Independent Booking module + ownership contracts (**P19-R1 RESOLVED**).
- Delivered: Contracts/Domain/Infrastructure scaffolding; schema `booking`; `BookingOwnershipBoundary`; opaque `TourDepartureReference`; host registration; no peer FKs; no Booking aggregate/lifecycle/hold/passenger/payment/public types.
- Forbidden kept: Booking product tables · Payment · Pricing mutation · TourDeparture clone · Agency settlement · public checkout · inventing R2–R8.

### TC-P19-T002 — Booking aggregate + lifecycle boundary

- Purpose: Booking aggregate vs status machine (**P19-R2 RESOLVED**).
- Delivered: independent `Booking` aggregate targeting one logical `TourDeparture`; statuses Pending/Confirmed/Cancelled; table `bookings`; UUIDv7 `BookingId`; Create → Pending; `CancelPending` only. No unrestricted Confirm, no Confirmed → Cancelled, no passengers/quote/payment/agency fields.
- Preserve: **BookingStatus ≠ PaymentStatus ≠ TourDepartureStatus**. **Confirmed != PaymentSucceeded**. **Cancelled != Refunded**. No generic BPM. No Payment execution.

### TC-P19-T003 — Capacity consumption / hold / concurrency

- Purpose: Booking-owned capacity consumption (**P19-R3 RESOLVED**) without stealing TourDeparture definition (P11-R3).
- Delivered: `CapacityHold` (Active/Consumed/Released/Expired); `DepartureCapacityAccount` per logical TourDeparture; explicit `ExpiresAt`; PostgreSQL advisory-lock concurrency; idempotent hold; release/expire free capacity once; consume remains counted. No Booking confirmation. No public hold API.
- Preserve: **CapacityDefinition != CapacityConsumption**. **CapacityHoldStatus != BookingStatus**. **Pending != CapacityHeld**. **Consumed != BookingConfirmed**. **Expired Hold != Expired Booking**. **HeldSeatCount != BookingPassenger**. Tour remains definition owner. No process-local lock as correctness.

### TC-P19-T004 — Booker / passengers / contact / PII

- Purpose: Transaction-time people facts (**P19-R4 RESOLVED**).
- Delivered: `BookingContactSnapshot`; optional logical `BookingActorReference` / `BookingPartyReference`; `BookingPassenger` child with GivenName/FamilyName/`TravelerCategory`; Unicode names; PassengerCount <= Active hold SeatCount; BirthDate/passport/upload omitted.
- Preserve: **PlannerTravelerComposition != BookingPassenger** · **BookingPassenger != Party Person Master** · **BookingContactSnapshot != Party** · **BookingContactSnapshot != Identity Account**. No passport upload. No Party clone.

### TC-P19-T005 — Quote consumption / monetary snapshot

- Purpose: Historical commercial facts inside Booking (**P19-R5**).
- Preserve: **Price ≠ Quote ≠ Booking amount ≠ Payment**. Quote engine stays Pricing. Live Price must not rewrite accepted Booking snapshot.

### TC-P19-T006 — Confirmation / cancellation / Payment interaction contracts

- Purpose: Confirmation and cancellation ownership; Payment **orchestration boundary** (**P19-R6**).
- Preserve: **Booking ≠ Payment**. Payment provider **OUT** (P20). T006 may be **contracts/docs-only** if Payment remains unimplemented — same pattern as P18-T006 DEFER. Distinguish Booking cancellation ≠ TourDeparture cancellation ≠ Payment refund.

### TC-P19-T007 — Agency / Lead / Visa / external module boundaries

- Purpose: Direct vs agency; Lead conversion; Visa non-ownership (**P19-R7**).
- Preserve: **Lead ≠ Booking** · **VisaApplication ≠ Booking** · Booking ≠ AgencyMarketplace ranking/settlement. No commission engine.

### TC-P19-T008 — Public booking experience / authorization / privacy

- Purpose: PE composition + authorized reads + honest CTA (**P19-R8**).
- Preserve: **PublicExperience ≠ Booking SoT**. P14-R2 remains until honest Book capability exists. No SEO-indexed PII pages. No public Lead-style listing of bookings. T008 may be VACANT only if R8 has no independent implementation after T001–T007.

### TC-P19-T009 — Hardening + evidence

- Purpose: Guardrails + evidence pack; **no new capability**. Does **not** execute GATE.

### TC-P19-GATE — Acceptance Gate

- Evidence only. Ceremonial Gate wait is **not** a pipeline stop.
- No new Booking product in GATE. Do not start P20 inside GATE.

Do not manufacture empty capabilities merely to fill numbering. T006 may remain boundary/docs-only if Payment interaction stays contract-only. T008 may remain VACANT only if R8 has no independent product slice.

---

## 8. Open decisions (must not invent)

| ID | Topic | Status | SoT notes (not a lock) |
|----|-------|--------|------------------------|
| **P19-R1** | Booking module ownership / schema / initial target | **RESOLVED** | Independent Booking module. Schema `booking`. Initial logical target = TourDeparture. Tour owns TourProduct, TourDeparture, capacity **definition**. Booking will own capacity **consumption** (implementation deferred to R3). No peer-schema FK. T001: no Booking aggregate, lifecycle, hold, passenger, pricing, payment, or public surface. |
| **P19-R2** | Booking lifecycle and aggregate boundary | **RESOLVED** | Independent `Booking` aggregate targets exactly one logical TourDeparture. Statuses: Pending, Confirmed, Cancelled. Pending does not imply capacity/payment/quote. **Confirmed != PaymentSucceeded**. **Cancelled != Refunded**. Create → Pending; Pending → Cancelled allowed. No unrestricted Confirm; no Confirmed → Cancelled (R6); no Cancelled → Pending; no generic SetStatus; no extra payment/capacity statuses. Persist `bookings` in schema `booking`. No passenger/quote/payment/agency columns. |
| **P19-R3** | Capacity reservation / hold / expiry / concurrency | **RESOLVED** | Tour owns configured capacity **definition**. Booking owns capacity **consumption**. Temporary `CapacityHold` is required before later confirmation. Hold states: Active / Consumed / Released / Expired. **CapacityHoldStatus != BookingStatus**. Hold has explicit `ExpiresAt` (no hardcoded product timeout). Overbooking prevention is atomic, server-side, database-backed (`pg_advisory_xact_lock` + unique constraints). Process-local locking is forbidden as correctness. Idempotent hold requests required. **Pending != CapacityHeld**. Consumed remains counted; Released/Expired free capacity once. Booking confirmation orchestration remains R6. Tour capacity-definition mutation coordination remains outside this task. |
| **P19-R4** | Booker / passengers / contact / PII boundary | **RESOLVED** | BookingContactSnapshot is transaction-time contact data. Optional authenticated/Party associations are logical only. BookingPassenger is a Booking-owned transaction child. **PlannerTravelerComposition != BookingPassenger**. **BookingPassenger != Party Person Master**. **BookingContactSnapshot != Party**. **BookingContactSnapshot != Identity Account**. Baseline passenger facts are minimized (GivenName/FamilyName/TravelerCategory). BirthDate omitted (category is explicit, not age-inferred). No passport/document upload. Infant seat-consumption special handling DEFERRED; PassengerCount counts every passenger and cannot exceed Active hold SeatCount. Under-filled holds allowed. Post-confirmation passenger amendment DEFERRED. PII retention = future explicit operational/legal policy. |
| **P19-R5** | Pricing Quote / Booking monetary snapshot | **OPEN** | P12-R4 Quote is Pricing-owned snapshot. Booking historically keeps accepted commercial facts. Whether Quote is mandatory, how expiry works, and post-confirmation authority are **not** locked. ADR 0003 applies. |
| **P19-R6** | Payment / confirmation / cancellation orchestration | **OPEN** | Payment module is P20. Constitution: Payment events; Booking reacts; no Payment DbContext mutation of Booking. Confirmation dependency on payment, refunds, and recovery races are **not** locked. Do not implement Payment. |
| **P19-R7** | Agency/direct booking and external module boundaries | **OPEN** | AgencyOffer is marketplace sales relationship (P13), not Booking owner. Lead ≠ Booking (P18). VisaApplication ≠ Booking (P17). Commission/settlement **OUT**. Conversion from Lead only if explicitly commanded. |
| **P19-R8** | Public booking experience / authorization / reads / privacy | **OPEN** | PE = composition (P14). Sticky Action ≠ Booking until capability exists (P14-R2). Customer booking pages ≠ SEO product. Object-level authorization required. Checkout UI ≠ Payment ownership. |

---

## 9. Architecture invariants (carry forward)

1. Booking != TourProduct · Booking != TourDeparture · Booking != Price · Booking != Quote · Booking != Payment.
2. Booking != TripPlanner Lead · Booking != VisaApplication · Booking != CRM Opportunity.
3. Booking != AgencyMarketplace ranking/settlement authority.
4. Capacity Definition (TourDeparture) != Capacity Consumption (Booking, after R3) != Availability Projection (owner OPEN).
5. PlannerTravelerComposition != BookingPassenger · BookingPassenger != Party Person Master · Booker != Identity Account entity.
6. BookingContactSnapshot != Party (if a contact snapshot is locked later).
7. BookingStatus != PaymentStatus != TourDepartureStatus != CatalogStatus != SEO IndexPolicy.
8. PublicExperience != Booking Source of Truth.
9. Historical Booking commercial snapshot survives live Pricing/FX changes.
10. Payment DbContext does not mutate Booking; no distributed optional transaction across Booking+Payment+Notification.
11. No peer-schema FK; logical ids only; per-module outbox.
12. Money = Amount + CurrencyCode; Toman is display, not CurrencyCode (ADR 0003).
13. NodaTime: local departure dates ≠ UTC Instants (ADR 0004 / P11-R2).
14. Tour capacity ≠ generic stock Inventory module.
15. Booking facts are not public Search documents.
16. Structured attributable facts first; no AI infrastructure in P19.
17. FA / EN / AR · RTL/LTR · bidi-safe names/phones/dates/money.
18. Honest CTA: do not show Book Now / Pay Now / Checkout until the matching capability exists (`docs/pages/09-page-state-and-composition-rules.md`).
19. Do not invent unlocked R# closures.

---

## 10. Security / privacy posture (planning only)

High-risk writes to plan (not implement here):

- overbooking
- price / quote tampering
- passenger PII exposure
- ownership bypass (IDOR)
- replay / duplicate confirmation
- forged Payment status (especially once P20 exists)

Posture:

- data minimization; field-level necessity
- authorization for owner / ops / (if locked) agency actor
- redaction in logs
- no indexable PII routes
- no passport/document upload by default

---

## 11. UX / conversion posture (composition; not implemented in PLAN)

Plan for (after R8 lock):

- mobile-first checkout with clear progress
- summary/review before commit
- disable Book/Pay CTAs when backend capability or Quote/capacity is absent
- do not imply Payment success from UI alone
- Public Tour detail may later compose an honest Book action; sticky chrome remains PE-owned

Entry points (candidates only):

- Tour detail / departure / price summary (P14)
- authorized account booking list/detail
- **not** `/plan` Lead flow as a silent Booking create

---

## 12. Messaging / recovery posture (planning only)

Reuse accepted transactional outbox. Example event names are **not locked**: BookingReserved · BookingConfirmed · BookingCancelled · BookingExpired.

Plan recovery conceptually for:

- payment succeeded / confirmation delayed
- capacity reserved / payment never completes
- duplicate payment success
- repeated cancellation
- message redelivery (idempotent consumers / inbox where already accepted)

Do not invent a second bus.

---

## 13. Conflict detection

Checked against SoT:

| Topic | Result |
|-------|--------|
| Phase ordering P18 → P19 → P20 | **Aligned** |
| Booking module already named vs “create a new owner” | **Aligned** — PLAN uses constitution Booking; does not invent a second booking owner |
| P11 Published ≠ Bookable vs P19 making some departures bookable | **Not a conflict** — P11 forbade inventing Booking *then*; P19 is the authorized phase to introduce bookability **after R# locks** |
| P14-R2 forbids Book Now | **Not a conflict** — remains until Booking capability + R8 allow honest CTA |
| P12 Quote owned by Pricing vs Booking snapshot | **Aligned** — snapshot ≠ engine ownership |
| Payment in P19 vs P20 | **Aligned** — P19 plans contracts; P20 implements Payment |
| Schema list omits `trip_planner` but includes `booking` | **Not blocking** — Booking schema is already listed; TripPlanner was added later as its own R1 |

No irreconcilable SoT conflict blocking PLAN authorship.

---

## 14. Repository safety

- Branch `main` · fast-forward push only · no force · CLEAN working tree before RESULT.
- One docs commit for PLAN (no product code).
- After PLAN ACCEPT, Auto-Execute first locked product task only when architect envelope names it **and** P19-R1 is locked.
- Do not start T001 from this PLAN document alone.
- Do not prematurely resolve P19-R1 through P19-R8.
