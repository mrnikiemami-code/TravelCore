# TravelCore Project State

این سند نقطهٔ ورود سریع برای بازیابی وضعیت پروژه است تا ChatGPT، Cursor، Hermes یا توسعه‌دهندهٔ جدید بدون اتکا به تاریخچهٔ چت، وضعیت فعلی را بفهمد.

جزئیات معماری در اسناد اختصاصی است؛ این فایل **فهرست وضعیت و بازیابی** است، نه طراحی تفصیلی.

### Emergency ChatGPT Recovery

اگر گفتگوی معمار ChatGPT از دست رفت، قبل از ادامه این Prompt را در Cursor اجرا کنید:

`docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md`

---

## Project Identity

| فیلد | مقدار |
|------|--------|
| Project | TravelCore |
| Repository | `mrnikiemami-code/TravelCore` |
| Architecture | Modular Monolith |
| Backend | .NET 10 / ASP.NET Core 10 Minimal API |
| Frontend | Next.js 16 / React 19 / TypeScript |
| Primary Database | PostgreSQL |
| Supporting infrastructure planned | Redis · S3-compatible Object Storage |

---

## Current Status

| فیلد | مقدار |
|------|--------|
| Current Phase | **P20 — Payment** (**PLAN ACCEPTED** · **P20-R1–R4 = RESOLVED** · **P20-R5–R8 OPEN** · T004 idempotency/reconciliation) |
| Previous Phase | **P18 — Trip Planner / Lead Experience** (**COMPLETE** — `TC-P18-GATE` ACCEPTED `73605aa`) |
| P00 | COMPLETE / ACCEPTED |
| P00 Final Gate | TC-P00-GATE — PASS |
| P00 Closure Task | TC-P00-CLOSE |
| Last Accepted P00 Task | TC-P00-T008 |
| Accepted Architecture Commit (T008 content) | `1bd4e95` |
| Acceptance / State Commit (T008A) | `0074437` |
| P00 Closure Commit | `6c65cb9` |
| TC-GOV-T001 | COMPLETE / ACCEPTED |
| TC-GOV-T001 Architecture/Protocol Commit | `f44f11e` |
| TC-GOV-T001A | COMPLETE / ACCEPTED |
| TC-GOV-T001A Activation Commit | `476ae67` |
| TC-GOV-T002 | COMPLETE / ACCEPTED |
| TC-GOV-T002 Protocol Consolidation Commit | `1cfe48a` |
| TC-GOV-T002A | COMPLETE / ACCEPTED (`1f9ad48`) |
| Last Accepted Commit | `b372367` (`TC-P12-GATE`) · P12 COMPLETE / ACCEPTED |
| ADR 0001–0014 | ALL Accepted |
| Unresolved Proposed ADR | NO |
| Accepted Pipeline Governance | ADR 0013 · ADR 0014 |
| Canonical Pipeline Entry | [`docs/ai/TRAVELCORE-PIPELINE-PROTOCOL.md`](ai/TRAVELCORE-PIPELINE-PROTOCOL.md) |
| Pipeline Protocol | **READY** |
| Pipeline Runtime Policy | [`docs/ai/pipeline-runtime-policy.json`](ai/pipeline-runtime-policy.json) |
| Operating Modes | HUMAN (default) / PIPELINE (USER opt-in) |
| Default Mode | **HUMAN** |
| Current Runtime Mode | **PIPELINE** |
| Automatic Pipeline | **ON** (USER re-entered PIPELINE this session; `TRAVELCORE_PHASE_CONFIRM: P08`) |
| Agent Handoff Envelopes | ACTIVE (ADR 0013) |
| Protocol | `TRAVELCORE_CURSOR_TASK_V1` · `TRAVELCORE_CURSOR_RESULT_V1` |
| Future Architecture Transition Map | [`docs/architecture/15-future-architecture-transition-map.md`](architecture/15-future-architecture-transition-map.md) |
| Agent Handoff Architecture | [`docs/architecture/16-agent-handoff-and-phase-gates.md`](architecture/16-agent-handoff-and-phase-gates.md) |
| Human/Pipeline Modes Architecture | [`docs/architecture/17-human-and-pipeline-operating-modes.md`](architecture/17-human-and-pipeline-operating-modes.md) |
| Handoff Protocol Docs | [`docs/ai/TRAVELCORE-PIPELINE-PROTOCOL.md`](ai/TRAVELCORE-PIPELINE-PROTOCOL.md) · [`01`](ai/01-chatgpt-cursor-handoff-protocol.md) · [`02`](ai/02-execution-state-machine.md) · [`03`](ai/03-human-confirmation-gates.md) · [`04`](ai/04-human-and-pipeline-modes.md) |
| Repository Normalization | TC-P00-T003R — PASS / ACCEPTED (`840c3e5`) |
| Emergency ChatGPT Recovery Drill | PASS |
| TC-P00-T007R | PASS (SAFE EXTENSION) |
| TC-P00-T008R | PASS |
| Repository Bootstrap | COMPLETE |
| Architecture Brain | COMPLETE |
| Master Execution Roadmap | [`docs/ROADMAP.md`](ROADMAP.md) |
| Emergency ChatGPT Recovery | [`docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md`](prompts/START-HERE-IF-CHATGPT-IS-LOST.md) |
| Current Active Product Task | `TC-P20-T004` — idempotency / retry safety / reconciliation baseline (AWAITING_ARCHITECT_REVIEW) |
| Current Next Task | Architect review of `TC-P20-T004` RESULT → do **not** execute T005 until T004 ACCEPTED; do not invent P20-R5–R8 |
| P01 | **COMPLETE** |
| P01 Plan | `TC-P01-PLAN-R1` Architect Accepted |
| P01 Implementation Started | **YES** |
| Last P01 Implementation Commit | `2370316` (`TC-P01-T019`) |
| P01 Phase Gate | **TC-P01-GATE** COMPLETE / ACCEPTED (`0853d04`) |
| P02 | **COMPLETE** |
| P02 Plan | `TC-P02-PLAN` COMPLETE / ACCEPTED (`47475ba`) — [`docs/plans/P02-frontend-foundation-walking-skeleton.md`](plans/P02-frontend-foundation-walking-skeleton.md) |
| P02 Implementation Started | **YES** (`TC-P02-T001`) |
| P02 Phase Gate | **TC-P02-GATE** COMPLETE / ACCEPTED (`4eacff5`) |
| P03 | **COMPLETE** (AUTHORIZED via `TRAVELCORE_PHASE_CONFIRM: P03`; closed by `TC-P03-GATE`) |
| P03 Plan | `TC-P03-PLAN` COMPLETE / ACCEPTED (`a779726`) — [`docs/plans/P03-implementation-plan.md`](plans/P03-implementation-plan.md) |
| P03 Implementation Started | **YES** (`TC-P03-T001`) |
| P03 Phase Gate | **TC-P03-GATE** COMPLETE / ACCEPTED (`6a8a5ce`) |
| P03 Gate Evidence | [`docs/plans/P03-GATE-acceptance-evidence.md`](plans/P03-GATE-acceptance-evidence.md) |
| P04 | **COMPLETE** (closed by `TC-P04-GATE` ACCEPTED `f70991f`) |
| P05 | **COMPLETE** (closed by `TC-P05-GATE` ACCEPTED `7f234e8`; R1 `bde6661`) |
| P05 Plan | `TC-P05-PLAN` COMPLETE / ACCEPTED — [`docs/plans/P05-implementation-plan.md`](plans/P05-implementation-plan.md) |
| P05 Plan Remediation | `TC-P05-PLAN-R1` COMPLETE / ACCEPTED — [`docs/plans/P05-PLAN-R1-baseline-reconciliation.md`](plans/P05-PLAN-R1-baseline-reconciliation.md) |
| P05-R1 (slug history ownership) | **RESOLVED** — Destination owns current `DestinationTranslation.Slug`; SEO owns path history/reservation/redirect mechanics |
| P05-R2 (default IndexPolicy) | **RESOLVED** — default missing policy = `noindex, follow`; explicit Index requires eligibility |
| P06 | **COMPLETE** (closed by `TC-P06-GATE` ACCEPTED `da345b5`; hygiene `0d2edad`) |
| P06 Plan | `TC-P06-PLAN` **COMPLETE / ACCEPTED** (`87069e4`) — [`docs/plans/P06-implementation-plan.md`](plans/P06-implementation-plan.md) |
| P06 Gate Evidence | [`docs/plans/P06-GATE-acceptance-evidence.md`](plans/P06-GATE-acceptance-evidence.md) |
| P06-T001 | **COMPLETE / ACCEPTED** (`e5bfd39`) |
| P06-T002 | **COMPLETE / ACCEPTED** (`020ce99`) |
| P06-T003 | **COMPLETE / ACCEPTED** (`cf95e5c`) |
| P06-T004 | **COMPLETE / ACCEPTED** (`7f83885`) — upload + validation; P06-R6 DENY SVG |
| P06-T005 | **COMPLETE / ACCEPTED** (`91444ad`) — variants + dimensions; **P06-R3 RESOLVED** (sync + sizing 1600/960/320) |
| P06-T006 | **COMPLETE / ACCEPTED** (`166e9db`) — focal metadata; coordinate policy reconciled in `TC-P06-T006-R1` (`b6f0cfb`) |
| P06-T007 | **COMPLETE / ACCEPTED** (`85c8e7a`) — MediaAsset alt/caption translations (ADR 0008; no AltFa/AltEn) |
| P06-T008 | **COMPLETE / ACCEPTED** (`f50cce3`; hygiene `1736a66`) — optimization contract + **P06-R1 RESOLVED DEFER** |
| P06-T009 | **COMPLETE / ACCEPTED** (`3a25e7d`; hygiene `d3ce295`/`71b2886`) — app-proxy public delivery; **P06-R4 RESOLVED APP PROXY** |
| P06-T010 | **COMPLETE / ACCEPTED** (`05ef0ac`) — contract-only consumer reference proof; **P06-R5 RESOLVED CONTRACT-ONLY** |
| P06-T011 | **COMPLETE / ACCEPTED** (`8b0de5a`) — Admin Media operational baseline (upload/inspect/alt/focal; R8 no-delete; R9 no consumer alt override; R5 no Destination assign) |
| P06-T012 | **COMPLETE / ACCEPTED** (`8981312`; hygiene `acfed76`) — hardening + evidence pack [`plans/P06-T012-hardening-and-evidence-pack.md`](plans/P06-T012-hardening-and-evidence-pack.md) |
| P06-GATE | **COMPLETE / ACCEPTED** (`da345b5`; hygiene `0d2edad`) |
| P06 Focal Coordinate Policy | **RESOLVED** — normalized [0,1] top-left (`TC-P06-T006-R1`) |
| P06-R1 (WebP/AVIF pipeline) | **RESOLVED — DEFER** — out of P06; evidence [`plans/P06-T008-optimization-contract-and-r1-defer.md`](plans/P06-T008-optimization-contract-and-r1-defer.md) |
| P06-R2 (object-storage ownership) | **RESOLVED** — Media-owned storage abstraction first; not Platform-wide `IObjectStorage` |
| P06-R3 (variant generation) | **RESOLVED** — SYNCHRONOUS; sizing large=1600 / medium=960 / thumbnail=320; fit-within; no crop/upscale; GIF fail-closed |
| P06-R4 (public URL strategy) | **RESOLVED — APP PROXY** — TravelCore delivery endpoints; anonymous Ready-only; StorageKey never public |
| P06-R5 (Destination MediaAssetId) | **RESOLVED — CONTRACT-ONLY** — `MediaAssetReference` + ArchitectureTests; no Destination schema MediaAssetId |
| P06-R6 (SVG acceptance) | **RESOLVED** — DENY `image/svg+xml` / `.svg` / detected SVG-XML payload |
| P06-R7 (malware/AV scanning) | **DEFERRED** — security requirement recorded; not in P06 product delivery |
| P06-R8 (domain delete lifecycle) | **UNRESOLVED** — OK for gate (no delete UX / not in P06 product scope; do not invent) |
| P06-R9 (consumer alt override) | **DEFERRED** — Media owns default alt/caption only |
| P07 | **COMPLETE** (closed by `TC-P07-GATE` ACCEPTED `84a0a48`; hygiene `8136455`/`003e9e4`) |
| P07 Plan | `TC-P07-PLAN` **COMPLETE / ACCEPTED** (`5dbc152`) — [`docs/plans/P07-implementation-plan.md`](plans/P07-implementation-plan.md) |
| P07 Gate Evidence | [`docs/plans/P07-GATE-acceptance-evidence.md`](plans/P07-GATE-acceptance-evidence.md) |
| P07-T001 | **COMPLETE / ACCEPTED** (`108ac34`; hygiene `a245358`) |
| P07-T002 | **COMPLETE / ACCEPTED** (`83529cf`; hygiene `d127ee7`) — Place catalog domain + persistence baseline |
| P07-T002-R1 | **COMPLETE / ACCEPTED** (`0b86f05`; hygiene `77f5386`) — PlaceId identity + T002 scope reconciliation |
| P07-T003 | **COMPLETE / ACCEPTED** (`3ec0f4c`; hygiene `5850e52`) — Localization + Destination link + geo/address |
| P07-T004 | **COMPLETE / ACCEPTED** (`6258003`; hygiene `b62b746`) — Facilities · classification · catalog status |
| P07-T005 | **COMPLETE / ACCEPTED** (`6246a09`; hygiene `0144f8d`) — Place↔Media Cover/Gallery |
| P07-T006 | **COMPLETE / ACCEPTED** (`74e8540`; hygiene `61ff89d`) — Access + Admin Place baseline |
| P07-T006-R1 | **COMPLETE / ACCEPTED** (`e4b5201`; hygiene `48aaaea`) — Admin Ready-media visual picker |
| P07-T007 | **COMPLETE / ACCEPTED** (`1c76f6b`; hygiene `b47f6de`) — Public Place detail + SEO hooks |
| P07-T008 | **COMPLETE / ACCEPTED** (`f7843cc`; hygiene `2d10fbd`/`fcefadd`) — hardening + evidence pack [`plans/P07-T008-hardening-and-evidence-pack.md`](plans/P07-T008-hardening-and-evidence-pack.md) |
| P07-GATE | **COMPLETE / ACCEPTED** (`84a0a48`; hygiene `8136455`/`003e9e4`) |
| P07-R1 (Place model shape) | **RESOLVED** — CORE PLACE + TYPED SPECIALIZATION (`PlaceId` only; Hotel/Restaurant/Attraction 1:1 tables; no TPH; no HotelBooking fields) |
| P07-R2 (Destination link requiredness) | **RESOLVED** — OPTIONAL SINGLE LOGICAL REFERENCE (0..1; Place-owned nullable DestinationId; no cross-schema FK; Contracts existence validation) |
| P07-R3 (Place delete/archive) | **UNRESOLVED** — OK for gate (no delete/archive product; do not invent) |
| P07-R4 (Slug ownership) | **RESOLVED** — PLACE owns current `PlaceTranslation.Slug`; SEO owns route history/redirects/IndexPolicy |
| P07-R5 (Public IndexPolicy default) | **RESOLVED** — default **noindex, follow**; Active/public ≠ Index |
| P08 | **COMPLETE** (closed by `TC-P08-GATE` ACCEPTED `576b7fa`; hygiene `6b72e60`) |
| P09 | **COMPLETE** (closed by `TC-P09-GATE` ACCEPTED `67fc580`; product T010 `0334bae`) |
| P09 Plan | `TC-P09-PLAN` **COMPLETE / ACCEPTED** (`7de2518`) — [`docs/plans/P09-implementation-plan.md`](plans/P09-implementation-plan.md) |
| P09-T001 | **COMPLETE / ACCEPTED** (`4794e6e`) — Tour module scaffolding (`tour` schema) |
| P09-T002 | **COMPLETE / ACCEPTED** (`a70331c`) — TourProduct shared-core + persistence (`tour_products`; P09-R1/R7) |
| P09-T003 | **COMPLETE / ACCEPTED** (`0bd50de`) — TourProduct translations (title/description locale rows; no slug) |
| P09-T004 | **COMPLETE / ACCEPTED** (`32a4701`) — Classification · Origin · Destination links (P09-R2) |
| P09-T005 | **COMPLETE / ACCEPTED** (`855f7a3`) — Agency reference (AgencyId 0..1; P09-R3) |
| P09-T006 | **COMPLETE / ACCEPTED** (`7e7ba6d`) — Services · Policies · Requirements baseline |
| P09-T007 | **COMPLETE / ACCEPTED** (`f0777f1`) — Tour↔Media Cover/Gallery (P09-R8) |
| P09-T008 | **COMPLETE / ACCEPTED** (`69e8f38`) — Publishing + localized slug + public/SEO hooks (P09-R4/R5/R6) |
| P09-T009 | **COMPLETE / ACCEPTED** (`e1fc751`) — Access-backed Admin Tour catalog baseline |
| P09-T010 | **COMPLETE / ACCEPTED** (`0334bae`) — Public Tour hardening + evidence pack |
| P09-GATE | **COMPLETE / ACCEPTED** (`67fc580`) — evidence [`plans/P09-GATE-acceptance-evidence.md`](plans/P09-GATE-acceptance-evidence.md) |
| P10 | **COMPLETE** |
| P10-GATE | **COMPLETE / ACCEPTED** (`c351bf9`) — evidence [`plans/P10-GATE-acceptance-evidence.md`](plans/P10-GATE-acceptance-evidence.md) |
| P10-R1…R8 | **ALL RESOLVED** |
| P11 | **COMPLETE** — GATE ACCEPTED (`6f7ea12`) · R1..R8 RESOLVED |
| P12 | **COMPLETE / ACCEPTED** — GATE `b372367` · T001–T009 ACCEPTED · **P12-R1…R8 RESOLVED** · evidence [`plans/P12-GATE-acceptance-evidence.md`](plans/P12-GATE-acceptance-evidence.md) |
| P12 Plan | `TC-P12-PLAN` COMPLETE / ACCEPTED (`d26078d`) — [`docs/plans/P12-implementation-plan.md`](plans/P12-implementation-plan.md) |
| P12-T001 | **COMPLETE / ACCEPTED** (`7c2e488`) — Pricing module scaffolding (`pricing` schema) |
| P12-T002 | **COMPLETE / ACCEPTED** (`6c1b4ce`) — Money / Currency baseline (platform Money reuse · EF owned mapping) |
| P12-T003 | **COMPLETE / ACCEPTED** (`58de552`) — Price + PriceComponent (polymorphic TargetType+TargetId · Base/Fee/Tax) |
| P12-T004 | **COMPLETE / ACCEPTED** (`81a3f26`) — Quote baseline (PriceSnapshot + Expiration; architect reorder vs old “Departure pricing attachment”) |
| P12-T005 | **COMPLETE / ACCEPTED** (`c90931d`) — Occupancy/passenger category pricing baseline (`PriceOccupancyRule`) |
| P12-T006 | **COMPLETE / ACCEPTED** (`e1d01c4`) — Admin Pricing baseline (Pricing-owned Admin API + Access `pricing.prices.read`/`write`) |
| P12-T007 | **COMPLETE / ACCEPTED** (`87b5dac`) — Quote requested-display-currency metadata + FX boundary contracts (no ExchangeRate table / no FX calculation) |
| P12-T008 | **COMPLETE / ACCEPTED** (`520a46d`) — Public read-only price summary query (currency, components, occupancy prices) by logical target |
| P12-T009 | **COMPLETE / ACCEPTED** (`a522dd5`) — Hardening + evidence pack [`plans/P12-T009-hardening-and-evidence-pack.md`](plans/P12-T009-hardening-and-evidence-pack.md) |
| P12-GATE | **COMPLETE / ACCEPTED** (`b372367`) — evidence [`plans/P12-GATE-acceptance-evidence.md`](plans/P12-GATE-acceptance-evidence.md) |
| P13 | **COMPLETE** — Gate ACCEPTED (`c0bcd78`) · R1–R7 RESOLVED · T008 vacant |
| P13 Plan | `TC-P13-PLAN` COMPLETE / ACCEPTED — [`docs/plans/P13-implementation-plan.md`](plans/P13-implementation-plan.md) |
| P13-T001 | **COMPLETE / ACCEPTED** (`9f61763`) — Agency Marketplace module scaffolding (`agency_marketplace` schema) |
| P13-T002 | **COMPLETE / ACCEPTED** (`809eb49`) — AgencyProfile commercial layer over Party identity |
| P13-T003 | **COMPLETE / ACCEPTED** (`a665272`) — AgencyOffer marketplace listing (logical TourProduct Guid) |
| P13-T004 | **COMPLETE / ACCEPTED** (`87931d9`) — AgencyOffer commercial terms boundary (no Price override) |
| P13-T005 | **COMPLETE / ACCEPTED** (`7234cc1`) — AgencyOffer capacity boundary (no seat inventory) |
| P13-T006 | **COMPLETE / ACCEPTED** (`8098a24`) — Agency Marketplace panel baseline (profile + offer ops) |
| P13-T007 | **COMPLETE / ACCEPTED** (`98ea1d1`) — AgencyOffer publishing and moderation baseline (not SEO / not CatalogStatus) |
| P13-T008 | **VACANT** — original publishing slot delivered as T007; no independent capability invented |
| P13-T009 | **COMPLETE / ACCEPTED** (`d813dbd`) — hardening + evidence pack [`plans/P13-T009-hardening-and-evidence-pack.md`](plans/P13-T009-hardening-and-evidence-pack.md) |
| P13-GATE | **COMPLETE / ACCEPTED** (`c0bcd78`) — evidence [`plans/P13-GATE-acceptance-evidence.md`](plans/P13-GATE-acceptance-evidence.md) |
| P14 | **COMPLETE / ACCEPTED** — Plan ACCEPTED · **P14-R1–R8 RESOLVED** · Gate ACCEPTED (`608216d`) |
| P14 Plan | `TC-P14-PLAN` COMPLETE / ACCEPTED — [`docs/plans/P14-implementation-plan.md`](plans/P14-implementation-plan.md) |
| P14-T001 | **COMPLETE / ACCEPTED** (`a7bd549`) — Public Experience surface inventory (Detail/Listing/Landing; no Search/catalog ownership) |
| P14-T002 | **COMPLETE / ACCEPTED** (`99818dd`) — public detail sticky presentation (P14-R2; Sticky Action ≠ Booking) |
| P14-T003 | **COMPLETE / ACCEPTED** (`f0e3df3`) — listing vs SEO landing boundary (P14-R3) |
| P14-SYNC001 | **COMPLETE / ACCEPTED** — origin/main synchronized at `f0e3df3` |
| P14-T004 | **COMPLETE / ACCEPTED** (`0b4fcbe`) — shared Detail shell + Experience specialized sections (P14-R4) |
| P14-T005 | **COMPLETE / ACCEPTED** (`c34e5b0`) — Related Tours composition (P14-R5; same-destination; not recommendation) |
| P14-T006 | **COMPLETE / ACCEPTED** (`5258e20`) — Content enrichment composition (P14-R6; Destination-based; Content remains CMS SoT) |
| P14-T007 | **COMPLETE / ACCEPTED** (`903cd29`) — Public AgencyOffer presentation (P14-R7; inquiry-only; Marketplace owns facts) |
| P14-T008 | **COMPLETE / ACCEPTED** (`a0209bd`) — Filter presentation boundary (P14-R8; presentation only; faceting = P15) |
| P14-T009 | **COMPLETE / ACCEPTED** (`6c0e218`) — Hardening + evidence pack |
| P14-GATE | **COMPLETE / ACCEPTED** (`608216d`) — Acceptance evidence |
| P15 | **COMPLETE / ACCEPTED** — Plan ACCEPTED · **P15-R1–R7 RESOLVED** · Gate ACCEPTED (`4e2098d`) |
| P15 Plan | `TC-P15-PLAN` COMPLETE / ACCEPTED (`fba7a51`) — [`docs/plans/P15-implementation-plan.md`](plans/P15-implementation-plan.md) |
| P15-T001 | **COMPLETE / ACCEPTED** (`bea92a1`) — Search module scaffolding (`search` schema) |
| P15-T002 | **COMPLETE / ACCEPTED** (`2b3c9d2`) — Search hybrid read-model / index abstraction |
| P15-T003 | **COMPLETE / ACCEPTED** (`2631c4e`) — Search projection synchronization boundary |
| P15-T004 | **COMPLETE / ACCEPTED** (`413d6fe`) — Search faceting ownership boundary |
| P15-T005 | **COMPLETE / ACCEPTED** (`7b22225`) — Search ranking model boundary |
| P15-T006 | **COMPLETE / ACCEPTED** (`edc176f`) — Search AI-readiness / semantic retrieval boundary |
| P15-T007 | **COMPLETE / ACCEPTED** (`183d09d`) — Public Search query API contract |
| P15-T008 | **VACANT** — no independent product scope |
| P15-T009 | **COMPLETE / ACCEPTED** (`b741bc5`) — Search hardening and evidence pack |
| P15-GATE | **COMPLETE / ACCEPTED** (`4e2098d`) — Acceptance evidence |
| P16 | **COMPLETE / ACCEPTED** — Plan ACCEPTED · **P16-R1–R8 RESOLVED** · T001–T009 ACCEPTED · `TC-P16-GATE` `538f3fc` |
| P16 Plan | `TC-P16-PLAN` COMPLETE / ACCEPTED (`bac626b`) — [`docs/plans/P16-implementation-plan.md`](plans/P16-implementation-plan.md) |
| P16-T001 | **COMPLETE / ACCEPTED** (`e5fa578`) — UGC module scaffolding (`ugc` schema) |
| P16-T002 | **COMPLETE / ACCEPTED** (`a5cccb2`) — Review aggregate + structured dimension ratings |
| P16-T003 | **COMPLETE / ACCEPTED** (`73f85f2`) — Review logical target attachment |
| P16-T004 | **COMPLETE / ACCEPTED** (`b35721c`) — Travelogue UGC narrative (not ContentItem) |
| P16-T005 | **COMPLETE / ACCEPTED** (`3d10913`) — UserPhoto relationship over logical MediaAssetId |
| P16-T006 | **COMPLETE / ACCEPTED** (`2d1dd59`) — Flat Comment; Like = DEFERRED |
| P16-T007 | **COMPLETE / ACCEPTED** (`30b3471`) — UGC moderation, publication, and reporting baseline |
| P16-T008 | **COMPLETE / ACCEPTED** (`62a1d7b`) — Public UGC composition / read contracts |
| P16-T009 | **COMPLETE / ACCEPTED** (`ee02dd8`) — Hardening and evidence pack |
| P16-GATE | **COMPLETE / ACCEPTED** (`538f3fc`) — Acceptance evidence (no new product capability) |
| P17 | **COMPLETE / ACCEPTED** — Plan ACCEPTED · **P17-R1–R8 RESOLVED** · T001–T009 ACCEPTED · `TC-P17-GATE` `f439924` |
| P17-GATE | **COMPLETE / ACCEPTED** (`f439924`) — Acceptance evidence (no new product capability) |
| P18 | **COMPLETE / ACCEPTED** — Plan ACCEPTED · **P18-R1–R8 RESOLVED** · T001–T009 ACCEPTED · `TC-P18-GATE` `73605aa` |
| P18 Plan | `TC-P18-PLAN` COMPLETE / ACCEPTED (`1826013`) — [`docs/plans/P18-implementation-plan.md`](plans/P18-implementation-plan.md) |
| P18-T001 | **COMPLETE / ACCEPTED** (`d29ab8e`) — TripPlanner module scaffolding (`trip_planner` schema) |
| P18-T002 | **COMPLETE / ACCEPTED** (`1163e47`) — TripIntent vs Lead aggregate boundary |
| P18-T003 | **COMPLETE / ACCEPTED** (`3ccabd2`) — anonymous-first identity/contact boundary |
| P18-T004 | **COMPLETE / ACCEPTED** (`bdace2e`) — structured travel preference model |
| P18-T005 | **COMPLETE / ACCEPTED** (`6a5b4ed`) — lead lifecycle baseline |
| P18-T006 | **COMPLETE / ACCEPTED** (`c79c07d`) — agency routing boundary (DEFERRED) |
| P18-T007 | **COMPLETE / ACCEPTED** (`b2e3173`) — lead consent/privacy boundary |
| P18-T008 | **COMPLETE / ACCEPTED** (`9e1b1e0`; final baseline `d302ad4`) — public Trip Planner experience |
| P18-FIX-TOUR-ROUTE | **COMPLETE / ACCEPTED** (`d302ad4`) — remove legacy `/tours/[productKey]` |
| P18-T009 | **COMPLETE / ACCEPTED** (`ad05e0f`) — hardening and evidence pack |
| P18-GATE | **COMPLETE / ACCEPTED** (`73605aa`) — Acceptance evidence (no new product capability) |
| P19 | **COMPLETE** — Plan ACCEPTED · **P19-R1–R8 RESOLVED** · T001–T009 ACCEPTED · GATE ACCEPTED (`d258933`) [`docs/plans/P19-GATE-acceptance-evidence.md`](plans/P19-GATE-acceptance-evidence.md) |
| P19-GATE | **COMPLETE / ACCEPTED** (`d258933`) — [`docs/plans/P19-GATE-acceptance-evidence.md`](plans/P19-GATE-acceptance-evidence.md); Payment/Confirm remain DEFERRED into P20 |
| P20 | **IN PROGRESS** — PLAN ACCEPTED · **P20-R1–R4 = RESOLVED** · **P20-R5–R8 OPEN** · T004 idempotency/reconciliation [`docs/plans/P20-implementation-plan.md`](plans/P20-implementation-plan.md) |
| P20 Plan | `TC-P20-PLAN` COMPLETE / ACCEPTED (`aca9c44`) — [`docs/plans/P20-implementation-plan.md`](plans/P20-implementation-plan.md) |
| P20-T001 | **COMPLETE / ACCEPTED** (`1ec8963`) — independent Payment module · schema `payment` · initial target = Booking · Tour Booking scope |
| P20-T002 | **COMPLETE / ACCEPTED** (`75a4f84`) — Payment aggregate + PaymentAttempt · PaymentStatus Pending/Succeeded · PaymentAttemptStatus Created/Initiated/Succeeded/Failed |
| P20-T003 | **COMPLETE / ACCEPTED** (`32e555d`) — provider-neutral initiation/verification/callback · no named provider |
| P20-T004 | **IMPLEMENTED** (awaiting architect ACCEPT) — one Booking/one Payment · attempt retry safety · reconciliation baseline |
| P20-R1 (Payment ownership / schema / target) | **RESOLVED** — independent Payment module · schema `payment` · initial Payment target = Booking · Tour Booking scope · Payment does not own Booking/Pricing · **Payment != Booking** · **PaymentStatus != BookingStatus** · **PaymentSucceeded != BookingConfirmed** |
| P20-R2 (Payment aggregate / attempts / lifecycle) | **RESOLVED** — Payment = one logical Booking collection · PaymentAttempt = one execution attempt · **Payment != PaymentAttempt** · **PaymentStatus != PaymentAttemptStatus** · **Failed PaymentAttempt != Failed Payment** · statuses Pending/Succeeded and Created/Initiated/Succeeded/Failed · at most one successful attempt · no attempt after success · verified provider evidence required for success |
| P20-R3 (Provider abstraction / initiation / verification / callback) | **RESOLVED** — Payment core is provider-neutral · NamedProvider = NONE · **BrowserReturn != PaymentSuccess** · **UnverifiedCallback != PaymentSuccess** · initiation/verification/query are neutral ports · network ambiguity is not definitive failure · Booking confirmation remains R5 · callback replay/reconciliation remains R4 · amount mismatch enforcement deferred to R5 |
| P20-R4 (Idempotency / retries / duplicate payment / reconciliation) | **RESOLVED** — one Booking -> one logical Payment · retries are PaymentAttempts · database-backed uniqueness/idempotency · ambiguous outcomes do not become Failed · unresolved Attempt blocks unsafe retry · **Reconciliation != Settlement** · **Reconciliation != Accounting** · external exactly-once is not assumed · Booking confirmation remains R5 · Refund remains R6 |
| P20-R5 through P20-R8 | **OPEN** |
| P19 Plan | `TC-P19-PLAN` COMPLETE / ACCEPTED (`9d4266b`) — [`docs/plans/P19-implementation-plan.md`](plans/P19-implementation-plan.md) |
| P19-T001 | **COMPLETE / ACCEPTED** (`e198daa`) — Booking module scaffolding (`booking` schema) |
| P19-T002 | **COMPLETE / ACCEPTED** (`7caa90a`) — Booking aggregate + Pending/Confirmed/Cancelled lifecycle (`bookings` table) |
| P19-T003 | **COMPLETE / ACCEPTED** (`8c79b02`) — CapacityHold + DepartureCapacityAccount + atomic overbooking protection |
| P19-T004 | **COMPLETE / ACCEPTED** (`b71fd15`) — BookingContactSnapshot + BookingPassenger transaction-time people facts |
| P19-T005 | **COMPLETE / ACCEPTED** (`66ec4e9`) — BookingMonetarySnapshot copied from authoritative Pricing Quote |
| P19-T006 | **COMPLETE / ACCEPTED** (`9dca5ef`) — Pending cancellation + Active-hold release; Confirm/Payment DEFERRED |
| P19-T007 | **COMPLETE / ACCEPTED** (`2e7937a`) — Direct/Agency source on one Booking aggregate; logical AgencyProfile/Offer refs |
| P19-T008 | **COMPLETE / ACCEPTED** (`5b4361e`) — public Pending Booking initiation, hashed access token, object-level reads, noindex transaction pages |
| P19-T009 | **COMPLETE / ACCEPTED** (`3a1f5a1`) — hardening/evidence pack; no new Booking capability |
| P19-GATE | **COMPLETE / ACCEPTED** (`d258933`) — [`docs/plans/P19-GATE-acceptance-evidence.md`](plans/P19-GATE-acceptance-evidence.md); P19 COMPLETE; Payment/Confirm deferred to P20 |
| P19-R1 (Booking ownership / schema / target) | **RESOLVED** — independent Booking module · schema `booking` · initial target = TourDeparture (logical) · Tour owns capacity definition · Booking owns consumption |
| P19-R2 (Lifecycle / aggregate) | **RESOLVED** — independent Booking aggregate targets one logical TourDeparture · statuses Pending/Confirmed/Cancelled · **Confirmed != PaymentSucceeded** · **Cancelled != Refunded** · Create→Pending · Pending→Cancelled · no unrestricted Confirm · no Confirmed→Cancelled · table `bookings` |
| P19-R3 (Capacity consumption / hold / concurrency) | **RESOLVED** — Tour owns definition · Booking owns consumption · CapacityHold Active/Consumed/Released/Expired · **CapacityHoldStatus != BookingStatus** · explicit ExpiresAt · advisory-lock concurrency · idempotent hold · confirmation remains R6 |
| P19-R4 (Booker / passengers / contact / PII) | **RESOLVED** — BookingContactSnapshot · BookingPassenger child · **PlannerTravelerComposition != BookingPassenger** · **BookingPassenger != Party Person Master** · no passport/upload · PassengerCount <= Active hold SeatCount |
| P19-R5 (Quote / monetary snapshot) | **RESOLVED** — Pricing owns Price/Quote · Booking stores immutable BookingMonetarySnapshot · **Price != Quote** · **Quote != BookingMonetarySnapshot** · **BookingMonetarySnapshot != PaymentAmount** · **Booking != Pricing Authority** · no FX/recalc/Payment |
| P19-R6 (Payment / confirmation / cancellation orchestration) | **RESOLVED** — Payment execution OUT of P19 · Confirm DEFERRED · Pending cancel IN · Confirmed cancel DEFERRED · **Booking != Payment** · **PaymentSucceeded != BookingConfirmed** · **BookingCancelled != PaymentRefunded** |
| P19-R7 (Agency / Lead / Visa / external boundaries) | **RESOLVED** — one Booking aggregate for Direct and Agency · `BookingSourceKind` Direct/Agency · logical AgencyProfileReference required for Agency · optional AgencyOfferReference · **Booking != AgencyMarketplace** · **BookingSourceKind != BookingStatus** · **AgencyOffer != Booking** · **AgencyOffer != Quote** · **Agency context != Pricing Authority** · **Lead != Booking** · **VisaApplication != Booking** · no commission/settlement/agency price/acceptance/capacity pool · agency PII/authorization object-level in R8 |
| P19-R8 (Public booking / authorization / privacy) | **RESOLVED** — **PublicExperience != Booking Source of Truth** · **Public Booking initiation != Booking confirmation** · **Pending != Confirmed** · **BookingId != Access Credential** · public Pending initiation · hashed Booking-scoped token · object-level actor reads · Direct public path · noindex transaction pages · no Confirm/Payment/listing/public cancel |
| P18-R8 (Public composition) | **RESOLVED** — PublicExperience composes `/plan` · TripPlanner owns TripIntent/Lead · honest follow-up CTA · no Search/Booking/Payment/CRM |
| P18-R1 (TripPlanner ownership) | **RESOLVED** — independent TripPlanner module · schema `trip_planner` · owns trip-intent/lead facts/lifecycle · does not own Destination/Tour/Place facts, Pricing/Quote, Booking, Payment, CRM, Search, AgencyMarketplace commercial allocation, Notification delivery, or Party/Identity master data · product refs = opaque logical id |
| P18-R2 (TripIntent vs Lead) | **RESOLVED** — TripIntent = mutable planning intent · Lead = submitted follow-up request · **TripIntent != Lead** · **Lead != Booking** · submission snapshot invariant |
| P18-R3 (Identity/contact) | **RESOLVED** — anonymous-first TripIntent · optional PlannerActorReference · LeadContactSnapshot at submission · Lead contact != Party/Identity master · draft access token · no Identity/Party clone |
| P18-R4 (Travel preferences) | **RESOLVED** — TravelPreferences on TripIntent · TravelPreferenceSnapshot at submission · BudgetPreference != Price/Quote · PlannerTravelerComposition != BookingPassenger · logical destination refs only |
| P18-R5 (Lead lifecycle) | **RESOLVED** — Submitted · Contacted · Closed · Cancelled · deterministic transitions · no CRM/qualification pipeline |
| P18-R6 (Agency routing) | **RESOLVED (DEFERRED)** — **P18 Agency Routing = DEFERRED** · no assignment/ranking/allocation product in P18 |
| P18-R7 (Consent/privacy) | **RESOLVED** — LeadConsentSnapshot at submission · follow-up != marketing consent · Notification provider DEFERRED · no hardcoded retention |
| P17 Plan | `TC-P17-PLAN` COMPLETE / ACCEPTED (`1b5c8ea`) — [`docs/plans/P17-implementation-plan.md`](plans/P17-implementation-plan.md) |
| P17-T001 | **COMPLETE / ACCEPTED** (`5f18f83`) — Visa module scaffolding (`visa` schema) |
| P17-T002 | **COMPLETE / ACCEPTED** (`12f19e7`) — VisaDefinition + VisaRequirementSet baseline |
| P17-T003 | **COMPLETE / ACCEPTED** (`8098ee2`) — VisaApplicability context baseline |
| P17-T004 | **COMPLETE / ACCEPTED** (`f5f52de`) — RequiredDocument + EligibilityRequirement |
| P17-T005 | **COMPLETE / ACCEPTED** (`90cd5f4`) — ProcessingTime / Validity / AllowedStay / EntryPolicy |
| P17-T006 | **COMPLETE / ACCEPTED** (`1f3d206`) — OfficialVisaFee vs Pricing |
| P17-T007 | **COMPLETE / ACCEPTED** — Public Visa vs Content vs SEO |
| P17-T008 | **COMPLETE / ACCEPTED** (`ee7a232`) — Visa application/transactional boundary |
| P17-T009 | **COMPLETE / ACCEPTED** (`120e92c`) — Visa hardening and evidence pack |
| P17-GATE | **COMPLETE / ACCEPTED** (`f439924`) — P17 acceptance evidence |
| P17-R1 (Visa ownership) | **RESOLVED** — independent Visa module · schema `visa` · owns structured visa-domain facts/lifecycle · does not own Destination/ReferenceData geography, Content CMS, MediaAsset technical truth, Pricing/Quote, Booking, Payment, SEO IndexPolicy, Search, Identity/Party · geographic refs = opaque logical id · T001: no VisaDefinition/requirement/document/fee/application product types |
| P17-R2 (Definition vs requirement) | **RESOLVED** — VisaDefinition = stable visa-type identity; VisaRequirementSet = context-dependent facts; 1 → 0..N; **VisaDefinition != VisaRequirementSet**; no applicability/docs/fees |
| P17-R3 (Applicability) | **RESOLVED** — exactly one VisaApplicability per RequirementSet · logical Destination/jurisdiction id · optional opaque nationality/residence alpha-2 · optional Adult/Minor/Other · **Applicability != Rules Engine** |
| P17-R4 (Documents / eligibility) | **RESOLVED** — RequiredDocument != EligibilityRequirement · row-based codes + RequirementLevel · eligibility is structured facts not a rules engine |
| P17-R5 (Processing / validity) | **RESOLVED** — ProcessingTime != VisaValidity != AllowedStay · EntryPolicy independent · no Duration field · effective-period readiness only |
| P17-R6 (Fee vs Pricing) | **RESOLVED** — OfficialVisaFee != CommercialPrice != Quote · platform Money in source currency · Pricing remains Price/Quote owner · no FX |
| P17-R7 (Public / Content / SEO) | **RESOLVED** — Visa owns structured facts + public read; PublicExperience composes VisaDetailPage; Content remains editorial; SEO owns IndexPolicy; public page != automatically indexed; no application workflow |
| P17-R8 (Application vs Booking) | **RESOLVED** — Visa owns visa policy/facts only; applicant-specific VisaApplication/case workflow explicitly deferred outside P17; **Visa != VisaApplication**; **VisaApplication != Booking**; **VisaApplication != Payment**; **RequiredDocument != ApplicantSubmittedDocument**; no application engine/PII/upload/appointment/external integration in P17 |
| P16-R1 (UGC ownership) | **RESOLVED** — independent UGC module · schema `ugc` · owns user-generated content lifecycle · does not own Identity/Party, Content CMS, MediaAsset technical truth, Tour/Place/Destination facts, SEO IndexPolicy, Search, Booking, Payment · actor = opaque logical id |
| P16-R2 (Review / Rating) | **RESOLVED** — Review is the aggregate. OverallRating (1..5) is part of Review. Dimension ratings are children (`DimensionCode` + `Value` 1..5, unique/normalized). Rating is not an independent aggregate. No hardcoded Hotel/Guide/Food/Service columns. |
| P16-R3 (Target attachment) | **RESOLVED** — Each Review owns exactly one logical target (`TargetType` + `TargetId`). Controlled: TourProduct · Place · Agency. No peer-schema FK. Structural `IReviewTargetValidator` only. |
| P16-R4 (Travelogue vs Content) | **RESOLVED** — Travelogue is an independent UGC aggregate. Article/Guide/LandingPage remain Content CMS. Travelogue != ContentItem. No Content schema change. Publication/moderation remains P16-R7. |
| P16-R5 (UserPhoto vs Media) | **RESOLVED** — UGC owns UserPhoto relationship (Actor + logical MediaAssetId). Media owns technical MediaAsset truth. UserPhoto != MediaAsset. No peer FK. No second media store. |
| P16-R6 (Comment / Like) | **RESOLVED** — Comment = IN (flat, Review/Travelogue). Like = DEFERRED. No threading / ranking / moderation. |
| P16-R7 (Moderation / publication / report) | **RESOLVED** — ModerationStatus != PublicationStatus. Approved != Published. Published != SEO Indexed. Public eligibility = Approved + Published. UgcReport is moderation input only. |
| P16-R8 (Public composition vs SEO/Search) | **RESOLVED** — UGC = fact owner including public-eligibility truth. PublicExperience = composition only. Search = later projection. SEO = IndexPolicy authority. Publicly Eligible != SEO Indexed. Publicly Eligible != Automatically Search Indexed. Rating summary is derived/rebuildable. |
| P15-R1 (Search ownership) | **RESOLVED** — Search = Discovery Owner · schema `search` · owns query/result contracts and future read models · does not own Tour/Content/Pricing/Agency facts or SEO IndexPolicy · Read Model/Projection later, not SoT · no LLM/business rules inside Search · T001: no projection tables / FTS / Elasticsearch / ranking / faceting |
| P15-R2 (Index / read model) | **RESOLVED** — Hybrid Read Model. Search owns `SearchDocument` + `ISearchIndex` abstraction. Domain modules remain SoT. No Elasticsearch/OpenSearch/SQL FTS in T002. SearchDocument is not a domain entity. |
| P15-R3 (Synchronization) | **RESOLVED** — Transactional Outbox + Async Projection Worker. Search failure must not fail domain transaction. Projection retryable + idempotent. No RabbitMQ/real queue in T003. |
| P15-R4 (Faceting ownership) | **RESOLVED** — Search owns Aggregation / Counting / Result composition. Domain owns attribute meaning + source facts. PE owns filter UI only (P14-R8). No facet engine / ES aggregations / domain facet tables in T004. |
| P15-R5 (Ranking model) | **RESOLVED** — Deterministic explainable signals + stable tie-break. Search owns ranking composition/ordering/metadata. Not business-policy authority. Ranking ≠ Recommendation. No ML/embeddings/personalization in T005. |
| P15-R6 (AI / Search readiness) | **RESOLVED** — Structured attributable locale-aware facts first. Semantic retrieval snapshot + provenance. No embeddings/vector/RAG/LLM/prompt infra. Search ≠ SoT. |
| P15-R7 (Query API contract) | **RESOLVED** — Engine-neutral `GET /api/search` · structured filters · continuation-ready pagination · locale explicit · not SEO IndexPolicy · empty stub execution allowed. |
| P14-R8 (Filters vs P15) | **RESOLVED** — Filter in P14 = Presentation only (UI/URL/selection). Faceting / retrieval / ranking / FTS = P15 Search. Filtered URLs ≠ SEO landings. |
| P14-R7 (Public AgencyOffer) | **RESOLVED** — AgencyOffer may be displayed; does not own commercial flow. Marketplace owns facts/publication. PE owns presentation. No agency prices / ranking / Booking. Visibility ≠ CatalogStatus / IndexPolicy. |
| P14-R6 (Content enrichment) | **RESOLVED** — Content = editorial SoT. Tour = tour-facts SoT. PE = composition only. Destination-based links. No TourProduct→ArticleId[]. Content publication ≠ SEO IndexPolicy. |
| P14-R5 (Related Tours) | **RESOLVED** — PE owns presentation only. Deterministic shared-destination retrieval behind Tour public-read. Related ≠ Recommendation. P15 may replace retrieval later. |
| P14-R4 (Shared vs specialized Detail) | **RESOLVED** — Shared Shell + kind-specific sections. Not independent Experience/Package pages. Not a giant union ViewModel. |
| P14-R3 (Listing vs SEO Landing) | **RESOLVED** — Listing = Discovery; Landing = Search Intent; Landing ≠ filtered listing; P15 owns Query/Ranking/FTS; SEO owns IndexPolicy |
| P14-R2 (Sticky actions vs Booking) | **RESOLVED** — Sticky Action ≠ Booking. Allowed View Departure / View Price / Contact-Request Information. Forbidden Book Now / Pay Now / Reserve Seat / Checkout |
| P14-R1 (Public surface ownership) | **RESOLVED** — Public Experience Layer owns Detail/Listing/Landing presentation. Not Search. Not Catalog. P14 = Presentation + SEO composition. P15 owns Query/Ranking/FTS |
| P13-R7 (Offer publishing / moderation) | **RESOLVED** — Agency Marketplace owns Offer publication status. Draft → Submitted → Approved → Published; Rejected/Archived returns. Published Offer ≠ SEO Indexed. No SEO ownership / ranking / Booking |
| P13-R6 (Agency Panel ownership) | **RESOLVED** — Agency Panel belongs to Agency Marketplace (not Tour Admin, not Identity). Foundation only: profile/offer management. No Booking/Payment/Commission/CRM |
| P13-R3 (Offer vs TourProduct) | **RESOLVED** — AgencyOffer owns the sales relationship; TourProduct remains catalog SoR; logical TourProduct Guid; no Tour FK / Price / Booking |
| P13-R4 (Agency rate override) | **RESOLVED** — Agency must NOT override Price. Commercial terms = Notes + SalesRules metadata. No Money/Discount/Commission/Currency/Quote |
| P13-R5 (Capacity/availability policy) | **RESOLVED** — Agency does NOT own capacity. TourDeparture remains capacity SoR. Offer may hold SalesAvailability metadata + optional logical TourDeparture Guid. No seats/reservation/allocation |
| P13-R1 (Marketplace ownership) | **RESOLVED** — Independent Agency Marketplace module owns Agency commercial relationship · schema `agency_marketplace` · Party remains identity SoR (`PartyKind.Agency`) · logical PartyId Guid only · no Party/Tour/Pricing merge · no Offer in T001 |
| P13-R2 (Marketplace profile vs Party identity) | **RESOLVED** — Party = identity SoR; Agency Marketplace owns AgencyProfile (0..1 per Agency PartyId); logical PartyId only; no Party schema change |
| P12-R1 (Pricing ownership) | **RESOLVED** — Independent Pricing module · schema `pricing` · logical TourDeparture Guid refs only · no Tour table ownership / no shared DbContext |
| P12-R2 (Money / currency posture) | **RESOLVED** — Reuse `TravelCore.Money`; one authoritative currency per price value; no twin SoR; no FX/Quote/Payment in T002 |
| P12-R3 (Price attachment target) | **RESOLVED** — Buyable/executable Price attaches conceptually to **TourDeparture** as the *initial* target. Pricing remains **generic**: it does **not** know TourDeparture types from Tour module. Polymorphic logical reference only: `TargetType` + `TargetId` (Guid). Example: TargetType=`TourDeparture`, TargetId=`uuid`. **No FK** · **No Booking** · **No Quote**. Product-level pricing DEFER (do not invent TourProduct pricing now). |
| P12-R4 (Quote model) | **RESOLVED** — Quote owned by Pricing · Quote is calculation snapshot · No Booking ownership · No Payment · No Customer/Passenger · No checkout flow |
| P12-R5 (Pricing occupancy/passenger baseline) | **RESOLVED** — **Pricing owns occupancy categories; Support tour market price types; No Booking passenger entity; No reservation calculation; No inventory.** Previous FX-authority wording for R5 is deferred as **implementation of FX Service** (not invented in T007; T007 only records the request boundary). |
| P12-R6 (Admin Pricing ownership) | **RESOLVED** — **Admin Pricing is operational UI/API for Pricing. Ownership stays in Pricing module (Admin API + Admin UI). Not Tour Admin ownership.** |
| P12-R7 (Pricing currency context / FX boundary) | **RESOLVED** — **P12-R7 RESOLVED:** Pricing keeps the price currency. Pricing does not convert currency. Exchange-rate ownership is not Pricing. Future FX Service owns ExchangeRate + Conversion; Pricing may only request conversion later. T007 records requested display-currency metadata / currency context only — no ExchangeRate table, no FX calculation, no Payment currency, no Settlement, no Booking. |
| P12-R8 (Public Pricing read model) | **RESOLVED** — **P12-R8 RESOLVED:** Pricing provides a public read-only query for price summary (currency, components, occupancy prices) by logical target (initial: TourDepartureId). No Booking, Payment, Checkout, Availability, Reservation, or FX conversion. |
| P10 Plan | `TC-P10-PLAN` **COMPLETE / ACCEPTED** — [`docs/plans/P10-implementation-plan.md`](plans/P10-implementation-plan.md) |
| P10-T001 | **COMPLETE / ACCEPTED** (`e5490ae`) — Experience specialization foundation |
| P10-T002 | **COMPLETE / ACCEPTED** (`757c9b8`) — Itinerary + Day + Stop (P10-R1) |
| P10-T003 | **COMPLETE / ACCEPTED** (`85553b7`) — Stop Destination/Place links (P10-R2) |
| P10-T004 | **COMPLETE / ACCEPTED** (`7589ad1`) — Meals + Accommodation (P10-R3/R5) |
| P10-T005 | **COMPLETE / ACCEPTED** (`f7ce58c`) — Difficulty · Eligibility · Equipment · LocalTransport (P10-R6) |
| P10-T006 | **COMPLETE / ACCEPTED** (`e3dbea6`) — Guide assignments (P10-R7) |
| P10-T007 | **COMPLETE / ACCEPTED** (`f262084`) — Media posture (P10-R4) |
| P10-T008 | **COMPLETE / ACCEPTED** (`0b6f191`) — Publishability (P10-R8) |
| P10-T009 | **COMPLETE / ACCEPTED** (`debd4d6`) — Hardening + evidence |
| P10-R1 (Experience specialization + Itinerary ownership) | **RESOLVED** — Experience owns Itinerary (0..1 child); Day/Stop under Itinerary |
| P10-R2 (Stop Destination/Place links) | **RESOLVED** — DestinationId 0..1 · PlaceId 0..1 (Attraction-kind) · logical · no exclusivity · no FK |
| P10-R3 (Accommodation plan) | **RESOLVED** — Experience accommodation plan 0..N · optional Place Hotel logical ref · no TourHotelOption/HotelBooking |
| P10-R5 (Meals) | **RESOLVED** — Meals on Day · Breakfast/Lunch/Dinner/Other · unique per day+type |
| P10-R6 (Difficulty/Eligibility/Equipment) | **RESOLVED** — Difficulty enum · Eligibility code/value/detail · Equipment Required/Recommended |
| P10-R7 (Guide) | **RESOLVED** — ExperienceGuideAssignment · logical GuidePartyId (Person) · Role Primary/Assistant · optional note |
| P10-R4 (Experience media) | **RESOLVED** — Cover/Gallery via TourProductMediaLink (P09-R8); Day/Stop media DEFERRED |
| P10-R8 (Experience publishability) | **RESOLVED** — Reuse TourCatalogStatus; publish gate title/cover/destination/facts; ≠ bookable |
| P09-R1 (TourProduct model shape) | **RESOLVED** — Core TourProduct + Typed Specialization; canonical `TourProductId`; Experience/Package = future typed specialization; TourDeparture = separate future aggregate |
| P09-R2 (Destination / Origin links) | **RESOLVED** — Destinations **0..N** logical join; Origin **0..1** nullable `OriginDestinationId`; no cross-schema FK; Contracts existence validation |
| P09-R3 (Agency reference) | **RESOLVED** — optional logical `AgencyId` **0..1**; PartyKind.Agency via Party.Contracts; no cross-schema FK |
| P09-R8 (Tour Media roles) | **RESOLVED** — Cover 0..1 · Gallery 0..N · logical MediaAssetId · Media.Contracts readiness · no StorageKey/FK |
| P09-R4 (Publishing lifecycle) | **RESOLVED** — Draft \| Published \| Inactive; Published = catalog-visible ≠ bookable; no hard-delete in P09 |
| P09-R5 (Slug ownership) | **RESOLVED** — TourProductTranslation owns current locale Slug; SEO owns history/redirects/IndexPolicy; path `tours/{slug}` |
| P09-R6 (IndexPolicy default) | **RESOLVED** — default missing = noindex, follow; Published ≠ Index |
| P09-R7 (Experience/Package specialty in P09) | **RESOLVED** — Specialty fields **DEFERRED** to P10/P11; P09 owns only shared TourProduct facts |
| P08 Plan | `TC-P08-PLAN` **COMPLETE / ACCEPTED** (`7012fe0`) — [`docs/plans/P08-implementation-plan.md`](plans/P08-implementation-plan.md) |
| P08-T001 | **COMPLETE / ACCEPTED** (`1b4a871`; hygiene `002cf2c`) — Content module scaffolding |
| P08-T002 | **COMPLETE / ACCEPTED** (`300b86b`; hygiene `d5e1a9f`) — ContentItem + Article/LandingPage/Guide persistence (P08-R1) |
| P08-T003 | **COMPLETE / ACCEPTED** (`ec3ad71`; hygiene `332a969`) — Localization title/body/excerpt locale rows (no slug; P08-R3 still open) |
| P08-T004 | **COMPLETE / ACCEPTED** (`c2b17a2`; hygiene `1a774b2`) — Category/Tag taxonomy baseline (Author deferred; P08-R7 open) |
| P08-T005 | **COMPLETE / ACCEPTED** (`f66458b`; hygiene `3220d58`) — Relational Content Blocks engine (P08-R2; no widgets / P08-R6) |
| P08-T006 | **COMPLETE / ACCEPTED** (`4e9c94e`; hygiene `ba391d0`) — Destination logical links 0..N (P08-R5) |
| P08-T007 | **COMPLETE / ACCEPTED** (`6a56a0d`; hygiene `4de0f93`) — Access + Admin Content baseline (no delete/archive; no slug/SEO; no Author/widgets) |
| P08-T008 | **COMPLETE / ACCEPTED** (`4924892`; hygiene `19beaca`) — Public Content pages + SEO hooks (R3/R4) |
| P08-T009 | **COMPLETE / ACCEPTED** (`2f9552f`; hygiene `a588614`) — hardening + evidence pack |
| P08-T009 | **COMPLETE / ACCEPTED** (`2f9552f`; hygiene `a588614`) — hardening + evidence pack |
| P08-GATE | **COMPLETE / ACCEPTED** (`576b7fa`) — evidence [`plans/P08-GATE-acceptance-evidence.md`](plans/P08-GATE-acceptance-evidence.md) |
| P08-R1 (Content model shape) | **RESOLVED** — Core Content Aggregate + Typed Content Variants (`ContentItemId` only; Article/LandingPage/Guide 1:1) |
| P08-R2 (Block storage) | **RESOLVED** — Relational Block Storage (`ContentBlock` first-class + ordering) |
| P08-R5 (Destination link) | **RESOLVED** — Content→Destination logical refs · cardinality 0..N · no cross-schema FK · contract existence validation |
| P08-R3 (Slug) | **RESOLVED** — `ContentItemTranslation` owns localized current slug; SEO owns route binding, redirect history, canonical/history, publication SEO state; no global slug engine in Content |
| P08-R4 (IndexPolicy) | **RESOLVED** — default IndexPolicy = **noindex, follow**; public route existence ≠ indexing; SEO owns final IndexPolicy; Content only exposes SEO hooks; publication services do not set IndexPolicy |
| P08-R6 (Widgets) | **UNRESOLVED** — no Tour/Hotel/Attraction widgets |
| P08-R7 (Author) | **UNRESOLVED** — Category/Tag only; no Author |
| P08-R8 (Delete/archive) | **UNRESOLVED** — no ContentItem delete/archive product |
| P04 Plan | `TC-P04-PLAN` COMPLETE / ACCEPTED (`9d264e6`) — [`docs/plans/P04-implementation-plan.md`](plans/P04-implementation-plan.md) |
| P04 Implementation Started | **YES** (`TC-P04-T001`) |
| Backend Physical Structure Doc | [`docs/architecture/18-backend-physical-structure.md`](architecture/18-backend-physical-structure.md) |
| API Foundation Doc | [`docs/architecture/19-api-error-and-serialization-foundation.md`](architecture/19-api-error-and-serialization-foundation.md) |
| Configuration Foundation Doc | [`docs/architecture/20-configuration-and-options-foundation.md`](architecture/20-configuration-and-options-foundation.md) |
| Health Foundation Doc | [`docs/architecture/21-health-check-foundation.md`](architecture/21-health-check-foundation.md) |
| Observability Foundation Doc | [`docs/architecture/22-observability-logging-and-correlation-foundation.md`](architecture/22-observability-logging-and-correlation-foundation.md) |
| UUID v7 Identity Foundation Doc | [`docs/architecture/23-uuid-v7-identity-foundation.md`](architecture/23-uuid-v7-identity-foundation.md) |
| NodaTime Temporal Foundation Doc | [`docs/architecture/24-nodatime-temporal-foundation.md`](architecture/24-nodatime-temporal-foundation.md) |
| Money / Currency Foundation Doc | [`docs/architecture/25-money-and-currency-foundation.md`](architecture/25-money-and-currency-foundation.md) |
| PostgreSQL Provider Foundation Doc | [`docs/architecture/26-postgresql-provider-and-connection-foundation.md`](architecture/26-postgresql-provider-and-connection-foundation.md) |
| Module-Owned DbContext Proof Doc | [`docs/architecture/27-module-owned-dbcontext-proof.md`](architecture/27-module-owned-dbcontext-proof.md) |
| Module-Owned Migrations Doc | [`docs/architecture/28-module-owned-migrations-and-runner-convention.md`](architecture/28-module-owned-migrations-and-runner-convention.md) |
| Module-Local Transactional Outbox Doc | [`docs/architecture/29-module-local-transactional-outbox.md`](architecture/29-module-local-transactional-outbox.md) |
| Automated Architecture Guardrails Doc | [`docs/architecture/30-automated-architecture-guardrails.md`](architecture/30-automated-architecture-guardrails.md) |
| Real PostgreSQL Integration Test Doc | [`docs/architecture/31-real-postgresql-integration-test-foundation.md`](architecture/31-real-postgresql-integration-test-foundation.md) |
| Real PostgreSQL Migration Proof Doc | [`docs/architecture/32-real-postgresql-migration-proof.md`](architecture/32-real-postgresql-migration-proof.md) |
| Minimal API Validation Foundation Doc | [`docs/architecture/33-minimal-api-validation-foundation.md`](architecture/33-minimal-api-validation-foundation.md) |
| Phase Transition State | **P17_T007_DELIVERED** · PLAN ACCEPTED · P17-R1–R7 RESOLVED · T007 awaiting review · R8 UNRESOLVED |
| P01 Phase Gate | **TC-P01-GATE** COMPLETE / ACCEPTED |
| P02 Phase Gate | **TC-P02-GATE** COMPLETE / ACCEPTED (`4eacff5`) |
| P03 Phase Gate | **TC-P03-GATE** COMPLETE / ACCEPTED (`6a8a5ce`) |
| P04 Phase Gate | **TC-P04-GATE** COMPLETE / ACCEPTED (`f70991f`) |
| P05 Phase Gate | **TC-P05-GATE** COMPLETE / ACCEPTED (`7f234e8`; R1 `bde6661`) |
| P06 Phase Gate | **TC-P06-GATE** COMPLETE / ACCEPTED (`da345b5`) |
| P07 Phase Gate | **TC-P07-GATE** COMPLETE / ACCEPTED (`84a0a48`) |
| Human Phase Confirmation | USER `TRAVELCORE_PHASE_CONFIRM: P08` received |
| Pipeline Product Execution | **NORMAL — AWAITING_ARCHITECT_REVIEW** (`TC-P17-T007`) |
| Human Confirmation Reason | Continuity override ON (USER 2026-08-17); stop only on architecture/path/SoT/unsafe/unlocked-decision |
| TC-P02-PLAN | COMPLETE / ACCEPTED (`47475ba`) |
| TC-P02-T001 | COMPLETE / ACCEPTED (`4e9d505`) |
| TC-P02-T002 | COMPLETE / ACCEPTED (`55ea466`) |
| TC-P02-T003 | COMPLETE / ACCEPTED (`49027f6`) |
| TC-P02-T004 | COMPLETE / ACCEPTED (`bcb06b7`) |
| TC-P02-T005 | COMPLETE / ACCEPTED (`67782e0`) |
| TC-P02-T006 | COMPLETE / ACCEPTED (`faa56c1`) |
| TC-P02-T007 | COMPLETE / ACCEPTED (`3db7237`) |
| TC-P02-T008 | COMPLETE / ACCEPTED (`ee64ea1`) |
| TC-P02-T009 | COMPLETE / ACCEPTED (`60c44f6`) |
| TC-P02-T010 | COMPLETE / ACCEPTED (`fc9a698`) |
| TC-P02-T011 | COMPLETE / ACCEPTED (`f776b64`) |
| TC-P02-T012 | COMPLETE / ACCEPTED (`44c91c9`) |
| TC-P02-T013 | COMPLETE / ACCEPTED (`ddf138f`) |
| TC-P02-T014 | COMPLETE / ACCEPTED (`4b6531b`) |
| TC-P02-T015 | COMPLETE / ACCEPTED (`8fc30ca`) |
| TC-P02-T016 | COMPLETE / ACCEPTED (`ea590d3`) |
| TC-P02-T017 | COMPLETE / ACCEPTED (`45adc28`) |
| TC-P02-GATE | COMPLETE / ACCEPTED |
| TC-P01-T006 | COMPLETE (accepted after T006R) |
| TC-P01-T006R | COMPLETE (`c6bd109`) |
| TC-P01-T007 | COMPLETE (`4420eef`; evidence via T007A) |
| TC-P01-T007A | COMPLETE |
| TC-P01-T008 | COMPLETE (`831ccd6`) |
| TC-P01-T009 | COMPLETE (`4d403c9`; accepted after T009R/T009A) |
| TC-P01-T009R | COMPLETE (`16e38b2`) |
| TC-P01-T009A | COMPLETE (READ_ONLY evidence) |
| TC-P01-T010 | COMPLETE (`c552953`; accepted after T010A) |
| TC-P01-T010A | COMPLETE (READ_ONLY equality evidence) |
| TC-P01-T011 | COMPLETE (`21b588d`; accepted after T011R) |
| TC-P01-T011R | COMPLETE (`354665c`) |
| TC-P01-T012 | COMPLETE (`1f8b465`; accepted after T012A/T012R) |
| TC-P01-T012A | COMPLETE (READ_ONLY package ownership audit) |
| TC-P01-T012R | COMPLETE (`f3798e2`) |
| TC-P01-T013 | COMPLETE (`7368284`) |
| TC-P01-T014 | COMPLETE (`bdd4a55`) |
| TC-P01-T015 | COMPLETE |
| TC-P01-T016 | COMPLETE |
| TC-P01-T017 | COMPLETE |
| TC-P01-T017A | COMPLETE |
| TC-P01-T018 | COMPLETE (`c8fb491`; accepted after T018R) |
| TC-P01-T018R | COMPLETE (`c1a1047`) |
| TC-P01-T019 | COMPLETE (`2370316`) |
| TC-P01-GATE | COMPLETE / ACCEPTED (`0853d04`) |
| TC-P02-PLAN | COMPLETE / ACCEPTED (`47475ba`) |
| TC-P02-T001 | COMPLETE / ACCEPTED (`4e9d505`) |
| TC-P02-T002 | COMPLETE / ACCEPTED (`55ea466`) |
| TC-P02-T003 | COMPLETE / ACCEPTED (`49027f6`) |
| TC-P02-T004 | COMPLETE / ACCEPTED (`bcb06b7`) |
| TC-P02-T005 | COMPLETE / ACCEPTED (`67782e0`) |
| TC-P02-T006 | COMPLETE / ACCEPTED (`faa56c1`) |
| TC-P02-T007 | COMPLETE / ACCEPTED (`3db7237`) |
| TC-P02-T008 | COMPLETE / ACCEPTED (`ee64ea1`) |
| TC-P02-T009 | COMPLETE / ACCEPTED (`60c44f6`) |
| TC-P02-T010 | COMPLETE / ACCEPTED (`fc9a698`) |
| TC-P02-T011 | COMPLETE / ACCEPTED (`f776b64`) |
| TC-P02-T012 | COMPLETE / ACCEPTED (`44c91c9`) |
| TC-P02-T013 | COMPLETE / ACCEPTED (`ddf138f`) |
| TC-P02-T014 | COMPLETE / ACCEPTED (`4b6531b`) |
| TC-P02-T015 | COMPLETE / ACCEPTED (`8fc30ca`) |
| TC-P02-T016 | COMPLETE / ACCEPTED (`ea590d3`) |
| TC-P02-T017 | COMPLETE / ACCEPTED (`45adc28`) |
| TC-P02-GATE | COMPLETE / ACCEPTED (`4eacff5`) |
| TC-P03-PLAN | COMPLETE / ACCEPTED (`a779726`) |
| TC-P03-T001 | COMPLETE / ACCEPTED (`afdf73c`) |
| TC-P03-T002 | COMPLETE / ACCEPTED (`393b7df`; evidence `5d5315e`/`036735d`) |
| TC-P03-T003 | COMPLETE / ACCEPTED (`5730074`) |
| TC-P03-T004 | COMPLETE / ACCEPTED (`91e530a`) |
| TC-P03-T005 | COMPLETE / ACCEPTED (`00dd11d`) |
| TC-P03-T006 | COMPLETE / ACCEPTED (`86f7107`) |
| TC-P03-T007 | COMPLETE / ACCEPTED (`089c396`) |
| TC-P03-T008 | COMPLETE / ACCEPTED (`289180c`; evidence `7c22c80`) |
| TC-P03-T009 | COMPLETE / ACCEPTED (`2843127`) |
| TC-P03-T010 | COMPLETE / ACCEPTED (`446d557`) |
| TC-P03-T011 | COMPLETE / ACCEPTED (`45aedb2`) |
| TC-P03-T012 | COMPLETE / ACCEPTED (`349bd8a`) |
| TC-P03-GATE | COMPLETE / ACCEPTED (`6a8a5ce`) |
| TC-P04-PLAN | COMPLETE / ACCEPTED (`9d264e6`) |
| TC-P04-T001 | COMPLETE / ACCEPTED (`5de2ae1`) |
| TC-P04-T002 | COMPLETE / ACCEPTED (`3363cf1`) |
| TC-P04-T003 | COMPLETE / ACCEPTED (`9176dbe`) |
| TC-P04-T004 | COMPLETE / ACCEPTED (`9c30e77`; docs `da9730e`) |
| TC-P04-T005 | COMPLETE / ACCEPTED (`3dabe6f`) |
| TC-P04-T006 | COMPLETE / ACCEPTED (`edc201f`; docs `124d57b`) |
| TC-P04-T007 | COMPLETE / ACCEPTED (`ba04618`; docs `76528e6`) |
| TC-P04-T008 | COMPLETE / ACCEPTED (`81fd6ce`) |
| TC-P04-T009 | COMPLETE / ACCEPTED (`660d2c4`) |
| TC-P04-T010 | COMPLETE / ACCEPTED (`dc9d00d`) |
| TC-P04-T011 | COMPLETE / ACCEPTED (`13b36b0`) |
| TC-P04-GATE | COMPLETE / ACCEPTED (`f70991f`) |
| TC-P05-PLAN | COMPLETE / ACCEPTED (`032dabc`) |
| TC-P05-PLAN-R1 | COMPLETE / ACCEPTED (`31c3283`; hygiene `f703d6a`) |
| TC-P05-T001 | COMPLETE / ACCEPTED (`a65fcc8`) |
| TC-P05-T002 | COMPLETE / ACCEPTED (`796e013`; hygiene `50ec735`) |
| TC-P05-T003 | COMPLETE / ACCEPTED (`8fb6ede`; hygiene `7226451`) |
| TC-P05-T003-R1 | COMPLETE / ACCEPTED (`fb00313`; hygiene `e24d09a`) |
| TC-P05-T004 | COMPLETE / ACCEPTED (`1573baf`; hygiene `f7d9e51`/`96a43a4`) |
| TC-P05-T005 | COMPLETE / ACCEPTED (`95c79da`; hygiene `77b0b82`) |
| TC-P05-T006 | COMPLETE / ACCEPTED (`0cba002`; hygiene `40253b4`/`fbc6fb1`) |
| TC-P05-T007 | COMPLETE / ACCEPTED (`d611263`; hygiene `e1eae24`/`e8544dc`) |
| TC-P05-T008 | COMPLETE / ACCEPTED (`1a98601`; hygiene `a4bf89a`) |
| TC-P05-T009 | COMPLETE / ACCEPTED (`09d6f5d`; hygiene `6dfc38c`/`a0fd6b7`) |
| TC-P05-T010 | COMPLETE / ACCEPTED (`78caf4b`; hygiene `28cfb41`/`84c7ab2`) |
| TC-P05-T011 | COMPLETE / ACCEPTED (`8a9c4b7`; hygiene `61dd8c1`/`9258479`/`85ac421`) |
| TC-P05-T012 | COMPLETE / ACCEPTED (`0c8ab0a`; hygiene `3351755`/`be407fc`/`6a02d9d`) |
| TC-P05-GATE | COMPLETE / ACCEPTED (`7f234e8`; hygiene `d6bcbfb`) |
| TC-P05-GATE-R1 | COMPLETE / ACCEPTED (`bde6661`; hygiene `37637bf`) |
| P05-R1 | **RESOLVED** (Destination current slug SoR; SEO path history/reservation/redirect mechanics) |
| P05-R2 | **RESOLVED** (default missing policy = noindex, follow; explicit Index requires eligibility) |
| TC-P06-PLAN | **COMPLETE / ACCEPTED** (`87069e4`; hygiene `f323857`/`1b2877b`) |
| TC-P06-T001 | **COMPLETE / ACCEPTED** (`e5bfd39`; hygiene `8e8fb63`) |
| TC-P06-T002 | **COMPLETE / ACCEPTED** (`020ce99`; hygiene `6100891`) |
| TC-P06-T003 | **COMPLETE / ACCEPTED** (`cf95e5c`; hygiene `1d4e497`) |
| TC-P06-T004 | **COMPLETE / ACCEPTED** (`7f83885`) |
| TC-P06-T005 | **COMPLETE / ACCEPTED** (`91444ad`) |
| TC-P06-T006 | **COMPLETE / ACCEPTED** (`166e9db`; R1 `b6f0cfb`) |
| TC-P06-T007 | **COMPLETE / ACCEPTED** (`85c8e7a`) |
| TC-P06-T008 | **COMPLETE / ACCEPTED** (`f50cce3`; hygiene `1736a66`) |
| TC-P06-T009 | **COMPLETE / ACCEPTED** (`3a25e7d`; hygiene `d3ce295`/`71b2886`) |
| TC-P06-T010 | **COMPLETE / ACCEPTED** (`05ef0ac`) |
| TC-P06-T011 | **COMPLETE / ACCEPTED** (`8b0de5a`) |
| TC-P06-T012 | **COMPLETE / ACCEPTED** (`8981312`; hygiene `acfed76`) — evidence pack `docs/plans/P06-T012-hardening-and-evidence-pack.md` |
| TC-P06-GATE | **COMPLETE / ACCEPTED** (`da345b5`; hygiene `0d2edad`) |
| P06-R1 | **RESOLVED — DEFER** (no WebP/AVIF conversion pipeline in P06; same-format variants only) |
| P06-R2 | **RESOLVED** (Media-owned storage abstraction; local filesystem + in-memory test adapters; vendor deferred) |
| P06-R3 | **RESOLVED** (SYNCHRONOUS variant generation; sizing 1600/960/320 fit-within; GIF fail-closed) |
| P06-R4 | **RESOLVED — APP PROXY** (TravelCore delivery endpoints; anonymous Ready-only; StorageKey never public) |
| P06-R5 | **RESOLVED — CONTRACT-ONLY** (`MediaAssetReference` + ArchitectureTests; no Destination schema MediaAssetId) |
| P06-R6 | **RESOLVED** (SVG DENY — Option A) |
| P06-R7 | **DEFERRED** (malware/AV scanning; recorded security requirement) |
| P06-R8 | **UNRESOLVED** (no Admin delete UI/actions; OK for gate — deletion not in P06 product scope) |
| P06-R9 | **DEFERRED** (consumer alt override; Media owns default alt/caption only) |
| TC-P07-PLAN | **COMPLETE / ACCEPTED** (`5dbc152`; hygiene `768a2c5`) |
| TC-P07-T001 | **COMPLETE / ACCEPTED** (`108ac34`; hygiene `a245358`) |
| TC-P07-T002 | **COMPLETE / ACCEPTED** (`83529cf`; hygiene `d127ee7`) |
| TC-P07-T002-R1 | **COMPLETE / ACCEPTED** (`0b86f05`; hygiene `77f5386`) — PlaceId identity + T002 scope reconciliation; artifact [`plans/P07-T002-R1-place-identity-and-scope-reconciliation.md`](plans/P07-T002-R1-place-identity-and-scope-reconciliation.md) |
| TC-P07-T003 | **COMPLETE / ACCEPTED** (`3ec0f4c`; hygiene `5850e52`) — Localization + Destination link + geo/address |
| TC-P07-T004 | **COMPLETE / ACCEPTED** (`6258003`; hygiene `b62b746`) — Facilities · classification · catalog status |
| TC-P07-T005 | **COMPLETE / ACCEPTED** (`6246a09`; hygiene `0144f8d`) — Place↔Media Cover/Gallery |
| TC-P07-T006 | **COMPLETE / ACCEPTED** (`74e8540`; hygiene `61ff89d`) — Access + Admin Place baseline |
| TC-P07-T006-R1 | **COMPLETE / ACCEPTED** (`e4b5201`; hygiene `48aaaea`) — Ready Media visual picker; evidence [`plans/P07-T006-R1-admin-place-media-picker-reconciliation.md`](plans/P07-T006-R1-admin-place-media-picker-reconciliation.md) |
| TC-P07-T007 | **COMPLETE / ACCEPTED** (`1c76f6b`; hygiene `b47f6de`) — Public Place detail + SEO hooks |
| TC-P07-T008 | **COMPLETE / ACCEPTED** (`f7843cc`; hygiene `2d10fbd`/`fcefadd`) — evidence pack [`plans/P07-T008-hardening-and-evidence-pack.md`](plans/P07-T008-hardening-and-evidence-pack.md) |
| TC-P07-GATE | **COMPLETE / ACCEPTED** (`84a0a48`; hygiene `8136455`/`003e9e4`) — [`plans/P07-GATE-acceptance-evidence.md`](plans/P07-GATE-acceptance-evidence.md) |
| P07-R1 | **RESOLVED** — CORE PLACE + TYPED SPECIALIZATION |
| P07-R2 | **RESOLVED** — OPTIONAL SINGLE LOGICAL REFERENCE Place→Destination (0..1; nullable DestinationId; no cross-schema FK; Contracts existence validation; no DestinationKind restriction in T003) |
| P07-R3 | **UNRESOLVED** (Place delete/archive) — OK for COMPLETE phase; CatalogStatus is catalog ops only; no delete/archive product invented |
| P07-R4 | **RESOLVED** — PLACE owns current locale-specific `PlaceTranslation.Slug`; SEO owns route binding/reservations/history/redirects/canonical/IndexPolicy |
| P07-R5 | **RESOLVED** — default Place SEO posture **noindex, follow**; Active/public/publish ≠ Index; no Destination IndexPolicy inheritance |
| Required Human Token | ceremonial phase/gate tokens not required under continuity override; P09 Gate ACCEPTED → P10 PLAN auto-started |

### P00 Exit Summary

- P00 Architecture Foundation formally complete
- TC-P00-GATE PASS
- ADR 0001–0014 Accepted
- Canonical pipeline entry ACTIVE: `docs/ai/TRAVELCORE-PIPELINE-PROTOCOL.md`
- Pipeline Protocol = READY; Current Runtime Mode = PIPELINE (USER opt-in); Automatic Pipeline = ON
- P01 product phase COMPLETE through `TC-P01-T019` (`2370316`); `TC-P01-GATE` COMPLETE / ACCEPTED (`0853d04`)
- P02 COMPLETE; `TC-P02-PLAN` through `TC-P02-T017` ACCEPTED; `TC-P02-GATE` COMPLETE / ACCEPTED (`4eacff5`); evidence: `docs/plans/P02-T017-walking-skeleton-validation-evidence.md`
- P04 COMPLETE (`TC-P04-GATE` ACCEPTED `f70991f`); **P05 COMPLETE** (`TC-P05-GATE` ACCEPTED `7f234e8` · `TC-P05-GATE-R1` ACCEPTED `bde6661`); **P06 COMPLETE** (`TC-P06-GATE` ACCEPTED `da345b5`); **P07 COMPLETE** (`TC-P07-GATE` ACCEPTED `84a0a48`); Runtime Mode = PIPELINE; **P08 COMPLETE** (`TC-P08-GATE` ACCEPTED `576b7fa`); **P08-R1/R2/R3/R4/R5 RESOLVED** · **P08-R6–R8 UNRESOLVED**; **P09 COMPLETE** (`TC-P09-GATE` ACCEPTED `67fc580` · T010 `0334bae` · R1–R8 RESOLVED); **P10 IN_PROGRESS** (T001–T005 ACCEPTED · T006 Guide · **P10-R1/R2/R3/R5/R6/R7 RESOLVED** · R4/R8 UNRESOLVED)

Recovery Drill note: recovery prompt successfully reconstructed current phase, accepted/pending task state, ADR statuses, and clean Git state without modifying the repository.

T007R note: integrity review PASS — the T007 update to `docs/ui/04-page-archetype-contract.md` was a compatible documentation traceability extension only (SAFE EXTENSION).

T008R note: repository integrity PASS — canonical origin already `mrnikiemami-code/TravelCore`; prior wrong-owner spelling was REPORT TYPO only.

---

## Completed Tasks

| Task | خلاصه | نتیجه | Commit مرتبط |
|------|--------|--------|----------------|
| TC-P00-T000A | Backend bootstrap (.NET 10 Minimal API) | PASS | بخشی از `cf97f35` |
| TC-P00-T000B | Frontend/repository bootstrap | Local PASS؛ remote بعداً حل شد | `cf97f35` |
| TC-P00-T000C | GitHub auth / private repo / push sync | PASS | روی `origin/main` |
| TC-P00-T001 | Architecture Brain & Constitution | PASS | `834e0c5` |
| TC-P00-T001A | Project continuity / PROJECT-STATE | PASS | `110c748` |
| TC-P00-T001B | Master execution roadmap | PASS | `783c4e4` |
| TC-P00-T001C | Emergency ChatGPT recovery prompt | PASS | `31d1bfe` |
| TC-P00-T002 | Domain map / module boundaries | ACCEPTED / COMPLETE | `08343e7` |
| TC-P00-T002A | Accept domain boundaries / advance state | ACCEPTED | `6f50897` |
| TC-P00-T003 | Data Architecture | ACCEPTED / COMPLETE | `3904bb9` |
| TC-P00-T003R | Normalize canonical GitHub identity | PASS / ACCEPTED | `840c3e5` |
| TC-P00-T003A | Accept data architecture | ACCEPTED | `f74f0a4` |
| TC-P00-T004 | UI Constitution | ACCEPTED / COMPLETE | `48e0472` |
| TC-P00-T004A | Accept UI constitution | ACCEPTED | `b477755` |
| TC-P00-T005 | Internationalization Architecture | ACCEPTED / COMPLETE | `66e6f32` |
| TC-P00-T005A | Accept i18n architecture | ACCEPTED | `b73bc10` |
| TC-P00-T006 | SEO Constitution | ACCEPTED / COMPLETE | `5dbbb45` |
| TC-P00-T006A | Accept SEO constitution | ACCEPTED | `5d81f5a` |
| TC-P00-T007 | Reference Page Archetypes | ACCEPTED / COMPLETE | `fbf1617` |
| TC-P00-T007R | Accepted-doc integrity review | PASS | review of `fbf1617` |
| TC-P00-T007A | Accept page archetypes | ACCEPTED | `b671f58` |
| TC-P00-T008 | Engineering Quality Constitution | ACCEPTED / COMPLETE | `1bd4e95` |
| TC-P00-T008R | Canonical repository integrity review | PASS | review of `1bd4e95` |
| TC-P00-T008A | Accept engineering quality constitution | ACCEPTED | `0074437` |
| TC-P00-GATE | Final Architecture Foundation Gate | PASS | audit (read-only) |
| TC-P00-CLOSE | Normalize recovery state and close P00 | PASS / COMPLETE | `6c65cb9` |
| TC-GOV-T001 | Controlled ChatGPT↔Cursor handoff + human phase gates | COMPLETE / ACCEPTED | `f44f11e` |
| TC-GOV-T001A | Accept ADR 0013 + activate handoff protocol | COMPLETE / ACCEPTED | `476ae67` |
| TC-GOV-T002 | Consolidate pipeline protocol + HUMAN/PIPELINE modes | COMPLETE / ACCEPTED | `1cfe48a` |
| TC-GOV-T002A | Accept ADR 0014 + activate Pipeline Protocol in AGENTS/Recovery | COMPLETE / ACCEPTED | `1f9ad48` |
| TC-P01-T019 | Security Hygiene Baseline | COMPLETE / ACCEPTED | `2370316` |
| TC-P01-GATE | P01 Acceptance Gate | COMPLETE / ACCEPTED | `0853d04` |
| TC-P02-PLAN | P02 Frontend Foundation + Walking Skeleton Plan | COMPLETE / ACCEPTED | `47475ba` |
| TC-P02-T001 | Frontend physical structure | COMPLETE / ACCEPTED | `4e9d505` |
| TC-P02-T002 | Locale-aware App Router root (lang / dir) | COMPLETE / ACCEPTED | `55ea466` |
| TC-P02-T003 | Design tokens + Tailwind semantic mapping | COMPLETE / ACCEPTED | `49027f6` |
| TC-P02-T004 | Direction-neutral primitives + bidi-safe text | COMPLETE / ACCEPTED | `bcb06b7` |
| TC-P02-T005 | Money / MixedCurrencyPrice presentation | COMPLETE / ACCEPTED | `67782e0` |
| TC-P02-T006 | Accessibility baseline | COMPLETE / ACCEPTED | `faa56c1` |
| TC-P02-T007 | App Router loading / error / not-found | COMPLETE / ACCEPTED | `3db7237` |
| TC-P02-T008 | Public + Admin shell layout foundation | COMPLETE / ACCEPTED | `ee64ea1` |
| TC-P02-T009 | Frontend API / read-model boundary | COMPLETE / ACCEPTED | `60c44f6` |
| TC-P02-T010 | Cross-domain workflow & navigation model | COMPLETE / ACCEPTED | `fc9a698` |
| TC-P02-T011 | Media / Image foundation | COMPLETE / ACCEPTED | `f776b64` |
| TC-P02-T012 | Foreign Tour Detail PVM + fixtures | COMPLETE / ACCEPTED | `44c91c9` |
| TC-P02-T013 | Foreign Tour Detail page + view | COMPLETE / ACCEPTED | `ddf138f` |
| TC-P02-T014 | Sticky booking CTA island | COMPLETE / ACCEPTED | `4b6531b` |
| TC-P02-T015 | SEO metadata baseline | COMPLETE / ACCEPTED | `8fc30ca` |
| TC-P02-T016 | Automated quality gates | COMPLETE / ACCEPTED | `ea590d3` |
| TC-P02-T017 | Walking skeleton validation evidence | COMPLETE / ACCEPTED | `45adc28` |
| TC-P02-GATE | P02 Acceptance Gate | COMPLETE / ACCEPTED | `4eacff5` |
| TC-P03-PLAN | P03 Identity + Access + Party Plan | COMPLETE / ACCEPTED | `a779726` |
| TC-P03-T001 | Identity / Access / Party module scaffolding | COMPLETE / ACCEPTED | `afdf73c` |
| TC-P03-T002 | Party domain + persistence foundation | COMPLETE / ACCEPTED | `393b7df` (+ evidence `5d5315e`/`036735d`) |
| TC-P03-T003 | Identity domain + credential persistence baseline | COMPLETE / ACCEPTED | `5730074` |
| TC-P03-T004 | Identity ↔ Party association contracts | COMPLETE / ACCEPTED | `91e530a` |
| TC-P03-T005 | Access taxonomy (Permission/Role) + persistence | COMPLETE / ACCEPTED | `00dd11d` |
| TC-P03-T006 | Authorization evaluation service | COMPLETE / ACCEPTED | `86f7107` |
| TC-P03-T007 | Subject role assignment foundation | COMPLETE / ACCEPTED | `089c396` |
| TC-P03-T008 | Host authentication ticket (HttpOnly cookie) | COMPLETE / ACCEPTED | `289180c` (+ evidence `7c22c80`) |
| TC-P03-T009 | Admin authz baseline (Access-backed) | COMPLETE / ACCEPTED | `2843127` |
| TC-P03-T010 | Guided Admin Identity↔Party workflow UI | COMPLETE / ACCEPTED | `446d557` |
| TC-P03-T011 | Agency presentation access baseline | COMPLETE / ACCEPTED | `45aedb2` |
| TC-P03-T012 | P03 hardening evidence pack | COMPLETE / ACCEPTED | `349bd8a` |
| TC-P03-GATE | P03 Acceptance Gate | COMPLETE / ACCEPTED | `6a8a5ce` |
| TC-P04-PLAN | P04 Reference Data + Destination Plan | COMPLETE / ACCEPTED | `9d264e6` |
| TC-P04-T001 | ReferenceData / Destination module scaffolding | COMPLETE / ACCEPTED | `5de2ae1` |
| TC-P04-T002 | ReferenceData catalogs + persistence baseline | COMPLETE / ACCEPTED | `3363cf1` |
| TC-P04-T003 | Destination hierarchy domain + persistence | COMPLETE / ACCEPTED | `9176dbe` |
| TC-P04-T004 | Destination translations + geographic identity | COMPLETE / ACCEPTED | `9c30e77` (+ `da9730e`) |
| TC-P04-T005 | Hierarchy query + path/ancestors contracts | COMPLETE / ACCEPTED | `3dabe6f` |
| TC-P04-T006 | Localized Destination slug hooks | COMPLETE / ACCEPTED | `edc201f` (+ `124d57b`) |
| TC-P04-T007 | Access permissions + Admin Destination authz | COMPLETE / ACCEPTED | `ba04618` (+ `76528e6`) |
| TC-P04-T008 | Guided Admin Destination hierarchy workflow | COMPLETE / ACCEPTED | `81fd6ce` |
| TC-P04-T009 | Public Destination read model / detail baseline | COMPLETE / ACCEPTED | `660d2c4` |
| TC-P04-T010 | ReferenceData Admin/read UX baseline (minimal) | COMPLETE / ACCEPTED | `dc9d00d` |
| TC-P04-T011 | Phase hardening tests & evidence pack | COMPLETE / ACCEPTED | `13b36b0` |
| TC-P04-GATE | P04 Acceptance Gate | COMPLETE / ACCEPTED | `f70991f` |
| TC-P05-PLAN | P05 SEO Engine Implementation Plan | COMPLETE / ACCEPTED | `032dabc` |
| TC-P05-PLAN-R1 | P05 Plan Baseline Reconciliation & Architect Review Evidence | COMPLETE / ACCEPTED | `31c3283` |
| TC-P05-T001 | SEO module scaffolding | COMPLETE / ACCEPTED | `a65fcc8` |
| TC-P05-T002 | SeoRoute + localized path binding baseline | COMPLETE / ACCEPTED | `796e013` |
| TC-P05-T003 | Slug history / reservation coordination | COMPLETE / ACCEPTED | `8fb6ede` |
| TC-P05-T003-R1 | Reconcile P05 R1 Decision State | COMPLETE / ACCEPTED | `fb00313` |
| TC-P05-T004 | Canonical + Redirect engine baseline | COMPLETE / ACCEPTED | `1573baf` |
| TC-P05-T005 | IndexPolicy + robots posture | COMPLETE / ACCEPTED | `95c79da` (+ `77b0b82`) |
| TC-P05-T006 | hreflang / alternate locale bindings | COMPLETE / ACCEPTED | `0cba002` (+ `40253b4`/`fbc6fb1`) |
| TC-P05-T007 | Metadata composition framework | COMPLETE / ACCEPTED | `d611263` (+ `e1eae24`/`e8544dc`) |
| TC-P05-T008 | Breadcrumb + structured data framework | COMPLETE / ACCEPTED | `1a98601` (+ `a4bf89a`) |
| TC-P05-T009 | Sitemap + robots.txt framework | COMPLETE / ACCEPTED | `09d6f5d` (+ `6dfc38c`/`a0fd6b7`) |
| TC-P05-T010 | Destination public integration + publication rules | COMPLETE / ACCEPTED | `78caf4b` (+ `28cfb41`/`84c7ab2`) |
| TC-P05-T011 | Admin SEO operational baseline | COMPLETE / ACCEPTED | `8a9c4b7` (+ `61dd8c1`/`9258479`/`85ac421`) |
| TC-P05-T012 | Phase hardening tests & evidence pack | COMPLETE / ACCEPTED | `0c8ab0a` (+ `3351755`/`be407fc`/`6a02d9d`) |
| TC-P05-GATE | P05 Acceptance Gate | COMPLETE / ACCEPTED | `7f234e8` (+ `d6bcbfb`) |
| TC-P05-GATE-R1 | Reconcile P05 Gate Baseline Drift | COMPLETE / ACCEPTED | `bde6661` (+ `37637bf`) |
| TC-P06-GATE | P06 Acceptance Gate | COMPLETE / ACCEPTED | `da345b5` |
| TC-P07-PLAN | P07 Place Catalog Implementation Plan | COMPLETE / ACCEPTED | `5dbc152` |
| TC-P07-T001 | Place module scaffolding | COMPLETE / ACCEPTED | `108ac34` |
| TC-P07-T002 | Place catalog domain + persistence baseline | AWAITING_ARCHITECT_REVIEW | `83529cf` |
| TC-P07-T002-R1 | PlaceId identity + T002 scope reconciliation (docs-only) | AWAITING_ARCHITECT_REVIEW | `0b86f05` |
| TC-P07-T003 | Localization + Destination link + geo/address | AWAITING_ARCHITECT_REVIEW | `3ec0f4c` |
| TC-P07-T004 | Facilities · classification · catalog status | AWAITING_ARCHITECT_REVIEW | `6258003` |
| TC-P07-T005 | Place↔Media relations (gallery meaning) | AWAITING_ARCHITECT_REVIEW | `6246a09` |
| TC-P07-T006 | Access permissions + Admin Place baseline | AWAITING_ARCHITECT_REVIEW | `74e8540` |
| TC-P07-T006-R1 | Admin Place media picker UX remediation | AWAITING_ARCHITECT_REVIEW | `e4b5201` |
| TC-P07-T007 | Public Place detail + SEO integration | AWAITING_ARCHITECT_REVIEW | `1c76f6b` |
| TC-P07-T008 | Phase hardening tests & evidence pack | AWAITING_ARCHITECT_REVIEW | `f7843cc` |
| TC-P08-T008 | Public Content pages + SEO integration hooks | COMPLETE / ACCEPTED | `4924892` |
| TC-P08-T009 | Phase hardening tests & evidence pack | COMPLETE / ACCEPTED | `2f9552f` |
| TC-P08-GATE | P08 Acceptance Gate | COMPLETE / ACCEPTED | `576b7fa` |
| TC-P09-GATE | P09 Acceptance Gate | COMPLETE / ACCEPTED | `67fc580` |
| TC-P10-PLAN | P10 Experience Tour Implementation Plan | COMPLETE / ACCEPTED | `d6d27e6` |
| TC-P10-T001 | Experience specialization scaffolding | COMPLETE / ACCEPTED | `e5490ae` |
| TC-P10-T002 | Experience Itinerary + Day + Stop baseline | COMPLETE / ACCEPTED | `757c9b8` |
| TC-P10-T003 | Experience Stop semantic references | COMPLETE / ACCEPTED | `85553b7` |
| TC-P10-T004 | Experience meals + accommodation plan | COMPLETE / ACCEPTED | `7589ad1` |
| TC-P10-T005 | Difficulty · Eligibility · Equipment · LocalTransport | COMPLETE / ACCEPTED | `f7ce58c` |
| TC-P10-T006 | Experience Guide relation baseline | COMPLETE / ACCEPTED | `e3dbea6` |
| TC-P10-T007 | Experience Media relations baseline | AWAITING_ARCHITECT_REVIEW | `f262084` |
| TC-P12-PLAN | P12 Pricing Implementation Plan | COMPLETE / ACCEPTED | `d26078d` |
| TC-P12-T001 | Pricing module scaffolding | COMPLETE / ACCEPTED | `7c2e488` |
| TC-P12-T002 | Money / Currency baseline | COMPLETE / ACCEPTED | `6c1b4ce` |
| TC-P12-T003 | Price + PriceComponent model | COMPLETE / ACCEPTED | `58de552` |
| TC-P12-T004 | Pricing Quote baseline | COMPLETE / ACCEPTED | `81a3f26` |
| TC-P12-T005 | Pricing occupancy/passenger category baseline | COMPLETE / ACCEPTED | `c90931d` |
| TC-P12-T006 | Admin Pricing baseline | COMPLETE / ACCEPTED | `e1d01c4` |
| TC-P12-T007 | Pricing currency context and FX boundary | COMPLETE / ACCEPTED | `87b5dac` |
| TC-P12-T008 | Public Pricing read model baseline | COMPLETE / ACCEPTED | `520a46d` |
| TC-P12-T009 | Pricing hardening and evidence pack | COMPLETE / ACCEPTED | `a522dd5` |
| TC-P12-GATE | P12 Pricing Acceptance Gate | COMPLETE / ACCEPTED | `b372367` |
| TC-P13-PLAN | P13 Agency Marketplace Implementation Plan | COMPLETE / ACCEPTED · R1 locked for T001 | see `docs/plans/P13-implementation-plan.md` |
| TC-P13-T001 | Agency Marketplace scaffolding | COMPLETE / ACCEPTED | `9f61763` |
| TC-P13-T002 | Agency Marketplace profile baseline | COMPLETE / ACCEPTED | `809eb49` |
| TC-P13-T003 | Agency Offer relationship baseline | COMPLETE / ACCEPTED | `a665272` |
| TC-P13-T004 | Agency commercial terms boundary baseline | COMPLETE / ACCEPTED | `87931d9` |
| TC-P13-T005 | Agency offer capacity boundary baseline | COMPLETE / ACCEPTED | `7234cc1` |
| TC-P13-T006 | Agency Marketplace panel baseline | COMPLETE / ACCEPTED | `8098a24` |
| TC-P13-T007 | Agency Offer publishing and moderation baseline | COMPLETE / ACCEPTED | `98ea1d1` |
| TC-P13-T008 | (vacant — publishing delivered as T007) | VACANT / SKIPPED | — |
| TC-P13-T009 | Agency Marketplace hardening and evidence pack | COMPLETE / ACCEPTED | `d813dbd` |
| TC-P13-GATE | P13 Agency Marketplace Acceptance Gate | COMPLETE / ACCEPTED | `c0bcd78` |
| TC-P14-PLAN | P14 Public Tour Experience Implementation Plan | COMPLETE / ACCEPTED | `cc3ed8b` |
| TC-P14-T001 | Public Tour Experience surface inventory | COMPLETE / ACCEPTED | `a7bd549` |
| TC-P14-T002 | Public Tour Experience detail composition baseline | COMPLETE / ACCEPTED | `99818dd` |
| TC-P14-T003 | Public Tour Listing and SEO Landing boundary baseline | COMPLETE / ACCEPTED | `f0e3df3` |
| TC-P14-SYNC001 | Synchronize accepted main with origin/main | COMPLETE / ACCEPTED | `f0e3df3` |
| TC-P14-T004 | Shared and specialized Tour Detail composition | COMPLETE / ACCEPTED | `0b4fcbe` |
| TC-P14-T005 | Related Tours composition baseline | COMPLETE / ACCEPTED | `c34e5b0` |
| TC-P14-T006 | Public Tour content enrichment composition | COMPLETE / ACCEPTED | `5258e20` |
| TC-P14-T007 | Public AgencyOffer presentation baseline | COMPLETE / ACCEPTED | `903cd29` |
| TC-P14-T008 | Public Experience filter presentation boundary | COMPLETE / ACCEPTED | `a0209bd` |
| TC-P14-T009 | Public Experience hardening and evidence pack | COMPLETE / ACCEPTED | `6c0e218` |
| TC-P14-GATE | P14 Public Experience Acceptance Gate | COMPLETE / ACCEPTED | `608216d` |
| TC-P15-PLAN | Search & Discovery Architecture Plan | COMPLETE / ACCEPTED · R1 locked for T001 | `fba7a51` |
| TC-P15-T001 | Search module scaffolding | COMPLETE / ACCEPTED | `bea92a1` |
| TC-P15-T002 | Search index model abstraction | COMPLETE / ACCEPTED | `2b3c9d2` |
| TC-P15-T003 | Search projection synchronization boundary | COMPLETE / ACCEPTED | `2631c4e` |
| TC-P15-T004 | Search faceting ownership boundary | COMPLETE / ACCEPTED | `413d6fe` |
| TC-P15-T005 | Search ranking model boundary | COMPLETE / ACCEPTED | `7b22225` |
| TC-P15-T006 | Search AI-readiness / semantic retrieval | COMPLETE / ACCEPTED | `edc176f` |
| TC-P15-T007 | Public Search query API contract | COMPLETE / ACCEPTED | `183d09d` |
| TC-P15-T008 | Vacant (no independent product scope) | VACANT | — |
| TC-P15-T009 | Search hardening and evidence pack | COMPLETE / ACCEPTED | `b741bc5` |
| TC-P15-GATE | P15 Search Acceptance Gate | COMPLETE / ACCEPTED | `4e2098d` |
| TC-P16-PLAN | UGC Architecture Plan | COMPLETE / ACCEPTED | `bac626b` |
| TC-P16-T001 | UGC module scaffolding | COMPLETE / ACCEPTED | `e5fa578` |
| TC-P16-T002 | Review / rating model baseline | COMPLETE / ACCEPTED | `a5cccb2` |
| TC-P16-T003 | Review target attachment baseline | COMPLETE / ACCEPTED | `73f85f2` |
| TC-P16-T004 | Travelogue UGC baseline | COMPLETE / ACCEPTED | `b35721c` |
| TC-P16-T005 | UserPhoto vs Media boundary | COMPLETE / ACCEPTED | `3d10913` |
| TC-P16-T006 | Comment baseline; Like deferred | COMPLETE / ACCEPTED | `2d1dd59` |
| TC-P16-T007 | Moderation / publication / report | COMPLETE / ACCEPTED | `30b3471` |
| TC-P16-T008 | Public composition / read contracts | COMPLETE / ACCEPTED | `62a1d7b` |
| TC-P16-T009 | Hardening and evidence pack | COMPLETE / ACCEPTED | `ee02dd8` |
| TC-P16-GATE | P16 UGC Acceptance Gate | COMPLETE / ACCEPTED | `538f3fc` |
| TC-P17-PLAN | Visa Architecture Plan | COMPLETE / ACCEPTED | `1b5c8ea` |
| TC-P17-T001 | Visa module scaffolding | COMPLETE / ACCEPTED | `5f18f83` |
| TC-P17-T002 | VisaDefinition / RequirementSet baseline | COMPLETE / ACCEPTED | `12f19e7` |
| TC-P17-T003 | Visa applicability context baseline | COMPLETE / ACCEPTED | `8098ee2` |
| TC-P17-T004 | Required documents / eligibility facts | COMPLETE / ACCEPTED | `f5f52de` |
| TC-P17-T005 | Processing / validity / stay / entry semantics | COMPLETE / ACCEPTED | `90cd5f4` |
| TC-P17-T006 | Official visa fee vs Pricing boundary | COMPLETE / ACCEPTED | `1f3d206` |
| TC-P17-T007 | Public Visa vs Content vs SEO | AWAITING_ARCHITECT_REVIEW | (this commit) |

Bootstrap commit اولیهٔ فنی: `cf97f35`
## Locked Architectural Decisions

این‌ها تصمیم‌های قفل‌شدهٔ فعلی‌اند؛ تصمیم جدید اختراع نشده است:

- Modular Monolith
- no microservices
- ASP.NET Core 10 Minimal API
- Next.js App Router
- Server Component first
- PostgreSQL primary database
- EF Core for transactional/domain persistence
- selective Dapper for complex read models
- strong module ownership
- no cross-module DbContext access
- Destination-centric travel knowledge graph
- **P05-R1 RESOLVED:** `DestinationTranslation.Slug` = authoritative current localized Destination slug (Destination-owned); SEO owns public route/path history, reservation, and redirect mechanics only (not Destination content SoR)
- Place separates Hotel / Restaurant / Attraction catalog concepts
- **P07-R1 RESOLVED:** Place = aggregate root; canonical catalog id = `PlaceId` only; closed PlaceKind; typed specialization tables 1:1 (no TPH; no HotelBooking fields)
- **P07-R2 RESOLVED:** Optional single logical Place→Destination reference (0..1); Place-owned nullable `DestinationId`; no cross-schema FK; validate via Destination.Contracts existence query
- Hotel Catalog ≠ Hotel Booking
- TourProduct ≠ TourDeparture
- Experience Tour و Foreign Package Tour کهن‌الگوهای متمایزند
- mixed / multi-currency pricing
- Price ≠ Quote ≠ Payment
- multilingual from day one
- no `NameFa` / `NameEn` / `NameAr` database pattern
- RTL/LTR from day one
- bidi-safe UI
- mobile-first
- SEO-first
- accessibility is first-class
- Admin is a presentation surface, not a domain module
- architecture changes require ADR
- One Task → One Writer
- P04 R3 RESOLVED: public Destination pages may exist for humans but MUST use robots noindex,follow (SEO/indexation engine deferred to P05)
- **P06-R3 RESOLVED:** synchronous Media-owned variant generation (no Hangfire/queue); sizing large=1600 / medium=960 / thumbnail=320 fit-within; no crop/upscale; original not duplicated; GIF fail-closed
- **P06-R1 RESOLVED — DEFER:** no WebP/AVIF conversion / automatic WebP generation / content negotiation in P06; accepted optimization posture = same-format derived variants (T005/T008)
- **P06-R4 RESOLVED — APP PROXY:** Browser → TravelCore Media delivery → `IMediaObjectStorage.OpenRead`; anonymous Ready-only; StorageKey never public; Signed URL deferred; Direct object URL rejected for P06
- **P06-R5 RESOLVED — CONTRACT-ONLY:** consumer MediaAssetId reference proven via Media.Contracts + ArchitectureTests; no Destination MediaAssetId/role in P06
- **P08-R3 RESOLVED:** `ContentItemTranslation` owns localized current slug; SEO owns route binding, redirect history, canonical/history, publication SEO state; no global slug engine in Content
- **P08-R4 RESOLVED:** default IndexPolicy = **noindex, follow**; public route existence ≠ indexing; SEO owns final IndexPolicy; Content only exposes SEO hooks; publication services do not set IndexPolicy
- **P13-R1 RESOLVED:** Independent Agency Marketplace module owns Agency commercial relationship (schema `agency_marketplace`). Party remains identity SoR; Marketplace is the commercial layer. Logical PartyId Guid only — no Party/Tour/Pricing merge, no Offer in T001.
- **P13-R2 RESOLVED:** Party remains identity SoR. Agency Marketplace owns AgencyProfile (0..1 per Agency PartyId). Logical PartyId Guid only — no Party schema change.
- **P13-R3 RESOLVED:** AgencyOffer owns the marketplace sales relationship. TourProduct remains catalog SoR. Logical TourProduct Guid only — no Tour FK, no Price/Booking/Payment/Inventory/Departure ownership.
- **P13-R4 RESOLVED:** Agency must NOT override Price. AgencyOffer commercial terms = Notes + SalesRules metadata only — no Money, Discount, Commission, Currency, or Quote.
- **P13-R5 RESOLVED:** Agency does NOT own capacity. TourDeparture remains capacity SoR. AgencyOffer may hold SalesAvailability metadata + optional logical TourDeparture Guid — no seats, reservation, or allocation.
- **P13-R6 RESOLVED:** Agency Panel belongs to Agency Marketplace (not Tour Admin, not Identity). Foundation: profile + offer management. No Booking/Payment/Commission/CRM.
- **P15-R1 RESOLVED:** Search is an independent Discovery Owner (schema `search`). Owns query/result contracts and future read models. Does not own Tour/Content/Pricing/Agency facts or SEO IndexPolicy. Search is a Read Model / Projection later, not SoT. No LLM/business rules inside Search. T001: no projection tables, no FTS, no Elasticsearch, no ranking/faceting engine.
- **P15-R2 RESOLVED:** Hybrid Read Model. Search owns `SearchDocument` + `ISearchIndex` abstraction. Domain modules remain SoT. No Elasticsearch/OpenSearch/SQL FTS/`pg_trgm` in T002. SearchDocument is not a domain entity.
- **P15-R3 RESOLVED:** Transactional Outbox + Async Projection Worker. Search failure must not fail domain transaction. Projection retryable + idempotent. No RabbitMQ/real queue in T003.
- **P15-R4 RESOLVED:** Search owns faceting Aggregation / Counting / Result composition. Domain modules own attribute meaning and source facts. PublicExperience owns filter UI only. No facet engine, Elasticsearch aggregations, ranking, recommendation, AI model, or Tour/Content/Pricing facet tables in T004. Structured fields on SearchDocument remain available for future facets.
- **P15-R5 RESOLVED:** Deterministic explainable ranking signals + stable tie-break. Search owns ranking composition / relevance ordering / ranking metadata. Does not own Tour/Agency commercial priority, commission, sponsorship, or profitability. Ranking ≠ Recommendation. No ML / embeddings / vector / personalization in T005.
- **P15-R6 RESOLVED:** Structured attributable locale-aware facts first. `SemanticRetrievalSnapshot` + `SearchFactProvenance`. Search is not AI platform / LLM gateway / vector store. No embeddings, vector DB, RAG, LLM, or AI-generated domain facts. Search ≠ SoT.
- **P15-R7 RESOLVED:** Engine-neutral public Search query API (`GET /api/search`). Structured filters + continuation-ready pagination + explicit locale. Not SEO IndexPolicy owner. Empty stub execution allowed. No provider DSL leak.
- **P16-R1 RESOLVED:** Independent UGC module (schema `ugc`). Owns user-generated content lifecycle. Does not own Identity/Party, Content CMS, MediaAsset technical truth, Tour/Place/Destination facts, SEO IndexPolicy, Search, Booking, or Payment. Actor = opaque logical id.
- **P16-R2 RESOLVED:** Review is the aggregate. OverallRating (1..5) is part of Review. Dimension ratings are children. Rating is not an independent aggregate. No hardcoded Hotel/Guide/Food/Service columns.
- **P16-R3 RESOLVED:** Each Review owns exactly one logical target (`TargetType` + `TargetId`). Controlled: TourProduct · Place · Agency. No peer-schema FK. Structural `IReviewTargetValidator` only.
- **P16-R4 RESOLVED:** Travelogue is an independent UGC narrative aggregate. Article/Guide/LandingPage remain Content CMS. Travelogue != ContentItem. Do not store Travelogue as a ContentItem flag. Publication/moderation remains P16-R7.
- **P16-R5 RESOLVED:** UGC owns UserPhoto relationship (Actor + logical MediaAssetId). Media owns technical MediaAsset truth. UserPhoto relationship != MediaAsset. No peer FK. No second media store.
- **P16-R6 RESOLVED:** Comment = IN (flat comments on Review/Travelogue). Like = DEFERRED. No threading, ranking, or moderation.
- **P16-R7 RESOLVED:** ModerationStatus (Pending/Approved/Rejected) is distinct from PublicationStatus (Draft/Published/Hidden/Archived). Approved != Published. Published != SEO Indexed. Public eligibility = Approved + Published. Rejected never public. UgcReport is UGC-owned moderation input only — no automatic hide/reject/ban/ranking/SEO.
- **P16-R8 RESOLVED:** UGC is the user-generated fact owner, including public-eligibility truth. PublicExperience owns composition/presentation only. Search is a retrieval projection (T008 must not build a Search engine). SEO owns IndexPolicy. Publicly Eligible != SEO Indexed. Publicly Eligible != Automatically Search Indexed. Rating summary is a derived rebuildable read model, not an independent Average Rating engine.
- **P17-R1 RESOLVED:** Independent Visa module (schema `visa`). Owns structured visa-domain facts and their lifecycle. Does not own Destination/ReferenceData geography, Content CMS, MediaAsset technical truth, Pricing/Quote, Booking, Payment, SEO IndexPolicy, Search, or Identity/Party. Geographic references are opaque logical ids only. T001: no VisaDefinition/requirement/document/fee/application product types.
- **P17-R2 RESOLVED:** VisaDefinition = stable visa-type identity/meaning. VisaRequirementSet = context-dependent requirement facts for one definition. VisaDefinition 1 → 0..N VisaRequirementSet. **VisaDefinition != VisaRequirementSet**. No applicability/docs/fees in T002.
- **P17-R3 RESOLVED:** Each VisaRequirementSet has exactly one VisaApplicability. Destination/jurisdiction is an opaque logical id. Nationality/residence are optional opaque ISO alpha-2 codes. Optional controlled ApplicantCategory. **Applicability != Rules Engine**. No peer-schema FK.
- **P17-R4 RESOLVED:** VisaRequirementSet owns RequiredDocument and EligibilityRequirement children. **RequiredDocument != EligibilityRequirement**. **EligibilityRequirement != Rules Engine**. No applicant uploads/OCR/MediaAsset.
- **P17-R5 RESOLVED:** ProcessingTime != VisaValidity != AllowedStay. EntryPolicy is independent. No Duration field. EffectiveFrom/EffectiveTo are readiness only.
- **P17-R6 RESOLVED:** OfficialVisaFee != CommercialPrice. OfficialVisaFee != Quote. Visa stores official/regulatory fee facts with platform Money. Pricing remains Price/Quote authority. No FX.
- **P17-R7 RESOLVED:** Visa owns structured facts and public read contracts. PublicExperience owns VisaDetailPage composition. Content remains editorial. SEO owns IndexPolicy. Public Visa Page != Automatically SEO Indexed. Public Visa Visibility != SEO Indexed. Structured Visa Fact != Editorial Guidance. Visa != PublicExperience. No application workflow.

منبع تفصیلی: `AGENTS.md` و `docs/architecture/00-constitution.md`

---

## Current Conceptual Modules

### Foundation / Business Identity

Identity · Access · Party · ReferenceData

### Discovery

Destination · Place · Media

### Knowledge / Community

Content · UGC (Review + Travelogue + UserPhoto + Comment + public composition reads · Like deferred · schema `ugc` · P16-R1–R8)

### Commerce

Tour · Pricing (Price + Quote baseline · public read-only price summary · requested display-currency metadata · FX boundary contracts · PriceComponent · occupancy rules · Admin Pricing API · Money · schema `pricing` · P12-R1/R2/R3/R4/R5/R6/R7/R8) · AgencyMarketplace (AgencyProfile + AgencyOffer · commercial terms without Price · sales availability without capacity · schema `agency_marketplace` · P13-R1–R5) · Visa (VisaDefinition + RequirementSet + Applicability + documents/eligibility + public read · schema `visa` · P17-R1–R7) · Booking · Payment

### External Inventory / Booking

HotelBooking · Flight

### Platform Capabilities

Search (scaffolded · schema `search` · hybrid read-model · projection · faceting · ranking · AI-readiness · public query API stub · P15-R1–R7) · SEO · Notification

**صریح:** Admin یک domain module نیست. Admin Panel (و همچنین Public Website و Agency Panel) سطوح Presentation هستند.

---

## Critical Domain Distinctions

- TourProduct ≠ TourDeparture
- Hotel Catalog ≠ Hotel Booking
- Price ≠ Quote ≠ Payment
- PassengerCategory ≠ Occupancy
- Locale ≠ Currency ≠ Calendar ≠ Timezone
- Domain Model ≠ Persistence Model ≠ API Contract ≠ Page View Model
- VisaDefinition != VisaRequirementSet
- Applicability != Rules Engine
- RequiredDocument != EligibilityRequirement

واژه‌نامه: `docs/domain/glossary.md`

---

## Reference Product Pages

این‌ها مرجع محصول / UX / دامنه / SEO هستند، **نه** وابستگی پیاده‌سازی و نه مجوز کپی کد یا محتوا.

| مرجع | URL |
|------|-----|
| LastSecond Foreign Package Tour | https://lastsecond.ir/tours/276507-%D8%AA%D9%88%D8%B1-%D8%A7%D8%B3%D8%AA%D8%A7%D9%86%D8%A8%D9%88%D9%84-%D8%AA%D8%A7%D8%A8%D8%B3%D8%AA%D8%A7%D9%86-1405 |
| LastSecond Experience Tour | https://lastsecond.ir/tours/g1487-%D8%AA%D9%88%D8%B1-%D8%AF%D8%B1%DB%8C%D8%A7%DA%86%D9%87-%D8%AF%D8%A7%D9%84%D8%A7%D9%85%D9%BE%D8%B1-%D8%AA%D8%A7-%D8%A7%D8%B1%D9%88%D9%85%DB%8C%D9%87 |
| LastSecond | https://lastsecond.ir/ |
| TahaGasht | https://www.tahagasht.com/ |

رجیستری کامل‌تر: `docs/reference-sites/page-registry.md`

---

## Known Environment Notes

نسخه‌های تأییدشدهٔ لحظه‌ای (قید ابدی معماری نیستند):

| ابزار | نسخه |
|------|------|
| .NET SDK | 10.0.103 |
| ASP.NET Core runtime | 10.0.3 |
| Next.js | 16.3.0 |
| React | 19.2.8 |
| TypeScript | 5.9.3 |
| Node | 24.19.0 |
| npm | 11.17.0 |

یادداشت محیطی bootstrap:

در طی bootstrap اولیهٔ بک‌اند، NuGet.org موقتاً در دسترس نبود؛ بنابراین API با قالب رسمی و فلگ no-OpenAPI scaffold شد.

این **تصمیم معماری برای حذف OpenAPI نیست**. OpenAPI همچنان برنامه‌ریزی شده و باید در Task صریح Foundation بعدی اضافه شود.

جزئیات: `docs/architecture/02-technology-baseline.md`

---

## Source of Truth Order

اولویت منبع حقیقت:

1. Accepted ADRs
2. `AGENTS.md`
3. اسناد فعلی architecture / domain / SEO / UI
4. `docs/PROJECT-STATE.md`
5. مشخصات Task پذیرفته‌شدهٔ جاری
6. Implementation / code
7. Historical prompts / chat discussions

اگر اسناد تعارض داشتند، تعارض را **گزارش** کنید؛ خاموش حل نکنید.

پیام چت قدیمی نباید بر ADR یا سند پذیرفته‌شدهٔ جدیدتر غلبه کند.

---

## Recovery Procedure

وقتی توسعه در چت/نشست AI جدید ادامه می‌یابد:

1. ریشهٔ ریپو را با `git rev-parse --show-toplevel` کشف کنید (مسیر ثابت یک ماشین الزامی نیست).
2. هویت remote را تأیید کنید: `mrnikiemami-code/TravelCore` (`git remote -v`).
3. `git fetch origin` و در صورت behind بودن، همگام‌سازی safe با `git pull --ff-only`.
4. `AGENTS.md` را بخوانید.
5. `docs/PROJECT-STATE.md` را بخوانید.
6. `docs/ROADMAP.md` را بخوانید.
7. اسناد ارجاع‌شده توسط **Current Next Task** را بخوانید.
8. تاریخچهٔ اخیر Git و وضعیت working tree را تأیید کنید.
9. از **Current Next Task** ادامه دهید.
10. فقط به‌خاطر نبودن context چت قبلی، معماری پذیرفته‌شده را بازطراحی نکنید.
11. تاریخچه را force-push / hard-reset نکنید.

جزئیات گردش‌کار چندماشینه: [`architecture/09-ai-development-workflow.md`](architecture/09-ai-development-workflow.md)

پیام پیشنهادی برای شروع گفت‌وگوی جدید با معمار:

> Continue TravelCore as the senior software architect. The repository is the source of truth. Read AGENTS.md and docs/PROJECT-STATE.md first. Continue from Current Next Task and do not redesign accepted decisions without ADR.

---

## Update Policy

`PROJECT-STATE.md` باید پس از این موارد به‌روز شود:

- هر Task عمدهٔ پذیرفته‌شده
- هر ADR پذیرفته‌شده
- انتقال Phase
- تغییر عمدهٔ محیط
- تغییر Next Task
- blocker مادی

این فایل را به مستند طراحی تفصیلی تبدیل نکنید. تصمیم‌های جزئی در اسناد اختصاصی خودشان می‌مانند.

---

## AI Working Rule

هیچ تصمیم معماری مهمی نباید **فقط** داخل گفت‌وگوی ChatGPT / Cursor / Hermes باقی بماند.

اگر گفت‌وگو به تصمیم معماری پذیرفته‌شده برسد، قبل از بسته شدن Task باید در مستندات مناسب ریپو persist شود.
