# Moderation Operations — Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-MODOPS-PLAN` |
| Track | Moderation Operations (post-DISCLINK · closes PRODSURF-T015 deferral) |
| Status | implementation **COMPLETE** · architect **RETROACTIVELY ACCEPTED AFTER FORENSIC REVIEW** (`TC-MODOPS-GATE`) |
| Baseline | `38604d3` (`feat(web): complete DISCLINK T001-GATE`) |
| Authoritative sources | P16 UGC lifecycle · PRODSURF-T015 deferral · Access P03 · admin surface patterns |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

Live admin moderation workflow for UGC travelogues — **not** Content CMS, **not** SEO IndexPolicy, **not** automated enforcement.

---

## 0. Transition resolve

| Question | Answer |
|----------|--------|
| Prior track | **DISCLINK COMPLETE** (`TC-DISCLINK-GATE` `38604d3`) |
| Architect priority | Close PRODSURF-T015 live moderation admin workflow gap |
| P16 domain lifecycle | Implemented (`Approve` / `Reject` / `Publish` on aggregates) |
| Prior gap | No admin HTTP API · no admin UI |
| Review / photo / comment queues | **DEFERRED** (T013 — travelogue-first scope) |
| Report-driven moderation | **DEFERRED** (T014) |
| Bulk / automated moderation | **DEFERRED** (T015) |

---

## 3. Ordered task map

| Task | Deliverable |
|------|-------------|
| `TC-MODOPS-PLAN` | This plan + SoT sync |
| `TC-MODOPS-T001` | Access permissions + authorization policies |
| `TC-MODOPS-T002` | UGC moderation contracts (queue DTOs + service port) |
| `TC-MODOPS-T003` | UGC moderation service (travelogue pending queue + mutations) |
| `TC-MODOPS-T004` | Admin moderation HTTP endpoints |
| `TC-MODOPS-T005` | UgcModule composition wiring |
| `TC-MODOPS-T006` | Admin route `/admin/ugc/moderation` |
| `TC-MODOPS-T007` | Moderation workflow island + server actions |
| `TC-MODOPS-T008` | Admin catalog hub entry link |
| `TC-MODOPS-T009` | Admin noindex metadata |
| `TC-MODOPS-T010` | Access policy guard checks |
| `TC-MODOPS-T011` | UGC ≠ Content CMS boundary guard |
| `TC-MODOPS-T012` | Approved ≠ Published lifecycle guard |
| `TC-MODOPS-T013` | Travelogue-first scope + review queue deferral |
| `TC-MODOPS-T014` | Report-driven moderation deferral |
| `TC-MODOPS-T015` | Bulk / automated moderation deferral |
| `TC-MODOPS-GATE` | Track acceptance |

---

## Revision history

| Date | Change |
|------|--------|
| 2026-08-20 | Initial plan after DISCLINK COMPLETE · architect moderation prioritization |
