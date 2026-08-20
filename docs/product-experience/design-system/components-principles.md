# Design System 2.0 — Component Principles

| Field | Value |
|-------|--------|
| Parent | [`../DESIGN-SYSTEM-2.0.md`](../DESIGN-SYSTEM-2.0.md) |
| Status | Foundation · `TC-P30-T003` |
| Product code | **NO** in T003 |

## Hierarchy

```text
Design Tokens
  → Primitives
  → Composite Components
  → Domain Components
  → Sections
  → Page Archetypes
  → Actual Routes
```

Reuse P02 primitives. **Extend — do not fork.**

## One system · three experiences

| Experience | Emphasis |
|------------|----------|
| Public | Discovery · trust · booking conversion |
| Admin | Density · clarity · operational control |
| Agency | Sales velocity · request queues · partner trust |

Shared: tokens, a11y, interaction standards, component philosophy.  
Different: IA, density, workflow chrome.

## Universal requirements

Every shared interactive component defines:

| State | Required |
|-------|----------|
| Loaded | Happy path |
| Loading | Non-janky pending |
| Empty | Honest empty — not fake rows |
| Error | Recoverable messaging |
| Partial Data | Degrade without lying |

## Lean Public set (later implementation)

- Hero
- Search / discovery controls
- Product / offer cards
- Trust strip
- Footer / legal strip

## Lean Admin set (later)

- App shell / nav
- Data grid + filter bar
- Form patterns
- Modal / drawer
- Stepper / workflow chrome

## Lean Agency set (later)

- Sales dashboard cards
- Booking / request lists
- Action queues
- Partner-facing status patterns

## Commercial rules

1. CTA clarity > decoration
2. Image-first when travel content exists
3. Domain truth > cosmetic fake density
4. No page invents private Card / Button / Table dialects
5. Prefer fewer excellent composites over speculative mega-libraries

## Explicitly out of T003

No React components, no Storybook build, no route wiring.
