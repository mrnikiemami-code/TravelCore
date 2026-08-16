# Image / media presentation foundation

Generic UI imagery for public pages. Media **bytes** are owned by the Media module
(app-proxy delivery, P06-R4); this UI layer only consumes presentation URLs.

## Pieces

| Artifact | Role |
|----------|------|
| `src/types/media-image.ts` | `MediaImagePresentation` contract |
| `src/components/ui/media-image.tsx` | `MediaImage` — `next/image` wrapper |
| `src/lib/media/media-presentation.ts` | App-proxy path helpers → `MediaImagePresentation` |
| `public/media/foundation-sample.png` | Local static smoke asset (P02) |

## Rules

- Server Component only — no `"use client"`
- `priority` is **opt-in** (LCP/critical only)
- Informative `alt` from model; decorative = `""`; never invent from URL
- Fill layout uses `aspect-ratio` + `sizes` to limit CLS / overflow
- Direction-neutral; do not mirror media solely for RTL
- **Never** put StorageKey, filesystem paths, or object-storage hosts in `src`

## Public URL policy (P06-R4 — APP PROXY)

Browser loads Media through TravelCore HTTP delivery:

- Original: `/api/media/assets/{id}/content`
- Variant: `/api/media/assets/{id}/variants/{large|medium|thumbnail}/content`

Use `buildMediaImagePresentation` / `resolveMediaAppProxySrc` rather than hand-rolling URLs.

### `remotePatterns`

- **Same-origin / unset API base:** no `images.remotePatterns` required (default).
- **Split FE/API origin:** set `TRAVELCORE_API_BASE_URL` (or `API_BASE_URL`); `next.config.ts`
  allowlists **only** that hostname with pathname `/api/media/**`.
- **Forbidden:** `**`, storage-provider wildcards, S3/R2/MinIO host allowlists, localhost as a
  production contract.

Static fixtures under `public/` remain valid for non-Media smoke paths.
