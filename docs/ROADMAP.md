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
| Current Phase | **P02 — Frontend Foundation + Walking Skeleton** |
| Phase Status | P00 COMPLETE · P01 COMPLETE · P02 IN_PROGRESS · P03 NOT_STARTED |
| Last Accepted P00 Task | TC-P00-T008 |
| Accepted Architecture Commit (T008) | `1bd4e95` |
| Acceptance / State Commit (T008A) | `0074437` |
| Last Accepted Commit | `0853d04` (`TC-P01-GATE`) |
| P00 Final Gate | TC-P00-GATE — PASS |
| P00 Closure | TC-P00-CLOSE |
| Current Next Task | Architect review of `TC-P02-T001`; then `TC-P02-T002` when issued |
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
- **P02** = IN_PROGRESS (`TC-P02-PLAN` ACCEPTED; `TC-P02-T001` awaiting architect review)
- **P03–P29 و Post-P29** = PLANNED

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

TC-P00-GATE = PASS. P00 = COMPLETE. P01 COMPLETE (`TC-P01-GATE` ACCEPTED `0853d04`). P02 IN_PROGRESS — authoritative plan: [`plans/P02-frontend-foundation-walking-skeleton.md`](plans/P02-frontend-foundation-walking-skeleton.md). P03 remains NOT_STARTED.

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

**Status:** IN_PROGRESS

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

معماری UI در RTL و LTR و mobile/desktop توسط معمار تأیید شود قبل از پیاده‌سازی عمدهٔ صفحات فیچر.

---

## P03 — Identity + Access + Party

**Status:** PLANNED

Identity · Access · Party · Organization · Agency identity · Roles/permissions طبق معماری پذیرفته‌شده · پایهٔ authz ادمین · دسترسی Presentation آژانس.

منطق دامنه را در Admin کپی نکنید.

---

## P04 — Reference Data + Destination

**Status:** PLANNED

ReferenceData fundamentals.

سلسله‌مراتب Destination: Continent → Country → Province/State/Region → City → District → Neighborhood · انواع قابل گسترش.

ترجمه‌ها · hierarchy · geographic identity · localized slug hooks · Admin management · Public read model.

Destination گرهٔ مرکزی knowledge graph است.

---

## P05 — SEO Engine

**Status:** PLANNED

پس از وجود Destination واقعی برای یکپارچه‌سازی:

SeoRoute · localized route · slug history · Redirect · Canonical · hreflang · IndexPolicy · metadata · breadcrumbs · structured data framework · sitemap framework · robots · route publication rules.

اعتبارسنجی روی صفحات واقعی Destination.

---

## P06 — Media

**Status:** PLANNED

Media Asset · object storage · upload · validation · variants · dimensions · focal point · alt/caption translations · WebP/AVIF pipeline در صورت تأیید · قرارداد بهینه‌سازی تصویر.

---

## P07 — Place Catalog

**Status:** PLANNED

Place · Hotel · Restaurant · Attraction با localization، رابطه با Destination، geo، facilities، media، وضعیت عملیاتی، جزئیات عمومی، Admin، یکپارچگی SEO.

**زندهٔ Hotel Booking inventory را اینجا پیاده نکنید.** Hotel Catalog ≠ Hotel Booking.

---

## P08 — Content CMS

**Status:** PLANNED

Article · LandingPage · Category · Tag · Author · Content Blocks با پیوند معنادار به Destination.

بلوک‌های برنامه‌ریزی‌شده: heading · paragraph · image · gallery · FAQ · table · video · CTA · Tour/Hotel/Attraction widget.

جایی که ارزش واقعی دارد از محتوای ساخت‌یافته استفاده کنید.

---

## P09 — Tour Core

**Status:** PLANNED

مبانی مشترک Tour: TourProduct · Classification · Origin · Destinations · Agency references · Services · Policies · Requirements · Media · Publishing lifecycle · Translations · SEO integration.

ExperienceTour و PackageTour را به یک مجموعهٔ غول‌پیکر property nullable اجباری نکنید.

---

## P10 — Experience Tour

**Status:** PLANNED

Itinerary · ItineraryDay · Stop · Destination/Attraction linking · Meals · Accommodation plan · Local transport · Equipment · Difficulty · Eligibility · Guide information.

اعتبارسنجی از طریق archetype صفحهٔ Experience Tour Detail.

---

## P11 — Foreign Package Tour

**Status:** PLANNED

TourDeparture · TransportSegment · FlightSegment · Airports · Carrier · Flight number · Cabin/Class · Baggage · Local dates/times/timezones · TourHotelOption · Stay plan · MealPlan · Passenger rules · Occupancy · Age policies · Capacity · Travel requirements · Passport/visa rules.

اعتبارسنجی با UI پکیج خارجی که در P02 طراحی/اثبات شده.

---

## P12 — Pricing

**Status:** PLANNED

Money · Currency · PriceComponent · TourRate · Mixed-currency rates · Passenger category · Occupancy · Age policy · Exchange rates · Conversion policy · Quote · Quote expiration در صورت نیاز · Price snapshot.

**Price ≠ Quote ≠ Payment.** هرگز همهٔ قیمت‌های تجاری را خاموش به یک ارز تبدیل نکنید.

مثال طبیعی: `1290 USD` + مؤلفهٔ ارز محلی.

---

## P13 — Agency Marketplace

**Status:** PLANNED

Agency business profile · Offer ownership · Tour offering · Capacity/availability policies · Agency-specific commercial rules · Agency Panel · Publishing/moderation در صورت نیاز.

معنای Marketplace نباید TourProduct را بی‌ضرورت تکراری کند.

---

## P14 — Public Tour Experience

**Status:** PLANNED

UX عمومی production برای: Tour Landing · Destination Tour Landing · Tour Listing/Search · Foreign Tour Detail · Experience Tour Detail · Filters · Sorting · Pagination · Mobile filters · Sticky/mobile booking actions · Related tours · تمایز SEO landing.

Search URL ≠ SEO Landing URL.

اعتبارسنجی: RTL · LTR · Mobile · Desktop · Accessibility · SEO · Performance.

---

## P15 — Search

**Status:** PLANNED

پیاده‌سازی اولیه: PostgreSQL Full Text Search + `pg_trgm`.

Persian/Arabic normalization · half-space · typo tolerance · autocomplete · faceting · ranking · filter query · no-result behavior.

Search پشت abstraction بماند تا موتور اختصاصی آینده بدون بازنویسی Domain ممکن شود.

---

## P16 — UGC

**Status:** PLANNED

Review · Rating · Rating dimensions · Travelogue · User Photo · Comment · Like در صورت تأیید · Report · Moderation · Publication state (Draft/Pending/Published/Rejected/Archived).

UGC باید به Destination / Place / Content وصل شود.

---

## P17 — Visa

**Status:** PLANNED

Visa catalog · رابطه با Destination/کشور · نوع ویزا · requirements · documents · processing · pricing · content · forms/workflow در صورت نیاز · SEO landing.

---

## P18 — Trip Planner / Lead Experience

**Status:** PLANNED

مفهوم Travel Planner / «سفرساز»: کمک به بیان نیاز سفر و کشف/درخواست محصول مناسب.

گردش‌کار دقیق قبل از پیاده‌سازی مشخص شود.

---

## P19 — Tour Booking

**Status:** PLANNED

traveler information · availability validation · Quote acceptance · reservation · Booking · status · price snapshot · cancellation foundation · confirmation.

قیمت‌های تاریخی پذیرفته‌شده با تغییر قیمت زنده/FX عوض نشوند.

---

## P20 — Payment

**Status:** PLANNED

Payment · attempts · provider abstraction · callback/webhook validation · success/failure lifecycle · refund foundation · payment snapshots · financial auditability.

Payment را با Price یا Quote ادغام نکنید.

---

## P21 — Hotel Booking

**Status:** PLANNED

جدا از Place Hotel Catalog: provider abstraction · mapping · search · availability · rooms · rates · cancellation rules · Quote · reservation · booking · voucher · provider sync.

---

## P22 — Flight

**Status:** PLANNED

provider abstraction · airport/reference · one-way · round-trip · multi-city در صورت تأیید · search · fare · baggage · passenger rules · Quote · booking/order · provider references.

Inventory پرواز نباید سخت به یک provider قفل شود.

---

## P23 — Dynamic Package / Flight + Hotel

**Status:** PLANNED

پس از پایدار شدن HotelBooking و Flight: خرید ترکیبی مثل Flight + Hotel.

قبل از وجود قابلیت‌های زیرساختی رزرو، پیاده نشود.

---

## P24 — B2B / Agency Commerce

**Status:** PLANNED

Agency access · B2B contracts · partner pricing · partner booking · credit/commercial rules در صورت تأیید.

Concernهای B2B را از UX مصرف‌کنندهٔ عمومی جدا نگه دارید.

---

## P25 — Notification

**Status:** PLANNED

Email · SMS · In-app · احتمالاً push/webhook بعداً · Preferences · transactional notifications · «خبرم کن» در صورت نیاز محصول · provider abstraction.

---

## P26 — Advanced SEO + Content Graph

**Status:** PLANNED

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
| P02 | Frontend Foundation + Walking Skeleton | IN_PROGRESS |
| P03 | Identity + Access + Party | PLANNED |
| P04 | Reference Data + Destination | PLANNED |
| P05 | SEO Engine | PLANNED |
| P06 | Media | PLANNED |
| P07 | Place Catalog | PLANNED |
| P08 | Content CMS | PLANNED |
| P09 | Tour Core | PLANNED |
| P10 | Experience Tour | PLANNED |
| P11 | Foreign Package Tour | PLANNED |
| P12 | Pricing | PLANNED |
| P13 | Agency Marketplace | PLANNED |
| P14 | Public Tour Experience | PLANNED |
| P15 | Search | PLANNED |
| P16 | UGC | PLANNED |
| P17 | Visa | PLANNED |
| P18 | Trip Planner / Lead Experience | PLANNED |
| P19 | Tour Booking | PLANNED |
| P20 | Payment | PLANNED |
| P21 | Hotel Booking | PLANNED |
| P22 | Flight | PLANNED |
| P23 | Dynamic Package / Flight + Hotel | PLANNED |
| P24 | B2B / Agency Commerce | PLANNED |
| P25 | Notification | PLANNED |
| P26 | Advanced SEO + Content Graph | PLANNED |
| P27 | Analytics + Product Intelligence | PLANNED |
| P28 | Performance & Scale | PLANNED |
| P29 | Production Hardening | PLANNED |
| Post-P29 | Continuous Evolution | PLANNED |
