# TC-P32-T004 — Hotel Public Browse Fix

| Field | Value |
|-------|--------|
| Task-ID | `TC-P32-T004` |
| Status (Cursor) | **PASS** |
| Root cause | EF Core could not translate `SelectMany` projecting `PlaceId.Value` / owned `Hotel` inside public hotel browse (+ similar full-Place projection in `FindBySlugAsync`) |
| Fix | Rewrite queries to project `PlaceId` struct + translation scalars only; omit owned Hotel join from browse |

## Changed files

- `src/backend/Modules/Place/.../Services/PlacePublicQuery.cs`
- `src/backend/Modules/Place/.../Services/PlaceApplicationService.cs` (`FindBySlugAsync`)

## API validation

| Call | Result |
|------|--------|
| `GET /api/place/public/hotels?localeCode=fa` | **200** · 2 DEMOFEED hotels |
| `GET /api/place/places/by-slug/fa/demofeed-hotel-tehran-1` | **200** |
| Place media presentation (prior T002) | Cover + Gallery Ready |

## Screenshots

- `fa-hotel-listing-desktop.png` / `fa-hotel-listing-mobile.png` — 2 DEMOFEED hotels listed
- `fa-hotel-detail.png` — Tehran hotel detail with enriched cover photo

## Visual self-review

| Dimension | Verdict |
|-----------|---------|
| Hotel listing reachable | **PASS** |
| Hotel detail reachable | **PASS** |
| Enriched media on detail | **PASS** |
| Listing card covers | **PARTIAL** — cards may still show gradient while detail consumes Media (enrich path separate) |
| Fake commerce | **PASS** — none |

## Known limitations

1. Browse `StarRating` temporarily null in public list DTO (owned Hotel join avoided for translation safety).
2. Destination media ownership still missing.
3. Listing card cover enrichment may still lag/fail independently of browse fix.

## Acceptance risks

Architect may ask to restore star ratings via a second translatable query, or improve listing cover enrichment.

## Recommended next (Architect file only)

`TC-P32-GATE` or Destination Media ownership — do not invent.
