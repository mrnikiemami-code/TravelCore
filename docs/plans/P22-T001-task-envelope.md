# TC-P22-T001 Task Envelope (architect, live)

Captured from the same ChatGPT tab after `TC-P22-PLAN = ACCEPTED` and `P22-R1 = RESOLVED`.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1
Task-ID: TC-P22-T001
Phase: P22
Title: Flight module foundation, schema ownership, FlightBooking transaction boundary, and Tour separation
Baseline: b32a867
Decision: P22-R1 = RESOLVED
Auto-Execute after PASS: return TC-P22-T001 RESULT; do NOT execute T002; do NOT invent R2–R8
END_TRAVELCORE_CURSOR_TASK_V1
```

Full architect envelope is in the ChatGPT conversation. Implementation follows the HotelBooking/Visa T001 scaffolding pattern: independent Flight module + schema `flight` + FlightBooking ownership assigned inside Flight without implementing the aggregate.
