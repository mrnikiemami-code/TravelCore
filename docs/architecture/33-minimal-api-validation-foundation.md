# Minimal API Validation Foundation

وضعیت: Active (`TC-P01-T018`)

## Why

TravelCore needs request-boundary validation for Minimal APIs using the **official** ASP.NET Core 10 path before any third-party validation framework is considered.

## Official path

| Piece | Choice |
|-------|--------|
| Package | `Microsoft.Extensions.Validation` **10.0.11** (direct on `TravelCore.Api`) |
| Registration | `builder.Services.AddValidation()` in `TravelCore.Api` |
| Rules | `System.ComponentModel.DataAnnotations` on request contracts |
| Error surface | Framework 400 + existing Problem Details (`IProblemDetailsService`) |

Do **not** recreate this with custom middleware, custom endpoint filters, or `Validator.TryValidateObject` as the primary path.

## Validation ≠ domain invariants

DataAnnotations at the API boundary may check required input, format, range, length, and shape.

They do **not** replace domain invariants. Future Domain models remain responsible for their own valid state. No DataAnnotations on domain entities in T018.

## Source-generator assembly ownership (critical)

`AddValidation()` participates in assembly-scoped generated validation metadata.

| Correct | Incorrect production pattern |
|---------|------------------------------|
| Call `AddValidation()` in the same assembly that owns Minimal API endpoints / request types | Hide the only `AddValidation()` call in `ApiFoundation` / Modularity / Configuration while endpoints live elsewhere |

Current host: endpoints live in `TravelCore.Api` → registration lives there.

### Future modules

When a module owns endpoints in its own assembly:

```text
TravelCore.Modules.<Module>.Api
  => Add<Module>ApiValidation(...)  // internally calls AddValidation()
Host
  => calls that module registration method
```

Host-only `AddValidation()` is not a substitute for module-owned registration once endpoints leave the host assembly.

## Explicit non-goals (T018)

- FluentValidation / third-party validation libraries
- Custom `IValidator<T>` abstractions / ValidationMiddleware / ValidationFilter
- Committed fake `/validation-test` endpoints
- Business request models (Tour, Booking, …)
- Database / remote / async validation
- Validation localization
- Custom Problem Details writer solely for validation
- Persistence / Docker for this foundation

## Runtime proof (not committed)

Official pipeline behavior (body / query / header, handler short-circuit, 400 details) is proven with a **temporary** out-of-repo Minimal API smoke using the same package/version. That project is not added to `TravelCore.sln`.

## Related

- [`19-api-error-and-serialization-foundation.md`](19-api-error-and-serialization-foundation.md)
- [`30-automated-architecture-guardrails.md`](30-automated-architecture-guardrails.md)
