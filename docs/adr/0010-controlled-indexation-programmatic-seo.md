# ADR 0010 — Controlled Indexation and Programmatic SEO

- **Status:** Proposed
- **Date:** 2026-08-11
- **Task:** TC-P00-T006
- **Related:** [`../seo/02-indexation-crawl-and-sitemaps.md`](../seo/02-indexation-crawl-and-sitemaps.md) · [`../seo/04-search-facets-and-programmatic-seo.md`](../seo/04-search-facets-and-programmatic-seo.md)

---

## Context

پلتفرم سفر به‌راحتی دچار facet explosion و thin programmatic pages می‌شود. ایندکس کردن هر ترکیب فیلتر/جستجو و هر صفحهٔ عمومی، کیفیت جستجو و crawl budget را خراب می‌کند.

---

## Decision

1. **Public ≠ indexable.** IndexPolicy صریح لازم است.
2. ترکیب‌های دلخواه search/filter/facet **خودکار indexable نیستند.**
3. SEO Landing فقط با intent معنادار، ارزش محتوایی، پایداری مسیر، و تأیید indexability.
4. Programmatic SEO کنترل‌شده است — mass generation صرفاً به‌خاطر امکان داده ممنوع است.
5. Sitemap فقط مسیرهای canonical واجد شرایط (نه Draft/NoIndex/redirect-only/fake locale).
6. کیفیت ایندکس بر کمیت URL اولویت دارد.
7. Thin content قابل withhold از ایندکس است حتی اگر route فنی وجود داشته باشد.

---

## Alternatives Considered

| گزینه | چرا کنار گذاشته شد |
|-------|---------------------|
| همهٔ public pages ایندکس شوند | کیفیت پایین · duplicate |
| همهٔ filter combos landing شوند | facet explosion |
| بدون programmatic SEO ابدی | فرصت landingهای معنادار از دست می‌رود |
| Sitemap از raw tables | draft/noindex/redirect نشت می‌کند |

---

## Consequences

### مثبت

- کنترل crawl/index quality
- جلوگیری از thin/fake landings
- تمایز روشن Search state vs SEO landing
- پایهٔ sitemap سالم

### منفی / هزینه

- نیاز به approval/policy برای landings
- بعضی URLهای مفید کاربر noindex می‌مانند
- طراحی IndexPolicy per route kind

### Mitigation

- اسناد Landing vs Filter
- eligibility sitemap صریح
- بازبینی کیفیت قبل از index approval در فازهای بعد

---

## Migration / Impact

پیاده‌سازی IndexPolicy، sitemap generator، و landing workflow در P05/P26 و صفحات عمومی. Status تا بازبینی معمار **Proposed**.
