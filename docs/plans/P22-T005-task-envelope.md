# TC-P22-T005 Task Envelope (architect, live)

Captured from the same ChatGPT tab after `TC-P22-T004 = ACCEPTED` and `P22-R5 = RESOLVED`.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1
Task-ID: TC-P22-T005
Phase: P22
Title: Flight supplier reservation / PNR lifecycle, idempotency, ambiguity, expiry, and reconciliation
Baseline: c1dbc5c
Decision: P22-R5 = RESOLVED

Purpose:
Implement the supplier reservation / PNR boundary for Flight:
- source-neutral final supplier reservation
- PNR correlation
- reservation attempts
- authoritative recheck
- expiry/time-limit semantics
- ambiguity-safe retry
- complete passenger/itinerary confirmation
- reconciliation

Do NOT implement ticketing, Payment, FlightBookingStatus, customer cancellation, public API, or T006.

Must implement:
- FlightSupplierReservation + FlightSupplierReservationId (UUIDv7)
- One FlightBooking → one logical FlightSupplierReservation covering complete itinerary/journeys/segments/passengers
- Do not model independent local PNRs per segment/passenger
- Narrow port IFlightReservationSource + resolver. No giant gateway. Only Create + Query in T005
- Reservation must use accepted FlightOfferSnapshot.SourceKey. Do not silently switch source
- Build request from persisted FlightBooking itinerary, passenger names/categories, accepted offer snapshot, monetary snapshot, source offer correlation. No client-reconstructed payload
- Passenger PII this task: GivenName, FamilyName, PassengerCategory only
- Persist SourceKey + SourceReservationReference (source-scoped uniqueness where present)
- Human-facing PNR as separate opaque ReservationLocator (not a type named PNR)
- IDs: FlightBookingId != FlightSupplierReservationId != SourceReservationReference != ReservationLocator
- FlightSupplierReservationStatus exactly: Pending, Confirmed, Expired, Cancelled
- Initial = Pending. No Failed reservation status
- Confirmed = authoritative complete reservation/PNR exists. Does not mean Payment succeeded, ticket issued, or customer journey complete
- Expired = source proves PNR/reservation expired (Confirmed → Expired only with source evidence)
- Cancelled = source proves reservation no longer active. Do not implement customer cancellation action
- FlightSupplierReservationAttempt (UUIDv7)
- Attempt statuses exactly: Created, Initiated, Confirmed, Failed
- Timeout/Unknown: Attempt stays Initiated, Reservation stays Pending. Timeout ≠ Failed
- Unresolved Created/Initiated blocks another attempt
- Only authoritative “no reservation created” may mark Attempt Failed; then a new attempt is allowed
- Confirmed reservation: no new attempt
- DB uniqueness/transactions/idempotency/concurrency; no process-local lock as authority
- Same FlightBooking + idempotency key → same effective reservation/attempt
- At most one unresolved Created/Initiated attempt
- Authoritative result must match complete journeys/segments, passenger set, source commercial reference before local Confirmed
- Partial passenger or partial itinerary → do not Confirm
- Structural mismatch → persist reconciliation; do not mutate FlightBooking or accepted snapshots
- Reconciliation kinds: ItineraryMismatch, PassengerMismatch, MonetaryMismatch, CurrencyMismatch, OfferMismatch, AmbiguousReservationOutcome, ContradictorySupplierEvidence
- Monetary/currency mismatch does not rewrite FlightBookingMonetarySnapshot; no silent reprice
- QueryReservationStatusAsync + Flight-local Recheck
- Query outcomes: Confirmed, Expired, Cancelled, NotCreated, PendingOrUnknown
- Pending/Unknown → leave unresolved, no unsafe retry
- NotCreated = definitive failure only if source contract guarantees no PNR exists
- Contradictory terminal evidence → reconciliation; do not silently flip terminal truth
- Persist ReservationExpiresAt : Instant? from source; do not fabricate; no universal TTL
- Preserve: OfferExpiresAt ≠ TicketingDeadline ≠ ReservationExpiresAt
- Capabilities: add ReservationCreate, ReservationQuery. Keep Search, AvailabilityCheck, OfferRevalidation
- Service: FlightSupplierReservationService.InitiateAsync / RecheckAsync
- MarkInitiated + SaveChanges before network (Hotel pattern)
- Catch timeout → leave Initiated/Pending
- No payment gating in T005
- Still no FlightBookingStatus
- Persistence tables:
  flight.flight_supplier_reservations
  flight.flight_supplier_reservation_attempts
  flight.flight_supplier_reservation_idempotency
  flight.flight_reconciliation_issues
- Constraints: one reservation per FlightBooking; one unresolved attempt; source-scoped reservation reference uniqueness where present
- Same-schema FK only
- New migration id must sort after 20260818220200_AddFlightOfferSnapshots (20260818220300_AddFlightSupplierReservations)

Forbidden:
- Ticket issuance (FlightTicket, TicketNumber, TicketIssued, TicketingAttempt)
- Payment integration (targets remain TourBooking + HotelBooking)
- FlightBookingStatus
- customer cancellation/refund execution
- public API/frontend
- giant gateway
- supplier SDK
- named supplier
- fake production source
- peer-schema FK
- shared DbContext
- peer Infrastructure deps
- silent repricing
- hardcoded TTL
- fabricate ReservationExpiresAt
- cancel/ticket/refund capabilities
- independent per-segment/passenger PNRs

Validation:
dotnet build TravelCore.sln
Flight.UnitTests
ArchitectureTests
Persistence.IntegrationTests
Host.IntegrationTests
git diff --check
Frontend expected untouched.

Return RESULT. Do NOT execute TC-P22-T006.

END_TRAVELCORE_CURSOR_TASK_V1
```
