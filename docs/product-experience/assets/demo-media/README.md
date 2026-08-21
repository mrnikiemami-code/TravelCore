# DEMOFEED Commercial Demo Media Pack

| Field | Value |
|-------|--------|
| Path | `docs/product-experience/assets/demo-media/` |
| Task | `TC-P32-T001` (pack) · `TC-P32-T002` (Media enrich via DEMOFEED) |
| Purpose | Repository-safe sales-demo imagery for existing DEMOFEED entities |
| Permanence | **Temporary / replaceable** — not production partner photography |
| Upload to Media | **Done for Hotel/Tour** via `tools/demofeed enrich-media` · Destination attach blocked (no owner API) |

## Rules

1. Clearly **demo / non-production**
2. **No scraping** · no competitor copy · no Booking/Airbnb/LastSecond/Tahagasht trade dress
3. No burned-in text/logos on images
4. Naming aligns with `demofeed-*` codes/slugs
5. Provenance recorded in `manifest.json`

## Layout

```text
demo-media/
  README.md
  manifest.json
  demofeed-*-cover.png
  demofeed-*-gallery-NN.png
```

## Coverage (T001)

| Entity | Files |
|--------|-------|
| Destination Istanbul | `demofeed-istanbul-cover.png` |
| Destination Tehran | `demofeed-tehran-cover.png` |
| Destination Turkey | `demofeed-turkey-cover.png` |
| Destination Iran | `demofeed-iran-cover.png` |
| Hotel Tehran 1 | cover + `gallery-01` |
| Hotel Istanbul 1 | cover + `gallery-01` |
| Tour Tehran 1 | cover |
| Tour Istanbul 1 | cover |

## Integration status (T002)

| Entity | Status |
|--------|--------|
| Hotels (`demofeed-hotel-*`) | **Enriched** — Cover + Gallery via Place Media ownership |
| Tours (`demofeed-tour-*`) | **Enriched** — Cover via Tour Media ownership |
| Destinations (`demofeed-ir*`, `demofeed-tr*`) | **Not attached** — no Destination↔Media owner API (Architectural Concern) |

Command:

```bash
dotnet run --project tools/demofeed -- enrich-media --connection "..."
```

Pack PNGs were re-encoded once so Media P06-R6 SVG sniff (first 512 bytes) does not false-positive on binary coincidence.
