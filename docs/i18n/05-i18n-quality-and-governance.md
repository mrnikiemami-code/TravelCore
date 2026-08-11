# i18n Quality and Governance

منبع: [`../architecture/11-internationalization-architecture.md`](../architecture/11-internationalization-architecture.md)

---

## 1. Accessibility — Language Semantics

`lang` صحیح برای screen readers مهم است.

Document root باید زبان صفحه را نشان دهد.

قطعات معنادار به زبان دیگر ممکن است `lang` تو در تو داشته باشند.  
همهٔ محتوای mixed را کورانه زبان root نکنید.

Language و direction concerns دسترس‌پذیری جدا هستند (مکمل ADR 0006).

### Localized alt text

Alt معنادار تصویر از locale محتوای فعال پیروی کند وقتی ترجمه وجود دارد.  
Alt فارسی را به‌عنوان alt دائمی انگلیسی reuse نکنید.

---

## 2. Translation Quality

- Machine translation خودکار Published نشود
- Completeness قبل از publish locale بررسی شود
- Source locale شفاف باشد
- Fallback مخفی در public ممنوع

---

## 3. Machine Translation Governance

نقش آینده: کمک editorial — نه انتشار خودکار.  
Provenance (human/machine/imported/provider) نباید مسدود شود. Provider/TM exact → deferred.

---

## 4. Observability

آینده باید قابل تشخیص کند:

- missing UI keys
- missing required translations
- failed locale mappings
- invalid locale codes
- fallback usage (جایی که مرتبط است)

مشکلات سیستمی missing-translation را خاموش پنهان نکنید. Logging/metrics دقیق → later.

---

## 5. Cache Awareness — مثال ۱۶

هر cache خروجی localized باید locale را در هویت cache داشته باشد:

```text
Destination:123:fa  ≠  Destination:123:en
```

Locale-blind cache pollution ممنوع.

---

## 6. API / Contracts Quality

Public: معمولاً یک locale.  
Admin: چند ترجمه مجاز.

بدون `NameFa`/`NameEn`. بدون کپی همهٔ ترجمه‌ها به هر public response.

### مثال ۱۷

```text
Semantic error identity (language-neutral)
→ Presentation localizes message
```

Domain به UI locale وابسته نیست.

---

## 7. Search / Projections

Search projection ممکن است اسناد localized داشته باشد — **derived** است، منبع حقیقت ترجمه نیست (P15).

---

## 8. Analytics

رویدادها ممکن است active locale را به‌عنوان metadata داشته باشند.  
هویت رویداد زبان‌خنثی می‌ماند — برچسب ترجمه‌شده به‌عنوان event ID استفاده نشود.

---

## 9. Unicode / Collation Future Concerns

Normalization و collation برای fa/ar intentional و use-case-specific باشند.  
جزئیات Search/PostgreSQL → later.  
Slug rules → T006.

---

## 10. Code / Resource Governance

شناسه‌های کد انگلیسی می‌مانند (`DestinationId`, `LocaleCode`).  
کامنت فارسی برای WHY مجاز است.

ساختار فیزیکی فایل‌های localization و انتخاب library → P01/P02 — الان ایجاد/نصب نشود.

---

## 11. Testing Expectations

حداقل اعتبارسنجی آینده:

| مورد | انتظار |
|------|--------|
| FA root | `lang=fa` `dir=rtl` |
| AR root | `lang=ar` `dir=rtl` |
| EN root | `lang=en` `dir=ltr` |
| Missing translation publication | no fake EN page with FA body |
| Language switch | equivalent localized resource |
| No silent public cross-language fallback | enforced |
| Bidi values | IKA/EK978/USD correct |
| Number/currency formatting | identifiers preserved; Money currency stable |
| Calendar independence | fa + Gregorian possible |
| Locale-aware caching | keys include locale |

کتابخانه‌های تست → deferred.

---

## 12. Anti-Pattern Checklist

- [ ] NameFa / NameEn / NameAr
- [ ] Global mega Translation table
- [ ] Three-language-only permanent enum
- [ ] Multi-language indexable content on one canonical URL
- [ ] Accept-Language overrides explicit locale route
- [ ] Silent FA under `/en/`
- [ ] Row exists = Published
- [ ] Locale determines Currency / Calendar / TimeZone
- [ ] Locale change converts Money
- [ ] Blind identifier digit localization
- [ ] Translated status text as status identity
- [ ] UI strings in Domain
- [ ] Provider locale as TravelCore canonical
- [ ] All translations in every public API payload
- [ ] Locale-blind localized caches
- [ ] Blind `/fa/` → `/en/` slug-preserving switch
- [ ] Auto-publish machine translation
- [ ] Mislabeled page language vs primary content
