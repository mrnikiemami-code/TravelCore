# TC-P22-T004 Task Envelope (architect, live)

Captured from the same ChatGPT tab after `TC-P22-T003 = ACCEPTED` and `P22-R4 = RESOLVED`.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1
Task-ID: TC-P22-T004
Phase: P22
Title: Flight offer, fare, monetary snapshot, revalidation, and fare-rule boundary
Baseline: 6470cf8
Decision: P22-R4 = RESOLVED

Purpose:
Implement immutable Flight commercial offer acceptance:
- authoritative FlightOfferSnapshot
- FlightBookingMonetarySnapshot
- source revalidation before acceptance
- fare breakdown
- fare rules / baggage commercial facts
- offer expiry and repricing rules

Do NOT implement PNR, ticketing, Payment, cancellation execution, public API, or T005.

Must implement:
- Narrow port IFlightOfferSource (or equivalent). Extend R3 source model minimally. No giant gateway.
- Revalidate before accept: itinerary identity, passenger composition, commercial availability, price, currency, expiry, fare-rule facts. Search-result price alone is insufficient.
- Immutable FlightOfferSnapshot + FlightOfferSnapshotId (UUIDv7), bound to exactly one FlightBooking
- Structural bind to persisted booking: TripType, journeys, segments, airports, carriers/flight numbers, dep/arr, passenger categories/counts
- Do not accept a client-reconstructed itinerary; derive from persisted FlightBooking
- Persist SourceKey + SourceOfferReference (source-scoped uniqueness)
- QuotedAt : Instant, OfferExpiresAt : Instant from source; no universal TTL
- now >= OfferExpiresAt → cannot accept
- No silent repricing (higher or lower) — explicit conflict/requote-required
- Immutable FlightBookingMonetarySnapshot = authoritative customer amount after accept
- One CurrencyCode per booking; reject mixed currency; no FX; Toman != CurrencyCode
- Use TravelCore Money (no float/double)
- Breakdown: BaseFare + Taxes + Fees = TotalAmount (source-supplied; TravelCore is not an airline tax engine)
- Optional per-passenger-category breakdown if source provides it
- FlightFareRulesSnapshot: structured facts, not free-text-only
  - refundable/non-refundable, changeability, cancel/change penalty if source provides, ticketing deadline if provided
- TicketingDeadline : Instant distinct from OfferExpiresAt
- Do not execute cancellation (R7) or Partial Refund (DEFERRED); may snapshot partial-refund-required fact
- Optional baggage allowance as simple structured fact (qty/weight/unit/category)
- Optional cabin / BookingClass / FareBasis / FareFamily as opaque source facts
- No ancillaries: seats, meals, extra baggage purchase, lounge, priority boarding
- Complete booking coverage only; reject partial passenger/journey offers
- At most one accepted offer + one monetary snapshot per FlightBooking
- Idempotency: same source offer + same acceptance key → same snapshot
- Different offer after accept cannot silently replace
- DB uniqueness/transactions; no process-local lock as authority
- Service e.g. FlightOfferAcceptanceService
- Preconditions: booking exists, structure matches, authoritative revalidation, available, unexpired, one currency, complete coverage
- R3 Available alone is not enough; R4 commercial revalidation is fare authority
- Timeout/Unknown → do not accept; Changed → do not accept old commercial truth
- Still no FlightBookingStatus, PNR, ticket, Payment, public API
- Persistence tables:
  flight.flight_offer_snapshots
  flight.flight_booking_monetary_snapshots
  flight.flight_fare_rule_snapshots
  plus child tables only if needed for immutable breakdown
- Same-schema FK only

Forbidden:
- PNR/reservation
- ticketing
- Payment integration (targets remain TourBooking + HotelBooking)
- cancellation/refund execution
- public API/frontend
- generalize P12 Pricing
- silent repricing
- hardcoded TTL
- giant gateway
- supplier SDK
- named supplier
- fake production source
- peer-schema FK
- shared DbContext
- peer Infrastructure deps
- FlightBookingStatus
- ancillaries

Validation:
dotnet build TravelCore.sln
Flight.UnitTests
ArchitectureTests
Persistence.IntegrationTests
Host.IntegrationTests
git diff --check
Frontend expected untouched.

Unit tests must cover:
valid accept; expired rejected; timeout/unknown rejected; changed → requote/conflict; higher/lower repricing not silent; itinerary mismatch; passenger mismatch; partial offer rejected; mixed currency rejected; money arithmetic; same-offer idempotent; different-offer conflict; ticketing deadline ≠ offer expiry.

Persistence tests:
immutable round-trip for offer/monetary/fare-rules/baggage facts/provenance/timestamps/uniqueness.
New migration id must sort after 20260818220100_AddFlightBookingItinerary
(do not change Initial 20260818220000_InitialFlightScaffolding).

Do NOT execute TC-P22-T005.

Locked inherited:
P22-R1, P22-R2, P22-R3 RESOLVED
Named Flight Supplier = NONE
Production Search/Availability/Offer Source = NONE
Payment kinds stay TourBooking, HotelBooking
Partial Refund remains DEFERRED

END_TRAVELCORE_CURSOR_TASK_V1
```
