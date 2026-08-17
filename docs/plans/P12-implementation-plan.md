# P12 Implementation Plan

| Field | Value |
|-------|--------|
| Plan-ID | `TC-P12-PLAN` |
| Phase | P12 — Pricing |
| Status | DRAFT — awaiting architect ACCEPT |
| Baseline | `6f7ea12` (`docs: P11 acceptance gate evidence [TC-P11-GATE]` — **TC-P11-GATE** ACCEPTED; P11 COMPLETE) |
| Authoritative sources | `docs/ROADMAP.md` § P12 · transition map · Tour/Departure boundaries · P09–P11 locks · ADR money foundation · ADR 0001 · ADR 0011–0014 · architect P11 Gate ACCEPT narrative (Price ≠ Quote ≠ Booking Amount) |
| Backend root | `src/backend` |
| Frontend root | `src/frontend/web` |

این سند **نقشهٔ اجرایی معتبر P12** است. پیاده‌سازی محصول در این سند انجام نمی‌شود؛ فقط Taskهای اجرایی را برای Cursor تعریف می‌کند.

> **Envelope note:** Authored from **repository SoT** + architect P11 Gate ACCEPT continuity (auto-start P12 PLAN). Under PIPELINE continuity, ceremonial confirms are **not required**. **No product code in PLAN task.**

---

## 1. Phase Purpose

P12 باید **موتور قیمت‌گذاری تور** را معرفی کند تا:

1. **Price ≠ Quote ≠ Payment / Booking Amount** به‌عنوان invariants معماری قفل شود.
2. قیمت‌گذاری روی **TourDeparture** (و در صورت قفل: قواعد مشترک محصول) بدون ادغام با Booking/Payment.
3. **Currency / Money** foundation موجود پلتفرم reuse شود — بدون خاموش‌کردن همهٔ قیمت‌ها به یک ارز.
4. مؤلفه‌های قیمت (PriceComponent) · نرخ (TourRate در صورت قفل) · مسافر/occupancy/age commercial rules در سطح Pricing (نه Reservation).
5. **Quote** به‌عنوان snapshot قابل‌انقضا در صورت قفل — بدون Settlement/Payment capture.
6. مرز شفاف با **Booking** (بعداً) و **Agency Marketplace (P13)** و **Search (P15)**.

P11 تحویل داد: TourDeparture + Admin + Public Published hooks (Published ≠ Bookable).  
P12 اضافه می‌کند: **Pricing structures / quotes** — **بدون** Booking CTA، بدون Payment، بدون Settlement.

P12 **Booking/Payment** · **Agency Marketplace (P13)** · **Public polish (P14)** · **Search (P15)** نیست.

---

## 2. Starting Baseline

| Item | Value |
|------|--------|
| P11 Gate | `TC-P11-GATE` COMPLETE / ACCEPTED (`6f7ea12`) |
| P11 evidence | [`P11-GATE-acceptance-evidence.md`](P11-GATE-acceptance-evidence.md) · [`P11-T010-hardening-and-evidence-pack.md`](P11-T010-hardening-and-evidence-pack.md) |
| P11 Plan | ACCEPTED · R1–R8 RESOLVED |
| Baseline HEAD | `6f7ea12` |
| P00–P11 | COMPLETE |
| TourDeparture | Present · Published public hooks · Admin Access-backed |
| Money platform | Existing ADR money / Currency foundation (reuse — do not reinvent) |
| Booking / Payment / Quote product | **Not implemented** |

---

## 3. Non-goals (explicit)

1. Booking engine / reservation / hold / inventory consumption.
2. Payment capture / refund / settlement / ledger.
3. Agency marketplace commercial ownership (P13).
4. Search indexing of prices (P15).
5. Public booking CTA / checkout UX (P14+).
6. Silent single-currency conversion of all commercial amounts.
7. Inventing unlocked R# closures — open decisions stay OPEN until architect lock.

---

## 4. Task sequence (proposed)

### TC-P12-PLAN — this document

### TC-P12-T001 — Pricing module / ownership scaffolding
- Purpose: Introduce Pricing ownership surface (module or Tour-owned pricing schema per **P12-R1**).
- Forbidden: Booking/Payment types.

### TC-P12-T002 — Money / Currency baseline binding
- Bind ADR money types; multi-currency posture per **P12-R2**.

### TC-P12-T003 — PriceComponent model
- Structured components (base / fees / taxes as locked) — not opaque blob.

### TC-P12-T004 — Departure pricing attachment
- Link pricing rules/components to TourDeparture per **P12-R3** (product-level pricing DEFER unless locked).

### TC-P12-T005 — Passenger / occupancy / age commercial rules
- Pricing-side category rules — distinct from P11 passenger *acceptance* rules.

### TC-P12-T006 — Quote baseline
- Quote / expiration / snapshot per **P12-R4** — ≠ Payment ≠ Booking Amount.

### TC-P12-T007 — Access + Admin Pricing baseline
- Permissions + Admin job for rates/components (Server Component First).

### TC-P12-T008 — Public / composition hooks (read-only price facts)
- Optional published price display hooks — no book/pay CTA.

### TC-P12-T009 — Hardening + evidence

### TC-P12-GATE — Acceptance Gate

---

## 5. Open decisions (must not invent)

| ID | Topic | Status | Notes |
|----|-------|--------|-------|
| **P12-R1** | Pricing ownership (new module vs Tour-owned schema) | **UNRESOLVED** | Architect must lock before T001 product |
| **P12-R2** | Mixed-currency / conversion policy SoT | **UNRESOLVED** | ROADMAP: never silent single-currency wipe |
| **P12-R3** | Pricing attaches to Departure vs Product vs both | **UNRESOLVED** | Architect P11 Gate narrative focused Departure |
| **P12-R4** | Quote model (required in P12? expiration? snapshot fields) | **UNRESOLVED** | Price ≠ Quote ≠ Booking Amount |
| **P12-R5** | Exchange rate source / authority | **UNRESOLVED** | Defer invention |
| Agency override of rates | Marketplace (P13) vs P12 | **UNRESOLVED** | Prefer DEFER to P13 |

---

## 6. Architecture invariants (carry forward)

1. TourProduct ≠ TourDeparture.
2. Published Departure ≠ Bookable.
3. Price ≠ Quote ≠ Payment / Booking Amount.
4. Tour ≠ Flight ownership · Tour ≠ HotelBooking.
5. Money foundation = platform ADR — do not invent parallel money types.
6. No Booking/Payment/Search engines in P12.

---

## 7. Continuity

After `TC-P12-GATE` ACCEPT, continuity may auto-start **P13 PLAN** (Agency Marketplace) unless a real Stop Condition applies.

---

## 8. PLAN acceptance criteria

- [x] Phase purpose + non-goals explicit
- [x] Task sequence proposed without product code
- [x] Open decisions listed (R1–R5) — no invention
- [x] Baseline = P11 Gate ACCEPT commit
- [ ] Architect ACCEPT + Auto-Execute first product task
