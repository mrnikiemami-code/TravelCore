# TC-P21-PLAN Task Envelope (architect, live)

Captured from the same ChatGPT tab after TC-P20-GATE RESULT.

```text
TC-P20-GATE = ACCEPTED
Gate Commit: fc41756
SoT / docs HEAD: 96be199
HEAD == origin/main
Working Tree: CLEAN
P20 = COMPLETE
```

Executable task:

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version:
1

Task-ID:
TC-P21-PLAN

Phase:
P21

Title:
Hotel Booking Architecture and Implementation Plan

Baseline:
96be199

Purpose:
Define the authoritative architecture and implementation plan for P21 Hotel Booking
before any Hotel Booking product implementation begins.

P21 must preserve the already locked TravelCore distinction:

Hotel Catalog
!=
HotelBooking

and must determine the transactional ownership, inventory/availability boundary,
room/rate selection model, guest snapshot, pricing/quote relationship, booking
lifecycle, supplier/external inventory posture, cancellation boundary, Payment
integration posture, public UX, authorization, operational visibility, and
future-provider readiness.

This task is architecture/planning/documentation only.

No Hotel Booking product code.

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

96be199

2. Read authoritative Source of Truth

Inspect at minimum:

- docs/PROJECT-STATE.md
- docs/ROADMAP.md
- architecture constitution
- domain map
- module ownership matrix
- dependency rules
- communication rules
- data architecture
- persistence architecture
- accepted ADRs
- Money/Currency ADR
- temporal/NodaTime ADR
- P07 Place
- any existing Hotel Catalog ownership docs
- P12 Pricing
- P19 Tour Booking
- P20 Payment
- Search
- SEO
- PublicExperience
- Party / Identity / Access
- accepted outbox/inbox/idempotency conventions
- any existing HotelBooking placeholders/references
- any external hotel/provider references already in repository

3. Confirm phase identity

P21 is:

Hotel Booking

Do not redesign it into:

- Hotel Catalog
- general accommodation CMS
- live Flight
- Tour Booking
- generic Reservation platform

4. Preserve critical architecture split

Lock as existing invariant:

Hotel Catalog
!=
HotelBooking

Hotel Catalog / Place-like hotel facts own descriptive/accommodation catalog truth.

HotelBooking owns hotel reservation transaction facts.

5. Determine Hotel Catalog owner

Inspect current SoT and determine the actual authoritative owner of hotel descriptive
facts.

Possibilities may involve:

Place
or
a specific Hotel Catalog module already defined by SoT.

Do NOT guess.

Report exact owner.

6. HotelBooking module candidate

Determine authoritative P21 module/schema ownership.

Expected candidate:

Independent HotelBooking module

schema candidate:

hotel_booking

But do not lock if SoT explicitly defines another schema/name.

7. HotelBooking target

Determine what HotelBooking reserves.

Possible concepts to analyze:

- Hotel / Accommodation logical reference
- RoomType
- RatePlan
- StayOffer
- StayInventoryUnit

Do not prematurely assume an external-provider schema.

8. Product vs transaction

Preserve:

Hotel descriptive product
!=
HotelBooking transaction

9. HotelBooking vs Tour Booking

Preserve:

HotelBooking
!=
Booking module Tour Booking

Do not reuse Tour Booking aggregate blindly.

Analyze what concepts can be shared only through contracts/value objects.

10. Generic Booking abstraction

Do NOT create a universal:

Booking<T>

or generic Booking platform merely because Tour Booking exists.

P21 must justify any shared primitive explicitly.

11. Stay dates

Hotel booking fundamentally involves:

CheckInDate
CheckOutDate

Use accepted LocalDate/NodaTime semantics.

12. Stay-night semantics

Define canonical rule for nights.

Expected:

Nights
=
CheckOutDate - CheckInDate

CheckOutDate must be after CheckInDate.

Do not store ambiguous inclusive-night counts without rule.

13. Property timezone

Analyze whether HotelBooking needs:

property IANA timezone

for:

- cancellation deadlines
- check-in cutoff
- stay date boundaries
- supplier synchronization

Do not use server-local time.

14. Hotel reference

HotelBooking references Hotel/Accommodation catalog logically.

No peer-schema FK.

15. Room type boundary

Determine authoritative owner of RoomType-like catalog facts.

Could be Hotel Catalog/Place-related module or a Hotel Booking-owned supplier offer.

Explicitly distinguish:

RoomType catalog
vs
bookable room/rate offer

16. Rate plan boundary

Determine whether RatePlan is:

catalog fact
commercial offer
supplier offer
Pricing concept

Do not mix these.

17. Availability concept

Define what “available room” means in P21.

Possible sources:

- TravelCore-owned inventory
- supplier/external provider inventory
- static allotment
- request-to-book

Do not assume one until repository/business context supports it.

18. Inventory ownership decision

Explicitly investigate:

Does HotelBooking own room availability consumption?

Or does a future Hotel Inventory/Supplier capability own it?

This must be a named P21-R# decision.

19. No fake availability

Do not invent room availability truth if no authoritative source exists.

20. External supplier posture

Inspect SoT for any existing hotel provider/supplier integration requirement.

If no provider is selected:

do NOT invent Booking.com / Expedia / Hotelbeds / WebBeds / local supplier.

21. Named supplier

Expected current planning posture if SoT is silent:

Named Hotel Supplier = NONE

But verify before recording.

22. Supplier-neutral architecture

If external hotel inventory is anticipated, plan provider-neutral contracts.

Do not implement provider adapters in PLAN.

23. HotelBooking transaction lifecycle

Determine minimum HotelBookingStatus.

Do NOT simply clone Tour Booking:

Pending / Confirmed / Cancelled

without analyzing hotel-specific semantics such as:

- Pending
- Confirmed
- Failed
- Cancelled
- Expired
- Requested

Keep supplier status separate from TravelCore domain status.

24. Confirmation authority

Define what makes HotelBooking truly Confirmed.

Potentially:

- TravelCore-owned inventory confirmation
or
- authoritative supplier confirmation

Preserve:

PaymentSucceeded
!=
HotelBookingConfirmed

25. Supplier confirmation boundary

If supplier inventory is external:

browser/client response
!=
HotelBooking confirmation

Only authoritative supplier evidence may confirm supplier-backed booking.

26. Hold/reservation concept

Investigate whether P21 requires:

HotelReservationHold

or equivalent temporary availability hold before payment/confirmation.

Do not copy Tour CapacityHold automatically.

27. Hold necessity

Analyze flows such as:

availability check
->
rate selection
->
temporary hold
->
payment
->
supplier confirmation

versus:

payment authorization
->
supplier booking

Do not lock without explicit decision.

28. Oversell risk

Plan how HotelBooking prevents:

room oversell
double booking
stale availability

under whichever inventory authority is accepted.

29. Availability snapshot

Determine whether a transaction-time availability/offer snapshot is required.

30. Hotel pricing boundary

Preserve:

Hotel price
!=
HotelBooking monetary snapshot
!=
Payment

31. Pricing module applicability

Determine whether existing Pricing module is authoritative for HotelBooking quote,
or whether external supplier rates are authoritative commercial offers.

This is critical.

32. Generic Pricing target

P12 Pricing initially targeted TourDeparture.

Do NOT assume it already supports HotelBooking.

Plan whether P21 must:

- extend Pricing target contract
- introduce HotelBooking-specific quote input
- consume supplier-authored rate offer

without making HotelBooking calculate price.

33. RateQuote distinction

Explicitly distinguish concepts such as:

HotelRateOffer
HotelQuote
HotelBookingMonetarySnapshot

Names may vary.

34. Price authority

Client must never be authoritative for:

- room price
- taxes
- fees
- board supplements
- cancellation penalty
- currency

35. Taxes and fees

Determine ownership of hotel-specific:

- tax
- service fee
- city tax
- resort fee
- pay-at-property charge

Do not bury all into one total if semantics matter.

36. Pay-now vs pay-at-property

Analyze whether P21 baseline supports:

PayNow
PayAtProperty

Do not automatically support both.

This likely requires an explicit decision.

37. Deposit/partial payment

Explicitly determine:

partial/deposit payment
=
IN / OUT / DEFERRED

Do not introduce unless required.

38. Currency

Reuse accepted Money/Currency semantics.

Preserve:

Toman != CurrencyCode

39. FX

Do not implement HotelBooking FX unless already explicitly required.

Use existing Pricing/Payment boundaries.

40. Guest composition

Define HotelBooking guest model.

Distinguish:

planner traveler intent
!=
Tour Booking passenger
!=
HotelBookingGuest

41. HotelBookingGuest

Determine minimum transaction-time guest data.

Possible:

- lead guest
- adult/child categories
- names

Avoid passport/document PII unless necessary.

42. Room occupancy

Hotel booking requires mapping guests to room(s).

Plan explicit:

RoomReservation
or equivalent child concept.

43. Multi-room booking

Determine whether P21 baseline supports:

one Booking with multiple rooms

This is a critical scope decision.

44. Same room type vs mixed room types

If multi-room is supported, determine whether one HotelBooking may contain:

- same RoomType repeated
- mixed RoomTypes
- mixed RatePlans

Do not assume.

45. Occupancy per room

Model occupancy at room-reservation level, not only aggregate traveler count, if
multi-room is in scope.

46. Child age

Hotel rate eligibility often depends on child age.

Determine whether exact child age or birth date is required.

Prefer minimal age-at-stay facts over full DOB unless business rule requires DOB.

47. Infant semantics

Explicitly defer or define if applicable.

Do not inherit Tour traveler categories blindly.

48. Lead guest/contact

Distinguish:

HotelBookingGuest
vs
HotelBookingContactSnapshot

49. Party linkage

Optional logical Party/Actor reference may exist.

HotelBooking must not become Party master-data owner.

50. Anonymous-first posture

Determine whether HotelBooking public flow supports anonymous initiation like Tour
Booking.

Inspect P19/P20 UX and SoT.

51. Access credential

If anonymous HotelBooking exists, decide whether it uses a HotelBooking-scoped
credential analogous to Tour Booking.

Do not reuse Tour Booking token across unrelated aggregate.

52. PII minimization

No unnecessary:

- passport number
- national ID
- card data
- health data
- scans

53. Special requests

Analyze whether hotel special requests belong in baseline:

- late arrival
- bed preference
- smoking/non-smoking
- accessibility request

Likely unguaranteed request text/structured fields.

Do not treat supplier acknowledgement as guarantee unless verified.

54. Amenities

Hotel amenities remain catalog facts.

HotelBooking may snapshot only selected commercial stay facts if needed.

55. Meal/board plan

Determine whether:

RoomOnly
Breakfast
HalfBoard
FullBoard
AllInclusive

is a catalog/rate-offer fact.

Do not create free-form duplicated truth.

56. Cancellation policy

Hotel cancellation policy is central.

Explicitly separate:

CancellationPolicy
!=
CancellationExecution
!=
Refund

57. Policy authority

Determine who owns cancellation terms:

- supplier/rate offer
- Hotel Catalog
- Pricing
- HotelBooking snapshot

58. Cancellation snapshot

Likely HotelBooking must snapshot accepted cancellation terms at booking time.

Analyze and create explicit P21-R#.

59. Cancellation deadline

Use property/local timezone or explicit Instant conversion.

Do not use server local time.

60. Cancellation penalty

Do not calculate from live mutable policy after booking.

If supported, transaction-time policy snapshot must be authoritative.

61. Free cancellation

Model as policy fact, not Boolean-only if multiple deadlines/penalties exist.

62. No-show

Determine whether no-show policy is in P21 baseline or deferred.

63. Amendments

Explicitly determine:

date change
room change
guest change
=
DEFERRED unless required.

64. HotelBooking cancellation lifecycle

Determine when HotelBooking may be cancelled and by whom.

65. Confirmed cancellation

Unlike Tour Booking P20 where Confirmed cancellation remains deferred, HotelBooking
may require cancellation as a core hotel feature.

Do NOT assume.

Plan this as explicit decision.

66. Refund interaction

If cancellation after payment exists:

HotelBooking decides cancellation business outcome.

Payment executes monetary refund.

Preserve:

HotelBookingCancelled
!=
PaymentRefunded

67. Payment integration

Determine whether P21 should reuse P20 Payment with a new HotelBooking payment target.

Current P20 target is Booking/Tour Booking.

Do NOT silently generalize.

68. Payment target extension

If HotelBooking will use Payment, planning must define safe evolution:

Payment target abstraction
or
separate contract extension

without breaking accepted P20 one-Booking-one-Payment semantics.

69. Generic Payment universalization

Do not turn Payment into arbitrary TargetType+TargetId unless P21 demonstrates the
need and architecture remains safe.

This is a major decision.

70. Payment obligation contract

HotelBooking must provide authoritative monetary obligation if Payment integration
is in P21 scope.

71. Payment success integration

Preserve:

PaymentSucceeded
!=
HotelBookingConfirmed

HotelBooking re-evaluates its own confirmation prerequisites.

72. Payment ordering

Analyze supplier-backed scenarios:

A. hold supplier -> pay -> confirm supplier
B. pay -> book supplier
C. book supplier -> pay

Financial compensation differs materially.

Plan explicitly.

73. Successful payment / supplier booking failure

Critical scenario:

Payment succeeds
+
Hotel supplier reservation fails

Must have explicit financial recovery/Refund posture.

74. Supplier booking success / payment failure

Critical scenario:

Supplier confirms room
+
Payment fails

Must define release/cancel/expiry behavior.

75. Distributed transaction

No distributed DB/network transaction with external supplier or Payment.

76. Saga/process manager

Determine whether HotelBooking needs an explicit process manager/orchestrator.

Do not create a generic workflow engine.

77. Transaction state vs Booking status

If orchestration steps are needed, keep process state separate from business
HotelBookingStatus where appropriate.

78. Exactly-once

External supplier/payment calls are not exactly-once.

Plan:

idempotency
+
reconciliation
+
compensation

79. Supplier booking idempotency

Plan duplicate-safe supplier reservation creation.

80. Supplier reservation reference

External supplier confirmation/reference:

!=
HotelBookingId

81. HotelBooking identity

Use UUIDv7.

82. Supplier status translation

Provider-specific reservation status must not leak into HotelBooking domain status.

83. Supplier callback/webhook

If future suppliers support callbacks:

UnverifiedSupplierCallback
!=
HotelBookingConfirmed

84. Supplier query/recheck

Plan authoritative status query/reconciliation capability.

85. Supplier timeout

NetworkTimeout
!=
BookingFailed

if reservation outcome is ambiguous.

86. Duplicate supplier reservation risk

Plan safeguards against retrying ambiguous booking requests and creating duplicate
hotel reservations.

87. HotelBooking reconciliation

Plan minimal reconciliation for:

- supplier says confirmed/local pending
- local expected reservation/supplier unknown
- payment succeeded/supplier failed
- supplier confirmed/payment unresolved
- cancellation uncertainty

88. Reconciliation != CRM

Do not create manual ticket/workflow suite.

89. HotelBooking audit facts

Plan immutable operational evidence:

- HotelBookingId
- supplier reference
- accepted rate snapshot
- cancellation policy snapshot
- lifecycle transitions
- payment correlation

90. Public hotel booking journey

Plan likely route(s).

Examples only:

/[locale]/hotels/[slug]/book

or:

/[locale]/hotels/[slug]/rooms/[offerId]/book

Do not invent until current hotel public route architecture is inspected.

91. Hotel public catalog route

Inspect actual current Hotel/Place frontend routes.

Preserve them.

92. Transactional private route

Plan private route such as:

/[locale]/hotel-bookings/[bookingId]

or repository-equivalent.

93. SEO

Hotel catalog pages may be indexable.

HotelBooking transactional pages:

noindex

94. Search

Search may index hotel catalog/search documents.

Search must NOT index private HotelBooking transactions.

95. Public API

Plan behavior-oriented APIs only.

No generic CRUD.

96. Hotel availability API

Determine whether availability search belongs to:

HotelBooking
Hotel Catalog/Search
supplier adapter

Explicitly decide.

97. Availability query vs booking command

Separate read/discovery:

availability/rate search

from transactional command:

reserve/book.

98. Search ownership

Do not make Search the source of live hotel availability.

99. SEO ownership

SEO remains canonical/index policy owner.

100. PublicExperience

Determine whether PublicExperience composes hotel booking UX or a Hotel-specific
presentation boundary is more appropriate.

Do not move transaction truth out of HotelBooking.

101. Server Component First

Preserve frontend architecture.

102. Mobile-first

Hotel room selection/booking must be mobile-first.

103. Accessibility

Plan:

- room/rate selection semantics
- price/cancellation disclosure
- keyboard support
- error/status accessibility

104. RTL/LTR/bidi

FA/EN/AR.

Direction-neutral.

105. Cancellation terms UX

Critical commercial terms must be presented before confirmation/payment.

Do not hide cancellation penalty in secondary detail.

106. Total price disclosure

Plan how customer sees:

- stay total
- taxes/fees
- pay now
- pay later

depending accepted scope.

107. Per-night display

Per-night breakdown may be presentation detail.

Authoritative booking snapshot should retain required pricing facts.

108. Occupancy UX

Room-specific occupancy must be clear.

109. Multi-room UX

If IN, mobile selection flow must avoid confusing guest assignment.

110. Accessibility of room choice

Room cards/options should be semantically selectable, not div-only click targets.

111. HotelBooking operational read

Plan read-only operational/support query.

No manual status mutation.

112. Manual confirmation

Do NOT allow operator:

ForceHotelBookingConfirmed

without authoritative supplier/inventory evidence.

113. Manual cancellation

Do not invent support status mutation bypass.

114. Supplier retry

Operational recheck may query supplier.

Operator should not choose authoritative outcome.

115. Secrets

Future supplier credentials belong secure configuration.

No secrets in repo.

116. Supplier raw payload

Avoid persisting raw sensitive/provider payload by default.

117. Guest privacy

Operational read should minimize guest PII.

118. Notification

HotelBooking may emit events.

Notification remains delivery/provider owner.

119. Content

Hotel descriptive/editorial content remains Content/Hotel Catalog owner.

HotelBooking does not own editorial hotel descriptions.

120. UGC

Hotel reviews remain UGC target if accepted.

HotelBooking does not own review facts.

121. Visa

HotelBooking != VisaApplication.

122. TripPlanner

TripPlanner intent/Lead != HotelBooking.

123. AgencyMarketplace

Analyze whether agencies may originate HotelBooking in baseline.

Do not automatically introduce agency settlement/commission.

124. Direct vs Agency

If HotelBooking supports agency origin, determine whether same aggregate is used.

Do not invent separate AgencyHotelBooking without evidence.

125. Commission

Agency/hotel commission settlement is OUT/DEFERRED unless existing SoT says otherwise.

126. Payment provider vs hotel supplier

Keep distinct:

PaymentProvider
!=
HotelSupplier

127. Hotel supplier settlement

Do not implement supplier payout/settlement/accounting.

128. Inventory model alternatives

Plan should explicitly compare at least:

A. TravelCore-owned allotment/inventory
B. External supplier authoritative live inventory
C. hybrid

and state accepted/deferred posture based on SoT.

129. Static hotel catalog without live inventory

If repository currently has no live inventory source, do not fake “Book Now”
availability.

Potential baseline may need:

request-to-book
or
provider-ready architecture

but do not choose without analysis.

130. Request-to-book

Analyze whether request-to-book is required.

Do not conflate:

request
with
confirmed reservation.

131. HotelBookingStatus if request-to-book exists

Do not overload Pending ambiguously.

Define semantics precisely.

132. Confirmation number

Determine whether HotelBooking needs an internal human-readable reference separate
from UUIDv7.

133. Supplier confirmation number

Keep supplier confirmation code separate from internal booking reference.

134. Voucher

Determine whether hotel voucher generation belongs to P21 or deferred.

Do not add PDF/document capability without need.

135. Check-in instructions

Determine whether supplier-confirmed instructions belong in booking snapshot/read
model.

136. Local hotel booking

Consider whether future direct/local hotels differ from external supplier bookings.

Do not fork aggregate unless necessary.

137. Inventory source abstraction

If needed, define source kind/provider-neutral boundary.

Avoid giant generic provider abstraction.

138. Room inventory identity

Do not persist external ephemeral OfferId as domain primary identity without
snapshot/correlation semantics.

139. Rate freshness

Hotel rates may expire.

Plan explicit:

RateOfferExpiresAt

or equivalent if external dynamic pricing is accepted.

140. Stale rate handling

If selected offer expires before booking:

do not silently reprice.

Require revalidation/requote.

141. Repricing

Explicitly define customer consent requirement when supplier price changes.

No silent amount increase.

142. Price decrease

Determine whether supplier lower price can be accepted automatically or requires
new snapshot.

Do not decide casually.

143. Quote expiry

Distinguish:

Hotel rate/offer expiry
!=
Payment attempt expiry
!=
inventory hold expiry

144. Cancellation policy expiry

Cancellation terms accepted at booking time remain immutable snapshot.

145. Occupancy validation

Authoritative booking logic must validate requested guests against accepted room
occupancy rules.

146. Room count availability

Availability check must account for requested quantity of rooms.

147. Children rules

Hotel child policies may vary by property/rate.

Plan owner/source; do not hardcode platform-wide ages.

148. Extra bed

Determine whether extra-bed request is baseline or deferred.

149. Pricing per room vs per person

Hotel pricing must not inherit Tour per-person pricing semantics blindly.

150. Taxes per stay vs per person/night

Model only if required by authoritative supplier/rate data.

151. Rate inclusions

Plan immutable accepted rate inclusion snapshot if needed:

breakfast
tax inclusion
cancellation
payment terms

152. Booking confirmation snapshot

At confirmation, HotelBooking should retain enough immutable facts to explain what
was booked even if catalog/rate changes later.

153. Catalog mutation

Hotel name/room description changes later must not corrupt transaction audit.

154. Localization snapshot

Determine whether localized display labels need snapshot or stable catalog reference
is sufficient.

Do not duplicate unnecessary text.

155. HotelBooking state history

Plan whether status transition history is required.

Prefer auditable transitions for supplier/payment operations.

156. Domain events

Plan only necessary events.

Possible examples:

HotelBookingCreated
HotelBookingConfirmed
HotelBookingCancelled

Do not lock unnecessary taxonomy.

157. Outbox

Use accepted transactional outbox.

158. Inbox

Supplier/payment cross-module messages must be idempotent.

159. Payment reuse decision

Create explicit decision for whether P21 extends P20 Payment to HotelBooking.

This should likely be a named P21-R#.

160. Refund reuse

If HotelBooking uses Payment, refunds should use existing Payment Refund execution
rather than a HotelBooking-specific money-transfer implementation.

161. Refund policy owner

HotelBooking owns cancellation/compensation amount decision.

Payment owns refund execution.

162. Partial hotel refund

Hotel cancellation penalties may make partial refunds necessary.

This is architecturally significant because P20 currently defers Partial Refund.

P21 PLAN must explicitly identify this conflict.

Do NOT silently implement partial refund.

163. Critical source-of-truth conflict analysis

If hotel cancellation requires partial refund but Payment currently only supports
full Refund:

record this as an explicit architecture dependency/decision.

Do not hide it.

164. P20 invariant preservation

Do not mutate accepted P20 semantics during PLAN.

Any future Payment extension must be a scoped P21 task/decision and preserve Tour
Booking behavior.

165. Payment target architecture options

Evaluate options such as:

A. generalize Payment target safely
B. introduce separate PaymentObligationReference abstraction
C. module-specific external payment correlation

Do not implement in PLAN.

166. Hotel booking/payment ordering matrix

Document expected behavior for:

availability held + payment success
availability held + payment failure
payment success + hotel reservation failure
hotel reservation success + payment failure
ambiguous supplier reservation
ambiguous payment

167. Compensation matrix

For each failed combined state, identify which module owns corrective decision.

168. Cancellation/payment matrix

Document:

HotelBooking cancellation decision
vs
Refund amount decision
vs
Payment refund execution

169. Supplier cancellation

If supplier confirms cancellation separately, determine authority.

170. Local cancellation truth

HotelBookingStatus should not become Cancelled merely because browser requested it
if supplier reservation remains active.

171. Cancellation pending state

Analyze whether supplier-backed cancellation needs a separate process state rather
than BookingStatus.

172. No status explosion

Avoid polluting HotelBookingStatus with every integration step.

173. Process state model

If needed, plan a separate orchestration/process entity.

174. Supplier reservation entity

Determine whether external reservation should be a child/entity separate from
HotelBooking.

175. Multiple supplier attempts

If supplier booking attempt fails and is retried, decide whether history is kept
like PaymentAttempt.

176. Duplicate hotel booking prevention

One customer request/idempotency key must not create multiple supplier reservations.

177. Public duplicate-submit

Plan server-side idempotency.

178. Hotel booking anonymous token

If anonymous public HotelBooking is accepted, raw token must not be in URL.

179. Private route access

Object-level authorization.

180. Enumeration protection

No public HotelBooking list by anonymous token.

181. Payment data privacy

Do not expose Payment internals through HotelBooking public read unnecessarily.

182. Supplier data privacy

Do not expose supplier secrets/internal diagnostics publicly.

183. Error mapping

Customer-safe states must distinguish:

- no availability
- price changed
- payment unavailable
- reservation pending
- booking confirmed
- compensation/refund in progress

without leaking raw provider states.

184. AI readiness

Structure Hotel booking facts for future AI use:

- attributable
- locale-aware
- stable identifiers
- explicit source/provenance
- normalized occupancy/rate/cancellation facts

Do not introduce:

LLM
embeddings
vector DB
RAG

185. Analytics

Do not create analytics warehouse/event platform in P21.

186. Performance

Availability search may be high read-volume.

Do not use transactional HotelBooking tables as discovery search index.

187. Caching

Supplier live availability caching must not become authoritative beyond defined TTL.

Plan if needed.

188. Availability freshness

Expose freshness/expiry semantics if cached supplier availability is used.

189. Multi-instance correctness

Booking/hold/idempotency correctness must be DB/provider-safe, not process-local.

190. PostgreSQL

Use module-local schema and accepted persistence patterns.

191. No cross-schema FK

Hard requirement.

192. No shared DbContext

Hard requirement.

193. No direct peer Infrastructure dependency

Hard requirement.

194. No cross-schema writes

Hard requirement.

195. Contract dependencies

Plan allowed contract dependencies explicitly.

196. Architecture decisions inventory

Create explicit P21-R# inventory.

At minimum investigate a decomposition such as:

P21-R1:
HotelBooking module ownership / schema / catalog reference

P21-R2:
stay structure / room reservations / guest occupancy / multi-room scope

P21-R3:
availability/inventory authority / hold / supplier-neutral reservation boundary

P21-R4:
hotel rate offer / quote / monetary snapshot / cancellation policy snapshot

P21-R5:
HotelBooking lifecycle / confirmation authority / supplier orchestration /
idempotency / reconciliation

P21-R6:
Payment integration / target extension / financial compensation / refund dependency

P21-R7:
cancellation / amendment / refund-policy boundary

P21-R8:
public UX / anonymous-auth authorization / privacy / operational reads /
supplier-provider readiness

Adjust only if SoT supports a better decomposition.

197. Do not prematurely resolve open decisions

PLAN may record decisions already unequivocally locked by accepted SoT.

All genuinely new P21-R# decisions remain OPEN until architect locks them.

198. Critical dependency list

Explicitly record dependencies/blockers such as:

- Hotel Catalog owner
- availability authority
- supplier/provider existence
- Payment target extension
- Partial Refund dependency
- cancellation semantics

199. IN / OUT / DEFER

Explicitly classify P21 scope.

200. Likely IN candidates to evaluate

- HotelBooking independent module
- Hotel logical reference
- stay dates
- room reservation structure
- guest/contact snapshot
- availability/offer binding
- monetary/cancellation snapshot
- provider-neutral supplier contract
- idempotent reservation orchestration
- public HotelBooking preparation/transaction UX
- Payment integration if architecture supports it
- cancellation baseline if required
- reconciliation
- privacy/security

201. Likely OUT/DEFER candidates

- hotel content/catalog redesign
- channel manager
- property PMS
- supplier settlement
- accounting
- commission settlement
- loyalty
- wallet
- dynamic packaging
- flight+hotel package orchestration
- AI recommendations
- fraud platform
- voucher PDF unless required
- amendments/rebooking unless accepted
- multi-supplier smart routing

202. Source-of-Truth conflict detection

Specifically inspect for conflicts around:

Hotel Catalog owner
HotelBooking schema
Pricing ownership
Payment target scope
Partial Refund
Hotel cancellation
supplier/provider choice

203. Partial Refund conflict reporting

Because P20 locked:

Partial Refund = DEFERRED

If P21 cancellation baseline would require penalty-based partial refund:

record exact architectural conflict/dependency.

Do not resolve by silently changing Payment.

204. No product implementation

Do NOT create:

- HotelBooking module code
- DbContext
- migrations
- APIs
- frontend routes
- supplier adapters
- Payment extensions

in PLAN.

205. Plan artifact

Create:

docs/plans/P21-implementation-plan.md

206. Plan sections

Include at minimum:

- Current SoT
- P21 purpose
- ownership boundaries
- Hotel Catalog relationship
- stay/room/guest domain analysis
- availability/inventory alternatives
- supplier/provider boundary
- pricing/quote/monetary snapshot
- cancellation policy
- lifecycle
- Payment integration
- refund dependency
- public UX
- authorization/privacy
- operational/reconciliation
- security threat model
- failure-mode matrix
- decision inventory
- IN/OUT/DEFER
- task sequence
- validation/evidence strategy

207. Security threat model

Cover at minimum:

- Booking ownership bypass
- stale availability
- price tampering
- occupancy tampering
- duplicate booking
- duplicate supplier reservation
- forged supplier confirmation
- supplier callback replay
- ambiguous supplier timeout
- booking confirmation without authoritative reservation
- payment success + supplier failure
- cancellation/refund mismatch
- guest PII leakage
- access-token leakage
- provider secret leakage

208. Failure-mode matrix

Include at minimum:

- availability disappears before hold
- hold expires before payment
- rate changes before booking
- supplier initiation timeout
- supplier confirms but local commit fails
- payment succeeds but supplier booking fails
- supplier booking succeeds but payment fails
- duplicate supplier callback
- cancellation request ambiguous
- refund execution delayed/fails
- system restart between durable steps

209. Task decomposition

Produce authoritative sequence:

TC-P21-T001
through
TC-P21-T008

then:

TC-P21-T009 hardening/evidence
TC-P21-GATE

210. Task mapping

Prefer one T00x per R# decision where practical.

211. T001 posture

Expected first implementation task should only lock/scaffold:

P21-R1

module ownership/schema/catalog reference.

No HotelBooking aggregate before ownership is accepted.

212. T002 likely scope

Stay/room/guest structure.

213. T003 likely scope

Availability/inventory/supplier reservation boundary.

214. T004 likely scope

Hotel rate/monetary/cancellation policy snapshot.

215. T005 likely scope

HotelBooking lifecycle/orchestration/reconciliation.

216. T006 likely scope

Payment integration / financial compensation.

If Payment extension is required, this task must be scoped carefully.

217. T007 likely scope

Cancellation/amendment/refund policy.

218. T008 likely scope

Public UX/auth/privacy/provider readiness/ops read.

219. T009

Hardening/evidence only.

220. Gate

Final validation/SoT sync only.

221. Test strategy

Plan exact later evidence for:

- stay date validation
- occupancy validation
- multi-room invariants if IN
- duplicate booking prevention
- supplier timeout ambiguity
- supplier confirmation authenticity
- callback replay
- price/rate expiry
- price change
- cancellation-policy snapshot
- Payment/supplier race
- recovery/refund
- anonymous token authorization
- cross-user denial
- no public enumeration

222. Concurrency strategy

Plan database/provider-safe tests for:

- same request duplicate reservation
- one hold/room inventory consumption
- supplier retry race
- payment/reservation race
- cancellation race

223. Outbox/inbox strategy

Plan durable handoffs for:

HotelBooking <-> Payment

HotelBooking <-> supplier adapter events if asynchronous

No distributed transaction.

224. Provider reconciliation

Plan callable recheck services, not speculative scheduler unless existing infra
naturally applies.

225. Public contract posture

Behavior-oriented.

No generic CRUD.

226. Documentation of exact unknowns

Explicitly list all business/external facts architecture cannot determine.

Examples:

- chosen hotel supplier
- supplier APIs/capabilities
- cancellation rules
- pay-now vs pay-at-property
- partial refund requirements
- multi-room requirements

Do not fabricate them.

227. Real blocker policy

If one of these unknowns prevents PLAN entirely, report blocker.

Otherwise plan provider-neutral architecture and mark business fact OPEN/DEFERRED.

228. SoT synchronization

Update authoritative SoT only enough to record:

- P20 COMPLETE / GATE ACCEPTED
- P21 PLANNED
- P21 implementation plan authored
- P21-R# inventory OPEN except decisions already locked by existing SoT
- task sequence
- IN / OUT / DEFER
- critical dependencies/conflicts
- no P21 product implementation yet

229. Do not start T001

After PLAN PASS:

do NOT execute TC-P21-T001 until architect accepts PLAN and explicitly locks P21-R1.

Allowed:

- architecture analysis
- domain modeling
- ownership analysis
- hotel availability/inventory analysis
- supplier-neutral integration planning
- pricing/payment/cancellation boundary analysis
- threat/failure modeling
- task decomposition
- documentation
- SoT synchronization

Forbidden:

- HotelBooking product implementation
- HotelBooking DbContext
- HotelBooking migration/schema creation
- HotelBooking aggregate code
- supplier SDK
- named supplier selection without SoT
- Payment code changes
- Partial Refund implementation
- Hotel cancellation implementation
- public HotelBooking API/UI
- new dependencies/packages
- unrelated refactors
- future-phase implementation

Done:

- P21 authoritative implementation plan exists
- HotelBooking != Hotel Catalog is explicit
- Hotel Catalog authoritative owner is identified from SoT
- HotelBooking ownership/schema candidate is explicit
- stay/room/guest modeling questions are explicit
- multi-room scope is explicit/open
- availability/inventory authority alternatives are explicit
- supplier/provider posture is explicit
- no fake availability is assumed
- hotel commercial offer/quote/snapshot boundaries are explicit
- cancellation-policy ownership/snapshot is explicit
- Payment integration dependency is explicit
- P20 Partial Refund dependency/conflict is explicitly analyzed
- supplier/payment orchestration failure modes are documented
- P21-R# inventory exists
- P21 T001-T009+GATE task sequence exists
- IN / OUT / DEFER exists
- no P21 product code exists
- no supplier/provider is invented
- no material SoT conflict is silently ignored

Validation:

Required:

git diff --check

Run repository documentation/state consistency checks used by governance.

Because this is a docs-only planning task, full backend/frontend validation is
required only if repository governance requires it or code/generated artifacts
are touched.

Report exactly what was run.

Repository safety:

- discover repository root using:
  git rev-parse --show-toplevel

- git fetch origin
- require branch main
- require main == origin/main
- require CLEAN working tree before changes

Forbidden repository operations:

- force push
- accepted-history rewrite
- reset discarding accepted work
- duplicate cherry-picks

Commit:

After successful validation:

- commit with TC-P21-PLAN in the commit message
- push main to origin/main using normal fast-forward push
- re-fetch origin
- verify HEAD == origin/main
- verify Working Tree CLEAN

Expected Baseline:
96be199

Auto-Execute:

After PASS:

- return TC-P21-PLAN RESULT to architect
- do NOT execute TC-P21-T001 until PLAN is architect ACCEPTED
- do NOT invent P21-R1 through P21-R8
- remain in PIPELINE

END_TRAVELCORE_CURSOR_TASK_V1
```
