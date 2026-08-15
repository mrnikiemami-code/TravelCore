# Accessibility baseline (T006)

Foundation conventions for TravelCore web. Not a full WCAG product claim.

## Semantic HTML

Prefer native elements: `header` · `nav` · `main` · `section` · `footer` · `button` · `a` · `label`.

- Navigation → `<a>` / `Link`
- Action → `<button>`
- Do not invent clickable `div`s

## Landmarks / skip

- Stable main landmark: `id="main-content"` on `<main>`
- `SkipLink` → `#main-content` (visible on focus only)

## Focus / keyboard

- Never remove outlines without `focus-visible` replacement
- Global `:focus-visible` uses `--tc-color-focus` / `ring-focus`
- No positive `tabindex` as layout hack
- No keyboard traps in foundation primitives

## Heading hierarchy

- Heading level = document structure, not font size
- `Text` roles (`heading`/`title`/…) are **visual**; use `as="h1"|h2|…` explicitly for real headings

## Forms (convention for later)

| Concern | Convention |
|---------|------------|
| Label | Programmatic `<label htmlFor>` — placeholder is not a label |
| Help | `FieldMessage tone="help"` + `aria-describedby` |
| Error | `FieldMessage tone="error"` (`role="alert"`) + `aria-invalid` + association |
| Required | Text / `aria-required` — not color alone |
| Invalid | Text + icon/prefix optional — not color alone |

## Errors / live regions

- Prefer associating field errors with the control
- Use `role="alert"` / `role="status"` only when justified — not for every message
- App Router `error.tsx` / loading / not-found → **T007**

## Touch

- Minimum interactive target ≈ `min-h-touch` / `--tc-size-touch-min` (44px)
- Critical actions must not depend on hover alone

## Reduced motion

- Token durations collapse under `prefers-reduced-motion`
- Global CSS also disables decorative transitions under the same media query

## Color

- Do not communicate error/success/selected/required by color alone
- Danger text pairs with explicit wording (`FieldMessage`)

## RTL / LTR

- Same a11y primitives in both directions
- Mixed-direction accessible names: compose with `BidiText` / `LtrValue`
