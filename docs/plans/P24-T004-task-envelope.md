# TC-P24-T004 Task Envelope (architect, live)

Captured from ChatGPT after `TC-P24-T003 = ACCEPTED` on commit `2f4788a`.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1
Task-ID: TC-P24-T004
Phase: P24
Title: Agency Commercial Profile Boundary
Baseline: 2f4788a

Purpose:
Define agency commercial information boundary in B2B without contract, commission, credit, settlement, or financial execution.

Allowed:
- AgencyCommercialProfileBoundary / AgencyBusinessReference / CommercialCapabilityReference
- Logical boundary only

Forbidden:
Agency table · Contract · Commission · CommissionRule · CreditLimit · Wallet · Settlement · Invoice · Payment changes · Booking changes · API · Frontend

Persistence: NO migration · NO business tables

Documentation: P24-R4 = RESOLVED

Commit: feat(b2b): define agency commercial profile boundary

STOP: Do not execute TC-P24-T005 until architect acceptance.

END_TRAVELCORE_CURSOR_TASK_V1
```
