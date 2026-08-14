# TravelCore.Observability

Framework-native observability baseline:

- `ILogger` + structured message templates
- Application correlation via `X-Correlation-ID` (max 128, validated)
- Logging scope fields: `CorrelationId`, and `TraceId` when `Activity.Current` exists
- Does **not** overwrite Activity / W3C trace context with caller correlation
- No Serilog, OpenTelemetry packages, HttpLogging, or business metrics in this foundation
