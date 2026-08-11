# Content Lifecycle and SEO Governance

منبع: [`../architecture/12-seo-constitution.md`](../architecture/12-seo-constitution.md)

---

## 1. Expired Tour — مثال ۷

Expired Tour خودکار 404 نیست. ممکن است ارزش محتوا/SEO نگه دارد:

- صفحه با وضعیت expired
- لینک به departureهای جدیدتر
- تورهای مرتبط
- محتوای اطلاعاتی تاریخی

سیاست دقیق به وضعیت Tour/Product وابسته است. Business availability ≠ SEO lifecycle.

---

## 2. Unavailable Product — مثال ۱۶

عدم دسترسی موقت (مثلاً بدون departure این هفته، یا outage provider) نباید خودکار equity مسیر را نابود کند.

TourProduct ممکن است مفید/indexable بماند.  
Hotel Catalog از HotelBooking جداست؛ outage رزرو زنده هویت کاتالوگ را باطل نمی‌کند.

---

## 3. Permanent Removal — مثال ۸–۹

اگر Entity واقعاً برای همیشه حذف شد:

| گزینه | کی |
|-------|-----|
| Redirect به replacement واقعی مرتبط | وقتی intent حفظ می‌شود |
| 410 Gone | حذف عمدی دائمی بدون replacement |
| 404 | نبودن واقعی بدون معنای Gone |

**Redirect کور به homepage ممنوع به‌عنوان استراتژی عمومی.**

Redirect باید intent کاربر/جستجو را حفظ کند.

---

## 4. Archived Editorial

Archive ≠ خودکار noindex. اگر مفید و دقیق است ممکن است indexable بماند. سیاست عمدی لازم است.

---

## 5. Surface Guidance

| Surface | جهت |
|---------|------|
| Destination | سطح SEO عمده؛ ترکیب Tours/Hotels/Content/UGC — SEO مالک datasetها نیست |
| Place (Hotel/Restaurant/Attraction) | مسیر عمومی وقتی publication اجازه دهد؛ live booking لازم نیست |
| TourProduct | هویت پایدار محصول؛ پیش‌فرض route پایدار؛ Departureها داخل آن |
| TourDeparture URLs | هر Departure لزوماً indexable route ندارد — deferred؛ جلوگیری از explosion |
| Content | دارایی SEO پایدار؛ Content مالک substance؛ SEO مالک route |
| UGC | فقط با کیفیت/moderation؛ هر review خودکار صفحهٔ indexable نیست |

---

## 6. Internal Linking

از روابط معنایی واقعی: Destination↔Tours · Tour→Destination · Hotel→Destination · Article→Destination · Destination→Attractions.

بلوک لینک بی‌ربط عظیم فقط برای crawler ممنوع.  
Locale هدف وقتی published است حفظ شود. هدف unavailable → رفتار صریح، نه لینک مرده.

---

## 7. Route Publication

قبل از public publish: uniqueness/reservation · locale publication · IndexPolicy.  
Consistency قوی برای جلوگیری از duplicate path. رویدادهای کسب‌وکار ممکن است projection SEO را به‌روز کنند.

---

## 8. Rendering / Performance

محتوای حیاتی server-renderable (ADR 0005).  
CWV رعایت شود؛ bundle بزرگ، CLS، duplicate render، third-party بیش‌ازحد برای SEO ممنوع به‌عنوان الگو.

---

## 9. Title Uniqueness

نگرانی کیفیت است. با تغییر مخرب عنوان کسب‌وکار uniqueness را «ضمانت» نکنید؛ ترکیب/override SEO ممکن است intent را متمایز کند.

---

## 10. Testing Expectations

حداقل اعتبارسنجی آینده:

canonical URL · redirect chain/loop · localized route · hreflang published equivalents · index/noindex · sitemap eligibility · 404 · 410 (جایی که عمدی) · soft-404 prevention · structured data validity · server-rendered critical content · filter/query indexation · pagination · expired product · locale integrity

ابزار دقیق → deferred.

---

## 11. Monitoring Direction

آینده: orphan routes · redirect health · index coverage quality · soft-404 · sitemap anomalies · locale integrity. جزئیات tooling later.

---

## 12. Anti-Pattern Checklist

- [ ] Every filter/search combination indexable
- [ ] Every TourDeparture automatic SEO page
- [ ] Fake translated / silent cross-language pages
- [ ] Canonical everything to homepage
- [ ] Redirect every deletion to homepage
- [ ] Redirect loops / long chains
- [ ] Soft-404 (200 + not found)
- [ ] Draft / noindex / redirect-only in sitemap
- [ ] SEO owns Tour/Hotel/Destination business content
- [ ] Search as SEO SoR
- [ ] Client-only critical SEO content
- [ ] Hardcoded scattered JSON-LD
- [ ] Fake ratings/prices / IRR as Toman / silent FX
- [ ] Tracking params in canonical
- [ ] Global URL string on every entity / slug as business PK
- [ ] Slug change without redirect history
- [ ] robots.txt / noindex as security
