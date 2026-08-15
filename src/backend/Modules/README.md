# Modules

Business and capability modules live under this directory.

## Active module shells (P03)

| Module | Infrastructure project | Schema |
|--------|------------------------|--------|
| Identity | `Identity/TravelCore.Modules.Identity.Infrastructure` | `identity` |
| Access | `Access/TravelCore.Modules.Access.Infrastructure` | `access` |
| Party | `Party/TravelCore.Modules.Party.Infrastructure` | `party` |

These are **scaffolding shells** introduced by `TC-P03-T001` (empty DbContext + host DI stubs). Domain persistence/features arrive in later P03 tasks.

## Naming

Preferred project naming when a module is actually introduced:

```text
TravelCore.Modules.<Module>.Domain
TravelCore.Modules.<Module>.Application
TravelCore.Modules.<Module>.Infrastructure
TravelCore.Modules.<Module>.Contracts
```

Create only the layers a module actually needs. Empty layer projects are not required.

## Rules

- Each persistent module owns its own DbContext and PostgreSQL schema (ADR 0001).
- Modules must not access another module’s persistence or use cross-module EF navigation.
- Cross-module collaboration uses contracts / semantic events — see architecture dependency docs.
- Identity ≠ Party ≠ Access.

## Host

`TravelCore.Api` remains the composition host. Modules register explicitly via `ITravelCoreModule` (no assembly scanning).
