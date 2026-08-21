# P35 — Provider-Specific Verification & Design (Iran + UAE)

| Field | Value |
|-------|--------|
| Document | `docs/plans/P35-provider-specific-design-iran-uae.md` |
| Task-ID | `TC-P35-T006` |
| Research / design date | **2026-08-21** |
| Nature | Research + architecture/design only — **no adapters · no SDKs · no credentials · no final Iran vendor pick** |
| Baseline | `675cbab` |
| Abstraction assessment | **NO CORE PAYMENT REDESIGN REQUIRED** |

Reference gateway: `IPaymentProviderGateway`  
(`InitiatePayment` · `VerifyPayment` · `QueryPaymentStatus` · `VerifyCallback` · `InitiateRefund` · `VerifyRefund` · `QueryRefundStatus`)  
Sandbox reference: `SandboxPaymentProviderGateway` · `NamedProductionAdapterImplemented=false`

---

## 1. Normalized user decisions (post T005)

| Decision | Normalized value |
|----------|------------------|
| Iran provider direction | Evaluate **both** Behpardakht Mellat **and** Zarinpal — **no final pick yet** |
| Iran prerequisites | User **willing** to complete tax / e-namad / merchant prerequisites |
| Currency / settlement (Iran) | Multi-currency posture; do **not** assume Iranian rails settle AED |
| UAE provider direction | **Stripe** preferred first path |
| UAE legal posture | User **willing** to establish company/legal structure if individual onboarding unavailable |
| UAE settlement | AED **not** mandatory; any provider-supported settlement currency OK |
| Launch | Iran + UAE **in parallel** (docs tracks; code One Writer) |
| Architecture | **MARKET_SPECIFIC_PROVIDER_ADAPTERS** |

---

## 2. Track A — Behpardakht Mellat / Bank Mellat

### Verification table

| Topic | Status | Notes / sources |
|-------|--------|-----------------|
| Contracting path | PARTIALLY_VERIFIED | Online IPG operated via **Behpardakht Mellat** on Shaparak hosts; Bank Mellat preference ≠ auto-contract. Sources: Shaparak migration PDF (`bpm.shaparak.ir`), Mellat PGW manuals |
| Individual eligibility | PARTIALLY_VERIFIED | Portal UX offers حقیقی (secondary how-tos cite `my.behpardakht.com`); **live approval UNKNOWN** |
| Prerequisites | PARTIALLY_VERIFIED | Secondary sources: tax registration, e-namad, business permit, bank account — **confirm with Behpardakht** |
| Initiate | VERIFIED | `bpPayRequest` → RefId → POST redirect `startpay.mellat` |
| Browser return | VERIFIED | Callback POST ResCode / SaleReferenceId — **not success alone** |
| Verify / settle | VERIFIED | `bpVerifyRequest` · `bpSettleRequest` |
| Inquiry | VERIFIED | Inquiry methods in PGW manuals |
| Refund / reversal | VERIFIED API | `bpRefundRequest` / related; **merchant entitlement UNKNOWN** |
| Charge currency | VERIFIED class | IRR / Iranian rails |
| Settlement AED | UNKNOWN | No Tier-A evidence; treat as unlikely |
| Sandbox | PARTIALLY_VERIFIED | Common practice; entitlement UNKNOWN until terminal |
| 10-minute expiry | UNKNOWN | Closest timeout must be confirmed |

### Readiness

**`READY_FOR_ADAPTER_DESIGN_ONLY`** · live code **`BLOCKED_ON_PROVIDER_ACCOUNT_FACTS`** (no merchant terminal yet)

---

## 3. Track B — Zarinpal

### Verification table

| Topic | Status | Notes / sources |
|-------|--------|-----------------|
| Individual onboarding | PARTIALLY_VERIFIED | Common حقیقی product; **eligibility/tourism MCC UNKNOWN** until account |
| Prerequisites | UNKNOWN | Confirm in merchant panel / support |
| Request / redirect | VERIFIED | Official request → `authority` → redirect ([docs](https://www.zarinpal.com/docs/sdk/nodejs/method/request)) |
| Callback | VERIFIED | Query status + authority; must **verify** server-side |
| Verify | VERIFIED | `verify` with authority + amount ([docs](https://www.zarinpal.com/docs/sdk/php/method/verify)); code 100/101 |
| Inquiry | VERIFIED | Inquiry is status-only — **not** verify ([docs](https://www.zarinpal.com/docs/paymentGateway/otherMethods/Inquiry)) |
| Refund | PARTIALLY_VERIFIED / RISK | API docs exist; feature page notes **temporary CBI disable** ([features/refund](https://www.zarinpal.com/features/refund/)) — **live status UNKNOWN** |
| Currency | VERIFIED class | IRR (rial/toman unit config) |
| Settlement AED | UNKNOWN | Not assumed |
| Sandbox | VERIFIED | `sandbox.zarinpal.com` ([sandbox docs](https://www.zarinpal.com/docs/paymentGateway/sandBox)) |
| Idempotency | PARTIALLY_VERIFIED | Authority uniqueness + verify codes 100/101; exact retry contract confirm with support |

### Readiness

**`READY_FOR_ADAPTER_DESIGN_ONLY`** · implementation **`BLOCKED_ON_PROVIDER_ACCOUNT_FACTS`** (account + **refund live status**)

---

## 4. Track C — Stripe UAE

### Verification table

| Topic | Status | Notes / sources |
|-------|--------|-----------------|
| UAE presence | VERIFIED | Stripe AE product pages / docs (`stripe.com/ae`, Checkout) |
| Individual vs company | UNKNOWN / HIGH RISK | Official support articles exist for UAE business types; content must be confirmed in-account. User accepts **company formation** if required |
| KYC / legal | PARTIALLY_VERIFIED | Standard Stripe verification; exact UAE docs list confirm at signup |
| Presentment AED/USD | PARTIALLY_VERIFIED | Checkout multi-currency ([Checkout](https://stripe.com/ae/payments/checkout)); account capability matrix confirm live |
| Settlement currency | PARTIALLY_VERIFIED | Payout currency per Stripe account banking — AED **not** mandatory per business decision |
| Hosted checkout | VERIFIED | Checkout Sessions / hosted page ([docs](https://docs.stripe.com/payments/checkout-sessions)) |
| 3DS / SCA | VERIFIED | Stripe-managed authentication for Checkout / PaymentIntents |
| Webhooks | VERIFIED | Signature verification ([webhooks](https://docs.stripe.com/webhooks)); do **not** trust browser return alone |
| PaymentIntent / Session lifecycle | VERIFIED | Map Session/PI → TravelCore PaymentAttempt |
| Refund | VERIFIED | Refunds API |
| Idempotency | VERIFIED | Idempotency-Key header ([idempotent requests](https://docs.stripe.com/api/idempotent_requests)) |
| Query / reconcile | VERIFIED | Retrieve Session / PaymentIntent / Charge |
| Test mode | VERIFIED | Stripe test mode |
| Tourism MCC | UNKNOWN | Confirm during account activation |

### Readiness

**`READY_FOR_ADAPTER_DESIGN_ONLY`** (T006) · refined by T007 [`P35-stripe-uae-adapter-design.md`](P35-stripe-uae-adapter-design.md): **`READY_FOR_ADAPTER_IMPLEMENTATION_WITH_TEST_MODE`** (code still needs Architect task) · production **`BLOCKED_ON_PROVIDER_ACCOUNT_FACTS`**

---

## 5. Provider → `IPaymentProviderGateway` mapping

| Gateway method | Behpardakht | Zarinpal | Stripe UAE |
|----------------|-------------|----------|------------|
| `InitiatePaymentAsync` | `bpPayRequest` + redirect URL/token | `request` + redirect | Create Checkout Session (preferred) or PaymentIntent + hosted URL |
| `VerifyPaymentAsync` | `bpVerify` (+ settle as ops step) | `verify` | Retrieve Session/PI; treat `succeeded` / paid |
| `QueryPaymentStatusAsync` | Inquiry APIs | `inquiry` | Retrieve Session/PI/Charge |
| `VerifyCallbackAsync` | Validate posted fields + server verify | Validate callback params + server verify | Prefer **webhook signature**; browser return secondary |
| `InitiateRefundAsync` | `bpRefundRequest` | Refund API (if live) | Create Refund |
| `VerifyRefundAsync` / `QueryRefundStatusAsync` | Inquiry / refund status fields | Refund timeline / status | Retrieve Refund |

**BrowserReturn ≠ success** remains invariant for all three.

---

## 6. Refund mapping summary

| Track | API exists? | Production readiness |
|-------|-------------|----------------------|
| Behpardakht | Yes (`bpRefund*`) | Entitlement UNKNOWN |
| Zarinpal | Yes in docs | **CBI temporary disable risk** — confirm before relying |
| Stripe | Yes | Account + capability dependent |

---

## 7. Callback / webhook / idempotency mapping

| Track | Trust signal | Idempotency notes |
|-------|--------------|-------------------|
| Behpardakht | Server verify/settle after callback | Unique `orderId`; replay via inquiry |
| Zarinpal | Server `verify`; inquiry ≠ verify | Authority + verify 100/101 |
| Stripe | Signed webhooks + retrieve | Idempotency-Key on create; event id replay |

---

## 8. Currency / settlement findings

| Market | Charge | Settlement |
|--------|--------|------------|
| Iran (both tracks) | IRR rails | AED settlement **UNKNOWN / not assumed**; IRR settlement acceptable per multi-currency business posture |
| UAE (Stripe) | AED/USD presentment PARTIAL→confirm | Any Stripe-supported payout currency OK per user; confirm bank account currency |

---

## 9. Merchant / KYC requirements (summary)

| Track | Individual | Company path | User posture |
|-------|------------|--------------|--------------|
| Behpardakht | PARTIAL | Available typically | Willing prerequisites |
| Zarinpal | PARTIAL | Available typically | Willing prerequisites |
| Stripe UAE | UNKNOWN (likely entity required) | Preferred if individual blocked | **Willing to form company** |

---

## 10. UNKNOWN facts requiring provider / account contact

1. Behpardakht: live حقیقی approval + tourism site + terminal issuance  
2. Exact Mellat bank contract vs Behpardakht-only relationship for this merchant  
3. Zarinpal: refund currently enabled? tourism MCC?  
4. Stripe UAE: accepted business types for this activity; company docs list; payout currency  
5. Session TTL closest to 10 minutes per provider  
6. Production domain allowlisting (Behpardakht domain controls)

---

## 11. Core abstraction gap assessment

**NO CORE PAYMENT REDESIGN REQUIRED.**

Existing `IPaymentProviderGateway` + PaymentAttempt lifecycle + ConfirmIfEligible already match redirect/hosted + verify + query + refund ports demonstrated by sandbox.  
Gaps are **account/provider facts**, not missing core ports.

If later a provider needs a capability flag not in `PaymentProviderCapability`, document for Architect — **do not patch in this task**.

---

## 12. Parallel rollout plan (One Writer)

| Order | Proposed future task (Architect-authorized only) | Type |
|-------|--------------------------------------------------|------|
| 1 | Stripe UAE adapter **design lock** (docs) OR Behpardakht design lock | Docs |
| 2 | First **implementation** task for the track with earliest **merchant account** | Code (single writer) |
| 3 | Second Iran track design/implementation after Iran final pick | Docs/Code |
| 4 | Remaining market adapter | Code |

Rules:

- Parallel **docs** OK  
- Parallel **code** on same module **forbidden**  
- Never flip `NamedProductionAdapterImplemented` until a real production adapter is registered and Architect-gated  
- Sandbox remains non-prod

Recommended implementation order **when accounts exist**:

1. **Stripe UAE** (clearest modern API + user preference)  
2. **Behpardakht Mellat** (Mellat preference alignment)  
3. **Zarinpal** (only if refund live + business prefers aggregator) — **no final Iran pick here**

---

## 13. Readiness classification by track

| Track | Classification |
|-------|----------------|
| Behpardakht Mellat | `READY_FOR_ADAPTER_DESIGN_ONLY` · blocked for live impl on account facts |
| Zarinpal | `READY_FOR_ADAPTER_DESIGN_ONLY` · blocked on account + refund live status |
| Stripe UAE | `READY_FOR_ADAPTER_DESIGN_ONLY` · blocked on UAE Stripe account/entity |

None currently: `READY_FOR_ADAPTER_IMPLEMENTATION`  
None: `BLOCKED_ON_ARCHITECTURE_GAP`

---

## 14. Recommended next authorized task

**`TC-P35-T007` — Stripe UAE adapter design lock** (docs-only: capability matrix, webhook contract, secret boundary, test-mode plan)  
**and/or** merchant-account checklist pack for Behpardakht + Zarinpal + Stripe (still no code).

Do **not** start adapter code until Architect issues an implementation `.task.md` **and** account facts unblock.
