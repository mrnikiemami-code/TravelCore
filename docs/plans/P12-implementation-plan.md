# P12 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P12-PLAN` |
| Phase | P12 — Pricing |
| Status | IN PROGRESS — P12-R1/R2/R3/R4/R5/R6/R7/R8 RESOLVED; T001–T008 ACCEPTED; T009 evidence pack delivered; next = GATE |
| Baseline | `520a46d` (T008 ACCEPTED baseline for T009) |
| Authoritative sources | `docs/ROADMAP.md` § P12 · transition map · Tour/Departure boundaries · P09–P11 locks · ADR money foundation · ADR 0001 · ADR 0011–0014 · architect P11 Gate ACCEPT narrative (Price ≠ Quote ≠ Booking Amount) |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P12** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

> **Envelope note:** Authored from **repository SoT** + architect P11 Gate ACCEPT continuity (auto-start P12 PLAN). Under PIPELINE continuity, ceremonial confirms are **not required**. **No product code in PLAN task.**

---

## 1. Phase Purpose

P12 باید **موتور قیمت‌گذاری تور** را معرفی کند تا:

1. **Price ≠ Quote ≠ Payment / Booking Amount** به‌عنوان invariants معماری قفل شود.
2. قیمت‌گذاری روی **TourDeparture** (و در صورت قفل: قواعد مشترک محصول) بدون ادغام با Booking/Payment.
3. **Currency / Money** foundation موجود پلتفرم reuse شود — بدون خاموش‌کردن همهٔ قیمت‌ها به یک ارز.
4. مؤلفه‌های قیمت (PriceComponent) · نرخ (TourRate در صورت قفل) · مسافر/occupancy/age commercial rules در سطح Pricing (نه Reservation).
5. **Quote** به‌عنوان snapshot قابل‌انقضا در صورت قفل — بدون Settlement/Payment capture.
6. مرز شفاف با **Booking** (بعداً) و **Agency Marketplace (P13)** و **Search (P15)**.

P11 تحویل داد: TourDeparture + Admin + Public Published hooks (Published ≠ Bookable).  
P12 اضافه می‌کند: **Pricing structures / quotes** — **بدون** Booking CTA، بدون Payment، بدون Settlement.

P12 **Booking/Payment** · **Agency Marketplace (P13)** · **Public polish (P14)** · **Search (P15)** نیست.

---

## 2. Starting Baseline

| Item | Value |
|------|--------|
| P11 Gate | `TC-P11-GATE` COMPLETE / ACCEPTED (`6f7ea12`) |
| P11 evidence | [`P11-GATE-acceptance-evidence.md`](P11-GATE-acceptance-evidence.md) · [`P11-T010-hardening-and-evidence-pack.md`](P11-T010-hardening-and-evidence-pack.md) |
| P11 Plan | ACCEPTED · R1–R8 RESOLVED |
| Baseline HEAD | `6f7ea12` |
| P00–P11 | COMPLETE |
| TourDeparture | Present · Published public hooks · Admin Access-backed |
| Money platform | Existing ADR money / Currency foundation (reuse — do not reinvent) |
| Booking / Payment / Quote product | Quote baseline = T004; Booking/Payment **Not implemented** |

---

## 3. Non-goals (explicit)

1. Booking engine / reservation / hold / inventory consumption.
2. Payment capture / refund / settlement / ledger.
3. Agency marketplace commercial ownership (P13).
4. Search indexing of prices (P15).
5. Public booking CTA / checkout UX (P14+).
6. Silent single-currency conversion of all commercial amounts.
7. Inventing unlocked R# closures — open decisions stay OPEN until architect lock.

---

## 4. Task sequence (proposed)

### TC-P12-PLAN — this document

### TC-P12-T001 — Pricing module / ownership scaffolding
- Purpose: Introduce Pricing ownership surface as **independent module** (P12-R1 RESOLVED).
- Delivered: Contracts/Domain/Infrastructure · schema `pricing` · host registration · guardrails · UnitTests smoke.
- Forbidden: Booking/Payment types · price calculation · Quote · FX · Checkout.

### TC-P12-T002 — Money / Currency baseline binding
- **Delivered / ACCEPTED:** `6c1b4ce` — Pricing reuses platform `TravelCore.Money` (`Money` + `CurrencyCode`); `PricingMoney` / `PricingCurrency` factories; EF `MoneyOwnedMapping` (Amount + CurrencyCode, `numeric(24,8)`); unit + architecture guardrails.
- **P12-R2:** one authoritative currency per price value; no twin multi-currency SoR; no FX/Quote/Payment in this task.

### TC-P12-T003 — PriceComponent model
- **Delivered / ACCEPTED:** `58de552` — `Price` aggregate + structured `PriceComponent` (Base / Fee / Tax) with polymorphic logical `TargetType` + `TargetId` (initial: `TourDeparture`); same-currency-within-Price; ≥1 Base; schema `pricing` tables + migration; no Tour FK / no Quote / no Booking / no Admin API.
- **P12-R3:** buyable Price targets TourDeparture via polymorphic logical reference; Pricing stays generic (no TourDeparture CLR types); product-level pricing DEFER.

### TC-P12-T004 — Pricing Quote baseline
> **Architect reorder (vs original plan title “Departure pricing attachment”):** T004 is **Quote baseline** (was plan T006). Departure-admin pricing attachment deferred to a later task if still needed.
- **Delivered:** `Quote` aggregate owned by Pricing — `SourcePriceId` logical provenance + optional target copy + immutable structured PriceSnapshot (`QuoteSnapshotComponent` kind+money) + required `ExpiresAt` (NodaTime Instant); Total/currency from snapshot; no Customer/Passenger/Payment/Booking/Reservation; schema `pricing` tables + migration; no Tour/Booking/Payment FK; no FX.
- **P12-R4 RESOLVED:** Quote owned by Pricing; Quote is calculation snapshot; No Booking ownership; No Payment; No Customer/Passenger; No checkout flow.

### TC-P12-T005 — Passenger / occupancy / age commercial rules
- **Delivered:** Pricing-owned structured occupancy/passenger pricing rules attached to `Price` (`PriceOccupancyRule`) with explicit dimensions: `TourMarketPriceType` + `PassengerCategory` + `OccupancyCategory` + `Money`; baseline categories include Adult / ChildWithBed / ChildWithoutBed and SingleRoom (plus DoubleRoom/TwinRoom support); persistence in `pricing.price_occupancy_rules`; no Booking passenger entity; no reservation calc; no inventory.
- **P12-R5 RESOLVED:** Pricing occupancy and passenger category baseline.

### TC-P12-T006 — Admin Pricing baseline
- **Delivered / ACCEPTED:** `e1d01c4` — Pricing-owned Admin API for create/update Price, manage PriceComponent, and manage OccupancyRules; Access permissions `pricing.prices.read` / `pricing.prices.write`; no Tour Admin ownership; no Booking/Payment/Checkout/FX/Quote workflow UI.
- **P12-R6 RESOLVED:** Admin Pricing is operational UI/API for Pricing. Ownership stays in Pricing module (Admin API + Admin UI). Not Tour Admin ownership.

### TC-P12-T007 — Pricing currency context and FX boundary
> **Architect reorder:** original plan slot was Access + Admin Pricing baseline; that work was delivered in T006 / P12-R6. T007 is currency context + FX boundary.
- **Delivered / ACCEPTED:** `87b5dac` — optional `RequestedDisplayCurrency` metadata on `Quote` (CurrencyCode via `PricingCurrency.ParseRequired` when present); `QuoteCurrencyContext` + `IFxConversionPort` fail-closed stub (`FxBoundaryUnavailableException`); nullable `quotes.requested_display_currency` column. Snapshot Money amounts unchanged; no second stored amount; no conversion.
- **P12-R7 RESOLVED:** Pricing keeps the price currency. Pricing does not convert currency. Exchange-rate ownership is not Pricing. Future FX Service owns ExchangeRate + Conversion; Pricing may only request conversion later. T007 records requested display-currency metadata / currency context only — no ExchangeRate table, no FX calculation, no Payment currency, no Settlement, no Booking.
- Original FX-authority wording previously parked under old R5 remains deferred as **implementation of FX Service** (not invented here).

### TC-P12-T008 — Public Pricing read model baseline
- **Delivered / ACCEPTED:** `520a46d` — public read-only query (`IPublicPricingQuery`) + Price Summary DTO (currency, components kind+money, occupancy prices categories+money) by logical `TargetType`+`TargetId` (thin helper for TourDeparture). Anonymous GET `/api/pricing/public`. Optional tour-detail display of starting price / occupancy lines. No Booking, Payment, Checkout, Availability, Reservation, or FX conversion. No Quote mutation.
- **P12-R8 RESOLVED:** Pricing provides a public read-only query for price summary (currency, components, occupancy prices) by logical target (initial: TourDepartureId). No Booking, Payment, Checkout, Availability, Reservation, or FX conversion.

### TC-P12-T009 — Hardening + evidence
- **Delivered:** architecture hardening + evidence pack only — [`P12-T009-hardening-and-evidence-pack.md`](P12-T009-hardening-and-evidence-pack.md). Extra phase-boundary guardrails (`PricingPhaseBoundaryGuardrailTests`). No new pricing features · no Booking · no Payment · no FX engine · no Checkout. Does **not** execute GATE.

### TC-P12-GATE — Acceptance Gate
- Next after T009 ACCEPT. Not executed in T009.

---

## 5. Open decisions (must not invent)

| ID | Topic | Status | Notes |
|----|-------|--------|-------|
| **P12-R1** | Pricing ownership (new module vs Tour-owned schema) | **RESOLVED** | Independent Pricing module owns schema `pricing`; Tour owns tour facts; Pricing may only logically reference TourDeparture identity (`Guid`) — no EF FK / no Tour table ownership / no shared DbContext |
| **P12-R2** | Mixed-currency / conversion policy SoT | **RESOLVED** | Reuse platform Money/Currency (ADR 0003). One authoritative currency per price value; no twin SoR duplicates (e.g. USD+IRR for same amount). Currency required; amount rules follow Money ADR. FX conversion / exchange-rate provider / Quote conversion / Payment currency / FX tables = deferred (not T002). Never silent single-currency wipe. |
| **P12-R3** | Pricing attaches to Departure vs Product vs both | **RESOLVED** | Buyable/executable Price attaches conceptually to **TourDeparture** as the *initial* target. Pricing remains **generic**: it does **not** know TourDeparture types from Tour module. Polymorphic logical reference only: `TargetType` + `TargetId` (Guid). Example: TargetType=`TourDeparture`, TargetId=`uuid`. **No FK** · **No Booking** · **No Quote**. Product-level pricing DEFER (do not invent TourProduct pricing now). |
| **P12-R4** | Quote model (required in P12? expiration? snapshot fields) | **RESOLVED** | Quote owned by Pricing · Quote is calculation snapshot · No Booking ownership · No Payment · No Customer/Passenger · No checkout flow. Price = defined system price; Quote = calculated price for a specific request (snapshot + expiration). Ownership: Pricing → Quote → PriceSnapshot + Expiration. |
| **P12-R5** | Pricing occupancy and passenger category baseline | **RESOLVED** | **Pricing owns occupancy categories; Support tour market price types; No Booking passenger entity; No reservation calculation; No inventory.** Previous FX-authority phrasing ("Exchange rate source / authority") is deferred to **implementation of FX Service** (not solved in T005; T007 only records the request boundary — see P12-R7). |
| **P12-R6** | Admin Pricing ownership | **RESOLVED** | Admin Pricing is operational UI/API for Pricing. Ownership stays in Pricing module (Admin API + Admin UI). Not Tour Admin ownership. |
| **P12-R7** | Pricing currency context / FX boundary | **RESOLVED** | **P12-R7 RESOLVED:** Pricing keeps the price currency. Pricing does not convert currency. Exchange-rate ownership is not Pricing. Future FX Service owns ExchangeRate + Conversion; Pricing may only request conversion later. T007 records requested display-currency metadata / currency context only — no ExchangeRate table, no FX calculation, no Payment currency, no Settlement, no Booking. |
| **P12-R8** | Public Pricing read model | **RESOLVED** | **P12-R8 RESOLVED:** Pricing provides a public read-only query for price summary (currency, components, occupancy prices) by logical target (initial: TourDepartureId). No Booking, Payment, Checkout, Availability, Reservation, or FX conversion. |
| Agency override of rates | Marketplace (P13) vs P12 | **UNRESOLVED** | Prefer DEFER to P13 |

---

## 6. Architecture invariants (carry forward)

1. TourProduct ≠ TourDeparture.
2. Published Departure ≠ Bookable.
3. Price ≠ Quote ≠ Payment / Booking Amount.
4. Tour ≠ Flight ownership · Tour ≠ HotelBooking.
5. Money foundation = platform ADR — do not invent parallel money types.
6. No Booking/Payment/Search engines in P12.

---

## 7. Continuity

After `TC-P12-GATE` ACCEPT, continuity may auto-start **P13 PLAN** (Agency Marketplace) unless a real Stop Condition applies.

---

## 8. PLAN acceptance criteria

- [x] Phase purpose + non-goals explicit
- [x] Task sequence proposed without product code
- [x] Open decisions listed (R1–R8) — no invention
- [x] Baseline = P11 Gate ACCEPT commit
- [x] Architect lock **P12-R1** (independent Pricing module) · first product task `TC-P12-T001` executable
- [x] Architect lock **P12-R2** (platform Money reuse · one currency per value · no twin SoR · no FX in T002)
- [x] Architect lock **P12-R3** (buyable Price → TourDeparture via polymorphic `TargetType`+`TargetId`; Pricing generic; no FK; product-level DEFER; no Quote/Booking)
- [x] Architect lock **P12-R4** (Quote owned by Pricing; calculation snapshot + expiration; no Booking/Payment/Customer/Passenger/checkout)
- [x] Architect lock **P12-R5** (Pricing occupancy and passenger category baseline; no Booking passenger entity/reservation/inventory)
- [x] Architect lock **P12-R6** (Admin Pricing is operational UI/API for Pricing; ownership stays in Pricing module (Admin API + Admin UI); not Tour Admin ownership)
- [x] Architect lock **P12-R7** (Pricing keeps the price currency; does not convert; Exchange-rate ownership is not Pricing; T007 records requested display-currency metadata / FX boundary only)
- [x] Architect lock **P12-R8** (public read-only price summary by logical target; initial TourDepartureId; no Booking/Payment/Checkout/Availability/Reservation/FX conversion)
- [ ] Architect ACCEPT + Auto-Execute subsequent product tasks
