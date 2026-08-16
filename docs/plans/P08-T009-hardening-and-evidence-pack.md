# P08 Evidence Pack — TC-P08-T009

**Task:** TC-P08-T009 — Phase hardening tests & evidence pack  
**Baseline HEAD:** `5a70dbe` (`docs: mark TC-P08-T008 ACCEPTED; await T009 Auto-Execute`)  
**Date:** 2026-08-17  
**Scope:** Validation / evidence + ArchitectureTests boundary hardening only — **no new product scope**; **gate not executed**; **P09 not started**; **R6/R7/R8 not closed by invention**.

## 1. Capability matrix (product commits)

| Task | Commit | Capability |
|------|--------|------------|
| PLAN | `7012fe0` | Authoritative P08 Content CMS plan |
| T001 | `1b4a871` | Content module scaffolding · schema `content` |
| T002 | `300b86b` | ContentItem + Article/LandingPage/Guide persistence (R1) |
| T003 | `ec3ad71` | Localization title/body/excerpt locale rows (ADR 0008) |
| T004 | `c2b17a2` | Category/Tag taxonomy (Author deferred; R7 open) |
| T005 | `f66458b` | Relational Content Blocks engine (R2; no widgets / R6) |
| T006 | `4e9c94e` | Destination logical links 0..N (R5; no cross-schema FK) |
| T007 | `6a56a0d` | Access `content.items.write` + Admin Content baseline |
| T008 | `4924892` | Public Article/LandingPage + SEO hooks · slug SoR · default noindex,follow |
| T009 | *(this commit)* | Hardening battery + evidence pack for gate prep |

Architect acceptance of T001–T008 remains as issued (`AWAITING_ARCHITECT_REVIEW` until ACCEPT for T009). T009 prepares gate evidence; it does **not** auto-ACCEPT prior tasks or execute `TC-P08-GATE`.

## 2. Ownership invariants

| Invariant | Posture |
|-----------|---------|
| Content schema separate | Proven — `ContentDbContext` schema `content`; ArchitectureTests schema ownership |
| No cross-schema FK writes | Proven — no Destination/Place/SEO/Media `principalSchema` / EF nav from Content (`ContentBoundaryGuardrailTests` + T005/T006 guards) |
| Content ≠ SEO substance | Proven — Content owns title/body/excerpt/blocks; SEO publication binds routes only; no `ContentItemTranslation` / body persistence in SEO services; Content does not own IndexPolicy |
| Content ≠ Tour/Place ownership | Proven — no TourWidget/HotelWidget/AttractionWidget; no TourProduct/Place aggregate ownership; Place stays Place |
| Content owns current locale slug | Proven — `ContentItemTranslation.Slug`; no global `ContentItem.Slug` / SlugFa/SlugEn (P08-R3) |
| SEO owns route history / redirects / IndexPolicy | Proven — Content↛SEO.Infrastructure/Domain; publication services do not `SetIndexPolicy` |
| Default Content SEO posture | Proven — missing/default ⇒ **noindex, follow**; public route existence ≠ Index (P08-R4) |
| Category/Tag without Author | Proven — taxonomy present; **P08-R7 UNRESOLVED** (no Author product) |
| Blocks without widgets | Proven — relational blocks; **P08-R6 UNRESOLVED** (no Tour/Hotel/Attraction widgets) |
| Catalog ops ≠ delete/archive | Proven — no Delete/Archive HTTP/UX; **P08-R8 UNRESOLVED** |

## 3. Locked decisions R1–R8

| ID | Decision | Classification | Gate posture |
|----|----------|----------------|--------------|
| R1 | Content model shape | **RESOLVED** | Core Content Aggregate + typed Article/LandingPage/Guide; `ContentItemId` only |
| R2 | Block storage | **RESOLVED** | Relational `ContentBlock` first-class + ordering |
| R3 | Slug ownership | **RESOLVED** | CONTENT owns current `ContentItemTranslation.Slug`; SEO owns binding/history/redirects/IndexPolicy |
| R4 | Public IndexPolicy default | **RESOLVED** | Default **noindex, follow**; Index requires explicit SEO authority |
| R5 | Destination link | **RESOLVED** | Logical refs 0..N; Content-owned; no cross-schema FK |
| R6 | Tour/Hotel/Attraction widgets | **UNRESOLVED** | **OK for gate prep** — **no** widgets invented |
| R7 | Author model | **UNRESOLVED** | **OK for gate prep** — Category/Tag only; **no** Author invented |
| R8 | Content delete/archive | **UNRESOLVED** | **OK for gate prep** — **no** delete/archive product invented |

### R6 / R7 / R8 explicit statement (gate prep)

**P08-R6, P08-R7, and P08-R8 remain UNRESOLVED and that is acceptable for `TC-P08-GATE` preparation** because:

1. Product scope of P08 does not include Tour/Hotel/Attraction widgets, Author attribution model, or Content hard-delete / archive lifecycle.
2. Guardrails forbid Widget kinds, `ContentAuthor`, and Delete/Archive HTTP / Admin UX / domain delete-archive signals.
3. Closing R6–R8 requires future architect decisions; inventing them in T009 is forbidden.

## 4. ArchitectureTests posture (T009)

| Guard | Status |
|-------|--------|
| Schema `content` ownership | PASS (`ArchitectureGuardrailTests`) |
| Localization without TitleFa/TitleEn | PASS (`ContentLocalizationGuardrailTests`) |
| Category/Tag · no Author (R7) | PASS (`ContentTaxonomyGuardrailTests`) |
| Relational blocks · no widgets (R6) | PASS (`ContentBlocksGuardrailTests`) |
| Destination links contracts-only / no cross-schema FK | PASS (`ContentDestinationLinkGuardrailTests`) |
| Admin Access write · no delete/Author/widgets/IndexPolicy | PASS (`ContentAdminAccessGuardrailTests`) |
| Current Slug · SEO publication does not set IndexPolicy | PASS (`ContentSlugGuardrailTests`) |
| **T009** Content↛SEO.Infrastructure/Domain · Domain purity | PASS (`ContentBoundaryGuardrailTests`) |
| **T009** No Tour/Place ownership / P09 product signals | PASS (`ContentBoundaryGuardrailTests`) |
| **T009** No cross-schema FK to Destination/Place/SEO/Media | PASS (`ContentBoundaryGuardrailTests`) |
| **T009** Content≠SEO substance (no IndexPolicy in Content; SEO≠body) | PASS (`ContentBoundaryGuardrailTests`) |
| **T009** R6/R7/R8 remain uninvented | PASS (`ContentBoundaryGuardrailTests`) |
| **T009** Public pages default noindex,follow · no Admin leak | PASS (`ContentBoundaryGuardrailTests`) |

## 5. Validation battery (this task)

| Suite | Result | Detail |
|-------|--------|--------|
| ArchitectureTests | **PASS** | **68** passed (was 60; +8 T009 boundary tests) |
| Content.UnitTests | **PASS** | **26** passed |
| Access.UnitTests | **PASS** | **5** passed |
| Host.IntegrationTests | **PASS** | **38** passed (includes Content Access + SEO Content publication) |
| Frontend `npx tsc --noEmit` | **PASS** | `src/frontend/web` |
| `git diff --check` | **PASS** | clean |
| HEAD == origin/main (preflight) | **PASS** | `5a70dbe` · clean tree |

## 6. Host / authz evidence (representative)

| Area | Proof |
|------|-------|
| Admin Content Access write | `ContentAccessAuthorizationTests` · `Access.Content.Items.Write` |
| SEO Content publication Access | `SeoContentPublicationHostTests` · `seo.content-posture.write` |
| Public Active-only slug | Content `by-slug` publicOnly + public page notFound for missing title/slug |

## 7. Frontend evidence

| Surface | Path / note |
|---------|-------------|
| Admin Content | `/[locale]/admin/catalog/content` — job-based ops + Ready media picker; no delete/Author/widgets |
| Public Article | `/[locale]/articles/[slug]` — Server Component · Content body + blocks · default noindex,follow |
| Public LandingPage | `/[locale]/landing-pages/[slug]` — same SEO compose posture |
| Typecheck | `npx tsc --noEmit` PASS |

## 8. Gate checklist preview (plan §10)

| # | Criterion | Evidence posture |
|---|-----------|------------------|
| 1 | Content module + separate schema / no cross-schema writes | Architecture + T009 boundary |
| 2 | Article/LandingPage editorial identity under Content | T002 + R1 |
| 3 | Localization without TitleFa/TitleEn | T003 + Architecture |
| 4 | Category/Tag/Author attribution as issued | T004 · Author deferred (R7 open) |
| 5 | Content Blocks for accepted types; no invented widgets | T005 · R6 open |
| 6 | Destination relationship by reference; Destination ≠ Content | T006 + R5 |
| 7 | Media used by ID; Content owns block meaning | T005 + Admin picker |
| 8 | Admin Content Access-backed; job-based UX | T007 |
| 9 | Public Content page baseline | T008 |
| 10 | SEO hooks without SEO owning Content body | T008 + T009 boundary |
| 11 | No Tour product / UGC / HotelBooking ownership | Architecture T009 |
| 12 | Evidence pack + tests green + clean tree | **this document** |

## 9. Deferred / known non-blocking limitations

| Item | Note |
|------|------|
| P08-R6 widgets | Unresolved; **no widget product behavior** — OK for gate prep |
| P08-R7 Author | Unresolved; Category/Tag only — OK for gate prep |
| P08-R8 delete/archive | Unresolved; **no delete product behavior** — OK for gate prep |
| Guide public route | Not a locked public path in T008 (Article/LandingPage only) |
| P09 Tour Core | **NOT_STARTED** |
| Architect ACCEPT of T009 | Pending after this pack; gate requires ACCEPTs + USER token |

## 10. Ready for gate?

**YES — evidence pack ready for `TC-P08-GATE`.**

This task does **not**:

- execute `TC-P08-GATE`
- mark P08 COMPLETE
- start P09 / Tour / UGC
- silently resolve P08-R6 / R7 / R8

Gate still requires architect acceptance of remaining tasks (as issued) and USER token `TRAVELCORE_TASK_CONFIRM: TC-P08-GATE` when the architect issues the gate.
