# TC-P21-T004 Task Envelope

Captured live after `TC-P21-T003-VERIFY = ACCEPTED` and `TC-P21-T003 = ACCEPTED`. `P21-R4 = RESOLVED`. Baseline `14c594c`.

```text
بررسی TC-P21-T003-VERIFY
TC-P21-T003-VERIFY = ACCEPTED
TC-P21-T003 = ACCEPTED


T003 implementation:
2696407


Verification correction:
5824acd


Current HEAD:
14c594c


HEAD == origin/main
Working Tree:
CLEAN

اختلاف Baseline کاملاً توضیح داده شد:

a844bcf
  T002 implementation


a0f5c99
  T002 result/docs bookkeeping only


2696407
  T003 implementation


77a9b8f
  T003 result/docs bookkeeping only


5824acd
  deterministic persistence-test correction only


14c594c
  VERIFY result/docs bookkeeping only

پس هیچ Product Work پنهان یا Scope خارجی بین T002 و T003 وارد نشده است.

R3 نیز با همان معماری قفل‌شده باقی مانده:

Availability authority:
IHotelAvailabilitySource


Named Hotel Supplier:
NONE


Production Availability Source:
NONE


HotelAvailabilityHoldStatus:
- Requested
- Active
- Released
- Expired

و مهم‌تر:

Partial source success != Active Hold


Network timeout / unknown
-> Requested / unresolved


Unresolved Hold
-> unsafe retry blocked


Correctness authority
-> PostgreSQL / persistent state

Validation نهایی:

HotelBooking.UnitTests: 30 PASS
ArchitectureTests: 297 PASS
Persistence.IntegrationTests: 85 PASS
Host.IntegrationTests: 57 PASS
git diff --check: PASS
وضعیت P21
P21-R1 ✅ Module / schema / Place reference
P21-R2 ✅ Stay / rooms / guests / occupancy
P21-R3 ✅ Availability / authoritative source / multi-room hold


P21-R4 ⏳ Commercial rate / monetary snapshot / cancellation terms
P21-R5 ⏳ HotelBooking lifecycle / final reservation / reconciliation
P21-R6 ⏳ Payment integration / compensation
P21-R7 ⏳ Cancellation / amendments / refund policy
P21-R8 ⏳ Public UX / authorization / privacy / supplier readiness
قفل P21-R4
P21-R4 = RESOLVED

در R4 سه حقیقت را از هم جدا می‌کنیم:

Live / offered hotel commercial rate
!=
HotelBooking monetary snapshot
!=
Payment

و همچنین:

Cancellation terms
!=
Cancellation execution
!=
Refund execution

برای P21، ماژول قدیمی Pricing فعلاً خودکار Generalize نمی‌شود، چون در P12 برای TourDeparture قفل شده بود.

بنابراین Baseline هتل:

Authoritative Hotel Commercial Offer
        |
        v
HotelRateOfferSnapshot
        |
        v
HotelBookingMonetarySnapshot

منبع Offer می‌تواند در آینده Supplier یا Internal Inventory/Commercial Source باشد، اما اکنون:

Named Hotel Supplier = NONE
Production Hotel Rate Source = NONE

Rate انتخاب‌شده باید دقیقاً برای همان:

Hotel
Stay dates
Room set
Room occupancy
Availability selections

معتبر باشد.

قیمت بعد از پذیرش Snapshot نیز دیگر از Catalog یا Supplier به‌صورت زنده خوانده نمی‌شود.

Accepted snapshot = immutable transaction truth

اگر Offer منقضی شود یا قیمت تغییر کند:

Silent repricing = FORBIDDEN

و Cancellation Terms نیز در زمان پذیرش Offer Snapshot می‌شوند. حتی اگر Policy منبع بعداً تغییر کند، رزرو قبلی نباید معنایش عوض شود.

یک نکته مهم: در R4 می‌توانیم Cancellation Penalty را Snapshot کنیم، حتی اگر مبلغش بخشی از کل باشد؛ ولی اجرای Partial Refund همچنان تغییر نمی‌کند:

P20 Partial Refund = DEFERRED

یعنی R4 فقط حقیقت قراردادی را ثبت می‌کند. R6/R7 بعداً باید تعارض اجرای Refund جزئی را حل کنند.

Task کامل:

BEGIN_TRAVELCORE_CURSOR_TASK_V1


Protocol-Version:
1


Task-ID:
TC-P21-T004


Phase:
P21


Title:
Hotel commercial rate offer, immutable monetary snapshot, cancellation-policy snapshot, and rate-expiry integrity


Baseline:
14c594c


Decision:
P21-R4 = RESOLVED


Purpose:
Establish the authoritative commercial-rate boundary for HotelBooking and persist
an immutable transaction-time HotelRateOfferSnapshot,
HotelBookingMonetarySnapshot, and CancellationPolicySnapshot.


This task must preserve the separation:


Hotel commercial offer
!=
HotelBooking transaction monetary truth
!=
Payment


and:


CancellationPolicySnapshot
!=
Cancellation execution
!=
Refund execution


Do NOT implement HotelBooking lifecycle/confirmation, final supplier reservation,
Payment integration, HotelBooking cancellation execution, partial Refund, or
public HotelBooking UX.


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


14c594c




2. Record T003 acceptance


Synchronize P21 SoT to record:


TC-P21-T003 = ACCEPTED


including verification/correction lineage where governance records task evidence.


Do not alter accepted R3 semantics.




3. Core commercial boundary


Lock:


HotelRateOffer
!=
HotelBookingMonetarySnapshot


HotelBookingMonetarySnapshot
!=
Payment


HotelRateOffer
!=
Payment




4. Pricing module boundary


Existing P12 Pricing was designed around accepted TourDeparture pricing scope.


Do NOT silently generalize Pricing to HotelBooking in T004.




5. No Pricing code changes


Do NOT modify existing Pricing module merely to make HotelBooking compile.




6. Hotel commercial-rate authority


For P21 baseline, HotelBooking receives an authoritative transaction-time hotel
commercial offer through a provider/source-neutral commercial-rate boundary.


The rate source may later be:


- external Hotel supplier
- TravelCore-owned hotel commercial/allotment source
- another authoritative Hotel rate source


No concrete production source is implemented now.




7. Named source posture


Record:


Named Hotel Supplier = NONE


Production Hotel Rate Source = NONE




8. No fake production price


Do NOT fabricate Hotel commercial prices when no authoritative rate source exists.




9. Rate source port


Introduce a minimal source-neutral contract such as:


IHotelRateOfferSource


or repository-equivalent.




10. Rate source resolver


Introduce a minimal:


IHotelRateOfferSourceResolver


only if needed by existing module composition patterns.




11. Source selection


Rate source selection remains:


server-controlled




12. No smart rate routing


Do NOT implement:


- cheapest supplier selection
- rate-source ranking
- supplier failover
- weighted routing
- arbitrage
- automatic source switching




13. Rate source != availability source


Conceptually preserve:


Rate source responsibility
!=
Availability source responsibility


The same future adapter MAY implement both.


Do not require they be separate physical vendors.




14. Cross-source provenance


A rate offer must explicitly retain enough provenance to know:


- source key
- source offer/reference
- exact HotelBooking/stay/occupancy it applies to


Do not accept anonymous monetary values.




15. Rate request


A rate request must derive authoritative requested structure from HotelBooking:


- HotelPlaceReference
- CheckInDate
- CheckOutDate
- complete RoomReservation set
- AdultCount per room
- Child AgeAtCheckIn values per room
- relevant R3 availability-selection correlations where required


Do not accept a contradictory client occupancy structure.




16. Guest PII minimization


Hotel rate request must NOT contain:


- guest names
- email
- phone
- passport
- national ID
- document scans


unless a future real source demonstrably requires it.


R4 baseline requires occupancy facts only.




17. HotelRateOfferSnapshot


Introduce an immutable HotelBooking-owned transaction-time:


HotelRateOfferSnapshot


or repository-equivalent.




18. Snapshot purpose


It records the exact authoritative commercial offer selected for this
HotelBooking.


It is NOT a mutable live supplier rate.




19. Rate offer identity


Introduce stable:


HotelRateOfferSnapshotId


using UUIDv7 or accepted identity convention.




20. Source offer reference


Persist separately:


RateSourceKey
SourceOfferReference


or repository-equivalent opaque values.




21. SourceOfferReference != snapshot ID


Preserve:


SourceOfferReference
!=
HotelRateOfferSnapshotId




22. Rate-offer binding


HotelRateOfferSnapshot must bind to exactly one:


HotelBookingId




23. Exact hotel binding


It must correspond to the Booking's:


HotelPlaceReference




24. Exact stay binding


It must correspond to the Booking's:


CheckInDate
CheckOutDate




25. Exact room-set binding


The accepted offer must cover every RoomReservation in the Booking.




26. No partial accepted offer


A commercial offer covering only a subset of rooms must NOT become the accepted
HotelRateOfferSnapshot for the complete HotelBooking.




27. Room rate lines


Introduce per-room immutable transaction records such as:


HotelRoomRateSnapshot


or equivalent.




28. Room rate line identity


Each room-rate line correlates to exactly one:


RoomReservationId




29. Complete coverage


There must be exactly one accepted commercial room-rate line per RoomReservation
for the selected offer baseline.




30. No duplicate room line


Do not allow two accepted rate lines for the same RoomReservation within one
HotelRateOfferSnapshot.




31. Availability correlation


Where R3 source provided an opaque availability selection/reference, the room-rate
line may record that correlation.


Do not make R4 the availability authority.




32. Availability selection != rate offer


Preserve:


HotelAvailabilitySelectionReference
!=
HotelRoomRateSnapshot




33. Room catalog authority


Do NOT turn HotelBooking into RoomType catalog owner.




34. Room description


Do NOT snapshot arbitrary catalog content such as:


- images
- full room description
- amenities
- dimensions


in T004.




35. Commercial room facts


Only snapshot commercial facts required to explain what was purchased.


If a source provides an opaque room/rate selection code, preserve it as
transaction provenance.


Do not invent a universal RoomType taxonomy.




36. Rate plan


Do NOT create a global Hotel RatePlan catalog in HotelBooking.




37. Board/meal plan


If authoritative commercial source provides a meal/board basis required to explain
the purchased rate, store it as a minimal immutable commercial fact.


Prefer a source-neutral code/value.


Do not create an entire hotel-meal catalog.




38. Do not invent board taxonomy unnecessarily


Do NOT hardcode a large enum merely from industry conventions unless the accepted
source-neutral contract requires it.




39. HotelBookingMonetarySnapshot


Introduce immutable:


HotelBookingMonetarySnapshot




40. Monetary authority


Once accepted, HotelBookingMonetarySnapshot becomes HotelBooking's authoritative
transaction-time monetary truth.




41. No live recalculation


After snapshot acceptance, HotelBooking must NOT derive transaction amount from:


- mutable Place data
- Search
- current supplier price
- current rate source
- Tour Pricing tables




42. Currency


Use accepted TravelCore:


CurrencyCode


semantics.




43. Toman


Preserve:


Toman != CurrencyCode




44. Money type


Use accepted Money/value conventions.


Do NOT create Hotel-specific money primitive.




45. Precision


Do not use:


float
double


as authoritative monetary storage/calculation.




46. Baseline total


HotelBookingMonetarySnapshot must have an authoritative:


TotalAmount
CurrencyCode


or repository-equivalent.




47. Per-room amount


Each HotelRoomRateSnapshot may retain its authoritative room-level monetary amount
if supplied/needed.


Sum consistency must be enforceable.




48. Monetary consistency


The accepted Booking total must equal the authoritative source offer total or
explicit source-provided breakdown.


Do not reconstruct arbitrary totals independently.




49. Tax/fee boundary


Hotel-specific taxes/fees may be commercially meaningful.


R4 must preserve authoritative source-provided monetary disclosure without making
HotelBooking a tax engine.




50. No tax engine


Do NOT calculate hotel tax rates inside HotelBooking.




51. No fee engine


Do NOT invent hotel service-fee algorithms.




52. Charge breakdown


If the source provides known separate components, snapshot them in a neutral
immutable breakdown structure.


Prefer minimal concepts such as:


HotelChargeComponentSnapshot


only if needed.




53. Charge component authority


Charge components come from authoritative commercial source.


HotelBooking does not calculate them from percentages/rules.




54. Charge component type


Avoid speculative giant enums.


Use only minimal types genuinely required by the accepted offer contract.




55. Pay-now vs pay-at-property


Do NOT implement a second payment execution flow in T004.




56. Baseline payment posture


HotelBookingMonetarySnapshot may distinguish source-provided monetary disclosure
such as:


amount payable through TravelCore
vs
amount explicitly payable at property


ONLY if the source contract supplies this distinction.




57. No fabricated pay-at-property


Do not invent pay-at-property amounts.




58. Payment integration remains R6


Even if a monetary snapshot contains a payable-now amount:


P21-R6 remains OPEN.




59. Deposit/partial collection


Do NOT introduce deposit/partial Payment execution.




60. Multi-currency per Booking


One accepted HotelRateOfferSnapshot / HotelBookingMonetarySnapshot uses exactly one
transaction CurrencyCode baseline.




61. Mixed room currencies


Reject an accepted multi-room offer whose room monetary lines cannot resolve to one
authoritative Booking transaction currency.


Do not perform implicit FX.




62. FX


Do NOT implement currency conversion in HotelBooking.




63. Offer issuance time


Persist:


QuotedAt / OfferedAt


using NodaTime Instant.




64. Offer expiry


Persist:


OfferExpiresAt


or repository-equivalent when the authoritative rate is time-limited.




65. No hardcoded offer TTL


Do NOT invent global:


5/10/15 minute


price expiry.




66. Expiry authority


Offer expiry comes from authoritative rate source/configuration contract.




67. Expired offer


An expired offer cannot become a newly accepted HotelBooking monetary snapshot.




68. Current time


Use accepted:


IClock
NodaTime Instant


for expiry evaluation.




69. Silent repricing forbidden


Preserve:


Expired offer
or
Price changed


must NOT silently replace the accepted monetary values.




70. Different offer


If a new rate is required after expiry/change:


it must be a new explicit offer/snapshot identity.




71. Immutable accepted snapshot


Do NOT overwrite an accepted HotelRateOfferSnapshot with a different amount,
currency, room set, source offer, or cancellation terms.




72. Same offer idempotency


Re-applying the exact same authoritative accepted offer may be idempotent.




73. Conflicting offer


Attempting to replace accepted snapshot with a materially different offer in this
baseline must return conflict/requote-required behavior.


Do not silently mutate.




74. Requote


Full customer requote workflow remains:


DEFERRED / later R5-R8 orchestration




75. Price increase


Never silently accept a higher price.




76. Price decrease


Do not silently mutate the accepted snapshot merely because a later source quote is
lower.


A new accepted quote requires explicit transaction semantics later.




77. Rate freshness != Hold expiry


Preserve:


RateOfferExpiresAt
!=
HotelAvailabilityHold.ExpiresAt




78. Rate freshness != Payment attempt


Preserve:


RateOffer expiry
!=
PaymentAttempt lifecycle




79. Cancellation policy centrality


Every accepted cancellable/non-cancellable commercial hotel offer must retain the
transaction-time cancellation terms required to explain the contract.




80. Cancellation policy snapshot


Introduce immutable:


HotelCancellationPolicySnapshot


or repository-equivalent.




81. Policy ownership


The live commercial source/rate offer is authoritative for offered cancellation
terms.


HotelBooking owns the immutable accepted transaction snapshot.




82. Policy snapshot != live policy


Changes to supplier/property cancellation rules after offer acceptance must not
rewrite an existing Booking snapshot.




83. Cancellation execution


Do NOT implement actual HotelBooking cancellation in T004.




84. Refund execution


Do NOT modify Payment Refund execution in T004.




85. Cancellation snapshot != Refund


Preserve:


HotelCancellationPolicySnapshot
!=
Refund




86. Cancellation snapshot != cancellation request


Preserve:


HotelCancellationPolicySnapshot
!=
HotelBooking cancellation execution




87. Free cancellation


Do NOT model the entire policy as only:


IsFreeCancellation = true/false




88. Policy rules


Support a minimal ordered penalty-rule snapshot sufficient for source-authored
cancellation economics.




89. Concrete penalty amount


Preferred baseline:


each accepted cancellation rule contains an authoritative concrete penalty amount
in the Booking currency.


HotelBooking should not recalculate source percentage policies later from mutable
price data.




90. Zero penalty


Free cancellation window can be represented by:


PenaltyAmount = 0


within the applicable rule/window.




91. Full penalty


Non-refundable or late cancellation can be represented by:


PenaltyAmount = TotalAmount


where that is what the authoritative offer states.




92. Partial penalty fact


A source may authoritatively state a penalty between:


0
and
TotalAmount


This monetary POLICY FACT may be snapshotted.




93. Partial Refund conflict


Explicitly preserve:


P20 Partial Refund execution = DEFERRED


Therefore:


a partial cancellation penalty snapshot
does NOT mean
TravelCore can yet execute a partial Refund.




94. Record dependency


Document this explicitly as a dependency for:


P21-R6 / P21-R7




95. Do not modify P20


No Payment/Refund code change in T004.




96. Penalty range


Require:


0 <= PenaltyAmount <= TotalAmount


for baseline policy rules.




97. Penalty currency


Cancellation penalty currency must equal HotelBookingMonetarySnapshot.CurrencyCode.




98. Cancellation deadline type


Use:


NodaTime Instant


for authoritative cancellation deadlines/cutovers.




99. Property/local timezone


Where source supplies hotel/property timezone required to explain cancellation
terms, persist a valid:


IANA timezone identifier


or accepted DateTimeZone ID string.




100. Server timezone forbidden


Do NOT calculate cancellation deadlines using machine/server local timezone.




101. Display vs authority


Deadline authority is Instant.


Property timezone is presentation/business-context metadata.




102. Cancellation schedule


Allow an ordered set of penalty windows/rules if required by source terms.




103. Overlapping rules


Reject ambiguous overlapping cancellation penalty intervals.




104. Policy gaps


Define deterministic meaning for uncovered periods.


Preferred:
require policy rules to be source-authored and deterministic for the periods where
cancellation is allowed.




105. Non-refundable rate


Model explicitly through immutable policy semantics.


Do not rely solely on display text.




106. No-show policy


No-show execution remains:


DEFERRED


If source commercial terms include a no-show penalty necessary to explain the rate,
it may be stored as immutable policy evidence only.




107. Cancellation free text


Optional supplier/public explanatory text may be retained only if required.


It must NOT be the sole machine-authoritative cancellation rule.




108. Localized policy text


Do not build localization CMS inside HotelBooking.


Structured rule facts remain authoritative.




109. Policy binding


CancellationPolicySnapshot belongs to exactly one accepted:


HotelRateOfferSnapshot / HotelBooking




110. No policy reuse by reference to mutable source


Do not leave transaction semantics dependent solely on a live supplier policy ID.




111. Offer acceptance operation


Introduce an application/domain service such as:


HotelRateOfferAcceptanceService


or repository-equivalent.




112. Acceptance prerequisites


Offer acceptance must verify:


- HotelBooking exists
- HotelPlaceReference matches
- stay dates match
- room set matches
- occupancy/request structure matches
- every room has commercial coverage
- currency is coherent
- offer not expired
- cancellation policy structurally valid




113. R3 hold relationship


Do NOT require HotelBookingStatus.


Where current flow has a HotelAvailabilityHold, rate acceptance must retain
correlation to the exact held/requested room structure.




114. Active Hold requirement


Do NOT prematurely require:


Active Hold


for all commercial offer acceptance unless R3/source semantics prove rate cannot be
quoted before hold.


Rate quote may precede hold in some supplier protocols.




115. Structural matching


Regardless of ordering, rate snapshot must match the same HotelBooking structure
that will later be held/reserved.




116. Booking structural mutation


Once an accepted commercial snapshot exists, prevent silent mutation of:


- HotelPlaceReference
- CheckInDate
- CheckOutDate
- room set
- occupancy


without future explicit requote/amendment semantics.




117. No amendment workflow


Do not implement those future semantics now.


Return conflict/guard failure.




118. HotelBookingStatus


P21-R5 remains OPEN.


Do NOT introduce:


HotelBookingStatus




119. Final supplier reservation


Do NOT introduce:


SupplierReservation
SupplierBookingAttempt
Hotel confirmation number




120. Hotel final confirmation


Do NOT implement.




121. Payment integration


P21-R6 remains OPEN.


Do NOT extend Payment target.




122. Payment obligation contract


Do NOT yet modify P20 Payment contracts.


HotelBookingMonetarySnapshot should be structured so R6 can later expose an
authoritative payment obligation.




123. Refund


Do NOT alter:


RefundStatus
RefundAttempt
full-refund P20 baseline




124. Partial Refund


Do NOT implement.




125. Cancellation execution


P21-R7 remains OPEN.


Do NOT create cancellation command/API.




126. Amendment


Remain DEFERRED.




127. Public API


P21-R8 remains OPEN.


Do NOT expose:


rate search
rate acceptance
HotelBooking public API




128. Frontend


Do NOT implement room/rate selection UI.




129. SEO/Search


No changes.




130. Operational read


Do not create operational dashboard in R4.




131. Rate source secrets


Future rate-source credentials belong secure configuration.


No credentials in repository.




132. Raw source payload


Do not persist raw supplier/rate-provider JSON merely as audit evidence.




133. Provenance


Persist structured provenance:


SourceKey
SourceOfferReference
QuotedAt
OfferExpiresAt where applicable




134. Auditability


The accepted snapshot must be sufficient to answer later:


- what Booking/stay was priced
- which rooms were covered
- what total/currency was accepted
- which source/offer produced it
- when it was quoted
- when it expired
- what cancellation terms were accepted




135. Catalog mutation safety


Later Place/room-catalog changes must not change monetary/cancellation history.




136. Availability mutation safety


Later source availability changes must not rewrite accepted commercial snapshot.




137. Persistence tables


Create minimum R4 persistence using repository naming conventions.


Expected conceptual tables:


hotel_booking.hotel_rate_offer_snapshots


hotel_booking.hotel_room_rate_snapshots


hotel_booking.hotel_booking_monetary_snapshots


hotel_booking.hotel_cancellation_policy_snapshots


hotel_booking.hotel_cancellation_penalty_rules


Adjust names to existing conventions.




138. Same-schema FKs


Allowed inside:


hotel_booking




139. No Pricing FK


No FK to Pricing schema.




140. No Place FK


PlaceId remains logical.




141. No Payment FK


No Payment integration yet.




142. No external-source FK


Source references remain opaque.




143. Monetary DB precision


Use accepted PostgreSQL numeric precision conventions.


No floating-point money.




144. Currency DB constraint


Use accepted CurrencyCode persistence conventions.




145. Offer unique acceptance


Protect:


one accepted HotelRateOfferSnapshot
per HotelBooking baseline


with DB-backed uniqueness where practical.




146. Monetary snapshot uniqueness


Protect:


one HotelBookingMonetarySnapshot
per HotelBooking baseline.




147. Snapshot source integrity


Monetary snapshot must correspond to its accepted RateOfferSnapshot.




148. Room coverage integrity


Persist same-schema relationship ensuring rate room lines point to Booking's
RoomReservations.




149. Cancellation policy integrity


Policy snapshot belongs to accepted offer/Booking.




150. Penalty rule order


Persist deterministic order/effective interval.




151. Idempotency


Offer acceptance must be DB-backed/idempotent for repeated same authoritative offer.




152. Concurrent acceptance


Two concurrent attempts to accept different commercial offers for one HotelBooking
must not both become accepted.




153. Concurrency authority


Do not rely on process-local locks.




154. Same offer concurrency


Concurrent acceptance of same offer must converge safely.




155. Different offer concurrency


Expected:


one accepted snapshot
other attempt conflicts/requote-required




156. Expired offer concurrency


An offer crossing its ExpiresAt during acceptance must not be accepted based on
stale precheck alone.


Use transaction/current-clock validation appropriate to architecture.




157. Architecture guardrails


Add tests proving:


HotelRateOfferSnapshot != HotelBookingMonetarySnapshot


HotelBookingMonetarySnapshot != Payment


HotelCancellationPolicySnapshot != Refund


Pricing module is not generalized/modified


Named Hotel Supplier = NONE


Production Hotel Rate Source = NONE


No fake production price source


No HotelBookingStatus


No SupplierReservation


No Payment integration


No Partial Refund


No cancellation execution


No public API/UI


No peer-schema FK


No shared DbContext


No peer Infrastructure dependency




158. Unit tests: rate offer


Cover:


- exact hotel/stay/rooms accepted
- mismatched HotelPlace rejected
- mismatched dates rejected
- missing room coverage rejected
- duplicate room line rejected
- same offer idempotent
- different offer conflict




159. Unit tests: money


Cover:


- accepted total/currency preserved
- no float/double authority
- mixed room currencies rejected
- room/offer total consistency
- Toman not used as CurrencyCode




160. Unit tests: expiry


Cover:


- valid unexpired offer accepted
- expired offer rejected
- no hardcoded TTL
- offer expiry uses Instant/IClock




161. Unit tests: repricing


Cover:


- higher replacement offer not silently accepted
- lower replacement offer not silently accepted
- accepted snapshot remains immutable




162. Unit tests: cancellation policy


Cover:


- zero-penalty free window
- full-penalty rule
- valid partial penalty fact can be snapshotted
- penalty < 0 rejected
- penalty > total rejected
- currency mismatch rejected
- overlapping/ambiguous penalty windows rejected
- deadlines use Instant




163. Partial refund architecture test


Explicitly prove:


partial cancellation penalty snapshot exists as contract evidence if source
supplies it


but


Partial Refund execution remains absent.




164. PII test


Hotel rate request/snapshots must contain no:


- email
- phone
- guest names
- passport
- national ID
- card details




165. Persistence tests


Cover:


- accepted rate-offer round-trip
- multi-room rate lines round-trip
- monetary snapshot round-trip
- cancellation-policy rules round-trip
- Money precision
- CurrencyCode
- Instant precision
- source provenance
- no Place/Pricing/Payment FK




166. Persistence concurrency


Two concurrent different offers for same HotelBooking:


at most one accepted.




167. Persistence idempotency


Repeated same offer acceptance:


same effective snapshot identity/result.




168. Persistence expiry


Expired source offer cannot be committed as accepted.




169. Host tests


Host continues to start with:


Production Hotel Rate Source = NONE




170. No public endpoint


Verify no HotelBooking/rate public endpoint exists.




171. No frontend


Frontend remains untouched.




172. Source-of-Truth synchronization


Update authoritative P21 docs to record:


TC-P21-T003 = ACCEPTED
P21-R4 = RESOLVED




173. R4 decision summary


Record exactly:


- hotel commercial rate source is authoritative for live offered terms
- HotelBooking owns immutable accepted HotelRateOfferSnapshot
- HotelBooking owns immutable HotelBookingMonetarySnapshot
- HotelBooking does not calculate live hotel price/tax/fee
- existing Pricing module is not generalized in R4
- accepted offer covers exact Hotel/Stay/Room/Occupancy structure
- multi-room offer must completely cover all RoomReservations
- one accepted commercial snapshot per HotelBooking baseline
- transaction uses one CurrencyCode baseline
- no implicit FX
- offer quote/expiry use Instant
- expired offer cannot be silently accepted
- silent repricing is forbidden
- different new offer requires future explicit requote semantics
- HotelCancellationPolicySnapshot is immutable transaction truth
- cancellation deadlines use Instant
- structured concrete penalty amounts may include zero/full/partial penalty facts
- partial penalty FACT does not imply Partial Refund execution
- P20 Partial Refund remains DEFERRED
- cancellation execution remains P21-R7
- Payment integration remains P21-R6
- Named Hotel Supplier = NONE
- Production Hotel Rate Source = NONE




174. Decision status


Record:


P21-R4 = RESOLVED




175. Remaining decisions


Keep:


P21-R5 through P21-R8 = OPEN




176. P21 status


Remain:


IN_PROGRESS




177. T005


Do NOT execute:


TC-P21-T005




Allowed:


- IHotelRateOfferSource
- rate-source resolver
- source-neutral commercial-rate contracts
- HotelRateOfferSnapshot
- HotelRateOfferSnapshotId
- HotelRoomRateSnapshot
- HotelBookingMonetarySnapshot
- minimal charge breakdown if source-authoritative
- offer quoted/expiry Instants
- source provenance
- HotelCancellationPolicySnapshot
- cancellation penalty rules as immutable policy evidence
- offer acceptance/idempotency/concurrency
- HotelBooking-local migrations
- unit/architecture/persistence/host tests
- SoT synchronization for R4




Forbidden:


- P21-R5 through R8 decisions
- Pricing module generalization
- Pricing module code changes
- HotelBookingStatus
- SupplierReservation
- SupplierBookingAttempt
- final HotelBooking confirmation
- named supplier
- supplier SDK
- production fake price source
- implicit FX
- silent repricing
- live mutable cancellation-policy dependency
- HotelBooking cancellation execution
- Payment target extension
- Payment code change
- Refund code change
- Partial Refund implementation
- public rate/HotelBooking API
- frontend HotelBooking UX
- generic Booking abstraction
- shared DbContext
- peer-schema FK
- peer Infrastructure dependency
- unrelated refactor
- dependency upgrade




Done:


- hotel commercial-rate authority is explicit
- no fake price truth exists
- provider-neutral rate source boundary exists
- accepted multi-room commercial offer is immutable
- accepted offer matches exact HotelBooking structure
- HotelBookingMonetarySnapshot is authoritative transaction truth
- no live Pricing/supplier recalculation occurs after acceptance
- offer expiry is explicit and source-authoritative
- silent repricing is impossible
- cancellation terms are immutable structured snapshots
- partial penalty facts can be represented without implementing Partial Refund
- P20 Partial Refund remains untouched/deferred
- no HotelBooking lifecycle/supplier reservation/payment/cancellation/public
  capability leaks into R4
- P21-R4 is recorded RESOLVED
- P21-R5 through P21-R8 remain OPEN




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


- commercial rate authority
- rate source port exact name
- Named Hotel Supplier
- Production Hotel Rate Source
- production fake rate source: NO
- Pricing module modified/generalized: NO
- HotelRateOfferSnapshot type
- HotelBookingMonetarySnapshot type
- accepted offer complete-room coverage
- accepted-offer uniqueness
- monetary CurrencyCode rule
- mixed room currencies result
- Money precision type/storage
- QuotedAt type
- OfferExpiresAt type/source
- hardcoded rate TTL: NO
- expired offer result
- silent higher repricing: NO
- silent lower repricing: NO
- same offer idempotency
- different offer conflict behavior
- cancellation snapshot type
- cancellation deadline type
- property timezone posture
- zero penalty representation
- full penalty representation
- partial penalty fact representable: YES/NO
- Partial Refund execution implemented: NO
- P20 Refund changed: NO
- HotelBookingStatus: NO
- final SupplierReservation: NO
- Payment integration/change: NO
- HotelBooking cancellation execution: NO
- public HotelBooking/rate API/UI: NO
- peer-schema FK: NO
- shared DbContext: NO
- peer Infrastructure dependency: NO
- P21-R4: RESOLVED
- P21-R5 through P21-R8: OPEN
- TC-P21-T005: NOT EXECUTED




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


- commit with TC-P21-T004 in commit message
- push main to origin/main using normal fast-forward push
- re-fetch origin
- verify HEAD == origin/main
- verify Working Tree CLEAN




Expected Baseline:
14c594c




Auto-Execute:


After PASS:


- return TC-P21-T004 RESULT to architect
- do NOT execute TC-P21-T005 until T004 is architect ACCEPTED
- do NOT invent P21-R5 through P21-R8
- remain in PIPELINE




END_TRAVELCORE_CURSOR_TASK_V1
``` 

