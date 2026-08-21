# TC-P33-T005 — I1 Commercial Data Foundation

| Field | Value |
|-------|--------|
| Task-ID | `TC-P33-T005` |
| Status (Cursor) | **PASS** |
| Scenario | `demofeed-tour-teh-1` → Published TourDeparture → Price (USD) |

## Implementation

- `tools/demofeed enrich-commerce` (owner paths)
  - `ITourDepartureAdminService` create → schedule → capacity → Published
  - `IPriceAdminService` create `TargetType=TourDeparture`
  - Idempotent ledger: `.local/demofeed-media/commerce-ledger.json`
- Pricing DbContext registered in DemoFeedHost; `PricingMigrator` on `--ensure-schema`
- No Booking / Payment / FE hardcodes

## Validation (in-process)

```text
Published departures for product: 1
Departure in list: True
Public price summary: USD components=1
Idempotent re-run: applied=0 skipped=2
```

## Validation (HTTP · Api :5275)

```text
GET /api/tour/products/{id}/departures/published → 200 · 1 Published · capacity 1..20
GET /api/pricing/public/tour-departures/{id} → 200 · USD · Base 1290 · Adult/DoubleRoom
```

## Boundaries

TourProduct ≠ TourDeparture · Price ≠ Quote · Quote ≠ Booking · DemoFeed removable · no Api module registration

## Limitations

- One DEMOFEED tour only (Tehran)
- No Public FE composition yet (I2)
- No Booking initiation (I3)
- No Payment (I4 Option A deferred)
- Amount is real Pricing row labeled DEMOFEED sample — not a production rate claim
- Local calendar display of schedule dates may follow Asia/Tehran calendar rules in JSON — facts still Published + priced
