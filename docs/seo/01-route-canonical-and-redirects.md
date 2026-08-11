# Route, Canonical, and Redirects

منبع: [`../architecture/12-seo-constitution.md`](../architecture/12-seo-constitution.md)  
ADR مرتبط (Proposed): [`../adr/0009-centralized-seo-route-ownership.md`](../adr/0009-centralized-seo-route-ownership.md)  
i18n routing: ADR 0007 Accepted

---

## 1. SeoRoute (مفهومی)

منبع عمومی indexable باید representation مسیر SEO داشته باشد.

فیلدهای مفهومی (نه schema نهایی):

```text
ResourceType · ResourceId · LocaleCode
LocalizedSlug · RouteKind
IndexPolicy · Publication state
Canonical relationship · Redirect history
```

Schema دقیق persistence → later.

---

## 2. Route Identity

```text
localized path → SeoRoute → ResourceType + ResourceId → read model
```

مسیر عمومی ≠ شکل جدول دیتابیس. Entity per locale duplicate نشود.

### مثال ۱

```text
/fa/destinations/استانبول
/en/destinations/istanbul
Same DestinationId
```

Unicode slugs مجاز مفهومی‌اند؛ نه اجباری لاتین، نه اجباری native-only. سیاست generator → later.

---

## 3. Slug Ownership

| لایه | مالک |
|------|------|
| محتوای معنایی ترجمه | ماژول کسب‌وکار / Content |
| LocalizedSlug و تاریخچهٔ مسیر | SEO |

```text
DestinationTranslation.Name = Istanbul
SeoRoute.LocalizedSlug = istanbul
```

Translation records مرجع تاریخچهٔ route نیستند.

---

## 4. Canonical

هر مسیر indexable باید canonical عمدی داشته باشد. پیش‌فرض: **self-canonical**.

Canonical جایگزین طراحی duplicate نمی‌شود و ترجمه جعلی نمی‌سازد (ADR 0008).

Tracking params (`utm_*`, `ref`) وارد canonical نشوند.

### مثال ۱۳

```text
/en/destinations/istanbul?utm_source=x
→ canonical: /en/destinations/istanbul
```

Host: یک canonical host per deployment/brand. Hostname نهایی اینجا hardcode نشود. Production: HTTPS. www vs non-www → deferred (یکی انتخاب شود). Trailing slash: یک سیاست ثابت؛ دو واریانت indexable ممنوع.

---

## 5. Hreflang Boundary

### مثال ۲

فقط معادل‌های published. Reciprocal جایی که کاربرد دارد.  
`x-default` اجباری همه‌جا نیست؛ برای entry خنثی ممکن است — fake locale نباشد. خروجی markup → later.

---

## 6. Slug Change و Redirect History

### مثال ۳

```text
old: /en/destinations/istanbul-city
new: /en/destinations/istanbul
→ permanent redirect (301 or platform equivalent)
```

Redirect records صریح؛ نه فقط از لاگ. جلوگیری از loop و زنجیرهٔ غیرضروری و redirect به Entity نامرتبط.

اگر A→B سپس B→C، ترجیحاً A مستقیماً به C (chain minimization) — پیاده‌سازی later.

---

## 7. Route Uniqueness و Reservation

در scope مسیریابی (مفهومی: Locale + RouteKind + Slug/path) دو صفحهٔ فعال برای یک canonical path رقابت نکنند.

Conflict قبل از publication تشخیص داده شود — نه بعد از duplicate.  
Consistency قوی برای uniqueness/reservation لازم است؛ صرفاً eventual background کافی نیست اگر conflict بسازد.

Namespace مسیر متمرکز در SEO؛ Destination/Tour/Place/Content رجیستری URL جداگانهٔ بی‌ربط پیاده نکنند.

---

## 8. Resolution و Cache

Lookup ممکن است cache شود؛ کلید شامل host/locale/path در صورت نیاز. SoR مسیر = دادهٔ persistent SEO.

Projectionهای SEO مشتق و rebuildableاند — نه business SoR.

رویدادهای معنایی (DestinationPublished · TranslationPublished · EntityArchived) ممکن است SEO را به‌روز کنند — قرارداد دقیق later.

---

## 9. Error Semantics

| وضعیت | جهت |
|-------|-----|
| واقعاً وجود ندارد | Not Found (نه soft-404 با 200) |
| عمداً برای همیشه حذف بدون جایگزینی | 410 ممکن است |
| جایگزینی معنادار | redirect به replacement مرتبط |
| شکست موقت سرور/provider | نه تبدیل به 404 دائمی |

### مثال ۸–۹، ۱۶

```text
Deleted Istanbul Tour → replacement Istanbul Tour  ✓ (if genuine)
Deleted Tour → homepage                           usually weak
HotelBooking outage ≠ delete Place Hotel SEO route
```

5xx و outage موقت ≠ nonexistent content.

---

## 10. Language Switch / Internal Links

لینک داخلی locale فعال را حفظ کند وقتی هدف published است (i18n Accepted). لینک مرده یا alternate شکسته نسازید.
