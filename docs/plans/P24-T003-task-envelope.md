# TC-P24-T003 Task Envelope (architect, live)

Captured from ChatGPT after `TC-P24-T002 = ACCEPTED` on commit `e811513`.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1
Task-ID: TC-P24-T003
Phase: P24
Title: Agency Membership & Access Relationship Boundary
Baseline: e811513

Purpose:
Define how an agency user connects to Access without creating User, Role, Permission, invitation, or user management.

Allowed:
- B2B.Domain models representing membership intent/reference
- AgencyMemberReference / AgencyAccessRelationshipBoundary (logical references only)
- No real user storage

Forbidden:
AgencyMember table · User table · Invitation flow · Role table · Permission table · Access policy · Authentication changes · Authorization changes · API · Frontend

Persistence: NO migration · NO tables

Documentation: P24-R3 = RESOLVED

Commit: feat(b2b): define agency membership access boundary

STOP: Do not execute TC-P24-T004 until architect acceptance.

END_TRAVELCORE_CURSOR_TASK_V1
```
