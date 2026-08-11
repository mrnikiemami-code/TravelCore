# واژه‌نامه دامنه TravelCore

هدف این سند تثبیت **نام انگلیسی کاننیکال**، توضیح فارسی، و تمایزهای خطرناک است تا توسعه‌دهنده و Agent یک زبان مشترک داشته باشند.

اصول مرتبط: [`../architecture/00-constitution.md`](../architecture/00-constitution.md)

---

## تمایزهای حیاتی (ابتدا بخوانید)

| تمایز | یادداشت |
|-------|---------|
| TourProduct ≠ TourDeparture | محصول می‌تواند چند Departure زمان‌بندی‌شده داشته باشد |
| Hotel Catalog ≠ Hotel Booking | «این هتل چیست؟» در برابر «الان قابل رزرو است؟» |
| Price ≠ Quote ≠ Payment | نرخ پایه ≠ پیشنهاد لحظه‌ای ≠ تسویه |
| PassengerCategory ≠ Occupancy | Adult/Child در برابر Single/Double/ExtraBed |
| Locale ≠ Currency ≠ Calendar ≠ Timezone | ترجیح‌های مستقل کاربر/بازار |
| Domain Model ≠ API Contract ≠ Persistence Model ≠ Page View Model | شباهت ساختاری ≠ یکی بودن |

---

## اصطلاحات

### TravelCore

- **Canonical:** TravelCore
- **فارسی:** پلتفرم یکپارچهٔ سفر چندزبانه (کشف + کاتالوگ + commerce + محتوا + SEO).
- **اشتباه رایج:** دانستن TravelCore به‌عنوان صرفاً «سایت تور» یا کپی یک رقیب.

### Destination

- **Canonical:** Destination
- **فارسی:** گرهٔ مکانی در سلسله‌مراتب جغرافیایی/کشف (قاره تا محله) و مرکز گراف SEO.
- **اشتباه رایج:** یکی‌دانستن Destination با Place تکی (هتل/رستوران).

### Place

- **Canonical:** Place
- **فارسی:** موجودیت کاتالوگ مکانی؛ می‌تواند Hotel / Restaurant / Attraction باشد.
- **اشتباه رایج:** ادغام Place با قابلیت Booking.

### Hotel

- **Canonical:** Hotel
- **فارسی:** گونهٔ Place که هویت و مشخصات هتل را توصیف می‌کند.
- **اشتباه رایج:** یکی‌دانستن با HotelBooking.

### Restaurant

- **Canonical:** Restaurant
- **فارسی:** گونهٔ Place برای رستوران/تجربهٔ غذایی.

### Attraction

- **Canonical:** Attraction
- **فارسی:** گونهٔ Place برای جاذبهٔ گردشگری.

### TourProduct

- **Canonical:** TourProduct
- **فارسی:** تعریف محصول تور (تجربه یا پکیج خارجی) مستقل از یک تاریخ حرکت خاص.
- **اشتباه رایج:** یکی‌دانستن با TourDeparture.

### ExperienceTour

- **Canonical:** ExperienceTour
- **فارسی:** کهن‌الگوی تور تجربه‌محور/داخلی با Itinerary ساخت‌یافته، توقف‌ها، تجهیزات و سختی.

### PackageTour

- **Canonical:** PackageTour (Foreign Package Tour)
- **فارسی:** کهن‌الگوی پکیج خارجی با پرواز، هتل، Departure و قواعد اشغال/قیمت.

### TourDeparture

- **Canonical:** TourDeparture
- **فارسی:** یک نوبت/حرکت زمان‌بندی‌شده از یک TourProduct.
- **اشتباه رایج:** ادغام با خود محصول.

### Itinerary

- **Canonical:** Itinerary
- **فارسی:** برنامهٔ سفر ساخت‌یافته (نه فقط یک HTML بزرگ).

### ItineraryDay

- **Canonical:** ItineraryDay
- **فارسی:** یک روز از برنامه با فعالیت‌ها/وعده‌ها/حمل‌ونقل محلی مرتبط.

### Stop

- **Canonical:** Stop
- **فارسی:** توقف در مسیر که می‌تواند به Destination یا Place واقعی ارجاع دهد.

### TransportSegment

- **Canonical:** TransportSegment
- **فارسی:** قطعهٔ حمل‌ونقل عمومی‌تر در پکیج (ممکن است پرواز یا سایر مدها).

### FlightSegment

- **Canonical:** FlightSegment
- **فارسی:** سگمنت پروازی با origin/destination، فرودگاه، carrier، شماره پرواز، زمان محلی، timezone، کلاس، بار.
- **مثال مفاهیم:** IKA → IST، EK978.

### TourHotelOption

- **Canonical:** TourHotelOption
- **فارسی:** گزینهٔ هتلِ وابسته به تور که با HotelId به کاتالوگ ارجاع می‌دهد و حقایق تورمحور (شب، MealPlan، ترکیب نرخ) را نگه می‌دارد.
- **اشتباه رایج:** کپی کامل Entity هتل داخل ماژول Tour یا navigation مستقیم EF به Place.

### MealPlan

- **Canonical:** MealPlan
- **فارسی:** طرح وعده غذایی (مثلاً در روز itinerary یا در گزینه هتل پکیج).

### Passenger

- **Canonical:** Passenger
- **فارسی:** مسافر واقعی/نمونه در زمینهٔ Quote یا Booking.

### PassengerCategory

- **Canonical:** PassengerCategory
- **فارسی:** طبقهٔ مسافر: Adult / Child / Infant و مشابه.
- **اشتباه رایج:** قاطی‌کردن با Occupancy.

### Occupancy

- **Canonical:** Occupancy
- **فارسی:** شرایط اشغال اقامت: Single / Double / ExtraBed / ChildWithBed / ChildWithoutBed.

### AgePolicy

- **Canonical:** AgePolicy
- **فارسی:** قواعد سنی مرتبط با دستهٔ مسافر یا شرایط رزرو؛ مرتبط ولی جدا از Occupancy.

### TourRate

- **Canonical:** TourRate
- **فارسی:** نرخ تجاری تور که می‌تواند چند PriceComponent داشته باشد.

### PriceComponent

- **Canonical:** PriceComponent
- **فارسی:** یک مؤلفهٔ پولی با Amount، Currency و معنای Purpose (مثلاً package، local charge، tax، fee).
- **اشتباه رایج:** فرض تک‌ارزی بودن کل نرخ.

### Money

- **Canonical:** Money
- **فارسی:** Value concept شامل Amount (`decimal`) و CurrencyCode.
- **ممنوع:** float/double.

### Currency

- **Canonical:** Currency
- **فارسی:** کد/هویت ارز معتبر؛ نه رشتهٔ نمایشی دلخواه.

### ExchangeRate

- **Canonical:** ExchangeRate
- **فارسی:** نرخ تبدیل در زمان/سیاست مشخص. تغییر نرخ نباید Booking تاریخی را خاموش عوض کند.

### Quote

- **Canonical:** Quote
- **فارسی:** پیشنهاد محاسبه‌شده برای درخواست مشخص در زمان مشخص.

### Booking

- **Canonical:** Booking
- **فارسی:** رزرو پذیرفته‌شده که باید snapshot قیمت/Quote را حفظ کند.

### Payment

- **Canonical:** Payment
- **فارسی:** تراکنش/تسویهٔ مالی واقعی.

### Agency

- **Canonical:** Agency
- **فارسی:** طرف B2B که از طریق Agency Panel به قابلیت‌های مجاز دسترسی دارد.

### Party

- **Canonical:** Party
- **فارسی:** هویت طرف تجاری/شخصی در لایهٔ Foundation (شخص حقیقی/حقوقی و نقش‌های مرتبط).

### Article

- **Canonical:** Article
- **فارسی:** محتوای editorial راهنما/دانش.

### Travelogue

- **Canonical:** Travelogue
- **فارسی:** سفرنامه/روایت UGC یا editorial مرتبط با مقصد.

### Review

- **Canonical:** Review
- **فارسی:** نظر/امتیاز کاربر دربارهٔ Place یا تجربه.

### SeoRoute

- **Canonical:** SeoRoute
- **فارسی:** مسیر عمومی کنترل‌شده برای ایندکس و کشف؛ متمایز از URL خام فیلتر جستجو.

### Canonical

- **Canonical:** Canonical
- **فارسی:** URL مرجع برای یک هویت محتوایی/locale.

### Hreflang

- **Canonical:** Hreflang (AlternateLocale)
- **فارسی:** پیوند جایگزین‌های زبانی/منطقه‌ای برای موتور جستجو.

### StructuredData

- **Canonical:** StructuredData
- **فارسی:** نشانه‌گذاری Schema.org متناسب با معنای واقعی صفحه (نه یک schema عمومی همه‌جا).

### PageArchetype

- **Canonical:** PageArchetype
- **فارسی:** الگوی صفحه با anatomy، داده، SEO و رفتار responsive مشخص (مثلاً ForeignTourDetailPage).

### LocalizedSlug

- **Canonical:** LocalizedSlug
- **فارسی:** بخش مسیر وابسته به locale برای یک Entity؛ مثلاً استانبول / istanbul / اسطنبول برای یک DestinationId.

### IndexPolicy

- **Canonical:** IndexPolicy
- **فارسی:** سیاست ایندکس‌پذیری (Published، Canonical، locale معتبر، عدم Redirect و …).

### SeoLandingPage

- **Canonical:** SeoLandingPage
- **فارسی:** مسیر عمومی کنترل‌شده و تأییدشده برای intent جستجوی معنادار؛ متمایز از URL خام فیلتر/جستجو.
- **اشتباه رایج:** یکی‌دانستن هر ترکیب فیلتر یا نتیجهٔ جستجو با Landing SEO.

### Locale

- **Canonical:** Locale
- **فارسی:** زمینهٔ زبان/فرهنگ UI و محتوا با شناسهٔ BCP 47 (مثلاً `fa`، `en`، `ar`، `fa-IR`).
- **اشتباه رایج:** یکی‌دانستن با Currency، Calendar، یا TimeZone؛ یا محدودکردن دائمی سیستم به دقیقاً سه کد.

### LanguageTag

- **Canonical:** LanguageTag
- **فارسی:** شناسهٔ نرمال‌شدهٔ زبان طبق semantics BCP 47 برای LocaleCode.

### SourceLocale

- **Canonical:** SourceLocale
- **فارسی:** locale مبدأ/اصلی یک آیتم قابل‌ترجمه برای workflow editorial؛ اجبارکنندهٔ public fallback نیست.

### LocalizedPublication

- **Canonical:** LocalizedPublication
- **فارسی:** وضعیت انتشار یک representation محلی‌شده (مثلاً Draft/Ready/Published)؛ جدا از lifecycle تجاری Entity و جدا از صرف وجود ردیف ترجمه.

### UUIDv7

- **Canonical:** UUIDv7
- **فارسی:** نسخهٔ ۷ شناسهٔ UUID برای هویت‌های دامنهٔ قابل‌ارجاع؛ تولید معمولاً در Application قبل از persistence.
- **اشتباه رایج:** استفاده از UUID v4 به‌عنوان پیش‌فرض، یا وابستگی به sequence دیتابیس برای هویت عمومی.

### ProviderMapping

- **Canonical:** ProviderMapping
- **فارسی:** نگاشت هویت داخلی TravelCore به `(ProviderCode, ExternalId)`؛ External ID هرگز PK داخلی نیست.
- **اشتباه رایج:** یک جدول ExternalReference سراسری بدون ADR، یا Provider ID به‌عنوان PK.

### Instant

- **Canonical:** Instant
- **فارسی:** نقطهٔ زمانی مطلق (NodaTime)؛ در PostgreSQL معمولاً `timestamptz`.

### LocalDate

- **Canonical:** LocalDate
- **فارسی:** تاریخ تقویمی بدون timezone (مثلاً check-in)؛ PostgreSQL `date`.

### LocalTime

- **Canonical:** LocalTime
- **فارسی:** ساعت دیواری محلی بدون تاریخ (مثلاً ۱۴:۰۰ check-in)؛ به UTC تبدیل نمی‌شود.

### TimeZoneId

- **Canonical:** TimeZoneId
- **فارسی:** شناسهٔ IANA timezone (مثلاً `Asia/Tehran`)؛ نه نام Windows-only به‌عنوان مدل هسته.

---

## واژگان فنی که ترجمهٔ اجباری نمی‌شوند

DbContext · Aggregate · Value Object · Minimal API · Vertical Slice · SEO · SSR · ISR · Modular Monolith · Outbox · ADR · NodaTime · JSONB · BCP 47

در نثر فارسی می‌توانند عیناً انگلیسی بمانند وقتی دقیق‌ترند.
