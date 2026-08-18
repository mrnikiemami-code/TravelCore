# TC-P20-T007 Task Envelope (architect, live)

Captured from the same ChatGPT tab after `TC-P20-T006` RESULT.

```text
TC-P20-T006 = ACCEPTED
Implementation Commit: 33f08d1
Result/docs HEAD: dfb45d8
HEAD == origin/main
Working Tree: CLEAN
P20-R7 = RESOLVED
```

Executable task:

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1
Protocol-Version: 1
Task-ID: TC-P20-T007
Phase: P20
Title: Public Booking payment journey, authorization, transactional reads, and privacy boundary
Baseline: dfb45d8
Decision: P20-R7 = RESOLVED
Auto-Execute after PASS: return TC-P20-T007 RESULT; do NOT execute T008; remain in PIPELINE
END_TRAVELCORE_CURSOR_TASK_V1
```

Core shape:

- Public Payment is Booking-scoped, not standalone CRUD.
- Reuse `X-TravelCore-Booking-Access-Token`; BookingId/PaymentId are not credentials.
- Authenticated access requires object-level ownership; missing/wrong token → 404.
- Behavior APIs only: initiate + status. No amount/currency/success from client. Server-selected provider.
- No real provider: honest unavailable. Browser return ≠ PaymentSuccess. Callback remains separate.
- No card collection, no public Refund API/UI, noindex transactional pages.
- UI: `/[locale]/bookings/[bookingId]/payment` + return; Payment Succeeded / Booking Pending is a first-class state.
- P20-R8 stays OPEN.
