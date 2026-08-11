# ADR 0011 — Evidence-Based Task Acceptance and Quality Gates

- **Status:** Proposed
- **Date:** 2026-08-11
- **Task:** TC-P00-T008
- **Related:** [`../architecture/14-engineering-quality-constitution.md`](../architecture/14-engineering-quality-constitution.md) · [`../quality/01-definition-of-done.md`](../quality/01-definition-of-done.md) · [`../quality/07-agent-review-and-task-acceptance.md`](../quality/07-agent-review-and-task-acceptance.md)

---

## Context

Agents and humans may treat `dotnet build` / `npm run build` as Task success, skip applicable gates, or report PASS without evidence. TravelCore already locks architecture that build alone cannot protect (module boundaries, RTL/bidi, SEO truthfulness, migrations, money, i18n).

---

## Decision

1. **Build passing is necessary but not sufficient** for Task acceptance.
2. Each Task must evaluate its **applicable** quality gates explicitly.
3. A required gate that was **not executed** must not be reported as **PASS**.
4. **PASS** requires **evidence** appropriate to the gate (command/result, suite output, matrix notes, migration apply result, architecture assertion output).
5. Gate states **PASS / FAIL / BLOCKED / NOT_APPLICABLE** are explicit and mandatory in reporting discipline.
6. **BLOCKED** is used when a required gate cannot run due to environment/access/decision — not silently skipped as PASS.
7. **NOT_APPLICABLE** requires a credible reason (e.g. docs-only → Frontend E2E N/A).
8. Exact tooling/CI implementation remains deferred; this ADR locks acceptance semantics.

---

## Alternatives Considered

| گزینه | چرا کنار گذاشته شد |
|-------|---------------------|
| Build-only acceptance | ناقص؛ نقض معماری/RTL/SEO/migration را نمی‌گیرد |
| یک checklist یکسان برای همه Taskها | افراطی و غیرمرتبط |
| PASS بدون شواهد | غیرقابل حسابرسی برای AI workflow |
| پوشش درصدی جهانی به‌عنوان Done | کیفیت رفتار را تضمین نمی‌کند |

---

## Consequences

### مثبت

- تعریف مشترک «PASS یعنی چه»
- جلوگیری از گزارش گمراه‌کننده Agentها
- هم‌راستایی با ADRها و constitutonهای پذیرفته‌شده

### منفی / هزینه

- گزارش‌دهی دقیق‌تر زمان می‌برد
- بعضی Taskها BLOCKED می‌شوند تا محیط آماده شود

---

## Migration / Impact

اعمال تدریجی با P01 tooling/CI. تا قبل از tooling، گزارش دستی + شواهد فرمان الزامی است.

---

## Status Note

**Proposed** تا پذیرش معمار. Cursor نمی‌تواند این ADR را خود Accepted کند.
