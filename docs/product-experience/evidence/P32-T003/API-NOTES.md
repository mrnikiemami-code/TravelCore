# P32-T003 — API media consumption notes

Captured against local Api `http://localhost:5275` + DB `TravelCore` after TC-P32-T002 enrichment.

## Hotels

- `demofeed-hotel-teh-1` / `demofeed-hotel-ist-1`: Cover + Gallery linked via Place ownership.
- Presentation compose returns app-proxy URLs under `/api/media/assets/{id}/content`.
- Content GET returns PNG bytes (evidence files in this folder).

## Tours

- `demofeed-tour-teh-1` / `demofeed-tour-ist-1`: Cover linked via Tour ownership.
- Frontend tour detail + destination-scoped listing render enriched cover successfully.

## Public browse blocker

`GET /api/place/public/hotels?localeCode=fa` → HTTP 500  
EF: LINQ expression for Place↔PlaceTranslation public browse cannot be translated.

This blocks Public Hotel listing/detail success screenshots. Not fixed in T003 (validation-only scope).
