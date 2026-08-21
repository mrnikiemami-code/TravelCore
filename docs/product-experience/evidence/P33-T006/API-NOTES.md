# TC-P33-T006 — API Notes (I2 composition)

| Field | Value |
|-------|--------|
| Task-ID | `TC-P33-T006` |
| Nature | Public FE composition only |
| Scenario | `demofeed-tour-tehran-1` (slug) · `demofeed-tour-teh-1` (code) |

## Call sequence (composed)

```text
1. GET /api/tour/products/by-slug/{locale}/{slug}
2. GET /api/tour/products/{tourProductId}?locale=…
3. GET /api/tour/products/{tourProductId}/departures/published
4. GET /api/pricing/public/tour-departures/{tourDepartureId}
   — SSR prefetch per published departure
   — client re-fetch on select via server action when cache miss
```

## Live checks (local Api :5275)

```text
by-slug fa/demofeed-tour-tehran-1 → 200 · product id resolved dynamically
departures/published → 200 · 1 Published
pricing public tour-departures/{id} → 200 · USD · Base 1290 · Adult/DoubleRoom
```

## Forbidden (verified by design)

- No `POST /api/booking/public/initiations`
- No Payment endpoints
- No FE-hardcoded TourDeparture Guids
- No invented money / bypass of Pricing

## Ownership preserved

`TourProduct ≠ TourDeparture` · `Price ≠ Quote` · `Quote ≠ Booking` · PublicExperience = composition only
