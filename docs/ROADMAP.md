# TravelCore Master Execution Roadmap

این سند **ترتیب اجرایی معماری** TravelCore است — نه سند تخمین اسپرینت و نه تاریخ تحویل.

هدف: حتی اگر گفت‌وگوی جاری AI از دست برود، ترتیب فازها، دروازه‌ها و نقطهٔ فعلی قابل بازیابی باشد.

---

## If You Are Continuing This Project in a New AI Conversation

به این ترتیب بخوانید:

1. `AGENTS.md`
2. `docs/PROJECT-STATE.md`
3. `docs/ROADMAP.md` (همین سند)
4. اسناد ارجاع‌شده توسط **Current Next Task**
5. آخرین Task/commit پذیرفته‌شده

سپس **فقط** از Current Next Task ادامه دهید.

معماری را از صفر دوباره تحلیل نکنید مگر مسئلهٔ صریحی پیدا شود.

### Emergency ChatGPT Recovery

اگر گفتگوی معمار از دست رفت:

`docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md`

این Prompt را در Cursor اجرا کنید و Recovery Packet را به ChatGPT جدید بدهید — قبل از ادامهٔ توسعه.

---

## Project Position

| فیلد | مقدار |
|------|--------|
| Project | TravelCore |
| Current Phase | **P26 — Advanced SEO + Content Graph** (**PLANNED** — `TC-P26-PLAN` authored) |
| Phase Status | P00–P25 COMPLETE · P26 plan authored · product NOT_STARTED |
| Last Accepted P00 Task | TC-P00-T008 |
| Accepted Architecture Commit (T008) | `1bd4e95` |
| Acceptance / State Commit (T008A) | `0074437` |
| Last Accepted Commit | `b372367` (`TC-P12-GATE`) · P12 COMPLETE / ACCEPTED |
| P00 Final Gate | TC-P00-GATE — PASS |
| P00 Closure | TC-P00-CLOSE |
| Current Next Task | Return `TC-P26-PLAN RESULT`; do **not** execute product tasks until architect accepts PLAN |
| TC-P00-T002 State | COMPLETE / ACCEPTED |
| TC-P00-T003 State | COMPLETE / ACCEPTED |
| TC-P00-T004 State | COMPLETE / ACCEPTED |
| TC-P00-T005 State | COMPLETE / ACCEPTED |
| TC-P00-T006 State | COMPLETE / ACCEPTED |
| TC-P00-T007 State | COMPLETE / ACCEPTED |
| TC-P00-T007R | PASS |
| TC-P00-T008 State | COMPLETE / ACCEPTED |
| TC-P00-T008R | PASS |
| TC-P00-GATE State | PASS |
| Future Transition Map | [`architecture/15-future-architecture-transition-map.md`](architecture/15-future-architecture-transition-map.md) |

### Accepted repository state (خلاصه)

- .NET 10 · ASP.NET Core 10 Minimal API
- Next.js 16 · React 19 · TypeScript
- PostgreSQL / Redis / S3-compatible object storage — planned
- Modular Monolith
- multilingual · RTL/LTR · mobile-first · SEO-first
- mixed / multi-currency

جزئیات وضعیت لحظه‌ای: [`PROJECT-STATE.md`](PROJECT-STATE.md)

---

## Source of Truth Relationship

| سند | نقش |
|-----|-----|
| `AGENTS.md` | قواعد اجرایی عامل‌ها |
| `docs/ROADMAP.md` | برنامهٔ مرتب master (همین فایل) |
| `docs/PROJECT-STATE.md` | موقعیت فعلی / بازیابی |
| `docs/architecture/*` | معماری پذیرفته‌شدهٔ تفصیلی |
| `docs/adr/*` | تغییرات معماری پذیرفته‌شده |
| `docs/prompts/*` | دستورهای اجرایی Task |

این Roadmap جزئیات معماری را تکرار نمی‌کند؛ به آن‌ها لینک می‌دهد.

قانون اساسی: [`architecture/00-constitution.md`](architecture/00-constitution.md)

---

## Roadmap Status Model

هر Phase یکی از این وضعیت‌ها را دارد:

| Status | معنی |
|--------|------|
| PLANNED | هنوز شروع نشده |
| IN_PROGRESS | در جریان |
| COMPLETE | پذیرفته‌شده و بسته |
| BLOCKED | متوقف به‌خاطر مانع صریح |

وضعیت فعلی:

- **P00** = COMPLETE
- **P01** = COMPLETE
- **P02** = COMPLETE (`TC-P02-GATE` ACCEPTED `4eacff5`)
- **P03** = COMPLETE (`TC-P03-GATE` ACCEPTED `6a8a5ce`)
- **P04** = COMPLETE (`TC-P04-GATE` ACCEPTED `f70991f`; R3 RESOLVED = noindex,follow)
- **P05** = COMPLETE (`TC-P05-GATE` ACCEPTED · `TC-P05-GATE-R1` ACCEPTED · **P05-R1/R2 RESOLVED**)
- **P06** = COMPLETE (`TC-P06-GATE` ACCEPTED `da345b5` · **P06-R1 DEFER**; **P06-R2/R3/R4/R5/R6 RESOLVED**; **P06-R7 DEFERRED**; **P06-R8 UNRESOLVED**; **P06-R9 DEFERRED**)
- **P07** = COMPLETE (`TC-P07-GATE` ACCEPTED `84a0a48` · **P07-R1/R2/R4/R5 RESOLVED** · **P07-R3 UNRESOLVED**)
- **P08** = COMPLETE (`TC-P08-GATE` ACCEPTED `576b7fa` · R1–R5 RESOLVED · R6–R8 UNRESOLVED)
- **P09** = COMPLETE (`TC-P09-GATE` ACCEPTED `67fc580` · T001–T010 ACCEPTED · **R1–R8 RESOLVED** · product T010 `0334bae`)
- **P10** = COMPLETE (`TC-P10-GATE` ACCEPTED `c351bf9` · **R1–R8 RESOLVED**)
- **P11** = COMPLETE (`TC-P11-GATE` ACCEPTED `6f7ea12` · **R1–R8 RESOLVED**)
- **P12** = COMPLETE / ACCEPTED (`b372367`) (**R1–R8 RESOLVED** · T001–T009 ACCEPTED — [`plans/P12-implementation-plan.md`](plans/P12-implementation-plan.md))
- **P13** = COMPLETE (`TC-P13-GATE` ACCEPTED `c0bcd78` · **R1–R7 RESOLVED** · T008 vacant)
- **P14** = COMPLETE (`TC-P14-GATE` ACCEPTED `608216d` · **R1–R8 RESOLVED**)
- **P15** = COMPLETE (`TC-P15-GATE` ACCEPTED `4e2098d` · **R1–R7 RESOLVED** · T008 vacant)
- **P16** = COMPLETE (`TC-P16-GATE` ACCEPTED `538f3fc` · **R1–R8 RESOLVED** · T001–T009 ACCEPTED) [`plans/P16-implementation-plan.md`](plans/P16-implementation-plan.md)
- **P17** = COMPLETE — GATE ACCEPTED [`plans/P17-GATE-acceptance-evidence.md`](plans/P17-GATE-acceptance-evidence.md)
- **P18** = COMPLETE (`TC-P18-GATE` ACCEPTED `73605aa` · **R1–R8 RESOLVED**) [`plans/P18-GATE-acceptance-evidence.md`](plans/P18-GATE-acceptance-evidence.md)
- **P19** = COMPLETE (`TC-P19-GATE` ACCEPTED `d258933` · **P19-R1–R8 RESOLVED** · T001–T009 ACCEPTED) [`plans/P19-implementation-plan.md`](plans/P19-implementation-plan.md)
- **P20** = COMPLETE (`TC-P20-GATE` ACCEPTED · **R1–R8 RESOLVED** · T001–T009 ACCEPTED) [`plans/P20-GATE-acceptance-evidence.md`](plans/P20-GATE-acceptance-evidence.md)
- **P21** = COMPLETE (`TC-P21-GATE` ACCEPTED `858b4be` / docs `d6bd842` · **P21-R1–R8 RESOLVED** · T001–T009 ACCEPTED) [`plans/P21-GATE-acceptance-evidence.md`](plans/P21-GATE-acceptance-evidence.md)
- **P22** = COMPLETE (`TC-P22-GATE` ACCEPTED `2a372ae` / docs `ed040f0` · **P22-R1–R8 RESOLVED** · T001–T009 ACCEPTED)
- **P23** = COMPLETE (`TC-P23-GATE` re-execution · **P23-R1–R8 RESOLVED** · T001–T009 ACCEPTED)
- **P24** = COMPLETE (`TC-P24-GATE` implemented · P24-R1–R8 RESOLVED)
- **P25** = **COMPLETE** (`TC-P25-GATE` ACCEPTED `ed5c95f`)
- **P26** = PLANNED / NOT_STARTED (`TC-P26-PLAN` authored · awaiting architect review)
- **P26–P29 و Post-P29** = PLANNED / NOT_STARTED

کار آینده را COMPLETE علامت نزنید.

---

## Roadmap vs Product Backlog

| مفهوم | معنی |
|-------|------|
| **ROADMAP** | ترتیب اجرایی معماری و وابستگی‌ها |
| **BACKLOG** | فیچرها، بهبودها و اولویت‌های تجاری جزئی |

این Roadmap موظف نیست همهٔ فیچرهای آیندهٔ TravelCore را فهرست کند.

---

## Phase Gates

معمار باید Phase/Task را صریحاً بپذیرد قبل از اینکه وابستگی بحرانی بعدی شروع شود.

Cursor نباید فقط چون «ماژول بعدی آسان بود» جلو بپرد.

پیاده‌سازی speculative آینده ممنوع است.

---

## Task Granularity

یک Phase برابر با یک Prompt واحد Cursor نیست.

مثال: P04 Destination ممکن است به Taskهایی مثل domain specification، persistence، use caseها، hierarchy query، localization، admin tree، public details، SEO integration و … شکسته شود.

شناسه‌های دقیق Task وقتی معمار به آن Phase برسد ساخته می‌شوند.

صدها Task ID جعلی از قبل نسازید. **ترتیب Phase قفل است.**

---

## Change Policy

Roadmap پایدار است ولی immutable مطلق نیست.

**مجاز:**

- ریزکردن Task داخل یک Phase
- افزودن Task جاافتاده داخل Phase موجود
- شکستن Phase اگر شواهد پیاده‌سازی ایجاب کند
- روشن‌کردن acceptance gateها

**نیاز به تأیید معمار / احتمالاً ADR:**

- جابه‌جایی وابستگی‌های عمده
- حذف قابلیت عمده
- تغییر رادیکال جهت فنی
- تغییر سبک معماری
- معرفی microservices
- دور زدن foundationهای UI / i18n / SEO

Cursor/Hermes نباید بر اساس سلیقه، `ROADMAP.md` را خاموش بازنویسی کنند.

---

## Cross-Cutting Requirements

این‌ها فازهای جدا و «بعداً» نیستند:

Multilingual · RTL/LTR · Bidi · Mobile · SEO · Accessibility · Security · Testing · Performance awareness

در Taskهای فیچر مرتبط از ابتدا ارزیابی می‌شوند.

مثال نقض Roadmap: اعلام Done برای صفحهٔ Tour با جملهٔ «RTL بعداً اضافه می‌شود».

---

# Master Phases

## P00 — Architecture Foundation

**Status:** COMPLETE

**Purpose:** قبل از پیاده‌سازی کسب‌وکار، معماری آن‌قدر صریح باشد که Cursor/Hermes مرز ماژول، مالکیت داده، رفتار UI، قواعد SEO یا localization را اختراع نکنند.

### Already completed

| Task | خلاصه |
|------|--------|
| TC-P00-T000A | Backend bootstrap |
| TC-P00-T000B | Frontend + local repository bootstrap |
| TC-P00-T000C | GitHub remote / private synchronization |
| TC-P00-T001 | Architecture Brain / Constitution |
| TC-P00-T001A | Project continuity / recovery state |
| TC-P00-T001B | Master Roadmap (همین سند) |
| TC-P00-T002 | Domain Map + Module Boundaries + Ownership Matrix + Dependency Rules — COMPLETE / ACCEPTED (`08343e7`) |
| TC-P00-T003 | Data Architecture — COMPLETE / ACCEPTED (`3904bb9`); ADRs 0001–0004 Accepted |
| TC-P00-T003R | Canonical repository identity normalization — PASS / ACCEPTED (`840c3e5`) |
| TC-P00-T004 | UI Constitution — COMPLETE / ACCEPTED (`48e0472`); ADRs 0005–0006 Accepted |
| TC-P00-T005 | Internationalization Architecture — COMPLETE / ACCEPTED (`66e6f32`); ADRs 0007–0008 Accepted |
| TC-P00-T006 | SEO Constitution — COMPLETE / ACCEPTED (`5dbbb45`); ADRs 0009–0010 Accepted |
| TC-P00-T007 | Reference Page Archetypes — COMPLETE / ACCEPTED (`fbf1617`); TC-P00-T007R PASS |
| TC-P00-T008 | Engineering Quality Constitution — COMPLETE / ACCEPTED (`1bd4e95`); ADRs 0011–0012 Accepted; TC-P00-T008R PASS |

### Next planned architecture tasks

#### TC-P00-GATE — Final Architecture Foundation Gate

**State:** PASS

Final architecture gate accepted by Chief Architect. P00 may close.

### P00 completion gate

TC-P00-GATE = PASS. P00 = COMPLETE. P01 COMPLETE (`TC-P01-GATE` ACCEPTED `0853d04`). P02 COMPLETE (`TC-P02-GATE` ACCEPTED `4eacff5`). P03 COMPLETE (`TC-P03-GATE` ACCEPTED `6a8a5ce`). P04 IN_PROGRESS — authoritative plan: [`plans/P04-implementation-plan.md`](plans/P04-implementation-plan.md) (`TC-P04-PLAN` ACCEPTED `9d264e6`). `TC-P04-T001` AWAITING_ARCHITECT_REVIEW. P05 remains NOT_STARTED.

---

## P01 — Platform / Backend Foundation

**Status:** COMPLETE

**Purpose:** اسکلت فنی را به host آمادهٔ Modular Monolith تبدیل کند.

حوزه‌های برنامه‌ریزی‌شده: ساختار پروژه · Module registration · Building Blocks فقط در صورت اشتراک واقعی · سازماندهی Minimal API · سطوح Public/Admin/Agency نسخه‌دار · ProblemDetails · validation · **OpenAPI restoration/configuration** · configuration · PostgreSQL · EF Core · migrations · Redis abstraction · object storage abstraction · health checks · structured logging · correlation · event abstraction · Outbox · background processing abstraction · test infrastructure · Architecture Tests · CI · Docker/dev environment

**یادداشت محیطی:** API اولیه به‌خاطر قطع موقت NuGet.org با `--no-openapi` ساخته شد. OpenAPI همچنان بخشی از foundation intentional است.

### P01 completion gate

Backend foundation بیلد و تست می‌شود و اجرای مرز ماژول قبل از بزرگ شدن فیچر ماژول‌ها وجود دارد.

---

## P02 — Frontend Foundation + Walking Skeleton

**Status:** COMPLETE

**Authoritative plan:** [`plans/P02-frontend-foundation-walking-skeleton.md`](plans/P02-frontend-foundation-walking-skeleton.md) (`TC-P02-PLAN`)

**Purpose:** قبل از ساخت ده‌ها صفحه، معماری UI / i18n / RTL-LTR / mobile / SEO را اثبات کند.

پیاده‌سازی پایه: app architecture · locale-aware root · HTML lang/dir · Design Tokens · typography · containers/grid · primitives · responsive · bidi-safe primitives · Money / MixedCurrencyPrice · loading/error · Public shell · Admin shell layout primitives (navigation IA after cross-domain workflow task) · cross-domain workflow & navigation model · API client strategy · OpenAPI-generated types/client اگر تأیید شد · image foundation · a11y baseline

سپس **UI Walking Skeleton واقعی** با fixtureهای typed.

**صفحهٔ اعتبارسنجی اصلی:** Foreign Package Tour Detail

چرا؟ چون هم‌زمان RTL فارسی، LTR انگلیسی، محتوای mixed-direction، کد فرودگاه، شماره پرواز، تاریخ، گزینه هتل، rating، قیمت چندارزی، pricing مسافر/اشغال، جدول/کارت responsive، sticky CTA، layout موبایل/دسکتاپ، metadata و معنای ساخت‌یافته را می‌آزماید.

ماتریس حداقل: FA Desktop · FA Mobile · EN Desktop · EN Mobile
عرض‌های نماینده: 360 · 390 · 768 · 1024 · 1280 · 1440

Prototype باید قرارداد Design System و fixture typed داشته باشد — HTML دورریختنی نیست.

### P02 completion gate

**TC-P02-GATE** = COMPLETE / ACCEPTED. معماری UI در RTL و LTR و mobile/desktop برای walking skeleton پذیرفته شد. P03 = COMPLETE (`TC-P03-GATE`).

---

## P03 — Identity + Access + Party

**Status:** COMPLETE

**Authoritative plan:** [`plans/P03-implementation-plan.md`](plans/P03-implementation-plan.md) (`TC-P03-PLAN`)

**Progress:** T001–T012 COMPLETE / ACCEPTED · **`TC-P03-GATE` COMPLETE / ACCEPTED** · evidence: [`plans/P03-GATE-acceptance-evidence.md`](plans/P03-GATE-acceptance-evidence.md) · **R1 RESOLVED** (secure HttpOnly cookie; Bearer deferred)

Identity · Access · Party · Organization · Agency identity · Roles/permissions طبق معماری پذیرفته‌شده · پایهٔ authz ادمین · دسترسی Presentation آژانس.

منطق دامنه را در Admin کپی نکنید.

Invariant قفل‌شده: **Identity ≠ Party ≠ Access**.

### P03 completion gate

**TC-P03-GATE** = COMPLETE / ACCEPTED (`6a8a5ce`). P03 = COMPLETE. P04 = IN_PROGRESS (`TRAVELCORE_PHASE_CONFIRM: P04`).

## P04 — Reference Data + Destination

**Status:** COMPLETE / ACCEPTED (`f70991f`)

**Authoritative plan:** [`plans/P04-implementation-plan.md`](plans/P04-implementation-plan.md) (`TC-P04-PLAN`)

**Progress:** `TC-P04-T001`–`T011` COMPLETE / ACCEPTED · `TC-P04-GATE` COMPLETE / ACCEPTED · evidence [`plans/P04-GATE-acceptance-evidence.md`](plans/P04-GATE-acceptance-evidence.md) · R3 RESOLVED · P05 NOT_STARTED (needs `TRAVELCORE_PHASE_CONFIRM: P05`)

ReferenceData fundamentals.

سلسله‌مراتب Destination: Continent → Country → Province/State/Region → City → District → Neighborhood · انواع قابل گسترش.

ترجمه‌ها · hierarchy · geographic identity · localized slug hooks · Admin management · Public read model.

Destination گرهٔ مرکزی knowledge graph است.

Invariant: Destination مالک Hotel/Tour/Article/Booking نیست؛ ReferenceData ≠ Destination.

## P05 — SEO Engine

**Status:** COMPLETE / ACCEPTED

**Authoritative plan:** [`plans/P05-implementation-plan.md`](plans/P05-implementation-plan.md) (`TC-P05-PLAN`)

**Progress:** `TC-P05-PLAN` ACCEPTED · `TC-P05-T001`–`T012` COMPLETE / ACCEPTED · **`TC-P05-GATE` COMPLETE / ACCEPTED** · **`TC-P05-GATE-R1` COMPLETE / ACCEPTED** · **P05-R1/R2 RESOLVED** · P06 COMPLETE

**Evidence pack:** [`plans/P05-T012-evidence-pack.md`](plans/P05-T012-evidence-pack.md)  
**Gate evidence:** [`plans/P05-GATE-acceptance-evidence.md`](plans/P05-GATE-acceptance-evidence.md)  
**Gate baseline R1:** [`plans/P05-GATE-R1-baseline-reconciliation.md`](plans/P05-GATE-R1-baseline-reconciliation.md)

**Remediation evidence:** [`plans/P05-PLAN-R1-baseline-reconciliation.md`](plans/P05-PLAN-R1-baseline-reconciliation.md) · [`plans/P05-T003-R1-r1-decision-reconciliation.md`](plans/P05-T003-R1-r1-decision-reconciliation.md)

پس از وجود Destination واقعی برای یکپارچه‌سازی:

SeoRoute · localized route · slug history · Redirect · Canonical · hreflang · IndexPolicy · metadata · breadcrumbs · structured data framework · sitemap framework · robots · route publication rules.

اعتبارسنجی روی صفحات واقعی Destination.

---

## P06 — Media

**Status:** COMPLETE

**Authoritative plan:** [`plans/P06-implementation-plan.md`](plans/P06-implementation-plan.md) (`TC-P06-PLAN` — COMPLETE / ACCEPTED · `87069e4`)

**Progress:** USER `TRAVELCORE_PHASE_CONFIRM: P06` · plan ACCEPTED · `TC-P06-T001`–`T012` COMPLETE / ACCEPTED · **`TC-P06-GATE` COMPLETE / ACCEPTED** (`da345b5`) · **P06-R1 DEFER** · **P06-R2/R3/R4/R5/R6 RESOLVED** · **P06-R7 DEFERRED** · **P06-R8 UNRESOLVED** · **P06-R9 DEFERRED** · P07 **IN_PROGRESS**

Media Asset · object storage · upload · validation · variants · dimensions · focal point · alt/caption translations · optimization contract (same-format; WebP/AVIF DEFERRED) · app-proxy presentation · consumer reference proof (contract-only) · Admin Media baseline · hardening evidence pack.

Invariant: Media مالک بایت/متادیتای دارایی است؛ معنای رابطهٔ گالری (ترتیب/نقش) متعلق به ماژول مصرف‌کننده است.

**Optimization contract:** [`plans/P06-T008-optimization-contract-and-r1-defer.md`](plans/P06-T008-optimization-contract-and-r1-defer.md)

**Consumer reference proof:** [`plans/P06-T010-consumer-reference-contract-proof.md`](plans/P06-T010-consumer-reference-contract-proof.md)

**Evidence pack:** [`plans/P06-T012-hardening-and-evidence-pack.md`](plans/P06-T012-hardening-and-evidence-pack.md) (`8981312`)  
**Gate evidence:** [`plans/P06-GATE-acceptance-evidence.md`](plans/P06-GATE-acceptance-evidence.md)

---

## P07 — Place Catalog

**Status:** COMPLETE

**Authoritative plan:** [`plans/P07-implementation-plan.md`](plans/P07-implementation-plan.md) (`TC-P07-PLAN` — COMPLETE / ACCEPTED · `5dbc152`)

**Progress:** USER `TRAVELCORE_PHASE_CONFIRM: P07` · plan ACCEPTED · `TC-P07-T001`–`T008` COMPLETE / ACCEPTED · **`TC-P07-GATE` COMPLETE / ACCEPTED** (`84a0a48`) · **P07-R1 RESOLVED** · **P07-R2 RESOLVED** · **P07-R4 RESOLVED** (Place owns current Slug) · **P07-R5 RESOLVED** (default noindex,follow) · **P07-R3 UNRESOLVED** · P08 **NOT_STARTED** (needs `TRAVELCORE_PHASE_CONFIRM: P08`)

Place · Hotel · Restaurant · Attraction با localization، رابطه با Destination، geo، facilities، media، وضعیت عملیاتی، جزئیات عمومی، Admin، یکپارچگی SEO.

**زندهٔ Hotel Booking inventory را اینجا پیاده نکنید.** Hotel Catalog ≠ Hotel Booking.

**Evidence pack:** [`plans/P07-T008-hardening-and-evidence-pack.md`](plans/P07-T008-hardening-and-evidence-pack.md) (`f7843cc`)  
**Gate evidence:** [`plans/P07-GATE-acceptance-evidence.md`](plans/P07-GATE-acceptance-evidence.md)

---

## P08 — Content CMS

**Status:** COMPLETE

**Authoritative plan:** [`plans/P08-implementation-plan.md`](plans/P08-implementation-plan.md) (`TC-P08-PLAN` — COMPLETE / ACCEPTED · `7012fe0`)

**Progress:** USER `TRAVELCORE_PHASE_CONFIRM: P08` · plan ACCEPTED · T001–T009 ACCEPTED · **`TC-P08-GATE` COMPLETE / ACCEPTED** (`576b7fa`) · evidence [`plans/P08-GATE-acceptance-evidence.md`](plans/P08-GATE-acceptance-evidence.md) · R1–R5 RESOLVED · R6–R8 UNRESOLVED · **P09 COMPLETE**

**P08-R3 RESOLVED:** `ContentItemTranslation` owns localized current slug; SEO owns route binding, redirect history, canonical/history, publication SEO state; no global slug engine in Content.

**P08-R4 RESOLVED:** Default IndexPolicy = **noindex, follow**. Public route existence ≠ indexing. SEO owns final IndexPolicy. Content only exposes SEO hooks. Publication services do not set IndexPolicy.

**Evidence pack:** [`plans/P08-T009-hardening-and-evidence-pack.md`](plans/P08-T009-hardening-and-evidence-pack.md) (`2f9552f`)

Article · LandingPage · Category · Tag · Author · Content Blocks با پیوند معنادار به Destination.

بلوک‌های برنامه‌ریزی‌شده: heading · paragraph · image · gallery · FAQ · table · video · CTA · Tour/Hotel/Attraction widget.

جایی که ارزش واقعی دارد از محتوای ساخت‌یافته استفاده کنید.

Invariant: Content مالک editorial است؛ SEO محتوا را duplicate نمی‌کند.

---

## P09 — Tour Core

**Status:** COMPLETE

**Authoritative plan:** [`plans/P09-implementation-plan.md`](plans/P09-implementation-plan.md) (`TC-P09-PLAN` — COMPLETE / ACCEPTED · `7de2518`)

**Progress:** T001–T010 ACCEPTED · **`TC-P09-GATE` COMPLETE / ACCEPTED** (`67fc580`) · evidence [`plans/P09-GATE-acceptance-evidence.md`](plans/P09-GATE-acceptance-evidence.md) · **R1–R8 RESOLVED** · **P10 STARTED** (`TC-P10-PLAN` ACCEPTED · T001)

**Evidence pack:** [`plans/P09-T010-hardening-and-evidence-pack.md`](plans/P09-T010-hardening-and-evidence-pack.md) (`0334bae`)

مبانی مشترک Tour: TourProduct · Classification · Origin · Destinations · Agency references · Services · Policies · Requirements · Media · Publishing lifecycle · Translations · SEO integration.

ExperienceTour و PackageTour را به یک مجموعهٔ غول‌پیکر property nullable اجباری نکنید.

---

## P10 — Experience Tour

**Status:** IN_PROGRESS (T003)

**Authoritative plan:** [`plans/P10-implementation-plan.md`](plans/P10-implementation-plan.md) (`TC-P10-PLAN` ACCEPTED · T001–T005 ACCEPTED · T006 Guide)

**Progress:** T001 `e5490ae` · T002 `757c9b8` · T003 `85553b7` · T004 `7589ad1` · T005 `f7ce58c` · **P10-R1/R2/R3/R5/R6/R7 RESOLVED** · R4/R8 UNRESOLVED · `TC-P10-T006` Guide assignments

Itinerary · ItineraryDay · Stop · Destination/Attraction linking · Meals · Accommodation plan · Local transport · Equipment · Difficulty · Eligibility · Guide information.

اعتبارسنجی از طریق archetype صفحهٔ Experience Tour Detail.

P09 delivered shared TourProduct; P10 extends Tour with Experience specialization / itinerary structures (no new module scaffold).

---

## P11 — Foreign Package Tour

**Status:** COMPLETE (`TC-P11-GATE` ACCEPTED · P11-R1..R8 RESOLVED)

**Progress:** TourDeparture scaffolding · baseline `66cab9b` (PLAN) · prior Gate `c351bf9`

TourDeparture · TransportSegment · FlightSegment · Airports · Carrier · Flight number · Cabin/Class · Baggage · Local dates/times/timezones · TourHotelOption · Stay plan · MealPlan · Passenger rules · Occupancy · Age policies · Capacity · Travel requirements · Passport/visa rules.

اعتبارسنجی با UI پکیج خارجی که در P02 طراحی/اثبات شده.

**Invariant:** TourProduct ≠ TourDeparture · Pricing/Booking deferred (P12 / later).

---

## P12 — Pricing

**Status:** COMPLETE / ACCEPTED — **P12-R1…R8 RESOLVED** · T001–T009 ACCEPTED · **`TC-P12-GATE` ACCEPTED** (`b372367`)

Money · Currency · PriceComponent · TourRate · Mixed-currency rates · Passenger category · Occupancy · Age policy · Exchange rates · Conversion policy · Quote · Quote expiration در صورت نیاز · Price snapshot.

**Price ≠ Quote ≠ Payment.** هرگز همهٔ قیمت‌های تجاری را خاموش به یک ارز تبدیل نکنید.

**P12-R1:** Pricing = independent module (`pricing` schema). Tour owns tour facts; Pricing may logically reference TourDeparture `Guid` only — no Tour table ownership / no shared DbContext.

**P12-R2:** Reuse platform `TravelCore.Money` (ADR 0003). One authoritative currency per price value; no twin multi-currency SoR; FX/Quote/Payment conversion deferred.

**P12-R3:** Buyable/executable Price attaches conceptually to **TourDeparture** as the *initial* target. Pricing remains **generic**: it does **not** know TourDeparture types from Tour module. Polymorphic logical reference only: `TargetType` + `TargetId` (Guid). Example: TargetType=`TourDeparture`, TargetId=`uuid`. **No FK** · **No Booking** · **No Quote**. Product-level pricing DEFER (do not invent TourProduct pricing now).

**P12-R4:** Quote owned by Pricing · Quote is calculation snapshot · No Booking ownership · No Payment · No Customer/Passenger · No checkout flow.

**P12-R5:** **Pricing owns occupancy categories; Support tour market price types; No Booking passenger entity; No reservation calculation; No inventory.** Previous R5 wording around FX authority is deferred as **implementation of FX Service** (not invented in T007; T007 only records the request boundary).

**P12-R6:** **Admin Pricing is operational UI/API for Pricing. Ownership stays in Pricing module (Admin API + Admin UI). Not Tour Admin ownership.**

**P12-R7 RESOLVED:** Pricing keeps the price currency. Pricing does not convert currency. Exchange-rate ownership is not Pricing. Future FX Service owns ExchangeRate + Conversion; Pricing may only request conversion later. T007 records requested display-currency metadata / currency context only — no ExchangeRate table, no FX calculation, no Payment currency, no Settlement, no Booking.

**P12-R8 RESOLVED:** Pricing provides a public read-only query for price summary (currency, components, occupancy prices) by logical target (initial: TourDepartureId). No Booking, Payment, Checkout, Availability, Reservation, or FX conversion.

مثال طبیعی: `1290 USD` + مؤلفهٔ ارز محلی.

Plan: [`docs/plans/P12-implementation-plan.md`](plans/P12-implementation-plan.md)

**Evidence pack:** [`plans/P12-T009-hardening-and-evidence-pack.md`](plans/P12-T009-hardening-and-evidence-pack.md) (`a522dd5`)

**Gate evidence:** [`plans/P12-GATE-acceptance-evidence.md`](plans/P12-GATE-acceptance-evidence.md)

Gate ACCEPTED (`b372367`). Continuity auto-started **P13 PLAN** (Agency Marketplace). Ceremonial Gate wait is not a pipeline stop.

---

## P13 — Agency Marketplace

**Status:** COMPLETE — GATE ACCEPTED (`c0bcd78`) · **P13-R1–R7 RESOLVED** · T008 vacant [`plans/P13-implementation-plan.md`](plans/P13-implementation-plan.md)

Agency business profile · Offer ownership · Tour offering · Capacity/availability policies · Agency-specific commercial rules · Agency Panel · Publishing/moderation در صورت نیاز.

معنای Marketplace نباید TourProduct را بی‌ضرورت تکراری کند.

**P13-R1:** Agency Marketplace = independent module (`agency_marketplace` schema). Owns Agency commercial relationship. Party remains identity SoR; Marketplace is the commercial layer.

**P13-R2:** Party = identity SoR. Agency Marketplace owns AgencyProfile (0..1 per Agency PartyId). Logical PartyId only — no Party schema change.

**P13-R7:** Agency Marketplace owns Offer publication status. Not SEO. Not TourProduct catalog status. Draft → Submitted → Approved → Published; Rejected/Archived returns. Published Offer ≠ SEO Indexed.

---

## P14 — Public Tour Experience

**Status:** IN_PROGRESS — PLAN ACCEPTED · **P14-R1–R6 RESOLVED** · T006 content enrichment composition [`plans/P14-implementation-plan.md`](plans/P14-implementation-plan.md)

UX عمومی production برای: Tour Landing · Destination Tour Landing · Tour Listing/Search · Foreign Tour Detail · Experience Tour Detail · Filters · Sorting · Pagination · Mobile filters · Sticky/mobile booking actions · Related tours · تمایز SEO landing.

Search URL ≠ SEO Landing URL.

اعتبارسنجی: RTL · LTR · Mobile · Desktop · Accessibility · SEO · Performance.

---

## P15 — Search

**Status:** COMPLETE / ACCEPTED (`TC-P15-GATE` `4e2098d`) · **P15-R1–R7 RESOLVED** · T008 VACANT ([`docs/plans/P15-GATE-acceptance-evidence.md`](plans/P15-GATE-acceptance-evidence.md))

**P15-R7:** Engine-neutral `GET /api/search`. Structured filters · continuation-ready pagination · explicit locale. Not SEO IndexPolicy. Empty stub execution allowed.

**P15-R6:** Structured attributable locale-aware facts first. Semantic retrieval + provenance. No embeddings/vector/RAG/LLM. Search ≠ SoT.

**P15-R5:** Deterministic explainable signals + stable tie-break. Search owns ranking composition/ordering/metadata. Not business-policy authority. Ranking ≠ Recommendation. No ML/embeddings/personalization in T005.

**P15-R4:** Search owns Aggregation / Counting / Result composition. Domain owns attribute meaning + source facts. PE owns filter UI only. No facet engine / ES aggregations / domain facet tables in T004.

**P15-R3:** Transactional Outbox + Async Projection Worker. Search failure must not fail domain transaction. Projection retryable + idempotent. No RabbitMQ/real queue in T003.

**P15-R2:** Hybrid Read Model. Search owns `SearchDocument` + `ISearchIndex` abstraction. Domain modules remain SoT. No Elasticsearch/OpenSearch/SQL FTS in T002.

**P15-R1:** Search = independent Discovery Owner (`search` schema). Owns query/result contracts and future read models. Tour/Content/Pricing/AgencyMarketplace remain fact SoT. SEO remains IndexPolicy owner. T001: no projection tables / FTS / Elasticsearch / ranking / faceting.

پیاده‌سازی اولیه: PostgreSQL Full Text Search + `pg_trgm` (behind abstraction; no premature Elasticsearch Domain SoR).

Persian/Arabic normalization · half-space · typo tolerance · autocomplete · faceting · ranking · filter query · no-result behavior.

Search پشت abstraction بماند تا موتور اختصاصی آینده بدون بازنویسی Domain ممکن شود.

Ownership carry-forward: Tour = Fact · Content = Editorial · Pricing = Price · AgencyMarketplace = Offer · SEO = IndexPolicy · Search = Retrieval/Discovery · PublicExperience = Presentation.

---

## P16 — UGC

**Status:** COMPLETE / ACCEPTED — `TC-P16-GATE` `538f3fc` · **P16-R1–R8 RESOLVED** · T001–T009 ACCEPTED ([`docs/plans/P16-implementation-plan.md`](plans/P16-implementation-plan.md) · [`docs/plans/P16-GATE-acceptance-evidence.md`](plans/P16-GATE-acceptance-evidence.md))

Review · Rating · Rating dimensions · Travelogue · User Photo · Comment · Like در صورت تأیید · Report · Moderation · Publication state (Draft/Pending/Published/Rejected/Archived).

UGC باید به Destination / Place / Content وصل شود.

---

## P17 — Visa

**Status:** COMPLETE — PLAN ACCEPTED · **P17-R1–R8 RESOLVED** · T001–T009 ACCEPTED · Gate evidence ([`docs/plans/P17-GATE-acceptance-evidence.md`](plans/P17-GATE-acceptance-evidence.md))

Visa catalog · رابطه با Destination/کشور · نوع ویزا · requirements · documents · processing · pricing · content · forms/workflow در صورت نیاز · SEO landing.

ROADMAP bullets above are **capability themes**, not ownership transfers. Visa remains the visa-domain owner; Pricing / Content / SEO / Search / Booking stay their own modules until an architect R# lock says otherwise.

---

## P18 — Trip Planner / Lead Experience

**Status:** COMPLETE — PLAN ACCEPTED · **P18-R1–R8 RESOLVED** · T001–T009 ACCEPTED · Gate evidence ([`docs/plans/P18-GATE-acceptance-evidence.md`](plans/P18-GATE-acceptance-evidence.md))

مفهوم Travel Planner / «سفرساز»: کمک به بیان نیاز سفر و کشف/درخواست محصول مناسب.

**TripIntent ≠ Lead ≠ Booking** · **TripPlanner ≠ Search** · **Lead Experience ≠ CRM by default** · **BudgetPreference ≠ Price/Quote**.

گردش‌کار دقیق قبل از پیاده‌سازی مشخص شود — PLAN only until architect ACCEPT + R1 lock.

---

## P19 — Tour Booking

**Status:** COMPLETE (`TC-P19-GATE` evidence [`docs/plans/P19-GATE-acceptance-evidence.md`](plans/P19-GATE-acceptance-evidence.md) · **P19-R1–R8 RESOLVED** · T001–T009 ACCEPTED) — [`docs/plans/P19-implementation-plan.md`](plans/P19-implementation-plan.md)

traveler information · availability validation · Quote acceptance · reservation · Booking · status · price snapshot · cancellation foundation · confirmation.

قیمت‌های تاریخی پذیرفته‌شده با تغییر قیمت زنده/FX عوض نشوند.

---

## P20 — Payment

**Status:** COMPLETE — GATE ACCEPTED · **P20-R1–R8 = RESOLVED** · T001–T009 ACCEPTED [`docs/plans/P20-GATE-acceptance-evidence.md`](plans/P20-GATE-acceptance-evidence.md)

Payment · attempts · provider abstraction · callback/webhook validation · success/failure lifecycle · refund foundation · payment snapshots · financial auditability.

Payment را با Price یا Quote ادغام نکنید.

---

## P21 — Hotel Booking

**Status:** COMPLETE (`TC-P21-GATE` ACCEPTED `858b4be` / docs `d6bd842`) · **P21-R1–R8 RESOLVED** · T001–T009 ACCEPTED · PayAtProperty DEFERRED · deposit/partial DEFERRED · Partial Refund DEFERRED · amendments/rebooking DEFERRED · Named Hotel Supplier NONE · Production Payment Provider NONE

P21-R8 lock: public HotelBooking is a transactional journey, not CRUD; anonymous access uses a HotelBooking-specific opaque token (`X-TravelCore-Hotel-Booking-Access-Token`) independent of Tour Booking; raw token is returned once and only the SHA-256 verifier is persisted; raw token never enters the URL; HotelBookingId/PaymentId/SupplierReservationId are not credentials; authenticated callers still need object-level authorization; initiation is DB-idempotent; customer amount/currency/success are never authoritative; production hotel sources and Payment provider remain NONE; zero sources is valid and must not fabricate availability/rate/reservation/redirect; HotelBooking Payment is HotelBooking-scoped; Payment Succeeded ≠ HotelBooking Confirmed; private transactional pages are noindex; no card collection; confirmed cancellation uses R7 only; partial-penalty cancellation stays blocked; operational reads are read-only; no smart routing/failover.

P21-R7 lock: customer cancellation targets Confirmed HotelBooking; HotelBookingCancellation is separate process state; economics from immutable HotelCancellationPolicySnapshot at RequestedAt Instant; PenaltyAmount = 0 => full Refund; PenaltyAmount = TotalAmount => no Refund; partial penalty requires Partial Refund and is not executable; partial-refund-required cancellation is rejected before supplier cancellation; supplier cancellation is authoritative, durable, idempotent, and ambiguity-aware; network timeout is not failure or success; HotelBooking remains Confirmed until supplier cancellation is authoritative; authoritative supplier cancellation performs Confirmed → Cancelled; full Refund is requested only after that; HotelBookingCancelled != RefundSucceeded; Payment owns Refund and remains Succeeded; no-refund completes without Refund; full-refund completes after RefundSucceeded; no distributed transaction.

جدا از Place Hotel Catalog: provider abstraction · mapping · search · availability · rooms · rates · cancellation rules · Quote · reservation · booking · voucher · provider sync.

---

## P22 — Flight

**Status:** COMPLETE (`TC-P22-GATE` ACCEPTED `2a372ae` / docs `ed040f0` · evidence [`docs/plans/P22-GATE-acceptance-evidence.md`](plans/P22-GATE-acceptance-evidence.md) · **P22-R1–R8 RESOLVED** · T001–T009 ACCEPTED) · Partial Refund DEFERRED · MultiCity DEFERRED · ancillaries DEFERRED · PayLater/deposit/partial DEFERRED · amendments/rebooking DEFERRED · Named Flight Supplier NONE · Production Flight Search/Availability/Offer/Reservation/Ticketing/Cancellation Source NONE · Production Payment Provider NONE

provider abstraction · airport/reference · one-way · round-trip · multi-city در صورت تأیید · search · fare · baggage · passenger rules · Quote · booking/order · provider references.

Inventory پرواز نباید سخت به یک provider قفل شود.

---

## P23 — Dynamic Package / Flight + Hotel

**Status:** COMPLETE (`TC-P23-GATE` re-execution · **P23-R1–R8 RESOLVED** · T001–T009 ACCEPTED) · evidence [`plans/P23-GATE-acceptance-evidence.md`](plans/P23-GATE-acceptance-evidence.md)

پس از پایدار شدن HotelBooking و Flight: خرید ترکیبی مثل Flight + Hotel.

قبل از وجود قابلیت‌های زیرساختی رزرو، پیاده نشود. P21 و P22 reservation infrastructures now exist; product implementation waits for PLAN ACCEPT and P23-R1 lock. Do not treat OPEN recommendations as resolved.

---

## P24 — B2B / Agency Commerce

**Status:** COMPLETE (`TC-P24-GATE` implemented · **P24-R1–R8 RESOLVED**)

Agency access · B2B contracts · partner pricing · partner booking · credit/commercial rules در صورت تأیید.

Concernهای B2B را از UX مصرف‌کنندهٔ عمومی جدا نگه دارید.

---

## P25 — Notification

**Status:** COMPLETE (`TC-P25-GATE` implemented · P25-R1–R8 RESOLVED · boundary-only foundation delivered)

Email · SMS · In-app · احتمالاً push/webhook بعداً · Preferences · transactional notifications · «خبرم کن» در صورت نیاز محصول · provider abstraction.

---

## P26 — Advanced SEO + Content Graph

**Status:** PLANNED (`TC-P26-PLAN` authored · product NOT_STARTED)

پس از وجود محتوای/موجودی معنادار: Destination hubs · content clusters · internal link graph · programmatic landings · route quality · orphan detection · indexation quality · sitemap scaling · structured data completeness · SEO landing factory.

Programmatic SEO فقط با inventory/value · unique purpose · content quality · internal linking · search intent مفید. URL نازک انبوه تولید نکنید.

**توجه:** SEO تا P26 صبر نمی‌کند — پایین را ببینید.

---

## P27 — Analytics + Product Intelligence

**Status:** PLANNED

رویدادهایی مانند SearchPerformed · SearchResultClicked · SearchNoResults · FilterApplied · TourViewed · HotelViewed · QuoteCreated · BookingStarted · BookingCompleted.

Analytics پشت abstraction معقول؛ فراخوانی provider-specific در Domain پراکنده نشود.

---

## P28 — Performance & Scale

**Status:** PLANNED

قبل از بهینه‌سازی، profile کنید.

PostgreSQL queries/indexes · Dapper projections در صورت توجیه · Redis caching/invalidation · CDN · image optimization · Next.js rendering · bundle size · third-party scripts · Core Web Vitals · search performance · load testing.

پیچیدگی توزیع‌شده بدون نیاز اندازه‌گیری‌شده معرفی نشود.

---

## P29 — Production Hardening

**Status:** PLANNED

security/authorization review · rate limiting · audit · content sanitization · file security · backup/restore · DR · health · observability · metrics · tracing · error monitoring · load tests · DB recovery · deployment · CI/CD · environment config · secret management · production SEO/mobile/a11y verification · operational runbooks.

---

## Post-P29 — Continuous Evolution

**Status:** PLANNED

پس از production، کار با متریک واقعی محصول ادامه می‌یابد.

تحول بالقوهٔ بعدی (بدون تعهد از قبل): موتور Search اختصاصی · provider بیشتر · personalization · recommendation · loyalty · promotions · pricing پیشرفته · اپ موبایل · تجزیهٔ یک ماژول **فقط** با شواهد مقیاس/تیم/عملیات.

به microservices پیش‌تعهد نکنید. هر تحول معماری عمده نیاز به ADR دارد.

---

## UI Validation Sequence

ترتیب اعتبارسنجی UI:

1. Foundation primitives
2. Foreign Package Tour Detail
3. Experience Tour Detail
4. Tour Listing/Search
5. Destination Landing
6. Hotel Detail
7. Home / Discovery
8. Content Article
9. Travelogue
10. Visa
11. Booking/Checkout
12. Flight Search
13. Hotel Booking Search
14. Admin surfaces
15. Agency surfaces

**Foreign Tour Detail** عمداً زود اعتبارسنجی می‌شود چون فشار می‌آورد روی: RTL/LTR · bidi · داده پرواز · قیمت · ارزهای ترکیبی · گزینه هتل · layout موبایل · پیچیدگی responsive · SEO metadata.

سپس **Experience Tour** اعتبارسنجی می‌کند: timeline · روزهای itinerary · نقشه · توقف‌ها · اطلاعات editorial ساخت‌یافته.

---

## SEO Validation Sequence

ترتیب اعتبارسنجی SEO:

1. URL/locale constitution
2. Destination entity
3. SeoRoute
4. Localized slugs
5. canonical
6. hreflang
7. redirects
8. sitemap
9. structured data
10. internal linking
11. Tour landing pages
12. Place pages
13. Content pages
14. controlled Programmatic SEO
15. Search Console / production validation later

**صریح:** SEO تا P26 صبر نمی‌کند.

P26 = Advanced SEO.

مبانی SEO از P00 آغاز می‌شود و از P05 به بعد پیاده می‌شود.

---

## Phase Index (Quick)

| Phase | Title | Status |
|-------|-------|--------|
| P00 | Architecture Foundation | COMPLETE |
| P01 | Platform / Backend Foundation | COMPLETE |
| P02 | Frontend Foundation + Walking Skeleton | COMPLETE |
| P03 | Identity + Access + Party | COMPLETE |
| P04 | Reference Data + Destination | COMPLETE |
| P05 | SEO Engine | COMPLETE |
| P06 | Media | **COMPLETE** (`TC-P06-GATE` ACCEPTED) |
| P07 | Place Catalog | **COMPLETE** (`TC-P07-GATE` ACCEPTED) |
| P08 | Content CMS | **COMPLETE** (`TC-P08-GATE` `576b7fa` · R6–R8 UNRESOLVED) |
| P09 | Tour Core | **COMPLETE** (`TC-P09-GATE` `67fc580` · R1–R8 RESOLVED) |
| P10 | Experience Tour | **COMPLETE** (Gate `c351bf9` · R1–R8 RESOLVED) |
| P11 | Foreign Package Tour | **COMPLETE** (`TC-P11-GATE` ACCEPTED) |
| P12 | Pricing | **COMPLETE** (`TC-P12-GATE` ACCEPTED `b372367` · R1–R8 RESOLVED) |
| P13 | Agency Marketplace | **COMPLETE** (`TC-P13-GATE` `c0bcd78` · R1–R7 RESOLVED) |
| P14 | Public Tour Experience | **COMPLETE** (`TC-P14-GATE` ACCEPTED `608216d` · R1–R8 RESOLVED) |
| P15 | Search | **COMPLETE** (`TC-P15-GATE` ACCEPTED `4e2098d` · R1–R7 RESOLVED · T008 VACANT) |
| P16 | UGC | **COMPLETE** (`TC-P16-GATE` ACCEPTED `538f3fc` · R1–R8 RESOLVED · T001–T009 ACCEPTED) |
| P17 | Visa | **COMPLETE** (R1–R8 RESOLVED · GATE) |
| P18 | Trip Planner / Lead Experience | **COMPLETE** (`TC-P18-GATE` ACCEPTED `73605aa` · R1–R8 RESOLVED) |
| P19 | Tour Booking | **COMPLETE** (`TC-P19-GATE` ACCEPTED `d258933` · R1–R8 RESOLVED · T001–T009 ACCEPTED) |
| P20 | Payment | **COMPLETE** (`TC-P20-GATE` ACCEPTED · R1–R8 RESOLVED · T001–T009 ACCEPTED) |
| P21 | Hotel Booking | **COMPLETE** (`TC-P21-GATE` ACCEPTED `858b4be` / docs `d6bd842` · R1–R8 RESOLVED · T001–T009 ACCEPTED) |
| P22 | Flight | **COMPLETE** (`TC-P22-GATE` ACCEPTED `2a372ae` / docs `ed040f0` · R1–R8 RESOLVED · T001–T009 ACCEPTED) |
| P23 | Dynamic Package / Flight + Hotel | **COMPLETE** (GATE re-execution · R1–R8 RESOLVED · T001–T009 ACCEPTED) |
| P24 | B2B / Agency Commerce | COMPLETE (`TC-P24-GATE` implemented · P24-R1–R8 RESOLVED) |
| P25 | Notification | COMPLETE (GATE evidence executed) |
| P26 | Advanced SEO + Content Graph | PLANNED (PLAN authored) |
| P27 | Analytics + Product Intelligence | PLANNED |
| P28 | Performance & Scale | PLANNED |
| P29 | Production Hardening | PLANNED |
| Post-P29 | Continuous Evolution | PLANNED |
