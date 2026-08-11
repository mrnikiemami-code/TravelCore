# Fallback and Publication Policy

منبع: [`../architecture/11-internationalization-architecture.md`](../architecture/11-internationalization-architecture.md)
ADR مرتبط (Accepted): [`../adr/0008-translation-publication-fallback.md`](../adr/0008-translation-publication-fallback.md)

---

## 1. سه سیاست جدا

| سیاست | حوزه |
|-------|------|
| UI fallback policy | واژگان رابط |
| Entity/editorial fallback policy (non-public) | Admin / internal / debug |
| Public publication policy | صفحات indexable/عمومی |

هر کامپوننت fallback خودش را اختراع نکند — سیاست centralized/configured.

Fallback ≠ Publication.

---

## 2. Translation Existence ≠ Publication

وجود ردیف ترجمه یا machine draft به معنای صفحهٔ عمومی منتشر نیست.

Publication **per locale** است.

### مثال ۵

```text
fa Published · en Draft · ar Missing
```

---

## 3. ممنوع: Fake Translated Public Page

### مثال ۶ — Forbidden

```text
URL: /en/tours/istanbul
Body/title: Persian content as permanent fallback
```

TravelCore نباید مسیر انگلیسی را با محتوای فارسی کسب‌وکار به‌خاطر missing English به‌صورت دائمی پر کند.

این UX و معنای SEO را خراب می‌کند.

---

## 4. Public Fallback Policy

برای محتوای Entity/editorial عمومی/indexable:

**Fallback نباید ترجمهٔ منتشرشده جعل کند.**

اگر locale درخواستی محتوای publishable ندارد → سیاست unavailable-locale صریح:

- پیشنهاد language switch
- redirect طبق قواعد تأییدشدهٔ SEO
- not-found / unavailable

جزئیات HTTP/canonical → **TC-P00-T006**.

Silent presentation زبان دیگر تحت URL locale درخواستی **ممنوع** است.

---

## 5. UI Fallback — متفاوت — مثال ۷

UI Translation ممکن است fallback کنترل‌شده داشته باشد:

```text
ar-AE → ar → configured fallback locale
```

این مجاز است چون واژگان UI متفاوت از تظاهر ترجمهٔ محتوای کسب‌وکار است.

Production نباید کلید خام مثل `tour.book_now` را به‌عنوان رفتار عادی به کاربر نشان دهد (dev ممکن است missing keys را تهاجمی نشان دهد). کتابخانه → deferred.

---

## 6. Admin / Internal Explicit Fallback — مثال ۸

در Admin/search/debug ممکن است برای usability مقدار fallback نشان داده شود با برچسب صریح:

```text
Fallback from fa
```

این با publication یکی نیست و باید قابل ردیابی باشد وقتی تصمیم editorial به آن وابسته است.

---

## 7. Language Integrity for Search Engines

صفحه‌ای که `lang="en"` اعلام می‌کند باید وقتی منتشر عمومی است محتوای اصلی واقعاً انگلیسی داشته باشد.

بدنهٔ فارسی زیر مسیر انگلیسی فقط برای پوشش URL ممنوع است.
صحت زبان > کامل‌بودن ظاهری locale routes.

---

## 8. Programmatic SEO Guard

سیستم i18n نباید خودکار هر Entity را به هر locale route عمومی تبدیل کند.

مسیر عمومی locale فقط وقتی سیاست انتشار اجازه می‌دهد وجود دارد.
قواعد indexation تفصیلی → T006.

---

## 9. Hreflang Prerequisite (مرز)

فقط معادل‌های واقعاً available/published کاندید رابطهٔ alternate-language هستند.

برای ترجمه‌های missing، alternate جعلی نسازید. خروجی hreflang → T006.

---

## 10. Language Switch Unavailable

اگر معادل منتشر نیست: switcher unavailable را نشان دهد یا جایگزین صریح — نه لینک مرده، نه تظاهر وجود ترجمه.

---

## 11. Business vs Translation Availability

```text
TourDeparture: Bookable
fa product page: Published
en product page: Missing
→ commercial ops may continue; English public page may be unavailable
```

این‌ها را قاطی نکنید.

---

## 12. Cache Invalidation Direction

Publish/update یک locale باید projection/cache مرتبط همان locale را invalidate کند.
لزوماً همهٔ localeها را وقتی فقط فارسی عوض شده invalidate نکنید مگر دادهٔ غیرمحلی مشترک هم عوض شده باشد.

پیاده‌سازی دقیق → later.
