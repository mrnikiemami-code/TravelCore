# TravelCore Design System 2.0 Foundation

| Field | Value |
|-------|--------|
| Document | `docs/product-experience/DESIGN-SYSTEM-2.0.md` |
| Status | **FOUNDATION LOCKED** by `TC-P30-T003` |
| Extends | Constitution · UI Constitution · `docs/ui/01-design-system-architecture.md` |
| North Star | [`assets/travelcore-ui-ux-north-star.png`](assets/travelcore-ui-ux-north-star.png) |
| Product pages | **NOT built in T003** |
| Visual Checkpoint | **A** required before page-first shells (`TC-P30-T004`) |

This is a **lean commercial Design System foundation** — enough to stop page-level inventing, not a heavyweight abstraction factory.

**Commercial priority:** TravelCore must become sellable fast. Prefer clear reusable patterns over speculative component frameworks.

---

## 1. Token philosophy

```text
Semantic intent first → raw values second
```

Rules:

- Tokens express **meaning** (`color.action.primary`), not feature names (`color.tourCardBlue`)
- Public / Admin / Agency share one token core
- Experience-specific aliases allowed; second competing systems forbidden
- Exact candidates below are **North Star–aligned proposals**
- Final token wiring into CSS/Tailwind happens only when later authorized implementation tasks begin
- T003 remains **documentation foundation** (no `src/**` changes)

### Supporting docs

| Topic | Path |
|-------|------|
| Tokens overview | [`design-system/tokens.md`](design-system/tokens.md) |
| Typography | [`design-system/typography.md`](design-system/typography.md) |
| Color semantics | [`design-system/color-semantics.md`](design-system/color-semantics.md) |
| Spacing / radius / elevation | [`design-system/spacing-radius-elevation.md`](design-system/spacing-radius-elevation.md) |
| Component principles | [`design-system/components-principles.md`](design-system/components-principles.md) |
| Responsive + a11y | [`design-system/responsive-a11y.md`](design-system/responsive-a11y.md) |

---

## 2. Color semantic system (candidates)

Aligned with North Star (Deep Ocean + Warm Gold + calm neutrals):

| Token | Candidate | Role |
|-------|-----------|------|
| `color.brand.primary` | `#0D47A1` | Primary brand / deep ocean |
| `color.brand.primary-strong` | `#1565C0` / `#1E88E5` | Interactive emphasis |
| `color.brand.accent` | `#F9A825` | Warm travel CTA / highlight |
| `color.surface.default` | `#FFFFFF` / warm white | Page surface |
| `color.surface.muted` | `#F8FAFC` | Soft section background |
| `color.text.primary` | `#0F172A` | Primary readable text |
| `color.text.secondary` | `#475569` | Secondary text |
| `color.border.default` | `#E2E8F0` | Default borders |
| `color.state.success` | green semantic | Success only |
| `color.state.danger` | red semantic | Errors / destructive |
| `color.state.warning` | amber semantic | Warning |
| `color.state.info` | blue semantic | Info |

Dark theme: intentional deep navy surfaces — **not** inverted light theme.

---

## 3. Typography system

Direction (from North Star / Constitution):

- Persian-first readability (Iran Sans family direction unless repo later locks a licensed alternative)
- Strong title/body hierarchy
- Price/numeral legibility
- FA/AR/EN all first-class
- No page invents a private type scale

Roles (minimum):

`display` · `title.lg/md/sm` · `body.lg/md/sm` · `label` · `caption` · `price`

Details: [`design-system/typography.md`](design-system/typography.md)

---

## 4. Spacing · radius · elevation

- Spacing scale: purposeful rhythm (4/8 base), no random gaps
- Radius: modern rounded, consistent by role (control / card / sheet)
- Elevation: restrained shadows; prefer border+surface hierarchy over heavy depth

Details: [`design-system/spacing-radius-elevation.md`](design-system/spacing-radius-elevation.md)

---

## 5. Component design principles

Hierarchy (unchanged from UI Constitution):

```text
Tokens → Primitives → Composite → Domain → Sections → Page Archetypes → Routes
```

Principles:

1. Reuse P02 primitives — **extend, do not fork**
2. One Design System / Three Experiences
3. Experience-specific composites OK; universal Card that fights Admin grids is NOT OK
4. Every shared interactive component defines: Loaded / Loading / Empty / Error / Partial Data
5. CTA clarity over decoration
6. Image-first where travel content exists
7. Domain truth > cosmetic fake density

Public lean set (later implementation): Hero · Search · Product cards · Trust strip · Footer  
Admin lean set (later): Shell · Data grid · Filter bar · Form · Modal/Drawer · Stepper  
Agency lean set (later): Sales dashboard cards · booking/request lists · action queues

Details: [`design-system/components-principles.md`](design-system/components-principles.md)

---

## 6. Responsive rules

- Mobile-first intentional design (not desktop shrinkage)
- Independent Desktop / Tablet / Mobile review for major surfaces
- Touch targets intentional
- Admin grids may transform to card/list operational views on mobile

Details: [`design-system/responsive-a11y.md`](design-system/responsive-a11y.md)

---

## 7. Accessibility baseline

Preserve and strengthen existing a11y docs:

- keyboard navigation · visible focus
- semantic landmarks / headings
- labeled forms · associated errors
- sufficient contrast
- reduced-motion respect
- no color-only state communication
- bidi-safe technical values

P30 public quality direction: Accessibility ≥ 95 where Lighthouse applies — without gaming Lighthouse.

---

## 8. Organizing principle · themes

Organizing principle (design — not marketing copy unless later approved):

```text
Discover + Trust + Book
```

| Theme | Architecture |
|-------|----------------|
| Light | Default calm travel surfaces (warm white / soft muted) |
| Dark | Intentional deep navy — **not** inverted light |

Shared token families serve both themes via semantic roles.

---

## 9. North Star alignment statement

Design System 2.0 **aligns** with:

`docs/product-experience/assets/travelcore-ui-ux-north-star.png`

| Dimension | Alignment |
|-----------|-----------|
| Primary deep blue | YES |
| Accent warm gold | YES |
| Calm light surfaces | YES |
| Card / search / dashboard maturity | YES (directional) |
| Premium travel feeling | YES |
| Pixel clone | NO |
| Fake commerce facts | NO |

Material visual regression below North Star remains forbidden.

---

## 10. What T003 does NOT do

- No real pages / routes
- No production UI code in `src/**`
- No dependency installs
- No backend/API/db changes
- No DEMOFEED
- No T004+ execution

---

## 11. Next gate

After architect ACCEPT of T003:

**Visual Checkpoint A** — representative primitives / component board  
Then authorized `TC-P30-T004` shells only.

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-20 | Initial Design System 2.0 foundation · `TC-P30-T003` |
