# TravelCore Product Experience Constitution

| Field | Value |
|-------|--------|
| Document | `docs/product-experience/TRAVELCORE-PRODUCT-EXPERIENCE-CONSTITUTION.md` |
| Phase lock | P30 — Product Experience Foundation |
| Status | **LOCKED** by `TC-P30-T002` |
| North Star | [`assets/travelcore-ui-ux-north-star.png`](assets/travelcore-ui-ux-north-star.png) |
| Related | UI Constitution · ADR 0005/0006 · P30 plan |
| Product code | **NO** in this document |

This is the **product-experience Source of Truth** for TravelCore visual and UX direction before Design System 2.0 (`TC-P30-T003`) and page implementation.

It **extends** (does not replace) `docs/architecture/10-ui-constitution.md` and related UI docs.

---

## 1. Core principle

TravelCore must never accept a **technically correct but commercially weak** product surface as complete.

Automated tests are **necessary** but **not sufficient** for major UI tasks.

Major P30 surfaces require:

```text
Technical Gate
+
Visual / Product Gate
```

### No Page-First Development

Major pages must consume an approved shared Design System.

Pages must **not** independently invent:

- colors · spacing · typography
- card / form / navigation / status patterns
- responsive behavior · data-grid behavior

### One Design System · Three Experiences

1. **Public Marketplace**
2. **Admin Console**
3. **Agency Portal**

They share visual DNA, tokens, accessibility, interaction standards, and component philosophy — with different information architecture and workflow emphasis.

---

## 2. Product identity

### Desired feeling

Premium · Modern · Trustworthy · Travel-first · Visual · Calm but rich · Conversion-oriented · Content-rich without clutter · Operationally serious where appropriate · Mobile-first · Accessible · SEO-compatible

### Must NOT feel like

- a developer demo
- a framework starter page
- a generic SaaS template
- an old travel-agency website
- a plain CRUD admin panel
- a Bootstrap-like internal tool
- a collection of unrelated cards
- a database schema rendered as forms
- a text-heavy empty travel website

### Organizing principle (design — not marketing copy unless later approved)

```text
Discover + Trust + Book
```

---

## 3. Competitive benchmark policy

| Surface | References (analysis only) |
|---------|----------------------------|
| Public (Iran) | LastSecond · Tahagasht |
| Public (global maturity) | Booking · Airbnb · Tripadvisor |
| Admin | Stripe Dashboard · Shopify Admin · Linear · Vercel Dashboard |
| Agency / B2B | Professional travel-commerce / agency operational systems |

### Forbidden

- NO PIXEL CLONING
- NO BRAND COPYING
- NO LOGO COPYING
- NO COMPETITOR COLOR-SYSTEM COPYING
- NO TRADE-DRESS COPYING

### Output

**TravelCore Design Language** — not a copy of another website.

---

## 4. Approved Visual North Star

```text
docs/product-experience/assets/travelcore-ui-ux-north-star.png
```

| Rule | Value |
|------|--------|
| Role | DIRECTIONAL visual-quality target |
| Pixel-perfect | NO |
| May improve upon | YES |
| Material regression below | FORBIDDEN |
| Fake domain data | NOT authorized |
| Competitor cloning | NOT authorized |
| Replace without approval | FORBIDDEN |

Regression dimensions include: visual hierarchy · richness · professionalism · travel feeling · imagery · conversion clarity · information organization · perceived quality · responsive quality · product maturity.

---

## 5. Visual language (direction — not final tokens)

Exact hex/token implementation belongs to **`TC-P30-T003`** unless already accepted tokens exist.

### Direction from North Star

| Family | Direction |
|--------|-----------|
| Primary | Deep Ocean / trustworthy deep blue |
| Accent | Warm gold / sunset / warm travel-energy |
| Surfaces | Warm white / calm neutrals |
| Dark surfaces | Deep neutral/navy (not pure black by default) |

### Lock

restrained color · semantic use of color · strong contrast · premium imagery · purposeful whitespace · coherent spacing rhythm · modern rounded geometry · restrained shadows/elevation · low visual noise · consistent icon language · clear hierarchy

### Avoid

excessive gradients · glassmorphism · neon UI · random colors · excessive shadows · tiny dense cards everywhere · ornamental animation · low-contrast fake luxury

---

## 6. Typography principles

- Excellent Persian readability
- Excellent Arabic readability
- Correct Latin rendering
- Strong numeral / price readability
- Clear title/body distinction
- Scan-friendly cards
- Accessible line-height
- Responsive typography

No single page may invent its own typography system.

Technical values (PNR · flight number · currency code · UUID · email · phone · identifiers) must remain **bidi-safe** and visually stable.

---

## 7. Image-first travel product

Travel is a **visual product**.

Primary public surfaces must be image-first **where data exists**:

Home · Destination · Hotel Listing · Hotel Detail · Tour Listing · Tour Detail · Travelogue / Travel Story

When real imagery is unavailable:

- use approved design-system placeholder
- preserve layout quality
- never fabricate a false hotel/tour image as factual

DEMOFEED may later provide identified demo/source data. P30 does **not** execute DEMOFEED.

---

## 8. Fake UI / fake data policy (hard rule)

Cursor must **not** improve appearance by inventing factual product data.

Forbidden unless clearly identified as explicit demo fixture in an authorized demo-data task:

fake prices · ratings · reviews · bookings · scarcity · availability · flight schedules · room inventory · discount percentages · revenue · agency commission · real customer names · trust claims

Screenshots for acceptance must disclose placeholder/demo content when used.

---

## 9. Experience → Data → Commercial

```text
EXPERIENCE
    ↓
DATA
    ↓
COMMERCIAL EXPANSION
```

DEMOFEED: plan authored · **DEFERRED** until approved P30 experience foundation.

Do not reverse this sequence without architect decision.

---

## 10. Cross-cutting locks

| Area | Lock |
|------|------|
| Locales / direction | FA=RTL · AR=RTL · EN=LTR · direction-neutral components · logical CSS |
| Theme | Light + Dark · token-based · dark is intentional (not invert) |
| States | Loaded · Loading · Empty · Error · Partial Data for major shared components |
| Accessibility | keyboard · visible focus · landmarks · heading order · labels · contrast · reduced-motion · no color-only state |
| SEO | semantic hierarchy · crawlable content · internal links · image semantics · SEO module ownership preserved |
| Performance | Server Component First · controlled client islands · no known severe regression |
| Motion | subtle · purposeful · fast · respectful |

Empty data must **not** make the site look broken or unfinished.

---

## 11. Companion specs

| Spec | Path |
|------|------|
| Public | [`P30-PUBLIC-EXPERIENCE-SPEC.md`](P30-PUBLIC-EXPERIENCE-SPEC.md) |
| Admin | [`P30-ADMIN-EXPERIENCE-SPEC.md`](P30-ADMIN-EXPERIENCE-SPEC.md) |
| Agency | [`P30-AGENCY-EXPERIENCE-SPEC.md`](P30-AGENCY-EXPERIENCE-SPEC.md) |
| Visual acceptance | [`P30-VISUAL-ACCEPTANCE-CHECKLIST.md`](P30-VISUAL-ACCEPTANCE-CHECKLIST.md) |

---

## 12. P30 Gate intent (locked now)

| Surface | Acceptance feeling |
|---------|-------------------|
| Public | «این سایت گردشگری حرفه‌ای است.» |
| Admin | «این سیستم قابل استفاده عملیاتی است.» |
| Agency | «این ابزار فروش است.» |

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-20 | Initial lock · `TC-P30-T002` |
