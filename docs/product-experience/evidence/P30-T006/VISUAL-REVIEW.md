# P30-T006 Visual Review Notes

| Field | Value |
|-------|--------|
| Task-ID | `TC-P30-T006` (+ review/fix `TC-P30-T006-VISUAL-ARCHITECT-REVIEW-FIX-001`) |
| Surfaces | `/[locale]/hotels` listing · `/[locale]/hotels/[slug]` detail |
| Evidence | `fa-hotels-desktop.png` · `fa-hotels-mobile.png` · `fa-hotel-detail-notfound-desktop.png` · `fa-hotel-detail-notfound-mobile.png` |

## Previous T006 completeness

Code for listing + detail commerce experience **was present** after `4db35b5`, but visual review found unfinished product polish:

| Issue | Assessment |
|-------|------------|
| Evidence only captured API-down listing | Incomplete for Architect visual ACCEPT of cards/detail |
| User-facing copy too engineering-oriented (Place / availability jargon) | Fixed in this review task |
| Primary CTAs weak vs North Star (Apply / View hotel / booking bar) | Fixed — primary / accent |
| Missing hotel detail fell to bare 404 without PublicShell | Fixed — shell + honest missing state |
| Live catalog / gallery / card grid with Place API | **Still blocked** in this local session (API unavailable) |

## Visual self-review (this run)

### North Star direction
- PublicShell chrome (logo, nav, accent Workspace, primary actions) aligns with commercial marketplace direction.
- Hotel listing uses content-width container, card/filter surfaces, primary Apply / retry CTAs.
- Not pixel-clone of North Star hotel detail (live gallery/map/price widget need real Place data).

### Product feeling
- Improved: clearer guest-facing Persian copy; stronger CTAs; soft brand gradients on card placeholders.
- Still limited: listing shows **honest load-failure** because Place public API is down — not a fake success catalog.

### Layout
- Desktop listing: header + filter toolbar + error panel + footer — acceptable commerce shell.
- Mobile listing: stacked filter controls + error + footer — acceptable; primary nav behind Menu (shell pattern).
- Missing-hotel detail: now rendered inside PublicShell (re-captured after fix).

### Responsive
- Desktop 1440 and mobile 390 captures refreshed after polish.

## Known limitations

1. Place API unavailable locally → no live hotel cards / gallery / facilities evidence in this run.
2. No invented prices, availability, ratings, or demo hotels.
3. Sticky booking CTA on success detail is presentation entry only (not payment).
4. Next.js dev indicator may appear in local screenshots (not product UI).

## Acceptance risks

1. Architect may **REWORK** until live Place-backed listing + detail screenshots exist.
2. Card/detail commercial density vs North Star (photo gallery, map, price panel) cannot be fully judged without API.
3. PROJECT-STATE / ROADMAP still lag behind T006 (docs SoT update not in this task scope).

## Architect gate

Visual ACCEPT still required (Cursor PASS ≠ Architect ACCEPT).
