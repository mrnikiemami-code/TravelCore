# TC-P21-GATE — P21 Acceptance Evidence

**Task:** TC-P21-GATE — Hotel Booking Acceptance Gate  
**Baseline HEAD:** `2706bfb` (`TC-P21-T009` **ACCEPTED**)  
**Date:** 2026-08-18  
**Scope:** Gate / acceptance only — **no new product capability**. Next phase is **not executed** here.

## 1. Preconditions

| Check | Result |
|-------|--------|
| USER PIPELINE + continuity override | YES |
| Architect Auto-Execute GATE after T009 ACCEPT | YES |
| PLAN + T001–T009 ACCEPTED · R1–R8 RESOLVED | YES |
| Evidence pack | YES — [`P21-T009-hardening-and-evidence-pack.md`](P21-T009-hardening-and-evidence-pack.md) |
| Working tree at gate start | CLEAN (`2706bfb`) |

## 2. Checklist (architect GATE)

| # | Criterion | Result |
|---|-----------|--------|
| 1 | Independent HotelBooking module/schema `hotel_booking`; Place catalog (P21-R1) | **PASS** — T001 |
| 2 | Stay / rooms / guests / occupancy / multi-room (P21-R2) | **PASS** — T002 |
| 3 | Availability hold via `IHotelAvailabilitySource` (P21-R3) | **PASS** — T003 |
| 4 | Rate / monetary / cancellation-policy snapshots (P21-R4) | **PASS** — T004 |
| 5 | Supplier reservation + HotelBookingStatus (P21-R5) | **PASS** — T005 |
| 6 | Typed HotelBooking Payment target + dual-evidence (P21-R6) | **PASS** — T006 |
| 7 | Confirmed cancellation process (P21-R7) | **PASS** — T007 |
| 8 | Public transactional journey + independent token (P21-R8) | **PASS** — T008 |
| 9 | Hardening / evidence | **PASS** — T009 |
| 10 | HotelBooking != Place · Tour Booking · Payment | **PASS** |
| 11 | No new HotelBooking capability in Gate | **PASS** — evidence only |

## 3. Locked decisions

**P21-R1…R8 all RESOLVED** — see [`P21-implementation-plan.md`](P21-implementation-plan.md).

**Hotel Catalog != Hotel Booking**. **HotelBooking != Place**. **HotelBooking != Tour Booking**. **Payment Succeeded != HotelBooking Confirmed**. **HotelBookingCancelled != RefundSucceeded**.

Named Hotel Supplier = NONE. Production Hotel Availability Source = NONE. Production Hotel Rate Source = NONE. Production Hotel Reservation Source = NONE. Production Payment Provider = NONE.

## 4. Accepted product commits (P21)

| Task | Commit | Status |
|------|--------|--------|
| PLAN | `f0ec6ae` | ACCEPTED |
| TC-P21-T001 | `7af55b2` / docs `7ebd0f1` | ACCEPTED |
| TC-P21-T002 | `a844bcf` / docs `a0f5c99` | ACCEPTED |
| TC-P21-T003 | `2696407` / docs `77a9b8f` | ACCEPTED |
| TC-P21-T003-VERIFY | `5824acd` / docs `14c594c` | ACCEPTED |
| TC-P21-T004 | `9d24b84` / docs `9f38ef6` | ACCEPTED |
| TC-P21-T005 | `8cc1b28` / docs `53e6e14` | ACCEPTED |
| TC-P21-T006 | `f2d4946` / docs `790765b` | ACCEPTED |
| TC-P21-T007 | `c3fabe9` / docs `836cd92` | ACCEPTED |
| TC-P21-T008 | `63b8ce3` / docs `d8bdf0f` | ACCEPTED |
| TC-P21-T009 | `ae84f62` / docs `2706bfb` | ACCEPTED |

## 5. Ownership / architecture matrix

| Invariant | Result |
|-----------|--------|
| Hotel Catalog owner | **Place** |
| HotelBooking schema | `hotel_booking` |
| live availability authority | `IHotelAvailabilitySource` |
| rate authority | `IHotelRateOfferSource` |
| reservation authority | `IHotelReservationSource` |
| Payment supported target kinds | TourBooking, HotelBooking |
| peer-schema FK | **NO** |
| shared DbContext | **NO** |
| peer Infrastructure dependency | **NO** |
| cross-schema SQL | **NONE** |
| distributed transaction | **NO** |
| supplier SDK | **NO** |
| Payment provider SDK | **NO** |
| PublicExperience != HotelBooking Source of Truth | **PASS** |

## 6. Domain evidence

- HotelBookingStatus: Pending, Confirmed, Cancelled
- HoldStatus: Requested, Active, Released, Expired
- SupplierReservationStatus: Pending, Confirmed, Cancelled
- SupplierReservationAttemptStatus: Created, Initiated, Confirmed, Failed
- HotelBookingCancellationStatus: Requested, SupplierCancellationPending, RefundPending, Completed
- SupplierCancellationAttemptStatus: Created, Initiated, Confirmed, Failed
- PaymentStatus: Pending, Succeeded
- PaymentAttemptStatus: Created, Initiated, Succeeded, Failed
- RefundStatus: Pending, Succeeded
- RefundAttemptStatus: Created, Initiated, Succeeded, Failed
- multi-room: YES
- child AgeAtCheckIn: YES
- BirthDate stored: NO
- passport/document stored: NO

## 7. Flow evidence

- Payment-only confirmation: stays Pending
- Supplier-only confirmation (new PayNow): stays Pending
- dual-evidence confirmation: Payment Succeeded AND SupplierReservation Confirmed
- supplier timeout: unresolved; no automatic Refund
- hold timeout: Requested remains unresolved; expiry is source Instant
- cancellation timeout: attempt Initiated; HotelBooking remains Confirmed; Refund not started
- partial penalty cancellation: `PartialRefundRequiredButUnsupported`; stays Confirmed
- partial cancellation supplier call count: 0
- full Refund compensation: YES (Payment-owned)
- PaymentStatus after Refund: Succeeded

## 8. Public / security evidence

- API: `POST /api/hotel-booking/public/initiations` · `GET /api/hotel-booking/public/{hotelBookingId}` · `POST .../availability` · `POST .../rate-offers` · `GET .../payment` · `POST .../payment/initiation` · `POST .../cancellation`
- Frontend: `/[locale]/places/[slug]/book` · `/[locale]/hotel-bookings/[hotelBookingId]` · `.../payment` · `.../payment/return`
- Header: `X-TravelCore-Hotel-Booking-Access-Token`
- raw token persisted: NO · verifier persisted: YES · token URL leakage: NO · localStorage: NO · sessionStorage: YES
- missing/wrong/cross-user: 404
- public list: NO · generic CRUD: NO · client price/success authority: NO · card collection: NO
- transactional noindex: YES · FA/EN/AR: YES · RTL/LTR/bidi: PASS · mobile/accessibility: PASS
- operational read: `IHotelBookingOperationalQuery` internal-only · operational mutation: NONE

## 9. Validation battery (gate re-run)

| Suite | Result | Detail |
|-------|--------|--------|
| `dotnet build TravelCore.sln` | **PASS** | 0 Error(s) |
| HotelBooking.UnitTests | **PASS** | **103** |
| Payment.UnitTests | **PASS** | **91** |
| Booking.UnitTests | **PASS** | **54** |
| ArchitectureTests | **PASS** | **316** (includes GATE evidence guardrail) |
| Persistence.IntegrationTests | **PASS** | **110** |
| Host.IntegrationTests | **PASS** | **61** |
| Frontend `npm run typecheck` | **PASS** | clean |
| Frontend `npm run lint` | **PASS** | clean |
| Frontend `npm run build` | **PASS** | hotel-booking routes present |
| `git diff --check` | **PASS** | clean |

## 10. Explicit OUT / DEFER

- Production Hotel Supplier = NONE
- Production Hotel Availability Source = NONE
- Production Hotel Rate Source = NONE
- Production Hotel Reservation Source = NONE
- Production Payment Provider = NONE
- Partial Refund = NOT IMPLEMENTED / DEFERRED
- PayAtProperty = NOT IMPLEMENTED / DEFERRED
- Deposit/Partial Payment = NOT IMPLEMENTED / DEFERRED
- Amendments = NOT IMPLEMENTED / DEFERRED
- Rebooking = NOT IMPLEMENTED / DEFERRED
- No-show execution = NOT IMPLEMENTED / DEFERRED
- Smart supplier routing/failover = NOT IMPLEMENTED / DEFERRED
- Accounting / Settlement / Supplier settlement / Agency commission / Wallet / Fraud/risk / Loyalty / AI infrastructure = OUT
- Next phase product — **not executed in this Gate**

This Gate does **not** claim external real-world Hotel booking/payment capability without configured real adapters/providers.

## 11. Architect STOP rules honored

| Rule | Honored |
|------|---------|
| No new HotelBooking product in GATE | YES |
| No inventing beyond P21-R1–R8 | YES |
| No next-phase product code | YES |
| No force-push / history rewrite | YES |

## 12. Ledger

- TC-P21-PLAN = ACCEPTED
- TC-P21-T001 = ACCEPTED
- TC-P21-T002 = ACCEPTED
- TC-P21-T003 = ACCEPTED
- TC-P21-T003-VERIFY = ACCEPTED
- TC-P21-T004 = ACCEPTED
- TC-P21-T005 = ACCEPTED
- TC-P21-T006 = ACCEPTED
- TC-P21-T007 = ACCEPTED
- TC-P21-T008 = ACCEPTED
- TC-P21-T009 = ACCEPTED
- TC-P21-GATE = ACCEPTED
- P21-R1 = RESOLVED
- P21-R2 = RESOLVED
- P21-R3 = RESOLVED
- P21-R4 = RESOLVED
- P21-R5 = RESOLVED
- P21-R6 = RESOLVED
- P21-R7 = RESOLVED
- P21-R8 = RESOLVED

## 13. Gate outcome

**TC-P21-GATE COMPLETE** · **P21 COMPLETE** · T001–T009 ACCEPTED · P21-R1–R8 RESOLVED.

Next phase from SoT: **P22 — Flight (PLANNED)**. This Gate does **not** start P22 product work.
