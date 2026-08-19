# Hotel Catalog Browse Index — Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-HOTIDX-PLAN` |
| Track | Hotel Catalog Browse Index (post-MODOPS · closes DISCLINK-T014 deferral) |
| Status | **COMPLETE / ACCEPTED** (`TC-HOTIDX-GATE`) |
| Baseline | `9961699` (`feat(ugc): complete MODOPS T001-GATE`) |
| Authoritative sources | DISCLINK-T014 deferral · P07 Place catalog · PRODSURF `/hotels/[slug]` |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

Public hotel discovery browse index — **not** Search engine · **not** HotelBooking availability.

---

## 0. Transition resolve

| Question | Answer |
|----------|--------|
| Prior track | **MODOPS COMPLETE** (`TC-MODOPS-GATE` `9961699`) |
| Architect priority | Close DISCLINK-T014 hotel catalog browse index gap |
| PRODSURF route | `/hotels/[slug]` exists but no browse index |
| Prior gap | Place admin list API lacks locale slug for public discovery |
| Personalized hotel feed | **DEFERRED** (T015) |

---

## 3. Ordered task map

| Task | Deliverable |
|------|-------------|
| `TC-HOTIDX-PLAN` | This plan + SoT sync |
| `TC-HOTIDX-T001` | Public hotel browse contracts |
| `TC-HOTIDX-T002` | Place public hotel browse query |
| `TC-HOTIDX-T003` | Public HTTP endpoint |
| `TC-HOTIDX-T004` | PlaceModule composition wiring |
| `TC-HOTIDX-T005` | `/hotels` discovery index route |
| `TC-HOTIDX-T006` | Hotel index SEO metadata |
| `TC-HOTIDX-T007` | Hotel list loader |
| `TC-HOTIDX-T008` | Hotel discovery view |
| `TC-HOTIDX-T009` | Index links to hotel detail |
| `TC-HOTIDX-T010` | Home discovery hotels entry link |
| `TC-HOTIDX-T011` | Public shell on hotel index |
| `TC-HOTIDX-T012` | Discovery-not-search guard |
| `TC-HOTIDX-T013` | Locale-prefixed internal links |
| `TC-HOTIDX-T014` | Places route retained alongside hotels |
| `TC-HOTIDX-T015` | Personalized hotel feed deferral |
| `TC-HOTIDX-GATE` | Track acceptance |

---

## Revision history

| Date | Change |
|------|--------|
| 2026-08-20 | Initial plan after MODOPS COMPLETE · architect hotel browse prioritization |
