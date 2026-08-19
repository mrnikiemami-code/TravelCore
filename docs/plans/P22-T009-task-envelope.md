# TC-P22-T009 Task Envelope (architect, live)

Captured after `TC-P22-T008 = ACCEPTED`. Implementation `d7c61d7`. Docs `65cf720`. HEAD `65cf720` == `origin/main`. P22-R1 through P22-R8 = RESOLVED. Do **not** execute `TC-P22-GATE`.

```text
TC-P22-T008 = ACCEPTED

Implementation Commit:
d7c61d7

Docs Commit:
65cf720

Current HEAD:
65cf720

HEAD == origin/main:
YES
```

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version:
1

Task-ID:
TC-P22-T009

Phase:
P22

Title:
Flight phase hardening, cross-module regression, security review, and gate evidence pack

Baseline:
65cf720

Decision:
P22-R1 through P22-R8 = RESOLVED

Purpose:
Perform final P22 hardening and produce the complete acceptance evidence pack for
Flight before TC-P22-GATE.

This task must NOT introduce new Flight product capability.

The goal is to prove that the accepted P22 architecture actually holds under:

- authorization attacks
- concurrency
- retries
- duplicate messages
- crash windows
- ambiguous supplier/source outcomes
- cross-booking/cross-target correlation
- immutable pricing/cancellation rules
- Payment/Refund compensation
- public API tampering
- frontend token handling
- zero-source/source production configuration
- modular-monolith boundaries
- regressions against accepted Tour Booking / HotelBooking / Payment behavior

Do NOT execute TC-P22-GATE.

1. Repository preflight

Run:

git rev-parse --show-toplevel
git fetch origin
git branch --show-current
git rev-parse HEAD
git rev-parse origin/main
git status --short

Require:

branch = main
HEAD == origin/main
Working Tree = CLEAN

Expected baseline:

65cf720


2. Record T008 acceptance

Synchronize authoritative SoT to record:

TC-P22-T008 = ACCEPTED (d7c61d7 / 65cf720)

Preserve:

P22-R1 through P22-R8 = RESOLVED

P22 remains IN_PROGRESS

TC-P22-GATE = NOT EXECUTED

P22 marked COMPLETE: NO


3. Scope posture

T009 is:

HARDENING + REGRESSION + EVIDENCE

not feature development.


4. No new architecture decisions

Do NOT invent P22-R9.

Do NOT reopen R1-R8 unless an actual correctness defect is found.


5. Correction policy

If a genuine defect is discovered:

fix only the smallest necessary defect inside already accepted P22 semantics.

Do not redesign the phase.

Document every concrete defect found and fixed.


6. Module ownership regression

Verify:

Flight != Tour
FlightBooking != Tour Booking
FlightBooking != HotelBooking
Tour Package Flight != live Flight inventory

schema = flight

no separate FlightBooking module/schema

TourDepartureTransportSegment stays Tour-owned


7. Airport / Airline catalog

Verify:

Airport/Airline authority = ReferenceData

Flight stores IATA logical references only

no peer-schema FK


8. Itinerary / passenger baseline

Verify:

TripType = OneWay / RoundTrip only
MultiCity = DEFERRED
1 Adult minimum
Adult / Child / Infant

PII stored: GivenName / FamilyName / PassengerCategory only

BirthDate / Gender / Nationality / Passport = NO


9. Temporal authority

Verify:

NodaTime Instant + IANA timezone

no naive DateTime as authority


10. Source ports and production posture

Verify:

no giant IFlightSupplierGateway

Named Flight Supplier = NONE

Production Search / Availability / Offer / Reservation / Ticketing / Cancellation Source = ALL NONE

Production Payment Provider = NONE

supplier SDK = NO

no fake production success


11. Timeout semantics

Verify:

Timeout != Failed
Timeout != Unavailable

Unknown remains unresolved


12. Offer / monetary snapshots

Verify:

immutable FlightOfferSnapshot
immutable FlightBookingMonetarySnapshot
search price is not monetary authority
no silent repricing
OfferExpiresAt != TicketingDeadline != ReservationExpiresAt

P12 Pricing is not generalized to airline fares


13. Reservation lifecycle

Verify:

one FlightSupplierReservation per FlightBooking

statuses: Pending / Confirmed / Expired / Cancelled

attempts: Created / Initiated / Confirmed / Failed


14. Payment target model

Verify:

PaymentTargetKind exactly TourBooking, HotelBooking, FlightBooking

one FlightBooking -> one Payment

exactly-one-target constraint


15. Order and confirmation

Verify Flight order:

accepted offer
-> confirmed reservation
-> Payment
-> tickets
-> FlightBooking Confirmed

Triple evidence confirmation required.

Payment Succeeded != FlightBooking Confirmed
PNR Confirmed != FlightBooking Confirmed
Ticket Issued != FlightBooking Confirmed


16. Ticketing ambiguity and R6 compensation

Verify:

ticketing timeout remains unresolved (Initiated)
no automatic Refund while ambiguous

R6 compensation is distinct from R7 cancellation
full refund only
PaymentStatus stays Succeeded after Refund
Partial Refund remains DEFERRED


17. Cancellation distinctions

Verify:

Cancellation process
!= reservation cancel
!= ticket void/refund
!= Payment Refund

Penalty = 0 -> FullRefund
Penalty = Total -> NoRefund
partial penalty blocked with 0 supplier calls
FlightBooking stays Confirmed

partial ticket reversal cannot cancel booking or trigger Payment Refund


18. Access token

Verify exact header:

X-TravelCore-Flight-Booking-Access-Token

raw token never persisted
hash/verifier persisted (SHA-256)
never in URL
never in localStorage
sessionStorage allowed for private frontend flow


19. Non-credentials

Verify these are not credentials:

FlightBookingId
PaymentId
ReservationLocator
TicketNumber


20. Authorization attacks

Verify:

missing token = 404
wrong token = 404
cross-user = 404
Flight/Tour/Hotel token isolation


21. Client is not authority

Client cannot author:

price
PNR
payment success
ticket success
penalty
status


22. No card collection

Search API/frontend for:

card number
PAN
CVV
CVC
PIN
bank password

Expected: NONE


23. Public surface

Verify:

no generic list
no PUT/PATCH
no force* endpoints
private pages noindex
FA/EN/AR + bidi


24. Operational surface

Verify:

IFlightOperationalQuery is internal read-only
no operational HTTP
no ForceConfirm / ForceTicket / ForceCancel / MarkPaid / MarkRefunded / SetStatus


25. Modular-monolith boundaries

Verify none of:

shared DbContext
peer Infrastructure dependency
cross-schema SQL
distributed transaction
BookingBase / GenericBookingAggregate


26. Persistence uniqueness constraints

Verify database-backed uniqueness including:

one FlightBooking -> one offer snapshot
one FlightBooking -> one monetary snapshot
one FlightBooking -> one supplier reservation
one unresolved reservation attempt
one FlightBooking -> one Payment (ux_payments_flight_booking_id)
exactly-one Payment target
one FlightBooking -> one ticket per passenger
one unresolved ticketing attempt
one FlightBooking -> one cancellation
one unresolved reversal attempt per kind
token_hash unique
initiation idempotency key unique


27. Idempotency / concurrency scenarios

Verify same-key / concurrent:

initiation
offer accept
reservation
Payment creation
ticketing
cancellation
Refund trigger
dual/triple evidence confirmation race


28. Failure / recovery matrix A–N

Document durable state / retry / auto-refund / reconciliation for:

A. search/availability timeout
B. offer timeout/unknown/changed
C. reservation timeout
D. reservation definitive failure
E. Payment success + ticketing timeout
F. Payment success + ticketing definitive failure
G. duplicate PaymentSucceeded delivery
H. crash after Payment success before local ticketing continuation
I. partial ticketing
J. customer cancel Penalty=0 FullRefund
K. customer cancel Penalty=Total NoRefund
L. partial penalty blocked
M. cancellation/reversal timeout
N. partial ticket reversal


29. Security grep

Search token/hash/secrets/card leaks.

Fix actual leaks only.

Do not print real secrets if unexpectedly found.


30. Frontend token storage

Verify sessionStorage only for Flight access token.


31. Evidence artifact

Create:

docs/plans/P22-T009-hardening-and-evidence-pack.md

Clone structure/quality of:

docs/plans/P21-T009-hardening-and-evidence-pack.md


32. Hardening guardrails

Create:

tests/Architecture/TravelCore.ArchitectureTests/FlightHardeningGuardrailTests.cs

if not already covered by:

FlightPublicJourneyGuardrailTests
FlightBoundaryGuardrailTests

Add only missing adversarial checks.

Clone quality of:

tests/Architecture/TravelCore.ArchitectureTests/HotelBookingHardeningGuardrailTests.cs


33. Evidence pack structure

Include at minimum:

1. Scope and baseline
2. P22 decision inventory
3. Module/schema ownership proof
4. Domain invariant proof
5. Search/availability/offer proof
6. Fare/monetary/fare-rules proof
7. Supplier reservation proof
8. Payment/ticketing/compensation proof
9. Confirmed cancellation proof
10. Public authorization/privacy proof
11. Frontend/noindex/a11y/bidi proof
12. Concurrency/idempotency proof
13. Outbox/inbox/crash-recovery proof
14. Cross-target/cross-booking isolation proof
15. Zero-source/provider posture
16. Deferred/out-of-scope inventory
17. Exact test/build results
18. Remaining known limitations
19. Gate readiness conclusion


34. Known limitations

The evidence pack must honestly include at least:

- no production Flight supplier
- no production Flight search/availability/offer/reservation/ticketing/cancellation source
- no production Payment provider
- Partial Refund unavailable
- partial-penalty confirmed cancellation blocked
- MultiCity unavailable
- ancillaries unavailable
- PayLater/deposit/partial collection unavailable
- amendments/rebooking/no-show unavailable
- per-passenger / partial-itinerary cancellation unavailable
- no smart supplier routing/failover


35. No false production-ready claim

Do not claim P22 can perform a real-world Flight reservation/payment/ticketing
in production without configured real adapters/providers.


36. Gate-ready meaning

READY_FOR_P22_GATE means:

architecture and implemented P22 scope are internally correct and tested.

It does NOT mean external production provider integrations exist.

Gate recommendation must be exactly:

READY_FOR_P22_GATE

or

NOT_READY_FOR_P22_GATE

with blockers.


37. Product-code change threshold

If no defects are found:

prefer tests/docs only.

Do not refactor working code for style.


38. Defect handling

If a defect is found:

fix it
add regression test
document correction in evidence pack.


39. No scope expansion

Do not implement deferred capability to make a test pass.


40. Persistence uniqueness constraints (authoritative list)

Report actual unique indexes / check constraints:

- ux_flight_offer_snapshots_flight_booking_id
- ux_flight_offer_snapshots_source_offer
- ux_flight_booking_monetary_snapshots_flight_booking_id
- ux_flight_supplier_reservations_flight_booking_id
- ux_flight_supplier_reservations_source_ref
- ux_flight_supplier_reservation_attempts_one_unresolved
- ux_flight_tickets_booking_passenger
- ux_flight_tickets_source_ticket_number
- ux_flight_ticketing_attempts_one_unresolved
- ux_flight_booking_cancellations_flight_booking_id
- ux_flight_supplier_reversal_attempts_one_unresolved_reservation
- ux_flight_supplier_reversal_attempts_one_unresolved_ticket
- ux_flight_booking_access_credentials_token_hash
- ux_flight_booking_payment_evidence_payment_id
- ux_flight_booking_payment_compensation_evidence_flight_booking_id
- ux_payments_flight_booking_id
- ck_payments_exactly_one_target
- ck_refunds_exactly_one_target
- flight_booking_public_idempotency PK (idempotency_key)


41. Idempotency / concurrency scenarios (authoritative list)

Exercise / cite existing proof for:

- duplicate public initiation same idempotency key
- concurrent initiation
- duplicate offer accept
- duplicate reservation initiation
- unresolved reservation attempt blocks unsafe retry
- failed reservation attempt allows explicit retry
- one Payment per FlightBooking
- duplicate PaymentSucceeded inbox
- unresolved ticketing attempt blocks unsafe retry
- duplicate cancellation same idempotency key
- unresolved reversal blocks unsafe retry
- duplicate compensation / Refund creation
- triple-evidence confirmation at most once


42. Failure / recovery matrix A–N (authoritative)

For each: durable state / retry allowed? / auto-refund? / reconciliation?

A. Search/availability timeout -> Unknown, not Unavailable; no fabricated options
B. Offer timeout/unknown/changed -> cannot accept; no silent replace
C. Reservation timeout -> attempt Initiated; reservation Pending; retry blocked until Failed/recheck
D. Reservation Failed -> explicit new attempt allowed; no Payment
E. Payment Succeeded + ticketing timeout -> attempt Initiated; Booking Pending; NO automatic Refund
F. Payment Succeeded + ticketing definitive failure -> full Refund compensation (R6); PaymentStatus stays Succeeded
G. Duplicate PaymentSucceeded -> one effective local result
H. Crash after Payment success/outbox -> eventual ticketing continuation via durable inbox/outbox
I. Partial ticketing -> not Confirmed; no whole-booking cancel; no Payment Refund
J. Confirmed cancel Penalty=0 -> FullRefund after authoritative supplier reversal (R7)
K. Confirmed cancel Penalty=Total -> NoRefund; completes without Refund
L. Partial penalty -> blocked; 0 supplier calls; Booking stays Confirmed
M. Cancellation/reversal timeout -> attempt Initiated; Booking Confirmed; Refund not started
N. Partial ticket reversal -> cannot cancel Booking; cannot trigger Payment Refund


43. Frontend validation

Run from src/frontend/web:

npm run typecheck
npm run lint
npm run build


44. Backend validation

Run:

dotnet build TravelCore.sln

Flight.UnitTests
Payment.UnitTests
Booking.UnitTests
HotelBooking.UnitTests
Tour.UnitTests if present
ArchitectureTests
Persistence.IntegrationTests
Host.IntegrationTests


45. git validation

Run:

git diff --check


46. SoT synchronization

Update:

docs/PROJECT-STATE.md
docs/ROADMAP.md
docs/plans/P22-implementation-plan.md

Record:

TC-P22-T008 = ACCEPTED (d7c61d7 / 65cf720)
TC-P22-T009 = implemented / AWAITING_ARCHITECT_REVIEW
P22 remains IN_PROGRESS
P22-R1 through P22-R8 = RESOLVED
TC-P22-GATE NOT EXECUTED
P22 marked COMPLETE: NO


47. Do not execute Gate

TC-P22-GATE = NOT EXECUTED

Do not change P20/P21 GATE evidence files.


48. Allowed

- hardening tests
- security/adversarial tests
- concurrency/idempotency tests
- architecture guardrails
- integration/host/frontend regressions
- minimal fixes to accepted P22 correctness defects
- documentation drift fixes
- evidence pack
- SoT synchronization for T009


49. Forbidden

- new P22 business capability
- P22-R9
- Partial Refund
- MultiCity
- ancillaries
- PayLater / deposit / partial payment
- amendments / rebooking / no-show
- per-passenger / partial-itinerary cancellation
- real supplier
- supplier SDK
- real Payment provider
- Payment provider SDK
- smart supplier routing/failover
- accounting / settlement / agency commission / wallet / fraud / loyalty
- generic Booking platform
- generic arbitrary Payment target
- public CRUD
- operational mutation
- shared DbContext
- peer-schema FK
- peer Infrastructure dependency
- distributed transaction
- unrelated refactor
- dependency upgrade
- TC-P22-GATE execution
- ChatGPT page access
- commit or push (this execution: USER forbade commit/push)


50. Done criteria

- all P22-R1 through P22-R8 invariants are revalidated
- no hidden boundary leakage exists
- public FlightBooking authorization withstands enumeration/token/cross-user attacks
- Payment / Tour Booking / HotelBooking regressions remain green
- supplier/payment/ticketing ambiguity handling is proven
- duplicate/retry/concurrency paths remain safe
- partial-penalty cancellation remains blocked before irreversible effects
- triple evidence is race-safe
- full-refund compensation is durable/idempotent
- public state presentation does not fabricate success
- no fake production source/provider exists
- no provider secrets/token leakage exists
- frontend token/noindex/a11y/bidi/mobile invariants hold
- deferred scope remains deferred
- P22-T009 hardening/evidence pack exists
- gate recommendation is READY_FOR_P22_GATE or NOT_READY_FOR_P22_GATE
- TC-P22-GATE is NOT EXECUTED


51. Required Result Evidence

Report exact:

- test counts
- frontend results
- defects found/fixed
- evidence path
- gate recommendation
- files changed

Do not commit.


52. Repository safety

- discover repository root using git rev-parse --show-toplevel
- git fetch origin
- require branch main
- require HEAD == origin/main before work

Forbidden repository operations:

- force push
- accepted-history rewrite
- reset discarding accepted work
- duplicate cherry-picks
- commit (this execution)
- push (this execution)


53. Auto-Execute / stop

After PASS:

- return TC-P22-T009 RESULT to architect
- do NOT execute TC-P22-GATE until T009 is architect ACCEPTED
- do NOT mark P22 COMPLETE

Expected Baseline:
65cf720

END_TRAVELCORE_CURSOR_TASK_V1
```
