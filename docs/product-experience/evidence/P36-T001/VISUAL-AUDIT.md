# P36-T001 — Visual / Commercial Sellability Audit

| Field | Value |
|-------|--------|
| Task-ID | `TC-P36-T001` |
| Date | 2026-08-21 |
| Runtime | Next `http://localhost:3000` · API restarted for catalog screenshots |
| Verdict | **`NOT_SELLABLE_VISUALLY`** |

## Sales-demo gate question

**Would a prospective travel-business customer pay for this product based on what they see?**  
**No.** Architecture honesty is present; premium travel marketplace perception is not.

Screenshots were captured and inspected before writing this audit.

## Evidence files

| File | Content |
|------|---------|
| `home-desktop.png` | Home hero + destinations (flat blue; category gradients) |
| `home-mobile.png` | Home @ ~390px |
| `hotels-listing-desktop.png` | Hotels catalog · **search value `undefined`** · DEMOFEED badges |
| `hotel-detail-desktop.png` | Hotel detail gallery + honesty CTA overlay |
| `tours-listing-desktop.png` | Tours after destination slug · DEMOFEED card |
| `tour-detail-desktop.png` | Tour detail hero/gallery |
| `commerce-hotel-book-desktop.png` | Hotel prepare-booking form (admin-like) |

## A. First impression / brand

| Criterion | Assessment |
|-----------|------------|
| Premium travel feeling | **Fail** — solid blue slab vs photographic North Star |
| Visual hierarchy | Partial — clear type scale, weak merchandising |
| Hero impact | **Weak** — no destination photography |
| Brand consistency | Partial — blue/orange present; not Iran Sans / DS 2.0 richness |
| Photography quality | Mixed — some DEMOFEED photos OK; many gradients/placeholders historically |
| Spacing / whitespace | Acceptable but sparse “product empty” feel |
| Trust / maturity | Honesty banners dominate over desirability |

## B. Home

- Looks like a polished **engineering demo**, not Booking.com/Expedia-class marketplace.
- Featured sections recover when API is up, but cards scream **DEMOFEED sample**.
- Destination strip uses abstract gradients, not city photography grids from North Star.
- Search widget is honest (“not a fake booking engine”) — good integrity, bad sales theater.

## C. Hotels

- Listing works with API: 2 cards, real images, star text.
- **P0 bug:** search input shows literal **`undefined`**.
- DEMOFEED badges on every card destroy agency sales credibility.
- Detail: large gallery photo good; facilities empty; reviews empty; sticky honesty overlay feels like scaffolding.

## D. Tours

- Default `/tours` requires destination slug — not a browsable marketplace first screen.
- With `demofeed-tehran`: one card with photo — OK.
- Detail: gallery strong; destination list shows **raw UUID**; price/departure UI functional but dense; bottom action rail utilitarian.

## E. Commerce

- Hotel `/book` is a **plain white form** — looks like internal tooling / admin CRUD, not traveler checkout theater.
- No product imagery, no stay summary card, no premium progress chrome.
- Honesty copy is correct; visual sellability is near zero.

## F. Mobile

- Layout stacks; CTAs usable.
- Same premium gap; Workspace chip orange is loud vs North Star balance.
- Next.js “1 Issue” overlay appears in captures (dev artifact — hide for demos).

## G. Design-system consistency

- Flat hero blues vs photographic North Star.
- Cards / radius / elevation mostly consistent but under-designed.
- Filter cards feel generic SaaS, not travel editorial.
- Commerce page abandons marketplace chrome entirely.

## H. North Star gaps (largest)

1. No photographic hero / destination mosaic  
2. No dense product merchandising with price chips / ratings theater (honest data only — still needs beautiful empty/filled states)  
3. DEMOFEED labeling everywhere  
4. Commerce not productized  
5. Typography/color not matching Iran Sans + accent system richness  

## Top 10 visual blockers

| # | Blocker | Priority |
|---|---------|----------|
| 1 | Flat blue heroes; no photo-led marketplace first impression | **P0** |
| 2 | DEMOFEED / sample badges dominate public UI | **P0** |
| 3 | Hotel search input renders `undefined` | **P0** |
| 4 | Commerce booking pages look like admin forms | **P0** |
| 5 | Tours default empty without destination slug (non-marketplace) | **P1** |
| 6 | Empty facilities/reviews/stories states look unfinished, not premium | **P1** |
| 7 | Raw UUIDs / internal codes leak into traveler copy | **P1** |
| 8 | Destination cards as gradients vs photography | **P1** |
| 9 | Inconsistent marketplace chrome (home vs commerce) | **P1** |
| 10 | Dev overlays (Next issue badge) visible in demo | **P2** |

## Do-not-regress rules (audit reminder)

No fake prices · no fake availability · no fake reviews · no fake payment success · preserve RTL/mobile/a11y · preserve architecture boundaries.

## Governance note

46 inbox transport stubs remain tracked (T010 audit) — **cleanup deferred** (not in P36-T001 scope).
