BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1

Task-ID: TC-DEMOFEED-T004

Phase: Post-P30 --- Data Enablement / DEMOFEED

Objective: Implement the next authorized DEMOFEED unit from repository
SoT: Place / Tour related demo data preparation according to the
existing DEMOFEED implementation plan.

This task continues: TC-DEMOFEED-T002 boundary TC-DEMOFEED-T003
Destination seed

Do not skip ahead to later DEMOFEED tasks.

Required first steps:

Read and verify:

docs/plans/DEMOFEED-implementation-plan.md

docs/PROJECT-STATE.md

docs/ROADMAP.md

docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md

Confirm: - T003 completed successfully - T004 is the next authorized
unit - no newer architecture decision conflicts

Architecture rules:

Preserve:

Modular Monolith boundaries

schema-per-module ownership

existing domain ownership

Place ≠ HotelBooking

Tour ≠ Pricing/Booking ownership

DemoFeed remains removable tooling

Forbidden:

registering DemoFeed as production module

modifying TravelCore.Api startup for demo purposes

domain redesign

database redesign

fake production claims

competitor scraping/content copying

Implementation:

Follow the existing DEMOFEED plan exactly.

If T004 contains multiple substeps: - execute only the first atomic
authorized substep - do not invent future DEMOFEED work

Demo data rules:

deterministic

clearly non-production

idempotent where required

removable according to existing DemoFeed boundaries

no fake business claims presented as real

Validation:

Before work:

git rev-parse --show-toplevel

git fetch origin

verify branch main

verify local main == origin/main

inspect working tree

Required:

DEMOFEED plan validation

dotnet build affected projects

git diff --check

architecture boundary verification

HEAD == origin/main

Working Tree status

Result:

Return:

BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version: 1

Task-ID: TC-DEMOFEED-T004

Status: PASS / FAIL / BLOCKED

Include:

SoT verification

implementation summary

changed files

validation

architecture boundary status

commit

HEAD status

Working Tree status

limitations

recommended next authorized DEMOFEED task

Next-State: AWAITING_ARCHITECT_REVIEW

Pipeline Continuity:

After RESULT:

Do not exit PIPELINE mode.

Enter WAITING MODE.

Do not auto-execute later DEMOFEED tasks.

Wait for next authorized .task.md or .gate.md file.

END_TRAVELCORE_CURSOR_TASK_V1
