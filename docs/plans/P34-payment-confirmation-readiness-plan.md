# P34 — Payment & Confirmation Readiness Plan

| Field | Value |
|-------|--------|
| Document | `docs/plans/P34-payment-confirmation-readiness-plan.md` |
| Task-ID | `TC-P34-T001` |
| Phase | P34 — Payment & Confirmation Readiness |
| Status | **PROPOSED / Cursor PASS** — awaiting Architect ACCEPT |
| Nature | Architecture + implementation planning only |
| Baseline | `52116f7` (`TC-P33-GATE` ACCEPTED WITH KNOWN LIMITATIONS) |
| Forbidden here | Provider integration · Booking state mutation · fake payment success · Confirm implementation |

---

## 1. Current state

P33 Tour-first slice is **accepted** through:

```text
Tour Discovery → Tour Detail → Published TourDeparture → Pricing
  → Booking Initiation → Pending Booking → Payment Boundary (Option A)
```

Live traveler posture after P33-T008:

- Pending booking is real (Booking ownership)
- Monetary/Quote snapshot is real (issued inside initiation)
- Payment UI is an **honest stop** — no fake success / receipt / Confirm

### Capability map (what already exists)

| Area | Status |
|------|--------|
| Payment provider ports (`IPaymentProviderGateway`, resolver, capability flags) | **Present** — no production adapter registered |
| Public Tour Booking payment compose / initiation endpoints | **Present** |
| PaymentAttempt / Payment lifecycle + execution snapshot | **Present** |
| Provider callback route + verify-before-apply | **Present** |
| Initiation idempotency + success outbox | **Present (partial ops)** |
| Booking `payment_success_inbox` + confirmation eligibility service | **Present** |
| Named production / sandbox adapter in host DI | **NONE** |
| Labeled sandbox UI | **NONE** |
| Reconciliation scheduler / process-local idempotency authority | **DEFERRED / absent** |

**Posture:** Payment & Confirmation **orchestration is built**; money-movement **activation** is not.

---

## 2. Gaps

1. No `IPaymentProviderGateway` registered for production or labeled sandbox.
2. Demo environments may lack `payment` schema until migrations are applied deliberately.
3. Confirmation path exists in code but never fires without authoritative payment success evidence.
4. Public Option A UX intentionally does not call payment initiation.
5. Ops gaps: reconciliation scheduler, duplicate-callback hardening beyond inbox, production secrets posture.
6. Hotel/Flight confirmation paths are heavier (supplier / ticket evidence) — out of Tour-first next step.

---

## 3. Provider options

| Option | Meaning | When |
|--------|---------|------|
| **A — Keep honest stop** | Traveler stays on Option A boundary until provider selected | Default production-honesty posture |
| **B — Labeled sandbox adapter** | Non-production adapter implementing the real gateway contract; UI must label non-production | After Architect lock + Payment schema readiness |
| **C — Real provider later** | Production adapter + secrets + compliance | After B evidence (or explicit Architect skip of B) |

### Recommendation (smallest safe next step)

**Keep Option A as the live traveler default.**

Authorize **Option B** only as an Architect-locked P34 implementation slice when a money-movement demo is required.

Do **not** jump to **Option C** as the first P34 product task.

**Rationale:** Confirm APIs and Payment core already exist. The risk is fake Confirm / unlabeled success theater — not missing Confirm methods. Sandbox without labeling or registering test as production would violate P20 trust boundaries.

---

## 4. Booking confirmation rules (SoT-aligned)

Preserve:

| Rule | Meaning |
|------|---------|
| Booking ≠ Payment | Payment never writes Booking tables |
| Payment success ≠ automatic Confirm | Booking consumes evidence via eligibility checks |
| Browser return ≠ Payment success | Return route must not mark success |
| Client amount/currency ≠ authority | Snapshot / Payment SoT only |

### Tour confirmation eligibility (existing shape — do not invent conflicting states)

Authoritative payment success may make Booking **eligible** to Confirm only when:

- Inbox dedupe allows processing
- Evidence is authoritative success
- Booking is still Pending (Cancelled → recovery/compensation paths)
- Monetary snapshot amount/currency match
- Contact + passengers present
- Active non-expired capacity hold can be consumed

States to **use existing SoT names**; do not invent parallel enums that conflict with Booking/Payment modules.

Suggested traveler-facing honesty labels (UX composition only — not new domain enums unless Architect authorizes):

| Label | Meaning |
|-------|---------|
| Pending | Booking initiated; not paid; not confirmed |
| Payment unavailable / boundary | Option A stop |
| Payment initiated (future B/C) | Attempt started; not success |
| Payment succeeded (future) | Payment SoT only — still ≠ Confirm until Booking accepts |
| Confirmed | Booking accepted authoritative payment success |
| Payment failed / expired | Honest failure; no fake Confirm |

---

## 5. Operational requirements (plan-level)

| Concern | Requirement |
|---------|-------------|
| Idempotency | Keep initiation keys + inbox dedupe; never double-Confirm |
| Duplicate callbacks | Verify signature/token; ignore unverified; inbox dedupe |
| Retry | Ambiguous provider results → query/reconcile — not ForceSuccess |
| Audit | Persist attempt/result evidence under Payment ownership |
| Observability | Structured logs/metrics on initiate / callback / confirm eligibility |
| Security | No secrets in repo; sandbox ≠ production keys; no card data in TravelCore |

---

## 6. Task breakdown (proposals — not executable until Architect `.task.md`)

| Unit | Theme | Allowed when authorized |
|------|-------|-------------------------|
| **T001** | This readiness plan | Docs only (this task) |
| **T002** | Env / Payment schema honesty | Document + optional migrate demo host — no fake UX |
| **T003** | Sandbox adapter design lock (if B) | ProviderKey · capabilities · labeling · secrets posture |
| **T004** | Implement labeled sandbox gateway (if B) | `IPaymentProviderGateway` — never production |
| **T005** | Tour public UX leave A **or** wire initiate+return with sandbox labels | Preserve BrowserReturn ≠ Success |
| **T006** | Confirmation evidence pack | Pending → (optional Succeeded) → Confirmed / recovery |
| **GATE** | P34 acceptance | Boundaries + honesty; C may remain OUT |

If Architect stays **A-only**, collapse to T001 + env honesty note + GATE (no provider code).

---

## 7. Risks

| Risk | Mitigation |
|------|------------|
| Fake Confirm for demo beauty | Forbidden; keep Option A until B/C locked |
| Registering test adapter as production | Checklist + ArchitectureTests |
| Hotel/Flight scope creep | Tour-first confirmation evidence first |
| Partial refund / Confirmed cancel reopen | Remain Architect-locked DEFERRED |
| Payment schema 500 misread as product bug | Document Option A / env posture |

---

## 8. Dependencies

- P33-GATE ACCEPTED WITH KNOWN LIMITATIONS
- P20 Payment module COMPLETE (ports + public Booking payment surface)
- P19 Booking confirmation-from-payment path COMPLETE
- Architect decision: stay A vs authorize B

---

## 9. Forbidden shortcuts

- Fake payment success / receipt / Confirmed theater
- Browser return or client flag ⇒ PaymentSucceeded / BookingConfirmed
- `ForceSuccess` / `MarkPaid` / `ForceConfirm` / public SetStatus
- Registering fake/test as production provider
- Payment writing Booking tables / shared DbContext / distributed TX
- Client-authored amount/currency as authority
- Secrets in repository
- Treating sandbox success as production truth in traveler copy
- Reopening Partial Refund / Confirmed cancel without Architect lock

---

## 10. Gate criteria (future P34-GATE)

PASS only if:

1. Ownership boundaries preserved (Booking ≠ Payment; success ≠ auto Confirm)
2. No fake money theater in public UX
3. If B: sandbox clearly labeled non-production
4. Confirmation evidence (if claimed) uses authoritative Payment evidence only
5. Known limitations explicit (C may remain OUT)

---

## 11. Recommended next authorized task

After Architect ACCEPT of this plan:

- If stay A: authorize env/docs honesty + defer provider — **or**
- If money demo required: authorize **TC-P34-T003/T004 sandbox design+adapter** (Option B)

Cursor must **not** auto-start provider implementation.
