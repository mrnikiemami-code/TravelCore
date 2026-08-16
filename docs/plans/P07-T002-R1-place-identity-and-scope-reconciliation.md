# TC-P07-T002-R1 — Place Identity & T002 Scope Reconciliation

| Field | Value |
|-------|--------|
| Task | `TC-P07-T002-R1` |
| Baseline | `HEAD=d127ee7` (clean at start) |
| Product under review | `TC-P07-T002` (`83529cf`) |
| Scope | **Docs-only** — no product/migration code changes |
| Authoritative lock | **P07-R1 RESOLVED** — CORE PLACE + TYPED SPECIALIZATION |

---

## 1. P07-R1 identity lock (authoritative)

From [`P07-implementation-plan.md`](P07-implementation-plan.md) §11 **P07-R1** and `PROJECT-STATE`:

| Rule | Lock |
|------|------|
| Aggregate root | **Place** |
| Canonical Place catalog identity | **`PlaceId` only** |
| Hotel / Restaurant / Attraction | Specializations of Place; use **`PlaceId`** (1:1 same-schema FK) |
| Independent `HotelId` / `RestaurantId` / `AttractionId` as public catalog PK | **Forbidden** for P07 Place catalog |
| TPH giant nullable Place table | Forbidden |
| HotelBooking fields on Place | Forbidden |
| Hotel Catalog ≠ Hotel Booking | Preserved — HotelBooking may map provider **`ExternalHotelId` → `PlaceId`** |

---

## 2. Identity inventory (classifications A / B / C / D)

Inventory of `HotelId` / related id usages found in architecture & data docs **before** this remediation, classified for architect RESULT:

| Class | Meaning | Examples (pre-R1 docs) | Disposition |
|-------|---------|------------------------|-------------|
| **A** | Canonical Place catalog identity (must be `PlaceId`) | `Place.Hotel: HotelId = H123 (canonical catalog)`; list entry `HotelId` beside `PlaceId` in identifiers doc; “logical Place.HotelId” | **Rewrite → `PlaceId`** |
| **B** | Cross-module logical reference *to* Place catalog (value is PlaceId of Hotel-kind Place) | `TourHotelOption.HotelId`; `tour.tour_hotel_options.hotel_id → Place.HotelId`; Tour→Place “HotelId validation” | **Semantics → reference `PlaceId`** (Hotel-kind). Column/property name `hotel_id` / `HotelId` may remain as a *domain alias* until Tour phase names it; docs must not imply a second catalog PK |
| **C** | Stale plan/docs treating typed subtype ids as canonical Place identity | P07 plan: “`PlaceId` / typed ids”; “`HotelId` already named”; Identifiers row listing `HotelId` as peer of `PlaceId` for Place | **Rewrite** — remove subtype ids as canonical Place catalog identity |
| **D** | Valid non-catalog / provider identity (keep) | `ExternalHotelId`; `ProviderCode` + provider hotel mapping; “ProviderHotelId never internal PK” | **Keep** — HotelBooking maps `ExternalHotelId` → **`PlaceId`** |

### Classification summary (HotelId)

| Occurrence family | Class | After reconciliation |
|-------------------|-------|----------------------|
| Place catalog canonical id | **A** | `PlaceId` |
| TourHotelOption / tour.hotel_id logical ref | **B** | Documents as `PlaceId` (Hotel-kind Place); alias name OK until Tour locks property name |
| P07 plan “typed ids / HotelId already named” | **C** | Removed as canonical |
| HotelBooking ExternalHotelId / provider mapping | **D** | Unchanged; mapping target = `PlaceId` |

---

## 3. Specialization fields evidence (`StarRating` / `CuisineType` / `CategoryCode`)

### 3.1 Were they explicitly authorized by name pre-T002?

| Field | Explicitly named in plans/architecture pre-T002? | Evidence |
|-------|--------------------------------------------------|----------|
| `StarRating` | **NO** | No hit in `docs/plans/P07-implementation-plan.md`, constitution Place section, or module-boundaries field lists as a named property |
| `CuisineType` | **NO** (concept only) | `docs/pages/05-place-details.md` mentions optional “cuisine” / “cuisine/category” for Restaurant anatomy — **not** the identifier `CuisineType` |
| `CategoryCode` | **NO** (concept only) | Same page: “cuisine/category”; Place owns “طبقه‌بندی” (classification) in `04-module-boundaries.md` — **not** `CategoryCode` |

### 3.2 What *was* authorized

| Source | Allowance |
|--------|-----------|
| **P07-R1** | “shared facts on Place; **type-specific on specialization tables**” |
| **TC-P07-T002** | Allowed: “**catalog metadata persistence** in `place` schema”; Done-when: “at least one Place type persistable end-to-end” |
| **Place ownership** (`04-module-boundaries`) | Owns facilities · **classification** · catalog status · descriptive profile |
| **T004** (later) | Facilities / classification / catalog status baseline — broader classification product; does **not** forbid a minimal specialization column in T002 |

### 3.3 Conclusion (fields)

Fields were **not** explicitly named pre-T002. Prefer classifying as:

> **Within R1 allowance** (“type-specific facts on specialization”) **+** T002 “catalog metadata persistence” as **minimal representative specialization metadata** — **without inventing new commercial/product policy** (no bookability, no provider rates, no live inventory).

Do **not** remove product code in this remediation task.

---

## 4. Governance classification

| Topic | Classification |
|-------|----------------|
| Stale docs presenting `HotelId` as canonical Place catalog id after P07-R1 | **DOC_DRIFT / SOURCE_OF_TRUTH_STALE** — remediated in this task |
| T002 introducing `StarRating` / `CuisineType` / `CategoryCode` without named field list | **WITHIN_R1_AND_T002_ALLOWANCE** (minimal type-specific catalog metadata) — **not** a product-policy invention requiring STOP |
| Independent public `HotelId` type in Place module product (`83529cf`) | **Aligned with R1** (implementation uses `PlaceId` only) — docs were the conflict |
| Residual conceptual `HotelId` in older constitution / dependency-rules examples (outside this patch list) | **NON_BLOCKING residual** — same Class B semantics; optional follow-up doc pass |

**Product code / migrations:** unchanged by this task (docs-only).

---

## 5. Docs patched in this task

| Document | Change |
|----------|--------|
| `docs/data/01-identifiers-and-references.md` | Place catalog id = `PlaceId`; Tour ref → `PlaceId`; provider mapping InternalId = `PlaceId` |
| `docs/architecture/04-module-boundaries.md` | TourHotelOption / HotelBooking examples → `PlaceId` |
| `docs/architecture/06-cross-module-communication.md` | TourHotelOption + HotelBooking mapping → `PlaceId` |
| `docs/architecture/07-data-architecture.md` | logical ref + provider example → `PlaceId` |
| `docs/architecture/08-persistence-and-migrations.md` | `logical Place.HotelId` → `PlaceId` |
| `docs/domain/module-ownership-matrix.md` | HotelBooking refs `PlaceId`; Tour→Place validation by `PlaceId` |
| `docs/ui/06-cross-domain-workflow-and-navigation.md` | Catalog refs / HotelBooking maps to `PlaceId` |
| `docs/plans/P07-implementation-plan.md` | Remove typed-subtype ids as canonical Place identity |
| `docs/domain/glossary.md` | TourHotelOption wording → `PlaceId` |

`ExternalHotelId` / provider mapping language **preserved** (Class D).

---

## 6. Recommendation (architect)

1. **ACCEPT** `TC-P07-T002` (`83529cf`) as COMPLETE for Place catalog domain + persistence baseline under P07-R1.
2. Treat `StarRating` / `CuisineType` / `CategoryCode` as **accepted minimal representative specialization metadata** within R1 + T002 catalog-metadata allowance (not a new product policy surface).
3. Treat this R1 docs pass as closing **Place catalog identity** SoT drift (`PlaceId` only).
4. Do **not** start `TC-P07-T003` until architect accepts T002 / issues Auto-Execute T003.
5. Optional later: align constitution §5/§7 and `05-dependency-rules.md` TourHotelOption snippets to `PlaceId` (Class B residual).

---

## 7. Open decisions (unchanged)

| ID | Status |
|----|--------|
| P07-R1 | **RESOLVED** (reinforced by this artifact) |
| P07-R2 | UNRESOLVED |
| P07-R3 | UNRESOLVED |
| P07-R4 | UNRESOLVED |
| P07-R5 | UNRESOLVED |
