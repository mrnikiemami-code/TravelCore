# Page Archetype Contract

منبع: [`../architecture/10-ui-constitution.md`](../architecture/10-ui-constitution.md)  
Roadmap validation sequence: [`../ROADMAP.md`](../ROADMAP.md)

---

## 1. صفحه فقط JSX نیست

قبل از پیاده‌سازی یک public Page Archetype مهم، مشخصات باید checklist زیر را پوشش دهد.

پیاده‌سازی بدون این قرارداد برای صفحات پیچیده = ناقص از نظر معماری UI.

---

## 2. Mandatory Specification Checklist

| محور | باید روشن شود |
|------|----------------|
| Purpose | صفحه چرا وجود دارد |
| Primary user intent | کاربر چه می‌خواهد انجام دهد |
| Domain data requirements | چه داده‌های معنایی لازم است |
| Page anatomy | ساختار بخش‌ها |
| Component hierarchy | Tokens→…→Sections مورد استفاده |
| Desktop layout | رفتار دسکتاپ |
| Tablet layout | رفتار تبلت |
| Mobile layout | رفتار موبایل |
| RTL behavior | fa/ar |
| LTR behavior | en |
| Bidi-sensitive values | کدها، پول، شناسه‌ها |
| Loading state | اسکلت/استراتژی |
| Empty state | اگر داده نیست |
| Error state | شکست خواندن/اقدام |
| Unavailable / expired state | محصول منقضی/غیرقابل رزرو |
| Accessibility | ساختار، کیبورد، focus، labels |
| SEO impact | render، metadata، indexability hints |
| Analytics events | در صورت ارتباط |
| Performance risks | LCP، hydration، تصاویر، JS |

---

## 3. Responsive Behavior Matrix (الزام آینده)

برای archetypeهای مهم، ماتریس Element × Desktop/Tablet/Mobile (+ RTL/LTR + a11y notes) تعریف شود. جزئیات: [`02-responsive-mobile-architecture.md`](02-responsive-mobile-architecture.md)

---

## 4. Validation Sequence (UI)

ترتیب اعتبارسنجی از Roadmap:

1. Foundation primitives
2. **Foreign Package Tour Detail**
3. **Experience Tour Detail**
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

---

## 5. Archetype اول — Foreign Package Tour Detail

اولین archetype اعتبارسنجی پیچیده:

`ForeignTourDetailPage`

چرا؟ هم‌زمان فشار می‌آورد روی:

- RTL / LTR / bidi
- airline codes · flight numbers
- dates/times
- hotel options · ratings
- mixed currencies · occupancy pricing
- responsive layout
- sticky booking · mobile bottom sheet
- image gallery
- SEO semantics

### جهت معماری UX (نه ظاهر نهایی)

**Desktop:** محتوای اصلی + sticky booking/pricing sidebar + hotel options غنی + پرواز/itinerary

**Mobile:** تک‌ستونه + sticky CTA + sheet جزئیات رزرو + کارت hotel options + گالری لمسی

### Fixture direction

بعداً با typed fixtures قبل از یکپارچگی کامل Tour backend پیاده می‌شود.

**الان پیاده نشود.**

---

## 6. Archetype دوم — Experience Tour Detail

`ExperienceTourDetailPage`

باید اعتبارسنجی کند:

- itinerary timeline · days · stops
- attraction links · maps
- meals · accommodation · equipment · difficulty
- structured editorial content
- long-form responsive content

---

## 7. مثال ۱۰ — Destination Landing composition

```text
Purpose: کشف مقصد و مسیر به tours/places/content
Anatomy:
  Hero
  Key facts / intro
  Tours teaser
  Hotels / Places teaser
  Guide / articles
  UGC teaser
  Related destinations
Data: Destination + composed read models (نه join همه‌ماژول در یک SQL)
SEO: server-renderable title/description/breadcrumbs
```

---

## 8. Minimum Locale/Direction Review Matrix

Archetype عمومی مهم حداقل در:

| Locale | Direction | Viewport |
|--------|-----------|----------|
| FA | RTL | Desktop |
| FA | RTL | Mobile |
| EN | LTR | Desktop |
| EN | LTR | Mobile |

Arabic با بلوغ locale پوشش داده شود. مقادیر mixed-bidi باید صریحاً تست شوند.

---

## 9. Loading / Empty / Error / Unavailable Contract

Task پیاده‌سازی UI عمده ناقص است اگر فقط happy path را مشخص کند.

حداقل در صورت ارتباط:

| State | مثال |
|-------|------|
| Loading | skeleton نزدیک به شکل نهایی |
| Success | محتوای کامل |
| Empty | No tours currently available |
| Error | خواندن شکست خورد — قابل فهم و قابل بازیابی |
| Unavailable/Expired | Tour departure expired — نه لزوماً 404 |

جزئیات تعامل: [`05-accessibility-and-interaction.md`](05-accessibility-and-interaction.md)

---

## 10. Design Review Gate

Done معماری UI ≠ فقط `npm run build`.

بازبینی مرتبط: responsive · RTL · LTR · bidi · loading · a11y · SEO · performance risk · hierarchy.

---

## 11. Reference Sites

LastSecond / TahaGasht برای تحلیل anatomy و flow.

کپی برند، کد، متن، دارایی، layout اختصاصی ممنوع.

---

## 12. No Implementation in P00-T004

این سند قرارداد است. هیچ route، کامپوننت، یا fixture در این Task ساخته نمی‌شود.
