# P38-T005 — Booking Offer Boundary (Public Initiation)

| Field | Value |
|-------|--------|
| Task-ID | `TC-P38-T005` |
| Date | 2026-08-22 |

## Model

```text
TourProduct → AgencyOffer(s) → Customer Selection → Public Initiation → Booking (Pending)
AgencyOffer ≠ TourDeparture ≠ Price ≠ Booking ≠ Payment
```

## API

- `POST /api/booking/public/initiations`
- Request: optional `agencyOfferId` (+ existing departure/contact/passengers/idempotency)
- Client `sourceKind` may only be `Direct` (or omitted). **Agency SourceKind is never client-forged.**
- When `agencyOfferId` is present, server resolves `BookingSourceContext.ForAgency` after trusted validation.

## Server validation (via `IAgencyOriginContextQuery`)

1. Offer exists
2. Public eligibility: Published + Listed + Active + SalesChannel=Public
3. Agency Active + PublicListingEnabled
4. Offer TourProduct matches selected TourDeparture product
5. Departure scope: `All` OK · `Listed` must include departure id

## FE

- `/[locale]/tours/[slug]/book?departureId=&agencyOfferId=`
- Prepare form forwards `agencyOfferId` into initiation body
- Direct path unchanged when offer id absent

## Out of scope (preserved)

- Commission / settlement
- Payment redesign
- Confirm endpoint
- Fake agencies / prices / KPIs

## Compatibility

- Zero-offer / Direct initiation still works
- Booking remains SoT; FE ≠ SoT
