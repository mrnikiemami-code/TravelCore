# P22 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P22-PLAN` |
| Phase | P22 — Flight |
| Status | PLAN ACCEPTED · **P22-R1 = RESOLVED** · **P22-R2 = RESOLVED** · **P22-R3 = RESOLVED** · **P22-R4–R8 OPEN** · T001–T002 ACCEPTED · T003 implemented / awaiting review · **TC-P22-T004 NOT EXECUTED** |
| Baseline | `d6bd842` (`docs(hotel-booking): add TC-P21-GATE result envelope` · GATE evidence `858b4be` · architect `TC-P21-GATE = ACCEPTED`) |
| Authoritative sources | `docs/ROADMAP.md` § P22 · `docs/PROJECT-STATE.md` · `04-module-boundaries.md` § Flight / Tour · `docs/domain/module-ownership-matrix.md` · `07-data-architecture.md` (schema `flight`) · `06-cross-module-communication.md` Example 7 · `15-future-architecture-transition-map.md` § U · P11-R5 (`TourDepartureTransportSegment`) · P12 Pricing · P19 Booking · P20 Payment · P21 HotelBooking · ADR 0003 (Money) · ADR 0004 (NodaTime) |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی P22** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط یافته‌های Repository، گزینه‌های معماری، موجودی تصمیم، و Taskهای اجرایی را برای Cursor تعریف می‌کند.

> **Envelope note:** Authored from **repository SoT** after architect `TC-P21-GATE` ACCEPT (`858b4be` / docs `d6bd842`). Next phase is **explicitly** `P22 — Flight` in `docs/ROADMAP.md` (not guessed). **No product code in PLAN task.** Open R# stay OPEN until architect lock. **Do not implement T001 until this PLAN is ACCEPTED and P22-R1 is architecturally locked.** Do **not** invent P22-R1 through P22-R8 closures here. Do **not** start P23.

---

## 0. Next-phase resolve (from SoT; no extra discovery task)

| Question | Answer from SoT |
|----------|-----------------|
| P21 completion | **COMPLETE / ACCEPTED** — Gate evidence `858b4be`; result docs `d6bd842`; architect ACCEPT issued this session |
| Authoritative next phase ID | **P22** |
| Title / purpose | **Flight** — provider abstraction · airport/reference · one-way · round-trip · multi-city در صورت تأیید · search · fare · baggage · passenger rules · Quote · booking/order · provider references (`docs/ROADMAP.md` § P22) |
| PLAN already existed? | **NO** — this document is the first P22 PLAN |
| SoT conflict? | **NO** — ROADMAP names P22 after P21; constitution already names Flight as an External/commerce module; schema `flight` is listed in `07-data-architecture.md`. **Tour package FlightSegment ≠ live Flight inventory** is already locked. |
| Dedicated module/schema in SoT today? | **YES (conceptual)** — Flight module exists in `04-module-boundaries.md`; PostgreSQL schema `flight` is listed. **No Flight product code / DbContext / aggregates exist.** |
| Tour flight representation today? | **Descriptive only** — `TourDepartureTransportSegment` (P11-R5): Sequence / TransportMode / Origin+Destination **labels**. No airline, flight number, ticket, seat, or live inventory. |
| Airport catalog owner today? | **Not implemented.** Ownership matrix: Flight **references** ReferenceData for airport/carrier. DestinationKind is Country/Region/City/Area (**not Airport**). Place is Hotel/Restaurant/Attraction (**not Airport**). |
| Payment Flight target today? | **NO** — `PaymentTargetKind` is closed: `TourBooking`, `HotelBooking` only. Tests explicitly forbid `Flight`. |
| Missing business fact blocking PLAN authorship? | **NO** — PLAN may enumerate R# and IN/OUT/DEFER without locking product semantics or choosing a supplier |
| Invented phase? | **NO** — P22 is already listed in ROADMAP |
| Speculative provider? | **NONE selected** — no named GDS/NDC/SDK in repository |

---

## 1. Phase Purpose

P22 باید قابلیت **Flight** را به‌عنوان دامنهٔ تراکنشی مستقل معرفی کند: جستجو/پیشنهاد/رزرو زندهٔ پرواز در برابر موجودی/offer قابل رزرو — **بدون** دزدیدن مالکیت Tour package transport، Tour Booking، HotelBooking، Place catalog، Pricing/Quote (تا طراحی صریح)، Payment execution، Search, SEO, Notification delivery, or Agency settlement.

Preserve (already locked; PLAN does not reopen them):

1. **Tour Package Flight ≠ live Flight inventory** — `04-module-boundaries.md` § Tour/Flight · matrix · `15` § U · `06` Example 7.
2. **HotelBooking ≠ Tour Booking ≠ Flight** — do **not** create `Booking<T>` or fold Flight into P19 Booking.
3. **Pricing dependency is not automatic** — mandatory Flight→Pricing ownership is forbidden until explicitly designed (`04` Pricing scope warning).
4. **Payment currently supports exactly two typed targets** — TourBooking and HotelBooking. Flight is **not** a Payment target today. Any third kind must be an explicit later R# lock, not a generic `TargetType` platform.
5. **Partial Refund = DEFERRED** (P20/P21). P22 must not hide this if executable Flight cancellation needs partial money movement.
6. Money = Amount + CurrencyCode. No Toman CurrencyCode. No float monetary persistence (ADR 0003).
7. Temporal facts use NodaTime. **Do not use server-local time** (ADR 0004). Flight local times need airport/zone context (`docs/data/03-temporal-model.md` § Airport IKA example).
8. **PublicExperience = composition only.** Search is not live-availability SoT. Transaction pages stay noindex unless SEO later locks otherwise.
9. Module-local transactional outbox / consumer-owned inbox. No second event bus. No peer-schema FK. No shared DbContext.
10. **AI-readiness = structured attributable facts** — identifiers, itinerary, statuses, provider references — **بدون** LLM/RAG/vector DB.

P11 delivered descriptive Tour transport. P19/P20/P21 delivered Tour Booking / Payment / HotelBooking. P22 must not collapse those owners.

---

## 2. Starting Baseline

| Item | Value |
|------|--------|
| P21 Gate | `TC-P21-GATE` COMPLETE / ACCEPTED (`858b4be` · docs `d6bd842`) |
| P21 evidence | [`P21-GATE-acceptance-evidence.md`](P21-GATE-acceptance-evidence.md) |
| Baseline HEAD | `d6bd842` |
| P00–P21 | COMPLETE |
| Flight module / schema | **Conceptual only** (`04-module-boundaries.md` · schema `flight`) — no product code |
| Tour transport | `TourDepartureTransportSegment` — labels + `TourDepartureTransportMode` Air/Ground/Other (`src/backend/Modules/Tour/.../TourDepartureTransportSegment.cs`) |
| Frontend Tour flight display | Presentation-only `FlightSegmentView` + fixtures (`src/frontend/web/src/types/pages/foreign-tour-detail.ts`) — **not** live Flight SoT |
| Airport / Airline catalogs | **NONE** in ReferenceData product code |
| Payment | Closed kinds TourBooking + HotelBooking; Production Payment Provider = NONE; Partial Refund = DEFERRED |
| Named Flight supplier SDK | **NONE** |
| Existing Flight APIs / routes | **NONE** |
| Host composition | `Program.cs` registers HotelBooking after Payment; **no FlightModule** |

---

## 3. Existing Tour flight representation (exact)

| Finding | Evidence |
|---------|----------|
| Canonical glossary `FlightSegment` | Richer descriptive package facts (airport, carrier, flight number, local times, cabin, baggage) — **glossary, not current product** (`docs/domain/glossary.md`) |
| Implemented Tour type | `TourDepartureTransportSegment` — Sequence, Mode, Origin label, Destination label only |
| Explicit domain comment | “not Flight entity, airline, ticket, seat inventory, or Booking” |
| P11-R5 lock | Tour ≠ Flight; no airline/flight number/ticket/seat inventory |
| Public Tour query | Maps ordered transport segments for package display |
| Frontend fixture | `originAirportCode` / `destinationAirportCode` / optional carrier+flightNumber — composition for Foreign Tour page, not Flight module |
| Live inventory leak | **NO** — no Flight DbContext, no GDS client, no PNR |

**Locked distinction to preserve:**

```text
Tour Package Flight / TourDepartureTransportSegment
!=
live Flight inventory / FlightBooking
```

Tour product stability must survive disappearance of a live Flight offer. Do **not** assume TourBooking consumes FlightBooking. Dynamic packaging is **P23**, not P22.

---

## 4. Flight domain ownership alternatives

### Candidate A — Independent Flight module (recommended)

One External module `Flight` owning live search/offer ports, itinerary/offer snapshots, `FlightBooking`, supplier reservation/PNR, ticketing process, and cancellation process. Schema `flight`. Airport/Airline **catalogs** stay ReferenceData (logical IATA/carrier codes; not yet implemented). Matches `04-module-boundaries.md` and ownership matrix row “Live flight offer / provider booking = Flight”.

Pros: same pattern as HotelBooking (transaction module ≠ catalog owner); one persistence boundary; host can register `FlightModule` after `HotelBookingModule`.
Cons: module will grow (search + booking + ticketing); must keep process aggregates separate so it does not become a mega-status blob.

### Candidate B — Flight catalog/search module separate from FlightBooking transaction module

Mirrors Place vs HotelBooking. For air, the durable catalog is **airport/airline**, already assigned to ReferenceData — not a second “Flight catalog” of scheduled inventory unless TravelCore later owns allotment.

Pros: thinner transaction module.
Cons: extra module/schema without a SoT catalog owner; risks duplicating Hotel’s Place split where no Place-equivalent exists.

### Candidate C — Flight search in Flight; transaction in Booking module

Pros: reuse P19 Booking.
Cons: contradicts HotelBooking ≠ Tour Booking, Identity ≠ Party style ownership, and would create a generic booking platform. **Not recommended.**

**PLAN recommendation (not a lock):** Candidate A. Leave **P22-R1 OPEN**.

**Recommended names (not locked):** module `Flight` · schema `flight` · transactional aggregate `FlightBooking` **inside** Flight (not a separate module). Host registration after HotelBooking.

---

## 5. Airport / Airline / Place / Destination

| Concept | Current owner | P22 implication |
|---------|---------------|-----------------|
| Destination graph | Destination (Country/Region/City/Area) | Logical city/country refs only. DestinationKind is **not** Airport. |
| Place catalog | Place (Hotel/Restaurant/Attraction) | Place is **not** airport catalog. |
| Airport / carrier refs | ReferenceData (matrix B) | **Do not** put airport master data in Flight schema as canonical catalog unless R1/R2 later lock otherwise. |
| IATA vs ICAO | Unimplemented | Do not make both mandatory in baseline. Prefer the codes actually required by search/booking. |
| Geography of airport city | Destination / ReferenceData | Logical references; Flight does not own Destination. |

---

## 6. What “Flight” in P22 represents

ROADMAP + `04` + `15` § U describe **live bookable flight inventory / provider commerce**, not TravelCore-owned scheduled airline operations.

| Alternative | Meaning | Fit |
|-------------|---------|-----|
| A. External supplier-authoritative offers | Search/book against GDS/NDC/airline/charter source | Matches “provider abstraction”; default HotelBooking analogue |
| B. TravelCore-owned allotment/schedule | Internal seat inventory | Not evidenced in SoT; Tour packages are descriptive |
| C. Hybrid | Mix allotment + live | Premature |
| D. Named GDS | Amadeus/Sabre/Travelport | **Forbidden to invent.** Repository is silent. |

**Recommended inventory posture (not locked):** A — external source-authoritative availability/fare. Production sources = **NONE**. Zero-source host remains valid and must not fabricate offers.

Charter vs scheduled: Tour business uses charter **descriptively** today. P22 baseline should not invent TravelCore charter inventory. Charter supplier capability may be a later source capability, not a second domain.

---

## 7. Itinerary / trip-type / passenger (R2 inventory)

Terminology (recommended, OPEN):

| Term | Meaning |
|------|---------|
| FlightItinerary | Complete customer journey (one-way or outbound+inbound) |
| FlightJourney / direction | One direction (outbound or inbound) |
| FlightSegment | One marketed flight (may include technical stops) |
| FlightLeg | Optional finer hop if a source distinguishes it — do not over-model unless required |

| Topic | Recommendation (OPEN) |
|-------|----------------------|
| One-way | **IN baseline** |
| Round-trip | **IN baseline** as two journeys, one FlightBooking |
| Multi-city | **DEFERRED** (ROADMAP says «در صورت تأیید») |
| Connecting flights | **IN candidate**: 1..N segments per journey. Needed for realistic offers. |
| Connection vs stopover | Document in UX; do not over-model |
| Codeshare | Marketing vs operating carrier as opaque source facts if present; do not overbuild |
| Adult / Child | **IN candidate** (already used in Tour/Hotel/Pricing presentation) |
| Infant | **IN candidate** — flights commonly require lap-infant handling unlike Hotel occupancy. Tour already has `InfantAllowed` on departure rules. |
| Search PII | Origin/destination/dates/pax counts/cabin — **not** passport |
| Booking PII | GivenName / FamilyName minimum; Gender/DOB/nationality/document **only when source/fare requires** |
| Document scans | **OUT** |
| Domestic vs international | Do not split modules; document-requirement timing is R2/R8 |

---

## 8. Search / availability / offer authority (R3)

Distinguish:

```text
schedule/search presentation  !=  live seat availability
live seat availability        !=  fare/price
FlightOffer                   !=  FlightBooking
```

Search module remains retrieval/read-model owner. **Do not** store external live availability in Search as authoritative truth. Public Flight search, if in P22, is Flight-owned transactional/discovery of **live offers**, not P15 index SoT.

Recommended ports (minimum likely; do not create all unless later R# justifies):

- `IFlightSearchSource` / `IFlightOfferSource` (may collapse if one source does both)
- `IFlightReservationSource`
- `IFlightTicketingSource` if reservation ≠ ticket (likely)

Avoid a giant `IFlightSupplierGateway`. Capability descriptors (Search, Revalidate, Reserve, ReservationQuery, Ticket, TicketQuery, Cancel, RefundQuote, Refund) should be explicit **capabilities**, not provider names.

**Hold:** do **not** copy `HotelAvailabilityHold` blindly. Many air sources use PNR + ticketing time limit as the hold. Evaluate `FlightAvailabilityHold` vs PNR-as-hold in R3/R5. Silent repricing is forbidden. Offer expiry is source-authoritative.

Zero production source → truthful unavailable (HTTP 503 analogue of Hotel), never fake offers.

Named Flight Supplier = **NONE**. Production Flight Availability / Pricing / Reservation / Ticketing Source = **NONE**. Supplier SDK = **NO**.

---

## 9. Fare / monetary / Pricing boundary (R4)

P12 Pricing owns **Tour** commercial rates/quotes. Constitution forbids mandatory Flight→Pricing until designed.

**Recommended (OPEN):** Flight owns immutable `FlightOfferSnapshot` and `FlightBookingMonetarySnapshot` (HotelBooking analogue). Do **not** generalize Pricing into an airline tax engine. Minimum breakdown: base + taxes/fees + total if source supplies them; otherwise opaque total with currency.

Baggage allowance = immutable purchased-offer fact, not a global rules engine. Seats/meals/extra baggage/lounge = **DEFERRED** ancillaries.

Revalidation immediately before reservation/payment is likely required because offers are volatile.

---

## 10. Reservation / PNR / ticketing (R5)

Flight differs from Hotel: reservation/PNR may exist **before** tickets.

```text
PaymentSucceeded  !=  PNRConfirmed  !=  TicketIssued
```

Do not use one mega status combining search/offer/hold/payment/PNR/ticket/refund.

Customer-facing “Confirmed” options (OPEN R5/R6):

1. supplier reservation/PNR exists
2. tickets issued
3. dual/triple evidence (Payment + PNR + tickets)

**Recommendation (not lock):** do not present Confirmed until tickets are authoritative **or** a locked dual-evidence rule says otherwise. Partial ticket issuance ≠ whole FlightBooking ticketed.

Network timeout ≠ reservation failed. Ticketing timeout ≠ ticket failed. Plan reservation-query and ticket-query reconciliation. DB-backed idempotency for initiation, reserve, ticket, payment prep, cancel.

Assume one logical supplier reservation per FlightBooking **unless** source evidence later requires multiple PNRs (OPEN).

---

## 11. Payment ordering and P20 dependency (R6)

Current Payment target kinds: **TourBooking, HotelBooking**. Flight = **NO**.

P22 must evaluate a **third typed target** `FlightBooking` the same way P21 added HotelBooking — closed enum, not `TargetType`/`TargetId`. **Do not silently extend Payment in T001.** Do not redesign Payment in this PLAN.

PayNow full collection is the likely baseline (Hotel/Tour). Pay-later / deposit / agency credit = **DEFERRED**. Production Payment Provider remains **NONE**.

Orderings to compare in R6 (do not lock here):

| Order | Inventory risk | Financial risk |
|-------|----------------|----------------|
| A. offer → payment → reserve → ticket | Offer may expire after pay | Compensation if reserve/ticket fails |
| B. offer → PNR/TTL → payment → ticket | PNR hold cost if unpaid | Ticketing deadline vs payment |
| C. payment → ticket-direct | Depends on source | Same compensation need |

Ticket-before-payment is not recommended without agency settlement (P24). Browser return ≠ Payment success ≠ ticket success.

Authoritative Flight amount/currency = accepted `FlightBookingMonetarySnapshot`, never the client.

---

## 12. Cancellation / void / refund (R7)

Distinguish: FlightBooking cancellation process ≠ supplier cancel ≠ ticket void ≠ ticket refund ≠ Payment Refund execution.

Airline economics often involve **penalties and partial refunds**. Current Payment: **Partial Refund = DEFERRED**. If executable P22 customer cancel requires partial money movement, that is an **architectural dependency / blocker** for that slice — do not hide it. Full-refund-only and no-refund paths can reuse P20 Refund like HotelBooking R7. Per-passenger / per-segment cancel = likely **DEFERRED**.

Supplier settlement / accounting / wallet / agency commission = **OUT**.

---

## 13. Public UX / auth / privacy / ops (R8)

Likely public journey (analyze, do not invent final routes): search → results → offer → passengers → booking → payment → confirmation/ticket.

Compare Tour/Hotel: anonymous booking is likely **IN candidate** with a **Flight-specific** access token header. Do not reuse Tour or Hotel tokens. Object-level 404 for missing/wrong/cross-user. IDs/PNR are not credentials. SessionStorage not localStorage for raw token. No card collection. Transactional noindex. FA/EN/AR + bidi + mobile-first. Flight times always with timezone/airport context.

Operational reads: internal-only query of offer provenance, booking/PNR/ticket/payment/refund/reconciliation. **No** ForceTicket / ForceConfirm / MarkPaid. Trusted recheck may query authoritative source.

Flight PII is richer than HotelBooking. Redact documents in DTOs/logs. No passport/PAN/provider-secret logging.

Smart supplier routing/failover = **DEFERRED** (same as P21).

---

## 14. Failure / compensation matrix (planning)

| Case | Direction |
|------|-----------|
| Offer expires before Payment | Revalidate/requote; no silent replace of accepted money |
| Payment succeeds, reservation not created | Full financial compensation (Refund) after authoritative inability; no fake PNR |
| PNR created, Payment never succeeds | Cancel/void PNR; do not ticket |
| Payment + PNR, ticketing fails definitively | Compensation and/or cancel PNR; do not claim tickets |
| Ticketing outcome ambiguous | Recheck before Refund/cancel; timeout ≠ failed |
| Supplier reservation invalid after Payment | Recheck then compensate |
| Customer cancel needing partial refund | **Blocked** while Partial Refund DEFERRED |
| Duplicate callback | Idempotent ignore |
| Process crash after supplier success before local commit | Reconciliation / inbox |

Do not claim distributed exactly-once. At-least-once + local idempotent effects.

---

## 15. Scope classification (IN / OUT / DEFER)

Classifications are **planning inventory**, not architect locks.

| Concept | Classification | Notes |
|---------|----------------|-------|
| Independent Flight module + schema `flight` | **IN (candidate)** | Locked only by **P22-R1** |
| `FlightBooking` aggregate inside Flight | **IN (candidate)** | R1 |
| Airport/Airline master catalogs | **ReferenceData (candidate)** | Not Flight-owned catalog; may stay unimplemented in early T# |
| One-way + round-trip live search/book | **IN (candidate)** | R2/R3/R8 |
| Connecting segments | **IN (candidate)** | R2 |
| Multi-city | **DEFERRED** | ROADMAP conditional |
| Infant passenger category | **IN (candidate)** | R2 |
| Offer/monetary snapshots owned by Flight | **IN (candidate)** | R4 |
| Typed Payment target FlightBooking | **IN (candidate, later T#)** | R6; **not T001** |
| PNR + ticketing as separate process states | **IN (candidate)** | R5 |
| Anonymous Flight token | **IN (candidate)** | R8 |
| Public CRUD / list of all bookings | **OUT** | |
| Generic Booking platform / Booking&lt;T&gt; | **OUT** | |
| TourDepartureTransportSegment rewrite | **OUT** | P11-R5 stays |
| Live inventory inside Tour | **OUT** | |
| Place/Destination owning airports | **OUT** unless later SoT lock | |
| Real named supplier / SDK | **OUT of PLAN; DEFERRED product** | NONE |
| Fake production source | **OUT** | |
| Partial Refund | **DEFERRED** (P20) | R7 dependency |
| Pay-later / deposit / agency credit | **DEFERRED** | |
| Ancillaries (seat/meal/extra bag) | **DEFERRED** | |
| Amendments / rebooking / no-show workflow | **DEFERRED** | |
| Smart routing/failover | **DEFERRED** | |
| Accounting / settlement / wallet / fraud / loyalty | **OUT** | |
| Dynamic Flight+Hotel package | **OUT (P23)** | |
| Notification sending | **OUT (P25)** | |
| LLM / RAG / vector DB | **OUT** | |

---

## 16. Decision inventory (must stay OPEN)

| ID | Topic | Status |
|----|-------|--------|
| **P22-R1** | Flight ownership / module / schema and Tour boundary | **RESOLVED** — independent Flight module · schema `flight` · FlightBooking owned inside Flight · **Flight != Tour** · **FlightBooking != Tour Booking** · **FlightBooking != HotelBooking** · **Tour Package Flight != live Flight inventory** |
| **P22-R2** | Itinerary / segment / airport / airline / passenger model | **RESOLVED** — FlightBooking aggregate · OneWay=1 journey · RoundTrip=2 journeys · MultiCity DEFERRED · Journey 1..N Segments · no FlightLeg · Airport/Airline authority = ReferenceData · Flight stores IATA logical references only · Adult/Child/Infant · no BirthDate/passport |
| **P22-R3** | Search / availability / offer authority and supplier capability | **RESOLVED** — live Flight search/availability is external source-authoritative · TravelCore-owned seat inventory not implemented · `IFlightSearchSource` + `IFlightOfferAvailabilitySource` · timeout/Unknown ≠ Unavailable · no hold/PNR · Named Flight Supplier = NONE · Production Search/Availability Source = NONE. **P22-R3 = RESOLVED**. **TC-P22-T004 NOT EXECUTED**. |
| **P22-R4** | Fare offer / revalidation / monetary snapshot / fare rules | **OPEN** |
| **P22-R5** | Supplier reservation / PNR lifecycle, idempotency, reconciliation | **OPEN** |
| **P22-R6** | Payment ordering / typed Flight target / ticketing / compensation | **OPEN** |
| **P22-R7** | Cancellation / void / refund / partial-refund dependency | **OPEN** |
| **P22-R8** | Public UX / auth / privacy / operational / provider readiness | **OPEN** |

Inherited locked facts (not new P22 decisions): Tour ≠ live Flight; schema name `flight` is the SoT candidate; Payment kinds currently TourBooking+HotelBooking; Partial Refund DEFERRED; Production Payment Provider NONE; no named Flight supplier.

---

## 17. Task sequence

### TC-P22-T001 — Flight module / schema foundation

- Depends on **P22-R1**. **COMPLETE / ACCEPTED** (`a31654a` / docs `4a22acc`). Independent `Flight.Contracts` / `Flight.Domain` / `Flight.Infrastructure` · schema `flight` · FlightBooking ownership assigned to Flight.

### TC-P22-T002 — Itinerary / segment / passenger structure

- Depends on **P22-R2**. **COMPLETE / ACCEPTED** (`9518018` / docs `7a1bf45`). FlightBooking · OneWay/RoundTrip · journeys/segments · IATA references · Adult/Child/Infant.

### TC-P22-T003 — Search / availability / offer source boundary

- Depends on **P22-R3**. **IMPLEMENTED / AWAITING_ARCHITECT_REVIEW.** `IFlightSearchSource` · `IFlightOfferAvailabilitySource` · external source-authoritative · zero production sources · **TC-P22-T004 NOT EXECUTED**.

### TC-P22-T004 — Fare / monetary / fare-rules snapshots

- Depends on **P22-R4**. Do not generalize Pricing.

### TC-P22-T005 — Reservation / PNR / reconciliation

- Depends on **P22-R5**.

### TC-P22-T006 — Payment integration / ticketing / compensation

- Depends on **P22-R6**. Typed Flight target only if R6 locks it. Dual/triple evidence as locked.

### TC-P22-T007 — Cancellation / void / refund boundary

- Depends on **P22-R7**. Must surface Partial Refund dependency honestly.

### TC-P22-T008 — Public UX / authorization / privacy / operational reads

- Depends on **P22-R8**.

### TC-P22-T009 — Hardening + evidence

- Guardrails + evidence pack; **no new capability**. Does **not** execute GATE.

### TC-P22-GATE — Acceptance Gate

- Evidence only. No new Flight product. Do not start P23 inside GATE.

---

## 18. Architecture invariants (carry forward)

1. Tour Package Flight ≠ live Flight inventory.
2. Flight ≠ Tour Booking ≠ HotelBooking ≠ Place.
3. No shared DbContext. No peer-schema FK. No peer Infrastructure dependency.
4. Provider/PNR/ticket IDs are never TravelCore primary identity.
5. Client is not monetary, availability, or success authority.
6. Browser return ≠ Payment success ≠ ticket issued.
7. Search ≠ live offer SoT.
8. PublicExperience ≠ Flight Source of Truth.
9. No distributed exactly-once claim.
10. No fake production Flight source.

---

## 19. Conflicts / documentation gaps

**No blocker.** Working tree at PLAN start was CLEAN at `d6bd842`.

Non-blocking gaps:

- `07-data-architecture.md` lists schema `flight`; `08-persistence-and-migrations.md` examples mention `hotel_booking` but not `flight`. Not a contradiction of ownership — persistence doc is illustrative.
- Glossary `FlightSegment` is richer than implemented `TourDepartureTransportSegment`. Product follows P11-R5; glossary is conceptual. P22 must not “complete” Tour glossary by stealing live inventory.
- Frontend `FlightSegmentView` is Tour presentation, not Flight module.

---

## 20. Gate criteria (phase, not this PLAN)

P22 may complete later only if R1–R8 are resolved, T001–T009 accepted, validation green, deferred limitations explicit, and no fake production capability is claimed.

This PLAN task is **not** Gate-ready and must not mark P22 READY_FOR_GATE.

---

## 21. Repository safety

- Branch `main` · fast-forward push only · no force.
- Docs-only: `docs/plans/P22-implementation-plan.md` · SoT · this envelope.
- **No** `src/...Flight` product code · **no** migration · **no** API · **no** frontend product · **no** packages.
- Do **not** execute `TC-P22-T003` until T002 ACCEPTED and P22-R3 locked.

---

## 22. PLAN Done criteria

- Flight phase title: **P22 — Flight**
- docs-only: **YES**
- Tour Package Flight ≠ live Flight inventory: **YES**
- Recommended ownership: independent Flight module / schema `flight` / `FlightBooking` inside Flight (**OPEN**)
- Airport owner candidate: **ReferenceData** (**OPEN** if catalog is implemented)
- Airline owner candidate: **ReferenceData**
- Named Flight Supplier: **NONE**
- Payment current target kinds: TourBooking, HotelBooking; Flight support: **NO**
- P22-R1: **RESOLVED**
- P22-R2: **RESOLVED**
- P22-R4 through P22-R8: **OPEN**
- T001–T009 + GATE sequenced
- T001 executed: **YES** (ACCEPTED)
- T002 executed: **YES** (awaiting architect review)
- **TC-P22-T004 NOT EXECUTED**
- P23 started: **NO**
