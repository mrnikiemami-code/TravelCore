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

## Active modules (P07)

| Module | Projects | Schema |
|--------|----------|--------|
| Place | `Place/TravelCore.Modules.Place.{Domain,Contracts,Infrastructure}` | `place` |

- **Place:** Place core + typed specializations (`TC-P07-T002`) — `PlaceId` canonical; Hotel/Restaurant/Attraction 1:1 tables in schema `place` (P07-R1).
- Invariant: **Hotel Catalog ≠ Hotel Booking**; Place.Hotel = canonical hotel catalog; Destination ≠ Place; no independent public HotelId/RestaurantId/AttractionId.

## Active modules (P08)

| Module | Projects | Schema |
|--------|----------|--------|
| Content | `Content/TravelCore.Modules.Content.{Domain,Contracts,Infrastructure}` | `content` |

- **Content:** ContentItem core + typed Article/LandingPage/Guide specializations (`TC-P08-T002`) — `ContentItemId` canonical; 1:1 tables in schema `content` (P08-R1).
- Invariant: **Content owns editorial**; SEO owns route/IndexPolicy; Destination/Place/Tour referenced by ID only (links deferred).

## Active modules (P09)

| Module | Projects | Schema |
|--------|----------|--------|
| Tour | `Tour/TravelCore.Modules.Tour.{Domain,Contracts,Infrastructure}` | `tour` |

- **Tour:** TourProduct shared-core (`TC-P09-T002`) — `TourProductId` canonical; `TourKind` Experience/Package on core; specialty tables deferred (P09-R7 → P10/P11); no TourDeparture in P09.
- Invariant: **TourProduct ≠ TourDeparture**; Experience itinerary = P10; Departure/Flight/TourHotelOption product = P11; Tour ≠ Pricing/Booking/Search/Content ownership.

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
- Place ≠ HotelBooking; Place ≠ Destination hierarchy ownership.
- Content ≠ SEO substance duplication; Content ≠ Tour/Place/UGC ownership.
- Tour ≠ TourDeparture (P11); Tour ≠ Pricing/Booking/Search; Tour ≠ Place Hotel ownership.
- Pricing ≠ Tour catalog ownership; Pricing ≠ Booking/Payment; logical TourDeparture Guid refs only (P12-R1).

## Active modules (P12)

| Module | Projects | Schema |
|--------|----------|--------|
| Pricing | `Pricing/TravelCore.Modules.Pricing.{Domain,Contracts,Infrastructure}` | `pricing` |

- **Pricing:** scaffolding + money baseline + Price/PriceComponent + Quote baseline (`TC-P12-T001`…`T004`) — schema `pricing`; platform `TravelCore.Money`; polymorphic `TargetType`+`TargetId`; structured Base/Fee/Tax; Quote = immutable PriceSnapshot + Expiration; no Booking/Payment/Customer/Passenger/checkout/Admin UI yet.
- **P12-R1 RESOLVED:** independent Pricing module; Tour owns tour facts; Pricing may logically reference TourDeparture identity (Guid) only — no EF FK / no Tour table ownership / no shared DbContext.
- **P12-R2 RESOLVED:** one authoritative currency per price value; reuse ADR 0003 Money; no twin multi-currency SoR; FX deferred.
- **P12-R3 RESOLVED:** Buyable/executable Price attaches conceptually to **TourDeparture** as the *initial* target. Pricing remains **generic**: it does **not** know TourDeparture types from Tour module. Polymorphic logical reference only: `TargetType` + `TargetId` (Guid). Example: TargetType=`TourDeparture`, TargetId=`uuid`. **No FK** · **No Booking** · **No Quote** (at Price attach time). Product-level pricing DEFER (do not invent TourProduct pricing now).
- **P12-R4 RESOLVED:** Quote owned by Pricing · Quote is calculation snapshot · No Booking ownership · No Payment · No Customer/Passenger · No checkout flow.
- Invariant: **Price ≠ Quote ≠ Payment / Booking Amount**; no silent single-currency wipe.

## Host

`TravelCore.Api` remains the composition host. Modules register explicitly via `ITravelCoreModule` (no assembly scanning).
