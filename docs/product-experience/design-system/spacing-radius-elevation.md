# Design System 2.0 — Spacing · Radius · Elevation

| Field | Value |
|-------|--------|
| Parent | [`../DESIGN-SYSTEM-2.0.md`](../DESIGN-SYSTEM-2.0.md) |
| Status | Foundation · `TC-P30-T003` |

## Spacing

Base rhythm: **4 / 8**.

| Token idea | Intent |
|------------|--------|
| `space.1` … `space.12` | Consistent scale — no random 13px gaps |
| Public | Slightly airier marketing rhythm |
| Admin / Agency | Denser operational rhythm using the **same scale** |

Rules:

- Page sections use shared section padding roles
- Card internal padding is consistent by density mode
- Do not invent one-off spacing for a single route

## Radius

| Role | Direction |
|------|-----------|
| `radius.sm` | Controls / chips |
| `radius.md` | Cards / inputs |
| `radius.lg` | Sheets / hero panels |
| `radius.full` | Avatars / pills only |

Modern rounded — not toy-like excessive rounding on dense Admin grids.

## Elevation

Prefer **border + surface hierarchy** over heavy shadow stacks.

| Role | Direction |
|------|-----------|
| `elevation.none` | Flat |
| `elevation.sm` | Soft card lift |
| `elevation.md` | Floating panels / popovers |
| `elevation.lg` | Modals (restrained) |

Rules:

- Shadows communicate layering, not decoration
- Admin tables stay flatter than Public hero cards
- No drop-shadow arms race between components

## Containers

Shared content max-width roles for Public marketing vs Console shells — exact px values refined at implementation / Checkpoint A.
