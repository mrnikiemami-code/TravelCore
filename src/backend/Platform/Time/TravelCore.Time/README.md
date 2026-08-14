# TravelCore.Time

NodaTime temporal foundation (ADR 0004).

- Canonical types: `Instant`, `LocalDate`, `LocalTime`, `LocalDateTime`, `DateTimeZone` (IANA/TZDB)
- Clock abstraction: `NodaTime.IClock` with production direction `SystemClock.Instance`
- Clock DI registration: **DEFERRED** until a real consumer needs injection (no DI package in this project)
- No custom clock interface wrapper
- No PostgreSQL/Npgsql mapping in this package
