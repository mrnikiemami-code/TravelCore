# ADR 0008 — Translation Publication and Fallback Policy

- **Status:** Accepted
- **Date:** 2026-08-11
- **Task:** TC-P00-T005
- **Related:** [`../i18n/04-fallback-and-publication-policy.md`](../i18n/04-fallback-and-publication-policy.md) · Constitution § i18n

---

## Context

چندزبانه بودن بدون سیاست انتشار/fallback باعث صفحات جعلی می‌شود: URL انگلیسی با بدنهٔ فارسی، یا فرض اینکه وجود ردیف ترجمه برابر انتشار است. این هم UX و هم یکپارچگی زبانی SEO را خراب می‌کند. در عین حال UI vocabulary بدون هیچ fallback شکننده می‌شود.

---

## Decision

1. **Publication is per locale.** انتشار تجاری Entity ⇒ همهٔ localeها منتشر نیستند.
2. **Translation existence ≠ publication.** Draft/machine/missing با Published یکی نیستند.
3. برای محتوای کسب‌وکار/editorial عمومی: **silent cross-language fallback تحت URL locale درخواستی ممنوع** است (مثلاً `/en/...` با محتوای فارسی دائمی).
4. **UI resources** ممکن است fallback کنترل‌شده داشته باشند (مثلاً `ar-AE → ar → configured fallback`).
5. **Admin/internal** ممکن است fallback صریح و برچسب‌دار نشان دهد (`Fallback from fa`) — بدون ایجاد publication.
6. Fallback سیاست centralized دارد؛ هر کامپوننت سیاست خودش را اختراع نمی‌کند.
7. Fallback ≠ publication.
8. فقط معادل‌های واقعاً published کاندید alternate-language/hreflang هستند (خروجی تفصیلی → T006).
9. Business availability ≠ translation availability برای صفحهٔ عمومی.

---

## Alternatives Considered

| گزینه | چرا کنار گذاشته شد |
|-------|---------------------|
| Always fall back to source locale on public pages | fake multilingual pages؛ lang نادرست |
| No UI fallback at all | UI شکننده برای localeهای جزئی |
| Row exists = auto Published | کیفیت و completeness نادیده گرفته می‌شود |
| Auto-publish machine translation | کیفیت و اعتماد خراب می‌شود |
| Component-local ad-hoc fallback | رفتار ناسازگار و غیرقابل‌ممیزی |

---

## Consequences

### مثبت

- یکپارچگی زبان صفحه و URL
- جلوگیری از fake SEO multilingual
- تفکیک واضح UI vs content fallback
- پشتیبانی workflow editorial (Draft/Ready/Published)

### منفی / هزینه

- برخی locale routes ممکن است موقتاً unavailable باشند
- نیاز به UX/SEO صریح برای missing locale
- ویراستاران باید completeness را مدیریت کنند

### Mitigation

- Admin indicators برای Missing/Draft/Published
- Language switcher unavailable states
- Observability برای missing required translations
- T006 برای رفتار HTTP/canonical

---

## Migration / Impact

قواعد validation انتشار در فازهای ماژول؛ routing/SEO در T006/P02. بدون پیاده‌سازی در این Task. Status تا بازبینی معمار **Proposed**.
