# Internationalization Architecture — معماری بین‌المللی‌سازی TravelCore

این سند **منبع حقیقت سطح‌بالای i18n** TravelCore است. قبل از پیاده‌سازی چندزبانهٔ محصول باید خوانده شود.

جزئیات:

| سند | نقش |
|-----|------|
| [`../i18n/01-locale-and-routing.md`](../i18n/01-locale-and-routing.md) | Locale · BCP 47 · مسیر عمومی · language switch |
| [`../i18n/02-translation-ownership-and-lifecycle.md`](../i18n/02-translation-ownership-and-lifecycle.md) | UI/Entity/Editorial · lifecycle · ownership |
| [`../i18n/03-formatting-and-cultural-preferences.md`](../i18n/03-formatting-and-cultural-preferences.md) | Number · Money · Calendar · TimeZone |
| [`../i18n/04-fallback-and-publication-policy.md`](../i18n/04-fallback-and-publication-policy.md) | Fallback · publication per locale |
| [`../i18n/05-i18n-quality-and-governance.md`](../i18n/05-i18n-quality-and-governance.md) | Quality · a11y lang · observability · tests |
| [`10-ui-constitution.md`](10-ui-constitution.md) · ADR 0005/0006 Accepted | UI · RTL/LTR · bidi · Server Components |
| [`07-data-architecture.md`](07-data-architecture.md) · ADR 0003/0004 Accepted | Money · IRR/Toman · temporal |
| SEO تفصیلی | **TC-P00-T006** |

ADRهای مرتبط این Task **Accepted** هستند.

---

## 1. اصول قفل‌شده

1. چندزبانه از روز اول؛ locales استراتژیک اولیه: `fa` · `en` · `ar` (قابل گسترش با BCP 47)
2. **Locale ≠ Currency ≠ Calendar ≠ TimeZone**
3. ممنوع: `NameFa` / `NameEn` / `NameAr`
4. سه دستهٔ ترجمهٔ جدا: UI · Entity · Editorial
5. ترجمه‌های Entity متعلق به ماژول مالک Entity هستند — بدون Translation table سراسری
6. مسیرهای عمومی indexable با prefix صریح locale: `/fa/...` · `/en/...` · `/ar/...`
7. برای صفحهٔ عمومی locale-prefixed، **URL locale authoritative** است
8. انتشار ترجمه **per locale** است؛ وجود ردیف ترجمه ≠ انتشار
9. محتوای کسب‌وکار/editorial عمومی نباید زیر URL locale درخواستی، زبان دیگر را خاموش fallback کند
10. UI resources ممکن است fallback کنترل‌شده داشته باشند (متفاوت از انتشار محتوا)
11. RTL/LTR و bidi از ADR Accepted 0006 پیروی می‌کنند — تکرار متناقض ممنوع
12. SEO mechanics تفصیلی (canonical · hreflang · redirect · IndexPolicy · slug history) متعلق به **T006** است

---

## 2. Locale

`Locale` = زمینهٔ زبان/فرهنگ UI و محتوا.

شناسه‌ها از semantics تگ زبان **BCP 47** پیروی می‌کنند و normalize می‌شوند.

Registry پشتیبانی‌شدهٔ صریح لازم است. Locale enum دائمی محدود به دقیقاً سه مقدار ممنوع است.

Default عمومی اولیهٔ محصول: **`fa`** — اما default یک مفهوم configuration/محصول است، نه فرض ابدی schema.

جزئیات: [`../i18n/01-locale-and-routing.md`](../i18n/01-locale-and-routing.md)

---

## 3. مسیر عمومی (خلاصه)

### مثال ۱–۳

```text
/fa/...
/en/...
/ar/...
```

صفحات SEO/عمومی مهم فقط به `Accept-Language` یا browser state وابسته نیستند.
یک canonical URL واحد برای زبان‌های indexable متفاوت سرو نشود.

پس از وجود locale در URL عمومی:

```text
Browser preference must NOT silently replace that route language.
```

مثال: `/en/tours/istanbul` را به‌خاطر browser فارسی، فارسی render نکنید.

`/` ممکن است entry/negotiation باشد؛ جزئیات redirect/canonical → T006.

---

## 4. سه دستهٔ ترجمه

| دسته | مالک | نمونه |
|------|------|--------|
| UI Translation | منابع localization فرانت‌اند/اپ | Search · Book now |
| Entity Translation | ماژول مالک Entity | Destination.Name · TourProduct.Title |
| Editorial Translation | Content / editorial structures | Article body · rich blocks |

### مثال ۴ — یک DestinationId

```text
DestinationId = same UUID
fa: استانبول
en: Istanbul
ar: اسطنبول
```

ترجمه ≠ Entity تکراری.

جزئیات: [`../i18n/02-translation-ownership-and-lifecycle.md`](../i18n/02-translation-ownership-and-lifecycle.md)

---

## 5. Lifecycle و انتشار

ترجمه lifecycle مستقل دارد (مفهومی: Draft · Ready · Published · Archived).

### مثال ۵

```text
TourProduct commercially active
fa translation: Published  → /fa/... available
en translation: Draft      → /en/... not public yet
ar translation: Missing    → /ar/... not public yet
```

کسب‌وکار bookable بودن ≠ در دسترس بودن صفحهٔ عمومی هر locale.

---

## 6. Fallback (خلاصه)

| زمینه | سیاست |
|-------|--------|
| Public business/editorial | بدون silent cross-language fallback زیر URL locale |
| UI resources | fallback کنترل‌شده مجاز (مثلاً ar-AE → ar → configured) |
| Admin/internal | fallback با برچسب صریح مجاز |

### مثال ۶ — ممنوع

```text
/en/tours/istanbul  با عنوان/بدنهٔ فارسی به‌عنوان fallback دائمی
```

جزئیات: [`../i18n/04-fallback-and-publication-policy.md`](../i18n/04-fallback-and-publication-policy.md)

---

## 7. Formatting و ترجیحات فرهنگی

Locale formatting را راهبری می‌کند؛ معنای دامنه را عوض نمی‌کند.

### مثال ۱۰–۱۴

```text
fa + Gregorian calendar          ✓
fa + USD                         ✓
Persian UI + IKA / EK978 / USD   ✓ (bidi LTR values)
1290 USD + 119,900,000 IRR       ✓ (no silent sum/convert)
Toman display ≠ forced by fa     ✓ (explicit display policy)
```

جزئیات: [`../i18n/03-formatting-and-cultural-preferences.md`](../i18n/03-formatting-and-cultural-preferences.md)

---

## 8. مالکیت slug / SEO

محتوای ترجمه متعلق به ماژول Entity/Content است.
مکانیک مسیر عمومی (LocalizedSlug · Canonical · Redirect · IndexPolicy) متعلق به **SEO** است.

حقیقت مسیر را داخل هر جدول translation دامنه تکرار نکنید. جزئیات → T006.

---

## 9. API و Cache

- Locale را در عملیات localized صریح کنید (نه `CurrentCulture` ماشین)
- Public: معمولاً قرارداد یک-locale؛ Admin: چند ترجمه مجاز
- بدون `NameFa`/`NameEn` در قراردادها
- Cache key محلی‌شده: `Destination:123:fa` ≠ `Destination:123:en`

### مثال ۱۶

```text
cache key includes locale
```

### مثال ۱۷

```text
Domain error code (language-neutral)
→ Presentation translates to user locale
```

---

## 10. RTL / Bidi

Locale جهت پیش‌فرض سند را می‌دهد (`fa`/`ar` rtl · `en` ltr).
صفحه ≠ مقدار: ADR 0006 Accepted.

### مثال ۱۲ و ۱۵

```text
Persian UI shows IKA, EK978, USD as LTR values
English UGC may appear inside Persian UI without being labeled Persian
```

---

## 11. ضدالگوهای i18n

- `NameFa` / `NameEn` / `NameAr`
- Translation table سراسری
- enum دائمی فقط سه زبان
- سرو زبان‌های indexable متفاوت از یک canonical URL
- Accept-Language که URL صریح را override کند
- silent Persian تحت `/en/`
- وجود ردیف = Published
- Locale ⇒ Currency / Calendar / TimeZone
- تغییر locale ⇒ تبدیل Money
- localize کردن ارقام شناسه‌ها کورانه
- status متن ترجمه‌شده به‌عنوان هویت status
- UI strings داخل Domain
- provider locale به‌عنوان canonical TravelCore
- کپی همهٔ ترجمه‌ها به هر public response
- cache بدون locale
- جایگزینی کور `/fa/` → `/en/` برای language switch
- machine translation خودکار Published
- page language نادرست نسبت به محتوای اصلی

---

## 12. Intentionally Deferred Decisions

- کتابخانهٔ i18n فرانت‌اند / localization بک‌اند
- layout فایل‌های translation
- persistence دقیق locale registry
- Admin translation editor UX
- machine translation / TM provider
- copy نهایی UX برای unavailable locale
- HTTP/canonical/hreflang/redirect دقیق (T006)
- قواعد نرمال‌سازی slug
- Persian search normalization / collation PostgreSQL
- کتابخانهٔ calendar/date/currency formatter
- cookie name / user preference schema

---

## 13. ADRهای Accepted این Task

| ADR | موضوع |
|-----|--------|
| [`../adr/0007-locale-prefixed-public-routing.md`](../adr/0007-locale-prefixed-public-routing.md) | Locale-prefixed public routing |
| [`../adr/0008-translation-publication-fallback.md`](../adr/0008-translation-publication-fallback.md) | Publication per locale · fallback policy |

وضعیت: **Accepted**.
