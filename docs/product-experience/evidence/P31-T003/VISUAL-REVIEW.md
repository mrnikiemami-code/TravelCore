# TC-P31-T003 — Visual Review (Public Home Commercial Upgrade)

| Field | Value |
|-------|--------|
| Task-ID | `TC-P31-T003` |
| Locale primary | `fa` RTL |
| Capture | Playwright Chromium |
| Cursor visual inspection | **DONE** (desktop + mobile + full-page) |

## Evidence paths

| Capture | Path |
|---------|------|
| Desktop viewport | `fa-home-desktop.png` |
| Desktop full page | `fa-home-desktop-full.png` |
| Mobile viewport | `fa-home-mobile.png` |
| Mobile full page | `fa-home-mobile-full.png` |
| Tablet viewport | `fa-home-tablet.png` |

## Visual review summary

### Improved vs P30 foundation

1. **Hero** — stronger marketplace framing (“سفر را حرفه‌ای شروع کنید”), dual CTAs (Hotels + Tours), honest search disclaimer.
2. **Commercial conversion** — agency-demo CTA band near footer (plan + hotels).
3. **Trust strip** — reframed around real catalog / honest commerce boundaries / agency demo / no fake claims.
4. **Live composition wiring** — Home loader now attempts DEMOFEED destination slugs + destination-scoped published tours + denser hotel preview limit (6).
5. **FA RTL** — layout remains RTL-correct on desktop and mobile.

### Observed runtime state (operator local)

At capture time, Place/Destination/Tour public APIs did **not** return DEMOFEED rows to the Next.js app (API connectivity / env), so:

- Destinations band → honest **discovery fallback cards** (tours/hotels/plan/stories entry)
- Tours band → honest **premium empty** (no fake tour inventory)
- Hotels band → honest **ready empty / skeleton** (no fake rates)
- Stories → honest empty

This is architecturally correct for P31 honesty rules. Density will jump when public APIs serve the seeded `demofeed-*` catalog.

### North Star comparison

| Dimension | Verdict |
|-----------|---------|
| Marketplace first impression | **IMPROVED** (commercial copy + CTAs) · still gradient-led vs photo-rich North Star |
| Search prominence | **PASS** for intent UI · not a fake booking engine |
| Catalog density | **PARTIAL** — composition ready; live DEMOFEED not visible in this capture environment |
| Trust / conversion | **PASS** foundation for agency demo narrative |
| Mobile / RTL | **PASS** |
| Fake commerce | **NONE** (no prices / availability / reviews invented) |

## Known limitations

1. No real destination photography yet (content strategy T002 media pack not executed).
2. Live DEMOFEED catalog not visible in evidence capture environment (API/env).
3. Tour listing remains destination-scoped at API layer — Home aggregates candidate city slugs only.
4. Hotel cards still gradient placeholders (browse DTO lacks cover media fields).

## Acceptance risks

1. Architect may require a **live DEMOFEED success screenshot** (API up + seeded DB) before ACCEPT.
2. Architect may require media covers on Home cards before treating commercial density as sufficient.
3. Gradient-heavy destinations/tours may still feel below North Star photography bar.

## Cursor recommendation

**PASS with known limitations** — commercial Home experience upgrade delivered; honesty preserved; live catalog composition ready pending API/media.
