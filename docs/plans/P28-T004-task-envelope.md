# TC-P28-T004 Task Envelope (architect, live)

Captured from ChatGPT after `TC-P28-T003 = ACCEPTED` on commit `4ac1876`.

```text
BEGIN_TRAVELCORE_CURSOR_EXECUTION_V1

Protocol-Version: 1
Execution-ID: P28-T004-EXECUTION-01
Phase: P28
Task: TC-P28-T004
Baseline: 4ac1876

Objective:
Execute TC-P28-T004 according to:
docs/plans/P28-implementation-plan.md

Repository is source of truth.

Execute ONLY TC-P28-T004.
Do not execute TC-P28-T005.

Objective:
Define runtime performance boundary and interaction model.

Focus:
- performance ownership boundary
- runtime interaction contracts
- measurement-driven approach

Restrictions:
- No Redis/cache/CDN implementation
- No database tuning or query optimization without evidence
- No APM vendor lock-in
- No infrastructure expansion
- No API/frontend changes
- No business ownership movement

After completion update:
docs/plans/P28-implementation-plan.md
docs/PROJECT-STATE.md
docs/ROADMAP.md

Validation:
dotnet build TravelCore.sln
git diff --check

Commit:
feat(performance): define T004 runtime boundary and interaction model

Return:
BEGIN_TRAVELCORE_CURSOR_RESULT_V1
STOP. Do not start TC-P28-T005.

END_TRAVELCORE_CURSOR_EXECUTION_V1
```
