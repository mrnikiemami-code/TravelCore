# ADR 0009 — Centralized SEO Route Ownership

- **Status:** Proposed
- **Date:** 2026-08-11
- **Task:** TC-P00-T006
- **Related:** [`../seo/01-route-canonical-and-redirects.md`](../seo/01-route-canonical-and-redirects.md) · [`../architecture/12-seo-constitution.md`](../architecture/12-seo-constitution.md)

---

## Context

بدون مالکیت متمرکز مسیر، هر ماژول (Destination، Tour، Place، Content) ممکن است رجیستری URL جدا، slug بدون تاریخچه، و conflict مسیر بسازد. در عین حال اگر SEO مالک محتوای کسب‌وکار شود، مرز ماژول می‌شکند.

---

## Decision

1. **SEO** مالک مکانیک مسیر عمومی است: SeoRoute · LocalizedSlug · Canonical · Redirect history · IndexPolicy · namespace تعارض مسیر.
2. **ماژول‌های کسب‌وکار** مالک محتوای کسب‌وکار باقی می‌مانند (عنوان، توضیح، وضعیت تجاری، قیمت، …).
3. مسیر عمومی به هویت معنایی `ResourceType + ResourceId` نگاشت می‌شود — مسیر هویت اصلی کسب‌وکار نیست.
4. Conflict در namespace مسیر عمومی به‌صورت متمرکز کنترل/رزرو می‌شود قبل از publication.
5. هر ماژول رجیستری URL مستقل بی‌ربط پیاده نمی‌کند.
6. این تمرکز فنی مسیر است، نه تمرکز دادهٔ کسب‌وکار / دیتابیس موازی محصول.

---

## Alternatives Considered

| گزینه | چرا کنار گذاشته شد |
|-------|---------------------|
| هر ماژول URL registry خودش | conflict · redirect history پراکنده · slug PK خطر |
| SEO مالک کل محتوای Tour/Hotel | نقض مرز ماژول |
| فقط URL string روی هر Entity | بدون canonical/redirect/index mechanics |
| Search index به‌عنوان SoR مسیر | Search derived است |

---

## Consequences

### مثبت

- یک namespace مسیر
- redirect/canonical منسجم
- مرز محتوا حفظ می‌شود
- پشتیبانی چندزبانهٔ مسیر بدون duplicate Entity

### منفی / هزینه

- وابستگی publication به قرارداد SEO
- نیاز به consistency قوی برای reservation
- طراحی event/projection برای به‌روزرسانی

### Mitigation

- SeoRoute concept و validation قبل از publish
- مصرف رویدادهای معنایی کسب‌وکار
- اسناد ownership صریح

---

## Migration / Impact

پیاده‌سازی در P05 و فازهای ماژول. Schema دقیق deferred. Status تا بازبینی معمار **Proposed**.
