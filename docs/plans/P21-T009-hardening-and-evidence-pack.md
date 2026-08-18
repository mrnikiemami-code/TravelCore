# TC-P21-T009 — Hotel Booking hardening and evidence pack

**Task:** TC-P21-T009 — Hardening + evidence  
**Product HEAD at T009 start:** `d8bdf0f` (`TC-P21-T008` **ACCEPTED**)  
**Date:** 2026-08-18  
**Scope:** Tests, architecture guardrails, security/privacy evidence, documentation, SoT sync — **no new product capability**.  
**Forbidden in this task:** new architecture decision · Partial Refund · PayAtProperty · deposit/partial payment · amendments · rebooking · no-show workflow · real supplier · supplier SDK · real Payment provider · Payment provider SDK · smart supplier routing/failover · accounting · settlement · agency commission · wallet · fraud · loyalty · generic Booking platform · generic arbitrary Payment target · public CRUD · operational mutation · shared DbContext · peer-schema FK · peer Infrastructure dependency · distributed transaction · unrelated refactor · dependency upgrade · `TC-P21-GATE` execution.  
**Not this task:** `TC-P21-GATE` (**NOT EXECUTED**). **P21 remains IN PROGRESS**. Do **not** close the phase.

## 1. Scope and baseline

| # | Verify | Result |
|---|--------|--------|
| 1 | Independent HotelBooking module/schema `hotel_booking`; Place remains catalog (P21-R1) | **PASS** — T001 |
| 2 | Stay / rooms / guests / occupancy / multi-room (P21-R2) | **PASS** — T002 |
| 3 | Availability hold via `IHotelAvailabilitySource` (P21-R3) | **PASS** — T003 |
| 4 | Rate / monetary / cancellation-policy snapshots (P21-R4) | **PASS** — T004 |
| 5 | Supplier reservation + HotelBookingStatus Pending/Confirmed/Cancelled (P21-R5) | **PASS** — T005 |
| 6 | Typed HotelBooking Payment target + dual-evidence confirmation (P21-R6) | **PASS** — T006 |
| 7 | Confirmed cancellation process + penalty economics (P21-R7) | **PASS** — T007 |
| 8 | Public transactional journey + independent token + operational reads (P21-R8) | **PASS** — T008 |
| 9 | No new product capability in this task | **PASS** — evidence/docs + hardening guardrails only |
| 10 | `TC-P21-GATE` remains NOT EXECUTED | **PASS** — this pack does not close GATE |

## 2. P21 decision inventory

| ID | Essence |
|----|---------|
| **P21-R1** | Independent HotelBooking module. Schema `hotel_booking`. Place is hotel catalog owner. Logical `HotelPlaceReference(PlaceId)`. **HotelBooking != Place**. **Hotel Catalog != Hotel Booking**. **HotelBooking != Tour Booking**. No shared DbContext. No peer-schema FK. Named Hotel Supplier = NONE. |
| **P21-R2** | NodaTime LocalDate CheckIn/CheckOut. 1..N RoomReservations (no Quantity=N). Adult/Child. Child AgeAtCheckIn. BirthDate stored = NO. Exactly one LeadGuest. Contact snapshot separate from lead identity. |
| **P21-R3** | `IHotelAvailabilitySource` is availability authority. Place/Search are not live availability. Production Hotel Availability Source = NONE. Hold statuses Requested/Active/Released/Expired. No Hold Failed. |
| **P21-R4** | `IHotelRateOfferSource`. Immutable `HotelRateOfferSnapshot` / `HotelBookingMonetarySnapshot` / `HotelCancellationPolicySnapshot`. Production Hotel Rate Source = NONE. Pricing module not generalized. Partial Refund remains DEFERRED. |
| **P21-R5** | HotelBookingStatus Pending/Confirmed/Cancelled. Distinct `HotelSupplierReservation`. Production Hotel Reservation Source = NONE. Named Hotel Supplier = NONE. Timeout ≠ Failed. |
| **P21-R6** | Payment supports closed target kinds TourBooking and HotelBooking only. Pay-first. Dual evidence: **Payment Succeeded != HotelBooking Confirmed**. Full-refund compensation. Confirmed cancel is R7. |
| **P21-R7** | Separate `HotelBookingCancellation`. Penalty=0 full Refund after authoritative supplier cancel. Penalty=Total no Refund. Partial penalty blocked before supplier side effect. **HotelBookingCancelled != RefundSucceeded**. amendments/rebooking remain DEFERRED. |
| **P21-R8** | Public HotelBooking is a transactional journey, not CRUD. Independent `X-TravelCore-Hotel-Booking-Access-Token`. SHA-256 verifier only. sessionStorage. 404 non-enumeration. Zero sources truthful. Operational reads internal-only. |

**Ledger:**

- TC-P21-PLAN ACCEPTED
- TC-P21-T001 ACCEPTED
- TC-P21-T002 ACCEPTED
- TC-P21-T003 ACCEPTED
- TC-P21-T004 ACCEPTED
- TC-P21-T005 ACCEPTED
- TC-P21-T006 ACCEPTED
- TC-P21-T007 ACCEPTED
- TC-P21-T008 ACCEPTED
- TC-P21-T009 awaiting architect review
- TC-P21-GATE NOT EXECUTED

## 3. Module/schema ownership proof

| Concern | Owner | P21 posture |
|---------|-------|-------------|
| Hotel catalog (name, amenities, address, PlaceId) | **Place** | Hotel Catalog != Hotel Booking |
| Hotel reservation transaction | **HotelBooking** | schema `hotel_booking` |
| Payment / Refund execution | **Payment** | schema `payment`; closed target kinds |
| Tour Booking | **Booking** | unchanged P19 aggregate |
| Live availability | **IHotelAvailabilitySource** | Place/Search are not live availability authority |
| IndexPolicy | **SEO** | transactional HotelBooking pages always noindex |

HotelBooking.Infrastructure references Payment.Contracts and Place.Contracts only — never peer Infrastructure/Domain. Payment.Infrastructure references HotelBooking.Contracts — never HotelBooking.Infrastructure/Domain.

## 4. Domain invariant proof

- HotelBookingStatus exactly Pending / Confirmed / Cancelled.
- HoldStatus exactly Requested / Active / Released / Expired.
- SupplierReservationStatus exactly Pending / Confirmed / Cancelled.
- SupplierReservationAttemptStatus and SupplierCancellationAttemptStatus exactly Created / Initiated / Confirmed / Failed.
- CancellationStatus exactly Requested / SupplierCancellationPending / RefundPending / Completed.
- PaymentStatus Pending / Succeeded. PaymentAttemptStatus Created / Initiated / Succeeded / Failed. RefundStatus Pending / Succeeded. RefundAttemptStatus Created / Initiated / Succeeded / Failed.
- Multi-room supported. Child AgeAtCheckIn required. BirthDate stored = NO. Passport/document stored = NO.
- No generic Confirm / SetStatus / ForceConfirm / ForceCancel / MarkPaid / MarkRefunded.

## 5. Availability/hold proof

- Authority: `IHotelAvailabilitySource`. Production Hotel Availability Source = NONE.
- One hold covers the complete room set. Partial hold cannot become Active.
- Timeout/Unknown remains Requested. Unresolved Requested/Active blocks unsafe duplicate acquisition.
- Expiry is source-authoritative Instant, not a hardcoded TTL.
- Process-local lock is not correctness authority.

## 6. Rate/monetary/cancellation-policy proof

- Authority: `IHotelRateOfferSource`. Production Hotel Rate Source = NONE.
- Accepted snapshots are immutable. Silent repricing does not mutate accepted truth.
- One transaction CurrencyCode. No implicit FX. Toman is not a CurrencyCode.
- PenaltyAmount is 0..Total. Partial penalty is representable; Partial Refund execution is NOT IMPLEMENTED.

## 7. Supplier reservation proof

- Authority: `IHotelReservationSource`. Named Hotel Supplier = NONE. Production Hotel Reservation Source = NONE. Supplier SDK = NO.
- Authoritative query-confirmed evidence required. Unverified callback / browser flags cannot confirm.
- Cross-booking / cross-attempt evidence cannot mutate the wrong aggregate.
- Mismatches become reconciliation issues, not silent mutation.

## 8. Payment/Refund integration proof

- Payment supported target kinds: TourBooking, HotelBooking. Arbitrary TargetType = absent.
- Obligation from `HotelBookingMonetarySnapshot` copied into immutable `PaymentExecutionSnapshot`.
- Pay-first: no new final supplier reservation without Payment success.
- Dual-evidence confirmation: Payment Succeeded AND SupplierReservation Confirmed.
- Payment-only stays Pending. Supplier-only (new PayNow) stays Pending.
- Compensation event has no amount authority. Refund amount = PaymentExecutionSnapshot.
- PaymentStatus stays Succeeded after Refund. Full-refund compensation is durable/idempotent.
- Production Payment Provider = NONE. Payment provider SDK = NO.

## 9. Confirmed cancellation proof

- Separate `HotelBookingCancellation` process.
- Penalty=0 → full Refund after authoritative supplier cancel.
- Penalty=Total → completes without Refund.
- Partial penalty: `PartialRefundRequiredButUnsupported`; supplier cancellation call count = 0; HotelBooking stays Confirmed.
- Cancellation timeout: attempt stays Initiated; HotelBooking remains Confirmed; Refund not started.
- HotelBookingCancelled != RefundSucceeded.

## 10. Public authorization/privacy proof

- Header: `X-TravelCore-Hotel-Booking-Access-Token` (independent of Tour Booking).
- Raw token returned once. Raw token persisted = NO. Verifier persisted = YES (SHA-256).
- Token URL exposure = NO. token localStorage = NO. sessionStorage = YES.
- Missing/wrong/cross-user/HotelBookingId-only/PaymentId-only = 404.
- Tour token ⇏ HotelBooking and vice versa.
- Client amount/currency/success are not authority.
- Public HotelBooking list = NO. Generic public CRUD = NO. Public Refund command = NO. Card collection = NO.

## 11. Frontend/noindex/a11y/bidi proof

- Entry: `/[locale]/places/[slug]/book`
- Private: `/[locale]/hotel-bookings/[hotelBookingId]`
- Payment + return: `.../payment` and `.../payment/return`
- Transactional routes noindex = YES.
- FA/EN/AR copy. RTL/LTR via LtrValue / MoneyText / BidiText.
- Mobile `min-h-11` / `min-h-touch`. focus-visible. FieldMessage error semantics.

## 12. Concurrency/idempotency proof

- Initiation idempotency converges to the same HotelBooking.
- Hold / rate / Payment / reservation / cancellation uniqueness is database-backed (unique indexes / constraints).
- Duplicate PaymentSucceeded / compensation / RefundSucceeded / CancellationRefundRequired deliveries have one effective local result.
- Dual-evidence confirmation is race-safe (HotelBooking confirms at most once).

## 13. Outbox/inbox/crash-recovery proof

| Flow | Mechanism |
|------|-----------|
| Payment success | `payment.outbox_messages` → HotelBooking `payment_success_inbox` |
| Hotel compensation | HotelBooking outbox → Payment `compensation_inbox` |
| Refund success | Payment outbox → HotelBooking `refund_success_inbox` |
| Cancellation Refund required | HotelBooking outbox → Payment `hotel_booking_cancellation_refund_inbox` |

Delivery semantics: at-least-once + local idempotent/effectively-once effects. Distributed exactly-once is not claimed. Restart correctness does not depend on in-memory dictionaries/locks.

## 14. Cross-target/cross-booking isolation proof

- Same UUID in TourBooking and HotelBooking namespaces cannot collide as one Payment target (exactly-one-target + typed kinds).
- HotelBooking target support cannot alter Tour Booking Payment records.
- Hotel cancellation/compensation events cannot affect Tour Payment.
- Evidence for HotelBooking A cannot mutate HotelBooking B.

## 15. Zero-source/provider posture

- Production Hotel Availability Source = NONE
- Production Hotel Rate Source = NONE
- Production Hotel Reservation Source = NONE
- Named Hotel Supplier = NONE
- Production Payment Provider = NONE
- Zero availability/rate/payment public initiation = truthful 503. No fake production source/provider.

## 16. Deferred/out-of-scope inventory

- Partial Refund remains DEFERRED
- PayAtProperty remains DEFERRED
- Deposit/Partial Payment = DEFERRED
- amendments/rebooking remain DEFERRED
- No-show execution = DEFERRED
- Smart supplier routing/failover = DEFERRED
- Real Hotel Supplier = NONE
- Real Payment Provider = NONE
- Accounting / Settlement / Agency commission / Wallet / Fraud / Loyalty / LLM / RAG / Embeddings / Vector DB = not implemented

## 17. Exact test/build results

| Surface | Result |
|---------|--------|
| `dotnet build TravelCore.sln` | PASS (0 errors) |
| HotelBooking.UnitTests | 103 passed |
| Payment.UnitTests | 91 passed |
| Booking.UnitTests | 54 passed |
| ArchitectureTests | 315 passed |
| Persistence.IntegrationTests | 110 passed |
| Host.IntegrationTests | 61 passed |
| frontend typecheck | PASS |
| frontend lint | PASS |
| frontend production build | PASS |
| `git diff --check` | PASS |

Architecture hardening added `HotelBookingHardeningGuardrailTests` (+5). Existing HotelBooking unit/host/persistence suites remain the primary behavioral proof. Product-code defect found: **NO**.

## 18. Remaining known limitations

- no production Hotel supplier
- no production Hotel availability source
- no production Hotel rate source
- no production Hotel reservation source
- no production Payment provider
- Partial Refund unavailable
- partial-penalty confirmed cancellation blocked
- PayAtProperty unavailable
- deposit/partial collection unavailable
- amendments/rebooking unavailable
- no smart supplier routing/failover

READY FOR GATE means architecture and implemented P21 scope are internally correct and tested. It does **not** mean P21 can perform a real-world Hotel reservation/payment in production without configured real adapters/providers.

## 19. Gate readiness conclusion

All accepted P21-R1 through P21-R8 invariants are verified together. Product-code defect found: **NO**. Correction commit: none.

**P21 READY FOR GATE: YES**

This task still does **not** execute GATE. **P21 remains IN PROGRESS**. **TC-P21-GATE NOT EXECUTED**.
