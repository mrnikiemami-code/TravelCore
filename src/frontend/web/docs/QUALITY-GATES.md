# P02 Frontend Quality Gates (TC-P02-T016)

Single command for foundation regression detection:

```bash
npm run quality
```

This runs, in order:

1. `npm run lint`
2. `npm run typecheck`
3. `npm run build`
4. `npm run test:quality`
   - Node built-in tests for money display invariants (`scripts/tests/money.test.ts`)
   - Deterministic P02 checks (`scripts/p02-quality-checks.mjs`)

## What the checks protect

| Gate | Mechanism |
|------|-----------|
| Lint / types / build | existing npm scripts |
| Server Component First | allowlisted `"use client"` only (`BookingCtaIsland`, locale `error.tsx`) |
| Locale tour route | `app/[locale]/tours/[productKey]/page.tsx` + `generateMetadata` |
| Published locales | FA/EN fixtures exist; no fabricated `ar.ts` fixture |
| RTL/LTR document | `[locale]/layout.tsx` sets `lang`/`dir` |
| A11y baseline files | SkipLink + FieldMessage present |
| Remote image safety | no `*` host wildcard in `next.config.ts` |
| Money display | IRR→Toman only when explicit; locale does not pick currency |

No Playwright / browser farm in T016. Manual matrix evidence is T017.
