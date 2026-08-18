# TC-P20-T008 Task Envelope (architect, live)

Captured from the same ChatGPT tab after `TC-P20-T007` RESULT.

```text
TC-P20-T007 = ACCEPTED
Implementation Commit: 542cee9
Result/docs HEAD: 8daeba7
HEAD == origin/main
Working Tree: CLEAN
P20-R8 = RESOLVED
```

Executable task:

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1
Protocol-Version: 1
Task-ID: TC-P20-T008
Phase: P20
Title: Provider capability model, operational payment reads, security hardening, and production-readiness boundary
Baseline: 8daeba7
Decision: P20-R8 = RESOLVED
Auto-Execute after PASS: return TC-P20-T008 RESULT; do NOT execute T009; remain in PIPELINE
END_TRAVELCORE_CURSOR_TASK_V1
```

Core shape:

- Named production provider remains NONE. No real SDK. Zero production providers is valid.
- Explicit capabilities: RedirectInitiation, CallbackVerification, PaymentStatusQuery, RefundInitiation, RefundVerification, RefundStatusQuery.
- Capabilities are declared by adapter/configuration, not inferred from provider name.
- Disabled/unknown/unsupported providers fail safely. No failover or public provider choice.
- Operational Payment/Refund reads are internal/read-only. No Booking-token access. No manual financial mutation.
- Recheck uses authoritative provider query only. Operator does not choose the outcome.
- Provider adapter checklist for future real-provider work. P20-R1 through P20-R8 RESOLVED.
- Do not execute T009.
