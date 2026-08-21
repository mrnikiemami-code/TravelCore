# TC-P31-T004 — Visual Review (Hotel Commerce Polish)

| Field | Value |
|-------|--------|
| Task-ID | `TC-P31-T004` |
| Locale primary | `fa` RTL |
| Capture | Playwright Chromium |
| Cursor visual inspection | **DONE** |

## Evidence paths

| Capture | Path |
|---------|------|
| Listing desktop | `fa-hotels-listing-desktop.png` |
| Listing mobile | `fa-hotels-listing-mobile.png` |
| Detail desktop | `fa-hotel-detail-desktop.png` |
| Detail mobile | `fa-hotel-detail-mobile.png` |

## Visual review summary

### Listing

- **Hotel Commerce hero** — Deep Ocean + Warm Gold accent; clear marketplace framing; honesty blurb (no fake prices/inventory/ratings).
- **Filters/sort** — usable toolbar retained; Apply CTA clear.
- **Runtime at capture** — Place public browse API failed → **premium honest error** with retry (no fake hotel cards).
- **FA RTL** — PASS.

### Detail

- Attempted DEMOFEED slug `demofeed-hotel-tehran-1`.
- Runtime at capture → **honest missing hotel** surface (shell + clear copy + back CTA).
- Confirms no invented hotel/price/availability when API/catalog miss.
- Note: when API serves Active hotels, UI includes hero gallery layout, facilities/location grid, trust band, HotelCard similar grid, sticky booking-path CTA.

### Commercial polish delivered (code)

1. Image-forward cards when Place media presentation returns cover (frontend enrichment).
2. Wider commercial listing chrome + denser cards.
3. Detail hero/gallery composition + trust section + similar hotels via `HotelCard`.
4. Honesty preserved across empty/error/missing.

## Known limitations

1. Capture environment could not load live hotel browse/detail catalog (API/env) — success grids not evidenced in screenshots.
2. DEMOFEED media may still be synthetic 1×1 when API is up.
3. Booking CTA remains future-path entry (no HotelBooking mutation).
4. Browse DTO still lacks native cover — enrichment is N+1 presentation calls (frontend-only).

## Acceptance risks

1. Architect may require **live success screenshots** (listing + detail with DEMOFEED hotels) before ACCEPT.
2. Architect may require richer photography pack before treating Hotel commerce as sellable.

## Cursor recommendation

**PASS with known limitations** — commercial Hotel listing/detail experience upgraded; honesty preserved; live catalog evidence blocked by API connectivity at capture time.
