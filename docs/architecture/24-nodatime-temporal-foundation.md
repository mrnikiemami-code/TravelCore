# NodaTime Temporal Foundation

وضعیت: Active (`TC-P01-T009`)

فیزیکی:

```text
src/backend/Platform/Time/TravelCore.Time/
```

## Authority

ADR 0004 remains authoritative.

## Packages (direct)

| Package | Version |
|---------|---------|
| NodaTime | 3.3.3 |
| NodaTime.Serialization.SystemTextJson | 1.4.0 (via ApiFoundation JSON integration) |

## Semantics

| Concern | Type / approach |
|---------|-----------------|
| System/audit instants | `Instant` |
| Date without zone | `LocalDate` |
| Time without date | `LocalTime` |
| Local schedule | `LocalDateTime` + IANA zone id |
| Timezones | IANA via `DateTimeZoneProviders.Tzdb` (`TravelCoreTemporal.TimeZones`) |
| Clock | `NodaTime.IClock` → `SystemClock.Instance` |

## API / JSON

System.Text.Json continues as the serializer. Official `ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)` is applied in `TravelCore.ApiFoundation` (no hand-written converters for standard NodaTime types).

## Non-goals

- Npgsql / EF Core temporal mapping
- Business schedules / tour-flight timing models
- Custom calendars as domain columns
- Custom `ITravelCoreClock` wrappers
- Replacing System.Text.Json
