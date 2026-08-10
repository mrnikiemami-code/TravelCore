# ADR 0002 — UUID v7 Domain Identity Policy

- **Status:** Proposed
- **Date:** 2026-08-10
- **Task:** TC-P00-T003
- **Related:** [`../data/01-identifiers-and-references.md`](../data/01-identifiers-and-references.md)

---

## Context

ماژول‌های TravelCore باید بتوانند بدون sequence مرکزی هویت تولید کنند، قبل از persistence به هویت برای رویداد/Outbox دسترسی داشته باشند، و ارجاع بین‌ماژولی را بدون افشای surrogate محلی انجام دهند.

UUID v4 کاملاً تصادفی برای ایندکس و locality زمانی ضعیف‌تر است. Integer identity سراسری یا per-table برای مرز ماژول و استخراج آینده نامناسب است.

---

## Decision

1. هویت‌های دامنهٔ قابل‌ارجاع/خارجی از **UUID version 7** استفاده می‌کنند.
2. نوع PostgreSQL: `uuid`.
3. هویت معمولاً توسط Application **قبل از** persistence تولید می‌شود.
4. Domain/Application باید به‌سوی strongly typed IDs برود؛ مقدار زیرین UUID می‌ماند. تکنیک/کتابخانه → P01.
5. ردیفات داخلی Aggregate که عمومی/بین‌ماژولی نیستند ممکن است کلید محلی (مثلاً bigint/composite) داشته باشند؛ افشای عمومی آن‌ها ممنوع است.
6. کدهای استاندارد طبیعی (`USD`, `fa`, `Asia/Tehran`) به UUID اجباری تبدیل نمی‌شوند.
7. Magic sentinel IDs (`-1`, `-2`, `0` به‌عنوان معنا) ممنوع‌اند.
8. External provider IDs هرگز PK داخلی نیستند؛ از mapping `(InternalId, ProviderCode, ExternalId)` استفاده می‌شود.

---

## Alternatives Considered

| گزینه | چرا کنار گذاشته شد |
|-------|---------------------|
| UUID v4 | locality/ایندکس ضعیف‌تر؛ بدون مزیت زمانی |
| `bigint` identity سراسری یا per-table به‌عنوان هویت عمومی | وابستگی به DB برای صدور هویت؛ ارجاع بین‌ماژولی و استخراج سخت‌تر |
| ULID به‌عنوان استاندارد هسته | اکوسیستم .NET/PostgreSQL برای UUID v7 کافی و familiarتر است؛ تغییر بدون سود معماری واضح |
| Provider external ID به‌عنوان PK | تعویض provider هویت داخلی را می‌شکند |

---

## Consequences

### مثبت

- هویت بدون round-trip دیتابیس
- مناسب Outbox/events و مرز ماژول
- یکتایی جهانی
- ایندکس زمانی بهتر از v4

### منفی / هزینه

- ۱۶ بایت به‌جای ۸ بایت integer
- نیاز به تولید صحیح v7 در Application
- strongly typed IDs پیاده‌سازی و نگاشت می‌خواهند (P01)

### Mitigation

- تولید متمرکز/استاندارد هویت در foundation بعدی
- استفاده از کلید محلی فقط برای ردیفات واقعاً داخلی
- نگاشت provider جدا برای سیستم‌های خارجی

---

## Migration / Impact

تولیدکنندهٔ UUID v7، انواع strongly typed، و conventions EF در P01 معرفی می‌شوند. دادهٔ فعلی bootstrap دامنهٔ کسب‌وکار ندارد.
