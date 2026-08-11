# SEO Constitution — قانون اساسی SEO TravelCore

این سند **منبع حقیقت سطح‌بالای SEO Architecture** TravelCore است. قبل از پیاده‌سازی صفحات عمومی محصول باید خوانده شود.

جزئیات:

| سند | نقش |
|-----|------|
| [`../seo/01-route-canonical-and-redirects.md`](../seo/01-route-canonical-and-redirects.md) | SeoRoute · canonical · redirect · hreflang boundary |
| [`../seo/02-indexation-crawl-and-sitemaps.md`](../seo/02-indexation-crawl-and-sitemaps.md) | IndexPolicy · robots · sitemap · pagination |
| [`../seo/03-structured-data-and-metadata.md`](../seo/03-structured-data-and-metadata.md) | title · JSON-LD · breadcrumbs · truthfulness |
| [`../seo/04-search-facets-and-programmatic-seo.md`](../seo/04-search-facets-and-programmatic-seo.md) | Search≠SEO · filters · landing · thin content |
| [`../seo/05-content-lifecycle-and-seo-governance.md`](../seo/05-content-lifecycle-and-seo-governance.md) | expired · removal · linking · quality · tests |
| [`11-internationalization-architecture.md`](11-internationalization-architecture.md) · ADR 0007/0008 | locale routes · publication |
| [`10-ui-constitution.md`](10-ui-constitution.md) · ADR 0005 | Server render · CWV |

ADRهای مرتبط این Task در وضعیت **Proposed** هستند تا بازبینی معمار.

---

## 1. مالکیت — قفل

### SEO مالک است

SeoRoute · LocalizedSlug · Canonical · Redirect history · IndexPolicy · CrawlPolicy · SeoMetadata overrides (در صورت نیاز صریح) · قواعد ترکیب StructuredData · قواعد مشارکت Sitemap

### SEO مالک نیست

Tour title · Hotel description · Destination content · Article body · Booking state · Pricing rules

ماژول‌های کسب‌وکار منبع حقیقت محتوا باقی می‌مانند. SEO مکانیک discoverability را مالک است — نه دیتابیس موازی محصول.

---

## 2. هویت مسیر ≠ هویت کسب‌وکار

### مثال ۱

```text
DestinationId = one UUID
/fa/destinations/استانبول
/en/destinations/istanbul
/ar/destinations/اسطنبول
```

Entityها per locale duplicate نمی‌شوند. ADR 0007: مسیرهای عمومی locale-prefixed.

Conceptually:

```text
localized path → SeoRoute → ResourceType + ResourceId → application read model
```

Slug/مسیر PK کسب‌وکار نیست.

---

## 3. کیفیت ایندکس — هدف

هدف = حداکثر تعداد URL ایندکس‌شده نیست.

هدف:

```text
high-quality · stable · intentful · unique · useful
```

`public ≠ indexable`. صفحه ممکن است در دسترس باشد ولی `noindex` عمدی داشته باشد.

---

## 4. Canonical و Hreflang (خلاصه)

- هر مسیر indexable باید canonical عمدی داشته باشد (معمولاً self-canonical)
- Canonical جایگزین طراحی تکراری مسیر نمی‌شود
- Canonical ترجمه جعلی نمی‌سازد (ADR 0008)
- Hreflang فقط برای معادل‌های واقعاً published

### مثال ۲

```text
fa Published + en Published + ar Missing
→ alternates: fa, en  (NOT ar)
```

---

## 5. Search ≠ SEO

| ماژول | نقش |
|-------|-----|
| Search | index · ranking · facets · autocomplete · projections |
| SEO | indexable route strategy · canonical · robots · sitemap · landing semantics |

### مثال ۴–۶

```text
/fa/search?q=istanbul          → NOT automatic SEO landing
/fa/tours?destination=istanbul&hotel=5  → NOT automatic indexable
/fa/tours/istanbul             → controlled SEO landing (when approved)
```

Facet explosion ممنوع. Programmatic SEO فقط با intent/value/approval صریح.

---

## 6. Lifecycle محتوا (خلاصه)

### مثال ۷–۹، ۱۶

```text
Expired Tour ≠ automatic 404
Temporary unavailable ≠ permanent removal
Permanent removal → relevant replacement redirect OR 410/404 (not blind homepage)
Hotel catalog page survives HotelBooking provider outage
```

Soft-404 (HTTP 200 با «یافت نشد») ممنوع.

---

## 7. Structured Data (خلاصه)

JSON-LD جهت ترجیحی است. SEO ترکیب می‌کند؛ SoR کسب‌وکار باقی می‌ماند.

حقیقت: قیمت · availability · rating — بدون جعل.  
Mixed-currency: جمع/تبدیل جعلی ممنوع (ADR 0003). IRR≠Toman در schema.

### مثال ۱۱، ۱۵، ۱۷

Breadcrumb از سلسله‌مراتب معنایی Destination؛ محتوای SEO-sensitive server-renderable (ADR 0005).

---

## 8. ضدالگوهای SEO

- هر ترکیب فیلتر/جستجو indexable
- هر TourDeparture خودکار SEO page
- fake translated pages / silent cross-language fallback
- canonical همه‌چیز به homepage
- redirect هر حذف به homepage
- redirect loop / زنجیرهٔ بلند
- soft-404
- draft/noindex/redirect در sitemap
- SEO مالک محتوای Tour/Hotel/Destination
- Search به‌عنوان SoR SEO
- محتوای SEO حیاتی فقط بعد از hydration
- JSON-LD هاردکد پراکنده
- rating/price جعلی · Toman/IRR قاطی
- tracking params در canonical
- یک فیلد URL سراسری روی هر Entity
- slug به‌عنوان business PK
- تغییر slug بدون redirect history
- robots/noindex به‌عنوان امنیت/access control

---

## 9. Intentionally Deferred Decisions

- schema دقیق DB / EF برای SeoRoute
- generator slug و Unicode normalization دقیق
- الگوی route نهایی همهٔ ماژول‌ها
- hostname تولید · www/non-www · trailing slash framework setting
- اندازهٔ partition sitemap · Next.js metadata API · JSON-LD library
- mapping دقیق schema.org per page
- Search Console / Bing / analytics / RUM
- robots.txt دقیق · x-default دقیق
- HTTP دقیق unavailable-locale فراتر از مرز i18n
- indexability دقیق TourDeparture · آستانهٔ UGC
- workflow تأیید programmatic landing

---

## 10. ADRهای Proposed این Task

| ADR | موضوع |
|-----|--------|
| [`../adr/0009-centralized-seo-route-ownership.md`](../adr/0009-centralized-seo-route-ownership.md) | مالکیت متمرکز مسیر SEO |
| [`../adr/0010-controlled-indexation-programmatic-seo.md`](../adr/0010-controlled-indexation-programmatic-seo.md) | ایندکس کنترل‌شده · programmatic SEO |

وضعیت: **Proposed** — تا پذیرش معمار Accepted نشوند.
