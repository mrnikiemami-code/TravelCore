# قالب Prompt و Traceability در TravelCore

این سند ساختار استاندارد Prompt پیاده‌سازی و ردپای Task را تعریف می‌کند. گردش‌کار کامل: [`../architecture/09-ai-development-workflow.md`](../architecture/09-ai-development-workflow.md)

---

## شناسهٔ Task

```text
TC-P03-T005
```

| بخش | معنی |
|-----|------|
| TC | TravelCore |
| P03 | Phase 03 |
| T005 | Task 005 |

Promptهای اجرایی در همین پوشه (`docs/prompts/`) ذخیره می‌شوند.

نمونه پیام commit:

```text
feat(destination): add localized public detail [TC-P05-T004]
```

---

## ساختار استاندارد Prompt

بخش‌های زیر استانداردند. Prompt مجبور نیست بخش‌های نامرتبط را خالی پر کند؛ اما محدودیت‌های مهم Scope باید صریح باشند.

```markdown
# TC-Pxx-Tyyy — Title

## Goal
...

## Context
...

## Read First
- AGENTS.md
- docs/...

## Scope
...

## Non-goals
...

## Domain Rules
...

## Data Rules
...

## API Contract
...

## UI Contract
...

## i18n Rules
...

## RTL/LTR Rules
...

## SEO Impact
...

## Allowed Files
...

## Tests
...

## Acceptance Criteria
...

## Required Final Report
...
```

---

## قواعد نوشتن Prompt خوب

- Goal یک جملهٔ قابل‌اندازه‌گیری باشد.
- Non-goals جلوی «تا اینجام» را بگیرد.
- Allowed Files مرز نوشتن را روشن کند.
- تمایزهای دامنهٔ مرتبط (مثلاً TourProduct/Departure یا Price/Quote) را صریح تکرار کند اگر Task به آن‌ها دست می‌زند.
- برای صفحات عمومی، i18n / RTL / SEO / Mobile را در صورت ارتباط خالی نگذارد.

---

## گزارش نهایی مورد انتظار از Agent

حداقل محورها (قابل تطبیق با Task):

- Status
- Files changed
- Tests/builds
- Deviations
- Architectural Concerns (در صورت وجود؛ بدون پیاده‌سازی خودسرانه)

جزئیات NEVER/ALWAYS در [`../../AGENTS.md`](../../AGENTS.md).
