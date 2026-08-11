# Locale and Routing

منبع: [`../architecture/11-internationalization-architecture.md`](../architecture/11-internationalization-architecture.md)
ADR مرتبط (Accepted): [`../adr/0007-locale-prefixed-public-routing.md`](../adr/0007-locale-prefixed-public-routing.md)
SEO تفصیلی: TC-P00-T006 · UI direction: ADR 0006 Accepted

---

## 1. Locale و BCP 47

شناسهٔ Locale از semantics تگ زبان **BCP 47** پیروی می‌کند و normalize می‌شود.

Locales استراتژیک اولیه:

```text
fa · en · ar
```

معماری باید بدون redesign schema از تگ‌های آینده پشتیبانی کند:

```text
fa-IR · en-US · en-GB · ar-AE
```

### ممنوع

```text
FA_IR · english · persian · arabic-uae
```

به‌عنوان شناسهٔ canonical. کتابخانهٔ normalization runtime → deferred.

Enum دائمی محدود به دقیقاً سه مقدار ممنوع است.

---

## 2. Locale Registry

TravelCore باید registry/پیکربندی صریح localeهای پشتیبانی‌شده داشته باشد.

فیلدهای مفهومی:

| فیلد | نقش |
|------|-----|
| Code | BCP 47 canonical |
| Language | زبان پایه |
| Direction | rtl / ltr پیش‌فرض سند |
| Enabled | فعال در سیستم |
| PublicAvailability | قابل ارائهٔ عمومی |
| DefaultCalendarPreference | ترجیح پیش‌فرض (نه اجبار ابدی) |
| Formatting metadata | در صورت مناسب |

هر formatting preference را ویژگی immutable غیرقابل‌تغییر Locale نکنید.

`fa` **اجبار نمی‌کند** که تقویم همیشه پارسی باشد.

---

## 3. Default Locale

جهت اولیهٔ محصول عمومی:

```text
fa
```

Default = configuration/محصول. Schema نباید فرض کند فارسی تا ابد default جهانی است.

---

## 4. Locale ≠ فقط زبان ترجمه

Locale گسترده‌تر از ترجمه است، اما مفاهیم جدا می‌مانند:

```text
Locale ≠ Currency ≠ Calendar ≠ TimeZone
```

### مثال ۱۰–۱۱

```text
Locale: fa
Currency display: USD
Calendar: Gregorian
TimeZone: Europe/Istanbul
→ VALID
```

---

## 5. Public Route Strategy — قفل

محتوای عمومی indexable از مسیر **locale-prefixed** استفاده می‌کند.

### مثال ۱–۳

```text
/fa/...
/en/...
/ar/...
```

Locale بخشی از هویت مسیر عمومی است.

### ممنوع

- وابستگی صفحات SEO مهم فقط به `Accept-Language` یا browser state
- سرو زبان‌های indexable متفاوت از **همان** canonical URL

---

## 6. Root Route `/`

`/` ممکن است entry/negotiation باشد.

عوامل مفهومی: ترجیح صریح کاربر · authenticated preference · browser language · default محصول.

محتوای canonical عمومی همچنان locale-explicit می‌ماند.

جزئیات redirect/status/canonical → **TC-P00-T006**.

---

## 7. Negotiation Priority (مفهومی)

1. locale صریح در URL
2. ترجیح ذخیره‌شدهٔ کاربر وقتی URL صریح نیست
3. browser language preference
4. default پیکربندی‌شدهٔ محصول

### قانون حیاتی

وقتی URL عمومی locale دارد، browser preference نباید خاموش آن را عوض کند.

```text
/en/tours/istanbul  + browser=fa  → still English page
```

---

## 8. URL Authoritative برای صفحهٔ عمومی

برای صفحات locale-prefixed، route locale حداقل تعیین می‌کند:

- `html lang`
- document direction
- انتخاب محتوای localized
- context ترجمهٔ UI
- تولید navigation محلی‌شده

جایی که کاربرد دارد.

### HTML direction (مکمل ADR 0006)

| Locale | lang | dir |
|--------|------|-----|
| fa | fa | rtl |
| ar | ar | rtl |
| en | en | ltr |

Direction از metadata/registry می‌آید، نه شرط‌های پراکندهٔ نام route.

---

## 9. Localized Slug

یک Entity عمومی ممکن است slug متفاوت per published locale داشته باشد.

### مثال ۴ / ۹

```text
/fa/destinations/استانبول
/en/destinations/istanbul
/ar/destinations/اسطنبول
```

یک slug جهانی اجباری بین زبان‌ها لازم نیست.

مالکیت: محتوای ترجمه با ماژول Entity؛ مکانیک مسیر با **SEO** (T006). حقیقت مسیر را در هر translation table تکرار نکنید.

---

## 10. Language Switcher

Language switcher باید به **منبع معادل محلی‌شده** برود، نه جایگزینی کور prefix.

### مثال ۹

```text
Current: /fa/destinations/استانبول
Switch EN: /en/destinations/istanbul
```

نه:

```text
/fa/ → /en/  while keeping invalid Persian slug
```

اگر معادل منتشر نشده: unavailable را نشان دهید یا جایگزین صریح طراحی‌شده — لینک مرده و تظاهر ترجمه ممنوع.

---

## 11. Localized Links

لینک‌های داخلی عمومی locale هدف را حفظ کنند وقتی صفحهٔ هدف در آن locale منتشر است.

از صفحهٔ انگلیسی تور → ترجیح `/en/destinations/istanbul` نه تصادفی `/fa/...` وقتی انگلیسی منتشر است.

اگر هدف unavailable → سیاست availability صریح.

---

## 12. User Preference

کاربر authenticated ممکن است preferred locale ذخیره کند؛ anonymous ممکن است cookie/local preference داشته باشد.

این‌ها به negotiation کمک می‌کنند؛ **override** مسیر locale-prefixed صریح نمی‌کنند.

ترجیح کاربر داخل aggregateهایی مثل Tour/Hotel/Destination ذخیره نشود.

---

## 13. Admin / Agency Routing

فرض نکنید Admin/Agency باید دقیقاً همان مکانیک URL عمومی را داشته باشند.
اما UI translation همان semantics Locale را رعایت می‌کند. طراحی دقیق → deferred.

---

## 14. SEO Boundary

i18n تعریف می‌کند: هویت locale · availability محتوا · رابطهٔ مسیر محلی · semantics language switch.

SEO مالک است: canonical · hreflang · redirects · IndexPolicy · slug history · persistence مسیر.

فقط معادل‌های واقعاً published کاندید alternate-language هستند — جزئیات خروجی hreflang → T006.

---

## 15. Testing Expectations (routing)

آینده حداقل:

| Locale | lang | dir |
|--------|------|-----|
| FA | fa | rtl |
| AR | ar | rtl |
| EN | en | ltr |

+ language switch equivalent route · no silent cross-language public fallback
