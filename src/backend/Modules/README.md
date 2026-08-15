# Modules

Business and capability modules live under this directory.

## Active modules (P03)

| Module | Projects | Schema |
|--------|----------|--------|
| Identity | `Identity/TravelCore.Modules.Identity.{Domain,Contracts,Infrastructure}` | `identity` |
| Access | `Access/TravelCore.Modules.Access.{Domain,Contracts,Infrastructure}` | `access` |
| Party | `Party/TravelCore.Modules.Party.{Domain,Contracts,Infrastructure}` | `party` |

- **Identity:** Account + credential hashing + association (`TC-P03-T003`/`T004`).
- **Access:** Permission/Role taxonomy + seed + CRUD stubs (`TC-P03-T005`).
- **Party:** Person/Organization/Agency persistence + stubs (`TC-P03-T002`).

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
