# TC-P22-T003 Task Envelope (architect, live)

Captured from the same ChatGPT tab after `TC-P22-T002 = ACCEPTED` and `P22-R3 = RESOLVED`.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1
Task-ID: TC-P22-T003
Phase: P22
Title: Flight search, live availability authority, source capability boundary, and freshness
Baseline: 9518018
Decision: P22-R3 = RESOLVED

Purpose:
Implement the Flight live-search and availability/source boundary only.

This task establishes:
- provider-neutral Flight search
- live availability authority
- source selection/resolution
- explicit source capabilities
- freshness/provenance
- safe zero-source behavior

Do NOT implement accepted fare snapshots, FlightBooking monetary snapshots,
PNR/reservation, ticketing, Payment, cancellation/refund, public API, or frontend.

1. Repository safety

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

9518018

2. SoT

Record:

TC-P22-T002 = ACCEPTED
P22-R3 = RESOLVED

Keep:

P22-R4 through P22-R8 = OPEN
P22 = IN_PROGRESS
TC-P22-T004 = NOT EXECUTED

3. Inventory authority

Lock baseline:

live Flight search / availability truth
=
external source-authoritative

TravelCore does NOT own seat inventory/allotment in P22 baseline.

4. No internal inventory

Do NOT implement:

SeatInventory
FlightInventory
InventoryBucket
SeatCount ledger
TravelCore-owned allotment

5. Search source port

Introduce:

IFlightSearchSource

6. Search responsibility

Search source accepts neutral customer search intent and returns source-neutral
candidate itineraries/options.

It does NOT create FlightBooking.

7. Availability/validation source

Introduce:

IFlightOfferAvailabilitySource

Its responsibility is authoritative live validation of a selected source option.

8. No giant gateway

Do NOT introduce one speculative:

IFlightSupplierGateway

9. Source resolver

Introduce server-controlled source resolution:

IFlightSearchSourceResolver
IFlightOfferAvailabilitySourceResolver

10. Source selection

Server-controlled only.

Client must not choose implementation class/provider type.

11. Named supplier

Keep:

Named Flight Supplier = NONE

12. Production sources

Keep:

Production Flight Search Source = NONE
Production Flight Availability Source = NONE

13. No fake production source

Test fakes allowed only in tests/non-production composition.

14. No supplier SDK

Do NOT add Amadeus/Sabre/Travelport/NDC SDK or any named supplier library.

15-58. Remaining locked R3 rules as issued by the architect (search request,
MultiCity DEFERRED, no cabin lock, no passenger PII, search result !=
FlightBooking, search result != accepted FlightOfferSnapshot, provenance
SourceKey/SourceOptionReference, ObservedAt Instant, ExpiresAt Instant? with
no hardcoded TTL, availability Available/Unavailable/Changed/Unknown,
timeout != Unavailable, no hold/PNR, capabilities Search+AvailabilityCheck
only, no smart routing/failover, Search module not authoritative, no product
persistence of transient search results, no accepted monetary truth).

59-66. Guardrails, unit/host/persistence tests, no endpoints, SoT, validation,
required result evidence.

67. Commit/push after PASS with TC-P22-T003 in the message.

68. Auto-Execute

Return TC-P22-T003 RESULT to architect.

Do NOT execute TC-P22-T004.

END_TRAVELCORE_CURSOR_TASK_V1
```

Full live capture is also retained in the architect ChatGPT tab for this conversation.
