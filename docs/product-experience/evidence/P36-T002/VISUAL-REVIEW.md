# P36-T002 — Home Visual Review

| Field | Value |
|-------|--------|
| Task-ID | `TC-P36-T002` |
| Date | 2026-08-21 |
| Verdict | **`PARTIALLY_SELLABLE_VISUALLY`** |

## Sales-demo question (Home only)

Home is no longer primarily an engineering flat-blue demo. It is **not yet** agency-sale ready alone — DEMOFEED product names/descriptions remain catalog truth, and Hotel/Tour/Commerce polish still pending.

## What changed

- Photographic hero using owned Destination/Hotel/Tour cover media (fallback gradient only if none)
- Search card redesigned: destination + Hotels/Tours intent (no fake date/availability engine)
- Destination cards image-led with overlay titles; subtle “Sample catalog” chips
- Hotels/Tours reuse commercial `HotelCard` / `TourCard` with cover enrichment on Home composition
- Trust copy updated (still truthful; no fake counts)
- Demo badges demoted (corner glass chips, not large DEMOFEED plaques)

## Visible before → after

| | Before (T001) | After (T002) |
|--|---------------|--------------|
| Hero | Flat blue gradient | Travel photography + navy overlay |
| Destinations | Gradient tiles / weak | Photo cards with overlay |
| Hotels/Tours | Sparse cards without covers on Home | Cover-enriched commercial cards |
| Demo labels | Dominant DEMOFEED plaques | Subtle Sample catalog chips |
| Search | Date/guests form | Honest explore + intent |

Evidence: `home-desktop-before.png` · `home-desktop-after.png` · `home-mobile-before.png` · `home-mobile-after.png` · `home-tablet-after.png`

## Remaining weaknesses

- Product titles still contain “DEMOFEED Sample …” (data provenance — not renamed here)
- Hotel descriptions still say “DEMOFEED sample data — non-production”
- Stories empty state still soft
- Header Workspace orange remains loud vs North Star balance
- Full SELLABLE needs Hotel/Tour/Commerce polish (T003+)

## Comparison to North Star

Closer: photo hero, blue/amber CTA language, merchandising density.  
Still short: editorial mosaic richness, Iran Sans polish, non-demo catalog naming, denser marketplace theater.

## Accessibility / performance notes

- Semantic headings retained; focus rings on controls
- Server Component First retained; covers via existing app-proxy URLs
- No autoplay video; no large decorative JS libraries
