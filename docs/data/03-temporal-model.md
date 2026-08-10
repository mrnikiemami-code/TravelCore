# Temporal Model — مدل زمانی TravelCore

منبع سطح‌بالا: [`../architecture/07-data-architecture.md`](../architecture/07-data-architecture.md)  
ADR مرتبط (Proposed): [`../adr/0004-temporal-model-nodatime.md`](../adr/0004-temporal-model-nodatime.md)

TravelCore یک سیستم travel با فشار timezone است. مدل زمانی Domain/Application نباید روی `DateTime` مبهم BCL تکیه کند.

---

## 1. جهت قفل‌شده: NodaTime

مدل زمانی Domain/Application به **NodaTime** قفل می‌شود.

در این Task پکیج نصب نمی‌شود. نسخه‌ها و یکپارچگی Npgsql → P01.

NodaTime برای انواع معنایی صریح انتخاب شده:

```text
Instant
LocalDate
LocalTime
LocalDateTime
DateTimeZone
ZonedDateTime (مفهومی)
```

از `DateTime` مبهم BCL در مدل‌سازی دامنه پرهیز کنید.

---

## 2. System Instants

زمان‌های سیستم/ممیزی نمایانگر Instant واقعی‌اند.

نمونه‌ها:

```text
BookingCreated
PaymentAttempted
QuoteCreated
EventOccurred
created_at / updated_at
```

| لایه | نمایش |
|------|--------|
| Domain | `Instant` |
| PostgreSQL | `timestamp with time zone` (`timestamptz`) |

Persistence باید Instantها را یکنواخت نرمال کند. timezone نشست/زیرساخت ترجیحاً UTC برای عملیات infrastructure.

---

## 3. LocalDate

تاریخی بدون معنای Instant/timezone:

| لایه | نمایش |
|------|--------|
| Domain | `LocalDate` |
| PostgreSQL | `date` |

نمونه‌ها:

- تاریخ check-in هتل
- روز تقویمی itinerary تور
- تاریخ تولد

فقط چون «تاریخ وجود دارد» آن را به UTC timestamp تبدیل نکنید.

---

## 4. LocalTime

ساعت دیواری محلی بدون تاریخ:

| لایه | نمایش |
|------|--------|
| Domain | `LocalTime` |
| PostgreSQL | `time without time zone` |

مثال:

```text
Hotel check-in time: 14:00
```

به UTC تبدیل نشود.

---

## 5. رویدادهای زمان‌بندی‌شدهٔ سفر

زمان‌بندی سفر اغلب به **هر دو** نیاز دارد:

1. تاریخ/ساعت محلی منتشرشده
2. هویت timezone

### مثال E — پرواز IKA

```text
Airport:              IKA
DepartureLocalDateTime: 2026-09-12 05:20
DepartureTimeZoneId:    Asia/Tehran
DepartureInstant:       (resolved from local + zone)
```

برای رویدادهای حمل‌ونقل مهم، حفظ:

- معنای برنامهٔ محلی اصلی (`LocalDateTime` + `TimeZoneId`)
- و در صورت مفید/لازم، Instant حل‌شده

ترجیح داده می‌شود.

فقط UTC ذخیره نکنید و برنامهٔ محلی منتشرشده را از دست ندهید.

---

## 6. شناسهٔ Timezone — IANA

شناسه‌های timezone کاننیکال از **IANA** هستند:

```text
Asia/Tehran
Europe/Istanbul
Asia/Dubai
```

معنای هستهٔ travel را با نام‌های Windows-only مدل نکنید. Adapterها در صورت نیاز IDهای platform-specific را ترجمه می‌کنند.

---

## 7. تقویم مستقل از Locale

Calendar یک ترجیح presentation/input است.

**توسط Locale ضمنی تعیین نمی‌شود.**

مثال معتبر:

```text
Locale: fa
Calendar: Gregorian
```

تقویم پارسی هم ممکن است توسط UI انتخاب شود.

### ممنوع

ستون‌های تکراری مثل:

```text
departure_date_gregorian
departure_date_persian
```

تاریخ معنایی کاننیکال را persist کنید. تبدیل تقویم در مرز input/presentation است.

اگر دامنهٔ آینده واقعاً به حفظ منبع تقویمی اصلی نیاز داشت، باید صریحاً مدل شود — نه با ستون‌های موازی پیش‌فرض.

---

## 8. ممنوع: DateTime مبهم

از propertyهای دامنه با نام‌های مبهم مثل:

```text
Date
DateTime
Time
```

وقتی معنا روشن نیست پرهیز کنید.

ترجیح نام‌های معنایی:

```text
CreatedAt
DepartureLocalDateTime
DepartureTimeZoneId
DepartureInstant
CheckInDate
CheckInTime
```

نوع باید با معنا جور باشد:

| نام نمونه | نوع مفهومی |
|-----------|------------|
| CreatedAt | Instant |
| CheckInDate | LocalDate |
| CheckInTime | LocalTime |
| DepartureLocalDateTime | LocalDateTime |
| DepartureTimeZoneId | IANA string / DateTimeZone id |
| DepartureInstant | Instant |

---

## 9. Locale ≠ Currency ≠ Calendar ≠ Timezone

این چهار ترجیح مستقل‌اند (قانون Constitution). مدل زمانی این استقلال را حفظ می‌کند:

- Locale مسیر/زبان UI را راهبری می‌کند
- Currency ارز نمایش/تسویه را
- Calendar نحوهٔ ورود/نمایش تاریخ را
- Timezone معنای محلی زمان‌بندی را

---

## 10. نگاشت خلاصه PostgreSQL

| مفهوم NodaTime | PostgreSQL |
|----------------|------------|
| Instant | `timestamptz` |
| LocalDate | `date` |
| LocalTime | `time` |
| LocalDateTime | معمولاً `timestamp without time zone` + ستون timezone جدا |
| TimeZoneId | `text` (IANA id) |

جزئیات mapping EF/Npgsql → P01.

---

## 11. ضدالگوهای زمانی

- همهٔ تاریخ‌ها به‌عنوان UTC timestamp
- از دست دادن local schedule با ذخیرهٔ فقط Instant
- Windows timezone به‌عنوان مدل هسته
- Locale که تقویم را تحمیل کند
- ستون‌های موازی شمسی/میلادی
- `DateTime` مبهم در Domain
- تبدیل check-in time محلی به UTC

---

## 12. Intentionally Deferred

- نسخهٔ پکیج NodaTime و Npgsql NodaTime plugin
- الگوی دقیق value conversion در EF
- سیاست DST edge-case برای انواع خاص پرواز/هتل (در فاز دامنه)
