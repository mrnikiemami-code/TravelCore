# P30-T007 Visual Review Notes

| Field | Value |
|-------|--------|
| Task-ID | `TC-P30-T007` |
| Surfaces | `/[locale]/tours` listing · `/[locale]/tours/[slug]` detail |
| Evidence | `fa-tours-listing-desktop.png` · `fa-tours-listing-mobile.png` · `fa-tours-listing-destination-desktop.png` · `fa-tour-detail-notfound-desktop.png` · `fa-tour-detail-notfound-mobile.png` |

## Implementation summary

### Listing
- PublicShell + PublicHeader / PublicFooter
- Destination + name filter + sort toolbar (primary Apply)
- Discovery cards (gradient media placeholder, title, kind, code, CTA)
- Loading route skeleton
- Honest states: needs-destination · empty · error (API/destination failure)
- Data contract: destination-scoped `related-published` only (no global browse API)

### Detail
- Gallery/hero pattern + honest empty media
- Summary, destinations (count, not GUID-first UX), Experience itinerary when kind=Experience
- Departures as surfaces: schedule, package transport chips, stay nights, MoneyText prices
- Trust block + sticky actions (accent prepare booking CTA → existing `/book`)
- Soft missing-tour panel inside PublicShell

## Visual self-review

| Check | Assessment |
|-------|------------|
| North Star | Marketplace chrome + primary/accent CTAs align directionally; not pixel clone |
| Travel-commerce feeling | Improved vs prior P14 skeleton; still limited without live catalog/media |
| Hero/media richness | Placeholder/empty honest states when no media; live gallery when API returns media |
| Hierarchy | Title → toolbar → states / sections clear on listing; gallery → summary → departures → price on detail |
| Tour cards | Present when destination returns related tours; this session showed needs-destination / API error only |
| Itinerary | Experience sections reused; Package specialty itinerary not invented |
| Hotel/flight | Honest package transport/stay chips; not live Flight inventory |
| Price/CTA | MoneyText wired for honest Pricing summaries; accent booking prepare CTA |
| Desktop/mobile | 1440 + 390 captures refreshed |
| RTL | FA listing/detail chrome RTL acceptable |
| Defects | Destination filter still requires known published slug; API down → error (honest) |

## Known limitations

1. No global public tour browse API — listing requires destination.
2. Place/Tour APIs unavailable/failed locally for `istanbul` → error/empty evidence, not success card grid with live prices.
3. RelatedTourView has no image/price fields — cards cannot show invent prices/photos.
4. Live tour detail with gallery/itinerary/prices not captured in this run (missing slug / API).
5. Next.js dev indicator may appear in local screenshots.

## Acceptance risks

1. Architect may REWORK until live destination-backed cards + detail success screenshots exist.
2. Destination-slug discovery may feel less “marketplace browse” than North Star until a browse contract exists.
3. Package itinerary depth remains Experience-section based; Package specialty not fabricated.

## Architect gate

Visual ACCEPT still required (Cursor PASS ≠ Architect ACCEPT).
