# Dependency Rules — قوانین وابستگی ماژول‌ها

مرز مالکیت: [`04-module-boundaries.md`](04-module-boundaries.md)
ارتباط: [`06-cross-module-communication.md`](06-cross-module-communication.md)

---

## طبقه‌بندی وابستگی

| کد | نوع | معنی |
|----|-----|------|
| A | None | بدون وابستگی |
| B | Reference Dependency | فقط ModuleId خارجی |
| C | Synchronous Contract | فراخوانی قرارداد Application عمومی |
| D | Event Consumer | مصرف رویداد کسب‌وکار |
| E | Projection Consumer | نگهداری read/derived model محلی |

از عبارات مبهم مثل «Tour به همه‌چیز وابسته است» پرهیز شود.

---

## قوانین مطلق ممنوع

1. **دسترسی مستقیم به DbContext ماژول دیگر** — ممنوع
2. **EF navigation بین‌ماژولی** — ممنوع
3. **Aggregate عبورکننده از مرز ماژول** — ممنوع
4. **تراکنش اشتراکی روتین روی چند ماژول DbContext** — ممنوع (مگر بازبینی معمار / احتمالاً ADR)
5. **چرخهٔ وابستگی سخت دامنه/Application** — ممنوع
6. **وابستگی هستهٔ دامنه به Search / SEO / Notification** — ممنوع
7. **Service Locator در کد کسب‌وکار** (`IServiceProvider.GetService`) — ممنوع
8. **IRepository\<T\> غول‌پیکر مشترک بین ماژول‌ها** — ممنوع؛ abstraction در صورت نیاز use-case/module-specific است
9. **افشای EF Entity / DbContext / repository داخلی از API عمومی** — ممنوع
10. **Provider external ID به‌عنوان PK داخلی TravelCore** — ممنوع

---

## Cross-module DbContext و EF

### Forbidden

```csharp
// Forbidden
TourApplicationService → PlaceDbContext
BookingHandler → PricingDbContext
SeoService → TourDbContext
SearchIndexer → arbitrary module DbContexts

class TourHotelOption
{
    public Hotel Hotel { get; set; } // Hotel belongs to Place
}
```

### Preferred

```csharp
class TourHotelOption
{
    public Guid HotelId { get; set; } // ID reference; exact ID type later in Data Architecture
}
```

اصل: ارجاع شناسه، نه navigation Entity بین‌ماژولی.

استراتژی فیزیکی FK دیتابیس متعلق به **TC-P00-T003 Data Architecture** است — نه اجباری فرض شود، نه برای همیشه ممنوع اعلام شود؛ مالکیت دامنه به ORM navigation وابسته نیست.

---

## Aggregate و Transaction

- Aggregate هرگز از مالکیت یک ماژول خارج نمی‌شود.
- تراکنش کسب‌وکار پیش‌فرض متعلق به **یک** ماژول است.
- گردش بین‌ماژولی ترجیح: validate با قرارداد عمومی → commit مالک → publish رویداد معنایی → واکنش پایین‌دست.

### Forbidden conceptual aggregate

```text
Booking
  → TourProduct EF entity
  → Hotel EF entity
  → Payment entity
```

Booking می‌تواند داشته باشد: TourProductId · TourDepartureId · QuoteSnapshot · traveler snapshot — بدون فرزند کردن Aggregateهای خارجی.

### Forbidden shared transaction example

تأیید Booking نباید هم‌زمان این‌ها را در یک تراکنش اختیاری باز کند:

BookingDbContext + PaymentDbContext + NotificationDbContext

اگر موردی واقعاً به mutation اتمی چندمرزی نیاز داشت → بازبینی معمار / ADR.

---

## جهت وابستگی مفهومی فعلی

### Foundation

| Module | May depend on |
|--------|----------------|
| Identity | Party association reference where required |
| Access | Identity, Party |
| Party | ReferenceData |
| ReferenceData | none (business modules) |

### Discovery

| Module | May depend on |
|--------|----------------|
| Destination | ReferenceData, Media |
| Place | Destination, Media, ReferenceData |
| Media | no business domain module |

### Knowledge

| Module | May depend on |
|--------|----------------|
| Content | Media; Destination; optionally Place/Tour/Visa by ID/contracts |
| UGC | Identity/Party subject IDs; Media; target module IDs/contracts |

ماژول‌های هدف برای تراکنش هسته‌شان به UGC وابسته نمی‌شوند.

### Commerce

| Module | May depend on |
|--------|----------------|
| Tour | Destination, Place, Party, Media, ReferenceData |
| Pricing | Tour contracts/references, ReferenceData |
| Visa | Destination, ReferenceData, Media |
| Booking | Party, Tour contracts/references, Pricing Quote contracts/snapshots |
| Payment | Booking reference/contracts, ReferenceData where needed |

### External

| Module | May depend on |
|--------|----------------|
| HotelBooking | Place, ReferenceData, provider abstractions |
| Flight | ReferenceData, provider abstractions |

HotelBooking/Flight را تا طراحی صریح به Pricing وابسته نکنید.

### Platform

| Module | Direction |
|--------|-----------|
| Search | ← consumes publishable data/events |
| SEO | ← consumes publishable data/events/contracts |
| Notification | ← consumes semantic events |

Core modules → Platform فقط از طریق رویداد/قرارداد publishable؛ وابستگی معکوس ممنوع.

---

## بدون چرخهٔ سخت

Forbidden cycle example:

```text
Tour → Place
AND Place → Tour   (as domain/application hard dependency)
```

نیاز معکوس Presentation (مثلاً «تورهایی که این هتل را دارند» در Hotel Detail) از طریق composition / projection / query روی Tour by HotelId حل می‌شود — نه وابستگی دامنه Place به Tour.

---

## قرارداد عمومی ماژول (مفهومی)

ساختار مفهومی آینده (پروژه/پوشه الان ساخته نشود):

```text
Module
├── Domain
├── Application
├── Infrastructure
└── Contracts   // intentional public module-facing abstractions
```

Contracts ≠ ریختن همهٔ DTOها در Shared سراسری.
انواع داخلی ماژول internal می‌مانند.
ساختار فیزیکی پروژه متعلق به P01 است.

---

## SharedKernel Policy

SharedKernel الان ایجاد نمی‌شود.

کد فقط به‌خاطر شباهت فعلی دو ماژول shared نمی‌شود. اشتراک نیاز به معنای معنایی واقعاً همگانی دارد.

نامزدهای بالقوهٔ بعدی (نیاز به تصمیم صریح): Result/Error · correlation · base event · شناسه‌های به‌شدت حاکم‌شده.

این‌ها را خودکار در SharedKernel نریزید بدون مالکیت صریح:

Money · Date rules · Entity base classes · localization models

Shared نباید dumping ground وابستگی شود.

### Money nuance

- مرجع Currency ممکن است ReferenceData باشد
- قواعد Pricing و ExchangeRate/Quote متعلق به Pricing است
- نمایش/value Money ممکن است بعداً در چند ماژول لازم شود (Pricing، Booking snapshot، Payment، fares خارجی)
- محل فیزیکی primitive مشترک → Data Architecture / ADR
- invariant: decimal Amount + Currency صریح

### Time nuance

- timezone IDs/reference ممکن است از ReferenceData/سیستم بیاید
- معنای دامنه متعلق به ماژول مالک است (departure time → Tour؛ creation Instant → Booking؛ attempt Instant → Payment)
- ماژول دامنهٔ «DateService» ساخته نشود
- انتخاب کتابخانه/VO → Data Architecture

---

## Provider ID Policy

```text
InternalEntityId + Provider + ExternalId
```

Provider switching هویت TravelCore را بازنویسی نمی‌کند. مدل persistence بعداً.

---

## Failure Policy (وابستگی)

Synchronous contracts: شکست صریح (وجود ندارد، فعال نیست، Quote منقضی، provider unavailable). مقدار پیش‌فرض خاموش که invariant را دور بزند ممنوع.

Asynchronous consumers: باید در نهایت idempotent باشند. شکست Search/Notification تراکنش SoR commit‌شده را rollback نمی‌کند. جزئیات retry/dead-letter → P01.

---

## Deletion / Reference Principle

حذف سخت Entityهایی که در تاریخچهٔ دیگران ارجاع شده‌اند، بدون توجه به تاریخچه/ارجاع، پیش‌فرض نیست. ترجیح lifecycle/archive/deactivate جایی که تاریخچه لازم است. سیاست دقیق هر دامنه در فاز خودش.

---

## Architecture Test Expectations (آینده — الان تست نسازید)

حداقل باید در آینده enforce شوند:

- Domain assemblies به EF Core وابسته نباشند
- Domain یک ماژول به Infrastructure ماژول دیگر وابسته نباشد
- Application یک ماژول به DbContext ماژول دیگر دسترسی نداشته باشد
- EF navigation بین‌ماژولی ممنوع باشد
- Search/SEO/Notification وابستگی Domain هسته نباشند
- مرز قرارداد عمومی صریح بماند
- API مستقیماً EF Entity افشا نکند
