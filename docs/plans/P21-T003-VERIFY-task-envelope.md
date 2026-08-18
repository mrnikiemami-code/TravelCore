# TC-P21-T003-VERIFY Task Envelope

Captured from the live ChatGPT architect tab after `TC-P21-T003` result review. T003 remains **not architect-accepted** until this verification PASSes.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1
Task-ID: TC-P21-T003-VERIFY
Parent-Task: TC-P21-T003
Phase: P21
Title: Verify T003 baseline lineage and R3 availability-hold evidence
Baseline: 2696407

Purpose:
Verify TC-P21-T003 before architect acceptance.

The T003 result reports:

Task-issued expected baseline:
a844bcf

Actual Starting-HEAD:
a0f5c99

Implementation commit:
2696407

This task must explain the commit lineage between a844bcf and a0f5c99 and verify
that no unreviewed or unrelated product work entered the repository before T003.

This is a verification task only.

Do NOT implement P21-R4.
Do NOT execute TC-P21-T004.
Do NOT introduce new HotelBooking capability unless a minimal correction is
required to restore already accepted R3 semantics.

END_TRAVELCORE_CURSOR_TASK_V1
```

Full live architect text is the source of Required items 1–44. Summary:

- Explain `a844bcf` → `a0f5c99` lineage (expected: T002 result/SoT docs only)
- Confirm T003 (`2696407`) is strictly P21-R3
- No hidden R4–R8, no Payment/Refund change, no public API, no named supplier
- Hold lifecycle exact: Requested / Active / Released / Expired
- Partial success cannot Activate; timeout remains Requested
- Database-backed idempotency + `ux_hotel_availability_holds_one_unresolved`
- Do not record T003 as architect ACCEPTED
- After PASS: return RESULT; do NOT execute T004
