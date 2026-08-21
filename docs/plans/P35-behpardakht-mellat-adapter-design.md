# P35 — Behpardakht Mellat Adapter Design Lock

| Field | Value |
|-------|--------|
| Document | `docs/plans/P35-behpardakht-mellat-adapter-design.md` |
| Task-ID | `TC-P35-T009` |
| Date | **2026-08-21** |
| Nature | Architecture / design lock only — **no code · no SDK · no credentials · no Iran final pick** |
| Primary readiness | **`READY_FOR_ADAPTER_IMPLEMENTATION_WITH_TEST_FIXTURES`** *(design locked; code needs Architect `.task.md`)* |
| Production readiness | **`BLOCKED_ON_PROVIDER_ACCOUNT_FACTS`** |
| Architecture gap | **Not** `BLOCKED_ON_ARCHITECTURE_GAP` · **NO CORE PAYMENT REDESIGN REQUIRED** |

Sources (authoritative / Tier A copies):  
Shaparak migration notes (`bpm.shaparak.ir`) · Behpardakht/Mellat PGW manuals (`bpPayRequest` / `bpVerifyRequest` / `bpSettleRequest` / `bpInquiry*` / `bpRefundRequest`) · English PGW user manual copies.  
Secondary portals (`my.behpardakht.com` how-tos) = PARTIAL only for onboarding UX.

---

## 1. Readiness split

| Layer | Classification |
|-------|----------------|
| Design | **LOCKED** (this document) |
| Code with fixtures / SOAP stubs | **May** be Architect-authorized later as `READY_FOR_ADAPTER_IMPLEMENTATION_WITH_TEST_FIXTURES` |
| Production | **`BLOCKED_ON_PROVIDER_ACCOUNT_FACTS`** |
| Iran vendor selection | **Not decided** — Zarinpal track still required |

This task does **not** authorize implementation.

---

## 2. Contracting / provider identity

| Entity | Role (evidence-based) |
|--------|------------------------|
| **Bank Mellat** | Brand / banking relationship context; preference stated by business |
| **Behpardakht Mellat** | Practical online IPG operator (SOAP PGW manuals) |
| **Shaparak** | National switch / hosting path (`bpm.shaparak.ir`) for internet transactions |
| Merchant contract | **UNKNOWN** exact paper identity until account issued |

**TravelCore ProviderKey (if implemented later):** `behpardakht` or `mellat-behpardakht` — lock exact string at implementation task.  
Do **not** use ProviderKey `sandbox` or `stripe`.

Bank Mellat ≠ automatic Behpardakht terminal without contract.

---

## 3. Adapter placement

| Item | Decision |
|------|----------|
| Type | `BehpardakhtMellatPaymentProviderGateway : IPaymentProviderGateway` |
| Module | `TravelCore.Modules.Payment.Infrastructure` |
| SOAP/HTTP client | Infrastructure-internal only (no Contracts leakage) |
| Booking | **No** Behpardakht types/packages |
| DI | Mirror Stripe/Sandbox gated registration |

---

## 4. Lifecycle mapping

| Provider step | TravelCore |
|---------------|------------|
| `bpPayRequest` → RefId | `InitiatePaymentAsync` → RequestReference=RefId (or orderId+RefId strategy) |
| Redirect POST `startpay.mellat` | `RedirectUri` / form-post helper |
| Browser return POST ResCode… | Non-authoritative; may trigger **query/verify** only |
| `bpVerifyRequest` | Required before Success evidence |
| `bpSettleRequest` | Ops step after verify (manuals: settle for settlement cycle) — map inside verify/success apply or explicit follow-up call **without** Booking Confirm |
| `bpInquiry*` | `QueryPaymentStatusAsync` |
| `bpRefundRequest` | `InitiateRefundAsync` (entitlement UNKNOWN) |

**BrowserReturn ≠ success** · **Verify required** · ConfirmIfEligible unchanged.

---

## 5. Initiation mapping

| TravelCore | Provider |
|------------|----------|
| Amount | Long amount in **IRR** (manuals use IRR units — confirm rial vs toman at terminal) |
| Currency | IRR only for v1; other codes fail closed |
| orderId | Unique long derived from PaymentAttempt (stable, unique per pay/refund pair rules) |
| callBackUrl | HTTPS production/test callback URL |
| terminalId / userName / userPassword | Config secrets — never logged |
| localDate / localTime | Server clock |
| Correlation | Persist RefId + orderId on PaymentAttempt |

---

## 6. Verification / callback / settle

| Topic | Design |
|-------|--------|
| Callback fields | ResCode, SaleOrderId, SaleReferenceId, RefId, CardHolderInfo… per manuals |
| Trust | Signature/domain controls per provider + **server verify** |
| Sequence | Return → `bpVerifyRequest` → (optional) `bpSettleRequest` |
| Duplicate callback | PaymentAttempt idempotency / unchanged if already Succeeded |
| Tamper / bad ResCode | Unverified or Failed — fail closed |
| Amount match | Execution snapshot vs reported — existing applier |
| Timeout | Network timeout ≠ AttemptFailed (existing trust boundary) |

---

## 7. Idempotency (no Stripe-style keys)

TravelCore protects via:

- Unique PaymentAttempt / orderId uniqueness on initiate  
- Callback processor + attempt state machine  
- Safe re-verify/settle (provider may return already-verified codes — map carefully)  
- Browser refresh must not create second Success

Do not invent provider Idempotency-Key.

---

## 8. Refund / reversal

| Topic | Status |
|-------|--------|
| API surface | VERIFIED manuals: `bpRefundRequest` / related |
| Full refund | Required by business — entitlement **UNKNOWN** |
| Reversal vs post-settle refund | PARTIAL in manuals — confirm with Behpardakht |
| Partial | Deferred unless Architect requires |
| TravelCore | Map full refund to existing refund ports when authorized |

---

## 9. Currency / settlement

| Topic | Status |
|-------|--------|
| Charge | IRR rails VERIFIED class |
| Amount units | Confirm rial vs toman at terminal — **UNKNOWN until merchant docs** |
| AED settlement | **UNKNOWN / not assumed** |
| Multi-currency business posture | Accept IRR settlement for Iran path |

---

## 10. Merchant / KYC blockers

| Item | Status |
|------|--------|
| Natural person (حقیقی) | PARTIAL (portal UX) — live approval UNKNOWN |
| Tax / e-namad / permit | PARTIAL secondary — confirm official |
| Bank Mellat account | UNKNOWN if mandatory |
| Terminal credentials | Not obtained |
| Domain allowlisting | PARTIAL (manuals mention domain checks) |
| Tourism MCC | UNKNOWN |
| User willingness | Yes — **≠ completed** |

---

## 11. Configuration / secrets (names only)

```text
Payment:Behpardakht:Enabled
Payment:Behpardakht:ProviderKey
Payment:Behpardakht:TerminalId
Payment:Behpardakht:UserName
Payment:Behpardakht:UserPassword
Payment:Behpardakht:CallbackBaseUrl
Payment:Behpardakht:WsdlUrl          # e.g. shaparak pgw wsdl
Payment:Behpardakht:StartPayUrl
```

No values in repo. Fail closed if missing when Enabled.

---

## 12. Production fail-closed

1. Never register in Production without Architect production gate + terminal.  
2. No fallback to sandbox/stripe when `behpardakht` selected.  
3. Incomplete config → initiation unavailable / definitive failure.  
4. `NamedProductionAdapterImplemented=true` only after accepted **production** adapter registration (Stripe test mode did **not** flip it; same rule).  
5. Test fixtures ≠ production.

---

## 13. Test matrix (future)

Initiate mapping · IRR amount units · callback parse · verify sequence · settle sequence · duplicate callback · bad signature/ResCode · amount mismatch · timeout · refund mapping · Booking independence · no Confirm shortcut · Production gate closed.

---

## 14. Implementation task breakdown (not executed)

| Step | Scope |
|------|--------|
| A | Adapter + SOAP client + DI gate |
| B | Callback + verify (+ settle) mapping |
| C | Refund/reversal mapping |
| D | Contract fixtures / recorded SOAP responses |
| E | Merchant test-environment E2E |
| F | Production activation gate |

---

## 15. External facts still required

Terminal issuance · exact contractual entity · rial/toman · refund entitlement · 10-minute session TTL · e-namad/tax completion · domain binding · tourism approval.

---

## 16. Recommended next authorized task

**`TC-P35-T010` — Zarinpal adapter design lock** (parity with this document; still no Iran final pick)  
**or** Behpardakht fixture-based implementation after Architect chooses code-first.

Also: Stripe awaits test-account credentials (independent track).
