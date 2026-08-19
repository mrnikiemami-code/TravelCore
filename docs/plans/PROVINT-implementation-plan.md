# Provider Integration Readiness — Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-PROVINT-PLAN` |
| Track | Provider Integration Readiness (post-PRODSURF) |
| Status | **COMPLETE / ACCEPTED** (`TC-PROVINT-GATE`) |
| Baseline | `b39d1f0` (`feat(web): complete PRODSURF T001-GATE and Product Surface Completion track`) |
| Authoritative sources | Post-P29-R3 · P20/P21/P22 provider ports · PRODSURF-T014 deferral |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

Prepare module-owned provider/source integration **without** selecting vendors or adding SDKs.

---

## 0. Transition resolve

| Question | Answer |
|----------|--------|
| Prior track | **PRODSURF COMPLETE** (`TC-PRODSURF-GATE` `b39d1f0`) |
| Architect priority | Real payment/flight/hotel providers (readiness first) |
| Post-P29-R3 | Module-owned expansion · no global registry mega-table |
| Production adapters today | **NONE** (valid configuration) |
| Named vendor selection | **DEFERRED** (T014 — requires architect lock + ADR) |
| Live sandbox/production credentials | **DEFERRED** (T015) |

---

## 3. Ordered task map

| Task | Deliverable |
|------|-------------|
| `TC-PROVINT-PLAN` | This plan + SoT sync |
| `TC-PROVINT-T001` | Payment provider readiness checklist posture |
| `TC-PROVINT-T002` | Payment module zero production gateway guard |
| `TC-PROVINT-T003` | Hotel availability source adapter checklist |
| `TC-PROVINT-T004` | Hotel rate source adapter checklist |
| `TC-PROVINT-T005` | Hotel reservation source adapter checklist |
| `TC-PROVINT-T006` | Flight source adapter checklist |
| `TC-PROVINT-T007` | Flight/Hotel NONE production source posture |
| `TC-PROVINT-T008` | Provider configuration / secrets posture doc |
| `TC-PROVINT-T009` | Post-P29-R3 evolution provider boundary |
| `TC-PROVINT-T010` | Hotel source catalog + resolver registration |
| `TC-PROVINT-T011` | Architecture SDK / vendor package ban guard |
| `TC-PROVINT-T012` | Test-only fake adapter conventions |
| `TC-PROVINT-T013` | Cross-module production provider NONE guard |
| `TC-PROVINT-T014` | Named vendor selection deferral evidence |
| `TC-PROVINT-T015` | Live credential wiring deferral evidence |
| `TC-PROVINT-GATE` | Track acceptance |

---

## Revision history

| Date | Change |
|------|--------|
| 2026-08-20 | Initial plan after PRODSURF COMPLETE · architect provider prioritization |
