# Home Discovery Composition — Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-HOMFEED-PLAN` |
| Track | Home Discovery Composition (post-HOTIDX · closes DISCLINK-T015 deferral) |
| Status | **COMPLETE / ACCEPTED** (`TC-HOMFEED-GATE`) |
| Baseline | `3b058e2` (`feat(web): complete HOTIDX T001-GATE`) |
| Authoritative sources | DISCLINK-T015 deferral · HOTIDX · DISCLINK travelogues · home discovery |
| Frontend root | `src/frontend/web` |

Curated locale-scoped home composition from existing public loaders — **not** user-personalized · **not** Search engine.

---

## 0. Transition resolve

| Question | Answer |
|----------|--------|
| Prior track | **HOTIDX COMPLETE** (`TC-HOTIDX-GATE` `3b058e2`) |
| Architect priority | Close DISCLINK-T015 home discovery composition gap |
| Home today | Entry links only — no preview sections |
| User profiling / ML recommendations | **DEFERRED** (T015) |

---

## 3. Ordered task map

| Task | Deliverable |
|------|-------------|
| `TC-HOMFEED-PLAN` | This plan + SoT sync |
| `TC-HOMFEED-T001` | Home composition types + preview limits |
| `TC-HOMFEED-T002` | load-home-discovery-composition loader |
| `TC-HOMFEED-T003` | Travelogue preview section on home |
| `TC-HOMFEED-T004` | Hotel preview section on home |
| `TC-HOMFEED-T005` | Wire locale home page to composition loader |
| `TC-HOMFEED-T006` | Index links (see all travelogues / hotels) |
| `TC-HOMFEED-T007` | Detail links from preview cards |
| `TC-HOMFEED-T008` | Discovery entry links retained |
| `TC-HOMFEED-T009` | Public shell unchanged |
| `TC-HOMFEED-T010` | Not-user-personalized guard |
| `TC-HOMFEED-T011` | Not-search-engine guard |
| `TC-HOMFEED-T012` | No dev links on production home |
| `TC-HOMFEED-T013` | UGC vs Place boundary on sections |
| `TC-HOMFEED-T014` | Locale-prefixed internal links |
| `TC-HOMFEED-T015` | ML recommendation engine deferral |
| `TC-HOMFEED-GATE` | Track acceptance |

---

## Revision history

| Date | Change |
|------|--------|
| 2026-08-20 | Initial plan after HOTIDX COMPLETE |
| 2026-08-20 | Track COMPLETE / ACCEPTED — `TC-HOMFEED-GATE` |
