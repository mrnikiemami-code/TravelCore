# TC-P28-T005 Task Envelope (architect, live)

Captured from ChatGPT after `TC-P28-T004 = ACCEPTED` on commit `e2eee8a`.

```text
BEGIN_TRAVELCORE_CURSOR_EXECUTION_V1

Protocol-Version: 1
Execution-ID: P28-T005-EXECUTION-01
Phase: P28
Task: TC-P28-T005
Baseline: e2eee8a

Objective:
Execute TC-P28-T005 according to:
docs/plans/P28-implementation-plan.md

Repository is source of truth.

Execute ONLY TC-P28-T005.
Do not execute TC-P28-T006.

Objective:
Define data access performance boundary.
Controlled architecture, not premature optimization.

Expected focus:
- data access measurement boundary
- read optimization boundary
- evidence-based optimization rules

Restrictions:
- No Dapper implementation unless explicitly justified by evidence
- No ORM replacement
- No query optimization without measurement
- No schema redesign
- No migration unless explicitly required
- No Redis/cache implementation
- No API/frontend changes
- No ownership changes

After completion update:
docs/plans/P28-implementation-plan.md
docs/PROJECT-STATE.md
docs/ROADMAP.md

Validation:
dotnet build TravelCore.sln
git diff --check

Commit:
feat(performance): define T005 data access performance boundary

Return:
BEGIN_TRAVELCORE_CURSOR_RESULT_V1
STOP. Do not start TC-P28-T006.

END_TRAVELCORE_CURSOR_EXECUTION_V1
```
