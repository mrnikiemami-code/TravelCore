# Foreign Tour Detail — PVM + fixtures (T012)

Presentation-only Page View Model for `ForeignTourDetailPage`.

| Path | Role |
|------|------|
| `src/types/pages/foreign-tour-detail.ts` | Typed PVM |
| `fa.ts` / `en.ts` | Distinct FA/EN fixtures |
| `index.ts` | `loadForeignTourDetailFixture(locale)` |

## Locked distinctions

- TourProduct ≠ TourDeparture (`product` vs `departures[]`)
- PassengerCategory ≠ Occupancy (separate axes on `pricingOffers`)
- Price display ≠ Quote ≠ Booking ≠ Payment
- Locale ≠ Currency ≠ Calendar ≠ Timezone
- IRR display unit explicit (`irrDisplayUnit`) — not implied by locale alone
- No fabricated `0` prices for unavailable offers

## Non-goals (T013+)

Page composition · booking · live APIs · SEO route implementation.
