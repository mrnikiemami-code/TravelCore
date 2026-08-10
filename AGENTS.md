# TravelCore — AGENTS.md

این فایل **قرارداد عملیاتی سطح‌بالا** برای هر coding agent (Cursor، Hermes و سایر عوامل) است.

قبل از هر پیاده‌سازی، این سند را بخوانید. جزئیات معماری در [`docs/architecture/`](docs/architecture/) و واژگان دامنه در [`docs/domain/glossary.md`](docs/domain/glossary.md) است.

---

## نقش Agent

Agent یک **پیاده‌ساز** است، نه Software Architect.

- تصمیم‌های معماری قفل‌شده را بازطراحی نکنید.
- فقط در محدودهٔ Task مشخص‌شده کار کنید.
- اگر بهبود معماری محتمل به‌نظر می‌رسد، آن را **پیاده نکنید**؛ در گزارش نهایی تحت عنوان **Architectural Concern** ثبت کنید و منتظر تصمیم معمار / ADR بمانید.

جزئیات گردش‌کار: [`docs/architecture/09-ai-development-workflow.md`](docs/architecture/09-ai-development-workflow.md)

---

## سطوح تصمیم

| سطح | مثال | آزادی Agent |
|-----|------|-------------|
| **Level 1 — Architecture Decision** | Modular Monolith | ممنوع بدون ADR |
| **Level 2 — Domain Decision** | TourProduct ≠ TourDeparture | ممنوع بدون مستند/ADR |
| **Level 3 — Feature Design** | Foreign Tour Detail نمایش HotelOptions | فقط طبق مشخصات Task |
| **Level 4 — Implementation Detail** | نام متد داخلی / سازماندهی محلی کوچک | آزادی معقول |

Agent عمدتاً در Level 4 اختیار دارد. Levels 1–3 باید از مستندات تأییدشده و Prompt پیروی کنند.

---

# NEVER

- معرفی microservices به‌جای Modular Monolith
- بازطراحی معماری قفل‌شده بدون ADR
- دسترسی مستقیم به DbContext ماژول دیگر
- ایجاد navigation propertyهای EF بین ماژول‌ها (مثلاً `TourHotelOption.Hotel` وقتی `Hotel` متعلق به Place است)
- وابسته‌کردن Domain به EF Core / ASP.NET Core / Dapper / Redis / HTTP
- افشای مستقیم EF Entity از طریق Public API
- ذخیرهٔ پول با `float` یا `double`
- الگوی schemaی `NameFa` / `NameEn` / `NameAr`
- فرض اینکه هر نرخ فقط یک ارز دارد
- تبدیل خاموش قیمت‌های چندارزی به یک ارز واحد
- یکی‌دانستن TourProduct و TourDeparture
- یکی‌دانستن Hotel Catalog و Hotel Booking
- پیاده‌سازی RTL فقط با `body { direction: rtl; }` و پراکندن `left`/`right`
- تبدیل کل درخت صفحهٔ Next.js به Client Component برای راحتی
- ایندکس‌پذیر کردن ترکیب‌های دلخواه فیلتر جستجو
- تغییر خاموش Canonical URL یا قواعد slug
- کپی منطق کسب‌وکار داخل Admin UI
- تغییر ماژول‌های نامرتبط در یک Task
- افزودن پکیج بزرگ بدون صراحت در Scope Task
- پیاده‌سازی قابلیت‌های آینده «تا اینجام»
- دستورات مخرب Git بدون دستور صریح (`reset --hard`، `push --force` و مشابه)
- commit کردن secrets
- نادیده گرفتن شکست build/test

---

# ALWAYS

- ابتدا `AGENTS.md` را بخوانید
- مستندات ارجاع‌شده در Task را بخوانید
- مالکیت ماژول را رعایت کنید
- محدودهٔ Task را رعایت کنید
- تمایزهای معنایی دامنه را حفظ کنید (TourProduct/Departure، Price/Quote/Payment، Locale/Currency/Calendar/Timezone و …)
- در کار مرتبط، localization را در نظر بگیرید
- در کار UI عمومی، RTL/LTR را در نظر بگیرید
- برای مقادیر mixed-direction، bidi-safe بودن را در نظر بگیرید
- برای UI عمومی، رفتار موبایل را در نظر بگیرید
- برای صفحات عمومی، تأثیر SEO را در نظر بگیرید
- accessibility را حفظ کنید
- build/testهای مرتبط را اجرا کنید
- در صورت مفید بودن، کامنت فارسی معنادار برای **چرا (WHY)** بنویسید — نه توضیح بدیهی WHAT
- نگرانی‌های معماری را گزارش کنید، نه اینکه خاموش بازطراحی کنید
- انحراف‌ها (Deviations) را گزارش کنید
- گزارش نهایی ساختاریافتهٔ Task را ارائه دهید
- working tree را متمرکز و قابل‌فهم نگه دارید

---

## کامنت فارسی

کامنت خوب دلیل کسب‌وکاری/معماری را توضیح می‌دهد:

```csharp
// نرخ پایه عمداً به یک ارز واحد تبدیل نمی‌شود.
// بعضی پکیج‌های خارجی هم‌زمان چند مؤلفه‌ی ارزی دارند.
// تبدیل ارز هنگام Quote و طبق سیاست Pricing انجام می‌شود.
```

شناسه‌های C#/TypeScript انگلیسی بمانند. UTF-8 حفظ شود.

---

## قانون One Task → One Writer

دو coding agent نباید هم‌زمان بدون هماهنگی صریح روی یک Feature واحد بنویسند.

نقش‌های پیشنهادی:

- **Architect** — تحلیل / مشخصات / بازبینی
- **Cursor** — پیاده‌سازی اصلی
- **Hermes** — بازبینی مستقل / audit
- **Automated Tests** — دروازهٔ عینی کیفیت

---

## ردپای Task

فرمت شناسه: `TC-P03-T005` (TravelCore / Phase / Task)

Promptها در `docs/prompts/` ذخیره می‌شوند. در commitهای پیاده‌سازی، درج Task ID ترجیح داده می‌شود:

```text
feat(destination): add localized public detail [TC-P05-T004]
```

---

## پیوندهای حیاتی

| سند | نقش |
|-----|-----|
| [`docs/architecture/00-constitution.md`](docs/architecture/00-constitution.md) | اصول قفل‌شده معماری |
| [`docs/architecture/01-product-vision.md`](docs/architecture/01-product-vision.md) | چشم‌انداز محصول |
| [`docs/architecture/02-technology-baseline.md`](docs/architecture/02-technology-baseline.md) | پایهٔ فناوری |
| [`docs/architecture/09-ai-development-workflow.md`](docs/architecture/09-ai-development-workflow.md) | گردش‌کار توسعه با AI |
| [`docs/domain/glossary.md`](docs/domain/glossary.md) | واژه‌نامه دامنه |
| [`docs/adr/README.md`](docs/adr/README.md) | فرایند ADR |
| [`docs/prompts/README.md`](docs/prompts/README.md) | قالب Prompt |
