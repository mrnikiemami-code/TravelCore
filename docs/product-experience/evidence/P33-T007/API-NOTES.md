# TC-P33-T007 — API Notes (I3 Booking Initiation)

| Field | Value |
|-------|--------|
| Task-ID | `TC-P33-T007` |
| Nature | Public FE wiring + existing Booking public initiation |
| Scenario | `demofeed-tour-tehran-1` · Departure `01a02414-65a9-7d3d-90f4-9b6179d3f0db` |

## Call sequence

```text
1. Tour Detail composition (I2):
   GET /api/tour/products/by-slug/{locale}/{slug}
   GET …/departures/published
   GET /api/pricing/public/tour-departures/{tourDepartureId}

2. Traveler opens prepare:
   /{locale}/tours/{slug}/book?departureId={publishedId}

3. Initiate (Booking ownership):
   POST /api/booking/public/initiations
   Headers: Idempotency-Key
   Body: tourDepartureId + contact + passengers + sourceKind=Direct
   → 201 Pending + accessToken + monetary (Quote issued server-side)

4. Authorized read:
   GET /api/booking/public/{bookingId}
   Header: X-TravelCore-Booking-Access-Token
```

## Live evidence (local Api :5275)

```text
POST /api/booking/public/initiations → 201
bookingId: 01a02438-44e3-7d07-93bd-cf9688db0034
status: Pending
confirmed: false
monetary.totalAmount: 1290 USD
monetary.sourcePriceId: 01a02414-662f-746c-9ccb-84f5d1d41cf5
hold.status: Active
accessTokenIssued: true
```

## Environment note (local demo DB)

Local PostgreSQL initially lacked `booking` schema tables. Applied **existing** Booking module EF migrations (no new migration authored in this task) so the already-shipped public initiation API could persist. This is environment recovery for evidence — not a Booking redesign.

## Forbidden (verified)

- No payment provider call in I3
- No FE-minted Quote / price
- No hardcoded departure IDs in FE (CTA uses selected published id)
- No marking Confirmed

## Ownership preserved

`TourProduct ≠ TourDeparture` · `Price ≠ Quote` · `Quote ≠ Booking` · `Booking ≠ Payment`
