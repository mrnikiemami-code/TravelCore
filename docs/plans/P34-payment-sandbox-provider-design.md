# P34 — Payment Sandbox Provider Design (Option B)

| Field | Value |
|-------|--------|
| Document | `docs/plans/P34-payment-sandbox-provider-design.md` |
| Task-ID | `TC-P34-T002` |
| Phase | P34 — Payment & Confirmation Readiness |
| Status | **PROPOSED / Cursor PASS** — awaiting Architect ACCEPT |
| Nature | Architecture / contract planning only |
| Baseline | `127aea0` · `TC-P34-T001` ACCEPTED · Option B Architect-locked |
| Forbidden here | Adapter implementation · DI registration · Booking Confirm edits · fake UI success |

---

## 1. Purpose

Lock a **non-production sandbox** that exercises the **real** Payment orchestration and Booking confirmation path:

```text
Pending Booking
  → Payment initiation (Payment app layer)
  → PaymentAttempt
  → Sandbox provider session / redirect
  → Callback / verify
  → PaymentSucceeded evidence (when verified)
  → Booking payment_success_inbox
  → ConfirmIfEligible (Booking-owned)
```

The sandbox is **not** a production provider and must never be presented as one.

Option A remains the default traveler posture until sandbox is Architect-activated in a non-prod host.

---

## 2. Chosen adapter placement

| Decision | Choice |
|----------|--------|
| Interface | Existing `IPaymentProviderGateway` only |
| Module ownership | Payment Infrastructure adapter assembly / folder under Payment module (not Booking, not Frontend) |
| Suggested type name | `SandboxPaymentProviderGateway` (exact name free to implementer) |
| ProviderKey | Fixed reserved key, e.g. `sandbox` (must parse via existing `ProviderKey`) |
| Capability declaration | Explicit `PaymentProviderCapability` flags — **never inferred from key** |
| Refunds | **Out of first sandbox slice** — declare Refund capabilities **unsupported** / fail closed |

Placement must mirror P20 checklist: provider-specific types stay in the adapter; Payment core remains provider-neutral.

Reference contracts:

- `IPaymentProviderGateway`
- `PaymentProviderCapability` / `PaymentProviderDescriptor`
- `PaymentProviderTrustBoundary`
- `PaymentProviderCallbackEndpoints` (`POST /api/payment/providers/{providerKey}/callback`)
- Public Booking payment compose/initiation (Tour-first)

---

## 3. Existing interfaces reused (no reinvent)

| Port / service | Role |
|----------------|------|
| `IPaymentProviderGateway` | Initiate / Verify / Query / VerifyCallback |
| `IPaymentProviderResolver` | Resolve by `ProviderKey` |
| `PaymentInitiationService` | Server-owned initiation + attempt persistence |
| Public Booking payment services | Tour compose + initiate (access-token gated) |
| Callback endpoints | Envelope → `VerifyCallbackAsync` → apply |
| Payment success outbox | `PaymentSucceededIntegrationEvent` |
| `BookingPaymentSucceededIntegrationHandler` | Inbox |
| `BookingPaymentConfirmationService.ConfirmIfEligibleAsync` | Sole Tour Confirm authority for this slice |

Sandbox **must not** call Booking Confirm APIs directly.

---

## 4. Activation / configuration rules

### 4.1 Environment gate (fail-closed)

Sandbox may register **only** when host environment is explicitly non-production, e.g.:

- `ASPNETCORE_ENVIRONMENT` ∈ { `Development`, `Local`, `Staging` } **and**
- config flag `Payment:Sandbox:Enabled = true`

**Production:** sandbox registration must be impossible (guard in `PaymentModule` / host composition). Fail closed = no gateway in DI.

### 4.2 Trust-boundary interaction (critical)

Today public Tour initiation short-circuits when:

```text
PaymentProviderTrustBoundary.NamedProductionAdapterImplemented == false
→ ProviderUnavailable
```

**Design lock for Option B:**

| Flag | Sandbox meaning |
|------|-----------------|
| `NamedProductionAdapterImplemented` | Remains **`false`** until a **real** production adapter exists |
| New / explicit non-prod gate | `SandboxAdapterAvailableForPublicInitiation` (name illustrative) — true **only** when sandbox registered + env gate passes |

Implementation must update public initiation eligibility to allow:

```text
(NamedProductionAdapterImplemented && production adapter)
  OR
(SandboxAdapterAvailable && non-production env)
```

**Forbidden:** flipping `NamedProductionAdapterImplemented = true` merely to enable sandbox.

### 4.3 DefaultProviderKey

Non-prod config may set `DefaultProviderKey = sandbox` only under the env gate.

Production config must not point DefaultProviderKey at sandbox.

### 4.4 Descriptor honesty

`PaymentProviderDescriptor` for sandbox:

- `DisplayName` includes explicit non-production label (e.g. `Sandbox (non-production)`)
- `AvailableForPublicInitiation = true` only when env + config allow
- Capabilities: `RedirectInitiation | CallbackVerification | PaymentStatusQuery` (minimum for first slice)

---

## 5. Lifecycle sequence

```text
1. Traveler has Pending Booking + access token
2. Public compose returns Initiate/Retry when sandbox available (non-prod)
3. POST initiation → PaymentInitiationService → creates PaymentAttempt (Created)
4. Resolver picks SandboxPaymentProviderGateway
5. InitiatePaymentAsync → returns redirect URI to sandbox outcome page/API
6. Traveler selects outcome: Success | Failure | Cancelled
7. Sandbox posts callback envelope to /api/payment/providers/sandbox/callback
8. VerifyCallbackAsync validates HMAC/shared secret (config-only; not in repo)
9. Verified success → PaymentAttempt/Payment Succeeded + outbox
10. Booking inbox → ConfirmIfEligible
11. Browser return route still ≠ PaymentSuccess (existing invariant)
```

### Supported outcomes (existing lifecycle only)

| Outcome | PaymentAttempt | Notes |
|---------|----------------|-------|
| Success | Succeeded | Only after verified callback / authoritative verify |
| Failure | Failed | Honest failure; Booking stays Pending |
| Cancelled | Failed or abandoned per existing semantics | No Confirm |

Do **not** invent new Payment domain enums for sandbox.

---

## 6. Callback / verification / idempotency

| Concern | Rule |
|---------|------|
| Callback shape | Same `PaymentCallbackEnvelope` path as production adapters |
| Verification | Shared secret / HMAC from secure config; unverified ≠ success |
| Idempotency | Same PaymentAttempt / Payment ids; duplicate verified success is no-op |
| Replay | Replay of same evidence must not mutate another Payment |
| Tampered callback | Reject; no status mutation |
| Query | `QueryPaymentStatusAsync` supported for ambiguity / reconciliation smoke |
| Browser return | Informational only; never marks success |

---

## 7. Booking confirmation boundary

**Locked:**

- Payment success = **evidence**, not Confirm
- `ConfirmIfEligible` remains the only Tour confirmation authority for this slice
- Monetary mismatch / missing hold / Cancelled booking → **no Confirm**
- Frontend must not show Confirmed until Booking read says Confirmed

---

## 8. Security guardrails

| Guardrail | Requirement |
|-----------|-------------|
| Production isolation | No sandbox DI registration in Production |
| No ForceSuccess endpoints | No public `MarkPaid` / `ForceConfirm` |
| Secrets | Sandbox HMAC secret in env/config store — never repository |
| UI labeling | Traveler-visible “non-production sandbox” copy when sandbox is active |
| CSRF / auth | Keep Booking access-token gate for initiation; callback uses provider verification |
| Audit | Persist attempt + verification evidence under Payment ownership |
| ArchitectureTests | Assert Production cannot resolve sandbox key; assert NamedProductionAdapterImplemented stays false while only sandbox exists |

---

## 9. Implementation task breakdown (proposals)

| Unit | Theme | Notes |
|------|-------|-------|
| **T003** | Sandbox adapter + DI + env gates | Implement `IPaymentProviderGateway`; wire eligibility without flipping production flag |
| **T004** | Payment schema / demo host readiness | Apply Payment migrations on demo if needed |
| **T005** | Tour public UX leave Option A when sandbox available | Labeled initiate + return; restore honest compose |
| **T006** | Evidence pack | initiate · success callback · duplicate · failure · Confirm eligibility · mismatch non-confirm |
| **GATE** | P34 honesty + boundaries | C may remain OUT |

Cursor must not auto-start these until Architect `.task.md` files arrive.

---

## 10. Tests / evidence matrix (for future implementation)

| Case | Expectation |
|------|-------------|
| Initiate (sandbox enabled, non-prod) | Attempt Created → Initiated; redirect returned |
| Verified success callback | Payment Succeeded; outbox emitted |
| Duplicate success callback | Idempotent; single Confirm eligibility process |
| Failed outcome | Attempt Failed; Booking Pending |
| Cancelled outcome | No Confirm |
| Tampered callback | Rejected; no success |
| Browser return only | No success |
| ConfirmIfEligible happy path | Pending → Confirmed when hold/money OK |
| Monetary mismatch | No Confirm |
| Production host | Sandbox unavailable; Option A / ProviderUnavailable |

---

## 11. Forbidden shortcuts

- Fake success button that writes Booking Confirmed
- Setting `NamedProductionAdapterImplemented = true` for sandbox
- Registering test/fake as production
- Bypassing Payment initiation / callback verification
- Client success flag ⇒ PaymentSucceeded
- Secrets in git
- Presenting sandbox as real payment in traveler copy
- Hotel/Flight confirmation scope creep in first sandbox slice

---

## 12. Relation to Option A

Until T003–T005 are Architect-authorized and activated on a non-prod host:

- Public traveler UX may remain Option A honest stop
- Design acceptance ≠ adapter live

---

## 13. Recommended next authorized task

**`TC-P34-T003` — Implement labeled Sandbox `IPaymentProviderGateway` + non-prod DI/eligibility gates** (no production flip; Tour-first only).
