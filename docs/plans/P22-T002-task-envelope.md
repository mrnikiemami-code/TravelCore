# TC-P22-T002 Task Envelope (architect, live)

Captured from the same ChatGPT tab after `TC-P22-T001 = ACCEPTED` and `P22-R2 = RESOLVED`.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1
Task-ID: TC-P22-T002
Phase: P22
Title: FlightBooking core journey, segment, reference, passenger, and temporal model
Baseline: a31654a
Decision: P22-R2 = RESOLVED

Purpose:
Implement the FlightBooking core transaction structure only.

This task defines:
- FlightBooking aggregate
- OneWay / RoundTrip structure
- journeys and connecting segments
- Airport/Airline logical reference semantics
- passenger categories
- flight temporal invariants

Do NOT implement search, live availability, offers/fares, supplier adapters, PNR,
ticketing, Payment, cancellation/refund, public API, or frontend.

1. Repository safety

Discover repository root:

git rev-parse --show-toplevel

Then require:

git fetch origin
branch = main
HEAD == origin/main
Working Tree = CLEAN

Expected baseline:

a31654a

2. SoT

Record:

TC-P22-T001 = ACCEPTED
P22-R2 = RESOLVED

Keep:

P22-R3 through P22-R8 = OPEN
P22 = IN_PROGRESS
TC-P22-T003 = NOT EXECUTED

3. FlightBooking aggregate

Implement:

FlightBooking
FlightBookingId

FlightBookingId must use accepted UUIDv7 conventions.

FlightBooking remains owned by Flight module/schema.

4. No lifecycle status yet

Do NOT introduce:

FlightBookingStatus

Lifecycle semantics belong to later reservation/ticketing/payment decisions.

5. Trip type

Introduce exactly:

FlightTripType:
- OneWay
- RoundTrip

MultiCity remains DEFERRED.

6. Journey structure

A FlightBooking contains:

1..N FlightJourneys

Use repository-consistent naming if a clearly better existing convention exists.

7. OneWay invariant

OneWay must contain exactly:

1 journey

8. RoundTrip invariant

RoundTrip must contain exactly:

2 journeys

Conceptually:

Outbound
Return

9. No MultiCity workaround

Do NOT represent MultiCity as malformed RoundTrip or arbitrary extra journeys.

10. Journey identity/order

Each journey must have:

FlightJourneyId
Ordinal

Ordinal defines deterministic customer itinerary order.

11. Segment structure

Each FlightJourney contains:

1..N FlightSegments

12. Connecting flights

Multiple segments in one journey are supported.

Example:

THR -> IST
IST -> LHR

is one journey with two segments.

13. Segment identity/order

Each segment must have:

FlightSegmentId
Ordinal

14. No separate Leg concept

Do NOT add FlightLeg in baseline.

Use:

Journey
Segment

as the canonical P22 terminology.

15. Airport ownership

Lock architectural authority:

ReferenceData
=
Airport reference/catalog authority

16. Airline ownership

Lock architectural authority:

ReferenceData
=
Airline/carrier reference/catalog authority

17. Do not implement ReferenceData catalogs

Do NOT modify ReferenceData in T002.

Airport/Airline catalogs may be implemented later if actually needed.

18. Flight references

Flight uses logical references/value objects only.

Introduce minimal concepts such as:

AirportReference
AirlineReference

or repository-equivalent.

19. No peer-schema FK

No FK to ReferenceData.

20. Airport reference

A segment requires:

OriginAirport
DestinationAirport

21. Airport identifier posture

Prefer a stable logical ReferenceData identifier if current architecture has an
appropriate reference type.

If no implemented airport entity exists yet, use a minimal validated external
reference value such as IATA code without pretending a catalog row exists.

Document the exact choice.

22. IATA posture

If IATA code is used:

- uppercase normalized
- exactly 3 ASCII letters
- not treated as globally immutable internal entity identity

23. ICAO

Do NOT require ICAO in baseline.

24. Airline reference

Each segment may carry the minimum operating/marketing carrier reference required
for itinerary truth.

Do not create Airline aggregate inside Flight.

25. Codeshare

Model only if needed without overbuilding.

Preferred minimal posture:

MarketingCarrier
OperatingCarrier optional

If both are absent from current business need, document and keep only the minimum
necessary reference.

26. Flight number

Segment may contain an optional/source flight number fact.

Do not treat flight number as globally unique.

27. No aircraft/seat inventory

Do NOT implement:

Aircraft
SeatMap
Seat
CabinInventory

28. Temporal model

Use NodaTime only.

29. Segment departure

Store authoritative departure instant plus local airport-facing representation
needed for correct display.

Preferred model:

DepartureAt : Instant
DepartureTimeZoneId : IANA timezone identifier

30. Segment arrival

Likewise:

ArrivalAt : Instant
ArrivalTimeZoneId : IANA timezone identifier

31. Temporal invariant

Require:

ArrivalAt > DepartureAt

32. Timezone safety

Do NOT use machine local timezone as business authority.

33. Overnight/timezone crossings

Model must naturally support:

arrival local date != departure local date

34. Journey continuity

For consecutive segments:

previous destination airport
=
next origin airport

35. Journey chronological order

For consecutive segments:

next.DepartureAt >= previous.ArrivalAt

Do not hardcode a minimum connection duration yet.

36. Journey origin/destination

Derived from:

first segment origin
last segment destination

37. RoundTrip consistency

For baseline RoundTrip:

return journey origin
=
outbound final destination

return journey destination
=
outbound initial origin

38. Passenger model

Introduce:

FlightPassenger
FlightPassengerId

39. Passenger categories

Exactly:

Adult
Child
Infant

40. At least one passenger

FlightBooking must contain >= 1 passenger.

41. Adult requirement

Require at least one Adult in baseline.

Do not allow infant-only/child-only FlightBooking.

42. Passenger names

Booking transaction may store minimum identity:

GivenName
FamilyName

Use existing validated text conventions.

43. Passenger category != hotel category

Do not reuse HotelBooking passenger/guest domain types directly.

44. BirthDate

Do NOT store BirthDate in T002.

45. Age

Do NOT hardcode airline age boundary calculations yet.

Passenger category is accepted transaction intent in R2.

Exact supplier/fare validation belongs later.

46. Infant

Include Infant as first-class baseline category.

Do not model Infant as Child.

47. Infant association

Do NOT invent seat/guardian-specific airline rules in T002.

If needed later, supplier/fare capability may impose them.

48. Gender

Do NOT store yet.

49. Nationality

Do NOT store yet.

50. Passport/document data

Do NOT store:

PassportNumber
DocumentNumber
DocumentExpiry
Nationality document
DocumentScan

51. Contact

Do not add a new contact model unless FlightBooking creation requires one for core
transaction consistency.

Preferred:
defer contact/customer authorization to R8 unless an existing cross-transaction
snapshot convention clearly warrants a minimal FlightBookingContactSnapshot.

If introduced, document why.

52. No supplier identifiers

Do NOT add:

PNR
supplier booking reference
ticket number
fare basis
offer reference

53. No fare/cabin commercial model

Do NOT add:

Cabin
BookingClass
FareFamily
FareBasis
BaggageAllowance

R4 owns commercial offer semantics.

54. No search criteria persistence

Do not turn FlightBooking into a search-request object.

55. Persistence

Create only tables needed for R2, conceptually:

flight.flight_bookings
flight.flight_journeys
flight.flight_segments
flight.flight_passengers

Use repository naming conventions.

56. Same-schema relationships

Same-schema FK allowed inside:

flight

57. Ordering constraints

Persist deterministic Ordinal ordering and guard duplicates within parent scope.

58. No external FK

No FK to:

ReferenceData
Tour
Booking
HotelBooking
Payment
Pricing
Place
Destination

59. No search/availability source

P22-R3 remains OPEN.

60. No offer/money snapshot

P22-R4 remains OPEN.

61. No PNR/reservation

P22-R5 remains OPEN.

62. No Payment/ticketing

P22-R6 remains OPEN.

Payment target kinds remain exactly:

TourBooking
HotelBooking

63. No cancellation/refund

P22-R7 remains OPEN.

Partial Refund remains DEFERRED.

64. No public API/UI

P22-R8 remains OPEN.

65. Architecture guardrails

Add tests proving:

- FlightBooking owned by Flight
- no generic Booking base
- OneWay = 1 journey
- RoundTrip = 2 journeys
- journey = 1..N segments
- connecting segments supported
- no FlightLeg baseline
- ReferenceData owns Airport/Airline authority
- no Airport/Airline aggregate in Flight
- no cross-schema FK
- Adult/Child/Infant exact passenger categories
- no BirthDate/passport/document data
- no search/offer/PNR/ticket/Payment/cancellation/public API

66. Unit tests

Cover at minimum:

- OneWay valid
- OneWay with !=1 journey rejected
- RoundTrip valid
- RoundTrip with !=2 journeys rejected
- connecting journey valid
- disconnected segment airports rejected
- chronological segment violation rejected
- ArrivalAt <= DepartureAt rejected
- round-trip reverse endpoint mismatch rejected
- no passengers rejected
- no Adult rejected
- Adult/Child/Infant accepted
- deterministic journey/segment order

67. Persistence tests

Cover round-trip:

FlightBooking
journeys
segments
passengers
airport/carrier references
Instant/timezone fields
ordinals

68. Host/architecture regression

Verify Flight module still exposes no public endpoints.

69. SoT decision summary

Record:

P22-R2 = RESOLVED

with:

- FlightBooking is Flight-owned aggregate
- TripType = OneWay / RoundTrip
- MultiCity = DEFERRED
- FlightBooking -> Journey -> Segment
- one journey may contain connecting segments
- FlightLeg not used in baseline
- Airport authority = ReferenceData
- Airline authority = ReferenceData
- Flight stores logical reference/snapshot facts only
- no peer-schema FK
- passenger categories = Adult / Child / Infant
- at least one Adult required
- passenger names stored
- BirthDate/Gender/Nationality/passport/documents not stored yet
- temporal authority = NodaTime Instant + explicit IANA airport timezone context
- search/offer/PNR/ticketing/Payment/cancellation/public UX remain later decisions

70. Validation

Run:

dotnet build TravelCore.sln

Flight.UnitTests
ArchitectureTests
Persistence.IntegrationTests
Host.IntegrationTests

git diff --check

Frontend expected untouched.

71. Required result evidence

Report:

- Flight Unit test count
- Architecture test count
- Persistence test count
- Host test count
- frontend touched YES/NO
- FlightBooking type
- FlightTripType exact values
- OneWay journey count rule
- RoundTrip journey count rule
- MultiCity status
- FlightJourney type
- FlightSegment type
- connecting segments supported
- FlightLeg present: NO
- Airport authority
- Airline authority
- exact Airport reference representation
- exact Airline reference representation
- peer-schema FK
- departure/arrival types
- timezone representation
- Passenger categories
- minimum Adult rule
- BirthDate stored
- Gender stored
- Nationality stored
- Passport/document stored
- search/availability implemented: NO
- offer/fare implemented: NO
- PNR/reservation implemented: NO
- ticketing implemented: NO
- Payment changed: NO
- Payment targets remain TourBooking, HotelBooking
- cancellation/refund implemented: NO
- public API/UI: NO
- P22-R2 = RESOLVED
- P22-R3 through P22-R8 = OPEN
- TC-P22-T003 = NOT EXECUTED

72. Commit/push

After PASS:

- commit with TC-P22-T002 in message
- push normal fast-forward to origin/main
- git fetch origin
- verify HEAD == origin/main
- verify Working Tree CLEAN

73. Auto-Execute

Return TC-P22-T002 RESULT to architect.

Do NOT execute TC-P22-T003.

END_TRAVELCORE_CURSOR_TASK_V1
```
