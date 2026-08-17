# P10 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P10-PLAN` |
| Phase | P10 — Experience Tour |
| Status | ACCEPTED (architect) · implementation IN_PROGRESS |
| Baseline | `67fc580` (`docs: sync ROADMAP for TC-P09-GATE review` — **TC-P09-GATE** ACCEPTED; P09 COMPLETE) |
| Authoritative sources | `docs/ROADMAP.md` § P10 · `docs/architecture/15-future-architecture-transition-map.md` § J · `04-module-boundaries.md` § Tour (Experience Tour) · `05-dependency-rules.md` · `docs/domain/module-ownership-matrix.md` · `docs/domain/glossary.md` (ExperienceTour · Itinerary · ItineraryDay · Stop · MealPlan adjacent) · P09 Tour Core locks (R1–R8; shared TourProduct delivered) · P07 Place locks (Attraction/Hotel catalog ≠ Tour ownership) · P06 Media locks (consumer owns relationship meaning) · P05 SEO locks · ADR 0001 · ADR 0007–0008 · ADR 0011–0014 |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P10** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

> **Envelope note:** Scope is authored from **repository SoT** (ROADMAP § P10 · transition map § J · Tour module boundaries · P09 RESOLVED R1–R8). **TC-P09-GATE** ACCEPTED (`67fc580`). **TC-P10-PLAN** ACCEPTED by architect; Auto-Execute issued for T001. Under PIPELINE continuity (USER 2026-08-17), ceremonial confirms are **not required**.

---

## 1. Phase Purpose

P10 باید **تخصص Experience** را روی ماژول Tour موجود (که در P09 هستهٔ مشترک TourProduct را تحویل داد) گسترش دهد تا:

1. **Experience specialization** به‌عنوان typed specialization روی TourProduct (قفل **P09-R1** / **P09-R7**) بدون تبدیل Package/Experience به یک blob nullable اجباری.
2. **Itinerary · ItineraryDay · Stop** به‌عنوان ساختارهای ساخت‌یافتهٔ برنامهٔ سفر تجربه‌محور تحت مالکیت Tour.
3. **پیوند Destination / Attraction (Place)** برای Stopها — ارجاع منطقی by ID/contracts؛ Destination و Place مالکیت entityهای خود را حفظ می‌کنند.
4. **Meals · Accommodation plan · Local transport · Equipment** به‌عنوان حقایق محصول Experience (نه Pricing · نه Booking · نه live inventory).
5. **Difficulty · Eligibility · Guide information** برای کهن‌الگوی Experience.
6. **اعتبارسنجی** از طریق archetype صفحهٔ **Experience Tour Detail** (public read + Admin ops در محدودهٔ صادرشده).
7. Invariantهای قفل‌شده حفظ شوند:
   - **TourProduct ≠ TourDeparture** (Departure = **P11**)
   - Experience itinerary ساخت‌یافته است — نه یک HTML بزرگ بدون مدل
   - Destination / Attraction مالکیت جدا می‌مانند (Stop فقط لینک می‌دهد)
   - Accommodation plan ≠ مالکیت Place Hotel catalog · ≠ `TourHotelOption` محصول P11
   - **Tour ≠ Pricing · Booking · Search · Agency Marketplace**

P09 تحویل داد: shared **TourProduct** + publishing/slug/media/admin/public hooks.  
P10 اضافه می‌کند: **Experience specialization / itinerary structures** تحت مالکیت Tour — **بدون** scaffold ماژول جدید (Tour از قبل وجود دارد؛ extend می‌شود).

P10 **TourDeparture / FlightSegment / TourHotelOption (P11)** · **Pricing (P12)** · **Booking** · **Search** · **Agency Marketplace (P13)** · **Public Tour Experience polish کامل (P14)** نیست. Specialty blob قفل‌نشده روی TourProduct اختراع نشود.

---

## 2. Starting Baseline

Accepted P09 final Gate + SoT sync HEAD:

| Item | Value |
|------|--------|
| P09 Gate | `TC-P09-GATE` COMPLETE / ACCEPTED (`67fc580`) |
| P09 product close | `TC-P09-T010` ACCEPTED (`0334bae`) |
| P09 Plan | `TC-P09-PLAN` ACCEPTED (`7de2518`) |
| Baseline HEAD | `67fc580` |
| P00–P09 | COMPLETE |
| Backend | Modular Monolith + … + Tour (shared TourProduct core; schema `tour`) |
| Frontend | Locale Admin Tour · Public Tour Core hooks · SEO/Media/Place/Destination/Content |
| Experience specialization / itinerary | **Not implemented** (architecture/docs + P09 specialty DEFER only) |

Continuity: under PIPELINE, ceremonial phase/gate tokens not required to proceed from Gate ACCEPT → this PLAN.

---

## 3. Authoritative Inputs

| Area | Sources |
|------|---------|
| Phase scope | `docs/ROADMAP.md` § P10 · transition map § J |
| Tour / Experience ownership | `04-module-boundaries.md` § Tour · Experience Tour bullets · ownership matrix · glossary ExperienceTour/Itinerary/ItineraryDay/Stop |
| Shared core already delivered | P09 — TourProduct · Classification · Origin/Dest · Agency · Services/Policies/Requirements · Media Cover/Gallery · Draft/Published/Inactive · slug · IndexPolicy defaults |
| Typed specialization posture | **P09-R1 RESOLVED** · **P09-R7 RESOLVED** (specialty deferred to P10/P11) |
| Place / Attraction adjacency | P07 — PlaceId for Attraction/Hotel-kind; no EF nav; no HotelBooking |
| Destination adjacency | P04 — hierarchy SoR; Tour/Stop links by ID/contracts |
| Media adjacency | P06 — MediaAssetId refs; Tour owns relationship meaning (new itinerary media roles need **P10-R#**) |
| SEO adjacency | P05 · P09-R5/R6 — path/slug/IndexPolicy for TourProduct; Experience Detail may reuse Tour publishable surface |
| Localization | ADR 0007–0008 |
| Authz | P03 Access + cookie Identity |
| Governance | ADR 0011–0014 · pipeline protocol · continuity (ceremonial confirms not required under PIPELINE) |

---

## 4. Scope (In)

1. **Extend existing Tour module** (no new module scaffold) for Experience specialization under locked **P09-R1** shape — exact specialization table/aggregate shape may be **P10-R1**.
2. **Itinerary** owned by Tour for Experience products (ownership/cardinality vs TourProduct may be **P10-R1**).
3. **ItineraryDay** ordered day structure under Itinerary.
4. **Stop** within days — with Destination and/or Attraction (Place) linking per locked cardinality (**P10-R2**).
5. **Meals** baseline belonging to Experience itinerary/day (shape may be **P10-R5**).
6. **Accommodation plan** as Experience product facts — references Place Hotel by ID if needed (**P10-R3**); must not invent `TourHotelOption` / package stay product.
7. **Local transport** baseline belonging to Experience itinerary (not FlightSegment / live Flight).
8. **Equipment** baseline for Experience products.
9. **Difficulty · Eligibility · Guide information** baseline (taxonomy/ref shape may be **P10-R6** / **P10-R7**).
10. Optional **itinerary/day/stop Media** relations if architect locks roles (**P10-R4**) — otherwise STOP rather than invent Hero/custom roles beyond P09 Cover/Gallery on TourProduct.
11. **Access-backed Admin** extension for Experience itinerary ops (job-based; reuse Tour Admin baseline from P09).
12. **Public Experience Tour Detail** archetype validation surface (Server Component First) composed from Tour + itinerary read models — not P14 polish factory.
13. Architecture/integration tests proving: Experience structures stay in Tour · no TourDeparture/FlightSegment/TourHotelOption product · no Pricing/Booking/Search ownership · Destination/Place remain SoR of their entities · no unlocked specialty blob.
14. Phase hardening evidence + `TC-P10-GATE`.

---

## 5. Non-Goals (Deferred)

| Deferred item | Owner phase / note |
|---------------|-------------------|
| TourDeparture · TransportSegment · FlightSegment · airports/carrier · TourHotelOption · package stay/MealPlan occupancy product | **P11** |
| Pricing rates · Quote · conversion | **P12** |
| Booking / Payment | **P19 / P20** |
| Search index / facets | **P15** |
| Agency Marketplace product | **P13** |
| Full Public Tour Experience polish / listing/search UX | **P14** (P10 validates Experience Detail archetype only) |
| Live Flight / HotelBooking inventory | **P22 / P21** |
| Content CMS / widgets ownership transfer | **P08** (unchanged) |
| UGC reviews owned by Tour | **Forbidden** |
| Inventing unlocked Experience specialty blob on TourProduct without **P10-R#** locks | **Forbidden** |
| New physical module / schema outside `tour` | **Forbidden** (extend Tour) |
| Bearer/JWT auth transport change | Forbidden (P03 cookie) |
| Auto-index because Experience detail exists | SEO Thin Content Guard — P09-R6 posture stands |

---

## 6. Architecture Constraints (Locked)

1. Modular Monolith — schema-per-module `tour`; no cross-module DbContext; **extend Tour**, do not invent a second Tour schema.
2. **TourProduct ≠ TourDeparture** — Departure product remains **P11**.
3. **P09-R1**: Core TourProduct + Typed Specialization — Experience specialization must follow that lock; do not collapse Experience+Package into one unlocked nullable bag.
4. Destination / Place (Attraction/Hotel) entities remain owned by Destination / Place; Stop only holds logical refs.
5. Media owns binaries; Tour owns any new itinerary media relationship semantics (**P10-R4** before inventing roles).
6. Accommodation plan ≠ Place Hotel ownership · ≠ P11 `TourHotelOption` product.
7. Pricing owns commercial calculation; Booking owns reservation; Search owns index — none become Tour SoR in P10.
8. Localization: no `TitleFa`/`TitleEn`/`NameFa` columns for any new Experience text.
9. Server Component First for Experience Detail surfaces in scope; Client islands allowlisted.
10. Access is authorization authority for Admin Experience mutations.
11. One Task → One Writer; evidence-based acceptance.
12. Do not continue across NEW baseline drift (STOP `BLOCKED_BASELINE_DRIFT`).
13. Do not start P11 product from P10 tasks.
14. Do not invent unresolved **P10-R#** policies — STOP `BLOCKED_ARCHITECT_DECISION_REQUIRED`.

---

## 7. Domain / Ownership Impact

| Concern | Owner after P10 |
|---------|-----------------|
| Shared TourProduct core (P09) | **Tour** (unchanged) |
| Experience typed specialization | **Tour** (new in P10; shape per **P10-R1**) |
| Itinerary · ItineraryDay · Stop | **Tour** |
| Stop→Destination / Attraction links | **Tour** (refs only; Destination/Place SoR unchanged) |
| Meals · Accommodation plan · Local transport · Equipment | **Tour** (Experience facts) |
| Difficulty · Eligibility · Guide information | **Tour** (shapes per R#) |
| Itinerary media order/role (if locked) | **Tour** |
| TourProduct Cover/Gallery | **Tour** (P09 unchanged) |
| Destination hierarchy | Destination (unchanged) |
| Place / Attraction / Hotel catalog | Place (unchanged) |
| MediaAsset bytes/metadata | Media (unchanged) |
| TourDeparture / FlightSegment / TourHotelOption | Tour (**P11** — not started) |
| Quote / rate calculation | Pricing (not started) |
| Booking / Payment | Booking / Payment (not started) |

---

## 8. Task Map

> Density mirrors P09 (T001–T010 + GATE). **No Tour module scaffolding task** — Tour already exists; T001 starts Experience specialization extension.

### TC-P10-T001 — Experience specialization baseline (extend Tour)

- **Purpose:** Introduce Experience typed specialization under existing TourProduct / `tour` schema per **P09-R1** and locked **P10-R1**.
- **Prerequisites:** PLAN ACCEPTED · **P10-R1 RESOLVED** (or STOP).
- **Allowed:** Experience specialization entity/table 1:1 (or as locked) with `TourProductId` · EF maps · migrations · unit tests · guards that Package specialty remains out of scope.
- **Forbidden:** inventing R1 · TourDeparture · FlightSegment · TourHotelOption · Pricing fields · collapsing Experience+Package into unlocked blob · new module outside Tour.
- **Validation:** build · ArchitectureTests · persistence smoke as scoped.
- **Done-when:** Experience specialization persistable under Tour ownership without P11 product leakage.

### TC-P10-T002 — Itinerary + ItineraryDay structure

- **Purpose:** Persist structured Itinerary and ordered ItineraryDay for Experience products.
- **Prerequisites:** T001 · itinerary ownership shape covered by **P10-R1** (or STOP).
- **Allowed:** Itinerary / ItineraryDay aggregates · ordering · translations for day titles/notes if needed (ADR 0008) · unit/persistence tests.
- **Forbidden:** HTML-only itinerary as sole model · inventing R1 · Departure calendars · package stay tables.
- **Validation:** unit + persistence as scoped.
- **Done-when:** Experience TourProduct can own a structured itinerary with ordered days.

### TC-P10-T003 — Stop + Destination / Attraction linking

- **Purpose:** Stop entities within days with Destination and/or Attraction (Place) logical links.
- **Prerequisites:** T002 · **P10-R2 RESOLVED** (or STOP).
- **Allowed:** Stop model · SortOrder · DestinationId / PlaceId (Attraction-kind) refs · Contracts existence validation · no cross-schema FK.
- **Forbidden:** Destination/Place EF nav collections · inventing R2 · copying Attraction aggregates into Tour.
- **Validation:** unit + architecture link guards.
- **Done-when:** Stops link per locked cardinality without ownership transfer.

### TC-P10-T004 — Meals + Accommodation plan baseline

- **Purpose:** Experience meals and accommodation plan facts (not commercial rates; not P11 TourHotelOption).
- **Prerequisites:** T002 · may need **P10-R3** / **P10-R5**.
- **Allowed:** meal models attached to day/itinerary as locked · accommodation plan fields/refs to Place Hotel by ID if R3 locks · Admin set/get later as issued.
- **Forbidden:** inventing R3/R5 · TourHotelOption product · HotelBooking · Pricing meal surcharges engine.
- **Validation:** unit/persistence scoped.
- **Done-when:** meals + accommodation plan persist as Experience facts under locked shapes (or STOP).

### TC-P10-T005 — Local transport + Equipment baseline

- **Purpose:** Local transport and equipment belonging to Experience itinerary/product.
- **Prerequisites:** T002 (and T001 for product-level equipment if locked there).
- **Allowed:** Tour-owned local transport / equipment models · translations as needed · no FlightSegment.
- **Forbidden:** FlightSegment · live Flight inventory · carrier/airport package product · inventing P11 transport.
- **Validation:** unit/contract tests.
- **Done-when:** local transport + equipment baseline persist and return on Experience reads.

### TC-P10-T006 — Difficulty · Eligibility · Guide information

- **Purpose:** Difficulty, eligibility, and guide information for Experience archetype.
- **Prerequisites:** T001 · may need **P10-R6** / **P10-R7**.
- **Allowed:** Tour-owned difficulty/eligibility/guide fields or value objects per lock · optional Party person ref **only if R7 locks** · no Identity merge.
- **Forbidden:** inventing R6/R7 · Access silo for guides · UGC ownership · Booking eligibility engine.
- **Validation:** unit/persistence scoped.
- **Done-when:** difficulty/eligibility/guide facts persist per locked shapes (or STOP).

### TC-P10-T007 — Itinerary Media relations (if locked) / otherwise contract posture

- **Purpose:** Define and implement itinerary/day/stop media relations **only after P10-R4**; otherwise record explicit defer and keep TourProduct Cover/Gallery (P09-R8) as the only media surface.
- **Prerequisites:** T002–T003 as issued · **P10-R4** when media deadline hits (RESOLVE or explicit DEFER — do not invent roles).
- **Allowed:** Tour-owned media link rows for locked roles · MediaAssetId + SortOrder · presentation via P06 app-proxy · ArchitectureTests.
- **Forbidden:** inventing Hero/custom roles without R4 · Media owning itinerary gallery meaning · StorageKey in Tour.
- **Validation:** unit + architecture (or evidence of R4 DEFER).
- **Done-when:** media posture matches locked R4 (implement or documented DEFER without invention).

### TC-P10-T008 — Access + Admin Experience itinerary baseline

- **Purpose:** Extend Access-backed Admin Tour job for Experience itinerary create/edit ops (days/stops/meals/equipment/etc. as issued).
- **Prerequisites:** T001–T007 as issued (media may be deferred per T007).
- **Allowed:** permission codes extension · `/[locale]/admin/...` Experience itinerary job · reuse Ready media picker for locked media · no raw-ID primary UX.
- **Forbidden:** silo CRUD for every table · DAM · Pricing/Booking Admin · P11 departure editor · inventing delete/archive beyond P09-R4 posture.
- **Validation:** host Access tests · frontend quality.
- **Done-when:** authorized operator can manage Experience itinerary baseline via Access-backed Admin.

### TC-P10-T009 — Public Experience Tour Detail archetype + SEO hooks

- **Purpose:** Validate Experience Tour Detail archetype (public read) composing TourProduct + Experience specialization + itinerary; SEO hooks reuse P09 Tour publication posture.
- **Prerequisites:** T008 · P09-R5/R6 remain in force.
- **Allowed:** public Server Components / read contracts · compose Destination/Place/Media via contracts · SEO resource hooks without changing IndexPolicy defaults · FA/EN responsive sanity as issued.
- **Forbidden:** inventing Published=Index · starting Search · P11 Foreign Package Detail product · P14 listing/search polish factory · SEO owning Tour body/itinerary text.
- **Validation:** host/SEO unit as scoped · frontend quality · public route checks as issued.
- **Done-when:** Experience Tour Detail archetype evidences structured itinerary without P11/Pricing leakage.

### TC-P10-T010 — Phase hardening tests & evidence pack

- **Purpose:** Regression pack proving Experience specialization + itinerary under Tour · TourProduct≠TourDeparture · no FlightSegment/TourHotelOption · Destination/Place ownership intact · green suites · gate evidence.
- **Prerequisites:** T001–T009 accepted (as issued).
- **Allowed:** docs evidence · architecture assertions · targeted tests.
- **Forbidden:** new product scope · closing unresolved decisions by invention · starting P11.
- **Validation:** backend suites · frontend quality · `git diff --check`.
- **Done-when:** evidence pack ready for `TC-P10-GATE`.

### TC-P10-GATE — P10 Acceptance Gate

- **Purpose:** Formal phase exit.
- **Prerequisites:** T001–T010 accepted (as issued). Ceremonial `TRAVELCORE_TASK_CONFIRM: TC-P10-GATE` **not required** under continuity override; stop only for architecture/path/SoT/unsafe/unlocked-decision.
- **Allowed:** evidence-only verification · SoT sync after accept · auto-start P11 PLAN after Gate ACCEPT (continuity).
- **Forbidden:** starting P11 product implementation before Gate ACCEPT · implementing Pricing/Booking/Search · rewriting history/force-push.
- **Validation:** gate checklist (§10).
- **Done-when:** architect ACCEPT → P10 COMPLETE.

---

## 9. Dependency Graph

```text
TC-P10-PLAN
   └─► T001 Experience specialization     (needs P10-R1)
         └─► T002 Itinerary + ItineraryDay (needs P10-R1)
               ├─► T003 Stop + Dest/Attraction links (needs P10-R2)
               ├─► T004 Meals + Accommodation plan   (may need P10-R3/R5)
               ├─► T005 Local transport + Equipment
               └─► T006 Difficulty · Eligibility · Guide (may need P10-R6/R7)
                     └─► T007 Itinerary Media posture (needs P10-R4 resolve/defer)
                           └─► T008 Access + Admin Experience itinerary
                                 └─► T009 Public Experience Detail + SEO hooks
                                       └─► T010 evidence
                                             └─► TC-P10-GATE
```

Exact parallelization may be adjusted by architect on accept; Cursor must not invent skipped prerequisites.

---

## 10. Acceptance Strategy (Gate must verify)

1. Tour schema `tour` extended — no second Tour module/schema.
2. Experience typed specialization exists under **P09-R1** / locked **P10-R1**.
3. Itinerary · ItineraryDay · Stop are structured (not HTML-only SoR).
4. Stop→Destination / Attraction refs by ID/contracts; Destination/Place ≠ Tour ownership.
5. Meals · Accommodation plan · Local transport · Equipment · Difficulty · Eligibility · Guide present as issued under locked R# (or explicitly deferred with evidence).
6. No TourDeparture · FlightSegment · TourHotelOption product in P10.
7. No Pricing calculation · Booking · Search · Agency Marketplace ownership in Tour.
8. Admin Experience itinerary Access-backed; job-based UX.
9. Public Experience Tour Detail archetype evidences itinerary composition.
10. SEO hooks without SEO owning Experience substance; IndexPolicy defaults per P09-R6.
11. Itinerary media roles match locked **P10-R4** (implement or documented DEFER).
12. Evidence pack + tests green + clean tree.

---

## 11. Risks / Open Decisions

| ID | Topic | Status | Notes |
|----|-------|--------|-------|
| **P10-R1** | Experience specialization + Itinerary ownership shape | **RESOLVED** | Experience specialization 1:1 with `TourProductId` (`TourKind.Experience` only). **Itinerary ownership:** `TourExperienceSpecialization` owns `ExperienceItinerary` as child aggregate (not standalone). **Cardinality:** 0..1 Itinerary per Experience. Day and Stop belong to Itinerary. Locked by architect 2026-08-17 (T001 ACCEPT → T002 Auto-Execute). |
| **P10-R2** | Stop → Destination / Attraction (Place) link cardinality | **RESOLVED** | DestinationId **0..1** optional · PlaceId **0..1** optional · Attraction = PlaceId with PlaceKind Attraction validation · both may coexist (no exclusivity) · logical refs only · no cross-schema FK · no ownership transfer. Locked by architect 2026-08-17 (T002 ACCEPT → T003 Auto-Execute). |
| **P10-R3** | Accommodation plan vs Place Hotel | **RESOLVED** | Experience owns accommodation plan facts (0..N entries) with optional logical PlaceId (Hotel-kind at app boundary). Place remains SoR. No TourHotelOption · no HotelBooking · no cross-schema FK. Locked architect 2026-08-17 (T004). |
| **P10-R4** | Media for itinerary / day / stop | **UNRESOLVED** | Whether day/stop media roles exist beyond TourProduct Cover/Gallery (P09-R8); roles/cardinality; or DEFER media to product-level only. |
| **P10-R5** | Meals model shape | **RESOLVED** | Meal items belong to ItineraryDay; closed enum Breakfast/Lunch/Dinner/Other; unique per day+type; no Pricing / surcharge. Locked architect 2026-08-17 (T004). |
| **P10-R6** | Difficulty / Eligibility taxonomy | **RESOLVED** | Difficulty = closed UX enum (Easy/Moderate/Challenging/Strenuous) on Experience. Eligibility = structured code/value/detail facts (not Booking rule engine). Equipment = structured code + Required/Recommended + optional detail. Locked by architect 2026-08-17 (T005 · ARCHITECT AUTONOMY). |
| **P10-R7** | Guide information shape | **UNRESOLVED** | Localized text-only vs optional Party person reference; no Identity/Access merge; no inventing guide marketplace. |
| **P10-R8** | Experience-only publishability rules | **UNRESOLVED** | Whether Experience specialization (itinerary completeness) gates catalog visibility beyond P09 Draft/Published/Inactive — or P09 lifecycle stands unchanged. |

Inherited (out of P10 invent scope):

| ID | Status | Note |
|----|--------|------|
| P09-R1–R8 | **RESOLVED** | Shared TourProduct locks stand; specialty deferred into P10/P11 as issued |
| P08-R6/R7/R8 | UNRESOLVED (Content) | Out of Experience product scope |
| P07-R3 | UNRESOLVED (Place) | Out of Experience product scope |
| P06-R8/R9 | Unresolved/Deferred (Media) | Tour uses Media defaults unless R4 expands |

Cursor must **STOP** with `BLOCKED_ARCHITECT_DECISION_REQUIRED` when a task deadline depends on an UNRESOLVED R# — do not invent policy.

---

## 12. Phase Exit Criteria (P10 COMPLETE)

1. `TC-P10-T001`–`T010` COMPLETE / ACCEPTED (as issued).
2. `TC-P10-GATE` COMPLETE / ACCEPTED.
3. Experience specialization + structured itinerary under Tour evidenced · TourProduct≠TourDeparture · no P11 FlightSegment/TourHotelOption · Destination/Place ownership intact.
4. P11+ **NOT_STARTED** until Gate ACCEPT; under continuity override, P11 PLAN may auto-start after Gate ACCEPT without ceremonial phase token.
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
8. Continuity after gate/phase follows governance docs (USER 2026-08-17) — ceremonial `TRAVELCORE_TASK_CONFIRM` / `TRAVELCORE_PHASE_CONFIRM` not required under PIPELINE; STOP only for architecture choice, multi-path preference, SoT conflict, unsafe repo state, or unlocked decision deadlines.
9. **This PLAN task is planning-only — no P10 product implementation code.**

---

## 14. Plan Delivery Checklist (this PLAN task)

- [x] `docs/plans/P10-implementation-plan.md` created (this file)
- [x] Scope matches ROADMAP § P10 + transition map § J + Tour Experience boundaries
- [x] Non-goals explicitly exclude P11 Departure/FlightSegment/TourHotelOption · Pricing · Booking · Search · Agency Marketplace
- [x] Open decisions listed (P10-R1–R8) as **UNRESOLVED** — no invented resolutions
- [x] Task map T001–T010 + GATE present (extend Tour; no new module scaffold)
- [x] Notes P09 shared TourProduct delivered; P10 adds Experience specialization / itinerary
- [x] Continuity: ceremonial confirms not required under PIPELINE
- [x] Explicit: planning only; no product code in this task
- [x] `docs/PROJECT-STATE.md` / `docs/ROADMAP.md` updated to P10 IN_PROGRESS (PLAN)
- [ ] Commit + push
- [ ] RESULT envelope returned to architect (parent)
