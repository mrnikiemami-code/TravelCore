# TC-P22-T006 Task Envelope (architect, live)

Captured from the same ChatGPT tab after `TC-P22-T005 = ACCEPTED` and `P22-R6 = RESOLVED`. Envelope baseline `cd05215`. Working HEAD at start: `1230fbf`.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1
Task-ID: TC-P22-T006
Phase: P22
Title: Flight Payment target, PNR-first orchestration, ticket issuance, confirmation, and compensation
Baseline: cd05215
Decision: P22-R6 = RESOLVED

Purpose:
Integrate FlightBooking with Payment and implement ticket issuance. Establish:
- FlightBooking lifecycle
- explicit FlightBooking Payment target
- PNR-before-Payment gating
- Flight ticketing source boundary
- per-passenger ticket facts
- ambiguity-safe ticketing
- multi-evidence FlightBooking confirmation
- full-refund compensation when paid booking cannot be ticketed

Ordering (Flight ≠ Hotel):
Accepted FlightOfferSnapshot → authoritative Supplier Reservation / PNR → Payment → authoritative Ticket issuance → FlightBooking Confirmed.

Identities: PNR Confirmed != Payment Succeeded != Ticket Issued.
Final confirm: Payment Succeeded AND SupplierReservation Confirmed AND all required tickets authoritatively Issued = FlightBooking Confirmed.

Must implement:
- FlightBookingStatus exactly: Pending, Confirmed, Cancelled. New booking starts Pending.
- Confirm only when Reservation Confirmed + Payment Succeeded + all required passenger tickets Issued.
- Do not add AwaitingPayment, Reserved, Ticketing, Paid, Failed, Refunding.
- PaymentTargetKind exactly: TourBooking, HotelBooking, FlightBooking. Tour/Hotel unchanged.
- One FlightBooking → one logical Payment. No peer-schema FK. Amount/currency from FlightBookingMonetarySnapshot.
- Payment eligible only if FlightBooking Pending, accepted offer + monetary snapshot exist, FlightSupplierReservation Confirmed, reservation not Expired/Cancelled, now has not passed TicketingDeadline / ReservationExpiresAt, no successful Payment already.
- PNR-before-Payment: initiating Payment before confirmed reservation is prohibited.
- Narrow port IFlightTicketingSource + resolver. TicketCreate + TicketQuery. Same source as confirmed reservation.
- Production Ticketing Source = NONE. Named supplier = NONE. No SDK.
- FlightTicket per passenger, UUIDv7. FlightTicketStatus Pending/Issued only (Voided/Refunded remain R7).
- FlightTicketingAttempt: Created, Initiated, Succeeded, Failed. Timeout → Initiated, not Failed. Unresolved Initiated blocks duplicate issuance.
- Recheck/query required. Partial passenger tickets ≠ Confirmed.
- Constrained ConfirmFromAuthoritativeReservationPaymentAndTickets. No generic Confirm().
- If Payment succeeded but Flight cannot complete: durable full-compensation requirement (ReservationExpired, ReservationCancelled, TicketingDeadlineExpired, TicketingDefinitivelyFailed). Ambiguous ticketing: do not refund; recheck first.
- Flight decides compensation required; Payment executes full Refund from PaymentExecutionSnapshot. PaymentStatus remains Succeeded after Refund.
- Refund success may Pending → Cancelled via constrained system-compensation. Never Confirmed → Cancelled in R6.
- Capabilities add TicketCreate, TicketQuery. Keep Search, AvailabilityCheck, OfferRevalidation, ReservationCreate, ReservationQuery.

Forbidden:
Customer cancellation/void/refund policy, Partial Refund, public Flight API/UI, real supplier, real Payment provider, giant gateway, SDK, named supplier, fake production source, Confirmed→Cancelled in R6, automatic Refund on ambiguous ticketing, type named PNR, T007.

Validation:
dotnet build TravelCore.sln
Flight.UnitTests
Payment.UnitTests
Booking.UnitTests
HotelBooking.UnitTests
ArchitectureTests
Persistence.IntegrationTests
Host.IntegrationTests
git diff --check

SoT evidence:
P22-R6 = RESOLVED
TC-P22-T007 NOT EXECUTED
Keep P22-R7/R8 OPEN.

Do NOT execute T007 inside T006.

END_TRAVELCORE_CURSOR_TASK_V1
```
