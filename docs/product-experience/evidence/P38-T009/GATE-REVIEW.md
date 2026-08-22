# P38-T009 — Multi-Agency Commerce Gate Review

| Field | Value |
|-------|--------|
| Task-ID | `TC-P38-T009` |
| Gate kind | Multi-Agency Commerce Gate (review only · no features) |
| Date | 2026-08-22 |
| HEAD reviewed | `00e6076` |
| Status recommendation | **`PASS WITH KNOWN LIMITATIONS`** |
| Slice verdict | **`READY_COMMERCE_VERTICAL`** (public + agency ops + quote context) |

## Reviewed tasks (Architect ACCEPTED)

| Task | Scope | Verdict |
|------|--------|---------|
| T001 | Commerce foundation plan | ACCEPTED |
| T002 | AgencyOffer contracts | ACCEPTED |
| T003 | AgencyOffer persistence | ACCEPTED |
| T004 | Public offer selection | ACCEPTED |
| T005 | Booking Offer boundary | ACCEPTED |
| T006 | Commerce Slice Gate | ACCEPTED · Commerce Depth |
| T007 | Agency Offer Operations | ACCEPTED |
| T008 | Offer-aware Quote metadata | ACCEPTED |

---

## 1. Path under review

```text
TourProduct
    + AgencyOffer(s)
    + Customer Selection
    + Quote Context (CommercialContextAgencyOfferId)
    + Booking (Source.AgencyOffer)
```

| Step | Ready? | Notes |
|------|--------|-------|
| TourProduct / Departure SoT | YES | Unchanged |
| AgencyOffer persistence + lifecycle | YES | Channel · scope · suspend/retire |
| Public selection | YES | Eligibility + URL selection |
| Booking initiation Offer binding | YES | Server-validated · no SourceKind forge |
| Quote commercial context | YES | Metadata only · amounts from TourDeparture Price |
| Agency Portal Offer ops | YES (foundation) | Acting-agency ownership isolation |
| Admin approve/moderate UX | PARTIAL | API exists · portal UI not agency-facing |
| Offer-differentiated amounts | NO | Intentional — AgencyOffer ≠ Price |
| Commission / Settlement | NO | Deferred |

---

## 2. Architecture boundary confirmation

| Rule | Status |
|------|--------|
| AgencyOffer ≠ TourDeparture | PASS |
| AgencyOffer ≠ Price / Quote amounts | PASS |
| Pricing owns Quote | PASS |
| Booking owns Booking | PASS |
| Payment unchanged | PASS |
| Agency A ⊄ manage Agency B offers | PASS |
| FE ≠ SoT · Booking ≠ Payment | PASS |

No architecture redesign required.

---

## 3. Remaining gaps

1. **Admin Offer approval UX** (platform moderate surface)
2. **Access depth** for agency members beyond baseline permissions
3. **Offer-differentiated commercial rules** (when Architect authorizes Pricing policy inputs — still Pricing-owned)
4. **Commission / Settlement** (financial layer)
5. Catalog density / DEMOFEED naming debt (P36)

---

## 4. Next path recommendation

### Candidates (Architect-named)

| Option | Focus |
|--------|--------|
| **A — Commission / Settlement Foundation** | Financial relationship layer |
| **B — Commerce Depth** | Admin approval · Access · policy inputs · operability |
| **C — Experience Refinement** | Shell/UX polish outside commerce differentiator |

### Recommendation: **Option B — Commerce Depth**

| Criterion | Rationale |
|-----------|-----------|
| Business value | Highest remaining gap is operable platform moderation + Access depth before money |
| Architecture readiness | Vertical public+ops+quote context landed; financial layer still premature |
| Risk of Option A now | High — settlement before commercial rules / admin approval UX |
| Risk of Option C now | Medium — polish without finishing commerce operability |

Suggested Commerce Depth themes (Architect-tasked later):

1. Admin Offer approve/reject/publish ops UX
2. Access membership / agency actor depth
3. Optional Pricing commercial-rule inputs (still amount-owned by Pricing)
4. Only then revisit Commission/Settlement

---

## 5. Gate verdict

**`PASS WITH KNOWN LIMITATIONS`**

P38 Multi-Agency Commerce vertical is **READY_COMMERCE_VERTICAL** for:

- Public multi-offer selection
- Agency-owned offer operations
- Booking + Quote commercial context without amount ownership transfer

Known limitations define **Commerce Depth** (not a rollback). Commission/Settlement remains deferred.

Do **not** treat this gate as authorization to implement the next slice until Architect issues the next downloadable `.task.md`.
