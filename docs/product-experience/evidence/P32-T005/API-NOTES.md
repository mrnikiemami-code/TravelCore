# P32-T005 API notes

## Listing covers

`enrichHotelsWithCoverMedia` previously expected flat `originalContentPath` fields.
Place media presentation returns nested:

```json
cover.presentation.originalContentUrl
cover.presentation.variants[]
```

Aligned with Tour enricher. Listing cards now resolve app-proxy URLs.

## StarRating

After browse SelectMany (T004-safe), second query:

```csharp
.Where(p => placeIds.Contains(p.Id)) // PlaceId struct list
.Select(p => new { PlaceId = p.Id, StarRating = p.Hotel == null ? null : p.Hotel.StarRating })
```

Validated:

- `demofeed-hotel-tehran-1` stars=4
- `demofeed-hotel-istanbul-1` stars=5
- `GET /api/place/public/hotels` remains HTTP 200
