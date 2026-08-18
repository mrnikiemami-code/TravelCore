# TC-P21-T008 Task Envelope

Captured live after `TC-P21-T007 = ACCEPTED`. `P21-R8 = RESOLVED`. Baseline `836cd92`.

```text
بررسی کامل انجام شد. نتیجه `T007` از نظر Scope، معماری، State transitions، Refund boundary و validation قابل قبول است.

```text
TC-P21-T007 = ACCEPTED

Implementation Commit:
c3fabe9

Current HEAD:
836cd92

HEAD == origin/main:
YES

Working Tree:
CLEAN
```

دلایل اصلی Acceptance:

```text
Confirmed HotelBooking cancellation
=
separate HotelBookingCancellation process
```

و وضعیت اصلی Booking دست‌کاری نشده:

```text
HotelBookingStatus:
- Pending
- Confirmed
- Cancelled
```

Cancellation process هم مستقل مانده:

```text
HotelBookingCancellationStatus:
- Requested
- SupplierCancellationPending
- RefundPending
- Completed
```

قانون مالی نیز دقیقاً مطابق تصمیم R7 پیاده شده:

```text
Penalty = 0
=> Full Refund

Penalty = TotalAmount
=> No Refund

0 < Penalty < TotalAmount
=> PartialRefundRequiredButUnsupported
```

مهم‌تر اینکه در حالت نیاز به Partial Refund هیچ Side Effect خارجی رخ نمی‌دهد:

```text
Supplier cancellation call = 0
HotelBooking remains Confirmed
SupplierReservation remains Confirmed
Refund = NONE
```

و ابهام شبکه نیز به Success یا Failure جعلی تبدیل نشده:

```text
Network timeout
!=
Supplier cancellation confirmed

Network timeout
!=
Supplier cancellation failed
```

در Timeout:

```text
Attempt = Initiated
Cancellation = SupplierCancellationPending
HotelBooking = Confirmed
SupplierReservation = Confirmed
Refund = NONE
```

Refund همچنان مالکیت Payment را حفظ کرده:

```text
HotelBooking decides refund requirement
Payment executes Refund
```

و این invariant نیز درست مانده:

```text
HotelBookingCancelled
!=
RefundSucceeded
```

Validation نهایی:

```text
HotelBooking.UnitTests: 97 PASS
Payment.UnitTests: 91 PASS
Booking.UnitTests: 54 PASS
ArchitectureTests: 306 PASS
Persistence.IntegrationTests: 109 PASS
Host.IntegrationTests: 57 PASS
Frontend touched: NO
git diff --check: PASS
```

بنابراین:

```text
P21-R7 = RESOLVED
```

و طبق حالت خودکار معماری، تصمیم بعدی نیز قفل می‌شود:

```text
P21-R8 = RESOLVED
```

تصمیم R8 این است که Public HotelBooking یک Flow تراکنشی امن باشد، نه CRUD عمومی؛ Anonymous booking توکن مستقل خودش را داشته باشد؛ Payment و Cancellation فقط در Scope همان HotelBooking قابل استفاده باشند؛ صفحات تراکنشی `noindex` باشند؛ و Operational surface فقط Read-only بماند.

Envelope کامل Task:

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version:
1

Task-ID:
TC-P21-T008

Phase:
P21

Title:
Public HotelBooking journey, anonymous authorization, payment/cancellation UX, privacy, operational reads, and supplier readiness

Baseline:
836cd92

Decision:
P21-R8 = RESOLVED

Purpose:
Complete the public and operational boundary for P21 Hotel Booking.

Expose the minimum secure HotelBooking customer journey around the already
accepted P21-R1 through P21-R7 domain model:

- HotelBooking initiation
- private HotelBooking read
- authoritative availability/rate progression
- HotelBooking-scoped Payment initiation/read
- payment / supplier / confirmation status presentation
- confirmed HotelBooking cancellation request where the R7 policy is executable
- truthful compensation/refund presentation
- anonymous and authenticated object-level authorization
- private transactional frontend
- read-only operational visibility
- provider/source readiness without selecting a real supplier/provider

Do NOT implement a real Hotel supplier or Payment provider.

Do NOT implement Partial Refund, PayAtProperty, deposits, amendments, supplier
settlement, accounting, smart supplier routing, or generic HotelBooking CRUD.

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

836cd92


2. Record T007 acceptance

Synchronize authoritative SoT to record:

TC-P21-T007 = ACCEPTED

Preserve all P21-R1 through P21-R7 semantics.


3. Public HotelBooking posture

Public HotelBooking is a transactional journey.

It is NOT generic CRUD.


4. No generic public list

Do NOT expose:

GET /api/hotel-bookings

or equivalent anonymous/general listing.


5. No arbitrary by-id authority

HotelBookingId alone must never authorize access.


6. Public initiation endpoint

Introduce one behavior-oriented endpoint for starting a HotelBooking transaction.

Preferred conceptual route:

POST /api/hotel-booking/public/initiations

or repository-consistent equivalent.


7. Initiation request

Public request may contain customer transaction intent required by accepted R2:

- PlaceId / hotel reference
- CheckInDate
- CheckOutDate
- room compositions
- guests
- lead guest
- contact snapshot

Do NOT accept authoritative:

- availability success
- rate amount
- CurrencyCode
- cancellation penalty
- Payment success
- Supplier confirmation


8. Occupancy authority

Room/guest composition accepted into HotelBooking becomes transaction intent.

Availability/rate sources evaluate that structure.

Client cannot later provide a contradictory second occupancy structure to
downstream source calls.


9. Hotel reference

Client may identify the hotel catalog Place being requested.

The server must validate it through accepted Place contract/boundary if current
repository already provides an appropriate neutral read.

Do NOT query Place persistence directly.


10. No Place ownership transfer

HotelBooking does not own hotel catalog truth.


11. Anonymous initiation

Support anonymous HotelBooking initiation.


12. Anonymous credential

Introduce HotelBooking-specific private access credential:

X-TravelCore-Hotel-Booking-Access-Token


13. Token independence

Preserve:

HotelBooking access token
!=
Tour Booking access token


14. Raw token issuance

Generate a high-entropy raw HotelBooking access token.

Return it only at successful initiation according to accepted P19-style
credential convention.


15. Token persistence

Persist only verifier/hash.

Do not persist raw token.


16. Hashing

Use accepted cryptographic verifier convention, preferably aligned with P19.

Do not invent reversible encryption for authorization token storage.


17. Raw token URL prohibition

Raw HotelBooking token must NOT appear in:

- path
- query string
- redirect URL
- provider callback URL


18. Client storage

Reuse accepted frontend private-transaction mechanism:

sessionStorage

unless repository current convention has changed.

Do NOT use localStorage.


19. BookingId != credential

Hard requirement.


20. PaymentId != credential

Hard requirement.


21. SupplierReservationId != credential

Hard requirement.


22. Anonymous private read

Expose:

GET /api/hotel-booking/public/{hotelBookingId}

or repository-equivalent.


23. Missing token

Expected non-enumerating result:

404


24. Wrong token

Expected:

404


25. Unknown HotelBooking

Expected:

404


26. Correct token

May read only that HotelBooking.


27. Authenticated object-level authorization

Authenticated user access must still enforce object ownership/accepted actor
relationship.

Preserve:

Authenticated
!=
authorized for arbitrary HotelBooking


28. Cross-user

User A must not access HotelBooking belonging to User B.


29. No enumeration leak

Unauthorized responses must not reveal whether:

- HotelBooking exists
- Payment exists
- supplier reservation exists
- Refund exists
- cancellation process exists


30. Optional actor linkage

If current HotelBooking contains an accepted logical actor/Party/User reference,
reuse it.

Do not invent a new Identity/Party model.


31. Initiation idempotency

Public creation must support database-backed request idempotency.


32. Idempotency key

Use repository-consistent header/contract.

Repeated same initiation request must not create duplicate HotelBookings.


33. Concurrent initiation

Same idempotency key concurrently:

one effective HotelBooking.


34. No process-local idempotency

Hard requirement.


35. HotelBooking creation

A successful initiation creates:

HotelBookingStatus = Pending


36. No false availability

Creation of HotelBooking does NOT mean:

rooms available
rate accepted
hold active
supplier reservation confirmed


37. Public journey orchestration

Implement a constrained application orchestration for progressing a Pending
HotelBooking through accepted stages.


38. Stage separation

Public/customer-safe stages may be derived from domain facts.

Do NOT add new HotelBookingStatus values such as:

AwaitingAvailability
AwaitingRate
AwaitingPayment
BookingSupplier
Refunding


39. Customer-safe view state

Introduce a presentation/API read model if useful.

It may express safe actions/states such as:

- NeedsAvailability
- AvailabilityPending
- HoldActive
- NeedsRate
- RateAccepted
- PaymentUnavailable
- ReadyForPayment
- PaymentPending
- PaymentReceived
- SupplierReservationPending
- Confirmed
- CancellationAvailable
- CancellationPending
- RefundPending
- Cancelled
- ReconciliationRequired

These are READ/PRESENTATION states only.

They must NOT become domain lifecycle enums.


40. No mega domain status

Hard requirement.


41. Availability progression endpoint

If a separate action is useful, expose a behavior endpoint such as:

POST /api/hotel-booking/public/{hotelBookingId}/availability

or repository-equivalent.

Alternatively orchestration may run as part of initiation.

Choose the smallest repository-consistent public contract.


42. Availability authorization

Must use HotelBooking object authorization.


43. Availability source selection

Server-controlled only.


44. No public arbitrary source key

Do NOT accept arbitrary Supplier/AvailabilitySource class/type from client.


45. No production availability source

Current production posture remains:

NONE


46. Zero-source behavior

When no production availability source is configured:

return truthful unavailable/service-not-configured response.

Do NOT fabricate an Active hold.


47. Availability result privacy

Do not expose source diagnostics/raw response.


48. Rate progression

Expose/compose rate-offer acquisition only through authorized HotelBooking context.


49. No client price

Client cannot submit authoritative total/currency.


50. No production rate source

Current:

NONE


51. Zero-rate-source behavior

Truthful unavailable result.

No fake HotelRateOfferSnapshot.


52. Rate acceptance consent

If authoritative source returns an offer, the customer-facing flow must present:

- total
- CurrencyCode/display unit
- cancellation terms
- rate expiry

before payment progression.


53. Customer acceptance

If explicit customer acceptance is required by current API structure, persist only
the accepted authoritative offer identity/reference.

Do not accept client-reconstructed monetary values.


54. Expired offer

Cannot proceed to payment.

Return requote/offer-expired customer-safe outcome.


55. Silent repricing

Forbidden.


56. Cancellation disclosure

Before Payment initiation, customer-facing view must make accepted cancellation
terms materially visible.


57. Partial-refund dependency disclosure

If a selected Hotel cancellation policy includes a partial-penalty interval, the
system may display those contract terms.

Do NOT imply such cancellation is currently executable.


58. Public Payment route

Expose HotelBooking-scoped Payment read/initiation only after object authorization.

Preferred conceptual routes:

GET  /api/hotel-booking/public/{hotelBookingId}/payment

POST /api/hotel-booking/public/{hotelBookingId}/payment/initiation

or repository-equivalent.


59. No generic Hotel Payment endpoint

Do NOT create target-type generic public Payment API.


60. Payment authorization

HotelBooking access authorization gates Hotel Payment access.


61. HotelBookingId only

Cannot read/initiate Payment.


62. PaymentId only

Cannot authorize.


63. Payment request

Must NOT accept authoritative:

- Amount
- CurrencyCode
- PaymentId
- IsPaid
- Success
- Provider implementation type


64. Payment provider selection

Server-controlled.


65. Production Payment Provider

Remain:

NONE


66. No-provider Payment result

Truthful:

503 / unavailable

according to accepted P20/R7 convention.


67. No fake provider redirect

Hard requirement.


68. Payment already Succeeded

No new provider call/Attempt.


69. Unresolved PaymentAttempt

No unsafe retry.


70. Definitively Failed PaymentAttempt

Explicit retry remains allowed under P20.


71. Payment UI

TravelCore frontend may display:

- booking summary
- total
- currency/display unit
- cancellation terms
- payment status
- Continue to payment action

No raw card input.


72. Card data

Do NOT collect:

- PAN
- card number
- CVV/CVC
- PIN
- banking password


73. Provider-hosted payment

Future production Payment remains external provider-hosted where adapter supports
it.


74. Payment browser return

Introduce HotelBooking private return route if required:

/[locale]/hotel-bookings/[hotelBookingId]/payment/return

or repository-equivalent.


75. Browser return != Payment success

Hard requirement.


76. Return route

Status/navigation only.


77. No token in return URL

Hard requirement.


78. Provider callback

Existing Payment technical callback remains separate.


79. Supplier reservation progression

After authoritative Payment success, public orchestration may show/trigger the
already accepted R6 durable supplier-reservation continuation.


80. Do not synchronously require supplier completion in Payment callback

Hard requirement.


81. Supplier reservation source

Server-controlled.


82. Production Reservation Source

Remain:

NONE


83. No supplier fake production success

Hard requirement.


84. Supplier reservation pending UX

Support truthful state:

Payment received; hotel confirmation is being processed.


85. Payment Succeeded != HotelBooking Confirmed

Must be visible in API/UI semantics.


86. Supplier Confirmed only != PayNow HotelBooking Confirmed

Preserve R6 dual-evidence rule.


87. Confirmed UX

Only show confirmed when:

HotelBookingStatus = Confirmed


88. Supplier confirmation code

Customer-safe view may expose supplier confirmation/reference only if it is a
human-facing safe confirmation code.

Do not expose internal diagnostic/source correlation IDs unnecessarily.


89. Booking private frontend route

Introduce:

/[locale]/hotel-bookings/[hotelBookingId]

or repository-equivalent.


90. Booking initiation frontend route

Inspect current Place/Hotel public routes.

Prefer a child booking journey associated with the current hotel catalog route.

Examples:

/[locale]/hotels/[slug]/book

or equivalent.

Do NOT invent a conflicting catalog route.

Use actual repository route ownership.


91. If no hotel-specific public catalog route exists

Use the accepted Place route as source context and create the smallest compatible
booking entry route.

Document exact chosen route.


92. Transactional page noindex

Private HotelBooking pages:

index = false


93. Payment return noindex

Hard requirement.


94. Search boundary

HotelBooking transactions must not enter Search index.


95. SEO boundary

SEO remains canonical/index policy owner.

Transactional booking pages are private/noindex.


96. Frontend architecture

Server Component First.


97. Minimal Client Components

Use Client Components only for:

- interactive room/guest forms
- sessionStorage credential handling
- mutation actions
- live status refresh where required


98. Mobile-first

HotelBooking form/journey must be designed for narrow viewport first.


99. Multi-room UX

Room 1..N must clearly show each room's assigned guests.


100. Guest assignment

Do not flatten multi-room guests into an ambiguous global list in customer UI.


101. Child age UX

Collect/display:

AgeAtCheckIn

not DateOfBirth.


102. Lead guest

Make lead guest selection clear.


103. Contact

Separate booking contact from lead guest identity in UI semantics.


104. Accessibility

At minimum:

- semantic headings
- proper field labels
- grouped room/guest controls
- keyboard navigation
- visible focus
- accessible validation
- status announcements
- loading state
- errors not conveyed by color alone


105. RTL/LTR

Support:

FA
EN
AR


106. Direction-neutral

Use logical layout/CSS.


107. Money bidi

Use accepted Money/bidi helpers.


108. Toman

Preserve:

Toman != CurrencyCode


109. Cancellation-policy display

Display structured cancellation terms in customer-readable form.

Do not expose raw internal policy enums/JSON.


110. Cancellation availability

Customer cancellation action is visible only when HotelBooking is Confirmed and
R7 can evaluate an executable outcome.


111. Public cancellation endpoint

Expose a HotelBooking-scoped behavior endpoint such as:

POST /api/hotel-booking/public/{hotelBookingId}/cancellation

or repository-equivalent.


112. Cancellation authorization

Requires HotelBooking object authorization.


113. No cancellation by ID alone

Hard requirement.


114. Cancellation request amount

Client does NOT submit Refund amount or Penalty amount.


115. Cancellation RequestedAt

Server IClock determines authoritative RequestedAt.


116. Cancellation policy source

Immutable HotelCancellationPolicySnapshot only.


117. Full Refund case

Public API may request R7 cancellation.

After authoritative supplier cancellation:

HotelBooking -> Cancelled

Refund follows Payment-owned path.


118. No Refund case

After authoritative supplier cancellation:

HotelBooking -> Cancelled

no Refund.


119. Partial penalty case

Public API must return explicit unsupported/blocked result.

Expected:

no supplier cancellation call
HotelBooking remains Confirmed.


120. Cancellation timeout UX

If supplier cancellation outcome is ambiguous:

show cancellation processing/pending state.

Do NOT display Cancelled.


121. Refund pending UX

HotelBooking may already be Cancelled while Refund is Pending.

Represent truthfully.


122. Refund succeeded UX

Represent money returned.

Payment historical state remains Succeeded internally.


123. No public Refund command

Do NOT expose:

Refund
RetryRefund
SetRefundStatus


124. No customer amendment UI

Do NOT expose:

change dates
change room
change guests
change rate


125. PayAtProperty

No UI/API.


126. Deposit

No UI/API.


127. Operational read boundary

Add a read-only internal operational HotelBooking query surface.


128. Operational query type

Use a contract/service such as:

IHotelBookingOperationalQuery

or repository-equivalent.


129. Operational route

If an accepted admin authorization layer is already reusable, a protected admin
route MAY be exposed.

If not, keep operational query internal only.


130. Do not invent admin authentication

Hard requirement.


131. Booking token cannot access ops

Hard requirement.


132. Operational facts

Read-only view may include:

- HotelBookingId
- PlaceId
- stay
- room count / occupancy summary
- HotelBookingStatus
- accepted rate/monetary snapshot
- Hold status
- supplier reservation status/attempts
- Payment/Refund summary through contracts
- cancellation process
- reconciliation issue summary


133. Operational privacy

Do not expose unnecessary full guest/contact PII.

Prefer redacted/minimal operational summary.


134. Provider/source references

Safe support view may include controlled:

SourceKey
SourceReservationReference
source attempt references

if operationally necessary.


135. No secrets

Never expose:

supplier credentials
Payment provider secrets
callback signatures
access tokens


136. No raw supplier payload

Hard requirement.


137. Operational mutation

Must remain:

NONE


138. No ForceConfirm

No.


139. No ForceCancel

No.


140. No MarkSupplierConfirmed

No.


141. No MarkPaid

No.


142. No MarkRefunded

No.


143. Operational recheck

Trusted internal operational use MAY trigger:

availability Hold recheck
supplier reservation recheck
supplier cancellation recheck
Payment/Refund recheck

only through existing authoritative source/provider query operations.


144. Operator does not choose result

Hard requirement.


145. Unsupported source query

Return safe capability-unavailable outcome.


146. Provider/source readiness

Define minimal descriptors/capabilities for future Hotel source adapters if R3-R7
ports currently lack explicit capability declarations.


147. Do not create giant supplier framework

Keep only capabilities already required by P21.


148. Candidate Hotel source capabilities

Only if needed, model neutral capabilities such as:

- AvailabilityCheck
- AvailabilityHold
- AvailabilityHoldQuery
- AvailabilityHoldRelease
- RateQuote
- ReservationCreate
- ReservationQuery
- ReservationCancel
- ReservationCancellationQuery


149. Capability explicitness

Capabilities must be declared, not inferred from source name.


150. Source descriptor

May contain safe:

- SourceKey
- enabled/configured state
- capabilities
- safe display name

No credentials.


151. Zero-source production

Host must remain valid with:

0 production hotel sources.


152. No arbitrary source public selection

Hard requirement.


153. No smart routing

Remain absent.


154. No failover

Remain absent.


155. No named supplier

Remain:

NONE


156. No supplier SDK

Remain:

NONE


157. Payment provider readiness

Do not reopen P20 provider capability architecture.


158. Production Payment Provider

Remain:

NONE


159. No Payment SDK

Remain none.


160. Security threat model regression

Add tests/evidence for:

- HotelBookingId enumeration
- missing/wrong token
- cross-user access
- token leakage
- price tampering
- occupancy tampering
- fake Payment success
- fake Supplier success
- browser-return trust
- duplicate booking initiation
- duplicate Payment initiation
- duplicate cancellation
- cancellation partial-refund bypass
- supplier callback/recheck spoofing
- operational endpoint exposure
- guest PII leakage
- provider/source secret leakage


161. Anonymous initiation test

Valid initiation returns:

HotelBookingId
raw access token once
safe initial public representation


162. Token persistence test

Raw token not persisted.


163. Missing-token read test

404.


164. Wrong-token read test

404.


165. Cross-user test

404/non-enumerating accepted equivalent.


166. BookingId-only test

Unauthorized.


167. Token isolation test

Tour Booking token cannot access HotelBooking.

HotelBooking token cannot access Tour Booking.


168. Duplicate initiation test

Same idempotency key:

same HotelBooking.


169. Different initiation key

Creates distinct HotelBooking only when explicitly requested.


170. Client amount tampering test

Cannot set accepted Hotel monetary snapshot.


171. Client currency tampering test

Cannot set CurrencyCode authority.


172. Client success tampering test

Cannot mark Payment/Supplier/HotelBooking success.


173. Occupancy tampering test

Downstream source request derives accepted HotelBooking structure.


174. Availability zero-source test

No Active hold fabricated.


175. Rate zero-source test

No Rate snapshot fabricated.


176. Payment zero-provider test

Safe unavailable/503.


177. Reservation zero-source test

No supplier Confirmed state fabricated.


178. Public Payment authorization tests

Missing/wrong HotelBooking token:

404

Correct token:
may access only its Payment.


179. Payment duplicate-initiation regression

No double charge Attempt.


180. Payment success / Hotel pending UX contract test

Public read distinguishes:

Payment Succeeded
HotelBooking Pending


181. Final confirmation test

Public read reports Confirmed only after actual HotelBooking Confirmed.


182. Public cancellation full-refund test

Authorized confirmed booking with Penalty=0:

request accepted
supplier cancellation flow invoked
full Refund path follows after authoritative cancellation.


183. Public cancellation no-refund test

Penalty=Total:

no Refund.


184. Public cancellation partial-penalty test

Blocked.

Supplier gateway call count = 0.


185. Cancellation wrong token

404.


186. Cancellation for Pending HotelBooking

Rejected.


187. Cancellation duplicate request

Idempotent.


188. Cancellation timeout public state

Not falsely Cancelled.


189. Refund pending public state

Truthful.


190. Refund succeeded public state

Truthful.


191. Frontend tests

Where repository conventions exist, cover:

- FA/EN/AR rendering
- multi-room guest forms
- private token handling
- no token URL
- noindex
- accessible action controls
- payment pending/received states
- cancellation policy display
- cancellation blocked for partial refund
- refund pending/completed messaging


192. No card fields static test

Verify frontend contains no customer card credential fields.


193. No localStorage token test

Verify HotelBooking raw access token is not persisted in localStorage.


194. Browser return security test

Cannot mark Payment/HotelBooking confirmed.


195. No public list test

Verify:

GET /api/hotel-booking/public

does not enumerate HotelBookings.


196. No generic CRUD test

No public PUT/PATCH status mutation.


197. No public operational route test

If operational query is internal only, verify likely routes return 404.


198. Booking token cannot access ops test

Hard requirement.


199. Operational read-only reflection test

No public operation methods such as:

SetStatus
ForceConfirm
ForceCancel
MarkPaid
MarkRefunded


200. PII operational test

Operational DTO contains no:

passport
national ID
document scans
access token
provider secret


201. Source descriptor tests

If capability model is added:

- zero sources valid
- duplicate SourceKey rejected
- disabled source unusable
- unknown source rejected
- unsupported capability rejected
- no source-name capability inference


202. No smart routing test

No automatic source fallback.


203. Persistence

Allowed R8 additions:

- HotelBooking anonymous credential verifier
- initiation idempotency
- private/public read metadata if needed
- operational read projections only if module-local
- no new lifecycle truth tables unless required by accepted R8 behavior


204. No cross-schema FK

Hard requirement.


205. No shared DbContext

Hard requirement.


206. No peer Infrastructure dependency

Hard requirement.


207. No cross-schema SQL joins

Hard requirement.


208. Payment composition

Use Payment.Contracts.


209. Place composition

Use Place.Contracts if required.


210. Search/SEO composition

Do not directly query their persistence.


211. Public API response privacy

Do not expose:

- internal reconciliation details
- raw source status
- source credentials
- provider request bodies
- Payment provider secrets
- access-token verifier/hash


212. Customer-safe errors

Map internal states to customer-safe messages.


213. Reconciliation state

Public UI may show neutral:

"We're checking your reservation"

or equivalent.

Do not expose raw reconciliation enum.


214. Source outage

Show truthful temporary unavailable state.


215. No fake booking promise

If no authoritative availability/rate/reservation source exists, do not render a
successful confirmed booking path from fake data in production.


216. Development/test source

Allowed only in test/dev configuration if repository conventions clearly isolate
it.

Report exact posture.


217. AI readiness

Keep structured HotelBooking read contracts stable and attributable.

Do NOT add:

LLM
embeddings
vector DB
RAG


218. Accounting

Not implemented.


219. Settlement

Not implemented.


220. Supplier settlement

Not implemented.


221. Agency commission

Not implemented.


222. Wallet

Not implemented.


223. Fraud/risk

Not implemented.


224. Loyalty

Not implemented.


225. Amendments

Remain DEFERRED.


226. Partial Refund

Remain DEFERRED.


227. PayAtProperty

Remain DEFERRED.


228. Deposit/partial payment

Remain DEFERRED.


229. Smart supplier routing

Remain DEFERRED.


230. Real supplier

Remain DEFERRED.


231. Real Payment provider

Remain DEFERRED.


232. Documentation

Update P21 plan/SoT to record:

P21-R8 = RESOLVED


233. R8 decision summary

Record exactly:

- public HotelBooking is transactional and behavior-oriented, not CRUD
- anonymous HotelBooking uses a HotelBooking-specific opaque access token
- raw token returned once; verifier/hash persisted
- raw token never enters URL
- BookingId/PaymentId/SupplierReservationId are not credentials
- authenticated callers require object-level authorization
- initiation is idempotent
- customer amount/currency/success are never authoritative
- availability/rate/reservation sources remain server-controlled
- zero production Hotel sources is a valid host posture
- no fake production availability/rate/reservation truth
- HotelBooking Payment access is HotelBooking-scoped
- Payment provider selection remains server-controlled
- no raw card collection
- private HotelBooking/payment pages are noindex
- Payment Succeeded / HotelBooking Pending is a first-class customer state
- confirmed cancellation API uses R7 policy and supplier authority
- partial-refund-required cancellation remains blocked
- operational HotelBooking reads are read-only
- no manual financial/booking truth mutation exists
- Hotel source/provider capabilities, if modeled, are explicit
- no smart routing/failover
- Named Hotel Supplier = NONE
- Production Payment Provider = NONE
- Partial Refund / amendments / PayAtProperty / deposits remain DEFERRED


234. Decision status

Record:

P21-R8 = RESOLVED


235. P21 decisions

After T008 implementation:

P21-R1 through P21-R8 = RESOLVED


236. P21 status

Remain:

IN_PROGRESS


237. Do not execute T009

TC-P21-T009 = NOT EXECUTED


Allowed:

- public HotelBooking initiation/read
- HotelBooking-specific anonymous token
- object-level authorization
- initiation idempotency
- HotelBooking-scoped availability/rate progression
- HotelBooking-scoped Payment read/initiation
- HotelBooking private payment return route
- public confirmed-cancellation request using R7
- customer-safe transactional read model
- truthful compensation/refund state
- private HotelBooking frontend
- mobile/a11y/bidi/noindex implementation
- internal read-only operational query
- minimal Hotel source capability descriptors if necessary
- unit/architecture/persistence/host/frontend tests
- SoT synchronization for R8


Forbidden:

- P21-T009 execution
- generic HotelBooking CRUD
- public HotelBooking list
- BookingId/PaymentId as credential
- Tour Booking token reuse
- raw token URL
- raw token localStorage
- client-authoritative amount/currency
- client-authoritative Payment/Supplier success
- arbitrary public source/provider selection
- real supplier
- real supplier SDK
- real Payment provider
- Payment SDK
- fake production source
- card collection
- public Refund command
- Partial Refund
- public partial-penalty cancellation execution
- amendments
- PayAtProperty
- deposits
- installments
- smart supplier routing/failover
- operational mutation
- public insecure admin route
- Search indexing of transactions
- SEO indexing of private routes
- peer-schema FK
- shared DbContext
- peer Infrastructure dependency
- cross-schema SQL
- accounting
- settlement
- unrelated refactor
- dependency upgrades


Done:

- secure public HotelBooking transaction initiation exists
- anonymous HotelBooking has independent private credential
- missing/wrong/cross-user access does not leak existence
- duplicate initiation is idempotent
- client cannot fabricate occupancy downstream, price, currency, Payment success,
  supplier success, cancellation economics, or Refund amount
- no source/provider configured is represented honestly
- no fake production booking path exists
- HotelBooking Payment is scoped by HotelBooking authorization
- Payment success and HotelBooking confirmation remain distinct
- confirmed cancellation safely exposes only R7-supported outcomes
- partial penalty cancellation remains blocked
- private HotelBooking pages are noindex
- no card credentials are collected
- FA/EN/AR, RTL/LTR, mobile and accessibility requirements hold
- operational HotelBooking visibility is read-only
- provider/source readiness does not invent a concrete supplier
- P21-R8 recorded RESOLVED
- P21-R1 through P21-R8 all RESOLVED
- P21 remains IN_PROGRESS
- T009 is NOT executed


Validation:

Run:

dotnet build TravelCore.sln

HotelBooking.UnitTests

Payment.UnitTests

Booking.UnitTests

ArchitectureTests

Persistence.IntegrationTests

Host.IntegrationTests

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

- public HotelBooking initiation route
- public HotelBooking private-read route
- public availability/rate action route(s), if separate
- HotelBooking Payment read route
- HotelBooking Payment initiation route
- public cancellation route
- private HotelBooking frontend route
- payment return frontend route
- booking entry frontend route
- anonymous access-token header exact name
- raw token returned once: YES/NO
- raw token persistence: YES/NO
- verifier/hash persistence: YES/NO
- token URL exposure: NO
- token localStorage: NO
- token sessionStorage posture
- missing token result
- wrong token result
- correct token result
- cross-user result
- HotelBookingId-only authorization result
- PaymentId-only authorization result
- Tour token -> HotelBooking result
- Hotel token -> Tour Booking result
- initiation idempotency result
- client amount tampering result
- client currency tampering result
- client success tampering result
- occupancy downstream source-of-truth result
- zero Availability Source result
- zero Rate Source result
- zero Reservation Source result
- zero Payment Provider result
- fake production source/provider: NO
- Payment Succeeded / HotelBooking Pending public state
- confirmed public state source
- partial-refund-required cancellation public result
- partial cancellation supplier call count
- cancellation timeout public result
- RefundPending public result
- RefundSucceeded public result
- public Refund command: NO
- card collection: NO
- public HotelBooking list: NO
- generic CRUD/status mutation: NO
- transactional routes noindex: YES
- FA/EN/AR: YES
- RTL/LTR/bidi: PASS
- mobile/accessibility: PASS
- operational HotelBooking read surface
- operational authorization mechanism
- HotelBooking token can access ops: NO
- operational mutation surface: NONE
- Hotel source capability exact values if modeled
- Named Hotel Supplier: NONE
- Production Hotel Availability Source: NONE
- Production Hotel Rate Source: NONE
- Production Hotel Reservation Source: NONE
- Production Payment Provider: NONE
- real supplier/provider SDK: NO
- Partial Refund: NO
- amendments: NO
- PayAtProperty: DEFERRED
- deposit/partial payment: DEFERRED
- smart routing/failover: NO
- peer-schema FK: NO
- shared DbContext: NO
- peer Infrastructure dependency: NO
- distributed transaction: NO
- P21-R1 through P21-R8: RESOLVED
- TC-P21-T009: NOT EXECUTED


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

- commit with TC-P21-T008 in commit message
- push main to origin/main using normal fast-forward push
- re-fetch origin
- verify HEAD == origin/main
- verify Working Tree CLEAN


Expected Baseline:
836cd92


Auto-Execute:

After PASS:

- return TC-P21-T008 RESULT to architect
- do NOT execute TC-P21-T009 until T008 is architect ACCEPTED
- remain in PIPELINE


END_TRAVELCORE_CURSOR_TASK_V1
```
``` 

