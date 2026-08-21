# TC-P32-T009 — Home Destination Cover Consumption

| Field | Value |
|-------|--------|
| Task-ID | `TC-P32-T009` |
| Status (Cursor) | **PASS** |

## Implementation

- Load Destination Cover via `/media/presentation` in `load-home-discovery-composition.ts`
- `HomeDestinationPreview.coverSrc` when Ready; honest gradient when missing
- Home cards render `<img>` for coverSrc (app-proxy only)

## Evidence

- `fa-home-desktop.png` / `fa-home-mobile.png` — DEMOFEED destination cards with real covers

## Visual self-review

| Dimension | Verdict |
|-----------|---------|
| Destination photos on Home | **PASS** |
| Honest fallback | **PASS** (gradient when no cover) |
| Hardcoded URLs | **PASS** — none |
| RTL / commercial chrome | **PASS** |

## Known limitations

Gallery not in Destination ownership (Option A). Destination landing pages may need separate consume if not yet wired.

## Recommended next

`TC-P32-GATE` (Architect file only).
