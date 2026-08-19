# TC-P22-GATE — P22 Acceptance Evidence

**Task:** TC-P22-GATE — Flight Acceptance Gate  
**Baseline HEAD:** `856bb06` (`TC-P22-T009` product) · docs `e76b562` (T009 result envelope)  
**Starting HEAD:** `e76b562` (`origin/main`)  
**Date:** 2026-08-19  
**Scope:** Gate / acceptance only — **no new product capability**. Next phase is **not executed** here.

## 1. Preconditions

| Check | Result |
|-------|--------|
| USER PIPELINE + continuity override | YES |
| Architect Auto-Execute GATE after T009 ACCEPT | YES |
| PLAN + T001–T009 ACCEPTED · R1–R8 RESOLVED | YES |
| Evidence pack | YES — [`P22-T009-hardening-and-evidence-pack.md`](P22-T009-hardening-and-evidence-pack.md) (input only; independently re-verified) |
| Working tree at gate start | CLEAN except GATE files (`e76b562` == `origin/main`) |

## 2. Checklist (architect GATE)

| # | Criterion | Result |
|---|-----------|--------|
| 1 | Independent Flight module/schema `flight`; Tour transport stays Tour (P22-R1) | **PASS** — T001 |
| 2 | Itinerary / segment / airport / airline / passenger (P22-R2) | **PASS** — T002 |
| 3 | Search / availability / offer source boundary (P22-R3) | **PASS** — T003 |
| 4 | Fare / monetary / fare-rules snapshots (P22-R4) | **PASS** — T004 |
| 5 | Supplier reservation / PNR lifecycle (P22-R5) | **PASS** — T005 |
| 6 | Payment target + ticketing + triple-evidence confirmation (P22-R6) | **PASS** — T006 |
| 7 | Confirmed cancellation process + penalty economics (P22-R7) | **PASS** — T007 |
| 8 | Public transactional journey + independent token (P22-R8) | **PASS** — T008 |
| 9 | Hardening / evidence | **PASS** — T009 |
| 10 | Flight != Tour · FlightBooking != Tour Booking · FlightBooking != HotelBooking | **PASS** |
| 11 | No new Flight capability in Gate | **PASS** — evidence only |

## 3. Locked decisions

**P22-R1…R8 all RESOLVED** — see [`P22-implementation-plan.md`](P22-implementation-plan.md).

**Flight != Tour**. **FlightBooking != Tour Booking**. **FlightBooking != HotelBooking**. **Tour Package Flight != live Flight inventory**. **Payment Succeeded != FlightBooking Confirmed**. **PNR Confirmed != FlightBooking Confirmed**. **Ticket Issued != FlightBooking Confirmed**. **FlightBookingCancelled != RefundSucceeded**.

Production Flight Search Source = NONE. Production Flight Availability Source = NONE. Production Flight Offer Source = NONE. Production Flight Reservation Source = NONE. Production Flight Ticketing Source = NONE. Production Flight Cancellation Source = NONE. Named Flight Supplier = NONE. Production Payment Provider = NONE.

## 4. Accepted product commits (P22)

| Task | Commit | Status |
|------|--------|--------|
| PLAN | `58a2590` / docs `b32a867` | ACCEPTED |
| TC-P22-T001 | `a31654a` / docs `4a22acc` | ACCEPTED |
| TC-P22-T002 | `9518018` / docs `7a1bf45` | ACCEPTED |
| TC-P22-T003 | `6470cf8` / docs `e62ea76` | ACCEPTED |
| TC-P22-T004 | `92f1554` / docs `c1dbc5c` | ACCEPTED |
| TC-P22-T005 | `cd05215` / docs `1230fbf` | ACCEPTED |
| TC-P22-T006 | `57731ed` / docs `935b668` | ACCEPTED |
| TC-P22-T007 | `0c39a60` / docs `1b344b9` | ACCEPTED |
| TC-P22-T008 | `d7c61d7` / docs `65cf720` | ACCEPTED |
| TC-P22-T009 | `856bb06` / docs `e76b562` | ACCEPTED |
| TC-P22-GATE | this task (uncommitted at RESULT draft) | implemented / AWAITING_ARCHITECT_REVIEW |

## 5. Ownership / architecture matrix

| Invariant | Result |
|-----------|--------|
| Live Flight inventory / offer / PNR / ticket owner | **Flight** |
| FlightBooking owner | **Flight** (not a separate FlightBooking module) |
| schema | `flight` |
| Tour package transport | **Tour** — `TourDepartureTransportSegment` |
| Airport / Airline authority | **ReferenceData** (logical IATA refs only in Flight) |
| Payment / Refund execution | **Payment** |
| live search authority | `IFlightSearchSource` |
| live availability authority | `IFlightOfferAvailabilitySource` |
| fare/offer authority | `IFlightOfferSource` |
| reservation/PNR authority | `IFlightReservationSource` |
| ticketing authority | `IFlightTicketingSource` |
| cancellation/reversal authority | `IFlightCancellationSource` |
| Payment supported target kinds | TourBooking, HotelBooking, FlightBooking |
| peer-schema FK | **NO** |
| shared DbContext | **NO** |
| peer Infrastructure dependency | **NO** |
| cross-schema SQL | **NONE** |
| distributed transaction | **NO** |
| supplier SDK | **NO** |
| Payment provider SDK | **NO** |
| PublicExperience != Flight Source of Truth | **PASS** |

## 6. Domain evidence

- FlightBookingStatus: Pending, Confirmed, Cancelled
- FlightTripType: OneWay, RoundTrip (MultiCity DEFERRED)
- OneWay: exactly 1 journey · RoundTrip: exactly 2 journeys · Journey: 1..N segments · connecting flights: YES
- FlightPassengerCategory: Adult, Child, Infant · >= 1 passenger · >= 1 Adult
- BirthDate stored: NO · Gender stored: NO · Nationality stored: NO · Passport/document stored: NO
- FlightSupplierReservationStatus: Pending, Confirmed, Expired, Cancelled
- FlightSupplierReservationAttemptStatus: Created, Initiated, Confirmed, Failed
- FlightTicketingAttemptStatus: Created, Initiated, Succeeded, Failed
- FlightTicketStatus: Pending, Issued, Voided, Refunded
- FlightBookingCancellationStatus: Requested, SupplierReversalPending, RefundPending, Completed
- FlightSupplierReversalAttemptStatus: Created, Initiated, Succeeded, Failed
- FlightBookingCancellationFinancialOutcome: FullRefund, NoRefund
- PaymentStatus: Pending, Succeeded
- PaymentTargetKind: TourBooking, HotelBooking, FlightBooking
- Money: `TravelCore.Money` decimal + CurrencyCode · no float/double money · no implicit FX · Toman != CurrencyCode
- OfferExpiresAt != TicketingDeadline != ReservationExpiresAt (NodaTime Instant)
- No type named `PNR`. No `BookingBase`. No `IFlightSupplierGateway`.

## 7. Flow evidence

- Order: accepted Flight Offer → Confirmed supplier reservation → Payment → ticket issuance → FlightBooking Confirmed
- Payment-only confirmation: stays Pending
- Reservation-only (PNR Confirmed) confirmation: stays Pending
- Partial ticketing: stays Pending; cannot confirm FlightBooking
- Triple-evidence confirmation: SupplierReservation Confirmed AND Payment Succeeded AND all required passenger Tickets Issued
- Reservation timeout: attempt remains Initiated; unresolved blocks unsafe retry
- Ticketing timeout: attempt remains Initiated; no automatic Refund while ticket state is ambiguous
- Paid-but-uncompleted compensation: distinct from customer cancellation; full Refund only; Payment owns Refund; PaymentStatus remains Succeeded after Refund
- Penalty = 0 → FullRefund after authoritative supplier reversal
- Penalty = TotalAmount → NoRefund
- 0 < Penalty < TotalAmount → PartialRefundRequiredButUnsupported; supplier reversal call count = 0; FlightBooking remains Confirmed
- Cancellation timeout: attempt Initiated; FlightBooking remains Confirmed; Refund not started
- Ticket void/refund != Payment Refund
- FlightBookingCancelled != RefundSucceeded

## 8. Public / security evidence

- API: `POST /api/flight-booking/public/search` · `POST /api/flight-booking/public/initiations` · `GET /api/flight-booking/public/{flightBookingId}` · `POST .../offers` · `POST .../reservations` · `GET .../payment` · `POST .../payment/initiation` · `POST .../cancellation`
- Frontend: `/[locale]/flights` · `/[locale]/flight-bookings/[flightBookingId]` · `.../payment` · `.../payment/return`
- Header: `X-TravelCore-Flight-Booking-Access-Token`
- raw token persisted: NO · verifier persisted: YES (SHA-256) · token in URL: NO · localStorage: NO · sessionStorage: YES (`tc.flight-booking.access.${flightBookingId}`)
- BookingId / PaymentId / ReservationLocator / TicketNumber are not credentials
- missing/wrong/cross-user/FlightBookingId-only: 404
- Flight token cannot authorize Tour Booking or HotelBooking (and vice versa)
- public list: NO · generic PUT/PATCH/CRUD: NO · set status / force confirm / mark paid / force ticket / force refund: NO
- client is not authority for price, currency, availability, PNR, Payment success, ticket, refund, cancellation economics, or FlightBooking status
- card collection: NO · provider secrets exposed: NO · fake Payment success: NO
- transactional noindex: YES · FA/EN/AR: YES · RTL/LTR/bidi: PASS · mobile/accessibility: PASS
- operational read: `IFlightOperationalQuery` internal-only · operational mutation: NONE

## 9. Validation battery (gate re-run)

| Suite | Result | Detail |
|-------|--------|--------|
| `dotnet build TravelCore.sln` | **PASS** | 0 Error(s) |
| Flight.UnitTests | **PASS** | **91** |
| Payment.UnitTests | **PASS** | **93** |
| Booking.UnitTests | **PASS** | **54** |
| HotelBooking.UnitTests | **PASS** | **103** |
| Tour.UnitTests | **PASS** | **84** |
| ArchitectureTests | **PASS** | **337** (includes GATE evidence guardrail) |
| Persistence.IntegrationTests | **PASS** | **125** |
| Host.IntegrationTests | **PASS** | **66** |
| Frontend `npm run typecheck` | **PASS** | clean |
| Frontend `npm run lint` | **PASS** | clean |
| Frontend `npm run build` | **PASS** | flight-booking routes present |
| `git diff --check` | **PASS** | clean |

## 10. Production source/provider matrix

| Source / provider | Production value |
|-------------------|------------------|
| Production Flight Search Source | **NONE** |
| Production Flight Availability Source | **NONE** |
| Production Flight Offer Source | **NONE** |
| Production Flight Reservation Source | **NONE** |
| Production Flight Ticketing Source | **NONE** |
| Production Flight Cancellation Source | **NONE** |
| Named Flight Supplier | **NONE** |
| Supplier SDK | **NO** |
| Production Payment Provider | **NONE** |

Zero configured sources: public search/offer/reservation/payment initiation is truthful 503. No fake production source/provider.

## 11. Explicit OUT / DEFER

- Partial Refund = NOT IMPLEMENTED / DEFERRED
- MultiCity = NOT IMPLEMENTED / DEFERRED
- Ancillaries = NOT IMPLEMENTED / DEFERRED
- PayLater = NOT IMPLEMENTED / DEFERRED
- Deposit / Partial Payment = NOT IMPLEMENTED / DEFERRED
- Amendments = NOT IMPLEMENTED / DEFERRED
- Rebooking = NOT IMPLEMENTED / DEFERRED
- No-show = NOT IMPLEMENTED / DEFERRED
- Per-passenger cancellation = NOT IMPLEMENTED / DEFERRED
- Partial-itinerary cancellation = NOT IMPLEMENTED / DEFERRED
- Smart supplier routing = NOT IMPLEMENTED / DEFERRED
- Automatic failover = NOT IMPLEMENTED / DEFERRED
- Real Flight supplier = NONE
- Real Payment provider = NONE
- Accounting / Settlement / Supplier settlement / Agency commission / Wallet / Fraud/risk / Loyalty / AI infrastructure = OUT
- Next phase product — **P23 — Dynamic Package / Flight + Hotel (PLANNED)** — **not executed in this Gate**

This Gate does **not** claim external real-world Flight reservation/payment/ticketing capability without configured real adapters/providers.

## 12. Architect STOP rules honored

| Rule | Honored |
|------|---------|
| No new Flight product in GATE | YES |
| No inventing beyond P22-R1–R8 (no P22-R9) | YES |
| No next-phase product code | YES |
| No force-push / history rewrite | YES |
| T009 pack left historically `TC-P22-GATE NOT EXECUTED` / not `P22 COMPLETE` | YES |

## 13. Ledger

- TC-P22-PLAN = ACCEPTED
- TC-P22-T001 = ACCEPTED
- TC-P22-T002 = ACCEPTED
- TC-P22-T003 = ACCEPTED
- TC-P22-T004 = ACCEPTED
- TC-P22-T005 = ACCEPTED
- TC-P22-T006 = ACCEPTED
- TC-P22-T007 = ACCEPTED
- TC-P22-T008 = ACCEPTED
- TC-P22-T009 = ACCEPTED
- TC-P22-GATE = ACCEPTED
- P22-R1 = RESOLVED
- P22-R2 = RESOLVED
- P22-R3 = RESOLVED
- P22-R4 = RESOLVED
- P22-R5 = RESOLVED
- P22-R6 = RESOLVED
- P22-R7 = RESOLVED
- P22-R8 = RESOLVED

## 14. T009 pack review

[`P22-T009-hardening-and-evidence-pack.md`](P22-T009-hardening-and-evidence-pack.md) was used as evidence **input**, not as authority. Independent re-check of module/schema, statuses, source ports, Payment targets, token header, uniqueness indexes, frontend noindex/FA-EN-AR, and zero production sources matches the current repository. T009 pack correctly remains historically **TC-P22-GATE NOT EXECUTED** and must not contain **P22 COMPLETE**. T009 recorded test counts are superseded by §9 of this Gate.

## 15. Defects / corrections

- Product-code defect found: **NO**
- T009 guardrail `P22_Closed_Lifecycles_And_Targets_Remain_Exact` asserted that `docs/plans/P22-GATE-acceptance-evidence.md` must **not** exist. That close-prevention check is an acceptance-blocking defect for GATE. Correction: removed the non-existence assertion and added `P22_GateEvidence_Exists_And_Closes_Phase` (clone of P21 `P21_GateEvidence_Exists_And_Closes_Phase`). `P22_EvidencePack_Exists_And_DoesNotClose_Gate` still requires the T009 pack to say `TC-P22-GATE NOT EXECUTED` and not `P22 COMPLETE`.
- `Flight_Evidence_Keeps_Ascii_Invariants` previously required plan wording `T009 implemented / awaiting architect review` and `TC-P22-GATE NOT EXECUTED`. Correction: those assertions now require GATE-closed wording (`T009 ACCEPTED` + `TC-P22-GATE COMPLETE / ACCEPTED` + `P22 COMPLETE`).
- No P20/P21 GATE evidence files were changed. No P22-R9 invented.

## 16. Gate outcome

**TC-P22-GATE COMPLETE** · **P22 COMPLETE** · T001–T009 ACCEPTED · P22-R1–R8 RESOLVED.

This Gate adds **no new product capability**.

Next phase from SoT: **P23 — Dynamic Package / Flight + Hotel (PLANNED)**. This Gate does **not** start P23 product work.
