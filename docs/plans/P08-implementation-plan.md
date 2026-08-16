# P08 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P08-PLAN` |
| Phase | P08 — Content CMS |
| Status | AWAITING_ARCHITECT_REVIEW |
| Baseline | `37956ef` (`docs: mark P07 COMPLETE after TC-P07-GATE accept`) |
| Architect note on baseline | ChatGPT cited `003e9e4` (GATE SHA hygiene) immediately after phase confirm; observed repo HEAD is `37956ef` (post-accept SoT sync only — **NON_BLOCKING** docs drift; same recovery pattern as prior phases). |
| Authoritative sources | `docs/ROADMAP.md` § P08 · `docs/architecture/15-future-architecture-transition-map.md` § H · `04-module-boundaries.md` § Content · `05-dependency-rules.md` · `07-data-architecture.md` §11 (Content Block JSONB) · `docs/domain/module-ownership-matrix.md` · `docs/domain/glossary.md` (Article) · `docs/seo/01`–`05` (Content substance vs SEO route) · P05 SEO locks (R1/R2) · P06 Media locks (consumer owns relationship meaning; MediaAssetId refs) · P07 Place locks (catalog ≠ Content; PlaceId refs only) · ADR 0001 · ADR 0007–0008 · ADR 0011–0014 |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P08** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

> **Envelope note:** Architect authorized `TC-P08-PLAN` after USER `TRAVELCORE_PHASE_CONFIRM: P08` + `TRAVELCORE_MODE: PIPELINE` (planning only; no T001+ until Auto-Execute). Scope is authored from **repository SoT**. Architect may amend on review.

---

## 1. Phase Purpose

P08 باید ماژول **Content** را به‌عنوان SoR محتوای editorial TravelCore با مالکیت schema-per-module پیاده‌سازی کند تا:

1. **Article · LandingPage** (و در صورت قفل معماری، Guide) به‌عنوان هویت editorial متمرکز شوند.
2. **Category · Tag · Author/attribution** برای سازمان‌دهی و انتساب editorial (بدون دزدیدن Party/Identity).
3. **Content Blocks** ساخت‌یافته: heading · paragraph · image · gallery · FAQ · table · video · CTA · Tour/Hotel/Attraction widget (widgetها فقط ارجاع شناسه/قرارداد — نه مالکیت Tour/Place).
4. **پیوند معنادار به Destination** (و در صورت نیاز Place/Tour/Visa by ID) بدون انتقال مالکیت.
5. **Localization** عنوان/بدنه/slug با ردیف‌های locale — **بدون** ستون‌های `TitleFa`/`TitleEn`.
6. **Publication lifecycle** متعلق به Content؛ **SEO** مالک route binding / history / IndexPolicy / canonical / hreflang / sitemap است (قفل P05).
7. **Admin Content** job-based + Access-backed + **صفحات عمومی** editorial با ترکیب Server Component First.
8. Invariant قفل‌شده حفظ شود: **Content مالک editorial است؛ SEO محتوا را duplicate نمی‌کند** · ارجاع Tour/Place، آن‌ها را بخشی از Content نمی‌کند · UGC ≠ Content.

P08 **Tour product (P09+)** · **UGC (P16)** · **Search engine** · **HotelBooking** · **Advanced Content Graph (P26)** · **commerce** نیست.

---

## 2. Starting Baseline

Accepted P07 final baseline + post-accept docs sync:

| Item | Value |
|------|--------|
| P07 Gate | `TC-P07-GATE` COMPLETE / ACCEPTED (`84a0a48`) |
| Docs sync HEAD | `37956ef` |
| P00–P07 | COMPLETE |
| Backend | Modular Monolith + Identity/Access/Party + ReferenceData + Destination + SEO + Media + Place |
| Frontend | Locale Admin Destination/ReferenceData/Media/Place/SEO · Public Destination/Place · Media presentation (app proxy) |
| Content module | **Not implemented** (architecture/docs only) |

USER phase token received: `TRAVELCORE_PHASE_CONFIRM: P08`.

---

## 3. Authoritative Inputs

| Area | Sources |
|------|---------|
| Phase scope | `docs/ROADMAP.md` § P08 · transition map § H |
| Content ownership | `04-module-boundaries.md` § Content · module-ownership-matrix · glossary Article |
| SEO adjacency | `docs/seo/01`–`05` · P05 R1/R2 — Content owns substance; SEO owns route/index |
| Media adjacency | P06 — MediaAssetId refs; Content owns block/gallery meaning inside editorial |
| Destination/Place adjacency | P04/P07 — semantic links by ID/contracts only |
| Block storage hint | `07-data-architecture.md` §11 — JSONB ممکن برای کانفیگ Content Block پذیرفته‌شده |
| Localization | ADR 0007–0008 |
| Authz | P03 Access + cookie Identity |
| Governance | ADR 0011–0014 · pipeline protocol |

---

## 4. Scope (In)

1. Physical **Content** module scaffolding under `src/backend/Modules/Content/` (Contracts/Domain/Infrastructure) with dedicated DbContext + PostgreSQL schema `content`.
2. **Editorial content model** covering Article · LandingPage (exact polymorphism / Guide inclusion may be locked by **P08-R1**).
3. Strong ids: canonical Content identity strategy per **P08-R1** (e.g. `ContentId` / typed ids — do not invent before lock).
4. **Translations** for title/body/excerpt (+ localized slug if Content-owned) — forbid `TitleFa`/`TitleEn`.
5. **Category · Tag · Author/attribution** baseline (Author shape may be **P08-R7**).
6. **Content Blocks** engine for accepted block types listed in ROADMAP (storage strategy may be **P08-R2**).
7. **Destination link(s)** (requiredness/cardinality may be **P08-R5**) without Destination owning Articles.
8. Optional **Place/Tour/Visa** widget refs by ID only (Tour widgets may be deferred/contract-stubbed until P09 — **P08-R6**).
9. **Media** usage inside image/gallery/video blocks via `MediaAssetId` + Content-owned ordering/role inside the block graph.
10. **Access permissions** + minimal **Admin Content** operational baseline (job-based; not silo CRUD for every table).
11. **Public Content detail / landing** read model (Server Component First) composed without EF nav into Destination/Place/Media/Tour aggregates.
12. **SEO integration hooks** for Content publishable surfaces (binding/IndexPolicy via SEO contracts — Content does not become SEO engine). Default IndexPolicy posture may be **P08-R4**.
13. Architecture/integration tests proving Content ↛ Tour/Place ownership; no cross-schema writes; SEO ≠ Content substance duplication.
14. Phase hardening evidence + `TC-P08-GATE`.

---

## 5. Non-Goals (Deferred)

| Deferred item | Owner phase / note |
|---------------|-------------------|
| Tour product / itinerary / departure ownership | **P09+** |
| UGC Review / Travelogue ownership | **P16** (UGC ≠ Content) |
| Search index / facets for Content | Search phase |
| Advanced SEO Content Graph | **P26** |
| HotelBooking / live inventory | **P21** |
| Full CMS DAM / Media replace | Media remains SoR bytes (P06) |
| Bearer/JWT auth transport change | Forbidden (P03 cookie) |
| Bidirectional Content↔Tour domain dependency | Forbidden |
| Auto-index thin/stub landings | SEO Thin Content Guard — withhold Index |

---

## 6. Architecture Constraints (Locked)

1. Modular Monolith — schema-per-module; no cross-module DbContext.
2. **Content owns editorial substance**; SEO owns route/IndexPolicy/canonical/hreflang/sitemap mechanics.
3. Destination/Place/Tour/Visa are **referenced by ID/contracts** — never owned/mutated by Content.
4. Media owns binaries; **Content owns editorial block/gallery relationship semantics**.
5. Localization: no `TitleFa`/`TitleEn`/`BodyFa` columns.
6. Server Component First for public Content pages; Client islands allowlisted.
7. Access is authorization authority for Admin Content mutations.
8. SEO authority from P05 is not duplicated.
9. One Task → One Writer; evidence-based acceptance.
10. Do not continue across NEW baseline drift (STOP `BLOCKED_BASELINE_DRIFT`).
11. Do not start P09+ from P08 tasks.
12. Do not invent unresolved **P08-R#** policies — STOP `BLOCKED_ARCHITECT_DECISION_REQUIRED`.

---

## 7. Domain / Ownership Impact

| Concern | Owner after P08 |
|---------|-----------------|
| Article / LandingPage editorial identity + body/blocks | **Content** |
| Category / Tag / Author attribution (as issued) | **Content** |
| Destination hierarchy | Destination (unchanged) |
| Place catalog | Place (unchanged) |
| MediaAsset bytes/metadata | Media (unchanged) |
| Content image/gallery block order/role | **Content** |
| SEO route/IndexPolicy mechanics | SEO |
| UGC reviews | UGC (not started) |
| TourProduct | Tour (P09+) |

---

## 8. Task Map

### TC-P08-T001 — Content module scaffolding

- **Purpose:** Physical module + schema `content` + DI registration + migration runner convention proof.
- **Prerequisites:** PLAN ACCEPTED.
- **Allowed:** Contracts/Domain/Infrastructure shells · `ContentDbContext` · empty/initial migration · host wiring · architecture smoke.
- **Forbidden:** product entities beyond scaffolding · Admin UI · SEO engine changes · Tour/Place product code.
- **Validation:** build · ArchitectureTests · Persistence smoke as applicable.
- **Done-when:** Content module exists; schema ownership asserted; no P09 leakage.

### TC-P08-T002 — Content catalog domain + persistence baseline

- **Purpose:** Persist at least one editorial type end-to-end under **P08-R1**.
- **Prerequisites:** T001 · **P08-R1 RESOLVED** (or STOP).
- **Allowed:** domain aggregates/entities · EF maps · migrations · unit tests.
- **Forbidden:** inventing R1 · blocks engine full product · public UI · closing other R# by invention.
- **Validation:** unit + persistence as scoped.
- **Done-when:** accepted editorial type persistable under Content ownership.

### TC-P08-T003 — Localization + slug baseline

- **Purpose:** Locale rows for title/body/excerpt (+ slug if Content-owned) without Fa/En columns.
- **Prerequisites:** T002 · may need **P08-R3** lock for slug ownership.
- **Allowed:** translations · slug normalization mirroring Destination/Place patterns if R3 locks Place-like ownership.
- **Forbidden:** SEO history tables inside Content · `TitleFa` columns · inventing R3/R4.
- **Validation:** unit + architecture ADR 0008 guards.
- **Done-when:** localized editorial text persistable; slug policy matches locked R3 (or STOP).

### TC-P08-T004 — Category · Tag · Author attribution

- **Purpose:** Taxonomy + attribution baseline for editorial ops.
- **Prerequisites:** T002 · may need **P08-R7** for Author shape.
- **Allowed:** Category/Tag models · Author as locked · relationships owned by Content.
- **Forbidden:** Party aggregate merge · Identity ownership transfer · inventing R7.
- **Validation:** unit/persistence scoped.
- **Done-when:** taxonomy/attribution usable for Admin baseline without CMS dumping-ground.

### TC-P08-T005 — Content Blocks engine

- **Purpose:** Structured blocks for ROADMAP types; storage per **P08-R2**.
- **Prerequisites:** T002 · **P08-R2** when deadline hits (or STOP).
- **Allowed:** block graph/order · typed validators · JSONB only if R2 allows for accepted configs · MediaAssetId inside image/gallery/video blocks.
- **Forbidden:** embedding Tour/Place aggregates · Media owning block order · inventing Tour live widgets beyond R6.
- **Validation:** unit tests for block invariants.
- **Done-when:** accepted block set persistable/ordered; widget refs are IDs only.

### TC-P08-T006 — Destination (+ optional Place) semantic links

- **Purpose:** Meaningful Destination linkage without ownership transfer.
- **Prerequisites:** T002 · may need **P08-R5**.
- **Allowed:** Content-owned link fields/tables · Contracts existence queries · no cross-schema FK.
- **Forbidden:** Destination EF nav collections of Articles · inventing R5.
- **Validation:** unit + architecture Destination-link guards.
- **Done-when:** Article/Landing can reference Destination per locked cardinality.

### TC-P08-T007 — Access permissions + Admin Content baseline

- **Purpose:** Access-backed Admin editorial job for create/edit/publish ops (no delete invent if R8 open).
- **Prerequisites:** T003–T006 as issued · T005 if blocks required for Admin UX.
- **Allowed:** permission codes · `/[locale]/admin/...` Content job · Ready media picker reuse (P06/P07 pattern) · no raw-ID primary UX.
- **Forbidden:** silo CRUD for every table · DAM · delete/archive product if **P08-R8** unresolved · Tour Admin.
- **Validation:** host Access tests · frontend quality.
- **Done-when:** editor can manage baseline Content via Access-backed Admin.

### TC-P08-T008 — Public Content pages + SEO integration hooks

- **Purpose:** Public Article/Landing routes + SEO publication binding without SEO owning body text.
- **Prerequisites:** T007 · **P08-R3/R4** when deadline hits (or STOP).
- **Allowed:** public Server Components · compose Destination/Media via contracts · SEO resource type/hooks · default IndexPolicy per R4.
- **Forbidden:** inventing Index=Active · SEO owning Content body · starting Search · P09 Tour detail ownership.
- **Validation:** host/SEO unit as scoped · frontend quality · public route checks.
- **Done-when:** public editorial page works for accepted type; SEO hooks evidence without duplicating P05 engine.

### TC-P08-T009 — Phase hardening tests & evidence pack

- **Purpose:** Regression pack proving Content≠SEO substance · Content≠Tour/Place ownership · green suites · gate evidence.
- **Prerequisites:** T001–T008 accepted (as issued).
- **Allowed:** docs evidence · architecture assertions · targeted tests.
- **Forbidden:** new product scope · closing unresolved decisions by invention.
- **Validation:** backend suites · frontend quality · `git diff --check`.
- **Done-when:** evidence pack ready for `TC-P08-GATE`.

### TC-P08-GATE — P08 Acceptance Gate

- **Purpose:** Formal phase exit.
- **Prerequisites:** T001–T009 accepted (as issued) · USER `TRAVELCORE_TASK_CONFIRM: TC-P08-GATE`.
- **Allowed:** evidence-only verification · SoT sync after accept.
- **Forbidden:** starting P09 · implementing Tour/UGC · rewriting history/force-push.
- **Validation:** gate checklist (§10).
- **Done-when:** architect ACCEPT → P08 COMPLETE.

---

## 9. Dependency Graph

```text
TC-P08-PLAN
   └─► T001 scaffolding
         └─► T002 domain/persistence  (needs P08-R1)
               ├─► T003 i18n + slug     (may need P08-R3)
               ├─► T004 taxonomy/author (may need P08-R7)
               ├─► T005 blocks          (may need P08-R2 / R6)
               └─► T006 Destination links (may need P08-R5)
                     └─► T007 Admin Access baseline
                           └─► T008 Public + SEO hooks (may need P08-R3/R4)
                                 └─► T009 evidence
                                       └─► TC-P08-GATE
```

Exact parallelization may be adjusted by architect on accept; Cursor must not invent skipped prerequisites.

---

## 10. Acceptance Strategy (Gate must verify)

1. Content module separate schema; no cross-schema writes.
2. Article/LandingPage editorial identity exists under Content ownership.
3. Localization without `TitleFa`/`TitleEn`.
4. Category/Tag/Author attribution baseline present (as issued).
5. Content Blocks baseline for accepted types; widget refs are IDs only.
6. Destination relationship by reference; Destination ≠ Content.
7. Media used by ID; Content owns editorial block/gallery meaning.
8. Admin Content baseline Access-backed; job-based UX.
9. Public Content page baseline present.
10. SEO hooks without SEO owning Content body text.
11. **No Tour product / UGC / HotelBooking ownership** in Content.
12. Evidence pack + tests green + clean tree.

---

## 11. Risks / Open Decisions

| ID | Topic | Status | Notes |
|----|-------|--------|-------|
| **P08-R1** | Content model shape (single ContentItem + kind vs typed Article/LandingPage/Guide aggregates / TPH) | **UNRESOLVED** | Do not invent polymorphism; STOP on T002 if unlocked. |
| **P08-R2** | Block storage (relational typed rows vs JSONB document for accepted block configs) | **UNRESOLVED** | Data arch allows JSONB for accepted Content Block config — still needs architect lock for P08 default. |
| **P08-R3** | Slug ownership (Content-localized slug vs SEO-only route key) | **UNRESOLVED** | Expect P05/P07 pattern (Content owns current locale slug; SEO owns history/IndexPolicy) but **do not assume** until locked. |
| **P08-R4** | Public IndexPolicy default for new Content | **UNRESOLVED** | Expect `noindex, follow` continuity from P05/P07 but **do not assume** until locked. |
| **P08-R5** | Destination link requiredness/cardinality | **UNRESOLVED** | ROADMAP requires meaningful Destination links; exact 0..1 / 1..1 / 1..N not locked. |
| **P08-R6** | Tour/Hotel/Attraction widget depth in P08 | **UNRESOLVED** | Prefer contract-only ID widgets; full Tour compose may defer to P09 — architect lock. |
| **P08-R7** | Author model (Content-owned Author vs Party reference only) | **UNRESOLVED** | Do not merge Party into Content. |
| **P08-R8** | Content delete/archive lifecycle | **UNRESOLVED** | Do not invent hard-delete product; publication status may suffice — architect lock. |
| P07-R3 | Place delete/archive | UNRESOLVED (Place) | Out of Content product scope. |
| P06-R8/R9 | Media delete / consumer alt override | Unresolved/Deferred (Media) | Content uses Media defaults unless expanded. |

Cursor must **STOP** with `BLOCKED_ARCHITECT_DECISION_REQUIRED` when a task deadline depends on an UNRESOLVED R# — do not invent policy.

---

## 12. Phase Exit Criteria (P08 COMPLETE)

1. `TC-P08-T001`–`T009` COMPLETE / ACCEPTED (as issued).
2. `TC-P08-GATE` COMPLETE / ACCEPTED.
3. Content≠SEO-substance-duplication · Content≠Tour/Place ownership evidenced.
4. P09+ **NOT_STARTED** without separate phase confirm.
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

- [x] `docs/plans/P08-implementation-plan.md` created (this file)
- [x] Scope matches ROADMAP § P08 + transition map § H + Content boundaries
- [x] Non-goals explicitly exclude Tour/UGC/HotelBooking/P09+ product
- [x] Open decisions listed (P08-R1–R8)
- [x] Task map + gate checklist present
- [x] `docs/PROJECT-STATE.md` / `docs/ROADMAP.md` updated to P08 IN_PROGRESS (PLAN awaiting review)
- [x] Commit + push on baseline `37956ef`
- [ ] RESULT envelope returned to architect
