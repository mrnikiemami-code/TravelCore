# Automated Architecture Guardrails

وضعیت: Active (`TC-P01-T015`)

## Why

Accepted ADRs and architecture docs define what is allowed. Architecture tests make important **forbidden** states mechanically detectable so drift fails early.

These tests **complement** architect review — they do not replace it. `dotnet build` PASS ≠ architecture PASS.

## Location

```text
tests/Architecture/TravelCore.ArchitectureTests/
```

Packages (test-only): `xunit.v3` 3.2.2 · `TngTech.ArchUnitNET` 0.13.3 · `TngTech.ArchUnitNET.xUnitV3` 0.13.3

## Canonical command

```bash
dotnet run --project tests/Architecture/TravelCore.ArchitectureTests/TravelCore.ArchitectureTests.csproj -c Debug
```

No PostgreSQL / Docker / Testcontainers. Model metadata inspection is allowed; connections stay closed.

## Techniques

| Technique | Use |
|-----------|-----|
| Project metadata (`.csproj` graph) | production→tests, package ownership, Domain rules, fixture location |
| Compiled / ArchUnitNET | provider must not own `DbContext` |
| EF model metadata | fixture schema / Outbox ownership |
| Synthetic fixtures | prove detectors catch violations without mutating the repo |

## Current protected boundaries (examples)

- Pure primitives (`Identifiers` / `Time` / `Money`) stay framework-independent
- `src/**` must not reference `tests/**`
- Api / Platform / shared PostgreSQL provider must not own module DbContexts
- PersistenceFixture stays under `tests/` with `p01_fixture` + Outbox
- Fixture migrations stay under the fixture project
- Future module Domain projects must not take EF/Npgsql/provider deps (engine + synthetic proofs)
- Broad dumping-ground projects (`SharedKernel` / `Common` / `Utilities`) forbidden under `src/`

## Evolving rules

When an Accepted ADR intentionally changes a protected invariant, update these tests in the **same** change set. Do not silently weaken tests merely to keep CI green.
