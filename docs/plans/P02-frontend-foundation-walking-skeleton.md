# P02 — Frontend Foundation + Walking Skeleton — Execution Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P02-PLAN` |
| Phase | P02 |
| Status | COMPLETE / ACCEPTED (`TC-P02-GATE`) |
| Baseline | `0853d04` |
| Authoritative sources | `docs/ROADMAP.md` · `docs/architecture/10-ui-constitution.md` · `docs/ui/*` · `docs/pages/01-foreign-tour-detail.md` · ADR 0005 / 0006 / 0007 |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P02** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند. نقشهٔ فعلی: T001–T017 + GATE (پس از revision معمار).

---

## 1. Phase objective

قبل از ساخت ده‌ها صفحهٔ فیچر، TravelCore باید ثابت کند که:

1. اسکلت Next.js App Router (Server Component first) قابل اتکاست.
2. RTL/LTR، bidi، mobile-first، a11y و SEO-sensitive rendering از روز اول درست‌اند.
3. یک **Walking Skeleton** واقعی برای `ForeignTourDetailPage` با fixtureهای typed، قرارداد archetype را در FA/EN × Desktop/Mobile اعتبارسنجی می‌کند.

P02 **Tour domain کامل**، Booking، Payment، Search، CMS، یا P03+ نیست.

---

## 2. Current frontend evidence (baseline)

| Item | Evidence |
|------|----------|
| Stack | Next.js 16.3 · React 19 · TypeScript · Tailwind 4 · ESLint |
| Location | `src/frontend/web` |
| App state | create-next-app scaffold (`layout.tsx` `lang="en"` ثابت؛ بدون locale routing) |
| Scripts | `dev` · `build` · `start` · `lint` — بدون `typecheck` اختصاصی و بدون test runner |
| Backend coupling | هیچ client API محصولی در frontend نیست |

---

## 3. Architectural constraints (locked)

- Next.js App Router · TypeScript · Tailwind
- Server Components first (ADR 0005); Client فقط برای تعامل واقعی مرورگر
- Locale-prefixed public routes `/fa/...` · `/en/...` · `/ar/...` (ADR 0007); URL locale authoritative
- Direction-neutral UI + bidi (ADR 0006); logical CSS; نه `body { direction: rtl; }` به‌تنهایی
- Mobile-first؛ sticky CTA موبایل وقتی archetype الزام می‌کند
- SEO-first: محتوای تصمیم‌ساز server-renderable
- بدون منطق کسب‌وکار authoritative در UI
- بدون وابستگی مستقیم frontend به DB / EF / module DbContext
- Money: decimal semantics؛ تمایز IRR/Toman؛ بدون جمع خاموش ارزهای مختلط؛ `MixedCurrencyPrice`
- TourProduct ≠ TourDeparture؛ Hotel catalog ≠ package hotel option
- بدون micro-frontends / repo جدا / redesign archetype پذیرفته‌شده

---

## 4. Cross-domain workflow-driven UX (locked for P02)

**Durable rule (also in UI constitution):**

> Domain boundaries protect architecture; they do not automatically define screen, form, menu, navigation, or workflow boundaries. UI is designed around user goals and may coordinate multiple domains through explicit application/API contracts while preserving domain ownership.

- Frontend **MUST NOT** mechanically mirror backend bounded contexts as screens/menus.
- Backend ownership remains separate even when one guided UI flow spans multiple domains.

### Explicit valid pattern — Identity + Party

Identity and Party remain separate domains. A user workflow MAY:

1. create Identity
2. create or link Party within the same guided flow
3. establish the relationship
4. continue to relevant profile/access steps

without forcing disconnected CRUD screens, raw ID copy/paste, or exposing internal foreign keys.

### Mobile-first workflow requirements (for T010+)

Cross-domain workflows must explicitly require: minimum typing · minimum context switching · inline create/link/select where useful · touch-friendly interaction · progressive disclosure · preserved form/workflow state · no raw IDs · no desktop-only critical path.

### Admin navigation freeze

Admin shell layout primitives (T008) must not freeze domain-mirroring navigation. Final Admin IA is planned only after `TC-P02-T010`.

---

## 5. Validation strategy (repository-supported)

برای Taskهای پیاده‌سازی P02، مگر خلاف صریح در Task:

| Gate | Tooling |
|------|---------|
| Lint | `npm run lint` در `src/frontend/web` |
| Typecheck | `npx tsc --noEmit` (یا script اضافه‌شده در همان Task کیفیت) |
| Production build | `npm run build` |
| Diff hygiene | `git diff --check` |
| Server/Client boundary | بررسی `"use client"` فقط روی islands تعاملی؛ page/root layout بدون Client اجباری |
| a11y | چک‌لیست `docs/ui/05-accessibility-and-interaction.md` + landmark/heading/focus؛ ابزار خودکار فقط اگر در Task کیفیت توجیه و اضافه شود |
| RTL/LTR | FA (`dir=rtl`) و EN (`dir=ltr`) روی همان archetype |
| Responsive | نماینده‌ها: 360 · 390 · 768 · 1024 · 1280 · 1440 (حداقل ماتریس GATE: FA/EN × Mobile/Desktop) |
| Walking skeleton | رندر بدون hydration/runtime error؛ fixture typed؛ بدون دادهٔ ساختگی گمراه‌کننده قیمت |

**Test runner جدید در PLAN اجباری نیست.** اگر Task کیفیت نیاز به unit/component test ثابت کند، فقط با توجیه صریح و حداقل پکیج هم‌راستا با baseline اضافه می‌شود. E2E کامل خارج از P02 است مگر smoke دستی/مستندسازی شواهد.

---

## 6. Out of scope for entire P02

- پیاده‌سازی کامل ماژول Tour / Booking / Payment / HotelBooking / Flight inventory / Search
- Authentication/Authorization redesign
- CMS · UGC کامل · Visa flows
- Design system فراتر از نیاز Walking Skeleton
- OpenAPI codegen اجباری (ارزیابی جدا؛ skeleton با typed fixture پیش می‌رود)
- P03+ · micro-frontends · repo frontend جدا

---

## 7. Ordered task map

### TC-P02-T001 — Frontend physical structure & conventions

| Field | Content |
|-------|---------|
| Objective | ساختار پوشه و قرارداد مالکیت frontend را با معماری UI هم‌تراز کند. |
| Exact scope | README/convention در `src/frontend/web`؛ پوشه‌های خالی/اسناد برای `components` · `lib` · `features` یا معادل پذیرفته‌شده؛ هم‌ترازی با monorepo docs در صورت نیاز. |
| Allowed | docs + ساختار پوشهٔ frontend بدون فیچر صفحه. |
| Forbidden | صفحهٔ تور · پکیج جدید · تغییر backend. |
| Dependencies | — |
| Acceptance | ساختار مستند و قابل‌پیمایش؛ بدون scaffold تصادفی جدید خارج از قرارداد. |
| Proofs | `npm run lint` · `npm run build` (اگر کد لمس شود) · `git diff --check` |
| Artifacts | README/convention + folder skeleton |
| Stop | redesign به micro-frontend یا جدا کردن appهای متعدد بدون ADR |

---

### TC-P02-T002 — Locale-aware App Router root (`lang` / `dir`)

| Field | Content |
|-------|---------|
| Objective | مسیرهای عمومی locale-prefixed و root `html lang/dir` را طبق ADR 0007 / i18n قفل کند. |
| Exact scope | `app/[locale]/...` (یا الگوی معادل App Router)؛ پشتیبانی `fa`/`en` (و scaffold `ar` در registry بدون الزام ماتریس کامل)؛ تنظیم `lang`/`dir` از URL locale؛ منع silent Accept-Language override. |
| Allowed | routing/layout locale؛ metadata پایهٔ locale. |
| Forbidden | SEO engine کامل (P05) · CMS · محتوای editorial واقعی. |
| Dependencies | T001 |
| Acceptance | `/fa` و `/en` `lang`/`dir` درست دارند؛ locale از URL authoritative است. |
| Proofs | build · دستی/مستند FA RTL + EN LTR · lint/tsc |
| Artifacts | locale layout(s) · locale registry helper |
| Stop | یک URL واحد برای زبان‌های indexable متفاوت |

---

### TC-P02-T003 — Design tokens + Tailwind semantic mapping

| Field | Content |
|-------|---------|
| Objective | لایهٔ Design Tokens معنایی را برای P02 برقرار کند. |
| Exact scope | color/typography/spacing/radius/container/breakpoint/z-index/motion/touch-target به‌صورت token؛ اتصال به Tailwind CSS variables؛ بدون brand copy از reference sites. |
| Allowed | `globals.css` / token modules · Tailwind theme extension. |
| Forbidden | کامپوننت‌های دامنهٔ تور · پکیج UI بزرگ بدون صراحت Task. |
| Dependencies | T001 |
| Acceptance | توکن‌های معنایی مستند و قابل استفاده در primitives بعدی. |
| Proofs | build · lint · نمونهٔ مصرف در یک primitive یا صفحهٔ موقت غیرمحصولی اگر لازم |
| Artifacts | token source of truth |
| Stop | tokenهای فیچرمحور بی‌دلیل (`color.tourHeroSpecial`) |

---

### TC-P02-T004 — Direction-neutral primitives + bidi-safe text

| Field | Content |
|-------|---------|
| Objective | primitives پایهٔ direction-neutral و ابزار bidi را بسازد (ADR 0006). |
| Exact scope | primitives حداقلی (مثلاً Text/Stack/Container/Inline یا معادل) با logical properties؛ helper برای مقادیر LTR-in-RTL (کد فرودگاه، شماره پرواز، کد ارز). |
| Allowed | `components/ui` یا مسیر قراردادی primitives. |
| Forbidden | `"use client"` روی primitives غیرتعاملی · صفحهٔ تور کامل. |
| Dependencies | T002 · T003 |
| Acceptance | در FA و EN بدون hard-coded left/right فیزیکی برای layout عمومی. |
| Proofs | build · نمونهٔ bidi در fixture کوچک · lint |
| Artifacts | primitive components + bidi utility |
| Stop | `dir=rtl` سراسری بدون logical CSS |

---

### TC-P02-T005 — Money / MixedCurrencyPrice presentation

| Field | Content |
|-------|---------|
| Objective | نمایش پول چندارزی را بدون تبدیل خاموش و با تمایز IRR/Toman پیاده کند. |
| Exact scope | کامپوننت(های) نمایش `Money` / `PriceComponents` / `MixedCurrencyPrice`؛ فرمت‌کردن نمایشی؛ بدون Quote/Payment. |
| Allowed | presentation components + typed view-model shapes. |
| Forbidden | نرخ‌گذاری backend · جمع خاموش ارزهای مختلط · ذخیره float/double. |
| Dependencies | T004 |
| Acceptance | چند مؤلفهٔ ارزی هم‌زمان قابل نمایش‌اند بدون القای جمع خودکار؛ IRR≠Toman در UI copy/semantics. |
| Proofs | unit/component test اگر runner موجود/اضافه شد؛ وگرنه fixture + build proof |
| Artifacts | Money presentation components |
| Stop | تبدیل ارز در UI |

---

### TC-P02-T006 — Accessibility baseline

| Field | Content |
|-------|---------|
| Objective | پایهٔ a11y عمومی را برای shell و صفحات بعدی قفل کند. |
| Exact scope | landmarks · heading hierarchy conventions · skip link · focus-visible · touch target guidance در primitives/shell؛ هم‌راستا با `docs/ui/05-accessibility-and-interaction.md`. |
| Allowed | shared a11y helpers/components. |
| Forbidden | audit کامل همهٔ صفحات آینده · پکیج سنگین a11y بدون توجیه. |
| Dependencies | T004 |
| Acceptance | shell/layout دارای landmark/skip؛ کنترل‌های پایه keyboard-reachable. |
| Proofs | checklist evidence · build |
| Artifacts | a11y baseline utilities/patterns |
| Stop | a11y فقط به‌عنوان TODO برای بعد از P02 |

---

### TC-P02-T007 — App Router loading / error / not-found

| Field | Content |
|-------|---------|
| Objective | قرارداد حالت‌های Loading/Error/NotFound را در App Router برقرار کند. |
| Exact scope | `loading.tsx` / `error.tsx` / `not-found.tsx` (یا معادل locale-aware) برای مسیر عمومی؛ اسکلت پایدار نه collapse کامل صفحه. |
| Allowed | route-level special files + shared skeletons. |
| Forbidden | منطق دامنهٔ تور واقعی · toast-only error برای شکست هسته. |
| Dependencies | T002 · T004 |
| Acceptance | مسیر نمونه حالت‌ها را بدون hydration error نشان می‌دهد. |
| Proofs | build · smoke routes |
| Artifacts | loading/error/not-found conventions |
| Stop | full-page client error boundary برای کل درخت بدون نیاز |

---

### TC-P02-T008 — Public shell + Admin shell layout primitives

| Field | Content |
|-------|---------|
| Objective | Public shell حداقلی و **primitives/layout** اسکلت Admin را جدا کند — بدون قفل کردن IA/navigation مبتنی بر دامنه. |
| Exact scope | header/nav/footer عمومی locale-aware؛ Admin shell = chrome/layout/slots فقط (sidebar/header placeholders)؛ **نه** منوی نهایی Admin هم‌تراز با bounded contexts. |
| Allowed | shell layouts/components · layout slots. |
| Forbidden | auth redesign · agency marketplace · admin CRUD واقعی · **domain-mirroring Admin navigation IA** · قفل منو بر اساس ماژول‌های Backend. |
| Dependencies | T002 · T004 · T006 |
| Acceptance | Public shell در FA/EN کار می‌کند؛ Admin foundation جداست و navigation نهایی Admin هنوز **unfrozen** است تا بعد از `TC-P02-T010`. |
| Proofs | build · FA/EN smoke |
| Artifacts | PublicShell · AdminShell layout primitives |
| Stop | تبدیل Admin shell به نقشهٔ منوی دامنه-به-دامنه قبل از Cross-Domain Workflow task
---

### TC-P02-T009 — Frontend API / read-model boundary

| Field | Content |
|-------|---------|
| Objective | مرز API/client مناسب Walking Skeleton را بدون کوپل به persistence بسازد. |
| Exact scope | قرارداد typed برای page read models؛ adapter دریافت داده (فعلاً fixture/file یا HTTP stub)؛ ممنوعیت import backend؛ تصمیم صریح دربارهٔ OpenAPI codegen: **در P02 پیش‌فرض = نه** مگر Task جدا و توجیه‌شده. |
| Allowed | `lib/api` یا `lib/read-models` · types · fixture loader. |
| Forbidden | EF Entity · DbContext · connection strings · OpenAPI codegen اجباری. |
| Dependencies | T001 |
| Acceptance | صفحه می‌تواند از port typed بخواند؛ هیچ وابستگی DB نیست. |
| Proofs | typecheck · build |
| Artifacts | API/read-model boundary + loader |
| Stop | mirror کردن schema دیتابیس در Page VM |

---

### TC-P02-T010 — Cross-Domain Workflow & Navigation Model

| Field | Content |
|-------|---------|
| Objective | مدل workflow-driven و navigation را برای UI چنددامنه‌ای قبل از قفل شدن Admin IA تثبیت کند. |
| Exact scope | تحلیل و مستندسازی الگوهای orchestration در لایهٔ application/API برای حداقل: Identity↔Party↔Access · Tour↔Destination↔Media↔Pricing · Booking↔Party↔Pricing↔Payment · Place/Hotel↔Media↔Pricing (در صورت کاربرد). برای هر جریان: user goal · persona · participating domains · owner هر عملیات · workflow sequence · create/link/select · API/application boundary · prefill/reuse · mobile behavior · validation · permissions · failure/retry/recovery · navigation implications · جزئیات دامنه‌ای که باید از کاربر پنهان بماند. |
| Allowed | docs under `docs/` (plan appendix و/یا `docs/ui`/`docs/architecture` مرتبط) — بدون پیاده‌سازی محصول. |
| Forbidden | backend domain redesign · CRUD صفحات جدا برای هر دامنه به‌عنوان مدل اجباری · اجرای T011+ محصولی · P03+. |
| Dependencies | T008 · T009 |
| Acceptance | اصل «مرز دامنه ≠ مرز صفحه/منو» در نقشهٔ P02 اجرایی است؛ الگوی Identity+Party create/link مستند است؛ Admin navigation هنوز قبل از این Task قابل freeze نیست و بعد از آن طراحی می‌شود. |
| Proofs | documentation consistency · `git diff --check` |
| Artifacts | Cross-domain workflow & navigation model doc/section |
| Stop | mirror کردن ماژول‌های Backend در منوی Admin به‌عنوان پیش‌فرض |

---

### TC-P02-T011 — Image foundation

| Field | Content |
|-------|---------|
| Objective | قرارداد تصویر برای hero/media عمومی را با `next/image` برقرار کند. |
| Exact scope | wrapper/convention برای تصویر responsive، alt اجباری، placeholder policy؛ بدون Media module کامل (P06). |
| Allowed | image helper/component · remotePatterns اگر لازم و مستند. |
| Forbidden | S3 pipeline · CDN کامل · CMS media. |
| Dependencies | T004 |
| Acceptance | تصویر نمونه در skeleton بدون layout shift فاجعه‌بار و با alt. |
| Proofs | build |
| Artifacts | Image foundation component/convention |
| Stop | `<img>` پراکنده بدون قرارداد برای سطوح عمومی مهم |

---

### TC-P02-T012 — Foreign Tour Detail PVM + typed fixture

| Field | Content |
|-------|---------|
| Objective | Page View Model و fixture typed برای `ForeignTourDetailPage` بسازد. |
| Exact scope | types مطابق anatomy پذیرفته‌شده؛ fixture FA و EN با bidi samples (فرودگاه، پرواز، ارز مختلط، هتل‌آپشن، وضعیت تجاری)؛ صراحت TourProduct≠TourDeparture. |
| Allowed | types + fixtures under frontend. |
| Forbidden | backend Tour module · booking API واقعی. |
| Dependencies | T005 · T009 |
| Acceptance | fixture هر دو locale؛ قیمت چندارزی؛ بدون قیمت جعلی ۰ برای unavailable. |
| Proofs | typecheck · fixture load smoke |
| Artifacts | `ForeignTourDetail` PVM + fixtures |
| Stop | یک fixture انگلیسی که در `/fa` silently reuse شود به‌عنوان محتوای منتشرشده |

---

### TC-P02-T013 — Foreign Tour Detail walking skeleton (Server)

| Field | Content |
|-------|---------|
| Objective | ترکیب Server Component صفحهٔ Foreign Tour Detail را به‌عنوان Walking Skeleton پیاده کند. |
| Exact scope | route locale-aware؛ sections تصمیم‌ساز (header/hero، departure summary، flight summary، hotel options، pricing/occupancy، services، requirements، CTA slot)؛ Server-first؛ بدون Client کردن کل صفحه. |
| Allowed | page + sections under feature folder. |
| Forbidden | Booking واقعی · Payment · Search · Experience Tour کامل. |
| Dependencies | T007 · T008 · T011 · T012 |
| Acceptance | FA و EN رندر می‌شوند؛ anatomy اصلی archetype پوشش داده شده؛ SEO-critical content در HTML اولیه موجود است. |
| Proofs | build · view-source/server HTML spot-check · lint |
| Artifacts | Foreign Tour Detail route + sections |
| Stop | `"use client"` روی page |

---

### TC-P02-T014 — Sticky / mobile booking CTA island

| Field | Content |
|-------|---------|
| Objective | affordance رزرو/اقدام موبایل و sticky دسکتاپ را با Client island کوچک اضافه کند. |
| Exact scope | CTA island تعاملی حداقلی؛ sticky summary در دسکتاپ بدون مالکیت محتوای منحصربه‌فرد غیرقابل دسترس؛ bottom sticky/sheet hook در موبایل طبق UI constitution. |
| Allowed | small `"use client"` island فقط برای تعامل. |
| Forbidden | checkout · auth · payment. |
| Dependencies | T013 |
| Acceptance | موبایل CTA پایدار و usable؛ محتوا بدون sticky هم خوانا است. |
| Proofs | build · FA/EN mobile/desktop smoke notes |
| Artifacts | BookingCtaIsland / sticky summary |
| Stop | کل صفحه Client به‌خاطر sticky |

---

### TC-P02-T015 — SEO metadata baseline for skeleton route

| Field | Content |
|-------|---------|
| Objective | metadata پایهٔ server برای مسیر Foreign Tour Detail را بدون ساخت SEO Engine کامل فراهم کند. |
| Exact scope | `generateMetadata` (یا معادل) از PVM/fixture؛ title/description؛ آمادگی برای hreflang/canonical بعدی بدون پیاده‌سازی P05. |
| Allowed | metadata API روی route اسکلت. |
| Forbidden | slug history · IndexPolicy engine · sitemap کامل (P05). |
| Dependencies | T013 |
| Acceptance | FA/EN metadata متمایز و server-emitted. |
| Proofs | build · metadata inspection |
| Artifacts | metadata for skeleton routes |
| Stop | وابسته کردن indexability فقط به Client runtime |

---

### TC-P02-T016 — Frontend quality gates

| Field | Content |
|-------|---------|
| Objective | دروازه‌های کیفیت خودکار مناسب ریپو را برای frontend پایدار کند. |
| Exact scope | scripts `lint` / `typecheck` / `build`؛ در صورت نیاز حداقل تست برای Money یا bidi helper با توجیه؛ مستندسازی دستورات در README؛ بدون CI کامل اجباری مگر فایل‌های موجود لمس شوند. |
| Allowed | `package.json` scripts · devDependency فقط با توجیه صریح در این Task. |
| Forbidden | تعویض کلی stack · Playwright کامل بدون نیاز اثبات‌شده. |
| Dependencies | T005 · T013 (حداقل یک سطح از کد محصولی) |
| Acceptance | یک دستور/مجموعه‌دستور مستند برای lint+typecheck+build سبز است. |
| Proofs | lint · tsc · build |
| Artifacts | scripts + optional minimal tests |
| Stop | افزودن چند framework تست هم‌زمان بدون نیاز |

---

### TC-P02-T017 — Walking skeleton validation matrix evidence

| Field | Content |
|-------|---------|
| Objective | شواهد اعتبارسنجی ماتریس حداقل را ثبت کند. |
| Exact scope | ماتریس FA Desktop · FA Mobile · EN Desktop · EN Mobile؛ پوشش RTL/LTR/bidi/sticky/a11y/SEO-render؛ نماینده عرض‌ها؛ بدون ادعای کامل بودن Experience Tour. |
| Allowed | docs evidence تحت `docs/` یا `docs/plans/` · checklist. |
| Forbidden | شروع P03 · پیاده‌سازی فیچر جدید خارج از رفع باگ اسکلت. |
| Dependencies | T014 · T015 · T016 |
| Acceptance | سند شواهد کامل و قابل‌بازبینی معمار. |
| Proofs | evidence doc · build still green |
| Artifacts | validation evidence document |
| Stop | PASS بدون ماتریس FA/EN × Mobile/Desktop |

---

### TC-P02-GATE — P02 Acceptance Gate

| Field | Content |
|-------|---------|
| Objective | دروازهٔ پذیرش فاز: foundation + walking skeleton تأیید معماری UI. |
| Exact scope | verification-only + truthful state/docs؛ بدون فیچر جدید. |
| Allowed | PROJECT-STATE/ROADMAP · شواهد. |
| Forbidden | P03 · redesign · گسترش Tour domain. |
| Dependencies | T001–T017 COMPLETE/ACCEPTED per architect |
| Acceptance | معمار معماری UI را در RTL/LTR و mobile/desktop برای اسکلت تأیید می‌کند. |
| Proofs | full P02 validation set |
| Artifacts | GATE result + state |
| Stop | اجرای GATE قبل از تکمیل T017 |

---

## 8. Dependency graph (summary)

```text
T001
├── T002 ──┐
├── T003 ──┼── T004 ── T005
│          │      ├── T006 ── T008
│          │      └── T007
│          └── T009 ── T010 ──┐
└── T011                      ├── T012 ── T013 ── T014
                              │              └── T015
                              └──────────────────── T016
                                                     │
                                                  T017 ── GATE
```

- **First implementation task:** `TC-P02-T001`
- **Cross-domain workflow task:** `TC-P02-T010` (before Admin navigation freeze)
- **Foreign Tour Detail tasks:** `TC-P02-T012` · `TC-P02-T013` · `TC-P02-T014` · `TC-P02-T015` · (evidence in `T017`)
- **Final gate:** `TC-P02-GATE`
- **P03 scope in this plan:** NO
## 9. Coverage checklist vs P02 objectives

| Objective | Covered by |
|-----------|------------|
| Frontend application foundation | T001–T011 |
| App Router architecture | T002 · T007 · T013 |
| Server Component First | T013–T015 constraints |
| Minimal Client | T014 only for interaction |
| Mobile-first | T008 · T010 · T013 · T014 · T017 |
| Cross-domain workflow-driven UX | T010 (+ durable rule in UI constitution) |
| Admin navigation freeze protection | T008 limited · T010 before final Admin IA |
| RTL/LTR + bidi | T002 · T004 · T012–T017 |
| Direction-neutral primitives | T004 |
| Accessibility baseline | T006 · T017 |
| SEO-sensitive SSR | T013 · T015 |
| Shared shells (layout only for Admin) | T008 |
| Design tokens/components | T003 · T004 · T005 · T011 |
| API/client boundary | T009 |
| Loading/error/not-found | T007 |
| Foreign Tour Detail skeleton | T012–T015 |
| FA/EN · desktop/mobile validation | T017 · GATE |
| Representative IA | T012 · T013 |
| Sticky/mobile booking affordance | T014 |
| Automated quality gates | T016 |
| No full Tour domain / no P03 | §6 · all Forbidden fields |
---

## 10. Execution rules for Cursor

1. فقط یک Task با envelope `Auto-Execute: YES` در هر چرخه.
2. بعد از Result → STOP تا بازبینی معمار.
3. invent کردن Task-ID خارج از این نقشه ممنوع است.
4. پکیج بزرگ فقط با صراحت Scope همان Task.
5. تعارض با ADR Accepted → `SOURCE_OF_TRUTH_CONFLICT` / BLOCKED.

---

## 11. Document history

| Date | Note |
|------|------|
| 2026-08-15 | `TC-P02-PLAN` initial authoritative map at baseline `0853d04` |
| 2026-08-15 | Architect revision: cross-domain workflow UX rule + T010 + renumber T011–T017; Admin shell non-freezing |
