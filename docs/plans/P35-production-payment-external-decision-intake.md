# P35 — Production Payment External Decision Intake

| Field | Value |
|-------|--------|
| Document | `docs/plans/P35-production-payment-external-decision-intake.md` |
| Task-ID | `TC-P35-T002` |
| Phase | P35 — Production Payment Provider Readiness |
| Status | **INTAKE LOCKED / Cursor PASS** — values not fabricated |
| Production provider selection | **`BLOCKED_ON_EXTERNAL_BUSINESS_INPUT`** |
| Nature | Documentation / governance only |

This is an **accepted blocker**, not an architecture failure. P34 sandbox path remains valid for non-production demos.

---

## A. Required business inputs (no fabricated values)

| # | Input | Current value | Classification |
|---|-------|---------------|----------------|
| 1 | Target country/market for first production launch | **UNKNOWN** | REQUIRED-BLOCKER |
| 2 | Legal/merchant entity contracting with provider | **UNKNOWN** | REQUIRED-BLOCKER |
| 3 | Merchant account status (exists / pending / none) | **UNKNOWN** | REQUIRED-BLOCKER |
| 4 | Settlement currency/currencies | **UNKNOWN** | REQUIRED-BLOCKER |
| 5 | Traveler charge currency/currencies | **UNKNOWN** | REQUIRED-BLOCKER |
| 6 | Bank/settlement constraints | **UNKNOWN** | REQUIRED-BEFORE-PRODUCTION |
| 7 | Preferred or already-contracted provider | **UNKNOWN** (none in SoT) | REQUIRED-BLOCKER |
| 8 | Refund expectations (full / none / delayed) | **UNKNOWN** | REQUIRED-BEFORE-PRODUCTION |
| 9 | Partial refund requirement | **UNKNOWN** (architecture still treats partial as deferred) | OPTIONAL-PREFERENCE until business requires |
| 10 | Payment expiry/timeout expectations | **UNKNOWN** | REQUIRED-BEFORE-PRODUCTION |
| 11 | 3DS / authentication requirements | **UNKNOWN** | REQUIRED-BEFORE-PRODUCTION |
| 12 | Invoice/receipt expectations | **UNKNOWN** | OPTIONAL-PREFERENCE |
| 13 | Production callback/domain availability | **UNKNOWN** | REQUIRED-BEFORE-PRODUCTION |
| 14 | Regulatory/compliance constraints known by business | **UNKNOWN** | REQUIRED-BLOCKER |
| 15 | Expected transaction volume (if relevant) | **UNKNOWN** | OPTIONAL-PREFERENCE |

**Do not invent defaults.** Fill only when Architect/business supplies facts.

---

## B. Decision status summary

| Class | Meaning | Count (current) |
|-------|---------|-----------------|
| REQUIRED-BLOCKER | Blocks authorizing provider-specific adapter design (T003+) | Multiple UNKNOWN |
| REQUIRED-BEFORE-PRODUCTION | Can design after vendor chosen, but blocks Stage E activation | Multiple UNKNOWN |
| OPTIONAL-PREFERENCE | Influences choice; not a hard stop alone | Multiple UNKNOWN |

---

## C. Provider selection gate (minimum before T003)

`TC-P35-T003` (provider-specific adapter design) may be authorized **only when** at least these are filled with real business values:

1. Target market/country
2. Merchant/legal entity
3. Merchant account status
4. Traveler charge currency(ies)
5. Settlement currency(ies)
6. Preferred/contracted provider **or** explicit shortlist with selection owner
7. Known regulatory constraints (even if “none beyond PCI hosted”)

Until then: **selection remains blocked**.

---

## D. Safe work while blocked (not auto-authorized)

May be proposed later via Architect `.task.md` without choosing a vendor:

- Provider-agnostic gateway contract tests / checklist hardening
- Ops/runbook templates for reconciliation & incidents
- Reconciliation interface review (docs)
- Security checklist freeze (docs)
- Production deploy readiness checks (infra docs)

**None of the above is auto-started by this RESULT.**

---

## E. Forbidden until provider decision

- Selecting a vendor by guess
- Committing provider credentials/secrets
- Flipping `NamedProductionAdapterImplemented=true`
- Treating TravelCore sandbox as production
- Provider-specific SDK/package installation
- Traveler UI claiming real production payment readiness
- Fake Confirm / MarkPaid / ForceSuccess

---

## F. Project state record

```text
PRODUCTION PROVIDER SELECTION:
BLOCKED_ON_EXTERNAL_BUSINESS_INPUT
```

Architecture readiness (ports/orchestration/ConfirmIfEligible) = **READY**.  
Vendor selection = **BLOCKED** on external inputs above.

---

## Exact inputs still required from Architect / user

Please supply (or mark N/A with owner signature) items **1–8, 10–11, 13–14** in section A. Prefer a short filled table reply or an authorized follow-up `.task.md` that embeds the answers.
