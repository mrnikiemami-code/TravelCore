# TC-P21-T006 Task Envelope

Captured live after `TC-P21-T005 = ACCEPTED`. `P21-R6 = RESOLVED`. Baseline `53e6e14`.

```text
### بررسی TC-P21-T005

```text
TC-P21-T005 = ACCEPTED

Implementation Commit:
8cc1b28

Current HEAD:
53e6e14

HEAD == origin/main
Working Tree:
CLEAN
```

R5 دقیقاً مرز درست را ساخته است:

```text
HotelBooking
!=
HotelSupplierReservation
!=
HotelSupplierReservationAttempt
```

Lifecycleها نیز تمیز از هم جدا هستند:

```text
HotelBookingStatus:
- Pending
- Confirmed
- Cancelled

HotelSupplierReservationStatus:
- Pending
- Confirmed
- Cancelled

HotelSupplierReservationAttemptStatus:
- Created
- Initiated
- Confirmed
- Failed
```

و رفتار مبهم شبکه درست باقی مانده:

```text
NetworkTimeout
!=
SupplierReservationAttempt.Failed
```

بنابراین در Timeout:

```text
Attempt = Initiated
Reservation = Pending
HotelBooking = Pending

Unsafe retry = BLOCKED
```

همچنین Confirmation فقط از مسیر محدودشده انجام می‌شود:

```text
ConfirmFromAuthoritativeSupplierReservation
```

و مسیر عمومی خطرناک نداریم:

```text
Confirm() = NO
SetConfirmed() = NO
ForceConfirm = NO
```

Validation:

```text
HotelBooking.UnitTests: 60 PASS
ArchitectureTests: 298 PASS
Persistence.IntegrationTests: 96 PASS
Host.IntegrationTests: 57 PASS
git diff --check: PASS
```

## وضعیت P21

```text
P21-R1 ✅ Module / schema / Place ownership
P21-R2 ✅ Stay / multi-room / guests
P21-R3 ✅ Availability / hold
P21-R4 ✅ Rate / monetary / cancellation snapshot
P21-R5 ✅ Supplier reservation / lifecycle / reconciliation

P21-R6 ⏳ Payment integration / compensation
P21-R7 ⏳ Cancellation / amendments / partial-refund policy
P21-R8 ⏳ Public UX / authorization / privacy / supplier readiness
```

# قفل P21-R6

```text
P21-R6 = RESOLVED
```

تصمیم اصلی این است که Payment را به یک Target آزاد و رشته‌ای تبدیل نمی‌کنیم.

یعنی این ممنوع می‌ماند:

```text
TargetType = "anything"
TargetId   = arbitrary Guid
```

Payment فقط Targetهای **صریح و شناخته‌شده** دارد:

```text
TourBooking
HotelBooking
```

و API/Contract هرکدام Typed باقی می‌ماند.

برای HotelBooking نیز Baseline پرداخت را این‌طور قفل می‌کنیم:

```text
Full TravelCore-collected PayNow
```

و فعلاً:

```text
PayAtProperty = DEFERRED
Deposit / Partial Payment = DEFERRED
Split Payment = DEFERRED
```

ترتیب امن Baseline:

```text
Active HotelAvailabilityHold
        |
        v
Accepted Rate / Monetary Snapshot
        |
        v
Payment
        |
        v
Payment Succeeded
        |
        v
Final Supplier Reservation
        |
        v
HotelBooking Confirmed
```

این ترتیب عمداً انتخاب می‌شود، چون اگر Supplier را اول Confirm کنیم ولی Payment بعداً شکست بخورد، برای آزادکردن رزرو Supplier به Cancellation واقعی نیاز پیدا می‌کنیم که هنوز R7 است.

در عوض حالت اصلی Failure ما می‌شود:

```text
Payment Succeeded
+
Supplier Reservation cannot complete
=
Full Refund Compensation
```

که P20 از قبل برایش مکانیزم امن Full Refund دارد.

یک تغییر مهم دیگر:

بعد از R6 دیگر صرفاً این کافی نیست:

```text
SupplierReservation = Confirmed
```

برای HotelBookingهایی که Payment-required هستند.

Confirmation نهایی باید هر دو Evidence را داشته باشد:

```text
Authoritative Payment Succeeded
+
Authoritative Supplier Reservation Confirmed
=
HotelBooking Confirmed
```

و هر دو به‌صورت Durable و Idempotent به هم می‌رسند؛ بدون Distributed Transaction.

Task کامل و یکپارچه:

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version:
1

Task-ID:
TC-P21-T006

Phase:
P21

Title:
HotelBooking typed Payment integration, pay-first orchestration, dual-evidence confirmation, and full-refund compensation

Baseline:
53e6e14

Decision:
P21-R6 = RESOLVED

Purpose:
Integrate HotelBooking with the accepted P20 Payment module without turning
Payment into an unrestricted generic TargetType/TargetId platform.

P21 baseline uses a full TravelCore-collected PayNow flow:

Hotel availability hold
->
accepted hotel monetary snapshot
->
Payment
->
authoritative Payment success
->
final supplier reservation
->
HotelBooking confirmation

HotelBooking confirmation for the P21 PayNow baseline requires BOTH:

1. authoritative successful Payment evidence
2. authoritative confirmed supplier reservation evidence

If Payment succeeds but HotelBooking can no longer be authoritatively confirmed,
the accepted financial compensation is a full Refund through the existing
Payment-owned Refund mechanism.

Do NOT implement:
- PayAtProperty
- deposit/partial collection
- partial Refund
- customer cancellation
- confirmed HotelBooking cancellation
- amendments
- a real payment provider
- a real hotel supplier
- public HotelBooking UX

Required:

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

53e6e14


2. Record T005 acceptance

Synchronize SoT to record:

TC-P21-T005 = ACCEPTED

Preserve all accepted P21-R1 through P21-R5 behavior.


3. Preserve P20

Do NOT redesign accepted P20.

Preserve:

PaymentStatus:
- Pending
- Succeeded

PaymentAttemptStatus:
- Created
- Initiated
- Succeeded
- Failed

RefundStatus:
- Pending
- Succeeded

RefundAttemptStatus:
- Created
- Initiated
- Succeeded
- Failed


4. Payment target evolution

P20 initially supported Tour Booking.

P21 now introduces a second explicitly supported Payment target:

HotelBooking.


5. No unrestricted generic target

Do NOT introduce an open-ended model such as:

string TargetType
Guid TargetId

with arbitrary values.


6. Closed supported-target model

Use a strongly controlled representation for exactly the supported targets.

Conceptually:

PaymentTargetKind:
- TourBooking
- HotelBooking

or a typed union/reference equivalent.


7. Existing Tour Booking behavior

All accepted Tour Booking Payment behavior must remain unchanged.


8. No Tour Booking migration breakage

Existing Payment rows and tests must retain their original semantic target.


9. Target-specific uniqueness

Preserve:

one Tour Booking
->
one logical Payment

and add:

one HotelBooking
->
one logical Payment


10. Database-backed uniqueness

Uniqueness must be PostgreSQL-backed.


11. Target identity safety

A Payment must belong to exactly one supported target.


12. No dual-target Payment

Forbidden:

same Payment
linked simultaneously to
TourBooking and HotelBooking.


13. Target reference != credential

HotelBookingId is not a Payment authorization credential.


14. No peer-schema FK

Payment may store logical HotelBooking reference only.

No FK from:

payment
to
hotel_booking.


15. Typed contracts

Use explicit typed HotelBooking Payment contracts.

Prefer patterns parallel to accepted Tour Booking contracts.


16. Do not make HotelBooking depend on Payment.Infrastructure

Allowed:

HotelBooking -> Payment.Contracts

Forbidden:

HotelBooking.Infrastructure -> Payment.Infrastructure


17. No reverse Infrastructure dependency

Payment.Infrastructure must not depend on HotelBooking.Infrastructure.


18. Contract composition

Use Contracts/application composition through DI.


19. Hotel payment obligation

Introduce an authoritative HotelBooking payment obligation contract.


20. Obligation authority

The obligation must come from:

HotelBookingMonetarySnapshot


21. No live rate lookup

Payment must NOT call:

IHotelRateOfferSource

to calculate payment amount.


22. No live supplier price

Payment must not query current supplier rate to construct payment amount.


23. No Pricing generalization

Do not modify Pricing module.


24. Full PayNow baseline

For P21 baseline:

HotelBooking Payment obligation
=
full HotelBookingMonetarySnapshot.TotalAmount


25. Currency

Payment obligation CurrencyCode
=
HotelBookingMonetarySnapshot.CurrencyCode


26. PayAtProperty

Remain:

DEFERRED


27. Deposit payment

Remain:

DEFERRED


28. Partial collection

Remain:

DEFERRED


29. Installments

Remain:

DEFERRED


30. Mixed settlement

Remain:

DEFERRED


31. Monetary snapshot prerequisite

HotelBooking cannot prepare Payment without an accepted:

HotelBookingMonetarySnapshot.


32. Accepted rate prerequisite

HotelRateOfferSnapshot must exist and remain the accepted transaction snapshot.


33. PaymentExecutionSnapshot

Reuse accepted Payment-owned immutable:

PaymentExecutionSnapshot.


34. Hotel obligation copy

Payment copies the HotelBooking obligation into its own immutable execution
snapshot exactly as it already does for the accepted Tour Booking flow.


35. Same obligation idempotency

Repeated preparation with same HotelBooking obligation:

idempotent.


36. Different obligation overwrite

Attempt to prepare an existing Payment with a materially different HotelBooking
amount/currency:

rejected.


37. No silent repricing

Do not replace PaymentExecutionSnapshot because supplier currently returns a
different rate.


38. Client amount authority

None.


39. Client currency authority

None.


40. Toman

Preserve:

Toman != CurrencyCode


41. Pay-first orchestration

P21 baseline ordering is:

Active unexpired HotelAvailabilityHold
+
accepted HotelBookingMonetarySnapshot
->
Payment preparation/initiation
->
Payment success
->
final HotelSupplierReservation initiation


42. Why pay-first

Do not create a supplier-confirmed hotel reservation first and then require Payment
in the baseline flow.

This avoids creating an unpaid confirmed supplier reservation that would require
R7 cancellation semantics.


43. Payment eligibility

HotelBooking Payment initiation requires at minimum:

- HotelBooking = Pending
- accepted HotelRateOfferSnapshot
- HotelBookingMonetarySnapshot
- Active unexpired HotelAvailabilityHold
- no HotelBooking confirmation
- no Cancelled HotelBooking


44. Requested hold

Requested/unresolved Hold:

Payment initiation prohibited.


45. Released hold

Payment initiation prohibited.


46. Expired hold

Payment initiation prohibited.


47. Confirmed HotelBooking

No new Payment initiation.


48. Cancelled HotelBooking

No new Payment initiation.


49. Existing successful Payment

No second Payment/charge.


50. Existing unresolved PaymentAttempt

Unsafe retry remains blocked per P20.


51. Definitively Failed PaymentAttempt

Explicit retry remains allowed under P20 rules.


52. Payment provider

Named Production Payment Provider remains:

NONE


53. Real Payment SDK

Do NOT add one.


54. No-provider behavior

HotelBooking Payment initiation must safely report provider unavailable when no
production provider is configured.


55. Payment target routing

Provider sees Payment transaction facts.

Do not expose HotelBooking internals unnecessarily.


56. Payment success durability

Preserve P20 invariant:

Payment Succeeded
+
durable Payment outbox event

same Payment-local transaction.


57. Hotel-specific success trigger

Introduce a typed HotelBooking Payment-success integration contract or evolve the
existing event safely so HotelBooking can receive Payment success without
breaking Tour Booking.


58. Preferred compatibility posture

Do NOT make existing Booking consumer infer HotelBooking from arbitrary TargetId.


59. HotelBooking payment success inbox

Add durable HotelBooking-local inbox/idempotency for Payment success.


60. Payment success event != HotelBooking confirmation

Preserve:

PaymentSucceeded
!=
HotelBookingConfirmed


61. Payment event is trigger

HotelBooking consumer must re-read authoritative Payment success evidence through
Payment.Contracts.


62. Payment evidence query

Introduce/extend a typed authoritative query for:

HotelBookingId
->
successful Payment evidence


63. HotelBooking never trusts event payload alone

Event is notification.

Payment query is authoritative verification.


64. HotelBooking Payment evidence

Persist minimal local durable evidence that Payment has authoritatively succeeded
for this HotelBooking.


65. Evidence persistence

Use HotelBooking-owned record/inbox such as:

HotelBookingPaymentEvidence

or minimal equivalent.


66. Evidence facts

Expected:

- HotelBookingId
- PaymentId
- amount
- CurrencyCode
- verified/received timestamp

No provider secret.


67. Payment evidence amount match

Payment amount must equal:

HotelBookingMonetarySnapshot.TotalAmount


68. Payment evidence currency match

Must equal snapshot CurrencyCode.


69. Payment mismatch

If Payment says Succeeded but amount/currency does not match HotelBooking snapshot:

do NOT confirm HotelBooking.

Persist reconciliation/recovery evidence.


70. Existing R5 supplier confirmation

R5 currently permits authoritative supplier reservation confirmation.


71. Strengthen final HotelBooking confirmation

For P21 PayNow HotelBooking after R6:

HotelBooking may transition:

Pending -> Confirmed

only when BOTH:

- authoritative Payment success evidence exists
- HotelSupplierReservation = Confirmed


72. Dual-evidence rule

Implement explicitly:

Payment Succeeded
AND
Supplier Reservation Confirmed
=
eligible for HotelBooking confirmation


73. Supplier-only confirmation

After R6:

SupplierReservation Confirmed alone
must NOT confirm PayNow HotelBooking.


74. Payment-only confirmation

Payment Succeeded alone
must NOT confirm HotelBooking.


75. Confirmation owner

HotelBooking remains owner of:

Pending -> Confirmed


76. No Payment write into HotelBooking

Payment module must not mutate HotelBooking tables.


77. No supplier source write into HotelBooking directly

Supplier adapter returns authoritative evidence through HotelBooking application
boundary.


78. No generic Confirm

Preserve absence of:

Confirm()
SetConfirmed()
ForceConfirm()


79. Constrained confirmation

Use a constrained operation such as:

TryConfirmFromAuthoritativePaymentAndSupplierEvidence

or equivalent.


80. Confirmation transaction

Within HotelBooking local transaction, verify:

- HotelBooking Pending
- Payment evidence matches immutable monetary snapshot
- SupplierReservation Confirmed
- supplier reservation matches hotel/stay/room set
- no blocking reconciliation issue
- no Cancelled state

then:

Pending -> Confirmed


81. Arrival ordering

The two authoritative evidences may arrive in either order.


82. Payment first

If Payment succeeds first:

store evidence
HotelBooking remains Pending
then initiate/continue final supplier reservation.


83. Supplier confirmation later

When authoritative supplier confirmation arrives:

recheck Payment evidence
then confirm atomically.


84. Supplier evidence first

For compatibility/recovery, if supplier confirmation is already persisted before
Payment evidence arrives:

HotelBooking stays Pending after R6 unless valid Payment evidence exists.


85. Payment evidence later

Payment consumer rechecks supplier reservation.

If both valid:

confirm once.


86. Duplicate Payment success

Idempotent.


87. Duplicate supplier confirmation

Idempotent.


88. Concurrent dual evidence

Payment-success handler and supplier-confirmation handler may race.

Expected:

HotelBooking confirms at most once.


89. DB-backed concurrency

Use HotelBooking DB transaction/locking/versioning.

No process-local correctness authority.


90. Supplier reservation initiation after Payment

Introduce orchestration so an authoritative Payment success can trigger/enable final
supplier reservation initiation.


91. No synchronous distributed chain

Provider callback must not synchronously require the complete Hotel supplier
reservation to finish before returning.


92. Durable orchestration

Payment success must durably trigger HotelBooking continuation.


93. HotelBooking-local outbox

If Payment success requires later supplier reservation processing, persist a
HotelBooking-local durable intent/event in the same local transaction as accepted
Payment evidence.


94. Example intent

Use repository-consistent naming such as:

HotelSupplierReservationRequiredIntegrationEvent

or an internal durable work item.


95. Do not create generic workflow engine

Keep P21-specific orchestration.


96. At-least-once

Cross-module delivery remains:

at-least-once


97. Local effects

Idempotent/effectively-once.


98. No distributed exactly-once claim

Hard requirement.


99. Supplier initiation eligibility after payment

Before calling supplier final reservation source, re-check:

- HotelBooking Pending
- Payment success evidence authoritative
- Active unexpired Hold
- accepted RateOfferSnapshot
- MonetarySnapshot
- no Confirmed reservation
- no unresolved supplier Attempt


100. Hold expires after Payment success

Critical scenario:

Payment = Succeeded
Hold expires before supplier reservation can be safely completed.


101. Hold-expired result

Do NOT confirm HotelBooking.

Create HotelBooking-owned financial recovery evidence.


102. Released hold after Payment

Same principle.


103. Supplier final reservation definitive failure

If Payment succeeded and supplier definitively proves reservation was not created,
HotelBooking cannot silently retain customer funds.


104. Retry before compensation

If a definitive Failed supplier attempt can safely be retried while:

- same Active Hold remains valid
- same immutable rate/terms remain valid

a safe explicit/system retry under the same SupplierReservation may occur.


105. No infinite retry policy

Do not invent retry count/scheduler policy.


106. Compensation trigger

When HotelBooking can authoritatively determine that it cannot complete after
successful Payment, create a durable:

HotelBookingPaymentCompensationRequired

business event/evidence.


107. Compensation != transient technical failure

Do NOT emit compensation merely because:

- handler crashed
- DB temporarily unavailable
- supplier timeout is ambiguous
- message delivery delayed


108. Authoritative compensation reasons

Create minimal HotelBooking-owned recovery reasons required by this flow.

Expected conceptual reasons may include:

- HoldExpired
- HoldReleased
- SupplierReservationNotCreated
- SupplierReservationCancelled
- MonetaryMismatch
- CurrencyMismatch
- RoomSetMismatch
- StayMismatch
- HotelMismatch
- CancellationTermsMismatch

Use exact minimal set justified by code.


109. Ambiguous supplier outcome

Do NOT automatically refund while supplier reservation outcome remains ambiguous.

That could create:

active hotel reservation
+
customer refunded

without knowing supplier truth.


110. Ambiguous reservation

Remain reconciliation-required.

Recheck authoritative supplier source first.


111. Full compensation

For accepted R6 failure path:

RefundAmount
=
full PaymentExecutionSnapshot / HotelBooking payment amount.


112. Partial compensation

Do NOT implement.


113. Cancellation penalty

Do NOT apply cancellation penalty in R6 compensation.

This is booking-not-confirmed compensation, not customer cancellation.


114. Existing P20 Refund

Reuse Payment-owned Refund.


115. No HotelBooking refund engine

HotelBooking decides:

full compensation required

Payment owns:

Refund execution.


116. Hotel compensation event

Introduce typed event/contract such as:

HotelBookingPaymentCompensationRequiredIntegrationEvent


117. Compensation payload

Keep minimal and PII-free:

- HotelBookingId
- PaymentId
- reason
- occurred timestamp

Do not send refund amount as authority.


118. Payment compensation consumer

Payment consumes the HotelBooking compensation event.


119. Payment validates itself

Before Refund creation:

- Payment belongs to HotelBooking
- PaymentStatus = Succeeded
- PaymentExecutionSnapshot exists


120. Refund amount authority

Payment uses its own:

PaymentExecutionSnapshot


121. One Refund

Preserve P20:

one logical full Refund per Payment baseline.


122. Compensation idempotency

Repeated HotelBooking compensation delivery:

one Refund.


123. Payment compensation inbox

Use durable Payment-local inbox for HotelBooking compensation events.


124. Refund initiation

Reuse P20 provider-neutral Refund flow.


125. No production refund provider

Still none if no Payment provider configured.


126. Refund success

Preserve:

RefundSucceeded
does NOT erase
PaymentStatus.Succeeded


127. Hotel-specific Refund success trigger

HotelBooking must receive authoritative Refund-success evidence for its Payment.


128. Existing Tour Booking flow

Do not break existing Booking Refund-success consumer.


129. HotelBooking refund-success inbox

Create durable HotelBooking-local inbox.


130. Refund event is trigger

HotelBooking should re-read authoritative Payment/Refund evidence if required.


131. Refund success finalization

For this compensation path, once authoritative full Refund succeeds:

HotelBooking may transition:

Pending -> Cancelled


132. Compensation cancellation

This is a system compensation terminalization.

It is NOT the R7 customer cancellation policy.


133. Constrained cancellation method

Use a narrowly scoped operation such as:

CancelFromAuthoritativePaymentCompensation

or equivalent.


134. No generic Cancel

Do NOT expose unrestricted:

Cancel()
SetCancelled()
ForceCancel()


135. Confirmed HotelBooking guardrail

Compensation Refund-success handler must NOT perform:

Confirmed -> Cancelled


136. Confirmed cancellation

Remain:

P21-R7 / DEFERRED


137. Supplier reservation confirmed contradiction

If a full compensation Refund succeeds but SupplierReservation is already
Confirmed unexpectedly:

do not automatically cancel supplier reservation or HotelBooking.

Create reconciliation evidence.


138. Hold release after compensation

For a Pending HotelBooking with an Active availability Hold when Refund succeeds,
attempt/reconcile authoritative hold release according to R3 semantics.


139. Do not fake release

A network timeout during Hold release does not prove Released.


140. HotelBooking cancellation finalization

Do not mark compensation flow complete based on fake hold release truth.

Keep reconciliation evidence if release remains ambiguous.


141. Supplier final reservation not created

If authoritative supplier evidence proves no reservation exists and full Refund
succeeds:

Pending HotelBooking may become Cancelled.


142. Supplier Reservation Cancelled

If source authoritatively reports final reservation Cancelled before HotelBooking
confirmation and Payment had succeeded:

full compensation required.


143. Payment success + reservation mismatch

Do not confirm.

Use recovery/compensation only when supplier outcome is authoritative and safe to
refund.


144. Payment success + ambiguous supplier timeout

No refund yet.

Reconciliation/recheck.


145. Payment success + partial room reservation

No HotelBooking confirmation.

If authoritative state proves partial external reservation exists, do NOT blindly
refund until external partial reservation compensation/release is resolved or
explicit reconciliation records the risk.

Avoid creating free active supplier inventory leakage.


146. R6 recovery safety

Compensation must not worsen external reservation uncertainty.


147. Payment-before-supplier failure matrix

Implement/document:

A. Payment Pending / no supplier reservation
   -> HotelBooking Pending

B. Payment Succeeded / supplier not started
   -> HotelBooking Pending; durable continuation

C. Payment Succeeded / supplier Initiated ambiguous
   -> HotelBooking Pending; recheck; no refund yet

D. Payment Succeeded / supplier authoritative Failed/not-created
   -> full compensation required

E. Payment Succeeded / supplier Confirmed
   -> HotelBooking eligible Confirmed

F. Payment Succeeded / Hold Expired before safe supplier booking
   -> full compensation required

G. Payment Succeeded / supplier mismatch
   -> reconciliation; no silent confirmation

H. Refund Succeeded after safe compensation
   -> Pending HotelBooking -> Cancelled


148. Supplier-first legacy/recovery matrix

Because T005 existed before R6, handle safely if repository has:

SupplierReservation = Confirmed
HotelBooking = Pending
Payment not yet Succeeded

Expected after R6:

remain Pending
do NOT create second supplier reservation
await/require Payment according to accepted PayNow flow.


149. Existing already-Confirmed rows

Do NOT corrupt or downgrade any HotelBooking already Confirmed under T005 test/data
history.

Migration/backfill must preserve existing state safely.


150. New confirmation semantics

All new PayNow confirmations after R6 require dual evidence.


151. Migration safety

If adding payment-required/evidence metadata, existing rows must be handled
explicitly and safely.


152. Payment target persistence design

Choose the smallest safe schema evolution.

Allowed examples:

A.
typed target kind + constrained target reference

or

B.
separate nullable typed target columns with exactly-one-target constraint

Choose based on existing Payment persistence.


153. Open string target forbidden

Whatever design is chosen, arbitrary runtime target type strings are forbidden.


154. Target enum exactness

If introducing:

PaymentTargetKind

baseline exact values must be only:

TourBooking
HotelBooking


155. No future speculative target values

Do NOT add:

Order
Flight
Visa
Subscription
Generic


156. Payment uniqueness constraints

Report exact DB constraints protecting both target kinds.


157. Existing BookingId contract compatibility

Avoid unnecessary breaking changes to existing public/internal Tour Booking APIs.


158. Events compatibility

Preserve existing Tour Booking outbox/inbox behavior.


159. HotelBooking Payment event names

Use explicit hotel-specific contracts if that provides safer compatibility.


160. Payment operational read

Extend internal operational read only enough to safely identify HotelBooking target.

No public ops endpoint.


161. No operational mutation

Still none.


162. Payment public API

Do NOT expose HotelBooking public Payment API in T006.

R8 owns HotelBooking public UX/API.


163. Existing Tour public Payment API

Must remain unchanged/regression-tested.


164. No generic Payment by Target API

Do not expose:

POST /api/payment/{targetType}/{targetId}


165. HotelBookingStatus exact values

Remain:

Pending
Confirmed
Cancelled


166. SupplierReservationStatus exact values

Remain:

Pending
Confirmed
Cancelled


167. SupplierReservationAttemptStatus exact values

Remain:

Created
Initiated
Confirmed
Failed


168. HoldStatus exact values

Remain:

Requested
Active
Released
Expired


169. No new lifecycle statuses

Do NOT add:

AwaitingPayment
Paid
Refunding
Failed
Compensating

to HotelBookingStatus.


170. Process state separation

If orchestration state is needed, use dedicated local evidence/work records.

Do not overload business enum.


171. No PaymentStatus changes

Hard requirement.


172. No RefundStatus changes

Hard requirement.


173. Partial Refund conflict

Explicitly preserve:

P20 Partial Refund = DEFERRED


174. R7 dependency

Document:

HotelCancellationPolicySnapshot can contain partial penalty facts,
but executing penalty-based partial customer cancellation cannot be completed until
Payment supports an explicitly accepted partial-refund evolution.


175. Do not solve R7 here

No customer cancellation.


176. No amendment

No date/room/guest amendment.


177. No PayAtProperty

Hard deferred baseline.


178. No deposit

Hard deferred baseline.


179. No card collection

No frontend/payment-card UI.


180. No Search/SEO changes

Hard requirement.


181. No real supplier

Named Hotel Supplier remains:

NONE


182. No real payment provider

Named Production Payment Provider remains:

NONE


183. No supplier SDK

None.


184. No payment provider SDK

None.


185. Architecture guardrails

Add tests proving:

Payment supports only explicitly closed TourBooking + HotelBooking targets

no arbitrary TargetType string target

one HotelBooking -> one Payment

Payment target belongs to exactly one supported target

HotelBooking Payment obligation comes from HotelBookingMonetarySnapshot

Payment does not calculate hotel price

HotelBooking confirmation requires supplier + Payment evidence for PayNow

Payment-only cannot confirm

Supplier-only cannot confirm new PayNow HotelBooking

Payment module does not write HotelBooking tables

HotelBooking does not write Payment tables

full compensation uses Payment Refund

Partial Refund absent

no generic Confirm/Cancel

no public HotelBooking Payment API/UI

no peer-schema FK

no shared DbContext

no peer Infrastructure dependency

no distributed transaction


186. Payment unit tests

Add regression for:

- existing Tour Booking Payment target unchanged
- HotelBooking Payment target creation
- duplicate HotelBooking target -> same Payment
- Tour and Hotel IDs cannot collide semantically
- Payment cannot have both target kinds
- Hotel obligation snapshot accepted
- changed hotel obligation rejected
- Hotel Payment success outbox created once
- Hotel compensation creates one Refund
- Hotel full Refund keeps Payment Succeeded


187. HotelBooking unit tests

Cover:

- valid Payment evidence recorded
- Payment amount mismatch rejected/reconciliation
- currency mismatch rejected/reconciliation
- Payment only -> Pending
- Supplier confirmed only -> Pending
- both authoritative evidences -> Confirmed
- duplicate evidence -> Confirm once
- Cancelled never reopens
- compensation full-refund success can cancel Pending only
- Confirmed not cancelled by compensation


188. Payment eligibility tests

Cover:

Active unexpired hold -> eligible

Requested hold -> ineligible

Released hold -> ineligible

Expired hold -> ineligible

Cancelled HotelBooking -> ineligible

Confirmed HotelBooking -> ineligible


189. Supplier initiation gating tests

Before Payment success:

final supplier initiation prohibited for new PayNow flow.

After Payment success + valid hold/snapshot:

allowed.


190. Payment-success durability test

Payment commits Succeeded
+
Hotel-specific durable success event

atomically.


191. Hotel payment inbox test

Duplicate Payment event:

one local evidence/effect.


192. Delayed Payment delivery

Payment succeeds
HotelBooking consumer delayed
then delivery later:

flow continues safely.


193. Supplier confirmation race test

Payment handler and supplier-confirmation handler race.

Expected:

one HotelBooking confirmation.


194. Payment succeeded / hold expired

Expected:

HotelBooking remains Pending
compensation-required evidence persisted
full Refund obligation created through durable integration.


195. Payment succeeded / hold released

Same compensation posture where authoritative.


196. Payment succeeded / supplier definitive failure

Expected:

HotelBooking not Confirmed
full compensation required.


197. Payment succeeded / supplier timeout

Expected:

no Refund yet
reservation attempt remains unresolved
recheck required.


198. Payment succeeded / supplier monetary mismatch

Expected:

no confirmation
no snapshot rewrite
reconciliation issue
no unsafe automatic refund unless authoritative external reservation state is safe.


199. Compensation outbox atomicity

HotelBooking recovery decision
+
compensation-required outbox

commit atomically.


200. Compensation rollback

Neither partial state nor message commits alone.


201. Payment compensation inbox test

Repeated Hotel compensation message:

one Refund.


202. Refund amount test

Refund amount equals PaymentExecutionSnapshot full amount.


203. Refund currency test

Same currency.


204. Refund success outbox

Preserve Payment-local atomic Refund success + durable event.


205. Hotel refund-success inbox

Durable and idempotent.


206. Refund success finalization

Pending HotelBooking with no active supplier reservation:

-> Cancelled


207. Already Cancelled

Idempotent.


208. Confirmed HotelBooking

Refund success handler:

must not cancel.


209. Unexpected supplier Confirmed

Refund success with SupplierReservation Confirmed:

create reconciliation evidence
do not silently Cancel.


210. Hold release ambiguity

Test timeout during compensation Hold release does not fabricate Released.


211. Cross-target event isolation

Tour Booking Payment event cannot mutate HotelBooking.

HotelBooking Payment event cannot mutate Tour Booking.


212. Cross-target compensation isolation

Same for compensation/Refund events.


213. Persistence tests

Cover:

- Hotel target Payment round-trip
- target exactly-one constraint
- target-specific uniqueness
- Hotel Payment evidence round-trip
- Hotel Payment-success inbox
- Hotel compensation outbox
- Payment Hotel compensation inbox
- Hotel Refund-success inbox
- no peer-schema FK


214. Concurrency: Hotel Payment creation

Two concurrent Payment preparations for same HotelBooking:

one logical Payment.


215. Concurrency: dual evidence confirmation

Concurrent Payment success + supplier confirmation:

one confirmation.


216. Crash window 1

Payment succeeds and commits outbox.
Process stops before HotelBooking receives it.

Expected:
durable recovery after restart.


217. Crash window 2

HotelBooking records compensation-required outbox.
Process stops before Payment consumes.

Expected:
Refund can later be created.


218. Crash window 3

Refund succeeds and commits outbox.
HotelBooking processing delayed.

Expected:
later safe finalization.


219. Existing Tour Booking regression

Run all Payment/Booking P20 tests.

Do not regress:

P20 Tour Booking confirmation
P20 Tour Booking compensation
P20 public Payment authorization


220. No Payment target authorization exposure

Hotel target support must not accidentally make:

PaymentId

a public credential.


221. No HotelBooking public route

T006 exposes no HotelBooking endpoint.


222. Frontend

Untouched.


223. Operational read

If Payment internal read now shows target, use safe fields only:

TargetKind
TargetReferenceId

No guest/contact PII.


224. Source-of-Truth synchronization

Update authoritative P21 docs to record:

TC-P21-T005 = ACCEPTED

P21-R6 = RESOLVED


225. R6 decision summary

Record exactly:

- Payment remains independent Payment module
- Payment now supports two explicitly closed target kinds:
  TourBooking and HotelBooking
- no arbitrary generic TargetType/TargetId target platform
- one HotelBooking -> one logical Payment
- HotelBooking payment amount/currency come from immutable
  HotelBookingMonetarySnapshot
- P21 baseline collection mode = full TravelCore PayNow
- PayAtProperty = DEFERRED
- deposit/partial collection = DEFERRED
- Payment must succeed before new final supplier reservation initiation
- final PayNow HotelBooking confirmation requires BOTH authoritative Payment success
  and authoritative SupplierReservation confirmation
- Payment-only does not confirm
- Supplier-only does not confirm new PayNow HotelBooking
- durable outbox/inbox connects Payment and HotelBooking
- Payment success + authoritative inability to confirm creates full financial
  compensation requirement
- existing Payment-owned Refund executes compensation
- partial Refund remains DEFERRED
- ambiguous supplier reservation outcome is rechecked before Refund
- Refund success may system-cancel only Pending unconfirmed HotelBooking
- Confirmed cancellation remains R7
- no distributed transaction
- no real Payment provider
- no real Hotel supplier


226. Decision status

Record:

P21-R6 = RESOLVED


227. Remaining decisions

Keep:

P21-R7
P21-R8

OPEN


228. P21 status

Remain:

IN_PROGRESS


229. Do not execute T007

TC-P21-T007 = NOT EXECUTED


Allowed:

- explicit closed Payment target evolution for HotelBooking
- HotelBooking payment obligation contract
- PaymentExecutionSnapshot reuse
- Hotel-specific Payment success event/query/inbox
- HotelBooking Payment evidence
- pay-first orchestration
- dual Payment+supplier confirmation guard
- HotelBooking compensation recovery evidence/outbox
- Payment compensation consumer/inbox
- existing full Refund reuse
- HotelBooking Refund-success consumer/inbox
- constrained system compensation cancellation of Pending HotelBooking
- migrations
- unit/architecture/persistence/host regression tests
- SoT synchronization for R6


Forbidden:

- P21-R7/R8 decisions
- arbitrary generic Payment target strings
- speculative Payment targets beyond TourBooking/HotelBooking
- PayAtProperty
- deposit
- partial payment
- installments
- PaymentStatus changes
- RefundStatus changes
- Partial Refund
- customer HotelBooking cancellation
- Confirmed HotelBooking cancellation
- amendments
- supplier smart routing
- real Hotel supplier
- real Payment provider
- supplier SDK
- Payment provider SDK
- public HotelBooking API
- frontend HotelBooking UX
- Search/SEO changes
- shared DbContext
- peer-schema FK
- peer Infrastructure dependency
- distributed transaction
- unrelated refactor
- dependency upgrades


Done:

- HotelBooking can produce an authoritative Payment obligation
- Payment safely supports HotelBooking without becoming an unrestricted generic
  target platform
- Tour Booking Payment behavior remains intact
- one HotelBooking has one logical Payment
- P21 baseline uses full PayNow
- Payment must succeed before new supplier final reservation initiation
- HotelBooking final confirmation requires Payment + supplier evidence
- either evidence alone is insufficient
- both delivery orders are race-safe/idempotent
- Payment success crash window is durable
- successful Payment followed by authoritative booking impossibility produces full
  compensation
- ambiguous supplier state does not trigger unsafe Refund
- existing Payment Refund executes full compensation
- Refund success can terminalize only Pending unconfirmed HotelBooking
- Confirmed cancellation remains absent
- Partial Refund remains absent
- no real provider/supplier exists
- P21-R6 recorded RESOLVED
- P21-R7 and P21-R8 remain OPEN


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

- Payment target representation
- Payment target exact supported values
- arbitrary string TargetType supported: NO
- existing Tour Booking Payment regression: PASS/FAIL
- one HotelBooking -> one Payment constraint
- target exactly-one constraint
- Hotel payment obligation source
- Hotel Payment amount source
- Hotel Payment CurrencyCode source
- PayNow baseline: YES
- PayAtProperty: NO/DEFERRED
- deposit/partial collection: NO
- Payment-before-supplier gating: YES
- Payment-only HotelBooking confirmation result
- Supplier-only HotelBooking confirmation result
- dual-evidence confirmation result
- concurrent dual-evidence result
- Hotel Payment success event contract
- HotelBooking Payment success inbox
- authoritative Payment evidence query
- Payment amount mismatch result
- Payment currency mismatch result
- Payment succeeds / Hold expires result
- Payment succeeds / Hold released result
- Payment succeeds / supplier definitive failure result
- Payment succeeds / supplier timeout result
- Payment succeeds / supplier mismatch result
- compensation event contract
- Hotel compensation outbox
- Payment compensation consumer/inbox
- compensation Refund amount source
- one Refund per Hotel Payment result
- Hotel Refund-success event/contract
- HotelBooking Refund-success inbox
- Pending HotelBooking after Refund success result
- Confirmed HotelBooking after Refund-success handler result
- Partial Refund implemented: NO
- customer HotelBooking cancellation: NO
- Confirmed HotelBooking cancellation: NO
- PaymentStatus exact values
- RefundStatus exact values
- HotelBookingStatus exact values
- SupplierReservationStatus exact values
- SupplierReservationAttemptStatus exact values
- Production Payment Provider: NONE
- Production Hotel Reservation Source: NONE
- real provider/supplier SDK: NO
- public HotelBooking API/UI: NO
- peer-schema FK: NO
- shared DbContext: NO
- peer Infrastructure dependency: NO
- distributed transaction: NO
- P21-R6: RESOLVED
- P21-R7/P21-R8: OPEN
- TC-P21-T007: NOT EXECUTED


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

- commit with TC-P21-T006 in commit message
- push main to origin/main using normal fast-forward push
- re-fetch origin
- verify HEAD == origin/main
- verify Working Tree CLEAN


Expected Baseline:
53e6e14


Auto-Execute:

After PASS:

- return TC-P21-T006 RESULT to architect
- do NOT execute TC-P21-T007 until T006 is architect ACCEPTED
- do NOT invent P21-R7 or P21-R8
- remain in PIPELINE


END_TRAVELCORE_CURSOR_TASK_V1
```
``` 

