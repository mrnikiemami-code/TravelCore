# P38-T007 — Agency Offer Operations Foundation

| Field | Value |
|-------|--------|
| Task-ID | `TC-P38-T007` |
| Date | 2026-08-22 |

## Delivered

- Acting AgencyProfile resolution: `GET /api/agency-marketplace/profiles/me`
- Offer list/detail bound to acting profile (Account → Party → AgencyProfile)
- Ownership: Agency write lifecycle requires `EnsureOfferOwnedByAgencyAsync`
- HTTP: `GET /offers/{id}`, `POST .../suspend`, `POST .../retire`
- FE: `/[locale]/agency/catalog` list+create · `/catalog/[offerId]` detail+lifecycle

## Ownership rule

```text
Agency A cannot manage Agency B offers
```

Client-supplied `agencyProfileId` that ≠ acting profile → Forbid.
Create always stamps acting AgencyProfileId.

## Explicit non-goals

- Commission / Settlement / Revenue / Ranking / fake financial metrics
- Approve/Reject remain platform Moderate (not agency self-serve)

## Compatibility

- Public offer selection + booking initiation unchanged
- FE ≠ SoT · Booking ≠ Payment
