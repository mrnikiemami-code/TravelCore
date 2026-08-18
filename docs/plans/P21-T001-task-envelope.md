# TC-P21-T001 Task Envelope (architect, live)

Captured from the same ChatGPT tab after `TC-P21-PLAN = ACCEPTED` and `P21-R1 = RESOLVED`.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1
Task-ID: TC-P21-T001
Phase: P21
Title: HotelBooking module foundation, schema ownership, and Place catalog reference
Baseline: 58a6206
Decision: P21-R1 = RESOLVED
Auto-Execute after PASS: return TC-P21-T001 RESULT; do NOT execute T002; do NOT invent R2–R8
END_TRAVELCORE_CURSOR_TASK_V1
```

Full architect envelope is in the ChatGPT conversation. Implementation follows the P20-T001 scaffolding pattern: independent module + schema `hotel_booking` + logical Place reference only.
