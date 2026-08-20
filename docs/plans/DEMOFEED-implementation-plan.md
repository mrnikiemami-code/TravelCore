# Demo Feeding — Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-DEMOFEED-PLAN` |
| Track | Temporary Demo Feeding (Post-P29 Evolution) |
| Status | **IN PROGRESS** — PLAN authored · awaiting architect ACCEPT |
| Permanence | **TEMPORARY** — not a TravelCore product module |
| Removable | **MUST remain removable** |
| Baseline | `327d18c` (`docs: correct pipeline compliance ledger after forensic review`) |
| Authoritative sources | P04 Destination · P06 Media · P07 Place · P09 Tour · pipeline ledger correction |
| Product code in this PLAN | **NO** |

Temporary realistic demo population of catalog surfaces — **not** a permanent module · **not** ownership change · **not** Booking/Payment/Pricing.

---

## 0. Transition resolve

| Question | Answer |
|----------|--------|
| Prior SoT | HOMFEED implementation COMPLETE · architect **RETROACTIVELY ACCEPTED AFTER FORENSIC REVIEW** |
| Architect envelope | `TC-DEMOFEED-PLAN` (this document) |
| Permanent DemoFeed module today? | **NO** — must stay that way |
| Core domain redesign? | **FORBIDDEN** |
| Scraping in this PLAN? | **FORBIDDEN** |

---

## 1. Purpose

Populate TravelCore with **realistic, removable demo data** so public discovery surfaces (destinations, hotels, tours, media) can be demonstrated.

This is **temporary demo data population only**.

---

## 2. Scope (IN)

| Entity | Owning module (unchanged) | Feeder role |
|--------|---------------------------|-------------|
| Destinations | Destination (P04) | Seed via owning write path |
| Hotels | Place catalog (P07) | Seed hotel Places + translations/slugs |
| Tours | Tour (P09) | Seed TourProduct + translations/slugs |
| Images | Media (P06) | Seed MediaAssets + attach via owner links |

Feeder **does not own** facts. Owners remain Destination / Place / Tour / Media.

---

## 3. Explicit OUT

| Forbidden | Reason |
|-----------|--------|
| Core domain redesign | Architecture locked |
| New permanent module | DemoFeed is disposable |
| Ownership changes | Existing SoR unchanged |
| Booking changes | P19/P21 out of scope |
| Payment changes | P20 out of scope |
| Pricing changes | P12 out of scope |
| Scraping implementation (this PLAN) | PLAN is docs only; later tasks still must not scrape unless a future envelope says otherwise |
| Search engine / ML / personalization | Not demo catalog feeding |
| UGC travelogue fabrication as CMS | UGC ≠ Content |
| Production-only secret/vendor lock-in | Temporary feeder |

---

## 4. Architecture — removable feeding boundary

**DEMOFEED is a disposable feeder, not a bounded context.**

| Rule | Requirement |
|------|-------------|
| Location | Isolated tree (e.g. `tools/demofeed` **or** `src/backend/Tools/DemoFeed`) — **outside** module `Modules/*` ownership |
| Registration | Dev/demo host only · not composed into production module catalog as a domain module |
| Persistence | Writes **only** through owning modules’ existing application/persistence paths |
| Schema | **No** new PostgreSQL schema · **no** `demofeed` table as SoR · **no** DEMOFEED migrations of domain schemas |
| Marker | Every seeded row must be identifiable for deletion (stable demo code/slug prefix and/or explicit demo marker already allowed by owner model — **no new domain columns without ADR**) |
| Dependencies | Feeder may reference Contracts of Destination/Place/Tour/Media; Domain modules must **not** reference the feeder |
| Frontend | No demo-only production UI; public pages consume real catalog reads |

If a demo marker column would require a domain migration, **defer that approach** — use reserved demo slugs/codes instead so deletion stays data-only.

---

## 5. Execution sequence

`TC-DEMOFEED-PLAN` is this document. Implementation starts only after architect ACCEPT **and** an authorized `TC-DEMOFEED-T002` envelope.

| Task | Deliverable |
|------|-------------|
| `TC-DEMOFEED-T002` | Removable feeder host/boundary (isolated tool, no module registration, no migrations) |
| `TC-DEMOFEED-T003` | Destination demo seed (owner write path · identifiable · no ownership leak) |
| `TC-DEMOFEED-T004` | Hotel (Place) + image demo seed (Place + Media attach · not HotelBooking) |
| `TC-DEMOFEED-T005` | Tour + image demo seed (Tour + Media attach · not Booking/Pricing mutation) |
| `TC-DEMOFEED-GATE` | Track acceptance: data present · boundaries held · deletion strategy evidenced |

Do **not** invent extra tasks. Do **not** start T002 from this PLAN result.

---

## 6. Deletion strategy

| Step | Action |
|------|--------|
| 1. Data | Delete seeded Destination / Place-hotel / Tour / Media rows identified by demo codes/slugs (and related media links) via owning modules or a feeder `purge` command |
| 2. Code | Delete the isolated feeder tree (`tools/demofeed` or equivalent) |
| 3. Host | Remove any demo-only composition/DI flag from the host |
| 4. Docs | Remove DEMOFEED plan/evidence after architect-authorized cleanup envelope |
| 5. Proof | After purge, public `/destinations` `/hotels` `/tours` must not retain DEMOFEED rows; domain schemas unchanged |

**Success criterion:** repository and database return to pre-DEMOFEED product architecture with **zero** leftover DemoFeed module, schema, or ownership.

---

## 7. Task map notes

- T002 must fail closed if it would register a permanent `IModule` in production composition.
- T004 must not call HotelBooking availability/rate/reservation ports.
- T005 must not insert Price rows or booking records.
- Images use Media upload/validation contracts already accepted in P06.

---

## Revision history

| Date | Change |
|------|--------|
| 2026-08-20 | Initial PLAN from `TC-DEMOFEED-PLAN` envelope · docs only |
