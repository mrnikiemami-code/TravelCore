# P35 — Provider Research Brief: Iran + UAE

| Field | Value |
|-------|--------|
| Document | `docs/plans/P35-provider-research-iran-uae.md` |
| Task-ID | `TC-P35-T004` |
| Research date | **2026-08-21** |
| Nature | Research / documentation only — **no vendor selection · no SDK · no credentials** |
| Baseline | `bdb0794` · `TC-P35-T003` ACCEPTED |

## Source-quality rules

| Tier | Allowed use |
|------|-------------|
| A — Official provider / bank / Shaparak docs & portals | Primary claims |
| B — Official Stripe/Telr developer docs | Primary for UAE API surfaces |
| C — Reputable secondary (integrator how-tos, blog comparisons) | Context only; must not override A/B |
| D — Community / GitHub samples | Integration hints only |

If not supported by Tier A/B → mark **UNKNOWN** or **PARTIALLY_VERIFIED**.

---

## Cross-market conclusion (first)

**No researched provider credibly supports BOTH Iran Shaparak rails and UAE card acquiring under one contract for this business posture.**

Retain **market-specific provider adapters** behind existing `IPaymentProviderGateway`.

---

## Research track A — Iran

### A.1 Bank Mellat preference → practical path

| Claim | Status | Evidence |
|-------|--------|----------|
| Online IPG associated with Mellat is operated via **Behpardakht Mellat** PGW (not “call the bank branch as API”) | **PARTIALLY_VERIFIED** | English PGW user manual (Behpardakht Mellat Co.) hosted copies; Shaparak migration notes naming Behpardakht |
| Production WSDL / startpay on Shaparak host `bpm.shaparak.ir` | **VERIFIED** (from public tech docs copies) | `https://bpm.shaparak.ir/pgwchannel/services/pgw?wsdl` · `…/startpay.mellat` ([Sepidan PDF migration](https://sepidan.net/sites/default/files/content/gateways/bank-mellat-shaparak.pdf)) |
| SOAP methods: `bpPayRequest`, `bpVerifyRequest`, `bpSettleRequest`, `bpInquiry*`, `bpRefundRequest` | **VERIFIED** (API surface in manuals) | [Mellat PGW tech doc copies](https://sepidan.net/sites/default/files/content/gateways/mellat_pgw_tech%20doc_ver%201.15_fa.pdf) · English manual copies |
| Browser return posts ResCode / SaleReferenceId; merchant must **verify** server-side | **VERIFIED** | Same manuals (callback ≠ success alone — aligns with TravelCore trust boundary) |
| Merchant portal for IPG request: `my.behpardakht.com`; true/legal person paths exist in onboarding UX | **PARTIALLY_VERIFIED** | Secondary integrator guides citing portal ([example](https://rayanmoshaver.ir/behpardakht-ipg/)); **confirm on live portal** |
| Individual merchant can complete onboarding without e-namad / tax / site prerequisites | **UNKNOWN** | Secondary sources claim tax registration + e-namad + business permit required — **must confirm with Behpardakht** |
| Settlement currency can be **AED** from Iranian IPG | **UNKNOWN** (likely false for Shaparak IRR rails) | No Tier A evidence of AED settlement; business preference conflicts with typical IRR acquiring |
| Exact 10-minute payment expiry | **UNKNOWN** | Manuals discuss session/ref lifecycle; closest timeout **must be confirmed with provider** |
| Sandbox/test terminal availability | **PARTIALLY_VERIFIED** | Common practice; exact entitlement **UNKNOWN** until merchant terminal issued |

**Interpretation:** “Bank Mellat” preference most plausibly maps to **Behpardakht Mellat IPG** as the acquiring technical path — **not** automatic contract selection and **not** proof of individual eligibility.

### A.2 Iran candidate table (≤3)

| Candidate / path | Merchant eligibility (individual) | Charge IRR | Settlement | Refund | Callback verify / inquiry | Hosted redirect | Sandbox | Overall |
|------------------|-----------------------------------|------------|------------|--------|---------------------------|-----------------|---------|---------|
| **Behpardakht Mellat IPG** | PARTIALLY_VERIFIED (portal offers حقیقی) | VERIFIED (IRR rails) | UNKNOWN vs AED pref | VERIFIED API surface (`bpRefundRequest`) — live eligibility UNKNOWN | VERIFIED verify/inquiry methods | VERIFIED redirect PGW | PARTIALLY_VERIFIED | **Shortlist #1** |
| **Zarinpal** (aggregator) | PARTIALLY_VERIFIED (common حقیقی product) | VERIFIED IRR | UNKNOWN AED | VERIFIED docs exist; **feature page notes refund temporarily disabled by CBI order** ([zarinpal.com/features/refund](https://www.zarinpal.com/features/refund/)) | VERIFIED REST/GraphQL docs | VERIFIED | VERIFIED marketing/docs | **Shortlist #2** (refund risk) |
| **Other Shaparak PSP / bank IPG** (e.g. Saman, etc.) | UNKNOWN | VERIFIED class (Iran rails) | UNKNOWN AED | UNKNOWN per vendor | UNKNOWN per vendor | PARTIALLY_VERIFIED class | UNKNOWN | **Shortlist #3 research slot** — no vendor locked |

### A.3 Iran readiness

`STILL_BLOCKED_ON_PROVIDER_FACTS`  
(not READY_FOR_BUSINESS_PROVIDER_SELECTION until merchant account path + AED conflict + individual docs confirmed)

---

## Research track B — UAE

### B.1 Candidate table (≤3)

| Candidate | Individual merchant | AED/USD charge | AED settlement | Refund | Hosted / Checkout | Webhook verify | Sandbox | Overall |
|-----------|---------------------|----------------|----------------|--------|-------------------|----------------|---------|---------|
| **Stripe (UAE)** | UNKNOWN (entity/activity restrictions common in secondary sources) | PARTIALLY_VERIFIED (Checkout multi-currency; confirm AED settlement account) | PARTIALLY_VERIFIED | VERIFIED platform capability | VERIFIED [Checkout](https://stripe.com/ae/payments/checkout) | VERIFIED [Webhooks](https://docs.stripe.com/webhooks) | VERIFIED test mode | **Shortlist #1** for tech-friendly path — **eligibility UNKNOWN** |
| **Telr** | UNKNOWN | PARTIALLY_VERIFIED (regional gateway) | PARTIALLY_VERIFIED | PARTIALLY_VERIFIED (webhook events include refunds) | VERIFIED [payment-page](https://docs.telr.com/reference/payment-page) | VERIFIED [webhook](https://docs.telr.com/reference/webhook) | PARTIALLY_VERIFIED | **Shortlist #2** |
| **Network International / N-Genius** or **PayTabs** | UNKNOWN | PARTIALLY_VERIFIED (regional acquirer / MENA) | PARTIALLY_VERIFIED | UNKNOWN official detail here | PARTIALLY_VERIFIED (hosted products exist) | PARTIALLY_VERIFIED | UNKNOWN | **Shortlist #3** — needs direct sales/docs confirmation |

Secondary comparison blogs (Tier C) discuss fees/onboarding speed — **not** used as eligibility proof.

### B.2 UAE critical unknowns

- Whether **natural person** (no UAE trade license / mainland or free-zone company) can obtain a merchant account on any shortlisted vendor
- Exact AED settlement + USD charge matrix for chosen vendor
- 10-minute session expiry support
- Travel / tourism MCC approval

### B.3 UAE readiness

`STILL_BLOCKED_ON_PROVIDER_FACTS`  
(shortlist exists; business selection needs eligibility confirmation)

---

## Shared TravelCore fit notes

| Requirement | Iran shortlist | UAE shortlist |
|-------------|----------------|---------------|
| Fits `IPaymentProviderGateway` | Yes (redirect + verify + query/refund ports) | Yes (Checkout/HPP + signed webhooks) |
| Browser return ≠ success | Enforced by verify/settle pattern | Enforced by webhook/session retrieve pattern |
| Full refund required | Mellat API surface yes; Zarinpal currently risky | Stripe/Telr platform yes — contract UNKNOWN |
| Same provider both markets | **Not verified** | **Not verified** |

---

## Critical unknowns requiring provider / merchant contact

1. Behpardakht: individual onboarding checklist + whether tourism site qualifies  
2. Settlement: can any Iran path settle **AED**? (likely need separate UAE settlement entity)  
3. Zarinpal: current refund availability under CBI restriction  
4. Stripe AE / Telr / NI: individual vs company mandatory documents  
5. Exact payment session TTL closest to 10 minutes per vendor  
6. Production callback domain binding / domain allowlisting (Mellat manuals mention domain controls)

---

## Recommended shortlist (no selection)

| Market | Shortlist (research only) |
|--------|---------------------------|
| Iran | 1) Behpardakht Mellat IPG · 2) Zarinpal · 3) Other Shaparak PSP (unspecified — further research) |
| UAE | 1) Stripe UAE · 2) Telr · 3) Network International **or** PayTabs (confirm which fits MCC) |

**Do NOT treat shortlist order as Architect/business selection.**

---

## Readiness classification by market

| Market | Classification |
|--------|----------------|
| Iran | `STILL_BLOCKED_ON_PROVIDER_FACTS` |
| UAE | `STILL_BLOCKED_ON_PROVIDER_FACTS` |
| Combined | Multi-adapter posture retained |

Not yet: `READY_FOR_BUSINESS_PROVIDER_SELECTION` (needs merchant-account answers)

---

## Recommended next Architect task

**`TC-P35-T005` — Business provider selection worksheet**  
Present shortlists + required contact questions; Architect/user picks **one Iran path** and **one UAE path** (or explicitly defers a market).  
Still **no** SDK/credentials until a later implementation task after selection.

Alternate: merchant legal-entity upgrade decision (individual → company) if eligibility fails.
