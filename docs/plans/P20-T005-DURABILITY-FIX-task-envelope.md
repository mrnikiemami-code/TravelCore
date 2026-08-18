# TC-P20-T005-DURABILITY-FIX Task Envelope (architect, live)

Captured from ChatGPT after T005-VERIFY. Executable task is `TC-P20-T005-DURABILITY-FIX`. Do not execute T006.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1
Task-ID: TC-P20-T005-DURABILITY-FIX
Phase: P20
Parent-Task: TC-P20-T005
Title: Close PaymentSucceeded to Booking confirmation crash gap with Payment-local transactional outbox
Baseline: ecc61c4

Purpose:
Payment success and Booking confirmation currently rely on a pull query with no Payment-local transactional outbox.
Crash after Payment commit Succeeded can leave Booking forever Pending.
Implement module-local transactional outbox + Booking idempotent consumer.
Event is a trigger, not trust. Keep IPaymentSuccessEvidenceQuery. No Refund. No T006.

Architect status of parent:
TC-P20-T005 = REWORK REQUIRED
P20-R5 = REWORKING DURABILITY GAP

Required shape:
Payment tx: Payment -> Succeeded + PaymentSucceeded outbox COMMIT ATOMICALLY
Outbox delivery -> Booking consumer -> IPaymentSuccessEvidenceQuery (revalidate)
-> Booking-owned ConfirmFromAuthoritativePaymentSuccess

Auto-Execute after PASS:
- return TC-P20-T005-DURABILITY-FIX RESULT to architect
- do NOT execute TC-P20-T006
- remain in PIPELINE

END_TRAVELCORE_CURSOR_TASK_V1
```
