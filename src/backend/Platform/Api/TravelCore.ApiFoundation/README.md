# TravelCore.ApiFoundation

Platform API foundation for `TravelCore.Api`:

- `AddProblemDetails()` + exception/status-code middleware
- System.Text.Json remains the serializer (ASP.NET Core defaults)
- Official `Microsoft.AspNetCore.OpenApi` document generation (`AddOpenApi` / Development-only `MapOpenApi`)

Not for Swagger UI / Scalar / ReDoc, validation, health, observability, or business DTOs.
