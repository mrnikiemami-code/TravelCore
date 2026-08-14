# Backend Physical Structure

وضعیت: Active (introduced by `TC-P01-T001`)

این سند قرارداد فیزیکی backend را مشخص می‌کند تا Modular Monolith بدون ساخت ماژول جعلی یا shared dumping ground قابل توسعه باشد.

اسناد مرجع:

- [`00-constitution.md`](00-constitution.md)
- [`02-technology-baseline.md`](02-technology-baseline.md)
- [`04-module-boundaries.md`](04-module-boundaries.md)
- [`05-dependency-rules.md`](05-dependency-rules.md)
- [`08-persistence-and-migrations.md`](08-persistence-and-migrations.md)
- ADR 0001 (schema-per-module)

---

## Layout

```text
src/backend/
├── TravelCore.Api/          # Host / composition root
├── Modules/                 # Future module-owned code
│   └── README.md
└── Platform/                # Narrow technical foundations (later tasks)
    └── README.md
```

Solution folders mirror this under `TravelCore.sln` → `src` → `backend` → `Host` | `Modules` | `Platform`.

---

## Host ownership

| Item | Rule |
|------|------|
| Host project | `TravelCore.Api` |
| Role | Single deployable ASP.NET Core Minimal API host |
| Relocate? | Do not move for aesthetic symmetry; only if accepted architecture requires it |
| Composition | Future modules register services/endpoints into the host (later tasks) |

---

## Future module location

| Item | Rule |
|------|------|
| Root | `src/backend/Modules/` |
| Naming | `TravelCore.Modules.<Module>.*` |
| Internal ownership | Domain / Application / Infrastructure / Contracts **only where needed** |
| Persistence | Module-local DbContext + schema; no global `TravelCoreDbContext` |
| Placeholders | **Forbidden** — do not invent empty Identity/Tour/Booking projects early |

---

## Technical foundation boundary

| Item | Rule |
|------|------|
| Root | `src/backend/Platform/` |
| Allowed | Narrow cross-module technical primitives (Time, Money, Identifiers, Observability, …) when a dedicated P01 task owns them |
| Forbidden | Generic `Shared` / `Common` / `BuildingBlocks` dumping ground for arbitrary code |
| Forbidden | Business entities, module policies, or “temporary shared domain” |
| Dependency | Platform ← Modules allowed for technical primitives; Platform → business modules forbidden |

---

## Dependency direction (physical intent)

```text
TravelCore.Api (host)
    → Modules.* (composition / wiring only as designed later)
    → Platform.* (technical foundations)

Modules.<A> Domain
    ✗ → Modules.<A> Infrastructure
    ✗ → Modules.<B>.*
    ✗ → Platform only via approved primitives (not reverse)

Modules.<A> Infrastructure
    → Modules.<A> Application/Domain
    → Platform technical packages as needed
    ✗ → other module persistence
```

T001 does not add architecture tests; those arrive in `TC-P01-T015`. This layout must not make those rules hard to enforce.

---

## Explicit non-goals of this structure document

- Implementing Identity / Access / Party / Tour / … modules
- Adding EF Core, PostgreSQL, OpenAPI, validation, health, observability code
- Creating empty speculative .NET projects
- Expanding API scaffold cleanup (belongs primarily to later API foundation tasks)
