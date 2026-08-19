# Launch Operations — Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-LAUNCHOPS-PLAN` |
| Track | Launch Operations Sequence (post-PRODDEL production readiness) |
| Status | **COMPLETE / ACCEPTED** (`TC-LAUNCHOPS-GATE`) |
| Baseline | `fad38a0` (`feat(web): complete PRODDEL T001-GATE and Product Delivery track`) |
| Authoritative sources | ADR 0007 · `docs/i18n/01-locale-and-routing.md` · P29 hardening · SEOVAL/PRODDEL deferrals |
| Frontend root | `src/frontend/web` |
| Backend root | `src/backend` |

Operational readiness track after Product Delivery — wires deferred launch items without new architecture phases.

---

## 0. Transition resolve

| Question | Answer |
|----------|--------|
| Prior track | **PRODDEL COMPLETE** (`TC-PRODDEL-GATE` `fad38a0`) |
| Root locale negotiation | **DEFERRED** since P02 — now in scope for T001 |
| Search Console live validation | **DEFERRED** to production ops (T015) |
| Full E2E browser farm | **DEFERRED** (T014) |

---

## 3. Ordered task map

| Task | Deliverable |
|------|-------------|
| `TC-LAUNCHOPS-PLAN` | This plan + SoT sync |
| `TC-LAUNCHOPS-T001` | Root Accept-Language entry negotiation |
| `TC-LAUNCHOPS-T002` | Explicit locale URL override guard |
| `TC-LAUNCHOPS-T003` | Backend health endpoints posture |
| `TC-LAUNCHOPS-T004` | Search Console ops runbook evidence |
| `TC-LAUNCHOPS-T005` | Production deployment checklist evidence |
| `TC-LAUNCHOPS-T006` | Public SEO endpoints (sitemap/robots) |
| `TC-LAUNCHOPS-T007` | Recovery prompt readiness |
| `TC-LAUNCHOPS-T008` | Pipeline protocol readiness |
| `TC-LAUNCHOPS-T009` | Hardening / health boundary checks |
| `TC-LAUNCHOPS-T010` | Observability deferred guard |
| `TC-LAUNCHOPS-T011` | Secrets / deployment deferred guard |
| `TC-LAUNCHOPS-T012` | Frontend a11y skip-link on layout |
| `TC-LAUNCHOPS-T013` | Mobile viewport on layout |
| `TC-LAUNCHOPS-T014` | E2E / live crawl deferral evidence |
| `TC-LAUNCHOPS-T015` | Live GSC verification deferral evidence |
| `TC-LAUNCHOPS-GATE` | Track acceptance |

---

## Revision history

| Date | Change |
|------|--------|
| 2026-08-20 | Initial plan after PRODDEL COMPLETE |
