# P35 — Production Payment Provider Readiness

| Field | Value |
|-------|--------|
| Document | `docs/plans/P35-production-payment-provider-readiness.md` |
| Task-ID | `TC-P35-T001` |
| Phase | P35 — Production Payment Provider Readiness |
| Status | **PROPOSED / Cursor PASS** — awaiting Architect ACCEPT |
| Nature | Architecture / security / ops planning only |
| Baseline | `ee6684b` · `TC-P34-GATE` ACCEPTED WITH KNOWN LIMITATIONS |
| Forbidden here | Real provider SDK · credentials · Booking lifecycle changes · production txns |

---

## 1. Current reusable capability map (from P34)

| Capability | Status |
|------------|--------|
| `IPaymentProviderGateway` port | **Reusable** |
| Initiation orchestration (`PaymentInitiationService`) | **Reusable** |
| PaymentAttempt / Payment lifecycle | **Reusable** |
| Callback route + verify-before-apply | **Reusable** |
| Idempotency (initiation keys + inbox) | **Reusable (partial ops)** |
| Payment success outbox → Booking inbox | **Reusable** |
| `ConfirmIfEligible` (Tour) | **Reusable — Booking-owned** |
| Public Tour payment compose/initiate | **Reusable** |
| Sandbox adapter (non-prod) | **Reference implementation** — never production |
| Named production adapter | **NONE** (`NamedProductionAdapterImplemented=false`) |

**Conclusion:** Production work is **adapter + config + ops**, not Payment/Booking redesign.

---

## 2. Provider capability checklist (vendor-agnostic)

Any production provider MUST (or must be adaptable to) support:

| Requirement | Notes |
|-------------|-------|
| Initiation / session creation | Via gateway `InitiatePaymentAsync` |
| Redirect or hosted payment UX | Traveler never enters PAN in TravelCore if hosted |
| Server-verifiable callback/webhook | Unverified ≠ success |
| Provider request + transaction references | Persist under Payment ownership |
| Amount + currency validation vs execution snapshot | Mismatch cannot succeed |
| Idempotent replay of same evidence | No double mutation |
| Success / Failure / Cancelled semantics | Map to existing attempt outcomes |
| Status query / reconciliation | Prefer `PaymentStatusQuery` capability |
| Timeout / expiry behavior | `NetworkTimeout ≠ Failed` |
| Refund initiation/verify (if product requires) | Declare capability honestly; partial refund may remain deferred |
| Supported currencies | Explicit matrix — no silent assumption |

**Do not select a vendor in this document** unless Architect + business SoT already names one.

---

## 3. Security requirements

| Topic | Production requirement |
|-------|------------------------|
| Secrets | Secure config/secret store only — never repository |
| Callback verification | Provider signature/HMAC/JWT — fail closed |
| TLS | HTTPS only for initiation + callback |
| Replay protection | Signature + idempotent application |
| Amount/currency | Re-validate against PaymentExecutionSnapshot |
| Provider reference uniqueness | Indexes / uniqueness constraints already modeled |
| Audit | Structured logs of initiate/callback/apply (no secrets/PAN) |
| PII | Minimize; no card data in TravelCore for hosted flows |
| Environment isolation | Prod keys ≠ sandbox keys; sandbox cannot register in Production |
| Credential rotation | Documented procedure; zero-downtime preferred |

---

## 4. Operational requirements

| Topic | Requirement |
|-------|-------------|
| Reconciliation | Periodic query for ambiguous attempts |
| Retry | Safe retries on network ambiguity — never ForceSuccess |
| Provider timeout | Leave attempt open until query/callback resolves |
| Duplicate callback | Idempotent (existing processor) |
| Delayed callback | Still apply if attempt eligible |
| Diagnostics | Admin-safe payment attempt status (no secret dump) |
| Observability | Metrics: initiate, redirect, callback verify fail, success, confirm lag |
| Incident | Runbook: stuck Pending, delayed Confirm, refund mismatch |
| Manual reconciliation | Architect-locked only; no public MarkPaid |

---

## 5. Booking confirmation safety (locked)

- Payment success = **evidence only**
- Tour Confirm remains `BookingPaymentConfirmationService.ConfirmIfEligible`
- Production adapter must **not** call Booking Confirm APIs
- Public `confirmed` must continue to mirror BookingStatus (P34-T005)

---

## 6. Production rollout stages

| Stage | Meaning |
|-------|---------|
| **A** | Adapter + contract/unit tests against gateway |
| **B** | Provider’s official test/sandbox environment (not TravelCore sandbox) |
| **C** | Limited non-production E2E with real test credentials |
| **D** | Production credentials present; feature flag **disabled** |
| **E** | Controlled production activation (single market/currency) |
| **F** | Reconciliation + monitoring evidence gate |

Fail-closed between stages. Do not skip A→E.

---

## 7. External business decisions (Architecture cannot invent)

| Decision | Owner |
|----------|-------|
| Provider / vendor selection | Business + compliance |
| Merchant / acceptor account | Business |
| Countries / markets | Business |
| Settlement currencies | Business + finance |
| Settlement / payout constraints | Business + finance |
| Provider-imposed callback quirks | Vendor + Architect review |
| Regulatory constraints (PCI, local PSP rules) | Compliance |

Mark these as **INPUTS** required before Stage A vendor-specific work.

---

## 8. Proposed future task breakdown (not executable until Architect files)

| Unit | Theme |
|------|-------|
| T002 | External decision intake template + checklist freeze |
| T003 | Selected provider adapter design lock (after vendor chosen) |
| T004 | Adapter implementation Stage A–B |
| T005 | Non-prod E2E Stage C + evidence |
| T006 | Production flag/config Stage D–E |
| GATE | Stage F monitoring/reconciliation honesty |

---

## 9. Acceptance criteria (for later P35 GATE)

1. Production adapter implements `IPaymentProviderGateway` only
2. `NamedProductionAdapterImplemented=true` only when real adapter registered
3. Sandbox remains fail-closed in Production
4. Callback verify-before-apply preserved
5. ConfirmIfEligible remains Booking SoT
6. No secrets in repo; no fake Confirm theater
7. Rollout stages evidenced

---

## 10. Forbidden shortcuts

- Choosing vendor without business input
- Committing credentials / SDK secrets
- Registering sandbox as production
- ForceSuccess / MarkPaid / ForceConfirm
- Frontend inventing payment success
- Skipping callback verification
- Redesigning Booking Confirm for provider convenience

---

## 11. Recommended next authorized task

After Architect ACCEPT:

- **`TC-P35-T002`** — freeze external decision intake + provider selection prerequisites  
  **or** if vendor already decided in business SoT: **T003** adapter design lock for that vendor.
