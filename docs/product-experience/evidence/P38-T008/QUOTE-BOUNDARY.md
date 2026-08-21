# P38-T008 — Offer-aware Quote Boundary Foundation

| Field | Value |
|-------|--------|
| Task-ID | `TC-P38-T008` |
| Date | 2026-08-22 |

## Model

```text
Customer selects AgencyOffer
        ↓
Booking initiation (validated Offer)
        ↓
Pricing issues Quote (TourDeparture Price + Offer metadata)
        ↓
Booking AcceptQuote
```

## Ownership

| Concern | Owner |
|---------|--------|
| Quote amounts | **Pricing** (TourDeparture Price snapshot) |
| Offer identity on Quote | Metadata only (`CommercialContextAgencyOfferId`) |
| Booking lifecycle | **Booking** (`Source.AgencyOffer` still authoritative) |
| Payment | Unchanged |

## Rules

- AgencyOffer ≠ Price
- Offer id does **not** change components/total
- No AgencyMarketplace FK from Pricing
- No Commission / Settlement / Discount Engine

## Artifacts

- `Quote.CommercialContextAgencyOfferId`
- `IAuthoritativeQuoteIssuer.IssueForTourDepartureAsync(..., commercialContextAgencyOfferId)`
- Migration `20260822020000_P38QuoteCommercialContextAgencyOffer`
- `PublicBookingSurfaceService` passes validated Offer id into issuance
