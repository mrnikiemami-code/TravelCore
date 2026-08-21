# P32 — Destination Media Ownership (Architecture Decision Prep)

| Field | Value |
|-------|--------|
| Document | `docs/plans/P32-destination-media-ownership.md` |
| Task-ID | `TC-P32-T007` |
| Phase | P32 — Commercial Demo Data & Media Enrichment |
| Status | **PROPOSED / Cursor PASS** — awaiting Architect ACCEPT |
| Inputs | `P32-T006/ARCHITECTURE-FINDING.md` · Place/Tour Media patterns · P06-R5 CONTRACT-ONLY |

---

## 1. Problem statement

Commercial Home destination cards remain **gradient-led** because:

1. DEMOFEED media pack includes Destination covers (T001).
2. T002 correctly **skipped** Destination attach — no owner API.
3. T006 confirmed: Destination has **no** Cover/Gallery/MediaLink capability.
4. P06-R5 is **RESOLVED — CONTRACT-ONLY** (`MediaAssetReference` + ArchitectureTests; **no** Destination schema `MediaAssetId`).

Hardcoded FE URLs / Media bypass / fake covers are **forbidden**.

---

## 2. Existing Media ownership patterns (SoT)

| Domain | Semantic ownership | Media role |
|--------|--------------------|------------|
| **Media** | Technical asset truth (upload, storage, variants, alt/caption) | Owns bytes + presentation URLs (app-proxy) |
| **Place** | `PlaceMediaLink` Cover 0..1 + Gallery 0..N | Domain owns relation |
| **Tour** | `TourProduct` media Cover 0..1 + Gallery 0..N via `ITourProductMediaService` | Domain owns relation |
| **Destination** | None today | Gap |

Presentation compose pattern (Place/Tour): domain links + `Media.Contracts` presentation → app-proxy paths only.

---

## 3. Architecture options

### Option A — Cover only (recommended first slice)

Destination owns **Cover 0..1** media link (same semantics as Place/Tour Cover).

- Gallery deferred.
- Minimal schema + contracts + admin/public presentation.
- Enough for Home destination cards + destination landing hero.

### Option B — Cover + Gallery (parity with Place/Tour)

Full Cover + Gallery 0..N in one delivery.

- Larger surface (reorder, remove, admin UX).
- Better long-term parity; higher cost for P32 demo urgency.

### Option C — Generic media references only (`MediaAssetReference` field)

Opaque reference(s) without role model.

- Weaker than Place/Tour; poor Gallery/Cover semantics.
- Conflicts with established Cover/Gallery product language.
- **Not recommended.**

### Option D — Defer indefinitely / FE workarounds

- Violates P32 media strategy and product experience rules.
- **Rejected.**

---

## 4. Recommended option

**Option A now**, with **Option B** as authorized follow-up if Architect wants Place/Tour parity.

### Why A

1. Unblocks Home commercial photo density with smallest ownership delta.
2. Matches proven Place/Tour Cover pattern.
3. Keeps DemoFeed enrich path clear (`entityType: destination`, `role: cover` already in pack manifest).
4. Avoids premature Gallery admin complexity.

---

## 5. Required changes (implementation prep — not executed in T007)

### Contracts (Destination)

- `IDestinationMediaService` (or extend existing Destination application service):
  - `GetMediaAsync` / `GetMediaPresentationAsync`
  - `SetCoverAsync` / `RemoveCoverAsync`
- DTOs mirroring Tour/Place Cover presentation compose.

### Domain / Infrastructure (Destination schema)

- Destination-owned media link table (e.g. `destination.destination_media_links`):
  - `destination_id`, `media_asset_id` (opaque Guid), `role` (`Cover`), `sort_order`, timestamps
  - Unique Cover per Destination
- **No** FK into Media schema (logical id only — same as Place/Tour).

### Application

- Upload remains `IMediaUploadService` (Media owns bytes).
- Destination only stores semantic Cover link.
- Presentation compose via Media.Contracts (app-proxy URLs).

### DEMOFEED (later authorized task)

- Extend `enrich-media` to attach Destination covers when owner API exists.
- Idempotent ledger keys already support `destination:*`.

### Frontend (later authorized task)

- Home destination cards / destination landing consume presentation compose — **no hardcoded URLs**.

### Migrations

- Destination-owned migrator only.
- No Media schema change required for Cover link pattern.

---

## 6. ADR requirement

**Yes — recommended.**

Upgrade path from **P06-R5 CONTRACT-ONLY** to:

> Destination may own Cover (0..1) semantic Media links; Media remains technical SoT; no Destination schema FK into Media tables; presentation via Media.Contracts only.

Architect may ACCEPT this plan as the ADR body or issue a numbered ADR task first.

---

## 7. Follow-up implementation tasks (not auto-executable)

| Proposed ID | Intent |
|-------------|--------|
| `TC-P32-T008` (or ADR then impl) | Destination Cover ownership (Contracts + Domain + Infrastructure + migrator + tests) |
| `TC-P32-T009` | DEMOFEED Destination cover enrich via owner path |
| `TC-P32-T010` | Home/Destination experience consume Destination media presentation |
| `TC-P32-GATE` | Phase gate (may still be WITH KNOWN LIMITATIONS if Gallery deferred) |

Exact IDs are Architect-owned.

---

## 8. Non-goals

- Redesign Media module
- Redesign Destination aggregate beyond media relation
- HotelBooking / Pricing / Booking
- Frontend hardcoded covers
- Scraping / competitor assets

---

## 9. Limitations of this document

- Decision prep only — **no code implemented** in T007.
- Gallery remaining deferred under Option A.
- Architect may choose Option B or require ADR numbering before any schema work.

---

## 10. Recommendation summary

```text
Media = technical truth
Destination = Cover semantic relation (0..1)  ← authorize & implement next
DEMOFEED = enrich via owner path
Experience = consume presentation compose
```
