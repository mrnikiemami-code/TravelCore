# TC-UIVAL-T004 — Tour Listing/Search Validation Evidence

**Task:** `TC-UIVAL-T004` — Tour Listing/Search validation (ROADMAP UI Validation #4)  
**Baseline:** `2a2b6f2` (`feat(web): UIVAL-T003 experience tour detail validation`)  
**Routes under test:** `/[locale]/dev/tour-listing` · production `/[locale]/tours`  
**Quality command:** `npm run quality` → **PASS**  
**Date:** 2026-08-20

Related: P14-T003/T008 · P14-R3/R8 · [`docs/plans/P14-implementation-plan.md`](P14-implementation-plan.md)

---

## 1. Scope delivered

| Artifact | Purpose |
|----------|---------|
| `lib/fixtures/tour-listing/index.ts` | Fixture catalog selection (Experience + Package cards) |
| `app/[locale]/dev/tour-listing/page.tsx` | Dev-only noindex listing validation with query filters |
| `scripts/uival-tour-listing-checks.mjs` | Listing shell · filters · sort · fixture checks |
| Extended `npm run test:quality` | Runs tour listing UIVAL checks |

Existing P14 surfaces validated:

- `PublicTourListingView` — discovery listing shell
- `ListingFilters` — URL/query presentation filters (GET form)
- `ListingSelection` — presentation sort + card grid
- `filter-presentation.ts` — criteria parse + href composition

---

## 2. Validation matrix

| Criterion | Evidence | Result |
|-----------|----------|--------|
| Not Search engine | copy + plan scope · no `/api/search` in listing view | **PASS** |
| Not SEO landing owner | `LISTING_PURPOSE` · same `/tours` path with query | **PASS** |
| URL filter state | `destination` + `sort` query params | **PASS** |
| Presentation sort only | `localeCompare` on loaded cards · no retrieval rewrite | **PASS** |
| Empty vs filtered states | no destination → prompt · with destination → fixture cards | **PASS** |
| Mixed kind cards | Experience + Package in fixture | **PASS** |
| Server Component first | listing view · filters · selection without `"use client"` | **PASS** |
| Touch targets | `min-h-touch` on filter controls | **PASS** |
| FA / EN | distinct fixture copy per locale | **PASS** |

---

## 3. Probe scenarios (dev route)

| Scenario | URL | Expected | Result |
|----------|-----|----------|--------|
| FA empty | `/fa/dev/tour-listing` | filter prompt · no cards | **PASS** |
| FA filtered | `/fa/dev/tour-listing?destination=istanbul&sort=name` | 2 cards · name sort | **PASS** |
| EN filtered | `/en/dev/tour-listing?destination=istanbul` | EN card labels | **PASS** |
| Sort links | `sort=code` vs `sort=name` | href composition via `listingFilterHref` | **PASS** |

---

## 4. Validation battery

| Suite | Result |
|-------|--------|
| `npm run quality` | **PASS** |
| Prior UIVAL checks | **PASS** |
| `git diff --check` | **PASS** |

---

## 5. Known limitations

1. Dev fixture replaces live Destination API + related-tours read when filter active.
2. Mobile filter sheet / facet UI deferred — current filters are server GET form.
3. Full Search module (P15) is out of scope — listing is discovery presentation only.
4. **TC-UIVAL-T005** — Destination Landing is next.

---

## 6. Gate readiness

**TC-UIVAL-T004 COMPLETE** · Tour Listing/Search validation accepted · **TC-UIVAL-T005** next.
