# Structured Data and Metadata

منبع: [`../architecture/12-seo-constitution.md`](../architecture/12-seo-constitution.md)  
Money ADR 0003 · i18n ADR 0008 · Server Component ADR 0005

---

## 1. Metadata Ownership

| نگرانی | مالک |
|--------|------|
| حقایق کسب‌وکار (نام، توضیح) | ماژول کسب‌وکار / Content |
| ترکیب title/description mechanics · overrides فنی | SEO |
| preview اجتماعی | presentation/SEO mechanics؛ reuse از ماژول‌های authoritative |

Title از business content + naming محلی + page type + brand policy ترکیب می‌شود.  
Description ممکن است editorial override یا derived باشد — اجباری دستی برای هر Entity اگر ترکیب باکیفیت کافی است، نه.

Override کل مدل محتوا را duplicate نکند.

### Localization — مثال metadata

`/en/...` نباید title/description فارسی را به‌عنوان fallback دائمی emit کند (ADR 0008).

---

## 2. Open Graph / Social

ممکن است title · description · media محلی‌شده را reuse کند.  
جزئیات پلتفرم‌محور → deferred.

---

## 3. Structured Data / JSON-LD

جهت ترجیحی: **JSON-LD** جایی که کاربرد دارد.

- الان پیاده نشود
- blob هاردکد در کامپوننت‌های تصادفی ممنوع
- ترکیب reusable و عمدی

SEO ترکیب می‌کند؛ SoR:

Tour · Hotel · Destination · Organization · Breadcrumb facts ← ماژول‌های کسب‌وکار

### Truthfulness

Structured data با محتوای قابل‌مشاهده و وضعیت کسب‌وکار جور باشد.

جعل ممنوع: price · availability · rating · review · event date.

### Types بالقوه (قفل mapping نیست)

BreadcrumbList · Organization · WebSite · Article · Hotel · TouristDestination · Product · Offer · Review · AggregateRating · Event

Mapping دقیق در پیاده‌سازی صفحه/ماژول اعتبارسنجی شود.

---

## 4. Breadcrumbs — مثال ۱۱

```text
Home > Turkey > Istanbul > Tour
```

از سلسله‌مراتب معنایی (Destination graph) — نه صرفاً بخش‌های URL اگر دامنه فرق دارد.  
Destination مالک hierarchy است؛ SEO مصرف می‌کند.

---

## 5. Price / Currency / Mixed — مثال ۱۵

Locale ≠ Currency. Structured pricing `CurrencyCode` واقعی را حفظ می‌کند.

IRR را به‌عنوان Toman در schema برچسب نزنید.

برای mixed-currency: جمع/تبدیل جعلی برای جا شدن در schema ممنوع.  
اگر schema نمی‌تواند صادقانه بیان کند → representation کاهش‌یافتهٔ صادق یا حذف جزئیات پشتیبانی‌نشده.

---

## 6. Availability و Rating

Availability از وضعیت authoritative کسب‌وکار.  
AggregateRating فقط با rating/review واقعی واجد شرایط — شمارش مصنوعی ممنوع.

---

## 7. Structured Data Language

مقادیر متنی schema با محتوای localized قابل‌مشاهده هم‌خوان باشند.  
عنوان فارسی روی صفحهٔ انگلیسی عمدی نباشد مگر محتوا واقعاً آن زبان باشد.

---

## 8. Rendering

محتوای SEO حیاتی server-renderable است (ADR 0005): title · primary content · breadcrumbs · main semantic content · important links.

JS برای تعامل مجاز است؛ کشف نباید فقط بعد از hydration غیرضروری باشد.
