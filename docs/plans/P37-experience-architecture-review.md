# P37 — Experience Architecture Review

| Field | Value |
|-------|--------|
| Task-ID | `TC-P37-T001` |
| Phase | P37 — Experience Platform Foundation |
| Date | 2026-08-21 |
| Type | Architecture / product experience review (no UI implementation) |
| Prior gate | `TC-P36-GATE` · **PASS WITH KNOWN LIMITATIONS** · **PARTIALLY_SELLABLE_VISUALLY** |

## 1. Experience Surface Architecture

TravelCore is **four product surfaces** on one commerce DNA — not one B2C website with roles bolted on.

| Surface | Audience | Primary job | Honesty posture |
|---------|----------|-------------|-----------------|
| **Public Marketplace** | Anonymous / traveler | Discover → trust → initiate booking | Catalog + published price/departure only; no fake inventory |
| **Customer Dashboard** | Authenticated traveler (Party: Person) | Own trips, bookings, payments, docs | Read server booking/payment truth |
| **Agency Portal** | Agency staff (Party: TravelAgency + Access) | Sell packages, manage agency bookings/customers | B2B pricing/commission; not consumer UI |
| **Admin Console** | Operators | Operate catalog, content, access, audit | Operational; not traveler merchandising |

### Shared vs unique

**Shared (Design System + shells)**

- Typography, color, spacing, buttons, chips, surfaces, forms, empty/error states
- Media presentation patterns
- Locale / RTL / bidi primitives
- Auth presentation hooks (login/session) without collapsing identities

**Unique workflows**

| Surface | Unique |
|---------|--------|
| Public | Merchandising, discovery, public CTA rails, honest commerce initiate |
| Customer | My Trips, documents, saved passengers, notifications |
| Agency | Agency catalog, commission, settlement, agency team permissions |
| Admin | Data grids, bulk ops, audit, workflows, cross-tenant ops |

### Navigation model (direction)

- Public: marketing IA (Home / Destinations / Hotels / Tours / Stories)
- Customer: account shell under `/[locale]/account/...` (or equivalent) — separate from Workspace admin
- Agency: dedicated portal route prefix — **not** the public listing with a role toggle
- Admin: existing Workspace / admin routes matured into operational console

### Access model (locked)

```text
Identity ≠ Party ≠ Access
Customer ≠ Agency User ≠ Admin User
Public Experience ≠ Backoffice Workflow
Booking ≠ Payment
Payment Success ≠ Auto Confirm
FE ≠ Booking Source of Truth
```

Agency listing and customer listing must remain **different views** over shared catalog facts — not one page with CSS differences.

---

## 2. Domain Boundaries (confirmed)

| Boundary | Status after P36 |
|----------|------------------|
| Identity ≠ Party ≠ Access | Preserved (P24/P25 lineage) |
| Booking ≠ Payment | Preserved in commerce UX |
| Payment success ≠ Auto Confirm | Preserved (sandbox honesty) |
| Price ≠ Quote ≠ Booking | Preserved |
| TourProduct ≠ TourDeparture | Preserved |
| Public FE not SoT | Preserved |

P37 must **not** invent parallel booking/payment truth in dashboards.

---

## 3. Customer Dashboard Direction (foundation only)

**Purpose:** post-purchase / account continuity for travelers.

| Area | Intent |
|------|--------|
| My Trips | Upcoming / past trips composed from Booking reads |
| Bookings | Status chips (Pending / Confirmed…) from server |
| Payments | Payment lifecycle display; sandbox/production labels honest |
| Documents | Attachments when domain supports; else honest empty |
| Passengers | Saved traveler profiles (preference/contact lineage) |
| Notifications | Notification module reads — no fake alerts |
| Profile | Identity/contact preferences |

**Out of scope for T001:** any UI build.

---

## 4. Agency Portal Direction (foundation only)

**Purpose:** B2B distribution experience for TravelAgency parties.

| Area | Intent |
|------|--------|
| Agency dashboard | Operational summary for agency (not public merchandising) |
| Tour catalog | Agency-sellable catalog view (terms/commission-aware) |
| Sales workflow | Initiate / track agency bookings within Booking ownership |
| Commission visibility | Commercial profile / offer terms — no invented rates |
| Bookings / Customers | Agency-scoped lists |
| Settlement direction | Future settlement UX; no fake balances |
| Users & permissions | AgencyMember / access relationships |

**Hard rule:** Agency Portal ≠ Customer Dashboard with extra fields.

---

## 5. Admin Console Direction (foundation only)

**Purpose:** operate the platform.

| Area | Intent |
|------|--------|
| Operations dashboard | Health / queues / exceptions (measurement-aware) |
| Catalog operations | Destinations, hotels, tours, departures, media |
| Content / media | Content graph + media ops |
| User / access | Identity + access admin |
| Agency management | Onboarding / commercial profile |
| Reporting | Analytics projections — no invented KPIs |
| Audit / workflows | Audit log + operational workflows |

Admin must stay **permission-aware** and grid/form heavy — not a clone of public marketplace.

---

## 6. Design System Evolution Findings

P36 established public marketplace DNA. Multi-surface gaps:

| Needed for P37+ | Notes |
|-----------------|-------|
| Dashboard layout primitives | Page header, KPI strip, section rails |
| Data grid / table patterns | Sort, filter, row actions, empty |
| Advanced forms | Multi-step, validation, sticky submit |
| Filter bars | Shared with listing but denser for ops |
| Charts / viz | Analytics-owned data only |
| Operational states | Processing, failed, retry, permission denied |
| Notifications UI | Toasts + inbox patterns |
| Permission-aware chrome | Hide/disable by Access — never by FE guess of money |

Do **not** fork a second design system; extend Design System 2.0.

---

## 7. Commercial Experience Roadmap Recommendation

Recommended order after T001:

| Order | Task theme | Rationale |
|-------|------------|-----------|
| 1 | **Customer Dashboard Foundation** | Highest traveler continuity value; reuses Booking/Payment reads already proven in P34–P36 |
| 2 | **Agency Portal Foundation** | Core marketplace differentiator; depends on Party/Access clarity already present |
| 3 | **Admin Console Foundation** | Operational leverage; heavier DS (grids) — benefit from dashboard patterns introduced in (1)–(2) |
| 4 | **Commercial Content & Campaign System** | Merchandising after shells exist; avoid fake campaigns |
| 5 | **Experience Gate** | Multi-surface sellability / readiness review |

**Alternative considered:** Admin first — rejected for P37 start because public/commerce already works; traveler continuity and B2B differentiation unlock more business value sooner.

**Deferred vs P37:** named production payment providers remain paused as P1 unless Architect re-authorizes.

---

## 8. Campaign / Banner Readiness (direction only)

Future merchandising should be **content/campaign-owned**, not hardcoded FE banners.

Direction:

- Placement slots (Home hero, listing rail, agency vs customer audiences)
- Audience targeting by surface (Public / Agency / Customer)
- No fake promotions, discounts, or urgency in P37-T001

---

## Decisions locked by this review

1. TravelCore = **Public + Customer + Agency + Admin** surfaces  
2. Agency catalog ≠ public catalog presentation  
3. Dashboards never become booking/payment SoT  
4. P37 sequence starts with **Customer Dashboard Foundation** next (pending Architect `.task.md`)  
5. No UI implementation in T001

---

## Explicit non-goals (this task)

- No Customer/Agency/Admin UI builds  
- No campaign/banner implementation  
- No provider/payment domain changes  
- No fake commerce data
