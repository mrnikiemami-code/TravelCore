# TC-DEMOFEED-GATE — Cursor Gate Review

| Field | Value |
|-------|--------|
| Task-ID | `TC-DEMOFEED-GATE` |
| HEAD at review | `ddd2ad4` |
| Status (Cursor) | **PASS** |
| Recommendation | **ACCEPT** DEMOFEED Data Enablement sequence (T002–T005) |

## Reviewed DEMOFEED units

| Unit | Deliverable | Evidence |
|------|-------------|----------|
| `TC-DEMOFEED-T002` | Removable feeder at `tools/demofeed` | Console tool · solution membership · **not** in Api module list |
| `TC-DEMOFEED-T003` | Destination seed | `list destinations` → 5 `demofeed-*` rows |
| `TC-DEMOFEED-T004` | Place (Hotel) + Media | `list places` → 2 hotels linked to demo cities |
| `TC-DEMOFEED-T005` | Tour + Media | `list tours` → 2 Published Package products |

## Architecture assessment

| Check | Verdict |
|-------|---------|
| Removable feeder boundary (`tools/demofeed`) | **PASS** |
| DemoFeed is not a production `ITravelCoreModule` | **PASS** (`TravelCore.Api/Program.cs` DemoFeed-free) |
| Owner application paths used (no direct domain-bypass invent) | **PASS** (Destination/Place/Tour/Media services) |
| Place ≠ HotelBooking | **PASS** (no HotelBooking refs in tool seed code) |
| Tour ≠ Pricing / Booking ownership | **PASS** (no Pricing/Booking inserts) |
| No demofeed PostgreSQL schema / demofeed domain migrations | **PASS** |
| Identity via `demofeed-*` codes/slugs | **PASS** |
| Scraping / competitor content | **PASS** (synthetic PNG only) |

## Live inventory check (operator DB)

```
destinations: 5 (demofeed-ir, demofeed-ir-thr, demofeed-ir-teh, demofeed-tr, demofeed-tr-ist)
places:       2 (demofeed-hotel-teh-1, demofeed-hotel-ist-1)
tours:        2 (demofeed-tour-teh-1, demofeed-tour-ist-1) Published Package
purge:        fail-closed (exit 2) — cleanup awaits authorized purge envelope
```

## Known limitations

1. Media covers are synthetic 1×1 PNG on local tool storage — not production S3 photography.
2. No TourDeparture / Pricing / Booking / availability / ratings / scarcity.
3. Tour seed is Package-only (no Experience itinerary specialization).
4. `purge` remains fail-closed until a future Architect-authorized cleanup task (deletion strategy documented in plan §6; GATE does not invent purge execution).
5. Public UX richness still depends on API connectivity + experience composition beyond seed rows.

## Recommended next (Architect decision — do not invent)

Options for Architect authorization via new `.task.md` / `.gate.md` only:

- Experience re-review of Public Hotel/Tour surfaces with live DEMOFEED catalog
- Authorized DEMOFEED purge / cleanup proof
- Next product phase connecting Experience → richer Commercial data

## Product code

No product code changes in this gate (docs/evidence only).
