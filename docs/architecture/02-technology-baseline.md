# پایهٔ فناوری TravelCore (Technology Baseline)

این سند **پایهٔ معماری فناوری** و **نسخه‌های فعلی تأییدشده** را جدا می‌کند. جزئیات اصول در [`00-constitution.md`](00-constitution.md).

---

## Architecture Baseline (قفل مفهومی)

| لایه | انتخاب |
|------|--------|
| شکل سیستم | Modular Monolith — یک Backend deployable |
| Backend runtime | .NET 10 / ASP.NET Core 10 |
| API style | Minimal API (نه Controllers به‌عنوان معماری پیش‌فرض) |
| Backend structure | Vertical Slice + Clean Domain Boundaries |
| Frontend | Next.js App Router · TypeScript · Tailwind CSS |
| Rendering | Server Component first |
| Persistence اصلی | EF Core برای تراکنش، دامنه، migration، aggregate write |
| خواندن بهینه‌شده | Dapper فقط وقتی برای projection/read-heavy/reporting توجیه دارد |
| RDBMS | PostgreSQL (system of record) |
| Cache / کمکی | Redis (نه system of record) |
| Media binaries | S3-compatible Object Storage |
| Search اولیه (برنامه‌ریزی) | PostgreSQL Full Text Search + `pg_trgm` پشت abstraction |
| Locales اولیه | `fa` · `en` · `ar` |

### صراحت‌ها

- **Dapper everywhere** نیست.
- **Generic Repository اجباری** به‌عنوان الگوی معماری نیست.
- Controllers معماری هدف نیستند؛ Minimal API قفل است.
- OpenAPI بخشی از پلتفرم API intentional است (نگاه کنید به یادداشت bootstrap پایین).

---

## ساختار Monorepo فعلی

```text
TravelCore/
├── TravelCore.sln
├── AGENTS.md
├── docs/
├── src/
│   ├── backend/
│   │   ├── TravelCore.Api/     # Host
│   │   ├── Modules/            # Future module-owned code
│   │   └── Platform/           # Narrow technical foundations (later tasks)
│   └── frontend/
│       └── web/
```

- Backend host: `src/backend/TravelCore.Api`
- Backend physical convention: [`18-backend-physical-structure.md`](18-backend-physical-structure.md)
- Frontend: `src/frontend/web`
- Solution: `TravelCore.sln`
- TargetFramework پروژهٔ API: `net10.0`

---

## Current Verified Versions (لحظه‌ای)

این اعداد **قید ابدی معماری نیستند**؛ وضعیت تأییدشده در زمان ثبت baselineاند و با ارتقای آگاهانهٔ ابزار ممکن است عوض شوند.

| ابزار / بسته | نسخهٔ تأییدشده |
|--------------|----------------|
| .NET SDK | 10.0.103 |
| Target Framework | net10.0 |
| ASP.NET Core runtime | 10.0.3 |
| Next.js | 16.3.0 |
| React | 19.2.8 |
| TypeScript | 5.9.3 |
| Node.js | 24.19.0 |
| npm | 11.17.0 |

**Architecture baseline** = .NET 10 / ASP.NET Core 10 / Next.js 16
**Verified install** = جدول بالا

---

## Persistence و داده

| فناوری | نقش |
|--------|-----|
| EF Core | نوشتن تراکنشی دامنه، migration، aggregate |
| Dapper | projection / read-heavy / reporting وقتی ارزش دارد |
| PostgreSQL | SoR رابطه‌ای |
| Redis | کش و کمک؛ نه منبع حقیقت |
| Object Storage | باینری رسانه؛ نه payload تصویر داخل جدول دامنه مگر ADR خلاف آن |

---

## Frontend Baseline

- App Router
- TypeScript
- Tailwind CSS
- ESLint (طبق scaffold رسمی)
- اصل: حداقل JavaScript غیرضروری، SEO-first، Mobile-first

Client Components فقط برای تعامل مرورگر (فیلتر، date picker، carousel controls، maps، dialogs، booking interaction، فرم پویا و …).

---

## یادداشت محیطی Bootstrap (غیرمعماری)

در طی `TC-P00-T000A`، دسترسی به NuGet.org موقتاً قطع بود.

در نتیجه `TravelCore.Api` با قالب رسمی و فلگ `--no-openapi` scaffold شد تا restore/build روی shared framework نصب‌شده ممکن شود.

این یک **ENVIRONMENTAL BOOTSTRAP WORKAROUND** است.

**تصمیم معماری برای حذف دائمی OpenAPI نیست.**

OpenAPI بخشی از پلتفرم intentional API باقی می‌ماند و در Task صریح Foundation بعدی، وقتی دسترسی بسته فراهم باشد، اضافه/پیکربندی می‌شود. در این فاز کد را عوض نکنید.

---

## آنچه هنوز intentional است ولی پیاده نشده

بدون اجرای این Task:

- ماژول‌های دامنه و class libraryها
- PostgreSQL / Redis / Object Storage واقعی
- Outbox / Events
- Authentication/Authorization کامل
- Localization / RTL پیاده‌سازی‌شده
- SEO runtime
- Docker Compose
- OpenTelemetry و پکیج‌های observability

این‌ها با Task/ADR بعدی وارد می‌شوند.
