# TravelCore.Identifiers

Technical UUID **v7** generation for TravelCore (ADR 0002).

```csharp
Guid id = Uuid7.New(); // Guid.CreateVersion7()
```

- Pure `net10.0` library — no ASP.NET Core, no DI, no EF Core, no packages
- Not a security token/secret API
- Strongly typed business IDs are **out of scope** (module-owned later)
