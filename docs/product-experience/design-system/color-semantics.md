# Design System 2.0 — Color Semantics

| Field | Value |
|-------|--------|
| Parent | [`../DESIGN-SYSTEM-2.0.md`](../DESIGN-SYSTEM-2.0.md) |
| Status | Foundation · `TC-P30-T003` |
| North Star | [`../assets/travelcore-ui-ux-north-star.png`](../assets/travelcore-ui-ux-north-star.png) |

## Brand candidates (North Star–aligned)

| Token | Candidate hex | Notes |
|-------|---------------|-------|
| `color.brand.primary` | `#0D47A1` | Deep Ocean — primary brand |
| `color.brand.primary-strong` | `#1565C0` / `#1E88E5` | Interactive emphasis |
| `color.brand.accent` | `#F9A825` | Warm gold CTA / highlight |

These are **proposals**, not irreversible locks until Visual Checkpoint A and later token wiring tasks.

## Surface / text / border

| Token | Candidate | Role |
|-------|-----------|------|
| `color.surface.default` | `#FFFFFF` / warm white | Default page |
| `color.surface.muted` | `#F8FAFC` | Soft bands / panels |
| `color.surface.inverse` | deep navy | Dark intentional surfaces |
| `color.text.primary` | `#0F172A` | Primary copy |
| `color.text.secondary` | `#475569` | Secondary copy |
| `color.text.on-brand` | `#FFFFFF` | Text on primary |
| `color.border.default` | `#E2E8F0` | Default borders |
| `color.border.strong` | deeper slate | Emphasis borders |

## Action / state

| Token family | Rule |
|--------------|------|
| `color.action.primary` | Brand primary or accent for CTA hierarchy — one clear primary CTA per view |
| `color.action.secondary` | Quiet supporting action |
| `color.action.destructive` | Danger semantic only |
| `color.state.success` | Success only |
| `color.state.warning` | Warning only |
| `color.state.danger` | Error / destructive |
| `color.state.info` | Informational |

## Rules

1. Do not communicate state by color alone
2. Dark theme = intentional deep navy — not inverted light
3. Public may use richer accent; Admin stays calmer
4. Agency uses Public trust cues + Admin operational clarity
5. Fake commerce density via random colored chips is forbidden

## Forbidden

- Competing brand blues invented per page
- Rainbow status systems without semantic meaning
- Low-contrast gray-on-gray primary text
