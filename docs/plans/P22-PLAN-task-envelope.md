# TC-P22-PLAN Task Envelope

Captured live after TC-P21-GATE = ACCEPTED. Baseline d6bd842. Planning only — do **not** implement Flight product code or T001.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version:
1

Task-ID:
TC-P22-PLAN

Phase:
P22

Title:
Flight architecture and implementation plan

Baseline:
d6bd842

Purpose:
Inspect the current TravelCore Source of Truth and repository and author the
complete architecture/implementation plan for:

P22 — Flight

This is a PLANNING / ARCHITECTURE / DOCUMENTATION task only.

Do NOT implement Flight product code.

Do NOT create the Flight module, FlightDbContext, schema migration, API, frontend,
supplier adapter, search integration, booking flow, payment integration, or
provider SDK.

The plan must derive Flight architecture from the accepted TravelCore architecture
and actual repository.

A critical accepted distinction already exists:

Tour Package Flight
!=
live Flight inventory / booking

The PLAN must preserve this distinction and determine the proper ownership and
integration boundaries for an independent Flight capability.

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

d6bd842


2. Verify P21 closure

Confirm authoritative SoT records:

P21 — Hotel Booking = COMPLETE
TC-P21-GATE = ACCEPTED

Do not change accepted P21 architecture.


3. Identify authoritative P22 definition

Inspect at minimum:

docs/PROJECT-STATE.md
docs/ROADMAP.md

and all P22/Flight-related architecture documents, ADRs, plans, domain docs,
page archetypes, existing code, contracts, tests, and comments.


4. Search repository comprehensively

Search for concepts including:

Flight
Flights
Airline
Airport
Route
Segment
Leg
Itinerary
PNR
Ticket
Fare
FareClass
Cabin
Passenger
Ancillary
Baggage
Seat
GDS
NDC
Amadeus
Sabre
Travelport
Charter
ScheduledFlight
TourFlight
TourPackageFlight
Departure
Arrival
BookingReference
ReservationReference


5. Do not assume terminology

Use actual repository terminology where it already exists.

Document conflicting or legacy terminology if found.


6. Critical Tour boundary

Explicitly inspect how Tour currently represents flight information.

Determine whether Tour owns concepts such as:

- outbound/inbound flight display information
- airline label
- origin/destination
- departure/arrival time
- package transport description
- tour itinerary flight segment

Do not assume exact shape.


7. Lock nothing new prematurely

The PLAN may mark already accepted SoT facts as LOCKED.

New P22 architecture questions must remain:

OPEN

until Architect acceptance of the PLAN and explicit resolution of each P22-R#.


8. Flight != Tour Package Flight

The PLAN must preserve:

Tour Package Flight
!=
live Flight inventory

Tour package descriptive/contractual flight information must not silently become
the live Flight inventory aggregate.


9. Determine whether Flight is independent

Analyze whether P22 should be an independent domain module.

Expected candidate:

Flight

but do not lock merely from the phase title.


10. Candidate schema

Analyze candidate:

flight

Do not create it.

Only recommend/lock later if consistent with SoT and repository conventions.


11. Module ownership analysis

Determine what Flight should own versus what remains owned elsewhere.

At minimum analyze boundaries with:

Tour
Booking
Payment
Pricing
Search
Place
ReferenceData
Party
Identity
Media
SEO


12. Airport ownership

Determine the authoritative owner of Airport data from actual SoT.

Do NOT assume Flight automatically owns airport catalog.


13. Airline ownership

Determine the authoritative owner of Airline/carrier reference data from actual
SoT.

Do not guess.


14. Geography boundary

Determine whether:

airport city
country
destination
Place

are Flight-owned or logical references to existing owners.


15. Flight inventory meaning

Define what “Flight” in P22 is intended to represent.

Analyze at least:

A. live bookable flight inventory / offers
B. TravelCore-owned scheduled flight inventory
C. charter inventory
D. external supplier/GDS/NDC inventory
E. hybrid

Do not choose until evidence/architecture analysis supports it.


16. Flight inventory authority alternatives

Explicitly compare:

A.
External supplier authoritative availability/inventory

B.
TravelCore-owned inventory/allotment

C.
Hybrid

For each evaluate:

- oversell risk
- multi-instance correctness
- freshness
- hold/reservation semantics
- supplier ambiguity
- reconciliation
- implementation complexity
- future extensibility


17. Current supplier posture

Inspect repository for any real Flight supplier/provider.

Report exact:

Named Flight Supplier:
<name or NONE>

Production Flight Availability Source:
<name or NONE>

Production Flight Pricing Source:
<name or NONE>

Production Flight Reservation/Ticketing Source:
<name or NONE>


18. No invented supplier

If repository is silent:

recommend:

NONE

Do NOT choose Amadeus/Sabre/Travelport/etc. merely because they are common.


19. Supplier-neutral architecture

Analyze provider-neutral ports required for P22 without designing a giant provider
framework.


20. Availability vs schedule

Distinguish:

Flight schedule/search data
!=
live seat availability


21. Availability vs price

Distinguish:

seat availability
!=
fare/price


22. Offer vs booking

Distinguish:

FlightOffer
!=
FlightBooking


23. Reservation vs ticketing

Analyze whether:

supplier reservation / PNR
!=
ticket issuance

must be separate concepts.


24. Ticketing importance

Flight differs materially from Hotel because a reservation may exist before tickets
are issued.

The PLAN must explicitly analyze this.


25. Flight business lifecycle

Analyze likely business states without prematurely locking exact enums.

Consider separate state for:

- TravelCore FlightBooking
- supplier reservation / PNR
- ticket issuance
- cancellation/refund


26. No mega status

Recommend separate process/aggregate states rather than one giant enum combining:

search
offer
hold
payment
PNR
ticket
refund
cancellation


27. Flight search request

Analyze core search dimensions:

- origin
- destination
- departure date
- return date if round-trip
- trip type
- passengers
- cabin/class
- direct/nonstop preference

Do not automatically implement every travel-industry filter.


28. Trip type

Analyze support baseline for:

- one-way
- round-trip
- multi-city

Decide whether multi-city belongs in P22 baseline or DEFERRED.


29. Segment model

Analyze:

FlightItinerary
FlightSegment
FlightLeg

and define clear terminology.


30. Connecting flights

Determine whether baseline should support:

one or multiple segments per journey direction.


31. Connection vs stopover

Do not over-model unless required.

Document terminology required for correct public UX.


32. Round-trip structure

Analyze whether outbound/inbound should be represented as:

journeys
directions
itineraries
or repository-consistent equivalent.


33. Passenger model

Analyze required baseline passenger categories.

Likely concepts may include:

Adult
Child
Infant

but do not simply copy Tour/Hotel semantics.


34. Infant semantics

Flights often require infant handling different from hotel occupancy.

Analyze whether Infant must be included in P22 baseline.


35. Passenger age policy

Analyze whether DOB is required for actual airline booking.

Do not preempt privacy requirements.


36. Passenger identity requirements

Analyze real flight reservation/ticketing needs such as:

GivenName
FamilyName
Gender if supplier requires it
BirthDate if fare/ticketing requires it
Nationality
Passport/document details for international flights

Do not implement them yet.


37. Privacy classification

The PLAN must classify which passenger facts are:

- required at search
- required at booking
- required at ticket issuance
- only supplier-specific
- not required in baseline


38. Domestic vs international

Analyze whether P22 baseline needs to distinguish domestic and international booking
requirements.


39. Passport/document timing

Explicitly analyze whether passport data should be:

A. mandatory at booking
B. collected only when supplier/fare requires it
C. deferred to post-booking fulfillment

Do not choose without architecture rationale.


40. PII minimization

The plan must favor minimum required data.

Do not store document scans unless an accepted business need exists.


41. Fare model

Analyze a source-neutral:

FlightOffer
FareOffer
FlightFareOffer

concept.


42. Offer authority

Determine whether offer truth should be external-source authoritative or internally
priced.


43. Existing Pricing module

Inspect P12 carefully.

Do NOT assume the Tour Pricing module can price Flight.


44. Pricing boundary

Analyze whether Flight should own immutable accepted:

FlightOfferSnapshot
FlightBookingMonetarySnapshot

similar to HotelBooking, versus reuse/generalization of existing Pricing.

Do not lock until plan findings.


45. Fare components

Analyze minimum monetary breakdown needed:

- base fare
- taxes
- fees
- total

Do not turn TravelCore into an airline tax engine.


46. Fare family/class

Analyze:

Cabin
BookingClass
FareBasis
FareFamily

and decide which are:

reference facts
opaque supplier facts
customer-facing commercial facts


47. Baggage

Analyze baggage allowance as an immutable purchased-offer fact.

Do not create a global baggage rules engine unnecessarily.


48. Ancillaries

Analyze but likely defer:

- seats
- meals
- extra baggage
- lounge
- priority boarding

unless SoT requires them now.


49. Offer expiry

Flight offers are usually volatile.

The PLAN must define explicit source-authoritative offer expiry/freshness.


50. Silent repricing

Preserve general principle:

silent repricing = forbidden


51. Revalidation

Analyze whether Flight booking requires offer revalidation immediately before
reservation/payment.


52. Requote

Analyze how a changed/expired fare should behave.

Do not silently replace customer-accepted monetary truth.


53. Inventory hold

Determine whether Flight suppliers support:

- explicit seat hold
- PNR reservation as the hold
- no hold, only immediate sell

Do not assume Hotel-style hold applies identically.


54. Flight hold abstraction

Evaluate whether a separate:

FlightAvailabilityHold

is appropriate or whether supplier reservation/PNR itself is the hold.


55. Supplier capability differences

The PLAN must support protocol capability differences explicitly without source-name
guessing.


56. Reservation ordering

Analyze at least these orderings:

A.
offer -> payment -> supplier reservation -> ticket

B.
offer -> supplier reservation/PNR -> payment -> ticket

C.
offer -> reservation with TTL -> payment -> ticket

D.
payment -> ticket-direct supplier flow

For each identify risks.


57. Payment interaction

P20 Payment currently supports exactly:

TourBooking
HotelBooking

This is critical.


58. Do not silently extend Payment

P22 PLAN must explicitly evaluate whether FlightBooking becomes a third supported
typed target.


59. No generic Payment target

Preserve:

no arbitrary TargetType / TargetId platform.


60. Flight Payment obligation

Analyze authoritative source for FlightBooking amount/currency.


61. Pay-now baseline

Analyze whether Flight should be:

full PayNow

for baseline.


62. Pay-later

Analyze but likely defer unless SoT says otherwise.


63. Partial payment

Likely defer.

Do not implement in PLAN.


64. Payment timing vs PNR

Flight reservation can have ticketing time limits.

Analyze payment ordering against:

PNR expiration / ticketing deadline.


65. Ticketing deadline

Analyze source-authoritative:

TicketingDeadline / TimeLimit

using NodaTime Instant.


66. Payment success != ticket issued

Critical distinction:

PaymentSucceeded
!=
PNRConfirmed
!=
TicketIssued


67. Flight confirmation semantics

Determine what customer-facing “Confirmed” should mean.

Possibilities:

- supplier reservation exists
- PNR exists
- tickets issued

The PLAN must explicitly resolve as future R decision, not guess.


68. Ticket entity

Analyze whether:

FlightTicket

needs its own child/entity/value structure.


69. E-ticket number

Analyze privacy/security treatment.


70. PNR

Analyze storage and exposure of supplier reservation locator.


71. Supplier reservation ambiguity

Preserve general rule:

network timeout
!=
reservation failed


72. Ticketing ambiguity

Also analyze:

ticket issuance timeout
!=
ticket failed


73. Reconciliation

Plan for authoritative:

reservation status query
ticket status query

where supplier supports them.


74. Idempotency

Plan DB-backed idempotency for:

- public FlightBooking initiation
- supplier reservation
- ticketing
- Payment preparation
- cancellation/refund


75. Concurrency

Plan multi-instance correctness.

No process-local lock as authority.


76. Exactly-once

Do not claim distributed exactly-once.


77. Outbox/inbox

Plan at-least-once + local idempotency for cross-module events.


78. Cancellation

Analyze FlightBooking cancellation separately from:

supplier reservation cancellation
ticket void
ticket refund


79. Ticket void

Flight industry may distinguish:

void
refund

Analyze whether baseline must represent both.


80. Refund type

Analyze whether flight cancellations frequently require:

partial refund
penalties
per-passenger refund
per-segment refund


81. Critical P20 limitation

Current Payment:

Partial Refund = DEFERRED

The P22 PLAN MUST explicitly identify any conflict with Flight cancellation/refund
requirements.


82. Do not hide partial-refund dependency

If realistic Flight cancellation requires partial monetary refund:

flag it as an architectural dependency/blocker for executable cancellation scope.


83. Per-passenger cancellation

Analyze but do not assume baseline support.


84. Partial itinerary cancellation

Analyze and likely defer unless SoT demands it.


85. Ticket refund authority

Distinguish:

supplier refund economics
!=
Payment refund execution


86. Payment Refund

Preserve Payment ownership of money movement.


87. Flight cancellation economics

Analyze immutable:

FlightCancellationPolicySnapshot
FareRulesSnapshot

or repository-equivalent.


88. Fare rules

Avoid storing only unstructured fare-rule text.

Plan structured minimum executable facts plus optional explanatory text.


89. No supplier settlement

Do not design supplier settlement/accounting unless P22 SoT explicitly includes it.


90. Search module boundary

Search is retrieval/discovery/read-model owner, not transactional truth.


91. Flight Search integration

Analyze whether P22 includes public Flight Search endpoints/UI and how Search module
should or should not participate.


92. Live flight search

Do not store external live availability in Search as authoritative truth.


93. Caching

Analyze safe caching of flight search/offers with explicit freshness.


94. SEO

Transactional/search result routes must follow SEO ownership rules.


95. Search-result indexation

Analyze whether Flight search result pages are indexable or controlled/noindex.

SEO owns final policy.


96. Public routes

Inspect current route patterns.

Do not invent final routes without repository analysis.


97. Likely public journey

Analyze:

search
results
offer selection
passenger details
booking
payment
confirmation/ticket

without implementing.


98. Server Component First

Preserve accepted frontend architecture.


99. Mobile-first

Flight UX must be explicitly mobile-first.


100. Accessibility

Plan keyboard, forms, status, validation, error semantics.


101. Bidi

FA/EN/AR and direction-neutral layout remain required.


102. Date/time presentation

Flight times must include timezone/context correctly.

Do not display ambiguous local times without airport/local-zone context.


103. Temporal model

Use NodaTime architecture.


104. Departure/arrival temporal modeling

Analyze use of:

LocalDateTime + DateTimeZone
Instant + airport timezone context

rather than naive DateTime.


105. Airports crossing timezones

Plan for departure and arrival having different timezones.


106. Overnight flights

Support arrival date differing from departure date.


107. Duration

Derive or source-authoritative; do not create ambiguous naive subtraction.


108. Reference data

Determine actual ownership of:

country codes
airport codes
airline codes
cabin codes

from SoT.


109. IATA/ICAO

Do not automatically make both mandatory.


110. External identifiers

Keep supplier IDs opaque and source-scoped.


111. Airline flight number

Analyze normalized components:

carrier code
flight number

without assuming uniqueness globally.


112. Codeshare

Analyze whether baseline should expose marketing vs operating carrier.

Do not overbuild if not needed.


113. Charter flights

Tour business may use charter inventory.

Explicitly analyze charter compatibility.


114. Scheduled vs charter

Determine whether P22 must support both or defer one.


115. Tour integration

Analyze how Tour package flight information may reference Flight data if ever
appropriate.


116. No Tour dependency on live inventory

Preserve Tour product stability even if live Flight offer disappears.


117. Flight booking reuse in Tour

Do not assume TourBooking should automatically consume FlightBooking.


118. Packaging

Package/dynamic packaging remains separate unless SoT says otherwise.


119. FlightBooking module name

Determine whether transactional aggregate should be named:

FlightBooking

inside Flight module

or separate module.

Do not lock before PLAN analysis.


120. Candidate architecture A

Independent Flight module containing:

search/source abstractions
offers
FlightBooking
supplier reservation/ticketing

Analyze pros/cons.


121. Candidate architecture B

Flight catalog/search module separate from FlightBooking transaction module.

Analyze pros/cons.


122. Candidate architecture C

Flight search/offers in Flight module,
transaction in Booking module.

Analyze and compare against:

Identity != Party != Access
HotelBooking != Tour Booking
domain ownership principles.


123. Preferred ownership

Recommend one candidate in the plan, but leave R1 OPEN until architect acceptance.


124. Schema boundary

Recommend schema(s) but do not create.


125. API contracts

Plan module-local Contracts and logical cross-module references.


126. No shared DbContext

Hard architectural invariant.


127. No peer-schema FK

Hard architectural invariant.


128. No peer Infrastructure dependency

Hard architectural invariant.


129. Payment coupling

Contracts only.

No Infrastructure-to-Infrastructure dependency.


130. Place/reference coupling

Logical references/contracts only.


131. Flight supplier ports

Identify minimum likely ports.

Potential concepts:

IFlightSearchSource
IFlightOfferSource
IFlightReservationSource
IFlightTicketingSource

but do not create all unless architecture analysis justifies them.


132. Avoid giant gateway

A single giant:

IFlightSupplierGateway

with every future capability may be undesirable.

Explicitly evaluate.


133. Capability descriptors

Analyze whether explicit supplier capabilities are required for:

Search
Price/Revalidate
Reserve
ReservationQuery
Ticket
TicketQuery
Cancel
RefundQuote
Refund


134. Capability != provider name

Preserve explicit capability modeling if chosen.


135. Zero-source production

Plan host validity with zero production Flight sources.


136. No fake production source

Hard requirement.


137. Public zero-source behavior

Plan truthful unavailable state.


138. Operational visibility

Plan read-only Flight operational view.


139. Operational facts

Potentially include:

search/offer provenance
Booking status
supplier reservation/PNR
ticketing status
Payment/Refund
reconciliation


140. No operational mutation

Do not plan ForceTicket / ForceConfirm / MarkPaid style endpoints.


141. Provider recheck

Trusted operational recheck may query authoritative supplier state.


142. Security

Plan object-level authorization for FlightBooking.


143. Anonymous booking

Analyze whether anonymous FlightBooking should be supported.

Compare with TourBooking/HotelBooking patterns.


144. Flight token

If anonymous booking is chosen, it must use a Flight-specific credential.

Do not reuse Tour/Hotel tokens.


145. Identity document sensitivity

Flight may contain substantially more PII than HotelBooking.

Plan stricter DTO/logging/operational redaction.


146. Logging

No passport/document/payment credential logging.


147. Secret handling

Provider credentials secure configuration only.


148. PCI

No raw card collection.

Payment remains provider-hosted model.


149. Public identifiers

FlightBookingId / PaymentId / PNR must not become authorization credentials.


150. Provider callback

Analyze authenticated technical callbacks separately from public customer routes.


151. Callback replay

Plan idempotency.


152. Cross-booking attack

Plan correlation guardrails.


153. Browser return

Must not be trusted as Payment/ticket success.


154. Ticket documents

Analyze whether ticket/PDF artifacts belong to Flight or Media.

Do not implement.


155. Notifications

Notification module may later deliver itinerary/ticket notifications.

Flight should not own email infrastructure.


156. AI readiness

Only structured attributable facts and provenance.

No:

LLM
RAG
embeddings
vector DB


157. Metrics/observability

Plan safe operational telemetry:

source latency
source errors
reservation ambiguity
ticketing ambiguity

without leaking PII/secrets.


158. Regulatory/compliance

Do not invent jurisdiction-specific rules.

Flag only architecture implications evident from stored passenger identity/document
data.


159. P22 risk inventory

The plan must explicitly list material risks, including at least:

- volatile offers
- inventory oversell
- supplier timeout ambiguity
- duplicate PNR/reservation
- ticket issuance ambiguity
- offer expiry during Payment
- Payment succeeded but ticketing failed
- ticketing succeeded but local commit failed
- partial refund dependency
- timezone/date mistakes
- PII/document exposure
- cross-booking authorization
- supplier capability mismatch


160. Failure matrix

Create a high-level failure/compensation matrix for at least:

A.
offer expires before Payment

B.
Payment succeeds but reservation not created

C.
reservation created but Payment fails/never succeeds

D.
Payment succeeds + PNR exists + ticketing fails definitively

E.
ticketing outcome ambiguous

F.
supplier reservation becomes invalid after Payment

G.
customer cancellation requiring partial refund

H.
duplicate supplier callback

I.
process crash after supplier success before local commit


161. Payment-before/after reservation comparison

Explicitly compare financial/inventory risks.


162. Ticket-first impossible posture

Do not recommend ticket issuance before required Payment unless SoT/business model
explicitly supports credit/agency settlement.


163. Agency/B2B

Analyze compatibility with future AgencyMarketplace / agency booking context.

Do not implement agency-specific settlement.


164. Buyer/booked-by

Inspect whether FlightBooking should eventually preserve the same Party/actor
principles used elsewhere.


165. One booking vs multiple passengers

Analyze aggregate boundary.


166. One PNR vs multiple PNRs

Analyze whether baseline should assume one logical supplier reservation per
FlightBooking.

Do not lock if supplier behavior requires multiple reservations.


167. Multi-passenger ticketing

Analyze tickets per passenger/segment.


168. Partial ticket issuance

Critical:

some passengers/segments ticketed
!=
whole FlightBooking successfully ticketed

Plan reconciliation semantics.


169. Confirmation definition

Make this an explicit P22-R decision.


170. Candidate decision inventory

The PLAN must create an explicit decision inventory.

Preferred initial structure:

P22-R1
Flight ownership/module/schema and Tour boundary

P22-R2
Flight itinerary/segment/airport/airline/passenger model

P22-R3
Search/availability/offer authority and supplier capability boundary

P22-R4
Fare offer/revalidation/monetary snapshot/fare rules

P22-R5
Supplier reservation/PNR lifecycle, idempotency and reconciliation

P22-R6
Payment ordering/typed Flight target/ticketing lifecycle/compensation

P22-R7
Cancellation/void/refund/partial-refund dependency

P22-R8
Public UX/auth/privacy/operational/provider readiness

Adjust exact breakdown only if repository evidence supports a materially better
decomposition.


171. New decisions remain OPEN

Unless an item is already explicitly locked by accepted SoT:

P22-R1 through P22-R8
=
OPEN


172. Implementation sequence

Plan:

TC-P22-T001
through
TC-P22-T008

TC-P22-T009
=
hardening/evidence

TC-P22-GATE
=
phase acceptance


173. T001 scope

Expected T001 after R1 acceptance:

minimal Flight ownership/module/schema foundation only.

No itinerary/offer/reservation/ticketing yet.


174. Do not write T001 now

Hard requirement.


175. PLAN artifact

Create:

docs/plans/P22-implementation-plan.md


176. Plan structure

Include at minimum:

1. Executive summary
2. Current SoT/repository findings
3. Existing Tour flight representation
4. Flight domain ownership alternatives
5. Recommended module/schema posture
6. Airport/Airline/Place/ReferenceData ownership
7. Itinerary/segment/passenger model alternatives
8. Search/availability/inventory authority
9. Supplier capability/source architecture
10. Fare offer/revalidation/monetary boundary
11. Reservation/PNR/ticketing model
12. Payment ordering and P20 dependency
13. Cancellation/void/refund dependency
14. Public UX/auth/privacy
15. Operational/reconciliation model
16. Security/PII
17. Concurrency/idempotency/outbox/inbox
18. Failure/compensation matrix
19. P22-R1-R8 decision inventory
20. Task sequence T001-T009 + GATE
21. IN scope
22. OUT scope
23. DEFERRED
24. Dependencies/conflicts
25. Gate criteria


177. Existing code findings

The plan must cite concrete repository paths/types discovered.

Do not write generic architecture disconnected from the repo.


178. SoT contradictions

If actual repository conflicts with docs:

report exact conflict.


179. Working-tree blocker

If repository is not synchronized/clean:

STOP.

Do not modify.


180. P20/P21 dependency section

Explicitly document:

Payment currently supports:
TourBooking
HotelBooking

Flight is NOT yet a Payment target.


181. Partial Refund dependency

Explicitly document current:

Partial Refund = DEFERRED


182. No real Payment Provider

Record:

Production Payment Provider = NONE


183. No real Flight supplier

Record exact repository finding.


184. Do not redesign Payment during PLAN

No code.


185. Do not redesign Search during PLAN

No code.


186. No new packages

Hard requirement.


187. No DB migrations

Hard requirement.


188. No backend product code

Hard requirement.


189. No frontend product code

Hard requirement.


190. No API endpoints

Hard requirement.


191. Documentation-only changes

Expected touched scope:

docs/plans/P22-implementation-plan.md
docs/PROJECT-STATE.md
docs/ROADMAP.md

plus task/result envelope documentation according to repository governance.


192. SoT update

Update SoT only enough to record:

P22 = IN_PROGRESS / PLAN_AUTHORED
or repository-standard equivalent

TC-P22-PLAN = implemented / awaiting architect review

P22-R inventory = OPEN

T001-T009 + GATE sequence documented


193. Do not mark R decisions resolved

Hard requirement unless a fact is explicitly inherited from locked SoT rather than
a new P22 decision.


194. Do not mark P22 READY_FOR_GATE

Hard requirement.


195. Do not start P23

Hard requirement.


196. Validation

Because this should be docs-only:

Run:

git diff --check

Run repository governance/document consistency checks if present.

If any code/generated/project files are touched unexpectedly, run the appropriate
full build/tests and explain why.


197. Review modified files

Verify no source code/migration/frontend/package file is modified.


198. Static phase check

Verify no:

src/...Flight product code

was created.


199. Commit

After successful validation:

commit with message containing:

TC-P22-PLAN


200. Push

Use normal fast-forward push to:

origin/main


201. Post-push

Run:

git fetch origin
git rev-parse HEAD
git rev-parse origin/main
git status --short

Require:

HEAD == origin/main
Working Tree = CLEAN


202. Required Result Evidence

Report exact:

- Flight phase authoritative title
- docs-only: YES/NO
- P22 plan artifact
- actual Tour flight representation findings
- Tour Package Flight != live Flight inventory: YES/NO
- recommended Flight ownership model
- recommended module name
- recommended schema
- Airport authoritative owner
- Airline authoritative owner
- Place/Destination relationship
- existing Flight code/module found: YES/NO and exact scope
- Named Flight Supplier
- Production Flight Availability Source
- Production Flight Rate/Pricing Source
- Production Flight Reservation Source
- Production Flight Ticketing Source
- supplier SDK present: YES/NO
- Flight inventory authority alternatives
- recommended inventory authority posture
- one-way baseline recommendation
- round-trip baseline recommendation
- multi-city recommendation
- connecting flight recommendation
- passenger categories recommendation
- Infant recommendation
- passenger PII/document posture
- offer authority recommendation
- Pricing module reuse/generalization recommendation
- FlightOfferSnapshot recommendation
- FlightBookingMonetarySnapshot recommendation
- PNR/reservation model recommendation
- ticketing model recommendation
- customer confirmation definition options
- Payment current target kinds
- Flight Payment target current support: NO
- recommended Payment integration direction
- recommended reservation/payment/ticket ordering
- Partial Refund dependency
- cancellation/void/refund baseline recommendation
- anonymous FlightBooking recommendation
- public UX/search recommendation
- operational read recommendation
- smart supplier routing recommendation
- major failure/compensation findings
- P22-R1 through P22-R8 exact status
- T001-T009 + GATE sequence
- P22 IN scope
- P22 OUT scope
- P22 DEFERRED
- Source-of-Truth conflict: YES/NO
- blocker: YES/NO
- product code created: NO
- migration created: NO
- API/frontend created: NO
- git diff --check
- TC-P22-T001: NOT EXECUTED


203. Required Result Format

Return:

BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version: 1
Task-ID: TC-P22-PLAN
Phase: P22
Status: PASS/FAIL

Repository:
Branch:
Baseline:
Implementation-Commit:
SoT-Sync-Commit:
Starting-HEAD:
Current-HEAD:
HEAD == origin/main:
Working-Tree:

Scope Delivered:
...

Key Artifacts:
...

Repository Findings:
...

Decision Inventory:
...

IN:
...

OUT:
...

DEFERRED:
...

Dependencies/Conflicts:
...

Exact-Validation:
...

Next-State:
AWAITING_ARCHITECT_REVIEW

T001-Executed:
NO

END_TRAVELCORE_CURSOR_RESULT_V1


204. Auto-Execute

After PASS:

- return TC-P22-PLAN result to architect
- do NOT execute TC-P22-T001
- remain in PIPELINE


END_TRAVELCORE_CURSOR_TASK_V1
```
