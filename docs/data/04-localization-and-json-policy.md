# Localization and JSON Policy — localization و JSONB

منبع سطح‌بالا: [`../architecture/07-data-architecture.md`](../architecture/07-data-architecture.md)  
SEO (مالکیت slug): به معماری SEO بعدی و Constitution مراجعه شود.

---

## 1. ممنوع: ستون‌های زبان ثابت

الگوی زیر ممنوع است:

```text
name_fa
name_en
name_ar
description_fa
description_en
...
```

این الگو:

- مقیاس‌پذیری locale را می‌شکند
- schema را برای هر زبان جدید عوض می‌کند
- با قانون Constitution در تضاد است

---

## 2. جدول ترجمهٔ متعلق به ماژول

از translation recordهای متعلق به **همان ماژول** استفاده کنید.

مفهومی:

```text
Destination        → DestinationTranslation
Place              → PlaceTranslation
TourProduct        → TourProductTranslation
```

هویت ترجمهٔ معمول:

```text
OwnerId + LocaleCode
```

یکتایی مفهومی:

```text
UNIQUE (owner_id, locale_code)
```

### مثال B — Destination

```text
destination.destinations
destination.destination_translations
  destination_id uuid NOT NULL
  locale_code    text NOT NULL
  name           ...
  ...
  CONSTRAINT uq_destination_translations_owner_locale
    UNIQUE (destination_id, locale_code)
```

هر ماژول ترجمه‌های خودش را مالک است.

### ممنوع

یک جدول Translation سراسری برای همهٔ ماژول‌ها.

---

## 3. LocaleCode

`LocaleCode` نرمال و قابل گسترش است.

Locales اولیه:

```text
fa · en · ar
```

معماری باید بدون redesign schema از tagهای آینده پشتیبانی کند:

```text
fa-IR · en-US · ar-AE
```

از semantics تگ زبان کاننیکال استفاده کنید. سه ستون زبان در دیتابیس hardcode نشود.

ReferenceData می‌تواند metadata locale را نگه دارد؛ خود مقدار در translationها `LocaleCode` است.

---

## 4. سه دستهٔ ترجمه — یکی نیستند

| دسته | محل | نمونه |
|------|-----|--------|
| UI Translation | منابع i18n فرانت‌اند | برچسب دکمه، پیام فرم |
| Entity Translation | جدول ترجمهٔ ماژول | Name مقصد، عنوان تور |
| Editorial Content Translation | ساختارهای غنی‌تر متعلق به Content | بدنهٔ مقاله، بلوک‌ها |

هر سه را به یک مدل persistence اجباری نکنید.

Entity translation ساخت‌یافته (Name، ShortDescription، …) مناسب جدول ترجمه است.  
Editorial Content ممکن است ساختار Content-owned غنی‌تر داشته باشد.

---

## 5. مالکیت LocalizedSlug / SEO

مسیر و slug محلی‌شده برای SEO متعلق به **معماری SEO** است.

مالکیت مسیر کاننیکال را داخل هر جدول translation تکرار نکنید.

ترجمه ممکن است محتوای محلی داشته باشد. SEO مالک است بر:

```text
SeoRoute
LocalizedSlug
Canonical
Redirect history
IndexPolicy
```

جزئیات persistence SEO بعداً مشخص می‌شود (عمداً deferred).

یادآوری Constitution: ترجمهٔ یک locale فقط وقتی منتشر و indexable است که محتوای آن locale منتشر محسوب شود — fallback فارسی را از URL انگلیسی SEO افشا نکنید.

---

## 6. سیاست JSONB — اجازه و ممنوع

JSONB وقتی مجاز است که داده واقعاً flexible / document-shaped باشد.

### کاندیدهای خوب بالقوه

- snapshot payload خام provider
- metadata انعطاف‌پذیر provider-specific
- کانفیگ Content Block جایی که پذیرفته شود
- payload تشخیصی webhook
- metadata توسعه‌پذیر غیرauthoritative

JSONB نباید صرفاً برای اجتناب از مدل‌سازی رابطه‌ای استفاده شود.

### میان‌برهای ممنوع

- ریختن کل Tour داخل JSONB
- قرار دادن PriceComponents به‌عنوان منبع حقیقت داخل JSONB
- قرار دادن هستهٔ state Booking داخل JSONB
- ترجمه‌های رابطه‌ای پیش‌فرض داخل JSONB

اگر فیلد:

- در invariant شرکت دارد
- زیاد query می‌شود
- به constraint نیاز دارد
- به join نیاز دارد
- ساختار دامنهٔ پایدار دارد

→ مدل رابطه‌ای را ترجیح دهید.

---

## 7. Provider Raw Payload

Payload خام provider در صورت مفید بودن عملیاتی قابل نگهداری است، اما:

- مدل دامنهٔ authoritative TravelCore نیست
- قواعد PII/امنیت اعمال می‌شود
- retention بعداً تعریف می‌شود
- جایگزین mapping نرمال‌شدهٔ provider نمی‌شود

---

## 8. ارتباط با Money و زمان

Localization ارز/تاریخ را قاطی نکند:

- نمایش ارز و واحد Toman در مرز UI است؛ persistence کاننیکال طبق [`02-money-and-currency.md`](02-money-and-currency.md)
- تقویم در مرز presentation است؛ تاریخ معنایی طبق [`03-temporal-model.md`](03-temporal-model.md)

---

## 9. ضدالگوها

- `name_fa` / `name_en` / `name_ar`
- یک Translation table سراسری
- hardcode کردن فقط سه زبان در schema
- کپی مالکیت SeoRoute داخل هر translation
- JSONB به‌جای PriceComponents / Tour aggregate / Booking core
- یکی‌دانستن UI i18n با Entity translation با Editorial content

---

## 10. Intentionally Deferred

- schema دقیق جداول SEO / LocalizedSlug
- schema کامل Content blocks
- انتخاب کتابخانهٔ i18n فرانت‌اند
- سیاست دقیق publication per-locale در هر ماژول (جزئیات فاز ماژول)
