# P38-T003 — AgencyOffer Persistence Foundation

| Field | Value |
|-------|--------|
| Task-ID | `TC-P38-T003` |
| Date | 2026-08-21 |
| Module | `AgencyMarketplace` (existing SoT — not Tour/Party) |

## Summary

Evolved existing `AgencyOffer` persistence toward P38 contracts:

- `SalesChannel` (Public / AgencyPortal / Private)
- `DepartureScopeMode` + `departure_scope_ids` (uuid[])
- Audit `CreatedAt` / `UpdatedAt` (`Instant`)
- Lifecycle: `Suspend` / `Retire` (+ `Suspended` / `Retired` publication statuses)
- Panel contracts: `GetOfferAsync`, `SuspendOfferAsync`, `RetireOfferAsync`; create accepts channel/scope

## Migration

`20260822010000_P38AgencyOfferPersistenceFoundation` on schema `agency_marketplace`

## Compatibility

- Existing Draft→Submitted→Approved→Published path preserved
- Single-path booking unchanged; no public selection UI; no settlement
- No fake offers; no Tour/Party FK

## Validation

```text
dotnet test tests/Unit/TravelCore.Modules.AgencyMarketplace.UnitTests → Passed 18
```
