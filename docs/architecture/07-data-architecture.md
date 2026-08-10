# Data Architecture — معماری داده TravelCore

این سند **منبع حقیقت سطح‌بالای Data Architecture** است. قبل از هر پیاده‌سازی persistence (DbContext، Entity، Migration) باید خوانده شود.

جزئیات مرتبط:

| سند | نقش |
|-----|------|
| [`08-persistence-and-migrations.md`](08-persistence-and-migrations.md) | DbContext · migrations · Dapper · Outbox/Inbox · seeds |
| [`../data/01-identifiers-and-references.md`](../data/01-identifiers-and-references.md) | UUID v7 · ارجاعات · Provider IDs |
| [`../data/02-money-and-currency.md`](../data/02-money-and-currency.md) | Money · IRR/Toman · mixed-currency |
| [`../data/03-temporal-model.md`](../data/03-temporal-model.md) | NodaTime · Instant · LocalDate/Time · timezone |
| [`../data/04-localization-and-json-policy.md`](../data/04-localization-and-json-policy.md) | ترجمه · LocaleCode · JSONB |
| [`04-module-boundaries.md`](04-module-boundaries.md) | مالکیت دامنه |
| [`05-dependency-rules.md`](05-dependency-rules.md) | قوانین وابستگی |
| [`06-cross-module-communication.md`](06-cross-module-communication.md) | قراردادها · رویداد · projection |

ADRهای مرتبط این Task در وضعیت **Proposed** هستند تا بازبینی معمار.

---

## 1. تصمیم توپولوژی

TravelCore در شروع از:

**یک PostgreSQL database**

برای Modular Monolith استفاده می‌کند.

یک دیتابیس **به معنای یک schema اشتراکی کسب‌وکار نیست**.

هر ماژول کسب‌وکار schemaی PostgreSQL خودش را مالک است.

```text
TravelCore PostgreSQL (single database)
├── identity
├── access
├── party
├── reference_data
├── destination
├── media
├── place
├── content
├── ugc
├── tour
├── pricing
├── visa
├── booking
├── payment
├── hotel_booking
├── flight
├── search
├── seo
└── notification
```

نام schemaها: `lowercase snake_case`.

### ممنوع

- ریختن همهٔ جداول کسب‌وکار داخل `public`
- یک schema مشترک به‌عنوان dumping ground
- تغییر اشیاء schema ماژول دیگر توسط migration ماژول دیگر

اشیاء platform/bootstrap که واقعاً جای دیگری لازم دارند، بعداً با تصمیم صریح طراحی می‌شوند — نه به‌عنوان میان‌بر مالکیت.

---

## 2. مالکیت داده

هر ماژول مالک است بر:

- جداول خودش
- indexes
- constraints
- migrations و تاریخچهٔ migration
- persistence mapping (DbContext / mappings)

ماژول دیگر نباید این اشیاء را مستقیماً تغییر دهد یا از DbContext آن‌ها بخواند/بنویسد.

مرز مالکیت دامنه در [`04-module-boundaries.md`](04-module-boundaries.md) و [`../domain/module-ownership-matrix.md`](../domain/module-ownership-matrix.md) قفل است؛ این سند همان مالکیت را به سطح persistence می‌برد.

---

## 3. DbContext و Migration (خلاصه)

جهت معماری:

- **یک DbContext به ازای هر ماژول persistent** (مثلاً `TourDbContext`، `PlaceDbContext`)
- **نه** یک `TravelCoreDbContext` سراسری
- هر ماژول migrations خودش را مالک است
- جدول تاریخچهٔ migration مفهومی داخل همان schema: `tour.__ef_migrations_history`

جزئیات: [`08-persistence-and-migrations.md`](08-persistence-and-migrations.md)

`EnsureCreated` به‌عنوان lifecycle تولید/عملیاتی ممنوع است.

---

## 4. شناسه‌ها و ارجاعات (خلاصه)

- هویت‌های دامنهٔ قابل‌ارجاع: **UUID v7** در ستون `uuid`
- تولید هویت معمولاً در Application قبل از persistence
- Strongly typed IDs در Domain/Application (پیاده‌سازی در P01)
- کدهای استاندارد طبیعی (`USD`، `fa`، `Asia/Tehran`) نیاز به UUID ندارند
- شناسهٔ محلی/سورگیت فقط برای ردیف‌های داخلی Aggregate
- بدون magic sentinel مثل `-1`

جزئیات: [`../data/01-identifiers-and-references.md`](../data/01-identifiers-and-references.md)

---

## 5. ارجاع بین‌ماژولی

ارجاع بین‌ماژولی = **شناسهٔ منطقی اسکالر**.

### مثال A — Tour → Hotel

```text
tour.tour_hotel_options.hotel_id  →  Place.HotelId (logical)
```

- بدون EF navigation به `Place.Hotel`
- بدون PostgreSQL FK به `place.hotels` (پیش‌فرض قفل‌شده)
- اعتبار وجود/نوع Hotel از طریق قرارداد Application ماژول Place

داخل **همان** ماژول، FK رابطه‌ای معمول است:

```text
tour.tour_departures.tour_product_id  → FK → tour.tour_products.id
```

استثناء cross-module FK فقط با تأیید معمار و ADR.

---

## 6. یکپارچگی بدون FK بین‌ماژولی

یکپارچگی منطقی از طریق:

1. قراردادهای Application (اعتبارسنجی ایجاد/به‌روزرسانی)
2. قواعد lifecycle دامنه
3. snapshot برای حقایق تاریخی
4. رویداد / projection وقتی لازم است

هزینه: دیتابیس نمی‌تواند ارجاع بین‌schema را enforce کند.  
سود: استقلال migration و استخراج آیندهٔ ماژول.

---

## 7. Domain Primitives جهانی (مرز خیلی کوچک)

`Money` یک مفهوم value جهانی است؛ فقط متعلق به Pricing به‌عنوان «نوع نمایش» نیست.

ماژول‌هایی که مشروعانه Money دارند: Pricing · Booking snapshots · Payment · HotelBooking offers · Flight offers.

مرز مفهومی آینده:

```text
TravelCore.Domain.Primitives   ← نه business module
  - Money
  - CurrencyCode
```

**ممنوع در این مرز:** TourStatus · BookingStatus · Agency · Address · User · Hotel · services · repositories · DbContexts.

اسمبلی الان ساخته نشود. SharedKernel بزرگ ساخته نشود. افزودن primitive بیشتر نیاز به تأیید معمار دارد.

جزئیات پول: [`../data/02-money-and-currency.md`](../data/02-money-and-currency.md)

---

## 8. پول و قیمت چندارزی (خلاصه)

```text
Money { Amount: decimal, CurrencyCode }
Amount → PostgreSQL numeric(24,8)  (پیش‌فرض)
هرگز float/double یا نوع money پستگرس
```

IRR کاننیکال است. Toman واحد نمایش/ورود است (۱ تومان = ۱۰ ریال)؛ تبدیل ضمنی ممنوع.

نرخ mixed-currency رابطه‌ای است:

```text
TourRate
  └── PriceComponents[]
        Amount · CurrencyCode · Purpose
```

### مثال C

```text
1290 USD + 119,900,000 IRR
```

### مثال D

```text
Display:  11,990,000 Toman
Canonical: 119,900,000 IRR
```

`Price ≠ Quote ≠ Booking ≠ Payment`. تغییر نرخ زنده Booking تاریخی را بازنویسی نمی‌کند.

---

## 9. زمان (خلاصه)

مدل زمانی Domain/Application: **NodaTime** (نصب پکیج در P01).

| معنا | نوع مفهومی | PostgreSQL |
|------|------------|------------|
| Instant سیستم/ممیزی | Instant | `timestamptz` |
| تاریخ بدون timezone | LocalDate | `date` |
| ساعت محلی بدون تاریخ | LocalTime | `time` |
| زمان‌بندی سفر | LocalDateTime + TimeZoneId (+ Instant در صورت نیاز) | ترکیب صریح |

Timezone کاننیکال: IANA (`Asia/Tehran`). تقویم ≠ Locale.

جزئیات: [`../data/03-temporal-model.md`](../data/03-temporal-model.md)

---

## 10. Localization و JSONB (خلاصه)

- ممنوع: `name_fa` / `name_en` / `name_ar`
- ترجمهٔ Entity: جدول ترجمهٔ متعلق به همان ماژول
- یک Translation table سراسری ممنوع
- slug/مسیر SEO متعلق به SEO است، نه تکرار در هر translation

### مثال B — Destination translation

```text
destination.destinations
destination.destination_translations
UNIQUE (destination_id, locale_code)
```

JSONB فقط برای دادهٔ واقعاً document-shaped؛ نه میان‌بر برای مدل دامنهٔ پایدار.

جزئیات: [`../data/04-localization-and-json-policy.md`](../data/04-localization-and-json-policy.md)

---

## 11. رابطه‌ای در برابر JSONB

| ترجیح رابطه‌ای وقتی | JSONB ممکن وقتی |
|---------------------|-----------------|
| invariant دارد | payload خام provider |
| query/join مکرر | metadata انعطاف‌پذیر provider |
| نیاز به constraint | کانفیگ Content Block پذیرفته‌شده |
| ساختار دامنه پایدار | webhook diagnostic |

ممنوع: ریختن کل Tour / PriceComponents / هستهٔ Booking / ترجمه‌های رابطه‌ای پیش‌فرض داخل JSONB.

---

## 12. Snapshot تاریخی

ماژول‌های تراکنشی حقایق پذیرفته‌شده را snapshot می‌کنند.

### مثال G — Booking price snapshot

```text
10:00  Quote Q = 1290 USD + 119,900,000 IRR  → پذیرفته می‌شود
       Booking snapshot از Q را نگه می‌دارد
15:00  نرخ زنده = 1350 USD + 125,000,000 IRR
       Booking قبلی بدون تغییر می‌ماند
       Payment تعهد پذیرفته‌شده را تسویه می‌کند
```

فقط به ارجاع به رکوردهای زندهٔ قابل‌تغییر تکیه نکنید.

---

## 13. Read Model و ترکیب صفحه

### مثال H — Search projection

Projection جستجو ممکن است Title تور، نام مقصد، نام آژانس و قیمت نمایشی را **تکرار** کند. این تکرار مشتق است؛ منبع حقیقت همچنان ماژول‌های مالک‌اند. Projection باید قابل rebuild باشد.

### ترکیب صفحهٔ Destination

صفحه ممکن است Destination + Tours + Hotels + Content + UGC + SEO بخواهد.

**ممنوع:** یک SQL که مستقیم `destination.*` را به `tour.*` و `place.*` و … join کند چون راحت است.

راه درست: application composition یا projection مشتق صریح.

Dapper مرز ماژول را دور نمی‌زند.

---

## 14. مرز تراکنش و Outbox

پیش‌فرض:

```text
یک تراکنش کسب‌وکار = یک ماژول مالک = یک DbContext / schema
```

گردش بین‌ماژولی:

```text
اعتبارسنجی با قرارداد (در صورت نیاز فوری)
→ commit مالک (شامل Outbox محلی در همان تراکنش)
→ واکنش پایین‌دست (Inbox/idempotency متعلق به مصرف‌کننده)
```

Outbox مفهومی: `tour.outbox_messages` داخل schema همان ماژول — نه یک جدول Outbox سراسری جدا از تراکنش مالک.

---

## 15. شناسهٔ Provider خارجی

### مثال F

```text
HotelId (TravelCore UUID v7)
+ ProviderCode
+ ExternalHotelId
```

External ID هرگز PK داخلی TravelCore نیست. نگاشت متعلق به ماژول مربوطه است؛ یک mega-table سراسری ExternalReference بدون ADR ممنوع است.

---

## 16. نام‌گذاری رابطه‌ای

PostgreSQL: `lowercase snake_case` بدون identifierهای quoted mixed-case.

پیشوندهای مفهومی: `pk_` · `fk_` · `uq_` · `ix_` · `ck_`

---

## 17. ضدالگوهای داده (سطح‌بالا)

- یک `TravelCoreDbContext` غول‌پیکر
- یک `public` schema کسب‌وکار
- EF navigation / DbContext / FK فیزیکی بین‌ماژولی (پیش‌فرض)
- join راحت Dapper بین‌schema
- Provider ID به‌عنوان PK داخلی
- `NameFa` / `UsdPrice` / float money / PostgreSQL `money`
- `TOMAN` به‌عنوان CurrencyCode تعریف‌نشده
- DateTime مبهم · ستون‌های تکراری شمسی/میلادی
- magic ID · Translation سراسری · IsDeleted سراسری · BaseEntity همه‌منظوره
- JSONB به‌جای مدل دامنه · `EnsureCreated` تولیدی

فهرست کامل‌تر در [`08-persistence-and-migrations.md`](08-persistence-and-migrations.md).

---

## 18. Intentionally Deferred Decisions

این Task عمداً تصمیم نمی‌گیرد:

- نسخهٔ دقیق EF Core / Npgsql / NodaTime
- تکنیک/کتابخانهٔ StronglyTypedId
- سازمان فیزیکی project/class library
- پیاده‌سازی migration runner
- schema دقیق Outbox dispatcher / Inbox
- encryption-at-rest · برنامهٔ retention PII
- پیاده‌سازی audit-event storage
- schema فیزیکی Search index و SEO tables
- schemaهای provider-specific Flight/HotelBooking fare
- schema کامل Pricing / Content blocks

این‌ها متعلق به P01 یا فازهای ماژول‌اند.

---

## 19. پیوند ADRهای Proposed

| موضوع | ADR |
|-------|-----|
| Schema-per-module و عدم FK بین‌ماژولی پیش‌فرض | [`../adr/0001-postgresql-schema-per-module.md`](../adr/0001-postgresql-schema-per-module.md) |
| UUID v7 | [`../adr/0002-uuid-v7-domain-identity.md`](../adr/0002-uuid-v7-domain-identity.md) |
| Money / IRR / Toman | [`../adr/0003-money-currency-irr-toman.md`](../adr/0003-money-currency-irr-toman.md) |
| NodaTime / IANA | [`../adr/0004-temporal-model-nodatime.md`](../adr/0004-temporal-model-nodatime.md) |

وضعیت همه در این Task: **Proposed** — تا پذیرش معمار، Accepted نشوند.
