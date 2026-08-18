# TC-P20-T009 — Payment hardening and evidence pack

**Task:** TC-P20-T009 — Hardening + evidence  
**Product HEAD at T009 start:** `7aab5b6` (`TC-P20-T008` **ACCEPTED**)  
**Date:** 2026-08-18  
**Scope:** Tests, architecture guardrails, security/privacy evidence, documentation, SoT sync — **no new product capability**.  
**Forbidden in this task:** new P20-R decision · real provider · provider SDK · provider credentials · confirmed Booking cancellation · consumed capacity reversal · partial Refund · general cancellation policy · accounting · settlement · agency settlement · wallet · fraud · chargeback · subscription billing · smart provider routing · manual financial mutation · public admin Payment surface · future phase implementation · unrelated refactoring · dependency upgrades · `TC-P20-GATE` execution.  
**Not this task:** `TC-P20-GATE` (**NOT EXECUTED**). **P20 remains IN PROGRESS**. Do **not** close the phase.

## 1. Mission checklist

| # | Verify | Result |
|---|--------|--------|
| 1 | Independent Payment module/schema `payment`; initial target Booking (P20-R1) | **PASS** — T001 |
| 2 | PaymentStatus Pending/Succeeded; PaymentAttempt Created/Initiated/Succeeded/Failed (P20-R2) | **PASS** — T002 |
| 3 | Provider-neutral ports; **BrowserReturn != PaymentSuccess** (P20-R3) | **PASS** — T003 |
| 4 | One Booking → one Payment; attempt retry; DB uniqueness (P20-R4) | **PASS** — T004 |
| 5 | PaymentExecutionSnapshot + payment-driven Booking confirm + outbox (P20-R5) | **PASS** — T005 |
| 6 | Full Refund + compensation durability; Confirmed cancel DEFERRED (P20-R6) | **PASS** — T006 |
| 7 | Booking-scoped public Payment; token reuse; noindex; no cards (P20-R7) | **PASS** — T007 |
| 8 | Capability model; zero providers valid; internal operational reads (P20-R8) | **PASS** — T008 |
| 9 | No new product capability in this task | **PASS** — evidence/docs + strengthened guardrails / callback correlation isolation only |
| 10 | `TC-P20-GATE` remains NOT EXECUTED | **PASS** — this pack does not close GATE |

## 2. Accepted product commits (P20)

| Task | Commit | Essence |
|------|--------|---------|
| PLAN | `aca9c44` | Authoritative P20 plan |
| T001 | `1ec8963` | Independent Payment module · schema `payment` — P20-R1 |
| T002 | `75a4f84` | Payment aggregate + PaymentAttempt lifecycle — P20-R2 |
| T003 | `32e555d` | Provider-neutral initiation/verification/callback — P20-R3 |
| T004 | `f286d9f` | One Booking/one Payment · retry safety · reconciliation — P20-R4 |
| T005 | `c7c846b` (verify `ecc61c4` · docs `930a3be`) | Execution snapshot · confirm · outbox/inbox — P20-R5 |
| T006 | `33f08d1` (docs `dfb45d8`) | Refund · compensation · Pending cancel after refund — P20-R6 |
| T007 | `542cee9` (docs `8daeba7`) | Public Booking-scoped Payment UX/auth/privacy — P20-R7 |
| T008 | `f11041a` (docs `7aab5b6`) | Capability model · operational reads · production-readiness — P20-R8 |

Architect acceptance of PLAN and T001–T008 is as issued. T009 prepares gate evidence; it does **not** execute `TC-P20-GATE`.

**Ledger:**

- TC-P20-PLAN ACCEPTED
- TC-P20-T001 ACCEPTED
- TC-P20-T002 ACCEPTED
- TC-P20-T003 ACCEPTED
- TC-P20-T004 ACCEPTED
- TC-P20-T005 ACCEPTED
- TC-P20-T006 ACCEPTED
- TC-P20-T007 ACCEPTED
- TC-P20-T008 ACCEPTED
- TC-P20-T009 awaiting architect review
- TC-P20-GATE NOT EXECUTED

## 3. Locked decisions (all RESOLVED)

| ID | Essence |
|----|---------|
| **P20-R1** | Independent Payment module. Schema `payment`. Initial target = Booking. **Payment != Booking**. **Payment != Pricing**. **Payment != Quote**. **Payment != BookingMonetarySnapshot**. **Payment != Bank Settlement**. **Payment != Accounting Ledger**. **Payment != Agency Settlement**. **PaymentStatus != BookingStatus**. **PaymentSucceeded != BookingConfirmed**. **BookingCancelled != PaymentRefunded**. No shared DbContext. No peer-schema FK. |
| **P20-R2** | **Payment != PaymentAttempt**. **PaymentStatus != PaymentAttemptStatus**. **Failed PaymentAttempt != Failed Payment**. PaymentStatus exactly Pending / Succeeded. PaymentAttemptStatus exactly Created / Initiated / Succeeded / Failed. At most one legitimate successful attempt. No new Attempt after Payment Succeeded. |
| **P20-R3** | Provider-neutral ports. Named Provider = NONE. **BrowserReturn != PaymentSuccess**. **UnverifiedCallback != PaymentSuccess**. **ClientSuccessFlag != PaymentSuccess**. **ProviderRedirect != PaymentSuccess**. Authoritative provider verification required. Real Provider SDK = NO. |
| **P20-R4** | One Booking → one logical Payment. Retry = PaymentAttempt. Database-backed uniqueness. Ambiguous/timeout ≠ Failed. Unresolved Attempt blocks unsafe retry. **Reconciliation != Settlement**. **Reconciliation != Accounting**. Distributed delivery is at-least-once; local effects are idempotent/effectively-once. |
| **P20-R5** | BookingMonetarySnapshot → trusted obligation → immutable PaymentExecutionSnapshot. No live Pricing recalculation. Provider amount/currency match required. Payment success outbox is atomic with Payment Succeeded. Booking owns confirmation; Payment does not mutate Booking. |
| **P20-R6** | **Payment != Refund**. RefundStatus Pending / Succeeded. RefundAttemptStatus Created / Initiated / Succeeded / Failed. PaymentStatus stays Succeeded after Refund. One logical Refund per Succeeded Payment. **RefundSucceeded != BookingCancelled**. Confirmed Booking cancellation remains DEFERRED. Consumed capacity reversal remains DEFERRED. Partial Refund remains DEFERRED. |
| **P20-R7** | Public Payment is Booking-scoped. Reuse `X-TravelCore-Booking-Access-Token`. Missing/wrong token and cross-user → 404. Client amount/currency/success ignored. No card collection. No public Refund API. Private pages noindex. FA/EN/AR, mobile-first, accessible, bidi-safe, Server Component First. |
| **P20-R8** | Explicit capabilities: RedirectInitiation / CallbackVerification / PaymentStatusQuery / RefundInitiation / RefundVerification / RefundStatusQuery. Zero production providers valid. **OperationalRead != FinancialTruthAuthority**. Recheck uses AuthoritativeProviderQuery. No manual SetStatus / ForceSuccess / MarkPaid / MarkRefunded / ForceConfirm. **Production Provider: NONE / NOT CONFIGURED**. |

## 4. Boundary / ownership matrix

| Concern | Owner | P20 posture |
|---------|-------|-------------|
| Payment / PaymentAttempt / Refund / RefundAttempt / execution snapshot | **Payment** | Transactional SoT in schema `payment` |
| BookingStatus / CapacityHold / passengers / contact / monetary snapshot | **Booking** | Payment does not write Booking tables |
| Price / Quote | **Pricing** | Payment does not calculate tax/fee/discount/FX |
| Public payment pages | **PublicExperience / Booking composition** | **PublicExperience != Payment Source of Truth** |
| Retrieval / ranking | **Search** | Payment/Refund transactional data is not projected |
| IndexPolicy | **SEO** | Payment/return pages always noindex |
| Accounting / settlement / wallet / fraud / chargeback | **OUT / DEFERRED** | Not implemented |

## 5. Invariant evidence (T001–T008 + T009 hardening)

### 5.1 R1 ownership
- Independent module + schema `payment`.
- No shared DbContext. No peer-schema FK.
- Payment.Infrastructure may consume Booking.Contracts only — never Booking.Infrastructure/Domain or Pricing.Infrastructure.
- Booking.Infrastructure consumes Payment.Contracts only — never Payment.Infrastructure/Domain.

### 5.2 R2 lifecycle
- Closed PaymentStatus set Pending / Succeeded.
- Closed PaymentAttemptStatus set Created / Initiated / Succeeded / Failed.
- Failed attempt does not fail the Payment. Success is irreversible.

### 5.3 R3 provider trust
- Callback processor requires verified provider evidence.
- Browser return page cannot mutate Payment.
- Network timeout / unknown initiation remains Created, not Failed.

### 5.4 R4 uniqueness / retries
- Unique logical Payment per Booking (`ux_payments_booking_id`).
- Concurrent GetOrCreate converges to one PaymentId.
- At most one Created/Initiated PaymentAttempt.
- Unresolved Attempt blocks a different retry key.
- Failed Attempt allows explicit new Attempt.
- Process-local ConcurrentDictionary / SemaphoreSlim / lock is not correctness authority.

### 5.5 R5 obligation / confirmation
- Snapshot bind is immutable; different obligation conflicts; initiation without snapshot rejected.
- Provider amount/currency mismatch cannot succeed Payment.
- Payment Succeeded + `PaymentSucceededIntegrationEvent` commit atomically in `payment.outbox_messages`.
- Booking consumer revalidates Pending Booking, snapshot, amount/currency, people, Active unexpired hold.
- Expired/Released/Cancelled delayed delivery does not confirm; `BookingConfirmationRecoveryIssue` exists.
- No generic `Confirm()` / `SetConfirmed()`.

### 5.6 R6 Refund / compensation
- One Payment → at most one logical Refund.
- Refund amount/currency = PaymentExecutionSnapshot.
- Compensation required event has no amount authority.
- RefundSucceeded cancels Pending Booking and releases Active hold; Expired stays Expired; Released stays Released.
- Confirmed Booking is not cancelled. Consumed hold is not released.
- Technical Booking handler failure leaves inbox empty for retry and does not invent compensation-required business evidence (`Missing_Evidence_Leaves_Inbox_Empty_For_Retry`).

### 5.7 R7 public surface
- Exact routes: `GET /api/booking/public/{bookingId}/payment` and `POST /api/booking/public/{bookingId}/payment/initiation`.
- Frontend: `/[locale]/bookings/[bookingId]/payment` and `/[locale]/bookings/[bookingId]/payment/return`.
- Callback: `POST /api/payment/providers/{providerKey}/callback`.
- Missing/wrong token / unknown Booking / cross-user → 404.
- No public Payment list. No generic Payment-by-id. No public Refund API.
- Token lives in sessionStorage; never URL/query/localStorage.
- No production provider → public initiation 503.

### 5.8 R8 capability / operational reads
- Capabilities declared, not inferred from ProviderKey.
- Duplicate ProviderKey rejected. Disabled/unknown fail safely. No failover.
- Internal `IPaymentOperationalQuery` only. `GET /api/payment/operational/{id}` = 404 even with Booking token.
- Recheck methods do not accept a caller-chosen Succeeded/Failed result.
- Unsupported Payment/Refund status-query does not mutate.

### 5.9 T009 correlation isolation
- Callback for Payment A cannot mutate Payment B.
- Collection evidence (non-refund callback kind) cannot succeed Refund.
- Refund callback kind cannot succeed PaymentAttempt.

## 6. Persistence / outbox / inbox inventory

| Flow | Mechanism | Table |
|------|-----------|-------|
| Payment success outbox | `PaymentSucceededIntegrationEvent` | `payment.outbox_messages` |
| Booking Payment-success inbox | `BookingPaymentSucceededIntegrationHandler` | `booking.payment_success_inbox` |
| Booking compensation outbox | `BookingPaymentCompensationRequiredIntegrationEvent` | `booking.outbox_messages` |
| Payment compensation inbox | `BookingPaymentCompensationRequiredHandler` | `payment.compensation_inbox` |
| Refund-success outbox | `RefundSucceededIntegrationEvent` | `payment.outbox_messages` |
| Booking Refund-success inbox | `BookingRefundSucceededIntegrationHandler` | `booking.refund_success_inbox` |
| Recovery evidence | `BookingConfirmationRecoveryIssue` | `booking.booking_confirmation_recovery_issues` |

Payment tables: `payments`, `payment_attempts`, `payment_initiation_idempotency`, `payment_reconciliation_issues`, `refunds`, `refund_attempts`, `refund_reconciliation_issues`, `compensation_inbox`, `outbox_messages`.

## 7. Route inventory

**Backend**
- `POST /api/booking/public/initiations`
- `GET /api/booking/public/{bookingId}`
- `GET /api/booking/public/{bookingId}/payment`
- `POST /api/booking/public/{bookingId}/payment/initiation`
- `POST /api/payment/providers/{providerKey}/callback`

**Absent (404):** `/api/payment`, `/api/payment/refund`, `/api/payment/{id}`, `/api/payment/operational/{id}`, `/api/admin/payments/{id}`, `/api/booking/public/{id}/confirm`, `/api/booking/public/{id}/payment/refund`.

**Frontend**
- `/[locale]/bookings/[bookingId]/payment` (noindex)
- `/[locale]/bookings/[bookingId]/payment/return` (noindex; BrowserReturn != PaymentSuccess)

## 8. Security / failure-mode matrix

| Threat / failure | Accepted behavior |
|------------------|-------------------|
| Booking ownership bypass | 404 |
| Amount/currency tampering (client) | ignored / non-authoritative |
| Amount/currency mismatch (provider) | Payment/Refund not succeeded; reconciliation issue |
| Forged / unverified callback | no mutation |
| Callback replay | idempotent logical success |
| Duplicate Payment / Attempt | unique constraint / active-attempt block |
| Ambiguous provider timeout | not Failed; unsafe retry blocked |
| Payment success crash window | outbox atomic with Succeeded |
| Expired/released/cancelled after Payment | no Confirm; recovery issue; compensation path |
| Duplicate compensation / Refund | one logical Refund |
| Forged Refund evidence / collection-as-refund | unknown attempt; no mutation |
| Token leakage via URL/localStorage | not used; sessionStorage only |
| Card-data collection | absent |
| Provider secret leakage | no committed secrets; no raw callback JSON persistence |
| Zero production providers | host starts; public initiation 503 |
| Provider lacks Refund capability | no RefundAttempt; Refund stays Pending |
| Open redirect | server-selected provider redirect only; no client provider URL authority |

## 9. Deferred / out-of-scope (preserved)

- Real production provider integration = DEFERRED
- Confirmed Booking cancellation = DEFERRED
- Consumed capacity reversal = DEFERRED
- Partial Refund = DEFERRED
- General cancellation/refund policy = DEFERRED
- Chargeback/dispute = DEFERRED
- Accounting ledger = OUT/DEFERRED
- Bank settlement = OUT/DEFERRED
- Agency settlement = OUT/DEFERRED
- Wallet = OUT/DEFERRED
- Fraud/risk engine = OUT/DEFERRED
- Recurring/subscription billing = OUT/DEFERRED
- Smart provider routing/failover = DEFERRED

## 10. Guardrail / test surfaces

| Area | Evidence |
|------|----------|
| Unit | Payment uniqueness, provider trust, snapshot, outbox, refund, capabilities, operational recheck, callback correlation isolation |
| Architecture | `PaymentBoundaryGuardrailTests` + `PaymentPhaseBoundaryGuardrailTests` — R1–R8, evidence pack, routes, peer refs, secrets, Search, frontend cardless/noindex |
| Persistence | concurrent Payment/Attempt/Refund; outbox atomicity; confirmation races; compensation E2E |
| Host | no public Payment CRUD; unknown provider callback 404; zero-provider startup; Booking token cannot access operational HTTP |
| Frontend | FA/EN/AR copy; sessionStorage token; LtrValue/MoneyText; `min-h-11`; robots noindex |

## 11. Validation commands (this task)

```text
dotnet build TravelCore.sln
dotnet test tests/Unit/TravelCore.Modules.Payment.UnitTests
dotnet test tests/Unit/TravelCore.Modules.Booking.UnitTests
dotnet test tests/Architecture/TravelCore.ArchitectureTests
dotnet test tests/Integration/TravelCore.Persistence.IntegrationTests
dotnet test tests/Integration/TravelCore.Host.IntegrationTests
npm run typecheck   (src/frontend/web)
npm run lint
npm run build
git diff --check
```

## 12. Gate readiness

All accepted P20-R1 through P20-R8 invariants are verified together. Full validation of this task is recorded in the RESULT envelope.

**P20 READY FOR GATE: YES**

This task still does **not** execute GATE. **P20 remains IN PROGRESS**. **TC-P20-GATE NOT EXECUTED**.
