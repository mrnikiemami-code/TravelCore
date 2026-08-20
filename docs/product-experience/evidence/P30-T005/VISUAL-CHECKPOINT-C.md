# P30-T005 — Visual Checkpoint C Evidence

| Field | Value |
|-------|--------|
| Task | `TC-P30-T005-VISUAL-CHECKPOINT-C` |
| Related implementation | `TC-P30-T005` (`d176045`) |
| Status (Cursor) | **REWORK_RECOMMENDED** |
| Route under review | `/fa` (primary) · `/en` · `/ar` (direction/layout only) |
| Dev server | `npm run dev` · `http://localhost:3000` |
| Capture method | Playwright Chromium screenshots |

## Screenshot paths

| Surface | Path |
|---------|------|
| FA Desktop 1440×900 | [`fa-home-desktop.png`](fa-home-desktop.png) |
| FA Mobile 390×844 | [`fa-home-mobile.png`](fa-home-mobile.png) |
| EN Desktop (LTR sanity) | [`en-home-desktop.png`](en-home-desktop.png) |
| AR Desktop (RTL sanity) | [`ar-home-desktop.png`](ar-home-desktop.png) |

## What is present (honest)

1. Public shell chrome (header / nav / footer from T004)
2. Deep Ocean hero + Warm Gold CTAs
3. Discovery entry cards (Tour / Hotel / Plan / Flight / Travelogue / Visa)
4. Destinations band as **text navigation intents** (not destination inventory cards)
5. Tours band as catalog CTA (no tour cards)
6. Hotels / Stories empty states (no invented inventory)
7. Trust band = capability honesty copy
8. Conversion CTA → Plan

## Observed visual defects (do not hide)

Versus North Star (`docs/product-experience/assets/travelcore-ui-ux-north-star.png`) and commercial peers:

1. **No travel imagery** in hero — solid/gradient blue only; North Star uses large destination photography
2. **No travel search widget** in hero — North Star has Flight/Hotel/Tour search prominence
3. **Destinations** are plain text buttons — not image cards with destination names / visual richness
4. **Tours** have no package cards, badges, itinerary cues, or pricing presentation
5. **Hotels** empty — no browse grid; empty state is honest but commercially thin
6. **Stories** empty — no inspiration photography/cards
7. **Trust** is abstract text, not commercial trust strip with icons/support cues from North Star
8. Overall feel is closer to a **structured foundation / marketing skeleton** than a **premium travel marketplace**
9. Density is low; large empty regions after empty data sections
10. Header “میز کار” CTA is product-shell oriented, not customer-booking oriented

Honesty note: empty hotels/stories comply with no-fake-commerce rules — correct for truth — but visual marketplace feeling still fails Checkpoint C.

## Checklist dimensions (quick)

| Dimension | Verdict |
|-----------|---------|
| Product feel (marketplace) | **FAIL** |
| Visual hierarchy | PARTIAL (hero CTA clear; below-fold sparse) |
| Travel imagery | **FAIL** |
| Composition | PARTIAL (sections exist; cards not commerce-grade) |
| Typography | PASS (readable FA) |
| Spacing / density | FAIL vs North Star richness |
| Conversion clarity | PARTIAL (CTAs exist; search missing) |
| Trust | PARTIAL (honest; not commercially convincing) |
| Responsive / mobile | PASS (usable; still sparse) |
| RTL FA / AR | PASS (`dir=rtl`) |
| LTR EN | PASS (`dir=ltr`, English hero present) |
| Domain truth | PASS (no fabricated prices/ratings) |
| North Star regression | **YES — materially below North Star commercial quality** |

## Gate summary

| Gate | Result |
|------|--------|
| Technical (prior T005) | PASS |
| Public marketplace feeling | **FAIL** |
| Mobile usability | **PASS** |
| RTL quality | **PASS** |
| Visual Checkpoint C | **REWORK_RECOMMENDED** |

## Recommended architect decision

**Do not ACCEPT `TC-P30-T005` yet.**  
Authorize a **T005 visual rework** (hero imagery/search prominence, destination/tour presentation richness, empty-state premiumization) **before** `TC-P30-T006`.

Do **not** execute DEMOFEED solely to fill empty sections unless architect authorizes it; prefer real catalog composition or designed empty that still feels premium.
