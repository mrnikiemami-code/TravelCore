# TC-P13-T009 — Agency Marketplace hardening tests & evidence pack

**Task:** TC-P13-T009 — P13 hardening tests and evidence pack  
**Product HEAD:** `98ea1d1` (`TC-P13-T007` **ACCEPTED**)  
**Date:** 2026-08-17  
**Scope:** Hardening + evidence **only** — no new product capability.  
**Forbidden in this task:** SEO ownership · Commission · Payment · Settlement · Ranking · Booking · Price override.  
**Not this task:** `TC-P13-GATE` (evidence pack only; Gate is next).

## 0. T008 vacant (architect reorder)

Original plan T007 was “Agency Panel operational baseline” (delivered as **T006** / P13-R6).  
Original plan T008 was “Publishing / moderation” (delivered as **T007** / P13-R7).  

**TC-P13-T008 has no remaining independent capability.** It is **vacant** — do not invent Admin/Public extra surfaces. Hardening is this T009 task.

## 1. Mission checklist

| # | Verify | Result |
|---|--------|--------|
| 1 | Independent Agency Marketplace module · schema `agency_marketplace` (P13-R1) | **PASS** — T001 |
| 2 | AgencyProfile 0..1 over Party identity; logical PartyId only (P13-R2) | **PASS** — T002 |
| 3 | AgencyOffer owns sales relationship; TourProduct remains catalog SoR (P13-R3) | **PASS** — T003 |
| 4 | Agency must NOT override Price; Notes + SalesRules only (P13-R4) | **PASS** — T004 |
| 5 | Agency does NOT own capacity; SalesOpen + logical Departure Guid (P13-R5) | **PASS** — T005 |
| 6 | Agency Panel owned by Marketplace, not Tour Admin / Identity (P13-R6) | **PASS** — T006 |
| 7 | Offer publication owned by Marketplace; ≠ SEO IndexPolicy; ≠ CatalogStatus (P13-R7) | **PASS** — T007 |
| 8 | P13-R1…R7 all RESOLVED | **PASS** — plan open-decisions table |
| 9 | No new domain entities / product features in this task | **PASS** — evidence/docs + phase boundary guardrails only |
| 10 | Published Offer ≠ SEO Indexed | **PASS** — T007 + phase boundary |

## 2. Accepted product commits (P13)

| Task | Commit | Essence |
|------|--------|---------|
| PLAN | `2b40e1c` | Authoritative P13 plan · R1–R7 listed |
| T001 | `9f61763` | Agency Marketplace scaffolding (`agency_marketplace` schema) — P13-R1 |
| T002 | `809eb49` | AgencyProfile commercial layer — P13-R2 |
| T003 | `a665272` | AgencyOffer marketplace listing — P13-R3 |
| T004 | `87931d9` | Commercial terms boundary (no Price override) — P13-R4 |
| T005 | `7234cc1` | Capacity boundary (no seats) — P13-R5 |
| T006 | `8098a24` | Agency Marketplace panel baseline — P13-R6 |
| T007 | `98ea1d1` | Offer publishing / moderation — P13-R7 **ACCEPTED** |
| T008 | — | **vacant** (publishing delivered as T007) |

Architect acceptance of T001–T007 is as issued. T009 prepares gate evidence; it does **not** execute `TC-P13-GATE`.

## 3. Locked decisions (all RESOLVED)

| ID | Essence |
|----|---------|
| **P13-R1** | Independent Agency Marketplace module · schema `agency_marketplace` · Party remains identity SoR · logical PartyId Guid only |
| **P13-R2** | Party = identity SoR; Marketplace owns AgencyProfile 0..1 per Agency PartyId |
| **P13-R3** | AgencyOffer owns the sales relationship; TourProduct remains catalog SoR; logical TourProduct Guid; no Tour FK |
| **P13-R4** | Agency must NOT override Price. Commercial terms = Notes + SalesRules. No Money / Discount / Commission / Currency / Quote |
| **P13-R5** | Agency does NOT own capacity. TourDeparture remains Min/Max Pax SoR. Offer may hold SalesOpen + optional logical TourDeparture Guid |
| **P13-R6** | Agency Panel belongs to Agency Marketplace (not Tour Admin, not Identity). Profile + offer management. No Booking/Payment/Commission/CRM |
| **P13-R7** | Marketplace owns Offer publication status. Draft → Submitted → Approved → Published; Rejected/Archived returns. **Published Offer ≠ SEO Indexed** |

## 4. Boundary / ownership matrix

| Concern | Owner | P13 posture |
|---------|-------|-------------|
| Agency identity (`PartyKind.Agency`) | **Party** | Logical Guid ref only |
| Tour catalog / CatalogStatus | **Tour** | TourProduct remains catalog SoR |
| TourDeparture capacity | **Tour** | Min/Max Pax; Offer does not own seats |
| Price / Quote | **Pricing** | No Agency PriceOverride |
| Offer publication | **AgencyMarketplace** | `AgencyOfferPublicationStatus` |
| SEO IndexPolicy / routes | **SEO** | Marketplace must not set IndexPolicy |
| Agency Panel ops | **AgencyMarketplace** | Access-backed profile/offer/publish |
| Booking / Payment / Commission / Ranking | **Out of P13** | Forbidden; Booking/Payment modules do not exist |

## 5. Invariant evidence (T001–T007)

### 5.1 Independent module

- Contracts / Domain / Infrastructure projects; schema `agency_marketplace`.
- No project-reference to Party/Tour/Pricing/Booking/Payment.
- Persistence: 6 migrations; zero FK into `party` / `tour` / `pricing`.

### 5.2 Agency ≠ Party

- `AgencyProfile.PartyId` is `MarketplacePartyId` (logical Guid).
- Unique 0..1 `ux_agency_profiles_party_id`.
- Party schema unchanged.

### 5.3 AgencyOffer ≠ TourProduct

- Logical `TourProductId` Guid; same-schema FK to AgencyProfile only.
- Unique `(agency_profile_id, tour_product_id)`.

### 5.4 No Price override / no capacity

- Commercial terms: Notes + `RequiresManualConfirmation` / `ExclusiveListing`.
- Sales availability: `SalesOpen` + optional `referenced_tour_departure_id`.
- No amount/currency/commission/seats columns.

### 5.5 Panel ownership

- `/api/agency-marketplace/profiles` + `/offers` in Marketplace module.
- Access: `agency.marketplace.profile.*` · `offers.read/write` · `offers.moderate` (admin-only).
- Tour Admin endpoints do not map marketplace routes.

### 5.6 Publication ≠ SEO ≠ Catalog

- `AgencyOfferPublicationStatus`: Draft / Submitted / Approved / Published / Rejected / Archived.
- Write: submit / publish / unpublish. Moderate: approve / reject.
- No `IndexPolicy` / `CatalogStatus` on Offer. Agency panel copy states **Published Offer ≠ SEO Indexed**.

## 6. Guardrail / test surfaces

| Area | Evidence |
|------|----------|
| Module / Offer / Profile / capacity / money | `AgencyMarketplaceBoundaryGuardrailTests` |
| Panel Access ownership | `AgencyMarketplacePanelAccessGuardrailTests` |
| **T009 phase boundary** | `AgencyMarketplacePhaseBoundaryGuardrailTests` |
| Domain unit | AgencyMarketplace.UnitTests (Offer publication workflow) |
| Access catalog | Access.UnitTests (`offers.moderate` admin-only) |
| Persistence | `AgencyMarketplaceMigrationLifecycleTests` (6 migrations · `publication_status` · no peer FK) |
| Host | `AgencyMarketplacePanelAccessTests` (authz + publication lifecycle) |

## 7. Validation battery (T009 re-run)

| Suite | Result | Detail |
|-------|--------|--------|
| `dotnet build` (ArchitectureTests restore/build) | **PASS** | 0 Error(s) |
| AgencyMarketplace.UnitTests | **PASS** | **16** |
| ArchitectureTests | **PASS** | **164** (incl. +4 T009 `AgencyMarketplacePhaseBoundaryGuardrailTests`) |
| Persistence.IntegrationTests | **PASS** | **24** |
| Host.IntegrationTests | **PASS** | **45** |
| Frontend `tsc --noEmit` | **PASS** | clean |
| `git diff --check` | **PASS** | clean |

**Total this battery (core):** 16 + 164 + 24 + 45 = **249** passed (plus FE tsc).

## 8. Explicit OUT / DEFER

- Booking engine / reservation / hold / inventory — **later (P19)**
- Payment capture / settlement / commission ledger — **later (P20/P24)**
- SEO IndexPolicy for agency offers — **SEO module; not Marketplace**
- Marketplace ranking engine — **not invented**
- AgencyAllocation / seat share — **DEFER** (P13-R5)
- Full SaaS Agency portal (CRM / financial reports) — **not invented**
- Public polish factory — **P14**
- Search indexing — **P15**
- **T008 extra Admin/Public integration** — **vacant; do not invent**

## 9. Pitfalls (do not regress)

1. Do not treat Published Offer as SEO Indexed or as bookable.
2. Do not add a Tour/Party/Pricing schema FK “to be helpful”.
3. Do not put Price / Commission / seats on AgencyOffer.
4. Do not move Agency Panel under Tour Admin or Identity.
5. Do not fill vacant T008 with invented product work.

## 10. Ready for Gate

After architect ACCEPT of T009 → Auto-Execute **TC-P13-GATE** (architect statement). This pack does **not** write GATE evidence and does not mark P13 COMPLETE. P14 PLAN may auto-start after Gate ACCEPT under continuity override. Ceremonial Gate wait is **not** a pipeline stop.
