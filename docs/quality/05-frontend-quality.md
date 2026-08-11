# Frontend Quality — TravelCore

منبع: [`../architecture/14-engineering-quality-constitution.md`](../architecture/14-engineering-quality-constitution.md)  
UI / i18n / SEO / pages: accepted constitutions and [`../pages/`](../pages/)

---

## 1. Build / Lint Gate

Frontend implementation tasks normally require applicable:

`npm run lint` · `npm run build` · future test scripts once established.

Do not claim frontend PASS with remaining TypeScript/build/lint failures. Exact scripts remain implementation-dependent.

---

## 2. Component & Interaction

Validate important presentation: states · semantics · user-visible conditions · a11y interaction.  
Do not test every Tailwind class.

Interactive controls: user-observable behavior (pickers, filters, sheets, booking initiation).

---

## 3. Responsive / Mobile-First

Representative widths (accepted UI): **360 · 390 · 768 · 1024 · 1280 · 1440**.

Risk-based: not every tiny component needs every width screenshot.

Mobile-ready ≠ desktop shrunk. Validate: content priority · touch · sticky · bottom sheets · keyboard · safe areas · tables · filters · text expansion.

---

## 4. Accessibility

Public/frontend features validate applicable: semantic HTML · keyboard · focus visibility · labels · error association · headings · landmarks · contrast where design exists · reduced motion · touch targets.

**Automation limit:** automated checkers are incomplete. Passing axe-like tools ≠ proven accessibility. High-risk UIs may need keyboard/manual/semantic review.

---

## 5. RTL / LTR

Significant frontend tasks verify directionality where applicable.

Mature page validation includes:

- FA + RTL  
- EN + LTR  
- AR + RTL as Arabic matures  

Do not infer correctness from Persian alone. RTL ≠ blind mirroring (maps, travel direction, media).

---

## 6. Bidi

Test realistic mixed values in RTL context: `IKA` · `IST` · `TK875` · `EK978` · `USD` · email · phone · booking reference · passport numbers.

---

## 7. i18n

Verify: correct locale · `lang` · `dir` · no fake cross-language public fallback · translation publication semantics · identifier preservation · locale-aware formatting.

English route silently showing Persian published content → **FAIL**.

---

## 8. SEO

SEO-sensitive public tasks verify applicable: canonical · index/noindex · localized route integrity · hreflang eligibility · server-rendered critical content · sitemap eligibility · structured-data **truthfulness** · 404/410 · filter/search indexation policy.

Schema validation ≠ truthful data. Fabricated converted price in valid Offer JSON-LD → **FAIL**.

Server Component first / critical SEO content server-renderable remains authoritative.

---

## 9. Performance

Risk-based: hydration size · client JS · LCP · INP · CLS · images · third-party scripts · SSR.

Do not block every early task on production Lighthouse scores before real pages exist. Apply by maturity/risk. Obvious severe regressions may block acceptance.

---

## 10. Page States

Frontend quality includes loading · empty · error · unavailable/expired · partial degradation per page archetype contracts. Optional section failure with correct degradation may PASS if required and tested.

---

## 11. Page Archetype Candidate (Foreign Tour)

Eventually validate Desktop · Mobile · FA RTL · EN LTR with IKA/IST/TK875/USD/mixed currencies and states: active · unavailable · expired · partial failure.

---

## 12. Anti-Patterns

- compressed desktop as mobile  
- hover-only critical actions  
- client-only critical SEO content  
- skipping RTL for affected public UI  
- a11y treated as optional  
- SEO syntax over semantics  

## 13. Deferred

Exact E2E/browser/a11y/visual tools · Lighthouse enforcement thresholds · component test runner choice.
