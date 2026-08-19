# TC-UIVAL-T003 — Experience Tour Detail Validation Evidence

**Task:** `TC-UIVAL-T003` — Experience Tour Detail validation (ROADMAP UI Validation #3)  
**Baseline:** `b776346` (`feat(web): UIVAL-T002 foreign tour detail validation`)  
**Route under test:** `/[locale]/dev/experience-tour` (typed fixture) · production catalog at `/[locale]/tours/[slug]` when `kind=Experience`  
**Quality command:** `npm run quality` → **PASS**  
**Date:** 2026-08-20

Related: P10 T009 · P14-R4 · [`docs/pages/02-experience-tour-detail.md`](../pages/02-experience-tour-detail.md)

---

## 1. Scope delivered

| Artifact | Purpose |
|----------|---------|
| `lib/fixtures/experience-tour-detail/{fa,en,index}.ts` | Typed Experience `TourDetailPageViewModel` fixtures |
| `app/[locale]/dev/experience-tour/page.tsx` | Dev-only noindex route → `TourDetailView` |
| `scripts/uival-experience-tour-checks.mjs` | Archetype + fixture + sticky action checks |
| Extended `npm run test:quality` | Runs experience tour UIVAL checks |

Existing composition preserved:

- `features/public-experience/experience-detail-sections.tsx` — itinerary timeline · difficulty · equipment · guides
- `features/tour-detail/tour-detail-view.tsx` — shared Detail shell; Experience sections compose in (P14-R4)
- `features/public-experience/detail-sticky-actions.tsx` — presentation-only sticky bar (not Booking)

---

## 2. Archetype pressure-test matrix

| Pressure (ROADMAP / page spec) | Evidence | Result |
|--------------------------------|----------|--------|
| Structured itinerary (not blob) | 3 days · stops · meals in fixture + sections | **PASS** |
| Difficulty / eligibility | `Moderate` + MinAge/Fitness facts | **PASS** |
| Equipment required vs recommended | Boots Required · RainJacket Recommended | **PASS** |
| Stops / destination-place refs | `LtrValue` on stop sortOrder + ids | **PASS** |
| Guide / accommodation / transport | guides · accommodationPlan · localTransport blocks | **PASS** |
| Not Foreign Package layout | no hotel-option cards · no flight segment table | **PASS** |
| Server Component first | page · view · sections without `"use client"` | **PASS** |
| Sticky presentation actions | `PublicDetailStickyActions` fixed bottom bar | **PASS** |
| RTL / LTR | FA `dir=rtl` · EN `dir=ltr` | **PASS** |

---

## 3. Locale matrix (dev fixture route)

| Cell | Locale | Fixture slug | HTTP | `html[dir]` | Itinerary days | Result |
|------|--------|--------------|------|-------------|----------------|--------|
| FA Desktop | fa | fixture-daryache-experience | 200 | `rtl` | 3 | **PASS** |
| FA Mobile | fa | same SSR | 200 | `rtl` | 3 | **PASS** |
| EN Desktop | en | fixture-daryache-experience | 200 | `ltr` | 3 | **PASS** |
| EN Mobile | en | same SSR | 200 | `ltr` | 3 | **PASS** |
| AR | ar | none | notFound | — | — | **PASS** |

---

## 4. Cross-cutting checks

| Check | Evidence | Result |
|-------|----------|--------|
| Kind guard on dev route | `kind !== "Experience"` → notFound | **PASS** |
| Distinct FA/EN copy | separate fixture files | **PASS** |
| UIVAL-T001/T002 regression | prior uival checks | **PASS** |
| P02 regression | `p02-quality-checks.mjs` | **PASS** |

---

## 5. Validation battery

| Suite | Result |
|-------|--------|
| `npm run lint` | **PASS** |
| `npm run typecheck` | **PASS** |
| `npm run build` | **PASS** |
| `npm run test:quality` | **PASS** |
| `git diff --check` | **PASS** |

---

## 6. Known limitations

1. Dev route uses typed fixture — not live P10 experience/presentation API.
2. Maps/widgets deferred — no map embed in T003 scope.
3. No pixel-perfect visual regression platform.
4. Tour Listing/Search is **TC-UIVAL-T004** next.

---

## 7. Gate readiness

**TC-UIVAL-T003 COMPLETE** · Experience Tour Detail validation accepted · **TC-UIVAL-T004** (Tour Listing/Search) next.
