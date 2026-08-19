# P22 future Flight source adapter checklist

Authoritative for a **future** real flight supplier task. Does **not** select a GDS/NDC vendor, store credentials, or implement an SDK.

## Current posture (P22-R8)

- Named flight supplier: **NONE**
- Production search / availability / offer / reservation / ticketing / cancellation sources: **NONE**
- Supplier SDK: **NOT IMPLEMENTED**
- Inventory authority: **external source-authoritative**

## Neutral ports (capability-split)

Implement only supported capabilities. Declare `FlightSourceCapability` explicitly — **never infer from SourceKey**.

| Port | Typical capabilities |
|------|---------------------|
| `IFlightSearchSource` | Search |
| `IFlightOfferAvailabilitySource` | AvailabilityCheck · OfferRevalidation |
| `IFlightOfferSource` | Offer retrieval |
| `IFlightReservationSource` | ReservationCreate · ReservationQuery |
| `IFlightTicketingSource` | TicketCreate · TicketQuery |
| `IFlightCancellationSource` | CancellationQuote · ReservationCancel · TicketVoid · TicketRefund · CancellationQuery |

Register via module resolvers. FlightBooking aggregate stays Flight-owned — not Tour/Hotel booking.

## Future adapter must verify

- credentials in secure configuration only (`ProviderSecretPosture`)
- PNR/ticket reference mapping (PNR model remains deferred where locked)
- partial refund / multi-city / ancillaries deferrals preserved
- sandbox vs production separation
- test fakes must never be host-registered as production

## Integrity that must remain true

- Flight ≠ Tour package transport inventory
- Browser/client success ≠ ticket issued
- Payment provider NONE until architect selects production adapter
- No global provider registry mega-table (Post-P29-R3)
