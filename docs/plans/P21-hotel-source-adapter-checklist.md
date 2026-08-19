# P21 future Hotel source adapter checklist

Authoritative for a **future** real hotel supplier task. Does **not** select a supplier, store credentials, or implement an SDK.

## Current posture (P21-R8)

- Named hotel supplier: **NONE**
- Production availability / rate / reservation sources: **NONE**
- Zero production sources is a valid host configuration.
- Smart routing and automatic failover: **NOT IMPLEMENTED**

## Neutral ports (capability-split)

Implement only the ports your supplier supports. Declare `HotelSourceCapability` explicitly — **never infer from SourceKey**.

| Port | Capabilities |
|------|----------------|
| `IHotelAvailabilitySource` | AvailabilityCheck · AvailabilityHold · AvailabilityHoldQuery · AvailabilityHoldRelease |
| `IHotelRateOfferSource` | RateQuote |
| `IHotelReservationSource` | ReservationCreate · ReservationQuery · ReservationCancel · ReservationCancellationQuery |

Register via module resolvers (`IHotelAvailabilitySourceResolver`, etc.). Place catalog remains canonical — `ExternalHotelId` never becomes internal Place PK.

## Future adapter must verify

- credentials in secure configuration only
- supported currencies and locale rules
- cancellation/refund policy mapping (partial refund remains deferred)
- sandbox vs production separation
- test fakes must never be host-registered as production

## Integrity that must remain true

- HotelBooking ≠ Place catalog ownership
- Provider quote ≠ confirmed reservation without explicit transition
- Payment success ≠ reservation confirmed without domain rules
- No global provider registry mega-table (Post-P29-R3)
