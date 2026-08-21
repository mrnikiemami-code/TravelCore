# P35 — Zarinpal Adapter Design Lock

| Field | Value |
|-------|--------|
| Document | `docs/plans/P35-zarinpal-adapter-design.md` |
| Task-ID | `TC-P35-T010` |
| Date | **2026-08-21** |
| Nature | Architecture / design lock only — **no code · no SDK · no credentials · no Iran final pick** |
| Primary readiness | **`READY_FOR_ADAPTER_IMPLEMENTATION_WITH_TEST_FIXTURES`** *(payment path; code still needs Architect `.task.md`)* |
| Refund production gate | **`BLOCKED_ON_REFUND_AVAILABILITY`** *(CBI temporary disable — official)* |
| Production readiness | **`BLOCKED_ON_PROVIDER_ACCOUNT_FACTS`** + refund gate above |
| Architecture gap | **Not** `BLOCKED_ON_ARCHITECTURE_GAP` · **NO CORE PAYMENT REDESIGN REQUIRED** |

Sources (official):  
[Payment request / StartPay / verify](https://www.zarinpal.com/docs/paymentGateway/connectToGateway) · [Sandbox](https://www.zarinpal.com/docs/paymentGateway/sandBox) · [Inquiry](https://www.zarinpal.com/docs/paymentGateway/otherMethods/Inquiry) · [Refund feature](https://www.zarinpal.com/features/refund/) · prior T004/T006 research notes

---

## 1. Readiness split

| Layer | Classification |
|-------|----------------|
| Design | **LOCKED** (this document) |
| Code with fixtures / sandbox UUID | May be Architect-authorized later |
| Refund in production | **`BLOCKED_ON_REFUND_AVAILABILITY`** until CBI/Zarinpal re-enable confirmed |
| Merchant account | **`BLOCKED_ON_PROVIDER_ACCOUNT_FACTS`** |
| Iran vendor selection | **Not decided** vs Behpardakht |

---

## 2. Provider identity

| Topic | Finding |
|-------|---------|
| Identity | **Zarinpal** — Iranian payment facilitator / پرداخت‌یار (self-description on official docs) |
| Network role | Aggregator-style IPG over Iranian rails (Shaparak class) — do not invent network guarantees |
| Credential | `merchant_id` (36-char UUID) |
| ProviderKey (future) | `zarinpal` (lock exact string at implementation) |

---

## 3. Adapter placement

| Item | Decision |
|------|----------|
| Type | `ZarinpalPaymentProviderGateway : IPaymentProviderGateway` |
| Module | `TravelCore.Modules.Payment.Infrastructure` |
| HTTP client | Infrastructure-internal REST to `payment.zarinpal.com` / `sandbox.zarinpal.com` |
| Booking | **No** Zarinpal dependency |

---

## 4. Lifecycle mapping

| Zarinpal | TravelCore |
|----------|------------|
| `POST .../request.json` → `authority` | `InitiatePaymentAsync` · RequestReference=`authority` |
| Redirect `StartPay/{authority}` | `RedirectUri` |
| Browser return `Authority` + `Status=OK\|NOK` | Non-authoritative |
| `POST .../verify.json` | Required for Success evidence (code 100 / already 101) |
| `inquiry` | Status only — **not** verify ([docs](https://www.zarinpal.com/docs/paymentGateway/otherMethods/Inquiry)) → `QueryPaymentStatusAsync` |
| Refund API / panel | `InitiateRefundAsync` — **fail closed** while CBI disable active |

**BrowserReturn ≠ success** · ConfirmIfEligible unchanged.

---

## 5. Initiation mapping

| TravelCore | Zarinpal |
|------------|----------|
| Amount | Integer `amount` + explicit `currency` |
| Currency unit | Official: `IRR` (rial) or `IRT` (toman) on request ([connect docs](https://www.zarinpal.com/docs/paymentGateway/connectToGateway)) |
| Verify amount note | Verify table documents amount **به ریال** — TravelCore must send verify amount consistent with request unit; **v1 recommendation: always use `currency=IRR` and TravelCore IRR minor/major policy locked at impl** |
| Correlation | `metadata.order_id` = PaymentAttempt id; persist `authority` |
| callback_url | HTTPS return URL |
| description | Non-sensitive booking label |
| mobile/email | Optional metadata only |

Unsupported non-IRR TravelCore currencies → fail closed for Iran v1.

---

## 6. Callback / verification

| Topic | Design |
|-------|--------|
| Query | `Authority`, `Status` |
| Rule | Call verify **only if** Status=OK; NOK → Failed/cancel path |
| Verify codes | 100 = first success · 101 = already verified success (idempotent success) |
| Amount match | Snapshot vs verify request amount |
| Duplicate | 101 + PaymentAttempt state machine |
| Tamper / wrong authority | Unverified / Failed |
| Inquiry | Never substitute for verify |

---

## 7. Amount-unit mapping (critical)

| Fact | Status |
|------|--------|
| Request supports `currency` IRR \| IRT | **VERIFIED** official connect docs |
| Default if currency omitted | **UNKNOWN** — do not omit; always send explicit `IRR` in v1 |
| Verify docs say amount in رial | **VERIFIED** table wording |
| TravelCore Money | Keep canonical money; adapter converts to Zarinpal integer + currency enum |
| Guessing toman vs rial without `currency` | **Forbidden** |

---

## 8. Idempotency

No Stripe-style Idempotency-Key documented.

TravelCore protects via:

- Unique PaymentAttempt / authority persistence  
- Verify 100/101 semantics  
- Callback processor unchanged-attempt rules  
- Browser refresh must not double-Confirm  

---

## 9. Refund findings

| Dimension | Status |
|-----------|--------|
| API_DESIGNED | **YES** — feature + API marketing ([refund](https://www.zarinpal.com/features/refund/)) |
| OPERATIONALLY_AVAILABLE | **NO (documented temporary)** — *«این سرویس حسب دستور بانک مرکزی، موقتاً غیرفعال است.»* |
| Full refund required by business | Conflict until re-enabled |
| Partial | Documented on feature page — deferred for TravelCore v1 |
| Adapter behavior while disabled | **Fail closed** on `InitiateRefundAsync` · no fake success |

**Production refund path:** `BLOCKED_ON_REFUND_AVAILABILITY` until official re-enable confirmed (re-check at activation gate).

---

## 10. Currency / settlement

| Topic | Status |
|-------|--------|
| Charge | IRR / IRT presentation units on Iranian rails |
| Settlement | Practical IRR merchant settlement — **AED not assumed** |
| Non-IRR settlement | UNKNOWN / not claimed |

---

## 11. Merchant / KYC blockers

| Item | Status |
|------|--------|
| Natural person | PARTIAL / common product — live tourism MCC **UNKNOWN** |
| Tax / e-namad | UNKNOWN exact list — confirm in panel/support |
| Bank account | Expected — UNKNOWN exact rules |
| Domain / callback HTTPS | Expected |
| User willingness | Yes ≠ completed |

---

## 12. Configuration / secrets (names only)

```text
Payment:Zarinpal:Enabled
Payment:Zarinpal:ProviderKey
Payment:Zarinpal:MerchantId
Payment:Zarinpal:UseSandbox
Payment:Zarinpal:CallbackBaseUrl
Payment:Zarinpal:DefaultCurrency   # lock IRR
Payment:Zarinpal:RefundsEnabled    # false until operational confirm
```

No values in repo.

---

## 13. Production fail-closed

1. No Production registration without Architect gate + live merchant.  
2. No fallback to sandbox/stripe/behpardakht when `zarinpal` selected.  
3. Missing merchant id → unavailable / definitive failure.  
4. `RefundsEnabled=false` while CBI disable → refund initiation fails honestly.  
5. `NamedProductionAdapterImplemented` only after accepted production adapter (Stripe test did not flip it).

---

## 14. Test matrix (future)

Request mapping · IRR explicit currency · toman path rejected or separately tested · callback OK/NOK · verify 100/101 · inquiry ≠ verify · duplicate callback · bad authority · refund fail-closed when disabled · Booking independence · no Confirm shortcut.

Sandbox: official `sandbox.zarinpal.com` + arbitrary UUID merchant id ([sandbox docs](https://www.zarinpal.com/docs/paymentGateway/sandBox)).

---

## 15. Parity comparison vs Behpardakht Mellat (no winner)

| Dimension | Behpardakht Mellat (T009) | Zarinpal (T010) |
|-----------|---------------------------|-----------------|
| Onboarding complexity | Bank/terminal/docs heavier (PARTIAL) | Faster aggregator UX (PARTIAL) |
| Integration complexity | SOAP verify+settle sequence | REST request/verify; simpler |
| Refund certainty | API yes · entitlement UNKNOWN | API yes · **CBI temporary disable VERIFIED** |
| Reconciliation | Inquiry + settle refs | Inquiry + ref_id · inquiry≠verify |
| Testability | Fixtures/SOAP stubs | Official sandbox host |
| Merchant prerequisites | Tax/e-namad/bank PARTIAL | Similar unknowns + panel KYC |
| Operational risk | Contract/terminal issuance | Refund outage risk higher **now** |
| Amount units | IRR · rial/toman UNKNOWN at terminal | Explicit IRR/IRT on request |

**No final selection in this task.**

---

## 16. Implementation task breakdown (not executed)

| Step | Scope |
|------|--------|
| A | Adapter + REST client + DI gate |
| B | Callback + verify mapping |
| C | Refund mapping + fail-closed when disabled |
| D | Sandbox/fixtures |
| E | Merchant test E2E |
| F | Production activation gate (incl. refund re-check) |

---

## 17. External facts still required

Merchant activation · tourism MCC · exact KYC list · refund CBI re-enable confirmation · 10-minute expiry behavior · domain allowlisting if any.

---

## 18. Pipeline inbox artifact audit (T010)

| State | Count | Notes |
|-------|-------|-------|
| Tracked under `docs/pipeline/inbox/` | **48** | Includes `.gitkeep` + `README.md` + **46** transport stubs accidentally committed in TC-P35-T008 |
| Untracked | **1** | `TC-P35-T009.task.md` (left alone) |

**Recommendation:** dedicated Architect-authorized **inbox cleanup** task — do **not** delete in T010.

---

## 19. Recommended next authorized task

**Iran comparison / selection decision worksheet** (Architect+user) **or**  
**Inbox transport cleanup** **or**  
first Iran adapter fixture implementation for the track Architect chooses — **no auto-pick**.
