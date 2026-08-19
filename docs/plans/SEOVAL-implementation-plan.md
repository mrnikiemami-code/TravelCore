# SEO Validation — Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-SEOVAL-PLAN` |
| Track | SEO Validation Sequence (product evolution) |
| Status | **COMPLETE / ACCEPTED** (`TC-SEOVAL-GATE`) |
| Baseline | `8204605` (`feat(web): complete UIVAL T011-GATE and UI Validation track`) |
| Authoritative sources | `docs/ROADMAP.md` § SEO Validation Sequence · P05 SEO Engine evidence · `src/lib/seo/*` · `TravelCore.Modules.Seo` |
| Frontend root | `src/frontend/web` |
| Backend SEO module | `src/backend/Modules/Seo` |

This document is the **authoritative execution plan** for the SEO Validation track after UI Validation completion. It validates SEO contracts, publication paths, and metadata integration — **not** new backend boundary phases.

> **Envelope note:** Master architecture pipeline (P00–P29 + Post-P29) and UI Validation are **COMPLETE**. SEO Validation is **product evolution** per ROADMAP — not a speculative P30 infrastructure phase.

---

## 0. Transition resolve (from SoT)

| Question | Answer |
|----------|--------|
| Prior track status | **UI Validation COMPLETE / ACCEPTED** (`TC-UIVAL-GATE` `8204605`) |
| Authoritative next track | **SEO Validation Sequence** (`docs/ROADMAP.md` § SEO Validation) |
| Backend SEO module (P05) | **COMPLETE** — SeoRoute, redirects, sitemap, hreflang, metadata compose, structured data |
| Frontend SEO integration | **PARTIAL / DELIVERED** — `loadComposedSeoMetadata`, hreflang/canonical mapping on public pages |
| New infrastructure phase without ADR? | **FORBIDDEN** — this track validates SEO only |

---

## 1. Track purpose

Validate TravelCore SEO against locked architecture **before** broad production SEO rollout:

1. **URL/locale constitution** — locale-prefixed public routes, registry, no silent negotiation override
2. **Entity publication** — Destination, Tour, Place, Content, Programmatic landing surfaces
3. **SEO engine contracts** — SeoRoute, slugs, canonical, hreflang, redirects, sitemap, JSON-LD
4. **Internal linking & programmatic boundaries** — graph boundaries, controlled landing pages
5. **Production Search Console** — explicitly deferred (ROADMAP item #15)

SEO Validation **does not**:

- Introduce new backend bounded contexts or Evolution/Hardening boundaries
- Redesign accepted ADRs or domain slug/route ownership
- Ship speculative SEO features outside validation scope
- Require live Search Console / production crawl in CI

---

## 2. Validation strategy (repository-supported)

| Gate | Tooling |
|------|---------|
| Lint | `npm run lint` in `src/frontend/web` |
| Typecheck | `npm run typecheck` |
| Production build | `npm run build` |
| Deterministic checks | `npm run test:quality` (extended per task) |
| Frontend unit tests | `hreflang.test.ts` · `metadata-compose.test.ts` · `breadcrumb-jsonld.test.ts` |
| Backend SEO unit tests | `tests/Unit/TravelCore.Modules.Seo.UnitTests/*` (referenced by checks; run via `dotnet test` separately) |
| Diff hygiene | `git diff --check` |
| Evidence | `docs/plans/SEOVAL-T00N-*-validation-evidence.md` per task |

Live Search Console validation is **out of scope** for CI — deferred to production operations (T015 evidence-only).

---

## 3. Ordered task map (ROADMAP sequence)

| Task | ROADMAP item | Primary deliverable |
|------|--------------|---------------------|
| `TC-SEOVAL-PLAN` | — | This plan + SoT sync |
| `TC-SEOVAL-T001` | 1. URL/locale constitution | Contract checks + evidence |
| `TC-SEOVAL-T002` | 2. Destination entity | Publication + page integration checks + evidence |
| `TC-SEOVAL-T003` | 3. SeoRoute | Domain + endpoint + unit test checks + evidence |
| `TC-SEOVAL-T004` | 4. Localized slugs | Slug ownership + dynamic route checks + evidence |
| `TC-SEOVAL-T005` | 5. canonical | Metadata contract + page mapping checks + evidence |
| `TC-SEOVAL-T006` | 6. hreflang | Frontend + backend hreflang checks + evidence |
| `TC-SEOVAL-T007` | 7. redirects | Redirect engine + endpoint checks + evidence |
| `TC-SEOVAL-T008` | 8. sitemap | Sitemap engine + endpoint checks + evidence |
| `TC-SEOVAL-T009` | 9. structured data | JSON-LD contract + page integration checks + evidence |
| `TC-SEOVAL-T010` | 10. internal linking | Graph boundary + related-content checks + evidence |
| `TC-SEOVAL-T011` | 11. Tour landing pages | Tour detail/listing metadata checks + evidence |
| `TC-SEOVAL-T012` | 12. Place pages | Place detail metadata checks + evidence |
| `TC-SEOVAL-T013` | 13. Content pages | Article metadata checks + evidence |
| `TC-SEOVAL-T014` | 14. controlled Programmatic SEO | Landing page + boundary checks + evidence |
| `TC-SEOVAL-T015` | 15. Search Console / production validation later | Deferral evidence + guard checks |
| `TC-SEOVAL-GATE` | — | Track acceptance |

---

## 4. Out of scope (entire SEOVAL track unless task says otherwise)

- Backend API/product changes beyond validation artifacts
- New platform modules (Evolution/Hardening/Analytics boundaries)
- Live Google Search Console integration in CI
- Full crawl/index monitoring platform
- UI Validation Sequence (separate ROADMAP track — COMPLETE)

---

## 5. Revision history

| Date | Change |
|------|--------|
| 2026-08-20 | Initial SEO Validation plan after UIVAL COMPLETE · baseline `8204605` |
