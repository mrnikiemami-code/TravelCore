# Product Surface Completion — Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-PRODSURF-PLAN` |
| Track | Product Surface Completion (post-LAUNCHOPS UIVAL gaps) |
| Status | **COMPLETE / ACCEPTED** (`TC-PRODSURF-GATE`) |
| Baseline | `027063b` (`feat(ops): complete LAUNCHOPS T001-GATE and Launch Operations track`) |
| Authoritative sources | UIVAL T006/T009 · PRODDEL gaps · P07 Place · P16 UGC · `docs/pages/06-content-and-travelogue.md` |
| Frontend root | `src/frontend/web` |
| Backend root | `src/backend` |

Close remaining UIVAL-validated surfaces without production routes — **not** new architecture phases.

---

## 0. Transition resolve

| Question | Answer |
|----------|--------|
| Prior track | **LAUNCHOPS COMPLETE** (`TC-LAUNCHOPS-GATE` `027063b`) |
| Architect priority | Product features — hotel catalog + travelogue production surfaces |
| Hotel detail (UIVAL T006) | Place catalog `Hotel` kind — needs user-facing `/hotels/[slug]` route |
| Travelogue (UIVAL T009) | UGC narrative — needs production route + public read by id |
| Real payment/flight/hotel providers | **OUT OF SCOPE** — deferred (T014) |

---

## 3. Ordered task map

| Task | Deliverable |
|------|-------------|
| `TC-PRODSURF-PLAN` | This plan + SoT sync |
| `TC-PRODSURF-T001` | UGC public travelogue GetById API |
| `TC-PRODSURF-T002` | Production `/travelogues/[travelogueId]` route |
| `TC-PRODSURF-T003` | Travelogue SEO metadata compose |
| `TC-PRODSURF-T004` | Production `/hotels/[slug]` Place catalog route |
| `TC-PRODSURF-T005` | Hotel SEO metadata on hotels route |
| `TC-PRODSURF-T006` | Production `/hotels/[slug]/book` prepare route |
| `TC-PRODSURF-T007` | Transactional noindex guard (hotel book) |
| `TC-PRODSURF-T008` | UIVAL dev routes retained (noindex) |
| `TC-PRODSURF-T009` | Public shell on new routes |
| `TC-PRODSURF-T010` | UGC ≠ Content boundary guard |
| `TC-PRODSURF-T011` | Place catalog ownership on hotels route |
| `TC-PRODSURF-T012` | TravelogueDetailView reuse |
| `TC-PRODSURF-T013` | `load-travelogue-detail` loader |
| `TC-PRODSURF-T014` | Real providers deferral evidence |
| `TC-PRODSURF-T015` | Live moderation workflow deferral evidence |
| `TC-PRODSURF-GATE` | Track acceptance |

---

## Revision history

| Date | Change |
|------|--------|
| 2026-08-20 | Initial plan after LAUNCHOPS COMPLETE · architect product prioritization |
