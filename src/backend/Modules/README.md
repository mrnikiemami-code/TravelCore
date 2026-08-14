# Modules

Future **business and capability modules** live under this directory.

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

- Do **not** create placeholder / fake production modules ahead of their phase.
- Each persistent module owns its own DbContext and PostgreSQL schema (ADR 0001).
- Modules must not access another module’s persistence or use cross-module EF navigation.
- Cross-module collaboration uses contracts / semantic events — see architecture dependency docs.

## Host

`TravelCore.Api` remains the composition host. Modules register into the host in later tasks; this folder only establishes the physical home.
