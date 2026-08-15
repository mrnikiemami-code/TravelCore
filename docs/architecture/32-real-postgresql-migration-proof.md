# Real PostgreSQL Migration Proof

وضعیت: Active (`TC-P01-T017`)

## Why

`TC-P01-T016` proved live persistence against PostgreSQL. This task is the dedicated **migration acceptance gate**:

```text
CLEAN DATABASE
  → PENDING MIGRATIONS
  → APPLY MODULE MIGRATIONS
  → MODULE-OWNED HISTORY
  → ZERO PENDING
  → SECOND MIGRATE IS NO-OP
  → NO MODEL/MIGRATION DRIFT
```

## Scope

- Fixture-owned migrations only (`TravelCore.PersistenceFixture`)
- Real ephemeral PostgreSQL via Testcontainers (`postgres:18.6`)
- Dedicated clean container (not the already-migrated T016 fixture DB)
- Apply path: `PersistenceFixtureMigrator` (no `EnsureCreated`, no host auto-migrate)

## Not in scope

- Business module migrations
- Destructive downgrade / remove flows
- Global migration orchestrator
- Changing `TravelCore.Api` startup to mutate schema

## Evidence location

```text
tests/Integration/TravelCore.Persistence.IntegrationTests/
  MigrationLifecycleContainerFixture
  PersistencePostgreSqlMigrationLifecycleTests
```

## Proven

| Gate | Result meaning |
|------|----------------|
| Clean start | `p01_fixture` / history / tables absent before apply |
| Inventory | Exactly two EF-defined migrations in order |
| Pending before | 2 pending, 0 applied |
| History schema | `p01_fixture.__EFMigrationsHistory` (not `public`) |
| History rows | Exact MigrationId values, no duplicates |
| Second migrate | Safe no-op; history unchanged |
| Model drift | `HasPendingModelChanges() == false` + `dotnet ef migrations has-pending-model-changes` |
| Type regression | NodaTime store types + Outbox `jsonb` on real catalog |

## CLI model-drift gate

```bash
dotnet ef migrations has-pending-model-changes \
  --project tests/Fixtures/Persistence/TravelCore.PersistenceFixture/TravelCore.PersistenceFixture.csproj \
  --context PersistenceFixtureDbContext
```

Uses the fixture design-time factory (fake non-secret connection). Must not require a live design database.

## Related

- [`28-module-owned-migrations-and-runner-convention.md`](28-module-owned-migrations-and-runner-convention.md)
- [`31-real-postgresql-integration-test-foundation.md`](31-real-postgresql-integration-test-foundation.md)
