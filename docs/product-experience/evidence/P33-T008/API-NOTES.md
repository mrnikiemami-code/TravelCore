# TC-P33-T008 — API Notes (I4 Payment Boundary · Option A)

| Field | Value |
|-------|--------|
| Task-ID | `TC-P33-T008` |
| Decision | **Option A** — honest stop at payment boundary |
| Why not B | Local TravelCore DB has **no `payment` schema**; Payment compose API returns **500** without migrations/provider. Architect roadmap recommends A first. No Architect authorization for sandbox provider labeling (B). |

## Analysis (required)

| Boundary | Evidence |
|----------|----------|
| Booking ≠ Payment | Pending booking exists via Booking public initiation; Payment schema/provider not activated for this slice |
| Payment initiation ≠ Payment success | Option A UI does **not** call `POST …/payment/initiation` |
| Payment success ≠ Booking confirmation | No success theater; `confirmed` remains false; status copy keeps «رزرو قطعی نیست» |

## UX sequence

```text
Pending booking status
  → Payment boundary panel (Option A)
  → «پرداخت آنلاین فعلاً در دسترس نیست»
  → no fake Confirm

Optional deep-link /bookings/{id}/payment
  → same Option A stop (no initiate button)
```

## Live checks

```text
GET /api/booking/public/{bookingId} + access token → Pending · monetary 1290 USD (I3)
GET /api/booking/public/{bookingId}/payment → 500 (payment schema absent) — Option A avoids this path for traveler UX
```

## Forbidden (verified)

- No fake transactions / receipts / successful payments / Confirm
- No real provider integration in this task
- Booking ownership unchanged

## Ownership preserved

`Booking ≠ Payment` · Payment module remains SoT for future provider activation (I4-B / later Architect file)
