# ADR 0001 — PostgreSQL Schema-per-Module and No Cross-Module FK by Default

- **Status:** Proposed
- **Date:** 2026-08-10
- **Task:** TC-P00-T003
- **Related:** [`../architecture/07-data-architecture.md`](../architecture/07-data-architecture.md) · [`../architecture/08-persistence-and-migrations.md`](../architecture/08-persistence-and-migrations.md)

---

## Context

TravelCore یک Modular Monolith با مالکیت قوی ماژول است. یک PostgreSQL واحد برای سادگی عملیاتی مطلوب است، اما اگر همهٔ جداول در `public` یا با FK فیزیکی بین‌ماژولی به هم قفل شوند، استقلال migration، مرز DbContext، و امکان استخراج آینده از بین می‌رود.

Dependency Rules دسترسی به DbContext و EF navigation بین‌ماژولی را ممنوع کرده بود؛ استراتژی FK فیزیکی تا این Task باز مانده بود.

---

## Decision

1. یک PostgreSQL database برای Modular Monolith اولیه.
2. هر ماژول کسب‌وکار schemaی خودش را مالک است (`tour`, `place`, `pricing`, … — `lowercase snake_case`).
3. `public` محل dumping جداول کسب‌وکار نیست.
4. هر ماژول DbContext و migrations خودش را مالک است؛ تاریخچهٔ migration مفهومی داخل همان schema.
5. **پیش‌فرض:** بدون PostgreSQL FK بین schemaهای ماژول. ارجاع بین‌ماژولی منطقی (مثلاً `hotel_id`) است.
6. داخل همان ماژول، FK رابطه‌ای معمول و مطلوب است.
7. استثناء cross-module FK فقط با تأیید معمار و ADR جدید.

---

## Alternatives Considered

| گزینه | چرا کنار گذاشته شد |
|-------|---------------------|
| یک schema `public` برای همه | مالکیت و migration مستقل را نابود می‌کند |
| یک database per module از روز اول | پیچیدگی عملیاتی زودهنگام؛ خلاف Modular Monolith فعلی |
| FK فیزیکی بین‌ماژولی همه‌جا | coupling schema؛ ترتیب migration شکننده؛ استخراج سخت |
| بدون هیچ FK حتی داخل ماژول | یکپارچگی ساختاری داخل مالکیت را بی‌جهت تضعیف می‌کند |

---

## Consequences

### مثبت

- مرز مالکیت داده با مرز دامنه هم‌راستا می‌شود
- migrationها محدود به schema مالک می‌مانند
- مسیر استخراج آیندهٔ ماژول بازتر است
- DbContext-per-module طبیعی‌تر enforce می‌شود

### منفی / هزینه

- دیتابیس نمی‌تواند referential integrity بین‌ماژولی را enforce کند
- orphan منطقی بدون validation دامنه ممکن است
- ابزارهای reporting خام نمی‌توانند روی FK بین‌schema تکیه کنند

### Mitigation

- قراردادهای Application برای اعتبارسنجی ایجاد/به‌روزرسانی
- قواعد lifecycle و snapshot تاریخی
- رویداد / projection برای خواندن مشتق
- ممنوعیت join راحت Dapper بین‌schema مگر تأیید معماری

---

## Migration / Impact

پیاده‌سازی schemaها، DbContextها و migration runner در P01. این ADR محتوایی پیاده‌سازی نمی‌کند.
