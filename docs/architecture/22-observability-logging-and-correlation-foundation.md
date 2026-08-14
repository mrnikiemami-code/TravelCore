# Observability — Logging and Correlation Foundation

وضعیت: Active (`TC-P01-T007`)

فیزیکی:

```text
src/backend/Platform/Observability/TravelCore.Observability/
```

## Ownership

Observability is a **platform capability** (HTTP host, future modules, background work, integrations). It is not folded into ApiFoundation and is not an `ITravelCoreModule`.

## Logging

- Abstraction: `ILogger` / `Microsoft.Extensions.Logging`
- Prefer structured message templates and named fields over string concatenation for operational values
- Do **not** wrap ILogger in a custom TravelCore logger facade merely to hide it
- Do **not** lock into Serilog / NLog / vendor providers in this foundation

## Correlation header

Canonical header: `X-Correlation-ID`

- Exactly one usable value may be accepted from the caller
- Max length: **128**
- Reject empty/whitespace, control characters, multi-value headers, excessive length
- Malformed optional header → safe fallback; do **not** fail the business request
- No P01 aliases (`X-Request-ID`, etc.)

## CorrelationId vs TraceId

| Concept | Role |
|---------|------|
| Application `CorrelationId` | Caller/business integration identifier (or safe fallback) |
| `Activity.TraceId` | Distributed tracing context from ASP.NET Core / W3C |

Rules:

- Do **not** overwrite Activity / W3C trace with `X-Correlation-ID`
- Do **not** overwrite `HttpContext.TraceIdentifier` with untrusted caller data
- Response always echoes the selected safe `X-Correlation-ID`

## Fallback order

When no valid caller correlation ID:

1. Current `Activity.TraceId` when available
2. Else `HttpContext.TraceIdentifier`

No UUID v7 generation here (T008 owns identifiers).

## Logging scope

Per request scope includes at least `CorrelationId`, and `TraceId` when `Activity.Current` exists.

Never put Authorization, cookies, tokens, bodies, or query strings into the scope by default.

## Activity

Preserve the framework server Activity. Do **not** create a duplicate request Activity. Custom `ActivitySource` spans come later for meaningful operations only.

## Explicit non-goals (T007)

- Serilog / OpenTelemetry / APM packages
- `UseHttpLogging()` / body-header logging
- Business metrics / Meter instruments “for show”
- Health publishers / telemetry exporters
- Redesigning Problem Details to embed correlation (response header is enough)
- Global OpenAPI requirement for the correlation header

## Domain boundary

Domain code must not depend on `ILogger`, `HttpContext`, `Activity`, or `TravelCore.Observability`. Application / infrastructure / host composition may.

## Future direction

Exporters/APM/OpenTelemetry packages only when TravelCore has a concrete collection requirement. Metrics instruments when a measurable operational need exists.
