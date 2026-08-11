# Formatting and Cultural Preferences

منبع: [`../architecture/11-internationalization-architecture.md`](../architecture/11-internationalization-architecture.md)  
Money ADR 0003 Accepted · Temporal ADR 0004 Accepted · Bidi ADR 0006 Accepted

---

## 1. قانون مرکزی

```text
Locale ≠ Currency ≠ Calendar ≠ TimeZone
```

Locale ممکن است **presentation formatting** را تحت تأثیر قرار دهد.  
معنای دامنهٔ Money / Instant / LocalDate / TimeZoneId را عوض نمی‌کند.

---

## 2. Number Formatting

Locale ممکن است روی این‌ها اثر بگذارد:

- digit shapes
- group separators
- decimal separators

اما باید تفکیک شود:

| نوع | رفتار |
|-----|--------|
| مقدار انسانی (قیمت نمایشی، تعداد) | formatting locale ممکن است |
| شناسه / کد | حفظ کاراکتر معنایی |

### شناسه‌ها localize-as-number نشوند

```text
EK978 · IKA · booking reference · passport · phone · provider code
```

ارقام لاتین شناسه‌ها را خودکار به ارقام پارسی/عربی تبدیل نکنید. قواعد bidi Accepted ادامه دارند.

---

## 3. Money Formatting

Money = `{ Amount, CurrencyCode }` (Data Architecture).

Locale presentation را کنترل می‌کند؛ **ارز زیربنایی را عوض نمی‌کند**.

### مثال ۱۱

```text
Locale: fa
Money: 1290 USD
→ still USD (no silent FX because locale changed)
```

### IRR / Toman — مثال ۱۴

```text
IRR = canonical currency
Toman = explicit display/input unit (1 Toman = 10 IRR)
```

Locale فارسی **اجبار نمی‌کند** همهٔ IRRها تومان نشان داده شوند.  
نمایش تومان = سیاست محصول/کاربر/display. واحد همیشه واضح باشد.

### Currency symbol / code

در UI سفر چندارزی، نمایش unambiguous ترجیح دارد (مثلاً `1290 USD` واضح‌تر از `$` مبهم).

### Mixed currency — مثال ۱۳

```text
1,290 USD
+
119,900,000 IRR
```

تغییر locale ممکن است جداکننده/ارقام/برچسب را عوض کند.  
جمع یا تبدیل خودکار مؤلفه‌ها ممنوع است.

---

## 4. Date Formatting

معنای زمانی کاننیکال از ADR 0004 است.

UI ممکن است تاریخ را بر اساس locale و **calendar preference** متفاوت نشان دهد — بدون mutate کردن LocalDate/Instant.

---

## 5. Calendar ≠ Locale — مثال ۱۰

```text
Locale: fa
Direction: RTL
Calendar: Gregorian
Currency: USD
TimeZone: Europe/Istanbul
→ VALID and must be supported
```

`fa` تقویم پارسی را اجباری نمی‌کند.  
`ar` تقویم هجری را اجباری نمی‌کند.  
`en` یک تقویم واحد را ابدی اجباری نمی‌کند.

Calendar = presentation/user preference مستقل.

ستون‌های موازی Gregorian/Persian برای همان تاریخ معنایی ممنوع (طبق Data Architecture).

---

## 6. Time Format

Locale/preferences ممکن است 12h/24h و نام روز/ماه را تحت تأثیر قرار دهند.

معنای زمان سفر صریح می‌ماند:

```text
DepartureLocalDateTime: 2026-09-12 05:20
TimeZoneId: Asia/Tehran
(+ Instant when required)
```

Formatting نباید schedule محلی/timezone را نابود کند.

---

## 7. TimeZone ≠ Locale

Locale، timezone رویداد سفر را تعیین نمی‌کند.

کاربر فارسی‌زبان که پرواز استانبول را می‌بیند همچنان به semantics `Europe/Istanbul` برای زمان محلی استانبول نیاز دارد.

User display timezone ≠ event timezone.

برچسب انسانی timezone ممکن است localize شود؛ هویت canonical باقی می‌ماند: **IANA TimeZoneId** (`Asia/Tehran`). متن ترجمه‌شده جایگزین ID persistشده نشود.

---

## 8. RTL / Bidi — مثال ۱۲

Locale جهت پیش‌فرض سند را می‌دهد.  
صفحه ≠ مقدار (ADR 0006).

در UI فارسی:

```text
IKA · IST · EK978 · USD · USDT · booking ref · email · URL
```

اغلب LTR می‌مانند. Bidi ≠ ترجمه.

---

## 9. Content Direction

محتوای ترجمه‌شدهٔ ساخت‌یافته معمولاً جهت locale را به ارث می‌برد.  
Editorial غنی ممکن است قطعهٔ mixed-direction داشته باشد — با جهت inline معنایی، نه Unicode control characters به‌عنوان معماری پیش‌فرض.

---

## 10. UGC — مثال ۱۵

زبان UGC ممکن است با UI locale فرق کند.

```text
Persian UI displays an English review
→ do not falsely label review content language as Persian
```

ترجمهٔ اجباری خودکار UGC ممنوع. metadata زبان/جهت بعداً در صورت نیاز.

---

## 11. Database Culture

رفتار DB/query به `CurrentCulture` / OS locale ماشین وابسته نباشد.  
Locale در عملیات localized صریح باشد.

---

## 12. Sorting / Collation / Normalization

مرتب‌سازی و جستجوی متن ممکن است locale-sensitive باشد.  
یک ترتیب لغوی جهانی برای fa/ar/en فرض نشود.

Unicode normalization برای فارسی/عربی مهم است و باید intentional و field-specific باشد — نه مخرب ad-hoc.

جزئیات collation/search → Search architecture / implementation later.  
Slug normalization → SEO (T006) — این Task تصمیم «همه slugها لاتین» یا عکس آن را نمی‌گیرد.
