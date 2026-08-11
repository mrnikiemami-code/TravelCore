# Translation Ownership and Lifecycle

منبع: [`../architecture/11-internationalization-architecture.md`](../architecture/11-internationalization-architecture.md)  
Data localization: [`../data/04-localization-and-json-policy.md`](../data/04-localization-and-json-policy.md)

---

## 1. سه دسته — قفل‌شده

| دسته | چیست | مالک |
|------|------|------|
| **UI Translation** | واژگان رابط | منابع localization اپ/فرانت |
| **Entity Translation** | برچسب/فیلد Entity | ماژول مالک Entity |
| **Editorial Translation** | محتوای غنی editorial | Content / editorial structures |

این سه را به یک مکانیزم جهانی اجباری نکنید.

---

## 2. UI Translation

نمونه‌ها: Search · Book now · No results · Passengers

معمولاً با application localization resources نسخه‌گذاری می‌شود — نه جداول کسب‌وکار دلخواه.

برای هر UI label نیاز به DB round-trip اجباری نیست.  
کتابخانهٔ دقیق فرانت‌اند → P02 (بدون انتخاب در این Task).

---

## 3. Entity Translation

هر ماژول ترجمه‌های Entityهای قابل‌ترجمه‌اش را مالک است.

```text
destination.destinations
destination.destination_translations
  DestinationId + LocaleCode + Name + ...
  UNIQUE (DestinationId, LocaleCode)
```

### ممنوع

- یک Translation table سراسری برای همهٔ ماژول‌ها
- `name_fa` / `name_en` / `name_ar`

### مثال ۴

```text
Same DestinationId
fa Name = استانبول
en Name = Istanbul
ar Name = اسطنبول
```

ترجمه ≠ ایجاد Aggregate جدید.

روابط به **Domain identity** اشاره می‌کنند (`DestinationId`)، نه `DestinationTranslationId`. در render، locale درخواستی representation را انتخاب می‌کند.

---

## 4. Editorial Translation

محتوای long-form lifecycle و ساختار متفاوتی از برچسب Entity دارد.

Content ممکن است از localized article versions · rich blocks · translation relationships استفاده کند (طراحی بعدی).

محتوای غنی را فقط برای یکنواختی داخل ردیف‌های کوچک Entity مجبور نکنید.

---

## 5. Source Locale

یک آیتم قابل‌ترجمه ممکن است `SourceLocale` داشته باشد (مثلاً `fa`) برای workflow editorial.

Source locale **اجبار نمی‌کند** که public fallback به آن زبان انجام شود.

---

## 6. Translation Lifecycle

وجود ترجمه ≠ انتشار.

وضعیت‌های مفهومی (مدل دقیق per-module):

```text
Draft · Ready · Published · Archived
```

### Publication per locale

منتشر بودن Entity تجاری ⇒ همهٔ localeها منتشر نیستند.

### مثال ۵

```text
fa: Published
en: Draft
ar: Missing
```

---

## 7. Completeness

وجود ردیف ≠ ready to publish.

نمونه‌های ناقص: Name هست / Description نیست · SEO route نیست · متن حقوقی لازم نیست.

هر ماژول فیلدهای الزامی قبل از publish locale را تعیین می‌کند — یک قانون جهانی «همهٔ فیلدهای nullable باید پر شوند» ممنوع است.

اعتبارسنجی authoritative انتشار با Application ماژول است، نه UI به‌تنهایی.

---

## 8. Business Lifecycle vs Localized Publication

| مفهوم | معنی |
|-------|------|
| Business availability | مثلاً TourDeparture bookable |
| Translation availability | صفحهٔ عمومی locale مشخص منتشر است؟ |

### مثال

Departure bookable + English translation Missing → عملیات ممکن است ادامه یابد؛ صفحهٔ انگلیسی عمومی ممکن است unavailable باشد.

Unpublish یک ترجمه لزوماً Entity را deactivate نمی‌کند.  
Archive/unavailable شدن Entity مالک → ترجمه‌ها Entity را زنده نگه نمی‌دارند.

---

## 9. Provider Content Boundary

Providerهای Flight/Hotel ممکن است متن محلی ناسازگار بدهند.

Mapping در **adapter boundary** به Locale TravelCore.  
کد locale provider ≠ شناسهٔ canonical TravelCore.

محتوای خام provider با editorial TravelCore یکی نیست.  
Missing Persian از provider ⇒ برچسب نادرست English به‌عنوان Persian ممنوع.

---

## 10. Machine Translation

ممکن است بعداً کمک editorial باشد.  
وجود machine translation ⇒ خودکار **Published** نیست. نیاز به workflow کیفیت/انتشار.

Provenance آینده (human / machine / imported / provider) نباید توسط معماری فعلی مسدود شود — پیاده‌سازی الان نه.

---

## 11. ReferenceData

ممکن است metadata locale/language را مالک باشد.  
مالک همهٔ ترجمه‌های کسب‌وکار پلتفرم نیست.

---

## 12. Enum / Status Labels

کدهای پایدار زبان‌خنثی می‌مانند (`BookingStatus = Confirmed`).  
برچسب UI localize می‌شود. متن ترجمه‌شده به‌عنوان هویت status persist نشود.

---

## 13. Errors و Domain

Domain به UI locale وابسته نیست.  
Invariantها و error identity معنایی‌اند؛ presentation ترجمه می‌کند.

UI translations داخل Domain ممنوع.

Notification: ماژول‌ها semantic events/facts می‌فرستند؛ Tour HTML فارسی ایمیل تولید نمی‌کند. جزئیات Notification بعداً.

---

## 14. Historical Snapshots

Booking/Payment حقایق کسب‌وکار را نگه می‌دارند.  
متن قراردادی پذیرفته‌شده ممکن است snapshot شود — متفاوت از مالکیت ترجمهٔ جاری.

---

## 15. Admin Editorial Direction

Admin آینده باید Missing / Draft / Ready / Published را واضح نشان دهد و source locale را در صورت ارتباط.  
Side-by-side translation نباید Aggregate کسب‌وکار را duplicate کند.

الان Admin UI پیاده نشود.
