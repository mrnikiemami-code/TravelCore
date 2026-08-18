# TC-P21-T003 Task Envelope (architect, live)

Captured from the same ChatGPT tab after TC-P21-T002 = ACCEPTED and P21-R3 = RESOLVED.

`	ext
TC-P21-T002 = ACCEPTED


Commit:
a844bcf


HEAD == origin/main
Working Tree:
CLEAN

مدل تراکنش پایه هتل درست بسته شده است:

HotelBooking
  ├─ Stay
  ├─ RoomReservation 1..N
  │    └─ HotelBookingGuest 1..N
  ├─ exactly one LeadGuest
  └─ HotelBookingContactSnapshot

قواعد مهم هم درست‌اند:

CheckInDate / CheckOutDate = NodaTime.LocalDate
CheckOutDate > CheckInDate
Nights = CheckOutDate - CheckInDate


Multi-room = IN


GuestCategory:
- Adult
- Child


Child -> AgeAtCheckIn required
BirthDate = NOT STORED
Passport/Documents = NOT STORED

Validation:

HotelBooking.UnitTests: 23 PASS
ArchitectureTests: 297 PASS
Persistence.IntegrationTests: 84 PASS
Host.IntegrationTests: 57 PASS
git diff --check: PASS
وضعیت P21
P21-R1 ✅ Module / schema / Place ownership
P21-R2 ✅ Stay / multi-room / guests / occupancy


P21-R3 ⏳ Availability / inventory / hold / supplier boundary
P21-R4 ⏳ Rate / monetary / ca
`

`	ext
BEGIN_TRAVELCORE_CURSOR_TASK_V1


Protocol-Version:
1


Task-ID:
TC-P21-T003


Phase:
P21


Title:
Authoritative hotel availability source, multi-room hold, idempotency, and supplier-neutral inventory boundary


Baseline:
a844bcf


Decision:
P21-R3 = RESOLVED


Purpose:
Establish the authoritative hotel availability boundary and implement a
supplier-neutral multi-room HotelAvailabilityHold model.


HotelBooking must not fabricate room availability or become the owner of hotel
inventory truth.


The accepted baseline is:


- availability truth comes from an authoritative HotelAvailabilitySource
- no production Hotel supplier/source is currently configured
- HotelBooking records and orchestrates a temporary multi-room availability hold
- one hold covers the complete requested HotelBooking room set
- ambiguous external outcomes must not cause unsafe duplicate holds/reservations
- no pricing/rate/cancellation/payment/lifecycle/public UX is introduced


Do NOT select or implement a real supplier.


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


a844bcf




2. Core ownership decision


Lock:


HotelBooking
!=
Hotel inventory authority


HotelBooking owns transactional reservation intent/hold orchestration.


The authoritative availability source owns whether the requested room inventory is
actually available.




3. Catalog ownership


Preserve:


Place
=
hotel/accommodation catalog truth


Place is NOT live room availability authority.




4. Search boundary


Preserve:


Search
!=
live Hotel availability authority


Search/read models may later support discovery, but transactional availability
must come from the authoritative availability boundary.




5. Current production source posture


Record:


Named Hotel Supplier = NONE


Production Hotel Availability Source = NONE


Do not fabricate a supplier or inventory source.




6. No fake production availability


Do NOT register a fake/test source as production truth.




7. Provider/source-neutral port


Introduce a minimal provider-neutral contract such as:


IHotelAvailabilitySource


or repository-equivalent.




8. Source semantics


The port represents any authoritative source capable of supplying transaction-time
hotel availability.


Future implementations MAY represent:


- external hotel supplier
- TravelCore-owned allotment/inventory
- another authoritative accommodation inventory source


Do not implement these concrete models now.




9. No source-specific types in Domain


HotelBooking.Domain must remain free of:


Booking.com
Expedia
Hotelbeds
WebBeds
Amadeus
supplier-specific DTOs/statuses




10. No named supplier SDK


Do NOT add any real supplier package/SDK.




11. Availability request


Introduce a provider-neutral availability request contract containing only facts
required to determine availability.


Expected concepts:


- HotelPlaceReference
- CheckInDate
- CheckOutDate
- requested room compositions
- per-room AdultCount
- per-room Child ages where required


Do not include Payment or client-authoritative price.




12. Availability occupancy source


Requested occupancy comes from the accepted HotelBooking room/guest structure.


Do not allow caller to submit a second contradictory occupancy model.




13. Multi-room availability


Availability must evaluate the complete requested room set.


One HotelBooking may request:


1..N rooms.




14. Room-level request identity


Each availability request room must correlate to one:


RoomReservationId


or repository-equivalent transaction room identity.




15. Opaque availability selection


Introduce a provider-neutral opaque reference for a selected available room
position/offer if needed.


Use naming such as:


HotelAvailabilitySelectionReference


or equivalent.




16. Selection reference semantics


The reference may identify an authoritative availability-source selection.


It must NOT become:


RoomType catalog authority
RatePlan authority
Price authority




17. No final RoomType ownership decision


P21-R4 remains OPEN regarding commercial/rate semantics.


Do not introduce a HotelBooking-owned RoomType catalog.




18. No rate amount in R3


Do NOT persist authoritative:


- room rate
- tax
- fee
- total
- cancellation penalty


inside R3 availability hold merely for convenience.




19. Availability response


Provider-neutral availability response may contain:


- whether requested room set can be held/booked
- per-room opaque availability selection reference
- freshness/expiry metadata
- source key
- source correlation/reference


No provider-specific status enums.




20. Availability freshness


If source availability is time-sensitive, carry an explicit:


ExpiresAt


or equivalent authoritative freshness boundary.


Use NodaTime Instant for absolute expiry.




21. Availability result != hold


Preserve:


Availability result
!=
HotelAvailabilityHold




22. Availability result != booking confirmation


Preserve:


AvailabilityAvailable
!=
HotelBookingConfirmed




23. Hold required


Before future Payment/confirmation, P21 baseline requires an authoritative temporary:


HotelAvailabilityHold




24. Hold scope


One HotelAvailabilityHold represents the complete HotelBooking requested room set.


Do NOT create unrelated independent holds that can leave the HotelBooking partially
held without explicit orchestration.




25. HotelAvailabilityHold identity


Introduce:


HotelAvailabilityHoldId


using UUIDv7.




26. Hold ownership


HotelAvailabilityHold is HotelBooking-owned transaction state.


It is NOT the source's inventory record.




27. External source hold reference


Store an opaque external/source reference separately.


Preserve:


SourceHoldReference
!=
HotelAvailabilityHoldId




28. Hold lifecycle


Introduce exactly:


HotelAvailabilityHoldStatus:
- Requested
- Active
- Released
- Expired




29. Requested semantics


Requested means:


HotelBooking has durably started a hold acquisition operation, but authoritative
external hold outcome is not yet known/active.




30. Active semantics


Active means:


authoritative availability source confirms the complete requested room set is
temporarily held.




31. Released semantics


Released means:


the temporary availability claim was authoritatively released or locally finalized
as released according to the source contract.




32. Expired semantics


Expired means:


the authoritative hold validity window has elapsed or the source confirms expiry.




33. No Hold Failed status


Do NOT add:


Failed


to HotelAvailabilityHoldStatus.


A definitive acquisition failure may leave the hold acquisition operation terminal
without an Active hold, but do not pollute the accepted lifecycle unless a minimal
attempt/result structure is required.




34. Ambiguous acquisition


Preserve:


NetworkTimeout
!=
Hold acquisition failure




35. Requested ambiguity safety


If source hold request outcome is ambiguous:


HotelAvailabilityHold remains Requested
or equivalent unresolved posture.


Do NOT immediately create a second hold.




36. Unsafe duplicate protection


An unresolved Requested hold blocks another hold acquisition for the same
HotelBooking.




37. Active hold uniqueness


At most one non-terminal/usable hold may exist for one HotelBooking baseline.




38. One active hold


Enforce:


one HotelBooking
->
at most one Active HotelAvailabilityHold




39. Hold room coverage


An Active hold must cover every RoomReservation in the HotelBooking.




40. No partial local Active hold


If only some requested rooms are authoritatively held:


do NOT mark the HotelBooking hold Active.




41. Partial external result


If source returns partial success:


- treat the overall HotelBooking hold acquisition as unsuccessful/unresolved
- release/compensate source-held parts where possible
- do not expose local Active state


Keep provider-specific compensation inside adapter/application orchestration.




42. Multi-room atomicity semantics


TravelCore cannot assume the external supplier performs a distributed atomic
multi-room transaction.


The HotelBooking local invariant is:


Active means complete-room-set hold succeeded.




43. Hold room selections


Persist per-room hold selection/correlation records if required.


Each record must reference:


RoomReservationId
opaque source selection/reference


No price.




44. No physical room number


Do not store actual hotel room number.




45. No room catalog cloning


Do not store room amenities/descriptions/images.




46. Hold expiry


Active hold requires:


ExpiresAt


using NodaTime Instant.




47. Active expiry invariant


ExpiresAt must be after hold activation time.




48. Current-time evaluation


Expired determination must use accepted IClock/NodaTime semantics.


No server-local DateTime.Now.




49. Expiry action


Provide explicit HotelBooking-owned behavior/application service to mark/reconcile
an elapsed Active hold as Expired.




50. No hardcoded TTL


Do NOT invent:


5 minutes
10 minutes
15 minutes


Hold expiry comes from authoritative source/configuration.


No global magic timeout.




51. Hold release


Provide provider-neutral release operation.




52. Release semantics


Release must coordinate with authoritative source.


Do not mark Released before the source/local contract provides sufficient
authoritative outcome unless architecture explicitly supports queued durable
release.




53. Release idempotency


Repeated release is safe.




54. Expiry idempotency


Repeated expiry processing is safe.




55. Active -> Released


Allowed.




56. Active -> Expired


Allowed.




57. Requested -> Active


Allowed only after authoritative source confirmation.




58. Requested -> Released


May be allowed for local abandonment only if source contract proves no active
external hold remains.


Otherwise keep unresolved and reconcile.




59. Terminality


Released and Expired are terminal for the temporary hold.




60. No Released -> Active


Forbidden.




61. No Expired -> Active


Forbidden.




62. No Active recreation


If a terminal hold exists and a later explicit retry is permitted, create a new
HotelAvailabilityHold identity/history record rather than reopening terminal hold.




63. Hold history


Retain historical holds for audit/reconciliation.


Do not overwrite terminal hold as a future retry.




64. Hold attempt limit


Do NOT invent a maximum number of hold attempts.




65. Idempotency


Introduce database-backed idempotency for hold acquisition.




66. Hold acquisition idempotency key


Repeated same request/idempotency key must converge on the same effective hold
operation.




67. Idempotency key identity


Do not use idempotency key as:


HotelAvailabilityHoldId




68. Idempotency persistence


Correctness must survive process restart.




69. Concurrency protection


Two concurrent hold-acquisition requests for the same HotelBooking must not create
two active/unresolved authoritative hold operations.




70. No process-local correctness


Do not use:


static lock
SemaphoreSlim
ConcurrentDictionary
process mutex


as authoritative concurrency mechanism.




71. Database-backed coordination


Use PostgreSQL transactions/constraints/advisory locking/optimistic concurrency
consistent with repository patterns.




72. Multi-instance safety


Correctness must hold across multiple application instances.




73. Source resolver


Introduce a minimal:


IHotelAvailabilitySourceResolver


or equivalent if required.




74. Source key


Use controlled:


AvailabilitySourceKey


or equivalent.




75. Source key != user input authority


Public user cannot instantiate arbitrary source implementation.




76. Server-controlled source selection


Source selection remains server/configuration-controlled.




77. No smart supplier routing


Do NOT implement:


- source ranking
- cheapest supplier
- failover
- weighted routing
- supplier recommendation




78. No automatic fallback


If an ambiguous request exists against Source A:


do NOT automatically create a new hold via Source B.




79. Capability boundary


Keep source capabilities minimal.


Expected R3 capabilities may include:


- CheckAvailability
- CreateHold
- ReleaseHold
- QueryHoldStatus


Do not design a giant hotel supplier capability framework.




80. Hold status query


Introduce provider-neutral authoritative:


QueryHoldStatusAsync


or equivalent.




81. Query outcomes


Translate source-specific result into neutral semantics such as:


- Active
- Released
- Expired
- Pending/Unknown
- NotFound


Do not expose raw supplier status in HotelBooking.Domain.




82. Recheck unresolved hold


Provide callable reconciliation/recheck service for Requested/unresolved hold.




83. Recheck Active result


Authoritative Active result may transition:


Requested -> Active




84. Recheck Released result


Apply safe terminal release semantics.




85. Recheck Expired result


Apply Expired.




86. Recheck Pending/Unknown


Leave unresolved.


Do not fabricate a terminal result.




87. Recheck NotFound


Do not automatically retry.


Interpret only according to source's authoritative certainty.




88. Supplier/source timeout


Preserve:


HTTP timeout
!=
source says NotFound




89. Reconciliation


Introduce only minimal hold/source reconciliation needed for ambiguous results.


Do NOT build operations workflow.




90. Reconciliation != HotelBookingStatus


P21-R5 remains OPEN.


Do not invent HotelBooking lifecycle states for integration uncertainty.




91. Reconciliation issue


If contradictory external evidence requires persistence, use a minimal
HotelBooking-owned operational issue record.


Do not create generic ticketing.




92. Availability source descriptor


Production host should be able to run with:


zero production availability sources configured.




93. Zero-source host


Host startup must remain valid with zero production Hotel availability sources.




94. No-source behavior


No production source means:


HotelBooking cannot claim authoritative availability.


Do not generate Active holds.




95. No public availability API


P21-R8 remains OPEN.


Do NOT expose public:


GET /hotel-availability


or booking endpoints in T003.




96. No fake Book Now path


No production path should imply:


Available / Hold created


without an authoritative source.




97. Test source


A deterministic fake availability source is allowed only inside test projects or
explicit non-production test infrastructure.




98. Test source isolation


Do not register test fake as production default.




99. Availability vs Pricing


Preserve:


Availability
!=
Rate/Price




100. Availability selection vs rate offer


An opaque availability selection may later correlate to a rate offer, but R3 does
not own monetary truth.




101. P21-R4 remains OPEN


Do NOT introduce:


HotelRateOffer
HotelQuote
HotelBookingMonetarySnapshot
CancellationPolicySnapshot




102. P21-R5 remains OPEN


Do NOT introduce:


HotelBookingStatus
SupplierReservation
SupplierBookingAttempt
final confirmation semantics




103. Hold != supplier final reservation


Preserve:


HotelAvailabilityHold
!=
Supplier final hotel reservation




104. Hold != HotelBooking confirmation


Preserve:


Active HotelAvailabilityHold
!=
HotelBookingConfirmed




105. Hold != Payment


Preserve:


HotelAvailabilityHold
!=
Payment




106. Payment integration


Do NOT modify Payment.




107. No Payment target extension


P21-R6 remains OPEN.




108. Refund


Do NOT modify P20 Refund.




109. Partial Refund


Remain DEFERRED.




110. Cancellation


P21-R7 remains OPEN.


Do NOT implement HotelBooking cancellation.




111. Contact/guest privacy


Availability request should send only occupancy facts actually required.


Do NOT send:


- contact email
- phone
- guest names


to availability source unless supplier protocol genuinely requires them.


For R3 baseline:
occupancy only.




112. Child ages


Availability request may include:


AgeAtCheckIn


because hotel occupancy eligibility may depend on child ages.




113. No BirthDate


Continue to preserve:


BirthDate not stored/sent.




114. No passport


No passport/document data.




115. Stay dates


Use HotelBooking accepted:


CheckInDate
CheckOutDate


as the authoritative availability request dates.




116. Room count


Availability request room count must derive from:


HotelBooking.RoomReservations




117. Occupancy tampering


Do not accept a separate client-supplied room/guest composition that differs from
HotelBooking aggregate.




118. Hold binding


A HotelAvailabilityHold must be bound to:


HotelBookingId
HotelPlaceReference
stay dates
complete RoomReservation set


or equivalent immutable correlation.




119. Booking mutation after hold


Until P21-R5/R7 define lifecycle/amendments, prevent silent mutation of stay/room
composition in a way that invalidates an Active hold.


If current aggregate supports mutation, add a guard that an Active/Requested hold
requires release/terminalization before structural reconfiguration.


Do not invent amendment workflow.




120. Hold fingerprint/version


Use immutable snapshot/version/fingerprint if needed to prove the hold corresponds
to the exact HotelBooking stay/occupancy structure.


Do not use a brittle raw JSON hash if simpler structured correlation works.




121. Persistence tables


Create minimum R3 persistence such as:


hotel_booking.hotel_availability_holds


hotel_booking.hotel_availability_hold_rooms


hotel_booking.hotel_hold_idempotency


and minimal reconciliation table only if needed.


Use repository naming conventions.




122. Hold table facts


Expected:


- HotelAvailabilityHoldId
- HotelBookingId
- SourceKey
- SourceHoldReference nullable until known
- Status
- RequestedAt
- ActivatedAt nullable
- ExpiresAt nullable
- ReleasedAt nullable
- version/concurrency field if required




123. Hold-room facts


Expected:


- HotelAvailabilityHoldId
- RoomReservationId
- opaque availability selection/source reference


No rate/money columns.




124. Same-schema FK


Allowed:


Hold -> HotelBooking


HoldRoom -> Hold


HoldRoom -> RoomReservation


inside:


hotel_booking




125. No external supplier FK


External/source references remain opaque values.




126. No Place FK


Place remains logical reference only.




127. Status DB constraint


Protect exact HoldStatus values.




128. Active expiry constraint


Where practical:


Active requires non-null ExpiresAt.




129. Source hold reference


Active should require a valid external/source hold reference where authoritative
source semantics provide one.


Do not force protocols that genuinely have no explicit external hold ID.


Use the smallest neutral representation.




130. One unresolved/active hold constraint


Use DB-backed constraint/index to protect one:


Requested/Active


hold per HotelBooking where practical.




131. Idempotency uniqueness


Use scoped uniqueness:


HotelBookingId + IdempotencyKey


or repository-equivalent.




132. Provider/source reference uniqueness


If source hold reference is unique within a source, enforce:


SourceKey + SourceHoldReference


uniqueness where appropriate.




133. Persistence transaction


Local hold state/idempotency writes are atomic inside HotelBookingDbContext.




134. External call posture


Do not hold a long DB transaction while waiting on external source network call.




135. Staged acquisition


Preferred conceptual flow:


- persist Requested hold/idempotency record
- commit
- call availability source
- persist authoritative result


This protects ambiguous external calls.




136. Exactly-once illusion


Do not claim hold creation is exactly-once externally.




137. Source API idempotency


If a future source supports supplier idempotency keys, pass a stable TravelCore
correlation/idempotency value.


Do not assume all suppliers support it.




138. External duplicate reservation risk


Document that ambiguous source calls remain blocked/reconciled before another hold
attempt.




139. Release call ambiguity


A release network timeout does NOT prove hold remains Active or Released.


Use source recheck where supported.




140. Expiry local clock


If ExpiresAt has passed, HotelBooking may locally treat the hold as unusable.


If supplier truth may differ, recheck/reconciliation remains possible.




141. No overbooking claim


TravelCore must NOT claim it can prevent external supplier overselling beyond the
supplier's authoritative hold guarantees.




142. Internal inventory future


If future TravelCore-owned allotment source is implemented, that adapter/source
must provide equivalent atomic/multi-instance-safe availability semantics.


Do not implement it now.




143. Architecture guardrails


Add tests proving:


HotelBooking != availability authority


Place != live availability authority


Search != live availability authority


Named supplier = NONE


No supplier SDK


HotelAvailabilityHold != HotelBooking confirmation


HotelAvailabilityHold != Payment


Availability != Price


HotelAvailabilityHoldStatus exact values:
Requested / Active / Released / Expired


No HotelBookingStatus


No rate/quote model


No cancellation model


No Payment changes


No public HotelBooking API/UI


No peer-schema FK


No shared DbContext


No peer Infrastructure dependency


No process-local lock as correctness authority




144. Unit tests: Hold lifecycle


Cover:


- new hold starts Requested
- authoritative activation -> Active
- Active requires expiry
- Active -> Released
- Active -> Expired
- Released terminal
- Expired terminal
- terminal hold cannot reactivate




145. Unit tests: room coverage


For multi-room HotelBooking:


Active hold must cover all RoomReservations.




146. Partial hold test


Source holds only some rooms.


Expected:


local HotelAvailabilityHold must NOT become Active.




147. Idempotency unit/service tests


Repeated same acquisition/idempotency key:


same effective hold operation.




148. Ambiguous timeout test


Source initiation timeout/unknown:


hold remains unresolved/Requested
no second automatic hold
no fabricated Active/Released/Expired state




149. Retry blocking test


Requested unresolved hold blocks new hold acquisition.




150. Terminal retry test


After a definitive terminal Released/Expired/failed acquisition posture where retry
is permitted:


a new explicit hold may use a new HotelAvailabilityHold identity.




151. Release idempotency test


Repeated release safe.




152. Expiry idempotency test


Repeated expiry safe.




153. Recheck tests


Authoritative source query:


Active -> correct activation/convergence
Released -> Released
Expired -> Expired
Pending/Unknown -> unresolved




154. Occupancy request test


Verify availability request derives room Adult/Child composition and child
AgeAtCheckIn from HotelBooking.




155. PII test


Availability request contract contains no:


email
phone
guest names
passport
documents




156. Persistence test


Verify hold round-trip.




157. Multi-room hold persistence


Verify all RoomReservation correlations round-trip.




158. Idempotency persistence


Verify restart-safe idempotency record.




159. Concurrency: hold acquisition


Run two concurrent hold requests for same HotelBooking.


Expected:


at most one Requested/Active effective hold.




160. Concurrency: same idempotency key


Expected:


same hold identity/effective operation.




161. Process restart


Verify no in-memory state is required for duplicate protection.




162. Source reference uniqueness test


Where implemented, same source hold reference cannot attach to two unrelated
HotelBookings.




163. Persistence inspection


Verify:


no money/rate columns
no supplier raw JSON
no peer FKs




164. Host tests


Verify:


HotelBooking module starts with zero production availability sources.




165. No endpoints


Verify no public HotelBooking/availability routes exist.




166. No frontend


Frontend remains untouched.




167. Documentation: inventory alternatives


Update P21 plan to record the accepted R3 posture:


P21 baseline does NOT implement a TravelCore-owned inventory ledger.


Availability comes through an authoritative source abstraction.


Future source implementation may be external supplier or TravelCore-owned
allotment without changing HotelBooking ownership boundaries.




168. R3 decision


Record:


- HotelBooking is not live inventory authority
- Place/Search are not live availability authority
- IHotelAvailabilitySource (or equivalent) owns authoritative availability
  integration boundary
- Named Hotel Supplier = NONE
- Production Availability Source = NONE
- no fake availability
- one HotelBooking hold covers complete 1..N room request
- HotelAvailabilityHoldStatus = Requested / Active / Released / Expired
- Active means complete requested room set is authoritatively held
- partial result never becomes local Active
- ambiguous network/source outcome remains unresolved and blocks unsafe retry
- hold expiry is source-provided; no hardcoded TTL
- source selection is server-controlled
- no supplier smart routing/failover
- hold is not final HotelBooking confirmation
- supplier final reservation remains R5
- price/rate/cancellation terms remain R4
- Payment remains R6




169. P21-R3


Record:


RESOLVED




170. Remaining decisions


Keep:


P21-R4 through P21-R8 = OPEN




171. P21 state


Remain:


IN_PROGRESS




172. Do not execute T004


TC-P21-T004 = NOT EXECUTED




Allowed:


- authoritative availability source port
- source resolver
- controlled source key
- neutral availability request/result
- opaque selection/source references
- HotelAvailabilityHold
- HotelAvailabilityHoldId
- HotelAvailabilityHoldStatus
- hold-room correlations
- hold expiry
- hold acquisition/release/recheck application services
- DB-backed idempotency/concurrency
- minimal hold reconciliation evidence
- test-only fake availability source
- HotelBooking-local migrations
- unit/architecture/persistence/host tests
- documentation/SoT synchronization for R3




Forbidden:


- P21-R4 through R8 decisions
- HotelBookingStatus
- final SupplierReservation
- SupplierBookingAttempt
- final booking confirmation
- named hotel supplier
- supplier SDK
- production fake supplier
- TravelCore-owned room inventory ledger
- RoomType catalog ownership
- RatePlan
- HotelRateOffer price
- HotelQuote
- HotelBookingMonetarySnapshot
- cancellation policy
- Payment target extension
- Refund changes
- Partial Refund
- public availability API
- public HotelBooking API
- frontend booking UI
- smart supplier routing/failover
- Search live-availability ownership
- shared DbContext
- peer-schema FK
- peer Infrastructure dependency
- process-local correctness authority




Done:


- HotelBooking no longer has any ambiguity about availability ownership
- authoritative availability comes through a source-neutral port
- no production supplier/source is invented
- no fake availability is exposed
- multi-room hold model exists
- one hold covers complete Booking room set
- Requested/Active/Released/Expired lifecycle exists
- partial source success cannot become Active
- ambiguous outcomes do not become false failure/success
- unresolved hold blocks unsafe duplicate acquisition
- idempotency/concurrency is database-backed
- hold expiry is source-authoritative
- no rate/pricing/payment/cancellation/lifecycle/public behavior leaks into R3
- P21-R3 recorded RESOLVED
- P21-R4 through P21-R8 remain OPEN




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


- availability authority
- Place live availability authority: NO
- Search live availability authority: NO
- availability source port name
- Named Hotel Supplier
- Production Availability Source
- production fake source: NO
- HotelAvailabilityHoldStatus exact values
- one hold covers multi-room booking: YES
- Active hold complete-room requirement
- partial source result behavior
- hold expiry source
- hardcoded TTL: NO
- ambiguous timeout behavior
- unresolved hold retry behavior
- concurrent hold result
- same idempotency-key result
- source selection server-controlled: YES
- automatic supplier failover: NO
- supplier smart routing: NO
- HotelBookingStatus: NO
- supplier final reservation model: NO
- rate/quote/money model: NO
- cancellation model: NO
- Payment integration/change: NO
- Refund/Partial Refund changes: NO
- public HotelBooking/availability API: NO
- peer-schema FK: NO
- shared DbContext: NO
- peer Infrastructure dependency: NO
- process-local correctness authority: NO
- P21-R3: RESOLVED
- P21-R4 through P21-R8: OPEN
- TC-P21-T004: NOT EXECUTED




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


- commit with TC-P21-T003 in commit message
- push main to origin/main using normal fast-forward push
- re-fetch origin
- verify HEAD == origin/main
- verify Working Tree CLEAN




Expected Baseline:
a844bcf




Auto-Execute:


After PASS:


- return TC-P21-T003 RESULT to architect
- do NOT execute TC-P21-T004 until T003 is architect ACCEPTED
- do NOT invent P21-R4 through P21-R8
- remain in PIPELINE




END_TRAVELCORE_CURSOR_TASK_V1
`
