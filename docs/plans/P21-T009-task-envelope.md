# TC-P21-T009 Task Envelope

Captured live after TC-P21-T008 = ACCEPTED. Baseline d8bdf0f. P21-R1 through P21-R8 = RESOLVED. Do **not** execute TC-P21-GATE.

`	ext
TC-P21-T008 = ACCEPTED

Implementation Commit:
63b8ce3

Docs Commit:
d8bdf0f

Current HEAD:
d8bdf0f

HEAD == origin/main:
YES

Working Tree:
CLEAN
`

`	ext
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version:
1

Task-ID:
TC-P21-T009

Phase:
P21

Title:
Hotel Booking hardening, adversarial regression, phase-boundary verification, and acceptance evidence pack

Baseline:
d8bdf0f

Decision:
P21-R1 through P21-R8 = RESOLVED

Purpose:
Perform final P21 hardening and produce the complete acceptance evidence pack for
Hotel Booking before TC-P21-GATE.

This task must NOT introduce new HotelBooking product capability.

The goal is to prove that the accepted P21 architecture actually holds under:

- authorization attacks
- concurrency
- retries
- duplicate messages
- crash windows
- ambiguous supplier/provider outcomes
- cross-booking/cross-target correlation
- immutable pricing/cancellation rules
- Payment/Refund compensation
- public API tampering
- frontend token handling
- zero-provider/source production configuration
- modular-monolith boundaries
- regressions against accepted Tour Booking / Payment behavior

Do NOT execute TC-P21-GATE.

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

d8bdf0f


2. Record T008 acceptance

Synchronize authoritative SoT to record:

TC-P21-T008 = ACCEPTED

Preserve:

P21-R1 through P21-R8 = RESOLVED


3. Scope posture

T009 is:

HARDENING + REGRESSION + EVIDENCE

not feature development.


4. No new architecture decisions

Do NOT invent P21-R9.

Do NOT reopen R1-R8 unless an actual correctness defect is found.


5. Correction policy

If a genuine defect is discovered:

fix only the smallest necessary defect inside already accepted P21 semantics.

Do not redesign the phase.


6. Module ownership regression

Verify:

Place
=
hotel/accommodation catalog truth

HotelBooking
=
hotel reservation transaction truth

Payment
=
payment/refund execution truth

Search
!=
live hotel availability authority

SEO
=
index/canonical policy owner


7. HotelBooking module independence

Verify:

HotelBooking.Contracts
HotelBooking.Domain
HotelBooking.Infrastructure

remain independent module projects.


8. Schema

Verify exact HotelBooking schema:

hotel_booking


9. No shared DbContext

Verify none introduced.


10. No cross-schema FK

Inspect migrations/model metadata.

Expected:

NO peer-schema FK.


11. No peer Infrastructure references

Verify HotelBooking.Infrastructure does not reference:

Place.Infrastructure
Booking.Infrastructure
Payment.Infrastructure
Pricing.Infrastructure


12. Payment reverse dependency

Verify Payment.Infrastructure does not depend on HotelBooking.Infrastructure.


13. No direct cross-schema SQL

Inspect HotelBooking and Payment R1-R8 implementation for direct peer-schema SQL
queries/joins.

Expected:
NONE.


14. Place reference

Verify:

HotelPlaceReference(PlaceId)

remains logical only.


15. HotelBooking != Tour Booking

Verify no shared generic Booking aggregate/base was introduced.


16. No generic Booking platform leakage

Confirm absence of speculative abstractions such as:

BookingBase
Booking<TTarget>
GenericBookingAggregate


17. R2 stay semantics regression

Verify:

CheckInDate = NodaTime.LocalDate
CheckOutDate = NodaTime.LocalDate
CheckOutDate > CheckInDate
Nights derived correctly


18. Multi-room regression

Verify:

HotelBooking supports 1..N RoomReservations.


19. Room semantics

One RoomReservation remains one booked room position.

No Quantity=N shortcut introduced.


20. Guest-room binding

Each HotelBookingGuest belongs to exactly one RoomReservation.


21. Minimum room/guest cardinality

Verify:

HotelBooking >= 1 room
RoomReservation >= 1 guest


22. Guest categories

Verify exact baseline:

Adult
Child


23. Child age

Verify:

Child requires AgeAtCheckIn.


24. No DOB regression

Verify HotelBooking model/public DTO/frontend does NOT introduce BirthDate.


25. No passport/document regression

Verify absence of:

Passport
PassportNumber
NationalId
DocumentScan
VisaDocument

from HotelBooking public/customer model.


26. Lead guest

Verify exactly one LeadGuest.


27. Contact snapshot

Verify HotelBookingContactSnapshot remains separate from LeadGuest identity.


28. R3 availability authority

Verify exact authority remains:

IHotelAvailabilitySource


29. Place live availability authority

Expected:
NO.


30. Search live availability authority

Expected:
NO.


31. Production Availability Source

Expected:

NONE


32. No fake production availability

Verify no test fake is registered in production DI.


33. Hold lifecycle

Verify exact values:

Requested
Active
Released
Expired


34. No Hold Failed status

Verify absent.


35. Multi-room hold

One HotelAvailabilityHold covers complete Booking room set.


36. Partial hold

Verify partial supplier/source result cannot transition local hold to Active.


37. Hold ambiguity

Timeout/Unknown must remain unresolved/Requested.


38. Hold retry safety

Unresolved Requested/Active blocks unsafe duplicate acquisition.


39. Hold concurrency

Verify database constraint/index protects one unresolved/active effective hold.


40. Hold idempotency

Repeated same idempotency key converges to same effective hold.


41. Hold expiry

Verify:

NodaTime Instant
source-authoritative expiry
no hardcoded TTL.


42. No process-local hold correctness

Verify no static lock/SemaphoreSlim/etc. is correctness authority.


43. R4 rate authority

Verify:

IHotelRateOfferSource


44. Production Rate Source

Expected:

NONE


45. No fake production rate

Verify.


46. Pricing module

Verify P12 Pricing was NOT generalized to HotelBooking.


47. Rate snapshot

Verify immutable:

HotelRateOfferSnapshot


48. Monetary snapshot

Verify immutable:

HotelBookingMonetarySnapshot


49. Rate vs monetary truth

Verify these remain distinct concepts.


50. Exact Booking binding

Verify accepted rate snapshot covers exact:

Hotel
stay
rooms
occupancy


51. Complete room coverage

Partial room rate offer cannot be accepted.


52. One transaction currency

Verify exact one CurrencyCode baseline.


53. No implicit FX

Hard requirement.


54. Toman invariant

Verify:

Toman != CurrencyCode


55. Monetary precision

Verify no authoritative float/double use.


56. Offer timestamps

Verify QuotedAt/OfferExpiresAt use Instant.


57. No hardcoded rate TTL

Verify.


58. Expired offer

Cannot be accepted.


59. Silent repricing

Verify both higher and lower replacement offers do not silently mutate accepted
snapshot.


60. Accepted snapshot immutability

Verify DB/domain APIs cannot overwrite accepted monetary/cancellation truth.


61. Cancellation policy snapshot

Verify immutable:

HotelCancellationPolicySnapshot


62. Cancellation deadline

Verify authority:

NodaTime Instant


63. Property timezone

Verify timezone is context/metadata only, not machine-local authority.


64. Penalty rules

Verify:

0 <= PenaltyAmount <= TotalAmount


65. Partial penalty fact

Verify representable.


66. Partial Refund execution

Verify still:

NOT IMPLEMENTED


67. R5 HotelBooking status

Verify exact:

Pending
Confirmed
Cancelled


68. No extra lifecycle states

Verify absence of:

AwaitingPayment
Paid
Refunding
Failed
SupplierPending
ReconciliationRequired

from HotelBookingStatus.


69. Supplier reservation

Verify distinct:

HotelSupplierReservation


70. Supplier reservation status

Exact:

Pending
Confirmed
Cancelled


71. Supplier reservation attempt status

Exact:

Created
Initiated
Confirmed
Failed


72. Network timeout

Verify:

timeout != Failed attempt.


73. Unresolved supplier attempt

Blocks unsafe retry.


74. Definitive Failed attempt

Allows explicit new attempt.


75. One logical supplier reservation

Verify one per HotelBooking baseline.


76. Multi-room supplier reservation

Verify complete room set required for local Confirmed.


77. Partial supplier confirmation

Must not confirm HotelBooking.


78. Supplier reservation authority

Verify:

IHotelReservationSource


79. Production Reservation Source

Expected:

NONE


80. Named Hotel Supplier

Expected:

NONE


81. Supplier SDK

Expected:
NONE.


82. No fake production supplier

Verify.


83. Authoritative supplier evidence

Only verified/query-confirmed source evidence may confirm supplier reservation.


84. Callback trust

Unverified callback cannot mutate truth.


85. Browser/client trust

Browser/client flags cannot confirm.


86. Cross-booking correlation

Adversarial test:

evidence for HotelBooking A cannot mutate B.


87. Cross-attempt correlation

Evidence for Attempt A cannot confirm B.


88. Source reference uniqueness

Verify provider/source correlation cannot bind to unrelated bookings.


89. Reconciliation mismatches

Verify at minimum:

MonetaryMismatch
CurrencyMismatch
RoomSetMismatch
StayMismatch
HotelMismatch
CancellationTermsMismatch

behave as reconciliation/evidence, not silent mutation.


90. R6 Payment target model

Verify Payment supports exactly:

TourBooking
HotelBooking


91. Arbitrary generic target

Verify absent.


92. Payment target storage

Verify exactly-one-target constraint.


93. One Tour Booking -> one Payment

Regression.


94. One HotelBooking -> one Payment

Regression.


95. Cross-target collision

Same UUID value in TourBooking and HotelBooking namespaces must not accidentally
become same logical target.


96. PaymentStatus

Exact:

Pending
Succeeded


97. PaymentAttemptStatus

Exact:

Created
Initiated
Succeeded
Failed


98. RefundStatus

Exact:

Pending
Succeeded


99. RefundAttemptStatus

Exact:

Created
Initiated
Succeeded
Failed


100. Hotel payment obligation

Verify source:

HotelBookingMonetarySnapshot


101. No live price lookup

Payment must not query current hotel rate/supplier to calculate charge.


102. PaymentExecutionSnapshot

Verify immutable Payment-owned copy.


103. Full PayNow baseline

Verify:

YES


104. PayAtProperty

Verify:

DEFERRED


105. Deposit/partial collection

Verify:

DEFERRED


106. Payment-before-supplier

For new P21 flow verify supplier final reservation initiation is gated by
authoritative Payment success.


107. Dual-evidence confirmation

Verify:

Payment Succeeded
AND
SupplierReservation Confirmed
=
HotelBooking Confirmed


108. Payment-only

Must stay Pending.


109. Supplier-only

New PayNow Booking must stay Pending.


110. Concurrent dual evidence

Stress/race test that HotelBooking confirms at most once.


111. No generic Confirm

Verify no unrestricted confirmation method was introduced.


112. Payment success durability

Verify Payment success + outbox atomicity.


113. Hotel payment inbox

Verify durable/idempotent.


114. Payment evidence query

Verify authoritative query is used, not event payload alone.


115. Payment amount mismatch

Must not confirm.


116. Payment currency mismatch

Must not confirm.


117. Hold expiry after Payment

Verify full compensation requirement.


118. Hold release after Payment

Verify full compensation requirement.


119. Supplier definitive failure after Payment

Verify full compensation requirement.


120. Supplier timeout after Payment

Verify:

NO automatic Refund.


121. Supplier mismatch after Payment

Verify:

no confirmation
no snapshot rewrite
no unsafe automatic Refund where external state remains uncertain.


122. Compensation event

Verify typed durable HotelBooking compensation event.


123. Compensation amount authority

Verify event does NOT authoritatively dictate Refund amount.


124. Refund amount authority

Verify:

PaymentExecutionSnapshot


125. One Refund

Verify duplicate Hotel compensation cannot create multiple logical Refunds.


126. Payment remains Succeeded after Refund

Regression.


127. R6 Refund-success behavior

Verify Pending unconfirmed HotelBooking may system-cancel after safe full
compensation.


128. Confirmed HotelBooking protection

R6 compensation Refund must not cancel Confirmed HotelBooking.


129. R7 cancellation process

Verify separate:

HotelBookingCancellation


130. Cancellation status exact values

Verify:

Requested
SupplierCancellationPending
RefundPending
Completed


131. Cancellation economics

At RequestedAt Instant verify:

Penalty = 0
-> FullRefund

Penalty = TotalAmount
-> NoRefund

0 < Penalty < TotalAmount
-> PartialRefundRequiredButUnsupported


132. Partial penalty hardening

Adversarial regression:

supplier cancellation call count = 0
Refund created = 0
HotelBooking stays Confirmed
SupplierReservation stays Confirmed


133. Supplier cancellation attempt

Verify exact statuses:

Created
Initiated
Confirmed
Failed


134. Supplier cancellation timeout

Verify:

Attempt stays Initiated
Booking remains Confirmed
Reservation remains Confirmed
Refund not started


135. Unresolved supplier cancellation

Blocks unsafe retry.


136. Definitive supplier cancellation failure

Allows explicit retry.


137. Authoritative cancellation

Only authoritative supplier cancellation may drive:

SupplierReservation Confirmed -> Cancelled

HotelBooking Confirmed -> Cancelled


138. No generic Cancel

Verify no unrestricted public/domain mutation surface.


139. HotelBookingCancelled != RefundSucceeded

Verify state distinction.


140. Full Refund cancellation

Verify:

supplier cancellation authoritative
then Booking Cancelled
then durable Refund request
then Payment Refund
then cancellation process Completed after RefundSucceeded.


141. No Refund cancellation

Verify cancellation process completes without Refund.


142. PaymentStatus after cancellation Refund

Must remain Succeeded.


143. Amendments

Verify absent:

date amendment
room amendment
guest amendment
rate amendment
rebooking


144. No-show execution

Verify absent.


145. R8 public initiation

Verify route exists exactly as implemented.


146. Public private-read

Verify route exists and is object-authorized.


147. No public HotelBooking list

Verify 404/route absence.


148. No generic public CRUD

Verify no PUT/PATCH status mutation.


149. HotelBooking token header

Verify exact:

X-TravelCore-Hotel-Booking-Access-Token


150. Token entropy

Verify cryptographically secure generation.


151. Raw token persistence

Expected:
NO.


152. Verifier/hash persistence

Expected:
YES.


153. Hash algorithm

Verify accepted SHA-256 posture or actual accepted implementation.


154. Token returned once

Verify.


155. Token URL leakage

Search frontend/backend for raw token in:

query
path
redirect
callback URLs

Expected:
NONE.


156. localStorage

Verify HotelBooking token never enters localStorage.


157. sessionStorage

Verify accepted private frontend posture.


158. Missing token

Expected:
404.


159. Wrong token

Expected:
404.


160. BookingId-only access

Expected:
404/non-authorized.


161. PaymentId-only access

Expected:
non-authorized.


162. Cross-user

Expected:
non-enumerating denial.


163. Cross-token module isolation

Tour Booking token cannot access HotelBooking.

HotelBooking token cannot access Tour Booking.


164. Initiation idempotency

Verify same idempotency key returns same HotelBooking.


165. Concurrent initiation

Stress one effective Booking.


166. Client monetary tampering

Adversarial payloads attempting:

amount
currency
penalty
payment success
supplier success

must not become authority.


167. Occupancy tampering

Verify downstream availability/rate requests use persisted HotelBooking structure,
not a new client-provided contradictory structure.


168. Zero availability source public behavior

Expected truthful unavailable/503.


169. Zero rate source public behavior

Expected truthful unavailable/503.


170. Zero reservation source behavior

No fake confirmation.


171. Zero Payment provider behavior

Expected truthful unavailable/503.


172. Fake production source

Verify:
NONE.


173. Public state distinction

Verify response can distinguish:

Payment Succeeded
HotelBooking Pending


174. Confirmed public state

Must derive only from:

HotelBookingStatus.Confirmed


175. Cancellation public partial penalty

Verify:

422 or accepted explicit blocked result

and zero supplier side effect.


176. Cancellation pending public state

Supplier timeout must not display Cancelled.


177. RefundPending state

Verify HotelBooking may be Cancelled while Refund pending.


178. RefundSucceeded state

Verify safe customer presentation without changing historical PaymentStatus.


179. Public Refund command

Expected:
NONE.


180. Card collection

Search API/frontend for:

card number
PAN
CVV
CVC
PIN
bank password

Expected no HotelBooking card collection.


181. Transaction pages

Verify:

noindex


182. Search indexing

Verify HotelBooking transaction data is not indexed.


183. SEO ownership

Verify no SEO ownership leakage.


184. Frontend locale coverage

Verify:

FA
EN
AR


185. RTL/LTR

Verify direction-neutral implementation.


186. Mixed bidi money

Verify accepted MoneyText/LtrValue/BidiText conventions as appropriate.


187. Mobile

Inspect forms/layout for narrow viewport behavior.


188. Accessibility

Verify:

labels
semantic headings
focus-visible
keyboard interaction
error semantics
status announcements
no color-only meaning


189. Operational query

Verify:

IHotelBookingOperationalQuery

or actual equivalent is read-only.


190. Operational HTTP exposure

If no accepted admin auth exists:

verify no public/admin HTTP endpoint was invented.


191. HotelBooking token -> ops

Must fail / no route.


192. Operational mutation

Verify exact:

NONE


193. No ForceConfirm

Verify absent.


194. No ForceCancel

Verify absent.


195. No MarkPaid

Verify absent.


196. No MarkRefunded

Verify absent.


197. Operational PII

Verify query DTO avoids unnecessary:

guest full PII
contact secrets
token hashes
provider secrets


198. Provider/source secrets

Search public/operational DTOs and logging.

Expected:
no secrets.


199. Raw supplier payload

Expected:
not publicly exposed/persisted as truth.


200. Logging security

Inspect HotelBooking/Payment public/auth paths.

Verify raw HotelBooking access token is not logged.


201. Error security

Unauthorized/missing/wrong token errors must not expose:

token verifier
internal source refs
provider payload
stack traces


202. Source capability model

If no explicit capability descriptor was needed in T008, verify NONE was not a
missing correctness issue.

Do not add one merely for completeness.


203. Smart routing

Verify:
NO.


204. Supplier failover

Verify:
NO.


205. Production source/provider matrix

Report exactly:

Hotel Availability Source = NONE
Hotel Rate Source = NONE
Hotel Reservation Source = NONE
Hotel Supplier = NONE
Payment Provider = NONE


206. Real SDK matrix

Report:

Hotel supplier SDK = NO
Payment provider SDK = NO


207. Distribution transaction

Verify none exists.


208. Exactly-once language

Search P21 docs/code comments.

Do not claim distributed exactly-once.

Accepted posture:

at-least-once delivery
+
local idempotent/effectively-once effects


209. Outbox/inbox durability

Review all P21 cross-module flows:

Payment success
Hotel compensation
Refund success
Cancellation Refund required

Verify durable outbox/inbox boundaries.


210. Crash window: Payment success

Payment commits success/outbox then process stops.

Verify eventual continuation.


211. Crash window: Hotel compensation

HotelBooking commits compensation outbox then stops.

Verify eventual Payment Refund creation.


212. Crash window: Refund success

Payment commits Refund success/outbox then stops.

Verify eventual HotelBooking finalization.


213. Crash window: supplier reservation success

Supplier confirms externally but local completion crashes.

Verify source references/recheck can recover.


214. Crash window: supplier cancellation success

Supplier cancels externally but local completion crashes.

Verify cancellation recheck can converge.


215. Duplicate delivery matrix

Exercise duplicates for:

- PaymentSucceeded
- Hotel compensation required
- RefundSucceeded
- CancellationRefundRequired

Expected one effective local result.


216. Out-of-order delivery

Test where practical:

supplier evidence before payment evidence
payment evidence before supplier evidence
Refund event replay after cancellation completion

Expected safe convergence.


217. Concurrent operations

Test:

duplicate initiation
duplicate hold acquisition
duplicate rate acceptance
duplicate Payment creation
duplicate supplier reservation initiation
Payment/supplier confirmation race
duplicate cancellation
duplicate Refund trigger


218. Database-backed correctness

For each above report actual authoritative:

constraint
unique index
transaction
version/advisory locking

as applicable.


219. No process-memory dependency

Restart correctness must not depend on in-memory dictionaries/locks.


220. P19 regression

Run accepted Booking/Tour Booking unit/integration behavior affected by Payment
changes.


221. P20 regression

Run accepted Payment/Refund/provider-neutral tests.


222. Tour Booking public Payment regression

Verify existing P20 public Payment routes/auth remain unchanged.


223. Hotel Payment target isolation

Verify HotelBooking target support cannot alter Tour Booking target records.


224. Tour compensation isolation

Verify Hotel cancellation/compensation events cannot affect Tour Payment.


225. Database migration chain

Apply all migrations to clean PostgreSQL test database according to integration
suite.

Verify migration chain succeeds.


226. Existing rows

Verify P21 migrations do not corrupt:

existing Tour Booking Payment
existing Payment Refund
legacy T005 HotelBooking rows


227. Schema inspection

Generate/inspect model metadata/database structure and report:

hotel_booking tables
payment changes relevant to HotelBooking target

No cross-schema FK.


228. Public contract review

List every P21 public HTTP route actually exposed.

Confirm no extra accidental endpoint.


229. Frontend route review

List every P21 frontend route actually exposed.


230. Private route indexing

Verify all transactional routes noindex.


231. Dependency review

Report new package dependencies introduced during P21.

Expected:
no supplier/provider SDK.

Flag unnecessary package additions.


232. Static secret scan

Search P21 changed files for patterns resembling:

API keys
passwords
tokens
supplier credentials
provider credentials

Do not print any real secret if unexpectedly found.

Report only safe finding status/file location.


233. Source-of-truth drift review

Compare:

docs/PROJECT-STATE.md
docs/ROADMAP.md
docs/plans/P21-implementation-plan.md

with actual implementation.

Resolve documentation drift only.


234. Decision inventory

Verify exact:

P21-R1 = RESOLVED
P21-R2 = RESOLVED
P21-R3 = RESOLVED
P21-R4 = RESOLVED
P21-R5 = RESOLVED
P21-R6 = RESOLVED
P21-R7 = RESOLVED
P21-R8 = RESOLVED


235. Deferred inventory

Verify explicitly documented:

Partial Refund = DEFERRED
PayAtProperty = DEFERRED
Deposit/Partial Payment = DEFERRED
Amendments = DEFERRED
Rebooking = DEFERRED
No-show execution = DEFERRED
Smart supplier routing/failover = DEFERRED
Real Hotel Supplier = NONE
Real Payment Provider = NONE


236. Out-of-scope inventory

Verify not implemented:

Accounting
Settlement
Supplier settlement
Agency commission
Wallet
Fraud/risk
Loyalty
LLM
RAG
Embeddings
Vector DB


237. Evidence artifact

Create:

docs/plans/P21-T009-hardening-and-evidence-pack.md


238. Evidence pack structure

Include at minimum:

1. Scope and baseline
2. P21 decision inventory
3. Module/schema ownership proof
4. Domain invariant proof
5. Availability/hold proof
6. Rate/monetary/cancellation-policy proof
7. Supplier reservation proof
8. Payment/Refund integration proof
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


239. Known limitations

The evidence pack must honestly include at least:

- no production Hotel supplier
- no production Hotel availability source
- no production Hotel rate source
- no production Hotel reservation source
- no production Payment provider
- Partial Refund unavailable
- partial-penalty confirmed cancellation blocked
- PayAtProperty unavailable
- deposit/partial collection unavailable
- amendments/rebooking unavailable
- no smart supplier routing/failover


240. No false production-ready claim

Do not claim P21 can perform a real-world Hotel reservation/payment in production
without configured real adapters/providers.


241. Gate-ready meaning

READY FOR GATE means:

architecture and implemented P21 scope are internally correct and tested.

It does NOT mean external production provider integrations exist.


242. Product-code change threshold

If no defects are found:

prefer tests/docs only.

Do not refactor working code for style.


243. Defect handling

If a defect is found:

fix it
add regression test
document correction in evidence pack.


244. No scope expansion

Do not implement deferred capability to make a test pass.


245. Frontend validation

Run from actual frontend root discovered in repository:

npm run typecheck
npm run lint
npm run build


246. Backend validation

Run:

dotnet build TravelCore.sln

HotelBooking.UnitTests
Payment.UnitTests
Booking.UnitTests
ArchitectureTests
Persistence.IntegrationTests
Host.IntegrationTests


247. Optional targeted reruns

Run additional targeted concurrency/security tests as needed.


248. git validation

Run:

git diff --check


249. Repository final state

Require:

branch = main
HEAD == origin/main
Working Tree = CLEAN


250. SoT synchronization

Update authoritative state to record:

TC-P21-T008 = ACCEPTED

TC-P21-T009 = implemented / awaiting architect review

P21-R1 through P21-R8 = RESOLVED

P21 = IN_PROGRESS

P21 = READY_FOR_GATE only if all T009 evidence passes.

Do NOT mark P21 COMPLETE.


251. Do not execute Gate

TC-P21-GATE = NOT EXECUTED


Allowed:

- hardening tests
- security/adversarial tests
- concurrency/idempotency tests
- architecture guardrails
- integration/host/frontend regressions
- minimal fixes to accepted P21 correctness defects
- documentation drift fixes
- evidence pack
- SoT synchronization for T009


Forbidden:

- new P21 business capability
- P21-R9
- Partial Refund
- PayAtProperty
- deposit/partial payment
- amendments
- rebooking
- no-show workflow
- real supplier
- supplier SDK
- real Payment provider
- Payment provider SDK
- smart supplier routing/failover
- accounting
- settlement
- agency commission
- wallet
- fraud
- loyalty
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
- TC-P21-GATE execution


Done:

- all P21-R1 through P21-R8 invariants are revalidated
- no hidden boundary leakage exists
- public HotelBooking authorization withstands enumeration/token/cross-user attacks
- Payment/TourBooking regressions remain green
- supplier/payment ambiguity handling is proven
- duplicate/retry/concurrency paths remain safe
- partial-penalty cancellation remains blocked before irreversible effects
- Payment/supplier dual evidence is race-safe
- full-refund compensation is durable/idempotent
- public state presentation does not fabricate success
- no fake production source/provider exists
- no provider secrets/token leakage exists
- frontend token/noindex/a11y/bidi/mobile invariants hold
- deferred scope remains deferred
- P21-T009 hardening/evidence pack exists
- P21 is READY_FOR_GATE
- TC-P21-GATE is NOT EXECUTED


Validation:

Run exact:

dotnet build TravelCore.sln

dotnet test tests/Unit/TravelCore.Modules.HotelBooking.UnitTests/TravelCore.Modules.HotelBooking.UnitTests.csproj

dotnet test tests/Unit/TravelCore.Modules.Payment.UnitTests/TravelCore.Modules.Payment.UnitTests.csproj

dotnet test tests/Unit/TravelCore.Modules.Booking.UnitTests/TravelCore.Modules.Booking.UnitTests.csproj

dotnet test tests/Architecture/TravelCore.ArchitectureTests/TravelCore.ArchitectureTests.csproj

dotnet test tests/Integration/TravelCore.Persistence.IntegrationTests/TravelCore.Persistence.IntegrationTests.csproj

dotnet test tests/Integration/TravelCore.Host.IntegrationTests/TravelCore.Host.IntegrationTests.csproj

Frontend:

npm run typecheck
npm run lint
npm run build

git diff --check


Required Result Evidence:

Report exact:

- HotelBooking Unit test count
- Payment Unit test count
- Booking Unit test count
- Architecture test count
- Persistence Integration test count
- Host Integration test count
- frontend typecheck
- frontend lint
- frontend production build
- git diff --check

Also report:

- evidence pack path
- product-code defect found: YES/NO
- correction commit if any
- HotelBooking schema
- peer-schema FK
- shared DbContext
- peer Infrastructure dependency
- cross-schema SQL
- Hotel Catalog owner
- live availability authority
- production Availability Source
- production Rate Source
- production Reservation Source
- Named Hotel Supplier
- supplier SDK
- Payment supported target kinds
- arbitrary Payment TargetType
- Production Payment Provider
- Payment provider SDK
- HotelBookingStatus exact values
- HoldStatus exact values
- SupplierReservationStatus exact values
- SupplierReservationAttemptStatus exact values
- CancellationStatus exact values
- SupplierCancellationAttemptStatus exact values
- PaymentStatus exact values
- PaymentAttemptStatus exact values
- RefundStatus exact values
- RefundAttemptStatus exact values
- multi-room supported
- child AgeAtCheckIn
- BirthDate stored
- passport/document stored
- Payment-only confirmation
- Supplier-only confirmation
- dual-evidence confirmation
- supplier timeout behavior
- cancellation timeout behavior
- partial penalty cancellation behavior
- partial cancellation supplier call count
- Partial Refund
- full-refund compensation
- PaymentStatus after Refund
- anonymous token header
- raw token persisted
- token URL exposure
- token localStorage
- missing token
- wrong token
- cross-user
- public HotelBooking list
- generic public CRUD
- client price authority
- client success authority
- card collection
- transactional noindex
- FA/EN/AR
- RTL/LTR/bidi
- mobile/accessibility
- operational read surface
- operational mutation surface
- smart routing/failover
- distributed transaction
- exactly-once claim
- outbox/inbox durability
- Tour Booking Payment regression
- public P21 route inventory
- frontend P21 route inventory
- static secret scan
- new P21 package dependencies
- P21-R1 through P21-R8 status
- deferred inventory
- P21 READY_FOR_GATE
- TC-P21-GATE NOT EXECUTED


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

- commit with TC-P21-T009 in commit message
- push main to origin/main using normal fast-forward push
- re-fetch origin
- verify HEAD == origin/main
- verify Working Tree CLEAN


Expected Baseline:
d8bdf0f


Auto-Execute:

After PASS:

- return TC-P21-T009 RESULT to architect
- do NOT execute TC-P21-GATE until T009 is architect ACCEPTED
- remain in PIPELINE


END_TRAVELCORE_CURSOR_TASK_V1
`
