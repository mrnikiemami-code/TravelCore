# TravelCore DEMOFEED Tool (temporary)

| Field | Value |
|-------|--------|
| Tool | `TravelCore.Tools.DemoFeed` |
| Path | `tools/demofeed` |
| Tasks | `TC-DEMOFEED-T002` (boundary) · `TC-DEMOFEED-T003` (Destination seed) |
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
dotnet run --project tools/demofeed -- list --connection "..."
```

Connection may also be supplied as env `ConnectionStrings__TravelCore`.

`purge` remains **fail-closed** until GATE. Hotel/Tour seeds are **T004+**.

## Destination demo identity (T003)

Deterministic codes/slugs with prefix `demofeed-`:

| Code | Kind | Notes |
|------|------|-------|
| `demofeed-ir` | Country | Sample IR — labeled DEMOFEED |
| `demofeed-ir-thr` | Region | Sample Tehran region |
| `demofeed-ir-teh` | City | Sample Tehran city · slug `demofeed-tehran` |
| `demofeed-tr` | Country | Sample TR |
| `demofeed-tr-ist` | City | Sample Istanbul · slug `demofeed-istanbul` |

Names/descriptions explicitly say DEMOFEED / non-production. Not commercial facts.

Writes go through `DestinationApplicationService` + owner migrators only.

## Architecture rules

1. Lives under `tools/demofeed` — outside `src/backend/Modules/*`
2. Must never be added to the explicit module list in `TravelCore.Api/Program.cs`
3. Seeds write only through Destination / Place / Tour / Media owner paths
4. Demo identity: reserved slug/code prefix `demofeed-` (no new domain columns without ADR)
5. Forbidden: Booking · Payment · Pricing ownership changes · scraping · competitor content copy
6. Deletion: purge identifiable demo rows, then delete this tree (see `docs/plans/DEMOFEED-implementation-plan.md`)

## Next tasks (repository SoT)

| Task | Deliverable |
|------|-------------|
| `TC-DEMOFEED-T004` | Hotel (Place) + Media demo seed |
| `TC-DEMOFEED-T005` | Tour + Media demo seed |
| `TC-DEMOFEED-GATE` | Acceptance + deletion evidence |
