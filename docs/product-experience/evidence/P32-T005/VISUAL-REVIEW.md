# TC-P32-T005 — Hotel Presentation Completeness

| Field | Value |
|-------|--------|
| Task-ID | `TC-P32-T005` |
| Status (Cursor) | **PASS** |
| HEAD | see RESULT commit |

## Implementation summary

1. **Listing cover consumption** — `enrich-hotel-covers.ts` now reads Place media presentation compose shape (`cover.presentation.originalContentUrl` / variants), matching Tour enricher. Root cause of listing gradients after T004.
2. **StarRating restore** — separate Place query using `placeIds.Contains(p.Id)` (PlaceId struct) after browse SelectMany; avoids EF translation regression from T004.

## Changed files

- `src/frontend/web/src/features/hotel-discovery/enrich-hotel-covers.ts`
- `src/backend/Modules/Place/.../Services/PlacePublicQuery.cs`
- `docs/product-experience/evidence/P32-T005/**`
- SoT sync docs

## Evidence

- `fa-hotel-listing-desktop.png` / `fa-hotel-listing-mobile.png` — enriched covers on both DEMOFEED cards
- `fa-hotel-detail-desktop.png` / `fa-hotel-detail-mobile.png` — cover + gallery slot
- `API-NOTES.md`

## Visual self-review

| Dimension | Verdict |
|-----------|---------|
| Listing covers | **PASS** (real photos) |
| Detail cover/gallery | **PASS** (cover + ≥1 gallery; extra slots empty OK) |
| Star presentation | **PASS** API stars 4/5; detail shows ۴ ستاره |
| Fake commerce | **PASS** |
| EF safety | **PASS** — browse still 200 |

## Known limitations

1. Destination media ownership still missing.
2. Detail gallery has fewer images than UI slots (pack has 1 gallery per hotel).
3. Listing star UI may still be card-chrome dependent (API returns stars).

## Acceptance risks

Architect may still require Destination Media before GATE.

## Recommended next (Architect file only)

`TC-P32-GATE` or Destination Media ownership.
