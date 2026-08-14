# Module-Owned DbContext Proof

وضعیت: Active (`TC-P01-T012`)

## Authority

ADR 0001 (schema-per-module / module-owned DbContext) · ADR 0004 (NodaTime) · PostgreSQL provider foundation (`26-…`).

## What was proven

Using a **non-production** fixture only:

```text
tests/Fixtures/Persistence/TravelCore.PersistenceFixture/
```

| Rule | Evidence |
|------|----------|
| Persistent module owns its DbContext | `PersistenceFixtureDbContext` lives in the fixture project |
| Persistent module owns its schema | default schema `p01_fixture` set by the fixture |
| DbContext not in Api host | no `TravelCore.Api` reference/registration |
| DbContext not in shared persistence project | provider project has no fixture/entity types |
| Provider supplies policy only | `UseTravelCorePostgreSql` (+ `UseNodaTime`) |
| No cross-module EF navigation | single fixture model; no other modules |
| No global DbContext | none in repository |

## Production pattern (future)

```text
TravelCore.Modules.<Module>.Infrastructure
  => <Module>DbContext
  => <module-schema>
```

Shared `TravelCore.Persistence.PostgreSql` supplies provider policy. The module owns DbContext, model, schema, and later migrations.

## Fixture rules

- Test/support only — must never evolve into a business module
- Probe entity (`PersistenceProbe`) is technical metadata only (no domain semantics)
- No Money mapping, no `EnsureCreated`, no real PostgreSQL connection in T012
- Migrations ownership / tooling → `TC-P01-T013` (`docs/architecture/28-module-owned-migrations-and-runner-convention.md`)
- Live DB apply → `TC-P01-T016` / `T017`

## NodaTime provider policy

`UseTravelCorePostgreSql` configures:

```csharp
UseNpgsql(connectionString, npgsql => npgsql.UseNodaTime())
```

via `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime` on the shared PostgreSQL foundation.
