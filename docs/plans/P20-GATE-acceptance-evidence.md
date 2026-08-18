# TC-P20-GATE — P20 Acceptance Evidence

**Task:** TC-P20-GATE — P20 Payment Acceptance Gate  
**Baseline HEAD:** `e5ba5e6` (`TC-P20-T009` **ACCEPTED**)  
**Date:** 2026-08-18  
**Scope:** Gate / acceptance only — **no new Payment capability**. Next phase is **not executed** here.

## 1. Preconditions

| Check | Result |
|-------|--------|
| USER PIPELINE + continuity override | YES |
| Architect Auto-Execute GATE after T009 ACCEPT | YES |
| PLAN + T001–T009 ACCEPTED · R1–R8 RESOLVED | YES |
| Evidence pack | YES — [`P20-T009-hardening-and-evidence-pack.md`](P20-T009-hardening-and-evidence-pack.md) |
| Working tree at gate start | CLEAN (`e5ba5e6`) |

## 2. Checklist (architect GATE)

| # | Criterion | Result |
|---|-----------|--------|
| 1 | Independent Payment module/schema `payment`; target Booking (P20-R1) | **PASS** — T001 |
| 2 | PaymentStatus Pending/Succeeded; PaymentAttempt Created/Initiated/Succeeded/Failed (P20-R2) | **PASS** — T002 |
| 3 | Provider-neutral ports; **BrowserReturn != PaymentSuccess** (P20-R3) | **PASS** — T003 |
| 4 | One Booking → one Payment; attempt retry; DB uniqueness (P20-R4) | **PASS** — T004 |
| 5 | PaymentExecutionSnapshot + confirm + outbox (P20-R5) | **PASS** — T005 |
| 6 | Full Refund + compensation; Confirmed cancel DEFERRED (P20-R6) | **PASS** — T006 |
| 7 | Booking-scoped public Payment; token; noindex; no cards (P20-R7) | **PASS** — T007 |
| 8 | Capability model; zero providers; internal operational reads (P20-R8) | **PASS** — T008 |
| 9 | Hardening / evidence | **PASS** — T009 |
| 10 | Payment != Booking · Pricing · Quote · BookingMonetarySnapshot | **PASS** |
| 11 | No new Payment capability in Gate | **PASS** — evidence only |

## 3. Locked decisions

**P20-R1…R8 all RESOLVED** — see [`P20-implementation-plan.md`](P20-implementation-plan.md).

**Production Provider: NONE / NOT CONFIGURED**. Real Provider SDK = NO.

Confirmed Booking cancellation remains **DEFERRED**. Consumed capacity reversal remains **DEFERRED**. Partial Refund remains **DEFERRED**.

## 4. Accepted product commits (P20)

| Task | Commit | Status |
|------|--------|--------|
| PLAN | `aca9c44` | ACCEPTED |
| TC-P20-T001 | `1ec8963` | ACCEPTED |
| TC-P20-T002 | `75a4f84` | ACCEPTED |
| TC-P20-T003 | `32e555d` | ACCEPTED |
| TC-P20-T004 | `f286d9f` | ACCEPTED |
| TC-P20-T005 | `c7c846b` (verify `ecc61c4` · docs `930a3be`) | ACCEPTED |
| TC-P20-T006 | `33f08d1` (docs `dfb45d8`) | ACCEPTED |
| TC-P20-T007 | `542cee9` (docs `8daeba7`) | ACCEPTED |
| TC-P20-T008 | `f11041a` (docs `7aab5b6`) | ACCEPTED |
| TC-P20-T009 | `75456e9` (docs `e5ba5e6`) | ACCEPTED |

## 5. Ownership / architecture matrix

| Invariant | Result |
|-----------|--------|
| Payment != Booking | **PASS** |
| Payment != Pricing | **PASS** |
| Payment != Quote | **PASS** |
| Payment != BookingMonetarySnapshot | **PASS** |
| Payment != PaymentAttempt | **PASS** |
| Payment != Refund | **PASS** |
| PaymentStatus != BookingStatus | **PASS** |
| PaymentStatus != PaymentAttemptStatus | **PASS** |
| Failed PaymentAttempt != Failed Payment | **PASS** |
| PaymentSucceeded != BookingConfirmed | **PASS** |
| BrowserReturn != PaymentSuccess | **PASS** |
| UnverifiedCallback != PaymentSuccess | **PASS** |
| ClientSuccessFlag != PaymentSuccess | **PASS** |
| ProviderRedirect != PaymentSuccess | **PASS** |
| BookingCancelled != PaymentRefunded | **PASS** |
| RefundSucceeded != BookingCancelled | **PASS** |
| OperationalRead != FinancialTruthAuthority | **PASS** |
| PublicExperience != Payment Source of Truth | **PASS** |

## 6. Public Payment contract

- API: `GET /api/booking/public/{bookingId}/payment` · `POST /api/booking/public/{bookingId}/payment/initiation`
- Callback: `POST /api/payment/providers/{providerKey}/callback`
- Frontend: `/[locale]/bookings/[bookingId]/payment` · `/[locale]/bookings/[bookingId]/payment/return` (always `robots: { index: false, follow: false }`; token never in URL)
- Anonymous credential: `X-TravelCore-Booking-Access-Token` (sessionStorage)
- Missing/wrong token / unknown Booking / cross-user → **404**
- No public Payment list · no generic Payment CRUD · no public Refund API · no card collection
- Operational mutation surface: **NONE**
- No peer-schema FK · no shared DbContext · no peer Infrastructure dependency · no distributed transaction

## 7. Validation battery (gate re-run)

Recorded from T009 PASS (`e5ba5e6`) and re-confirmed in this Gate commit:

| Suite | Result | Detail |
|-------|--------|--------|
| `dotnet build TravelCore.sln` | **PASS** | 0 Error(s) |
| Payment.UnitTests | **PASS** | **81** |
| Booking.UnitTests | **PASS** | **54** |
| ArchitectureTests | **PASS** | **286** |
| Persistence.IntegrationTests | **PASS** | **81** |
| Host.IntegrationTests | **PASS** | **56** |
| Frontend `npm run typecheck` | **PASS** | clean |
| Frontend `npm run lint` | **PASS** | clean |
| Frontend `npm run build` | **PASS** | payment + return routes present |
| `git diff --check` | **PASS** | clean |

```text
dotnet build TravelCore.sln
dotnet test tests/Unit/TravelCore.Modules.Payment.UnitTests
dotnet test tests/Unit/TravelCore.Modules.Booking.UnitTests
dotnet test tests/Architecture/TravelCore.ArchitectureTests
dotnet test tests/Integration/TravelCore.Persistence.IntegrationTests
dotnet test tests/Integration/TravelCore.Host.IntegrationTests
npm run typecheck
npm run lint
npm run build
git diff --check
```

## 8. Explicit OUT / DEFER

- Real production provider integration — **DEFERRED**
- Confirmed Booking cancellation — **DEFERRED**
- Consumed capacity reversal — **DEFERRED**
- Partial Refund — **DEFERRED**
- General cancellation/refund policy — **DEFERRED**
- Chargeback/dispute — **DEFERRED**
- Accounting ledger / bank settlement / agency settlement / wallet / fraud / subscriptions — **OUT/DEFERRED**
- Smart provider routing/failover — **DEFERRED**
- Next phase product — **not executed in this Gate**

## 9. Architect STOP rules honored

| Rule | Honored |
|------|---------|
| No new Payment product in GATE | YES |
| No inventing beyond P20-R1–R8 | YES |
| No next-phase product code | YES |
| No force-push / history rewrite | YES |

## 10. Gate outcome

**TC-P20-GATE COMPLETE** · P20 COMPLETE · T001–T009 ACCEPTED · P20-R1–R8 RESOLVED.

Next phase from SoT: **P21 — Hotel Booking (PLANNED)**. This Gate does **not** start P21 product work.
