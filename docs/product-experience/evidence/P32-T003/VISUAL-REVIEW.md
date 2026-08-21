# TC-P32-T003 — Live Commercial Demo Scenario Validation

| Field | Value |
|-------|--------|
| Task-ID | `TC-P32-T003` |
| Phase | P32 — Commercial Demo Data & Media Enrichment |
| Cursor Status | **PASS WITH KNOWN LIMITATIONS** / **PARTIAL** vs full sellable photo-dense journey |
| HEAD | see RESULT commit |
| Question | «آیا TravelCore الان قابل ارائه به‌عنوان محصول گردشگری حرفه‌ای است؟» |
| Cursor answer | **بله، با محدودیت‌های شناخته‌شده** — مسیر تور DEMOFEED با cover واقعی قابل دموست؛ مسیر هتل public browse هنوز شکست می‌خورد (باگ EF از قبل)؛ مقصدها هنوز بدون Media مالک |

## Scenario journey

| Step | Surface | Evidence | Verdict |
|------|---------|----------|---------|
| Home desktop/mobile | Commercial marketplace | `fa-home-desktop.png` · `fa-home-mobile.png` | **PASS direction** · destination cards still **gradient** (no Destination↔Media) |
| Destination | Home destination band | same | **PARTIAL** — DEMOFEED labels present, no destination photos |
| Hotel listing d/m | `/fa/hotels` | `fa-hotel-listing-*.png` | **FAIL live success** — honest error («بارگذاری فهرست هتل‌ها ناموفق») due to `/api/place/public/hotels` EF LINQ 500 |
| Hotel detail | `/fa/hotels/demofeed-hotel-tehran-1` | `fa-hotel-detail.png` | **FAIL live success** — honest «هتل پیدا نشد» (depends on public browse/slug path) |
| Tour listing d/m | `/fa/tours?destination=demofeed-tehran` | `fa-tour-listing-*.png` | **PASS** — 1 DEMOFEED tour card with **real cover photo** |
| Tour detail | `/fa/tours/demofeed-tour-tehran-1` | `fa-tour-detail.png` | **PASS** — enriched cover rendered in gallery |

## API-layer media verification (T002 consumption)

| Check | Result |
|-------|--------|
| Place media presentation (hotel Istanbul/Tehran) | Cover + Gallery Ready · alt fa published |
| Tour media presentation (Tehran/Istanbul) | Cover Ready · alt fa published |
| `GET /api/media/assets/{id}/content` | **200** image/png (blobs synced to Api `.local/media-objects`) |
| Byte evidence files | `api-hotel-teh-cover.png` · `api-hotel-teh-gallery.png` · `api-tour-teh-cover.png` · `api-tour-ist-cover.png` |

## Visual self-review

| Dimension | Verdict |
|-----------|---------|
| Commercial feeling (chrome / honesty) | **PASS** |
| Photo-dense sellable demo | **PARTIAL** — Tour success yes · Hotel public path no · Destination covers no |
| Mobile readiness | **PASS** (captures present) |
| FA RTL | **PASS** |
| Fake commerce | **PASS** — no fake prices/availability; honest errors |
| Architecture | **PASS** — no unauthorized Destination Media invention |

## Known limitations

1. **Destination media attach** still missing (Architectural Concern from T002).
2. **Public hotel browse** (`GET /api/place/public/hotels`) throws EF translation `InvalidOperationException` — pre-existing; **out of T003 scope** (validation only).
3. Hotel detail public success not evidenced because listing/slug path fails.
4. Home destination band remains gradient-led.
5. Feeder media blobs live under demofeed tool FS; Api demo required copy into `TravelCore.Api/.local/media-objects` (runtime env, not product code).
6. Next.js / extension chrome may appear in screenshots.

## Acceptance risks

1. Architect may require a follow-up task to fix Place public browse LINQ before full Hotel success evidence.
2. Architect may authorize Destination↔Media ownership before treating Home as photo-complete.
3. Strict North Star photography bar still PARTIAL overall.

## Recommended next (Architect file only)

- Fix / authorize Place public hotel browse regression, **or**
- Destination Media ownership decision, **or**
- `TC-P32-GATE`

Do **not** invent tasks from this review.
