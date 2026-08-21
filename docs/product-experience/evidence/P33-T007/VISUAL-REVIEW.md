# TC-P33-T007 — VISUAL-REVIEW (I3 Public Booking Initiation UX)

| Field | Value |
|-------|--------|
| Task-ID | `TC-P33-T007` |
| Locale | `fa` (RTL) |
| Scenario | `demofeed-tour-tehran-1` |
| Date | 2026-08-21 |

## Surfaces reviewed

| Surface | Desktop | Mobile | Result |
|---------|---------|--------|--------|
| Tour Detail — departure + price + Start Pending CTA | `fa-tour-detail-booking-cta-desktop.png` | `fa-tour-detail-booking-cta-mobile.png` | **PASS** |
| Booking prepare form (`/tours/{slug}/book`) | `fa-booking-prepare-desktop.png` | (same flow; prepare is narrow max-w) | **PASS** |
| Pending status (`/bookings/{id}`) | `fa-booking-pending-desktop.png` | `fa-booking-pending-mobile.png` | **PASS** |

## Honesty checks

| Check | Result |
|-------|--------|
| CTA enabled only with Published Departure + Pricing summary | PASS |
| Copy states Pending ≠ Confirmed · no payment on detail/prepare | PASS |
| Money on Pending comes from Booking monetary/Quote snapshot (USD 1290) | PASS |
| No fake Confirmed badge | PASS |
| RTL preserved on FA | PASS |

## Known visual notes

- Sticky bar may overlap lower content on mobile (known from I2).
- Status view still exposes a legacy “پرداخت رزرو” link from prior Booking/Payment surfaces — **I3 did not implement Payment**; Option A / I4 owns that boundary.
- Next.js hydration warning badge may appear in local DevTools (“1 Issue”) — unrelated to commerce composition.

## Verdict

**Desktop PASS · Mobile PASS · RTL PASS** for I3 initiation UX.
