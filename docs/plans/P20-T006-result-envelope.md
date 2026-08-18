# TC-P20-T006 Result Envelope

```text
BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version: 1
Task-ID: TC-P20-T006
Phase: P20
Status: PASS

Repository:
C:/Users/User/TravelCore
https://github.com/mrnikami-code/TravelCore.git

Branch: main
Baseline: 930a3be
Implementation-Commit: 33f08d1
SoT-Sync-Commit: 33f08d1
Starting-HEAD: 930a3be
Working-Tree: CLEAN

Scope Delivered:
- Payment-owned full Refund aggregate + RefundAttempt (Created/Initiated/Succeeded/Failed)
- Booking recovery writes compensation-required outbox atomically
- Payment GetOrCreate Refund from PaymentExecutionSnapshot (event amount not trusted)
- Provider-neutral refund initiate/verify/query; no named production provider; no SDK
- Authoritative refund success + RefundSucceeded outbox in the same Payment transaction
- Booking RefundSucceeded consumer: Pending cancel via BookingCancellationService; Active hold Released; Expired/Released preserved; Confirmed records invariant issue and is not cancelled
- Confirmation expires a time-elapsed Active hold when recording ExpiredHold so later cancel does not Release it
- No public refund API; no PaymentStatus.Refunded; no partial refund

Key Artifacts:
- src/backend/Modules/Payment/TravelCore.Modules.Payment.Domain/Refund.cs
- src/backend/Modules/Payment/TravelCore.Modules.Payment.Domain/RefundAttempt.cs
- src/backend/Modules/Payment/TravelCore.Modules.Payment.Domain/PaymentRefundBoundary.cs
- src/backend/Modules/Payment/TravelCore.Modules.Payment.Contracts/BookingPaymentCompensationRequiredIntegrationEvent.cs
- src/backend/Modules/Payment/TravelCore.Modules.Payment.Contracts/RefundSucceededIntegrationEvent.cs
- src/backend/Modules/Payment/TravelCore.Modules.Payment.Infrastructure/Services/RefundGetOrCreateService.cs
- src/backend/Modules/Payment/TravelCore.Modules.Payment.Infrastructure/Services/RefundInitiationService.cs
- src/backend/Modules/Payment/TravelCore.Modules.Payment.Infrastructure/Services/VerifiedRefundOutcomeApplier.cs
- src/backend/Modules/Payment/TravelCore.Modules.Payment.Infrastructure/Services/BookingPaymentCompensationRequiredHandler.cs
- src/backend/Modules/Payment/TravelCore.Modules.Payment.Infrastructure/Migrations/20260818140000_AddPaymentRefundAndCompensation.cs
- src/backend/Modules/Booking/TravelCore.Modules.Booking.Infrastructure/Services/BookingPaymentConfirmationService.cs
- src/backend/Modules/Booking/TravelCore.Modules.Booking.Infrastructure/Services/BookingRefundSucceededIntegrationHandler.cs
- src/backend/Modules/Booking/TravelCore.Modules.Booking.Infrastructure/Migrations/20260818100819_AddBookingCompensationOutboxAndRefundInbox.cs

Exact-Validation:
dotnet build TravelCore.sln: PASS (0 errors)
Payment.UnitTests: 70 passed
Booking.UnitTests: 54 passed
ArchitectureTests: 280 passed
Persistence.IntegrationTests: 81 passed
Host.IntegrationTests: 53 passed
frontend touched: NO
frontend typecheck: N/A
frontend lint: N/A
frontend build: N/A
git diff --check: PASS

Refund-Evidence:
RefundStatus exact: Pending / Succeeded
RefundAttemptStatus exact: Created / Initiated / Succeeded / Failed
PaymentStatus exact: Pending / Succeeded (no PaymentStatus.Refunded)
PaymentStatus after Refund success: Succeeded
full Refund amount source: PaymentExecutionSnapshot only
Refund currency source: PaymentExecutionSnapshot only
duplicate Refund creation: same RefundId (GetOrCreate; concurrent compensation converges to one refund)
failed RefundAttempt retry: new RefundAttempt after definitive Failed
unresolved RefundAttempt retry: blocked (InvalidOperationException); NetworkTimeout/ambiguous initiation does not mark Failed
provider refund amount mismatch: VerificationApplyStatus.AmountMismatch; Refund stays Pending; attempt not Succeeded
provider refund currency mismatch: VerificationApplyStatus.CurrencyMismatch; Refund stays Pending; attempt not Succeeded
compensation-required event: BookingPaymentCompensationRequiredIntegrationEvent(BookingId, PaymentId, RecoveryReason, OccurredAt) — no amount
Booking compensation outbox: booking.outbox_messages; BookingCompensationOutboxWriter in same Booking SaveChanges as recovery issue
Payment compensation consumer/inbox: BookingPaymentCompensationRequiredHandler + payment.compensation_inbox (PK payment_id)
RefundSucceeded event: RefundSucceededIntegrationEvent(RefundId, PaymentId, BookingId, OccurredAt, Amount, CurrencyCode) — no PII; EventMeansBookingCancelled = false
Payment Refund-success outbox: payment.outbox_messages keyed by RefundId; same Payment SaveChanges as Refund Succeeded
Booking Refund-success consumer/inbox: BookingRefundSucceededIntegrationHandler + booking.refund_success_inbox (PK refund_id)
expired-hold E2E: Pending -> Cancelled; hold remains Expired; Refund Succeeded; Payment remains Succeeded
cancelled-Booking compensation: Refund Succeeded; Booking stays Cancelled; duplicate dispatch one Refund
duplicate delivery: one Refund; cancel idempotent
process-crash durability: compensation outbox unprocessed => no Refund yet; refund-success outbox unconsumed => Booking stays Pending until consumed
time-elapsed Active hold at confirmation: hold.Expire + account.ReleaseActive in same recovery transaction
Confirmed Booking cancellation: NO (BookingRefundInvariantIssueKind.ConfirmedBooking)
Consumed hold reversal: NO
Partial Refund: NO
public Refund API: NO (host /api/payment/refund and /api/payment/{id}/refund => 404)
real provider: NO (named provider NONE; test fake only)
distributed transaction: NO
peer-schema FK: NO
shared DbContext: NO
Payment does not write Booking tables: YES
Delivery semantics: at-least-once + local idempotent effects (not distributed exactly-once)
P20-R6: RESOLVED (architect lock; T006 implements, awaiting acceptance)
P20-R7/P20-R8: OPEN
T007 executed: NO

Cumulative Execution Ledger (P20):
- TC-P20-T001 => COMPLETE / ACCEPTED (1ec8963)
- TC-P20-T002 => COMPLETE / ACCEPTED (75a4f84)
- TC-P20-T003 => COMPLETE / ACCEPTED (32e555d)
- TC-P20-T004 => COMPLETE / ACCEPTED (f286d9f)
- TC-P20-T005 => COMPLETE / ACCEPTED (VERIFY ecc61c4 · DURABILITY-FIX c7c846b · docs 930a3be)
- TC-P20-T006 => PASS (implemented) / AWAITING_ARCHITECT_REVIEW (33f08d1)
- Next => Architect review/acceptance of TC-P20-T006; do not execute TC-P20-T007; do not invent P20-R7/P20-R8

Next-State: AWAITING_ARCHITECT_REVIEW
Stop-After-Result: YES
T007-Executed: NO

END_TRAVELCORE_CURSOR_RESULT_V1
```
