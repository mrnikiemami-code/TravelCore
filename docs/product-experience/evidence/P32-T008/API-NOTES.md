# P32-T008 API notes

## Destination Cover ownership (Option A)

Destination owns Cover 0..1 semantic media links in `destination.destination_media_links`.
Media owns technical assets; link stores opaque `MediaAssetId` Guid only (no FK to Media schema).
Gallery is deferred.

## Endpoints

| Method | Path | Auth |
|--------|------|------|
| PUT | `/api/destination/destinations/{id}/media/cover` | `Access.Destination.Destinations.Write` |
| DELETE | `/api/destination/destinations/{id}/media/cover` | `Access.Destination.Destinations.Write` |
| GET | `/api/destination/destinations/{id}/media` | public |
| GET | `/api/destination/destinations/{id}/media/presentation?locale=` | public |

## Presentation shape

```json
{
  "destinationId": "...",
  "cover": {
    "mediaAssetId": "...",
    "role": "Cover",
    "sortOrder": 0,
    "presentation": {
      "originalContentUrl": "/api/media/assets/{id}/content",
      "variants": []
    }
  }
}
```

No `gallery` field (Option A).

## DEMOFEED

`enrich-media` resolves Destination by code, uploads via `IMediaUploadService`, attaches via `IDestinationMediaService.SetCoverAsync`, ledger-idempotent like hotel.
