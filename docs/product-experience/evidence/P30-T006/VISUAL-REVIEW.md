# P30-T006 Visual Review Notes

| Field | Value |
|-------|--------|
| Task-ID | `TC-P30-T006` |
| Surfaces | `/[locale]/hotels` listing · `/[locale]/hotels/[slug]` detail |
| Evidence | `fa-hotels-desktop.png` · `fa-hotels-mobile.png` |

## What was implemented (experience only)

### Listing
- PublicShell with PublicHeader / PublicFooter
- Filter pattern (name/description query) + sort pattern
- Hotel card grid (image area placeholder, name, location/blurb, CTA)
- Loading route skeleton
- Honest **error** and **empty** states (no invented prices/availability)

### Detail
- HotelDetailView: gallery pattern, summary, facilities, location, reviews pattern (UGC or honest empty), similar hotels pattern, sticky future-booking CTA
- PublicShell chrome aligned with Home commerce shell

## Visual capture notes

Desktop + mobile captures of `/fa/hotels` were taken with backend Place API **unavailable** in the local session.

Observed: listing UI + filter/sort + **error state** rendered correctly (honest failure, no fake catalog).

Detail screenshots with live Place data were **not** captured in this run because the public hotels API did not respond. Detail UI is implemented in code and typechecks.

## Do not invent

No fake prices, availability, reviews, or ratings were introduced.

## Architect gate

Visual ACCEPT still required (Cursor PASS ≠ Architect ACCEPT).
