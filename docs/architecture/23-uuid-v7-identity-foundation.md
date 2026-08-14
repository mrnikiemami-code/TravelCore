# UUID v7 Identity Foundation

وضعیت: Active (`TC-P01-T008`)

فیزیکی:

```text
src/backend/Platform/Identifiers/TravelCore.Identifiers/
```

## Authority

ADR 0002 remains authoritative: referencable/external domain identities use **UUID version 7**.

## Generation API

```csharp
Guid id = TravelCore.Identifiers.Uuid7.New();
```

Implementation:

```csharp
Guid.CreateVersion7()
```

No custom UUID algorithm. No external packages (`UUIDNext`, `Medo.Uuid7`, etc.).

## Non-goals

- Strongly typed business IDs (`TourId`, …) — later, module-owned
- EF Core / PostgreSQL mapping / ValueConverters — later persistence tasks
- DI interfaces around generation (`IUuidGenerator`, …)
- Coupling to NodaTime / T009 time foundation
- Using UUID v7 as auth tokens, API keys, or cryptographic secrets

## Host impact

T008 does not change host HTTP behavior. The library is a pure technical primitive available for Application/composition layers.
