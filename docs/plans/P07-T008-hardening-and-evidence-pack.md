# P07 Evidence Pack — TC-P07-T008

**Task:** TC-P07-T008 — Phase hardening tests & evidence pack
**Baseline HEAD:** `b47f6de` (`docs: record TC-P07-T007 commit SHA 1c76f6b`)
**Date:** 2026-08-16
**Scope:** Validation / evidence + ArchitectureTests boundary hardening only — **no new product scope**; **gate not executed**; **P08 not started**; **R3 not closed by invention**.

## 1. Capability matrix (product commits)

| Task | Commit | Capability |
|------|--------|------------|
| PLAN | `5dbc152` | Authoritative P07 Place Catalog plan |
| T001 | `108ac34` | Place module scaffolding · schema `place` |
| T002 | `83529cf` | Place catalog domain + persistence baseline (R1) |
| T002-R1 | `0b86f05` | PlaceId identity + T002 scope reconciliation (docs-only) |
| T003 | `3ec0f4c` | Localization + optional DestinationId + geo/address (R2) |
| T004 | `6258003` | Facilities · classification · `PlaceCatalogStatus` Draft/Active/Inactive |
| T005 | `6246a09` | Place↔Media Cover/Gallery · `place_media_links` |
| T006 | `74e8540` | Access `place.places.write` + Admin `/[locale]/admin/catalog/places` |
| T006-R1 | `e4b5201` | Admin Ready-media visual picker (no MediaAssetId paste primary) |
| T007 | `1c76f6b` | Public Place detail + SEO hooks · `PlaceTranslation.Slug` · default noindex,follow |
| T008 | *(this commit)* | Hardening battery + evidence pack for gate prep |

Architect acceptance of T002–T007 remains as issued (`AWAITING_ARCHITECT_REVIEW` until ACCEPT). T008 prepares gate evidence; it does **not** auto-ACCEPT prior tasks.

## 2. Ownership invariants

| Invariant | Posture |
|-----------|---------|
| Hotel Catalog ≠ Hotel Booking | Proven — no reservation/inventory/rate/voucher product signals in Place (ArchitectureTests + domain comments) |
| Place ≠ Destination | Proven — optional nullable `DestinationId` logical ref; Contracts existence query; no cross-schema FK / EF navigation |
| Place owns gallery meaning | Proven — `PlaceMediaRole` Cover/Gallery only; MediaAssetId refs; Media.Contracts readiness/presentation |
| Place owns current locale slug | Proven — `PlaceTranslation.Slug`; no global `Place.Slug` / SlugFa/SlugEn (ADR 0008) |
| SEO owns route history / redirects / IndexPolicy | Proven — Place has no PreviousSlug/RedirectTo/HistoricalSlug; Place↛SEO.Infrastructure; SEO publication binds `places/{slug}` without flipping IndexPolicy |
| Default Place SEO posture | Proven — missing/default ⇒ **noindex, follow**; Active/public/publish ≠ Index; no Destination IndexPolicy inheritance |
| Cover + ordered Gallery | Proven — T005 + public composition via app-proxy (no StorageKey; no Hero) |
| Optional DestinationId | Proven — R2 optional 0..1 |
| No cross-schema FK | Proven — Place schema `place`; Destination/Media/SEO refs are logical / contracts only |
| Catalog status ≠ delete/archive | Proven — Draft/Active/Inactive only; **P07-R3 UNRESOLVED** (no delete/archive product) |

## 3. Locked decisions R1–R5

| ID | Decision | Classification | Gate posture |
|----|----------|----------------|--------------|
| R1 | Place model shape | **RESOLVED** | Core Place + typed Hotel/Restaurant/Attraction specialization; `PlaceId` only; no TPH; no HotelBooking fields |
| R2 | Destination link | **RESOLVED** | Optional single logical reference 0..1; Place-owned nullable DestinationId; no cross-schema FK |
| R3 | Place delete/archive lifecycle | **UNRESOLVED** | **OK for gate prep** — catalog status is not Deleted/Archived; **no** delete/archive product invented |
| R4 | Slug ownership | **RESOLVED** | PLACE owns current `PlaceTranslation.Slug`; SEO owns binding/history/redirects/IndexPolicy |
| R5 | Public IndexPolicy default | **RESOLVED** | Default **noindex, follow**; Index requires explicit SEO authority |

### R3 explicit statement (gate prep)

**P07-R3 remains UNRESOLVED and that is acceptable for `TC-P07-GATE` preparation** because:

1. Product scope of P07 does not include Place hard-delete / archive / soft-delete lifecycle.
2. `PlaceCatalogStatus` {Draft, Active, Inactive} is catalog operational status only — it does **not** resolve R3.
3. Repo guardrails forbid Delete/Archive HTTP, Admin delete UX, and domain delete/archive signals.
4. Closing R3 requires a future architect decision; inventing delete/archive semantics in T008 is forbidden.

## 4. ArchitectureTests posture (T008)

| Guard | Status |
|-------|--------|
| Schema `place` ownership | PASS (`ArchitectureGuardrailTests`) |
| Destination link contracts-only / no cross-schema FK | PASS (`PlaceDestinationLinkGuardrailTests`) |
| Place↔Media Cover/Gallery · no StorageKey · Media.Contracts | PASS (`PlaceMediaRelationGuardrailTests`) |
| Catalog status closed set · no R3 / bookable-now | PASS (`PlaceCatalogOpsGuardrailTests`) |
| Current Slug allowed · redirect history forbidden on Place | PASS (`PlaceCatalogOpsGuardrailTests`) |
| Admin Access write · no delete · Ready picker | PASS (`PlaceAdminAccessGuardrailTests`) |
| **T008** Place↛SEO.Infrastructure/Domain · Domain purity | PASS (`PlaceCatalogBoundaryGuardrailTests`) |
| **T008** Hotel booking product signals forbidden | PASS (`PlaceCatalogBoundaryGuardrailTests`) |
| **T008** No global Place.Slug · translation owns Slug | PASS (`PlaceCatalogBoundaryGuardrailTests`) |
| **T008** Public Place detail app-proxy · no Admin/IndexPolicy leak | PASS (`PlaceCatalogBoundaryGuardrailTests`) |

## 5. Validation battery (this task)

| Suite | Result | Detail |
|-------|--------|--------|
| ArchitectureTests | **PASS** | **45** passed (was 40; +5 T008 boundary tests) |
| Place.UnitTests | **PASS** | **26** passed |
| Seo.UnitTests | **PASS** | **41** passed |
| Access.UnitTests | **PASS** | **5** passed |
| Media.UnitTests | **PASS** | **67** passed |
| Persistence.IntegrationTests | **PASS** | **19** passed |
| Host.IntegrationTests | **PASS** | **36** passed |
| Frontend `npm run quality` | **PASS** | lint · typecheck · build · 12 node tests · p02 PASS |
| `git diff --check` | **PASS** | clean |
| HEAD == origin/main (preflight) | **PASS** | `b47f6de` · clean tree |

## 6. Host / authz evidence (representative)

| Area | Proof |
|------|-------|
| Admin Place Access write | Place host/admin surfaces + `Access.Place.Places.Write` |
| SEO Place publication Access | `SeoPlacePublicationHostTests` · `seo.place-posture.write` |
| Place migrations | `PlaceMigrationLifecycleTests` (incl. translation Slug) |
| Public Active-only slug | Place application `FindBySlugAsync` publicOnly + public page notFound |

## 7. Frontend evidence

| Surface | Path / note |
|---------|-------------|
| Admin Place | `/[locale]/admin/catalog/places` — catalog ops + Ready media picker + slug/SEO publish hooks |
| Public Place | `/[locale]/places/[slug]` — Server Component · Cover + Gallery · Destination compose · default noindex,follow |
| Quality gates | `p02-quality-checks.mjs` includes public Place checks |

## 8. Gate checklist preview (plan §10)

| # | Criterion | Evidence posture |
|---|-----------|------------------|
| 1 | Place module + separate schema / no cross-schema writes | Architecture + Persistence |
| 2 | Hotel/Restaurant/Attraction under Place ownership | T002 + R1 |
| 3 | Localization without NameFa/NameEn | T003 + Architecture |
| 4 | Destination relationship by reference; Destination ≠ Place | T003 + R2 |
| 5 | Geo/address on Place | T003 |
| 6 | Facilities/classification/catalog status | T004 |
| 7 | Place↔Media owned by Place; MediaAssetId refs | T005 |
| 8 | Admin Place Access-backed; job-based UX | T006 + T006-R1 |
| 9 | Public Place detail baseline | T007 |
| 10 | SEO hooks without SEO owning Place text | T007 + R4/R5 |
| 11 | No HotelBooking / live inventory / reservation / voucher | Architecture T008 + domain |
| 12 | Evidence pack + tests green + clean tree | **this document** |

## 9. Deferred / known non-blocking limitations

| Item | Note |
|------|------|
| P07-R3 delete/archive | Unresolved; **no delete product behavior** — OK for gate prep |
| P06-R8 Media delete | Unresolved (Media); out of Place product scope |
| P06-R9 Consumer alt override | Deferred; Place uses Media defaults |
| HotelBooking / inventory | Explicitly out of P07 |
| P08 CMS | **NOT_STARTED** |
| Architect ACCEPT of T002–T007 | Still as issued; gate requires ACCEPTs + USER token |

## 10. Ready for gate?

**YES — evidence pack ready for `TC-P07-GATE`.**

This task does **not**:

- execute `TC-P07-GATE`
- mark P07 COMPLETE
- start P08 / Tour / HotelBooking
- silently resolve P07-R3

Gate still requires architect acceptance of remaining tasks (as issued) and USER token `TRAVELCORE_TASK_CONFIRM: TC-P07-GATE` when the architect issues the gate.
