# TravelCore Web Frontend

Next.js App Router frontend for TravelCore (`src/frontend/web`).

Authoritative product architecture lives in repository docs (`AGENTS.md`, `docs/architecture/*`, `docs/plans/P02-frontend-foundation-walking-skeleton.md`). This README is the **local convention map** for P02+.

## Commands

```bash
npm run dev
npm run lint
npm run typecheck
npm run build
npm run test:quality
npm run quality
```

`npm run quality` is the **P02 quality gate** (lint + typecheck + build + deterministic checks). See [`docs/QUALITY-GATES.md`](docs/QUALITY-GATES.md).
## Physical structure

```text
src/
  app/                 App Router entry — locale-prefixed public routes under `app/[locale]/` (ADR 0007)
  components/ui/       Shared direction-neutral UI primitives (no business logic)
  features/            Page/feature composition (workflow-oriented; not domain silos)
  lib/
    api/               Frontend application/API access boundary (no DB/persistence)
    i18n/              Locale/i18n infrastructure location (behavior comes in later tasks)
    formatting/        Presentation helpers (money/date display later; no authoritative calc)
  types/               Shared frontend contracts / view-model shapes (not backend entities)
```

### Responsibility rules

1. **Server Components by default.** `"use client"` only for real browser interaction/runtime needs.
2. Do not put `"use client"` on shared/root foundations by convenience.
3. No authoritative business logic in UI components.
4. No direct database / EF / module DbContext / persistence access from frontend.
5. Backend bounded contexts do **not** automatically become frontend folders, menus, or screens.
6. Feature composition may span multiple domains through explicit application/API contracts while backend ownership stays separate.
7. Shared code must be genuinely reusable — `components/ui` is not a dumping ground.
8. Keep page-specific composition under `features/`, reusable primitives under `components/ui`.
9. Prefer direction-neutral naming and logical layout assumptions (RTL is not a secondary branch).
10. Do not treat raw backend entities as page view models by default.
11. No global mutable client state infrastructure in early foundation tasks.
12. Import alias: `@/*` → `./src/*` (see `tsconfig.json`).

### Explicitly out of scope for T001

Locale routing/`lang`/`dir` behavior · design tokens · bidi primitives · money components · shells · API clients · Foreign Tour Detail · Admin navigation IA.

## Import hygiene

- Prefer `@/` imports for app-local modules.
- Keep dependency direction: `app` / `features` → `components` / `lib` / `types` (not the reverse into routes).
- Avoid circular imports between `features` and `components`.
