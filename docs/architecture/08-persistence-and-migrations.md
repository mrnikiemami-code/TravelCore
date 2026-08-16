# Persistence and Migrations — پایداری و مهاجرت‌ها

این سند جهت معماری persistence TravelCore را قبل از پیاده‌سازی P01 قفل می‌کند.

منبع سطح‌بالا: [`07-data-architecture.md`](07-data-architecture.md)

**در این Task هیچ DbContext، Entity، Migration یا پکیجی ایجاد نمی‌شود.**

---

## 1. DbContext Ownership

هر ماژول persistent یک DbContext اختصاصی دارد.

نمونه‌های مفهومی:

```text
TourDbContext
PlaceDbContext
PricingDbContext
BookingDbContext
DestinationDbContext
...
```

### قوانین

- DbContext فقط Entity/aggregateهای **مالک همان ماژول** را map می‌کند
- map کردن aggregate ماژول دیگر «برای راحتی» ممنوع است
- افشای DbContext / EF Entity از Public API ممنوع است

### ممنوع

```text
TravelCoreDbContext   ← یک context سراسری برای همهٔ Entityها
```

---

## 2. Migration Ownership

هر ماژول migrations EF Core خودش را مالک است.

| قانون | معنی |
|-------|------|
| Scope | migration ماژول Tour فقط اشیاء `tour.*` |
| Forbidden | migration Tour که `place.*` / `pricing.*` / `booking.*` را عوض کند |
| History | هر DbContext جدول تاریخچهٔ خودش را داخل schema خودش نگه می‌دارد |

مفهومی:

```text
tour.__ef_migrations_history
place.__ef_migrations_history
pricing.__ef_migrations_history
booking.__ef_migrations_history
```

پیاده‌سازی دقیق در P01.

---

## 3. EnsureCreated ممنوع

چرخهٔ عمر دیتابیس تولیدی/عملیاتی باید بر **migrations نسخه‌دار** باشد.

ممنوع به‌عنوان مکانیزم lifecycle برنامه:

```text
EnsureCreated
```

تغییر schema تولیدی نباید SQL دستی بدون مستندسازی باشد. اگر SQL اضطراری لازم شد، بعداً باید با تاریخچهٔ migration reconcile شود.

---

## 4. ترتیب اجرای Migration

چون هر ماژول schema/migration خودش را دارد، ترتیب اجرا باید در آینده **deterministic** باشد.

معماری نباید به حافظهٔ توسعه‌دهنده وابسته باشد («اول Tour را دستی اجرا کن»).

وابستگی‌های لازم برای startup باید در مکانیزم migration/bootstrap آینده encode شوند. پیاده‌سازی runner در P01 است.

---

## 5. Constraints داخل ماژول

داخل همان ماژول از قابلیت‌های رابطه‌ای PostgreSQL به‌درستی استفاده کنید:

- Primary Keys
- Foreign Keys (داخل همان schema/ماژول)
- Unique Constraints
- Check Constraints
- NOT NULL
- Indexes

هر invariant را فقط به Application نسپارید اگر دیتابیس می‌تواند یکپارچگی ساختاری داخل مالکیت ماژول را امن enforce کند.

### FK بین‌ماژولی

پیش‌فرض: **بدون** FK فیزیکی بین schemaهای ماژول.  
استثناء: ADR + تأیید معمار.

جزئیات ارجاع منطقی: [`../data/01-identifiers-and-references.md`](../data/01-identifiers-and-references.md)

---

## 6. Index Policy

Index متعلق به ماژول مالک جدول است.

بر اساس نیاز واقعی query/invariant:

- شناسه‌های ارجاعی
- کلیدهای کسب‌وکار یکتا
- فیلتر publication/status
- queryهای زمانی
- lookup keys

Index حدسی همه‌جا ممنوع. Indexهای عملکردی بعداً با شواهد query تأیید شوند.

نام‌گذاری توصیفی با پیشوندهایی مثل `ix_` · `uq_` · `pk_` · `fk_` · `ck_`.

---

## 7. Optimistic Concurrency

جایی که lost update مهم است، concurrency خوش‌بینانه استفاده شود.

**نه** افزودن کورکورانه به هر جدول.

Aggregate Rootهای mutable که نیاز به حفاظت دارند:

```text
version  : bigint
```

- با به‌روزرسانی موفق Aggregate عوض می‌شود
- `updated_at` به‌عنوان concurrency token ممنوع است
- قرارداد دامنه به `xmin` پستگرس قفل نشود (ترفندهای پیاده‌سازی بعداً قابل بررسی‌اند؛ مدل معنایی = version صریح)

---

## 8. Audit Metadata

از یک `BaseEntity` غول‌پیکر با همهٔ فیلدهای فنی پرهیز کنید.

متادیتای رایج persistence در صورت معنا:

```text
created_at   Instant → timestamptz
updated_at   Instant → timestamptz
```

اجباری روی همهٔ جداول نیست:

```text
created_by · updated_by · deleted_at · tenant_id · row_version
```

ممیزی پرریسک کسب‌وکار (انتشار تور، تغییر قیمت، redirect SEO، تغییر مجوز، عملیات پرداخت‌محور) جدا از متادیتای سادهٔ ردیف است و در کار Security/Foundation بعدی مشخص می‌شود.

---

## 9. حذف و Archive — بدون Soft Delete سراسری

ممنوع به‌عنوان الگوی جهانی:

```text
is_deleted روی هر Entity
```

Lifecycle متعلق به هر دامنه است. نمونه‌های معنادار:

Archived · Inactive · Closed · Cancelled · Superseded

ردیفات فنی وابسته ممکن است hard-delete شوند اگر تاریخچه/ارجاع اجازه دهد.

Entityهایی که تراکنش تاریخی به آن‌ها ارجاع می‌دهد نباید طوری ناپدید شوند که تاریخچه خراب شود. Booking snapshot نگه می‌دارد؛ نگاشت‌های provider ممکن است Inactive شوند نه اینکه تاریخچه بازنویسی شود.

---

## 10. Database Enum Policy

PostgreSQL native ENUM پیش‌فرض برای وضعیت‌های lifecycle در حال تحول کسب‌وکار نیست.

وضعیت‌های کسب‌وکار تکامل می‌یابند؛ نمایش persistence باید با migration عادی تکامل‌پذیر باشد بدون coupling غیرضروری به enum پستگرس.

کدلیست‌های استاندارد پایدار ممکن است متفاوت باشند. انتخاب دقیق per-module است.

---

## 11. Boolean Flag Policy

از چند boolean موازی برای یک lifecycle واحد پرهیز کنید:

```text
is_active + is_published + is_cancelled + is_archived + is_deleted
```

وقتی یک state صریح دقیق‌تر است، از state modeling استفاده کنید. Boolean برای حقایق واقعاً مستقل yes/no مناسب می‌ماند.

---

## 12. Dapper Policy

| لایه | نقش |
|------|-----|
| EF Core | پایداری تراکنشی authoritative |
| Dapper | read model هدفمند وقتی توجیه دارد |

Dapper:

- CRUD پیش‌فرض نیست
- مدل‌هایش projection/read contractاند، نه Domain Entity
- **همان قوانین مالکیت ماژول** را رعایت می‌کند

### ممنوع

```sql
-- Forbidden convenience join across module schemas
SELECT ...
FROM tour.tour_products t
JOIN place.hotels h ON ...
JOIN pricing.tour_rates r ON ...
```

برای نیاز چندماژولی: application composition · projection اختصاصی · Search read model · سایر read model صریح.

اگر reporting پرترافیک واقعاً به SQL بین‌schema نیاز داشت → تأیید معماری صریح.

---

## 13. Read Models و Rebuildability

Read model مشتق ممکن است داده را تکرار کند (مثال Search). مالکیت authoritative با ماژول منبع است. Projection باید قابل rebuild باشد.

ترکیب صفحهٔ چندماژولی با join همه‌schema حل نمی‌شود — نگاه کنید به [`07-data-architecture.md`](07-data-architecture.md) بخش Page Composition.

---

## 14. Outbox Persistence Direction

هر ماژول event-producing باید Outbox را **تراکنشی با state خودش** persist کند.

مالکیت ترجیحی: storage محلی داخل schema ماژول.

```text
tour.outbox_messages
booking.outbox_messages
```

پیاده‌سازی dispatch مشترک می‌تواند از infrastructure building blocks بیاید؛ اما پیام خروجی persistشده متعلق به تراکنش همان ماژول است.

یک جدول Outbox سراسری که از تراکنش مالک جدا باشد ممنوع است.

Schema دقیق جدول و dispatcher → P01.

---

## 15. Inbox / Idempotency Direction

مصرف‌کنندگان ناهمزمان باید idempotency داشته باشند.

Inbox / processed-message persistence ماژول‌محلی مجاز است. ماژول مصرف‌کننده مالک state مصرف/idempotency خودش است.

پیاده‌سازی دقیق → P01.

---

## 16. Seed Data

دو دستهٔ جدا:

| نوع | ویژگی |
|-----|--------|
| System Reference Seed | deterministic · versioned · idempotent · امن برای production |
| Development Demo Data | فقط local/test · لازم برای startup تولیدی نیست · جدا |

Demo تصادفی داخل migrationهای production ممنوع است.

Reference seed پایدار ممکن است شامل تعاریف currency، localeهای شناخته‌شده، و کدهای پایدار تأییدشده باشد. هر ردیف ReferenceData لزوماً seed نیست؛ دادهٔ مرجع مدیریت‌شونده توسط کسب‌وکار lifecycle خودش را دارد.

---

## 17. امنیت داده (مرز)

Secrets برنامه در جداول کسب‌وکار persist نشوند:

provider credentials · API keys · DB passwords · signing keys → secret/configuration infrastructure.

Payment هرگز دادهٔ حساس تأیید کارت مثل CVV را ذخیره نکند. سیاست PCI/امنیت تفصیلی در فازهای بعدی.

---

## 18. PII (جهت)

TravelCore در Identity · Party · Booking · Visa · metadata مرتبط با Payment دادهٔ شخصی خواهد داشت.

PII را بی‌ضرورت بین ماژول‌ها تکرار نکنید. Snapshot تاریخی فقط آنچه برای تاریخچهٔ مشروع لازم است را نگه می‌دارد.

طبقه‌بندی، رمزنگاری و retention → Security / Data Governance بعدی.

---

## 19. نام‌گذاری Persistence

- schema / table / column: `lowercase snake_case`
- بدون quoted mixed-case identifiers
- پیشوندهای توصیفی برای constraint/index

نمونه‌ها:

```text
hotel_booking          -- schema
tour_products          -- table
created_at             -- column
tour_product_id        -- column
currency_code          -- column
```

---

## 20. Persistence Anti-Patterns

صریحاً ممنوع:

1. یک `TravelCoreDbContext` غول‌پیکر
2. یک `public` schema برای همهٔ جداول کسب‌وکار
3. دسترسی به DbContext ماژول دیگر
4. EF navigation بین‌ماژولی
5. FK فیزیکی بین‌ماژولی به‌عنوان پیش‌فرض
6. join راحت Dapper بین‌schema
7. Provider ID به‌عنوان PK داخلی
8. `name_fa` / `name_en` / `name_ar`
9. `usd_price` / `irr_price` / `eur_price`
10. پول با `float` / `double`
11. نوع PostgreSQL `money` برای Money دامنه
12. `TOMAN` به‌عنوان CurrencyCode تعریف‌نشده
13. DateTime مبهم
14. ستون‌های تکراری Gregorian/Persian برای همان تاریخ
15. magic IDs (`-1`, `-2`, `0` به‌عنوان None/All/…)
16. یک Translation table سراسری
17. یک ExternalReference mega-table سراسری بدون ADR
18. `is_deleted` سراسری روی همه
19. `BaseEntity` همه‌منظوره با concerns نامرتبط
20. JSONB برای دور زدن مدل دامنه
21. `EnsureCreated` در production lifecycle
22. Demo data مخلوط با reference seed تولیدی
23. `updated_at` به‌عنوان concurrency token
24. تراکنش روتین چند-DbContext

---

## 21. Intentionally Deferred (Persistence)

- نسخه‌های پکیج EF / Npgsql / NodaTime
- تکنیک StronglyTypedId
- ساختار فیزیکی پروژه‌ها
- migration runner
- schema دقیق Outbox/Inbox و dispatcher
- encryption-at-rest
- retention PII
- audit-event storage
- schemaهای Search/SEO/provider-specific

---

## 22. مثال‌های عملی Persistence

### Tour → Hotel (ارجاع منطقی)

```text
Module: tour
Table:  tour.tour_hotel_options
Column: hotel_id uuid NOT NULL   -- logical PlaceId (Hotel-kind Place; alias column name OK until Tour)
FK to place.places / place.hotels: NONE (default)
EF navigation to Hotel: NONE
```

### Booking snapshot vs live Pricing

```text
pricing owns live rates + Quote calculation
booking owns accepted commercial snapshot columns/tables
payment settles accepted obligation
live rate UPDATE must not UPDATE historical booking snapshot rows
```

### Module-local Outbox

```text
BEGIN;
  -- booking state change
  INSERT INTO booking.outbox_messages (...);
COMMIT;
-- dispatcher reads booking.outbox_messages asynchronously
```
