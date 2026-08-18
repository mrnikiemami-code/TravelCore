# TC-P21-T005 Task Envelope

Captured live after `TC-P21-T004 = ACCEPTED`. `P21-R5 = RESOLVED`. Baseline `9f38ef6`.

```text
بررسی TC-P21-T004
TC-P21-T004 = ACCEPTED


Implementation Commit:
9d24b84


Final HEAD:
9f38ef6


HEAD == origin/main
Working Tree:
CLEAN

R4 دقیقاً در مرز تجاری درست بسته شده است. این سه حقیقت از هم جدا مانده‌اند:

HotelRateOfferSnapshot
!=
HotelBookingMonetarySnapshot
!=
Payment

و همین‌طور:

HotelCancellationPolicySnapshot
!=
Cancellation Execution
!=
Refund Execution

نکات کلیدی صحیح‌اند:

Commercial Rate Authority:
IHotelRateOfferSource


Production Hotel Rate Source:
NONE


Named Hotel Supplier:
NONE


Pricing generalized:
NO


Partial Refund implemented:
NO

قیمت پذیرفته‌شده نیز دیگر Live نیست:

Accepted HotelRateOfferSnapshot
        |
        v
HotelBookingMonetarySnapshot
        |
        v
immutable transaction truth

و Repricing خاموش هم ممنوع مانده است:

Expired Offer -> rejected
Higher replacement -> not silently accepted
Lower replacement -> not silently accepted

Validation:

HotelBooking.UnitTests: 51 PASS
ArchitectureTests: 298 PASS
Persistence.IntegrationTests: 89 PASS
Host.IntegrationTests: 57 PASS
git diff --check: PASS
وضعیت P21
P21-R1 ✅ Module / schema / Place ownership
P21-R2 ✅ Stay / multi-room / guests
P21-R3 ✅ Availability source / hold
P21-R4 ✅ Rate / monetary snapshot / cancellation terms


P21-R5 ⏳ Lifecycle / final supplier reservation / reconciliation
P21-R6 ⏳ Payment integration / compensation
P21-R7 ⏳ Cancellation / amendment / refund policy
P21-R8 ⏳ Public UX / authorization / privacy / supplier readiness
قفل P21-R5
P21-R5 = RESOLVED

در R5 باید یک نکته را خیلی تمیز نگه داریم:

HotelAvailabilityHold
!=
SupplierReservation
!=
HotelBooking

Hold فقط تضمین موقت Availability است. رزرو نهایی Supplier یک اجرای جداگانه است، و HotelBooking حقیقت تجاری TravelCore است.

Lifecycle پایه HotelBooking:

HotelBookingStatus:
- Pending
- Confirmed
- Cancelled

اما Pending نباید وضعیت فنی Supplier را مخفی کند. برای مراحل اجرای خارجی، Entity جدا داریم:

HotelSupplierReservation

و Attemptهای آن:

HotelSupplierReservationAttempt

پس:

HotelBookingStatus
!=
SupplierReservationStatus
!=
SupplierReservationAttemptStatus

Baseline رزرو Supplier:

HotelSupplierReservationStatus:
- Pending
- Confirmed
- Cancelled

و Attempt:

HotelSupplierReservationAttemptStatus:
- Created
- Initiated
- Confirmed
- Failed

مهم‌ترین اصل:

NetworkTimeout
!=
SupplierReservationAttempt.Failed

اگر نتیجه مبهم باشد، Attempt در حالت غیرترمینال می‌ماند و Retry کور ممنوع است.

Confirmation نیز فقط وقتی انجام می‌شود که:

authoritative supplier reservation confirmed
+
accepted HotelRateOfferSnapshot still matches transaction
+
HotelBookingMonetarySnapshot exists
+
required hold/reservation prerequisites valid

و:

Supplier callback/browser/client flag
!=
HotelBookingConfirmed

Payment هنوز وارد نمی‌شود؛ R6 مالک آن است. بنابراین R5 باید HotelBooking confirmation را از Payment مستقل نگه دارد و فقط Supplier reservation side را بسازد.

Task کامل:

BEGIN_TRAVELCORE_CURSOR_TASK_V1


Protocol-Version:
1


Task-ID:
TC-P21-T005


Phase:
P21


Title:
HotelBooking lifecycle, final supplier reservation, idempotent reservation attempts, authoritative confirmation, and reconciliation


Baseline:
9f38ef6


Decision:
P21-R5 = RESOLVED


Purpose:
Implement the HotelBooking business lifecycle and the final supplier-reservation
execution boundary.


This task establishes:


- HotelBookingStatus
- HotelSupplierReservation
- HotelSupplierReservationAttempt
- authoritative supplier confirmation
- duplicate-safe/idempotent supplier reservation execution
- ambiguity/recheck/reconciliation
- HotelBooking confirmation driven by authoritative reservation evidence


Do NOT implement Payment integration, payment-driven confirmation, HotelBooking
cancellation policy/execution, partial Refund, public HotelBooking API/UI, or a real
hotel supplier.


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


9f38ef6




2. Record T004 acceptance


Synchronize SoT to record:


TC-P21-T004 = ACCEPTED


Preserve all accepted R1-R4 semantics.




3. Core lifecycle decision


Introduce exactly:


HotelBookingStatus:
- Pending
- Confirmed
- Cancelled




4. Initial status


A newly created HotelBooking starts:


Pending




5. Pending semantics


Pending means:


the HotelBooking transaction exists,
but final authoritative reservation confirmation has not yet been accepted.




6. Confirmed semantics


Confirmed means:


TravelCore has authoritative evidence that the final hotel reservation exists and
the HotelBooking confirmation invariants are satisfied.




7. Cancelled semantics


Cancelled means:


the HotelBooking is no longer active as a TravelCore booking.


Actual confirmed-cancellation execution remains P21-R7.




8. No extra HotelBooking statuses


Do NOT add:


Requested
Failed
Expired
Refunding
AwaitingPayment
SupplierPending
ReconciliationRequired




9. Integration/process state separation


Preserve:


HotelBookingStatus
!=
supplier execution state
!=
Payment state




10. Final supplier reservation entity


Introduce HotelBooking-owned:


HotelSupplierReservation


or repository-equivalent.




11. Supplier reservation ownership


HotelSupplierReservation represents TravelCore's durable correlation to the
authoritative final hotel reservation source.


It is NOT the external supplier's database row.




12. Supplier reservation identity


Introduce:


HotelSupplierReservationId


using UUIDv7.




13. Supplier reservation target


Exactly one logical HotelSupplierReservation belongs to one HotelBooking baseline.




14. One final reservation baseline


Use:


one HotelBooking
->
one logical final HotelSupplierReservation


for the complete multi-room Booking baseline.




15. Multi-room final reservation


The logical reservation covers the complete accepted room set.


Do not independently confirm only some RoomReservations while marking HotelBooking
Confirmed.




16. Partial reservation result


If source confirms only a subset of rooms:


HotelBooking must NOT become Confirmed.




17. Reservation status


Introduce exactly:


HotelSupplierReservationStatus:
- Pending
- Confirmed
- Cancelled




18. New reservation status


New logical HotelSupplierReservation starts:


Pending




19. Confirmed reservation


Confirmed means authoritative supplier/source evidence confirms the complete
reservation.




20. Cancelled reservation


Cancelled means authoritative supplier/source evidence confirms the final hotel
reservation has been cancelled or is no longer active.


P21-R7 owns business cancellation initiation/policy.




21. No Failed reservation status


Do NOT add:


HotelSupplierReservationStatus.Failed




22. Reservation vs attempt


Preserve:


HotelSupplierReservation
!=
HotelSupplierReservationAttempt




23. Attempt entity


Introduce:


HotelSupplierReservationAttempt




24. Attempt identity


Use:


HotelSupplierReservationAttemptId


UUIDv7.




25. Attempt lifecycle


Introduce exactly:


HotelSupplierReservationAttemptStatus:
- Created
- Initiated
- Confirmed
- Failed




26. Created semantics


Local execution attempt exists but no external reservation request has completed.




27. Initiated semantics


External reservation request may have been sent / outcome may still be unresolved.




28. Confirmed attempt


Authoritative supplier evidence proves reservation success for this attempt.




29. Failed attempt


Authoritative supplier evidence proves this attempt did NOT create the final
reservation.




30. Failed attempt != failed HotelBooking


Preserve:


Failed SupplierReservationAttempt
!=
HotelBooking failed state




31. No HotelBooking Failed status


Hard requirement.




32. Network ambiguity


Preserve:


NetworkTimeout
!=
HotelSupplierReservationAttempt.Failed




33. Ambiguous attempt


If external call times out or outcome is unknown:


Attempt remains Initiated/unresolved.




34. Unsafe retry


An unresolved Initiated attempt blocks another supplier reservation attempt.




35. Definitive failure retry


After authoritative Failed attempt:


a new explicit attempt may be created under same logical HotelSupplierReservation.




36. No retry after confirmed reservation


If HotelSupplierReservation is Confirmed:


no new reservation attempt.




37. One legitimate successful attempt


At most one attempt may legitimately confirm one logical reservation.




38. Active attempt uniqueness


At most one:


Created / Initiated


attempt per HotelSupplierReservation.




39. Database-backed correctness


Use PostgreSQL constraints/transactions/advisory locks/optimistic concurrency
consistent with repository patterns.




40. No process-local authority


Do not use static/process locks as correctness authority.




41. Supplier-neutral final reservation port


Introduce a minimal source-neutral port such as:


IHotelReservationSource


or equivalent.




42. Reservation source resolver


Introduce:


IHotelReservationSourceResolver


if needed.




43. Source responsibility


The final reservation source is authoritative for external hotel reservation
creation/query/cancellation evidence.




44. No named supplier


Keep:


Named Hotel Supplier = NONE




45. Production reservation source


Keep:


Production Hotel Reservation Source = NONE




46. No production fake


Test fake may exist only in test/non-production infrastructure.




47. No supplier SDK


Do NOT add any real supplier SDK/package.




48. Source selection


Server-controlled.




49. No smart routing


Do NOT implement:


- supplier ranking
- cheapest booking source
- automatic failover
- weighted routing
- source switching




50. Source correlation


Store:


ReservationSourceKey
SourceReservationReference


or equivalent.




51. External reference separation


Preserve:


SourceReservationReference
!=
HotelSupplierReservationId
!=
HotelBookingId




52. Supplier confirmation number


If source provides a human-facing confirmation code/reference, persist it as an
opaque supplier confirmation fact separate from internal IDs.




53. No physical room number


Do not store hotel-assigned physical room number unless source confirmation
requires it as immutable confirmation fact.


Preferred:
defer.




54. Reservation request source


Reservation request must derive from accepted HotelBooking transaction facts:


- HotelPlaceReference
- CheckInDate
- CheckOutDate
- complete RoomReservation set
- occupancy
- accepted HotelRateOfferSnapshot
- HotelBookingMonetarySnapshot
- accepted cancellation-policy snapshot
- active/usable availability hold correlation where source semantics require it




55. No client reservation payload authority


Do not accept an arbitrary client-supplied reservation structure as authoritative.




56. Guest data boundary


Final reservation source may require guest names.


Unlike R3/R4, reservation creation may legitimately need:


- GivenName
- FamilyName
- lead guest
- occupancy
- contact where supplier protocol requires it


Use only existing HotelBooking snapshots.




57. No additional PII invention


Do NOT add:


passport
national ID
document scans
DOB


unless a real supplier later proves it mandatory.




58. Reservation request PII minimization


Send only facts required by the neutral reservation contract.




59. No raw supplier payload persistence


Do not persist raw supplier JSON as authoritative model.




60. Reservation prerequisites


Before creating supplier reservation attempt, require:


- HotelBooking Pending
- accepted HotelRateOfferSnapshot exists
- HotelBookingMonetarySnapshot exists
- accepted room coverage complete
- accepted cancellation-policy snapshot exists where rate requires it
- no existing Confirmed supplier reservation
- no unresolved active reservation attempt




61. Hold prerequisite


If R3 hold is still part of the accepted source flow, require an Active unexpired
HotelAvailabilityHold before reservation initiation.


Do not use Expired/Released hold for final reservation.




62. Source protocol flexibility


If reservation source contract explicitly proves a hold is not required for that
source, model the capability/requirement in neutral source descriptor rather than
hardcoding all suppliers identically.




63. Baseline safest posture


Preferred baseline:
reservation initiation requires an Active unexpired HotelAvailabilityHold.


Do not weaken unless current R3 source contract already models no-hold capability.




64. Hold expiry check


Use IClock / NodaTime Instant.




65. Expired hold before supplier reservation


Expected:


do not call reservation source.




66. Released hold before supplier reservation


Expected:


do not call reservation source.




67. Requested unresolved hold


Expected:


do not call final reservation source.




68. Rate offer validity


Do not use a structurally mismatched or superseded rate snapshot.




69. Offer expiry after acceptance


Accepted immutable rate snapshot remains transaction truth.


Do not silently reprice.


Whether final supplier requires revalidation must be handled through authoritative
reservation response/reconciliation, not mutation.




70. No silent price change


If supplier final reservation reports a different monetary amount/currency:


do NOT confirm HotelBooking automatically.




71. Monetary mismatch


Persist reconciliation evidence.


Do not rewrite HotelBookingMonetarySnapshot.




72. Currency mismatch


Same rule.




73. Cancellation terms mismatch


If supplier final reservation terms materially differ from accepted cancellation
snapshot:


do not silently confirm.


Persist reconciliation evidence.




74. Room-set mismatch


If supplier confirms incomplete/different room set:


do not confirm.




75. Stay mismatch


Different dates:


do not confirm.




76. Hotel mismatch


Different Hotel/Place correlation:


do not confirm.




77. Authoritative confirmation


Only authoritative supplier reservation verification/query/callback evidence may
transition:


HotelSupplierReservation -> Confirmed




78. Client success flag


Preserve:


ClientReservationSuccess
!=
HotelSupplierReservation.Confirmed




79. Browser return


Preserve:


BrowserReturn
!=
HotelBooking.Confirmed




80. Unverified supplier callback


Preserve:


UnverifiedSupplierCallback
!=
HotelSupplierReservation.Confirmed




81. Reservation callback contract


Introduce provider-neutral technical callback handling only if needed by neutral
source boundary.


Do NOT expose public booking-success endpoint.




82. Callback verification


Source adapter must verify authenticity before authoritative state transition.




83. Callback replay


Repeated verified reservation success must be idempotent.




84. Cross-booking correlation


Evidence for HotelBooking A must not affect HotelBooking B.




85. Cross-attempt correlation


Evidence for Attempt A must not confirm Attempt B.




86. Reservation query


Provide:


QueryReservationStatusAsync


or equivalent authoritative source query capability.




87. Recheck service


Provide callable:


HotelSupplierReservationRecheckService


or equivalent.




88. Query outcome neutralization


Translate supplier-specific statuses to neutral outcomes such as:


Confirmed
Failed/NotCreated
Cancelled
Pending/Unknown
NotFound


Do not leak supplier status enum into Domain.




89. Pending/Unknown query


Leave attempt unresolved.


Do not fabricate failure.




90. NotFound ambiguity


Treat as authoritative failure only if source contract defines NotFound as proof no
reservation exists.


Otherwise remain reconciliation-required/unresolved.




91. No auto retry on ambiguous query


Hard requirement.




92. Reconciliation model


Introduce minimal:


HotelBookingReconciliationIssue


or more narrowly named supplier-reservation reconciliation record.




93. Reconciliation ownership


HotelBooking owns reservation reconciliation evidence.




94. Reconciliation != HotelBookingStatus


Do not add integration exceptions to HotelBookingStatus.




95. Minimum issue kinds


Model only concrete R5 needs, such as:


- MonetaryMismatch
- CurrencyMismatch
- RoomSetMismatch
- StayMismatch
- HotelMismatch
- CancellationTermsMismatch
- ContradictorySupplierEvidence
- AmbiguousReservationOutcome


Adjust exact names to implementation conventions.




96. No generic ticket workflow


Do NOT add:


Assigned
Investigating
ResolvedBy
SLA
ticket queue




97. Contradictory terminal evidence


Do not silently flip:


Confirmed -> Pending
Confirmed -> Failed
Cancelled -> Confirmed


based on contradictory later evidence.




98. Terminal contradiction


Create reconciliation evidence instead.




99. HotelBooking confirmation operation


Implement a constrained:


ConfirmFromAuthoritativeSupplierReservation


or equivalent.




100. No unrestricted Confirm


Do NOT expose generic:


Confirm()
SetConfirmed()
MarkConfirmed(bool)




101. Confirmation ownership


HotelBooking owns the transition:


Pending -> Confirmed




102. Confirmation prerequisites


Before confirming HotelBooking, verify:


- current HotelBooking = Pending
- HotelSupplierReservation = Confirmed
- reservation belongs to this HotelBooking
- complete room set confirmed
- hotel/stay facts match
- HotelBookingMonetarySnapshot exists and matches authoritative reservation evidence
- accepted cancellation terms are not contradicted
- no blocking reconciliation issue




103. Payment independence


P21-R6 remains OPEN.


Do NOT require Payment success for R5 confirmation yet.


Do NOT modify P20 Payment.




104. R5 confirmation semantics


This task only establishes:


supplier reservation confirmation authority


not the final combined supplier+Payment orchestration decision.




105. Future R6


R6 may later strengthen final confirmation prerequisites if PayNow is selected.


Do not pre-generalize in T005.




106. Confirmation transaction


Locally persist:


HotelSupplierReservation confirmed state
+
HotelBooking Pending -> Confirmed


atomically where the authoritative evidence is applied in the same module transaction.




107. If confirmation evidence arrives asynchronously


Use HotelBooking-local transaction + outbox as appropriate.




108. Domain event


Emit minimal durable:


HotelBookingConfirmedIntegrationEvent


or equivalent only if current architecture needs it.


Do not create unnecessary event taxonomy.




109. Confirmation event semantics


Event means:


HotelBooking is authoritatively Confirmed


not:


Payment succeeded.




110. No Payment event coupling


Do not consume/emit Payment success in T005.




111. Reservation created but local commit fails


Design callback/query/idempotency path so authoritative supplier reservation can be
recovered/reconciled later.




112. Supplier confirms but process crashes


Database/provider correlation must allow recheck.


Do not rely on memory.




113. Source request idempotency


Use stable idempotency correlation for reservation attempt when source supports it.




114. External exactly-once


Do NOT claim exactly-once supplier reservation creation.




115. Duplicate reservation prevention


Design:


persistent local idempotency
+
one unresolved attempt
+
source idempotency key where supported
+
authoritative recheck before retry




116. Reservation initiation idempotency key


Persist:


HotelSupplierReservationId + IdempotencyKey


or repository-equivalent.




117. Duplicate same idempotency key


Converge to same effective attempt.




118. Concurrent initiation


Two concurrent initiations:


at most one Created/Initiated active attempt.




119. Attempt history


Retain prior Failed attempts for audit.




120. No overwriting attempt history


Hard requirement.




121. Final reservation uniqueness


Protect:


one logical HotelSupplierReservation per HotelBooking baseline.




122. Source reservation uniqueness


Where source reference is unique per source:


SourceKey + SourceReservationReference


must not bind to two HotelBookings.




123. Reservation version/concurrency


Use DB-backed optimistic concurrency/versioning where appropriate.




124. Persistence tables


Create minimum R5 tables conceptually:


hotel_booking.hotel_supplier_reservations


hotel_booking.hotel_supplier_reservation_attempts


hotel_booking.hotel_supplier_reservation_idempotency


hotel_booking.hotel_booking_reconciliation_issues


Use repository naming conventions.




125. HotelBooking status persistence


Add HotelBookingStatus to HotelBooking persistence.




126. Existing rows/migration


Migration must safely assign existing T002/T004 HotelBookings:


Pending


without fabricating confirmation.




127. Same-schema FK


Allowed within:


hotel_booking




128. No supplier FK


External supplier refs opaque.




129. No Place FK


Place logical only.




130. No Payment FK


No Payment integration.




131. No cross-schema FK


Hard requirement.




132. No shared DbContext


Hard requirement.




133. No peer Infrastructure dependency


Hard requirement.




134. No direct Payment/Booking/Pricing schema access


Hard requirement.




135. Cancellation status


Do NOT implement user cancellation command.




136. HotelBooking Cancelled transition


The enum exists, but R7 owns the business cancellation operation.


Do not expose generic Cancel in T005.




137. SupplierReservation Cancelled


May be applied only from authoritative external evidence/reconciliation if such
evidence arrives.


Do not create public cancellation workflow.




138. Confirmed Booking cancellation


Do NOT implement:


HotelBooking Confirmed -> Cancelled


as user/business capability.




139. Pending local abandonment


Do not add broad cancellation command merely to use Cancelled enum.




140. Payment


No Payment target extension.




141. Refund


No Refund changes.




142. Partial Refund


Remain DEFERRED.




143. Public API


No HotelBooking public API.




144. Frontend


No HotelBooking UI.




145. Search/SEO


No changes.




146. Operational read


Do not build R8 operations surface yet.


Reconciliation records may have internal query repository tests only.




147. Supplier descriptors/capabilities


Keep minimal R5 source requirements.


Do not build full provider capability matrix reserved for R8.




148. Production source zero


Host must start with:


Production Hotel Reservation Source = NONE




149. No-source behavior


Reservation initiation must fail safely/honestly.


No fabricated Confirmed reservation.




150. Architecture guardrails


Add tests proving exact:


HotelBookingStatus:
Pending / Confirmed / Cancelled


HotelSupplierReservationStatus:
Pending / Confirmed / Cancelled


HotelSupplierReservationAttemptStatus:
Created / Initiated / Confirmed / Failed


HotelBooking != SupplierReservation


SupplierReservation != Attempt


Failed Attempt != failed HotelBooking


NetworkTimeout != Failed Attempt


HotelBooking confirmation requires authoritative reservation


Unverified callback cannot confirm


No generic Confirm


No cancellation execution


No Payment integration


No Refund changes


No public HotelBooking API/UI


Named supplier = NONE


Production reservation source = NONE


No supplier SDK


No peer-schema FK


No shared DbContext


No peer Infrastructure dependency




151. Unit tests: HotelBooking lifecycle


Cover:


- new HotelBooking = Pending
- authoritative valid supplier confirmation -> Confirmed
- Confirmed terminal against duplicate confirm
- no generic confirmation bypass
- Cancelled cannot be reopened by confirmation evidence




152. Unit tests: reservation lifecycle


Cover:


- new logical reservation = Pending
- authoritative success -> Confirmed
- authoritative cancellation evidence -> Cancelled where valid
- terminal contradiction produces reconciliation issue




153. Unit tests: attempt lifecycle


Cover:


- Created -> Initiated
- definitive failure -> Failed
- authoritative success -> Confirmed
- Failed allows new attempt
- Initiated unresolved blocks new attempt
- confirmed reservation blocks new attempt




154. Timeout test


External timeout:


Attempt remains Initiated
Reservation remains Pending
HotelBooking remains Pending




155. No-source test


No configured production source:


no reservation created/confirmed externally
HotelBooking not Confirmed




156. Multi-room confirmation


Complete multi-room authoritative result:


eligible to confirm.




157. Partial room confirmation


Expected:


HotelBooking remains Pending
Reservation not authoritatively Confirmed locally
reconciliation evidence exists




158. Monetary mismatch


Expected:


HotelBooking remains Pending
no snapshot mutation
reconciliation issue




159. Currency mismatch


Same.




160. Cancellation-terms mismatch


Expected:


no silent confirmation
reconciliation evidence




161. Stay mismatch


No confirmation.




162. Hotel mismatch


No confirmation.




163. Callback replay


Same authoritative success applied twice:


one effective reservation confirmation
one HotelBooking confirmation.




164. Cross-booking callback


Evidence for Booking A cannot confirm B.




165. Cross-attempt evidence


Cannot confirm wrong attempt.




166. Idempotency test


Same initiation idempotency key:


same effective attempt.




167. Concurrent attempt test


Two concurrent initiations:


at most one Created/Initiated attempt.




168. Failed retry test


New attempt after definitive Failed.




169. Ambiguous retry test


Blocked after unresolved Initiated.




170. Persistence round-trip


Cover:


HotelBookingStatus
SupplierReservation
Attempts
source references
reconciliation issue




171. Persistence uniqueness


Cover:


one logical reservation per HotelBooking.




172. Persistence active-attempt uniqueness


Cover.




173. Persistence source-reference uniqueness


Cover where applicable.




174. Crash/recheck scenario


Simulate:


source reservation confirmed
local application did not finish confirmation
later QueryReservationStatus returns confirmed
system converges safely.




175. No process-memory requirement


Prove recovery depends on persisted IDs/references.




176. Host tests


Host starts with no production reservation source.




177. No endpoints


Verify no public HotelBooking reservation route.




178. Frontend untouched


Expected.




179. Source-of-Truth synchronization


Update P21 docs to record:


TC-P21-T004 = ACCEPTED


P21-R5 = RESOLVED




180. R5 decision summary


Record:


- HotelBookingStatus = Pending / Confirmed / Cancelled
- HotelSupplierReservation is a distinct HotelBooking-owned external reservation
  correlation
- one logical SupplierReservation per HotelBooking baseline
- SupplierReservationStatus = Pending / Confirmed / Cancelled
- reservation has retryable SupplierReservationAttempts
- AttemptStatus = Created / Initiated / Confirmed / Failed
- Failed Attempt does not fail HotelBooking
- ambiguous timeout remains unresolved
- unresolved attempt blocks unsafe retry
- authoritative supplier evidence is required
- one logical final reservation covers complete multi-room request
- partial room confirmation cannot confirm HotelBooking
- amount/currency/stay/hotel/cancellation-term mismatch blocks confirmation
- contradictory evidence creates reconciliation evidence
- HotelBooking confirmation is Booking-owned
- no unrestricted Confirm
- Payment integration remains R6
- cancellation business execution remains R7
- Named Hotel Supplier = NONE
- Production Hotel Reservation Source = NONE




181. Decision status


Record:


P21-R5 = RESOLVED




182. Remaining decisions


Keep:


P21-R6 through P21-R8 = OPEN




183. P21


Remain:


IN_PROGRESS




184. Do not execute T006


TC-P21-T006 = NOT EXECUTED




Allowed:


- HotelBookingStatus
- HotelSupplierReservation
- HotelSupplierReservationId
- HotelSupplierReservationStatus
- HotelSupplierReservationAttempt
- HotelSupplierReservationAttemptId
- HotelSupplierReservationAttemptStatus
- IHotelReservationSource
- resolver
- neutral reservation request/result
- source refs
- reservation initiation/recheck/verification
- DB-backed idempotency/concurrency
- minimal reconciliation issue model
- constrained authoritative HotelBooking confirmation
- HotelBooking-local migrations
- unit/architecture/persistence/host tests
- SoT synchronization for R5




Forbidden:


- P21-R6-R8 decisions
- Payment target extension
- Payment code changes
- Refund changes
- Partial Refund
- public HotelBooking API
- frontend HotelBooking UI
- user cancellation workflow
- confirmed cancellation
- supplier SDK
- named supplier
- production fake supplier
- smart supplier routing
- generic Booking abstraction
- shared DbContext
- peer-schema FK
- peer Infrastructure dependency
- unrelated refactor
- dependency upgrades




Done:


- HotelBooking lifecycle exists
- final supplier reservation is distinct from HotelBooking
- supplier reservation attempts preserve retry history
- ambiguous external result does not fabricate failure/success
- unsafe duplicate supplier booking is blocked
- final multi-room confirmation is all-or-nothing locally
- authoritative supplier evidence is required
- supplier/local mismatch creates reconciliation instead of silent mutation
- HotelBooking confirmation is constrained and Booking-owned
- no Payment/cancellation/public capability leaks into R5
- P21-R5 recorded RESOLVED
- P21-R6 through P21-R8 remain OPEN




Validation:


Run:


dotnet build TravelCore.sln


HotelBooking.UnitTests


ArchitectureTests


Persistence.IntegrationTests


Host.IntegrationTests


frontend validation only if frontend files are touched


git diff --check




Required Result Evidence:


Report exact:


- HotelBooking Unit test count
- Architecture test count
- Persistence Integration test count
- Host Integration test count
- frontend touched YES/NO
- git diff --check


Also report:


- HotelBookingStatus exact values
- initial HotelBooking status
- SupplierReservation type
- SupplierReservationStatus exact values
- SupplierReservationAttemptStatus exact values
- one logical reservation per HotelBooking
- one reservation covers complete multi-room set
- partial room confirmation behavior
- Named Hotel Supplier
- Production Hotel Reservation Source
- supplier SDK
- network timeout behavior
- unresolved attempt retry behavior
- definitive failed attempt retry behavior
- concurrent attempt result
- same idempotency-key result
- authoritative confirmation source
- unverified callback behavior
- callback replay result
- cross-booking correlation result
- monetary mismatch result
- currency mismatch result
- cancellation-terms mismatch result
- stay/hotel mismatch result
- HotelBooking confirmation owner
- generic Confirm surface: NO
- user cancellation execution: NO
- Confirmed cancellation: NO
- Payment integration/change: NO
- Refund/Partial Refund change: NO
- public HotelBooking API/UI: NO
- peer-schema FK: NO
- shared DbContext: NO
- peer Infrastructure dependency: NO
- P21-R5: RESOLVED
- P21-R6 through P21-R8: OPEN
- TC-P21-T006: NOT EXECUTED




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


- commit with TC-P21-T005 in commit message
- push main to origin/main using normal fast-forward push
- re-fetch origin
- verify HEAD == origin/main
- verify Working Tree CLEAN




Expected Baseline:
9f38ef6




Auto-Execute:


After PASS:


- return TC-P21-T005 RESULT to architect
- do NOT execute TC-P21-T006 until T005 is architect ACCEPTED
- do NOT invent P21-R6 through P21-R8
- remain in PIPELINE




END_TRAVELCORE_CURSOR_TASK_V1
``` 

