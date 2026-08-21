# P37-GATE — Experience Platform Foundation Review

| Field | Value |
|-------|--------|
| Gate-ID | `TC-P37-GATE` |
| Date | 2026-08-21 |
| Type | Architecture / experience review only (no P38 implementation) |
| Status recommendation | **`PASS WITH KNOWN LIMITATIONS`** |
| Foundation verdict | **`READY_FOUNDATION`** across four surfaces |
| HEAD reviewed | `5464118` |

## Reviewed tasks

| Task | Status | Evidence |
|------|--------|----------|
| T001 Experience Architecture | ACCEPTED | `docs/plans/P37-experience-architecture-review.md` |
| T002 Customer Dashboard | ACCEPTED · READY_FOUNDATION | `docs/product-experience/evidence/P37-T002/` |
| T003 Agency Portal | ACCEPTED · READY_FOUNDATION | `docs/product-experience/evidence/P37-T003/` |
| T004 Admin Console | ACCEPTED · READY_FOUNDATION | `docs/product-experience/evidence/P37-T004/` |

---

## 1. Experience surface assessment

| Surface | Route | Separation | DS reuse | Workflow maturity |
|---------|-------|------------|----------|-------------------|
| Public Marketplace | `/[locale]/…` | Merchandising / discovery | Yes | Commerce path exists (P33–P36); still PARTIALLY_SELLABLE |
| Customer Dashboard | `/[locale]/me` | Traveler account — not admin | Yes (CustomerShell) | Foundation shells + honest empties |
| Agency Portal | `/[locale]/agency` | B2B sales — not public+role | Yes (AgencyShell · accent) | Foundation IA; catalog not agency-offer wired |
| Admin Console | `/[locale]/admin` | Ops — workflow-oriented | Yes (OpsConsoleShell · primary) | Foundation IA + workflow direction cards |

**Consistency:** Shared AdminShell primitives + Surface/Text; distinct chrome (traveler / B2B accent / ops primary).

**Navigation:** Four separate IAs; cross-links exist without collapsing surfaces into one role toggle.

**Maturity:** Shell + IA + honesty posture are in place. Operational depth (live queues, agency offers, full publish wizards) is intentionally incomplete.

---

## 2. Architecture boundary assessment

| Boundary | Gate finding |
|----------|--------------|
| Identity ≠ Party ≠ Access | Preserved in copy + nav; no fake hardcoded role matrices |
| Customer ≠ Agency ≠ Admin | Confirmed by separate routes/shells |
| Public ≠ Backoffice | Public merchandising vs `/admin` ops |
| Booking ≠ Payment | Preserved from P33–P34 lineage; dashboards do not invent confirmations |
| Payment Success ≠ Auto Confirm | Not violated in P37 foundations |
| FE ≠ Source of Truth | Honest empties; no fake booking/payment/agency KPIs |

No architecture redesign performed in P37 foundations.

---

## 3. Commercial platform readiness

| Capability | Ready as foundation? | Notes |
|------------|----------------------|-------|
| B2C marketplace | **Partial** | Public + Customer shells; sell path from P36 still PARTIALLY_SELLABLE |
| B2B agency distribution | **Foundation only** | Agency IA exists; multi-agency offers not modeled in UX yet |
| Operational management | **Foundation only** | Admin workflow direction; grids/permissions incomplete |

P37 unlocks the **product surface topology** required for a commerce platform. It does **not** yet deliver the multi-agency commercial differentiator end-to-end.

---

## 4. Remaining experience gaps

### Customer
- Trip management depth beyond booking links
- Live documents / notifications / passenger workflows
- Deeper post-purchase continuity

### Agency
- **Multi-agency selling model** (core gap)
- Agency Offers over shared TourProduct
- Real commission / settlement contracts (not fake numbers)
- Agency-scoped booking/customer rosters

### Admin
- Full operational grids / filters / bulk actions beyond pattern board
- End-to-end publish wizards (direction only today)
- Real permission wiring
- Content operations depth

### Cross-cutting
- Design System depth (tables/toolbars still feature-local)
- Catalog DEMOFEED naming debt from P36 remains

---

## 5. Next strategic phase recommendation

### Candidates

| Option | Focus |
|--------|--------|
| **A** | P38 — Experience Depth (deepen Customer/Agency/Admin workflows on existing shells) |
| **B** | P38 — Multi-Agency Commerce (Tour Product + Multiple Agencies + Agency Offers + Customer Selection) |

### Recommendation: **Option B — P38 Multi-Agency Commerce**

| Criterion | Rationale |
|-----------|-----------|
| **Business value** | Highest — realizes the stated marketplace differentiator (“one tour, multiple agencies”) rather than polishing empty shells |
| **Architecture readiness** | Surfaces + boundaries exist; Agency Portal and Admin Agency Management are ready to host Offer/Access work without collapsing into Public+Role |
| **Implementation risk** | Medium — requires careful domain contracts (Agency Offer ≠ Public Tour card; commission honesty). Prefer thin vertical slice over full settlement engine |

**Option A** remains valuable as a **follow-on / interleaved** stream after a Multi-Agency commerce slice lands — otherwise depth work risks decorating a single-agency mental model.

### Explicit critical direction (locked)

```text
TravelCore = multi-agency travel marketplace

Tour Product
    + Multiple Agencies
    + Agency Offers
    + Customer Selection
```

P38 must not regress to “one agency owns the public tour page.”

---

## 6. Gate verdict

**`PASS WITH KNOWN LIMITATIONS`**

P37 Experience Platform Foundation is accepted as complete for shell/IA/honesty goals. Known limitations are depth and multi-agency commercial wiring — expected for foundation — and they define P38.

Do **not** treat this gate as authorization to implement P38 until Architect issues the next `.task.md`.
