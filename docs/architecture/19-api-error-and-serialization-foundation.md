# API Error and Serialization Foundation

وضعیت: Active (`TC-P01-T003`)

مرجع فیزیکی:

```text
src/backend/Platform/Api/TravelCore.ApiFoundation/
```

## What this establishes

| Capability | Choice |
|------------|--------|
| HTTP API errors | ASP.NET Core **Problem Details** (`application/problem+json`) |
| Unhandled exceptions | Centralized via `UseExceptionHandler()` |
| Empty status-code responses | `UseStatusCodePages()` so unknown routes get an intentional body |
| JSON serializer | **System.Text.Json** only (ASP.NET Core web defaults) |
| Custom JSON option overrides | None in T003 — defaults are the intentional baseline |

## Explicit non-goals

- OpenAPI (`TC-P01-T004`)
- Validation (`TC-P01-T018`)
- Health (`TC-P01-T006`)
- Correlation / observability (`TC-P01-T007`)
- Business error codes, localization, or domain exception taxonomies
- Newtonsoft.Json

## Host composition order

```text
create builder
AddTravelCoreApiFoundation()
AddTravelCoreModules(explicit list)
build app
UseTravelCoreApiFoundation()
UseHttpsRedirection()
MapTravelCoreModules(explicit list)
run
```

## Contracts vs models

Future public API contracts are not persistence entities and not domain models:

`Domain Model ≠ Persistence Model ≠ API Contract ≠ Page View Model`

No business DTOs are introduced in this foundation.
