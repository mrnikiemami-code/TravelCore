# P35 — Production Payment External Decision Intake

| Field | Value |
|-------|--------|
| Document | `docs/plans/P35-production-payment-external-decision-intake.md` |
| Task-ID | `TC-P35-T002` · updated by `TC-P35-T003` |
| Phase | P35 — Production Payment Provider Readiness |
| Status | **DECISIONS RECORDED** — dual Iran tracks + Stripe UAE; accounts still open |
| Production provider selection | **`BLOCKED_ON_PROVIDER_ACCOUNT_FACTS`** (design ready; no live adapters) |
| Nature | Documentation / governance only |

See also: [`P35-market-provider-decision-matrix.md`](P35-market-provider-decision-matrix.md)

---

## A. Required business inputs

| # | Input | Current value | Classification |
|---|-------|---------------|----------------|
| 1 | Target country/market for first production launch | **Iran** · **UAE** (both supplied; treated as **separate** markets) | REQUIRED-BLOCKER *(vendor per market still open)* |
| 2 | Legal/merchant entity contracting with provider | **Individual / natural person** | REQUIRED-BLOCKER *(eligibility unverified)* |
| 3 | Merchant account status | **Not yet obtained** | REQUIRED-BLOCKER |
| 4 | Settlement currency/currencies | **AED** (preference) | REQUIRED-BEFORE-PRODUCTION *(capability per PSP unverified)* |
| 5 | Traveler charge currency/currencies | **IRR** · **AED** · **USD** | REQUIRED-BEFORE-PRODUCTION |
| 6 | Bank/settlement constraints | **Iran:** Iranian rails · Bank Mellat preference/context · **UAE:** none additionally stated | REQUIRED-BEFORE-PRODUCTION |
| 7 | Preferred or already-contracted provider | **Iran:** Bank Mellat mentioned (not a locked PSP identity) · **UAE:** none · **Contract:** none | REQUIRED-BLOCKER |
| 8 | Refund expectations | **Full refund required** | REQUIRED-BEFORE-PRODUCTION |
| 9 | Partial refund requirement | **Not stated** — remain deferred unless business requires | OPTIONAL-PREFERENCE |
| 10 | Payment expiry/timeout expectations | **10 minutes** | REQUIRED-BEFORE-PRODUCTION |
| 11 | 3DS / authentication requirements | **Follow selected provider** | REQUIRED-BEFORE-PRODUCTION |
| 12 | Invoice/receipt expectations | **Not stated** | OPTIONAL-PREFERENCE |
| 13 | Production callback/domain availability | **Yes at real deployment** | REQUIRED-BEFORE-PRODUCTION |
| 14 | Regulatory/compliance constraints known by business | **None currently known** (≠ proof none exist) | REQUIRED-BLOCKER *(per-provider KYC still open)* |
| 15 | Expected transaction volume | **Not stated** | OPTIONAL-PREFERENCE |

---

## B. Decision status summary

| Class | Status |
|-------|--------|
| REQUIRED-BLOCKER | Merchant account · exact Iran PSP identity · UAE shortlist · individual eligibility · KYC per provider |
| REQUIRED-BEFORE-PRODUCTION | Currencies/settlement capability · refunds · expiry · 3DS · callback domain ops |
| OPTIONAL-PREFERENCE | Receipt · volume · partial refund |

---

## C. Provider selection gate (before T00x adapter design)

Still required before **Iran-specific** or **UAE-specific** adapter design:

1. Exact PSP/vendor identity (Iran: clarify Mellat preference)  
2. UAE shortlist or contracted vendor  
3. Individual merchant onboarding eligibility per market  
4. Merchant account obtained or clear path  
5. Settlement matrix verified (esp. AED preference vs Iranian rails)

---

## D. Safe work while blocked

Provider-agnostic docs/tests only if Architect authorizes — see decision matrix §8.

---

## E. Forbidden until provider decision

- Selecting vendor by guess (including assuming Behpardakht from “Bank Mellat”)
- Credentials / SDK / `NamedProductionAdapterImplemented=true`
- Treating sandbox as production
- Single-provider claim for Iran+UAE without evidence

---

## F. Project state record

```text
PRODUCTION PROVIDER SELECTION:
BLOCKED_ON_PROVIDER_ACCOUNT_FACTS

IRAN TRACKS (no final pick):
Behpardakht Mellat · Zarinpal

UAE TRACK:
Stripe (preferred)

STRATEGY_POSTURE:
MARKET_SPECIFIC_PROVIDER_ADAPTERS
LAUNCH: parallel (One Writer for code)
```
