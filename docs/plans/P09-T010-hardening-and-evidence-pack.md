# P09 Evidence Pack — TC-P09-T010

**Task:** TC-P09-T010 — Public Tour detail hardening + phase evidence pack  
**Baseline HEAD:** `e1fc751` (`feat(tour): Access-backed Admin Tour catalog baseline [TC-P09-T009]`)  
**Date:** 2026-08-17  
**Scope:** Public Tour media presentation compose · Server Component hardening · ArchitectureTests · evidence documentation — **gate not executed**; **P10/P11 not started**.

## 1. Capability matrix (product commits)

| Task | Commit | Capability |
|------|--------|------------|
| PLAN | `7de2518` | Authoritative P09 Tour Core plan · R1–R8 listed |
| T001 | `4794e6e` | Tour module scaffolding · schema `tour` |
| T002 | `a70331c` | TourProduct shared-core + persistence (R1/R7) |
| T003 | `0bd50de` | Localization title/description locale rows (ADR 0008) |
| T004 | `32a4701` | Classification · Origin · Destination links (R2) |
| T005 | `855f7a3` | Agency reference AgencyId 0..1 (R3) |
| T006 | `7e7ba6d` | Services · Policies · Requirements baseline |
| T007 | `f0777f1` | Tour↔Media Cover/Gallery (R8) |
| T008 | `69e8f38` | Publishing status · localized slug · public/SEO hooks (R4/R5/R6) |
| T009 | `e1fc751` | Access-backed Admin Tour catalog baseline |
| T010 | *(this commit)* | Public media presentation harden + evidence pack |

Architect acceptance of T001–T009 remains as issued until ACCEPT for T010. T010 prepares gate evidence; it does **not** execute `TC-P09-GATE`.

## 2. Ownership invariants

| Invariant | Posture |
|-----------|---------|
| Tour schema separate | Proven — `TourDbContext` schema `tour`; ArchitectureTests schema ownership |
| No cross-schema FK writes | Proven — Destination/Party/Media/SEO refs logical / Contracts only |
| TourProduct ≠ TourDeparture | Proven — no TourDeparture/FlightSegment/TourHotelOption product (T010 boundary) |
| Tour ≠ Pricing / Booking / Search | Proven — T010 boundary + plan forbidden list |
| Tour ≠ Place Hotel ownership | Proven — Place remains Place; Tour owns TourProduct only |
| Tour owns current locale slug | Proven — `TourProductTranslation.Slug`; path `tours/{slug}` (P09-R5) |
| SEO owns route history / redirects / IndexPolicy | Proven — Tour↛SEO.Infrastructure/Domain; publication does not `SetIndexPolicy` |
| Default Tour SEO posture | Proven — missing/default ⇒ **noindex, follow**; Published ≠ Index (P09-R6) |
| Cover + ordered Gallery | Proven — T007 + public compose via `/media/presentation` (no StorageKey; no Hero) |
| Catalog status ≠ delete/archive | Proven — Draft \| Published \| Inactive (P09-R4); no hard-delete product |
| Specialty fields deferred | Proven — Experience/Package specialty **DEFERRED** (P09-R7) |

## 3. Locked decisions R1–R8

| ID | Decision | Classification | Gate posture |
|----|----------|----------------|--------------|
| R1 | TourProduct model shape | **RESOLVED** | Core + Typed Specialization · `TourProductId` · Departure separate future aggregate |
| R2 | Destination / Origin cardinality | **RESOLVED** | Destinations 0..N · Origin 0..1 · logical refs only |
| R3 | Agency reference | **RESOLVED** | optional AgencyId 0..1 · PartyKind.Agency via Contracts |
| R4 | Publishing vs delete-archive | **RESOLVED** | Draft \| Published \| Inactive · no hard-delete in P09 |
| R5 | Slug ownership | **RESOLVED** | Tour translation owns current slug · SEO owns history/IndexPolicy |
| R6 | Public IndexPolicy default | **RESOLVED** | Default **noindex, follow** · Published ≠ Index |
| R7 | Experience/Package specialty | **RESOLVED** | Specialty **DEFERRED** to P10/P11 |
| R8 | Media relation policy | **RESOLVED** | Cover 0..1 · Gallery 0..N · MediaAssetId only · no StorageKey |

All P09-R1–R8 are **RESOLVED** — acceptable for `TC-P09-GATE` preparation. No unresolved R# remains open inside P09 product scope.

## 4. ArchitectureTests posture (T010)

| Guard | Status |
|-------|--------|
| Schema `tour` ownership | PASS (`ArchitectureGuardrailTests`) |
| Localization without TitleFa/TitleEn | PASS (`TourLocalizationGuardrailTests`) |
| Destination/Origin contracts-only | PASS (`TourDestinationLinkGuardrailTests`) |
| Agency contracts-only | PASS (`TourAgencyLinkGuardrailTests`) |
| Catalog facts descriptive only | PASS (`TourCatalogFactGuardrailTests`) |
| Cover/Gallery · no StorageKey · Media.Contracts | PASS (`TourMediaRelationGuardrailTests`) |
| Catalog status closed · SEO publication no SetIndexPolicy | PASS (`TourCatalogOpsGuardrailTests`) |
| Admin Access write · no delete invent | PASS (`TourAdminAccessGuardrailTests`) |
| **T010** Tour↛SEO.Infrastructure/Domain | PASS (`TourPublicDetailBoundaryGuardrailTests`) |
| **T010** No TourDeparture/Booking/Pricing/Search product | PASS (`TourPublicDetailBoundaryGuardrailTests`) |
| **T010** Public page P09-R6 · robotsFromComposed · no SetIndexPolicy | PASS (`TourPublicDetailBoundaryGuardrailTests`) |
| **T010** App-proxy media presentation · no StorageKey | PASS (`TourPublicDetailBoundaryGuardrailTests`) |

## 5. Validation battery (this task)

| Suite | Result | Detail |
|-------|--------|--------|
| Tour.UnitTests | **PASS** | **32** passed |
| ArchitectureTests | **PASS** | **93** passed (includes +4 T010 public Tour boundary tests) |
| Host.IntegrationTests | **PASS** | **40** passed |
| Frontend `npx tsc --noEmit` | **PASS** | `src/frontend/web` |
| `git diff --check` | **PASS** | clean |
| HEAD baseline | **PASS** | `e1fc751` · then this commit |

## 6. Host / authz evidence (representative)

| Area | Proof |
|------|-------|
| Admin Tour Access write | `TourAdminAccessGuardrailTests` · `Access.Tour.Products.Write` |
| SEO Tour publication Access | `SeoTourProductPublicationService` · no `SetIndexPolicy` |
| Public Published-only slug | Tour `FindBySlugAsync` publicOnly + public page notFound for Draft/Inactive |

## 7. Frontend evidence

| Surface | Path / note |
|---------|-------------|
| Admin Tour | `/[locale]/admin/catalog/tours` — Access-backed catalog ops + Ready media picker |
| Public Tour | `/[locale]/tours/[slug]` — Server Component · Cover + Gallery · default noindex,follow |
| Media compose | `GET /api/tour/products/{id}/media/presentation?locale=` · app-proxy only |
| Typecheck | `npx tsc --noEmit` PASS |

## 8. Gate checklist preview (plan §10)

| # | Criterion | Evidence posture |
|---|-----------|------------------|
| 1 | Tour module + separate schema / no cross-schema writes | Architecture + T010 boundary |
| 2 | TourProduct shared core under Tour ownership | T002 + R1 |
| 3 | Localization without TitleFa/TitleEn | T003 + Architecture |
| 4 | Classification / Origin / Destination by reference | T004 + R2 |
| 5 | Agency by Party/Agency ID | T005 + R3 |
| 6 | Services / Policies / Requirements | T006 |
| 7 | Tour↔Media owned by Tour; MediaAssetId refs | T007 + R8 + T010 presentation |
| 8 | Publishing + Admin Tour Access-backed | T008/T009 + R4 |
| 9 | Public Tour Core hooks baseline | T008 + T010 media harden |
| 10 | SEO hooks without SEO owning Tour body; R6 default | T008 + T010 public page guards |
| 11 | No TourDeparture / FlightSegment / TourHotelOption / itinerary | Architecture T010 |
| 12 | No Pricing · Booking · Search · Content CMS ownership | Architecture T010 |
| 13 | Experience/Package not unlocked nullable blob | R7 deferred |
| 14 | Evidence pack + tests green + clean tree | **this document** |

## 9. Deferred / known non-blocking limitations

| Item | Note |
|------|------|
| P10 Experience itinerary | Explicitly out of P09 |
| P11 Package Departure / Flight / HotelOption | Explicitly out of P09 |
| Pricing / Booking / Search | Explicitly out of P09 |
| P08-R6/R7/R8 (Content) | Out of Tour product scope |
| Architect ACCEPT of T010 | Pending after this pack; gate requires ACCEPTs |

## 10. Ready for gate?

**YES — evidence pack ready for `TC-P09-GATE`.**

This task does **not**:

- execute `TC-P09-GATE`
- mark P09 COMPLETE
- start P10 / P11 / Pricing / Booking / Search
- invent TourDeparture product behavior

Gate still requires architect acceptance of remaining tasks (as issued). Ceremonial `TRAVELCORE_TASK_CONFIRM: TC-P09-GATE` is **not required** under USER continuity override (2026-08-17); stop only for architecture/path/SoT/unsafe/unlocked-decision.
