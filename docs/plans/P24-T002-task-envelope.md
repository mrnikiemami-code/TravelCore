# TC-P24-T002 Task Envelope (architect, live)

Captured from ChatGPT after `TC-P24-T001 = ACCEPTED` on commit `cc4adcc`.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1
Task-ID: TC-P24-T002
Phase: P24
Title: Agency Business Identity Boundary
Baseline: cc4adcc

Purpose:
تعریف مرز مفهومی Agency به عنوان یک Business Concept در B2B بدون انتقال مالکیت از Party، Identity یا Access.

Rules:
Repository is source of truth.
Do NOT redesign architecture.
Do NOT move organization ownership from Party.
Do NOT create Identity entities.
Do NOT create Access entities.
Do NOT create Booking relations.
Do NOT create Payment relations.

Allowed domain boundary concepts (names are candidates):
AgencyReference / AgencyRelationshipBoundary / AgencyMembershipBoundary

Forbidden:
Agency table · Agency CRUD · Agency registration · Contract · Commission · Credit · Wallet · Settlement · Booking distribution · Payment changes · Public API · Frontend

Persistence: NO migration · NO new business tables

Documentation: P24-R2 = RESOLVED

Commit: feat(b2b): define agency identity boundary

STOP: Do not execute TC-P24-T003 until architect acceptance.

END_TRAVELCORE_CURSOR_TASK_V1
```
