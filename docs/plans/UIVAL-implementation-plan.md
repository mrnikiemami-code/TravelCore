# UI Validation — Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-UIVAL-PLAN` |
| Track | UI Validation Sequence (product evolution) |
| Status | **IN PROGRESS** · T005 **COMPLETE** · T006 next |
| Baseline | `b27f820` (`docs: sync Post-P29 full task ledger in SoT`) |
| Authoritative sources | `docs/ROADMAP.md` § UI Validation Sequence · `docs/ui/*` · `docs/architecture/10-ui-constitution.md` · `docs/ui/04-page-archetype-contract.md` · P02 foundation evidence |
| Frontend root | `src/frontend/web` |

This document is the **authoritative execution plan** for the UI Validation track after master roadmap completion (Post-P29). It validates production UI surfaces against the UI constitution — **not** new backend boundary phases.

> **Envelope note:** Master architecture pipeline (P00–P29 + Post-P29) is **COMPLETE**. UI Validation is **product evolution** per ROADMAP and architect handoff — not a speculative P30 infrastructure phase.

---

## 0. Transition resolve (from SoT)

| Question | Answer |
|----------|--------|
| Prior track status | **Post-P29 COMPLETE / ACCEPTED** (`TC-Post-P29-GATE` `f0d897b`) |
| Authoritative next track | **UI Validation Sequence** (`docs/ROADMAP.md` § UI Validation) |
| Backend module phases remaining? | **NO** — master roadmap exhausted |
| P02 foundation primitives exist? | **YES** — T003–T011 delivered; home smoke at `/[locale]/` |
| Foreign Tour walking skeleton exists? | **YES** — P02 T012–T017 + P11/P14 product surfaces |
| New infrastructure phase without ADR? | **FORBIDDEN** — this track validates UI only |

---

## 1. Track purpose

Validate TravelCore public/admin UI against locked architecture **before** broad product polish:

1. **Foundation primitives** — tokens, direction-neutral layout, bidi, money, a11y, media, route states
2. **Page archetypes** — Foreign Tour Detail first (ROADMAP pressure test), then Experience Tour, listings, destinations, etc.
3. **Cross-cutting matrix** — FA/EN × mobile/desktop × SSR/hydration × SEO-sensitive rendering where applicable

UI Validation **does not**:

- Introduce new backend bounded contexts or Evolution/Hardening boundaries
- Redesign accepted ADRs or domain ownership
- Ship speculative features outside validation scope
- Replace architect/product backlog prioritization for post-validation delivery

---

## 2. Validation strategy (repository-supported)

| Gate | Tooling |
|------|---------|
| Lint | `npm run lint` in `src/frontend/web` |
| Typecheck | `npm run typecheck` |
| Production build | `npm run build` |
| Deterministic checks | `npm run test:quality` (extended per task) |
| Diff hygiene | `git diff --check` |
| Server/Client boundary | no `"use client"` in `components/ui`; allowlist unchanged unless task adds islands |
| RTL/LTR | FA (`dir=rtl`) and EN (`dir=ltr`) on same validation surface |
| Responsive | representative widths: 360 · 1280 minimum per validation task |
| Evidence | `docs/plans/UIVAL-T00N-*-validation-evidence.md` per task |

Full browser E2E farm is **out of scope** unless a future task explicitly adds it with justification.

---

## 3. Ordered task map (ROADMAP sequence)

| Task | ROADMAP item | Primary deliverable |
|------|--------------|---------------------|
| `TC-UIVAL-PLAN` | — | This plan + SoT sync |
| `TC-UIVAL-T001` | 1. Foundation primitives | Dev validation route + automated primitive checks + evidence (**COMPLETE**) |
| `TC-UIVAL-T002` | 2. Foreign Package Tour Detail | Archetype validation matrix + evidence (**COMPLETE**) |
| `TC-UIVAL-T003` | 3. Experience Tour Detail | Archetype validation matrix + evidence (**COMPLETE**) |
| `TC-UIVAL-T004` | 4. Tour Listing/Search | Surface validation + evidence (**COMPLETE**) |
| `TC-UIVAL-T005` | 5. Destination Landing | Surface validation + evidence (**COMPLETE**) |
| `TC-UIVAL-T006` | 6. Hotel Detail | Surface validation + evidence |
| `TC-UIVAL-T007` | 7. Home / Discovery | Surface validation + evidence |
| `TC-UIVAL-T008` | 8. Content Article | Surface validation + evidence |
| `TC-UIVAL-T009` | 9. Travelogue | Surface validation + evidence |
| `TC-UIVAL-T010` | 10. Visa | Surface validation + evidence |
| `TC-UIVAL-T011` | 11. Booking/Checkout | Surface validation + evidence |
| `TC-UIVAL-T012` | 12. Flight Search | Surface validation + evidence |
| `TC-UIVAL-T013` | 13. Hotel Booking Search | Surface validation + evidence |
| `TC-UIVAL-T014` | 14. Admin surfaces | Surface validation + evidence |
| `TC-UIVAL-T015` | 15. Agency surfaces | Surface validation + evidence |
| `TC-UIVAL-GATE` | — | Track acceptance evidence · UI Validation COMPLETE |

---

## 4. TC-UIVAL-T001 — Foundation primitives validation

| Field | Content |
|-------|---------|
| Objective | Formally validate P02 foundation primitives (T003–T011) against UI constitution via dedicated showcase + automated checks. |
| Exact scope | `/[locale]/dev/foundation` noindex route · `FoundationPrimitivesShowcase` feature · `uival-foundation-checks.mjs` · evidence doc |
| Allowed | frontend validation route · quality script extension · docs |
| Forbidden | new interactive primitives (Button/Dialog product) · backend · `"use client"` in `components/ui` · indexable SEO surface |
| Dependencies | P02 COMPLETE · Post-P29 COMPLETE |
| Acceptance | all exported primitives demonstrated · FA/EN SSR probes PASS · automated checks PASS · evidence doc complete |
| Proofs | `npm run quality` · `git diff --check` |
| Artifacts | showcase feature · dev route · checks script · `docs/plans/UIVAL-T001-foundation-primitives-validation-evidence.md` |
| Stop | design system expansion beyond validation · theme switching · new form controls without architect lock |

---

## 5. Out of scope (entire UIVAL track unless task says otherwise)

- Backend API/product changes
- New platform modules (Evolution/Hardening/Analytics boundaries)
- Microservice extraction · mobile native apps
- Full visual regression platform
- SEO Validation Sequence (separate ROADMAP track)

---

## 6. Revision history

| Date | Change |
|------|--------|
| 2026-08-20 | Initial UI Validation plan after Post-P29 COMPLETE · baseline `b27f820` |
