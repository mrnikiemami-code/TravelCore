# Definition of Done — TravelCore

منبع: [`../architecture/14-engineering-quality-constitution.md`](../architecture/14-engineering-quality-constitution.md)

---

## 1. Core Rule

یک Task وقتی **Done** است که همهٔ دروازه‌های **کاربردی** آن **PASS** (یا معتبرانه **NOT_APPLICABLE**) باشند و هیچ دروازهٔ لازم **FAIL** یا **BLOCKED** نمانده باشد.

`dotnet build` / `npm run build` به‌تنهایی Done نیست.

---

## 2. Gate States

| State | معنی | مثال |
|-------|------|------|
| PASS | اجرا + شواهد موفقیت | `dotnet build` exit 0 |
| FAIL | اجرا شد و شکست خورد / الزام نقض شد | architecture test failed |
| BLOCKED | لازم است ولی محیط مانع است | PostgreSQL test env unavailable |
| NOT_APPLICABLE | با دلیل معتبر لازم نیست | E2E روی docs-only task |

گزارش **PASS** برای دروازهٔ اجرا‌نشده ممنوع است.

---

## 3. Conceptual Gate Families

| ID | Gate family |
|----|-------------|
| A | Scope correctness |
| B | Architecture correctness |
| C | Build correctness |
| D | Behavioral correctness |
| E | Test correctness |
| F | Persistence / migration (when applicable) |
| G | Security (when applicable) |
| H | UI / a11y / responsive (when applicable) |
| I | i18n / RTL / bidi (when applicable) |
| J | SEO (when applicable) |
| K | Documentation / state correctness |
| L | Git / repository integrity |

---

## 4. Evidence Requirements

| Gate type | Evidence examples |
|-----------|-------------------|
| Build | command + exit code |
| Tests | suite identity + counts + result |
| Architecture | named assertions / tool output |
| Migration | apply result on target DB |
| Frontend | viewport × locale × state matrix notes |
| Security | checklist + scan/review notes as available |

Vague «looks good» alone is insufficient for critical gates.

---

## 5. Risk Levels

| Level | Meaning | Verification depth |
|-------|---------|-------------------|
| LOW | low engineering-failure impact | lighter validation OK |
| MEDIUM | ordinary feature/change | normal automated tests + review |
| HIGH | migration, authz, pricing, booking, SEO routes, cross-module contracts | deeper verification; likely independent review |
| CRITICAL | payment settlement, production data migration, credential architecture, destructive large schema, financial money semantics | high-scrutiny + recovery planning |

Risk ≠ product priority. A low-priority feature may still be CRITICAL technically.

### Examples

- **LOW:** docs typo, non-semantic copy, tiny refactor with existing tests  
- **MEDIUM:** ordinary API endpoint, reusable UI component, simple command/query  
- **HIGH:** DB migration, authorization, pricing calc, booking transition, SEO route change  
- **CRITICAL:** payment, prod data migration, security architecture, money rules, large destructive schema  

---

## 6. Task-Type Definition of Done

### Documentation-only architecture task

Scope · architecture respect · required docs · state update · no unintended source changes · clean diff · commit/push when required · architect review when task says so.  
Frontend E2E typically **NOT_APPLICABLE**.

### Backend feature

Scope · architecture · build · unit/application tests · integration/DB as relevant · security · failure paths · observability where relevant · docs · git integrity.

### Frontend feature

Scope · build · lint · component/interaction · responsive/mobile · RTL/LTR/bidi · a11y · i18n · SEO if public/indexable · performance risk · loading/empty/error states · git integrity.

### Database change

Migration created · reviewed · clean DB apply · upgrade path where relevant · constraints/indexes · data risk · rollback/forward considered · integration tests · no cross-module ownership violation.

### External integration

Adapter isolation · fixtures · timeout/cancel · retry where applicable · idempotency · error mapping · secrets · observability · sandbox when available · degradation.

### Architecture change

Architect decision · ADR when meaningful · docs update · compatibility impact · implementation after decision · enforceable tests where practical.  
**Cursor cannot self-accept architectural change.**

---

## 7. Conceptual Quality Matrix

Values: **R** = Required · **C** = Conditional · **N** = N/A  

| Task Type | Build | Unit | Integration | Architecture | DB/Migration | Frontend | A11y | RTL/Bidi | SEO | Security |
|-----------|-------|------|-------------|--------------|--------------|----------|------|----------|-----|----------|
| Documentation | C | N | N | C | N | N | N | N | N | C |
| Backend Feature | R | R | C | R | C | N | N | N | N | C |
| Frontend Feature | R | C | C | C | N | R | C | C | C | C |
| Database Change | R | C | R | R | R | N | N | N | N | C |
| Architecture Change | C | C | C | R | C | C | C | C | C | C |
| External Integration | R | R | C | R | C | N | N | N | N | R |

Not a rigid universal checklist — tailor per Task prompt.

---

## 8. Practical Examples

1. Build PASS + architecture test FAIL → **Task FAIL**  
2. Build PASS + required migration never applied → **NOT PASS**  
3. Required DB gate cannot run (no PostgreSQL env) → **BLOCKED** (not PASS)  
4. Docs-only → Frontend E2E **NOT_APPLICABLE**  
5. Foreign Tour build PASS but RTL broken → **FAIL**  
6. `/en` silently shows Persian content → **FAIL**  
7. JSON-LD syntax valid + fake converted price → **FAIL**  
8. Migration applies on clean DB but destroys production-compatible data → **NOT sufficient**  
9. Flaky test green on 3rd rerun → quality concern; not automatic PASS  
10. Optional Destination section fails + tested graceful degradation → may **PASS**  

---

## 9. Quality Debt

Temporary acceptance of debt requires: description · reason · risk · remediation ownership/task. Vague «TODO later» is insufficient.

## 10. Warnings Direction

New avoidable warnings from the Task normally block acceptance. Legacy warnings: report, distinguish, track. Do not hide warnings globally. TreatWarningsAsErrors rollout deferred to P01/P02 — not mandated globally here.
