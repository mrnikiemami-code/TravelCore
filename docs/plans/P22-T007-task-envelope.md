# TC-P22-T007 Task Envelope (architect, live)

Captured from the same ChatGPT tab after `TC-P22-T006 = ACCEPTED` and `P22-R7 = RESOLVED`. Envelope baseline `57731ed`. Working HEAD at start: `935b668`.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1
Task-ID: TC-P22-T007
Phase: P22
Title: Flight confirmed cancellation, ticket void/refund boundary, supplier reversal, and Payment refund policy
Baseline: 57731ed
Decision: P22-R7 = RESOLVED

Purpose:
Implement the safe customer/business cancellation boundary for a Confirmed
FlightBooking.

Preserve:

FlightBooking cancellation
!=
supplier reservation cancellation
!=
ticket void/refund
!=
Payment Refund

P22 baseline may execute cancellation only when authoritative fare/supplier
economics result in:

- full customer Refund, or
- no customer Refund.

If a partial customer Refund would be required, cancellation must be blocked
before irreversible supplier-side cancellation/void/refund effects.

Partial Refund remains NOT IMPLEMENTED.

Do NOT implement amendments, rebooking, public API/UI, real supplier, or real
Payment provider.

1. Repository preflight

Run:

git rev-parse --show-toplevel
git fetch origin

Require:

branch = main
HEAD == origin/main
Working Tree = CLEAN

Expected baseline:

57731ed


2. SoT

Record:

TC-P22-T006 = ACCEPTED
P22-R7 = RESOLVED

Keep:

P22-R8 = OPEN
TC-P22-T008 = NOT EXECUTED


3. Cancellation target

Customer/business cancellation baseline applies only to:

FlightBookingStatus.Confirmed


4. Separate process

Introduce:

FlightBookingCancellation

or equivalent.

Do not encode cancellation workflow inside FlightBookingStatus.


5. Process status

Use the smallest explicit lifecycle needed.

Preferred:

Requested
SupplierReversalPending
RefundPending
Completed

Report exact enum used.


6. No mega status

Do NOT add to FlightBookingStatus:

Cancelling
RefundPending
Refunded
VoidPending
Failed


7. Cancellation authority

Financial cancellation outcome must derive from:

accepted FlightFareRulesSnapshot
+
authoritative supplier cancellation/refund quote where the source protocol
requires revalidation.

Do not use client-supplied penalty/refund values.


8. RequestedAt

Use:

NodaTime Instant
IClock


9. No live silent rewrite

Supplier may report changed cancellation economics, but must not silently mutate
the accepted FlightOfferSnapshot / FlightBookingMonetarySnapshot.


10. Cancellation quote port

Extend Flight supplier contracts with a narrow source-neutral operation such as:

IFlightCancellationSource

or equivalent.

Avoid a giant gateway.


11. Required source capabilities

Add only capabilities actually implemented by R7, conceptually:

CancellationQuote
ReservationCancel
TicketVoid
TicketRefund
CancellationQuery

Use the smallest justified set.


12. Source consistency

Cancellation/reversal must use the source that owns the confirmed reservation and
issued tickets.

No cross-source reversal.


13. Named supplier

Remain:

NONE


14. Production cancellation source

Remain:

NONE


15. No supplier SDK

Hard requirement.


16. Cancellation economics

Determine authoritative customer outcome:

RefundAmount = TotalAmount - PenaltyAmount

using accepted Money/CurrencyCode semantics.


17. Supported outcome A

If:

PenaltyAmount = 0

then:

FullRefund


18. Supported outcome B

If:

PenaltyAmount = TotalAmount

then:

NoRefund


19. Unsupported outcome

If:

0 < PenaltyAmount < TotalAmount

then:

PartialRefundRequiredButUnsupported


20. Critical safety

For partial-refund-required cancellation:

- no reservation cancellation call
- no ticket void/refund call
- no Payment Refund
- FlightBooking remains Confirmed


21. Partial Refund

Remain:

DEFERRED


22. Ticket reversal distinction

Model supplier ticket reversal separately from Payment Refund.

A supplier ticket may be:

voided
or
refunded

depending on supplier rules/timing.


23. Ticket state evolution

Extend FlightTicketStatus only as required for R7.

Preferred final values:

Pending
Issued
Voided
Refunded

If exact implementation needs different naming, keep equivalent semantics and
report it.


24. Void meaning

Voided means supplier authoritatively confirms the issued ticket was voided.


25. Refunded ticket meaning

Refunded means supplier authoritatively confirms airline-side ticket refund/reversal.

This is NOT the same as Payment Refund to customer.


26. Supplier reservation

Confirmed supplier reservation may transition to:

Cancelled

only from authoritative supplier evidence.


27. Reversal ordering

For Confirmed FlightBooking:

1. evaluate cancellation economics
2. reject unsupported partial-refund case
3. initiate authoritative supplier ticket/reservation reversal
4. recheck ambiguous outcomes
5. only after required supplier reversal is authoritative:
   FlightBooking -> Cancelled
6. request Payment Refund only when FullRefund applies


28. No money before external certainty

Do NOT refund customer while ticket/reservation reversal outcome is ambiguous.

Avoid:

valid active ticket
+
customer already refunded


29. Ticket reversal attempts

Introduce durable attempt entity/entities using existing style.

Statuses:

Created
Initiated
Succeeded
Failed


30. Timeout

Network timeout / Unknown:

remains Initiated.

Do NOT mark Failed.


31. Unsafe retry

Unresolved reversal attempt blocks duplicate supplier action.


32. Definitive failure

Retry allowed only after authoritative proof the previous attempt failed without
completing the reversal.


33. Recheck

Provide authoritative query/recheck for ambiguous cancellation/void/refund state.


34. Multi-passenger safety

FlightBooking cannot become Cancelled while required passenger tickets remain
authoritatively active.


35. Partial ticket reversal

If only some passenger tickets are voided/refunded:

- FlightBooking remains Confirmed
- do not issue Payment Refund
- persist reconciliation evidence


36. Reservation/ticket contradiction

If PNR cancellation says cancelled but some tickets remain active:

do not mark FlightBooking Cancelled.

Reconciliation required.


37. No-refund case

For Penalty = TotalAmount:

after all required supplier reversals are authoritative:

FlightBooking Confirmed -> Cancelled
Cancellation -> Completed
Payment Refund = NONE


38. Full-refund case

For Penalty = 0:

after all required supplier reversals are authoritative:

FlightBooking -> Cancelled
persist durable full-refund-required event
Cancellation -> RefundPending


39. Payment ownership

Payment remains owner of customer money movement.


40. Refund amount authority

Payment uses its own:

PaymentExecutionSnapshot

Do not trust refund amount from Flight event payload.


41. One logical Refund

Preserve Payment baseline uniqueness/idempotency.


42. Refund success

After authoritative full Payment Refund success:

FlightBooking remains Cancelled
Cancellation -> Completed
PaymentStatus remains Succeeded


43. Confirmed cancellation vs R6 compensation

Keep distinct:

R6:
paid but never successfully completed Booking

R7:
customer/business cancellation of already Confirmed FlightBooking


44. Refund correlation

Ensure Refund-success handling can distinguish R6 compensation from R7 confirmed
cancellation.


45. No generic Cancel

FlightBooking transition must use a constrained method such as:

CancelFromAuthoritativeSupplierReversal

No generic:

Cancel()
SetCancelled()
ForceCancel()


46. Client/browser trust

Browser/client cancellation success flags cannot mutate domain truth.


47. Callback verification

Unverified supplier callback cannot void/refund tickets or cancel Booking.


48. Idempotency

Cancellation request must be DB-backed idempotent.

Same key -> same logical cancellation.


49. Concurrent cancellation

At most one logical active cancellation process per FlightBooking.


50. Cross-booking isolation

Cancellation evidence for Booking A cannot affect B.


51. Cross-passenger isolation

Ticket reversal evidence must correlate to the correct passenger ticket.


52. Reconciliation reasons

Add only minimal R7 reasons, e.g.:

PartialTicketReversal
SupplierCancellationAmbiguous
SupplierEconomicsMismatch
TicketStillActive
ContradictorySupplierEvidence

Do not create generic support-ticket workflow.


53. Amendments

Remain DEFERRED:

- date changes
- route changes
- passenger changes
- fare changes
- rebooking


54. No-show

Remain DEFERRED.


55. Per-passenger customer cancellation

Remain DEFERRED.

Baseline cancellation is whole FlightBooking only.


56. Partial itinerary cancellation

Remain DEFERRED.


57. Public API/UI

P22-R8 remains OPEN.

No public cancellation endpoint yet.


58. Payment regression

Do not change:

PaymentStatus
PaymentAttemptStatus
RefundStatus
RefundAttemptStatus


59. No generic Payment target changes

Supported targets remain exactly:

TourBooking
HotelBooking
FlightBooking


60. Persistence

Add minimum Flight-owned tables for:

FlightBookingCancellation
supplier reversal attempts
cancellation idempotency
reconciliation additions/inbox/outbox as needed

Same-schema FK only.


61. Architecture boundaries

No:

peer-schema FK
shared DbContext
peer Infrastructure dependency
distributed transaction
process-local correctness authority


62. Unit tests

Cover at minimum:

- Confirmed booking cancellation eligibility
- Pending booking rejected
- Penalty=0 -> FullRefund path
- Penalty=Total -> NoRefund path
- partial penalty -> blocked
- partial penalty supplier call count = 0
- ticket reversal timeout -> unresolved
- unresolved blocks retry
- definitive failure permits retry
- complete multi-passenger reversal
- partial passenger reversal -> no Booking cancellation
- supplier contradiction -> reconciliation
- no-refund cancellation -> Completed without Payment Refund
- full-refund cancellation -> RefundPending
- RefundSucceeded -> Completed
- Payment remains Succeeded
- duplicate cancellation idempotent
- concurrent cancellation one process
- cross-booking isolation


63. Payment tests

Cover duplicate full-refund event:

one Refund only.

TourBooking/HotelBooking Payment regressions must stay green.


64. Persistence tests

Cover:

cancellation process
attempt history
idempotency
one cancellation per Booking
ticket state transitions
inbox/outbox durability


65. SoT summary

Record:

P22-R7 = RESOLVED

with:

- confirmed Flight cancellation is a separate process
- cancellation economics are authoritative and immutable for customer outcome
- full-refund and no-refund outcomes are executable
- partial customer Refund remains unsupported
- partial-refund cancellation is blocked before supplier side effects
- ticket void/refund is distinct from Payment Refund
- supplier reversal must be authoritative before FlightBooking cancellation
- ambiguous supplier reversal does not trigger Payment Refund
- partial ticket reversal cannot cancel the whole Booking
- Payment owns customer Refund execution
- PaymentStatus remains Succeeded after Refund
- whole-booking cancellation only
- amendments/rebooking/no-show remain DEFERRED
- Named Flight Supplier = NONE
- Production Flight cancellation source = NONE


66. Validation

Run:

dotnet build TravelCore.sln

Flight.UnitTests
Payment.UnitTests
Booking.UnitTests
HotelBooking.UnitTests
ArchitectureTests
Persistence.IntegrationTests
Host.IntegrationTests

git diff --check

Frontend expected untouched.


67. Required result evidence

Report:

- Flight Unit test count
- Payment Unit test count
- Booking Unit test count
- HotelBooking Unit test count
- Architecture test count
- Persistence test count
- Host test count
- FlightBookingCancellation type
- cancellation status exact values
- cancellation source port
- supplier capabilities added
- Penalty=0 result
- Penalty=Total result
- partial penalty result
- partial penalty supplier call count
- Partial Refund implemented: NO
- FlightTicketStatus exact values
- supplier reversal attempt status values
- timeout behavior
- unresolved retry behavior
- partial ticket reversal behavior
- authoritative complete reversal result
- FlightBooking status before/after reversal
- full-refund event
- Payment Refund amount authority
- one Refund behavior
- PaymentStatus after Refund
- generic Cancel: NO
- per-passenger cancellation: NO
- partial itinerary cancellation: NO
- amendments/rebooking: NO
- public API/UI: NO
- Named Flight Supplier: NONE
- Production cancellation source: NONE
- real supplier/provider SDK: NO
- peer-schema FK: NO
- shared DbContext: NO
- distributed transaction: NO
- P22-R7 = RESOLVED
- P22-R8 = OPEN
- TC-P22-T008 = NOT EXECUTED


68. Commit/push

After PASS:

- commit with TC-P22-T007
- push fast-forward to origin/main
- re-fetch
- verify HEAD == origin/main
- verify Working Tree CLEAN


69. Auto-Execute

Return TC-P22-T007 RESULT.

Do NOT execute TC-P22-T008.

END_TRAVELCORE_CURSOR_TASK_V1
```
