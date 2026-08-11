# Engineering Quality Constitution — TravelCore

این سند اصول غیرقابل‌مذاکرهٔ کیفیت مهندسی TravelCore را قفل می‌کند. جزئیات در [`../quality/`](../quality/).

مرجع‌های محافظت‌شونده: Accepted ADRs 0001–0010 · [`00-constitution.md`](00-constitution.md) · UI/i18n/SEO/Page Archetypes · [`09-ai-development-workflow.md`](09-ai-development-workflow.md)

ADRهای مرتبط (Proposed تا پذیرش معمار): [`../adr/0011-evidence-based-task-acceptance.md`](../adr/0011-evidence-based-task-acceptance.md) · [`../adr/0012-automated-architecture-guardrails.md`](../adr/0012-automated-architecture-guardrails.md)

---

## 1. Build PASS ≠ Task PASS

موفقیت `dotnet build` یا `npm run build` **لازم** است ولی **کافی نیست**.

Task ممکن است با وجود build سبز، به‌خاطر نقض مرز معماری، تست‌نشده بودن رفتار، migration ناامن، RTL/a11y/SEO شکسته، امنیت، تغییر ناگهانی API، ناسازگاری state docs، یا fallback پنهان **FAIL** شود.

---

## 2. Evidence-Based Acceptance

هر ادعای **PASS** باید شاهد داشته باشد (فرمان، خروجی، ماتریس viewport/locale، assertion معماری، نتیجه migration).

«به‌نظر خوب می‌آید» به‌تنهایی شاهد دروازهٔ حیاتی نیست.

---

## 3. Gate States

| State | معنی |
|-------|------|
| **PASS** | دروازه اجرا شد و شواهد موفقیت دارد |
| **FAIL** | اجرا شد و شکست خورد، یا الزام نقض شد |
| **BLOCKED** | باید اجرا می‌شد ولی محیط/دسترسی/تصمیم مانع است |
| **NOT_APPLICABLE** | با دلیل معتبر برای این Task لازم نیست |

دروازهٔ اجرا‌نشدهٔ لازم را **PASS** گزارش نکنید. اجرا‌نشده ≠ `NOT_APPLICABLE`.

---

## 4. Applicable Gates Only — Not Silent Skip

هر Task دروازه‌های **کاربردی** خود را دارد. همهٔ دسته‌های تست برای هر Task اجباری نیستند؛ اما دروازهٔ کاربردی را نمی‌توان خاموش حذف کرد.

جزئیات: [`../quality/01-definition-of-done.md`](../quality/01-definition-of-done.md)

---

## 5. Protect Accepted Architecture

Quality gates باید از این‌ها محافظت کنند (نه بازطراحی):

Modular Monolith · module ownership · no cross-module DbContext/EF navigation · schema-per-module · UUID v7 · Money/IRR/Toman · NodaTime/IANA · Server Component first · mobile-first · RTL/LTR/bidi · i18n · SEO · page archetypes · One Task → One Writer · AI workflow

---

## 6. Architecture Tests Are Safety Rails

تست‌های معماری drift مکانیکی را می‌گیرند. جایگزین بازبینی معمار نیستند. Passing architecture suite ≠ اثبات همهٔ تصمیم‌ها.

---

## 7. Security From Day One

امنیت فقط penetration نهایی نیست. طراحی و تست پیوسته الزامی است.

---

## 8. Accessibility / Direction / SEO Are Quality

برای UI عمومی مرتبط: a11y، RTL/LTR/bidi، i18n، SEO معنایی — اختیاری نیستند. Schema معتبر با دادهٔ ساختگی همچنان FAIL است.

---

## 9. Database Truth

رفتار PostgreSQL با EF InMemory یا EnsureCreated اثبات نمی‌شود. Migration lifecycle باید از مسیر migrations تأیید شود.

---

## 10. Agent Governance

- Cursor قبل از commit باید self-review کند
- Hermes بازبینی مستقل ریسک‌محور
- معمار برای ADR و تغییر معماری
- Cursor نمی‌تواند ADR را خود پذیرفته اعلام کند
- Accepted docs را خاموش بازنویسی نکنید

جزئیات: [`../quality/07-agent-review-and-task-acceptance.md`](../quality/07-agent-review-and-task-acceptance.md)

---

## 11. CI Direction (Not Implementation)

CI باید در آینده دروازه‌های تکرارپذیر را enforce کند. پیاده‌سازی YAML/ابزار در این Task نیست (P01+).

---

## 12. Anti-Patterns

- Build PASS = Task PASS  
- تست نزده ولی PASS  
- rerun flaky تا سبز  
- InMemory/SQLite به‌عنوان اثبات PostgreSQL  
- EnsureCreated به‌عنوان تست migration  
- architecture tests جایگزین architect review  
- پوشش ۱۰۰٪ به‌عنوان معیار جهانی  
- snapshot عظیم بدون تفسیر  
- فقط E2E برای همه چیز  
- mock بی‌معنا  
- داده/راز واقعی در fixture  
- وابستگی CI به شبکهٔ production provider  
- پنهان کردن warningها  
- بلعیدن استثنا  
- لاگ راز  
- نقض معماری چون «تست‌ها سبزند»  
- frontend بدون RTL/mobile وقتی لازم است  
- SEO به‌خاطر syntax JSON-LD  
- migration فقط چون compile می‌شود  
- Cursor خود-پذیرش ADR  
- force-push عادی به main  
- working tree کثیف نادیده  

---

## 13. Intentionally Deferred

Exact unit/architecture/assertion/mock/testcontainers/E2E/browser/a11y/visual/security/coverage/formatter/analyzer packages · CI YAML · branch protection · test folder layout · CI parallelization · benchmark/load tools · secret-scan vendor · dependency bot · SAST/DAST products · TreatWarningsAsErrors rollout · coverage thresholds · mutation testing requirement

این‌ها متعلق به P01/P02/hardening هستند — نه این Task.
