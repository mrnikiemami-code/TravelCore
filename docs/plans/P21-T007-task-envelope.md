# TC-P21-T007 Task Envelope

Captured live after `TC-P21-T006 = ACCEPTED`. `P21-R7 = RESOLVED`. Baseline `790765b`.

```text
### بررسی TC-P21-T006

```text
TC-P21-T006 = ACCEPTED

Implementation Commit:
f2d4946

Current HEAD:
790765b

HEAD == origin/main
Working Tree:
CLEAN
```

R6 مهم‌ترین اتصال بین HotelBooking و Payment را بدون خراب‌کردن معماری P20 انجام داده است.

مدل Target همچنان بسته و Typed است:

```text
Payment supported targets:

TourBooking
HotelBooking
```

و این مدل عمداً وجود ندارد:

```text
TargetType = arbitrary string
TargetId   = arbitrary Guid
```

قیود Persistence نیز درست‌اند:

```text
ck_payments_exactly_one_target

ux_payments_booking_id

ux_payments_hotel_booking_id
```

ترتیب تراکنشی HotelBooking نیز اکنون روشن شده است:

```text
Active Availability Hold
        ↓
Accepted Rate / Monetary Snapshot
        ↓
Payment
        ↓
Payment Succeeded
        ↓
Final Supplier Reservation
        ↓
HotelBooking Confirmed
```

و مهم‌ترین invariant جدید:

```text
Payment Succeeded
AND
Supplier Reservation Confirmed
=
HotelBooking Confirmed
```

بنابراین:

```text
Payment only  -> Pending
Supplier only -> Pending
Both          -> Confirmed
```

مسیر جبران مالی هم درست است:

```text
Payment Succeeded
+
authoritative inability to complete HotelBooking
=
Full Refund compensation
```

در حالی که حالت مبهم Supplier هنوز Refund ایجاد نمی‌کند:

```text
Supplier timeout / ambiguous
=
Recheck / Reconciliation

NOT automatic Refund
```

Validation:

```text
HotelBooking.UnitTests: 75 PASS
Payment.UnitTests: 89 PASS
Booking.UnitTests: 54 PASS
ArchitectureTests: 303 PASS
Persistence.IntegrationTests: 106 PASS
Host.IntegrationTests: 57 PASS
git diff --check: PASS
```

پس وضعیت P21:

```text
P21-R1 ✅
P21-R2 ✅
P21-R3 ✅
P21-R4 ✅
P21-R5 ✅
P21-R6 ✅

P21-R7 ⏳ Cancellation / amendments / refund policy
P21-R8 ⏳ Public UX / auth / privacy / supplier readiness
```

# قفل P21-R7

```text
P21-R7 = RESOLVED
```

در R7 یک نکته بسیار مهم داریم. Policy از R4 ممکن است سه نتیجه مالی بدهد:

```text
Penalty = 0
-> Full Refund

Penalty = TotalAmount
-> No Refund

0 < Penalty < TotalAmount
-> Partial Refund required
```

اما P20 هنوز:

```text
Partial Refund = DEFERRED
```

پس TravelCore حق ندارد در حالت سوم Supplier Reservation را لغو کند و بعد تازه بفهمد توان اجرای Refund صحیح را ندارد.

بنابراین:

```text
Partial-refund-required cancellation
=
BLOCK BEFORE EXTERNAL CANCELLATION SIDE EFFECT
```

برای دو حالت قابل اجرا، ترتیب لغو Confirmed HotelBooking چنین است:

```text
Cancellation policy evaluation
        ↓
Supplier cancellation
        ↓
Authoritative supplier cancellation confirmed
        ↓
HotelBooking -> Cancelled
        ↓
Full Refund if required
```

در نتیجه:

```text
HotelBookingCancelled
!=
RefundSucceeded
```

اگر Refund طول بکشد، رزرو واقعاً Cancelled است ولی وضعیت مالی Refund هنوز Pending است. این دو حقیقت نباید یکی شوند.

همچنین Timeout هنگام لغو Supplier:

```text
NetworkTimeout
!=
SupplierCancellationConfirmed
```

پس تا زمانی که Supplier cancellation مبهم است:

```text
HotelBooking remains Confirmed
Refund NOT started
```

و Amendments همچنان Deferred می‌مانند.

Task کامل:

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version:
1

Task-ID:
TC-P21-T007

Phase:
P21

Title:
Confirmed HotelBooking cancellation, immutable penalty evaluation, supplier cancellation orchestration, and refund boundary

Baseline:
790765b

Decision:
P21-R7 = RESOLVED

Purpose:
Implement the P21 baseline customer/business cancellation flow for an already
Confirmed HotelBooking.

Cancellation must use the immutable HotelCancellationPolicySnapshot accepted in
P21-R4 and must preserve the distinction:

HotelBooking cancellation
!=
supplier cancellation execution
!=
Refund execution

P21 baseline supports executable confirmed cancellation only when the immutable
policy resolves to one of:

1. Full Refund
   PenaltyAmount = 0

2. No Refund
   PenaltyAmount = HotelBooking total amount

If the immutable cancellation policy requires:

0 < PenaltyAmount < TotalAmount

then a Partial Refund would be required.

Because accepted P20 currently has:

Partial Refund = DEFERRED

that cancellation MUST be rejected before an irreversible supplier cancellation
side effect is started.

Do NOT implement Partial Refund.

Do NOT implement HotelBooking amendments/rebooking.

Do NOT expose the public cancellation UX/API yet; P21-R8 owns the public surface.

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

790765b


2. Record T006 acceptance

Synchronize authoritative SoT to record:

TC-P21-T006 = ACCEPTED

Preserve all accepted P21-R1 through P21-R6 semantics.


3. Preserve lifecycle enums

HotelBookingStatus remains exactly:

- Pending
- Confirmed
- Cancelled


4. Preserve supplier reservation lifecycle

HotelSupplierReservationStatus remains exactly:

- Pending
- Confirmed
- Cancelled


5. Preserve supplier reservation attempt lifecycle

HotelSupplierReservationAttemptStatus remains exactly:

- Created
- Initiated
- Confirmed
- Failed


6. Preserve Payment lifecycle

PaymentStatus remains exactly:

- Pending
- Succeeded


7. Preserve Refund lifecycle

RefundStatus remains exactly:

- Pending
- Succeeded


8. No cancellation lifecycle pollution

Do NOT add to HotelBookingStatus:

- CancellationPending
- Cancelling
- RefundPending
- Refunded
- CancellationFailed
- AmendmentPending


9. Separate cancellation process

Introduce a distinct HotelBooking-owned cancellation process/entity such as:

HotelBookingCancellation

or repository-equivalent.


10. Cancellation process purpose

HotelBookingCancellation tracks orchestration of:

- immutable policy evaluation
- external supplier cancellation
- optional full Refund requirement
- final financial completion evidence

It is NOT HotelBookingStatus.


11. One cancellation process baseline

For one Confirmed HotelBooking baseline:

at most one logical customer/business cancellation process.


12. Cancellation identity

Introduce:

HotelBookingCancellationId

using UUIDv7.


13. Cancellation process status

Use exactly the minimal process states required by the accepted flow.

Preferred baseline:

HotelBookingCancellationStatus:
- Requested
- SupplierCancellationPending
- RefundPending
- Completed

If repository implementation proves a slightly different minimal naming is safer,
keep semantics equivalent and report exact values.

Do NOT add generic workflow/ticket states.


14. Requested semantics

Requested means:

the immutable cancellation policy has been evaluated as executable and a durable
cancellation process exists.


15. SupplierCancellationPending semantics

External supplier cancellation may have been requested and authoritative outcome
is not yet known.


16. RefundPending semantics

Supplier reservation has been authoritatively cancelled and the policy requires a
full Refund that has not yet authoritatively succeeded.


17. Completed semantics

The cancellation business process is complete.

For NoRefund cancellation:

supplier cancellation confirmed
+
HotelBooking cancelled
=
Completed

For FullRefund cancellation:

supplier cancellation confirmed
+
HotelBooking cancelled
+
Refund succeeded
=
Completed


18. No generic Failed cancellation status

Do not treat network/infrastructure ambiguity as business Failed state.


19. Cancellation eligibility

Customer/business confirmed cancellation baseline requires:

- HotelBookingStatus = Confirmed
- HotelSupplierReservationStatus = Confirmed
- immutable HotelCancellationPolicySnapshot exists
- HotelBookingMonetarySnapshot exists
- authoritative HotelBooking Payment evidence exists
- Payment belongs to this HotelBooking
- PaymentStatus = Succeeded
- no existing active/logical cancellation process


20. Why Payment evidence is required

New P21 PayNow confirmed bookings are fully paid.

Cancellation financial outcome must not be inferred without authoritative payment
truth.


21. Legacy T005 confirmed rows

T005 may have produced Confirmed HotelBooking state before R6 dual-evidence payment
semantics existed.

If a Confirmed legacy/test row lacks authoritative Payment evidence:

do NOT guess whether money was collected.

Do NOT initiate supplier cancellation.

Return explicit unsupported/reconciliation-required outcome.


22. Pending HotelBooking customer cancellation

Do NOT generalize R7 into a broad Pending cancellation feature.

R6 already owns system compensation for paid-but-unconfirmed flows.

Customer confirmed cancellation is the R7 baseline.


23. Already Cancelled

Repeated cancellation request is idempotent/safe according to existing logical
cancellation record.

Do not reopen.


24. Cancellation timestamp

Evaluate cancellation terms at an authoritative:

RequestedAt

using:

NodaTime Instant
IClock


25. No server local time

Do NOT use:

DateTime.Now
DateTimeOffset.Now

as cancellation business authority.


26. Cancellation policy authority

Use only the immutable accepted:

HotelCancellationPolicySnapshot


27. No live supplier policy lookup for economics

Do NOT ask a live supplier policy endpoint to replace the accepted penalty terms.


28. No mutable Place policy

Place/catalog changes must not alter cancellation economics.


29. Policy rule evaluation

Determine the exact applicable penalty rule at:

RequestedAt.


30. Deterministic rule

Exactly one applicable authoritative policy outcome must be selected.

Ambiguous/overlapping/no-deterministic-rule result must not initiate supplier
cancellation.


31. Penalty amount

Use the concrete snapshotted:

PenaltyAmount


32. Currency

Penalty CurrencyCode must match:

HotelBookingMonetarySnapshot.CurrencyCode


33. Total amount

Use:

HotelBookingMonetarySnapshot.TotalAmount


34. Full Refund outcome

If:

PenaltyAmount = 0

then:

RefundAmount
=
TotalAmount


35. No Refund outcome

If:

PenaltyAmount = TotalAmount

then:

RefundAmount
=
0


36. Partial penalty outcome

If:

0 < PenaltyAmount < TotalAmount

then:

RefundAmount
=
TotalAmount - PenaltyAmount

which requires Partial Refund execution.


37. Partial Refund hard boundary

Because Partial Refund is not implemented:

partial-penalty customer cancellation is NOT executable in P21 baseline.


38. Critical ordering

For partial-refund-required outcome:

reject BEFORE:

- supplier cancellation network call
- SupplierReservation -> Cancelled
- HotelBooking -> Cancelled
- Payment Refund creation


39. Partial refund result

Return an explicit domain/application outcome such as:

PartialRefundRequiredButUnsupported

or repository-equivalent.


40. No fake full refund

Do NOT convert a partial refund requirement into a full Refund.


41. No fake no-refund

Do NOT convert a partial refund requirement into zero Refund.


42. No silent rounding into boundary outcome

Financial comparison must use accepted Money precision.

Do not accidentally classify partial penalty as zero/full due floating-point
rounding.


43. No float/double

Use accepted Money/numeric semantics.


44. Cancellation financial outcome

A minimal domain value/result may represent:

- FullRefund
- NoRefund
- PartialRefundRequiredUnsupported

Do not make this a HotelBookingStatus.


45. Supplier cancellation authority

External supplier/source remains authoritative for final supplier reservation
cancellation truth.


46. Extend reservation source port

Extend:

IHotelReservationSource

minimally with provider-neutral cancellation operations.


47. Expected operations

Conceptually:

InitiateCancellationAsync
QueryCancellationStatusAsync

or repository-equivalent.


48. No separate concrete supplier

Named Hotel Supplier remains:

NONE


49. Production reservation source

Remain:

NONE


50. No supplier SDK

Do not add one.


51. Server-controlled source

Supplier source remains server-controlled.


52. No supplier failover

Cancellation must go through the source that owns the existing confirmed
reservation.


53. Cross-source cancellation forbidden

Do NOT cancel Supplier A reservation through Supplier B.


54. Supplier cancellation reference

Use existing:

SourceReservationReference

to correlate cancellation.


55. Cancellation attempt history

Introduce:

HotelSupplierCancellationAttempt

or repository-equivalent.


56. Cancellation attempt identity

Use:

HotelSupplierCancellationAttemptId

UUIDv7.


57. Cancellation attempt lifecycle

Use exactly:

HotelSupplierCancellationAttemptStatus:
- Created
- Initiated
- Confirmed
- Failed


58. Created semantics

Local cancellation attempt exists before definitive external outcome.


59. Initiated semantics

Supplier cancellation request may have been sent and outcome may be unresolved.


60. Confirmed semantics

Authoritative source evidence confirms the external reservation is cancelled.


61. Failed semantics

Authoritative source evidence proves that this attempt did not cancel the
reservation and reservation remains active/uncancelled.


62. Network timeout

Preserve:

NetworkTimeout
!=
HotelSupplierCancellationAttempt.Failed


63. Timeout result

On timeout/unknown:

Attempt remains Initiated
Cancellation remains SupplierCancellationPending
SupplierReservation remains Confirmed
HotelBooking remains Confirmed
Refund is NOT started


64. Unsafe retry

An unresolved Initiated cancellation attempt blocks another supplier cancellation
attempt.


65. Definitive failure retry

After authoritative Failed:

a new explicit retry may be allowed under the same logical cancellation.


66. No retry after supplier cancellation confirmed

Once SupplierReservation is authoritatively Cancelled:

do not call supplier cancellation again.


67. One unresolved cancellation attempt

At most one:

Created / Initiated

cancellation attempt per HotelBookingCancellation.


68. DB-backed uniqueness

Use database constraints/transactions.


69. No process-local correctness

No static lock/SemaphoreSlim/ConcurrentDictionary as cancellation correctness
authority.


70. Supplier cancellation idempotency

Use stable idempotency key/correlation where source supports it.


71. Same cancellation idempotency key

Repeated same request:

same effective HotelBookingCancellation / attempt.


72. External exactly-once

Do NOT claim supplier cancellation exactly-once.


73. Recheck

Provide callable authoritative cancellation recheck.


74. Pending/Unknown recheck

Remain unresolved.

No refund.


75. Authoritative Cancelled recheck

Converge to supplier cancellation confirmation safely.


76. Authoritative not-cancelled/failure

Apply Failed only when source semantics prove the reservation remains active.


77. Contradictory evidence

If source later contradicts terminal supplier cancellation truth:

do not silently reopen HotelBooking.

Create reconciliation evidence.


78. Apply authoritative supplier cancellation

On verified supplier cancellation:

HotelSupplierReservation:
Confirmed -> Cancelled


79. HotelBooking cancellation ownership

HotelBooking owns:

Confirmed -> Cancelled


80. Constrained cancellation transition

Introduce only a narrow operation such as:

CancelFromAuthoritativeSupplierCancellation

or equivalent.


81. No generic Cancel

Do NOT expose unrestricted:

Cancel()
SetCancelled()
ForceCancel()


82. Local cancellation transaction

When authoritative supplier cancellation is accepted, atomically persist in
HotelBooking local transaction:

SupplierReservation -> Cancelled

HotelBooking -> Cancelled

HotelBookingCancellation process advancement

and, if FullRefund required:

durable Refund-required outbox event


83. BookingCancelled != RefundSucceeded

Preserve:

HotelBooking may already be Cancelled while Refund is still Pending.


84. Why cancellation precedes Refund

Supplier reservation must no longer be active before TravelCore returns money for
customer-requested confirmed cancellation.


85. Full Refund required event

For PenaltyAmount = 0, emit a typed PII-free event such as:

HotelBookingCancellationRefundRequiredIntegrationEvent


86. Refund event payload

Expected minimal facts:

- HotelBookingCancellationId
- HotelBookingId
- PaymentId
- occurred timestamp

Do NOT use event-supplied amount as financial authority.


87. Cancellation outbox

Persist Refund-required event atomically with accepted HotelBooking/Supplier
cancellation.


88. Crash window

If supplier cancellation local confirmation commits and process crashes:

Refund-required event must survive.


89. Payment consumer

Payment owns the consumer for the cancellation Refund request.


90. Payment authoritative validation

Before creating Refund:

- Payment target = HotelBooking
- Payment belongs to supplied HotelBookingId
- PaymentStatus = Succeeded
- PaymentExecutionSnapshot exists


91. Refund amount authority

Payment calculates accepted P21 full cancellation refund from its own
PaymentExecutionSnapshot.

For the only executable refund case:

full amount.


92. Reuse existing Refund

Use P20 Payment-owned:

Refund


93. One Refund per Payment

Preserve current baseline uniqueness.


94. Duplicate refund-required event

Must converge to one Refund.


95. Payment inbox

Use durable Payment-local cancellation-refund inbox/idempotency.


96. No HotelBooking Refund engine

HotelBooking decides financial business outcome.

Payment executes money movement.


97. No Partial Refund implementation

Hard requirement.


98. No change to RefundStatus

Remain:

Pending
Succeeded


99. No change to RefundAttemptStatus

Remain:

Created
Initiated
Succeeded
Failed


100. Refund external ambiguity

Preserve P20 rules.

Network timeout does not fabricate RefundAttempt Failed.


101. Refund success event

Reuse/evolve existing HotelBooking refund-success integration safely.


102. Cross-purpose handling

HotelBooking must distinguish:

R6 system compensation Refund
vs
R7 customer confirmed-cancellation Refund

without creating a second Refund aggregate.


103. Cancellation refund success correlation

Refund success must correlate to:

HotelBookingCancellationId

or be resolvable safely through HotelBookingId/PaymentId/RefundId.


104. Cancellation Refund success inbox

Use HotelBooking-local durable idempotency.


105. Full-refund completion

If HotelBooking is already Cancelled due authoritative supplier cancellation and
matching Refund succeeds:

HotelBookingCancellation:
RefundPending -> Completed


106. HotelBooking status after Refund

Remains:

Cancelled


107. Supplier reservation after Refund

Remains:

Cancelled


108. Payment after Refund

Preserve:

PaymentStatus = Succeeded


109. Refund success does not change historical Payment truth

Hard requirement.


110. No-refund completion

If penalty equals TotalAmount:

after authoritative supplier cancellation:

HotelBooking -> Cancelled
SupplierReservation -> Cancelled
HotelBookingCancellation -> Completed

No Refund is created.


111. Partial-refund unsupported cancellation

Expected:

HotelBooking remains Confirmed
SupplierReservation remains Confirmed
no supplier cancellation attempt
no Refund
no Payment mutation


112. Customer request race

Two concurrent cancellation requests:

one logical HotelBookingCancellation.


113. Cancellation vs duplicate supplier callback

Idempotent.


114. Cancellation vs supplier-confirmation replay

Already Confirmed reservation replay must not interfere with cancellation process.


115. Cancellation vs Refund event duplicate

Idempotent.


116. Cancellation vs R6 compensation

A HotelBooking already in R6 paid-but-unconfirmed compensation path must not start
R7 confirmed customer cancellation.


117. Confirmed prerequisite

R7 customer cancellation requires:

HotelBookingStatus = Confirmed


118. System compensation distinction

R6 Pending cancellation after failed booking completion remains separate.


119. Payment target isolation

TourBooking Refund/cancellation behavior must remain unchanged.


120. HotelBooking target isolation

Hotel cancellation event cannot refund a TourBooking Payment.


121. Supplier reservation correlation isolation

Cancellation for HotelBooking A cannot cancel HotelBooking B's supplier
reservation.


122. Source reference isolation

External reservation reference must be matched against correct SourceKey.


123. Cancellation terms snapshot immutability

Do not modify accepted cancellation policy when cancellation occurs.


124. Monetary snapshot immutability

Do not rewrite HotelBookingMonetarySnapshot.


125. Rate snapshot immutability

Do not rewrite HotelRateOfferSnapshot.


126. No live repricing

No supplier rate re-query to determine refund economics.


127. Property timezone

Cancellation deadlines were snapshotted as Instant.

Use Instant as authority.

Property timezone remains explanatory context only.


128. Cancellation request after deadline

Apply the applicable snapshotted penalty rule at RequestedAt.

Do not reinterpret based on processing-completion time.


129. Supplier processing delay

If cancellation was requested before a free-cancellation cutoff but supplier
processing completes after cutoff:

the customer financial policy outcome remains based on accepted RequestedAt
according to the immutable TravelCore contract.

Record source contradiction/reconciliation if supplier economics differ.


130. Supplier cancellation fee mismatch

If supplier returns materially different cancellation economics than accepted
snapshot:

do not silently change customer policy.

Record reconciliation evidence.


131. External supplier monetary liability

Do not invent supplier settlement/accounting to solve this mismatch.


132. Cancellation reconciliation kinds

Add only minimal HotelBooking-owned issues needed, such as:

- SupplierCancellationAmbiguous
- SupplierCancellationContradiction
- SupplierCancellationEconomicsMismatch
- MissingPaymentEvidence
- RefundInvariantMismatch

Adjust naming to repository conventions.


133. Reconciliation != status

Do not add these to HotelBookingStatus.


134. Reconciliation != ticket workflow

No CRM/helpdesk system.


135. Amendments

Explicitly:

Date amendment = DEFERRED
Room amendment = DEFERRED
Guest amendment = DEFERRED
Rate-plan amendment = DEFERRED


136. Rebooking

DEFERRED.


137. No-show execution

DEFERRED.


138. Partial Refund dependency

Document explicitly:

Hotel cancellation policies may contain partial penalty facts.

TravelCore cannot execute those customer cancellations until an explicitly
accepted Payment partial-refund capability exists.


139. Do not modify P20 partial-refund decision

Remain:

DEFERRED


140. PayAtProperty

Remain:

DEFERRED


141. Deposit

Remain:

DEFERRED


142. Public API

P21-R8 remains OPEN.

Do NOT expose public cancellation endpoint.


143. Frontend

Do NOT add cancellation UI.


144. Operational query

Internal cancellation process/reconciliation may be queryable for tests/ops later.

No public operational route.


145. Notification

Do not send email/SMS directly.


146. Search

Do not index cancellation transaction state.


147. SEO

No changes.


148. Supplier cancellation callback

If neutral technical callback handling is added, it must require source adapter
verification.

Unverified callback cannot cancel.


149. Callback replay

Repeated authoritative cancellation evidence is idempotent.


150. Browser/client flag

Preserve:

ClientCancellationSuccess
!=
SupplierReservation.Cancelled


151. Architecture guardrails

Add tests proving:

HotelBookingCancellation != HotelBookingStatus

CancellationPolicySnapshot != cancellation execution

HotelBookingCancelled != RefundSucceeded

Partial penalty cancellation blocked before supplier side effect

Penalty=0 -> full Refund path

Penalty=Total -> no Refund path

Supplier cancellation must be authoritative

Network timeout != cancellation Failed

Unresolved cancellation blocks retry

HotelBooking Confirmed -> Cancelled only through constrained authoritative supplier
cancellation path

No generic Cancel

Payment executes Refund

HotelBooking does not write Payment tables

Payment does not write HotelBooking tables

Partial Refund absent

Amendments absent

No public HotelBooking cancellation API/UI

No peer-schema FK

No shared DbContext

No peer Infrastructure dependency

No distributed transaction


152. Unit tests: policy evaluation

Cover:

Penalty = 0
-> FullRefund

Penalty = Total
-> NoRefund

0 < Penalty < Total
-> PartialRefundRequiredButUnsupported


153. Partial penalty side-effect test

For partial penalty:

supplier cancellation gateway call count = 0
Refund creation = 0
HotelBooking remains Confirmed
SupplierReservation remains Confirmed


154. Cancellation eligibility test

Confirmed valid booking accepted.


155. Pending booking cancellation test

Rejected by R7 customer-cancellation service.


156. Cancelled booking duplicate test

Idempotent/no reopen.


157. Missing Payment evidence test

No supplier call.
No cancellation.
Explicit reconciliation/unsupported outcome.


158. Cancellation attempt lifecycle tests

Created -> Initiated
Initiated -> Confirmed
authoritative definitive failure -> Failed
timeout -> stays Initiated
unresolved blocks retry
Failed allows explicit retry


159. Full refund path unit/integration test

Scenario:

Confirmed HotelBooking
Payment Succeeded
SupplierReservation Confirmed
Penalty = 0

customer cancellation request
supplier cancellation authoritatively succeeds
SupplierReservation -> Cancelled
HotelBooking -> Cancelled
Refund-required outbox exists
Payment creates one full Refund
Refund succeeds
HotelBookingCancellation -> Completed
Payment remains Succeeded


160. No-refund path test

Penalty = Total

supplier cancellation succeeds
HotelBooking -> Cancelled
SupplierReservation -> Cancelled
no Refund
Cancellation -> Completed


161. Supplier timeout test

Penalty outcome executable
supplier cancellation times out

Expected:

HotelBooking = Confirmed
SupplierReservation = Confirmed
Cancellation = SupplierCancellationPending
Attempt = Initiated
Refund = none


162. Supplier recheck success

Later authoritative cancellation query returns Cancelled.

Flow converges safely.


163. Supplier definitive failure

Attempt -> Failed

Booking remains Confirmed
Reservation remains Confirmed
no Refund


164. Failed retry

Explicit retry allowed.


165. Duplicate cancellation request

Same idempotency key:

same logical cancellation.


166. Concurrent cancellation requests

At most one logical HotelBookingCancellation.


167. One unresolved cancellation attempt

Database-backed uniqueness.


168. Cancellation outbox atomicity

For full-refund cancellation:

SupplierReservation Cancelled
+
HotelBooking Cancelled
+
Refund-required outbox

must commit atomically.


169. Cancellation rollback test

No partial local cancellation without durable Refund trigger.


170. Payment cancellation-refund inbox

Duplicate event creates one Refund.


171. Refund amount test

Full refund equals PaymentExecutionSnapshot amount.


172. Refund currency test

Equals PaymentExecutionSnapshot CurrencyCode.


173. Refund success completion

Cancellation process completes once.


174. Duplicate RefundSucceeded delivery

Idempotent.


175. Payment remains Succeeded

After Refund.


176. Cross-target isolation tests

Hotel cancellation cannot affect TourBooking Payment/Refund.


177. Cross-booking supplier cancellation test

Cancellation evidence for HotelBooking A cannot cancel B.


178. Callback verification test

Unverified supplier cancellation callback cannot mutate state.


179. Persistence tables

Create minimum R7 persistence conceptually:

hotel_booking.hotel_booking_cancellations

hotel_booking.hotel_supplier_cancellation_attempts

hotel_booking.hotel_booking_cancellation_idempotency

HotelBooking-local inbox/outbox/reconciliation additions as needed.

Use repository conventions.


180. Same-schema FK

Allowed only inside:

hotel_booking


181. Payment schema additions

Allowed:

minimal Payment cancellation-compensation inbox/correlation required to consume
HotelBooking full-refund event.

No HotelBooking FK.


182. No cross-schema FK

Hard requirement.


183. No shared DbContext

Hard requirement.


184. No peer Infrastructure dependency

Hard requirement.


185. No distributed transaction

Hard requirement.


186. No process-local correctness authority

Hard requirement.


187. Migration safety

Existing Confirmed HotelBookings remain unchanged.

Do not automatically create cancellation processes.


188. No backfilled refund assumptions

Do not infer payment/refund state for legacy rows.


189. Host

Production can still run with:

Payment Provider = NONE
Hotel Supplier = NONE


190. No fake production provider/supplier

Hard requirement.


191. Existing Tour Booking regression

Run Payment and Booking tests.


192. Existing Hotel R6 regression

Re-run dual-evidence and compensation tests.


193. Source-of-Truth synchronization

Update authoritative P21 docs to record:

TC-P21-T006 = ACCEPTED

P21-R7 = RESOLVED


194. R7 decision summary

Record exactly:

- P21 customer cancellation baseline targets Confirmed HotelBooking
- HotelBookingCancellation is separate process state
- cancellation economics come from immutable HotelCancellationPolicySnapshot
- RequestedAt Instant selects the applicable accepted penalty rule
- PenaltyAmount = 0 => full Refund
- PenaltyAmount = TotalAmount => no Refund
- partial penalty requires Partial Refund and is not executable in P21 baseline
- partial-refund-required cancellation is rejected before supplier cancellation
- supplier cancellation authority belongs to authoritative reservation source
- supplier cancellation attempts are durable/idempotent/ambiguity-aware
- network timeout does not mean cancellation failure or success
- HotelBooking remains Confirmed until supplier cancellation is authoritative
- authoritative supplier cancellation performs Confirmed -> Cancelled
- full Refund is requested durably only after authoritative supplier cancellation
- HotelBookingCancelled != RefundSucceeded
- Payment owns Refund execution
- Payment remains Succeeded after Refund
- no-refund cancellation completes without Refund
- full-refund cancellation completes financially after RefundSucceeded
- Partial Refund remains DEFERRED
- amendments/rebooking remain DEFERRED
- PayAtProperty/deposit remain DEFERRED
- no distributed transaction
- Named Hotel Supplier = NONE
- Production Payment Provider = NONE


195. Decision status

Record:

P21-R7 = RESOLVED


196. Remaining decision

Keep:

P21-R8 = OPEN


197. P21 state

Remain:

IN_PROGRESS


198. T008

Do NOT execute:

TC-P21-T008


Allowed:

- HotelBookingCancellation
- HotelBookingCancellationId
- minimal cancellation process status
- immutable cancellation-policy evaluation
- supplier cancellation port operations
- HotelSupplierCancellationAttempt
- cancellation attempt statuses
- DB-backed idempotency/concurrency
- authoritative supplier cancellation
- constrained Confirmed -> Cancelled transition
- full-refund-required durable outbox
- Payment full-Refund consumer/inbox
- HotelBooking Refund-success completion correlation
- cancellation reconciliation evidence
- migrations
- unit/architecture/persistence/host regression tests
- SoT synchronization for R7


Forbidden:

- P21-R8 decision
- Partial Refund
- arbitrary Refund amount
- penalty-based partial Refund
- cancelling partial-penalty booking anyway
- generic HotelBooking Cancel()
- Pending customer cancellation expansion
- date amendment
- room amendment
- guest amendment
- rate amendment
- rebooking
- no-show execution
- PayAtProperty
- deposit
- installments
- PaymentStatus changes
- RefundStatus changes
- real supplier
- real Payment provider
- supplier SDK
- Payment SDK
- supplier failover
- public HotelBooking cancellation API
- frontend cancellation UX
- Search/SEO changes
- accounting
- settlement
- shared DbContext
- peer-schema FK
- peer Infrastructure dependency
- distributed transaction
- unrelated refactor
- dependency upgrades


Done:

- Confirmed HotelBooking has a safe cancellation process
- accepted immutable policy determines financial outcome
- free-cancellation produces full Refund requirement
- non-refundable cancellation produces no Refund
- partial penalty cancellation is blocked before irreversible side effect
- supplier cancellation is authoritative and ambiguity-safe
- duplicate cancellation cannot double-cancel supplier reservation
- HotelBooking only becomes Cancelled after authoritative supplier cancellation
- full Refund starts only after supplier reservation is cancelled
- Refund remains Payment-owned
- Payment remains Succeeded after Refund
- Refund success completes financial cancellation processing
- partial Refund remains absent
- amendments remain absent
- no public cancellation surface exists
- P21-R7 recorded RESOLVED
- P21-R8 remains OPEN


Validation:

Run:

dotnet build TravelCore.sln

HotelBooking.UnitTests

Payment.UnitTests

Booking.UnitTests

ArchitectureTests

Persistence.IntegrationTests

Host.IntegrationTests

frontend validation only if frontend files are touched

git diff --check


Required Result Evidence:

Report exact:

- HotelBooking Unit test count
- Payment Unit test count
- Booking Unit test count
- Architecture test count
- Persistence Integration test count
- Host Integration test count
- frontend touched YES/NO
- git diff --check

Also report:

- HotelBookingCancellation type
- HotelBookingCancellationStatus exact values
- cancellation target baseline
- cancellation policy source
- cancellation evaluation timestamp type
- Penalty=0 result
- Penalty=Total result
- partial penalty result
- partial penalty supplier-call count/result
- Partial Refund implemented: NO
- HotelSupplierCancellationAttempt type
- HotelSupplierCancellationAttemptStatus exact values
- supplier cancellation source port
- Named Hotel Supplier
- Production Hotel Reservation Source
- cancellation timeout behavior
- unresolved cancellation retry behavior
- failed cancellation retry behavior
- same idempotency-key behavior
- concurrent cancellation result
- authoritative supplier cancellation result
- HotelBooking status before authoritative cancellation
- HotelBooking status after authoritative cancellation
- SupplierReservation status after authoritative cancellation
- generic Cancel surface: NO
- full-refund event contract
- Hotel cancellation outbox
- Payment full-refund consumer/inbox
- Refund amount authority
- one Refund result
- RefundSucceeded correlation/inbox
- full-refund final cancellation process result
- no-refund final result
- PaymentStatus after Refund success
- confirmed cancellation supported: YES, through constrained R7 path
- Pending customer cancellation added: NO
- amendments implemented: NO
- PayAtProperty: DEFERRED
- deposit/partial collection: DEFERRED
- public cancellation API/UI: NO
- peer-schema FK: NO
- shared DbContext: NO
- peer Infrastructure dependency: NO
- distributed transaction: NO
- Production Payment Provider: NONE
- real supplier/provider SDK: NO
- P21-R7: RESOLVED
- P21-R8: OPEN
- TC-P21-T008: NOT EXECUTED


Repository safety:

- discover repository root using:
  git rev-parse --show-toplevel
- git fetch origin
- require branch main
- require HEAD == origin/main
- require clean working tree before work

Forbidden repository operations:

- force push
- accepted-history rewrite
- reset discarding accepted work
- duplicate cherry-picks


Commit:

After successful validation:

- commit with TC-P21-T007 in commit message
- push main to origin/main using normal fast-forward push
- re-fetch origin
- verify HEAD == origin/main
- verify Working Tree CLEAN


Expected Baseline:
790765b


Auto-Execute:

After PASS:

- return TC-P21-T007 RESULT to architect
- do NOT execute TC-P21-T008 until T007 is architect ACCEPTED
- do NOT invent P21-R8
- remain in PIPELINE


END_TRAVELCORE_CURSOR_TASK_V1
```
``` 

