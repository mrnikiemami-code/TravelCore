# TC-P34-T004 — Visual Review (Tour Sandbox Payment UX)

| Field | Value |
|-------|--------|
| Task-ID | `TC-P34-T004` |
| Surfaces | Public Tour booking status · payment · payment/return · Sandbox outcome host page |
| Audience | Non-production traveler demo / developer verification |

## Visual checklist

| Check | Status | Evidence |
|-------|--------|----------|
| Status shows Sandbox CTA when `safeAction=Initiate` | Pass | `01-status-pending-cta-desktop.png` · `02-status-pending-cta-mobile.png` |
| CTA labeled NON-PRODUCTION (EN) | Pass | «Go to sandbox payment — non-production» |
| Payment page Sandbox CTA | Pass | `03` / `04-payment-pending-cta-*.png` |
| Sandbox outcome chooser NON-PRODUCTION banner | Pass | `05-sandbox-outcome-chooser.png` |
| Success submitted · browser return ≠ success copy | Pass | `06-sandbox-outcome-success-submitted.png` |
| After ConfirmIfEligible: status shows Confirmed · Succeeded | Pass | `07-payment-success-confirmed.png` (`Confirmed · Succeeded · Succeeded`) |
| Failure → Retry CTA (still labeled sandbox) | Pass | `08-payment-failure-retry.png` |
| Cancel → Failed/Retry (same truthful posture) | Pass | `09-payment-cancel-retry.png` |
| Failure submitted on sandbox host page | Pass | `10-sandbox-outcome-failure-submitted.png` |
| No fake Confirm when payment not Succeeded | Pass | Fail/cancel stay Pending + Retry |
| Option A when unavailable | Pass (code path) | `safeAction=Unavailable` renders boundary panel without pay CTA |

## Screenshot inventory

| File | Subject |
|------|---------|
| `01-status-pending-cta-desktop.png` | Status · Pending · CTA |
| `02-status-pending-cta-mobile.png` | Status mobile |
| `03-payment-pending-cta-desktop.png` | Payment Initiate CTA |
| `04-payment-pending-cta-mobile.png` | Payment mobile |
| `05-sandbox-outcome-chooser.png` | Sandbox chooser |
| `06-sandbox-outcome-success-submitted.png` | Success callback posted |
| `07-payment-success-confirmed.png` | Return UI after Confirm |
| `08-payment-failure-retry.png` | Failure → Retry |
| `09-payment-cancel-retry.png` | Cancel → Retry |
| `10-sandbox-outcome-failure-submitted.png` | Failed outcome host page |

## Known visual limitations

- Next.js hydration warning observed once on payment page (`MoneyText` / `Text`) during E2E; does not fake payment success.
- Public shell footer on status page still uses generic «Not confirmed» caption (page chrome); body caption uses server `status` string.
- Desktop payment screenshots show content in a narrow column (existing `max-w-xl` PublicShell layout).

## Architect note

Traveler UX is labeled non-production Sandbox only. Confirm remains Booking-owned via outbox → `ConfirmIfEligible` (observed Confirmed after ~1 minute outbox period).
