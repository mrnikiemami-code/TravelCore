# Design System 2.0 — Tokens Overview

| Field | Value |
|-------|--------|
| Parent | [`../DESIGN-SYSTEM-2.0.md`](../DESIGN-SYSTEM-2.0.md) |
| Status | Foundation · `TC-P30-T003` |
| Implementation | Docs only — no CSS/Tailwind wiring in T003 |

## Intent

Tokens are the single shared vocabulary for Public, Admin, and Agency experiences.

## Categories (required)

| Category | Purpose |
|----------|---------|
| Color | Brand, surface, text, border, action, state |
| Typography | Roles and scale |
| Spacing | Rhythm / density |
| Radius | Control / card / sheet corners |
| Elevation | Depth / shadow roles |
| Container | Content max-widths |
| Breakpoint | Layout review points |
| Z-index | Overlay layers |
| Motion | Duration / easing |
| Control size | Compact / default / comfortable |
| Touch target | Minimum interactive hit area |
| Border | Width / style roles |

## Naming

Prefer:

```text
color.surface.default
color.text.primary
color.action.primary
space.4
radius.md
elevation.sm
```

Avoid feature-coupled names unless unavoidable:

```text
color.hotelCardBlue   ✗
```

## Experience aliases

Allowed:

```text
public.hero.overlay → color.brand.primary (with opacity)
admin.grid.header → color.surface.muted
```

Forbidden: a second independent palette or competing spacing scales.

## Gate

Token candidates may be proposed now. Binding into production styles requires later authorized implementation + Visual Checkpoint A for page-first work.
