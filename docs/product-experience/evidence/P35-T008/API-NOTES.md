# P35-T008 — Stripe UAE Test-Mode Adapter Evidence

| Field | Value |
|-------|--------|
| Task-ID | `TC-P35-T008` |
| Date | 2026-08-21 |
| Live Stripe test payments | **NOT performed** (no authorized test credentials in environment) |
| Classification | **`CODE_READY_FOR_STRIPE_TEST_ACCOUNT`** |

## Implementation summary

- `StripePaymentProviderGateway` implements `IPaymentProviderGateway`
- Official `Stripe.net` 48.5.0 confined to Payment.Infrastructure
- Registration gated by `PaymentStripeGate` (non-Production + Enabled + `sk_test_` only; `sk_live_` rejected)
- Checkout Session hosted initiation; webhook via existing `/api/payment/providers/{providerKey}/callback` + `Stripe-Signature`
- `NamedProductionAdapterImplemented` remains **false**
- Sandbox provider retained

## Configuration

`Payment:Stripe` in `appsettings.Development.json` defaults **Enabled=false** (no secrets committed).

## Tests

- Unit: `PaymentStripeProviderTests`
- Architecture: `PaymentStripeProviderGuardrailTests`

## Boundaries

| Rule | Status |
|------|--------|
| BrowserReturn ≠ success | Preserved |
| Booking ConfirmIfEligible owns Confirm | Untouched |
| No Booking → Stripe dependency | Guardrailed |
| Production activation | Blocked |

## Known limitations

- Live Stripe Checkout/webhook E2E awaits test account keys
- Refund verify/query is PendingUnknown until refund webhooks wired deeper
- Session expiry closest to 10 minutes not forced in Create options yet
