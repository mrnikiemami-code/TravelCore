# ADR 0005 — Server Component First and Minimal Client Boundary

- **Status:** Accepted
- **Date:** 2026-08-11
- **Task:** TC-P00-T004
- **Related:** [`../architecture/10-ui-constitution.md`](../architecture/10-ui-constitution.md) · [`../architecture/02-technology-baseline.md`](../architecture/02-technology-baseline.md)

---

## Context

TravelCore public UI باید SEO-first، mobile-friendly و با JavaScript مرورگر حداقلی باشد. Next.js App Router امکان Server Components پیش‌فرض را می‌دهد، اما الگوی راحتی `"use client"` روی کل صفحه، hydration و کشف‌پذیری را تضعیف می‌کند.

صفحات سفر (مثل Foreign Tour Detail) ترکیبی از محتوای عمدتاً خواندنی و جزایر تعاملی کوچک (فیلتر، date picker، booking CTA) هستند.

---

## Decision

1. **Server Components پیش‌فرض** برای pages، layouts محتوایی، sections و بیشتر Domain/Compositeهای غیرتعاملی.
2. Client Components فقط وقتی تعامل مرورگر واقعاً لازم است: فیلتر، date/passenger picker، carousel controls، map، modal/sheet state، فرم پویا، booking interaction، APIهای browser-only.
3. **Client boundary را کوچک نگه دارید** — ترجیح: صفحه Server + جزیرهٔ Client، نه کل صفحه Client.
4. `"use client"` روی page/root layout صرفاً برای راحتی **ممنوع** است.
5. محتوای SEO-sensitive (عنوان، توضیح اصلی، breadcrumb، زمینهٔ قیمت در صورت مناسب) باید server-renderable باشد و فقط به fetch بعد از hydration وابسته نباشد.
6. Frontend از semantic read models مصرف می‌کند؛ EF Entity و فیلدهای layout در API ممنوع‌اند.

---

## Alternatives Considered

| گزینه | چرا کنار گذاشته شد |
|-------|---------------------|
| Client Components به‌عنوان پیش‌فرض | JS بیشتر، SEO/LCP ضعیف‌تر، hydration گسترده |
| کل Tour page به‌عنوان یک Client Component | مرز داده و عملکرد را خراب می‌کند |
| SPA جدا برای public site | خلاف baseline Next.js App Router و SEO-first |
| RSC فقط برای shell و همهٔ داده client-only | محتوای کشف‌پذیر دیر یا غایب می‌شود |

---

## Consequences

### مثبت

- JS مرورگر کمتر
- SEO و initial render بهتر
- hydration محدودتر
- مرز دادهٔ واضح‌تر بین Server و Client

### منفی / هزینه

- تیم باید مرز Server/Client را آگاهانه طراحی کند
- برخی کتابخانه‌های صرفاً client نیاز به island دارند
- ترکیب دادهٔ Server با state محلی Client نیاز به قرارداد واضح دارد

### Mitigation

- Page Archetype Contract قبل از پیاده‌سازی صفحات مهم
- مثال‌های island در UI Constitution
- بازبینی Design Review Gate شامل hydration/performance risk

---

## Migration / Impact

پیاده‌سازی در P02 و صفحات بعدی. این ADR کتابخانه یا کد نصب/ایجاد نمی‌کند. Status: **Accepted**.
