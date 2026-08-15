# Image / media presentation foundation (T011)

Generic UI imagery for public pages — **not** Media module ownership.

## Pieces

| Artifact | Role |
|----------|------|
| `src/types/media-image.ts` | `MediaImagePresentation` contract |
| `src/components/ui/media-image.tsx` | `MediaImage` — `next/image` wrapper |
| `public/media/foundation-sample.png` | Local static smoke asset |

## Rules

- Server Component only — no `"use client"`
- `priority` is **opt-in** (LCP/critical only)
- Informative `alt` from model; decorative = `""`; never invent from URL
- Fill layout uses `aspect-ratio` + `sizes` to limit CLS / overflow
- Direction-neutral; do not mirror media solely for RTL
- Remote hosts: **not configured** until Media/storage allowlist is architecture-approved (no arbitrary wildcards)

## Remote policy (current)

`next.config.ts` has **no** `images.remotePatterns`.  
Local/static assets under `public/` are the T011 proof path.  
Future S3-compatible hosts require an explicit, narrow allowlist task — do not invent hosts here.
