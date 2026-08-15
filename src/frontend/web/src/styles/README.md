# Design tokens

Source of truth: [`tokens.css`](./tokens.css)  
Tailwind bridge: [`../app/globals.css`](../app/globals.css) (`@theme inline`)

## Categories (semantic / purpose-based)

| Category | Examples |
|----------|----------|
| Color | `background`, `foreground`, `surface`, `surface-muted`, `muted-foreground`, `border`, `primary`, `primary-foreground`, `danger`, `focus`, `ring` |
| Typography | `caption`, `label`, `body`, `title`, `heading`, `display` |
| Spacing | `--tc-space-*` scale; `touch` / `control-*` sizes |
| Radius | `sm`, `md`, `lg`, `full` |
| Border | `--tc-border-width` |
| Elevation | `shadow-sm`, `shadow-md` |
| Container | `narrow`, `content`, `wide` |
| Breakpoint | `sm` 390 · `md` 768 · `lg` 1024 · `xl` 1280 · `2xl` 1440 |
| Z-index | `base` · `sticky` · `dropdown` · `overlay` · `modal` · `toast` |
| Motion | `--tc-duration-*`, `--tc-ease-standard` (+ `prefers-reduced-motion`) |

## Usage (Tailwind)

```tsx
<div className="bg-background text-foreground border border-border rounded-md">
  <h1 className="text-heading font-semibold">Title</h1>
  <p className="text-body text-muted-foreground">Supporting copy</p>
  <button className="bg-primary text-primary-foreground min-h-touch px-4 rounded-md">
    Action
  </button>
</div>
```

## Rules

- Prefer semantic utilities over feature/domain names (`tour-card-blue` forbidden).
- No RTL/LTR token forks; no `--margin-left-*` style abstractions.
- Brand palette deferred — current colors are a **neutral baseline**.
- OS `prefers-color-scheme: dark` remaps the same semantic names only; there is **no** theme switcher.
- Tokens require no Client Component / JS runtime.
