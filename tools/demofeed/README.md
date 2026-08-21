# TravelCore DEMOFEED Tool (temporary)

| Field | Value |
|-------|--------|
| Tool | `TravelCore.Tools.DemoFeed` |
| Path | `tools/demofeed` |
| Task | `TC-DEMOFEED-T002` |
| Permanence | **TEMPORARY / REMOVABLE** |
| Product module | **NO** |

This is the isolated DEMOFEED feeder **host/boundary**.

It is **not**:

- a bounded context
- an `ITravelCoreModule`
- part of `TravelCore.Api` production composition
- a home for domain migrations / `demofeed` schema

## Commands (T002)

```bash
dotnet run --project tools/demofeed -- status
dotnet run --project tools/demofeed -- boundaries
dotnet run --project tools/demofeed -- help
```

`seed` and `purge` are **fail-closed** until later authorized DEMOFEED tasks.

## Architecture rules

1. Lives under `tools/demofeed` — outside `src/backend/Modules/*`
2. Must never be added to the explicit module list in `TravelCore.Api/Program.cs`
3. Future seeds write only through Destination / Place / Tour / Media owner paths
4. Demo identity: reserved slug/code prefix `demofeed-` (no new domain columns without ADR)
5. Forbidden: Booking · Payment · Pricing ownership changes · scraping · competitor content copy
6. Deletion: purge identifiable demo rows, then delete this tree (see `docs/plans/DEMOFEED-implementation-plan.md`)

## Next tasks (repository SoT)

| Task | Deliverable |
|------|-------------|
| `TC-DEMOFEED-T003` | Destination demo seed |
| `TC-DEMOFEED-T004` | Hotel (Place) + Media demo seed |
| `TC-DEMOFEED-T005` | Tour + Media demo seed |
| `TC-DEMOFEED-GATE` | Acceptance + deletion evidence |
