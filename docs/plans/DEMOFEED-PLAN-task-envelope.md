# TC-DEMOFEED-PLAN Task Envelope (architect, live)

Captured after pipeline ledger correction `327d18c` · Post-P29 Evolution · architect issued `TC-DEMOFEED-PLAN`.

```text
BEGIN_TRAVELCORE_CURSOR_EXECUTION_V1

Protocol-Version: 1
Execution-ID: DEMOFEED-PLAN-EXECUTION-01
Phase: Post-P29 Evolution
Task: TC-DEMOFEED-PLAN
Baseline: 327d18c

Objective:
Create a temporary Demo Feeding plan to populate TravelCore with realistic demo data.
This is NOT a permanent TravelCore module. MUST remain removable.
Repository is source of truth.

Scope:
Documentation only.

Create:
- docs/plans/DEMOFEED-implementation-plan.md
- docs/plans/DEMOFEED-PLAN-task-envelope.md

Update:
- docs/PROJECT-STATE.md
- docs/ROADMAP.md

Restrictions:
No product code · migration · API · frontend · scraping implementation

Return:
BEGIN_TRAVELCORE_CURSOR_RESULT_V1
STOP. Do not execute TC-DEMOFEED-T002.

END_TRAVELCORE_CURSOR_EXECUTION_V1
```
