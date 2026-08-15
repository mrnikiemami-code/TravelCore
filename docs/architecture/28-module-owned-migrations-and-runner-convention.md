# Module-Owned Migrations and Runner Convention

وضعیت: Active (`TC-P01-T013`)

## Authority

ADR 0001 (schema-per-module / module-owned DbContext + migrations) · ADR 0004 (NodaTime) · provider foundation (`26-…`) · DbContext proof (`27-…`).

## Ownership rule

The same owner that owns the `DbContext` and PostgreSQL schema also owns:

- EF Core migrations
- the model snapshot
- the migration history table (same module schema)
- the explicit migrate entry point for that owner

Proof owner (non-production):

```text
TravelCore.PersistenceFixture
  DbContext: PersistenceFixtureDbContext
  Schema:    p01_fixture
  Migrations: tests/Fixtures/Persistence/TravelCore.PersistenceFixture/Migrations/
  History:   p01_fixture.__EFMigrationsHistory
  Runner:    PersistenceFixtureMigrator
```

## Shared provider role

`TravelCore.Persistence.PostgreSql` supplies:

```csharp
UseTravelCorePostgreSql(connectionString, migrationsHistorySchema: moduleSchema)
```

- Applies Npgsql + `UseNodaTime()`
- Places `__EFMigrationsHistory` in the **caller-supplied** schema
- Does **not** hard-code `p01_fixture` or any module schema name

## Tooling

| Item | Location / version |
|------|--------------------|
| Local `dotnet-ef` | `.config/dotnet-tools.json` → **10.0.11** |
| Design package | Fixture only: `Microsoft.EntityFrameworkCore.Design` **10.0.11** (`PrivateAssets=all`) |
| Design-time factory | `PersistenceFixtureDbContextFactory` (fake non-secret connection string; no live connect) |

Do **not** use a global `dotnet-ef` install as the source of truth. Do **not** add `Microsoft.EntityFrameworkCore.Tools` PackageReference.

## Explicit migration execution

- Use `Database.MigrateAsync` via a module/fixture-owned migrator
- No `EnsureCreated` / `EnsureDeleted`
- No automatic migrate on `TravelCore.Api` startup
- No reflection / assembly-scanning discovery of migrators
- No central registry of all future module DbContexts in T013

Production pattern (future):

```text
TravelCore.Modules.<Module>.Infrastructure
  => <Module>DbContext
  => <module-schema>
  => Migrations/
  => <Module>Migrator (explicit)
```

A future host/deployment orchestrator may call module migrators **explicitly**.

## Deferred

Real PostgreSQL apply / live proof → `TC-P01-T016` / `TC-P01-T017`.

Module-local Outbox persistence (same DbContext/schema) → `TC-P01-T014` / `docs/architecture/29-module-local-transactional-outbox.md`.
