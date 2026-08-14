# PostgreSQL Provider and Connection Foundation

وضعیت: Active (`TC-P01-T011`)

فیزیکی:

```text
src/backend/Platform/Persistence/PostgreSql/TravelCore.Persistence.PostgreSql/
```

## Authority

ADR 0001 remains authoritative for one database / schema-per-module / module-owned DbContext + migrations.

## Packages (direct)

| Package | Version |
|---------|---------|
| Microsoft.EntityFrameworkCore | 10.0.11 |
| Microsoft.EntityFrameworkCore.Relational | 10.0.11 |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 |

Microsoft EF Core runtime packages used by this foundation are intentionally aligned on the same patch (`10.0.11`). `Relational` is a direct reference for that alignment — not a new architecture capability.

Deferred (not in T011):

- `Microsoft.EntityFrameworkCore.Design` / Tools → migration task (`TC-P01-T013`)
- `Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime` → with real DbContext proof (`TC-P01-T012`)
- Direct `Npgsql` PackageReference — only if ADO.NET types are used directly (not required here)
- Dapper — later read projections when justified

## Connection convention

Logical name: **`TravelCore`** under `ConnectionStrings` (`ConnectionStrings:TravelCore`).

Secrets are deployment/runtime configuration only — never committed.

Composition may pass an already-resolved connection string into provider configuration; modules should not each invent a divergent connection-name literal.

## Provider API

```csharp
optionsBuilder.UseTravelCorePostgreSql(connectionString);
```

Builds EF `DbContextOptions` with the Npgsql provider. Building options does **not** open a PostgreSQL connection and must not require a running server at host startup.

## Architecture rules (this foundation)

- PostgreSQL is the relational source of record
- EF Core is the transactional persistence baseline
- One database; **schema-per-module**; one DbContext per persistent module
- **No** global / application-wide DbContext
- **No** `EnsureCreated` lifecycle (migrations later)
- **No** external application-wide `NpgsqlDataSource` yet
- **No** Money / NodaTime persistence mapping here
- Domain projects must not reference this package; module infrastructure may

## Deferred proofs

- Non-business fixture DbContext / schema ownership → `TC-P01-T012`
- Migrations tooling → `TC-P01-T013`
- Real PostgreSQL infrastructure / migration proof → `TC-P01-T016` / `TC-P01-T017`

## Host impact

T011 does not wire persistence into `TravelCore.Api` — no persistent module exists yet.
