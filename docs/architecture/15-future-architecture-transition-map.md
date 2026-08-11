# Future Architecture Transition Map

این سند **سنتز بازیابی** است، نه مشخصات معماری جدید و نه ADR جدید.

هدف: خوانندهٔ recovery بفهمد چه چیزی پذیرفته شده، چه چیزی عمداً به فاز بعدی موکول است، و کدام invariantها هنگام پیاده‌سازی نباید بشکند.

| سند | نقش |
|-----|-----|
| [`../ROADMAP.md`](../ROADMAP.md) | چه / کی / ترتیب Phaseها |
| این فایل | Current State → Target State → Trigger Phase → Invariants |
| [`../PROJECT-STATE.md`](../PROJECT-STATE.md) | موقعیت فعلی ریپو |
| [`../adr/`](../adr/) | چرایی تصمیم‌های Accepted |

**قانون:** بازنمایی موقت/bootstrap در کد، معماری دائمی نمی‌سازد. فازهای بعدی باید به Target State پذیرفته‌شده همگرا شوند. ریزکاری پیاده‌سازی ممکن است اصلاح شود، اما ADRهای Accepted را خاموش برنگردانید.

اگر فیلدی با اسناد Accepted پشتیبانی نشود: `Not explicitly specified by accepted architecture`.

---

## A. Platform / Backend Foundation

| فیلد | مقدار |
|------|--------|
| Concern | Host مونولیت ماژولار، persistence، OpenAPI، Outbox، health، logging، CI |
| Current State | فقط bootstrap `TravelCore.Api` (Minimal API · `net10.0` · scaffold بدون OpenAPI به‌خاطر workaround محیطی) |
| Accepted Target State | ساختار ماژول/ثبت ماژول، DbContext و migrations per module، ProblemDetails، validation، OpenAPI intentional، PostgreSQL، Redis abstraction، object storage abstraction، Outbox، Architecture Tests، CI/Docker جهت‌گیری |
| Transition Trigger / Target Phase | **P01** |
| Affected Modules / Consumers | همهٔ ماژول‌های آینده؛ Public/Admin/Agency surfaces |
| Invariant That Must Survive Transition | Modular Monolith · schema-per-module · no global TravelCoreDbContext · no cross-module DbContext/EF navigation · OpenAPI حذف‌شدهٔ bootstrap تصمیم معماری نیست |
| Source References | ROADMAP P01 · `02-technology-baseline.md` · ADR 0001 · `07/08` data+persistence · Constitution |

---

## B. Frontend Foundation

| فیلد | مقدار |
|------|--------|
| Concern | Design system · Server Component first · RTL/LTR/bidi · mobile · shell |
| Current State | Next.js bootstrap App Router / Tailwind فقط |
| Accepted Target State | Tokens/primitives، locale-aware root، `lang`/`dir`، responsive، MixedCurrencyPrice، Public/Admin shell، Walking Skeleton با Foreign Package Tour Detail |
| Transition Trigger / Target Phase | **P02** |
| Affected Modules / Consumers | همهٔ صفحات عمومی بعدی |
| Invariant That Must Survive Transition | Server Component first · Client = interactive islands · direction-neutral UI · Locale ≠ Currency ≠ Calendar ≠ TimeZone |
| Source References | ROADMAP P02 · ADR 0005 · ADR 0006 · `10-ui-constitution.md` · `ui/**` |

---

## C. Identity / Access / Party

| فیلد | مقدار |
|------|--------|
| Concern | احراز هویت، مجوز، هویت کسب‌وکار |
| Current State | فقط مرز مفهومی مستند |
| Accepted Target State | Identity / Access / Party پیاده‌سازی‌شده طبق مالکیت جدا |
| Transition Trigger / Target Phase | **P03** |
| Affected Modules / Consumers | Admin · Agency · Booking · UGC subjects |
| Invariant That Must Survive Transition | **Identity ≠ Party ≠ Access** |
| Source References | `03/04` domain map/boundaries · ROADMAP P03 |

---

## D. ReferenceData / Destination

| فیلد | مقدار |
|------|--------|
| Concern | مرجع پایدار + سلسله‌مراتب کشف مقصد |
| Current State | معماری مالکیت فقط |
| Accepted Target State | ReferenceData + Destination hierarchy با ترجمه/slug hooks عمومی |
| Transition Trigger / Target Phase | **P04** |
| Affected Modules / Consumers | Place · Tour · Content · SEO · Search |
| Invariant That Must Survive Transition | Destination مالک Hotel/Tour/Article/Booking نیست؛ ترکیب صفحه ≠ مالکیت |
| Source References | ROADMAP P04 · module boundaries Destination/ReferenceData |

---

## E. SEO

| فیلد | مقدار |
|------|--------|
| Concern | SeoRoute · canonical · redirect · indexation |
| Current State | قانون اساسی SEO / ADR فقط |
| Accepted Target State | زیرساخت واقعی route/indexation/canonical/sitemap |
| Transition Trigger / Target Phase | **P05** |
| Affected Modules / Consumers | Destination و سایر موجودیت‌های عمومی |
| Invariant That Must Survive Transition | SEO مالک مکانیک route است، نه محتوای کسب‌وکار؛ Public ≠ Indexable؛ Search URL ≠ SEO Landing |
| Source References | ROADMAP P05 · ADR 0009 · ADR 0010 · `12-seo-constitution.md` |

---

## F. Media

| فیلد | مقدار |
|------|--------|
| Concern | MediaAsset + object storage |
| Current State | جهت معماری |
| Accepted Target State | upload/validation/variants/alt طبق Media ownership |
| Transition Trigger / Target Phase | **P06** |
| Affected Modules / Consumers | Destination · Place · Tour · Content · UGC |
| Invariant That Must Survive Transition | باینری در S3-compatible storage؛ معنای رابطهٔ رسانه متعلق به ماژول مصرف‌کننده است |
| Source References | ROADMAP P06 · Media boundaries · technology baseline |

---

## G. Place

| فیلد | مقدار |
|------|--------|
| Concern | کاتالوگ Hotel/Restaurant/Attraction |
| Current State | مرز مفهومی |
| Accepted Target State | Place catalog عمومی + Admin |
| Transition Trigger / Target Phase | **P07** |
| Affected Modules / Consumers | Tour · HotelBooking · SEO · Search · Content |
| Invariant That Must Survive Transition | **Place.Hotel = canonical hotel catalog** · Hotel Catalog ≠ HotelBooking |
| Source References | ROADMAP P07 · Place/HotelBooking boundaries |

---

## H. Content

| فیلد | مقدار |
|------|--------|
| Concern | محتوای editorial |
| Current State | مرز مفهومی |
| Accepted Target State | Article/Guide/Blocks با پیوند Destination |
| Transition Trigger / Target Phase | **P08** |
| Affected Modules / Consumers | SEO · Destination hubs · Public pages |
| Invariant That Must Survive Transition | Content مالک editorial است؛ SEO محتوا را duplicate نمی‌کند |
| Source References | ROADMAP P08 · Content/SEO ownership |

---

## I. Tour Core

| فیلد | مقدار |
|------|--------|
| Concern | مبانی مشترک TourProduct |
| Current State | مرز مفهومی |
| Accepted Target State | TourProduct مشترک + publishing/SEO hooks |
| Transition Trigger / Target Phase | **P09** |
| Affected Modules / Consumers | Pricing · Booking · Public Tour · Search · SEO |
| Invariant That Must Survive Transition | **TourProduct ≠ TourDeparture** · Experience و Package یک blob nullable اجباری نشوند |
| Source References | ROADMAP P09 · Tour boundaries · glossary |

---

## J. Experience Tour

| فیلد | مقدار |
|------|--------|
| Concern | itinerary ساخت‌یافته |
| Current State | archetype + دامنه |
| Accepted Target State | ItineraryDay/Stop/meals/equipment/difficulty |
| Transition Trigger / Target Phase | **P10** |
| Affected Modules / Consumers | Public Experience Tour Detail |
| Invariant That Must Survive Transition | itinerary ساخت‌یافته؛ Destination/Attraction مالکیت جدا می‌مانند |
| Source References | ROADMAP P10 · pages Experience Tour · Tour boundaries |

---

## K. Foreign Package Tour

| فیلد | مقدار |
|------|--------|
| Concern | Departure · FlightSegment · TourHotelOption |
| Current State | archetype + دامنه |
| Accepted Target State | Package Tour capability کامل طبق مرزها |
| Transition Trigger / Target Phase | **P11** |
| Affected Modules / Consumers | Pricing · Public Foreign Tour Detail · P02 skeleton |
| Invariant That Must Survive Transition | **Tour FlightSegment ≠ Flight live inventory** · **TourHotelOption ≠ Hotel ownership** |
| Source References | ROADMAP P11 · Foreign Tour page · Tour/Flight/Place boundaries |

---

## L. Pricing

| فیلد | مقدار |
|------|--------|
| Concern | نرخ جاری · Quote · mixed currency |
| Current State | پیاده‌سازی Pricing شروع نشده |
| Accepted Target State | Pricing authoritative برای current commercial pricing / Quote semantics |
| Transition Trigger / Target Phase | **P12** |
| Affected Modules / Consumers | Public Tour · Booking · Agency commerce where applicable |
| Invariant That Must Survive Transition | **Price ≠ Quote ≠ Booking ≠ Payment** · Money = Amount + CurrencyCode · mixed PriceComponents جدا · IRR کاننیکال / Toman display · snapshot تاریخی Booking با تغییر نرخ زنده بازنویسی نشود |
| Source References | ROADMAP P12 · ADR 0003 · Pricing/Booking boundaries · cross-module communication examples |

### مثال بازیابی (Pricing)

```text
Current: Pricing not implemented
Target: Pricing owns current rates + Quote
Phase: P12
Later consumers: Tour UX, Booking, Agency (as approved)
Invariant: accepted Booking commercial snapshot survives later Pricing changes
```

---

## M. Agency Marketplace

| فیلد | مقدار |
|------|--------|
| Concern | پروفایل/پیشنهاد آژانس |
| Current State | مرز مفهومی Party/Agency presentation |
| Accepted Target State | Agency marketplace طبق ROADMAP P13 |
| Transition Trigger / Target Phase | **P13** |
| Affected Modules / Consumers | Tour offering · Agency Panel |
| Invariant That Must Survive Transition | Agency Panel منطق Tour/Pricing را کپی/مالک نمی‌شود؛ قواعد فروشندهٔ جزئیِ پذیرفته‌نشده اختراع نشود |
| Source References | ROADMAP P13 · Party/Admin presentation rules |

---

## N. Public Tour Experience

| فیلد | مقدار |
|------|--------|
| Concern | UX عمومی Tour listing/detail |
| Current State | archetypeها مستند |
| Accepted Target State | صفحات production با RTL/LTR/mobile/SEO |
| Transition Trigger / Target Phase | **P14** |
| Affected Modules / Consumers | Tour · Pricing · Place · SEO · Search projections |
| Invariant That Must Survive Transition | Page composition ≠ module ownership · Search URL ≠ SEO Landing |
| Source References | ROADMAP P14 · pages/** · SEO constitution |

---

## O. Search

| فیلد | مقدار |
|------|--------|
| Concern | index/projection مشتق |
| Current State | جهت PostgreSQL FTS + pg_trgm |
| Accepted Target State | Search platform مشتق با abstraction |
| Transition Trigger / Target Phase | **P15** |
| Affected Modules / Consumers | Public discovery |
| Invariant That Must Survive Transition | **Search = derived** · **Search ≠ SEO** · SoR را overwrite نمی‌کند |
| Source References | ROADMAP P15 · Search boundaries · SEO/Search consistency |

---

## P. UGC

| فیلد | مقدار |
|------|--------|
| Concern | Review / Travelogue / moderation |
| Current State | مرز مفهومی |
| Accepted Target State | UGC lifecycle طبق مالکیت |
| Transition Trigger / Target Phase | **P16** |
| Affected Modules / Consumers | Place/Destination/Tour composition |
| Invariant That Must Survive Transition | هدف review مالک Aggregate UGC نیست؛ مرز زبان/محتوا حفظ شود |
| Source References | ROADMAP P16 · UGC boundaries |

---

## Q. Visa

| فیلد | مقدار |
|------|--------|
| Concern | کاتالوگ/گردش ویزا |
| Current State | مرز سطح‌بالا |
| Accepted Target State | Visa module per ROADMAP P17 |
| Transition Trigger / Target Phase | **P17** |
| Affected Modules / Consumers | Destination · Public Visa pages |
| Invariant That Must Survive Transition | جزئیات دامنهٔ پذیرفته‌نشده اختراع نشود؛ Tour مالک Visa product نمی‌شود |
| Source References | ROADMAP P17 · Visa boundaries |

---

## R. Booking

| فیلد | مقدار |
|------|--------|
| Concern | رزرو و snapshot تجاری پذیرفته‌شده |
| Current State | مرز مفهومی |
| Accepted Target State | Booking با Quote snapshot تاریخی |
| Transition Trigger / Target Phase | **P19** |
| Affected Modules / Consumers | Payment · Notification · Public checkout |
| Invariant That Must Survive Transition | Booking حقایق تجاری پذیرفته‌شده را نگه می‌دارد؛ تغییر Pricing زنده تاریخچه را بازنویسی نمی‌کند |
| Source References | ROADMAP P19 · Booking/Pricing communication examples |

---

## S. Payment

| فیلد | مقدار |
|------|--------|
| Concern | تسویه مالی |
| Current State | مرز مفهومی |
| Accepted Target State | Payment lifecycle + provider abstraction |
| Transition Trigger / Target Phase | **P20** |
| Affected Modules / Consumers | Booking (via events/contracts) |
| Invariant That Must Survive Transition | **Payment ≠ Booking ≠ Quote ≠ Price** · Payment DbContext Booking را مستقیم mutate نمی‌کند |
| Source References | ROADMAP P20 · Payment boundaries |

---

## T. HotelBooking

| فیلد | مقدار |
|------|--------|
| Concern | موجودی زندهٔ provider هتل |
| Current State | مرز مفهومی |
| Accepted Target State | search/availability/reservation provider |
| Transition Trigger / Target Phase | **P21** |
| Affected Modules / Consumers | Place.Hotel mapping · Public hotel booking UX |
| Invariant That Must Survive Transition | **Hotel Catalog ≠ HotelBooking** · قطع provider هویت کاتالوگ را پاک نمی‌کند · ExternalId ≠ PK داخلی |
| Source References | ROADMAP P21 · HotelBooking/Place examples |

---

## U. Flight

| فیلد | مقدار |
|------|--------|
| Concern | جستجو/رزرو زندهٔ پرواز |
| Current State | مرز مفهومی |
| Accepted Target State | Flight provider commerce |
| Transition Trigger / Target Phase | **P22** |
| Affected Modules / Consumers | Public flight UX · later dynamic packages |
| Invariant That Must Survive Transition | **Tour package FlightSegment ≠ live Flight inventory** |
| Source References | ROADMAP P22 · Tour/Flight distinction |

---

## V. Notification

| فیلد | مقدار |
|------|--------|
| Concern | ارسال اعلان پایین‌دستی |
| Current State | مرز مفهومی |
| Accepted Target State | Notification channels/provider abstraction |
| Transition Trigger / Target Phase | **P25** |
| Affected Modules / Consumers | Booking/Payment/Visa events |
| Invariant That Must Survive Transition | Notification پایین‌دست است؛ صحت هستهٔ دامنه به‌صورت همزمان به Notification وابسته نیست |
| Source References | ROADMAP P25 · Platform downstream rules |

---

## W. Advanced SEO / Content Graph

| فیلد | مقدار |
|------|--------|
| Concern | hubs · programmatic SEO کنترل‌شده · link graph |
| Current State | قانون اساسی SEO |
| Accepted Target State | Advanced SEO پس از موجودی/محتوای واقعی |
| Transition Trigger / Target Phase | **P26** |
| Affected Modules / Consumers | Destination · Content · Tour · Place |
| Invariant That Must Survive Transition | مالکیت SEO/Content را معکوس نکند؛ thin programmatic URL ممنوع |
| Source References | ROADMAP P26 · ADR 0010 · SEO constitution |

---

## X. Production Hardening

| فیلد | مقدار |
|------|--------|
| Concern | امنیت · observability · DR · CI/CD عملیاتی |
| Current State | جهت Quality Constitution |
| Accepted Target State | hardening production per ROADMAP P29 |
| Transition Trigger / Target Phase | **P29** |
| Affected Modules / Consumers | کل پلتفرم |
| Invariant That Must Survive Transition | Build PASS ≠ Task PASS · evidence-based gates · tooling دقیق هنوز deferred مگر Accepted صریح |
| Source References | ROADMAP P29 · ADR 0011 · ADR 0012 · `quality/**` |

---

## Areas not fully specified

برای برخی جزئیات (کلاس‌ها، جداول دقیق، نام رویداد، پکیج UI، قرارداد B2B فروشنده، …):

`Not explicitly specified by accepted architecture`

این موارد در Transition Map تصمیم‌گیری نمی‌شوند.
