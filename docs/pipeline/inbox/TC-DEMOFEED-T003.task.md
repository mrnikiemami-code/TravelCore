BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1

Task-ID: TC-DEMOFEED-T003

Phase: Post-P30 --- Data Enablement / DEMOFEED

Objective: Implement the next authorized DEMOFEED unit from the
repository SoT: Destination demo seed.

This task continues TC-DEMOFEED-T002. Do not redesign DEMOFEED
architecture. Do not skip ahead to T004+.

Scope:

Allowed: - existing DEMOFEED tool boundary created in T002 -
docs/plans/DEMOFEED-implementation-plan.md defined paths - demo data
assets required by this task only

Forbidden: - domain redesign - production database changes unless
explicitly authorized by DEMOFEED plan - registering DemoFeed as a
production module - changing TravelCore.Api startup/module
registration - fake production claims - scraping competitor content -
copying competitor data

Required first steps:

Read and verify:

docs/plans/DEMOFEED-implementation-plan.md

docs/PROJECT-STATE.md

docs/ROADMAP.md

docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md

Confirm: - TC-DEMOFEED-T002 boundary exists - this task is the next
authorized DEMOFEED step - no newer architecture decision conflicts

Implementation goal:

Implement Destination demo seed according to the existing DEMOFEED plan.

Requirements:

Keep DemoFeed removable.

Keep demo concerns isolated from domain ownership.

Make seed behavior deterministic if the plan requires it.

Ensure demo data is clearly distinguishable from production data.

Do not create fake real-world claims.

Do not bypass existing architecture boundaries.

Validation:

Before work: - git rev-parse --show-toplevel - git fetch origin - verify
branch main - verify local main == origin/main - inspect working tree

Required: - follow DEMOFEED plan validation - git diff --check - dotnet
build affected projects - architecture boundary verification - HEAD ==
origin/main - Working Tree status reported

Result:

Return:

BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version: 1

Task-ID: TC-DEMOFEED-T003

Status: PASS / FAIL / BLOCKED

Include: - SoT verification - implementation summary - changed files -
validation - architecture boundary status - commit - HEAD status -
Working Tree status - limitations - recommended next authorized DEMOFEED
task from repository SoT

Next-State: AWAITING_ARCHITECT_REVIEW

Pipeline Continuity:

After RESULT:

Do not exit PIPELINE mode.

Enter WAITING MODE.

Do not auto-execute T004+.

Wait for next authorized .task.md or .gate.md file.

END_TRAVELCORE_CURSOR_TASK_V1
