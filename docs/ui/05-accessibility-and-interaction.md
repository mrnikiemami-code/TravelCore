# Accessibility and Interaction

منبع: [`../architecture/10-ui-constitution.md`](../architecture/10-ui-constitution.md)

Accessibility فاز نهایی نیست — first-class است.

---

## 1. Semantic HTML

ترجیح:

```html
header · nav · main · section · article · aside · footer
button · form · label · a
```

هر عنصر تعاملی را `div` نکنید. اقدام کلیک‌پذیر باید عنصر تعاملی مناسب باشد.

### مثال ۱۴ — Button vs Link

| نیاز | عنصر |
|------|------|
| Navigation به URL | `Link` / `<a>` |
| انجام action (submit، apply filter، open sheet) | `<button>` |

فقط به‌خاطر استایل از button برای navigation یا برعکس استفاده نکنید.

---

## 2. Heading Hierarchy

سلسله‌مراتب heading از **معنای محتوا** می‌آید، نه از اندازهٔ فونت.

Typography = styling.  
Heading levels = document structure.

---

## 3. Keyboard و Focus

- ناوبری کیبورد برای جریان‌های حیاتی
- focus مرئی؛ `outline` را بدون جایگزین accessible حذف نکنید
- Dialog/Sheet باید focus management درست داشته باشد (پیاده‌سازی P02)

---

## 4. ARIA

اول HTML بومی معنایی. ARIA وقتی semantics بومی کافی نیست.

ARIA دلخواه «برای ظاهر accessible» اضافه نکنید.

---

## 5. Contrast و رنگ

رنگ به‌تنهایی برای success/failure/selection/availability کافی نیست. Contrast رعایت شود.

---

## 6. Touch

Touch target تقریبی ~44px usable. اقدامات حیاتی بدون وابستگی به hover. جزئیات: [`02-responsive-mobile-architecture.md`](02-responsive-mobile-architecture.md)

---

## 7. Reduced Motion

`prefers-reduced-motion` احترام شود. Motion سنگین که performance/a11y/پاسخ‌گویی را خراب کند ممنوع به‌عنوان الگوی پیش‌فرض.

---

## 8. Form Architecture

فرم باید پشتیبانی کند:

- labels (placeholder جایگزین label نیست)
- help text
- error text
- required / disabled
- loading/submitting
- server validation feedback
- keyboard navigation
- per-field direction (email/flight LTR؛ نام فارسی RTL/auto)

### Input mode

موبایل: phone · email · numeric amount · passport/code · date در صورت مناسب.  
کیبورد عددی را به مقادیری که ممکن است حرف داشته باشند تحمیل نکنید.

---

## 9. Validation UX

Client validation = usability.  
Server validation = authoritative.

خطا نزدیک فیلد؛ در فرم‌های بزرگ در صورت مفید خلاصه هم.  
Validation نباید همهٔ ورودی کاربر را نابود کند.

---

## 10. Loading States

### مثال ۱۱ — Skeleton

وقتی شکل قابل پیش‌بینی است، skeleton با ابعاد نزدیک به محتوای نهایی ترجیح دارد تا CLS کم شود.

Spinner جایگزین کل layout بی‌ضرورت نشود.

---

## 11. Empty States

Empty ≠ Error.

### مثال ۱۲ — Empty search

```text
چه شد: نتیجه‌ای برای فیلتر/جستجوی فعلی نیست
بعداً چه کار کند: تغییر فیلتر / پاک کردن فیلتر / مشاهدهٔ مقاصد مرتبط
```

نمونه‌های دیگر: No reviews yet · No tours currently available · No saved travelers

---

## 12. Error States

برای کاربر عادی:

- قابل فهم
- در صورت امکان recoverable
- غیرتکنیکی

نمایش به کاربر عمومی ممنوع:

```text
stack trace · SQL error · raw provider exception
```

تشخیص فنی → observability.

---

## 13. Unavailable / Expired

### مثال ۱۳ — Expired Tour detail

محصول سفر می‌تواند منقضی/unavailable شود:

Tour departure expired · Tour unavailable · Hotel no longer bookable · Offer/Quote expired

**هر unavailable را خودکار 404 نکنید.**  
چرخهٔ SEO ممکن است از availability عملیاتی جدا باشد (TC-P00-T006).

---

## 14. Interaction Feedback

حالت‌های اقدام:

idle · hover (در صورت کاربرد) · focus · pressed · disabled · loading · success/error feedback

ارسال تکراری به‌خاطر نبودن pending feedback نباید رخ دهد.

---

## 15. Optimistic UI

جایی که rollback امن و سود واضح است مجاز است.

تأیید خوش‌بینانه برای Payment / Booking / Reservation برگشت‌ناپذیر قبل از تأیید backend ممنوع.

---

## 16. Toast Policy

Toast بازخورد موقت مکمل است. اطلاعات حیاتی پایدار را فقط داخل toast ناپدیدشونده نگذارید. خطاهای نیازمند اقدام در context بمانند.

---

## 17. Navigation a11y

Navigation با keyboard · touch · pointer. Mega menu دسکتاپ معادل موبایل usable دارد. Breadcrumb هم ناوبری معنایی و هم پشتیبان SEO است.

---

## 18. Content disclosure

Accordion/tabs محتوا را از دسترس‌پذیری یا crawl ضروری بی‌جهت محروم نکنند. جزئیات density: [`01-design-system-architecture.md`](01-design-system-architecture.md)

---

## 19. Testing Expectation

پیاده‌سازی UI مهم آینده ترکیبی متناسب از:

component tests · interaction tests · visual/responsive review · accessibility checks · E2E

کتابخانه‌های دقیق → later. الان انتخاب نشوند.

ماتریس حداقل بازبینی: FA RTL Desktop/Mobile · EN LTR Desktop/Mobile (+ AR) و mixed bidi.
