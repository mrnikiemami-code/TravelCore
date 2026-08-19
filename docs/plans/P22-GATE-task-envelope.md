# TC-P22-GATE Task Envelope

Captured live after architect `TC-P22-T009 = ACCEPTED` on the same ChatGPT conversation. Baseline `856bb06`. Current HEAD at capture `e76b562` (T009 docs). Do **not** start P23.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1
Task-ID: TC-P22-GATE
Phase: P22
Title: Flight phase acceptance gate
Baseline: 856bb06

Purpose:
Perform the final acceptance gate for P22 — Flight.

This is a verification and SoT closure task.

Do NOT add new product capability.
Do NOT expand deferred scope.
Fix only a concrete acceptance-blocking defect if discovered, and report it explicitly.

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

Expected accepted lineage includes:

856bb06


2. Verify execution ledger

Require all accepted:

TC-P22-PLAN
TC-P22-T001
TC-P22-T002
TC-P22-T003
TC-P22-T004
TC-P22-T005
TC-P22-T006
TC-P22-T007
TC-P22-T008
TC-P22-T009


3. Verify architecture decisions

Require:

P22-R1 = RESOLVED
P22-R2 = RESOLVED
P22-R3 = RESOLVED
P22-R4 = RESOLVED
P22-R5 = RESOLVED
P22-R6 = RESOLVED
P22-R7 = RESOLVED
P22-R8 = RESOLVED


4. Verify core module boundary

Require:

Flight = independent module
schema = flight

FlightBooking owner = Flight

No separate FlightBooking module/schema.

No generic Booking base/platform.


5. Verify Tour separation

Require:

TourDepartureTransportSegment remains Tour-owned.

Tour package transport fact is not:

live Flight inventory
FlightOffer
PNR
Ticket


6. Verify itinerary model

Require:

FlightTripType:
- OneWay
- RoundTrip

MultiCity:
DEFERRED

OneWay:
exactly 1 journey

RoundTrip:
exactly 2 journeys

Journey:
1..N segments

Connecting flights:
supported


7. Verify passenger model

Require categories exactly:

Adult
Child
Infant

Require:

>= 1 passenger
>= 1 Adult

Verify no unapproved passenger PII was introduced:

BirthDate
Gender
Nationality
Passport/document data


8. Verify Airport/Airline boundary

Require:

Airport authority = ReferenceData
Airline authority = ReferenceData

Flight contains logical references only.

No peer-schema FK.


9. Verify source authority

Require external source-authoritative Flight posture.

Production matrix must remain:

Flight Search Source = NONE
Flight Availability Source = NONE
Flight Offer Source = NONE
Flight Reservation Source = NONE
Flight Ticketing Source = NONE
Flight Cancellation Source = NONE

Named Flight Supplier = NONE
Supplier SDK = NO


10. Verify source capabilities

Require implemented source capabilities remain explicit and narrow:

Search
AvailabilityCheck
ReservationCreate
ReservationQuery
TicketCreate
TicketQuery
CancellationQuote
ReservationCancel
TicketVoid
TicketRefund
CancellationQuery

No giant supplier gateway.


11. Verify commercial truth

Require:

FlightSearchResult
!=
FlightOfferSnapshot
!=
FlightBookingMonetarySnapshot
!=
Payment

Flight owns airline commercial snapshots.

P12 Pricing is not generalized for Flight.


12. Verify monetary invariants

Require:

one CurrencyCode
Money type / accepted precision
no float/double money
no implicit FX
Toman != CurrencyCode
no silent repricing


13. Verify offer temporal invariants

Require:

OfferExpiresAt
!=
TicketingDeadline
!=
ReservationExpiresAt

All use accepted NodaTime semantics.

No fabricated universal TTL.


14. Verify reservation lifecycle

Require:

FlightSupplierReservationStatus:
- Pending
- Confirmed
- Expired
- Cancelled

Attempt statuses:
- Created
- Initiated
- Confirmed
- Failed

Timeout remains unresolved and blocks unsafe retry.


15. Verify Payment target model

Require Payment targets exactly:

TourBooking
HotelBooking
FlightBooking

No arbitrary generic TargetType/TargetId.

Exactly one Payment target.


16. Verify Flight execution order

Require baseline:

Accepted Flight Offer
->
Confirmed Supplier Reservation
->
Payment
->
Ticket issuance
->
FlightBooking confirmation


17. Verify FlightBooking status

Require exactly:

Pending
Confirmed
Cancelled

No workflow mega-status additions.


18. Verify final confirmation invariant

FlightBooking Confirmed requires all:

SupplierReservation Confirmed
Payment Succeeded
all required passenger Tickets Issued

No weaker confirmation path may exist.


19. Verify ticketing safety

Require:

one ticket per passenger baseline
ticket ambiguity != failure
unresolved ticketing blocks duplicate issuance
partial ticket issuance cannot confirm FlightBooking


20. Verify compensation

Require paid-but-uncompleted compensation:

- distinct from customer cancellation
- full Refund only
- Payment owns Refund
- no automatic Refund while ticket state is ambiguous
- PaymentStatus remains Succeeded after Refund


21. Verify confirmed cancellation

Require separate:

FlightBooking cancellation
supplier reservation cancellation
ticket void/refund
Payment Refund


22. Verify cancellation economics

Require:

Penalty = 0
=> FullRefund

Penalty = TotalAmount
=> NoRefund

0 < Penalty < TotalAmount
=> PartialRefundRequiredButUnsupported


23. Verify partial-refund safety

Critical:

PartialRefundRequiredButUnsupported
must produce zero irreversible supplier reversal calls.

FlightBooking remains Confirmed.


24. Verify deferred scope remains deferred

Require NOT implemented:

Partial Refund
MultiCity
Ancillaries
PayLater
Deposit
Partial Payment
Amendments
Rebooking
No-show
Per-passenger cancellation
Partial-itinerary cancellation
Smart supplier routing
Automatic failover
Real Flight supplier
Real Payment provider


25. Verify public API security

Require header exactly:

X-TravelCore-Flight-Booking-Access-Token

Require:

raw token persisted = NO
hash/verifier persisted = YES
token in URL = NO
localStorage = NO

BookingId alone is not authorization.


26. Verify token isolation

Require Flight token cannot authorize:

Tour Booking
HotelBooking

and vice versa.


27. Verify public API posture

Require behavior-oriented endpoints only.

No generic:

public list
PUT/PATCH booking
set status
force confirm
mark paid
force ticket
force refund


28. Verify client authority

Client must NOT authoritatively control:

price
currency
availability truth
PNR truth
Payment success
ticket truth
refund truth
cancellation economics
FlightBooking status


29. Verify Payment security

Require:

no card collection
no provider secrets exposed
no fake Payment success
Production Payment Provider = NONE


30. Verify public state correctness

Require:

PNR Confirmed
!=
Payment Succeeded
!=
Ticket Issued
!=
FlightBooking Confirmed

Partial ticketing must not present Confirmed.


31. Verify SEO/i18n/frontend

Require private transactional pages:

noindex

Require:

FA
EN
AR

RTL/LTR/bidi-safe.

Mobile baseline and accessibility baseline remain intact.


32. Verify operational boundary

Require:

IFlightOperationalQuery

read-only/internal.

No public operational mutation API.


33. Verify persistence boundaries

Require:

no peer-schema FK
no shared DbContext
no peer Infrastructure dependency
no cross-schema SQL
no distributed transaction


34. Verify database constraints

Check accepted constraints for:

- one accepted offer per FlightBooking
- one monetary snapshot per FlightBooking
- one reservation per FlightBooking
- one unresolved reservation attempt
- one Payment per FlightBooking
- exactly-one Payment target
- one ticket per passenger
- one unresolved ticketing attempt
- one cancellation per FlightBooking
- source-scoped uniqueness
- idempotency


35. Review T009 evidence pack

Use:

docs/plans/P22-T009-hardening-and-evidence-pack.md

Verify it remains consistent with current repository state.

Do not accept stale evidence blindly.


36. Full validation

Run:

dotnet build TravelCore.sln

Flight.UnitTests
Payment.UnitTests
Booking.UnitTests
HotelBooking.UnitTests
Tour.UnitTests

ArchitectureTests
Persistence.IntegrationTests
Host.IntegrationTests

Frontend:

npm run typecheck
npm run lint
npm run build

Then:

git diff --check


37. Gate artifact

Create:

docs/plans/P22-GATE-acceptance-evidence.md

Include:

- final execution ledger
- R1-R8 decision matrix
- architecture boundary evidence
- test counts
- frontend validation
- production source/provider matrix
- public endpoint inventory
- security/token evidence
- payment/ticket/cancellation evidence
- deferred scope
- any gate defects and corrections
- final acceptance verdict


38. Gate outcome

If all requirements pass:

TC-P22-GATE = ACCEPTED
P22 = COMPLETE

If any material requirement fails:

TC-P22-GATE = FAILED
P22 remains IN_PROGRESS

List exact blocker(s).


39. SoT closure on PASS

Update authoritative:

docs/PROJECT-STATE.md
docs/ROADMAP.md
docs/plans/P22-implementation-plan.md

Record:

P22 — Flight = COMPLETE
TC-P22-GATE = ACCEPTED

Preserve all deferred items explicitly.


40. Determine next phase

After P22 closure, inspect authoritative roadmap/state.

Report the exact next phase/task from SoT.

Do NOT invent a phase from memory.

Do NOT execute the next phase.


41. Required result evidence

Return:

- Gate status
- baseline
- implementation/SoT commit
- current HEAD
- HEAD == origin/main
- Working Tree
- all backend test counts
- frontend typecheck/lint/build
- P22-R1 through R8 statuses
- Payment target exact values
- FlightBookingStatus exact values
- production Flight source matrix
- Production Payment Provider
- raw token persisted
- token in URL
- localStorage
- peer-schema FK
- shared DbContext
- distributed transaction
- Partial Refund implemented
- real supplier implemented
- real Payment provider implemented
- defects discovered/fixed
- deferred-scope list
- gate evidence artifact
- P22 status
- exact next phase/task from authoritative SoT
- next phase executed: NO


42. Commit/push

On PASS:

- commit with TC-P22-GATE in message
- push normal fast-forward to origin/main
- re-fetch origin
- verify HEAD == origin/main
- verify Working Tree CLEAN


43. Stop

Return TC-P22-GATE RESULT to architect.

Do NOT execute the next phase.

END_TRAVELCORE_CURSOR_TASK_V1
```
