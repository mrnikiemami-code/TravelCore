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
| Current Phase | **P01 — Platform / Backend Foundation** (**IN_PROGRESS**) |
| Previous Phase | P00 — Architecture Foundation |
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
| TC-GOV-T002A | AWAITING_ARCHITECT_REVIEW (ADR 0014 acceptance / AGENTS+Recovery activation) |
| Last Accepted Commit | `1cfe48a` |
| ADR 0001–0014 | ALL Accepted |
| Unresolved Proposed ADR | NO |
| Accepted Pipeline Governance | ADR 0013 · ADR 0014 |
| Canonical Pipeline Entry | [`docs/ai/TRAVELCORE-PIPELINE-PROTOCOL.md`](ai/TRAVELCORE-PIPELINE-PROTOCOL.md) |
| Pipeline Protocol | **READY** |
| Pipeline Runtime Policy | [`docs/ai/pipeline-runtime-policy.json`](ai/pipeline-runtime-policy.json) |
| Operating Modes | HUMAN (default) / PIPELINE (USER opt-in) |
| Default Mode | **HUMAN** |
| Current Runtime Mode | **PIPELINE** |
| Automatic Pipeline | **ON** (USER opted in with phase confirm) |
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
| Current Active Product Task | **TC-P01-T016** (AWAITING_ARCHITECT_REVIEW) |
| Current Next Product Phase | P01 — Platform / Backend Foundation |
| Current Next Task | Architect review of `TC-P01-T016`; do not start `TC-P01-T017` until accepted |
| P01 | **IN_PROGRESS** (AUTHORIZED) |
| P01 Plan | `TC-P01-PLAN-R1` Architect Accepted |
| P01 Implementation Started | **YES** |
| Last P01 Implementation Commit | pending T016 (`TC-P01-T015` = `6d66d9e`) |
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
| Phase Transition State | **P01_IN_PROGRESS** |
| P01 Phase Gate | NOT_STARTED (after T001–T019) |
| Human Phase Confirmation | P01 confirmed; P02 still requires `TRAVELCORE_PHASE_CONFIRM: P02` later |
| Pipeline Product Execution | **NORMAL — AWAITING_ARCHITECT_REVIEW** (`TC-P01-T016`) |
| Human Confirmation Reason | None for current T016 |
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
| TC-P01-T016 | AWAITING_ARCHITECT_REVIEW |
| Required Human Token | `TRAVELCORE_PHASE_CONFIRM: P01` |

### P00 Exit Summary

- P00 Architecture Foundation formally complete
- TC-P00-GATE PASS
- ADR 0001–0014 Accepted
- Canonical pipeline entry ACTIVE: `docs/ai/TRAVELCORE-PIPELINE-PROTOCOL.md`
- Pipeline Protocol = READY; Current Runtime Mode = HUMAN; Automatic Pipeline = OFF
- P01 is next **product** phase but has **not** started
- Product execution is stopped at the P00→P01 human phase gate until a new USER-authored `TRAVELCORE_PHASE_CONFIRM: P01`

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
| TC-GOV-T002A | Accept ADR 0014 + activate Pipeline Protocol in AGENTS/Recovery | AWAITING_ARCHITECT_REVIEW | see git log for this commit |

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
- Place separates Hotel / Restaurant / Attraction catalog concepts
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
