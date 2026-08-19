# TC-P22-T009 — Flight hardening and evidence pack

**Task:** TC-P22-T009 — Hardening + evidence  
**Product HEAD at T009 start:** `65cf720` (`TC-P22-T008` **ACCEPTED** at `d7c61d7` / docs `65cf720`)  
**Date:** 2026-08-19  
**Scope:** Tests, architecture guardrails, security/privacy evidence, documentation, SoT sync — **no new product capability**.  
**Forbidden in this task:** new architecture decision · Partial Refund · MultiCity · ancillaries · PayLater · deposit/partial payment · amendments · rebooking · no-show workflow · per-passenger cancellation · partial-itinerary cancellation · real supplier · supplier SDK · real Payment provider · Payment provider SDK · smart supplier routing/failover · accounting · settlement · agency commission · wallet · fraud · loyalty · generic Booking platform · generic arbitrary Payment target · public CRUD · operational mutation · shared DbContext · peer-schema FK · peer Infrastructure dependency · distributed transaction · unrelated refactor · dependency upgrade · `TC-P22-GATE` execution.  
**Not this task:** `TC-P22-GATE` (**NOT EXECUTED**). **P22 remains IN PROGRESS**. Do **not** close the phase.

## 1. Scope and baseline

| # | Verify | Result |
|---|--------|--------|
| 1 | Independent Flight module/schema `flight`; Tour transport stays Tour (P22-R1) | **PASS** — T001 |
| 2 | Itinerary / segment / airport / airline / passenger (P22-R2) | **PASS** — T002 |
| 3 | Search / availability / offer source boundary (P22-R3) | **PASS** — T003 |
| 4 | Fare / monetary / fare-rules snapshots (P22-R4) | **PASS** — T004 |
| 5 | Supplier reservation / PNR lifecycle (P22-R5) | **PASS** — T005 |
| 6 | Payment target + ticketing + triple-evidence confirmation (P22-R6) | **PASS** — T006 |
| 7 | Confirmed cancellation process + penalty economics (P22-R7) | **PASS** — T007 |
| 8 | Public transactional journey + independent token + operational reads (P22-R8) | **PASS** — T008 |
| 9 | No new product capability in this task | **PASS** — evidence/docs + hardening guardrails only |
| 10 | `TC-P22-GATE` remains NOT EXECUTED | **PASS** — this pack does not close GATE |

## 2. P22 decision inventory

| ID | Essence |
|----|---------|
| **P22-R1** | Independent Flight module. Schema `flight`. FlightBooking owned inside Flight. **Flight != Tour**. **FlightBooking != Tour Booking**. **FlightBooking != HotelBooking**. **Tour Package Flight != live Flight inventory**. `TourDepartureTransportSegment` remains Tour-owned. No shared DbContext. No peer-schema FK. Named Flight Supplier = NONE. |
| **P22-R2** | TripType OneWay/RoundTrip. **MultiCity remains DEFERRED**. Journey 1..N Segments. Airport/Airline authority = ReferenceData. Adult/Child/Infant. 1 Adult min. BirthDate/Gender/Nationality/Passport stored = NO. NodaTime Instant + IANA TZ. |
| **P22-R3** | `IFlightSearchSource` + `IFlightOfferAvailabilitySource`. Timeout/Unknown ≠ Unavailable. No giant `IFlightSupplierGateway`. Production Flight Search Source = NONE. Production Flight Availability Source = NONE. |
| **P22-R4** | `IFlightOfferSource`. Immutable `FlightOfferSnapshot` / `FlightBookingMonetarySnapshot` / `FlightFareRulesSnapshot`. Search price is not authority. No silent repricing. `TicketingDeadline != OfferExpiresAt`. Production Flight Offer Source = NONE. P12 Pricing not generalized. Partial Refund remains DEFERRED. |
| **P22-R5** | One `FlightSupplierReservation` per FlightBooking. Statuses Pending/Confirmed/Expired/Cancelled. Attempts Created/Initiated/Confirmed/Failed. Timeout ≠ Failed. `ReservationExpiresAt` source-authored. Production Flight Reservation Source = NONE. Named Flight Supplier = NONE. |
| **P22-R6** | PNR-first: accepted offer → confirmed reservation → Payment → tickets → FlightBooking Confirmed. `PaymentTargetKind` exactly TourBooking, HotelBooking, FlightBooking. Triple evidence. **Payment Succeeded != FlightBooking Confirmed**. Ticketing timeout unresolved; no auto Refund while ambiguous. Full-refund compensation. Confirmed cancel is R7. |
| **P22-R7** | Separate `FlightBookingCancellation`. Penalty=0 FullRefund after authoritative supplier reversal. Penalty=Total NoRefund. Partial penalty blocked before supplier side effect. Ticket void/refund ≠ Payment Refund. Partial ticket reversal cannot cancel Booking. **FlightBookingCancelled != RefundSucceeded**. **amendments/rebooking remain DEFERRED**. |
| **P22-R8** | Public Flight is transactional, not CRUD. Independent `X-TravelCore-Flight-Booking-Access-Token`. SHA-256 verifier only. sessionStorage. 404 non-enumeration. Zero sources truthful. Operational reads internal-only. **PNR Confirmed != FlightBooking Confirmed**. **Ticket Issued != FlightBooking Confirmed**. |

**Ledger:**

- TC-P22-PLAN ACCEPTED
- TC-P22-T001 ACCEPTED
- TC-P22-T002 ACCEPTED
- TC-P22-T003 ACCEPTED
- TC-P22-T004 ACCEPTED
- TC-P22-T005 ACCEPTED
- TC-P22-T006 ACCEPTED
- TC-P22-T007 ACCEPTED
- TC-P22-T008 ACCEPTED
- TC-P22-T009 awaiting architect review
- TC-P22-GATE NOT EXECUTED

## 3. Module/schema ownership proof

| Concern | Owner | P22 posture |
|---------|-------|-------------|
| Live Flight inventory / offer / PNR / ticket | **Flight** | schema `flight` |
| Tour package transport labels | **Tour** | `TourDepartureTransportSegment` |
| Tour Booking | **Booking** | unchanged P19 aggregate |
| Hotel reservation transaction | **HotelBooking** | schema `hotel_booking` |
| Airport / Airline catalogs | **ReferenceData** | logical IATA refs only in Flight |
| Payment / Refund execution | **Payment** | schema `payment`; closed target kinds |
| Tour commercial rates | **Pricing** | not generalized to airline fares |
| IndexPolicy | **SEO** | transactional Flight pages always noindex |

Flight.Infrastructure references Payment.Contracts only — never peer Infrastructure/Domain. Payment.Infrastructure references Flight.Contracts — never Flight.Infrastructure/Domain.

## 4. Domain invariant proof

- FlightBookingStatus exactly Pending / Confirmed / Cancelled.
- FlightSupplierReservationStatus exactly Pending / Confirmed / Expired / Cancelled.
- FlightSupplierReservationAttemptStatus exactly Created / Initiated / Confirmed / Failed.
- FlightTicketingAttemptStatus exactly Created / Initiated / Succeeded / Failed.
- FlightTicketStatus exactly Pending / Issued / Voided / Refunded.
- FlightBookingCancellationStatus exactly Requested / SupplierReversalPending / RefundPending / Completed.
- FlightSupplierReversalAttemptStatus exactly Created / Initiated / Succeeded / Failed.
- FlightBookingCancellationFinancialOutcome exactly FullRefund / NoRefund.
- PaymentStatus Pending / Succeeded. PaymentTargetKind exactly TourBooking, HotelBooking, FlightBooking.
- TripType exactly OneWay / RoundTrip. MultiCity remains DEFERRED.
- BirthDate stored = NO. Gender stored = NO. Nationality stored = NO. Passport stored = NO.
- No generic Confirm / SetStatus / ForceConfirm / ForceTicket / ForceCancel / MarkPaid / MarkRefunded.
- No `BookingBase`. No `IFlightSupplierGateway`. No type named `PNR`.

## 5. Search/availability/offer proof

- Authority: `IFlightSearchSource` finds candidates; `IFlightOfferAvailabilitySource` revalidates.
- Production Flight Search Source = NONE. Production Flight Availability Source = NONE.
- Timeout/Unknown does not mean Unavailable. No fabricated search options.
- No FlightAvailabilityHold. Source selection is server-controlled.
- Process-local lock is not correctness authority.

## 6. Fare/monetary/fare-rules proof

- Authority: `IFlightOfferSource`. Production Flight Offer Source = NONE.
- Search-result price != `FlightBookingMonetarySnapshot`.
- Accepted snapshots are immutable. Silent repricing does not mutate accepted truth.
- One transaction CurrencyCode. No implicit FX. Toman is not a CurrencyCode.
- `TicketingDeadline != OfferExpiresAt`. `ReservationExpiresAt` is a distinct source-authored Instant.
- P12 Pricing not generalized. Partial penalty is representable; Partial Refund execution is NOT IMPLEMENTED.

## 7. Supplier reservation proof

- Authority: `IFlightReservationSource`. Named Flight Supplier = NONE. Production Flight Reservation Source = NONE. Supplier SDK = NO.
- One reservation covers the complete itinerary/passengers. Partial passenger/segment confirmation cannot confirm the reservation.
- Timeout leaves attempt Initiated. Unresolved Created/Initiated blocks unsafe duplicate acquisition.
- Authoritative query-confirmed evidence required. Unverified callback / browser flags cannot confirm.
- Cross-booking / cross-attempt evidence cannot mutate the wrong aggregate.
- **PNR Confirmed != FlightBooking Confirmed**.

## 8. Payment/ticketing/compensation proof

- Payment supported target kinds: TourBooking, HotelBooking, FlightBooking. Arbitrary TargetType = absent.
- Obligation from `FlightBookingMonetarySnapshot` copied into immutable `PaymentExecutionSnapshot`.
- Order: accepted offer → confirmed reservation → Payment → tickets → FlightBooking Confirmed.
- Triple evidence: Payment Succeeded AND reservation Confirmed AND all passenger tickets Issued.
- Payment-only stays Pending. Reservation-only stays Pending. Partial ticketing stays Pending.
- Ticketing timeout: attempt stays Initiated; no automatic Refund.
- Definitive ticketing failure after Payment: R6 full-refund compensation. PaymentStatus stays Succeeded.
- Compensation event has no amount authority. Refund amount = PaymentExecutionSnapshot.
- Production Payment Provider = NONE. Payment provider SDK = NO. Production Flight Ticketing Source = NONE.

## 9. Confirmed cancellation proof

- Separate `FlightBookingCancellation` process. R6 compensation is distinct from R7 cancellation.
- Penalty=0 → FullRefund after authoritative supplier reversal.
- Penalty=Total → NoRefund; completes without Refund.
- Partial penalty: blocked; supplier reversal call count = 0; FlightBooking stays Confirmed.
- Cancellation timeout: attempt stays Initiated; FlightBooking remains Confirmed; Refund not started.
- Ticket void/refund ≠ Payment Refund. Partial ticket reversal cannot cancel the Booking or trigger Payment Refund.
- FlightBookingCancelled != RefundSucceeded.

## 10. Public authorization/privacy proof

- Header: `X-TravelCore-Flight-Booking-Access-Token` (independent of Tour Booking and HotelBooking).
- Raw token returned once. Raw token persisted = NO. Verifier persisted = YES (SHA-256).
- Token URL exposure = NO. token localStorage = NO. sessionStorage = YES (`tc.flight-booking.access.${id}`).
- FlightBookingId / PaymentId / ReservationLocator / TicketNumber are not credentials.
- Missing/wrong/cross-user/FlightBookingId-only/PaymentId-only = 404.
- Flight token ⇏ Tour/Hotel and vice versa.
- Client amount/currency/PNR/ticket/penalty/success are not authority.
- Public FlightBooking list = NO. Generic public CRUD = NO. Public Refund command = NO. Card collection = NO.

## 11. Frontend/noindex/a11y/bidi proof

- Search: `/[locale]/flights`
- Private: `/[locale]/flight-bookings/[flightBookingId]`
- Payment + return: `.../payment` and `.../payment/return`
- Transactional routes noindex = YES.
- FA/EN/AR copy. RTL/LTR via LtrValue / MoneyText.
- Mobile `min-h-11`. focus-visible. FieldMessage error semantics.

## 12. Concurrency/idempotency proof

- Initiation idempotency converges to the same FlightBooking (`flight_booking_public_idempotency`).
- Offer / reservation / Payment / ticketing / cancellation uniqueness is database-backed (unique indexes / constraints). See §40 of the T009 envelope.
- Duplicate PaymentSucceeded / compensation / RefundSucceeded / CancellationRefundRequired deliveries have one effective local result.
- Triple-evidence confirmation is race-safe (FlightBooking confirms at most once).

## 13. Outbox/inbox/crash-recovery proof

| Flow | Mechanism |
|------|-----------|
| Payment success | `payment.outbox_messages` → Flight `payment_success_inbox` |
| Flight compensation (R6) | Flight outbox → Payment `compensation_inbox` |
| Refund success | Payment outbox → Flight `refund_success_inbox` |
| Cancellation Refund required (R7) | Flight outbox → Payment `flight_booking_cancellation_refund_inbox` |
| Ticketing required | Flight local outbox continuation after Payment evidence |

Delivery semantics: at-least-once + local idempotent/effectively-once effects. Distributed exactly-once is not claimed. Restart correctness does not depend on in-memory dictionaries/locks.

## 14. Cross-target/cross-booking isolation proof

- Same UUID in TourBooking, HotelBooking, and FlightBooking namespaces cannot collide as one Payment target (`ck_payments_exactly_one_target` + typed kinds).
- FlightBooking target support cannot alter Tour Booking or HotelBooking Payment records.
- Flight cancellation/compensation events cannot affect Tour or Hotel Payment.
- Evidence for FlightBooking A cannot mutate FlightBooking B.

## 15. Zero-source/provider posture

- Production Flight Search Source = NONE
- Production Flight Availability Source = NONE
- Production Flight Offer Source = NONE
- Production Flight Reservation Source = NONE
- Production Flight Ticketing Source = NONE
- Production Flight Cancellation Source = NONE
- Named Flight Supplier = NONE
- Production Payment Provider = NONE
- Zero search/offer/reservation/payment public initiation = truthful 503. No fake production source/provider.

## 16. Deferred/out-of-scope inventory

- Partial Refund remains DEFERRED
- MultiCity remains DEFERRED
- Ancillaries = DEFERRED
- PayLater / deposit / partial collection = DEFERRED
- amendments/rebooking remain DEFERRED
- No-show execution = DEFERRED
- Per-passenger / partial-itinerary cancellation = DEFERRED
- Smart supplier routing/failover = DEFERRED
- Real Flight Supplier = NONE
- Real Payment Provider = NONE
- Accounting / Settlement / Agency commission / Wallet / Fraud / Loyalty / LLM / RAG / Embeddings / Vector DB = not implemented

## 17. Exact test/build results

Recorded after validation in this task (see RESULT). Architecture hardening added `FlightHardeningGuardrailTests`. Existing Flight unit/host/persistence suites remain the primary behavioral proof.

| Surface | Result |
|---------|--------|
| `dotnet build TravelCore.sln` | PASS (0 errors) |
| Flight.UnitTests | 91 passed |
| Payment.UnitTests | 93 passed |
| Booking.UnitTests | 54 passed |
| HotelBooking.UnitTests | 103 passed |
| Tour.UnitTests | 84 passed |
| ArchitectureTests | 336 passed |
| Persistence.IntegrationTests | 125 passed |
| Host.IntegrationTests | 66 passed |
| frontend typecheck | PASS |
| frontend lint | PASS |
| frontend production build | PASS |
| `git diff --check` | PASS |

Architecture hardening added `FlightHardeningGuardrailTests` (+6). Existing Flight unit/host/persistence suites remain the primary behavioral proof. Product-code defect found: **NO**. Documentation drift fixed: P22-R1 SoT still said Payment kinds were TourBooking+HotelBooking only; corrected to include FlightBooking (T006 lock). Guardrail `Flight_Evidence_Keeps_Ascii_Invariants` updated so T009 is no longer frozen as unexecuted.

## 18. Remaining known limitations

- no production Flight supplier
- no production Flight search source
- no production Flight availability source
- no production Flight offer source
- no production Flight reservation source
- no production Flight ticketing source
- no production Flight cancellation source
- no production Payment provider
- Partial Refund unavailable
- partial-penalty confirmed cancellation blocked
- MultiCity unavailable
- ancillaries unavailable
- PayLater/deposit/partial collection unavailable
- amendments/rebooking/no-show unavailable
- per-passenger / partial-itinerary cancellation unavailable
- no smart supplier routing/failover

Historical T004/T007 task-scoped boundary flags (`FlightOfferOwnershipBoundary.PaymentIntegrationImplemented = false`, `FlightBookingCancellationOwnershipBoundary.PublicCancellationApiImplemented = false`) remain frozen by those tasks' unit tests. Authoritative current flags live on `FlightOwnershipBoundary` / `PublicFlightBookingCompositionBoundary`. Not a runtime defect.

READY_FOR_P22_GATE means architecture and implemented P22 scope are internally correct and tested. It does **not** mean P22 can perform a real-world Flight reservation/payment/ticketing in production without configured real adapters/providers.

## 19. Gate readiness conclusion

All accepted P22-R1 through P22-R8 invariants are verified together. Product-code defect found: **NO**. Correction commit: none (this execution does not commit).

**READY_FOR_P22_GATE**

This task still does **not** execute GATE. **P22 remains IN PROGRESS**. **TC-P22-GATE NOT EXECUTED**.
