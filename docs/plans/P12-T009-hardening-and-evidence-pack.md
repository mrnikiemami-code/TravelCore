# TC-P12-T009 — Pricing hardening tests & evidence pack

**Task:** TC-P12-T009 — P12 hardening tests and evidence pack  
**Product HEAD:** `520a46d` (`TC-P12-T008` **ACCEPTED**)  
**Date:** 2026-08-17  
**Scope:** Hardening + evidence **only** — no new product capability (architect Auto-Execute).  
**Forbidden in this task:** new pricing features · Booking · Payment · FX engine · Checkout.  
**Not this task:** `TC-P12-GATE` (evidence pack only; Gate is next).

## 1. Mission checklist

| # | Verify | Result |
|---|--------|--------|
| 1 | Pricing independent of Tour (no Tour.Domain/Infrastructure refs, no Tour FK) | **PASS** — logical `TargetType`+`TargetId` only; schema `pricing` |
| 2 | No Booking / Payment / Checkout / Reservation | **PASS** — modules absent; no ownership types |
| 3 | FX boundary (no ExchangeRate table / no conversion in Pricing) | **PASS** — `IFxConversionPort` fail-closed stub; metadata only |
| 4 | Quote immutable snapshot + expiration | **PASS** — `Quote` + `QuoteSnapshotComponent` + required `ExpiresAt` |
| 5 | Public pricing read-only (**no Book Now**) | **PASS** — anonymous GET; tour-detail facts only |
| 6 | Occupancy rules ≠ Booking passenger | **PASS** — `PriceOccupancyRule` + category enums; no passenger entity |
| 7 | Admin Pricing owned by Pricing, not Tour Admin | **PASS** — `/api/pricing/prices` + Access `pricing.prices.*` |
| 8 | P12-R1…R8 all RESOLVED | **PASS** — plan open-decisions table |
| 9 | No new domain entities / product features in this task | **PASS** — evidence/docs + phase boundary guardrails only |

## 2. Accepted product commits (P12)

| Task | Commit | Essence |
|------|--------|---------|
| PLAN | `d26078d` | Authoritative P12 plan · R1–R8 listed |
| T001 | `7c2e488` | Pricing module scaffolding (`pricing` schema) — P12-R1 |
| T002 | `6c1b4ce` | Platform Money / Currency binding — P12-R2 |
| T003 | `58de552` | Price + PriceComponent (polymorphic target) — P12-R3 |
| T004 | `81a3f26` | Quote baseline (PriceSnapshot + Expiration) — P12-R4 |
| T005 | `c90931d` | Occupancy / passenger category pricing rules — P12-R5 |
| T006 | `e1d01c4` | Admin Pricing API (Pricing-owned, not Tour Admin) — P12-R6 |
| T007 | `87b5dac` | Quote display-currency metadata + FX boundary — P12-R7 |
| T008 | `520a46d` | Public read-only price summary — P12-R8 **ACCEPTED** |

Architect acceptance of T001–T008 is as issued. T009 prepares gate evidence; it does **not** execute `TC-P12-GATE`.

## 3. Locked decisions (all RESOLVED)

| ID | Essence |
|----|---------|
| **P12-R1** | Independent Pricing module · schema `pricing` · logical TourDeparture Guid only · no Tour FK / no shared DbContext |
| **P12-R2** | Reuse platform Money/Currency; one currency per price value; no twin SoR; no FX in T002 |
| **P12-R3** | Buyable Price → TourDeparture via polymorphic `TargetType`+`TargetId`; Pricing generic; product-level DEFER |
| **P12-R4** | Quote owned by Pricing; calculation snapshot + expiration; **Price ≠ Quote ≠ Payment**; no Customer/Passenger/checkout |
| **P12-R5** | Pricing owns occupancy / passenger **categories**; no Booking passenger entity; no reservation/inventory |
| **P12-R6** | Admin Pricing operational API owned by Pricing module; **not Tour Admin** |
| **P12-R7** | Pricing keeps price currency; does not convert; no ExchangeRate table; future FX Service owns rates |
| **P12-R8** | Public read-only price summary by logical target (initial TourDepartureId); no Booking/Payment/Checkout/FX |

Agency override of rates remains **UNRESOLVED** (prefer DEFER to P13) — not invented here.

## 4. Boundary / ownership matrix

| Concern | Owner | P12 posture |
|---------|-------|-------------|
| Tour catalog / Departure facts | **Tour** | TourProduct ≠ TourDeparture; Published ≠ bookable |
| Price definition | **Pricing** | `Price` + `PriceComponent` (Base/Fee/Tax) |
| Quote snapshot | **Pricing** | Immutable `Quote` + `ExpiresAt`; not Booking amount |
| Occupancy commercial rules | **Pricing** | `PriceOccupancyRule` categories + Money |
| Admin mutations | **Access** `pricing.prices.read/write` | Pricing-owned Admin API; Tour Admin does not own it |
| Public price facts | **Pricing** `IPublicPricingQuery` | Anonymous GET; optional tour-detail display |
| Money primitives | **Platform** `TravelCore.Money` | No parallel Pricing Money types |
| ExchangeRate / conversion | **Out (future FX Service)** | Fail-closed `IFxConversionPort` stub only |
| Booking / Payment / Checkout / Reservation | **Out of P12** | Forbidden; modules do not exist |
| Agency marketplace rates | **Out (P13)** | UNRESOLVED / DEFER |

## 5. Invariant evidence (T001–T008)

### 5.1 Pricing independent of Tour

- Pricing.Domain / Infrastructure project-references do **not** include Tour.
- Source has no `using TravelCore.Modules.Tour.(Domain|Infrastructure)`.
- `Price.TargetId` is a logical Guid; EF maps `target_id` with **no** `principalSchema: "tour"` FK.
- Persistence proof: `PricingMigrationLifecycleTests` asserts zero `tour` FKs from `pricing` tables.

### 5.2 No Booking / Payment / Checkout / Reservation

- `src/backend/Modules/Booking` and `.../Payment` directories do not exist.
- Pricing types do not declare Booking/Payment/Checkout/Reservation/Customer/Passenger entities.
- Quote has no `BookingId` / `PaymentId` / `CustomerId` / `PassengerId`.
- Public + Admin endpoints do not expose checkout / PaymentIntent.

### 5.3 FX boundary

- No `exchange_rates` / `fx_rates` tables in schema `pricing`.
- Quote stores optional `requested_display_currency` **metadata only** — no second stored amount, no `converted_amount`.
- `IFxConversionPort` + `FxBoundaryUnavailablePort` fail closed; Pricing does not multiply amounts.
- Public query does not call the FX port.

### 5.4 Quote immutable snapshot + expiration

- `Quote.CreateFromPrice` copies component Money into `QuoteSnapshotComponent` lines.
- Later `Price.AddComponent` does not change an existing Quote total (unit proof).
- `QuoteSnapshotComponent` has no public mutators; Quote has no public `AddSnapshotComponent`.
- `ExpiresAt` required and strictly after `CreatedAt`; `IsExpired(now)` at/after expiration.
- `SourcePriceId` is logical provenance — **no EF FK** to `prices` so snapshots survive live Price edits.

### 5.5 Public pricing read-only (no Book Now)

- Anonymous GET `/api/pricing/public` (+ TourDeparture helper). `AsNoTracking`; no POST/PUT/PATCH/DELETE.
- Live public Tour detail (`features/tour-detail`) shows starting price / occupancy lines only — **no Book Now**, no `BookingCta`, no checkout.
- Missing price summary does not hide the tour; Published ≠ bookable remains.

### 5.6 Occupancy rules vs Booking passenger

- `PriceOccupancyRule` is a commercial line (market type + `PassengerCategory` + `OccupancyCategory` + Money).
- `PassengerCategory` is a **closed enum** (Adult / ChildWithBed / ChildWithoutBed) — not a Booking passenger entity.
- No passport / DOB / reservation fields; no inventory consumption.

### 5.7 Admin Pricing owned by Pricing, not Tour Admin

- Admin HTTP: `/api/pricing/prices` mapped from Pricing module; Access policies `Access.Pricing.Prices.Read/Write`.
- Tour Admin endpoints / `admin-tour` / `admin-departure` UI do not own Pricing Admin.
- Dedicated Next.js Admin Pricing page was **not** added in P12 (API-first operational baseline). T009 does not invent that UI.

## 6. Guardrail / test surfaces

| Area | Evidence |
|------|----------|
| Module / Money / Price / Quote / FX / public query | `PricingBoundaryGuardrailTests` |
| Admin Access ownership | `PricingAdminAccessGuardrailTests` |
| **T009 phase boundary** | `PricingPhaseBoundaryGuardrailTests` |
| Public tour-detail (no Book Now; public query only) | `TourPublicDetailBoundaryGuardrailTests` |
| Domain unit | Pricing.UnitTests (Price / Quote / occupancy / FX / public query / Money) |
| Access catalog | Access.UnitTests (`pricing.prices.read/write` in AdminBaseline) |
| Persistence | `PricingMigrationLifecycleTests` (5 migrations · no tour FK · no FX tables) |
| Host | `PricingAccessAuthorizationTests` · `PricingPublicQueryHostTests` |

## 7. Validation battery (T009 re-run)

| Suite | Result | Detail |
|-------|--------|--------|
| `dotnet build TravelCore.sln` | **PASS** | 0 Error(s) · 15 Warning(s) (unrelated xUnit analyzers) |
| Pricing.UnitTests | **PASS** | **63** passed |
| ArchitectureTests | **PASS** | **145** passed (incl. +4 T009 `PricingPhaseBoundaryGuardrailTests`) |
| Persistence.IntegrationTests | **PASS** | **23** passed |
| Host.IntegrationTests | **PASS** | **43** passed |
| Frontend `tsc --noEmit` | **PASS** | clean |
| `git diff --check` | **PASS** | clean |

**Total this battery (core):** 63 + 145 + 23 + 43 = **274** passed (plus FE tsc).

## 8. Explicit OUT / DEFER

- Booking engine / reservation / hold / inventory consumption — **later (P19)**
- Payment capture / settlement — **later (P20)**
- FX Service ExchangeRate + conversion — **deferred** (boundary recorded in T007)
- Checkout / public Book Now CTA — **later (P14+)**
- Product-level (TourProduct) pricing — **DEFER** (P12-R3)
- Agency override of rates — **UNRESOLVED · prefer P13**
- Dedicated Admin Pricing Next.js page — **not invented in T009** (API baseline exists)
- Search indexing of prices — **P15**

## 9. Pitfalls (do not regress)

1. **P02 walking skeleton** (`foreign-tour-detail` + `BookingCtaIsland`) still uses **fixture** pricing/CTA. That is not live Booking and must not be treated as P12 public commerce. Live `tour-detail` has **no Book Now**.
2. Do not add a Tour schema FK “to be helpful” when attaching Price to a Departure.
3. Do not convert Quote snapshot amounts when `RequestedDisplayCurrency` is set.
4. Do not put occupancy rules on Booking passenger records; categories stay in Pricing.
5. Do not move Admin Pricing under Tour Admin.

## 10. Ready for Gate

After architect ACCEPT of T009 → Auto-Execute **TC-P12-GATE** (architect statement). This pack does **not** write GATE evidence and does not mark P12 COMPLETE. P13 PLAN may auto-start after Gate ACCEPT under continuity override.
