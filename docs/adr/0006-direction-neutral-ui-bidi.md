# ADR 0006 — Direction-Neutral UI, Logical CSS, and Explicit Bidi Handling

- **Status:** Accepted
- **Date:** 2026-08-11
- **Task:** TC-P00-T004
- **Related:** [`../ui/03-rtl-ltr-bidi.md`](../ui/03-rtl-ltr-bidi.md) · [`../architecture/10-ui-constitution.md`](../architecture/10-ui-constitution.md) · ADR 0003 (Money/IRR)

---

## Context

TravelCore از روز اول locales با جهت‌های متفاوت دارد (`fa`/`ar` RTL، `en` LTR) و مقادیر mixed-direction فراوان (کد فرودگاه، شماره پرواز، ارز، شناسه رزرو). حل RTL فقط با `direction: rtl` روی body و آینه‌سازی کور، یا قاطی‌کردن bidi با جهت صفحه، UX و صحت معنایی را خراب می‌کند.

---

## Decision

1. Document root محلی‌شده باید `lang` و `dir` صحیح تولید کند (`fa`/`ar` → `rtl`، `en` → `ltr`).
2. کامپوننت‌های عمومی **direction-neutral** با مفاهیم `start`/`end` و CSS logical properties.
3. از physical `left`/`right` / `margin-left` / … در کامپوننت‌های direction-neutral پراکنده استفاده نشود مگر جهت فیزیکی واقعاً معنایی باشد.
4. آینه‌سازی RTL معنایی است — لوگو، نقشه، مسیر جغرافیایی، media، و نمادهای فنی کورانه آینه نشوند.
5. **Bidi جدا از RTL صفحه است.** مقادیر مانند `IKA`، `IST`، `EK978`، `USD`، email، booking reference معمولاً LTR می‌مانند؛ با `dir="ltr"` / `dir="auto"` یا کامپوننت‌های bidi-safe مفهومی.
6. Unicode directional control characters استراتژی پیش‌فرض نیستند.
7. MixedCurrencyPrice مؤلفه‌ها را جدا نشان می‌دهد؛ جمع/تبدیل خاموش ممنوع. واحد Toman در UI صریح است؛ IRR کاننیکال می‌ماند.
8. مسیر معنایی مثل `IKA → IST` کورانه به‌خاطر RTL آینه نشود.

---

## Alternatives Considered

| گزینه | چرا کنار گذاشته شد |
|-------|---------------------|
| فقط `body { direction: rtl }` | کافی نیست؛ physical CSS می‌شکند؛ bidi حل نمی‌شود |
| آینه‌سازی خودکار همه‌چیز | معنای جغرافیایی/برند/media را خراب می‌کند |
| دو درخت کامپوننت جدا RTL و LTR | هزینه نگهداری بالا؛ duplication |
| فرض یکی بودن Locale و جهت هر مقدار | کدها و شناسه‌ها اشتباه render می‌شوند |
| کنترل‌کاراکترهای Unicode همه‌جا | شکننده، سخت برای نگهداری |

---

## Consequences

### مثبت

- یک Design System برای fa/en/ar
- صحت bidi برای travel codes
- کاهش باگهای left/right
- هم‌راستایی با Money/IRR accepted policy در نمایش

### منفی / هزینه

- نیاز به انضباط logical CSS / Tailwind direction-safe
- برخی utilityهای physical باید encapsulate شوند
- تست صریح mixed-bidi لازم است

### Mitigation

- سند `03-rtl-ltr-bidi.md`
- ماتریس بازبینی FA RTL / EN LTR
- کامپوننت‌های bidi-safe مفهومی در P02
- ممنوعیت آینه‌سازی کور در Design Review

---

## Migration / Impact

پیاده‌سازی root `lang`/`dir`، primitives direction-neutral، و bidi utilities در P02. بدون نصب پکیج در این Task. Status تا بازبینی معمار **Proposed** می‌ماند.
