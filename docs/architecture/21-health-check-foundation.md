# Health Check Foundation

وضعیت: Active (`TC-P01-T006`)

فیزیکی:

```text
src/backend/Platform/Health/TravelCore.Health/
```

## Ownership

Health is **host/platform infrastructure**, not a business module.

`TravelCore.Health` owns the liveness/readiness endpoint convention and the readiness tag. It does not invent dependency probes. Future capabilities (for example PostgreSQL after connectivity exists) register framework `IHealthCheck` implementations with the readiness tag.

## Liveness vs readiness

| Concept | Path | Meaning |
|---------|------|---------|
| Liveness | `/health/live` | Is this process alive and able to respond? |
| Readiness | `/health/ready` | Is this instance ready for normal workload given currently registered required dependencies? |

These remain separate.

- Liveness **does not** run checks tagged `ready`.
- Readiness runs **only** checks tagged `ready`.
- A database outage must not by itself imply the process is dead.

## Readiness tag

```text
ready
```

Constant: `TravelCoreHealthTags.Ready`.

Today no required dependency checks are registered, so readiness may legitimately return Healthy. PostgreSQL probing is deferred (not part of T006).

## Framework-native

Uses ASP.NET Core:

- `AddHealthChecks()`
- `MapHealthChecks()`
- `HealthCheckOptions` predicates

No `AspNetCore.HealthChecks.*` or other third-party health packages.

## Response shape

Default framework health response (minimal operational text). Not a rich diagnostics API. Do not expose topology, connection strings, secrets, or exception details.

## OpenAPI

Health probe endpoints are operational, not public business contracts. They are excluded from the generated OpenAPI document via `ExcludeFromDescriptionAttribute` metadata.

## Module composition

Health is **not** an `ITravelCoreModule`. Host wires `AddTravelCoreHealth` / `MapTravelCoreHealth` explicitly.

## Deferred

- PostgreSQL / EF / Npgsql health probes
- Authentication on health endpoints
- Metrics, tracing, OpenTelemetry, health publishers (T007+)
