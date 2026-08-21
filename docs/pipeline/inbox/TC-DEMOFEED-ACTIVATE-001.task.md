BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1

Task-ID: TC-DEMOFEED-ACTIVATE-001

Phase: Post-P30 --- Data Enablement / DEMOFEED

Objective: Transition TravelCore from the accepted P30 Product
Experience Foundation into the previously-authored DEMOFEED
data-enablement path.

P30 is accepted. The locked product sequence is:

Experience → Data → Commercial

This task authorizes DEMOFEED activation only if the existing repository
plan is complete, compatible with current accepted architecture, and
safe to execute.

Do not redesign DEMOFEED from scratch. Repository SoT is authoritative.

Required first step:

Read and verify the existing DEMOFEED plan and current recovery/roadmap
state.

At minimum inspect:

docs/PROJECT-STATE.md

docs/ROADMAP.md

docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md

the existing TC-DEMOFEED implementation plan / plan artifact(s)

P30 gate evidence and accepted product-experience locks

Confirm:

P30 is accepted.

DEMOFEED was previously authored and deferred until after P30.

The existing DEMOFEED plan still matches current architecture.

No newer accepted decision supersedes it.

Working tree / branch / origin state is safe.

Execution authorization:

If the existing DEMOFEED plan contains a clearly ordered first
executable task: - execute ONLY that first DEMOFEED implementation task
in this cycle - do not invent a different task - do not execute later
DEMOFEED tasks in the same cycle unless the existing plan explicitly
defines them as one atomic task

If the plan is incomplete, stale, conflicting, or does not identify a
safe first executable unit: - do not invent implementation - return
BLOCKED with the exact SoT conflict or missing decision

Architecture constraints:

Preserve all accepted boundaries.

Forbidden: - backend redesign - domain ownership changes - database
redesign - fake production claims - scraping competitor sites - copying
competitor content - weakening P30 visual/product locks - changing
Pricing / Booking / Payment ownership - changing Place / Destination /
Tour ownership - changing Search / SEO ownership

DEMOFEED intent:

Provide controlled, honest, development/demo data needed to make
accepted P30 experiences meaningfully reviewable.

Demo data must be: - clearly non-production - deterministic where
practical - architecture-compatible - locale-aware where required - safe
to reset/reseed if the existing plan specifies it - free of copied
competitor content - free of invented claims presented as real
customer/business facts

Before work:

git rev-parse --show-toplevel

git fetch origin

require branch main

require local main == origin/main

inspect working tree

do not overwrite unrelated user work

Validation:

Follow the existing DEMOFEED plan's validation requirements.

Also require: - git diff --check - architecture boundaries preserved -
no unrelated product changes - successful push when implementation
succeeds - HEAD == origin/main - Working Tree state reported

Result contract:

Return to the authoritative Architect chat:

BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version: 1

Task-ID: TC-DEMOFEED-ACTIVATE-001

Status: PASS / FAIL / BLOCKED

Include: - DEMOFEED plan artifact(s) found - current DEMOFEED plan
status - first executable unit identified - whether it was executed -
implementation summary if executed - changed files - validation -
commit - HEAD == origin/main - Working Tree status - architecture
conflicts / limitations, if any - recommended next authorized DEMOFEED
task ID from repository SoT

Next-State: AWAITING_ARCHITECT_REVIEW

Pipeline Continuity:

After sending RESULT: - do not exit PIPELINE mode - enter WAITING MODE -
wait for the next authorized .task.md or .gate.md - do not infer or
auto-execute the next DEMOFEED unit without a new Architect file

END_TRAVELCORE_CURSOR_TASK_V1
