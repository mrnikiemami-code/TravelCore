# TC-P20-T006 Task Envelope (architect, live)

Captured from the same ChatGPT tab after `TC-P20-T005-DURABILITY-FIX` RESULT.

```text
TC-P20-T005 = ACCEPTED
Verification fix: ecc61c4
Durability fix: c7c846b
Result/docs HEAD: 930a3be
P20-R6 = RESOLVED
```

Executable task:

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1
Protocol-Version: 1
Task-ID: TC-P20-T006
Phase: P20
Title: Full Refund and financial compensation for successful Payment with failed Booking confirmation
Baseline: 930a3be
Decision: P20-R6 = RESOLVED
Auto-Execute after PASS: return TC-P20-T006 RESULT; do NOT execute T007; remain in PIPELINE
END_TRAVELCORE_CURSOR_TASK_V1
```

Full numbered requirements are in the live ChatGPT message and `P20-T006-task-envelope-raw-tail.txt`. Core shape:

- Refund is Payment-owned, distinct from Payment. PaymentStatus stays Pending/Succeeded (no Refunded).
- One logical full Refund per Succeeded Payment; amount/currency from PaymentExecutionSnapshot.
- RefundStatus Pending/Succeeded. RefundAttemptStatus Created/Initiated/Succeeded/Failed.
- BookingConfirmationRecoveryIssue atomically writes Booking-local compensation-required outbox.
- Payment consumes that event idempotently, GetOrCreate Refund, does not trust event amount.
- Authoritative provider refund success + RefundSucceeded outbox atomically.
- Booking consumes RefundSucceeded: Pending -> Cancelled via existing cancellation; Active hold Released; Confirmed not cancelled; Consumed not reversed.
- No public refund API, no partial refund, no real provider, no distributed tx, no peer-schema FK, no shared DbContext.
- P20-R7 and P20-R8 stay OPEN.
