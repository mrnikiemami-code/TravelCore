# P07 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P07-PLAN` |
| Phase | P07 — Place Catalog |
| Status | COMPLETE / ACCEPTED |
| Baseline | `77eb9dd` (`docs: mark P06 COMPLETE after TC-P06-GATE accept`) |
| Authoritative sources | `docs/ROADMAP.md` § P07 · `docs/architecture/15-future-architecture-transition-map.md` § G · `04-module-boundaries.md` § Place (+ HotelBooking) · `00-constitution.md` §7 · `05-dependency-rules.md` · `docs/domain/module-ownership-matrix.md` · `docs/domain/glossary.md` · `docs/data/01-identifiers-and-references.md` · `docs/ui/06-cross-domain-workflow-and-navigation.md` §11 Workflow D · P04 Destination patterns · P05 SEO locks (R1/R2) · P06 Media locks (R1–R6; consumer gallery meaning deferred to P07) · ADR 0001 · ADR 0007–0008 · ADR 0011–0014 |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P07** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

> **Envelope note:** ChatGPT issued `TC-P07-PLAN` with `Auto-Execute: YES` / baseline `77eb9dd` / phase confirm YES, but the DOM envelope truncated mid-MISSION. This plan is authored from **repository SoT** (same recovery rule used throughout PIPELINE). Architect may amend on review.

---

## 1. Phase Purpose

P07 باید ماژول **Place** را به‌عنوان SoR کاتالوگ مکانی TravelCore با مالکیت schema-per-module پیاده‌سازی کند تا:

1. **Place** به‌عنوان هویت کاتالوگ برای **Hotel · Restaurant · Attraction** متمرکز شود (`PlaceId` only — P07-R1; no independent public HotelId/RestaurantId/AttractionId).
2. **Localization** نام/توضیح (و slug در صورت مالکیت Place) با ردیف‌های locale — **بدون** ستون‌های `NameFa`/`NameEn`.
3. **رابطه با Destination** با ارجاع شناسه/قرارداد (نه مالکیت Destination بر Hotel/POI؛ نه EF nav دوطرفهٔ ممنوع).
4. **Geo / address** برای مکان‌های کاتالوگ (جدا از geo سلسله‌مراتبی Destination که در P04 است).
5. **Facilities · classification · catalog operational status** متعلق به Place.
6. **روابط media** با مالکیت معنای گالری در Place (`MediaAssetId` + SortOrder/Role) — Media فقط SoR باینری/metadata است (قفل P06).
7. **پروفایل توصیفی عمومی** + **Admin Place** job-based (Access-backed) + **یکپارچگی SEO** روی سطح Place (بدون دزدیدن SoR محتوا از SEO).
8. Invariant قفل‌شده حفظ شود: **Hotel Catalog ≠ Hotel Booking** · `Place.Hotel` = canonical hotel catalog · هیچ live provider inventory / reservation / voucher در Place نیست.

P07 **HotelBooking (P21)** · **Content CMS (P08)** · **Tour product (P09+)** · **Search engine** · **UGC** · **Pricing commercial rates product** نیست.

---

## 2. Starting Baseline

Accepted P06 final baseline + post-accept docs sync:

| Item | Value |
|------|--------|
| P06 Gate | `TC-P06-GATE` COMPLETE / ACCEPTED (`da345b5`) |
| Docs sync HEAD | `77eb9dd` |
| P00–P06 | COMPLETE |
| Backend | Modular Monolith + Identity/Access/Party + ReferenceData + Destination + SEO + Media |
| Frontend | Locale Admin Destination/ReferenceData/Media/SEO · Public Destination · Media presentation (app proxy) |
| Place module | **Not implemented** (architecture/docs only) |

USER phase token received: `TRAVELCORE_PHASE_CONFIRM: P07`.

---

## 3. Authoritative Inputs

| Area | Sources |
|------|---------|
| Phase scope | `docs/ROADMAP.md` § P07 · transition map § G |
| Place ownership | `04-module-boundaries.md` § Place · module-ownership-matrix · glossary |
| Catalog ≠ booking | constitution §7 · HotelBooking boundaries · ROADMAP P07/P21 |
| Destination adjacency | P04 Destination module (hierarchy SoR; explicitly ≠ Place) |
| Media adjacency | P06 Media + `MediaAssetReference` · consumer owns gallery meaning |
| SEO adjacency | P05 R1/R2 · SEO binds publishable surfaces; does not own Place catalog text |
| Identifiers | `docs/data/01-identifiers-and-references.md` (`PlaceId` canonical for Place catalog; HotelBooking `ExternalHotelId` → `PlaceId`) |
| Admin UX | `docs/ui/06` Workflow D (Place/Hotel ↔ Media) |
| Localization | ADR 0007–0008 |
| Authz | P03 Access + cookie Identity |
| Governance | ADR 0011–0014 · pipeline protocol |

---

## 4. Scope (In)

1. Physical **Place** module scaffolding under `src/backend/Modules/Place/` (Contracts/Domain/Infrastructure) with dedicated DbContext + PostgreSQL schema `place`.
2. **Place catalog model** covering Hotel · Restaurant · Attraction (exact polymorphism strategy may be locked by **P07-R1**).
3. Strong ids: `PlaceId` only for Place catalog (Hotel/Restaurant/Attraction specializations share `PlaceId`; P07-R1).
4. **Translations** for name/description (+ localized slug if Place-owned) — forbid `NameFa`/`NameEn`.
5. **DestinationId** reference (required/optional policy may be **P07-R2**) without Destination owning hotels.
6. **Address / coordinates** on Place catalog entities.
7. **Facilities / classification / catalog operational status** baseline.
8. **Place↔Media relations** (Place owns SortOrder/Role; references `MediaAssetId`; no Media owning gallery meaning).
9. **Access permissions** + minimal **Admin Place** operational baseline (job-based; not silo CRUD for every table; no HotelBooking UX).
10. **Public Place detail** read model (Server Component First) composed without EF nav into Media/Destination aggregates.
11. **SEO integration hooks** for Place publishable surfaces (binding/IndexPolicy via SEO contracts — Place does not become SEO engine).
12. Architecture/integration tests proving Place ↛ HotelBooking; no live inventory; no cross-schema writes; Destination remains hierarchy SoR.
13. Phase hardening evidence + `TC-P07-GATE`.

---

## 5. Non-Goals (Deferred)

| Deferred item | Owner phase / note |
|---------------|-------------------|
| Live provider availability / room inventory / book / voucher | **P21 HotelBooking** |
| Content CMS blocks / editorial Place pages product | **P08** |
| TourHotelOption / Tour product wiring | **P09+** |
| Search index / facets for Place | Search phase |
| UGC reviews collection owned by Place | **Forbidden** (UGC owns; Place may compose later via contracts) |
| Pricing commercial rates as Place-owned SoR | Deferred / Pricing |
| CDN / multi-region ops | later / ops |
| Media delete domain lifecycle (P06-R8) | Unresolved Media; do not invent in P07 |
| Consumer alt override (P06-R9) | Deferred; Place uses Media default alt unless architect expands |
| Bearer/JWT auth transport change | Forbidden (P03 cookie) |

---

## 6. Architecture Constraints (Locked)

1. Modular Monolith — schema-per-module; no cross-module DbContext.
2. **Hotel Catalog ≠ Hotel Booking** — Place never owns live provider inventory/reservation/voucher.
3. Destination owns hierarchy/discovery nodes; Place owns catalog POIs; composition ≠ ownership.
4. Place may depend on Destination / Media / ReferenceData **by ID/contracts only**.
5. Media owns binaries; **Place owns gallery relationship semantics**.
6. Localization: no `NameFa`/`NameEn`/`NameAr` columns.
7. Server Component First for public Place pages; Client islands allowlisted.
8. Access is authorization authority for Admin Place mutations.
9. SEO authority from P05 is not duplicated (canonical/IndexPolicy/hreflang/sitemap remain SEO).
10. One Task → One Writer; evidence-based acceptance.
11. Do not continue across NEW baseline drift (STOP `BLOCKED_BASELINE_DRIFT`).
12. Do not start P08+ from P07 tasks.

---

## 7. Domain / Ownership Impact

| Concern | Owner after P07 |
|---------|-----------------|
| Hotel/Restaurant/Attraction catalog identity | **Place** |
| Destination hierarchy | Destination (unchanged) |
| MediaAsset bytes/metadata/variants | Media (unchanged) |
| Place gallery order/role | **Place** |
| SEO route/IndexPolicy mechanics | SEO |
| Live hotel bookability | HotelBooking (not in P07) |
| TourHotelOption | Tour (later; references PlaceId of Hotel-kind Place) |

---

## 8. Task Map

### TC-P07-T001 — Place module scaffolding

- **Purpose:** Physical Place module (Contracts/Domain/Infrastructure), DbContext, schema `place`, host registration, ArchitectureTests hooks.
- **Prerequisites:** `TC-P07-PLAN` architect accept.
- **Allowed:** empty/minimal persistence proof · module registration.
- **Forbidden:** HotelBooking · Tour · Content · product galleries.
- **Validation:** build · ArchitectureTests.
- **Done-when:** Place module exists; Place ↛ business peers illegally; no cross-schema writes.

### TC-P07-T002 — Place catalog domain + persistence baseline

- **Purpose:** Place (+ Hotel/Restaurant/Attraction) aggregates/entities + create/get/list contracts; strong ids.
- **Prerequisites:** T001 · **P07-R1** resolved (model shape).
- **Allowed:** catalog metadata persistence in `place` schema.
- **Forbidden:** booking fields · provider external id as PK · Destination schema pollution.
- **Validation:** unit + persistence tests.
- **Done-when:** at least one Place type persistable end-to-end.

### TC-P07-T003 — Localization + Destination link + geo/address

- **Purpose:** Translation rows; DestinationId reference; address/coordinates on Place.
- **Prerequisites:** T002 · **P07-R2** if Destination link cardinality/requiredness open.
- **Allowed:** ADR 0008 fallback patterns · ProblemDetails.
- **Forbidden:** `NameFa`/`NameEn` · Destination owning Place rows.
- **Validation:** unit + FA/EN cases · geo sanity.
- **Done-when:** localized Place readable; Destination reference stored without cross-schema FK abuse.

### TC-P07-T004 — Facilities · classification · catalog status

- **Purpose:** Facilities/classification/catalog operational status baseline.
- **Prerequisites:** T002.
- **Allowed:** Place-owned enums/value objects + Admin set/get.
- **Forbidden:** inventing live availability status as “bookable now”.
- **Validation:** unit/contract tests.
- **Done-when:** status/classification/facilities persist and return on reads.

### TC-P07-T005 — Place↔Media relations (gallery meaning)

- **Purpose:** Place-owned media relations referencing `MediaAssetId` with SortOrder/Role.
- **Prerequisites:** T002 · P06 Media Ready assets available in tests.
- **Allowed:** Place media link table/aggregate · Admin attach/reorder · presentation via P06 app-proxy URLs.
- **Forbidden:** Media owning Place gallery · storing binaries in Place · Destination gallery engines.
- **Validation:** unit + host/integration · ArchitectureTests (consumer meaning stays in Place).
- **Done-when:** Place can attach/list media with stable order/role; public/admin read uses Media delivery contracts.

### TC-P07-T006 — Access permissions + Admin Place baseline

- **Purpose:** Access-backed Admin Place/Hotel (and related) operational UI/API baseline.
- **Prerequisites:** T003–T005 (as issued) · P03 Access patterns.
- **Allowed:** job-based Admin under `/[locale]/admin/...` · create/edit/list/inspect · media attach · no raw-ID primary UX.
- **Forbidden:** HotelBooking admin · public unauthenticated mutations · inventing delete policy if open (**P07-R3**).
- **Validation:** host authz tests · frontend quality.
- **Done-when:** authorized operator can manage catalog Place baseline end-to-end.

### TC-P07-T007 — Public Place detail + SEO integration hooks

- **Purpose:** Public Place detail read model + SEO binding hooks for Place surfaces.
- **Prerequisites:** T003 · T005 · P05 SEO contracts.
- **Allowed:** Server Components · composition of Place + Media presentation + SEO metadata contracts.
- **Forbidden:** SEO owning Place text SoR · Search engine · UGC embed product unless already contracted.
- **Validation:** `npm run quality` · host/public smoke · SEO binding tests as applicable.
- **Done-when:** public Place detail works for an accepted type; SEO hooks evidence without duplicating P05 engine.

### TC-P07-T008 — Phase hardening tests & evidence pack

- **Purpose:** Regression pack proving catalog≠booking, module boundaries, green suites, evidence for gate.
- **Prerequisites:** T001–T007 accepted (as issued).
- **Allowed:** docs evidence artifact · architecture assertions · targeted tests.
- **Forbidden:** new product scope · closing unresolved decisions by invention.
- **Validation:** backend suites · frontend quality · `git diff --check`.
- **Done-when:** evidence pack ready for `TC-P07-GATE`.

### TC-P07-GATE — P07 Acceptance Gate

- **Purpose:** Formal phase exit.
- **Prerequisites:** T001–T008 accepted (as issued) · USER `TRAVELCORE_TASK_CONFIRM: TC-P07-GATE`.
- **Allowed:** evidence-only verification · SoT sync after accept.
- **Forbidden:** starting P08 · implementing HotelBooking · rewriting history/force-push.
- **Validation:** gate checklist (§10).
- **Done-when:** architect ACCEPT → P07 COMPLETE.

---

## 9. Dependency Graph

```text
TC-P07-PLAN
   └─► T001 scaffolding
         └─► T002 domain/persistence  (needs P07-R1)
               ├─► T003 i18n + Destination + geo  (may need P07-R2)
               ├─► T004 facilities/status
               └─► T005 Place↔Media
                     └─► T006 Admin Access baseline
                     └─► T007 Public + SEO hooks
                           └─► T008 evidence
                                 └─► TC-P07-GATE
```

Exact parallelization may be adjusted by architect on accept; Cursor must not invent skipped prerequisites.

---

## 10. Acceptance Strategy (Gate must verify)

1. Place module separate schema; no cross-schema writes.
2. Hotel/Restaurant/Attraction catalog identity exists under Place ownership.
3. Localization without `NameFa`/`NameEn`.
4. Destination relationship by reference; Destination ≠ Place.
5. Geo/address on Place catalog entities.
6. Facilities/classification/catalog status baseline present.
7. Place↔Media relations owned by Place; MediaAssetId refs only.
8. Admin Place baseline Access-backed; job-based UX.
9. Public Place detail baseline present.
10. SEO hooks without SEO owning Place catalog text.
11. **No HotelBooking / live inventory / reservation / voucher** in Place.
12. Evidence pack + tests green + clean tree.

---

## 11. Risks / Open Decisions

| ID | Topic | Status | Notes |
|----|-------|--------|-------|
| **P07-R1** | Place model shape (single Place + kind vs typed Hotel/Restaurant/Attraction aggregates / TPH) | **RESOLVED** | **CORE PLACE + TYPED SPECIALIZATION:** Place = aggregate root; canonical id = `PlaceId` only (no independent public HotelId/RestaurantId/AttractionId); closed `PlaceKind` {Hotel, Restaurant, Attraction}; one Place = one primary kind; tables `place.places` + `place.hotels` / `restaurants` / `attractions` (1:1 same-schema FK); shared facts on Place; type-specific on specialization tables; **no** TPH giant nullable Place table; **no** HotelBooking fields. |
| **P07-R2** | Destination link requiredness/cardinality (every Place must have DestinationId?) | **RESOLVED** | **OPTIONAL SINGLE LOGICAL REFERENCE** Place → Destination; cardinality **0..1**; Place owns nullable `DestinationId` (logical identity only); **no** multiple DestinationIds / primary+secondary / join table / nearest-city inference; **no** cross-schema FK (`place.*` → `destination.*`); no EF navigation to Destination; no Destination.Infrastructure/Domain dependency from Place; when `DestinationId` supplied validate via Destination.Contracts (`IDestinationExistenceQuery`): null = VALID, existing identity = VALID, empty Guid = INVALID, nonexistent = REJECT mutation; **no** DestinationKind restriction in T003. |
| **P07-R3** | Place delete/archive lifecycle | **UNRESOLVED** | Do not invent hard-delete product; archive/status may suffice — architect lock. |
| **P07-R4** | Slug ownership (Place-localized slug vs SEO-only route key) | **UNRESOLVED** | Align with P04 Destination + P05 patterns; STOP rather than invent conflict. |
| **P07-R5** | Public IndexPolicy default for new Place | **UNRESOLVED** | Must not violate P05 locks; default should come from SEO policy patterns. |
| P06-R8 | Media delete | UNRESOLVED (Media) | Out of Place product scope unless architect expands. |
| P06-R9 | Consumer alt override | DEFERRED | Place uses Media defaults unless expanded. |

Cursor must **STOP** with `BLOCKED_ARCHITECT_DECISION_REQUIRED` when a task deadline depends on an UNRESOLVED R# — do not invent policy.

---

## 12. Phase Exit Criteria (P07 COMPLETE)

1. `TC-P07-T001`–`T008` COMPLETE / ACCEPTED (as issued).
2. `TC-P07-GATE` COMPLETE / ACCEPTED.
3. Catalog≠booking invariant evidenced.
4. P08+ **NOT_STARTED** without separate phase confirm.
5. SoT (`PROJECT-STATE` · `ROADMAP` · this plan) coherent.

---

## 13. Cursor Execution Rules for This Plan

1. Envelope titles may be generic — use this plan’s task titles.
2. If ChatGPT DOM truncates envelopes, **this plan + ROADMAP + architecture docs are SoT**.
3. Prefer one product commit per task (+ optional SHA hygiene; dual commits usually NON_BLOCKING).
4. On baseline mismatch → STOP `BLOCKED_BASELINE_DRIFT`.
5. On open decisions with task deadline → STOP `BLOCKED_ARCHITECT_DECISION_REQUIRED`.
6. After PLAN PASS: do **not** auto-start T001 until architect accepts plan and issues Auto-Execute T001.
7. Never force-push / rewrite history.

---

## 14. Plan Delivery Checklist (this PLAN task)

- [x] `docs/plans/P07-implementation-plan.md` created (this file)
- [x] Scope matches ROADMAP § P07 + transition map § G + Place boundaries
- [x] Non-goals explicitly exclude HotelBooking / P08+ product
- [x] Open decisions listed (P07-R1–R5)
- [x] Task map + gate checklist present
- [x] `docs/PROJECT-STATE.md` / `docs/ROADMAP.md` updated to P07 IN_PROGRESS (PLAN awaiting review)
- [x] Commit + push on baseline `77eb9dd`
- [ ] RESULT envelope returned to architect
)
