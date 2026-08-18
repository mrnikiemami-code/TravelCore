# P20 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P20-PLAN` |
| Phase | P20 — Payment |
| Status | PLAN ACCEPTED · **P20-R1 = RESOLVED** · **P20-R2 = RESOLVED** · **P20-R3 = RESOLVED** · **P20-R4 = RESOLVED** · **P20-R5 = RESOLVED** · **P20-R6 through P20-R8 = OPEN** · T005 durability outbox delivered, awaiting architect acceptance |
| Baseline | `d258933` (`docs(booking): add P19 acceptance gate evidence [TC-P19-GATE]`) |
| Authoritative sources | `docs/ROADMAP.md` § P20 · `docs/PROJECT-STATE.md` · `04-module-boundaries.md` § Payment · `05-dependency-rules.md` · `06-cross-module-communication.md` · `07-data-architecture.md` (schema `payment`) · `08-persistence-and-migrations.md` · `29-module-local-transactional-outbox.md` · `docs/domain/module-ownership-matrix.md` · `15-future-architecture-transition-map.md` § S Payment · ADR 0003 (Money) · ADR 0004 (NodaTime) · P12 Pricing · P19 Booking (`P19-GATE-acceptance-evidence.md`) |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P20** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی و موجودی تصمیم را برای Cursor تعریف می‌کند.

> **Envelope note:** Authored from **repository SoT** after architect `TC-P19-GATE` ACCEPT (`d258933`). Next phase is **explicitly** `P20 — Payment` in `docs/ROADMAP.md` (not guessed). Under PIPELINE continuity, ceremonial confirms are **not required**. **No product code in PLAN task.** Open R# must stay OPEN until architect lock. **Do not implement T001 until this PLAN is ACCEPTED and P20-R1 is architecturally locked.** Do **not** invent P20-R1 through P20-R8 closures here.

---

## 0. Next-phase resolve (from SoT; no extra discovery task)

| Question | Answer from SoT |
|----------|-----------------|
| P19 completion | **COMPLETE / ACCEPTED** — Gate evidence `d258933`; architect ACCEPT issued this session |
| Authoritative next phase ID | **P20** |
| Title / purpose | **Payment** — attempts · provider abstraction · callback/webhook validation · success/failure lifecycle · refund foundation · payment snapshots · financial auditability (`docs/ROADMAP.md` § P20) |
| PLAN already existed? | **NO** — this document is the first P20 PLAN |
| SoT conflict? | **NO** — ROADMAP names P20 after P19; constitution already names Payment as a Commerce module; schema `payment` is listed in `07-data-architecture.md`. Capability themes are **not** ownership transfers to Booking, Pricing, Agency settlement, or accounting ledger. |
| Dedicated module/schema in SoT today? | **YES (conceptual)** — Payment module exists in `04-module-boundaries.md`; PostgreSQL schema `payment` is listed. **No Payment product code / DbContext / aggregates exist in the repository yet.** |
| Booking confirmation today? | **DEFERRED** — P19 has no `Confirm()`, no Payment-driven confirmation, no PaymentIntent |
| Missing business fact blocking PLAN authorship? | **NO** — PLAN may enumerate R# and IN/OUT/DEFER without locking product semantics or choosing a provider |
| Invented phase? | **NO** — P20 is already listed in ROADMAP |
| Speculative provider? | **NONE selected** — constitution says “provider abstraction”; no Stripe/Zarinpal/etc. lock in SoT |

---

## 1. Phase Purpose

P20 باید قابلیت **Payment** را به‌عنوان دامنهٔ تراکنشی مستقل معرفی کند: اجرای و ثبت تلاش‌های پرداخت پولی و نتایج authoritative آن‌ها — **بدون** دزدیدن مالکیت Booking status، Pricing/Quote، BookingMonetarySnapshot، Tour catalog، Party/Identity master، Agency settlement، accounting ledger، Notification delivery، or Search/SEO.

Preserve (already locked by constitution / P12 / P19; PLAN does not reopen them):

1. **Payment != Booking** — Booking owns BookingStatus; Payment owns payment execution facts (`04-module-boundaries.md`).
2. **Payment != Pricing != Quote != BookingMonetarySnapshot** — Pricing owns Price/Quote; Booking owns accepted commercial snapshot; Payment owns money movement (P12, P19-R5, ADR 0003).
3. **PaymentStatus != BookingStatus** · **PaymentSucceeded != BookingConfirmed** · **BookingCancelled != PaymentRefunded**.
4. **Payment != Bank Settlement != Accounting Ledger != Agency Settlement**.
5. Payment DbContext **must not** mutate Booking tables. Success emits a contract/event; Booking reacts (`15-future-architecture-transition-map.md` § S).
6. Money = Amount + CurrencyCode. No Toman CurrencyCode. No float monetary persistence. No FX engine invented in P20 unless SoT later requires it (ADR 0003).
7. **PublicExperience = composition only** — payment UI is presentation; Payment remains transactional SoT. Honest copy only; sandbox success is not production truth.
8. **Notification = delivery owner** — Payment may emit semantic events; it does not own SMTP/SMS/push.
9. Module-local transactional outbox / consumer-owned inbox (`07` / `29`). No second event bus.
10. **AI-readiness = structured payment facts** — identifiers, status, provider references, timestamps — **بدون** fraud-AI / LLM payment agent.

P19 تحویل داد: Tour Booking Pending initiation, capacity hold, Quote snapshot, hashed access token. Confirm and Payment execution remain DEFERRED until P20 locks and implements them **without** bypassing Booking capacity/people/snapshot invariants.

---

## 2. Starting Baseline

| Item | Value |
|------|--------|
| P19 Gate | `TC-P19-GATE` COMPLETE / ACCEPTED (`d258933`) |
| P19 evidence | [`P19-GATE-acceptance-evidence.md`](P19-GATE-acceptance-evidence.md) · [`P19-T009-hardening-and-evidence-pack.md`](P19-T009-hardening-and-evidence-pack.md) |
| P19 Plan | ACCEPTED · R1–R8 RESOLVED · T001–T009 ACCEPTED |
| Baseline HEAD | `d258933` |
| P00–P19 | COMPLETE |
| Payment module / schema | **Conceptual only** (`04-module-boundaries.md` · `07-data-architecture.md` schema `payment`) — no product code |
| Booking Confirm() | **Not implemented** (P19-R6 DEFERRED to Payment integration) |
| Provider SDK | **NONE** in repository |
| Notification module | **Not implemented** (architecture docs only) |
| Existing Payment APIs | **NONE** |

---

## 3. Scope classification (IN / OUT / DEFER)

Classifications below are **planning inventory**, not architect locks. Dependent product tasks must not treat IN as permission to invent unlocked R# closures.

| Concept | Classification | Notes |
|---------|----------------|-------|
| Independent Payment module + schema `payment` (SoT candidate) | **IN (candidate)** | Constitution + data-architecture; exact scaffolding locked by **P20-R1** |
| Payment aggregate vs PaymentAttempt + lifecycle | **IN (candidate)** | Constitution names Payment · PaymentAttempt; exact split **P20-R2** |
| Provider-neutral initiation / verification / callback security | **IN (candidate)** | Abstraction required; **no named provider lock in PLAN**; **P20-R3** |
| Idempotency / retries / duplicate payment / reconciliation baseline | **IN (candidate)** | **P20-R4** — not full accounting reconciliation |
| Booking confirmation integration after authoritative Payment success | **IN (candidate)** | Booking still owns Confirm decision; **P20-R5** |
| Refund / cancellation / compensation boundary | **IN (candidate as boundary)** | Constitution “پایهٔ refund”; depth **P20-R6** |
| Public payment step + authorization / privacy | **IN (candidate)** | PE composes; Payment SoT; **P20-R7** |
| Provider capability / operational read / hardening | **IN (candidate)** | **P20-R8** |
| Transactional outbox in `payment` schema | **IN (candidate)** | Module-local outbox; Booking consumes via inbox/idempotency |
| Authoritative webhook/callback verification (server-side) | **IN (candidate)** | Browser return URL is not final evidence |
| Booking-linked Payment for Tour Booking | **IN (candidate)** | Initial payer target is existing P19 Booking; not a universal payment platform unless later SoT requires it |
| Accounting ledger / GL / bank settlement | **OUT** | **Payment != Accounting Ledger != Bank Settlement** |
| Agency commission / settlement / wallet | **OUT** | P24 / AgencyMarketplace; **Payment != Agency Settlement** |
| Stored cards / card PAN/CVV persistence | **OUT** | PCI; do not collect card credentials unless a later R# lock requires hosted-fields and still forbids storage |
| Recurring billing / subscriptions / BNPL / credit | **OUT** | |
| Multi-provider ranking/routing/failover | **OUT / DEFER** | Do not invent routing unless SoT requires; **P20-R8** |
| FX engine / Toman CurrencyCode | **OUT** | ADR 0003 |
| Notification provider | **OUT / DEFER** | Payment does not send SMTP/SMS |
| Fraud engine / AI scoring | **OUT** | |
| Price/Quote engine | **OUT** | P12 remains owner |
| Direct Booking table writes from Payment | **OUT** | Forbidden by constitution |
| HotelBooking / Flight live payment products | **OUT** | P21 / P22 |
| Generic payment CRUD / public payment listing | **OUT** | Enumeration resistance |
| Distributed DB transaction Payment+Booking+Notification | **OUT** | Forbidden by `05-dependency-rules.md` |

---

## 4. Explicit non-goals

P20 must not silently become:

- Booking (status authority / Confirm ownership)
- Pricing / Quote engine
- Accounting ledger or bank settlement
- Agency settlement / commission netting / wallets
- Card vault / PCI card storage
- Notification provider
- Fraud-AI platform
- Multi-acquirer routing marketplace
- Generic mall checkout for every future product
- Second event bus / shared DbContext / peer-schema FK

---

## 5. Core invariants (carry forward; not new locks)

1. Payment != Booking != Quote != Price != BookingMonetarySnapshot.
2. PaymentStatus != BookingStatus.
3. PaymentSucceeded != BookingConfirmed.
4. BookingCancelled != PaymentRefunded.
5. Payment != Bank Settlement != Accounting Ledger != Agency Settlement.
6. Client cannot authoritatively set Payment amount/currency/success.
7. Browser-controlled callback parameters are not final payment evidence.
8. Provider reference is not primary identity.
9. Do not claim exactly-once external provider processing; design for at-least-once + idempotent consumption.
10. Do not hold long DB locks across provider HTTP calls unless a later R# lock proves otherwise.
11. Successful Payment + failed Booking confirmation is a first-class scenario (compensation/retry), not a silent Failed/Refunded collapse — exact policy **P20-R5 / P20-R6 OPEN**.
12. Payment success must not bypass Booking capacity / people / monetary-snapshot invariants.
13. No card-number/CVV storage.
14. No public payment enumeration.
15. Sandbox success != production truth.

---

## 6. Decision inventory (must not invent)

**P20-R1 lock (architect):**

```
P20-R1 = RESOLVED

Payment = Independent Domain Module
schema = payment
initial Payment target = Booking
P20 Payment scope is initially Tour Booking payment
Payment does not own Booking/Pricing
PaymentStatus != BookingStatus
PaymentSucceeded != BookingConfirmed
no Payment aggregate/lifecycle/provider implemented yet
```

Keep **P20-R6 through P20-R8 = OPEN**.

**P20-R2 lock (architect):**

```
P20-R2 = RESOLVED

Payment = one logical Booking monetary collection
PaymentAttempt = one concrete execution attempt
one Payment may have multiple attempts
PaymentStatus = Pending / Succeeded
PaymentAttemptStatus = Created / Initiated / Succeeded / Failed
Failed PaymentAttempt != Failed Payment
Payment != PaymentAttempt
PaymentStatus != PaymentAttemptStatus
at most one legitimate successful attempt exists
no further collection attempt after Payment succeeds
authoritative provider success requires verified provider evidence
refund remains R6
Booking confirmation remains R5
```

**P20-R3 lock (architect):**

```
P20-R3 = RESOLVED

Payment core is provider-neutral
provider-specific implementation is adapter/infrastructure concern
NamedProvider = NONE
BrowserReturn != PaymentSuccess
UnverifiedCallback != PaymentSuccess
ClientSuccessFlag != PaymentSuccess
ProviderRedirect != PaymentSuccess
ProviderReference != PaymentId
ProviderReference != PaymentAttemptId
NetworkTimeout != PaymentAttemptFailed
provider initiation/verification/query are neutral ports
authoritative provider verification is required for success
provider-specific statuses are translated at adapter boundary
network ambiguity is not definitive payment failure
Booking confirmation remains R5
callback replay/reconciliation automation remains R4
```

**P20-R4 lock (architect):**

```
P20-R4 = RESOLVED

one Booking has one logical Payment in P20 baseline
retries create PaymentAttempts, not new Payments
database-backed uniqueness/idempotency protects duplicate Payment creation
at most one non-terminal Attempt exists per Payment
ambiguous provider outcomes do not become Failed
unresolved attempts block automatic/new retry until reconciled/terminal
callback/provider evidence processing is idempotent
external exactly-once is not assumed
reconciliation uses provider query to converge unresolved state
contradictory terminal evidence is not silently flipped
Reconciliation != Settlement
Reconciliation != Accounting
no automatic cross-provider failover
Booking confirmation remains R5
Refund remains R6
```

Keep **P20-R6 through P20-R8 = OPEN**.



| ID | Topic | Status | SoT notes (not a lock) |
|----|-------|--------|------------------------|
| **P20-R1** | Payment module ownership / target / schema | **RESOLVED** | Independent Payment domain module. Schema `payment`. Initial Payment target = Booking. P20 scope is initially Tour Booking payment. Payment does not own Booking or Pricing. **Payment != Booking**. **PaymentStatus != BookingStatus**. **PaymentSucceeded != BookingConfirmed**. T001: no Payment aggregate/lifecycle/attempt/refund/provider. |
| **P20-R2** | Payment aggregate vs PaymentAttempt and lifecycle | **RESOLVED** | Payment is one logical Booking monetary collection. PaymentAttempt is one concrete execution attempt. One Payment may have multiple attempts. **Payment != PaymentAttempt**. **PaymentStatus != PaymentAttemptStatus**. **Failed PaymentAttempt != Failed Payment**. PaymentStatus = Pending / Succeeded. PaymentAttemptStatus = Created / Initiated / Succeeded / Failed. At most one successful attempt. No further collection attempt after Payment succeeds. Authoritative provider success requires verified provider evidence. Refund remains R6. Booking confirmation remains R5. |
| **P20-R3** | Provider abstraction / initiation / verification / callback security | **RESOLVED** | Payment core is provider-neutral. No named provider selected. **BrowserReturn != PaymentSuccess**. **UnverifiedCallback != PaymentSuccess**. Provider initiation/verification/query are neutral ports. Authoritative provider verification is required for success. Network ambiguity is not definitive failure. Signature validation belongs in adapters. Booking confirmation remains R5. Callback replay/reconciliation remains R4. Amount/currency mismatch enforcement deferred to R5. |
| **P20-R4** | Idempotency / retries / duplicate payment / reconciliation | **RESOLVED** | One Booking -> one logical Payment. Retries are PaymentAttempts. Database-backed uniqueness/idempotency. Ambiguous outcomes do not become Failed and block unsafe retry. Callback/evidence processing is idempotent. Reconciliation uses provider query. **Reconciliation != Settlement**. **Reconciliation != Accounting**. External exactly-once is not assumed. Booking confirmation remains R5. Refund remains R6. |
| **P20-R5** | Booking confirmation integration after authoritative Payment success | **RESOLVED** | Payment binds to immutable Booking monetary obligation via `PaymentExecutionSnapshot`, enforces provider amount/currency integrity, and uses Booking-owned confirmation transaction (consume Active CapacityHold, Pending -> Confirmed). Payment must not write Booking tables. PaymentSucceeded remains distinct from BookingConfirmed; when confirmation prerequisites fail, Booking does not confirm (compensation/refund remains deferred to P20-R6). |
| **P20-R6** | Refund / cancellation / compensation boundary | **OPEN** | Constitution includes refund foundation. Confirmed Booking cancel remains coupled to refund/consumed-capacity (P19 DEFERRED). Do not assume partial refund. Do not auto-refund merely because Confirm failed unless R# locks that policy. |
| **P20-R7** | Public Payment UX / anonymous-authenticated authorization / privacy | **OPEN** | PE composes. Booking access token / object-level actor must not be replaced by UUID-only payment access. No generic CRUD. Noindex transaction pages. Honest FA/EN/AR copy. No fake Payment completed / Booking confirmed. |
| **P20-R8** | Provider selection/capabilities / operational read model / hardening | **OPEN** | Do not select Stripe/Zarinpal/etc. in PLAN. Currency capability must not be silently overclaimed. Operational reads are not a BI platform. Hardening/evidence is T009/GATE, not a license to invent engines. |

---

## 7. Boundary sketches (planning only)

### 7.1 Ownership / schema

**P20-R1 lock:** independent Payment module at `src/backend/Modules/Payment/{Contracts,Domain,Infrastructure}` + schema `payment`. No shared DbContext. No peer-schema FK. Initial Payment target = Booking. Payment.Infrastructure must not reference Booking.Infrastructure/Domain. A Payment-owned opaque BookingReference is sufficient in T001.

### 7.2 Amounts

Payable amount is **not** client-authored. Candidate source is BookingMonetarySnapshot (Booking-owned historical Quote copy). Payment does not recalculate tax/fee/discount. Currency follows ADR 0003.

### 7.3 Provider trust

Initiation may redirect/open a provider session. Authoritative outcome is server-side verification of provider callback/webhook (or equivalent server fetch). Return-query success flags are hints only. Secrets stay off the public surface. Raw sensitive provider payloads are not stored by default.

### 7.4 Booking integration

Likely sequence (not locked): PaymentSucceeded (authoritative) → Booking evaluates Confirm prerequisites → Confirm or compensation path. Timeouts of Payment, provider session, Quote, and CapacityHold **must not be collapsed** into one clock (P19 already: QuoteExpiresAt != CapacityHold.ExpiresAt conceptually).

### 7.5 Events (names are examples only; taxonomy not locked)

Candidates: PaymentInitiated · PaymentSucceeded · PaymentFailed · RefundSucceeded. Consume via module-local outbox + consumer inbox. Do not introduce a new bus.

### 7.6 Authorization

Anonymous Booking retrieval uses Booking-scoped token (P19). Payment access must remain object-level / Booking-linked. Login alone does not grant payment on arbitrary BookingId.

---

## 8. Task sequence (proposed)

Do **not** execute any product task until PLAN ACCEPT **and** the matching R# is locked.

### TC-P20-PLAN — this document

### TC-P20-T001 — Payment module scaffolding / ownership / target boundary

- Purpose: Independent Payment module + ownership contracts (**P20-R1 = RESOLVED**).
- Executed after PLAN ACCEPT + R1 lock.
- Must not implement Payment aggregate, provider, callback, refund, Confirm, or public checkout.

### TC-P20-T002 — Payment aggregate / attempt / lifecycle boundary

- Depends on **P20-R2**.

### TC-P20-T003 — Provider abstraction + initiation/verification/callback security

- Depends on **P20-R3**. No speculative SDK. Implemented after R3 lock: provider-neutral port, trusted initiation/verification/callback processor, generic `/api/payment/providers/{providerKey}/callback`. Amount mismatch executable comparison deferred to R5.

### TC-P20-T004 — Idempotency / retries / duplicate-payment / reconciliation baseline

- Depends on **P20-R4**. Implemented after R4 lock: unique Booking Payment, scoped initiation idempotency, concurrent attempt protection, callable reconciliation without scheduler.

### TC-P20-T005 — Booking confirmation integration after Payment success

- Depends on **P20-R5**. Booking still owns Confirm. Must address PaymentSucceeded ∧ Confirm-failed.

### TC-P20-T006 — Refund / cancellation / compensation boundary

- Depends on **P20-R6**.

### TC-P20-T007 — Public payment experience / authorization / privacy

- Depends on **P20-R7**. Honest UX; no fake success.

### TC-P20-T008 — Provider capability / operational read / remaining public-ops slice

- Depends on **P20-R8**. Vacant if R8 has no independent product slice.

### TC-P20-T009 — Hardening + evidence

- Guardrails + evidence pack; **no new capability**. Does **not** execute GATE.

### TC-P20-GATE — Acceptance Gate

- Evidence only. No new Payment product in GATE. Do not start P21 inside GATE.

Do not manufacture empty capabilities merely to fill numbering.

---

## 9. Architecture invariants (carry forward)

1. Payment != Booking · Payment != Pricing · Payment != Quote · Payment != BookingMonetarySnapshot.
2. PaymentStatus != BookingStatus · PaymentSucceeded != BookingConfirmed · BookingCancelled != PaymentRefunded.
3. Payment != Bank Settlement · Payment != Accounting Ledger · Payment != Agency Settlement.
4. Payment DbContext does not mutate Booking; no distributed optional transaction across Payment+Booking+Notification.
5. Client is not monetary or success authority.
6. Provider reference != primary identity.
7. PublicExperience != Payment Source of Truth.
8. Booking PII / payment artifacts != Search/SEO data.
9. Notification Intent != Notification Delivery.
10. Historical BookingMonetarySnapshot survives live Pricing/FX; Payment records movement against that snapshot, it does not rewrite it.

---

## 10. Conflicts

**None material.** Schema `payment` is already listed. Payment module is already named. P19 explicitly deferred Payment execution and Confirm. No accepted SoT names a concrete provider. PLAN does not choose one.

---

## 11. Repository safety

- Branch `main` · fast-forward push only · no force · CLEAN working tree before RESULT.
- One docs commit for PLAN (no product code).
- After PLAN ACCEPT, Auto-Execute first locked product task only when architect envelope names it **and** P20-R1 is locked.
- Do not start T001 from this PLAN document alone.
- Do not prematurely resolve P20-R1 through P20-R8.
