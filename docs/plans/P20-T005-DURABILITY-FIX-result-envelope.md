# TC-P20-T005-DURABILITY-FIX Result Envelope

```text
BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version: 1
Task-ID: TC-P20-T005-DURABILITY-FIX
Parent-Task: TC-P20-T005
Phase: P20
Status: PASS

Repository:
C:/Users/User/TravelCore
https://github.com/mrnikami-code/TravelCore.git

Branch: main
Starting-HEAD: ecc61c4
Final-HEAD: c7c846b
origin/main: c7c846b
HEAD == origin/main: YES
Working-Tree: CLEAN

Commit-Lineage:
ecc61c4  fix(p20): verify T005 evidence, recovery issues, amount integrity [TC-P20-T005-VERIFY]  (baseline)
c7c846b  fix(p20): Payment success transactional outbox + Booking inbox [TC-P20-T005-DURABILITY-FIX]
         purpose: Payment-local transactional outbox + Booking inbox consumer

Exact-Validation:
dotnet build: PASS (0 errors)
Payment.UnitTests: 54 passed
Booking.UnitTests: 45 passed
ArchitectureTests: 280 passed
Persistence.IntegrationTests: 69 passed
Host.IntegrationTests: 53 passed
frontend touched: NO
frontend typecheck: N/A
frontend lint: N/A
frontend build: N/A
git diff --check: PASS

Durability-Evidence:
Payment success + PaymentSucceeded outbox: same PaymentDbContext SaveChanges (callback + recheck)
Outbox table: payment.outbox_messages (Id=PaymentId, OccurredAt, MessageType, Payload jsonb, ProcessedAt)
One logical success event per Payment: unique PK on payment id; duplicate verified success does not insert a second row
Event contract: PaymentSucceededIntegrationEvent (PaymentId, BookingId, OccurredAt, Amount, CurrencyCode) — no PII
Event is trigger not trust: PaymentSuccessOutboxBoundary.EventMeansBookingConfirmed = false
Dispatcher: PaymentSuccessOutboxDispatcher (callable) + delayed hosted drain (1 minute first tick)
Delivery semantics: at-least-once; ProcessedAt stays null if consumer throws
Booking consumer: BookingPaymentSucceededIntegrationHandler implements IPaymentSucceededIntegrationHandler
Consumer revalidates via IPaymentSuccessEvidenceQuery then BookingPaymentConfirmationService.ConfirmIfEligibleAsync
Confirmation clock: consumer uses IClock now, not event.OccurredAt (delayed expire/cancel sees current hold/status)
Booking inbox: booking.payment_success_inbox (PK payment_id) — idempotent local effect
Delayed delivery after expire: Booking stays Pending; BookingConfirmationRecoveryIssue.ExpiredHold; inbox recorded
Delayed delivery after cancel: Booking stays Cancelled; reason CancelledBooking; inbox recorded
Duplicate delivery: Booking confirms once; hold consumed once; one inbox row
Missing payment evidence: Confirm throws; inbox not written; outbox remains retryable
Query contract kept: IPaymentSuccessEvidenceQuery still required
Refund: NO
T006 executed: NO
Distributed Payment+Booking transaction: NO
Peer-schema FK: NO
Shared DbContext: NO
Payment does not write Booking tables: YES
Booking owns Confirm: YES (ConfirmFromAuthoritativePaymentSuccess)
Named production provider: NO
PaymentStatus exact: Pending / Succeeded
PaymentAttemptStatus exact: Created / Initiated / Succeeded / Failed
BookingStatus exact: Pending / Confirmed / Cancelled
CapacityHoldStatus exact: Active / Consumed / Released / Expired
P20-R6 through P20-R8: OPEN
T005 architect-ACCEPTED: NO (DURABILITY-FIX delivered; awaiting review)

T006-Executed: NO
Next-State: AWAITING_ARCHITECT_REVIEW

END_TRAVELCORE_CURSOR_RESULT_V1
```
