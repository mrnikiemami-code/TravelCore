# TC-UIVAL-T001 — Foundation Primitives Validation Evidence

**Task:** `TC-UIVAL-T001` — Foundation primitives validation (ROADMAP UI Validation #1)  
**Baseline:** `084b480` (`docs: add UI Validation implementation plan`)  
**Route under test:** `/[locale]/dev/foundation`  
**Quality command:** `npm run quality` → **PASS** (lint · typecheck · build · test:quality)  
**Date:** 2026-08-20

Related: P02 T003–T011 · [`docs/ui/01-design-system-architecture.md`](../ui/01-design-system-architecture.md) · [`docs/plans/UIVAL-implementation-plan.md`](UIVAL-implementation-plan.md)

---

## 1. Scope delivered

| Artifact | Purpose |
|----------|---------|
| `features/foundation-validation/foundation-primitives-showcase.tsx` | Exhaustive Server Component composition of foundation primitives |
| `app/[locale]/dev/foundation/page.tsx` | Dev-only noindex validation route under PublicShell |
| `scripts/uival-foundation-checks.mjs` | Deterministic export/boundary/route checks |
| Extended `npm run test:quality` | Runs UIVAL foundation checks after P02 checks |

---

## 2. Primitive coverage matrix

| Primitive | Demonstrated | Server Component | Notes |
|-----------|--------------|------------------|-------|
| `Container` | PASS | PASS | narrow · content · wide widths |
| `Stack` | PASS | PASS | gap composition |
| `Inline` | PASS | PASS | touch-target samples |
| `Surface` | PASS | PASS | default + muted tones |
| `Text` | PASS | PASS | display → caption roles |
| `BidiText` | PASS | PASS | `dir="auto"` email sample |
| `LtrValue` | PASS | PASS | refs · routes · flight codes |
| `MoneyText` | PASS | PASS | USD + IRR display unit |
| `MixedCurrencyPrice` | PASS | PASS | no FX / no silent sum |
| `FieldMessage` | PASS | PASS | help · error · status tones |
| `VisuallyHidden` | PASS | PASS | SR-only label association |
| `MediaImage` | PASS | PASS | local static fixture |
| `RouteStatePanel` | PASS | PASS | generic status panel |
| `RouteLoadingSkeleton` | PASS | PASS | aria-live loading chrome |
| `SkipLink` | PASS (layout) | PASS | locale layout → `#main-content` |
| `NotFoundView` | PASS (export) | PASS | export lock in checks; used on not-found routes |

---

## 3. Locale matrix (SSR probes)

Representative widths: **360** (mobile) · **1280** (desktop).  
Method: production `next build` + HTML probe expectations via automated checks + build SSR.

| Cell | Locale | HTTP | `html[dir]` | Primitives in HTML | noindex | Result |
|------|--------|------|-------------|-------------------|---------|--------|
| FA Desktop | fa | 200 | `rtl` | Money · bidi · FieldMessage | PASS | **PASS** |
| FA Mobile | fa | 200 | `rtl` | same SSR tree (CSS responsive) | PASS | **PASS** |
| EN Desktop | en | 200 | `ltr` | Money · bidi · FieldMessage | PASS | **PASS** |
| EN Mobile | en | 200 | `ltr` | same SSR tree | PASS | **PASS** |

---

## 4. Cross-cutting checks

| Check | Evidence | Result |
|-------|----------|--------|
| No `"use client"` in `components/ui` | `uival-foundation-checks.mjs` | **PASS** |
| Dev route not indexed | `robots: { index: false, follow: false }` | **PASS** |
| Token source present | `styles/tokens.css` semantic vars | **PASS** |
| Money invariants | existing `money.test.ts` | **PASS** |
| P02 quality regression | `p02-quality-checks.mjs` | **PASS** |
| Backend authority | no pricing/booking logic in showcase | **PASS** |
| Business logic in UI layer | presentation/format only | **PASS** |

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

1. T001 does not add Button/Dialog/Sheet interactive primitives — out of scope per UIVAL plan stop rule.
2. No screenshot/visual diff platform — deterministic SSR + CSS architecture only.
3. Home page (`/[locale]/`) smoke remains; `/dev/foundation` is the canonical exhaustive validation surface.
4. AR locale not in matrix — consistent with P02 fixture policy (fa/en only).

---

## 7. Gate readiness

**TC-UIVAL-T001 COMPLETE** · Foundation primitives validation accepted · **TC-UIVAL-T002** (Foreign Package Tour Detail) next.
