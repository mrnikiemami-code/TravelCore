BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1

Task-ID: TC-DEMOFEED-T005

Phase: Post-P30 --- Data Enablement / DEMOFEED

Objective: Implement the next authorized DEMOFEED unit from repository
SoT: Tour demo data seed.

This task continues:

TC-DEMOFEED-T002 removable feeder boundary

TC-DEMOFEED-T003 Destination demo seed

TC-DEMOFEED-T004 Place + Media seed

Do not redesign DEMOFEED. Do not skip ahead to later DEMOFEED tasks.

Required first steps:

Read and verify:

docs/plans/DEMOFEED-implementation-plan.md

docs/PROJECT-STATE.md

docs/ROADMAP.md

docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md

Confirm:

T004 completed successfully

T005 is the next authorized unit

no newer architecture decision conflicts

Architecture constraints:

Preserve:

Modular Monolith boundaries

schema-per-module ownership

existing domain ownership

Tour ownership boundaries

Pricing ownership

Booking ownership

removable DemoFeed tooling model

Forbidden:

registering DemoFeed as production module

modifying TravelCore.Api startup for demo purposes

direct database bypass of owner paths

domain redesign

pricing redesign

booking redesign

fake production claims

competitor scraping

copied competitor content

Implementation:

Follow the existing DEMOFEED plan exactly.

Implement Tour demo seed according to the plan.

Requirements:

use owner application paths

deterministic identifiers

idempotent seed behavior

clearly non-production demo labeling

compatible with existing Tour architecture

preserve Pricing/Booking as separate ownership

Data rules:

Do not invent:

real commercial availability

real customer demand

fake reviews

fake ratings

fake scarcity

fake sales numbers

Demo data must remain development/demo data.

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

seed/reseed validation if applicable

git diff --check

architecture boundary verification

HEAD == origin/main

Working Tree status

Result:

Return:

BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version: 1

Task-ID: TC-DEMOFEED-T005

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
