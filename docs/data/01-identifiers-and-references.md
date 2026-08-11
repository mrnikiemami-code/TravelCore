# Identifiers and References — شناسه‌ها و ارجاعات

منبع سطح‌بالا: [`../architecture/07-data-architecture.md`](../architecture/07-data-architecture.md)
مرز ماژول: [`../architecture/04-module-boundaries.md`](../architecture/04-module-boundaries.md)
ADR مرتبط (Accepted): [`../adr/0002-uuid-v7-domain-identity.md`](../adr/0002-uuid-v7-domain-identity.md)
ADR schema/FK (Accepted): [`../adr/0001-postgresql-schema-per-module.md`](../adr/0001-postgresql-schema-per-module.md)

---

## 1. هویت دامنه — UUID v7

هویت‌های دامنهٔ قابل‌ارجاع/خارجی TravelCore از **UUID version 7** استفاده می‌کنند.

نوع دیتابیس: `uuid`

نمونه‌ها:

```text
TourProductId
TourDepartureId
DestinationId
PlaceId
HotelId
PartyId
BookingId
PaymentId
MediaAssetId
```

### چرا

- یکتایی جهانی بدون sequence مرکزی
- مناسب مرز ماژول و استخراج آینده
- locality زمانی/ایندکس بهتر از UUID v4 کاملاً تصادفی
- هویت قبل از persistence در دسترس است
- مناسب رویداد / Outbox

### تولید

هویت معمولاً توسط **Application قبل از persistence** تولید می‌شود.
برای به‌دست‌آوردن هویت دامنه به round-trip دیتابیس وابسته نشوید.

---

## 2. Strongly Typed IDs

در Domain/Application، هویت‌ها در صورت عملی بودن strongly typed باشند.

مفهومی: `TourProductId`، `HotelId`، `BookingId`، `PartyId` نباید در کد دامنه به‌عنوان `Guid` خام قابل‌تعویض رفتار کنند.

مقدار persistence زیرین همچنان UUID است.

در این Task کتابخانهٔ third-party StronglyTypedId انتخاب/نصب نمی‌شود. مکانیسم پیاده‌سازی → P01.
قانون معماری: **ایمنی معنایی نوع**.

---

## 3. شناسهٔ داخلی فرزند Aggregate

هر ردیف لزوماً UUID عمومی ندارد.

ردیفات aggregate-internal که:

- از مرز Aggregate خارج نمی‌شوند
- مستقلاً addressable نیستند
- توسط ماژول دیگر ارجاع نمی‌شوند
- هویت Public API نیستند

ممکن است از:

- composite key
- bigint surrogate محلی
- کلید persistence محلی دیگر ماژول

استفاده کنند.

اما وقتی Entity مستقلاً addressable شد یا از مرز ماژول/API عبور کرد → استراتژی هویت دامنهٔ تأییدشده (UUID v7).

هرگز bigint محلی را به‌عنوان هویت عمومی تصادفی افشا نکنید.

---

## 4. کدهای استاندارد پایدار (Natural Codes)

برخی مفاهیم کد استاندارد طبیعی دارند و فقط برای یکنواختی به UUID نیاز ندارند.

نمونه‌ها:

| مفهوم | نمونه |
|-------|--------|
| CurrencyCode | `USD` · `EUR` · `IRR` · `USDT` |
| LocaleCode | `fa` · `en` · `ar` |
| ISO country | طبق استاندارد |
| IANA timezone | `Asia/Tehran` · `Europe/Istanbul` |

ReferenceData می‌تواند metadata توصیفی این کدها را مالک باشد.
در قراردادهای عمومی/دامنه، استانداردهای بین‌المللی معنادار را با integer ID دلخواه جایگزین نکنید.

---

## 5. ممنوع: Magic Sentinel IDs

هرگز از شناسه‌های جادویی مثل:

```text
-1 · -2 · 0
```

برای معناهایی مثل None · Multiple · Unknown · Inherited · All استفاده نکنید.

به‌جای آن:

- nullable
- enum / value object صریح
- status صریح
- مدل صریح

بسته به معنای دامنه.

---

## 6. ارجاع بین‌ماژولی

ارجاع بین‌ماژولی به‌صورت **scalar logical identity** ذخیره می‌شود.

### مثال A — Tour referencing Hotel

```csharp
// Conceptual — no Place.Hotel navigation
public class TourHotelOption
{
    public HotelId HotelId { get; set; }
}
```

```text
tour.tour_hotel_options.hotel_id  -- uuid, logical reference
```

Booking می‌تواند `TourProductId` و `TourDepartureId` نگه دارد بدون map کردن Entityهای Tour.

---

## 7. FK فیزیکی بین‌ماژولی — پیش‌فرض ممنوع

پیش‌فرض قفل‌شده:

**بدون** database-level foreign key بین schemaهای ماژول.

```text
tour.tour_hotel_options.hotel_id
  → logical Place.HotelId
  → NO PostgreSQL FK to place.hotels
```

دلیل: coupling persistence بین schemaها و پیچیدگی migration/extraction.

یکپارچگی از طریق: قرارداد Application · validation · lifecycle · snapshot · event/projection.

### داخل همان ماژول

FK رابطه‌ای معمول است:

```text
tour.tour_departures.tour_product_id
  → FK → tour.tour_products.id
```

استثناء cross-module FK: تأیید معمار + ADR.

---

## 8. شناسهٔ Provider خارجی

External ID هرگز هویت اصلی داخلی TravelCore نمی‌شود.

### مثال F — Provider hotel mapping

```text
InternalId:      HotelId (UUID v7)
ProviderCode:    ProviderA
ExternalHotelId: 998812
```

نگاشت متعلق به ماژول مربوطه است (مثلاً HotelBooking برای inventory زنده؛ Place برای هویت کاتالوگ — طبق مرزهای مالکیت موجود).

یک mega-table سراسری `ExternalReference` بین‌ماژولی بدون ADR صریح ممنوع است.

---

## 9. یکتایی نگاشت Provider

معمولاً یکتایی مناسب enforce شود، مثلاً:

```text
(ProviderCode, ExternalId)  within relevant entity/provider scope
```

هویت داخلی TravelCore باید جایگزینی provider را تحمل کند (mapping عوض می‌شود؛ PK داخلی می‌ماند).

---

## 10. Dapper و ارجاعات

Dapper ارجاع منطقی را دور نمی‌زند و اجازهٔ join بین‌schema برای راحتی نمی‌دهد. همان قوانین مالکیت اعمال می‌شود.

---

## 11. خلاصهٔ تصمیم‌ها

| موضوع | تصمیم |
|-------|--------|
| Domain-facing ID | UUID v7 |
| DB type | `uuid` |
| Generation | Application-first |
| Strong typing | جهت اجباری؛ پیاده‌سازی P01 |
| Internal child rows | کلید محلی مجاز |
| Standard codes | natural code، نه UUID اجباری |
| Magic IDs | ممنوع |
| Cross-module ref | scalar ID |
| Cross-module FK | پیش‌فرض ممنوع |
| Provider ID | mapping جدا؛ نه PK داخلی |
