# Cross-Module Communication — ارتباط بین‌ماژولی

قوانین وابستگی: [`05-dependency-rules.md`](05-dependency-rules.md)
مرز مالکیت: [`04-module-boundaries.md`](04-module-boundaries.md)

---

## الگوی کلی

```text
Prefer:
1) ID reference
2) narrow synchronous public application contract (when immediacy required)
3) semantic event + Outbox → downstream reaction
4) local projection / composed read model for high-volume reads
```

هر خواندن بین‌ماژولی را به «RPC همزمان داخل مونولیت» تبدیل نکنید.

---

## 1. ID Reference

روابط بین‌ماژولی در سطح دامنه، ارجاع منطقی شناسه‌اند.

```csharp
// Preferred conceptual shape
public class TourHotelOption
{
    public Guid HotelId { get; set; }
}
```

نه navigation به Entity متعلق به Place.

نوع دقیق شناسه → TC-P00-T003.

FK فیزیکی دیتابیس → تصمیم persistence جدا (T003)، نه شرط مالکیت دامنه.

---

## 2. Synchronous Public Application Contracts

وقتی اطلاعات فوری واقعاً لازم است:

| Consumer | Provider | Example question |
|----------|----------|------------------|
| Booking | Pricing | Quote X معتبر و قابل مصرف است؟ |
| Tour | Place | HotelId X وجود دارد و Hotel است؟ |
| Pricing | Tour | DepartureId X متعلق به TourProduct Y و قابل قیمت‌گذاری است؟ |

### Rules

- بدون اشتراک DbContext
- بدون نشت نوع Infrastructure
- بدون نشت EF Entity
- بدون نشت repository داخلی
- قرارداد معنایی و باریک
- فقط اطلاعات لازم به مصرف‌کننده

Forbidden: `IPlaceService.GetEverything()`.

---

## 3. Synchronous Validation vs Local Projection

| رویکرد | کی |
|--------|----|
| Synchronous contract | اعتبارسنجی فوری · داده کوچک · نیاز به consistency جاری |
| Replicated/local projection | خواندن پرترافیک · ترکیب گران · eventual consistency قابل قبول · خواندن محلی بهینه‌شده |

به‌روزرسانی projection از طریق رویداد.

---

## 4. Asynchronous Events

رویدادها واقعیت کسب‌وکار را بیان می‌کنند؛ دستور فنی نیستند.

نمونه‌ها (قفل نیستند):

DestinationPublished · PlacePublished · TourPublished · TourDepartureChanged · PriceChanged · BookingConfirmed · PaymentSucceeded · ReviewPublished

Prefer: `TourPublished`
Avoid: `RefreshSearchTable` · `SendSeoUpdate`

مصرف‌کننده واکنش خودش را انتخاب می‌کند.

---

## 5. Outbox Direction

الگوی برنامه‌ریزی‌شده (پیاده‌سازی در P01):

```text
Module transaction
→ persist domain/application changes
→ persist outgoing event / outbox record
→ commit
→ asynchronous dispatcher
→ consumers
```

هدف: تراکنش commit‌شده و رویداد خروجی به‌خاطر شکست process از هم جدا نشوند.

الان Outbox پیاده نشود.

---

## 6. Platform Downstream Pattern

```text
Business Module
→ domain/application event
→ Outbox
→ Search / SEO / Notification consumers
```

مثال:

```text
TourPublished
├── Search projection updated
├── SEO projection / invalidation updated
└── optional Notification reaction
```

Search موقتاً down باشد → تراکنش انتشار Tour (با invariant خودش) معتبر می‌ماند.

---

## 7. Snapshot Semantics

ماژول‌های تاریخی/تراکنشی ممکن است snapshot حقایق متعلق به دیگران را نگه دارند وقتی تاریخچه نباید mutate شود.

| سؤال | مالک پاسخ |
|------|-----------|
| مشتری در آن لحظه چه چیزی پذیرفت؟ | Snapshot در Booking |
| الان حقیقت چیست؟ | ماژول authoritative جاری (Tour / Pricing / …) |

Snapshot، Booking را SoR جاری Tour/Pricing نمی‌کند.

---

## 8. Presentation / API Composition

صفحه یا endpoint ممکن است چند قرارداد خواندن را ترکیب کند.

```text
Endpoint
→ application/read contracts
→ composed response DTO
```

افشا نشود: EF entities · DbContext · repositories · Infrastructure models.

DTO عمومی برای use case/page ساخته می‌شود. ترکیب صفحه ≠ مالکیت دامنه.

---

## 9. Read Model Ownership

یک سند مشتق‌شده (مثلاً TourSearchDocument با عنوان تور + نام مقصد + نام آژانس + حداقل قیمت نمایشی) می‌تواند متعلق به Search باشد.

مالک projection: Search
مالکان SoR: Tour / Destination / Party / Pricing

---

## 10. Failure Behavior

- قرارداد همزمان: شکست صریح؛ بدون default خاموش ضد-invariant
- مصرف‌کننده ناهمزمان: idempotent؛ شکست پایین‌دست SoR را rollback نمی‌کند
- جزئیات retry/dead-letter → P01

---

## Practical Examples

### Example 1 — Tour → Hotel reference

```text
TourHotelOption.HotelId → Place.Hotel
Tour stores tour-specific nights / meal plan / package config
Tour does NOT duplicate Hotel catalog entity
No EF navigation TourHotelOption.Hotel
```

### Example 2 — Mixed-currency rate

```text
TourRate PriceComponents:
  1290 USD (package)
  119,900,000 local (local charge)
Pricing owns calculation; never silently collapse to one currency
```

### Example 3 — Booking price snapshot

```text
10:00  Rate = 1290 USD + 119,900,000 local → Quote Q-123 accepted
Booking stores accepted pricing snapshot
15:00  Rate becomes 1350 USD + 125,000,000 local
Existing Booking still represents 10:00 agreement
Pricing = current rates/quotes
Booking = accepted historical snapshot
Payment = settlement of accepted obligation
```

### Example 4 — Search projection

```text
Destination authoritative name: استانبول
Search may store normalized copies: استانبول / istanbul / اسطنبول
Search never overwrites Destination authoritative text
Projection rebuildable; SoR independent of index existence
```

### Example 5 — SEO route ownership

```text
Tour owns TourProduct + title translations + publication state
SEO owns SeoRoute + canonical + redirect history + IndexPolicy
Public page composes Tour response + SEO response
```

### Example 6 — Provider Hotel mapping

```text
Place.Hotel: HotelId=H123, name, address, Destination, facilities, media
HotelBooking mapping: HotelId=H123, Provider=ProviderA, ExternalHotelId=998812
Live: rooms, rates, availability, cancellation
No second canonical hotel merely because ProviderA returned one
```

### Example 7 — Tour FlightSegment vs Flight inventory

```text
Tour package FlightSegment: IKA→IST, EK978, local departure/arrival times
Describes package transport
Does NOT mean Tour owns Flight module live airline inventory
Flight later owns live provider search/bookable offers
```

### Example 8 — Destination page multi-module composition

```text
Destination Landing composition:
  Destination module data
+ Tour cards (Tour/Search projection contracts)
+ Place/Hotel cards
+ Content
+ UGC reviews projection
+ SEO information
Destination does not own those other aggregates
```
