# ADR 0007 — Locale-Prefixed Public Routing

- **Status:** Accepted
- **Date:** 2026-08-11
- **Task:** TC-P00-T005
- **Related:** [`../i18n/01-locale-and-routing.md`](../i18n/01-locale-and-routing.md) · [`../architecture/11-internationalization-architecture.md`](../architecture/11-internationalization-architecture.md)

---

## Context

TravelCore یک محصول چندزبانهٔ SEO-first است. اگر صفحات عمومی مهم فقط از `Accept-Language` یا state مرورگر locale بگیرند، canonical URL مبهم می‌شود، اشتراک‌گذاری خراب می‌شود، و موتور جستجو نمی‌تواند نسخه‌های زبانی را قابل‌اعتماد از هم جدا کند.

---

## Decision

1. محتوای عمومی indexable از مسیر **locale-prefixed** استفاده می‌کند: `/fa/...`, `/en/...`, `/ar/...` (و تگ‌های گسترش‌یافتهٔ آینده).
2. برای این صفحات، **locale موجود در URL authoritative** است برای `lang`/`dir`، انتخاب محتوا، context UI translation، و navigation محلی.
3. استراتژی canonical محتوا که فقط به Accept-Language وابسته باشد **ممنوع** است.
4. زبان‌های indexable متفاوت از یک canonical URL واحد سرو نشوند.
5. وقتی URL locale صریح دارد، preference مرورگر نباید خاموش آن را جایگزین کند.
6. Language switcher به منبع معادل محلی‌شده می‌رود (نه جایگزینی کور prefix با slug نامعتبر).
7. مسیر عمومی locale فقط وقتی سیاست انتشار آن locale اجازه می‌دهد عمومی می‌شود.
8. مکانیک تفصیلی canonical/hreflang/redirect/IndexPolicy/slug history متعلق به **TC-P00-T006** است.

---

## Alternatives Considered

| گزینه | چرا کنار گذاشته شد |
|-------|---------------------|
| Locale فقط از Accept-Language | canonical مبهم؛ SEO ضعیف؛ share نادرست |
| یک URL بدون prefix + cookie زبان | indexable languages قاطی می‌شوند |
| Subdomain per language از روز اول | پیچیدگی عملیاتی زودهنگام بدون سود کافی |
| Query `?lang=` به‌عنوان هویت اصلی SEO | ضعیف‌تر از path prefix برای TravelCore |

---

## Consequences

### مثبت

- هویت زبانی واضح در URL
- هم‌راستایی با SEO-first و Server rendering
- جلوگیری از override خاموش توسط browser
- پایهٔ صحیح برای hreflang/canonical بعدی

### منفی / هزینه

- نیاز به localized slug mapping برای language switch
- negotiation در `/` باید بعداً دقیق طراحی شود
- مسیرهای بدون ترجمهٔ published نباید جعلی ساخته شوند

### Mitigation

- سند routing + publication policy
- SEO Constitution (T006) برای HTTP/canonical
- Unavailable locale UX صریح

---

## Migration / Impact

پیاده‌سازی routing در P02 و SEO persistence در T006/P05. این ADR کد/پکیج نصب نمی‌کند. Status: **Accepted**.
