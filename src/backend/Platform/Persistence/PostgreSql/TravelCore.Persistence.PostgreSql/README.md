# TravelCore.Persistence.PostgreSql

Shared PostgreSQL / EF Core provider configuration (ADR 0001).

- Logical connection name: `ConnectionStrings:TravelCore`
- `UseTravelCorePostgreSql(...)` for module-owned DbContexts
- No global DbContext, migrations, or startup connectivity
