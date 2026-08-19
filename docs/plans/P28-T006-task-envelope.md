# TC-P28-T006 Task Envelope (architect, live)

Captured from ChatGPT after `TC-P28-T005 = ACCEPTED` on commit `05d50c8`.

```text
BEGIN_TRAVELCORE_CURSOR_EXECUTION_V1

Protocol-Version: 1
Execution-ID: P28-T006-EXECUTION-01
Phase: P28
Task: TC-P28-T006
Baseline: 05d50c8

Objective:
Execute TC-P28-T006 according to:
docs/plans/P28-implementation-plan.md

Repository is source of truth.

Execute ONLY TC-P28-T006.
Do not execute TC-P28-T007.

Objective:
Define caching boundary and cache policy architecture.

Focus:
- cache ownership
- cache eligibility rules
- invalidation principles
- consistency boundaries

Restrictions:
- Redis implementation is NOT required
- Redis is NOT Source of Truth
- No cache provider implementation
- No distributed cache deployment
- No API/frontend changes
- No business data ownership movement
- No premature optimization
- No migration unless explicitly required

After completion update:
docs/plans/P28-implementation-plan.md
docs/PROJECT-STATE.md
docs/ROADMAP.md

Validation:
dotnet build TravelCore.sln
git diff --check

Commit:
feat(performance): define T006 caching boundary and policy architecture

Return:
BEGIN_TRAVELCORE_CURSOR_RESULT_V1
STOP. Do not start TC-P28-T007.

END_TRAVELCORE_CURSOR_EXECUTION_V1
```
