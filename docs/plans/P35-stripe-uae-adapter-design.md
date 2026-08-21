# P35 — Stripe UAE Adapter Design Lock

| Field | Value |
|-------|--------|
| Document | `docs/plans/P35-stripe-uae-adapter-design.md` |
| Task-ID | `TC-P35-T007` |
| Date | **2026-08-21** |
| Nature | Architecture / design lock only — **no SDK · no credentials · no code · no Stripe resources** |
| Primary readiness | **`READY_FOR_ADAPTER_IMPLEMENTATION_WITH_TEST_MODE`** *(design locked; code still needs Architect `.task.md`)* |
| Production readiness | **`BLOCKED_ON_PROVIDER_ACCOUNT_FACTS`** |
| Architecture gap | **`NO CORE PAYMENT REDESIGN REQUIRED`** / not `BLOCKED_ON_ARCHITECTURE_GAP` |

Sources (official):  
[Checkout Sessions](https://docs.stripe.com/payments/checkout-sessions) · [Payment Intents](https://docs.stripe.com/payments/payment-intents) · [Webhooks](https://docs.stripe.com/webhooks) · [Idempotent requests](https://docs.stripe.com/api/idempotent_requests) · [3D Secure](https://docs.stripe.com/payments/3d-secure) · [Stripe AE Checkout](https://stripe.com/ae/payments/checkout) · [Verifying status](https://docs.stripe.com/payments/payment-intents/verifying-status)

---

## 1. Readiness classification (split)

| Layer | Classification |
|-------|----------------|
| Design | **LOCKED** by this document (awaiting Architect ACCEPT) |
| Non-prod adapter code | **May** be authorized later as `READY_FOR_ADAPTER_IMPLEMENTATION_WITH_TEST_MODE` using Stripe **test** keys |
| Production enablement | **`BLOCKED_ON_PROVIDER_ACCOUNT_FACTS`** (UAE entity, KYC, live keys, webhook endpoint) |
| NamedProductionAdapterImplemented | Remains **`false`** until production adapter registered + Architect gate |

This task does **not** authorize implementation.

---

## 2. Adapter placement

| Item | Decision |
|------|----------|
| Module | `TravelCore.Modules.Payment.Infrastructure` |
| Suggested type | `StripePaymentProviderGateway : IPaymentProviderGateway` |
| ProviderKey | e.g. `stripe` or `stripe-uae` (exact key locked at implementation task) |
| DI / resolver | Via existing `PaymentProviderResolver` patterns (mirror sandbox) |
| Booking | **Must not** reference Stripe types or packages |
| Frontend | May use Checkout **hosted URL** only (no Stripe.js required for v1); publishable key only if Elements chosen later |

Payment owns provider integration. Booking ConfirmIfEligible remains confirmation SoT.

---

## 3. Stripe object / lifecycle mapping (smallest compatible model)

**Chosen model: Checkout Session (hosted)** as primary initiation UI.

Rationale: hosted page reduces PCI scope; Stripe manages 3DS; aligns with TravelCore redirect/hosted sandbox pattern; less custom UI than raw PaymentIntent + Elements.

| Stripe concept | TravelCore concept |
|----------------|--------------------|
| Checkout Session `id` | Provider session / initiation reference |
| PaymentIntent (on Session) | Underlying money movement evidence |
| Charge (as needed) | Reconciliation detail |
| Event / Webhook | Authoritative async evidence → `VerifyCallback` / processor |
| Refund | Refund attempt via gateway refund ports |
| Idempotency-Key | Initiate / refund create safety |
| `client_reference_id` / metadata | Correlation to PaymentAttempt / Booking ids (minimal) |

PaymentIntent-only path is a **fallback** if Checkout Session constraints block tourism flows — prefer Checkout first.

---

## 4. Initiation contract mapping

| TravelCore field | Stripe mapping |
|------------------|----------------|
| Amount | Minor units per currency (Stripe amount integer) — must match Payment snapshot |
| Currency | ISO code (AED / USD as allowed by account) |
| Correlation | `client_reference_id` = PaymentAttempt id (or opaque) · metadata: `paymentAttemptId`, `bookingId` only |
| Success URL | TravelCore browser-return URL (labeled return — **not** success evidence) |
| Cancel URL | TravelCore cancel/return URL |
| Provider reference | Persist Session id (+ PI id when available) on PaymentAttempt |
| Expiry | Prefer Session `expires_at` closest to **10 minutes** — exact support **confirm at impl** (UNKNOWN until API options verified for chosen mode) |

Forbidden: card PAN in TravelCore · secrets in metadata · treating return URL hit as Success.

---

## 5. Webhook verification design

| Item | Design |
|------|--------|
| Ownership | Payment module HTTP endpoint (e.g. `/api/payment/providers/stripe/webhook`) |
| Auth | Stripe-Signature header + webhook signing secret ([docs](https://docs.stripe.com/webhooks)) |
| Relevant events (v1) | `checkout.session.completed` · `checkout.session.expired` · `payment_intent.succeeded` · `payment_intent.payment_failed` · `charge.refunded` / `refund.*` as needed |
| Idempotency | Persist Stripe `event.id`; ignore duplicates |
| Out-of-order | Re-query Session/PI before mutating PaymentAttempt |
| Tamper | Fail closed on bad signature |
| BrowserReturn | Non-authoritative; may trigger **query** only |

Existing Payment callback/processor path remains authoritative for applying evidence → lifecycle transitions.

---

## 6. Success / failure / cancel semantics

| Condition | TravelCore treatment |
|-----------|----------------------|
| Authoritative success | Signed webhook + Session/PI retrieve shows paid/`succeeded` **and** amount/currency match snapshot |
| Failure | `payment_intent.payment_failed` / unpaid expired with decline evidence |
| Cancel / abandon | Session expired / canceled without paid PI — map to existing Failure/Cancel/Pending paths (**do not invent states**) |
| Browser return alone | **Never** Success |

Booking Confirm remains via ConfirmIfEligible after Payment success evidence — unchanged.

---

## 7. Refund mapping

| Item | Design |
|------|--------|
| Full refund | Required by business — map `InitiateRefundAsync` → Stripe Refund create on Charge/PI |
| Partial refund | Stripe supports; TravelCore exposure **deferred** unless Architect requires (default: full only in v1) |
| Reference | Persist Stripe refund id |
| Verify / query | Retrieve Refund |
| Failure | Surface provider error; no silent Booking change |
| Reconciliation | Refund events via webhook + query |

No refund implementation in this task.

---

## 8. Currency / settlement findings

| Topic | Status |
|-------|--------|
| AED presentment | PARTIALLY_VERIFIED via Checkout multi-currency / AE product pages — **confirm on live UAE account capabilities** |
| USD presentment | PARTIALLY_VERIFIED same |
| Settlement / payout currency | UNKNOWN until UAE bank account + Stripe payout settings — business accepts any supported currency |
| FX | UNKNOWN implications — keep Money semantics = charged currency snapshot |

Do not invent settlement currency claims.

---

## 9. UAE merchant / entity prerequisites

| Topic | Status |
|-------|--------|
| Individual / sole proprietor | **UNKNOWN** (must confirm in Stripe UAE onboarding) |
| Company / legal entity | Likely required path — user **willing** but **not completed** |
| UAE bank account | Expected for payouts — **UNKNOWN** exact bank list until signup |
| KYC documents | Standard Stripe verification — exact list **UNKNOWN** until account |
| Trade license | UNKNOWN if required for activity |
| Tourism MCC | UNKNOWN |

Willingness ≠ onboarded.

---

## 10. Configuration / secrets (names only — no values)

Suggested structure (final names at implementation):

```text
Payment:Stripe:Enabled
Payment:Stripe:ProviderKey
Payment:Stripe:SecretKey          # env/secret store only
Payment:Stripe:PublishableKey     # only if frontend needs it
Payment:Stripe:WebhookSecret      # env/secret store only
Payment:Stripe:WebhookEndpointPath
Payment:Stripe:SuccessUrlTemplate
Payment:Stripe:CancelUrlTemplate
Payment:Stripe:DefaultCurrency    # optional
```

Rules: never commit secrets · separate test vs live keys · Production fail-closed if incomplete.

---

## 11. Production fail-closed rules

1. Do not register Stripe gateway in Production unless `Enabled` + live keys + webhook secret present and validated.  
2. Explicit provider selection — no silent fallback to `sandbox`.  
3. Incomplete config → initiation eligibility false / hard fail.  
4. Startup/runtime validation: reject Production with test keys labeled as live.  
5. `NamedProductionAdapterImplemented=true` only after Architect-authorized production gate.  
6. Sandbox remains Development/non-prod only.

---

## 12. Test matrix (future)

| Case | Expectation |
|------|-------------|
| Initiation mapping | Amount/currency/metadata correct |
| Webhook good signature | Accepted |
| Duplicate event.id | Idempotent no-op |
| Tampered signature | Rejected |
| Success evidence | PaymentAttempt Success; Booking still Pending until ConfirmIfEligible |
| Failure / cancel | No fake Confirm |
| Refund full | Refund ports update Payment evidence only |
| Prod config missing | Fail closed |
| Booking Confirm | Booking-owned only |

---

## 13. Implementation task breakdown (not executed)

| Step | Scope |
|------|--------|
| A | Stripe adapter infrastructure + DI + ProviderKey (test mode) |
| B | Webhook endpoint + signature + event→lifecycle mapping |
| C | Refund mapping ports |
| D | Public UX labeling (Stripe / non-sandbox honesty) |
| E | Non-production Stripe **test-mode** E2E |
| F | Production activation gate (account facts + NamedProduction flag) |

Iran Behpardakht/Zarinpal tracks remain separate — **do not auto-start** from this design.

---

## 14. External facts still required

**Before test-mode code (Architect may still allow stubs):** test API keys + webhook secret for a Stripe account (test).  

**Before production:** UAE legal entity completion · KYC pass · live keys · payout bank · tourism MCC approval · HTTPS webhook URL · capability confirmation for AED/USD.

---

## 15. Recommended next authorized task

**`TC-P35-T008` — Stripe UAE adapter infrastructure (test mode only)**  
**or** merchant-account checklist pack (UAE entity + Iran tracks) if Architect prefers onboarding first.

Until Architect files the next `.task.md`: **no Stripe SDK, no code.**
