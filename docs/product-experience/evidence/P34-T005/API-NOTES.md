# TC-P34-T005 — API Notes (public `confirmed` / `bookingConfirmed` honesty)

| Field | Value |
|-------|--------|
| Task-ID | `TC-P34-T005` |
| Nature | Fix hardcoded `Confirmed: false` on public Booking reads; reflect Booking-owned status |
| Baseline | `0597f8d` (`TC-P34-T004`) |
| Production flag | `NamedProductionAdapterImplemented` remains **`false`** |
| Confirm lifecycle | Unchanged — no Payment Confirm invention; no ConfirmIfEligible changes |

## Root cause

`PublicBookingMapper.ToInitiation` / `ToRead` hardcoded `Confirmed: false` even when `booking.Status == BookingStatus.Confirmed`.

Public payment compose (`PublicBookingEndpoints.Compose`) already passes `booking.Confirmed` into `PublicBookingPaymentRead.BookingConfirmed`. Payment contracts (`PublicPaymentRead`) do **not** expose `bookingConfirmed` and do not invent Confirm — so fixing the mapper is sufficient for compose honesty.

## Before (T004 evidence)

After verified sandbox Success + ConfirmIfEligible:

```json
{"bookingStatus":"Confirmed","paymentStatus":"Succeeded","safeAction":"Succeeded","bookingConfirmed":false}
```

`status`/`bookingStatus` string was truthful; boolean `confirmed` / `bookingConfirmed` lied.

## After

| BookingStatus | `confirmed` (read/initiation) | `bookingConfirmed` (payment compose) |
|---------------|-------------------------------|--------------------------------------|
| Pending | `false` | `false` |
| Confirmed | `true` | `true` |

Mapping:

```csharp
Confirmed: booking.Status == BookingStatus.Confirmed
```

## Audit — Payment compose

| Surface | Result |
|---------|--------|
| `PublicPaymentRead` | No `BookingConfirmed` field |
| `PublicBookingPaymentRead.BookingConfirmed` | Reflects `PublicBookingRead.Confirmed` only |
| `EventMeansBookingConfirmed` constants | Remain `false` (PaymentSucceeded ≠ BookingConfirmed) — intentional boundary, not a mapping bug |

## Tests run

```text
TravelCore.Modules.Booking.UnitTests --filter-class *PublicBookingMapperConfirmedTests
  → Passed 5/5
    ToRead_Pending_Maps_Confirmed_False
    ToRead_Confirmed_Maps_Confirmed_True
    ToInitiation_Pending_Maps_Confirmed_False
    ToInitiation_Confirmed_Maps_Confirmed_True
    Payment_Compose_Reflects_Booking_Confirmed_Without_Inventing_Confirm

TravelCore.Host.IntegrationTests --filter-method *Public_Confirmed*
  → Passed 1/1
    Public_Confirmed_Booking_Exposes_confirmed_And_bookingConfirmed_True

TravelCore.Host.IntegrationTests --filter-method *Public_Payment_Is_Booking_Scoped*
  → Passed 1/1
    (asserts Pending ⇒ bookingConfirmed=false)
```

## Ownership preserved

Booking owns Confirm · Payment compose reflects Booking status only · ConfirmIfEligible / Payment lifecycle untouched · `NamedProductionAdapterImplemented=false`

VISUAL: not required (API boolean honesty only).
