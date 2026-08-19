# P25 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P25-PLAN` |
| Phase | P25 — Notification |
| Status | PLAN ACCEPTED · **P25 IN_PROGRESS** · T001–T005 progression · channel boundary delivered |
| Baseline | `b6a090a` (`feat(notification): add T004 module schema foundation`) |
| Authoritative sources | `docs/ROADMAP.md` § P25 · `docs/PROJECT-STATE.md` · `docs/architecture/04-module-boundaries.md` · `docs/architecture/05-dependency-rules.md` · `docs/architecture/06-cross-module-communication.md` · `docs/architecture/07-data-architecture.md` · `docs/domain/module-ownership-matrix.md` · `docs/architecture/15-future-architecture-transition-map.md` § V · P18 TripPlanner notification intent boundaries · P19 Booking · P20 Payment · P24 B2B |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

This document is the architecture plan for the Notification phase.

> **Envelope note:** `TC-P25-T001`–`T004` ACCEPTED · `TC-P25-T005` implemented (channel boundary) · **do not execute `TC-P25-T006` until architect accepts `T005`**.

---

## 0. Next-phase resolve (from SoT)

| Question | Answer |
|----------|--------|
| Prior phase status | **P24 COMPLETE / ACCEPTED** |
| Authoritative next phase | **P25 — Notification** |
| Declared status before this plan | **PLANNED / NOT_STARTED** |
| PLAN already existed? | **YES** — minimal plan authored in `TC-P25-T001` |
| Dedicated Notification module/schema in SoT today? | **YES** — `TC-P25-T004` delivered independent module + schema `notification` (no product tables) |
| Notification provider implemented? | **NO** — TripPlanner/Payment/Booking expose intent/boundary markers only |

---

## 1. Phase purpose

P25 introduces Notification delivery boundaries as a downstream platform capability without collapsing Booking, Payment, Identity, Access, Party, TripPlanner, or B2B ownership.

Planned scope themes from SoT:

- Notification channels: Email · SMS · In-app
- Provider abstraction posture (no named production provider in early tasks)
- Explicit separation from Booking/Payment execution ownership
- Semantic event consumption only — Notification must not become a business-rule owner

---

## 2. Preserved locked architecture

P25 must preserve:

1. Modular Monolith: schema-per-module boundaries; no peer-schema FK.
2. No shared DbContext across modules.
3. No distributed transactions.
4. Notification delivery must not become an execution owner for Payment/Booking.
5. Core domain correctness must not depend synchronously on Notification delivery success.
6. Booking/Payment/TripPlanner may emit semantic events or intent markers; they do not own SMTP/SMS/push providers.

---

## 3. Current SoT baseline snapshot

- P24 B2B is complete; B2B does not own notification delivery.
- P20 Payment owns money movement; `OwnsNotificationDelivery = false`.
- P18 TripPlanner owns lead/contact/consent facts; `TripPlannerNotificationBoundary` marks provider as not implemented.
- Notification module scaffolding delivered via `TC-P25-T004` (schema `notification`; no product tables). Further R2–R8 decisions remain OPEN until architect lock.
- Transition map § V targets P25 for channels/provider abstraction.

---

## 4. Decision inventory for P25 (open for architect locks)

| ID | Topic | Status |
|----|-------|--------|
| `P25-R1` | Notification module ownership / schema / downstream posture vs Booking/Payment/TripPlanner | **RESOLVED** — independent Notification module · schema `notification` · **Notification != Booking** · **Notification != Payment** · **Notification != Identity** · **Notification != Access** · **Notification != Party** · **Notification != TripPlanner** · **Notification != B2B** · semantic event consumption only · no peer-schema FK · host registers after B2B without endpoints |
| `P25-R2` | Channel boundary (Email / SMS / In-app) | **RESOLVED** — `NotificationChannelKind` / `NotificationChannelBoundary` / `NotificationChannelReference` in Notification.Domain · channel taxonomy only · Notification owns channel semantics · publishers do not call providers directly · no channel persistence · no provider execution |
| `P25-R3` | Provider abstraction boundary | **OPEN** — provider-neutral delivery contracts · no named production provider · zero-provider posture valid until explicit lock |
| `P25-R4` | Template / orchestration boundary | **OPEN** — template/render orchestration owned by Notification · business modules publish semantic intent/facts only |
| `P25-R5` | Preferences / consent interaction boundary | **OPEN** — delivery preferences distinct from TripPlanner consent snapshots · marketing vs transactional separation preserved |
| `P25-R6` | Event consumption / idempotency boundary | **OPEN** — downstream async consumer · failed Notification must not rollback committed SoR transactions · idempotent delivery posture required |
| `P25-R7` | Public/admin operational boundary | **OPEN** — no fake production send success · internal read/ops posture only until explicit product lock |
| `P25-R8` | Deferred/out-of-scope posture (push/webhook/marketing platform/advanced routing) | **OPEN** — push/webhook/campaign tooling remain deferred unless explicitly locked |

---

## 5. Execution sequence

Proposed sequence after plan acceptance:

1. `TC-P25-T001` — P25 architecture implementation plan (**IMPLEMENTED / ACCEPTED**)
2. `TC-P25-T002` — plan-driven SoT alignment (**IMPLEMENTED / ACCEPTED**)
3. `TC-P25-T003` — plan decision inventory + execution sequence authoring (**IMPLEMENTED / ACCEPTED**)
4. `TC-P25-T004` — ownership/module/schema foundation (**IMPLEMENTED / ACCEPTED**)
5. `TC-P25-T005` — channel boundary (**IMPLEMENTED / AWAITING_ARCHITECT_REVIEW**)
6. `TC-P25-T006` — provider abstraction boundary (**NOT EXECUTED**)
7. `TC-P25-T007` — event consumption / template orchestration boundary (**NOT EXECUTED**)
8. `TC-P25-T008` — hardening and guardrails (**NOT EXECUTED**)
9. `TC-P25-T009` — evidence pack (**NOT EXECUTED**)
10. `TC-P25-GATE` — acceptance gate (**NOT EXECUTED**)

### TC-P25-T005 — Channel boundary

- Purpose: define Notification-owned channel taxonomy (Email · SMS · In-app) without provider execution or channel persistence.
- Delivered: `NotificationChannelKind` · `NotificationChannelBoundary` · `NotificationChannelReference` · `ChannelBoundaryImplemented` flag · guardrail tests.
- Forbidden in this task: provider SDK · SMTP/SMS/push adapters · channel/delivery persistence · API/frontend · migrations beyond T004 schema foundation.

### TC-P25-T004 — Ownership/module/schema foundation

- Purpose: introduce independent Notification module scaffolding with schema `notification` only.
- Delivered: Contracts/Domain/Infrastructure · `NotificationOwnershipBoundary` · `NotificationPublisherBoundary` · `NotificationDbContext` · host registration after B2B · EnsureSchema migration · guardrail tests.
- Forbidden in this task: provider SDK · channel/template/delivery persistence · API/frontend · peer-schema FK · shared DbContext.

### TC-P25-T003 — Plan decision inventory + execution sequence

- Purpose: expand the approved P25 plan from minimal scope notes into an executable decision inventory and task sequence without adding product code.
- Delivered: sections 0–5 in this document · P25-R1–R8 enumerated OPEN · execution sequence through GATE declared · envelope note updated.
- Forbidden in this task: module code · schema/migration · API · frontend · provider SDK · product tables.

---

## 6. Out-of-scope (explicitly not executed in plan-driven tasks)

- Module code implementation
- Schema/migration implementation
- API / Frontend / Dashboard surfaces
- Tests (except documentation validation and architecture tests explicitly required by a product task)

---

## 7. Plan outcome target

- `TC-P25-T001`–`T003` establish the authoritative P25 execution map.
- Product tasks (`T004+`) may begin only after architect acceptance of the corresponding prior task.
- `P25-GATE` closes the phase after R1–R8 are RESOLVED and T004–T009 are accepted.
