# Design System 2.0 — Responsive & Accessibility

| Field | Value |
|-------|--------|
| Parent | [`../DESIGN-SYSTEM-2.0.md`](../DESIGN-SYSTEM-2.0.md) |
| Status | Foundation · `TC-P30-T003` |
| Related | `docs/ui/02-responsive-mobile-architecture.md` · `docs/ui/03-rtl-ltr-bidi.md` · `docs/ui/05-accessibility-and-interaction.md` |

## Responsive

1. **Mobile-first intentional design** — not desktop layouts shrunk
2. Major surfaces reviewed independently for Desktop / Tablet / Mobile
3. Touch targets intentional on public and agency mobile flows
4. Admin dense grids may transform to card/list operational views on small screens
5. Breakpoint tokens are shared; experience layouts may differ

## Accessibility baseline

Preserve and strengthen existing standards:

| Requirement | Rule |
|-------------|------|
| Keyboard | All interactive paths operable |
| Focus | Visible focus rings |
| Structure | Semantic landmarks / headings |
| Forms | Labels + associated errors |
| Contrast | Sufficient text/UI contrast |
| Motion | Respect reduced-motion |
| State | Never color-only |
| Bidi | Technical values as LTR islands |

## Quality direction

Public major surfaces: Accessibility ≥ 95 where Lighthouse applies — **without gaming Lighthouse**.

## Checkpoint

Responsive + a11y boards are part of Visual Checkpoint A before page-first shells.
