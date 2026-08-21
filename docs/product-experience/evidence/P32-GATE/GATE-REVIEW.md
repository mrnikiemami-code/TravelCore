# TC-P32-GATE — Cursor Gate Review

| Field | Value |
|-------|--------|
| Task-ID | `TC-P32-GATE` |
| HEAD at review | `9dd9f5e` |
| Status (Cursor) | **PASS WITH KNOWN LIMITATIONS** |
| Gate question | «Can TravelCore now be demonstrated as a professional travel commerce product with honest demo data and media?» |
| Cursor answer | **Yes, with known limitations** |
| Recommendation | **ACCEPT WITH KNOWN LIMITATIONS** — photo-backed commercial demo path is live for Home destinations, Hotel discovery, and Tour discovery; not a full sellable agency showcase / not Gallery-complete for Destination |

---

## Completed P32 units (Cursor)

| Unit | Deliverable | Cursor | Evidence / artifact |
|------|-------------|--------|---------------------|
| `TC-P32-T001` | Media strategy + demo asset pack | PASS (Architect ACCEPTED WITH KNOWN LIMITATIONS earlier) | `docs/plans/P32-commercial-demo-media-strategy.md` · pack under `docs/product-experience/assets/demo-media/` |
| `TC-P32-T002` | DEMOFEED enrich-media via Media ownership | PASS | Hotel/Tour attach; Destination skipped until ownership |
| `TC-P32-T003` | Live scenario validation | PASS WITH KNOWN LIMITATIONS | `evidence/P32-T003/` — Tour photo OK; Hotel browse then blocked (EF) |
| `TC-P32-T004` | Hotel public browse EF fix | PASS | `evidence/P32-T004/` — listing + detail reachable |
| `TC-P32-T005` | Hotel listing covers + star restore | PASS | `evidence/P32-T005/` — photo cards + stars |
| `TC-P32-T006` | Destination Media ownership finding | BLOCKED (finding only) | `evidence/P32-T006/ARCHITECTURE-FINDING.md` |
| `TC-P32-T007` | Ownership decision prep | PASS | `docs/plans/P32-destination-media-ownership.md` (Option A) |
| `TC-P32-T008` | Destination Cover ownership + enrich | PASS | `evidence/P32-T008/API-NOTES.md` |
| `TC-P32-T009` | Home Destination Cover consume | PASS | `evidence/P32-T009/` — desktop Home with real destination photos |

Cursor PASS ≠ Architect ACCEPT for units still awaiting Architect review.

---

## Reviewed surfaces

### Public Experience

| Surface | Check | Evidence inspected | Cursor verdict |
|---------|-------|--------------------|----------------|
| Home destination media | Real covers vs gradient | `P32-T009/fa-home-desktop.png` · `fa-home-mobile.png` · T009 VISUAL-REVIEW | **PASS** on desktop (4 DEMOFEED destination cards with photos). Mobile capture crop may cut destination band — density PASS on desktop evidence. |
| Hotel discovery | Listing + detail media | `P32-T005/fa-hotel-listing-{desktop,mobile}.png` · `fa-hotel-detail-{desktop,mobile}.png` · T004/T005 reviews | **PASS** — 2 hotels with covers; detail cover/gallery present; stars restored on listing/detail |
| Tour discovery | Listing + detail media | `P32-T003/fa-tour-listing-{desktop,mobile}.png` · `fa-tour-detail.png` | **PASS** — DEMOFEED tour with real cover |
| Commercial density | Marketplace chrome + catalog | Home/Hotel/Tour heroes + cards | **PASS direction** — commercial labeling, DEMOFEED honesty badges, CTAs |
| Mobile/Desktop quality | Responsive captures | paired d/m evidence across T003–T009 | **PASS** (captures present; some viewport crops) |

### Data and Media

| Check | Verdict | Notes |
|-------|---------|-------|
| Destination Cover ownership | **PASS** | Option A: `destination.destination_media_links` · opaque `MediaAssetId` · Cover 0..1 · presentation endpoints (T008) |
| Hotel media | **PASS** | Place Cover + Gallery via Media ownership; listing compose fixed (T005) |
| Tour media | **PASS** | Tour Cover Ready; listing/detail consume (T002/T003) |
| DemoFeed boundaries | **PASS** | Tool-side enrich; removable; DEMOFEED badges visible; no Api registration of DemoFeed |
| No fake commerce | **PASS** | No fake prices/availability/reviews; honest catalog copy |

### Architecture

| Check | Verdict | Notes |
|-------|---------|-------|
| Media technical ownership | **PASS** | Upload/storage/content URLs remain Media; domains store opaque asset ids |
| Domain semantic ownership | **PASS** | Place / Tour / Destination Cover links own roles |
| DemoFeed removable | **PASS** | Feeder remains tools/demofeed; not production domain |
| No Booking/Pricing/HotelBooking changes | **PASS** (gate scope) | Review-only gate; P32 delivery path did not introduce Pricing/Booking engines |

---

## Overall commercial readiness

```text
P30 Product Experience Foundation     ✅ ACCEPTED
DEMOFEED Data Enablement              ✅ GATE ACCEPTED
P31 Commercial Experience UX          ✅ ACCEPTED WITH KNOWN LIMITATIONS
P32 Media Enrichment + ownership      ✅ Cursor READY (awaiting Architect GATE)
Honest photo-backed demo path         ✅ Home Dest Cover · Hotel · Tour
Destination Gallery parity            ⚠️ DEFERRED (Option A)
Full sellable agency photo density    ⚠️ PARTIAL vs North Star maximum
```

**Gate question answer:** TravelCore **can** be demonstrated as a professional travel commerce product with **honest** demo data and media on the primary Public surfaces (Home destinations, Hotels, Tours), provided DEMOFEED is seeded/enriched and Media blobs are available to the Api runtime.

---

## Evidence reviewed

- `docs/product-experience/evidence/P32-T003/VISUAL-REVIEW.md` (+ screenshots)
- `docs/product-experience/evidence/P32-T004/VISUAL-REVIEW.md` (+ screenshots)
- `docs/product-experience/evidence/P32-T005/VISUAL-REVIEW.md` (+ screenshots)
- `docs/product-experience/evidence/P32-T008/API-NOTES.md`
- `docs/product-experience/evidence/P32-T009/VISUAL-REVIEW.md` (+ screenshots)
- `docs/product-experience/evidence/P32-T006/ARCHITECTURE-FINDING.md`
- `docs/plans/P32-commercial-demo-media-strategy.md`
- `docs/plans/P32-destination-media-ownership.md`
- `docs/PROJECT-STATE.md` · `docs/ROADMAP.md` · `docs/ai/TRAVELCORE-RECOVERY-CONTEXT.md`

### Visual rollup (inspected screenshots)

| Dimension | Verdict |
|-----------|---------|
| North Star direction | **PASS direction** — marketplace chrome + photo cards; not pixel-clone of North Star |
| Professional travel commerce feeling | **PASS** |
| Responsive quality | **PASS** (d/m evidence present) |
| Honesty of data | **PASS** — DEMOFEED labeled; no fake prices/inventory |

---

## Accepted limitations

1. **Destination Gallery** not implemented (Option A Cover-only by design).
2. **Hotel detail gallery** has fewer images than UI slots (pack density).
3. **Tour listing** remains destination-scoped (no global tour browse).
4. **Pricing / Booking / availability** engines intentionally absent.
5. **Runtime media blob sync**: demofeed tool FS → Api `.local/media-objects` required for live demos (env, not product architecture).
6. Some screenshots include Next.js / extension chrome; mobile Home crop may omit destination band.
7. Individual P32 unit Architect ACCEPT may still be pending for T002–T009.

## Remaining blockers

| Item | Blocking commercial demo with media? |
|------|--------------------------------------|
| Destination Cover ownership | **No** — delivered in T008/T009 |
| Hotel public browse EF | **No** — fixed in T004 |
| Destination Gallery | **No** for Cover-led Home demo — deferred product scope |
| Fake commerce pressure | **No** — honesty preserved |

No open architecture blocker that reverts the gate question to “cannot demonstrate.”

## Acceptance risks

1. Architect may require Destination Gallery before calling P32 “media-complete.”
2. Architect may require fresher mobile Home evidence showing destination covers in-frame.
3. Architect may treat pending unit ACCEPTs (T002–T009) separately from GATE ACCEPT.
4. Strict North Star photography bar may still rate overall density PARTIAL.

## Recommendation

**ACCEPT WITH KNOWN LIMITATIONS.**

Do **not** invent next phase / polish tasks from this review. After Architect decision, wait for the next authorized `.task.md` / `.gate.md` only.

## Product code

No product code changes in this gate (docs / evidence / SoT sync only).
