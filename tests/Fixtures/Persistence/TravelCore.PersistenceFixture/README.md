# TravelCore.PersistenceFixture

Non-production persistence fixture for `TC-P01-T012` / `TC-P01-T013` / `TC-P01-T014`.

- Proves module-owned `DbContext` + schema (`p01_fixture`)
- Owns EF migrations + model snapshot under `Migrations/`
- Migration history table: `p01_fixture.__EFMigrationsHistory` (via shared provider API)
- Module-local Outbox table: `p01_fixture.outbox_messages` (same DbContext; no dispatcher)
- Uses `UseTravelCorePostgreSql(...)` provider policy (including NodaTime)
- Design-time factory + local `dotnet-ef` for reproducible tooling
- `PersistenceFixtureMigrator` is explicit/test-support only — not applied in T013/T014
- Must never be referenced or registered by `TravelCore.Api`
