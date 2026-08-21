# TravelCore DEMOFEED Tool (temporary)

| Field | Value |
|-------|--------|
| Tool | `TravelCore.Tools.DemoFeed` |
| Path | `tools/demofeed` |
| Tasks | `TC-DEMOFEED-T002` (boundary) · `TC-DEMOFEED-T003` (Destination) · `TC-DEMOFEED-T004` (Place Hotel + Media) · `TC-DEMOFEED-T005` (Tour + Media) |
| Permanence | **TEMPORARY / REMOVABLE** |
| Product module | **NO** |

This is the isolated DEMOFEED feeder **host/boundary**.

It is **not**:

- a bounded context
- an `ITravelCoreModule`
- part of `TravelCore.Api` production composition
- a home for domain migrations / `demofeed` schema

## Commands

```bash
dotnet run --project tools/demofeed -- status
dotnet run --project tools/demofeed -- boundaries
dotnet run --project tools/demofeed -- ensure-schema --connection "Host=...;Database=TravelCore;Username=...;Password=..."
dotnet run --project tools/demofeed -- seed destinations --ensure-schema --connection "..."
dotnet run --project tools/demofeed -- seed places --ensure-schema --connection "..."
dotnet run --project tools/demofeed -- seed tours --ensure-schema --connection "..."
dotnet run --project tools/demofeed -- list destinations --connection "..."
dotnet run --project tools/demofeed -- list places --connection "..."
dotnet run --project tools/demofeed -- list tours --connection "..."
dotnet run --project tools/demofeed -- enrich-media --connection "..." [--pack-root docs/product-experience/assets/demo-media]
```

Connection may also be supplied as env `ConnectionStrings__TravelCore`.

`purge` remains **fail-closed** until GATE.

## Media pack enrichment (TC-P32-T002)

Command: `enrich-media`

- Reads `docs/product-experience/assets/demo-media/manifest.json`
- Uploads via `IMediaUploadService` (local FS under `.local/demofeed-media`)
- Attaches via owner paths: Place `SetCover` / `AddGalleryItem`, Tour `SetCover`
- Idempotent via feeder-local ledger (`.local/demofeed-media/enrichment-ledger.json`)
- **Destination covers skipped** — no Destination↔Media owner attach API (Architectural Concern / known limitation)
- Does **not** change frontend pages in this task

## Destination demo identity (T003)

Deterministic codes/slugs with prefix `demofeed-`:

| Code | Kind | Notes |
|------|------|-------|
| `demofeed-ir` | Country | Sample IR — labeled DEMOFEED |
| `demofeed-ir-thr` | Region | Sample Tehran region |
| `demofeed-ir-teh` | City | Sample Tehran city · slug `demofeed-tehran` |
| `demofeed-tr` | Country | Sample TR |
| `demofeed-tr-ist` | City | Sample Istanbul · slug `demofeed-istanbul` |

## Place (Hotel) demo identity (T004)

Hotel Places linked to existing demofeed city Destinations. Writes via `IPlaceService` / `PlaceApplicationService`. Cover images via `IMediaUploadService` + `SetCover` (synthetic 1×1 PNG placeholder — not scraped, not competitor content).

| Code | Kind | Destination | Slugs |
|------|------|-------------|--------|
| `demofeed-hotel-teh-1` | Hotel | `demofeed-ir-teh` | `demofeed-hotel-tehran-1` |
| `demofeed-hotel-ist-1` | Hotel | `demofeed-tr-ist` | `demofeed-hotel-istanbul-1` |

Names/descriptions explicitly say DEMOFEED / non-production. CatalogStatus set to Active for browse demos.

Requires destinations seeded first (`seed destinations`).

## Tour demo identity (T005)

TourProduct Packages linked to existing demofeed city Destinations. Writes via `ITourProductService` + `ITourProductSemanticLinkService` + `ITourProductMediaService`. Cover via Media upload + Tour `SetCover` (synthetic 1×1 PNG — not scraped). **No** Booking / Pricing / TourDeparture rows.

| Code | Kind | Destination | Slugs |
|------|------|-------------|--------|
| `demofeed-tour-teh-1` | Package | `demofeed-ir-teh` | `demofeed-tour-tehran-1` |
| `demofeed-tour-ist-1` | Package | `demofeed-tr-ist` | `demofeed-tour-istanbul-1` |

CatalogStatus set to Published for catalog browse demos (Published ≠ bookable ≠ priced). Titles/descriptions labeled DEMOFEED / non-production.

Requires destinations seeded first (`seed destinations`).

Media blobs land under tool-local filesystem (`.local/demofeed-media` under the tool output dir) via Media’s local filesystem adapter — not production object storage.

## Architecture rules

1. Lives under `tools/demofeed` — outside `src/backend/Modules/*`
2. Must never be added to the explicit module list in `TravelCore.Api/Program.cs`
3. Seeds write only through Destination / Place / Tour / Media owner paths
4. Demo identity: reserved slug/code prefix `demofeed-` (no new domain columns without ADR)
5. Forbidden: Booking · Payment · Pricing · HotelBooking · scraping · competitor content copy
6. Deletion: purge identifiable demo rows, then delete this tree (see `docs/plans/DEMOFEED-implementation-plan.md`)

## Next (repository SoT)

| Task | Deliverable |
|------|-------------|
| `TC-DEMOFEED-GATE` | Acceptance + deletion evidence (awaits Architect `.gate.md` / `.task.md`) |
