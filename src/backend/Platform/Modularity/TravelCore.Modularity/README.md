# TravelCore.Modularity

Narrow composition contract for plugging modules into `TravelCore.Api`.

- `ITravelCoreModule` — register services + map endpoints
- `ModuleComposition` — applies an **explicit** host-provided module list

Not for business rules, persistence, or reflective discovery.
