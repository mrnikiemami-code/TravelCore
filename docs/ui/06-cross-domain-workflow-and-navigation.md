# Cross-Domain Workflow & Navigation Model (TC-P02-T010)

**Status:** Authoritative for P02+ Admin/application UX modeling  
**Phase:** P02 — Frontend Foundation + Walking Skeleton  
**Depends on:** T008 (AdminShell slots) · T009 (API/read-model boundary)  
**Non-goals:** product workflow screens · concrete menus · backend changes · T011+

Ownership evidence (read-only):  
[`../architecture/03-domain-map.md`](../architecture/03-domain-map.md) ·  
[`../architecture/04-module-boundaries.md`](../architecture/04-module-boundaries.md) ·  
[`../domain/module-ownership-matrix.md`](../domain/module-ownership-matrix.md) ·  
[`../architecture/10-ui-constitution.md`](../architecture/10-ui-constitution.md) ·  
[`../frontend` API boundary](../../src/frontend/web/src/lib/api/README.md)

---

## 1. Locked principles

### 1.1 Domain ≠ navigation ≠ screen ≠ form ≠ workflow

| Protects architecture | Does **not** automatically define |
|-----------------------|-----------------------------------|
| Backend module / aggregate ownership | Menu items |
| DbContext / migration ownership | Screens |
| Command/query authority | Forms |
| Cross-module communication style | User journeys |

**Frontend MUST** organize around **user goals / jobs-to-be-done**.  
**Frontend MUST NOT** mirror bounded contexts as a default CRUD/menu tree.

A single coherent UX journey **MAY** coordinate several modules through **explicit application/API contracts** while each module remains authoritative for its own commands/queries.

### 1.2 Presentation is not a domain module

Public Website · Admin Panel · Agency Panel own presentation concerns only.  
They compose read models and invoke owning application contracts.  
They do **not** become a new combined aggregate.

### 1.3 Backend remains authoritative

| Concern | Owner |
|---------|-------|
| Validation invariants | Owning module application layer |
| Authorization | Access (+ subject Identity/Party) |
| Price / Quote / Booking / Payment truth | Pricing · Booking · Payment respectively |
| What the user sees | Presentation + WorkflowViewModel composition (T009) |

UI may hide/disable actions; **hiding a button is not security**.

### 1.4 Raw identifiers are not UX

Users must not be required to copy/paste `IdentityId`, `PartyId`, `TourId`, foreign keys, or other internal IDs as a normal workflow step.  
IDs may exist in contracts; the UI surfaces **search / select / create / link** patterns instead.

### 1.5 Cross-domain ≠ frontend business engine

React/components coordinate presentation and **explicit** API calls.  
They do not host authoritative orchestration rules, pricing engines, or domain invariants.

---

## 2. Relationship UX patterns (reusable)

These are **presentation patterns**, not permission to change domain invariants.

| Pattern | Meaning | Notes |
|---------|---------|-------|
| **Select existing** | Search/browse then attach known record | Preferred when reuse is likely |
| **Create new (inline/guided)** | Create related record inside current journey | Still calls owning module create contract |
| **Link existing** | Establish association between already-known records | Association semantics owned by the module that owns the relationship |
| **Replace relationship** | Swap linked target | Validate both unlink + link rules |
| **Unlink** | Remove association when business rules allow | Never “delete the other module’s aggregate” by default |
| **Inspect without context loss** | Peek/detail sheet/drawer while keeping journey state | Mobile: sheet; Desktop: panel/drawer |

**Forbidden UX defaults:** “open Module A CRUD → copy ID → open Module B CRUD → paste ID”.

---

## 3. API / read-model boundary (T009)

| Rule | Application |
|------|-------------|
| PageViewModel | Purpose-specific presentation contract for one screen/section |
| WorkflowViewModel | May **compose** multi-module read data for one user goal |
| Writes | Remain **explicit per owning application contract** — one composed PVM ≠ one backend aggregate |
| Persistence | Forbidden in frontend |
| Cache/revalidate | Caller-explicit (T009) |
| Errors | Prefer Problem Details–aware `ApiResult` kinds |
| Correlation | Optional `X-Correlation-ID` |

Partial multi-command sequences are documented here; **T010 does not implement** those commands.

---

## 4. Navigation model (provisional, evidence-based)

### 4.1 Organizing concept: user jobs (not module names)

Provisional Admin job areas (labels are illustrative, not final product branding):

| Job area | Typical user goals | Modules often involved (not menu 1:1) |
|----------|--------------------|----------------------------------------|
| **Accounts & people** | Onboard staff/agency users; link business profiles; assign access | Identity · Party · Access |
| **Catalog & destinations** | Maintain places/hotels catalog; destination graph; media assets | Destination · Place · Media · ReferenceData |
| **Offers & pricing** | Shape tour products/departures; attach media; manage commercial rates/quotes | Tour · Destination · Media · Pricing · Place |
| **Orders & money** | Complete bookings; attach travelers/parties; take payment | Booking · Party · Pricing · Payment |
| **External inventory** *(when enabled)* | Live hotel/flight provider ops mapped to catalog | HotelBooking · Flight · Place · ReferenceData |
| **System** | Roles catalog, reference data ops, platform settings | Access · ReferenceData · platform modules |

**Proof this is not a renamed module list:** one job area routinely spans multiple modules; one module (e.g. Media, Pricing, Party) appears in several job areas without becoming multiple “Media menus” as the mental model.

### 4.2 Navigation rules

1. One backend module ≠ one menu item (default).
2. One aggregate ≠ one screen (default).
3. Tasks may span modules; navigation minimizes context switching.
4. Related create/link/select actions appear **in context**.
5. Raw IDs are not navigation concepts.
6. Permissions may hide/disable without redefining ownership.
7. **AdminShell (T008)** supplies regions only (`header` · `navigation` · `actions` · `content`).
8. **T010** supplies the model that later may populate those slots — **no concrete domain menu tree in this task**.

### 4.3 AdminShell compatibility

`AdminShell` already exposes a **navigation slot** with no IA freeze.  
T010 does **not** require redesign: later screens pass job-based navigation content into the slot.  
IA freeze of concrete items remains a later product decision informed by this model.

---

## 5. Mobile-first workflow rules

Apply to every representative cross-domain journey:

| Rule | Requirement |
|------|-------------|
| Primary path | Complete the job on ~360–390px without a desktop-only critical step |
| Typing | Minimize; prefer select/search over free-text IDs |
| Transitions | Prefer one continuous journey; stepwise only when it improves comprehension |
| Disclosure | Progressive — show next needed fields, not giant multi-column forms |
| Create/select | Touch-friendly targets; sheets/drawers over tiny modals |
| State | Preserve in-progress draft across steps/back |
| Back/cancel | Explicit; warn on unsaved committed-vs-pending clarity |
| Failure | Recover without restarting from zero when safe |
| IDs | Never require raw ID entry |
| Wizards | Not automatic — use only when the job truly benefits |

---

## 6. Partial failure / retry (no distributed transaction)

Cross-module sequences can **partially succeed**. Do **not** assume a distributed transaction across module DbContexts.

For each multi-command journey document:

- what already committed
- what remains incomplete
- what the user sees
- safe retry / continue path
- idempotency expectations where contracts support them
- compensation **only** if authoritative backend design supports it

### Canonical example (Identity → Party link)

| Step | Module | Outcome |
|------|--------|---------|
| 1 Create Identity | Identity | Success → Identity exists |
| 2 Create/link Party | Party / Identity association | Fails |

**UX must:**

- Show Identity as created (not pretend nothing happened)
- Offer **retry link/create Party** without recreating Identity blindly
- Avoid forcing the user to “go find the IdentityId”
- Never invent cross-module rollback unless an accepted backend compensation exists

---

## 7. Permissions model (presentation)

| UI may | UI must not |
|--------|-------------|
| Hide irrelevant actions | Treat visibility as authorization SoR |
| Disable with short explanation | Bypass Access evaluation |
| Conditionally skip steps the subject cannot perform | Invent roles inside React |

Backend Access evaluation remains final.

---

## 8. Workflow A — Identity ↔ Party ↔ Access

### Primary user goal

Onboard a person who can sign in, has a business profile (Person/Organization/Agency), and receives appropriate permissions — **without understanding bounded contexts**.

### Likely persona

Admin / agency manager / platform operator.

### Entry point

Accounts & people job area → “Invite / onboard user” guided flow (future screen).

### Participating modules & ownership

| Operation | Owner | Notes |
|-----------|-------|-------|
| Create/authenticate account | **Identity** | Credentials / login identity |
| Create Person/Org/Agency profile | **Party** | Business who |
| Associate Identity↔Party | Owning association contract (Identity may store PartyId ref; Party remains Party) | No merged aggregate |
| Assign roles/permissions | **Access** | Subject = Identity and/or Party IDs by contract |
| Reference catalogs | ReferenceData | As needed |

**Backend separation remains:** Identity ≠ Party ≠ Access.

### Workflow sequence (logical)

1. Capture login identity (Identity create/invite).
2. **Branch:** create new Party **or** search/select/link existing Party.
3. Establish relationship via explicit contract (no raw ID paste).
4. Continue to Access assignment steps appropriate to the Party type.
5. Confirm summary; exit to job hub.

### Create / link / select

| Situation | Pattern |
|-----------|---------|
| New hire, no profile | Create Party inline |
| Existing Agency Party | Select/link existing |
| Wrong link | Replace relationship (Access rules apply) |

### Prefill / explicit input

| Prefill / reuse | User must provide |
|-----------------|-------------------|
| Locale defaults, known org from context | Credentials/invite channel; display name; Party type; contact essentials; role set |

### Validation / authz / failure

- Validation: per owning module.
- Authorization: Access gates each step; UI reflects but does not decide.
- Partial failure: see §6 Identity→Party example.
- Retry: resume from last incomplete step; avoid duplicate Identity when already committed.

### Mobile

Single column; Party search sheet; Access as progressive checklist; no desktop-only role matrix as sole path.

### Invisible to user

DbContexts, FK columns, module names as primary IA, “go to Identity CRUD then Party CRUD”.

### Must never be raw-FK workflow

Copying IdentityId/PartyId between pages.

---

## 9. Workflow B — Tour ↔ Destination ↔ Media ↔ Pricing

### Primary user goal

Shape a sellable tour offering with destinations, representative media, and commercial pricing presentation — as **one content/commerce job**, not four module silos.

### Likely persona

Catalog/content operator · product manager.

### Entry

Offers & pricing job area → create/edit tour product/departure (future).

### Ownership matrix (commands/queries)

| Concern | Owner |
|---------|-------|
| TourProduct / TourDeparture / itinerary / TourHotelOption / package transport facts | **Tour** |
| Destination nodes | **Destination** |
| MediaAsset bytes/metadata | **Media** |
| TourMedia ordering/role | **Tour** (references MediaAssetId) |
| Hotel catalog facts for options | **Place** (Tour references HotelId) |
| TourRate / PriceComponent / Quote | **Pricing** |
| Currency catalog | **ReferenceData** |

**Preserve:** TourProduct ≠ TourDeparture; Tour does not own Quote calc; Media does not own gallery meaning.

### Sequence (logical)

1. Create/edit Tour product shell (Tour).
2. Select destinations (Destination search/select; Tour stores links).
3. Attach representative media (upload/select Media → Tour owns TourMedia relation).
4. Configure hotel options by selecting Place.Hotel (not copying hotel aggregate).
5. Define/attach commercial rates or request Quote prep (Pricing contracts).
6. Review composed WorkflowViewModel for publish readiness (presentation only).

### Create / link / select

| Need | Pattern |
|------|---------|
| Existing destination | Select |
| New media asset | Create Media then link as TourMedia |
| Hotel option | Select Place.Hotel; Tour owns option config |

### Prefill / explicit

Reuse destination/media libraries; user supplies tour commercial structure and which assets/options apply.

### Partial failure example

Tour + destination links saved; Pricing rate create fails → show tour as saved; offer retry pricing without recreating tour; do not invent “all or nothing” across Tour/Pricing DbContexts.

### Mobile

Sectioned progressive disclosure: Basics → Destinations → Media → Options → Pricing; sticky save/status; sheets for pickers.

### Invisible

Module folders, EF shapes, “Pricing menu then Tour menu” as required choreography.

---

## 10. Workflow C — Booking ↔ Party ↔ Pricing ↔ Payment

### Primary user goal

Complete a reservation journey: identify customer party, lock commercial quote facts, create booking, collect payment — **without collapsing money concepts**.

### Critical distinctions (must remain)

```text
Price  ≠  Quote  ≠  Booking  ≠  Payment
```

| Concept | Owner |
|---------|-------|
| Live/commercial rate & Quote | **Pricing** |
| Accepted historical commercial snapshot / reservation state | **Booking** |
| Money movement / attempts / refunds foundation | **Payment** |
| Customer business identity | **Party** |

### Sequence (logical)

1. Identify/select/create customer **Party**.
2. Obtain/validate **Quote** (Pricing) for chosen product/departure/occupancy.
3. Create **Booking** referencing/snapshotting accepted Quote + travelers.
4. Initiate **Payment** against Booking purpose.
5. React to payment outcome via backend events/contracts (Booking status updates owned by Booking rules).

### UX may be one journey; ownership stays split

One page/flow may display quote summary + pay CTA composed in a WorkflowViewModel.  
Writes remain separate commands to Pricing / Booking / Payment as designed.

### Partial failure example

Booking created; Payment attempt fails → Booking remains in unpaid/pending-per-rules state; user retries payment **without** recreating booking or regenerating a new Quote unless expiry rules require it.

### Mobile

Summary-first; traveler fields progressive; payment step isolated; clear “what is unpaid”.

### Forbidden collapses

- Treating Payment success as “Booking module owns card capture”
- Recalculating authoritative Quote in React
- Showing Price as if it were a paid Booking

---

## 11. Workflow D — Place / Hotel ↔ Media ↔ Pricing

### Primary user goal

Maintain hotel (and related place) catalog quality: descriptive profile, media, and any TravelCore-owned commercial presentation — **without merging Hotel Catalog and HotelBooking**.

### Ownership

| Concern | Owner |
|---------|-------|
| Hotel descriptive catalog | **Place** |
| MediaAsset | **Media** |
| Place↔media meaning/order | **Place** (references MediaAssetId) |
| Live provider search/book/voucher | **HotelBooking** (maps to Place.HotelId) |
| TourCore Pricing aggregates for provider fares | **Not fully decided** — do not invent; Money meaning may be shared later |

### Sequence (catalog path)

1. Create/edit Place.Hotel profile.
2. Select Destination.
3. Attach media (Media create/select → Place relation).
4. If/when commercial catalog rates exist under accepted design, attach via Pricing contracts — **not** by owning live provider inventory in Place.

### HotelBooking path (separate job when enabled)

Live availability/book flows invoke **HotelBooking**, referencing Place.HotelId.  
UI must not let catalog edit screens silently become provider booking engines.

### Partial failure

Place saved; media processing fails → show place with media pending/retry; do not delete place.

### Mobile

Catalog edit sections; media upload sheet; never require desktop-only map/media tools as sole path.

---

## 12. Module ownership quick matrix (workflow lens)

| Workflow | Primary owners | Presentation may compose | Must not merge |
|----------|----------------|--------------------------|----------------|
| A | Identity, Party, Access | Yes (WorkflowViewModel) | Identity+Party+Access aggregates |
| B | Tour, Destination, Media, Pricing (+Place refs) | Yes | Tour+Pricing DbContexts; Media asset≠TourMedia meaning |
| C | Booking, Party, Pricing, Payment | Yes | Price/Quote/Booking/Payment meanings |
| D | Place, Media (+HotelBooking when live) | Yes | Place catalog ≠ HotelBooking inventory |

Ambiguities intentionally deferred by architecture (not invented here): full Pricing ownership of HotelBooking/Flight provider fares.

---

## 13. Proof checklist (T010)

| ID | Claim | Result |
|----|-------|--------|
| PROOF-01 | Domain ≠ navigation/menu/screen | §1.1 · §4 |
| PROOF-02 | Identity/Party/Access coherent UX, separate ownership | §8 · §6 |
| PROOF-03 | Tour/Destination/Media/Pricing analyzed | §9 |
| PROOF-04 | Booking path preserves Price≠Quote≠Booking≠Payment | §10 |
| PROOF-05 | Place/Hotel vs HotelBooking preserved | §11 |
| PROOF-06 | Job-based navigation, not module rename list | §4 |
| PROOF-07 | Mobile-first rules | §5 + per workflow |
| PROOF-08 | Partial failure without distributed TX | §6 + examples |
| PROOF-09 | T009 boundary preserved | §3 |
| PROOF-10 | AdminShell slot-compatible; no IA freeze | §4.3 |
| PROOF-11 | No product workflow implementation in this task | Docs-only deliverable |

---

## 14. Downstream use

- Later Admin IA may populate `AdminShell` navigation from **job areas** in §4, not from a module folder tree.
- Page Archetypes (T011+) must cite which workflow(s) they serve and which commands they invoke.
- Concrete menus, screens, and forms are **out of scope** for T010.
