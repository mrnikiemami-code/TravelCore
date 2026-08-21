# P38-T004 — Public Multi-Agency Offer Selection Foundation

| Field | Value |
|-------|--------|
| Task-ID | `TC-P38-T004` |
| Date | 2026-08-21 |

## Public offer composition

- API: `GET /api/agency-marketplace/offers/related-published?tourProductId=`
- Eligibility (P38): **Published + Listed + Active + SalesChannel=Public** + Active agency with PublicListingEnabled
- Public DTO no longer returns `AgencyProfileId` (internal)

## Customer selection

- Tour detail: selectable Agency Offers → URL `?agencyOfferId=`
- Single offer auto-selected; ≥2 requires selection before booking CTA
- Booking prepare link may include `agencyOfferId` (boundary only — Booking initiation not rewritten)

## Booking boundary

```text
Customer selects AgencyOffer
        ↓
/book?departureId=…&agencyOfferId=…   (prepared)
        ↓
Booking ownership still owns initiation (T005+)
```

## Compatibility

- Zero offers: legacy departure→book path unchanged
- No fake agencies/ratings/commissions
- FE ≠ SoT

## Validation

- AgencyMarketplace unit tests
- Frontend typecheck/lint
