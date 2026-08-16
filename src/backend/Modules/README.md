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

## Active modules (P04)

| Module | Projects | Schema |
|--------|----------|--------|
| ReferenceData | `ReferenceData/TravelCore.Modules.ReferenceData.{Domain,Contracts,Infrastructure}` | `reference_data` |
| Destination | `Destination/TravelCore.Modules.Destination.{Domain,Contracts,Infrastructure}` | `destination` |

- **ReferenceData:** Currency / Locale / ISO Country / IANA TimeZone catalogs + read APIs (`TC-P04-T002`).
- **Destination:** Hierarchy + translations + slug hooks + public/Admin surfaces (P04 complete).
- Invariant: **ReferenceData ≠ Destination**.

## Active modules (P05)

| Module | Projects | Schema |
|--------|----------|--------|
| Seo | `Seo/TravelCore.Modules.Seo.{Domain,Contracts,Infrastructure}` | `seo` |

- **Seo:** Route/indexation mechanics complete through P05 Gate (Destination ≠ SEO authority).
- Invariant: **SEO ≠ Destination content ownership**; Destination ≠ SEO authority.

## Active modules (P06)

| Module | Projects | Schema |
|--------|----------|--------|
| Media | `Media/TravelCore.Modules.Media.{Domain,Contracts,Infrastructure}` | `media` |

- **Media:** MediaAsset metadata SoR (`TC-P06-T002`) + Media-owned object-storage port (`TC-P06-T003`; local/dev adapter; no vendor lock-in) + variants/focal/alt-caption translations through T007.
- Invariant: **Media owns technical asset truth**; consumers own relationship meaning (gallery/hero/order). Media ≠ SEO authority.
- **P06-R2 RESOLVED:** storage abstraction is Media-owned first (not Platform-wide `IObjectStorage`).

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
- ReferenceData ≠ Destination.
- Destination ≠ SEO content ownership; SEO owns route/indexation mechanics only (P05).
- SEO ≠ Search.
- Media ≠ consumer gallery/hero semantics; Media ≠ SEO IndexPolicy.

## Host

`TravelCore.Api` remains the composition host. Modules register explicitly via `ITravelCoreModule` (no assembly scanning).
