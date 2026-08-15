# Real PostgreSQL Integration Test Foundation

وضعیت: Active (`TC-P01-T016`)

## Why

Architecture and EF model metadata are not persistence proof. TravelCore needs evidence that the shared PostgreSQL provider policy, NodaTime mappings, jsonb Outbox payloads, and module-local transactional Outbox behavior work against a **real** PostgreSQL server.

This suite is the first such gate for P01.

## What counts as real PostgreSQL

An actual PostgreSQL server process in an ephemeral Docker container (Testcontainers).

Not accepted as T016 proof:

- EF InMemory
- SQLite
- mock / fake connections
- metadata-only model inspection without a live server

## Mechanism

```text
tests/Integration/TravelCore.Persistence.IntegrationTests/
  PostgreSqlContainerFixture   → postgres:18.6 (ephemeral)
  PersistencePostgreSqlIntegrationTests
```

- Package (test-only): `Testcontainers.PostgreSql` **4.14.0**
- Framework: `xunit.v3` **3.2.2** (same standalone MTP pattern as T015)
- DbContext construction uses production provider policy: `UseTravelCorePostgreSql(...)` (includes `UseNodaTime()` and module-owned migrations-history schema)
- Schema setup applies committed fixture migrations via `PersistenceFixtureMigrator` (**test setup only**)
- Formal migration lifecycle acceptance → [`32-real-postgresql-migration-proof.md`](32-real-postgresql-migration-proof.md) (`TC-P01-T017`).

Pinned image `postgres:18.6` is the **P01 integration-test baseline**. It does not by itself establish the permanent production deployment version.

## Isolation rules

- No developer / staging / production connection strings
- Container credentials are ephemeral test data only
- Shared container for the suite; parallelization disabled for this collection; unique UUID v7 row IDs
- No Testcontainers packages in production / provider / fixture / architecture projects
- Docker is a required local/CI prerequisite for this gate (unavailable Docker ⇒ BLOCKED, not skipped green)

## Canonical command

```bash
dotnet run --project tests/Integration/TravelCore.Persistence.IntegrationTests/TravelCore.Persistence.IntegrationTests.csproj -c Debug
```

## Proven here

| Proof | Meaning |
|-------|---------|
| Connectivity + `SHOW server_version` | Live Npgsql connection; server reports 18.6 |
| `p01_fixture` catalog | Probe + Outbox + history tables exist on the real server |
| Public schema safety | No accidental `public.persistence_probes` / `public.outbox_messages` |
| NodaTime round-trip | Instant / LocalDate / LocalTime / LocalDateTime + store types |
| jsonb Outbox | Payload column type `jsonb`; semantic JSON round-trip |
| Same DbContext | Probe + Outbox persist together |
| Transaction commit / rollback | Module state + Outbox atomic on real PostgreSQL |

## Explicitly not claimed

- Exactly-once delivery
- Broker / dispatcher behavior
- Cross-module atomicity
- Complete migration architecture acceptance (→ T017)

## Related

- [`29-module-local-transactional-outbox.md`](29-module-local-transactional-outbox.md)
- [`30-automated-architecture-guardrails.md`](30-automated-architecture-guardrails.md)
- [`26-postgresql-provider-and-connection-foundation.md`](26-postgresql-provider-and-connection-foundation.md)
