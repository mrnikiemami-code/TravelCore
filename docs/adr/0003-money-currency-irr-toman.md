# ADR 0003 — Money, Currency, and IRR/Toman Policy

- **Status:** Accepted
- **Date:** 2026-08-10
- **Task:** TC-P00-T003
- **Related:** [`../data/02-money-and-currency.md`](../data/02-money-and-currency.md) · Constitution § پول

---

## Context

TravelCore قیمت‌های mixed-currency واقعی دارد (مثلاً USD + IRR). بازار ایران اغلب ورودی/نمایش تومان دارد در حالی که واحد رسمی ریال است. استفاده از `float`، ستون‌های ثابت per-currency، یا `TOMAN` به‌عنوان کد ارز، تاریخچهٔ مالی و Quote را فاسد می‌کند.

چند ماژول (Pricing، Booking، Payment، HotelBooking، Flight) به نمایش پولی نیاز دارند؛ قرار دادن نوع Money فقط داخل Pricing باعث تکرار یا وابستگی نادرست می‌شود.

---

## Decision

1. `Money = { Amount: decimal, CurrencyCode }` یک primitive جهانی در مرز بسیار کوچک `Domain.Primitives` آینده است (الان ساخته نشود).
2. Amount هرگز `float`/`double` نیست. PostgreSQL: `numeric` (پیش‌فرض `numeric(24,8)` برای مبلغ). نوع `money` پستگرس ممنوع است.
3. `CurrencyCode` uppercase canonical است؛ ممکن است کدهای non-fiat پیکربندی‌شده مثل `USDT` را شامل شود. Metadata در ReferenceData.
4. **IRR** کاننیکال است. **Toman** واحد DISPLAY/INPUT است: `1 Toman = 10 IRR`. تبدیل ضمنی ممنوع. `CurrencyCode = TOMAN` بدون ADR آینده ممنوع است.
5. Adapterهای provider که Toman می‌دهند باید صریحاً به IRR تبدیل کنند و واحد منبع را بدانند.
6. نرخ mixed-currency با `PriceComponents[]` رابطه‌ای ذخیره می‌شود (Amount + CurrencyCode + Purpose). ستون‌های `usd_price`/`irr_price` و total تک‌ارزی به‌عنوان SoR ممنوع‌اند.
7. `ExchangeRate` باید Source/Target/Rate/Provider/CapturedAt داشته باشد (precision پیشنهادی `numeric(28,12)`).
8. `Price ≠ Quote ≠ Booking ≠ Payment`. تغییر نرخ زنده Booking تاریخی را بازنویسی نمی‌کند.

---

## Alternatives Considered

| گزینه | چرا کنار گذاشته شد |
|-------|---------------------|
| ذخیره فقط به یک ارز پایه | واقعیت پکیج‌های خارجی mixed-currency را نابود می‌کند |
| `CurrencyCode = TOMAN` | استاندارد مبهم؛ دوگانگی با IRR؛ خطای ۱۰× |
| PostgreSQL `money` | وابسته به locale؛ نامناسب برای چندارزی صریح |
| Money فقط داخل ماژول Pricing | تکرار نوع یا وابستگی نادرست در Booking/Payment |
| JSON blob برای کل نرخ | constraint/query/invariant ضعیف |

---

## Consequences

### مثبت

- حفظ مؤلفه‌های ارزی اصلی
- یک زبان مشترک برای پول در چند ماژول بدون SharedKernel بزرگ
- جلوگیری از باگ ۱۰× تومان/ریال با تبدیل صریح
- تاریخچهٔ Quote/Booking reproducible

### منفی / هزینه

- UI باید واحد ورودی/نمایش را صریح مدیریت کند
- گزارش‌گیری ممکن است نیاز به سیاست تبدیل جدا داشته باشد
- مرز Domain.Primitives باید بسیار کوچک بماند وگرنه dumping ground می‌شود

### Mitigation

- قوانین UI/adapter صریح برای Toman
- taxonomy Purpose در فاز Pricing
- ممنوعیت گسترش بی‌رویهٔ Domain.Primitives بدون تأیید معمار

---

## Migration / Impact

نوع Money، mapping EF، و seed currency در P01 / فازهای ماژول. این ADR فقط سیاست را ثبت می‌کند.
