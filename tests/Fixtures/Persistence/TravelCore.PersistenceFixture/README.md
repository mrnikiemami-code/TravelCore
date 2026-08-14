# TravelCore.PersistenceFixture

Non-production persistence fixture for `TC-P01-T012`.

- Proves module-owned `DbContext` + schema (`p01_fixture`)
- Uses `UseTravelCorePostgreSql(...)` provider policy (including NodaTime)
- Must never be referenced or registered by `TravelCore.Api`
