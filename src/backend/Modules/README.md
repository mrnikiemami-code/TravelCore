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
- AgencyMarketplace ≠ Party identity · ≠ Pricing · ≠ Booking · ≠ TourProduct catalog; logical PartyId Guid only (P13-R1).
- PublicExperience ≠ Tour catalog · ≠ Search engine · ≠ Booking; presentation + SEO composition only (P14-R1).
- Search ≠ Tour/Content/Pricing/Agency facts · ≠ SEO IndexPolicy · ≠ Booking · ≠ Recommendation; schema `search` only in T001 (P15-R1).

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

## Active modules (P13)

| Module | Projects | Schema |
|--------|----------|--------|
| AgencyMarketplace | `AgencyMarketplace/TravelCore.Modules.AgencyMarketplace.{Domain,Contracts,Infrastructure}` | `agency_marketplace` |

- **AgencyMarketplace:** AgencyProfile + AgencyOffer + Agency Panel + Offer publication (`TC-P13-T002`–`T007`) — schema `agency_marketplace`; commercial terms without Price; sales availability without capacity; Access-backed panel; publication ≠ SEO IndexPolicy.
- **P13-R1 RESOLVED:** independent Agency Marketplace module owns Agency commercial relationship; Party remains identity SoR; logical PartyId Guid only — no EF FK / no Party table ownership / no shared DbContext / no Offer in T001.
- **P13-R2 RESOLVED:** AgencyProfile is the commercial layer over Party identity (0..1). Not a second identity SoR.
- **P13-R3 RESOLVED:** AgencyOffer owns the marketplace sales relationship. TourProduct remains catalog SoR. Logical TourProduct Guid only.
- **P13-R4 RESOLVED:** Agency must NOT override Price. Commercial terms = Notes + SalesRules metadata only.
- **P13-R5 RESOLVED:** Agency does NOT own capacity. SalesAvailability metadata + optional logical TourDeparture Guid only.
- **P13-R6 RESOLVED:** Agency Panel belongs to Agency Marketplace (not Tour Admin, not Identity). Profile + offer management only.
- **P13-R7 RESOLVED:** Agency Marketplace owns Offer publication status. Not SEO IndexPolicy. Not TourProduct CatalogStatus. Published Offer ≠ SEO Indexed.
- Invariant: **Agency ≠ Party · Agency ≠ Pricing · Agency ≠ Booking · Agency ≠ TourProduct**.

## Active modules (P14)

| Module | Projects | Schema |
|--------|----------|--------|
| PublicExperience | `PublicExperience/TravelCore.Modules.PublicExperience.Contracts` | none (composition / presentation; no DbContext in T001) |

- **PublicExperience:** Detail / Listing / Landing surface contracts (`TC-P14-T001`) — presentation + SEO composition. Not Search engine. Not Tour catalog SoR. No Booking/Payment.
- **P14-R1 RESOLVED:** Public Experience Surface belongs to Public Experience Layer (not Search, not Catalog).
- Invariant: **Public Experience ≠ Booking · SEO Page ≠ Commercial Transaction · Content ≠ Catalog · Search URL ≠ SEO Landing URL**.

## Active modules (P15)

| Module | Projects | Schema |
|--------|----------|--------|
| Search | `Search/TravelCore.Modules.Search.{Domain,Contracts,Infrastructure}` | `search` |

- **Search:** scaffolding through public query API (`TC-P15-T001`–`T007`) — schema `search`; hybrid read-model; outbox projection; facets; ranking; AI-readiness; `GET /api/search` empty stub; no broker / FTS / ML / vector / LLM / Elasticsearch.
- **P15-R1 RESOLVED:** Search is an independent Discovery Owner. Tour/Content/Pricing/AgencyMarketplace remain fact SoT. SEO remains IndexPolicy owner. Search is a future read-model/projection, not a second write SoR. No LLM/business rules inside Search. No FTS/index engine in T001.
- **P15-R2 RESOLVED:** Hybrid Read Model. Search owns document/index abstraction. Physical engine not committed. SearchDocument is not a domain entity.
- **P15-R3 RESOLVED:** Transactional Outbox + Async Projection Worker. Search failure must not fail domain transaction. Projection retryable + idempotent. No RabbitMQ/real queue in T003.
- **P15-R4 RESOLVED:** Search owns faceting Aggregation / Counting / Result composition. Domain owns attribute meaning + source facts. PE owns filter UI only. No facet engine / ES aggregations / domain facet tables in T004.
- **P15-R5 RESOLVED:** Deterministic explainable ranking + stable tie-break. Search owns ranking composition/ordering/metadata. Not business-policy authority. Ranking ≠ Recommendation. No ML/embeddings/personalization in T005.
- **P15-R6 RESOLVED:** Structured attributable locale-aware facts first. Semantic retrieval + provenance. No embeddings/vector/RAG/LLM. Search ≠ SoT.
- **P15-R7 RESOLVED:** Engine-neutral public Search query API. Structured filters + continuation-ready pagination + explicit locale. Not SEO IndexPolicy. Empty stub allowed.
- Invariant: **Search ≠ Catalog · Search ≠ Pricing · Search ≠ AgencyOffer · Search ≠ IndexPolicy · Search ≠ Booking · Search ≠ Recommendation · Filter UI ≠ Faceting · Ranking ≠ Business Priority · Search ≠ AI Platform · Search API ≠ Search Engine API**.

## Host

`TravelCore.Api` remains the composition host. Modules register explicitly via `ITravelCoreModule` (no assembly scanning).
