# TC-P29-T003 Task Envelope (plan-derived, continuous pipeline)

Derived from `P29-implementation-plan.md` after `TC-P29-T002` on commit `8308bb2` · continuous pipeline execution.

```text
BEGIN_TRAVELCORE_CURSOR_EXECUTION_V1

Protocol-Version: 1
Execution-ID: P29-T003-EXECUTION-01
Phase: P29
Task: TC-P29-T003
Baseline: 8308bb2

Objective:
Define security/authorization review boundary without identity provider or permission engine product.

Execute ONLY TC-P29-T003.
Do not execute TC-P29-T004.

Restrictions:
No identity provider · No OAuth/OIDC product · No permission engine rewrite · No API/frontend

Validation:
dotnet build TravelCore.sln
dotnet test tests/Architecture/TravelCore.ArchitectureTests --filter "FullyQualifiedName~HardeningSecurity"

Commit:
feat(hardening): define T003 security authorization boundary

END_TRAVELCORE_CURSOR_EXECUTION_V1
```
