# Money and Currency — پول و ارز

منبع سطح‌بالا: [`../architecture/07-data-architecture.md`](../architecture/07-data-architecture.md)  
واژه‌نامه: [`../domain/glossary.md`](../domain/glossary.md)  
ADR مرتبط (Proposed): [`../adr/0003-money-currency-irr-toman.md`](../adr/0003-money-currency-irr-toman.md)

---

## 1. Money به‌عنوان Primitive جهانی

`Money` یک value concept جهانی است.

Pricing مالک قواعد قیمت، نرخ، سیاست تبدیل و Quote است — اما **مالک انحصاری نوع Money نیست**.

ماژول‌هایی که مشروعانه Money دارند:

- Pricing
- Booking (snapshots)
- Payment
- HotelBooking (provider offers)
- Flight (provider offers)

مرز مفهومی آینده (الان ساخته نشود):

```text
TravelCore.Domain.Primitives
  Money
  CurrencyCode
```

این مرز business module نیست و SharedKernel بزرگ هم نیست.

---

## 2. ساختار Money

```text
Money
├── Amount       : decimal
└── CurrencyCode : canonical uppercase code
```

### ممنوع برای Amount

- `float`
- `double`

### نمایش دیتابیس پیش‌فرض Amount

```text
numeric(24,8)
```

مگر دامنهٔ خاص دلیل مستند برای precision دیگر داشته باشد.

### ممنوع

نوع PostgreSQL `money` برای Money دامنه — به‌خاطر semantics وابسته به locale و عدم تطابق با مدل چندارزی صریح TravelCore.

از `numeric` / `decimal` استفاده کنید.

---

## 3. CurrencyCode

`CurrencyCode` صریح و نرمال‌شده به **uppercase canonical** است.

نمونه‌ها:

```text
USD · EUR · IRR · AED · TRY · USDT
```

فرض نکنید هر ارز TravelCore فقط ISO-4217 fiat است؛ کدهای non-fiat صریحاً پیکربندی‌شده (مثل `USDT`) ممکن است پشتیبانی شوند.

ReferenceData مالک metadata است:

- code
- display name
- symbol
- type
- minor unit configuration در صورت ارتباط

خود `Money` فقط به مقدار canonical `CurrencyCode` نیاز دارد.

---

## 4. IRR / Toman — تصمیم حیاتی

**IRR** کد ارز کاننیکال ایران است.

**Toman یک CurrencyCode جدا و تعریف‌نشده نیست.**

Toman واحد **DISPLAY / INPUT** مشتق از IRR است:

```text
1 Toman = 10 IRR
```

تبدیل **هرگز ضمنی نیست**.

مرز UI/Application باید بداند مقدار وارد/نمایش‌شده:

- IRR است، یا
- واحد نمایش Toman است.

Money persistشدهٔ کاننیکال برای ریال ایران:

```text
CurrencyCode = IRR
```

### مثال D

```text
User display:   11,990,000 Toman
Canonical Money: Amount = 119900000, CurrencyCode = IRR
```

### ممنوع

```text
CurrencyCode = TOMAN
```

مگر ADR آینده مدل رسمی non-ISO currency/unit را معرفی کند.

---

## 5. ورودی Provider بر حسب Toman

اگر provider مبلغ را به‌عنوان Toman توصیف کرد:

adapter باید **صریحاً** به IRR کاننیکال تبدیل کند و واحد منبع را بداند.

برای audit/debug، raw payload و metadata واحد منبع ممکن است جداگانه حفظ شود.

هرگز واحد مبلغ provider را خاموش فرض نکنید.

---

## 6. ذخیره‌سازی قیمت چندارزی (Mixed-Currency)

قیمت چندارزی باید با **مؤلفه‌های رابطه‌ای** مدل شود.

```text
TourRate
  └── PriceComponents[]
        ├── Amount
        ├── CurrencyCode
        └── Purpose
```

### مثال C

```text
Component 1: 1290 USD     (Purpose مثلاً PackagePrice)
Component 2: 119900000 IRR (Purpose مثلاً LocalCharge)
```

### ممنوع به‌عنوان مدل بنیادین

- serialize کردن نرخ authoritative عمدتاً به‌صورت یک JSON blob
- ستون‌های ثابت `usd_price` / `irr_price` / `eur_price`
- یک `total_amount` که ارزهای اصلی مؤلفه‌ها را نابود کند

معماری نباید خاموش همهٔ مؤلفه‌ها را به یک ارز تبدیل کند. تبدیل طبق سیاست Pricing هنگام Quote انجام می‌شود.

---

## 7. Purpose مؤلفهٔ قیمت

هر PriceComponent معنای Purpose دارد.

نمونه‌های مفهومی بعدی (taxonomy دقیق متعلق به طراحی دامنهٔ Pricing):

```text
PackagePrice · LocalCharge · Tax · ServiceFee · Surcharge
```

Data Architecture فقط الزام می‌کند مؤلفه‌ها صریح و مستقل currency-qualified بمانند.

---

## 8. ExchangeRate

`ExchangeRate` باید صریحاً حفظ کند:

| فیلد مفهومی | نقش |
|-------------|-----|
| SourceCurrency | ارز مبدأ |
| TargetCurrency | ارز مقصد |
| Rate | نرخ |
| Provider/Source | منبع |
| CapturedAt | زمان ثبت (Instant) |

Precision پیشنهادی persistence:

```text
numeric(28,12)
```

فقط «نرخ جاری» بدون زمینهٔ منبع/زمان persist نشود. محاسبات Quote/Booking تاریخی باید reproducible باشند.

---

## 9. Price ≠ Quote ≠ Booking ≠ Payment

| مفهوم | مالک / نقش persistence |
|-------|-------------------------|
| Price / TourRate | نرخ تجاری زنده — Pricing |
| Quote | پیشنهاد محاسبه‌شده برای درخواست مشخص در زمان مشخص — Pricing |
| Booking | snapshot تجاری پذیرفته‌شده — Booking |
| Payment | تسویه/lifecycle پرداخت واقعی — Payment |

تغییر نرخ زنده یا FX نباید حقایق Booking تاریخی را mutate کند.

### مثال G — Booking snapshot

```text
10:00  Quote Q = 1290 USD + 119,900,000 IRR → مشتری می‌پذیرد
       Booking snapshot پذیرفته‌شده را ذخیره می‌کند
15:00  نرخ زنده → 1350 USD + 125,000,000 IRR
       Booking قبلی بدون تغییر
       Payment تعهد پذیرفته‌شده را تسویه می‌کند
```

---

## 10. PassengerCategory ≠ Occupancy (یادآوری دامنه)

قیمت‌گذاری ممکن است هر دو محور را داشته باشد؛ آن‌ها یکی نیستند:

- PassengerCategory: Adult / Child / Infant
- Occupancy: Single / Double / ExtraBed / …

جزئیات دامنه در glossary و آیندهٔ Pricing؛ اینجا فقط تأکید می‌شود مدل پول این تمایز را خراب نکند.

---

## 11. ضدالگوهای پول

- `float` / `double` برای مبلغ
- PostgreSQL `money`
- `TOMAN` به‌عنوان CurrencyCode
- تبدیل ضمنی Toman↔IRR
- `UsdPrice` / `IrrPrice` ستون‌های ثابت
- یک total تک‌ارزی به‌عنوان منبع حقیقت mixed-currency
- JSONB به‌عنوان منبع حقیقت PriceComponents
- بازنویسی Booking با تغییر نرخ زنده
- ادغام Payment با Quote/Price

---

## 12. Intentionally Deferred

- taxonomy کامل Purpose
- schema کامل جداول Pricing
- سیاست دقیق rounding/minor unit per currency در Quote
- نسخهٔ پکیج‌ها و mapping EF برای Money
