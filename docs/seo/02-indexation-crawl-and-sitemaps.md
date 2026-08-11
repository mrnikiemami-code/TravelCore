# Indexation, Crawl, and Sitemaps

منبع: [`../architecture/12-seo-constitution.md`](../architecture/12-seo-constitution.md)
ADR مرتبط (Accepted): [`../adr/0010-controlled-indexation-programmatic-seo.md`](../adr/0010-controlled-indexation-programmatic-seo.md)

---

## 1. IndexPolicy صریح

Indexability استنباط صرف از «صفحه وجود دارد» یا HTTP 200 نیست.

وضعیت‌های مفهومی: Index · NoIndex (و بعداً crawl/link semantics در صورت نیاز).

### Public ≠ Indexable

صفحه ممکن است عمومی باشد ولی عمداً noindex (کمپین موقت، بعضی filter views، workflowهای غیرحساب).

### NoIndex ≠ Security

noindex دستور SEO است، نه کنترل دسترسی. دادهٔ خصوصی با authorization محافظت می‌شود — نه robots/noindex.

---

## 2. Robots

Robots crawl را عمدی کنترل می‌کند.

- ابزار اصلی حذف URLهای از قبل ایندکس‌شده نیست
- منابع لازم برای render/index صفحات مهم را تصادفی block نکند
- امنیت نیست

خروجی دقیق robots → later.

---

## 3. Sitemaps

Sitemap از مسیرهای canonical واقعاً indexable تولید می‌شود.

Partition مفهومی آینده: Destinations · Tours · Places · Content · Visa · …

نه از raw table بدون فیلتر SEO/publication.

### مثال ۱۲ — Eligibility

معمولاً ورود به sitemap وقتی:

- public
- locale publication وجود دارد
- canonical وجود دارد
- IndexPolicy اجازه می‌دهد
- lifecycle اجازهٔ discovery می‌دهد

**خارج از sitemap:** Draft · fake locale · redirect-only · noindex

مقیاس: sitemap index/partitioning با رشد حجم — یک فایل static ابدی معماری نیست. حدود دقیق → later.

---

## 4. Query Parameters

Tracking params canonical variant نمی‌سازند.

Sort (`?sort=price`) معمولاً presentation است — نه صفحهٔ indexable جدا.

Filter/search query → بخش [`04-search-facets-and-programmatic-seo.md`](04-search-facets-and-programmatic-seo.md).

---

## 5. Pagination

### مثال ۱۴

Listing صفحه‌بندی‌شده باید URL پایدار داشته باشد (`?page=2`).
کشف فقط به infinite scroll کلاینت وابسته نباشد؛ UX scroll مجاز است ولی مسیر crawlable لازم است.

**Canonical هر صفحه به page 1 به‌صورت خودکار ممنوع نیست** — هر صفحه ممکن است محتوای لیست متمایز داشته باشد. nuance عمدی است.

---

## 6. Crawl Behavior Direction

Index quality > URL quantity.
Facet explosion و thin content را crawl/index نکنید.
Duplicate از query order، tracking، slash/case، route pattern، locale mistake با canonical/redirect کنترل شود.
