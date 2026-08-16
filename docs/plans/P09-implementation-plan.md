# P09 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P09-PLAN` |
| Phase | P09 — Tour Core |
| Status | ACCEPTED (`7de2518`) — architect ACCEPT 2026-08-17; Auto-Execute T001 authorized |
| Baseline | `f9ab2e8` (`docs: fix residual P08 COMPLETE SoT rows` — P08 COMPLETE SoT) |
| Authoritative sources | `docs/ROADMAP.md` § P09 · `docs/architecture/15-future-architecture-transition-map.md` § I · `04-module-boundaries.md` § Tour (+ Pricing/Booking/SEO/Media/Place/Destination adjacent) · `05-dependency-rules.md` · `docs/domain/module-ownership-matrix.md` · `docs/domain/glossary.md` (TourProduct · ExperienceTour · PackageTour · TourDeparture · FlightSegment · TourHotelOption) · P05 SEO locks (R1/R2; Content/Place slug+IndexPolicy continuity) · P06 Media locks (consumer owns relationship meaning; MediaAssetId refs) · P07 Place locks (Hotel catalog ≠ booking; PlaceId for Hotel-kind) · P08 Content locks (Content ≠ Tour product) · ADR 0001 · ADR 0007–0008 · ADR 0011–0014 |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P09** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

> **Envelope note:** ChatGPT DOM for the planning envelope may be truncated. This plan is authored from **repository SoT** (ROADMAP § P09 · transition map § I · Tour module boundaries · ownership matrix · glossary). USER `TRAVELCORE_PHASE_CONFIRM: P09` received. **NEW continuity rule** (auto-continue after gate/phase under PIPELINE) is recorded separately in governance docs — this PLAN does not redefine that rule. Architect may amend on review. Planning only; no T001+ until Auto-Execute.

---

## 1. Phase Purpose

P09 باید ماژول **Tour** را به‌عنوان SoR **مبانی مشترک Tour Core** با مالکیت schema-per-module پیاده‌سازی کند تا:

1. **TourProduct** به‌عنوان هویت محصول تور (تجربه یا پکیج) متمرکز شود — **بدون** ادغام با `TourDeparture` (Departure = **P11**).
2. **Classification · Origin · Destinations** برای سازمان‌دهی جغرافیایی/کشف محصول (ارجاع Destination by ID/contracts؛ نه مالکیت Destination).
3. **Agency references** به Party/Agency by ID — بدون merge با Party و بدون Agency silo auth.
4. **Services · Policies · Requirements** متعلق به Tour به‌عنوان مبانی مشترک محصول (نه Quote/Payment/Booking).
5. **روابط Media** با مالکیت معنای گالری/نقش در Tour (`MediaAssetId` + SortOrder/Role) — Media فقط SoR باینری/metadata است (قفل P06).
6. **Publishing lifecycle** متعلق به Tour؛ **SEO** مالک route binding / history / IndexPolicy / canonical / hreflang / sitemap است (قفل P05؛ الگوی Place/Content).
7. **Translations** عنوان/توضیح (+ slug در صورت مالکیت Tour) با ردیف‌های locale — **بدون** ستون‌های `TitleFa`/`TitleEn`.
8. **Admin Tour** job-based + Access-backed baseline + **hooks عمومی/SEO** برای سطح Tour Core (نه Experience Detail کامل P10 و نه Package Departure/Flight/HotelOptions محصول P11).
9. Invariantهای قفل‌شده حفظ شوند:
   - **TourProduct ≠ TourDeparture**
   - **ExperienceTour و PackageTour** به یک blob غول‌پیکر property nullable اجباری نشوند
   - Tour package `FlightSegment` ≠ live Flight inventory (**P11/P22** — خارج از محصول P09)
   - `TourHotelOption` ≠ مالکیت Hotel catalog (**Place** مالک Hotel)
   - **Tour ≠ Pricing calculation · Tour ≠ Booking · Tour ≠ Search · Tour ≠ Content editorial CMS**

P09 **Experience Tour detail structures (P10)** · **Foreign Package Departure / FlightSegment / TourHotelOption product (P11)** · **Pricing (P12)** · **Agency Marketplace (P13)** · **Public Tour Experience polish (P14)** · **Search** · **UGC** · **HotelBooking / Flight live inventory** نیست.

---

## 2. Starting Baseline

Accepted P08 final baseline + post-accept docs sync:

| Item | Value |
|------|--------|
| P08 Gate | `TC-P08-GATE` COMPLETE / ACCEPTED (`576b7fa`) |
| Docs sync HEAD | `f9ab2e8` |
| P00–P08 | COMPLETE |
| Backend | Modular Monolith + Identity/Access/Party + ReferenceData + Destination + SEO + Media + Place + Content |
| Frontend | Locale Admin Destination/ReferenceData/Media/Place/SEO/Content · Public Destination/Place/Content · Media presentation (app proxy) |
| Tour module | **Not implemented** (architecture/docs only) |

USER phase token received: `TRAVELCORE_PHASE_CONFIRM: P09`.

---

## 3. Authoritative Inputs

| Area | Sources |
|------|---------|
| Phase scope | `docs/ROADMAP.md` § P09 · transition map § I |
| Tour ownership | `04-module-boundaries.md` § Tour · module-ownership-matrix · glossary Tour* terms |
| Product ≠ departure | constitution / glossary — TourProduct ≠ TourDeparture |
| Experience ≠ Package blob | ROADMAP P09 · transition map § I invariant |
| Place adjacency | P07 — Hotel catalog via `PlaceId`; TourHotelOption product deferred to **P11** |
| Content adjacency | P08 — Content ≠ Tour product; Tour widgets/content ownership stay separate |
| Media adjacency | P06 — MediaAssetId refs; Tour owns gallery/relationship meaning |
| SEO adjacency | P05 R1/R2 · Place/Content resolved slug+IndexPolicy patterns as **adjacent precedent only** (P09-R5/R6 still open) |
| Destination adjacency | P04 — hierarchy SoR; Tour links by ID/contracts |
| Party/Agency | P03 Party — Agency business identity; Tour refs by ID only |
| Localization | ADR 0007–0008 |
| Authz | P03 Access + cookie Identity |
| Governance | ADR 0011–0014 · pipeline protocol · continuity rule recorded in governance docs (separate) |

---

## 4. Scope (In)

1. Physical **Tour** module scaffolding under `src/backend/Modules/Tour/` (Contracts/Domain/Infrastructure) with dedicated DbContext + PostgreSQL schema `tour`.
2. **TourProduct** domain/persistence baseline under locked model shape (**P09-R1**) — shared core foundations only.
3. Strong ids: canonical TourProduct identity strategy per **P09-R1** (do not invent before lock).
4. **Translations** for title/description (+ localized slug if Tour-owned) — forbid `TitleFa`/`TitleEn`.
5. **Classification · Origin · Destination link(s)** baseline (cardinality/requiredness may be **P09-R2**).
6. **Agency references** by Party/Agency ID (exact shape may be **P09-R3**) — no Party merge.
7. **Services · Policies · Requirements** baseline belonging to Tour product (shared core; not Pricing/Booking rules).
8. **Tour↔Media relations** (Tour owns SortOrder/Role; references `MediaAssetId`; no Media owning gallery meaning).
9. **Publishing lifecycle** + **Access permissions** + minimal **Admin Tour** operational baseline (job-based; not silo CRUD for every table). Delete/archive vs publish status may need **P09-R4**.
10. **Public Tour Core hooks** + **SEO integration hooks** for Tour publishable surfaces (binding/IndexPolicy via SEO contracts — Tour does not become SEO engine). Slug ownership **P09-R5**; default IndexPolicy **P09-R6**.
11. Explicit **non-implementation** of P10 Experience itinerary structures and P11 Package Departure/FlightSegment/TourHotelOption product — contract ID stubs only if architect-approved under **P09-R7**; prefer open decision over invention.
12. Architecture/integration tests proving Tour ↛ Pricing/Booking/Search ownership; Tour ↛ Place Hotel catalog; Tour ↛ Content CMS; no cross-schema writes; TourProduct ≠ TourDeparture tables/product in P09.
13. Phase hardening evidence + `TC-P09-GATE`.

---

## 5. Non-Goals (Deferred)

| Deferred item | Owner phase / note |
|---------------|-------------------|
| Experience itinerary · ItineraryDay · Stop · meals · equipment · difficulty · guide detail | **P10** |
| TourDeparture · TransportSegment · FlightSegment · airports/carrier facts · TourHotelOption · stay/MealPlan · occupancy/age/capacity package rules | **P11** |
| Live Flight inventory / book | **P22** / Flight module |
| Pricing rates · Quote · conversion | **P12** |
| Agency Marketplace product | **P13** |
| Public Tour Experience polish / full public archetypes | **P14** (P09 may add minimal public hooks only) |
| Search index / facets for Tour | **P15** |
| UGC reviews owned by Tour | **Forbidden** (UGC owns; compose later via contracts) |
| Content editorial CMS / blocks ownership | **P08** (unchanged; Content ≠ Tour) |
| HotelBooking / live hotel inventory | **P21** |
| Visa product ownership | Visa module (Tour may later reference by ID — not P09 product) |
| Bearer/JWT auth transport change | Forbidden (P03 cookie) |
| Bidirectional Tour↔Content / Tour↔Pricing domain dependency | Forbidden |
| Auto-index because Published/Active | SEO Thin Content Guard — withhold Index until policy lock |

---

## 6. Architecture Constraints (Locked)

1. Modular Monolith — schema-per-module; no cross-module DbContext.
2. **TourProduct ≠ TourDeparture** — Departure product is out of P09.
3. **ExperienceTour / PackageTour** must not collapse into one giant nullable property bag (**P09-R1** / **P09-R7**).
4. Tour may depend on Destination / Place / Party / Media / ReferenceData **by ID/contracts only**.
5. Media owns binaries; **Tour owns gallery relationship semantics**.
6. Place owns Hotel catalog; Tour must not copy Hotel aggregates or introduce EF nav to Place entities.
7. Pricing owns commercial calculation; Booking owns reservation; Search owns index; SEO owns IndexPolicy/canonical/history mechanics; Content owns editorial CMS — none of these become Tour SoR.
8. Localization: no `TitleFa`/`TitleEn`/`NameFa` columns.
9. Server Component First for any public Tour surfaces in scope; Client islands allowlisted.
10. Access is authorization authority for Admin Tour mutations.
11. One Task → One Writer; evidence-based acceptance.
12. Do not continue across NEW baseline drift (STOP `BLOCKED_BASELINE_DRIFT`).
13. Do not start P10/P11 product from P09 tasks.
14. Do not invent unresolved **P09-R#** policies — STOP `BLOCKED_ARCHITECT_DECISION_REQUIRED`.

---

## 7. Domain / Ownership Impact

| Concern | Owner after P09 |
|---------|-----------------|
| TourProduct shared identity + publishing lifecycle (as issued) | **Tour** |
| Classification / Origin / Destination links (Tour-owned refs) | **Tour** |
| Agency reference on Tour product | **Tour** (Party remains SoR of Agency identity) |
| Services / Policies / Requirements baseline | **Tour** |
| Tour media order/role | **Tour** |
| Destination hierarchy | Destination (unchanged) |
| Place / Hotel catalog | Place (unchanged) |
| MediaAsset bytes/metadata | Media (unchanged) |
| Content editorial Article/Blocks | Content (unchanged) |
| SEO route/IndexPolicy mechanics | SEO |
| TourDeparture / FlightSegment / TourHotelOption product | Tour (**P11** — not started in P09) |
| Experience itinerary structures | Tour (**P10** — not started in P09) |
| Quote / rate calculation | Pricing (not started) |
| Booking / Payment | Booking / Payment (not started) |

---

## 8. Task Map

### TC-P09-T001 — Tour module scaffolding

- **Purpose:** Physical module + schema `tour` + DI registration + migration runner convention proof.
- **Prerequisites:** PLAN ACCEPTED.
- **Allowed:** Contracts/Domain/Infrastructure shells · `TourDbContext` · empty/initial migration · host wiring · architecture smoke.
- **Forbidden:** product entities beyond scaffolding · Admin UI · SEO engine changes · Pricing/Booking/Search · P10/P11 product entities.
- **Validation:** build · ArchitectureTests · Persistence smoke as applicable.
- **Done-when:** Tour module exists; schema ownership asserted; no P10/P11 leakage.

### TC-P09-T002 — TourProduct domain + persistence baseline

- **Purpose:** Persist TourProduct shared core end-to-end under **P09-R1** (and specialty-field posture **P09-R7** when deadline hits).
- **Prerequisites:** T001 · **P09-R1 RESOLVED** (or STOP). **P09-R7** if specialty fields would otherwise be invented.
- **Allowed:** domain aggregates/entities for shared TourProduct core · EF maps · migrations · unit tests · typed Experience/Package specialization **only if R1/R7 lock it for P09**.
- **Forbidden:** inventing R1/R7 · TourDeparture tables/product · FlightSegment/TourHotelOption product · Pricing fields · Content CMS merge · giant nullable blob without lock.
- **Validation:** unit + persistence as scoped.
- **Done-when:** accepted TourProduct core persistable under Tour ownership without Departure/P10/P11 product.

### TC-P09-T003 — Localization + translations baseline

- **Purpose:** Locale rows for title/description (+ slug if Tour-owned) without Fa/En columns (ADR 0008).
- **Prerequisites:** T002 · may need **P09-R5** lock for slug ownership when slug work is in deadline.
- **Allowed:** translations · slug normalization mirroring Destination/Place/Content patterns **if R5 locks Tour-like ownership**.
- **Forbidden:** SEO history tables inside Tour · `TitleFa`/`TitleEn` columns · inventing R5.
- **Validation:** unit + architecture ADR 0008 guards.
- **Done-when:** localized TourProduct text persistable; slug policy matches locked R5 (or STOP / defer slug until R5).

### TC-P09-T004 — Classification · Origin · Destination links

- **Purpose:** Classification + Origin + Destination linkage without ownership transfer.
- **Prerequisites:** T002 · may need **P09-R2**.
- **Allowed:** Tour-owned classification/origin fields · Destination link fields/tables · Contracts existence queries · no cross-schema FK.
- **Forbidden:** Destination EF nav collections of Tours · inventing R2 · Place ownership transfer.
- **Validation:** unit + architecture Destination-link guards.
- **Done-when:** TourProduct can carry classification/origin/destination refs per locked cardinality.

### TC-P09-T005 — Agency references

- **Purpose:** Reference Party/Agency by ID for offer/ownership semantics without Party merge.
- **Prerequisites:** T002 · may need **P09-R3**.
- **Allowed:** Tour-owned Agency/PartyId reference shape as locked · contract existence validation.
- **Forbidden:** Party aggregate merge · Identity ownership transfer · inventing R3 · Agency silo auth.
- **Validation:** unit/persistence scoped · architecture Party-boundary guards.
- **Done-when:** TourProduct can reference Agency/Party per locked shape without owning Party.

### TC-P09-T006 — Services · Policies · Requirements baseline

- **Purpose:** Shared Tour-owned services/policies/requirements baseline (product facts, not commercial Quote rules).
- **Prerequisites:** T002.
- **Allowed:** Tour-owned models/value objects for services/policies/requirements · Admin set/get as later issued.
- **Forbidden:** Pricing calculation · Booking cancellation engine · Payment · inventing P11 passenger/occupancy package product · Visa module ownership transfer.
- **Validation:** unit/contract tests.
- **Done-when:** baseline services/policies/requirements persist and return on reads for TourProduct core.

### TC-P09-T007 — Tour↔Media relations

- **Purpose:** Tour-owned media relations referencing `MediaAssetId` with SortOrder/Role.
- **Prerequisites:** T002 · P06 Media Ready assets available in tests.
- **Allowed:** Tour media link table/aggregate · Admin attach/reorder (when Admin task lands) · presentation via P06 app-proxy URLs.
- **Forbidden:** Media owning Tour gallery · storing binaries in Tour · Content block engines · inventing Place gallery takeover.
- **Validation:** unit + host/integration · ArchitectureTests (consumer meaning stays in Tour).
- **Done-when:** TourProduct can attach/list media with stable order/role; reads use Media delivery contracts.

### TC-P09-T008 — Publishing lifecycle + Access + Admin Tour baseline

- **Purpose:** Tour publishing/catalog status lifecycle + Access-backed Admin Tour job for create/edit/publish ops (no delete invent if R4 open).
- **Prerequisites:** T003–T007 as issued · **P09-R4** when delete/archive deadline hits (or STOP).
- **Allowed:** permission codes · `/[locale]/admin/...` Tour job · Ready media picker reuse (P06/P07/P08 pattern) · no raw-ID primary UX · publication status as locked.
- **Forbidden:** silo CRUD for every table · DAM · delete/archive product if **P09-R4** unresolved · Pricing/Booking Admin · P10 itinerary editor product · P11 departure editor product.
- **Validation:** host Access tests · frontend quality.
- **Done-when:** authorized operator can manage Tour Core baseline via Access-backed Admin; publishing lifecycle evidenced without inventing unresolved delete policy.

### TC-P09-T009 — Public hooks + SEO publication integration

- **Purpose:** Minimal public Tour Core read hooks + SEO publication binding without SEO owning Tour body text.
- **Prerequisites:** T008 · **P09-R5/R6** when deadline hits (or STOP).
- **Allowed:** public Server Components / read contracts as scoped · compose Destination/Media via contracts · SEO resource type/hooks · default IndexPolicy **only after R6 lock**.
- **Forbidden:** inventing Index=Active / Published=Index · SEO owning Tour title/body · starting Search · P10 Experience Detail full product · P11 Foreign Package Detail product.
- **Validation:** host/SEO unit as scoped · frontend quality · public route checks as issued.
- **Done-when:** public Tour Core hook works for accepted surface; SEO hooks evidence without duplicating P05 engine; IndexPolicy default matches locked R6 (or STOP).

### TC-P09-T010 — Phase hardening tests & evidence pack

- **Purpose:** Regression pack proving TourProduct≠TourDeparture · Tour≠Pricing/Booking/Search/Content · Tour≠Place Hotel ownership · green suites · gate evidence.
- **Prerequisites:** T001–T009 accepted (as issued).
- **Allowed:** docs evidence · architecture assertions · targeted tests.
- **Forbidden:** new product scope · closing unresolved decisions by invention · starting P10/P11.
- **Validation:** backend suites · frontend quality · `git diff --check`.
- **Done-when:** evidence pack ready for `TC-P09-GATE`.

### TC-P09-GATE — P09 Acceptance Gate

- **Purpose:** Formal phase exit.
- **Prerequisites:** T001–T010 accepted (as issued). Ceremonial `TRAVELCORE_TASK_CONFIRM: TC-P09-GATE` **not required** under USER continuity override (2026-08-17); stop only for architecture/path/SoT/unsafe/unlocked-decision.
- **Allowed:** evidence-only verification · SoT sync after accept · auto-start P10 PLAN after Gate ACCEPT (continuity override).
- **Forbidden:** starting P10/P11 product implementation before Gate ACCEPT · implementing Pricing/Booking/Search · rewriting history/force-push.
- **Validation:** gate checklist (§10).
- **Done-when:** architect ACCEPT → P09 COMPLETE.

---

## 9. Dependency Graph

```text
TC-P09-PLAN
   └─► T001 scaffolding
         └─► T002 TourProduct domain/persistence  (needs P09-R1; may need P09-R7)
               ├─► T003 i18n + translations        (may need P09-R5)
               ├─► T004 classification/origin/dest (may need P09-R2)
               ├─► T005 agency references          (may need P09-R3)
               ├─► T006 services/policies/requirements
               └─► T007 Tour↔Media
                     └─► T008 publishing + Access Admin  (may need P09-R4)
                           └─► T009 public hooks + SEO    (may need P09-R5/R6)
                                 └─► T010 evidence
                                       └─► TC-P09-GATE
```

Exact parallelization may be adjusted by architect on accept; Cursor must not invent skipped prerequisites.

---

## 10. Acceptance Strategy (Gate must verify)

1. Tour module separate schema `tour`; no cross-schema writes.
2. TourProduct shared core identity exists under Tour ownership.
3. Localization without `TitleFa`/`TitleEn`.
4. Classification / Origin / Destination refs by ID/contracts; Destination ≠ Tour.
5. Agency references by Party/Agency ID; Party ≠ Tour merge.
6. Services / Policies / Requirements baseline present (as issued).
7. Tour↔Media relations owned by Tour; MediaAssetId refs only.
8. Publishing lifecycle + Admin Tour baseline Access-backed; job-based UX.
9. Public Tour Core hooks baseline present (as issued).
10. SEO hooks without SEO owning Tour substance; IndexPolicy default per locked R6 (or explicitly deferred with evidence).
11. **No TourDeparture / FlightSegment / TourHotelOption / Experience itinerary product** from P10/P11 in P09 scope.
12. **No Pricing calculation · Booking · Search · Content CMS ownership** in Tour.
13. Experience/Package not collapsed into an unlocked nullable blob.
14. Evidence pack + tests green + clean tree.

---

## 11. Risks / Open Decisions

| ID | Topic | Status | Notes |
|----|-------|--------|-------|
| **P09-R1** | TourProduct model shape (shared core + typed Experience/Package specialization vs other) | **UNRESOLVED** | Must not invent polymorphism; STOP on T002 if unlocked. Must preserve Experience ≠ Package blob invariant. |
| **P09-R2** | Destination / Origin link cardinality (0..1 / 1..1 / 1..N; requiredness) | **UNRESOLVED** | ROADMAP lists Origin · Destinations; exact cardinalities not locked. |
| **P09-R3** | Agency reference shape (single PartyId vs role-typed refs / offering agency semantics) | **UNRESOLVED** | Do not merge Party into Tour. |
| **P09-R4** | Publishing/catalog status vs delete-archive lifecycle | **UNRESOLVED** | Do not invent hard-delete product; publication status may suffice — architect lock. |
| **P09-R5** | Slug ownership (Tour-localized current slug vs SEO-only route key) | **UNRESOLVED** | Expect P05/P07/P08 pattern (Tour owns current locale slug; SEO owns history/IndexPolicy) but **do not assume** until locked. |
| **P09-R6** | IndexPolicy default for public Tour | **UNRESOLVED** | Expect `noindex, follow` continuity from P05/P07/P08 but **do not assume** / do **not** invent Active=Index until locked. |
| **P09-R7** | Whether any Experience/Package specialty fields enter P09 vs deferred entirely to P10/P11 | **UNRESOLVED** | Prefer deferral; contract ID stubs only if architect-approved — do not invent P10/P11 product. |
| P08-R6/R7/R8 | Content widgets / Author / delete-archive | UNRESOLVED (Content) | Out of Tour product scope. |
| P07-R3 | Place delete/archive | UNRESOLVED (Place) | Out of Tour product scope. |
| P06-R8/R9 | Media delete / consumer alt override | Unresolved/Deferred (Media) | Tour uses Media defaults unless expanded. |

Cursor must **STOP** with `BLOCKED_ARCHITECT_DECISION_REQUIRED` when a task deadline depends on an UNRESOLVED R# — do not invent policy.

---

## 12. Phase Exit Criteria (P09 COMPLETE)

1. `TC-P09-T001`–`T010` COMPLETE / ACCEPTED (as issued).
2. `TC-P09-GATE` COMPLETE / ACCEPTED.
3. TourProduct≠TourDeparture · Experience/Package non-blob · Tour≠Pricing/Booking/Search/Content · Tour≠Place Hotel ownership evidenced.
4. P10/P11+ **NOT_STARTED** until Gate ACCEPT; under continuity override, P10 PLAN may auto-start after Gate ACCEPT without ceremonial phase token.
5. SoT (`PROJECT-STATE` · `ROADMAP` · this plan) coherent.

---

## 13. Cursor Execution Rules for This Plan

1. Envelope titles may be generic — use this plan’s task titles.
2. If ChatGPT DOM truncates envelopes, **this plan + ROADMAP + architecture docs are SoT**.
3. Prefer one product commit per task (+ optional SHA hygiene; dual commits usually NON_BLOCKING).
4. On baseline mismatch → STOP `BLOCKED_BASELINE_DRIFT`.
5. On open decisions with task deadline → STOP `BLOCKED_ARCHITECT_DECISION_REQUIRED`.
6. After PLAN PASS: do **not** auto-start T001 until architect accepts plan and issues Auto-Execute T001 (architect may Auto-Execute immediately after ACCEPT under PIPELINE).
7. Never force-push / rewrite history.
8. Continuity after gate/phase follows governance docs (NEW continuity rule USER 2026-08-17) — ceremonial `TRAVELCORE_TASK_CONFIRM` / `TRAVELCORE_PHASE_CONFIRM` no longer required under PIPELINE; STOP only for architecture choice, multi-path preference, SoT conflict, unsafe repo state, or unlocked decision deadlines.

---

## 14. Plan Delivery Checklist (this PLAN task)

- [x] `docs/plans/P09-implementation-plan.md` created (this file)
- [x] Scope matches ROADMAP § P09 + transition map § I + Tour boundaries
- [x] Non-goals explicitly exclude P10/P11/Pricing/Booking/Search/Content product
- [x] Open decisions listed (P09-R1–R7) as UNRESOLVED — no invented resolutions
- [x] Task map + gate checklist present
- [x] `docs/PROJECT-STATE.md` / `docs/ROADMAP.md` updated to P09 IN_PROGRESS (PLAN awaiting review)
- [ ] Commit + push on baseline `f9ab2e8`
- [ ] RESULT envelope returned to architect
