# ADR 0004 — Temporal Model with NodaTime and IANA Timezones

- **Status:** Proposed
- **Date:** 2026-08-10
- **Task:** TC-P00-T003
- **Related:** [`../data/03-temporal-model.md`](../data/03-temporal-model.md)

---

## Context

سفر intrinsically timezone-heavy است: پرواز IKA/IST، check-in هتل، itinerary روزانه، و timestampهای سیستم معانی متفاوت دارند. مدل‌سازی همه با `DateTime` BCL یا «همه UTC» باعث از دست رفتن برنامهٔ محلی منتشرشده و ابهام معنایی می‌شود.

Locale نباید تقویم را تحمیل کند؛ تقویم presentation preference است.

---

## Decision

1. مدل زمانی Domain/Application به **NodaTime** قفل می‌شود (نصب/نسخه در P01).
2. Instantهای سیستم/ممیزی → `Instant` / PostgreSQL `timestamptz`.
3. تاریخ بدون timezone → `LocalDate` / `date`.
4. ساعت محلی بدون تاریخ → `LocalTime` / `time`.
5. زمان‌بندی سفر مهم: `LocalDateTime` + IANA `TimeZoneId` و در صورت نیاز Instant حل‌شده؛ فقط UTC کافی نیست.
6. شناسهٔ timezone کاننیکال: **IANA** (نه Windows-only به‌عنوان مدل هسته).
7. Calendar مستقل از Locale است؛ ستون‌های موازی Gregorian/Persian برای همان تاریخ ممنوع‌اند.
8. نام‌گذاری معنایی الزامی است (`CreatedAt`, `DepartureLocalDateTime`, …) — نه `Date`/`DateTime` مبهم.

---

## Alternatives Considered

| گزینه | چرا کنار گذاشته شد |
|-------|---------------------|
| فقط BCL `DateTime` | ابهام Kind؛ مدل‌سازی ضعیف Local vs Instant |
| همه چیز UTC Instant | برنامهٔ محلی پرواز/هتل از دست می‌رود |
| `DateTimeOffset` همه‌جا | جایگزین کامل LocalDate/LocalTime/zone identity نیست |
| Windows timezone IDs به‌عنوان هسته | portability ضعیف؛ استاندارد travel IANA است |
| Locale ⇒ Calendar ضمنی | خلاف استقلال Locale/Calendar |

---

## Consequences

### مثبت

- معنای زمانی صریح در دامنه
- حفظ schedule محلی منتشرشده
- هم‌راستایی با PostgreSQL types مناسب
- کاهش باگ DST/timezone در travel

### منفی / هزینه

- وابستگی به NodaTime و یادگیری تیم
- mapping EF/Npgsql باید با دقت در P01 ساخته شود
- مدل‌سازی غنی‌تر از یک فیلد DateTime واحد است

### Mitigation

- primitives و conventions مشترک در foundation
- مثال‌های دامنه (پرواز، هتل) در اسناد data
- اجتناب از DateTime مبهم در code review / Architecture Tests بعدی

---

## Migration / Impact

پکیج NodaTime، Npgsql integration، و conversions در P01. بدون پیاده‌سازی در این Task.
