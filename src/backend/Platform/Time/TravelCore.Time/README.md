# TravelCore.Time

NodaTime temporal foundation (ADR 0004).

- Canonical types: `Instant`, `LocalDate`, `LocalTime`, `LocalDateTime`, `DateTimeZone` (IANA/TZDB)
- Clock: `NodaTime.IClock` → `SystemClock.Instance` via `AddTravelCoreTime()`
- No custom clock interface wrapper
- No PostgreSQL/Npgsql mapping in this package
