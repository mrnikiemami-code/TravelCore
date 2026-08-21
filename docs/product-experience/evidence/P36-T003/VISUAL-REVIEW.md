# P36-T003 — Hotel Visual Review

| Field | Value |
|-------|--------|
| Task-ID | `TC-P36-T003` |
| Date | 2026-08-21 |
| Verdict | **`PARTIALLY_SELLABLE_VISUALLY`** |

## Defects fixed

- Search `defaultValue` / query sanitization — no visible **`undefined`/`null`**
- Removed giant empty Facilities / Reviews blocks when data absent
- Hid noise DEMOFEED description copy from cards/detail body
- Softened sample chips; removed code/UUID from primary hotel chrome
- Aligned listing/detail heroes with Home photographic direction

## What changed

- Listing: photo hero, cleaner toolbar, commercial cards
- Detail: photo hero + gallery, omit empty sections, honest sticky CTA rail
- HotelCard: quieter demo chip + filtered descriptions

## Before → after

Evidence files under this folder (`*-before.png` from T001 · `*-after.png` captured here).

## Remaining weaknesses

- Product titles still contain “DEMOFEED Sample …” (catalog naming)
- Destination names similarly demo-prefixed
- Commerce `/book` form still admin-like (T005)
- Full SELLABLE needs Tour polish + commerce polish

## Mobile

390px: hero readable, CTA sticky usable, cards stack — intentional.

## Accessibility / performance

Semantic headings retained · Server Component First · no fake commerce data.
