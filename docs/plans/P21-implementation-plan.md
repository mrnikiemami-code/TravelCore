# P21 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P21-PLAN` |
| Phase | P21 — Hotel Booking |
| Status | PLAN ACCEPTED · **P21-R1 = RESOLVED** · **P21-R2 = RESOLVED** · **P21-R3–R8 = OPEN** · T002 implemented / awaiting architect review |
| Baseline | `96be199` (`docs(p20): record ArchitectureTests 286 in GATE evidence` · `TC-P20-GATE` ACCEPTED `fc41756`) |
| Authoritative sources | `docs/ROADMAP.md` § P21 · `docs/PROJECT-STATE.md` · `04-module-boundaries.md` § HotelBooking · `docs/domain/module-ownership-matrix.md` · `07-data-architecture.md` (schema `hotel_booking`) · `08-persistence-and-migrations.md` · P07 Place (`Hotel Catalog ≠ Hotel Booking`) · P12 Pricing · P19 Tour Booking · P20 Payment · ADR 0003 (Money) · ADR 0004 (NodaTime) |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P21** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی و موجودی تصمیم را برای Cursor تعریف می‌کند.

> **Envelope note:** Authored from **repository SoT** after architect `TC-P20-GATE` ACCEPT (`fc41756` / docs `96be199`). Next phase is **explicitly** `P21 — Hotel Booking` in `docs/ROADMAP.md` (not guessed). Under PIPELINE continuity, ceremonial confirms are **not required**. **No product code in PLAN task.** Open R# must stay OPEN until architect lock. **Do not implement T001 until this PLAN is ACCEPTED and P21-R1 is architecturally locked.** Do **not** invent P21-R1 through P21-R8 closures here.

---

## 0. Next-phase resolve (from SoT; no extra discovery task)

| Question | Answer from SoT |
|----------|-----------------|
| P20 completion | **COMPLETE / ACCEPTED** — Gate evidence `fc41756`; architect ACCEPT issued this session |
| Authoritative next phase ID | **P21** |
| Title / purpose | **Hotel Booking** — جدا از Place Hotel Catalog: provider abstraction · mapping · search · availability · rooms · rates · cancellation rules · Quote · reservation · booking · voucher · provider sync (`docs/ROADMAP.md` § P21) |
| PLAN already existed? | **NO** — this document is the first P21 PLAN |
| SoT conflict? | **NO** — ROADMAP names P21 after P20; constitution already names HotelBooking as an External/commerce module; schema `hotel_booking` is listed. Capability themes are **not** ownership transfers to Place catalog, Tour Booking, Flight, Payment, Pricing, Search, or SEO. |
| Dedicated module/schema in SoT today? | **YES (conceptual)** — HotelBooking module exists in `04-module-boundaries.md`; PostgreSQL schema `hotel_booking` is listed. **No HotelBooking product code / DbContext / aggregates exist in the repository yet.** |
| Hotel Catalog owner today? | **Place** — P07-R1: Place aggregate + typed Hotel specialization 1:1; canonical catalog id = `PlaceId`; **Hotel Catalog ≠ Hotel Booking**; no HotelBooking fields on Place |
| Missing business fact blocking PLAN authorship? | **NO** — PLAN may enumerate R# and IN/OUT/DEFER without locking product semantics or choosing a supplier |
| Invented phase? | **NO** — P21 is already listed in ROADMAP |
| Speculative provider? | **NONE selected** — constitution says provider abstraction; no named hotel supplier SDK lock in SoT |

---

## 1. Phase Purpose

P21 باید قابلیت **HotelBooking** را به‌عنوان دامنهٔ تراکنشی مستقل معرفی کند: رزرو اقامت هتل در برابر موجودی/پیشنهاد زندهٔ قابل رزرو — **بدون** دزدیدن مالکیت Place Hotel catalog، Tour Booking، Flight، Pricing/Quote (تا طراحی صریح)، Payment execution، Search, SEO, Notification delivery, or Agency settlement.

Preserve (already locked; PLAN does not reopen them):

1. **Hotel Catalog ≠ Hotel Booking** — Place owns descriptive Hotel catalog (`PlaceId`). HotelBooking owns reservation transaction facts (`04-module-boundaries.md` · P07-R1).
2. **HotelBooking ≠ Tour Booking** — P19 Booking module remains TourDeparture-scoped. Do **not** create `Booking<T>` or reuse the Tour Booking aggregate blindly.
3. **`ProviderHotelId` / `ExternalHotelId` never Place PK.** Mapping belongs to HotelBooking.
4. **Payment != HotelBooking** — P20 Payment remains money-movement SoT. P21 must not mutate Payment internals or invent TargetType/TargetId universalization unless a later locked R# proves it.
5. **Pricing dependency is not automatic** — constitution: mandatory HotelBooking→Pricing ownership is forbidden until explicitly designed.
6. Money = Amount + CurrencyCode. No Toman CurrencyCode. No float monetary persistence. No FX engine invented in P21 unless SoT later requires it (ADR 0003).
7. Temporal facts use NodaTime. **Do not use server-local time** (ADR 0004).
8. **PublicExperience = composition only.** Search is not live-availability SoT. SEO IndexPolicy remains SEO-owned. Transaction pages stay noindex.
9. Module-local transactional outbox / consumer-owned inbox (`07` / `29`). No second event bus. No peer-schema FK. No shared DbContext.
10. **AI-readiness = structured stay facts** — identifiers, stay dates, statuses, provider references — **بدون** pricing-AI / booking agent.

P07 delivered Hotel catalog specialization. P19 delivered Tour Booking. P20 delivered Payment. P21 must not collapse those owners.

---

## 2. Starting Baseline

| Item | Value |
|------|--------|
| P20 Gate | `TC-P20-GATE` COMPLETE / ACCEPTED (`fc41756` · docs `96be199`) |
| P20 evidence | [`P20-GATE-acceptance-evidence.md`](P20-GATE-acceptance-evidence.md) · [`P20-T009-hardening-and-evidence-pack.md`](P20-T009-hardening-and-evidence-pack.md) |
| Baseline HEAD | `96be199` |
| P00–P20 | COMPLETE |
| Hotel catalog | **Place.Hotel** (`src/backend/Modules/Place/.../Hotel.cs`) — descriptive only |
| HotelBooking module / schema | **Conceptual only** (`04-module-boundaries.md` · schema `hotel_booking`) — no product code |
| Tour Booking | COMPLETE (P19) — separate aggregate; do not reuse as HotelBooking |
| Payment | COMPLETE (P20) — Tour Booking payment target; HotelBooking payment target is **OPEN (P21-R6)** |
| Named hotel supplier SDK | **NONE** in repository |
| Existing HotelBooking APIs | **NONE** |

---

## 3. Scope classification (IN / OUT / DEFER)

Classifications below are **planning inventory**, not architect locks. Dependent product tasks must not treat IN as permission to invent unlocked R# closures.

| Concept | Classification | Notes |
|---------|----------------|-------|
| Independent HotelBooking module + schema `hotel_booking` (SoT candidate) | **IN (candidate)** | Locked only by **P21-R1** |
| Logical Place Hotel / `PlaceId` reference (no peer FK) | **IN (candidate)** | Catalog owner remains Place |
| Provider mapping `ExternalHotelId` → `PlaceId` | **IN (candidate)** | External ID is never Place PK |
| Stay dates CheckIn/CheckOut + nights rule | **IN (candidate)** | NodaTime LocalDate; **P21-R2** |
| Availability / hold / supplier-neutral reservation | **IN (candidate)** | **P21-R3** — do not copy Tour CapacityHold automatically |
| Rate offer / quote / monetary + cancellation-policy snapshot | **IN (candidate)** | **P21-R4** — Pricing ownership remains OPEN |
| Lifecycle / confirmation / idempotency / reconciliation | **IN (candidate)** | **P21-R5** |
| Payment integration / refund dependency | **IN (candidate) / DEFER until R6 lock** | Do not mutate accepted P20 semantics in PLAN |
| Cancellation / amendment / refund-policy boundary | **IN (candidate)** | **P21-R7** |
| Public UX / authorization / privacy / operational reads / provider readiness | **IN (candidate)** | **P21-R8** |
| Place Hotel catalog fields / CMS | **OUT** | Place / Content |
| Tour Booking aggregate reuse / `Booking<T>` | **OUT** | P19 remains TourDeparture |
| Flight live inventory | **OUT** | P22 |
| Dynamic package Flight+Hotel | **OUT** | P23 |
| Search as live availability SoT | **OUT** | Search is retrieval projection |
| SEO IndexPolicy ownership | **OUT** | SEO |
| Notification delivery / SMTP/SMS | **OUT** | future Notification |
| Accounting / bank settlement / agency settlement / wallet / fraud / chargeback | **OUT / DEFER** | same posture as P20 |
| Named production hotel supplier SDK | **DEFER** | no provider selected |
| Supplier payout | **OUT** | not P21 |
| AgencyHotelBooking fork / commission | **OUT** unless later R# proves one aggregate is insufficient |
| PDF voucher engine | **DEFER** | do not add document capability without need |
| Analytics warehouse | **OUT** | |
| Manual status mutation / support SetStatus | **OUT** | |

---

## 4. Open decisions (must stay OPEN)

PLAN records already-locked constitution. **New P21 product semantics stay OPEN.**

| ID | Topic | Status |
|----|--------|--------|
| **P21-R1** | HotelBooking module ownership / schema / catalog reference | **RESOLVED** — independent HotelBooking module · schema `hotel_booking` · Place remains catalog owner · logical PlaceId / `HotelPlaceReference` · no peer-schema FK · no shared DbContext · **HotelBooking != Place** · **HotelBooking != Tour Booking** · named supplier = NONE · product model deferred |
| **P21-R2** | Stay structure / room reservations / guest occupancy / multi-room scope | **RESOLVED** — NodaTime LocalDate CheckIn/CheckOut · Nights derived · 1..N RoomReservations · guests assigned per room · Adult/Child · Child AgeAtCheckIn · no BirthDate · exactly one LeadGuest · HotelBookingContactSnapshot · occupancy is requested composition not availability |
| **P21-R3** | Availability/inventory authority / hold / supplier-neutral reservation boundary | **OPEN** |
| **P21-R4** | Hotel rate offer / quote / monetary snapshot / cancellation policy snapshot | **OPEN** |
| **P21-R5** | HotelBooking lifecycle / confirmation authority / supplier orchestration / idempotency / reconciliation | **OPEN** |
| **P21-R6** | Payment integration / target extension / financial compensation / refund dependency | **OPEN** |
| **P21-R7** | Cancellation / amendment / refund-policy boundary | **OPEN** |
| **P21-R8** | Public UX / anonymous-auth / privacy / operational reads / supplier-provider readiness | **OPEN** |

Do **not** invent closures here. T001 may scaffold only after R1 is architect-locked.

### Already locked (constitution / prior phases; not P21 inventions)

```text
Hotel Catalog != Hotel Booking
canonical Hotel catalog identity = PlaceId (Place module)
HotelBooking mapping: PlaceId + Provider + ExternalHotelId
ExternalHotelId never Place PK
HotelBooking != Tour Booking
no Booking<T> platform
no peer-schema FK
no shared DbContext
Money = Amount + CurrencyCode
no Toman CurrencyCode
NodaTime; no server-local time
Search != live hotel availability SoT
PublicExperience != HotelBooking Source of Truth
Payment != HotelBooking
Pricing mandatory ownership of HotelBooking fares = NOT locked (forbidden until designed)
```

### Candidate R1 posture (not locked)

```text
P21-R1 RESOLVED:

HotelBooking = Independent Domain Module
schema = hotel_booking
catalog owner = Place
catalog reference = logical PlaceId (HotelPlaceReference)
HotelBooking does not own Place.Hotel
no HotelBooking aggregate/lifecycle/provider in T001
```

---

## 5. Architecture investigation notes (evidence, not locks)

### 5.1 Catalog owner
**Place** owns Hotel descriptive facts (P07). Typed specialization table 1:1. No TPH. No HotelBooking columns on Place.

### 5.2 Module / schema candidate
Constitution names **HotelBooking**. Data architecture lists schema **`hotel_booking`**. Persistence docs list the same schema. No competing schema name exists. **Still OPEN until P21-R1 lock.**

### 5.3 What HotelBooking reserves
SoT purpose: live bookable inventory + provider interaction: mapping · availability search · room offers · live rates · cancellation conditions · provider quote · reservation · booking · voucher · sync/reference.

Exact reservation unit (Hotel / RoomType / RatePlan / StayOffer / StayInventoryUnit) is **OPEN (R2/R3)**.

### 5.4 Stay dates
Hotel booking involves CheckInDate / CheckOutDate. Canonical nights candidate: `Nights = CheckOutDate - CheckInDate` with CheckOut after CheckIn. **OPEN (R2)** — do not persist ambiguous inclusive-night counts without a locked rule.

### 5.5 Timezone
Property IANA timezone is a candidate concern for cancellation deadlines, check-in cutoff, stay boundaries, supplier sync. Server-local time is forbidden. **OPEN (R2/R5/R7).**

### 5.6 RoomType / RatePlan
Distinguish **RoomType catalog** vs **bookable room/rate offer**. Owner of catalog RoomType (Place vs HotelBooking) is **OPEN**. RatePlan as catalog vs supplier offer is **OPEN (R3/R4).**

### 5.7 Inventory
Do not invent availability truth without an authoritative source. Tour `CapacityHold` is **not** automatically copied. Hold vs supplier reservation vs allotment is **OPEN (R3).**

### 5.8 Money / Quote
HotelBooking offers may use Money. Whether Pricing owns hotel quotes, or HotelBooking snapshots supplier offers, is **OPEN (R4).** Do not implement HotelBooking FX in PLAN.

### 5.9 Guests
Do not inherit Tour traveler categories blindly. Occupancy / guest snapshot / PII minimization is **OPEN (R2/R8).**

### 5.10 Payment
P20 Payment initial target is Tour Booking. Extending Payment to HotelBooking, vs a later composition, is **OPEN (R6).** Do not silently change P20 enums or invent generic TargetType+TargetId.

### 5.11 Public / auth
Do not reuse Tour Booking access token across an unrelated aggregate. Public route architecture is **OPEN (R8)** after inspecting current Place/hotel public routes. Transaction pages remain noindex candidates.

### 5.12 Search / SEO
Search must not become live availability SoT. Transactional HotelBooking tables are not a discovery index.

### 5.13 Operational reads
Read-only operational visibility is a candidate (P20 lesson). Manual SetStatus / ForceConfirm remains forbidden. **OPEN (R8).**

### 5.14 Provider readiness
Named production supplier = NONE. Capabilities must be explicit if/when adapters exist. PLAN does not select a supplier.

### 5.15 Multi-room / occupancy (RESOLVED — R2)
One HotelBooking supports **one or more** `RoomReservation`s. Each `RoomReservation` is one booked room position (no `Quantity`). Every `HotelBookingGuest` belongs to exactly one room. Categories = Adult / Child. Child stores `AgeAtCheckIn`. BirthDate is not stored. Exactly one LeadGuest. `HotelBookingContactSnapshot` is independent contact data. Occupancy is requested transaction composition, not availability/rate eligibility. No platform-wide occupancy/room/guest limits.

### 5.16 Inventory authority alternatives (OPEN — R3)
Candidate authorities (not locked): supplier live query · HotelBooking-owned allotment · hybrid hold-then-reserve · no local inventory until a provider exists. **Do not invent fake availability.** Tour `CapacityHold` is not automatically copied.

### 5.17 P20 Partial Refund conflict
P20 Partial Refund is **DEFERRED**. Hotel cancellation penalties / partial stay refunds must **not** silently reopen Partial Refund inside Payment. If a hotel penalty needs a partial money movement, that is an **OPEN R6/R7 conflict** for architect lock — not a P20 mutation and not a PLAN closure.

### 5.18 Supplier / Payment orchestration failure modes (inventory only)
Documented for later R5/R6/R7; not implemented:

- supplier reserve succeeds / Payment never starts
- Payment succeeds / supplier reserve fails or times out
- callback/query ambiguous
- cancellation after supplier confirm but before Payment success
- browser/supplier acknowledgement without verified reservation
- retry of an already-reserved stay

These remain **OPEN**. Compensation must compose existing Payment Refund semantics; it must not invent a second money engine.

---

## 6. Critical dependencies (carry-forward)

| Dependency | Posture |
|------------|---------|
| Place Hotel catalog | **Required logical reference**; Place remains catalog SoT |
| ReferenceData | Country/currency as already accepted |
| Pricing | **Not mandatory owner** until R4 lock |
| P19 Booking | **Peer**; do not merge aggregates |
| P20 Payment | **Peer**; R6 decides integration |
| Search | Composition/projection only |
| SEO | IndexPolicy owner |
| PublicExperience | Composition only |
| Identity / Access / Party | Opaque refs + snapshots; no master reuse |
| Notification | Delivery deferred |

---

## 7. Task decomposition (authoritative sequence)

Tasks below are **planning slots**. They do **not** authorize implementation until PLAN is ACCEPTED and the matching R# is locked.

### TC-P21-T001 — Module / schema / catalog reference

- Depends on **P21-R1**. Scaffold only after R1 lock. No aggregate/lifecycle/provider yet.

### TC-P21-T002 — Stay / room / guest structure

- Depends on **P21-R2**. **IMPLEMENTED / AWAITING_ARCHITECT_REVIEW**
- Locked: NodaTime `LocalDate` CheckIn/CheckOut · derived Nights · 1..N `RoomReservation` · room-assigned Adult/Child guests · Child `AgeAtCheckIn` · no BirthDate · exactly one LeadGuest · `HotelBookingContactSnapshot` · occupancy is requested composition, not availability.

### TC-P21-T003 — Availability / inventory / supplier reservation boundary

- Depends on **P21-R3**.

### TC-P21-T004 — Rate offer / monetary / cancellation-policy snapshot

- Depends on **P21-R4**.

### TC-P21-T005 — Lifecycle / confirmation / idempotency / reconciliation

- Depends on **P21-R5**.

### TC-P21-T006 — Payment integration / compensation / refund dependency

- Depends on **P21-R6**. Must not mutate accepted P20 semantics.

### TC-P21-T007 — Cancellation / amendment / refund-policy boundary

- Depends on **P21-R7**.

### TC-P21-T008 — Public UX / authorization / privacy / operational reads / provider readiness

- Depends on **P21-R8**.

### TC-P21-T009 — Hardening + evidence

- Guardrails + evidence pack; **no new capability**. Does **not** execute GATE.

### TC-P21-GATE — Acceptance Gate

- Evidence only. No new HotelBooking product in GATE. Do not start P22 inside GATE.

Do not manufacture empty capabilities merely to fill numbering. A slot may become VACANT only if the matching R# has no independent product slice after architect lock.

---

## 8. Architecture invariants (carry forward)

1. Hotel Catalog ≠ Hotel Booking.
2. HotelBooking ≠ Tour Booking ≠ Flight.
3. PlaceId is canonical catalog identity; ExternalHotelId is mapping only.
4. HotelBooking DbContext does not mutate Place/Tour/Booking/Payment/Pricing tables.
5. No peer-schema FK. No shared DbContext. No Booking&lt;T&gt; platform.
6. Client is not monetary, availability, or success authority.
7. PublicExperience ≠ HotelBooking Source of Truth.
8. Search ≠ live availability SoT.
9. Browser/supplier acknowledgement ≠ confirmed reservation until locked verification rules say so.
10. Historical commercial snapshots survive live rate changes.

---

## 9. Conflicts

**None material.** Schema `hotel_booking` is already listed. HotelBooking module is already named. Place already owns Hotel catalog without HotelBooking fields. P19/P20 explicitly kept HotelBooking out. PLAN does not choose a supplier.

---

## 10. Repository safety

- Branch `main` · fast-forward push only · no force · CLEAN working tree before RESULT.
- One docs commit for PLAN (no product code).
- Do **not** execute `TC-P21-T001` until PLAN ACCEPTED and P21-R1 locked.

---

## 11. PLAN Done criteria

- Hotel Catalog owner reported exactly: **Place**
- HotelBooking ownership/schema **candidate** explicit: independent module / `hotel_booking`
- Hotel Catalog ≠ Hotel Booking preserved
- HotelBooking ≠ Tour Booking preserved
- P21-R1 through P21-R8 inventoried and **OPEN**
- T001–T009 + GATE sequence exists
- No HotelBooking product code / migration / SDK
- P22/P23 not started
