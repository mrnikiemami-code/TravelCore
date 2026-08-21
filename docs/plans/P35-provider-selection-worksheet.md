# P35 — Business Provider Selection Worksheet

| Field | Value |
|-------|--------|
| Document | `docs/plans/P35-provider-selection-worksheet.md` |
| Task-ID | `TC-P35-T005` |
| Status | **ACCEPTED path** — user answers recorded 2026-08-21 · see T006 design |
| Nature | Decision support only — **no vendor selected by Cursor** |
| Sources | Facts only from `P35-provider-research-iran-uae.md` (T004) |

---

## 1. Current business profile

| Item | Value |
|------|--------|
| Markets | Iran + UAE (separate) |
| Merchant posture | Individual / natural person |
| Merchant account | Not yet obtained |
| Charge currencies | IRR · AED · USD |
| Settlement preference | AED |
| Iran context | Bank Mellat preference (research → Behpardakht IPG path) |
| Refund | Required (full) |
| Payment expiry target | 10 minutes |
| 3DS / auth | Provider-dependent |
| Production HTTPS callback | Available at real deployment |

Architecture posture (not a vendor pick): **MARKET_SPECIFIC_PROVIDER_ADAPTERS**

---

## 2. Iran decision table

| Candidate | Mellat fit | IRR | Refund | API maturity | Callback verify | Inquiry | Individual eligibility | Prerequisites | Sandbox | Major unknowns | Confidence |
|-----------|------------|-----|--------|--------------|-----------------|---------|------------------------|---------------|---------|----------------|------------|
| **Behpardakht Mellat IPG** | Strongest research fit | Yes | API surface yes | SOAP PGW mature | Yes (verify/settle) | Yes | PARTIAL (portal حقیقی) | tax / e-namad / site often required (secondary) | PARTIAL | AED settlement; eligibility; 10m TTL; contract | Medium |
| **Zarinpal** | Weak Mellat fit | Yes | Docs yes; **live refund may be CBI-disabled** | REST/GraphQL | Yes | Yes | PARTIAL | Panel onboarding | Yes | Refund live status; AED | Medium− |
| **Other Shaparak PSP** | Unknown | Yes class | Unknown | Unknown | Unknown | Unknown | Unknown | Unknown | Unknown | Entire vendor identity | Low |

---

## 3. UAE decision table

| Candidate | AED/USD charge | AED settlement | Refund | Hosted checkout | Webhook verify | 3DS | Individual eligibility | Prerequisites | Sandbox | Major unknowns | Confidence |
|-----------|----------------|----------------|--------|-----------------|----------------|-----|------------------------|---------------|---------|----------------|------------|
| **Stripe UAE** | PARTIAL | PARTIAL | Platform yes | Checkout yes | Signature webhooks | Platform | UNKNOWN | Often company/tech activity | Test mode | Individual OK? tourism MCC | Medium |
| **Telr** | PARTIAL | PARTIAL | Webhook events | HPP yes | SHA1 check webhooks | Provider | UNKNOWN | Merchant admin / store | PARTIAL | Individual OK? fees contract | Medium |
| **Network International / PayTabs** | PARTIAL | PARTIAL | Unknown here | Hosted products exist | PARTIAL | Provider | UNKNOWN | Sales / MID process | Unknown | Which brand + eligibility | Low−Medium |

---

## 4. Decision questions (Architect / user — short answers)

### Iran

**A.** First Iran path priority?  
`1` Bank Mellat / Behpardakht · `2` Aggregator (Zarinpal) · `3` No preference / research further

**B.** Willing to create merchant/tax/e-namad/business prerequisites for chosen Iran path?  
`Yes` / `No` / `Need help`

**C.** Is **IRR settlement** acceptable for Iran even if global preference is AED?  
`Yes` / `No` / `Only if AED possible`

### UAE

**D.** Next UAE path priority?  
`1` Stripe · `2` Telr · `3` Network International / PayTabs · `4` No preference / compare further

**E.** Willing to establish company/legal merchant structure if individual onboarding is unavailable?  
`Yes` / `No` / `Defer UAE`

**F.** Is AED settlement **mandatory** for UAE?  
`Yes` / `No` / `Preferred only`

### Rollout

**G.** Which market first?  
`Iran` / `UAE` / `Both parallel` (docs tracks only; code still One Writer)

---

## 5. Architecture recommendation

Assume **MARKET_SPECIFIC_PROVIDER_ADAPTERS** unless later evidence proves a shared provider is viable.  
Do **not** assume one provider for Iran + UAE.

---

## 6. Next authorization rules

| User choice | Architect may authorize next |
|-------------|------------------------------|
| Iran first | Iran provider-specific verification/design `.task.md` for chosen path |
| UAE first | UAE provider-specific verification/design `.task.md` |
| Both parallel | Two **docs** tracks OK; implementation remains sequential (One Writer) |

---

## 7. Stop condition

If merchant/legal onboarding facts remain unknown → **no production adapter implementation**.  
Architecture/design research may continue only via authorized `.task.md`.

---

## Recorded user answers (2026-08-21)

| Q | Answer (normalized) |
|---|---------------------|
| A Iran path | **Both** Behpardakht Mellat **and** Zarinpal (evaluate; no final pick) |
| B Iran prerequisites | **Yes** (willing) |
| C Settlement | Multi-currency OK; do not assume AED on Iranian rails |
| D UAE path | **Stripe** |
| E UAE company if needed | **Yes** |
| F AED mandatory UAE | **No** |
| G Launch | **Both parallel** |

See `P35-provider-specific-design-iran-uae.md` (TC-P35-T006).
