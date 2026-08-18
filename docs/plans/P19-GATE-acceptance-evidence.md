# TC-P19-GATE — P19 Acceptance Evidence

**Task:** TC-P19-GATE — P19 Tour Booking Acceptance Gate  
**Baseline HEAD:** `3a1f5a1` (`TC-P19-T009` **ACCEPTED**)  
**Date:** 2026-08-18  
**Scope:** Gate / acceptance only — **no new Booking capability**. Ceremonial Gate wait is **not** a pipeline stop. Next phase is **not executed** here.

## 1. Preconditions

| Check | Result |
|-------|--------|
| USER PIPELINE + continuity override | YES |
| Ceremonial GATE token | **Not required** |
| Architect Auto-Execute GATE | YES |
| PLAN + T001–T009 ACCEPTED · R1–R8 RESOLVED | YES |
| Evidence pack | YES — [`P19-T009-hardening-and-evidence-pack.md`](P19-T009-hardening-and-evidence-pack.md) |
| Working tree at gate start | CLEAN (`3a1f5a1`) |

## 2. Checklist (architect GATE)

| # | Criterion | Result |
|---|-----------|--------|
| 1 | Independent Booking module/schema `booking`; target TourDeparture (P19-R1) | **PASS** — T001 |
| 2 | BookingStatus exactly Pending / Confirmed / Cancelled (P19-R2) | **PASS** — T002 |
| 3 | CapacityHold + `pg_advisory_xact_lock` concurrency (P19-R3) | **PASS** — T003 |
| 4 | Booker / passengers / contact snapshots; minimized PII (P19-R4) | **PASS** — T004 |
| 5 | Authoritative Quote → immutable BookingMonetarySnapshot (P19-R5) | **PASS** — T005 |
| 6 | Pending cancel + Active-hold release; Confirm/Payment DEFERRED (P19-R6) | **PASS** — T006 |
| 7 | One Booking aggregate for Direct and Agency (P19-R7) | **PASS** — T007 |
| 8 | Public Pending initiation + hashed token + noindex (P19-R8) | **PASS** — T008 |
| 9 | Hardening / evidence | **PASS** — T009 |
| 10 | Booking != Tour · TourDeparture · Pricing · Payment · TripPlanner · VisaApplication · AgencyMarketplace · Search · SEO · Notification Provider | **PASS** |
| 11 | No new Booking capability in Gate | **PASS** — evidence only |

## 3. Locked decisions

**P19-R1…R8 all RESOLVED** — see [`P19-implementation-plan.md`](P19-implementation-plan.md) open-decisions table.

Payment execution remains **DEFERRED**. Executable payment-driven Booking confirmation remains **DEFERRED**. Confirmed cancellation/refund remains **DEFERRED**. Public initiation ends in **Pending**.

## 4. Accepted product commits (P19)

| Task | Commit | Status |
|------|--------|--------|
| PLAN | `9d4266b` | ACCEPTED |
| TC-P19-T001 | `e198daa` | ACCEPTED |
| TC-P19-T002 | `7caa90a` | ACCEPTED |
| TC-P19-T003 | `8c79b02` | ACCEPTED |
| TC-P19-T004 | `b71fd15` | ACCEPTED |
| TC-P19-T005 | `66ec4e9` | ACCEPTED |
| TC-P19-T006 | `9dca5ef` | ACCEPTED |
| TC-P19-T007 | `2e7937a` | ACCEPTED |
| TC-P19-T008 | `5b4361e` | ACCEPTED |
| TC-P19-T009 | `3a1f5a1` | ACCEPTED |

## 5. Ownership / architecture matrix

| Invariant | Result |
|-----------|--------|
| Booking != Tour | **PASS** |
| Booking != TourDeparture | **PASS** |
| Booking != Pricing | **PASS** |
| Booking != Payment | **PASS** |
| Booking != TripPlanner | **PASS** |
| Booking != VisaApplication | **PASS** |
| Booking != AgencyMarketplace | **PASS** |
| Booking != Search | **PASS** |
| Booking != SEO | **PASS** |
| Booking != Notification Provider | **PASS** |
| CapacityDefinition != CapacityConsumption | **PASS** |
| CapacityHoldStatus != BookingStatus | **PASS** |
| Pending != CapacityHeld | **PASS** |
| Consumed != BookingConfirmed | **PASS** |
| Expired Hold != BookingExpired | **PASS** |
| PlannerTravelerComposition != BookingPassenger | **PASS** |
| BookingPassenger != Party Person Master | **PASS** |
| BookingContactSnapshot != Party | **PASS** |
| BookingContactSnapshot != Identity Account | **PASS** |
| Price != Quote | **PASS** |
| Quote != BookingMonetarySnapshot | **PASS** |
| BookingMonetarySnapshot != PaymentAmount | **PASS** |
| PaymentSucceeded != BookingConfirmed | **PASS** |
| BookingCancelled != PaymentRefunded | **PASS** |
| AgencyOffer != Booking | **PASS** |
| AgencyOffer != Quote | **PASS** |
| PublicExperience != Booking Source of Truth | **PASS** |
| Public Booking initiation != Booking confirmation | **PASS** |
| BookingId != Access Credential | **PASS** |
| BookingStatus != PaymentStatus | **PASS** |
| Booking PII != Search/SEO data | **PASS** |

## 6. Public Booking contract

- Public route: `/[locale]/tours/[slug]/book` (Server Component + prepare-form island; FA/EN/AR)
- Private read route: `/[locale]/bookings/[bookingId]` (always `robots: { index: false, follow: false }`; token never in URL)
- API: `POST /api/booking/public/initiations` · `GET /api/booking/public/{bookingId}`
- Successful public initiation status: **Pending**
- Anonymous credential: raw token once; SHA-256 verifier persisted; header `X-TravelCore-Booking-Access-Token`
- No credential → **404** · wrong token → **404** · correct token → **200** · cross-user → **404**
- Duplicate submit: same BookingId; token not reissued; no extra capacity
- Insufficient capacity → **409** · expired Quote rejected (no Confirmed fabrication)
- CapacityHold statuses: Active / Consumed / Released / Expired
- Concurrency: capacity=1 two concurrent 1-seat holds → exactly one success; capacity=5 with existing 3 + two concurrent 2-seat holds → final ≤ 5
- No Confirm endpoint · no Payment implementation · no public Booking list
- No peer-schema FK · no shared DbContext
- Existing routes remain: `/[locale]/tours/[slug]` · `/[locale]/tours/[slug]/[intent]` · `/[locale]/plan`

## 7. Validation battery (gate re-run)

| Suite | Result | Detail |
|-------|--------|--------|
| `dotnet build TravelCore.sln` | **PASS** | 0 Error(s) |
| Booking.UnitTests | **PASS** | **40** |
| ArchitectureTests | **PASS** | **266** |
| Persistence.IntegrationTests | **PASS** | **46** |
| Host.IntegrationTests | **PASS** | **52** |
| Frontend `npm run typecheck` | **PASS** | clean |
| Frontend `npm run lint` | **PASS** | clean |
| Frontend `npm run build` | **PASS** | `/[locale]/tours/[slug]/book` · `/[locale]/bookings/[bookingId]` |
| `git diff --check` | **PASS** | clean |

```text
dotnet build TravelCore.sln
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

- Payment execution / PaymentIntent / provider / capture / refund — **DEFERRED** (P20)
- Executable payment-driven Booking confirmation — **DEFERRED**
- Confirmed → Cancelled / refund / consumed-capacity reversal — **DEFERRED**
- Public cancellation — **OUT of P19**
- Agency commission / settlement / markup / inbox — **OUT**
- Requote / repricing — **DEFERRED**
- Passport / national ID / document storage — **OUT**
- CRM / Search engine / SEO ownership / Notification provider / AI — **OUT**
- Next phase product — **not executed in this Gate**

## 9. Architect STOP rules honored

| Rule | Honored |
|------|---------|
| No new Booking product in GATE | YES |
| No inventing beyond P19-R1–R8 | YES |
| No next-phase product code | YES |
| No force-push / history rewrite | YES |

## 10. Gate outcome

**TC-P19-GATE COMPLETE** · P19 COMPLETE · T001–T009 ACCEPTED · P19-R1–R8 RESOLVED.

Next phase from SoT: **P20 — Payment (PLANNED)**. This Gate does **not** start P20 product work.
