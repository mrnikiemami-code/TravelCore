# P04 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P04-PLAN` |
| Phase | P04 — Reference Data + Destination |
| Status | COMPLETE / ACCEPTED (`9d264e6`) |
| Baseline | `6a8a5ce` (`TC-P03-GATE`) |
| Authoritative sources | `docs/ROADMAP.md` · `docs/architecture/03-domain-map.md` · `04-module-boundaries.md` · `05-dependency-rules.md` · `07-data-architecture.md` · `08-persistence-and-migrations.md` · `10-ui-constitution.md` · `11-internationalization-architecture.md` · `12-seo-constitution.md` · `15-future-architecture-transition-map.md` § D · `18-backend-physical-structure.md` · `docs/domain/module-ownership-matrix.md` · `docs/domain/glossary.md` · `docs/ui/06-cross-domain-workflow-and-navigation.md` · ADR 0001–0014 |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P04** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

---

## 1. Phase Purpose

P04 باید **مرجع‌های پایدار مشترک (ReferenceData)** و **سلسله‌مراتب مقصد سفرمحور (Destination)** را به‌صورت دو ماژول جدا با مالکیت schema-per-module پیاده‌سازی کند تا:

1. کاتالوگ‌های enumerated/پایدار (Currency، locale/language refs، ISO country، timezone و مشابه تأییدشده) تحت **ReferenceData** بمانند — نه dumping ground برای statusهای کسب‌وکار.
2. سلسله‌مراتب **Continent → Country → Province/State/Region → City → District → Neighborhood** (انواع قابل گسترش) تحت **Destination** شکل بگیرد.
3. ترجمه‌ها · hierarchy queries · geographic identity · **localized slug hooks** · مدیریت Admin · Public read model حداقلی آماده شوند.
4. Invariant قفل‌شده حفظ شود: **Destination مالک Hotel/Tour/Article/Booking نیست**؛ ترکیب صفحه ≠ مالکیت.
5. مرز **ReferenceData ≠ Destination** اثبات شود (ISO country catalog ≠ Istanbul discovery node).

P04 **موتور SEO کامل (P05)**، **Media engine (P06)**، **Place catalog (P07)**، **Content CMS (P08)**، یا commerce نیست.

---

## 2. Starting Baseline

Accepted P03 final baseline:

| Item | Value |
|------|--------|
| Commit | `6a8a5ce` |
| P00 / P01 / P02 / P03 | COMPLETE |
| Backend | Modular Monolith host + Platform foundations + Identity/Access/Party modules (auth cookie R1; Access-backed Admin; Agency presentation baseline) |
| Frontend | Locale-prefixed App Router · Server Component First · Admin guided Identity↔Party workflow · Agency stub · `npm run quality` |
| ReferenceData / Destination code | **Not implemented** (ownership docs only) |

USER phase token already received: `TRAVELCORE_PHASE_CONFIRM: P04`.

---

## 3. Authoritative Inputs

| Area | Sources |
|------|---------|
| Phase scope | `docs/ROADMAP.md` § P04 · `15-future-architecture-transition-map.md` § D |
| Module ownership | `03-domain-map.md` · `04-module-boundaries.md` · `05-dependency-rules.md` · `docs/domain/module-ownership-matrix.md` · `glossary.md` |
| Persistence | ADR 0001 · `07` / `08` · `18` / `26`–`29` / `32` |
| I18n / slug | ADR 0007–0008 · `11-internationalization-architecture.md` |
| SEO boundary (hooks only) | ADR 0009–0010 · `12-seo-constitution.md` — **engine deferred to P05** |
| UI / workflow | ADR 0005–0006 · `10-ui-constitution.md` · `docs/ui/06-…` |
| Governance | ADR 0011–0014 · pipeline protocol |

---

## 4. Scope (In)

1. Physical module scaffolding for **ReferenceData** and **Destination** under `src/backend/Modules/` with separate DbContexts and PostgreSQL schemas.
2. ReferenceData catalogs (conservative): Currency/code · language/locale refs as appropriate · ISO country reference · timezone catalog if appropriate · only truly shared enumerated references.
3. Destination hierarchy domain + persistence (typed nodes; parent/child; extensible type set).
4. Destination translations (name/description as owned by Destination; same DestinationId across locales).
5. Hierarchy query/read contracts (ancestors/descendants/children; tree/path helpers).
6. Geographic identity baseline (coordinates / geo fields as owned by Destination where applicable — without Place catalog).
7. Localized **slug hooks** on Destination (stable entity-owned slug fields/history hooks) — **not** SeoRoute/redirect/sitemap engine.
8. Admin management baseline: guided Destination tree/create/edit/translate workflow; ReferenceData admin read/seed management as needed — **no domain-mirrored CRUD menus as default IA**.
9. Public Destination read model / detail baseline (locale-aware) composing only Destination (+ ReferenceData refs) — no Tour/Hotel/Content widgets as ownership.
10. Architecture/integration tests proving schema isolation and ReferenceData ≠ Destination.
11. Phase hardening evidence + `TC-P04-GATE`.

---

## 5. Non-Goals (Deferred)

| Deferred item | Owner phase / note |
|---------------|-------------------|
| SeoRoute · slug history engine · Redirect · Canonical · hreflang · sitemap · robots | **P05** |
| MediaAsset upload/variants/object storage | **P06** (Destination may hold `MediaAssetId` refs later; no Media engine in P04) |
| Place / Hotel / Restaurant / Attraction catalog | **P07** |
| Content CMS / Articles / Blocks | **P08** |
| Tour / Pricing / Booking / Payment | later commerce phases |
| Search index projections | Search platform phase |
| Airline/airport catalogs (unless architect explicitly pulls a minimal ReferenceData slice) | later / optional R-item |
| Treating `TourStatus` / `BookingStatus` / `PaymentStatus` as ReferenceData | **Forbidden permanently** — business-module owned |

---

## 6. Architecture Constraints (Locked)

1. Modular Monolith — no premature microservices.
2. Schema-per-module (ADR 0001); no cross-schema writes; no cross-module DbContext.
3. UUID v7 identities (ADR 0002).
4. **ReferenceData ≠ Destination** · Destination ≠ Place · page composition ≠ ownership.
5. Domain Model ≠ Persistence Model ≠ API Contract ≠ Page View Model.
6. Server-authoritative business rules.
7. Server Component First (ADR 0005); intentional Client islands only.
8. Direction-neutral / bidi / mobile-first / a11y for Admin workflows (ADR 0006).
9. Locale ≠ Currency ≠ Calendar ≠ Timezone.
10. Domain ≠ navigation ≠ screen ≠ form ≠ workflow (`docs/ui/06`).
11. Raw IDs are not UX.
12. One Task → One Writer; evidence-based acceptance (ADR 0011).
13. Destination must not own or mutate Tour/Place/Content/UGC aggregates.
14. SEO constitution hooks allowed; SEO **engine** forbidden until P05.

---

## 7. Domain / Ownership Impact

| Module | Owns | Must not own | References |
|--------|------|--------------|------------|
| **ReferenceData** | Stable shared catalogs (currency codes, locale/language refs, ISO country defs, timezone defs as accepted) | Destination hierarchy · Tour/Booking/Payment statuses · business lifecycles | — |
| **Destination** | Destination nodes + types · hierarchy · translations · geo identity · localized slug fields/hooks · destination-owned admin/public contracts | Hotels · restaurants · attractions · tours · articles · reviews · SEO route engine · Media binaries | ReferenceData IDs (ISO/geo); optional future MediaAssetId |
| **Presentation** | UX composition only | Combined discovery aggregate | Explicit Destination/ReferenceData contracts |

---

## 8. Cross-Domain Workflow Impact (Mandatory)

Admin Destination management is a **job-to-be-done workflow** (find node → create child → translate → publish readiness), not three silo menus named after modules.

Public Destination experience may later compose Tour/Place/Content via contracts — **P04 public baseline stays Destination-centric** without importing those modules’ ownership.

Identity/Access from P03 remain authoritative for Admin authn/authz; Destination Admin endpoints must use Access-backed policies (extend permission catalog as needed in Access-owned seeds via contracts — do not invent a second authz system).

---

## 9. Data / Persistence Impact

- Schemas: `referencedata` · `destination` (exact names follow Platform convention; must be module-owned).
- No cross-schema FK enforcement that couples modules; ID references only.
- Module-owned migrations + runner convention (P01).
- Seed strategy: ReferenceData may include stable seeds; Destination sample hierarchy may be fixture/seed for tests — business-managed destinations remain mutable data.

---

## 10. API / Contract Impact

- ReferenceData: read APIs for catalogs; admin mutate only where catalog is managed (not for every row if seed-only).
- Destination: commands for create/move/rename/translate; queries for get-by-id, children, ancestors, localized detail, slug lookup hook.
- Public read models are explicit contracts — not leaking EF entities.
- Admin routes remain non-indexable.

---

## 11. Frontend / UI Impact

| Area | P04 expectation |
|------|-----------------|
| Admin | Guided Destination hierarchy workflow under locale Admin shell; ReferenceData management only if needed for ops |
| Public | Minimal Destination detail/landing read path FA/EN (walking-skeleton style) |
| Navigation | Do **not** freeze IA as ReferenceData + Destination CRUD silos |
| SEO UI | No P05 engine screens |
| Quality | `npm run quality` when UI ships |

---

## 12. Security / Access Impact

1. Destination Admin mutations require authenticated Identity + Access permissions.
2. Public Destination reads are intentionally public (content policy later); do not expose Admin contracts.
3. No secrets in ReferenceData/Destination payloads.
4. Frontend never becomes authorization authority.

---

## 13. Testing Strategy

| Layer | Purpose |
|-------|---------|
| Unit | Destination hierarchy invariants · translation rules · slug hook rules · ReferenceData catalog invariants |
| Architecture tests | project placement · no cross-DbContext · ReferenceData ≠ Destination ownership |
| Integration (PostgreSQL) | migrations · CRUD/hierarchy smoke |
| API | Admin allow/deny · public read |
| Frontend quality | when Admin/Public UI lands |
| Gate | `TC-P04-GATE` end-to-end |

---

## 14. Ordered Task Map

### TC-P04-T001 — ReferenceData + Destination module scaffolding

- **Purpose:** Create physical module projects, host registration hooks, schema naming, empty DbContext shells.
- **Prerequisites:** `TC-P04-PLAN` accepted.
- **Allowed:** `src/backend/Modules/{ReferenceData,Destination}/**` scaffolding · solution entries · host DI stubs · ownership pointers.
- **Forbidden:** entities/tables/APIs/UI · Place/Tour · SEO engine.
- **Validation:** build · architecture placement · `git diff --check`.
- **Done-when:** two compile-ready module shells with clear ownership.

### TC-P04-T002 — ReferenceData catalogs + persistence baseline

- **Purpose:** Implement conservative ReferenceData catalogs + first migration/seeds.
- **Prerequisites:** T001.
- **Allowed:** Currency · locale/language · ISO country · timezone (as accepted) · owning contracts/APIs.
- **Forbidden:** Destination hierarchy · business status enums · Place/Tour.
- **Validation:** migration · unit/integration · architecture tests.
- **Done-when:** ReferenceData catalogs persist and are readable via owning contracts.

### TC-P04-T003 — Destination hierarchy domain + persistence

- **Purpose:** Implement Destination node aggregate (types + parent/child) with Destination schema migration.
- **Prerequisites:** T001; T002 recommended before ISO/geo refs are wired.
- **Allowed:** Destination domain/persistence · hierarchy invariants · create/get/children APIs.
- **Forbidden:** translations UI · SEO engine · Place catalog · Media engine.
- **Validation:** hierarchy invariant tests · migration proof.
- **Done-when:** Destination nodes persist under Destination ownership only.

### TC-P04-T004 — Destination translations + geographic identity baseline

- **Purpose:** Locale translations for Destination + geo identity fields without Place semantics.
- **Prerequisites:** T003.
- **Allowed:** translation tables/value objects · lat/long or equivalent owned fields · APIs.
- **Forbidden:** Article/content CMS · Tour itinerary geo · SEO redirects.
- **Validation:** same DestinationId multi-locale · unit/integration.
- **Done-when:** FA/EN (or configured locales) translations persist for Destination.

### TC-P04-T005 — Hierarchy query + path/ancestors contracts

- **Purpose:** Efficient read contracts for tree navigation (ancestors, descendants depth-limited, breadcrumbs data).
- **Prerequisites:** T003.
- **Allowed:** query services · read models · tests for path integrity.
- **Forbidden:** Search engine · SEO sitemap · cross-module projections of Tour/Place.
- **Validation:** query integration tests.
- **Done-when:** hierarchy navigation works via Destination contracts.

### TC-P04-T006 — Localized Destination slug hooks

- **Purpose:** Entity-owned localized slug fields/hooks enabling future P05 SeoRoute binding — without implementing SEO engine.
- **Prerequisites:** T004.
- **Allowed:** slug fields per locale · uniqueness rules within Destination ownership · lookup-by-slug hook API.
- **Forbidden:** Redirect table · canonical engine · hreflang publisher · sitemap · index policies (P05).
- **Validation:** uniqueness/conflict tests · architecture note that SEO engine remains P05.
- **Done-when:** slug hooks exist and are Destination-owned.

### TC-P04-T007 — Access permissions + Admin Destination authz wiring

- **Purpose:** Extend Access permission catalog for Destination/ReferenceData Admin ops; enforce server-side.
- **Prerequisites:** T002 · T003 · P03 Access baseline.
- **Allowed:** Access seed permissions · host policies on Admin endpoints · allow/deny tests.
- **Forbidden:** frontend-only authz · new auth system.
- **Validation:** 401/403/200 matrix.
- **Done-when:** Admin Destination/ReferenceData mutations are Access-backed.

### TC-P04-T008 — Guided Admin Destination hierarchy workflow UI

- **Purpose:** Mobile-first Admin workflow to manage Destination tree/translations without raw-ID UX and without module-silo menus.
- **Prerequisites:** T004 · T005 · T007 · AdminShell + `docs/ui/06`.
- **Allowed:** locale Admin routes · Server Components · minimal Client islands · FA/EN RTL/LTR.
- **Forbidden:** Place/Tour admin · SEO engine UI · three mandatory CRUD menus for every catalog table.
- **Validation:** `npm run quality` · FA/EN smoke.
- **Done-when:** Admin can create/navigate/edit/translate Destinations via guided workflow.

### TC-P04-T009 — Public Destination read model / detail baseline

- **Purpose:** Public locale Destination detail (or minimal landing) reading Destination contracts only.
- **Prerequisites:** T004 · T005 · T006.
- **Allowed:** public locale route · Server Component page · PVM · noindex/index policy deferred carefully (default conservative; full SEO P05).
- **Forbidden:** embedding Tour/Hotel/Article ownership · Agency commerce · SEO engine features.
- **Validation:** FA/EN render · quality gates.
- **Done-when:** public Destination baseline page works from Destination read model.

### TC-P04-T010 — ReferenceData Admin/read UX baseline (minimal)

- **Purpose:** Minimal Admin/read surfaces for ReferenceData catalogs needed to operate Destination (e.g., ISO country pickers) — not a dumping-ground CMS.
- **Prerequisites:** T002 · T007 · T008 (or parallel after T007 if T008 blocked).
- **Allowed:** picker/read APIs already used by Destination Admin · thin Admin views only if required.
- **Forbidden:** moving Destination hierarchy into ReferenceData UI · status enums from other domains.
- **Validation:** quality + authz.
- **Done-when:** Destination Admin can resolve ReferenceData refs without raw IDs.

### TC-P04-T011 — Phase hardening tests & evidence pack

- **Purpose:** Consolidate architecture/integration/frontend evidence for gate readiness.
- **Prerequisites:** T001–T010 accepted (or architect-trimmed subset explicitly accepted).
- **Allowed:** tests · docs evidence · tiny unambiguous P04 fixes.
- **Forbidden:** starting P05 · new modules outside plan.
- **Validation:** full battery green.
- **Done-when:** evidence pack ready for `TC-P04-GATE`.

### TC-P04-GATE — P04 Acceptance Gate

- **Purpose:** Verify P04 exit criteria; mark phase COMPLETE only on PASS.
- **Prerequisites:** T001–T011 accepted (as issued/accepted).
- **Allowed:** validation + state hygiene · tiny unambiguous P04 regression fixes.
- **Forbidden:** starting P05 · Place/Tour/Media engines · history rewrite.
- **Validation:** checklist §16.
- **Done-when:** architect-accepted COMPLETE or explicit FAIL/BLOCKED.

---

## 15. Dependency Graph

```text
TC-P04-PLAN (architect accept)
 └─> T001 scaffolding
      ├─> T002 ReferenceData catalogs
      └─> T003 Destination hierarchy
           ├─> T004 translations + geo
           ├─> T005 hierarchy queries
           │     └─> T008 Admin workflow (also needs T007)
           └─> T006 slug hooks
                  └─> T009 Public Destination baseline
      T002 + T003 ──> T007 Access wiring ──> T008 / T010
      T001–T010 ──> T011 evidence ──> TC-P04-GATE
```

Parallelism note: after T001, **T002** and **T003** may proceed in parallel; **T004/T005** wait for T003; **T006** waits for T004; **T007** waits for T002+T003; **T008** waits for T004+T005+T007; **T009** waits for T004+T005+T006.

---

## 16. Acceptance Strategy (Gate must verify)

1. ReferenceData and Destination modules exist with separate schemas and no cross-schema writes.
2. ReferenceData ≠ Destination proven by architecture tests + ownership docs.
3. Destination hierarchy supports extensible types along Continent→…→Neighborhood path.
4. Translations exist for Destination; same DestinationId across locales.
5. Hierarchy queries (children/ancestors/path) work via Destination contracts.
6. Localized slug hooks exist without P05 SEO engine features.
7. Admin Destination workflow is Access-backed, FA/EN capable, no raw-ID primary UX, no module-silo IA freeze.
8. Public Destination baseline reads Destination (and ReferenceData refs) only — no Place/Tour ownership leakage.
9. Server Component First retained; Client islands contained.
10. P05+ engines absent (SEO/Media/Place/Content/Tour).
11. Evidence pack complete; quality/tests green; working tree clean after gate hygiene.

---

## 17. Risks and Deferred Decisions

| ID | Item | Classification |
|----|------|----------------|
| R1 | Exact initial Destination type enum set (Island vs Region granularity) | **REQUIRES_ARCHITECT_CONFIRMATION_AT_T003** if docs remain ambiguous; default proposal: Continent, Country, ProvinceOrState, Region, City, District, Neighborhood, Island (extensible) |
| R2 | Whether timezone/language catalogs ship in first ReferenceData seed or later slice | **RESOLVED_BY_PLAN_DEFAULT**: include minimal seeds needed by Destination Admin; expand only with evidence |
| R3 | Public Destination indexation vs noindex until P05 | **REQUIRES_ARCHITECT_DECISION_BEFORE_T009** if SEO constitution conflicts; default proposal: conservative noindex until P05 unless architect says otherwise |
| R4 | Airline/airport ReferenceData | **DEFERRED** unless explicitly pulled |
| R5 | Destination↔Media gallery ownership semantics | **DEFERRED_TO_P06** |
| R6 | Moving ISO country solely into Destination | **FORBIDDEN** — keep ReferenceData ownership for ISO defs |

If R1/R3 block a task, Cursor must **STOP** with `BLOCKED_ARCHITECTURE_CONFLICT` rather than inventing policy.

---

## 18. Phase Exit Criteria (P04 COMPLETE)

P04 may be marked COMPLETE only when all are true:

1. ReferenceData + Destination modules implemented with separate persistence ownership.
2. Hierarchy + translations + slug hooks + Admin/Public baselines accepted.
3. Invariants ReferenceData ≠ Destination and Destination ≠ Place/Tour ownership proven.
4. Access-backed Admin mutations proven.
5. No P05 SEO engine / P06 Media engine / P07 Place catalog leakage.
6. Evidence + gate accepted.

---

## 19. Cursor Execution Rules for This Plan

1. Execute **only** the architect-issued Auto-Execute task.
2. When envelopes truncate in ChatGPT DOM, **this plan is source of truth** for scope.
3. Do not start `TC-P04-T001` until `TC-P04-PLAN` is architect-accepted and T001 is issued.
4. Do not start P05.
5. Prefer exactly one commit per task unless architect allows evidence follow-up.
6. PowerShell: use `;` not `&&`.

---

## 20. Plan Delivery Checklist (this PLAN task)

- [x] Plan document created at `docs/plans/P04-implementation-plan.md`
- [x] `docs/PROJECT-STATE.md` updated (P04 IN_PROGRESS; PLAN awaiting review)
- [x] `docs/ROADMAP.md` updated
- [x] No product code / migrations
- [ ] Single commit · push · RESULT
