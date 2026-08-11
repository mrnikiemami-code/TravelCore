# Search, Facets, and Programmatic SEO

منبع: [`../architecture/12-seo-constitution.md`](../architecture/12-seo-constitution.md)  
ADR مرتبط (Proposed): [`../adr/0010-controlled-indexation-programmatic-seo.md`](../adr/0010-controlled-indexation-programmatic-seo.md)

---

## 1. Search ≠ SEO

| Search | SEO |
|--------|-----|
| index · ranking · facets · autocomplete · query projections | indexable public routes · canonical · robots · sitemap · landing semantics |

Search derived است، SoR محتوا/SEO نیست.  
Search result state خودکار SEO page نیست.

---

## 2. Search Query URL — مثال ۴

```text
/fa/search?q=istanbul
```

برای کاربر مفید است. **خودکار canonical SEO landing نیست.**  
معمولاً نباید صفحهٔ indexable کنترل‌نشده شود. رفتار دقیق noindex/crawl → later.

---

## 3. Filter URL — مثال ۵

```text
/fa/tours?destination=istanbul&duration=7&hotel=5-star
```

حالت listing فیلترشدهٔ کارکردی است. **خودکار SEO landing / indexable نیست.**

Sort params (`?sort=price`) معمولاً presentationاند.

---

## 4. Facet Explosion

ابعاد نمونه: destination · origin · date · duration · hotel stars · airline · meal · price · agency · transport · visa

ترکیب‌های دلخواه همه‌شان indexable URL نمی‌شوند. این قانون صریح است.

---

## 5. SEO Landing Page — مثال ۶

صفحهٔ indexable تأییدشده که intent جستجوی معنادار را نمایندگی می‌کند.

```text
Controlled: /fa/tours/istanbul   (when route design + publication approve)
≠ arbitrary filter state
```

---

## 6. Controlled Programmatic SEO

مجاز فقط وقتی:

- search intent معنادار
- محتوا به‌اندازهٔ کافی متمایز
- کیفیت داده بالا
- مسیر پایدار
- indexability صریحاً تأیید شده
- ریسک thin/duplicate کنترل شده

Mass-generate فقط چون داده ساخت‌یافته اجازه می‌دهد ممنوع است.

مالکیت: SEO ممکن است route و تعریف/projection landing را مالک باشد؛ حقایق از Destination/Tour/Place/Content/Pricing می‌آیند. دیتابیس موازی محصول SEO ممنوع.

Editorial landings: Content مالک بدنه؛ SEO مالک route/index/canonical. Composition بین‌ماژولی مالکیت را عوض نمی‌کند.

SeoRoute ≠ الزام CMS page برای هر مسیر. بعضی مسیرها مستقیم به Destination/Tour/Hotel/Article وصل‌اند؛ بعضی landing کنترل‌شده‌اند.

---

## 7. Thin Content Guard

مسیر فنی معتبر اگر ارزش یکتای کافی ندارد (stub یک‌خطی، landing خالی تور، auto page بدون محتوا) نباید خودکار ایندکس شود. IndexPolicy می‌تواند withhold کند.

---

## 8. Duplicate Content Causes

query parameter order · tracking · legacy slugs · case · trailing slash · duplicate patterns · locale mistakes

Canonicalization و redirect عمدی لازم است.
