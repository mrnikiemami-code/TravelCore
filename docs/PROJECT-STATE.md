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
| Current Phase | P00 — Architecture Foundation |
| Last Accepted Task | TC-P00-T005 |
| Last Accepted Commit | `66e6f32` |
| Accepted Architecture Commit | `66e6f32` |
| Repository Normalization | TC-P00-T003R — PASS / ACCEPTED |
| Repository Normalization Commit | `840c3e5` |
| Emergency ChatGPT Recovery Drill | PASS |
| Repository Bootstrap | COMPLETE |
| Architecture Brain | COMPLETE |
| Master Execution Roadmap | [`docs/ROADMAP.md`](ROADMAP.md) |
| Emergency ChatGPT Recovery | [`docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md`](prompts/START-HERE-IF-CHATGPT-IS-LOST.md) |
| Current Active Task | **TC-P00-T006** — SEO Constitution |
| Task State | AWAITING_ARCHITECT_REVIEW |
| Current Next Task | **TC-P00-T006** — SEO Constitution |

Recovery Drill note: recovery prompt successfully reconstructed current phase, accepted/pending task state, ADR statuses, and clean Git state without modifying the repository.

---

## Completed Tasks

| Task | خلاصه | نتیجه | Commit مرتبط |
|------|--------|--------|----------------|
| TC-P00-T000A | Backend bootstrap (.NET 10 Minimal API) | PASS | بخشی از `cf97f35` |
| TC-P00-T000B | Frontend/repository bootstrap (Next.js monorepo) | Local PASS؛ مشکل remote بعداً حل شد | `cf97f35` |
| TC-P00-T000C | GitHub auth / private repo / push sync | PASS | همان پایه روی `origin/main` |
| TC-P00-T001 | Architecture Brain & Constitution | PASS | `834e0c5` |
| TC-P00-T001A | Project continuity / recovery state | PASS | `110c748` |
| TC-P00-T002 | Domain Map + Module Boundaries + Ownership Matrix + Dependency Rules | ACCEPTED / COMPLETE | `08343e7` |
| TC-P00-T003 | Data Architecture | ACCEPTED / COMPLETE | `3904bb9` |
| TC-P00-T003R | Normalize canonical GitHub repository identity | PASS / ACCEPTED | `840c3e5` |
| TC-P00-T004 | UI Constitution | ACCEPTED / COMPLETE | `48e0472` |
| TC-P00-T005 | Internationalization Architecture | ACCEPTED / COMPLETE | `66e6f32` |

Bootstrap commit اولیهٔ فنی:

`cf97f35 chore: bootstrap TravelCore repository`

---

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
