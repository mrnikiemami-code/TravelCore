# TC-UIVAL-T002 — Foreign Package Tour Detail Validation Evidence

**Task:** `TC-UIVAL-T002` — Foreign Package Tour Detail validation (ROADMAP UI Validation #2)  
**Baseline:** `97840cd` (`feat(web): UIVAL-T001 foundation primitives validation`)  
**Route under test:** `/[locale]/dev/foreign-tour` (typed fixture) · production catalog at `/[locale]/tours/[slug]`  
**Quality command:** `npm run quality` → **PASS**  
**Date:** 2026-08-20

Related: P02 T012–T015 · P11/P14 · [`docs/pages/01-foreign-tour-detail.md`](../pages/01-foreign-tour-detail.md)

---

## 1. Scope delivered

| Artifact | Purpose |
|----------|---------|
| `app/[locale]/dev/foreign-tour/page.tsx` | Dev-only noindex route wiring fixture → `ForeignTourDetailView` |
| `scripts/uival-foreign-tour-checks.mjs` | Archetype structure + fixture + CTA island checks |
| Extended `npm run test:quality` | Runs foreign tour UIVAL checks |

Existing walking skeleton preserved:

- `features/foreign-tour-detail/foreign-tour-detail-view.tsx` (T013)
- `features/foreign-tour-detail/booking-cta-island.tsx` (T014 sticky Client island)
- `lib/fixtures/foreign-tour-detail/{fa,en}.ts` (T012 typed PVM)

---

## 2. Archetype pressure-test matrix

| Pressure (ROADMAP) | Evidence in fixture view | Result |
|--------------------|--------------------------|--------|
| RTL / LTR | FA `dir=rtl` · EN `dir=ltr` on document | **PASS** |
| Bidi (airline / flight / refs) | `LtrValue` for `IKA → IST` · `TK875` · refs | **PASS** |
| Mixed currencies | `MixedCurrencyPrice` USD + IRR components | **PASS** |
| Hotel options | fixture hotel option cards with ratings | **PASS** |
| Mobile sticky CTA | `BookingCtaIsland` `sticky bottom-0` + `min-h-touch` | **PASS** |
| Server Component first | page + view without `"use client"` | **PASS** |
| SEO metadata | dev route `noindex/nofollow`; production tour route uses composed SEO separately | **PASS** |
| Commercial status | active / no_departure / expired / unavailable labels | **PASS** |

---

## 3. Locale matrix (dev fixture route)

| Cell | Locale | Fixture | HTTP | `html[dir]` | CTA island | Result |
|------|--------|---------|------|-------------|------------|--------|
| FA Desktop | fa | istanbul-package | 200 | `rtl` | sticky classes | **PASS** |
| FA Mobile | fa | same SSR | 200 | `rtl` | sticky classes | **PASS** |
| EN Desktop | en | istanbul-package | 200 | `ltr` | sticky classes | **PASS** |
| EN Mobile | en | same SSR | 200 | `ltr` | sticky classes | **PASS** |
| AR | ar | none | notFound | — | — | **PASS** (no fabricated fixture) |

---

## 4. Cross-cutting checks

| Check | Evidence | Result |
|-------|----------|--------|
| Typed PVM | `types/pages/foreign-tour-detail.ts` | **PASS** |
| No backend pricing authority in UI | display-only Money/MixedCurrency | **PASS** |
| Client boundary isolated | only `BookingCtaIsland` in feature folder | **PASS** |
| UIVAL-T001 regression | `uival-foundation-checks.mjs` | **PASS** |
| P02 regression | `p02-quality-checks.mjs` (allowlist synced) | **PASS** |

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

1. Dev route uses P02 fixture — not live P11 TourProduct API composition (validated separately on `/tours/[slug]`).
2. No pixel-perfect visual regression platform.
3. Live booking handoff remains placeholder (Walking Skeleton policy).
4. Experience Tour archetype is **TC-UIVAL-T003** next.

---

## 7. Gate readiness

**TC-UIVAL-T002 COMPLETE** · Foreign Package Tour Detail validation accepted · **TC-UIVAL-T003** (Experience Tour Detail) next.
