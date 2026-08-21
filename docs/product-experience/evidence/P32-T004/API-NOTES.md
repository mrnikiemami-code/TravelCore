# P32-T004 API notes

## Root cause

`PlacePublicQuery.ListByLocaleAsync` used:

```csharp
.SelectMany(p => p.Translations.Select(t => new { Place = p, Translation = t }))
```

and/or projected `p.Id.Value` / owned `Hotel` through SelectMany+OrderBy.

EF Core 10 could not translate → HTTP 500 on `/api/place/public/hotels`.

`FindBySlugAsync` had the same full-Place SelectMany pattern and failed with owned-reference projection errors during detail resolution.

## Fix

1. Public browse: SelectMany translations filtered by locale/slug; project `PlaceId` (struct) + translation scalars; OrderBy Name/PlaceId; Take.
2. FindBySlug: project scalar place fields + translation slug (no full Place entity).
3. StarRating omitted from browse projection (owned Hotel join deferred).

## Post-fix

- `GET /api/place/public/hotels?localeCode=fa` → 200, DEMOFEED hotels present.
- Slug lookup for `demofeed-hotel-tehran-1` → 200.
