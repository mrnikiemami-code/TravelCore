# Backend physical layout

TravelCore backend is a **Modular Monolith** hosted by a single deployable ASP.NET Core app.

Authoritative convention: [`docs/architecture/18-backend-physical-structure.md`](../../docs/architecture/18-backend-physical-structure.md)

```text
src/backend/
├── TravelCore.Api/     # application host (composition root)
├── Modules/            # future module-owned code (no fake modules yet)
└── Platform/           # narrowly scoped cross-module technical foundations (later tasks)
```
