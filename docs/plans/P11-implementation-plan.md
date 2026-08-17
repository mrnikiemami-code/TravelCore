# P11 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P11-PLAN` |
| Phase | P11 — Foreign Package Tour / TourDeparture |
| Status | ACCEPTED (architect) · executing T001 |
| Baseline | `c351bf9` (`docs: P10 GATE acceptance evidence [TC-P10-GATE]` — **TC-P10-GATE** ACCEPTED; P10 COMPLETE) |
| Authoritative sources | `docs/ROADMAP.md` § P11 · transition map · Tour module boundaries · P09/P10 locks · ADR 0001 · ADR 0007–0008 · ADR 0011–0014 · architect Gate ACCEPT narrative (TourProduct ≠ TourDeparture) |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P11** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

> **Envelope note:** Authored from **repository SoT** + architect P10 Gate ACCEPT continuity (auto-start P11 PLAN). Under PIPELINE continuity, ceremonial confirms are **not required**. **No product code in PLAN task.**

---

## 1. Phase Purpose

P11 باید **اجرای زمان‌مند تور (TourDeparture)** را روی ماژول Tour موجود اضافه کند تا:

1. **TourProduct ≠ TourDeparture** به‌عنوان invariant قفل‌شده حفظ شود (محصول catalog ≠ اجرا با تاریخ/ظرفیت).
2. **TourDeparture** به‌عنوان aggregate جدا (تحت schema `tour`) با تاریخ شروع/پایان، ظرفیت، وضعیت اجرا، و پیوند منطقی به TourProduct.
3. پایهٔ **Transport / FlightSegment** (و در صورت قفل: Airport/Carrier facts) بدون زنده کردن inventory یا Booking.
4. پایهٔ **TourHotelOption / Stay** برای پکیج خارجی — بدون HotelBooking و بدون تصاحب Place Hotel catalog.
5. قوانین مسافر / occupancy / age / capacity در سطح Departure (نه Pricing engine کامل).
6. مرز شفاف با **Pricing (P12)** و **Booking** — Quote/Payment وارد نشود.
7. اعتبارسنجی با archetype **Foreign Package Tour Detail** (سطح صادرشده؛ polish کامل = P14).

P10 تحویل داد: Experience specialization + itinerary + ops + guide + media posture + publishability.  
P11 اضافه می‌کند: **Departure / package execution structures** — **بدون** scaffold ماژول جدید.

P11 **Pricing کامل (P12)** · **Booking/Payment** · **Search (P15)** · **Agency Marketplace (P13)** · **Public polish factory (P14)** نیست.

---

## 2. Starting Baseline

| Item | Value |
|------|--------|
| P10 Gate | `TC-P10-GATE` COMPLETE / ACCEPTED (`c351bf9`) |
| P10 evidence | [`P10-GATE-acceptance-evidence.md`](P10-GATE-acceptance-evidence.md) · [`P10-T009-hardening-and-evidence-pack.md`](P10-T009-hardening-and-evidence-pack.md) |
| P10 Plan | ACCEPTED · R1–R8 RESOLVED |
| Baseline HEAD | `c351bf9` |
| P00–P10 | COMPLETE |
| Tour schema | `tour` — TourProduct + Experience specialization/itinerary/ops/guide + CatalogStatus + Cover/Gallery |
| TourDeparture / FlightSegment / TourHotelOption | **Not implemented** (explicitly forbidden in P10) |

---

## 3. Authoritative Inputs

| Area | Sources |
|------|---------|
| Phase scope | `docs/ROADMAP.md` § P11 |
| Invariant | TourProduct ≠ TourDeparture (P09/P10 + architect) |
| Experience adjacency | P10 — Experience remains catalog/product facts; Departure is execution |
| Package specialization | P09-R1 — Package typed specialization may appear in P11 as issued |
| Place / Hotel | P07 — Place Hotel SoR; TourHotelOption ≠ Place ownership |
| Media / SEO | P06 / P05 / P09 — reuse; no StorageKey in Tour |
| Money | ADR money foundation — no commercial Pricing engine in P11 |
| Governance | ADR 0011–0014 · continuity |

---

## 4. Scope (In)

1. Extend Tour module for **TourDeparture** aggregate (identity, TourProductId link, schedule, capacity, status).
2. Optional **Package specialization** baseline if locked (typed specialization for TourKind.Package) — do not invent Experience-only fields onto Package.
3. **TransportSegment / FlightSegment** baseline facts (airports/carrier/flight number/cabin/baggage/local times) under locked ownership — no live inventory.
4. **TourHotelOption / stay plan** baseline for package departures — logical Place Hotel refs; no HotelBooking.
5. Passenger rules / occupancy / age policies / capacity on Departure as issued.
6. Travel requirements / passport-visa **hooks** only if locked (Visa module remains SoR for catalog).
7. Access-backed Admin baseline for Departure ops (job-based; no silo CRUD factory).
8. Public Foreign Package Detail composition hooks as issued (not P14 polish).
9. Architecture tests: Departure ≠ Product · no Booking/Pricing ownership · Place/Destination ownership intact.
10. Hardening evidence + `TC-P11-GATE`.

---

## 5. Non-Goals (Deferred)

| Deferred | Owner phase |
|----------|-------------|
| Commercial Pricing / Quote / Exchange conversion engine | **P12** |
| Booking / Payment / Inventory holds | Booking / later |
| Agency Marketplace ownership of offers | **P13** |
| Full public listing/search polish | **P14 / P15** |
| Live GDS / airline inventory | Out |
| HotelBooking product | Out |
| Replacing Experience itinerary with Departure | Forbidden |

---

## 6. Proposed Task Sequence

### TC-P11-PLAN — this document

### TC-P11-T001 — TourDeparture scaffolding

- **Purpose:** Introduce TourDeparture aggregate linked to TourProduct (logical ownership rules per **P11-R1**).
- **Forbidden:** Booking · Pricing · inventing R1.

### TC-P11-T002 — Schedule + timezone baseline

- Local start/end dates/times with NodaTime; timezone policy per lock.

### TC-P11-T003 — Capacity model

- Capacity / remaining / holds posture per **P11-R2** (no inventory engine).

### TC-P11-T004 — Departure status lifecycle

- Draft/Open/Closed/Cancelled (or locked enum) — ≠ CatalogStatus of TourProduct.

### TC-P11-T005 — FlightSegment / transport baseline

- Per **P11-R3**; airports/carrier as facts or refs — no live flight.

### TC-P11-T006 — TourHotelOption / stay baseline

- Per **P11-R4**; Place Hotel logical refs; no HotelBooking.

### TC-P11-T007 — Passenger / occupancy / age rules baseline

- Structured facts for later Pricing/Booking — not price calculation.

### TC-P11-T008 — Access + Admin Departure baseline

### TC-P11-T009 — Public Foreign Package Detail hooks

### TC-P11-T010 — Hardening + evidence

### TC-P11-GATE — Acceptance Gate

Exact task titles/order may be adjusted by architect on PLAN ACCEPT; Cursor must not invent skipped R# locks.

---

## 7. Open Decisions (must lock before dependent tasks)

| ID | Topic | Status | Notes |
|----|-------|--------|-------|
| **P11-R1** | Departure ownership / cardinality vs TourProduct | **RESOLVED** | TourDeparture ∈ Tour module; child execution aggregate; TourProduct ≠ TourDeparture; 0..N Departures per product; identity + product link only in T001 |
| **P11-R2** | Departure schedule / timezone model | **RESOLVED** | LocalDate Start/End + required IANA TimeZoneId; NodaTime; no DateTimeOffset SoT; Instant only where exact moments needed (TC-P11-T002) |
| **P11-R3** | Departure capacity ownership | **RESOLVED** | TourDeparture owns Min/Max Pax capacity rules; Booking owns reservation consumption later; no booked/available counts in P11 (TC-P11-T003) |
| **P11-R4** | Departure lifecycle status | **RESOLVED** | TourDepartureStatus: Draft/Published/Closed/Cancelled/Completed; ≠ CatalogStatus/SEO/Booking; transitions Draft→Published→Closed→Completed and Published→Cancelled (TC-P11-T004) |
| **P11-R5** | Departure transport segment ownership | **RESOLVED** | Descriptive TourDepartureTransportSegment (Sequence/Mode/Origin/Destination labels); Tour ≠ Flight; no airline/flight number/ticket/seat inventory (TC-P11-T005) |
| **P11-R6** | Departure accommodation option ownership | **RESOLVED** | TourDepartureAccommodationOption (logical PlaceId + Nights + BoardType); Place owns hotel identity; HotelBooking deferred; intentionally not named TourHotelOption (TC-P11-T006) |
| **P11-R7** | Departure passenger occupancy rules | **RESOLVED** | TourDeparturePassengerRule (MinimumAdults/ChildAllowed/InfantAllowed/MaximumPassengers); Booking owns actual travellers later (TC-P11-T007) |
| Pricing boundary | What P11 may store vs must DEFER to P12 | **UNRESOLVED** | Deferred (was draft-plan R5; architect remapped R5 → transport) |
| Booking boundary | Availability signal without Booking module | **UNRESOLVED** | Deferred (was draft-plan R6; architect remapped R6 → accommodation) |
| Flight / transport relation | Segment ownership; airport/carrier reference shape | **SUPERSEDED by R5** | Descriptive segments locked; real Flight domain deferred |
| Hotel option relation | TourHotelOption cardinality; Place Hotel link rules | **SUPERSEDED by R6** | AccommodationOption locked; TourHotelOption name avoided |

Under **ARCHITECT AUTONOMY**, normal alternatives are decided by architect without stopping the human; Cursor STOPs only on SoT contradiction / unsafe / missing external business fact / corruption.

---

## 8. Acceptance Strategy (Gate)

1. TourDeparture exists under Tour schema; TourProduct ≠ TourDeparture.
2. Capacity/status/schedule baseline under locked R#.
3. Flight/Hotel option shapes match locks; Place/Destination ownership intact.
4. No Pricing engine · no Booking · no Payment · no Search ownership.
5. Experience P10 structures untouched as SoR of Experience catalog facts.
6. Evidence + green suites.

---

## 9. Cursor Execution Rules

1. Prefer one product commit per task.
2. PowerShell: use `;` not `&&`.
3. Do not invent TourHotelOption/FlightSegment shapes before R# lock.
4. Do not start P12 product in P11.
5. After PLAN ACCEPT, Auto-Execute next task as issued.

---

## 10. Continuity

After `TC-P11-GATE` ACCEPT, continuity may auto-start **P12 PLAN** (Pricing) unless a real Stop Condition applies.
