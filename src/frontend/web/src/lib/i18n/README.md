# `lib/i18n`

Locale registry and helpers for **routing / document `lang` + `dir`**.

## T002 scope

- Supported public locales: `fa` · `en` · `ar` (BCP 47 codes)
- Default product locale (root entry): `fa`
- HTML `lang` / `dir` derived server-side from the URL `[locale]` segment

## Explicit non-goals (later tasks)

- Translation catalogs / CMS publication
- Currency, calendar, or timezone inference from locale
- Bidirectional UI primitives (T004)
- Full SEO / hreflang / negotiation engine (P05 / T006 SEO track)

## Concept separation

```text
Locale ≠ Currency ≠ Calendar ≠ TimeZone
```
