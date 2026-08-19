# TC-P22-T008 Task Envelope (architect, live)

Captured from the same ChatGPT tab after `TC-P22-T007 = ACCEPTED` and `P22-R8 = RESOLVED`. Envelope baseline `0c39a60`. Working HEAD at start: `1b344b9`.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1
Task-ID: TC-P22-T008
Phase: P22
Title: Public Flight search/booking journey, private authorization, ticket/payment UX, privacy, and operations
Baseline: 0c39a60
Decision: P22-R8 = RESOLVED

Purpose:
Expose the minimum secure public Flight journey using the already accepted
P22-R1 through P22-R7 domain model.

Implement:
- public Flight search
- authorized FlightBooking initiation/read
- offer selection/acceptance
- PNR progression
- FlightBooking-scoped Payment
- ticketing/confirmation presentation
- supported confirmed cancellation
- Flight-specific anonymous authorization
- mobile/accessible FA/EN/AR frontend
- internal read-only operational view

Do NOT implement:
- real Flight supplier/provider
- real Payment provider
- Partial Refund
- multi-city
- ancillaries
- amendments/rebooking
- per-passenger cancellation
- smart supplier routing/failover
- generic CRUD

1. Repository preflight

Run:

git rev-parse --show-toplevel
git fetch origin

Require:

branch = main
HEAD == origin/main
Working Tree = CLEAN

Use actual current HEAD after fetch as authoritative baseline.
Expected accepted lineage includes:

0c39a60


2. SoT

Record:

TC-P22-T007 = ACCEPTED
P22-R8 = RESOLVED

Keep:

P22 = IN_PROGRESS
TC-P22-T009 = NOT EXECUTED


3. Public posture

Flight public surface is behavior-oriented, not CRUD.

Do NOT expose:

GET /api/flight-bookings
PUT/PATCH arbitrary FlightBooking
status mutation endpoints


4. Public search

Expose a Flight search endpoint using R3:

IFlightSearchSource

Support baseline:

- OneWay
- RoundTrip
- Adult / Child / Infant
- connecting itineraries

MultiCity remains DEFERRED.


5. Search request authority

Client may provide search intent only:

origin
destination
dates
trip type
passenger counts

Do NOT accept:

price authority
availability success
supplier implementation type
PNR
ticket success


6. Zero-source search

With no configured production Flight source:

return truthful unavailable/service-not-configured result.

Do not fabricate search options.


7. Booking initiation

Expose behavior-oriented initiation from a selected source option.

Preferred route shape:

POST /api/flight-booking/public/initiations

or repository-consistent equivalent.


8. Initiation idempotency

Database-backed.

Same idempotency key:

same effective FlightBooking.


9. Flight access credential

Introduce exact header:

X-TravelCore-Flight-Booking-Access-Token


10. Token isolation

Flight token is independent from:

Tour Booking token
HotelBooking token


11. Token security

Require:

- cryptographically secure raw token
- raw token returned once
- only hash/verifier persisted
- raw token never appears in URL
- no localStorage
- sessionStorage allowed for private frontend flow


12. Authorization

FlightBookingId alone is never authorization.

Missing/wrong/cross-user token must produce non-enumerating denial, preferably 404
consistent with existing private transaction conventions.


13. Private read

Expose:

GET /api/flight-booking/public/{flightBookingId}

or equivalent.

Response must contain only customer-safe transaction facts.


14. Offer action

Expose authorized offer/revalidation/acceptance action using R4.

Client cannot submit authoritative:

Amount
CurrencyCode
Taxes
Fees
FareRules
OfferExpiresAt


15. Offer UX

Before progressing to reservation/payment, display:

- itinerary
- total
- currency/display unit
- baggage facts if present
- fare/cancellation rules
- offer expiry
- ticketing deadline if present


16. Silent repricing

Forbidden.

Changed/expired offer must return customer-safe requote/expired state.


17. PNR progression

Expose/compose authorized reservation progression using R5.

Do not let client mark PNR Confirmed.


18. Zero Reservation Source

Truthful unavailable result.

No fake PNR.


19. PNR public state

Customer-safe representation may show:

ReservationPending
ReservationConfirmed
ReservationExpired

These are presentation states, not new FlightBookingStatus values.


20. PNR != FlightBooking confirmation

Hard requirement.


21. Payment routes

Expose FlightBooking-scoped Payment:

GET /api/flight-booking/public/{flightBookingId}/payment

POST /api/flight-booking/public/{flightBookingId}/payment/initiation

or equivalent.


22. Payment authorization

Flight access token gates Payment.

PaymentId alone cannot authorize.


23. Payment request

Client cannot submit:

Amount
CurrencyCode
Payment success
Payment provider implementation


24. Payment provider

Server-controlled.

Production Payment Provider remains:

NONE


25. No-provider result

Truthful unavailable/503.

No fake redirect.


26. No card collection

Do NOT collect:

PAN
card number
CVV/CVC
PIN
bank password


27. Payment return

Introduce private return route if required:

/[locale]/flight-bookings/[flightBookingId]/payment/return

Browser return is navigation/status only.

It cannot mark Payment succeeded.


28. Ticketing progression

After authoritative Payment success, expose truthful processing state while R6
durable ticketing proceeds.

Do not require ticketing to complete synchronously inside Payment callback.


29. Zero Ticketing Source

No fake ticket issuance.


30. Ticket public state

Customer-safe read may expose:

- ticket pending
- ticket issued
- booking confirmed

Do not expose internal attempt/reconciliation details.


31. Final confirmation

Public response reports Confirmed only when:

FlightBookingStatus = Confirmed


32. Ticket number

May expose customer-safe e-ticket number only for the authorized FlightBooking.

Ticket number is NOT a credential.


33. Partial ticketing

If only some passenger tickets are issued:

do not report Booking Confirmed.

Show neutral processing/reconciliation state.


34. Confirmed cancellation endpoint

Expose authorized behavior endpoint:

POST /api/flight-booking/public/{flightBookingId}/cancellation

or equivalent.


35. Cancellation economics

Client does not send:

PenaltyAmount
RefundAmount


36. R7 rules

Public cancellation supports only:

FullRefund
NoRefund

Partial-refund-required outcome returns explicit blocked result.


37. Partial refund safety

For partial penalty:

supplier reversal call count must remain 0.


38. Cancellation pending UX

Ambiguous supplier reversal:

do not show Cancelled.


39. Refund pending UX

Booking may be Cancelled while Payment Refund is pending.

Represent truthfully.


40. No public Refund command

No:

Refund
RetryRefund
SetRefundStatus


41. Anonymous/private frontend routes

Implement repository-consistent routes for:

- Flight search
- Flight results/selection
- Flight booking details
- Flight payment
- Payment return
- confirmation/ticket state

Inspect existing route conventions before choosing exact paths.

Report them.


42. SEO

Search/transaction route policy must respect SEO ownership.

Private booking/payment/confirmation pages:

noindex


43. Search result SEO

Do not guess global indexation.

Use existing SEO policy; dynamic transactional search results should not become
uncontrolled indexable pages.


44. Frontend architecture

Server Component First.

Use Client Components only where needed for:

- search form
- passenger interaction
- token/sessionStorage handling
- mutations/status refresh


45. Mobile-first

Flight search and passenger forms must be usable on narrow mobile screens.


46. Accessibility

Require:

- labels
- grouped passenger controls
- keyboard navigation
- focus-visible
- accessible errors
- status announcements
- no color-only state


47. FA/EN/AR

Support all three.


48. Bidi

Direction-neutral layout.

Use accepted bidi/money primitives.


49. Flight time presentation

Show departure/arrival with appropriate local airport timezone context.

Avoid ambiguous timezone-less times.


50. Passenger privacy

Public authorized read may expose only that Booking's passenger data.

Operational/public DTOs must not expose unnecessary sensitive data.


51. Current PII posture

T002 has only:

GivenName
FamilyName
PassengerCategory

Do not add BirthDate/passport/document fields in R8 merely for UI completeness.


52. Operational query

Introduce read-only internal:

IFlightOperationalQuery

or equivalent.


53. Operational surface

If no accepted admin authorization layer exists:

keep it internal-only.

Do NOT invent admin authentication.


54. Operational facts

May include safe summary of:

- FlightBooking status
- itinerary
- offer provenance
- reservation state
- Payment/Refund state via Contracts
- ticket state
- cancellation process
- reconciliation summary


55. Operational mutation

NONE.


56. Forbidden operational commands

No:

ForceConfirm
ForceTicket
MarkPaid
ForceCancel
MarkRefunded


57. Secrets/privacy

Never expose/log:

- access token
- token hash
- supplier credentials
- Payment provider secrets
- callback signature secrets
- raw supplier payload


58. Source selection

Server-controlled only.

No client source/provider selection.


59. Smart routing

Remain:

NO


60. Failover

Remain:

NO


61. Production matrix

Keep:

Named Flight Supplier = NONE
Production Flight Search Source = NONE
Production Flight Availability Source = NONE
Production Flight Offer Source = NONE
Production Flight Reservation Source = NONE
Production Flight Ticketing Source = NONE
Production Flight Cancellation Source = NONE
Production Payment Provider = NONE


62. Security tests

Cover:

- missing token
- wrong token
- cross-user
- BookingId-only access
- PaymentId-only access
- Flight/Tour/Hotel token isolation
- duplicate initiation
- client price tampering
- client success tampering
- fake PNR/ticket success
- browser return trust
- partial-refund cancellation bypass
- public list absence
- generic status mutation absence


63. Public flow tests

Cover:

- zero search source
- search OneWay/RoundTrip
- connecting result
- offer expired/requote
- zero reservation source
- PNR pending/confirmed presentation
- Payment unavailable without provider
- Payment Succeeded but ticket pending
- full confirmation only from FlightBookingStatus.Confirmed
- partial ticket state not confirmed
- cancellation full-refund
- cancellation no-refund
- partial penalty blocked
- cancellation timeout
- RefundPending / completed presentation


64. Frontend security tests/static checks

Verify:

- no token in URL
- no localStorage for Flight token
- no card fields
- transactional pages noindex
- FA/EN/AR
- RTL/LTR
- accessible controls


65. Architecture boundaries

No:

peer-schema FK
shared DbContext
peer Infrastructure dependency
cross-schema SQL
distributed transaction


66. No new domain lifecycle states

Do not expand:

FlightBookingStatus
FlightSupplierReservationStatus
PaymentStatus
RefundStatus

for presentation concerns.


67. Deferred

Remain:

Partial Refund
MultiCity
Ancillaries
PayLater
Deposit/Partial Payment
Amendments
Rebooking
No-show
Per-passenger cancellation
Partial-itinerary cancellation
Smart routing/failover
Real supplier/provider


68. SoT summary

Record:

P22-R8 = RESOLVED

with:

- public Flight is transactional, not CRUD
- Flight-specific anonymous token
- raw token once / verifier persisted / no URL / no localStorage
- object-level authorization
- live source/provider selection is server-controlled
- no fake production success
- customer cannot author price/Payment/PNR/ticket/cancellation economics
- PNR/Payment/ticket/Booking confirmation remain distinct
- Flight Payment is FlightBooking-scoped
- no card collection
- supported confirmed cancellation uses R7
- partial-refund cancellation remains blocked
- private transaction pages are noindex
- operational read is read-only/internal
- no smart routing/failover
- production Flight sources/provider remain NONE


69. Validation

Run:

dotnet build TravelCore.sln

Flight.UnitTests
Payment.UnitTests
Booking.UnitTests
HotelBooking.UnitTests
ArchitectureTests
Persistence.IntegrationTests
Host.IntegrationTests

Frontend:
npm run typecheck
npm run lint
npm run build

git diff --check


70. Required result evidence

Report exact:

- test counts
- frontend validation
- public search route
- FlightBooking initiation/read routes
- offer action route
- reservation action route
- Payment read/initiation routes
- cancellation route
- frontend route inventory
- access-token header
- raw token persisted YES/NO
- verifier persisted
- token URL exposure
- localStorage/sessionStorage
- missing/wrong/cross-user result
- Flight/Tour/Hotel token isolation
- duplicate initiation behavior
- client price/success authority
- zero-source/provider behavior
- Payment Succeeded / ticket pending state
- partial ticketing public state
- confirmed state authority
- partial-refund cancellation result and supplier call count
- cancellation timeout state
- RefundPending/RefundSucceeded state
- card collection
- public list
- generic CRUD/status mutation
- noindex
- FA/EN/AR
- RTL/LTR/bidi
- mobile/accessibility
- operational query type
- operational HTTP exposure
- operational mutation
- production source/provider matrix
- smart routing/failover
- peer-schema FK
- shared DbContext
- distributed transaction
- P22-R8 = RESOLVED
- P22-R1 through P22-R8 = RESOLVED
- TC-P22-T009 = NOT EXECUTED


71. Commit/push

After PASS:

- commit with TC-P22-T008
- push normal fast-forward
- re-fetch
- verify HEAD == origin/main
- verify clean working tree


72. Auto-Execute

Return TC-P22-T008 RESULT.

Do NOT execute TC-P22-T009.

END_TRAVELCORE_CURSOR_TASK_V1
```
