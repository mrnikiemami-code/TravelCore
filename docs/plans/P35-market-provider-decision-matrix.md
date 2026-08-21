# P35 — Market Provider Decision Matrix

| Field | Value |
|-------|--------|
| Document | `docs/plans/P35-market-provider-decision-matrix.md` |
| Task-ID | `TC-P35-T003` |
| Phase | P35 — Production Payment Provider Readiness |
| Status | **PROPOSED / Cursor PASS** — awaiting Architect ACCEPT |
| Nature | Architecture / business-input normalization only |
| Baseline | `1d8b1a2` · business inputs supplied after T002 ACCEPT |

**Forbidden assumptions:** Bank Mellat ≠ automatic Behpardakht / PSP identity · Iran rails ≠ UAE rails · one provider for both markets · “no known regulation” ≠ none exist.

---

## 1. Supplied business inputs (normalized)

| Input | Value |
|-------|--------|
| Launch markets | **Iran** · **United Arab Emirates** (separate) |
| Merchant / legal posture | **Individual / natural person** |
| Merchant account | **Not yet obtained** |
| Traveler charge currencies | **IRR** · **AED** · **USD** |
| Settlement preference | **AED** |
| Iran bank preference/context | **Bank Mellat** (Iran only; meaning unresolved) |
| Iran banking constraint | Iranian banking / payment rails |
| UAE banking constraint | None additionally stated |
| Refunds | **Full refund required** |
| Payment expiry | **10 minutes** (or closest provider-compatible) |
| 3DS / auth | Follow selected provider |
| Production HTTPS callback | Available at real deployment |
| Known regulatory constraints (business) | None currently known |

---

## 2. Shared requirements (both markets)

- Hosted / redirect flow preferred (no card PAN in TravelCore)
- Server-verifiable callback/webhook
- Amount/currency match vs Payment execution snapshot
- Idempotent replay
- Full refund capability declared honestly
- ~10 minute payment expiry (or documented closest behavior)
- Existing `IPaymentProviderGateway` adapter model
- Booking ConfirmIfEligible remains confirmation SoT
- Sandbox stays non-production only

---

## 3. Iran requirements

| Topic | Requirement |
|-------|-------------|
| Rails | Iranian payment rails |
| Charge | IRR (and possibly others only if provider allows — not assumed) |
| Settlement | Prefer AED stated globally — **must verify** if Iranian PSP can settle AED to individual merchant |
| Preference | Bank Mellat **context only** — does **not** lock Behpardakht Mellat or any PSP without contract |
| Onboarding | Individual merchant eligibility **unverified** |
| Refund | Full refund required |
| Expiry | 10 minutes target |
| Auth | Provider-dependent |
| Callback | HTTPS production domain |

**Iran readiness:** `BLOCKED_PENDING_PROVIDER_RESEARCH` · branch lean **D** until exact PSP/contract + individual eligibility + settlement reality known.

---

## 4. UAE requirements

| Topic | Requirement |
|-------|-------------|
| Charge | AED · USD (IRR not assumed for UAE) |
| Settlement | AED preference |
| Vendor | **No UAE vendor named** in SoT — do not invent |
| Onboarding | Individual merchant eligibility **unverified** |
| Refund | Full refund required |
| Expiry | 10 minutes target |
| Auth / 3DS | Provider-dependent |
| Callback | HTTPS production domain |

**UAE readiness:** `BLOCKED_PENDING_PROVIDER_RESEARCH` · branch lean **D** / parallel research — **not** READY_FOR_PROVIDER_SPECIFIC_DESIGN_UAE until shortlist + eligibility exist.

---

## 5. Single vs multi-provider assessment

| Classification | Chosen? |
|----------------|---------|
| ONE_PROVIDER_POSSIBLE_BUT_UNVERIFIED | No — Iran rails + UAE multi-currency settlement make single-vendor claim unsafe |
| MULTI_PROVIDER_LIKELY | **Yes (default posture)** |
| MARKET_SPECIFIC_PROVIDER_REQUIRED | **Likely** for Iran vs UAE |
| BLOCKED_PENDING_PROVIDER_RESEARCH | **Yes** for naming/implementing any production adapter |

Architecture must remain **multi-adapter capable** behind existing gateway (already true from P20/P34).

---

## 6. Remaining true blockers

| Blocker | Markets |
|---------|---------|
| Exact Iranian PSP/provider contract | Iran |
| Meaning of “Bank Mellat” (bank relation vs Behpardakht vs other) | Iran |
| UAE provider/vendor shortlist | UAE |
| Individual merchant onboarding eligibility | Both |
| Merchant account creation | Both |
| Exact settlement capabilities (esp. AED settlement from Iranian rails) | Both / Iran critical |
| KYC / regulatory constraints per provider | Both |

“None currently known” ≠ proof none exist.

---

## 7. Pipeline branch decision

| Market | Branch |
|--------|--------|
| Iran | **D. BLOCKED_ON_MERCHANT/PROVIDER_FACTS** (+ research before A) |
| UAE | **D. BLOCKED_ON_MERCHANT/PROVIDER_FACTS** (+ research before B) |
| Combined | **C. READY_FOR_PARALLEL_PROVIDER_RESEARCH** is the only safe next *authorized* direction once Architect files it — **not** auto-started |

Not ready for:

- `READY_FOR_PROVIDER_SPECIFIC_DESIGN_IRAN`
- `READY_FOR_PROVIDER_SPECIFIC_DESIGN_UAE`

---

## 8. Safe provider-agnostic work (proposals only)

May be Architect-authorized later without picking a vendor:

- Multi-provider selection / routing policy design (docs)
- Provider capability contract tests
- Payment routing abstraction review (docs)
- Ops/reconciliation + refund orchestration readiness (docs)
- Config/secret boundary review (docs)

**Do not implement automatically.**

---

## 9. Forbidden assumptions

- Bank Mellat ⇒ Behpardakht Mellat (or any specific PSP)
- One provider covers Iran + UAE
- Individual merchant can onboard in either market without verification
- AED settlement works on Iranian PSP by preference alone
- USD/IRR charge available on every shortlisted provider
- Absence of known regulation ⇒ compliance complete

---

## 10. Recommended next authorized task

**`TC-P35-T004` — Parallel provider research brief (Iran + UAE)**  
Docs-only: shortlist candidates, map eligibility for individual merchants, clarify Mellat preference meaning, settlement matrix — **no SDK, no credentials, no adapter code**.

Alternate if Architect prefers: freeze merchant-account prerequisites checklist only.
