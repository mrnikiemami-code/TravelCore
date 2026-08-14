# Configuration and Options Foundation

وضعیت: Active (`TC-P01-T005`)

فیزیکی:

```text
src/backend/Platform/Configuration/TravelCore.Configuration/
```

## Ownership

Configuration belongs to the **capability that consumes it**.

Forbidden:

- one giant `TravelCoreOptions` / `GlobalOptions` / `AllSettings`
- scanning assemblies to auto-bind every Options class
- deriving section names from type names via reflection

## Options convention

Use the standard .NET Options pattern.

Preferred registration helper:

```csharp
services.AddTravelCoreOptions<SomeOptions>(configuration, "Some:Section");
```

This binds the explicit section, applies DataAnnotations validation, and validates on startup.

Call the helper only when that capability is actually enabled and needs the section. Do not invent mandatory configuration merely to demonstrate failure.

## Environment layering

Keep ASP.NET Core defaults:

- `appsettings.json`
- `appsettings.{Environment}.json`
- environment variables / secret stores at deployment time

Do not commit real Development/Production secrets or deployment values.

## Secrets

Never commit:

- database passwords
- API tokens
- provider secrets
- private keys
- connection credentials

Development: use local/user-secret or machine-local mechanisms outside source control.  
Production: supply via deployment/runtime configuration.  
No cloud secret-manager vendor lock-in is chosen in P01.

## Connection strings

Connection strings are external configuration. PostgreSQL wiring itself belongs to later P01 tasks — T005 only establishes the convention.

## Validation boundary

| This task | Later task |
|-----------|------------|
| Options / configuration validation | HTTP request validation (`TC-P01-T018`) |

## Host impact

`TravelCore.Api` remains runnable without PostgreSQL/Redis/S3 or other future infrastructure. No Options are registered for unimplemented capabilities yet.
