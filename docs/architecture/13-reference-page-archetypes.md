# Reference Page Archetypes — کهن‌الگوهای صفحهٔ عمومی TravelCore

این سند فلسفهٔ Page Archetype را قفل می‌کند. مشخصات تفصیلی در [`../pages/`](../pages/).

مرجع‌های UI/i18n/SEO: [`10-ui-constitution.md`](10-ui-constitution.md) · [`11-internationalization-architecture.md`](11-internationalization-architecture.md) · [`12-seo-constitution.md`](12-seo-constitution.md)

---

## 1. Archetype ≠ Route Instance ≠ Aggregate

| مفهوم | معنی |
|-------|------|
| **Page Archetype** | قرارداد ساختاری قابل‌استفادهٔ مجدد |
| **Route Instance** | URL محلی‌شده که archetype را render می‌کند |
| **Domain Aggregate** | مالکیت کسب‌وکار در ماژول |

```text
ForeignTourDetailPage          ← archetype
/fa/tours/...                  ← route instance (conceptual)
TourProduct                    ← domain identity (not a page)
```

یک مشخصات صفحه برای هر تور جدا نسازید.

---

## 2. Composition بدون تغییر مالکیت

صفحه ممکن است از چند ماژول بخواند. این مالکیت دامنه را عوض نمی‌کند.

مثال Destination Landing: Destination + Tour + Place + Content + UGC + SEO.

بدون: cross-module DbContext · EF Include بین‌ماژولی · SQL join همه‌schema.

---

## 3. Page View Model

```text
Domain Model ≠ Persistence Model ≠ API Contract ≠ Page View Model
```

صفحات آینده از purpose-built page/read contracts مصرف می‌کنند — نه EF Entity به‌عنوان DTO صفحه.

---

## 4. قرارداد الزامی Archetype

هر archetype باید پوشش دهد:

Purpose · Primary/Secondary intents · Primary/Secondary CTA · Target modules · Required/Optional data · Anatomy · Above-the-fold · Desktop/Tablet/Mobile · RTL/LTR · Bidi · Loading/Empty/Error/Unavailable-Expired · Accessibility · SEO role · Indexability direction · Canonical/locale relationship · Internal linking · Structured-data candidates · Performance risks · Analytics intent · Future notes · Non-goals

جزئیات حالت‌ها: [`../pages/09-page-state-and-composition-rules.md`](../pages/09-page-state-and-composition-rules.md)

---

## 5. Above-the-Fold

محتوای تصمیم‌ساز زود دیده شود. بنر تزئینی غول‌پیکر / بلوک بازاریابی بی‌ربط قبل از اطلاعات اصلی ممنوع به‌عنوان الگو.

---

## 6. Reference-Site Rule

LastSecond / TahaGasht فقط برای: IA · coverage · anatomy · decision support · filter/booking concepts · hierarchy.

**کپی نشود:** کد · متن دارای حق نشر · تصویر · برند · visual exact · CSS · اندازه‌ها · presentation اختصاصی.

الگوی دیده‌شده فقط وقتی الزام TravelCore می‌شود که با user intent · دامنه · معماری پذیرفته‌شده · ارزش محصول توجیه شود.

رجیستری: [`../reference-sites/page-registry.md`](../reference-sites/page-registry.md)

---

## 7. i18n / SEO / UI Alignment

- یک archetype برای fa/en/ar — نه ForeignTourFa/En/Ar جدا
- بدون silent cross-language fallback (ADR 0008)
- Search Results ≠ SEO Landing؛ Filter URL ≠ controlled landing (ADR 0010)
- Server Component first؛ محتوای SEO حیاتی server-renderable (ADR 0005)
- RTL منطقی؛ bidi جدا (ADR 0006)

---

## 8. Non-Goals این Task

Home نهایی · Checkout · Payment · live Flight search · live HotelBooking search · Admin · Agency Panel · visual design · component library · fixture/code

---

## 9. Intentionally Deferred

طراحی بصری · فونت/رنگ/آیکون · نام فایل کامپوننت · URL نهایی جایی که قفل نشده · analytics taxonomy کامل · JSON-LD mapping دقیق · لیست فیلتر نهایی · اندازه‌های sticky · map/gallery library · wizard رزرو

---

## 10. Anti-Patterns

- یک detail page غول‌پیکر برای همه چیز
- Place subtypes با معنای دامنهٔ یکسان
- Foreign Tour == Experience Tour layout
- Page DTO == EF Entity
- composition با DbContext بین‌ماژولی
- موبایل = دسکتاپ فشرده
- hover برای اقدام حیاتی
- fake translated pages
- client-only critical SEO content
- هر فیلتر indexable
- هر TourDeparture صفحهٔ SEO
- Tour مالک Hotel catalog
- ناپدید شدن Hotel با down بودن HotelBooking
- جمع ضمنی ارزهای ترکیبی
- قیمت 0 به‌معنی unavailable
- expired → خودکار 404
- شکست بخش ثانویه → شکست کل صفحه
- همهٔ sectionها اولویت برابر
- دیوار متن SEO بالای intent کاربر
- path به‌عنوان domain PK
