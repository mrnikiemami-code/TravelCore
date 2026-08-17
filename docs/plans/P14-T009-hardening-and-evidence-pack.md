# TC-P14-T009 — Public Experience hardening tests & evidence pack

**Task:** TC-P14-T009 — P14 hardening tests and evidence pack  
**Product HEAD:** `a0209bd` (`TC-P14-T008` **ACCEPTED**)  
**Date:** 2026-08-17  
**Scope:** Hardening + evidence **only** — no new product capability.  
**Forbidden in this task:** Booking · Payment · Search engine · Recommendation · AI infrastructure · P15 implementation · new domain ownership.  
**Not this task:** `TC-P14-GATE` (evidence pack only; Gate is next).

## 1. Mission checklist

| # | Verify | Result |
|---|--------|--------|
| 1 | Public Experience owns Detail / Listing / Landing presentation only (P14-R1) | **PASS** — T001 |
| 2 | Sticky Action ≠ Booking (P14-R2) | **PASS** — T002 |
| 3 | Listing ≠ SEO Landing (P14-R3) | **PASS** — T003 |
| 4 | Shared Detail shell + kind-specific sections (P14-R4) | **PASS** — T004 |
| 5 | Related Tours = composition, not recommendation (P14-R5) | **PASS** — T005 |
| 6 | Content enrichment = composition; Content remains CMS SoT (P14-R6) | **PASS** — T006 |
| 7 | AgencyOffer presentation = inquiry only; Marketplace owns facts (P14-R7) | **PASS** — T007 |
| 8 | Filter presentation ≠ Search faceting (P14-R8) | **PASS** — T008 |
| 9 | P14-R1…R8 all RESOLVED | **PASS** — plan open-decisions table |
| 10 | No new product capability in this task | **PASS** — evidence/docs + phase boundary guardrails only |

## 2. Accepted product commits (P14)

| Task | Commit | Essence |
|------|--------|---------|
| PLAN | `cc3ed8b` | Authoritative P14 plan · R1–R8 listed |
| T001 | `a7bd549` | Public Experience surface inventory — P14-R1 |
| T002 | `99818dd` | Sticky presentation (Sticky Action ≠ Booking) — P14-R2 |
| T003 | `f0e3df3` | Listing vs SEO Landing boundary — P14-R3 |
| SYNC001 | `f0e3df3` | origin/main synchronized |
| T004 | `0b4fcbe` | Shared Detail shell + Experience sections — P14-R4 |
| T005 | `c34e5b0` | Related Tours composition — P14-R5 |
| T006 | `5258e20` | Destination-based content enrichment — P14-R6 |
| T007 | `903cd29` | Public AgencyOffer presentation — P14-R7 |
| T008 | `a0209bd` | Filter presentation boundary — P14-R8 **ACCEPTED** |

Architect acceptance of T001–T008 is as issued. T009 prepares gate evidence; it does **not** execute `TC-P14-GATE`.

## 3. Locked decisions (all RESOLVED)

| ID | Essence |
|----|---------|
| **P14-R1** | Public Experience Layer owns Detail/Listing/Landing presentation. Not Search. Not Catalog. P15 owns Query/Ranking/FTS |
| **P14-R2** | Sticky Action ≠ Booking. Allowed View Departure / View Price / Request Information. Forbidden Book Now / Pay Now / Reserve Seat / Checkout |
| **P14-R3** | Listing = Discovery; Landing = Search Intent; Landing ≠ filtered listing; SEO owns IndexPolicy |
| **P14-R4** | Shared Shell + kind-specific sections. Not independent Experience/Package pages. Not a giant union ViewModel |
| **P14-R5** | Related ≠ Recommendation. Deterministic shared-destination retrieval behind Tour public-read |
| **P14-R6** | Content = editorial SoT. Tour = tour-facts SoT. PE = composition only. Destination-based. Content publication ≠ IndexPolicy |
| **P14-R7** | AgencyOffer may be displayed; does not own commercial flow. Marketplace owns facts. Inquiry-oriented. No agency prices / ranking |
| **P14-R8** | Filter in P14 = Presentation only. Faceting / retrieval / ranking / FTS = P15 Search. Filtered URLs ≠ SEO landings |

## 4. Boundary / ownership matrix

| Concern | Owner | P14 posture |
|---------|-------|-------------|
| Public Detail / Listing / Landing surfaces | **PublicExperience** | Presentation + SEO composition |
| Tour catalog facts / CatalogStatus | **Tour** | PE composes reads only |
| Editorial Content | **Content** | Destination-linked enrichment; not copied into Tour |
| AgencyOffer / publication | **AgencyMarketplace** | Inquiry presentation only on Detail |
| Price public summary | **Pricing** | Read model; not Booking |
| SEO IndexPolicy / canonical | **Seo** | Filtered URLs do not own IndexPolicy |
| Sticky actions | **PublicExperience** | Contact/Request — not Book Now |
| Search / Faceting / Ranking / FTS | **Search (P15)** | Not implemented in P14 |
| Booking / Payment | **Out of P14** | Modules do not exist |

## 5. Invariant evidence (T001–T008)

### 5.1 Presentation-only module

- PublicExperience.Contracts only (no Domain/Infrastructure/DbContext).
- No project-reference to Tour/Seo/Pricing/Search/Booking/Payment/AgencyMarketplace.

### 5.2 Sticky ≠ Booking

- `detail-sticky-actions.tsx`: View departures / View price / Request information.
- Architecture forbids Book Now / Checkout / `/api/booking` on sticky chrome.

### 5.3 Listing ≠ SEO Landing

- `/tours` Discovery vs `/tours/{topic}/{intent}` Search Intent.
- `LANDING_IS_FILTERED_LISTING = false`.

### 5.4 Related / Content / Agency composition

- Related Tours / Related Content / AgencyOffer: PE presentation; fact owners elsewhere.
- Deterministic replaceable public reads; max 6; no score/popularity/`pg_trgm`.

### 5.5 Filters ≠ Faceting

- URL/query state + GET form + selection presentation.
- Reuses Tour `related-published` after Destination by-slug.
- Listing SEO metadata remains path `tours` (no filter IndexPolicy ownership).

## 6. Guardrail / test surfaces

| Area | Evidence |
|------|----------|
| Surface / ownership / sticky / listing-landing / related / content / agency / filters | `PublicExperienceBoundaryGuardrailTests` |
| **T009 phase boundary** | `PublicExperiencePhaseBoundaryGuardrailTests` |
| Contracts smoke | `PublicExperience.UnitTests` |
| Tour / Content / AgencyMarketplace public eligibility | respective unit suites |
| Frontend | `tsc --noEmit` |

## 7. Validation battery (T009 re-run)

| Suite | Result | Detail |
|-------|--------|--------|
| `dotnet build TravelCore.sln` | **PASS** | 0 Error(s) |
| PublicExperience.UnitTests | **PASS** | **9** |
| ArchitectureTests | **PASS** | **179** (incl. +4 T009 `PublicExperiencePhaseBoundaryGuardrailTests`) |
| Frontend `tsc --noEmit` | **PASS** | clean |
| `git diff --check` | **PASS** | clean |

## 8. Explicit OUT / DEFER

- Search engine / FTS / faceting / ranking — **P15**
- Booking / Payment — **later phases**
- Recommendation / personalization / AI embeddings — **not invented**
- Package specialized Detail sections — **future contributor**
- Agency commercial flow / commission / checkout — **not invented**
- Filter IndexPolicy / programmatic SEO factory — **SEO + P15; not PE**

## 9. Pitfalls (do not regress)

1. Do not treat Filtered URLs as SEO Landings.
2. Do not invent Search/FTS/`pg_trgm` inside PublicExperience.
3. Do not add Book Now / Checkout to sticky or Agency Information.
4. Do not copy Content CMS into TourProduct.
5. Do not make AgencyOffer own Price or Booking.
6. Do not invent P15 in this hardening task.

## 10. Ready for Gate

After architect ACCEPT of T009 → Auto-Execute **TC-P14-GATE** (architect statement). This pack does **not** write GATE evidence and does not mark P14 COMPLETE. P15 PLAN may auto-start after Gate ACCEPT under continuity override. Ceremonial Gate wait is **not** a pipeline stop.
