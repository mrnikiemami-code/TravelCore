# ADR 0012 — Automated Architecture Guardrails

- **Status:** Proposed
- **Date:** 2026-08-11
- **Task:** TC-P00-T008
- **Related:** [`../architecture/14-engineering-quality-constitution.md`](../architecture/14-engineering-quality-constitution.md) · [`../quality/03-architecture-and-contract-testing.md`](../quality/03-architecture-and-contract-testing.md) · [`../architecture/04-module-boundaries.md`](../architecture/04-module-boundaries.md) · [`../architecture/05-dependency-rules.md`](../architecture/05-dependency-rules.md)

---

## Context

Accepted TravelCore architecture (Modular Monolith, schema-per-module, no cross-module DbContext/EF navigation, Domain purity, Server Component first, no NameFa/NameEn/NameAr, …) can drift through incremental PRs if only human review is relied upon. Mechanical enforcement is needed for high-value structural rules without claiming that tests replace architects.

---

## Decision

1. Accepted **structural** architecture should be enforced by **automated architecture tests** where mechanically practical.
2. Architecture tests **supplement**, not replace, architecture review and ADR process.
3. **High-priority automated guardrails** include: module/dependency direction, Domain ↛ Infrastructure/frameworks, no cross-module persistence/DbContext/EF navigation, no giant shared DbContext, detectable NameFa/NameEn/NameAr patterns, Search/SEO/Notification not becoming core business correctness dependencies.
4. Detectable **architecture drift** should fail the relevant Architecture quality gate (Task cannot be PASS while required architecture assertions fail).
5. Exact architecture-test library and project layout are deferred to P01; this ADR locks the obligation and philosophy.

---

## Alternatives Considered

| گزینه | چرا کنار گذاشته شد |
|-------|---------------------|
| فقط code review انسانی | drift تکراری و خطای Agent |
| architecture tests جایگزین ADR/معمار | تصمیم معنایی را نمی‌توان فقط مکانیکی قفل کرد |
| بدون gate معماری در CI آینده | نقض مرزها دیر کشف می‌شود |

---

## Consequences

### مثبت

- تشخیص زودهنگام نقض مرز
- محافظت از تصمیم‌های Accepted در برابر پیاده‌سازی شتاب‌زده
- شاهد عینی برای Architecture gate

### منفی / هزینه

- نگهداری قوانین تست معماری
- false positive اگر قوانین خیلی سخت‌گیرانه/ناپخته نوشته شوند — نیاز به طراحی دقیق در P01

---

## Migration / Impact

پیاده‌سازی suite در P01. تا آن زمان، بازبینی دستی مرز + گزارش Architecture gate با شواهد دستی/جزئی.

---

## Status Note

**Proposed** تا پذیرش معمار. Cursor نمی‌تواند این ADR را خود Accepted کند.
