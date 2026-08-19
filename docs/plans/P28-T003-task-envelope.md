# TC-P28-T003 Task Envelope (architect, live)

Captured from ChatGPT after `TC-P28-T002 = ACCEPTED` on commit `38d9ca4`.

```text
BEGIN_TRAVELCORE_CURSOR_EXECUTION_V1

Protocol-Version: 1
Execution-ID: P28-T003-EXECUTION-01
Phase: P28
Task: TC-P28-T003
Baseline: 38d9ca4

Objective:
Execute TC-P28-T003 according to:
docs/plans/P28-implementation-plan.md

Repository is source of truth.

Execute ONLY TC-P28-T003.
Do not execute TC-P28-T004.

Objective:
Define performance observability and measurement boundary.
Measurement foundation, not optimization implementation.

Restrictions:
- Profile before optimize remains mandatory
- No speculative tuning
- No benchmark claims without evidence
- No Redis/cache/CDN implementation
- No database optimization without measured evidence
- No API/frontend changes
- No ownership leakage into Performance module

After completion update:
docs/plans/P28-implementation-plan.md
docs/PROJECT-STATE.md
docs/ROADMAP.md

Validation:
dotnet build TravelCore.sln
git diff --check

Commit:
feat(performance): define T003 measurement and observability boundary

Return:
BEGIN_TRAVELCORE_CURSOR_RESULT_V1
STOP. Do not start TC-P28-T004.

END_TRAVELCORE_CURSOR_EXECUTION_V1
```
