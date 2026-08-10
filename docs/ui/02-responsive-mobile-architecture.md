# Responsive and Mobile Architecture

منبع: [`../architecture/10-ui-constitution.md`](../architecture/10-ui-constitution.md)

---

## 1. Mobile First

TravelCore public UI **Mobile First** است.

موبایل **نیست**:

```text
Desktop layout compressed
```

هر Page Archetype مهم نیاز به طراحی رفتار صریح Desktop / Tablet / Mobile دارد.

---

## 2. Representative Viewports

نقاط اعتبارسنجی آینده:

```text
360 · 390 · 768 · 1024 · 1280 · 1440
```

الزام شش media query جدا نیستند. Layout باید بین عرض‌ها پیوسته رفتار کند.

---

## 3. Touch Target

ناحیهٔ لمسی تعاملی عموماً حدود:

```text
≈ 44px
```

usable area. اندازهٔ بصری آیکون می‌تواند کوچک‌تر باشد؛ ناحیهٔ لمس باید usable بماند.

کنترل‌های موبایل را فقط برای تطبیق تراکم دسکتاپ ریز نکنید.

---

## 4. No Hover-Only Critical UX

اقدامات حیاتی نباید به hover وابسته باشند:

Tour booking · Filters · Gallery · Menu · Price selection

باید با touch · keyboard · pointer کار کنند.

---

## 5. Responsive Behavior Matrix

هر archetype مهم باید در نهایت ماتریس رفتار داشته باشد.

ستون‌های مفهومی:

| Element | Desktop | Tablet | Mobile | RTL/LTR Notes | Accessibility Notes |
|---------|---------|--------|--------|---------------|---------------------|

صرفاً نوشتن «responsive» کافی نیست — رفتار را مشخص کنید.

### نمونه مفهومی — Booking Sidebar

| Element | Desktop | Tablet | Mobile |
|---------|---------|--------|--------|
| Booking / pricing | sticky side panel | ممکن است فشرده‌تر / زیر محتوا | sticky bottom CTA |
| جزئیات رزرو | در sidebar | ترکیبی | bottom sheet / sheet |
| Hotel options | جدول/کارت غنی | کارت | کارت‌های لمسی |

---

## 6. مثال ۸ — Foreign Tour Detail (جهت معماری UX)

### Desktop

- محتوای اصلی
- sticky booking/pricing sidebar
- ارائهٔ غنی hotel options
- اطلاعات itinerary / flight

### Mobile

- تک‌ستونه
- CTA رزرو sticky/persistent در صورت مفید
- جزئیات رزرو در sheet / bottom sheet
- hotel options به‌صورت کارت (نه جدول عریض ناخوانا)
- گالری touch-friendly

ظاهر نهایی بصری طراحی نمی‌شود — فقط جهت معماری.

---

## 7. Tables → Mobile

جدول دسکتاپ نباید روی موبایل صرفاً overflow ناخوانا شود.

برای دادهٔ پیچیده:

| Desktop | Mobile |
|---------|--------|
| Table | Cards / stacked rows / summary→detail |

### مثال ۹ — Pricing table

ردیف‌هایی مثل:

```text
Adult / Double
Adult / Single
Child with bed
Child without bed
```

با ارزهای ترکیبی.

موبایل باید هویت معنایی ردیف را حفظ کند. اسکرول افقی جدول عظیم به‌عنوان UX پیش‌فرض ممنوع است.

---

## 8. Filter UX

| Desktop | Mobile |
|---------|--------|
| Sidebar / panel | Sheet · Bottom Sheet · Full-screen filter |

وضعیت فیلتر باید واضح باشد. Apply / Reset صریح.

### URL filter state

وضعیت فیلتر عمومی در URL جایی که به shareability · back · SSR · کنترل SEO · restore کمک کند مفید است.

اما URL فیلتر به‌طور خودکار Landing SEO ایندکس‌پذیر نیست → TC-P00-T006.

---

## 9. Sticky UI

Sticky وقتی تکمیل وظیفه را بهتر کند مجاز است:

Desktop booking sidebar · Mobile booking CTA · Filter controls

نباید:

- محتوا را بپوشاند
- viewport را گیر بیندازد
- با keyboard مرورگر بجنگد
- ارتفاع موبایل را بیش از حد مصرف کند

---

## 10. Modals / Sheets

| الگو | کاربرد |
|------|--------|
| Dialog | اقدام فشردهٔ متمرکز |
| Sheet | workflow ثانویهٔ بزرگ‌تر |
| Bottom Sheet | اقدام زمینه‌ای موبایل |
| Full route/page | عملیات چندمرحله‌ای پیچیده |

جریان رزرو پیچیده را در dialog کوچک نگذارید.

محتوای عمومی/SEO-significant نباید فقط داخل modal state کلاینت پنهان شود — URL پایدار ترجیح دارد.

---

## 11. Safe Areas و Mobile Keyboard

Sticky/bottom UI باید safe-area insets دستگاه را در نظر بگیرد (booking CTA · bottom nav · bottom sheets).

Sheet/form باید keyboard نرم‌افزاری موبایل را در نظر بگیرد تا CTA پایین غیرقابل دسترس نشود. پیاده‌سازی دقیق → P02.

---

## 12. Localization Length

کنترل‌ها باید انبساط/انقباض متن fa / ar / en را تحمل کنند. عرض ثابت متن مگر ضرورت معنایی.

Truncation عمدی باشد. هرگز بدون مکانیزم accessible، این‌ها را truncate نکنید:

critical price · booking reference · validation errors مهم

---

## 13. Date/Time UI

طبق temporal architecture پذیرفته‌شده:

ممکن است local date/time + timezone context نشان داده شود. Calendar display = presentation. معنای زمانی کاننیکال در UI mutate نشود.
