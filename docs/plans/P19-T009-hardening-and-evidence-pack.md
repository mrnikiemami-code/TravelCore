# TC-P19-T009 — Tour Booking hardening and evidence pack

**Task:** TC-P19-T009 — Hardening + evidence  
**Product HEAD at T009 start:** `5b4361e` (`TC-P19-T008` **ACCEPTED**)  
**Date:** 2026-08-18  
**Scope:** Tests, architecture guardrails, security/privacy evidence, documentation, SoT sync — **no new product capability**.  
**Forbidden in this task:** new Booking product capability · new BookingStatus · new passenger fields · passport/document storage · Payment implementation · Confirm implementation · Confirmed cancellation/refund · requote/repricing · agency commission/settlement · agency Booking inbox · public cancellation · CRM · Search implementation · SEO ownership changes · Notification provider · AI booking agent · P20 implementation · new P19 architecture decision beyond R1–R8.  
**Not this task:** `TC-P19-GATE` (**NOT EXECUTED**). P19 remains IN PROGRESS. Do **not** start P20.

## 1. Mission checklist

| # | Verify | Result |
|---|--------|--------|
| 1 | Independent Booking module/schema `booking`; initial target TourDeparture (P19-R1) | **PASS** — T001 |
| 2 | BookingStatus exactly Pending / Confirmed / Cancelled (P19-R2) | **PASS** — T002 |
| 3 | CapacityHold + advisory-lock concurrency (P19-R3) | **PASS** — T003 |
| 4 | Booker / passengers / contact snapshots; minimized PII (P19-R4) | **PASS** — T004 |
| 5 | Authoritative Quote → immutable BookingMonetarySnapshot (P19-R5) | **PASS** — T005 |
| 6 | Pending cancel + Active-hold release; Confirm/Payment DEFERRED (P19-R6) | **PASS** — T006 |
| 7 | One Booking aggregate for Direct and Agency (P19-R7) | **PASS** — T007 |
| 8 | Public Pending initiation + hashed token + noindex (P19-R8) | **PASS** — T008 |
| 9 | No new product capability in this task | **PASS** — evidence/docs + strengthened guardrails / a11y-bidi only |
| 10 | `TC-P19-GATE` remains NOT EXECUTED | **PASS** — this pack does not close GATE |

## 2. Accepted product commits (P19)

| Task | Commit | Essence |
|------|--------|---------|
| PLAN | `9d4266b` | Authoritative P19 plan |
| T001 | `e198daa` | Booking module scaffolding (`booking` schema) — P19-R1 |
| T002 | `7caa90a` | Booking aggregate + Pending/Confirmed/Cancelled — P19-R2 |
| T003 | `8c79b02` | CapacityHold + DepartureCapacityAccount + concurrency — P19-R3 |
| T004 | `b71fd15` | Booker / passengers / contact snapshot — P19-R4 |
| T005 | `66ec4e9` | Quote consumption into BookingMonetarySnapshot — P19-R5 |
| T006 | `9dca5ef` | Pending cancellation + atomic hold release — P19-R6 |
| T007 | `2e7937a` | Direct/Agency source on one aggregate — P19-R7 |
| T008 | `5b4361e` | Public Pending initiation / authorization / privacy — P19-R8 |

Architect acceptance of PLAN and T001–T008 is as issued. T009 prepares gate evidence; it does **not** execute `TC-P19-GATE`.

**Ledger:**

- TC-P19-PLAN ACCEPTED
- TC-P19-T001 ACCEPTED
- TC-P19-T002 ACCEPTED
- TC-P19-T003 ACCEPTED
- TC-P19-T004 ACCEPTED
- TC-P19-T005 ACCEPTED
- TC-P19-T006 ACCEPTED
- TC-P19-T007 ACCEPTED
- TC-P19-T008 ACCEPTED
- TC-P19-T009 awaiting architect review
- TC-P19-GATE NOT EXECUTED

## 3. Locked decisions (all RESOLVED)

| ID | Essence |
|----|---------|
| **P19-R1** | Independent Booking module. Schema `booking`. Initial logical target = TourDeparture. **Booking != Tour**. **Booking != TourDeparture**. **Booking != Pricing**. **Booking != Payment**. **Booking != TripPlanner**. **Booking != VisaApplication**. **Booking != AgencyMarketplace**. **Booking != Search**. **Booking != SEO**. **Booking != Notification Provider**. Tour owns capacity **definition**. Booking owns capacity **consumption**. No peer-schema FK. No shared DbContext. |
| **P19-R2** | Statuses exactly Pending · Confirmed · Cancelled. **BookingStatus != PaymentStatus**. New Booking = Pending. Pending does not imply CapacityHeld / PaymentPending / QuoteValid. **Confirmed != PaymentSucceeded**. **Cancelled != Refunded**. No Expired / AwaitingPayment / Paid / Refunded / Reserved / Held / PriceLocked in BookingStatus. |
| **P19-R3** | Hold lifecycle Active · Consumed · Released · Expired. **CapacityDefinition != CapacityConsumption**. **CapacityHoldStatus != BookingStatus**. **Pending != CapacityHeld**. **Consumed != BookingConfirmed**. **Expired Hold != Expired Booking**. **Expired Hold != BookingExpired**. Authoritative overbooking protection is server-side + database-backed + transactional (`pg_advisory_xact_lock`). Process-local lock is not the correctness mechanism. |
| **P19-R4** | **PlannerTravelerComposition != BookingPassenger**. **BookingPassenger != Party Person Master**. **BookingContactSnapshot != Party**. **BookingContactSnapshot != Identity Account**. **BookingPassenger != CapacityHold**. Baseline passenger facts: GivenName / FamilyName / TravelerCategory. No passport / national ID / document scan / visa document / biometric / health / payment card. **Booking PII != Search/SEO data**. PII retention remains future explicit operational/legal policy. |
| **P19-R5** | **Price != Quote**. **Quote != BookingMonetarySnapshot**. **BookingMonetarySnapshot != PaymentAmount**. **Booking != Pricing Authority**. Booking consumes Quote via Pricing contracts only. Client cannot authoritatively set TotalAmount / CurrencyCode / Tax / Fee / QuoteExpiresAt. Snapshot is immutable after accept. No float/double monetary persistence. No Toman CurrencyCode. No FX. |
| **P19-R6** | **Booking != Payment**. **BookingStatus != PaymentStatus**. **PaymentSucceeded != BookingConfirmed**. **BookingCancelled != PaymentRefunded**. **BookingMonetarySnapshot != PaymentTransaction**. Payment execution DEFERRED. Executable payment-driven confirmation DEFERRED. No public Confirm endpoint. No unrestricted Confirm(). No caller-controlled paymentSucceeded boolean. Pending → Cancelled IN; Confirmed → Cancelled DEFERRED. |
| **P19-R7** | One Booking aggregate for Direct and Agency. SourceKind Direct / Agency. **AgencyOffer != Booking**. **AgencyOffer != Quote**. Direct cannot carry AgencyProfileReference / AgencyOfferReference. Agency requires AgencyProfileReference. No AgencyPrice / markup / commission / settlement / agency quota / AwaitingAgency status. **Lead != Booking**. **VisaApplication != Booking**. |
| **P19-R8** | **PublicExperience != Booking Source of Truth**. **Public Booking initiation != Booking confirmation**. **Pending != Confirmed**. **BookingId != Access Credential**. Public initiation creates Pending Booking. Canonical public route `/[locale]/tours/[slug]/book`. Private read `/[locale]/bookings/[bookingId]` is noindex. Header `X-TravelCore-Booking-Access-Token`. Raw token returned once; SHA-256 verifier persisted. Public clients cannot forge Agency source. |

## 4. Boundary / ownership matrix

| Concern | Owner | P19 posture |
|---------|-------|-------------|
| Booking aggregate / status / passengers / contact / hold / monetary snapshot | **Booking** | Transactional SoT in schema `booking` |
| TourProduct / TourDeparture / configured MaxPax | **Tour** | **CapacityDefinition != CapacityConsumption**; logical TourDeparture reference only |
| Price / Quote issuance | **Pricing** | Booking copies snapshot; **Booking != Pricing Authority** |
| Payment / capture / refund | **Payment (P20)** | **DEFERRED**; **Booking != Payment** |
| AgencyProfile / AgencyOffer | **AgencyMarketplace** | Logical refs only; no commission/settlement |
| Public composition / CTA | **PublicExperience** | **PublicExperience != Booking Source of Truth** |
| Retrieval / ranking | **Search** | Booking PII is not Search-indexed |
| IndexPolicy | **SEO** | Transaction pages always noindex |
| TripIntent / Lead | **TripPlanner** | **Lead != Booking**; no automatic conversion |
| VisaApplication | **Visa (future application)** | **VisaApplication != Booking** |
| Notification delivery | **Notification (future)** | Provider DEFERRED; initiation not gated on delivery |
| Identity / Party master | **Identity / Party** | Opaque actor/party refs + snapshots only |

## 5. Invariant evidence (T001–T008)

### 5.1 R1 ownership
- Independent module + schema `booking`.
- No shared DbContext. No peer-schema FK.
- Booking.Infrastructure may consume Pricing.Contracts / Tour.Contracts / AgencyMarketplace.Contracts only — never peer Infrastructure or Domain.

### 5.2 R2 lifecycle
- Closed set Pending / Confirmed / Cancelled.
- Create → Pending. Pending → Cancelled implemented.
- No `Confirm()` / `SetStatus()`. No extra payment/capacity statuses.

### 5.3 R3 capacity
- `CapacityHold` Active/Consumed/Released/Expired.
- `DepartureCapacityAccount` effective consumption = Active + Consumed.
- Released / Expired free capacity once. Consumed remains consumed.
- Concurrency: `pg_advisory_xact_lock` in `BookingCapacityService` / `BookingCancellationService` / public initiation orchestration.
- Evidence: capacity=1 two concurrent requests of 1 → exactly one success; capacity=5 existing 3 + two concurrent requests of 2 → final consumption ≤ 5 (`BookingCapacityPersistenceTests`).
- Retrying the same hold idempotency key does not consume twice.

### 5.4 R4 people / PII
- `BookingPassenger` and `BookingContactSnapshot` are transaction-time facts.
- Actor/Party references are logical/opaque; no Identity/Party persistence dependency.
- PassengerCount ≤ Active Hold SeatCount; under-fill allowed; SeatCount is not silently reduced.
- No passport/national ID/document columns in schema `booking`.

### 5.5 R5 Quote / money
- Public initiation issues Quote via Pricing `IAuthoritativeQuoteIssuer`.
- `AcceptQuote` copies immutable `BookingMonetarySnapshot`.
- Expired Quote rejected. TourDeparture mismatch rejected. Same Quote acceptance idempotent. Different Quote cannot overwrite accepted snapshot.
- Components are copied Quote facts; Booking does not recalculate tax/fee/discount.

### 5.6 R6 Payment / confirm / cancel
- No PaymentIntent / PaymentStatus / provider / capture / refund / fake Payment service.
- No public Confirm endpoint. Host `POST /api/booking/public/{id}/confirm` → 404.
- Pending cancel atomically releases Active hold. Consumed is not silently Released.
- Cancellation does not erase monetary snapshot / passengers / contact.
- Future confirmation must re-evaluate: still Pending, applicable capacity, accepted snapshot, passenger/contact invariants, authoritative Payment satisfaction when required.

### 5.7 R7 Direct / Agency
- Controlled `BookingSourceKind` Direct / Agency.
- Public path is Direct only; client `sourceKind=Agency` is rejected.
- No AgencyBooking / DirectBooking aggregate types.

### 5.8 R8 public surface
- Backend: `POST /api/booking/public/initiations` and `GET /api/booking/public/{bookingId}`.
- Frontend: `/[locale]/tours/[slug]/book` (Server Component + form island) and `/[locale]/bookings/[bookingId]` (noindex; token never in URL).
- Existing routes remain valid: `/[locale]/tours/[slug]`, `/[locale]/tours/[slug]/[intent]`, `/[locale]/plan`. No `/[locale]/tours/[productKey]`.
- Honest FA/EN/AR copy. No Book Now / Booking confirmed / Payment completed.
- No public listing (`GET /api/booking/public` → 404). No public cancellation.
- Anonymous: no credential → 404; wrong token → 404; correct token → 200.
- Authenticated object-level: cross-user → 404.
- Duplicate submit: same BookingId; no extra capacity; token not reissued.
- Insufficient capacity → 409. Expired Quote fails without fabricating Confirmed.

### 5.9 Token security
- Cryptographically unpredictable raw token returned once.
- SHA-256 verifier persisted in `booking_access_credentials`.
- Raw token is not persisted. Token is not global identity. Token is not in canonical/SEO URL. Hash is not exposed publicly.

## 6. Guardrail / test surfaces

| Area | Evidence |
|------|----------|
| Unit | `TravelCore.Modules.Booking.UnitTests` — aggregate, hold, people, money, cancel, source, public surface |
| Architecture | `BookingBoundaryGuardrailTests` + `BookingPhaseBoundaryGuardrailTests` — R1–R8, engines, T009 evidence pack, routes, peer refs |
| Persistence | schema `booking`; 7 migrations; allowlisted tables only; concurrency tests; no peer FK |
| Host | `BookingPublicHostTests` — Pending initiation, token 404/200, idempotency, 409 overbook, no Confirm, no listing |
| Frontend | `/[locale]/tours/[slug]/book` Server Component + prepare-form island; `/[locale]/bookings/[bookingId]`; FA/EN/AR; LtrValue / MoneyText; labels; `min-h-touch`; robots `index:false` |

## 7. Validation commands (this task)

```text
dotnet build TravelCore.sln
dotnet test tests/Unit/TravelCore.Modules.Booking.UnitTests
dotnet test tests/Architecture/TravelCore.ArchitectureTests
dotnet test tests/Integration/TravelCore.Persistence.IntegrationTests
dotnet test tests/Integration/TravelCore.Host.IntegrationTests
npm run typecheck   (src/frontend/web)
npm run lint
npm run build
git diff --check
```

## 8. Required result evidence (runtime values recorded in RESULT)

- Public Booking route: `/[locale]/tours/[slug]/book`
- Private Booking route: `/[locale]/bookings/[bookingId]`
- BookingStatus after public initiation: **Pending**
- Anonymous token/verifier: raw token once + SHA-256 verifier; header `X-TravelCore-Booking-Access-Token`
- No-credential read: **404**
- Wrong-token read: **404**
- Correct-token read: **200**
- Cross-user authorization: **404**
- Duplicate-submit idempotency: same BookingId; token not reissued; no extra capacity
- Capacity concurrency: capacity=1 two concurrent 1-seat holds → exactly one success; capacity=5 with existing 3 + two concurrent 2-seat holds → final ≤ 5
- Expired Quote: rejected; no Confirmed fabrication
- Insufficient capacity: **409**
- No Confirm endpoint
- No Payment implementation
- No public arbitrary Booking list
- No peer-schema FK
- No shared DbContext
- Payment/Confirm deferral still recorded

## 9. Carry-forward invariants into GATE

Booking != Tour · Booking != TourDeparture · Booking != Pricing · Booking != Payment · Booking != TripPlanner · Booking != VisaApplication · Booking != AgencyMarketplace · Booking != Search · Booking != SEO · Booking != Notification Provider · CapacityDefinition != CapacityConsumption · CapacityHoldStatus != BookingStatus · Pending != CapacityHeld · Consumed != BookingConfirmed · Expired Hold != BookingExpired · PlannerTravelerComposition != BookingPassenger · BookingPassenger != Party Person Master · BookingContactSnapshot != Party · BookingContactSnapshot != Identity Account · Price != Quote · Quote != BookingMonetarySnapshot · BookingMonetarySnapshot != PaymentAmount · PaymentSucceeded != BookingConfirmed · BookingCancelled != PaymentRefunded · AgencyOffer != Booking · AgencyOffer != Quote · PublicExperience != Booking Source of Truth · Public Booking initiation != Booking confirmation · BookingId != Access Credential · BookingStatus != PaymentStatus · Booking PII != Search/SEO data · Payment execution remains DEFERRED · executable payment-driven Booking confirmation remains DEFERRED · Confirmed cancellation/refund remains DEFERRED · public Booking initiation ends in Pending.

T009 does **not** close `TC-P19-GATE`.
P19 remains IN PROGRESS.
T009 does **not** start P20.
No new Booking product capability.
