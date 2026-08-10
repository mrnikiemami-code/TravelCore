# RTL / LTR / Bidi Architecture

منبع: [`../architecture/10-ui-constitution.md`](../architecture/10-ui-constitution.md)  
ADR مرتبط (Proposed): [`../adr/0006-direction-neutral-ui-bidi.md`](../adr/0006-direction-neutral-ui-bidi.md)  
پول: [`../data/02-money-and-currency.md`](../data/02-money-and-currency.md)

---

## 1. Document Root

پیاده‌سازی آیندهٔ root محلی‌شده:

### مثال ۱ — فارسی

```html
<html lang="fa" dir="rtl">
```

### مثال ۲ — انگلیسی

```html
<html lang="en" dir="ltr">
```

### عربی

```html
<html lang="ar" dir="rtl">
```

**ممنوع به‌عنوان راه‌حل کامل:** فقط `body { direction: rtl; }` و پراکندن `left`/`right`.

الان پیاده نشود.

---

## 2. Direction-Neutral Components

کامپوننت‌ها تا حد امکان direction-neutral باشند.

مفاهیم معنایی:

```text
start · end · inline-start · inline-end
```

ترجیح CSS logical:

```css
margin-inline-start
margin-inline-end
padding-inline-start
padding-inline-end
inset-inline-start
inset-inline-end
border-inline-start
text-align: start
```

از physical پراکنده پرهیز کنید مگر جهت فیزیکی واقعاً معنایی باشد:

```css
left · right · margin-left · margin-right · padding-left · padding-right
```

در Tailwind: از پراکندن `ml-*` / `mr-*` / `pl-*` / `pr-*` / `left-*` / `right-*` در کامپوننت‌های direction-neutral اجتناب کنید؛ رفتار direction-sensitive را عمداً encapsulate کنید.

---

## 3. RTL ≠ آینه کردن همه چیز

آینه‌سازی معنایی است، نه تزئین خودکار.

### معمولاً direction-aware

- جریان navigation
- breadcrumbs
- لبهٔ drawer
- تراز متن
- اقدامات start/end
- layout جهت‌دار

### اغلب کورانه آینه نشوند

- لوگو برند
- نقشه
- جهت جغرافیایی
- charts
- media
- آیکون play (بسته به معنا)
- معنای مسیر فرودگاه
- نمادهای فنی
- برخی کنترل‌های pagination/media بسته به semantics

---

## 4. Bidi جدا از RTL صفحه است

جهت کلی صفحه ≠ جهت هر مقدار.

مقادیری که اغلب داخل UI RTL همچنان LTR می‌مانند:

```text
IKA · IST · EK978
USD · EUR · USDT
URL · email
booking reference · passport number
phone number · provider code · technical identifier
```

### مثال ۳ — IKA / IST در UI فارسی

صفحه `dir=rtl` است؛ کدهای فرودگاه LTR می‌مانند و مسیر معنایی حفظ می‌شود.

### مثال ۴ — EK978

شماره پرواز LTR / identifier است؛ به ارقام محلی تبدیل نشود اگر معنا خراب می‌شود.

### مثال ۵ — USD در UI فارسی

کد ارز LTR می‌ماند؛ مبلغ طبق سیاست formatting نمایش داده می‌شود.

---

## 5. Bidi-safe components (مفهومی — الان ساخته نشوند)

BidiText · MoneyDisplay · CurrencyCode · AirportCode · FlightNumber · Phone · Email · BookingReference · PassportNumber

هدف: encapsulation جهت مقدار بدون Unicode control characters به‌عنوان استراتژی پیش‌فرض.

مکانیزم HTML مناسب:

```html
dir="ltr"
```

یا در صورت مناسب:

```html
dir="auto"
```

---

## 6. Mixed Currency و IRR/Toman

### مثال ۶

```text
1,290 USD
+
119,900,000 IRR
```

مؤلفه‌ها جدا؛ جمع خاموش ممنوع؛ تبدیل بدون سیاست Pricing/display ممنوع.

### مثال ۷

```text
Display unit clearly labeled: تومان
Canonical persisted value: IRR
```

کاربر نباید عددی ببیند که نداند IRR است یا Toman.

---

## 7. مثال ۱۴ — مسیر پرواز در RTL

```text
IKA → IST
```

معنای جغرافیایی سفر است. فلش/ترتیب مسیر را فقط به‌خاطر RTL کورانه آینه نکنید؛ خوانایی معنایی مسیر اولویت دارد.

---

## 8. Forms و جهت فیلد

هر فیلد ممکن است جهت مستقل از صفحه داشته باشد:

| فیلد | جهت معمول |
|------|-----------|
| Email | LTR |
| Flight number | LTR |
| Passport number | LTR |
| نام فارسی | RTL / auto |

معماری فرم باید per-field direction را پشتیبانی کند.

---

## 9. Date/Time نمایش

زمان محلی + timezone context ممکن است نشان داده شود. Calendar = presentation preference (مستقل از Locale طبق Data Architecture). معنای Instant/Local* mutate نشود.

---

## 10. ضدالگوها

- RTL فقط با body direction
- آینه‌سازی کور همه چیز
- فرض یکی بودن RTL و bidi
- physical left/right پراکنده در primitives
- تبدیل شناسه‌ها به ارقام محلی مخرب
- مخفی کردن واحد Toman/IRR
