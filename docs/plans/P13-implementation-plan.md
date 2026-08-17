# P13 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P13-PLAN` |
| Phase | P13 — Agency Marketplace |
| Status | IN PROGRESS — P13-R1–R5 RESOLVED; T005 capacity boundary delivered |
| Baseline | `b372367` (`docs: P12 acceptance gate evidence [TC-P12-GATE]` — **TC-P12-GATE** ACCEPTED; P12 COMPLETE) |
| Authoritative sources | `docs/ROADMAP.md` § P13 · P09-R3 AgencyId · P03 Agency Panel non-ownership · P12 R1–R8 · ADR 0001 · ADR 0011–0014 · architect P12 Gate ACCEPT narrative (Agency ≠ Party · Agency ≠ Pricing · Agency ≠ Booking) |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P13** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

> **Envelope note:** Authored from **repository SoT** + architect P12 Gate ACCEPT continuity (auto-start P13 PLAN). Under PIPELINE continuity, ceremonial confirms and ceremonial Gate waits are **not required**. **No product code in PLAN task.**

---

## 1. Phase Purpose

P13 باید **لایهٔ Agency Marketplace** را معرفی کند تا بازار واقعی تور بتواند آژانس را به‌عنوان فروشنده/عرضه‌کننده ببیند، بدون قاطی شدن با هویت Party، موتور Pricing، یا Booking.

هدف:

1. **Agency ≠ Party** — Party SoR هویت تجاری `PartyKind.Agency` می‌ماند؛ Marketplace مالک هویت حقوقی نیست.
2. **Agency ≠ Pricing** — قیمت و Quote در Pricing می‌مانند؛ Marketplace مالک موتور قیمت نیست.
3. **Agency ≠ Booking** — رزرو/hold/inventory consumption متعلق به Booking آینده است.
4. **TourProduct را بی‌ضرورت تکرار نکند** — P09 قبلاً `AgencyId` منطقی **0..1** روی TourProduct قفل کرده (P09-R3).
5. Offer ownership · Tour offering · commercial rules آژانس · Agency Panel عملیاتی · publishing/moderation **فقط پس از قفل معمار**.
6. مرز شفاف با **Public polish (P14)** · **Search (P15)** · **B2B commerce (P24)** · **Payment**.

P12 تحویل داد: Pricing مستقل + Quote + Occupancy + Admin + Public read + FX boundary.  
P13 اضافه می‌کند: **Marketplace / Agency offering surface** — **بدون** Booking CTA، بدون Payment، بدون FX engine، بدون تکرار TourProduct.

P13 **Booking/Payment** · **Public polish factory (P14)** · **Search (P15)** · **Visa (P17)** نیست.

---

## 2. Starting Baseline

| Item | Value |
|------|--------|
| P12 Gate | `TC-P12-GATE` COMPLETE / ACCEPTED (`b372367`) |
| P12 evidence | [`P12-GATE-acceptance-evidence.md`](P12-GATE-acceptance-evidence.md) · [`P12-T009-hardening-and-evidence-pack.md`](P12-T009-hardening-and-evidence-pack.md) |
| P12 Plan | ACCEPTED · R1–R8 RESOLVED |
| Baseline HEAD | `b372367` |
| P00–P12 | COMPLETE |
| Party Agency identity | Present (`PartyKind.Agency`) · `IPartyReadQuery` |
| Tour Agency ref | P09-R3 — optional logical `TourProduct.AgencyId` 0..1 · Party.Contracts validation · no cross-schema FK |
| Agency Panel | P03 T011 — presentation/capability stub only (`GET /api/agency/panel/capabilities`) · Access `agency.panel.open` · **no marketplace** |
| Pricing | Independent module · `TourMarketPriceType.Public/Agency` exists · rate override **UNRESOLVED** (deferred from P12) |
| Booking / Payment | **Not implemented** |

---

## 3. Non-goals (explicit)

1. Booking engine / reservation / hold / inventory consumption.
2. Payment capture / refund / settlement / ledger / credit accounts (P24).
3. Duplicating `TourProduct` / `TourDeparture` as a second catalog SoR.
4. Merging Agency Marketplace into Party, Tour, or Pricing modules.
5. FX engine / ExchangeRate tables.
6. Public polish factory (P14) / Search indexing (P15).
7. Inventing unlocked R# closures — open decisions stay OPEN until architect lock.

---

## 4. Task sequence (proposed)

### TC-P13-PLAN — this document

### TC-P13-T001 — Marketplace ownership scaffolding
- Purpose: Introduce Marketplace ownership surface as **independent Agency Marketplace module** (P13-R1 RESOLVED).
- Delivered: Contracts/Domain/Infrastructure · schema `agency_marketplace` · host registration · Party logical Guid readiness (`MarketplacePartyId`) · guardrails · UnitTests smoke.
- Forbidden: Booking/Payment · Pricing engine · duplicating TourProduct · Agency merge into Party · Offer / AgencyProfile product types.

### TC-P13-T002 — Agency marketplace profile baseline
- Purpose: Marketplace-facing profile **beyond** Party identity (P13-R2 RESOLVED).
- Delivered: `AgencyProfile` aggregate (0..1 per PartyId) · Display/Contact/Commercial owned values · Draft/Active/Archived lifecycle · logical PartyId + optional logical Logo MediaAsset Guid.
- Party remains SoR of `PartyKind.Agency`; logical PartyId refs only.
- Forbidden: copying Party aggregate · Identity ownership transfer · Offer · Pricing · Commission · Booking · Payment · Party schema changes.

### TC-P13-T003 — Offer ownership model
- Purpose: AgencyOffer owns the offerable listing (P13-R3 RESOLVED). TourProduct remains catalog SoR.
- Delivered: `AgencyOffer` aggregate · same-schema FK to AgencyProfile · logical TourProduct Guid · Draft/Active/Archived + Listed/Unlisted · display/commercial metadata without Price.
- Forbidden: Pricing ownership · Booking · Payment · Departure creation · Inventory · Commission engine.

### TC-P13-T004 — Agency commercial terms boundary baseline
- Purpose: Define AgencyOffer commercial posture without owning price (P13-R4 RESOLVED).
- Architect lock: Agency must **not** override Price. Commercial terms = Notes + SalesRules metadata. No Money.
- Delivered: `AgencyOfferSalesRules` (RequiresManualConfirmation · ExclusiveListing) · `Deactivate()` lifecycle refinement · persistence columns without Price/Discount/Commission/Currency.
- Forbidden: Price override · Discount engine · Commission calculation · Currency · Quote · Booking.
- Note: original plan T004 (Tour offering linkage) is **not** this task. Do not invent that work until architect re-issues it.

### TC-P13-T005 — Agency offer capacity boundary baseline
- Purpose: Marketplace capacity posture without owning departure capacity (P13-R5 RESOLVED).
- Architect lock: Agency does **not** own capacity. TourDeparture remains Min/Max Pax SoR.
- Delivered: `AgencyOfferSalesAvailability` (SalesOpen) · optional logical `MarketplaceTourDepartureId` · OpenSales requires Active · no seats.
- Forbidden: Seat inventory · Capacity ownership · Reservation · Booking · Pricing · Allocation engine.

### TC-P13-T006 — Capacity / availability policies (commercial, not reservation)
- Purpose: Agency-side capacity/availability **policy** (P13-R5) — not booked-seat counts.
- TourDeparture already owns Min/Max Pax rules (P11-R3). Booking will own consumption later.
- Forbidden: reservation consumption · availability engine · inventory ledger.

### TC-P13-T007 — Agency Panel operational baseline
- Purpose: Operational Agency Panel for Marketplace (P13-R6) — extend P03 presentation stub **or** Pricing-style module-owned Admin, as locked.
- Access-backed. Server Component First.
- Forbidden: Tour Admin ownership of Marketplace · Booking/Payment/Checkout.

### TC-P13-T008 — Publishing / moderation (if locked)
- Purpose: Offer/agency publishing + moderation (P13-R7) only if architect requires it in P13.
- Explicit DEFER allowed. Do not invent workflow.

### TC-P13-T009 — Hardening + evidence

### TC-P13-GATE — Acceptance Gate
- Evidence only. Ceremonial Gate wait is **not** a pipeline stop. Continuity may auto-start **P14 PLAN** after ACCEPT unless a real Stop Condition applies.

---

## 5. Open decisions (must not invent)

| ID | Topic | Status | Notes |
|----|-------|--------|-------|
| **P13-R1** | Marketplace ownership (new module vs Party-owned vs Tour-owned) | **RESOLVED** | Independent Agency Marketplace module owns Agency commercial relationship · schema `agency_marketplace` · Party remains identity SoR · logical PartyId Guid only · no Party/Tour/Pricing merge · no Offer in T001 |
| **P13-R2** | Marketplace profile vs Party Agency identity | **RESOLVED** | Party = identity SoR. Agency Marketplace owns AgencyProfile (0..1 per Agency PartyId). Logical PartyId only. No Party schema change. |
| **P13-R3** | Offer aggregate vs TourProduct remaining SoR | **RESOLVED** | AgencyOffer owns the sales relationship. TourProduct remains catalog SoR. Logical TourProduct Guid. No Tour FK / Price / Booking. |
| **P13-R4** | Agency override of rates | **RESOLVED** | Agency must NOT override Price. Commercial terms = Notes + SalesRules metadata. No Money / Discount / Commission / Currency / Quote. |
| **P13-R5** | Capacity/availability policy owner | **RESOLVED** | Agency does NOT own capacity. TourDeparture remains capacity SoR. Offer may hold SalesAvailability metadata + optional logical TourDeparture Guid. No seats / reservation / allocation. |
| **P13-R6** | Agency Panel ownership for Marketplace ops | **UNRESOLVED** | P03 Panel is presentation-only. P12 set Admin Pricing inside Pricing module, not Tour Admin. |
| **P13-R7** | Publishing / moderation of agency offers | **UNRESOLVED** | Roadmap says «در صورت نیاز». DEFER is valid. |

---

## 6. Architecture invariants (carry forward)

1. Agency ≠ Party · Agency ≠ Pricing · Agency ≠ Booking.
2. TourProduct ≠ TourDeparture.
3. Published Departure ≠ Bookable.
4. Price ≠ Quote ≠ Payment / Booking Amount.
5. P09-R3: Tour→Agency is optional logical Guid; Party is identity SoR; no cross-schema FK.
6. Agency Panel (P03) is presentation/capability, not commerce SoR — until P13-R6 lock.
7. No Booking/Payment/Search/FX engines in P13.
8. Do not duplicate TourProduct as a second catalog.

---

## 7. Continuity

After `TC-P13-GATE` ACCEPT, continuity may auto-start **P14 PLAN** (Public Tour Experience) unless a real Stop Condition applies.

**Pipeline rule (USER lock):** ceremonial Gate is **not** a stop. Cursor stays in PIPELINE and continues.

---

## 8. PLAN acceptance criteria

- [x] Phase purpose + non-goals explicit
- [x] Task sequence proposed without product code
- [x] Open decisions listed (R1–R7) — no invention
- [x] Baseline = P12 Gate ACCEPT commit `b372367`
- [x] Architect ACCEPT plan + lock **P13-R1** (independent Agency Marketplace module) then Auto-Execute `TC-P13-T001`
