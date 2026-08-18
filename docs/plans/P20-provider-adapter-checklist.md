# P20 future Payment provider adapter checklist

Authoritative for a **future** real-provider task. This document does **not** select a provider, store credentials, or implement an SDK.

## Current posture (P20-R8)

- Provider infrastructure: **READY FOR ADAPTERS**
- Production provider: **NOT CONFIGURED / NONE**
- Named production adapter: **false**
- Zero production providers is a valid host configuration.
- Public initiation stays server-controlled and honest when no production adapter is configured.

## Neutral ports

Implement `IPaymentProviderGateway` only. Declare `Capabilities` explicitly. Do **not** infer capability from `ProviderKey`.

Required capability names:

- `RedirectInitiation`
- `CallbackVerification`
- `PaymentStatusQuery`
- `RefundInitiation`
- `RefundVerification`
- `RefundStatusQuery`

## Future adapter must verify (do not resolve here)

- credentials/secrets live in secure configuration, never the repository
- supported currencies (Payment core must not assume all currencies)
- callback verification algorithm (unverified callback cannot succeed)
- amount units vs `PaymentExecutionSnapshot`
- status-query support; if absent, recheck returns capability-unavailable and does not mutate
- refund support; if absent, Refund stays Pending and compensation is not complete
- refund limits (partial refund remains deferred)
- timeout / network-ambiguity semantics (`NetworkTimeout != Failed`)
- sandbox vs production separation
- `test` fake must never be registered as production

## Integrity that must remain true

- `BrowserReturn != PaymentSuccess`
- amount/currency mismatch cannot succeed Payment or Refund
- Payment collection evidence must not succeed Refund, and vice versa
- replay is idempotent; Payment A evidence cannot mutate Payment B
- no public arbitrary provider selection
- no manual `SetStatus` / `ForceSuccess` / `MarkPaid` / `MarkRefunded` / `ForceConfirm`
- Payment != Accounting / Settlement / Chargeback / Fraud / Wallet
