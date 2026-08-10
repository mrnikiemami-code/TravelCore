# Design System Architecture — معماری سیستم طراحی

منبع سطح‌بالا: [`../architecture/10-ui-constitution.md`](../architecture/10-ui-constitution.md)

این سند سلسله‌مراتب کامپوننت و قواعد مسئولیت را تعریف می‌کند. **فایل کامپوننت ساخته نمی‌شود.**

---

## 1. سلسله‌مراتب

```text
Design Tokens
  → Primitives
  → Composite Components
  → Domain Components
  → Sections
  → Page Archetypes
  → Actual Routes
```

---

## 2. Design Tokens

مقادیر بصری سیستمی/تکرارشونده از token می‌آیند.

### دسته‌های الزامی مفهومی

| دسته | نقش |
|------|-----|
| Color | معنایی (surface, text, border, action, …) |
| Typography | نقش‌های متنی |
| Spacing | مقیاس فاصله |
| Radius | گوشه |
| Shadow / Elevation | عمق |
| Container | عرض محتوا |
| Breakpoint | نقاط اعتبارسنجی/توکن |
| Z-index | لایه‌های معنایی |
| Motion | مدت/easing کنترل‌شده |
| Control size | اندازه کنترل |
| Touch target | حداقل ناحیه لمسی |
| Border | ضخامت/سبک |
| Surface | سطوح صفحه/کارت/panel |

### Semantic tokens

ترجیح intent:

```text
color.surface.default
color.surface.muted
color.text.primary
color.text.secondary
color.text.danger
color.border.default
color.action.primary
```

نه مقادیر فیچرمحور بی‌دلیل:

```text
tourBlue · hotelGray · visaRed
```

مگر نیاز برند/دامنه بعداً تأیید شود.

کامپوننت‌ها semantic tokens مصرف می‌کنند.

### Theme direction

تم اولیه ممکن است یک brand theme باشد. معماری token نباید dark mode / brand variants / partner themes آینده را مسدود کند.

پیاده‌سازی theme switching الان ممنوع/غیرضروری است. Over-engineering multi-brand ممنوع.

### مقادیر دلخواه

پراکندن `#17324d` / `37px` / `19px` در featureها بدون دلیل ممنوع. هر پیکسل لزوماً token نیست.

پیاده‌سازی دقیق token → P02.

---

## 3. Primitives — مثال‌های مفهومی

Button · Input · Textarea · Select · Checkbox · Radio · Switch · Badge · Avatar · Separator · Skeleton · Dialog · Sheet · Tooltip · Accordion · Tabs · Popover · Card

### قاعده Primitive

Primitive **نباید** قواعد کسب‌وکار TravelCore بداند.

`Button` نباید `TourStatus` یا `BookingStatus` بشناسد. ممکن است حالت بصری بداند (disabled, loading, …).

---

## 4. Composite Components — مثال‌های مفهومی

SearchField · DatePicker · DateRangePicker · PassengerPicker · RatingDisplay · MoneyDisplay · MixedCurrencyPrice · Breadcrumb · Pagination · FilterGroup · ImageGallery · PhoneInput · LocalizedLink

این‌ها ترکیب عمومی primitivesاند؛ هنوز Domain کامل نیستند مگر معنای دامنه داشته باشند (مثل MixedCurrencyPrice).

---

## 5. Domain Components — مثال‌های مفهومی

TourCard · HotelCard · RestaurantCard · AttractionCard · DestinationCard · AgencyCard · FlightSegment · TourHotelOption · TourPriceRow · TourItineraryDay · VisaSummary · BookingSummary

### قاعده Domain Component

ممکن است دادهٔ معنایی دامنه را برای **نمایش** بفهمد.

`MixedCurrencyPrice` می‌تواند `PriceComponent[]` را بفهمد.

**نباید** قواعد Pricing authoritative را محاسبه کند. UI دادهٔ پذیرفته‌شده را format/present می‌کند.

---

## 6. Sections — مثال‌های مفهومی

TourHeroSection · TourHotelOptionsSection · TourPricingSection · TourItinerarySection · TourServicesSection · RelatedToursSection · DestinationHeroSection · NearbyPlacesSection

Section بلوک بزرگ‌تر صفحه است؛ anatomی Page Archetype را می‌سازد.

---

## 7. Page Archetypes — مثال‌های مفهومی

ForeignTourDetailPage · ExperienceTourDetailPage · TourListingPage · DestinationLandingPage · HotelDetailPage · RestaurantDetailPage · AttractionDetailPage · ArticlePage · TraveloguePage · VisaPage · FlightSearchPage · HotelBookingSearchPage

Route واقعی = نمونهٔ محلی‌شدهٔ این archetypeها.

قرارداد مشخصات: [`04-page-archetype-contract.md`](04-page-archetype-contract.md)

---

## 8. Typography roles

نقش‌های معنایی مفهومی:

Display · Heading · Title · Body · Label · Caption · Numeric/Tabular (در صورت مفید)

فونت نهایی در این Task انتخاب نمی‌شود. Font stack آینده باید Persian / Arabic / Latin را با حداقل layout shift پشتیبانی کند.

آگاهی متریک: line-height · نسبت اسکریپت · weight mapping · CLS · fallback · numeral rendering. Fix فونت per-component دلخواه ممنوع.

---

## 9. Numbers

Locale ممکن است digit shape / separators را تعیین کند.

اما شناسه‌ها نباید اگر صحت معنایی را خراب می‌کند به ارقام محلی تبدیل شوند:

```text
EK978 · booking reference · passport number
```

تفکیک:

| نوع | رفتار |
|-----|--------|
| مقدار انسانی (قیمت نمایشی، تعداد) | formatting locale ممکن است |
| identifier/code | حفظ شکل معنایی/معمولاً LTR |

---

## 10. Container و Grid

صفحات نباید مستقلاً max-width اختراع کنند.

مفهومی:

```text
full-width surface
standard content container
wide content container
narrow editorial container
```

Layout مدرن: CSS Grid / Flexbox بر اساس نیاز معنایی.  
بازآفرینی framework 12ستونهٔ قدیمی از روی عادت ممنوع.  
همهٔ layoutها را به یک abstraction grid اجباری نکنید.

Spacing از مقیاس token؛ ترجیح `gap` به‌جای هماهنگی دستی margin فرزندان وقتی معنا دارد.

---

## 11. Color / Radius / Shadow / Z-index

Color معنایی است؛ رنگ به‌تنهایی برای success/failure/selection کافی نیست (a11y).

نقش‌های آینده: primary · secondary · success · warning · danger · info · muted.

Radius و elevation از مقیاس کنترل‌شده.  
Z-index معنایی: base · sticky · dropdown · overlay · modal · toast — نه `z-[99999]` دلخواه.

پالت برند دقیق → deferred.

---

## 12. Icons و Motion

Icon مکمل معناست؛ برای اقدام حیاتی مبهم جایگزین label نشود. جهت icon در RTL معنایی ارزیابی شود. پکیج icon → deferred.

Motion برای orientation / feedback / transition — نه تزئین صرف. `prefers-reduced-motion` رعایت شود. کتابخانهٔ animation → deferred.

---

## 13. Image / Media UX

TravelCore تصویرمحور است:

- رزرو ابعاد/aspect ratio
- جلوگیری از CLS
- تصویر responsive
- فرمت بهینه
- alt معنادار
- عدم بارگذاری original غول‌پیکر بی‌ضرورت
- گالری touch-friendly

Above-the-fold / LCP: hero مهم را عمداً اولویت دهید. کل گالری را eager نکنید.

---

## 14. Styling با Tailwind

Tailwind baseline است. استفاده باید:

- از tokens پیروی کند
- logical/direction-safe باشد
- در کامپوننت‌های reusable متمرکز شود

از پراکندن `ml-*` / `mr-*` / `pl-*` / `pr-*` / `left-*` / `right-*` در کامپوننت‌های direction-neutral پرهیز کنید. اگر utility مناسب نبود، رفتار direction-sensitive را عمداً encapsulate کنید.

---

## 15. Content density

همه‌چیز را یک‌جا نشان ندهید. از hierarchy · progressive disclosure · sections · accordion ثانویه · summary/detail استفاده کنید.

Tabs فقط وقتی peer views معنا دارند. محتوا را فقط برای زیبایی از crawl/a11y پنهان نکنید.

Accordion برای اطلاعات ثانویهٔ فشرده مفید است؛ محتوای ضروری را بی‌جهت پنهان نکند و در صورت نیاز SEO/کاربر server-rendered بماند.

---

## 16. Composition مثال ۱۰ — Destination public page

```text
DestinationLandingPage (Server)
  ├── DestinationHeroSection
  ├── Tours section (composed read / cards)
  ├── Hotels section
  ├── Attractions / Restaurants
  ├── Content / Guide teasers
  ├── UGC / Reviews teaser
  └── SEO metadata + breadcrumbs (server-renderable)
```

ترکیب با application composition / projection — نه join همه‌schema در یک query دیتابیس (طبق Data Architecture).

---

## 17. Deferred (Design System)

مقادیر دقیق token · فونت · icon · component library · Storybook · aesthetics نهایی header/footer.
