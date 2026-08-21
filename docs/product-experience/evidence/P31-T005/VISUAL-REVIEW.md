# TC-P31-T005 — Visual Review (Tour Commerce Polish)

| Field | Value |
|-------|--------|
| Task-ID | `TC-P31-T005` |
| Locale primary | `fa` RTL |
| Capture | Playwright Chromium |
| Cursor visual inspection | **DONE** |

## Evidence paths

| Capture | Path |
|---------|------|
| Listing desktop | `fa-tours-listing-desktop.png` |
| Listing mobile | `fa-tours-listing-mobile.png` |
| Listing + destination (extra) | `fa-tours-listing-dest-desktop.png` |
| Detail desktop | `fa-tour-detail-desktop.png` |
| Detail mobile | `fa-tour-detail-mobile.png` |

## Visual review summary

### Listing (`/fa/tours`)

- **Tour Commerce hero** — Deep Ocean + Warm Gold accent; clear marketplace framing; honesty blurb (no fake prices/inventory/sales claims).
- **Filters** — destination-scoped toolbar retained; placeholder points to `demofeed-tehran`; Apply CTA clear.
- **Needs-destination state** — premium split card (gradient media plane + honest copy) instead of bare dashed empty.
- **FA RTL** — PASS (desktop + mobile).
- **Mobile** — hero + toolbar stack cleanly; tap targets adequate.

### Listing with destination (`?destination=demofeed-tehran`)

- Hero shows marketplace hint with destination label.
- Runtime at capture → **premium honest error** (destination/API miss) with retry — no fake tour cards.

### Detail (`/fa/tours/demofeed-tour-tehran-1`)

- Attempted DEMOFEED slug `demofeed-tour-tehran-1`.
- Runtime at capture → **honest missing tour** surface (shell + clear copy + back CTA).
- Confirms no invented tour/price/availability when API/catalog miss.
- Note: when API serves Published tours, UI includes commercial hero band, hero/gallery composition, destinations/included grid, Experience itinerary polish, trust band, sticky prepare actions.

## Commercial polish delivered (code)

1. Image-forward cards when Tour media presentation returns cover (frontend enrichment).
2. Wider commercial listing chrome + denser cards + destination marketplace framing.
3. Detail hero/gallery composition + destinations/included presentation + trust honesty + Experience itinerary surfaces.
4. Honesty preserved across needs-destination / empty / error / missing.

## Known limitations

1. Capture environment could not load live destination-scoped tour list or demofeed tour detail (API/env) — success grids not evidenced in screenshots.
2. DEMOFEED media may still be synthetic 1×1 when API is up.
3. Booking CTA remains prepare-path entry (no Booking/Payment mutation; no invented prices).
4. Related-tour DTO still lacks native cover — enrichment is N+1 presentation calls (frontend-only).
5. Tour listing remains destination-scoped (no global browse index on this layer).

## Acceptance risks

1. Architect may require **live success screenshots** (listing with DEMOFEED destination + detail with cover) before ACCEPT.
2. Architect may require richer photography pack before treating Tour commerce as sellable.

## Cursor recommendation

**PASS with known limitations** — commercial Tour listing/detail experience upgraded to Hotel polish bar; honesty preserved; live catalog evidence blocked by API connectivity at capture time.
