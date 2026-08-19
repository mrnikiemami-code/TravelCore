# P23 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P23-PLAN` |
| Phase | P23 — Dynamic Package / Flight + Hotel |
| Status | PLAN ACCEPTED · **P23-R1 = RESOLVED** · **P23-R2–R6 OPEN** · **P23-R7 = OPEN** · **P23-R8 = OPEN** · T001–T008 implemented · **not COMPLETE** |
| Baseline | `2a372ae` (`feat(flight): close P22 with acceptance gate evidence [TC-P22-GATE]`) · GATE docs `ed040f0` · architect `TC-P22-GATE = ACCEPTED` |
| Authoritative sources | `docs/ROADMAP.md` § P23 · `docs/PROJECT-STATE.md` · `04-module-boundaries.md` § Tour / Booking / Pricing / Payment / HotelBooking / Flight / Search / SEO · `docs/domain/module-ownership-matrix.md` · `07-data-architecture.md` · `06-cross-module-communication.md` Example 7 · `15-future-architecture-transition-map.md` § T/U · P11 Tour transport · P12 Pricing · P15 Search · P19 Booking · P20 Payment · P21 HotelBooking · P22 Flight · ADR 0003 (Money) · ADR 0004 (NodaTime) |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی P23** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط یافته‌های Repository، گزینه‌های معماری، موجودی تصمیم، و Taskهای اجرایی را برای Cursor تعریف می‌کند.

> **Envelope note:** Authored from **repository SoT** after architect `TC-P22-GATE` ACCEPT (`2a372ae` / docs `ed040f0`). Next phase is **explicitly** `P23 — Dynamic Package / Flight + Hotel` in `docs/ROADMAP.md` (not guessed). **No product code in PLAN task.** Open R# stay OPEN until architect lock. **Do not implement T001 until this PLAN is ACCEPTED and P23-R1 is architecturally locked.** Do **not** invent P23-R1 through P23-R8 closures here. Do **not** start P24. Do **not** execute `TC-P23-T001`.

---

## 0. Next-phase resolve (from SoT; no extra discovery task)

| Question | Answer from SoT |
|----------|-----------------|
| P22 completion | **COMPLETE / ACCEPTED** — Gate evidence `2a372ae`; result docs `ed040f0`; architect ACCEPT issued this session |
| Authoritative next phase ID | **P23** |
| Title / purpose | **Dynamic Package / Flight + Hotel** — پس از پایدار شدن HotelBooking و Flight: خرید ترکیبی مثل Flight + Hotel (`docs/ROADMAP.md` § P23) |
| Declared status before this PLAN | **PLANNED** / NOT_STARTED |
| Dependencies | P21 HotelBooking COMPLETE · P22 Flight COMPLETE. ROADMAP: «قبل از وجود قابلیت‌های زیرساختی رزرو، پیاده نشود» — both reservation infrastructures now exist. |
| Next-phase relationship | P24 is B2B / Agency Commerce (PLANNED). P23 does not start P24. |
| PLAN already existed? | **NO** — this document is the first P23 PLAN |
| SoT conflict? | **NO** — ROADMAP names P23 after P22. Constitution already forbids collapsing Flight/Hotel/Tour/Payment. **No Dynamic Package module/schema/row exists yet.** |
| Dedicated module/schema in SoT today? | **NO** — `04-module-boundaries.md` and ownership matrix have **no Dynamic Package / packaging row**. `07-data-architecture.md` lists `hotel_booking` and `flight` but **not** `dynamic_package`. Domain map External Inventory = HotelBooking + Flight only. |
| Tour today | TourBooking is **not** a package of live Flight+Hotel. `TourDepartureTransportSegment` is Tour-owned labels only (P11-R5). |
| Missing business fact blocking PLAN authorship? | **NO** — PLAN may enumerate R# and IN/OUT/DEFER without locking product semantics or choosing a supplier |
| Invented phase? | **NO** — P23 is already listed in ROADMAP |
| Speculative supplier/provider? | **NONE selected** — Flight production sources ALL NONE · Hotel production sources ALL NONE · Named suppliers NONE · Production Payment Provider NONE |

---

## 1. Phase Purpose

P23 باید قابلیت **خرید ترکیبی زندهٔ Flight + Hotel** را معرفی کند: یک تجربهٔ مشتری برای انتخاب، تعهد، پرداخت، تأیید و لغو یک ترکیب — **بدون** دزدیدن مالکیت `FlightBooking`، `HotelBooking`، Tour package transport، Place Hotel catalog، P19 Tour Booking، Pricing Tour rates، Payment execution، Search index، or SEO IndexPolicy.

Preserve (already locked; PLAN does not reopen them):

1. **FlightBooking ≠ HotelBooking** — independent aggregates, schemas `flight` / `hotel_booking`, independent tokens, independent confirmation evidence.
2. **Hotel Catalog ≠ HotelBooking** — Place remains catalog owner.
3. **Flight ≠ Tour package transport** — `TourDepartureTransportSegment` remains Tour-owned labels. Live inventory stays in Flight.
4. **Price ≠ Quote ≠ Booking ≠ Payment** — each component keeps its own offer/monetary snapshot; Payment owns money movement.
5. **Search is not transaction SoT** — P15 Search is retrieval/discovery; live Flight/Hotel offers remain source-authoritative in their modules.
6. **No shared DbContext / schema-per-module / no peer-schema FK / no distributed transaction.**
7. **UUIDv7 · NodaTime · Money/Currency** — Toman is display-only, never `CurrencyCode`. No implicit FX.
8. **No generic `BookingBase` / universal Booking.** P19 Booking remains TourDeparture-scoped.
9. **P20/P21/P22 Partial Refund = DEFERRED.** P23 must not hide this.
10. **Flight confirmation** = `FlightSupplierReservation.Confirmed` AND `Payment.Succeeded` AND all required tickets Issued.
11. **Hotel PayNow** = Payment succeeds **before** final supplier reservation. Do **not** copy Flight PNR-first onto Hotel.
12. **Flight production sources ALL NONE. Hotel production sources ALL NONE. Named suppliers NONE. Production Payment Provider NONE.**
13. **MultiCity remains DEFERRED** unless P23 genuinely requires it — this PLAN recommends it does **not**.
14. **PublicExperience = composition only.** Transaction pages stay noindex unless SEO later locks otherwise.
15. **AI-readiness = structured attributable facts** — identifiers, component IDs, statuses, provenance — **بدون** LLM/RAG/vector DB.

P21 delivered HotelBooking. P22 delivered Flight. P23 must coordinate them, not merge them.

---

## 2. Starting Baseline

| Item | Value |
|------|--------|
| P22 Gate | `TC-P22-GATE` COMPLETE / ACCEPTED (`2a372ae` · docs `ed040f0`) |
| P22 evidence | [`P22-GATE-acceptance-evidence.md`](P22-GATE-acceptance-evidence.md) |
| Baseline HEAD | `2a372ae` (product) / starting docs HEAD `ed040f0` |
| P00–P22 | COMPLETE |
| DynamicPackage module / schema | **FOUNDATION ONLY** — independent `DynamicPackage.Contracts` / `Domain` / `Infrastructure` · schema `dynamic_package` · EnsureSchema migration · **no product tables** · **DynamicPackageBooking not implemented** |
| Flight | Independent module · schema `flight` · `FlightBooking` inside Flight · OneWay/RoundTrip · MultiCity DEFERRED · PNR-first Payment · triple-evidence confirmation |
| HotelBooking | Independent module · schema `hotel_booking` · hold then PayNow reservation · dual-evidence confirmation |
| Tour transport | `TourDepartureTransportSegment` — Sequence / Mode / Origin+Destination labels only |
| Booking (P19) | TourDeparture-scoped aggregate · schema `booking` · **not** a package of live Flight+Hotel |
| Pricing (P12) | Tour commercial rates/quotes · **not** generalized to airline fares or hotel live rates |
| Payment | Closed kinds **exactly** `TourBooking`, `HotelBooking`, `FlightBooking` · Partial Refund DEFERRED · Production Payment Provider NONE |
| Search | Schema `search` · not live supplier truth · not transaction SoT |
| SEO | IndexPolicy owner · transactional Flight/Hotel/Tour booking pages hardcoded noindex |
| Named suppliers / SDKs | **NONE** |
| Host composition | `Program.cs` registers `HotelBookingModule` then `FlightModule` then `DynamicPackageModule` · DynamicPackage has **no endpoints** |

---

## 3. Existing component facts (exact; do not invent)

### 3.1 Flight (P22)

| Finding | Evidence |
|---------|----------|
| Owner | Flight module · schema `flight` · `FlightBooking` inside Flight |
| Trip types | `FlightTripType` = OneWay, RoundTrip only. MultiCity DEFERRED |
| Connecting flights | Journey → 1..N Segments (implemented) |
| Passengers | Adult / Child / Infant; names stored; BirthDate/passport not stored |
| Monetary | Immutable `FlightBookingMonetarySnapshot` (BaseFare + Taxes + Fees = Total; one CurrencyCode) |
| Offer | Immutable `FlightOfferSnapshot` + `FlightFareRulesSnapshot`; source `OfferExpiresAt`; no silent repricing |
| Reservation | `IFlightReservationSource` · one `FlightSupplierReservation` per booking · opaque `ReservationLocator` (no type named PNR) · `PaymentRequiredForReservation = false` (PNR-first) |
| Confirmation | `FlightBooking.Confirmed = Reservation.Confirmed AND Payment.Succeeded AND all required tickets Issued` |
| Payment | One FlightBooking → one Payment · amount from Flight monetary snapshot · target kind `FlightBooking` |
| Cancellation | Whole-booking only · FullRefund / NoRefund executable · partial blocked · ticket void/refund ≠ Payment Refund |
| Public token | `X-TravelCore-Flight-Booking-Access-Token` (SHA-256 verifier; not reusable as Hotel/Tour token) |
| Production sources | Search / Availability / Offer / Reservation / Ticketing / Cancellation = **NONE** |
| Named Flight Supplier | **NONE** |

### 3.2 HotelBooking (P21)

| Finding | Evidence |
|---------|----------|
| Owner | HotelBooking module · schema `hotel_booking` |
| Catalog | Place owns Hotel catalog; logical `PlaceId` only |
| Stay | NodaTime LocalDate CheckIn/CheckOut · 1..N `RoomReservations` · Adult/Child · no Infant · no BirthDate |
| Hold | `HotelAvailabilityHold` Requested/Active/Released/Expired · one hold covers complete room set · **hold ≠ reservation ≠ confirmation** |
| Monetary | Immutable `HotelBookingMonetarySnapshot` (Total · optional PayableNow / PayableAtProperty) |
| PayNow lock | Payment **must succeed before** final supplier reservation initiation (`HotelSupplierReservationService`) |
| Confirmation | Dual evidence: Payment Succeeded **and** SupplierReservation Confirmed |
| Payment | One HotelBooking → one Payment · target kind `HotelBooking` |
| Cancellation | Confirmed-only process · Penalty=0 full Refund · Penalty=Total no Refund · partial blocked |
| Public token | `X-TravelCore-Hotel-Booking-Access-Token` |
| Production sources | Availability / Rate / Reservation = **NONE** |
| Named Hotel Supplier | **NONE** |

### 3.3 Payment (P20 + typed targets)

| Finding | Evidence |
|---------|----------|
| Target kinds | Enum closed: `TourBooking = 1`, `HotelBooking = 2`, `FlightBooking = 3` |
| Aggregate | Exactly one of Booking / HotelBooking / FlightBooking reference per Payment |
| Refund | Full Refund only · `PartialRefundImplemented = false` · PaymentStatus stays Succeeded after Refund |
| Production provider | `NOT CONFIGURED / NONE` · NamedProvider = NONE |

### 3.4 Tour + Booking + Pricing

| Finding | Evidence |
|---------|----------|
| TourBooking | P19 Booking module · TourDeparture logical target · Quote snapshot from Pricing |
| Tour transport | `TourDepartureTransportSegment` comment: “not Flight entity, airline, ticket, seat inventory, or Booking” |
| Package hotel option | `TourHotelOption` references PlaceId — catalog, not live HotelBooking |
| Pricing | TourRate / Quote · constitution forbids mandatory Flight/HotelBooking → Pricing until designed · P21/P22 owned their own snapshots |

### 3.5 Search / SEO / ReferenceData / Place

| Finding | Evidence |
|---------|----------|
| Search | Retrieval + hybrid read-model. `Search != live availability authority` (Hotel). Flight live search is Flight-owned, not P15. Projected facts are not Search-owned truth. |
| SEO | Owns IndexPolicy / SeoRoute. Transaction pages (`/flight-bookings/...`, `/hotel-bookings/...`, `/bookings/...`) are hardcoded noindex. |
| ReferenceData | Airport/Airline catalog authority (P22-R2); catalogs may still be unimplemented. Currency codes. |
| Place | Hotel/Restaurant/Attraction catalog — not airport, not live booking. |

### 3.6 Ownership matrix / data architecture gap

There is **no** ownership-matrix row, module-boundary section, or PostgreSQL schema for Dynamic Package. `15` § U lists “later dynamic packages” only as a Flight consumer, not as an owned module. This is a **documentation gap for R1**, not permission to fold packaging into Tour, Booking, Flight, or HotelBooking.

---

## 4. What “Dynamic Package” in P23 represents

ROADMAP: combined purchase **like Flight + Hotel** after both reservation infrastructures exist. Not a new Tour product. Not a rewrite of P11 Foreign Package Tour.

| Alternative | Meaning | Fit |
|-------------|---------|-----|
| **A. New commercial Product aggregate** | Catalog product like TourProduct that “is” a Flight+Hotel SKU | **Poor.** Live Flight/Hotel offers expire; Tour products are descriptive and survive offer disappearance. Would confuse TourProduct with volatile supplier inventory. |
| **B. Customer Quote/selection aggregate only** | Durable selection/quote without owning the transaction | Incomplete. P23 must coordinate reservation, payment, confirmation, cancellation. Quote-only leaves saga state homeless. |
| **C. Transaction/orchestration aggregate coordinating FlightBooking + HotelBooking** | New durable identity that references component bookings and orchestrates them | **Best fit.** Matches ROADMAP “خرید ترکیبی”, preserves component ownership, gives Payment/cancellation/ops a package identity. |
| **D. Composition without a new persistent owner** | UI/application layer stitches two existing journeys | **Weak.** Cannot own combined payment obligation, saga/compensation state, or package confirmation without either lying or mutating Flight/Hotel internals. Crash between components has no owner. |
| **E. Fold into Tour or P19 Booking** | TourBooking becomes live Flight+Hotel | **Forbidden.** TourBooking ≠ live Flight+Hotel. Would create generic Booking. |

**PLAN recommendation (not a lock):** **C**. Leave **P23-R1 OPEN**.

Locked distinction to preserve:

```text
TourProduct / TourDeparture / TourDepartureTransportSegment / TourHotelOption
!=
live FlightBooking + live HotelBooking
!=
DynamicPackageBooking (orchestration of the live pair)
```

---

## 5. Ownership question (exactly one recommended baseline)

### Candidate A — New DynamicPackage module (recommended)

Independent External/commerce-adjacent module owning package search-composition (transient), package offer/monetary snapshot, `DynamicPackageBooking` orchestration identity, saga/compensation process state, package-scoped public token, and operational package read. Schema candidate `dynamic_package` (not listed in `07` today). Host registration after `FlightModule`. Talks to Flight, HotelBooking, and Payment **only** via Contracts / events / outbox-inbox. No peer-schema FK. No shared DbContext.

Pros: same pattern as not folding HotelBooking into Place or Flight into Tour; one persistence boundary for package saga; Payment can later add a fourth **typed** target without a generic `TargetType` platform.
Cons: new module/schema not yet in constitution tables — R1 must add them when locked; risk of becoming a mega-status blob if it copies component statuses.

### Candidate B — Booking (P19) module

Pros: “booking” noun already exists.
Cons: P19 Booking is TourDeparture-scoped. Extending it would create `Booking<T>` / universal Booking. **Not recommended.**

### Candidate C — Tour module

Pros: “package” noun exists on Foreign Package Tour.
Cons: Tour package transport/hotel options are **descriptive catalog facts**, not live reservations. Absorbing live Flight+Hotel would violate P11-R5 / P22-R1. **Not recommended.**

### Candidate D — Application/orchestration layer only (no module)

Pros: fewer schemas.
Cons: no durable package identity; payment/compensation/crash recovery have no owner; presentation would copy business rules. **Not recommended.**

### Candidate E — Own it inside Flight or HotelBooking

Pros: reuse one existing schema.
Cons: the other component becomes a peer-owned child. Violates FlightBooking ≠ HotelBooking. **Not recommended.**

**PLAN recommendation (not a lock):** **Candidate A — new DynamicPackage module**. Recommended names (not locked): module `DynamicPackage` · schema `dynamic_package` · transactional aggregate `DynamicPackageBooking` **inside** DynamicPackage. Leave **P23-R1 OPEN**.

FlightBooking ownership unchanged: **YES** (recommended).
HotelBooking ownership unchanged: **YES** (recommended).

---

## 6. Transaction boundary

P23 **does** need a durable transaction identity.

| Identity | Role |
|----------|------|
| `FlightBooking` | Live flight transaction SoT (unchanged) |
| `HotelBooking` | Live hotel stay transaction SoT (unchanged) |
| `DynamicPackageBooking` (recommended name, OPEN) | Package orchestration SoT: logical `FlightBookingId` + logical `HotelBookingId`, package lifecycle, package monetary snapshot, package payment correlation, compensation process |

`DynamicPackageBooking` **references** the two components by Guid. It does **not** replace them, does **not** inherit them, and does **not** introduce `BookingBase`.

Prevent:

- `Booking<TComponent>`
- shared EF navigation Flight ↔ Hotel ↔ Package
- treating TourBooking as the package
- a second FlightBooking/HotelBooking schema “for packages”

New persistent package aggregate recommended: **YES** (`DynamicPackageBooking`). Component aggregates remain the owners of reservation/PNR/hold/tickets/vouchers.

---

## 7. Composition semantics (cardinality)

| Topic | Recommendation (OPEN) |
|-------|----------------------|
| Baseline cardinality | **Exactly one `FlightBooking` + exactly one `HotelBooking`** per `DynamicPackageBooking` |
| Flight trip | Reuse P22: OneWay **or** RoundTrip; connecting segments already in that one FlightBooking |
| Hotel rooms | Reuse P21: 1..N rooms inside that one HotelBooking |
| Extra FlightBookings (e.g. separate inbound ticket) | **OUT of baseline** |
| Extra HotelBookings | **OUT of baseline** |
| MultiCity Flight | **DEFERRED** — P23 does not require it; RoundTrip + connections cover typical Flight+Hotel |
| Open-jaw / multi-hotel | **DEFERRED** |
| Infant | Flight may include Infant; Hotel stay has no Infant guest slot. Copy independently; do not invent a shared person aggregate |

Round-trip flight + multi-room hotel is **one package** because each side already models that internally.

---

## 8. Search / discovery composition

Distinguish:

```text
customer destination/date intent
!= Flight live search (Flight / IFlightSearchSource)
!= Hotel live availability/rate (HotelBooking / IHotelAvailabilitySource + IHotelRateOfferSource)
!= package candidate combination (DynamicPackage, recommended)
!= P15 Search index
```

| Concern | Recommended owner (OPEN) |
|---------|--------------------------|
| Destination/date/occupancy/pax intent | DynamicPackage query contract (public UX) · Destination graph remains Destination-owned |
| Flight candidates | Flight (`IFlightSearchSource` / revalidation) |
| Hotel candidates | HotelBooking (availability + rate) |
| Package candidate combination | DynamicPackage application — pair already-revalidated component offers; do not invent a third live supplier |
| Ranking of combinations | DynamicPackage (deterministic, documented). **Not** P15 ranking engine. **Not** commission/profit engine (no evidence) |
| Transient package result | DynamicPackage-owned ephemeral candidate id + provenance; **not** SearchDocument SoT; **not** a booking |

Preserve: **Search module ≠ live supplier truth.** Do not store live Flight/Hotel availability in `search` as authoritative. Zero production sources remain valid: package search must fail honestly, never fabricate a combination.

---

## 9. Package candidate vs accepted transaction

Required separations (names OPEN):

```text
DynamicPackageCandidate / search result
  != DynamicPackageOffer (accepted combination snapshot)
    != DynamicPackageBooking (durable orchestration)
      != FlightBooking
      != HotelBooking
      != Payment
```

A transient combination is **not** transaction truth. Accepting a candidate must revalidate **both** component offers immediately (Flight `IFlightOfferAvailabilitySource` / `IFlightOfferSource` + Hotel rate revalidation). Timeout/Unknown/Changed on either side cannot accept. No silent substitute of a different flight or hotel.

---

## 10. Price authority

Do **not** generalize P12 Pricing into a package/airline/hotel fare engine.

| Layer | Authority |
|-------|-----------|
| Flight money | `FlightBookingMonetarySnapshot` (Flight-owned, immutable) |
| Hotel money | `HotelBookingMonetarySnapshot` (HotelBooking-owned, immutable) |
| Package money | Recommended: DynamicPackage-owned **immutable composition snapshot** = same-currency sum of component customer-payable amounts. Not a new Pricing Quote. Not FX. |

Breakdown recommendation (OPEN): store component totals + package total + currency; do not re-derive taxes/fees at package level.

Package-owned fees / markup: **no repository evidence** → **DEFERRED**.
Package discount: see §12 → **DEFERRED**.

Hotel `PayableAtProperty` / PayAtProperty remains P21-DEFERRED; package PayNow should use the hotel amount that PayNow actually collects (`PayableNow` if present, else `Total`).

---

## 11. Currency rule

| Case | Baseline recommendation (OPEN) |
|------|--------------------------------|
| Same CurrencyCode on both snapshots | Allow. Package total = Flight.Total + Hotel PayNow amount. Persist both components + sum. No FX. |
| Different CurrencyCodes | **Reject in baseline** (or DEFER mixed-currency packages). **No implicit FX.** Do not convert via Pricing ExchangeRate to invent a single charge. |
| Toman | Display-only. Never persist as `CurrencyCode`. IRR ≠ Toman (ADR 0003). |

If architect later wants mixed-currency packages, that is either two customer charges or an explicit FX lock — **not** silent conversion in P23.

---

## 12. Package discount

Repository/business evidence for a genuine Flight+Hotel package discount: **NONE**.

Tour Pricing has TourRate/Quote for **Tour products**, not live supplier pairs.

**Recommend DEFERRED.** Do not invent discount economics, allocation across components, or “package save 10%” without a later lock. A deferred discount would also collide with Partial Refund DEFERRED if one component later fails.

---

## 13. Availability consistency (the race)

Flight available at T1 and Hotel available at T2 does **not** mean the pair is still available at commit.

Required before customer commitment (recommended):

1. Revalidate Flight offer (availability + fare).
2. Revalidate Hotel rate/availability.
3. Take **HotelAvailabilityHold** (Hotel already has this process; covers full room set).
4. Create **FlightSupplierReservation** (PNR-as-hold; Flight has no `FlightAvailabilityHold`).
5. If either step fails/expires/Unknown: release/cancel the other via that module’s contracts; do not proceed to Payment.
6. Only then collect Payment.

Do not treat Search results, cached combinations, or client-posted prices as availability.

---

## 14. Reservation ordering

Compare:

| Option | Sequence | Verdict |
|--------|----------|---------|
| A. Flight reservation first → Hotel hold/reservation | Spends PNR TTL before hotel is locked | Hotel may then fail; PNR must be cancelled. Workable but burns the scarcer/more expensive hold first. |
| B. Hotel hold first → Flight reservation | Uses Hotel hold (pre-payment) then Flight PNR (pre-payment) | **Better inventory order.** Still must not do Hotel **final** reservation before Payment (PayNow lock). |
| C. Parallel reservation | Parallel PNR + hotel final reserve | Violates Hotel PayNow (final reserve needs Payment). Parallel hold+PNR is possible later; baseline should stay serial for compensation clarity. |
| **D. Evidence-based hybrid (recommended)** | Revalidate both → **Hotel hold** → **Flight PNR** → **Package Payment** → **Hotel final reservation** → **Flight ticketing** | Preserves Hotel PayNow **and** Flight PNR-first. No distributed transaction. |

**PLAN recommendation (not a lock):** **D**.

Rationale:

- Hotel `PaymentRequiredForConfirmation` / PayNow: payment before **final** reservation — preserved.
- Flight `PaymentRequiredForReservation = false`: PNR before payment — preserved.
- Flight tickets after payment — preserved.
- Do **not** copy PNR-first onto Hotel or PayNow onto Flight PNR creation.

PNR can expire (`ReservationExpiresAt`). Hotel hold can expire. Ambiguous supplier outcomes stay Unknown/Initiated and must be **rechecked** before compensate. Timeout ≠ Failed.

---

## 15. Atomicity posture

**There is no distributed transaction across Flight / Hotel / Payment / suppliers.**

P23 needs a **saga / orchestration / compensation** process owned by DynamicPackage:

- Local DB transaction only inside `dynamic_package` for package state + outbox.
- Component work via Flight/HotelBooking/Payment application contracts.
- At-least-once delivery + consumer inbox idempotency (existing 29/outbox pattern).
- **Do not claim exactly-once** at suppliers.

Crash between steps is a first-class reconciliation case, not an exception.

---

## 16. Failure matrix

| Case | Retry | Recheck | Compensation | Notes |
|------|-------|---------|--------------|-------|
| Flight succeeds (PNR), Hotel hold/rate fails | No fake hotel | Recheck hotel Unknown | Cancel/void Flight PNR via Flight contracts; release nothing on hotel | Before Payment |
| Hotel hold succeeds, Flight PNR fails | No fake PNR | Recheck Flight Unknown | Release Hotel hold via HotelBooking contracts | Before Payment |
| Flight PNR ambiguous | Do not Fail locally | `ReservationQuery` | Do not release hotel or pay until resolved | Timeout ≠ Failed |
| Hotel hold/reservation ambiguous | Do not Fail locally | Hotel query | Do not ticket / do not Refund yet | Same as P21 |
| Component offer expires before accept | Revalidate/requote | — | No reservation | No silent replace |
| One component reprices | Reject accept | Show new totals | None | No silent package total patch |
| Payment succeeds, Hotel final reservation cannot complete | Recheck hotel first | Authoritative inability | **Full** package Refund (Payment) + cancel Flight PNR (not yet ticketed / ticketed per Flight rules) | Partial keep-flight **blocked** by Partial Refund DEFERRED |
| Payment succeeds, Flight ticketing cannot complete | Recheck tickets | Authoritative inability | Full package Refund + cancel Hotel reservation (Hotel R7 path if already reserved) | Keep-hotel **blocked** by Partial Refund |
| Only one component becomes Confirmed | Recheck the other | — | Treat as incomplete package; compensate the confirmed side via **that module’s** cancel path; full package Refund if money moved | Do not mark package Confirmed |
| Crash between component operations | Resume saga from durable package state | Recheck both + Payment | Continue or compensate; never assume supplier success from local silence | Inbox/outbox |
| Duplicate callback / retry | Idempotent ignore | — | None | DB-backed idempotency |
| Customer cancel needing partial money | Reject before supplier side effects | — | None | Same P21/P22 block |

---

## 17. Payment architecture

Current Payment target model: **exactly** `TourBooking`, `HotelBooking`, `FlightBooking`. Not an open `TargetType` platform.

| Option | Meaning | Fit |
|--------|---------|-----|
| **A. Fourth explicit typed target `DynamicPackageBooking`** | One customer Payment; amount from package monetary snapshot | **Recommended** for one-charge UX. Same extension pattern as P21 (HotelBooking) and P22 (FlightBooking). **Do not implement in PLAN/T001.** |
| B. Separate Flight + Hotel Payments | Two charges; each component keeps today’s 1:1 Payment | Preserves current Payment enum; worse UX; still needs orchestration for order. Useful fallback if architect rejects A. |
| C. Generic TargetType/TargetId | Open payment platform | **Forbidden.** |
| D. Package Payment plus hidden component Payments | Double-charge risk / split fiction | **Forbidden** unless a later lock defines internal non-customer obligations (no evidence). |

**PLAN recommendation (not a lock):** **A**.

Implication (OPEN, must not silently mutate P22-R6 / P21-R6):

- Customer does **not** pay FlightBooking and HotelBooking separately in baseline.
- Flight ticketing and Hotel final reservation consume **package Payment succeeded evidence** via Contracts (new correlation), not by inventing a second customer charge and not by peer Payment DbContext writes.
- Component modules remain confirmation owners: Flight still requires reservation + payment evidence + tickets; Hotel still requires payment evidence **before** final reservation + supplier confirmation.
- `one FlightBooking → one Payment` / `one HotelBooking → one Payment` remains true for **standalone** journeys; package journey is a **new** target kind, not a reuse of those Payments.

Do **not** implement the fourth kind in this PLAN.

---

## 18. One-charge customer experience

**Recommend YES** — one customer Payment for the package.

Combined monetary obligation comes from the **package monetary snapshot** (immutable same-currency sum of component snapshots), never from the client.

If architect chooses two charges (B): UX must show two payment steps; confirmation of the package waits for both; failure of the second still needs compensation of the first (full Refund of that component Payment — executable today) plus supplier cancel. That is safer under Partial Refund DEFERRED but is **not** “one package purchase”.

Do not silently assume one-charge while still creating two Payment rows.

---

## 19. Refund dependency (Partial Refund = DEFERRED)

Dynamic packages are sensitive because one component may fail or cancel while the other remains valid.

| Slice | Blocked now? |
|-------|----------------|
| Baseline: any post-pay inability → **full** package Refund + compensate **both** components | **NO** — full Refund exists |
| Keep the successful component and refund only the failed share | **YES** — Partial Refund DEFERRED |
| Independent component cancellation after package Confirmed | **YES** — leftover component would need partial money movement or a second commercial product |
| Package discount then reverse one component | **YES** (discount already DEFERRED) |
| Executable Partial Refund implementation in P23 | **OUT** — do not implement Partial Refund during planning or P23 product tasks |

**This limitation blocks those slices, not the whole phase**, provided baseline compensation is always **all-or-nothing** after money movement.

Standalone Flight/Hotel cancellation economics (FullRefund/NoRefund) still apply **inside** each module when the package saga asks that module to cancel. If **either** component quotes `PartialRefundRequired`, package cancel/compensation of that path is **rejected before supplier side effects** — same as P21/P22.

---

## 20. Cancellation semantics

| Choice | Recommendation (OPEN) |
|--------|----------------------|
| Cancel whole package only | **IN baseline** |
| Component cancellation allowed | **DEFERRED** until Partial Refund (or an explicit “keep remaining component as standalone booking” lock — no evidence; would also need token/UX split) |
| Flight cancellation independent | **DEFERRED** for package-owned bookings |
| Hotel cancellation independent | **DEFERRED** for package-owned bookings |

Safely implementable now: package cancellation process that (1) evaluates both component penalty snapshots at `RequestedAt`, (2) rejects if either needs Partial Refund, (3) if both FullRefund or both NoRefund consistently, runs each module’s cancel contracts, (4) requests Payment full Refund only after authoritative supplier reversals where money must return, (5) `DynamicPackageCancelled != RefundSucceeded`.

Ambiguous mixed economics (flight FullRefund + hotel NoRefund) is a **partial money** problem → **blocked** while Partial Refund DEFERRED. Flag as R7 blocker for that mixed-penalty slice; do not invent netting.

---

## 21. Component status vs package status

P23 **does** need a package lifecycle. Keep it **minimal**. Do not duplicate `FlightBookingStatus`, `HotelBookingStatus`, `PaymentStatus`, PNR, tickets, hold, or hotel reservation states.

Recommended package statuses (OPEN):

| Status | Meaning |
|--------|---------|
| `Pending` | Orchestration in progress; not both components Confirmed |
| `Confirmed` | Package confirmation evidence met (see §22) |
| `Cancelled` | Package cancellation process completed at package level |

Avoid mega-status values like `FlightTicketedHotelHeld`. Present those as **composed read facts**.

Optional internal saga step enum (process state, not `DynamicPackageBookingStatus`) may exist later — keep it out of customer-facing status.

---

## 22. Confirmation semantics

A package transaction may become **Confirmed** only when **all** of the following are true (recommendation, OPEN):

1. `FlightBooking.Status == Confirmed` — which itself requires reservation Confirmed **and** Payment succeeded evidence **and** all required tickets Issued.
2. `HotelBooking.Status == Confirmed` — which itself requires Payment succeeded evidence **and** supplier reservation Confirmed (PayNow).
3. Package Payment `Succeeded` (if R6 locks one-charge target A), amount/currency matching the package monetary snapshot.
4. Package still references those exact component ids (no silent swap).

Do **not** reduce this to one boolean on the package. Public UX may show a single “confirmed” only when the composed evidence is complete. Payment-only, PNR-only, hotel-hold-only, or one-component-Confirmed **must not** confirm the package.

---

## 23. Compensation semantics

Package orchestration may need to request:

| Action | Owner | How |
|--------|-------|-----|
| Flight cancellation / PNR cancel / ticket void | Flight | Flight contracts + Flight R7 process |
| Hotel hold release / hotel cancellation | HotelBooking | HotelBooking contracts + Hotel R7 process |
| Customer Refund | Payment | Payment-owned full Refund |

DynamicPackage **orchestrates**; it does not write Flight/Hotel/Payment tables. No peer Infrastructure access. No cross-schema SQL.

Refund is requested only after authoritative inability or authoritative supplier reversal, never on timeout.

---

## 24. Public UX

Candidate public journey (do not lock exact endpoints):

```text
Search Flight + Hotel
→ compare combinations (transient candidates)
→ select package
→ passenger details (Flight) + guest details (Hotel) + contact
→ authoritative revalidation
→ Hotel hold + Flight PNR
→ one Payment
→ Hotel final reservation + Flight ticketing
→ package confirmation
```

Auth: **package-specific** anonymous token (do **not** reuse Flight/Hotel/Tour headers). Object-level 404. Raw token once; SHA-256 verifier; not in URL/localStorage. Component ids are not credentials.

FA / EN / AR · Server Component first · mobile/accessibility/bidi. Flight times with timezone/airport context. Mixed-direction names remain bidi-safe.

No card collection. Browser return ≠ Payment success ≠ package Confirmed.

---

## 25. Existing identifiers reuse / PII

Passenger (Flight) and Guest (Hotel) facts may be **copied at initiation** from the same form into each component’s existing snapshot types.

**Do not** introduce a shared mutable Passenger/Guest aggregate.

Hotel does not store Infant/BirthDate/passport. Flight does not store hotel lead-guest-on-room assignment. Contact email/phone may be duplicated into both contact snapshots plus an optional package contact snapshot for orchestration — still immutable transaction-time copies.

Do **not** broaden Flight PII merely because Hotel has other guest facts (or vice versa). No document scans. Redact in logs/DTOs.

---

## 26. SEO

| Page class | Posture |
|------------|---------|
| Discovery (package search/landing) | SEO may later allow indexation **only** with unique purpose/content; default missing IndexPolicy = noindex,follow. Do not mass-produce thin combination URLs. |
| Private transaction (package booking, payment, confirmation, cancel) | **noindex** — same as Flight/Hotel/Tour booking pages. SEO remains policy owner; product must not hardcode index. |

P23 does not wait for P26 Advanced SEO.

---

## 27. Operational support

Internal-only operational read composing, via contracts (not cross-schema SQL):

- Package id / status / saga step / timestamps
- Logical FlightBookingId + HotelBookingId
- Component statuses, PNR/locator, tickets, hold, hotel reservation
- Package + component monetary snapshots
- Payment / Refund ids and statuses
- Reconciliation issues

**No** ForceConfirm / ForceTicket / MarkPaid / ForceCancel. Trusted recheck may call each module’s authoritative query ports.

---

## 28. Supplier posture (keep truthful)

| Source | Value |
|--------|-------|
| Production Flight Search Source | NONE |
| Production Flight Availability Source | NONE |
| Production Flight Offer Source | NONE |
| Production Flight Reservation Source | NONE |
| Production Flight Ticketing Source | NONE |
| Production Flight Cancellation Source | NONE |
| Named Flight Supplier | NONE |
| Production Hotel Availability Source | NONE |
| Production Hotel Rate Source | NONE |
| Production Hotel Reservation Source | NONE |
| Named Hotel Supplier | NONE |
| Production Payment Provider | NONE |
| Named Payment Provider | NONE |
| Supplier SDKs | NO |

Do **not** add named suppliers/providers. Zero-source host remains valid. No fake production combinations.

---

## 29. AI readiness (structural only)

Ready later if P23 stores: package id, component ids, locale, currency, immutable snapshots, statuses, provenance (which source/offer ids), timestamps (NodaTime).

**Do not introduce:** LLM, RAG, vector DB, embeddings, AI orchestration, chatbot ranking of packages.

---

## 30. Decision inventory (must stay OPEN)

| ID | Topic | Status |
|----|-------|--------|
| **P23-R1** | Ownership / module / schema / transaction boundary (`DynamicPackage` vs Booking vs Tour vs no owner) | **RESOLVED** — independent DynamicPackage module · schema `dynamic_package` · DynamicPackageBooking owned inside DynamicPackage · **DynamicPackage != Tour** · **DynamicPackage != Tour Booking** · **DynamicPackage != Flight** · **DynamicPackage != HotelBooking** · **DynamicPackageBooking != FlightBooking** · **DynamicPackageBooking != HotelBooking** · **Tour Package Flight != live Flight inventory** · Flight/Hotel/Payment execution ownership unchanged |
| **P23-R2** | Component composition cardinality + package lifecycle statuses | **OPEN** |
| **P23-R3** | Search / combination / revalidation authority | **OPEN** |
| **P23-R4** | Package quote / monetary snapshot / currency / discount | **OPEN** |
| **P23-R5** | Reservation orchestration / hold-PNR order / idempotency / reconciliation | **OPEN** |
| **P23-R6** | Payment ordering / typed target / one-charge / confirmation / compensation | **OPEN** |
| **P23-R7** | Cancellation / refund / Partial Refund dependency | **OPEN** |
| **P23-R8** | Public UX / auth token / privacy / operations / SEO | **OPEN** |

Do not treat PLAN recommendations as locks.

Inherited locked facts (not new P23 decisions): FlightBooking ≠ HotelBooking; Hotel Catalog ≠ HotelBooking; Tour transport ≠ live Flight; Price ≠ Quote ≠ Booking ≠ Payment; Payment owns money movement; Search ≠ transaction SoT; no shared DbContext; schema-per-module; no peer-schema FK; no distributed transaction; UUIDv7; NodaTime; Money/Currency; no BookingBase; Partial Refund DEFERRED; Flight production sources NONE; Hotel production sources NONE; Named suppliers NONE; Production Payment Provider NONE; Flight triple-evidence confirmation; Hotel PayNow; MultiCity DEFERRED unless a later lock says otherwise.

---

## 31. Task sequence

Do **not** execute any of these in this PLAN task.

### TC-P23-T001 — DynamicPackage module / schema foundation

- Depends on **P23-R1**. **IMPLEMENTED / AWAITING_ARCHITECT_REVIEW.** Independent `DynamicPackage.Contracts` / `DynamicPackage.Domain` / `DynamicPackage.Infrastructure` · schema `dynamic_package` · DynamicPackageBooking ownership assigned to DynamicPackage without implementing the aggregate · no Payment kind yet · no Flight/Hotel behavior change · **TC-P23-T002 EXECUTED**.

### TC-P23-T002 — Composition boundary only (P23-R2)

- Depends on **P23-R2**. Exactly one `FlightBookingId` reference + exactly one `HotelBookingId` reference.
- No `DynamicPackageBooking` aggregate, no package lifecycle/status, no payment/reservation/orchestration/quote/offer/money.

### TC-P23-T003 — Search composition / revalidation

- Depends on **P23-R3**. **TC-P23-T003 EXECUTED.** Transient candidate composed from exactly one FlightComponent reference and exactly one HotelComponent reference; non-persistent and non-transactional; no PackageBooking/payment/reservation/orchestration.

### TC-P23-T004 — Package quote / monetary boundary

- Depends on **P23-R4**. **TC-P23-T004 EXECUTED.** Transient PackageMonetarySnapshot (FlightTotal + HotelTotal = PackageTotal, same-currency enforced per ADR 0003, mixed-currency rejected). TransientPackageQuote combines candidate + monetary. DynamicPackage is NOT price authority. Discount/markup/commission: DEFERRED. No persistence. No Payment change.

### TC-P23-T005 — Orchestration boundary

- Depends on **P23-R5**. **TC-P23-T005 EXECUTED.** Transient PackageOrchestrationPlan: choreography via outbox/inbox, no distributed transactions, no saga, no compensation implemented. DynamicPackage coordinates Flight+Hotel+Payment lifecycles but does NOT own their execution. Failure boundaries documented only.

### TC-P23-T006 — Payment boundary

- Depends on **P23-R6**. **TC-P23-T006 EXECUTED.** PackagePaymentBoundary: no new PaymentTargetKind (requires DynamicPackageBooking aggregate first), component payments remain component-owned, transient obligation only. No distributed transactions, no compensation implemented, no Payment/Flight/Hotel changes.

### TC-P23-T007 — Package confirmation / consistency boundary

- Depends on **P23-R7**. **TC-P23-T007 EXECUTED.** Defines confirmation posture via transient `TransientPackageConfirmation` (plus `PackageConfirmationBoundary`), with confirmation meaning + consistency only; no persistence, no saga, no compensation, no payment execution, no public API.

### TC-P23-T008 — Public UX / auth / privacy / operational reads / SEO

- Depends on **P23-R8**. **TC-P23-T008 EXECUTED.** Defines DynamicPackage public journey boundary posture (no supplier/payment integration, no token reuse, discovery index allowed, transactional noindex, no operational mutation, no distributed transactions); no production API/UI implemented.

### TC-P23-T009 — Hardening + evidence

- Guardrails + evidence pack; **no new capability**. READY_FOR_GATE only after R1–R8 locked and T001–T008 accepted.

### TC-P23-GATE — Acceptance Gate

- Evidence only. No new product in GATE. Do not start P24 inside GATE.

---

## 32. Scope classification (IN / OUT / DEFER)

Classifications are **planning inventory**, not architect locks.

| Concept | Classification | Notes |
|---------|----------------|-------|
| New DynamicPackage module + schema `dynamic_package` | **IN (candidate)** | Locked only by **P23-R1** |
| Durable `DynamicPackageBooking` | **IN (candidate)** | R1 |
| Exactly 1 FlightBooking + 1 HotelBooking | **IN (candidate)** | R2 |
| Round-trip + connecting segments via existing FlightBooking | **IN** | Reuse P22 |
| Multi-room via existing HotelBooking | **IN** | Reuse P21 |
| Transient package candidates + both-side revalidation | **IN (candidate)** | R3 |
| Package monetary snapshot as same-currency sum | **IN (candidate)** | R4 |
| Fourth typed Payment target | **IN (candidate, later T006)** | R6; **not T001** |
| One customer charge | **IN (candidate)** | R6 |
| Saga D: hold → PNR → pay → hotel reserve → ticket | **IN (candidate)** | R5 |
| Whole-package cancellation | **IN (candidate)** | R7 |
| Package-specific access token | **IN (candidate)** | R8 |
| Generic Booking / BookingBase / Booking&lt;T&gt; | **OUT** | |
| TourBooking as live Flight+Hotel | **OUT** | |
| Fold into Flight or HotelBooking | **OUT** | |
| Peer-schema FK / shared DbContext / distributed TX | **OUT** | |
| Generalize P12 Pricing | **OUT** | |
| Named supplier / SDK / fake production source | **OUT** | |
| Implement Partial Refund | **OUT** | |
| MultiCity | **DEFERRED** | Not required by P23 |
| Package discount / markup / owned fees | **DEFERRED** | No evidence |
| Mixed-currency one-charge | **DEFERRED** | No implicit FX |
| Independent component cancel / keep-one-after-fail | **DEFERRED** | Partial Refund |
| PayAtProperty / deposit / pay-later | **DEFERRED** | P21/P22 |
| Ancillaries / amendments / smart routing | **DEFERRED** | |
| Agency package commerce | **OUT (P24)** | |
| Notification sending | **OUT (P25)** | |
| LLM / RAG / vector DB | **OUT** | |

---

## 33. Risk register

| Risk | Why it matters |
|------|----------------|
| Distributed consistency | Two suppliers + Payment; no 2PC; saga/compensation required |
| Cross-component expiry | PNR TTL vs hotel hold TTL vs offer expiry vs ticketing deadline |
| Ambiguous external states | Timeout must not trigger Refund or cancel |
| One-charge vs two-charge | One-charge needs fourth Payment target **and** all-or-nothing Refund; two-charge splits UX |
| Partial Refund limitation | Blocks keep-one, component cancel, mixed penalties |
| Cancellation compensation | Must call both modules then Payment; order and ambiguity matter |
| Package repricing | Pair may change independently; silent patch forbidden |
| Duplicated passenger/guest PII | Copy snapshots; no shared mutable person |
| Accidental generic Booking | Highest ownership risk |
| Accidental Pricing generalization | P12 is Tour-shaped |
| Copying Flight PNR-first onto Hotel | Would violate PayNow |
| Copying Hotel PayNow onto Flight PNR | Would violate P22-R6 |
| Treating Search as live combination SoT | Stale pairs |
| Schema not in `07` / matrix | R1 must update SoT when locked; PLAN must not pretend it already exists |

---

## 34. Architecture invariants (carry forward)

1. FlightBooking ≠ HotelBooking ≠ TourBooking ≠ DynamicPackageBooking.
2. Hotel Catalog ≠ HotelBooking. Tour package transport ≠ live Flight.
3. Price ≠ Quote ≠ Booking ≠ Payment. Payment owns money movement.
4. Search ≠ live offer SoT ≠ transaction SoT.
5. No shared DbContext. No peer-schema FK. No peer Infrastructure dependency. No distributed transaction.
6. Client is not monetary, availability, or success authority.
7. Browser return ≠ Payment success ≠ package Confirmed.
8. Hotel PayNow and Flight PNR-first both survive inside the saga.
9. Partial Refund remains DEFERRED.
10. No fake production Flight/Hotel/Payment source.
11. MultiCity remains DEFERRED unless a later explicit lock.
12. No LLM/RAG/vector DB.

---

## 35. Conflicts / documentation gaps / blockers

**No blocker for PLAN authorship.** Working tree at PLAN start was `ed040f0` == `origin/main` with only the captured envelope untracked.

Non-blocking gaps:

- `04-module-boundaries.md`, ownership matrix, `03-domain-map.md`, and `07-data-architecture.md` have **no Dynamic Package module/schema**. `15` § U only mentions “later dynamic packages” as a Flight consumer. R1 should add SoT rows when locked — **do not add them in this PLAN task** beyond PROJECT-STATE/ROADMAP status.
- `08-persistence-and-migrations.md` examples mention `hotel_booking`, not package schema.
- Glossary “package” still means Tour Foreign Package, not P23.

**Blockers for later slices (not for planning):**

1. **Partial Refund DEFERRED** — keep-one-after-fail, independent component cancel, mixed FullRefund+NoRefund package cancel, discount reversal.
2. **Payment closed enum** — one-charge requires P23-R6 lock to add a fourth **explicit** kind (implementation in T006, not T001).
3. **Production sources NONE** — not a phase blocker (P21/P22 proved zero-source is valid); blocks only real combined purchase in production.

**Source-of-Truth conflict:** **NO**.

---

## 36. Gate criteria (phase, not this PLAN)

P23 may complete later only if R1–R8 are resolved, T001–T009 accepted, validation green, deferred limitations explicit (especially Partial Refund and MultiCity), and no fake production capability is claimed.

This PLAN task is **not** Gate-ready and must not mark P23 COMPLETE or READY_FOR_GATE.

---

## 37. Repository safety

- Branch `main` · T001 adds DynamicPackage foundation only (schema EnsureSchema; no product tables; no endpoints).
- **No** DynamicPackageBooking aggregate · **no** fourth Payment target · **no** Flight/Hotel/Payment/Pricing behavior change.
- Do **not** execute `TC-P23-T009` until T008 public journey boundary validated and P23-R8 locked.

---

## 38. PLAN Done criteria

- Dynamic Package phase title: **P23 — Dynamic Package / Flight + Hotel**
- Recommended owner: **new DynamicPackage module** (**RESOLVED at R1**)
- Recommended schema: **`dynamic_package`** (**RESOLVED at R1**)
- New persistent package aggregate: **YES** — `DynamicPackageBooking` (ownership assigned; aggregate **not implemented** in T001)
- FlightBooking ownership unchanged: **YES**
- HotelBooking ownership unchanged: **YES**
- P23-R1: **RESOLVED**
- P23-R2 through P23-R8: **OPEN**
- T001–T009 + GATE sequenced
- T001 executed: **YES** (awaiting architect review)
- P23 COMPLETE: **NO**
