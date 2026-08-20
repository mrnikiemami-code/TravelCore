# TC-P30-GATE — Cursor Gate Review

| Field | Value |
|-------|--------|
| Task-ID | `TC-P30-GATE` |
| HEAD | `1a7e070` |
| Status (Cursor) | **PASS** |
| Recommendation | **ACCEPT WITH KNOWN LIMITATIONS** |

## Reviewed surfaces

| Surface | Evidence | Acceptance question | Cursor verdict |
|---------|----------|---------------------|----------------|
| Public Marketplace (Home) | `P30-T005/` rework + checkpoint | «این سایت گردشگری حرفه‌ای است.» | **PASS foundation** — professional commercial direction after rework; still below North Star photo/catalog density |
| Hotel Commerce | `P30-T006/` | marketplace commerce | **PASS foundation** — shell/filters/CTAs/honest error; live catalog blocked by Place API |
| Tour Commerce | `P30-T007/` | marketplace commerce | **PASS foundation** — shell/filters/honest empty; destination-scoped contract; no fake inventory |
| Admin Foundation | `P30-T008/` | «این سیستم قابل استفاده عملیاتی است.» | **PASS** — dense ops console + labeled UI-pattern rows; no fake KPIs |
| Agency Foundation | `P30-T009/` | «این ابزار فروش است.» | **PASS** — sales-tool messaging clear; honest empties; distinct from Admin |

## Evidence completeness

Reviewed existing folders:

- `docs/product-experience/evidence/P30-T005/` (desktop/mobile/rework + EN/AR sanity)
- `docs/product-experience/evidence/P30-T006/` (desktop/mobile listing + missing detail)
- `docs/product-experience/evidence/P30-T007/` (desktop/mobile listing + destination + missing detail)
- `docs/product-experience/evidence/P30-T008/` (operations desktop/mobile + catalog)
- `docs/product-experience/evidence/P30-T009/` (agency desktop/tablet/mobile)

Also checked authority docs:

- `TRAVELCORE-PRODUCT-EXPERIENCE-CONSTITUTION.md`
- `P30-VISUAL-ACCEPTANCE-CHECKLIST.md`
- `assets/travelcore-ui-ux-north-star.png`
- per-task `VISUAL-REVIEW.md` notes

## Checklist dimensions (gate rollup)

| Dimension | Verdict |
|-----------|---------|
| Design consistency (one DS, three experiences) | PASS |
| Mobile readiness | PASS (intentionally composed mobile captures present) |
| RTL/LTR readiness | PASS (FA RTL primary; EN/AR home sanity present) |
| Visual quality / commercial feeling | PASS foundation / PARTIAL vs full North Star richness |
| Architecture boundary preservation | PASS (no fake commerce; experience-only) |
| Evidence completeness for P30 foundation | PASS |

## Known limitations (carry forward)

1. Public still uses gradient/intent composition more than real destination photography + live priced inventory.
2. Hotel/Tour live success grids/details often unavailable locally when Place/Tour APIs are down — honest empty/error evidence only.
3. Tour listing is destination-scoped (no global browse API in this layer).
4. Admin DataGrid server wiring and full module migration remain future work; pattern rows are explicitly labeled.
5. Agency commission/credit/settlement and live sales/booking/customer feeds intentionally absent.
6. Next.js / extension chrome may appear in local screenshots.

## Remaining risks

1. Architect may require DEMOFEED or live-API success screenshots before treating marketplace as commercially “done” beyond foundation.
2. Stronger Admin↔Agency visual token differentiation may still be requested.
3. Home search widget is discovery intent UI, not a full booking engine.

## Product code

No product code changes in this gate.
