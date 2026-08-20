# P30 T005 — Public Home Experience Foundation — Evidence

| Field | Value |
|-------|--------|
| Task | `TC-P30-T005` |
| Status | Cursor PASS · AWAITING_ARCHITECT_REVIEW |
| Routes | `/fa` · `/en` (also `/ar`) |
| Shell chrome | `PublicShell` + `PublicHeader` + `PublicFooter` (T004) |
| Commit message | `feat(ui): add P30 public home experience foundation` |

## What to review

1. **Hero** — Deep Ocean / Warm Gold · strong CTA to Tours / Hotels · no fake booking claims
2. **Discovery entry** — Tours · Hotels · Plan · Flights · Travelogues · Visa (existing routes only)
3. **Destinations band** — honest nav intents; primary = fixture destination sample (no invented index)
4. **Tours band** — catalog entry CTA (no fake tour rows)
5. **Hotels band** — real composition cards when present; premium empty state when not
6. **Trust** — static capability copy only (no invented ratings / partner logos)
7. **Stories** — real travelogues when present; empty state when not
8. **Conversion CTA** → Plan
9. **Footer** via PublicShell

## Honesty rules applied

- Prefer empty / omit over inventing commerce facts
- No invented prices, availability, discounts, ratings, review counts
- Destinations primary link uses existing `/destinations/fixture-istanbul` only

## Validation

```text
cd src/frontend/web && npm run typecheck   # PASS
```

Manual: open `/fa` (desktop + mobile width) — confirm section order and chrome.

## Out of scope

- DEMOFEED / live booking / payments
- Full destinations index page
- T006+ Hotel/Tour experience depth
- Invented inventory

## Architect note

Cursor PASS ≠ Architect ACCEPT. Live `/fa` visual review vs North Star required.
