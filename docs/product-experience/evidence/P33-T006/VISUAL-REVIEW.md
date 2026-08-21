# TC-P33-T006 — Visual Review (I2 Public Tour Commerce Composition)

| Field | Value |
|-------|--------|
| Task-ID | `TC-P33-T006` |
| Locale primary | `fa` RTL |
| Scenario | `demofeed-tour-tehran-1` / code `demofeed-tour-teh-1` |
| Capture | Playwright Chromium |
| Cursor visual inspection | **DONE** |
| Status (Cursor) | **PASS** |

## Evidence paths

| Capture | Path |
|---------|------|
| Detail commerce desktop | `fa-tour-detail-commerce-desktop.png` |
| Detail commerce mobile | `fa-tour-detail-commerce-mobile.png` |

URL: `http://localhost:3000/fa/tours/demofeed-tour-tehran-1`

## Implementation summary

1. **TourCommercePanel** — selectable Published `TourDeparture` list (radio) from product-scoped published API composition.
2. **Price summary** — selected departure loads / shows Pricing public summary only (`GET /api/pricing/public/tour-departures/{id}`); honest empty when missing.
3. **Booking-boundary CTA** — disabled “ادامه به‌سوی رزرو · بعداً”; explicitly does **not** call Booking APIs.
4. Sticky actions remain presentation-only (departures / price / request) — **no** prepare-booking link on I2.

## Visual self-review

| Dimension | Verdict |
|-----------|---------|
| Departure selection visible | **PASS** — one Published DEMOFEED departure selected |
| Price from Pricing | **PASS** — از ۱٬۲۹۰ USD · بزرگسال/دوتخته · پایه |
| Fake / hardcoded IDs | **PASS** — no FE hardcodes; IDs from APIs |
| Booking create / Payment | **PASS** — boundary copy + disabled CTA |
| RTL / FA | **PASS** desktop + mobile |
| Mobile composition | **PASS** — panel stacks; sticky presentation actions |

## Known limitations

1. Single DEMOFEED priced departure (I1 data scope).
2. Booking initiation UX is **I3** — intentionally out of scope.
3. Sticky mobile bar can overlap lower CTA; copy still visible above.

## Acceptance risks

None observed for I2 scope. Architect may want I3 next for Pending initiation.
