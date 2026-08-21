# P36-T004 — Tour Visual Review

| Field | Value |
|-------|--------|
| Task-ID | `TC-P36-T004` |
| Date | 2026-08-21 |
| Verdict | **`PARTIALLY_SELLABLE_VISUALLY`** |

## Technical leakage removed

- Destination UUID chips removed from Tour detail (was raw `destinationIds`)
- Origin destination ID no longer shown
- Tour code removed from card footer / hero primary chrome
- Listing no longer asks travelers to type `demofeed-*` slugs

## What changed

- Listing: photo hero, destination **select** with human labels (Istanbul/Tehran/…), quick-pick chips, commercial TourCards
- Cards: image-led, kind chip, no mono code footer
- Detail: photo hero + gallery, destination count without IDs, quieter empty sections
- Commerce panel: less engineering wording around departures

## Before → after

See `*-before.png` (from T001) and `*-after.png` in this folder.

## Remaining weaknesses

- Product titles still contain “DEMOFEED Sample …”
- Sticky action rail still somewhat utilitarian
- Commerce `/book` still pending T005 polish
- Grammar “1 tours” minor copy niggle

## Mobile

390px: destination select usable, cards/CTA readable.

## Accessibility / performance

Semantic headings retained · Server Component First · no fake prices/availability.
