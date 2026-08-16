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
| Current Phase | **P07 — Place Catalog** (**IN_PROGRESS**) |
| Previous Phase | **P06 — Media** (**COMPLETE**) |
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
| Last Accepted Commit | `da345b5` (`TC-P06-GATE`) · hygiene `0d2edad` · P06 COMPLETE docs `77eb9dd` |
| ADR 0001–0014 | ALL Accepted |
| Unresolved Proposed ADR | NO |
| Accepted Pipeline Governance | ADR 0013 · ADR 0014 |
| Canonical Pipeline Entry | [`docs/ai/TRAVELCORE-PIPELINE-PROTOCOL.md`](ai/TRAVELCORE-PIPELINE-PROTOCOL.md) |
| Pipeline Protocol | **READY** |
| Pipeline Runtime Policy | [`docs/ai/pipeline-runtime-policy.json`](ai/pipeline-runtime-policy.json) |
| Operating Modes | HUMAN (default) / PIPELINE (USER opt-in) |
| Default Mode | **HUMAN** |
| Current Runtime Mode | **PIPELINE** |
| Automatic Pipeline | **ON** (USER `TRAVELCORE_MODE: PIPELINE`; `TRAVELCORE_PHASE_CONFIRM: P07`) |
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
| Current Active Product Task | `TC-P07-T006-R1` — AWAITING_ARCHITECT_REVIEW |
| Current Next Product Phase | P07 — Place Catalog (**IN_PROGRESS**) |
| Current Next Task | Architect review of `TC-P07-T006-R1`; then Auto-Execute `TC-P07-T007` only after ACCEPT of T006(+R1) |
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
| P07 | **IN_PROGRESS** (AUTHORIZED via `TRAVELCORE_PHASE_CONFIRM: P07`) |
| P07 Plan | `TC-P07-PLAN` **COMPLETE / ACCEPTED** (`5dbc152`) — [`docs/plans/P07-implementation-plan.md`](plans/P07-implementation-plan.md) |
| P07-T001 | **COMPLETE / ACCEPTED** (`108ac34`; hygiene `a245358`) |
| P07-T002 | **AWAITING_ARCHITECT_REVIEW** (Place catalog domain + persistence baseline; `83529cf`) |
| P07-T002-R1 | **AWAITING_ARCHITECT_REVIEW** (`0b86f05`) — PlaceId identity + T002 scope reconciliation (docs-only) |
| P07-T003 | **AWAITING_ARCHITECT_REVIEW** — Localization + Destination link + geo/address |
| P07-T004 | **AWAITING_ARCHITECT_REVIEW** — Facilities · classification · catalog status |
| P07-T005 | **AWAITING_ARCHITECT_REVIEW** — Place↔Media relations (Cover/Gallery) |
| P07-T006 | **AWAITING_ARCHITECT_REVIEW** — Access permissions + Admin Place baseline (`place.places.write` · `/[locale]/admin/catalog/places`) |
| P07-T006-R1 | **AWAITING_ARCHITECT_REVIEW** — Admin Place Ready-media picker (no MediaAssetId paste primary UX) |
| P07-R1 (Place model shape) | **RESOLVED** — CORE PLACE + TYPED SPECIALIZATION (`PlaceId` only; Hotel/Restaurant/Attraction 1:1 tables; no TPH; no HotelBooking fields) |
| P07-R2 (Destination link requiredness) | **RESOLVED** — OPTIONAL SINGLE LOGICAL REFERENCE (0..1; Place-owned nullable DestinationId; no cross-schema FK; Contracts existence validation) |
| P07-R3 (Place delete/archive) | **UNRESOLVED** |
| P07-R4 (Slug ownership) | **UNRESOLVED** |
| P07-R5 (Public IndexPolicy default) | **UNRESOLVED** |
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
| Phase Transition State | **P07_IN_PROGRESS** · `TC-P07-PLAN` ACCEPTED · `TC-P07-T001` COMPLETE / ACCEPTED · `TC-P07-T002`–`T006` AWAITING_ARCHITECT_REVIEW · `TC-P07-T006-R1` AWAITING_ARCHITECT_REVIEW · **P07-R1 RESOLVED** · **P07-R2 RESOLVED** · R3–R5 UNRESOLVED |
| P01 Phase Gate | **TC-P01-GATE** COMPLETE / ACCEPTED |
| P02 Phase Gate | **TC-P02-GATE** COMPLETE / ACCEPTED (`4eacff5`) |
| P03 Phase Gate | **TC-P03-GATE** COMPLETE / ACCEPTED (`6a8a5ce`) |
| P04 Phase Gate | **TC-P04-GATE** COMPLETE / ACCEPTED (`f70991f`) |
| P05 Phase Gate | **TC-P05-GATE** COMPLETE / ACCEPTED (`7f234e8`; R1 `bde6661`) |
| P06 Phase Gate | **TC-P06-GATE** COMPLETE / ACCEPTED (`da345b5`) |
| Human Phase Confirmation | USER `TRAVELCORE_PHASE_CONFIRM: P07` received |
| Pipeline Product Execution | **ACTIVE** |
| Human Confirmation Reason | None for current task |
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
| TC-P07-T002 | **AWAITING_ARCHITECT_REVIEW** (`83529cf`) |
| TC-P07-T002-R1 | **AWAITING_ARCHITECT_REVIEW** (`0b86f05`) — PlaceId identity + T002 scope reconciliation (docs-only); artifact [`plans/P07-T002-R1-place-identity-and-scope-reconciliation.md`](plans/P07-T002-R1-place-identity-and-scope-reconciliation.md) |
| TC-P07-T003 | **AWAITING_ARCHITECT_REVIEW** — Localization + Destination link + geo/address (translations; optional DestinationId; Place-owned address/coordinates) |
| TC-P07-T004 | **AWAITING_ARCHITECT_REVIEW** — Facilities · classification · catalog status (`PlaceCatalogStatus` Draft/Active/Inactive; opaque ClassificationCode; `place_facilities`) |
| TC-P07-T005 | **AWAITING_ARCHITECT_REVIEW** — Place↔Media relations (Cover/Gallery; `place_media_links`; Ready validation via `IMediaAssetReadinessQuery`; presentation via Media contracts) |
| TC-P07-T006 | **AWAITING_ARCHITECT_REVIEW** — Access + Admin Place baseline (`place.places.write` / `Access.Place.Places.Write`; Admin `/[locale]/admin/catalog/places`; no Delete/Archive/Slug/SEO; R3–R5 required-now NO) |
| TC-P07-T006-R1 | **AWAITING_ARCHITECT_REVIEW** — Case B remediation: Ready Media visual picker for Cover/Gallery (reuses P06 list + app-proxy; no raw-ID paste primary; no DAM; no StorageKey; no Hero); evidence [`plans/P07-T006-R1-admin-place-media-picker-reconciliation.md`](plans/P07-T006-R1-admin-place-media-picker-reconciliation.md) |
| P07-R1 | **RESOLVED** — CORE PLACE + TYPED SPECIALIZATION |
| P07-R2 | **RESOLVED** — OPTIONAL SINGLE LOGICAL REFERENCE Place→Destination (0..1; nullable DestinationId; no cross-schema FK; Contracts existence validation; no DestinationKind restriction in T003) |
| P07-R3 | **UNRESOLVED** (Place delete/archive) — T004 CatalogStatus is catalog ops only (Draft/Active/Inactive); does **not** resolve R3 |
| P07-R4 | **UNRESOLVED** (Slug ownership) |
| P07-R5 | **UNRESOLVED** (Public IndexPolicy default) |
| Required Human Token | GATE later needs `TRAVELCORE_TASK_CONFIRM: TC-P07-GATE` |

### P00 Exit Summary

- P00 Architecture Foundation formally complete
- TC-P00-GATE PASS
- ADR 0001–0014 Accepted
- Canonical pipeline entry ACTIVE: `docs/ai/TRAVELCORE-PIPELINE-PROTOCOL.md`
- Pipeline Protocol = READY; Current Runtime Mode = PIPELINE (USER opt-in); Automatic Pipeline = ON
- P01 product phase COMPLETE through `TC-P01-T019` (`2370316`); `TC-P01-GATE` COMPLETE / ACCEPTED (`0853d04`)
- P02 COMPLETE; `TC-P02-PLAN` through `TC-P02-T017` ACCEPTED; `TC-P02-GATE` COMPLETE / ACCEPTED (`4eacff5`); evidence: `docs/plans/P02-T017-walking-skeleton-validation-evidence.md`
- P04 COMPLETE (`TC-P04-GATE` ACCEPTED `f70991f`); **P05 COMPLETE** (`TC-P05-GATE` ACCEPTED `7f234e8` · `TC-P05-GATE-R1` ACCEPTED `bde6661`); **P06 COMPLETE** (`TC-P06-GATE` ACCEPTED `da345b5`); Runtime Mode = PIPELINE; **P07 IN_PROGRESS** (`TC-P07-T001` COMPLETE / ACCEPTED · `TC-P07-T002`–`T006` AWAITING_ARCHITECT_REVIEW · **P07-R1 RESOLVED** · **P07-R2 RESOLVED**; R3–R5 UNRESOLVED)

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
| TC-P07-T006-R1 | Admin Place media picker UX remediation | AWAITING_ARCHITECT_REVIEW | — |

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

منبع تفصیلی: `AGENTS.md` و `docs/architecture/00-constitution.md`

---

## Current Conceptual Modules

### Foundation / Business Identity

Identity · Access · Party · ReferenceData

### Discovery

Destination · Place · Media

### Knowledge / Community

Content · UGC

### Commerce

Tour · Pricing · Visa · Booking · Payment

### External Inventory / Booking

HotelBooking · Flight

### Platform Capabilities

Search · SEO · Notification

**صریح:** Admin یک domain module نیست. Admin Panel (و همچنین Public Website و Agency Panel) سطوح Presentation هستند.

---

## Critical Domain Distinctions

- TourProduct ≠ TourDeparture
- Hotel Catalog ≠ Hotel Booking
- Price ≠ Quote ≠ Payment
- PassengerCategory ≠ Occupancy
- Locale ≠ Currency ≠ Calendar ≠ Timezone
- Domain Model ≠ Persistence Model ≠ API Contract ≠ Page View Model

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
