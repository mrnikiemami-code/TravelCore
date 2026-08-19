# Discovery Linking — Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-DISCLINK-PLAN` |
| Track | Discovery Linking Sequence (post-PRODSURF surface wiring) |
| Status | **COMPLETE / ACCEPTED** (`TC-DISCLINK-GATE`) |
| Baseline | `7065f33` (`feat(providers): complete PROVINT T001-GATE`) |
| Authoritative sources | PRODSURF routes · SEOVAL internal linking · P16 UGC · home discovery |
| Frontend root | `src/frontend/web` |

Wire production surfaces into discovery paths and internal UGC links — **not** Search engine or personalized feed.

---

## 0. Transition resolve

| Question | Answer |
|----------|--------|
| Prior track | **PROVINT COMPLETE** (`TC-PROVINT-GATE` `7065f33`) |
| Architect priority | Product discovery wiring after surface completion |
| PRODSURF routes | `/travelogues/[id]` · `/hotels/[slug]` exist but under-linked |
| Hotel catalog browse index | **DEFERRED** (T014 — slug not on list API) |
| Personalized feed | **DEFERRED** (T015) |

---

## 3. Ordered task map

| Task | Deliverable |
|------|-------------|
| `TC-DISCLINK-PLAN` | This plan + SoT sync |
| `TC-DISCLINK-T001` | `/travelogues` discovery index route |
| `TC-DISCLINK-T002` | Travelogue index SEO metadata |
| `TC-DISCLINK-T003` | UGC composition links to travelogue detail |
| `TC-DISCLINK-T004` | Hotel book CTA uses `/hotels/[slug]/book` |
| `TC-DISCLINK-T005` | Home discovery travelogues entry link |
| `TC-DISCLINK-T006` | Production home link hygiene |
| `TC-DISCLINK-T007` | Public shell on travelogue index |
| `TC-DISCLINK-T008` | Travelogue list loader |
| `TC-DISCLINK-T009` | Discovery-not-search guard |
| `TC-DISCLINK-T010` | Locale-prefixed internal links |
| `TC-DISCLINK-T011` | Places route retained alongside hotels |
| `TC-DISCLINK-T012` | UGC ≠ Content boundary on links |
| `TC-DISCLINK-T013` | Internal linking loaders unchanged |
| `TC-DISCLINK-T014` | Hotel catalog browse index deferral |
| `TC-DISCLINK-T015` | Personalized feed deferral |
| `TC-DISCLINK-GATE` | Track acceptance |

---

## Revision history

| Date | Change |
|------|--------|
| 2026-08-20 | Initial plan after PROVINT COMPLETE |
