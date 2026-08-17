# Module Boundaries — مرزهای مالکیت ماژول‌ها

برای هر ماژول تأییدشده: Purpose · Owns · Does Not Own · References · Key boundaries · Upstream/Downstream.

ماتریس سریع: [`../domain/module-ownership-matrix.md`](../domain/module-ownership-matrix.md)
نقشهٔ دامنه: [`03-domain-map.md`](03-domain-map.md)

---

## Presentation Surfaces (نه ماژول دامنه)

### Public Website · Admin Panel · Agency Panel

**Purpose:** ارائه و orchestration UI/API برای مصرف‌کننده، ادمین، و آژانس.

**Owns:** فقط concerns ارائه (layout، navigation، فرم UI، فراخوانی capabilityها).

**Does Not Own:** قواعد کسب‌وکار Tour/Booking/Pricing/Payment یا کپی آن‌ها.

**Forbidden:**

- `AdminService` که منطق Tour را کپی کند
- ماژول Agency که منطق Tour/Booking/Pricing را فقط به‌خاطر UI جدا تکرار کند

Presentation → Application contracts ماژول‌ها.

---

## Identity

**Purpose:** هویت احراز هویت — «چه کسی authenticate شد؟»

**Owns (مفهومی):** Account · credential/authentication identity · login identity · external login association · مفاهیم session/token وقتی بعداً مشخص شود · وضعیت امنیتی/حساب.

**Does Not Own:** پروفایل مسافر/مشتری · پروفایل تجاری Agency · Organization · taxonomy مجوزها · Tour · Booking · اطلاعات پرداخت.

**References:** ممکن است `PartyId` را وقتی پروفایل کسب‌وکار به حساب وصل است نگه دارد؛ مالک Party همچنان Party است.

**Key boundary:** Identity ≠ Party ≠ Access.

**Upstream/Downstream:** Access و Presentation از Identity استفاده می‌کنند. Identity به Tour/Booking وابسته نیست.

---

## Access

**Purpose:** مجوزدهی — «این subject چه کاری مجاز است انجام دهد؟»

**Owns:** Permission · Role · assignment · رابطهٔ سیاست · ارزیابی authorization.

**Does Not Own:** credentials · Agency/Person profile · سیاست‌های کسب‌وکار Tour · visibility UI به‌عنوان منبع حقیقت.

**References:** شناسه‌های Identity/Party به‌عنوان subject — بدون EF navigation به Entityهای آن‌ها.

**Key boundary:** پنهان کردن دکمه در UI ≠ Authorization. Backend authoritative است.

---

## Party

**Purpose:** هویت کسب‌وکار — «این شخص/سازمان در دامنه کیست؟»

**Owns:** Party · Person · Organization · هویت/پروفایل تجاری Agency · اطلاعات تماس/شناسه‌های تجاری · هویت حقوقی/تجاری مرتبط.

**Does Not Own:** login credentials · تعریف Role/Permission · موجودی Tour · Booking · Payment · سلسله‌مراتب Destination · کاتالوگ Hotel.

**Key boundary:** Agency silo احراز هویت نیست. کاربر آژانس از Identity لاگین می‌کند و طبق Access به‌نمایندگی Agency Party عمل می‌کند.

**References:** ReferenceData در صورت نیاز به مرجع‌های بنیادین.

---

## ReferenceData

**Purpose:** مرجع‌های پایدار/اشتراکی که چرخهٔ عمر محصول سفر نیستند.

**Owns (محافظه‌کارانه):** تعریف Currency/code · مرجع زبان/locale در صورت مناسب بودن · اطلاعات ISO کشور · کاتالوگ timezone در صورت مناسب بودن · metadata مرجع airline/airport اگر بعداً پذیرفته شود · taxonomyهایی مثل MealPlan فقط اگر واقعاً مرجع مشترک‌اند · سایر کاتالوگ‌های enumerated پایدار.

**Does Not Own / نه dumping ground:**

- `TourStatus` → Tour
- `BookingStatus` → Booking
- `PaymentStatus` → Payment
- Destination hierarchy → Destination

اگر مفهوم رفتار/lifecycle/مالکیت کسب‌وکار جدی دارد، متعلق به ماژول کسب‌وکار است.

**References:** هیچ ماژول کسب‌وکار.

---

## Destination

**Purpose:** سلسله‌مراتب جغرافیایی/کشف سفرمحور و گرهٔ مرکزی knowledge/SEO graph.

**Owns:** Destination با انواعی مانند Continent · Country · State · Province · Region · City · Island · District · Neighborhood.

مثال: Asia → Turkey → Istanbul → Beyoglu → Taksim

**Does Not Own:** hotels · restaurants · attractions · tours · articles · reviews.

**References:** کدهای جغرافیایی استاندارد از ReferenceData؛ Media برای دارایی مقصد.

**Key boundary:** ReferenceData ممکن است تعریف ISO کشور را مالک باشد؛ Destination مالک گره TravelCore برای discovery، محتوا، SEO graph، روابط Tour/Place است.

Destination برای نگهداری aggregate خودش Tour/Place/Content را query نمی‌کند. صفحات عمومی از چند قرارداد خواندن ترکیب می‌شوند.

---

## Media

**Purpose:** چرخهٔ عمر و متادیتای رسانهٔ باینری.

**Owns:** MediaAsset · هویت object-storage · MIME/metadata · dimensions · variants · focal point · alt/caption translations طبق معماری بعدی · processing status · upload lifecycle.

بایت فایل در S3-compatible storage است.

**Does Not Own:** ترتیب گالری Tour · ترتیب گالری Hotel · محتوای Article · منطق Review.

**الگو:** ماژول دیگر `MediaAssetId` را ارجاع می‌دهد و معنای رابطه را خودش مالک است.

مثال:

```text
Tour owns TourMedia { TourId, MediaAssetId, SortOrder, Role }
Media owns the asset
```

**References:** به ماژول کسب‌وکار وابسته نیست.

---

## Place

**Purpose:** کاتالوگ مکان سفر.

**Owns:** Place → Hotel · Restaurant · Attraction و اطلاعات توصیفی/کاتالوگ: هویت · نام/توضیح محلی‌شده · رابطه Destination · آدرس/مکان · مختصات · facilities · طبقه‌بندی · وضعیت کاتالوگ · روابط media · پروفایل توصیفی عمومی.

**Does Not Own:** availability زنده provider · room inventory · نتایج جستجوی provider · نرخ رزرو زنده · reservation · voucher.

**Key boundary:** Hotel Catalog ≠ Hotel Booking.

---

## Content

**Purpose:** محتوای editorial.

**Owns:** Article · Guide · Landing Page editorial · Category · Tag · Author/attribution · Content Blocks · publication lifecycle.

**References (شناسه/قرارداد):** Destination · Place · Tour · Visa — بدون انتقال مالکیت.

**Key boundary:** راهنمای استانبول با `TourProductId`، Tour را بخشی از Content نمی‌کند. Tour برای عملیات کسب‌وکارش به Content وابسته نیست. از وابستگی دوطرفهٔ دامنه پرهیز شود.

---

## UGC

**Purpose:** محتوای کاربرساخت.

**Owns:** Review · Rating · RatingDimension · Travelogue · UserPhoto relationship · Comment · Moderation · Report/abuse · publication state.

**Targets (ارجاع):** Destination · Place · Tour · و احتمالاً Content اگر بعداً تأیید شود.

**Key boundary:** موجودیت هدف مالک Aggregate UGC نیست. Place نباید collection ناوبری EF از `UGC.Review` داشته باشد. صفحهٔ Place می‌تواند Place data + UGC projection را از قراردادهای خواندن ترکیب کند.

**References:** شناسهٔ Identity/Party به‌عنوان subject · Media · شناسهٔ هدف.

---

## Tour

**Purpose:** دامنهٔ محصول تور قابل‌فروش/توصیفی.

**Owns:** TourProduct · ساختار Experience Tour · ساختار Package Tour · TourDeparture · itinerary · itinerary day · stop · پیکربندی خدمات · سیاست تور · eligibility · equipment · ظرفیت‌های متعلق به Tour · تعریف سگمنت حمل‌ونقل پکیج · TourHotelOption · پیکربندی اقامت پکیج · applicability مسافر/اشغال · الزامات سفر پکیج · publishing lifecycle.

**Does Not Own:** کاتالوگ Hotel · هویت Organization/Agency · سیاست نرخ ارز · Quote · Payment · Booking · Search index · SEO engine.

**Critical:** TourProduct ≠ TourDeparture.

**Experience Tour (داخل Tour):** itinerary ساخت‌یافته · days · stops · meals · accommodation plan · local transport · equipment · difficulty · eligibility · guide info · لینک Destination/Place. Entityهای Destination/Attraction همچنان متعلق به Destination/Placeاند.

**Foreign Package Tour (داخل Tour):** departures · transport segments · حقایق حمل‌ونقل پکیج · hotel options · stay · applicability · services · travel requirements.

**Key boundary:** Tour package `FlightSegment` ≠ موجودی زندهٔ Flight. TourHotelOption با `PlaceId` (Hotel-kind Place) به Place.Hotel ارجاع می‌دهد و کاتالوگ کامل را کپی نمی‌کند.

**References:** Destination · Place/Hotel · Party/Agency · Media · ReferenceData از طریق شناسه/قرارداد.

---

## Pricing

**Purpose:** موتور تجاری قیمت برای محصولات متعلق به TravelCore جایی که Pricing منبع حقیقت تجاری است.

**Owns:** معنای Money · TourRate · PriceComponent · نرخ‌های چندارزی · ExchangeRate · conversion policy · Quote · اعتبار/انقضای Quote در صورت نیاز · محاسبه قیمت · آماده‌سازی snapshot قیمت.

**Rules:**

- Amount با `decimal`
- یک نرخ می‌تواند چند مؤلفهٔ ارزی هم‌زمان داشته باشد (مثلاً 1290 USD + مؤلفهٔ محلی)
- نه مدل بنیادین `UsdPrice` / `IrrPrice` / `AdultPrice` / `ChildPrice`
- Price ≠ Quote ≠ Booking ≠ Payment
- Tour مالک محاسبه قیمت نیست

**References:** TourProductId · TourDepartureId · PassengerCategory/Occupancy از طریق قرارداد/کد — بدون دسترسی به Tour DbContext. اعتبارسنجی فوری از قرارداد Application عمومی Tour.

**Scope warning:** اینکه هر fare provider در HotelBooking/Flight باید به‌صورت Aggregate Pricing ذخیره شود، **عمداً هنوز کاملاً تصمیم‌گیری نشده**. معنای مشترک Money/Currency ممکن است از قرارداد/value abstraction استفاده شود؛ یکپارچگی کامل Pricing با provider fares در فازهای مربوط مشخص می‌شود.

---

## Visa

**Purpose:** محصول/گردش‌کار ویزا و سند سفر.

**Owns:** VisaType · visa offering · applicability مقصد/کشور · requirements · documents · processing info · workflow بعدی · مرجع تجاری خدمت در صورت نیاز.

**References:** Destination · ReferenceData · Media در صورت نیاز.

**Key boundary:** توصیف «این پکیج نیاز به ویزا دارد» در Tour مالکیت Visa را منتقل نمی‌کند. از وابستگی سخت دوطرفه Tour ↔ Visa پرهیز شود؛ UI عمومی می‌تواند هر دو را ترکیب کند.

---

## Booking

**Purpose:** وضعیت رزرو/سفارش برای جریان‌های رزرو متعلق به TravelCore.

**Owns (برای Tour Booking مفهومی):** Booking · status · traveler/passenger snapshot · ارجاع/snapshot Quote پذیرفته‌شده · خطوط/اقلام در صورت نیاز · reservation state · confirmation · پایهٔ cancellation.

**Does Not Own:** اجرای تراکنش Payment · کاتالوگ Tour · موتور محاسبه قیمت · پروفایل Agency.

**References:** Party/customer · Tour product/departure · Pricing Quote.

**Key boundary:** Booking حقایق تجاری تاریخی پذیرفته‌شده را حفظ می‌کند. تغییر زندهٔ Pricing، تاریخچهٔ Booking را بازنویسی نمی‌کند. Booking ممکن است به رویداد Payment واکنش دهد؛ Payment ماژول جداست.

---

## Payment

**Purpose:** چرخهٔ عمر تراکنش مالی.

**Owns:** Payment · PaymentAttempt · provider · نتیجه callback/webhook · transaction/reference · success/failure · پایهٔ refund · اطلاعات audit/snapshot پرداخت.

**Does Not Own:** lifecycle کسب‌وکار Booking · محاسبه قیمت · Tour · تولید Quote.

**Key boundary:** موفقیت Payment رویداد می‌دهد؛ Booking واکنش می‌دهد. Payment مستقیماً Booking DbContext را mutate نمی‌کند.

---

## HotelBooking

**Purpose:** موجودی زندهٔ قابل‌رزرو هتل و تعامل provider.

**Owns:** Provider · mapping هتل provider · availability search · room offers · live rates · cancellation conditions · provider quote · reservation · booking · voucher · sync/reference.

**References:** Place.Hotel با `PlaceId` داخلی + mapping provider · ReferenceData · provider abstractions.

**Key boundary:** HotelBooking کاتالوگ Place را به‌عنوان canonical جایگزین نمی‌کند. `ProviderHotelId` / `ExternalHotelId` هرگز PK داخلی Place نمی‌شود.

```text
Place.Hotel: PlaceId = H123 (canonical catalog identity)
HotelBooking mapping: PlaceId=H123, Provider=ProviderA, ExternalHotelId=998812
```

---

## Flight

**Purpose:** تجارت/جستجو/رزرو زندهٔ پرواز و تعامل provider.

**Owns:** provider · flight search · itineraries · segments · provider offer/fare · baggage/rules · quote · booking/order · provider references.

**References:** ReferenceData برای airport/country/carrier در صورت پذیرش مالکیت · provider abstractions.

**Key boundary:**

```text
Tour Package FlightSegment (IKA→IST EK978 local times)
!=
Flight live bookable inventory
```

ممکن است مفاهیم معنایی مشترک داشته باشند؛ مالکیت Aggregate یکی نیست. IDهای provider-specific به‌عنوان هویت دامنهٔ اولیه معرفی نشوند.

**Pricing dependency:** وابستگی اجباری HotelBooking/Flight به Pricing تا طراحی صریح مالکیت قیمت آن‌ها ممنوع است.

---

## Search

**Purpose:** قابلیت پلتفرم پایین‌دستی برای بازیابی و Discovery (P15-R1).

**Owns:** Search query/result contracts · schema `search` · `SearchDocument` / `ISearchIndex` hybrid read-model abstraction (P15-R2) · projection sync boundary (P15-R3) · faceting Aggregation / Counting / Result composition contracts (P15-R4) · deterministic ranking composition / ordering / metadata contracts (P15-R5) · structured semantic retrieval / provenance readiness (P15-R6).

**Does Not Own (authoritative):** Tour facts · Content facts · Pricing facts · AgencyOffer facts · SEO IndexPolicy · attribute meaning of domain facets · Tour/Agency commercial priority · commission/sponsorship/profitability policy · AI platform / LLM gateway / vector store · Destination · Place · UGC · Visa · Booking · Payment · Recommendation · filter UI (PublicExperience).

**Technology direction:** PostgreSQL FTS + `pg_trgm` behind abstraction later — ماژول‌های کسب‌وکار به این پیاده‌سازی وابسته نمی‌شوند. **T001 does not implement FTS/`pg_trgm`/Elasticsearch.**

**Forbidden:** Tour → SearchDbContext · Destination → جداول Search برای validation کسب‌وکار.

شکست Search نباید تراکنش SoR را فاسد کند.

**Example:** Destination نام authoritative را مالک است؛ Search ممکن است کپی نرمال‌شده (`استانبول` / `istanbul` / `اسطنبول`) نگه دارد بدون overwrite متن مرجع.

---

## SEO

**Purpose:** مکانیک پلتفرم SEO — نه مالک Entity کسب‌وکار.

**Owns:** SeoRoute · localized slug routing · canonical · hreflang · redirects · slug history · IndexPolicy · مشارکت sitemap · orchestration/contracts Structured Data · مکانیک metadata.

**Does Not Own:** عنوان/چرخهٔ عمر کسب‌وکار Tour یا سایر Entityها.

**Example:**

```text
Tour owns: TourProduct, title translations, publication state
SEO owns: SeoRoute, canonical URL, redirect history, index policy
Public composition: Tour page response + SEO response
```

از انباشت همهٔ routing SEO داخل جدول Tour و از مالک شدن محتوای Tour توسط SEO جلوگیری می‌کند.

---

## Notification

**Purpose:** قابلیت ارسال اعلان.

**Owns:** Notification · channel · orchestration قالب/ارسال · provider abstraction · delivery state · preferences در صورت پذیرش بعدی.

**Consumes:** رویدادهای معنایی مانند BookingConfirmed · PaymentSucceeded · VisaApplicationUpdated (نمونه‌ها؛ فهرست قفل نیست).

**Forbidden:** Booking مستقیماً Twilio را صدا بزند · Tour ایمیل بفرستد · Payment اعلان UI تولید کند.

---

## Intentionally Deferred Decisions

این موارد عمداً اینجا تصمیم‌گیری نمی‌شوند (متعلق به TC-P00-T003 / P01 / فازهای بعدی):

- استراتژی دقیق schema-per-module در PostgreSQL
- استراتژی فیزیکی FK بین‌ماژولی
- ساختار دقیق project/class-library
- پیاده‌سازی دقیق ID / نسخه UUID
- محل فیزیکی primitive مشترک Money
- پیاده‌سازی کتابخانه/value-object تاریخ/زمان
- مشارکت generic Pricing در fareهای Flight/HotelBooking
- پیاده‌سازی دقیق event bus
- پیاده‌سازی دقیق Outbox
- packaging دقیق قرارداد ماژول
- استراتژی ذخیره‌سازی read-model
