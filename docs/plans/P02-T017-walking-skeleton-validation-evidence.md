# TC-P02-T017 — Walking Skeleton Validation Matrix Evidence

**Status:** Evidence for architect review (P02 walking skeleton)  
**Baseline:** `ea590d3` (`origin/main`)  
**Route under test:** `/[locale]/tours/fixture-istanbul-package`  
**Quality command:** `npm run quality` → **PASS** (lint · typecheck · build · test:quality)  
**Date:** 2026-08-15

Related: T013 page · T014 CTA island · T015 metadata · T016 quality gates · [`docs/pages/01-foreign-tour-detail.md`](../pages/01-foreign-tour-detail.md)

---

## 1. Mandatory matrix

Representative widths (plan): **360** (mobile) · **1280** (desktop).  
Evidence method: production `next start` HTML probes + T016 automated gates (no browser farm / screenshot platform in T016/T017).

| Cell | Locale | Width intent | HTTP | Title/SEO | `html[dir]` | Bidi islands (`IKA`/`USD`) | CTA present | Result |
|------|--------|--------------|------|-----------|-------------|---------------------------|-------------|--------|
| FA Desktop | fa | 1280 | 200 | FA title in `<title>` | `rtl` | PASS | PASS | **PASS** |
| FA Mobile | fa | 360 | 200 | same SSR HTML (single-column Stack) | `rtl` | PASS | sticky island classes present | **PASS** |
| EN Desktop | en | 1280 | 200 | EN title in `<title>` | `ltr` | PASS | PASS | **PASS** |
| EN Mobile | en | 360 | 200 | same SSR HTML | `ltr` | PASS | sticky island classes present | **PASS** |

Notes:

- FA/EN share one Server Component composition; mobile vs desktop is CSS (Stack/Surface/`min-h-touch`/sticky), not separate trees.
- FA HTML also contains intentional `dir="ltr"` on bidi islands (`LtrValue`) — **not** a document-direction failure.

---

## 2. Cross-cutting checks

| Check | Evidence | Result |
|-------|----------|--------|
| AR unpublished | `/ar/tours/fixture-istanbul-package` → no FA/EN product title; fixture loader has no `ar.ts`; page `notFound()` path | **PASS** (soft-404 HTTP 200 streaming acceptable per T007) |
| Server Component First | page/view have no `"use client"`; T016 allowlist only `BookingCtaIsland` + `error.tsx` | **PASS** |
| Client boundary | `npm run test:quality` client allowlist | **PASS** |
| RTL/LTR | FA `dir=rtl` · EN `dir=ltr` on `<html>` | **PASS** |
| Bidi | `IKA → IST` / `TK875` / `USD` present without mirroring travel direction | **PASS** |
| Accessibility baseline | SkipLink present; headings hierarchy in view; focus-visible CTA; min-h-touch | **PASS** |
| Money / mixed currency | USD+IRR components; FA `irrDisplayUnit=Toman`; money unit tests PASS | **PASS** |
| Media | `MediaImage` hero + `/media/foundation-sample.png` | **PASS** |
| Sticky CTA | T014 island; presentation-only click message; no booking mutation | **PASS** |
| SEO metadata | FA/EN distinct `<title>` · canonical · hreflang fa/en | **PASS** |
| T007 route states | unknown productKey → not-found path (no product title); locale error/loading foundations unchanged | **PASS** |
| Domain ≠ navigation | AdminShell still slot-only; no domain menu freeze | **PASS** |
| Raw-FK UX | presentation keys only; no IdentityId/PartyId paste workflow | **PASS** |
| Backend authority | no pricing/booking engine in frontend | **PASS** |

---

## 3. Runtime probe log (local production server)

```text
next start -p 3021
GET /fa/tours/fixture-istanbul-package → 200 · title FA · dir=rtl · IKA · USD · canonical · hreflang · CTA
GET /en/tours/fixture-istanbul-package → 200 · title EN · dir=ltr · IKA · USD · canonical · hreflang · CTA
GET /ar/tours/fixture-istanbul-package → 200 soft not-found · no published tour title
GET /fa/tours/does-not-exist → 200 soft not-found · no published tour title
npm run quality → PASS
```

---

## 4. Known limitations (explicit)

1. T017 does **not** claim full visual pixel QA across all plan widths; matrix uses SSR HTML + CSS mobile-first architecture + quality gates.
2. Sticky CTA interaction is placeholder handoff (Walking Skeleton) — not live Booking.
3. Soft HTTP 200 on some not-found probes is known Next streaming behavior (accepted in T007).
4. Full Experience Tour / P03 features are out of scope.
5. SEO is baseline metadata only — not P05 SEO engine.

---

## 5. Gate readiness

Walking skeleton is ready for **architect review toward `TC-P02-GATE`**.  
This task does **not** execute the phase gate.
