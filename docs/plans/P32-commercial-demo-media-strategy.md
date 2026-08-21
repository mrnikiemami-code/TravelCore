# P32 — Commercial Demo Media Strategy

| Field | Value |
|-------|--------|
| Document | `docs/plans/P32-commercial-demo-media-strategy.md` |
| Task-ID | `TC-P32-T001` |
| Phase | P32 — Commercial Demo Data & Media Enrichment |
| Status | **PROPOSED / Cursor PASS** — awaiting Architect ACCEPT |
| Companion | [`P31-demo-content-strategy.md`](P31-demo-content-strategy.md) · Asset pack [`../product-experience/assets/demo-media/`](../product-experience/assets/demo-media/) |

---

## 0. P31 limitation verification

| P31 finding | Status |
|-------------|--------|
| Commercial UX direction acceptable | Confirmed (`TC-P31-GATE` ACCEPTED WITH KNOWN LIMITATIONS) |
| Photo density / synthetic placeholders | **Primary remaining blocker** |
| Live DEMOFEED success evidence | Separate from this media pack (API/env); pack prepares assets for T002 enrichment |
| No superseding decision against P32 media focus | Confirmed |

---

## 1. Purpose

Close the largest remaining sales-demo gap:

> TravelCore looks like a serious travel product when opened in a sales demo.

This document defines **media strategy** and the **demo asset pack foundation**.  
Uploading into Media/DEMOFEED owners is **`TC-P32-T002`** (do not invent here).

---

## 2. Commercial Demo Media Strategy

### 2.1 Surface coverage

| Surface | Required media |
|---------|----------------|
| Home | Destination / hotel / tour covers for featured bands |
| Hotel listing | Cover per hotel card |
| Hotel detail | Cover + ≥1 gallery image (prefer ≥3 later) |
| Tour listing | Cover per tour card |
| Tour detail | Cover + optional gallery |
| Destination | Strong cover per featured destination |

### 2.2 Aspect ratios

| Role | Ratio | Notes |
|------|-------|-------|
| Destination / Tour cover | **16:9** | Hero + listing cards |
| Hotel cover / gallery | **4:3** | Marketplace card density |
| Optional square thumb | 1:1 | Future only — not required for T001 |

### 2.3 Minimum dimensions

| Role | Minimum |
|------|---------|
| Cover | ≥1600px on the long edge |
| Gallery | ≥1200px on the long edge |
| Reject | 1×1 PNG · tiny placeholders · watermarked competitor stock |

### 2.4 File naming

```text
demofeed-<entity>-cover.png
demofeed-<entity>-gallery-NN.png
```

Examples:

- `demofeed-istanbul-cover.png`
- `demofeed-hotel-tehran-1-cover.png`
- `demofeed-hotel-tehran-1-gallery-01.png`
- `demofeed-tour-istanbul-1-cover.png`

Entity tokens must align with DEMOFEED codes/slugs (`demofeed-*`).

### 2.5 Localization / captions

| Rule | Requirement |
|------|-------------|
| Alt text | FA primary · EN secondary in manifest |
| On-image text | **Forbidden** (no burned-in titles/logos) |
| DEMOFEED label | UI may badge `DEMOFEED sample`; do not bake into pixels |

### 2.6 Attribution / provenance

| Rule | Requirement |
|------|-------------|
| Source | Cursor-generated demo imagery for TravelCore sales demos |
| License posture | Repository-owned demo assets · replaceable · non-production |
| Forbidden sources | Scraped competitor sites · Booking/Airbnb/LastSecond/Tahagasht copy · undocumented third-party stock |
| Manifest | Every file listed in `manifest.json` with provenance |

### 2.7 Fallback behavior

1. Prefer Media presentation cover when available  
2. Else demo pack path from manifest (after T002 upload)  
3. Else premium gradient placeholder (honest · never fake inventory)

### 2.8 Performance

- Prefer WebP/AVIF later via Media pipeline variants  
- Pack stores PNG masters for T001  
- Covers should stay under ~2.5MB each when practical  

### 2.9 Accessibility

- Meaningful alt text in manifest  
- Never rely on color/gradient alone for product identity when cover exists  
- Decorative-only images marked accordingly  

---

## 3. Integration boundary

| Boundary | Rule |
|----------|------|
| Media ownership | Media owns technical asset truth |
| Semantic links | Place / Tour / Destination own relation meaning |
| DEMOFEED | Removable feeder uploads via owner Media paths in **T002** |
| This task (T001) | Strategy + pack on disk under `docs/product-experience/assets/demo-media/` |
| Forbidden | Ad-hoc hard-coded competitor URLs · bypassing Media · Api DemoFeed module |

**Next authorized step (recommended):** `TC-P32-T002` — DEMOFEED Media Enrichment (upload pack through Media owner path; attach to demofeed destinations/hotels/tours).

---

## 4. Initial pack coverage (T001)

See `docs/product-experience/assets/demo-media/manifest.json`.

| Category | Coverage |
|----------|----------|
| Destinations | IR · TR · Tehran · Istanbul covers |
| Hotels | Teh-1 · Ist-1 covers + 1 gallery each |
| Tours | Teh-1 · Ist-1 covers |

Region `demofeed-tehran-region` may reuse Tehran cover until a dedicated asset is authorized.

---

## 5. Commercial quality bar

Must materially improve on:

- pure gradients  
- 1×1 synthetic placeholders  
- empty visual blocks  

Must **not**:

- scrape / clone competitors  
- invent live rates/availability/reviews  
- claim production photography from real partner hotels  

---

## Revision

| Date | Change |
|------|--------|
| 2026-08-21 | Initial strategy + pack foundation from `TC-P32-T001` |
