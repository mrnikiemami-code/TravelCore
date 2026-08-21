# P38-T006 — Commerce Slice Gate Review

| Field | Value |
|-------|--------|
| Task-ID | `TC-P38-T006` |
| Gate kind | Commerce Slice Gate (review only · no implementation) |
| Date | 2026-08-22 |
| HEAD reviewed | `80c6546` |
| Status recommendation | **`PASS WITH KNOWN LIMITATIONS`** |
| Slice verdict | **`READY_PUBLIC_SLICE`** · Agency operational depth still incomplete |

## Reviewed tasks (Architect ACCEPTED)

| Task | Scope | Evidence |
|------|--------|----------|
| T001 | Multi-Agency Commerce foundation plan | `docs/plans/P38-multi-agency-commerce-foundation.md` |
| T002 | AgencyOffer contracts | `docs/plans/P38-agency-offer-contracts.md` |
| T003 | AgencyOffer persistence (AgencyMarketplace evolve) | `docs/product-experience/evidence/P38-T003/` |
| T004 | Public offer selection | `docs/product-experience/evidence/P38-T004/` |
| T005 | Booking initiation Offer boundary | `docs/product-experience/evidence/P38-T005/` |

---

## 1. Path under review

```text
TourProduct
        ↓
AgencyOffer(s)
        ↓
Customer Selection (?agencyOfferId=)
        ↓
Public Booking Initiation (server-validated AgencyOfferId)
        ↓
Quote / Booking (Pending) boundary
```

### Slice readiness matrix

| Step | Ready? | Notes |
|------|--------|-------|
| TourProduct / Departure SoT | YES | Unchanged Tour ownership |
| AgencyOffer domain + persistence | YES | SalesChannel · DepartureScope · Suspend/Retire |
| Public eligibility filter | YES | Published+Listed+Active+Public + agency listing |
| Customer selection UX | YES | Tour detail selection · single auto · ≥2 required |
| Booking initiation binding | YES | Optional `agencyOfferId` · Agency SourceKind server-derived |
| Direct path without offer | YES | Preserved |
| Offer-aware Quote inputs | **PARTIAL** | Initiation still issues Quote by TourDeparture; Offer identity on Booking source only |
| Agency Portal offer ops UX | **NO** | Panel APIs exist; `/agency` shells do not manage offers yet |
| Commission / Settlement | **NO** (intentional) | Explicitly out of T001–T005 |

---

## 2. Multi-agency marketplace readiness

### What this slice proves

- One TourProduct can surface multiple AgencyOffers without inventing sellers.
- Customer selection is identity-only until Booking owns initiation.
- Booking remains SoT; FE ≠ SoT; client cannot forge Agency SourceKind.
- Booking ≠ Payment · Confirm shortcut absent · no fake KPIs.

### What it does **not** yet prove

- An agency operator can create/publish offers from Agency Portal UI.
- Pricing/Quote differs by selected Offer (margin/commission policy).
- Cross-agency settlement or commission accounting.
- End-to-end seeded multi-agency catalog for sellable density (DEMOFEED / catalog debt from P36 remains).

**Marketplace differentiator status:** **thin public vertical slice landed**; **operable B2B offer lifecycle incomplete**.

---

## 3. Architecture boundary assessment

| Boundary | Gate finding |
|----------|--------------|
| AgencyOffer ≠ TourDeparture | Preserved (scope modes; product match) |
| AgencyOffer ≠ Price / Quote | Preserved (Quote still Pricing-owned) |
| AgencyOffer ≠ Booking | Preserved (selection ≠ initiation ≠ confirm) |
| Booking ≠ Payment | Preserved |
| FE ≠ SoT | Preserved |
| Client SourceKind forge | Rejected on public initiation |
| Module boundaries | Booking consumes `IAgencyOriginContextQuery` contracts only |

No architecture redesign required by this gate.

---

## 4. Agency Portal status

| Area | Status |
|------|--------|
| P37 Agency shell / IA | READY_FOUNDATION (`/agency` routes) |
| Offer panel API (create/suspend/retire/…) | Exists in AgencyMarketplace |
| Offer management UI in Agency Portal | **Missing** |
| Commission / Settlement pages | Honest empty / direction shells — **not** financial engines |

Agency Portal is **ready to host** Offer management; it is **not** yet the operator surface for P38 offers.

---

## 5. Commission / Settlement need

**Recommendation: defer.**

Reasons:

1. Public slice already preserves commercial honesty without money theater.
2. Premature settlement would expand scope before operators can publish real offers.
3. Architect T005 review explicitly valued `Commission/Settlement = NOT IMPLEMENTED`.

Commission/Settlement Foundation should follow **after** Agency Offer ops UX exists and Offer→Quote policy inputs are defined.

---

## 6. Remaining gaps (ordered)

1. **Agency Portal Offer lifecycle UX** (create → submit → publish · Access-aware)
2. **Admin offer approval / cross-agency ops** (beyond foundation IA)
3. **Offer-aware Pricing/Quote inputs** (without inventing amounts in FE)
4. Catalog density / DEMOFEED naming debt (P36)
5. Commission / Settlement contracts (later)

---

## 7. Next path recommendation

### Candidates (Architect-named)

| Option | Focus |
|--------|--------|
| **A — P38 Commerce Depth** | Make the slice operable: Agency Offer management UX · Access · Admin approval · Quote offer inputs |
| **B — Commission/Settlement Foundation** | Financial relationship layer |
| **C — Experience Refinement** | Deepen Customer/Agency/Admin workflows unrelated to offer ops |

### Recommendation: **Option A — P38 Commerce Depth**

| Criterion | Rationale |
|-----------|-----------|
| Business value | Highest — without operator Offer UX, public selection stays empty/demo-risk |
| Architecture readiness | Panel APIs + Agency shell exist; thin UI/Access wiring is natural next |
| Risk of Option B now | High — financial complexity before real offer inventory |
| Risk of Option C now | Medium — polishes shells while commerce differentiator stays incomplete |

Suggested Commerce Depth themes (for Architect tasking — not auto-authorized):

1. Agency Portal Offer management (honest · no fake rates)
2. Access permissions for offer actors
3. Admin approval/publish policy surface
4. Quote/Pricing offer-aware inputs (still Pricing-owned)

---

## 8. Gate verdict

**`PASS WITH KNOWN LIMITATIONS`**

The P38 **public commerce slice** (plan → contracts → persistence → selection → booking initiation) is architecturally sound and landed on `main` at `80c6546`.

Known limitations are intentional incompleteness of Agency operational depth, Offer-aware Quote policy, and Commission/Settlement — they define **Commerce Depth**, not a rollback of this slice.

Do **not** treat this gate as authorization to implement the next slice until Architect issues the next downloadable `.task.md`.
