# فرایند ADR در TravelCore

ADR = Architecture Decision Record

این سند فقط **فرایند** را تعریف می‌کند. در `TC-P00-T001` هیچ ADR محتوایی اختراع یا ثبت نمی‌شود؛ تصمیم‌های واقعی در Taskهای صریح بعدی ثبت خواهند شد.

---

## چرا ADR؟

تصمیم‌های Level 1–2 (و برخی Level 3های بنیادین) باید قابل ردیابی، قابل بازبینی و قابل جایگزینی کنترل‌شده باشند تا Agentها و انسان‌ها ماه‌ها بعد بدانند **چه چیزی قفل است و چرا**.

قرارداد Agent: [`../../AGENTS.md`](../../AGENTS.md)
قانون اساسی: [`../architecture/00-constitution.md`](../architecture/00-constitution.md)

---

## وضعیت‌ها (Statuses)

| Status | معنی |
|--------|------|
| Proposed | پیشنهاد شده؛ هنوز Accepted نیست |
| Accepted | تصمیم فعال و لازم‌الاجرا |
| Superseded | با ADR جدیدتر جایگزین شده |
| Rejected | بررسی و رد شده |

---

## محتوای معمول هر ADR

هر ADR معمولاً شامل:

1. **Context** — زمینه و فشار مسئله
2. **Decision** — تصمیم اتخاذشده
3. **Alternatives considered** — گزینه‌های ردشده یا کنارگذاشته
4. **Consequences** — پیامدهای مثبت/منفی
5. **Migration / impact** — اگر مهاجرت یا اثر بین‌ماژولی دارد
6. **Status** — یکی از وضعیت‌های بالا
7. **Date** — تاریخ

---

## چه زمانی ADR لازم است؟

نمونه‌ها:

- حرکت به microservices
- تغییر مالکیت ماژول
- جایگزینی PostgreSQL
- تغییر فلسفهٔ ذخیره‌سازی localization
- تغییر استراتژی URL/slug
- تغییر معنای Pricing / mixed-currency
- تغییر مدل ارتباط بین‌ماژولی عمده
- تغییر معماری امنیت
- پذیرش framework/کتابخانهٔ بزرگ که معماری را عوض کند

Agent بدون ADR این‌ها را پیاده نمی‌کند؛ حداکثر **Architectural Concern** گزارش می‌دهد.

---

## نام‌گذاری فایل

```text
NNNN-short-kebab-title.md
```

مثال: `0001-postgresql-schema-per-module.md`

- شماره‌ها صفرپر چهار رقمی و یکنواخت افزایشی‌اند
- شمارهٔ موجود را بازنویسی/بازاستفاده نکنید
- Status داخل خود ADR ثبت می‌شود (`Proposed` · `Accepted` · `Superseded` · `Rejected`)

## قانون ایجاد

- ADR محتوایی فقط در Task صریحی که تصمیم را می‌گیرد ایجاد شود
- Agent بدون دستور Task، ADR اختراع نکند
- Accepted شدن نیازمند بازبینی معمار است؛ Proposed به‌تنهایی لازم‌الاجرا به‌معنای Accepted نیست

---

## ADR Index

| ADR | Title | Status |
|-----|-------|--------|
| [`0001`](0001-postgresql-schema-per-module.md) | PostgreSQL schema-per-module · no cross-module FK by default | Accepted |
| [`0002`](0002-uuid-v7-domain-identity.md) | UUID v7 domain identity | Accepted |
| [`0003`](0003-money-currency-irr-toman.md) | Money · Currency · IRR/Toman | Accepted |
| [`0004`](0004-temporal-model-nodatime.md) | Temporal model · NodaTime · IANA | Accepted |
| [`0005`](0005-server-component-first.md) | Server Component first · minimal Client boundary | Accepted |
| [`0006`](0006-direction-neutral-ui-bidi.md) | Direction-neutral UI · logical CSS · explicit bidi | Accepted |
| [`0007`](0007-locale-prefixed-public-routing.md) | Locale-prefixed public routing | Accepted |
| [`0008`](0008-translation-publication-fallback.md) | Translation publication · fallback policy | Accepted |
| [`0009`](0009-centralized-seo-route-ownership.md) | Centralized SEO route ownership | Accepted |
| [`0010`](0010-controlled-indexation-programmatic-seo.md) | Controlled indexation · programmatic SEO | Accepted |
| [`0011`](0011-evidence-based-task-acceptance.md) | Evidence-based task acceptance · quality gates | Accepted |
| [`0012`](0012-automated-architecture-guardrails.md) | Automated architecture guardrails | Accepted |
