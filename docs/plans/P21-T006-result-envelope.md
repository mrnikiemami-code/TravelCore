# TC-P21-T006 Result Envelope

```text
BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version: 1
Task-ID: TC-P21-T006
Phase: P21
Status: PASS

Repository:
C:/Users/User/TravelCore
https://github.com/mrnikami-code/TravelCore.git

Branch: main
Baseline: 53e6e14
Implementation-Commit: f2d4946
Starting-HEAD: 53e6e14
Working-Tree: CLEAN

Scope Delivered:
- Payment supports two explicitly closed target kinds: TourBooking and HotelBooking
- typed nullable columns booking_id + hotel_booking_id; CHECK exactly-one-target; filtered uniques
- no arbitrary string TargetType / generic TargetId platform
- one HotelBooking -> one logical Payment (ux_payments_hotel_booking_id)
- HotelBooking payment obligation from HotelBookingMonetarySnapshot (amount + CurrencyCode)
- full TravelCore PayNow baseline; PayAtProperty / deposit / partial collection DEFERRED
- Payment must succeed before new final supplier reservation initiation
- dual-evidence confirmation: Payment Succeeded AND HotelSupplierReservation Confirmed
- Payment-only and supplier-only cannot confirm new PayNow HotelBooking
- Hotel-specific outbox/inbox (PaymentSucceeded / compensation-required / RefundSucceeded)
- Payment success + authoritative inability to complete -> full Refund compensation via P20 Refund
- Refund success may Cancel Pending unconfirmed HotelBooking only; Confirmed cancellation remains R7
- Partial Refund remains DEFERRED; no customer cancellation; no public HotelBooking API/UI
- Production Payment Provider = NONE; Named Hotel Supplier = NONE
- P21-R6 recorded RESOLVED; P21-R7/R8 remain OPEN; T007 not executed

Key Artifacts:
- src/backend/Modules/Payment/**
- src/backend/Modules/HotelBooking/**
- tests/Unit/TravelCore.Modules.HotelBooking.UnitTests/HotelBookingPaymentConfirmationTests.cs
- tests/Unit/TravelCore.Modules.HotelBooking.UnitTests/HotelBookingPayNowOrchestrationTests.cs
- tests/Architecture/TravelCore.ArchitectureTests/HotelBookingPaymentIntegrationGuardrailTests.cs
- tests/Integration/TravelCore.Persistence.IntegrationTests/HotelBookingPaymentPersistenceTests.cs
- tests/Integration/TravelCore.Persistence.IntegrationTests/PaymentHotelTargetPersistenceTests.cs
- docs/plans/P21-implementation-plan.md
- docs/PROJECT-STATE.md
- docs/ROADMAP.md
- docs/plans/P21-T006-task-envelope.md

Exact-Validation:
dotnet build TravelCore.sln: PASS (0 errors)
HotelBooking.UnitTests: 75 passed
Payment.UnitTests: 89 passed
Booking.UnitTests: 54 passed
ArchitectureTests: 303 passed
Persistence.IntegrationTests: 106 passed
Host.IntegrationTests: 57 passed
frontend touched: NO
git diff --check: PASS

Required Result Evidence:
- Payment target representation: nullable typed columns booking_id + hotel_booking_id (not string TargetType)
- Payment target exact supported values: TourBooking=1, HotelBooking=2
- arbitrary string TargetType supported: NO
- existing Tour Booking Payment regression: PASS (Booking.UnitTests 54; Tour initiate/public payment APIs unchanged)
- one HotelBooking -> one Payment constraint: ux_payments_hotel_booking_id (filtered unique WHERE hotel_booking_id IS NOT NULL)
- target exactly-one constraint: ck_payments_exactly_one_target (and ck_refunds_exactly_one_target)
- Hotel payment obligation source: IHotelBookingPaymentObligationQuery / HotelBookingMonetarySnapshot
- Hotel Payment amount source: HotelBookingMonetarySnapshot.TotalAmount copied into PaymentExecutionSnapshot
- Hotel Payment CurrencyCode source: HotelBookingMonetarySnapshot.CurrencyCode
- PayNow baseline: YES
- PayAtProperty: DEFERRED
- deposit/partial collection: NO
- Payment-before-supplier gating: YES (new final reservation requires Payment evidence)
- Payment-only HotelBooking confirmation result: Pending
- Supplier-only HotelBooking confirmation result: Pending (already-Confirmed T005 rows not downgraded)
- dual-evidence confirmation result: Pending -> Confirmed via ConfirmFromAuthoritativePaymentAndSupplierEvidence
- concurrent dual-evidence result: HotelBooking confirms at most once (DB transaction / unique evidence / status guard)
- Hotel Payment success event contract: HotelBookingPaymentSucceededIntegrationEvent
- HotelBooking Payment success inbox: hotel_booking.payment_success_inbox
- authoritative Payment evidence query: IPaymentSuccessEvidenceQuery.GetByHotelBookingIdAsync
- Payment amount mismatch result: no confirm; MonetaryMismatch compensation/reconciliation evidence
- Payment currency mismatch result: no confirm; CurrencyMismatch compensation/reconciliation evidence
- Payment succeeds / Hold expires result: HotelBooking remains Pending; compensation-required (HoldExpired); full Refund obligation
- Payment succeeds / Hold released result: HotelBooking remains Pending; compensation-required (HoldReleased)
- Payment succeeds / supplier definitive failure result: HotelBooking not Confirmed; compensation-required (SupplierReservationNotCreated)
- Payment succeeds / supplier timeout result: no Refund; attempt remains Initiated/unresolved; recheck required
- Payment succeeds / supplier mismatch result: no confirmation; snapshot unchanged; reconciliation issue; no unsafe automatic refund
- compensation event contract: HotelBookingPaymentCompensationRequiredIntegrationEvent
- Hotel compensation outbox: hotel_booking.outbox_messages (MessageType HotelBookingPaymentCompensationRequiredIntegrationEvent)
- Payment compensation consumer/inbox: HotelBookingPaymentCompensationRequiredHandler + payment.compensation_inbox
- compensation Refund amount source: PaymentExecutionSnapshot (event amount is not authority)
- one Refund per Hotel Payment result: P20 one logical full Refund; duplicate compensation is idempotent
- Hotel Refund-success event/contract: HotelBookingRefundSucceededIntegrationEvent
- HotelBooking Refund-success inbox: hotel_booking.refund_success_inbox
- Pending HotelBooking after Refund success result: Pending -> Cancelled (CancelFromAuthoritativePaymentCompensation)
- Confirmed HotelBooking after Refund-success handler result: remains Confirmed; refund invariant issue recorded; R7
- Partial Refund implemented: NO
- customer HotelBooking cancellation: NO
- Confirmed HotelBooking cancellation: NO
- PaymentStatus exact values: Pending, Succeeded
- RefundStatus exact values: Pending, Succeeded
- HotelBookingStatus exact values: Pending=1, Confirmed=2, Cancelled=3
- SupplierReservationStatus exact values: Pending=1, Confirmed=2, Cancelled=3
- SupplierReservationAttemptStatus exact values: Created=1, Initiated=2, Confirmed=3, Failed=4
- Production Payment Provider: NONE
- Production Hotel Reservation Source: NONE
- real provider/supplier SDK: NO
- public HotelBooking API/UI: NO
- peer-schema FK: NO
- shared DbContext: NO
- peer Infrastructure dependency: NO
- distributed transaction: NO
- P21-R6: RESOLVED
- P21-R7/P21-R8: OPEN
- TC-P21-T007: NOT EXECUTED

Persistence tables:
- payment.payments.booking_id (nullable) + hotel_booking_id (nullable)
- payment.refunds.booking_id (nullable) + hotel_booking_id (nullable)
- hotel_booking.hotel_booking_payment_evidence
- hotel_booking.payment_success_inbox
- hotel_booking.refund_success_inbox
- hotel_booking.hotel_booking_payment_compensation_evidence
- hotel_booking.outbox_messages
- hotel_booking.hotel_booking_refund_invariant_issues

Migrations:
- 20260818170426_AddHotelBookingPaymentTarget
- 20260818170438_AddHotelBookingPaymentIntegration

Next-State:
AWAITING_ARCHITECT_REVIEW

END_TRAVELCORE_CURSOR_RESULT_V1
```
