# UI Constitution — قانون اساسی رابط کاربری TravelCore

این سند **اصول قفل‌شدهٔ UI Architecture** TravelCore است. قبل از پیاده‌سازی UI محصول (P02 و بعد) باید خوانده شود.

جزئیات:

| سند | نقش |
|-----|------|
| [`../ui/01-design-system-architecture.md`](../ui/01-design-system-architecture.md) | Tokens · hierarchy · مسئولیت کامپوننت |
| [`../ui/02-responsive-mobile-architecture.md`](../ui/02-responsive-mobile-architecture.md) | Mobile-first · matrix · sticky · tables |
| [`../ui/03-rtl-ltr-bidi.md`](../ui/03-rtl-ltr-bidi.md) | dir/lang · logical CSS · bidi |
| [`../ui/04-page-archetype-contract.md`](../ui/04-page-archetype-contract.md) | چک‌لیست Page Archetype · validation |
| [`../ui/05-accessibility-and-interaction.md`](../ui/05-accessibility-and-interaction.md) | a11y · forms · states · feedback |
| [`00-constitution.md`](00-constitution.md) | قانون اساسی سیستم |
| [`02-technology-baseline.md`](02-technology-baseline.md) | Next.js · Tailwind baseline |

ADRهای مرتبط این Task در وضعیت **Proposed** هستند تا بازبینی معمار.

---

## 1. سلسله‌مراتب UI — قفل‌شده

```text
Design Tokens
    ↓
Primitives
    ↓
Composite Components
    ↓
Domain Components
    ↓
Sections
    ↓
Page Archetypes
    ↓
Actual Routes (localized)
```

### ممنوع به‌عنوان مدل پیش‌فرض

```text
Route → random JSX → arbitrary CSS → duplicated controls
```

صفحه فقط یک فایل JSX نیست. Page Archetype قبل از پیاده‌سازی صفحات مهم مشخص می‌شود.

---

## 2. اصول غیرقابل‌مذاکره

1. **Server Component first** — Client فقط برای تعامل واقعی مرورگر
2. **Minimal Client boundary** — جزیرهٔ تعاملی کوچک، نه کل صفحه Client
3. **Mobile-first** — موبایل نسخهٔ فشردهٔ دسکتاپ نیست
4. **RTL/LTR از روز اول** — نه `body { direction: rtl; }` به‌تنهایی
5. **Bidi جدا از RTL** — کدهای پرواز/ارز/شناسه اغلب LTR می‌مانند
6. **Accessibility first-class** — نه فاز نهایی QA
7. **SEO-first rendering** — محتوای کشف‌پذیر وابسته به hydration نباشد
8. **بدون منطق کسب‌وکار authoritative در UI**
9. **بدون فیلد layout در API** (`leftColumn`, `desktopSidebar`, …)
10. **مرجع‌های محصول برای تحلیل‌اند** — کپی برند/کد/محتوا/layout اختصاصی ممنوع

---

## 3. Next.js Rendering

| قاعده | معنی |
|-------|------|
| پیش‌فرض | Server Component |
| Client | فیلتر، date picker، passenger selector، carousel، map، modal/sheet، فرم پویا، booking interaction، APIهای مرورگر |
| ممنوع | `"use client"` روی page/root layout فقط برای راحتی |

### مثال ۱۵ — Server page + Client island

```text
ForeignTourDetailPage (Server)
  ├── TourHeroSection (Server)
  ├── TourPricingSection (Server)
  ├── TourHotelOptionsSection (Server)
  └── PassengerPicker / BookingCTA island (Client — small)
```

نه:

```text
entire Tour page = "use client"
```

جزئیات: ADR Proposed [`../adr/0005-server-component-first.md`](../adr/0005-server-component-first.md)

---

## 4. قرارداد دادهٔ فرانت‌اند

Server Components از **semantic API / read model** مصرف می‌کنند.

- بدون افشای EF Entity
- بدون mirror کردن ساختار دیتابیس در Page View Model
- Frontend presentation را مالک است؛ API دادهٔ معنایی می‌دهد

### ممنوع در قرارداد API

```text
leftColumn · rightColumn · desktopSidebar · mobileCard · cardColor
```

---

## 5. بدون موتور کسب‌وکار در UI

Frontend ممکن است:

- validation نمایشی
- validation تعامل
- optimistic UX امن

داشته باشد.

Frontend **نباید**:

- bookable بودن تور را با قواعد کپی‌شده تصمیم بگیرد
- Quote authoritative را دوباره محاسبه کند
- authorization را فقط از مخفی بودن دکمه نتیجه بگیرد

Backend برای invariants کسب‌وکار authoritative است.

---

## 6. Design Tokens (خلاصه)

مقادیر بصری تکرارشونده از semantic tokens می‌آیند.

دسته‌ها: Color · Typography · Spacing · Radius · Shadow · Container · Breakpoint · Z-index · Motion · Control size · Touch target · Border · Surface

هر پیکسل لزوماً token نیست؛ اما ارزش‌های سیستمی تکرارشونده باید token شوند.

معماری token نباید dark mode / brand variant آینده را مسدود کند — بدون over-engineering الان.

جزئیات: [`../ui/01-design-system-architecture.md`](../ui/01-design-system-architecture.md)

---

## 7. مسئولیت لایه‌های کامپوننت (خلاصه)

| لایه | می‌داند | نمی‌داند |
|------|---------|----------|
| Primitive | حالت بصری | TourStatus / BookingStatus |
| Composite | ترکیب UI عمومی | محاسبه Quote |
| Domain | مفاهیم دامنه برای نمایش | موتور Pricing authoritative |
| Section | بلوک صفحه | مالکیت API layout |
| Page Archetype | anatomy + رفتار | پیاده‌سازی پراکنده |

`MixedCurrencyPrice` می‌تواند `PriceComponent[]` را نمایش دهد — محاسبهٔ قیمت نمی‌کند.

---

## 8. Mobile-first و Responsive

عرض‌های اعتبارسنجی: `360 · 390 · 768 · 1024 · 1280 · 1440`  
این‌ها نقطهٔ بررسی‌اند، نه الزام شش media query جدا.

هر Page Archetype مهم نیاز به **Responsive Behavior Matrix** دارد (Desktop / Tablet / Mobile / RTL-LTR / a11y).

Touch target تقریبی: ~`44px` usable area.

اقدامات حیاتی نباید فقط به hover وابسته باشند.

جزئیات: [`../ui/02-responsive-mobile-architecture.md`](../ui/02-responsive-mobile-architecture.md)

---

## 9. RTL / LTR / Bidi (خلاصه)

### مثال ۱ — FA root

```html
<html lang="fa" dir="rtl">
```

### مثال ۲ — EN root

```html
<html lang="en" dir="ltr">
```

Arabic: `lang="ar" dir="rtl"`.

کامپوننت‌ها direction-neutral با logical properties (`margin-inline-start`, `start`/`end`).

آینه‌سازی کور همه چیز ممنوع است. Bidi جداست.

### مثال‌های bidi در UI فارسی

| مقدار | جهت معنایی |
|-------|-------------|
| IKA / IST | LTR |
| EK978 | LTR |
| USD | LTR |
| Booking reference | LTR |

### مثال ۸ — مسیر پرواز

در UI فارسی، `IKA → IST` معنای جغرافیایی دارد — فلش مسیر را فقط به‌خاطر RTL کورانه آینه نکنید.

جزئیات: [`../ui/03-rtl-ltr-bidi.md`](../ui/03-rtl-ltr-bidi.md) · ADR Proposed [`../adr/0006-direction-neutral-ui-bidi.md`](../adr/0006-direction-neutral-ui-bidi.md)

---

## 10. پول در UI

طبق ADR Accepted پول/IRR:

- IRR کاننیکال است
- Toman واحد DISPLAY/INPUT است (`1 Toman = 10 IRR`)
- UI باید واحد نمایش را واضح کند
- MixedCurrencyPrice مؤلفه‌ها را جدا نشان می‌دهد؛ جمع/تبدیل خاموش ممنوع

### مثال ۶ — Mixed currency

```text
1,290 USD
+
119,900,000 IRR
```

### مثال ۷ — Toman display

```text
Display:  11,990,000 تومان
Canonical Money remains: 119,900,000 IRR
```

---

## 11. حالت‌های تعامل (خلاصه)

هر فیچر UI مهم باید در صورت ارتباط مشخص کند:

Loading · Success · Empty · Error · Unavailable/Expired

- Empty ≠ Error
- Expired tour لزوماً 404 نیست (SEO lifecycle جداست — جزئیات TC-P00-T006)
- Error عمومی بدون stack/SQL/provider raw

جزئیات: [`../ui/05-accessibility-and-interaction.md`](../ui/05-accessibility-and-interaction.md)

---

## 12. Accessibility

Semantic HTML · keyboard · focus · labels · heading hierarchy · landmarks · contrast · screen readers · reduced motion · touch targets · error association.

`button` برای action، `a`/`Link` برای navigation.  
Heading level از معنا می‌آید، نه از اندازهٔ فونت.

---

## 13. Performance و Core Web Vitals

ترجیح: Server Components · minimal hydration · streaming مفید · تصویر بهینه · third-party کنترل‌شده · layout پایدار · client island کوچک.

اهداف کیفیت (نه invariant دامنه):

```text
LCP ≤ 2.5s
INP ≤ 200ms
CLS ≤ 0.1
```

Hero مهم را بی‌فکر lazy نکنید؛ کل گالری را eager نکنید.

---

## 14. SEO-sensitive rendering

محتوای کشف‌پذیر (عنوان تور، نام مقصد، توضیح اصلی، زمینهٔ قیمت در صورت مناسب، breadcrumb، محتوای ساخت‌یافتهٔ معنایی) باید server-renderable باشد.

جزئیات SEO → TC-P00-T006.

---

## 15. Tailwind baseline

Tailwind CSS baseline تأییدشدهٔ bootstrap است.

اما:

- از Design Tokens پیروی کند
- direction-safe باشد
- abstraction و reuse داشته باشد
- به styling یک‌بارمصرف پراکنده در routeها تبدیل نشود

پیاده‌سازی token در P02.

---

## 16. مراجع محصول

LastSecond / TahaGasht برای تحلیل IA، anatomy، hierarchy، flow مفیدند.

**کپی نشود:** هویت بصری برند · کد · متن دارای حق نشر · آیکون/دارایی · layout اختصاصی دقیق.

TravelCore باید Design System منسجم خودش را بسازد.

---

## 17. Page Archetype Contract (خلاصه)

قبل از پیاده‌سازی archetype عمومی مهم: Purpose · intent · data · anatomy · hierarchy · desktop/tablet/mobile · RTL/LTR · bidi · loading/empty/error/unavailable · a11y · SEO · analytics · performance risks.

اولویت اعتبارسنجی UI:

1. Foreign Package Tour Detail
2. Experience Tour Detail
…

جزئیات: [`../ui/04-page-archetype-contract.md`](../ui/04-page-archetype-contract.md)

---

## 18. Design Review Gate

`npm run build` به‌تنهایی Done معماری UI نیست.

بازبینی مرتبط: responsive · RTL · LTR · bidi · loading · a11y · SEO impact · performance risk · visual hierarchy.

ماتریس حداقل: FA+RTL Desktop/Mobile · EN+LTR Desktop/Mobile (+ Arabic با بلوغ locale).

---

## 19. Intentionally Deferred

- پالت برند نهایی · فونت · icon library · animation library · form library
- client state / data-fetching library · Storybook · visual regression
- مقادیر دقیق breakpoint/spacing/radius/shadow
- طراحی نهایی header/footer/aesthetics
- dark mode launch · Admin/Agency design specifics
- انتخاب shadcn/Radix/MUI/Ant/Chakra/Mantine/Headless UI/React Aria

این‌ها P02 یا Task صریح بعدی‌اند و در صورت معنادار بودن ممکن است ADR بخواهند.

---

## 20. ضدالگوهای UI

- کل صفحه Client برای راحتی
- منطق Pricing/Booking/Authz در UI
- فیلد layout در API
- RTL فقط با direction روی body + left/right پراکنده
- آینه‌سازی کور همه چیز
- موبایل = دسکتاپ فشرده
- hover برای اقدام حیاتی
- جدول دسکتاپ ناخوانا روی موبایل به‌عنوان UX پیش‌فرض
- جمع/تبدیل خاموش ارزهای ترکیبی
- Toman به‌عنوان CurrencyCode کاننیکال
- محتوای SEO فقط بعد از hydration
- accessibility به‌عنوان فاز پایانی
- کپی بصری سایت مرجع
