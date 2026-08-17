# TC-P17-T009 — Visa hardening tests & evidence pack

**Task:** TC-P17-T009 — Visa hardening and evidence pack  
**Product HEAD:** `ee7a232` (`TC-P17-T008` **ACCEPTED**)  
**Date:** 2026-08-17  
**Scope:** Hardening + evidence **only** — no new product capability.  
**Forbidden in this task:** VisaApplication · applicant case workflow · document upload · OCR · appointment · embassy integration · Pricing/Quote/FX · Booking · Payment · Search engine · SEO ownership changes · Content ownership changes · AI infrastructure · P18.  
**Not this task:** `TC-P17-GATE` (evidence pack only; Gate is next).

## 1. Mission checklist

| # | Verify | Result |
|---|--------|--------|
| 1 | Independent Visa module/schema owns structured visa-domain facts (P17-R1) | **PASS** — T001 |
| 2 | VisaDefinition != VisaRequirementSet (P17-R2) | **PASS** — T002 |
| 3 | Structured applicability context; no rules engine (P17-R3) | **PASS** — T003 |
| 4 | RequiredDocument != EligibilityRequirement; no uploads (P17-R4) | **PASS** — T004 |
| 5 | ProcessingTime != VisaValidity != AllowedStay; EntryPolicy independent (P17-R5) | **PASS** — T005 |
| 6 | OfficialVisaFee != CommercialPrice/Quote/PaymentAmount (P17-R6) | **PASS** — T006 |
| 7 | Public Visa read/composition; Visa != Content/SEO/Search (P17-R7) | **PASS** — T007 |
| 8 | Visa != VisaApplication; deferred case workflow (P17-R8) | **PASS** — T008 |
| 9 | P17-R1…R8 all RESOLVED | **PASS** — plan open-decisions table |
| 10 | No new product capability in this task | **PASS** — evidence/docs + strengthened guardrails only |

## 2. Accepted product commits (P17)

| Task | Commit | Essence |
|------|--------|---------|
| PLAN | `1b5c8ea` | Authoritative P17 plan |
| T001 | `5f18f83` | Visa module scaffolding (`visa` schema) — P17-R1 |
| T002 | `12f19e7` | VisaDefinition + VisaRequirementSet — P17-R2 |
| T003 | `8098ee2` | VisaApplicability context — P17-R3 |
| T004 | `f5f52de` | RequiredDocument + EligibilityRequirement — P17-R4 |
| T005 | `90cd5f4` | ProcessingTime / Validity / AllowedStay / EntryPolicy — P17-R5 |
| T006 | `1f3d206` | OfficialVisaFee vs Pricing — P17-R6 |
| T007 | `d31f027` | Public Visa read + VisaDetailPage composition — P17-R7 |
| T008 | `ee7a232` | Visa application/transactional boundary — P17-R8 **ACCEPTED** |

Architect acceptance of T001–T008 is as issued. T009 prepares gate evidence; it does **not** execute `TC-P17-GATE`.

## 3. Locked decisions (all RESOLVED)

| ID | Essence |
|----|---------|
| **P17-R1** | Independent Visa module. Schema `visa`. Owns structured visa-domain facts/lifecycle. Does not own Destination/ReferenceData geography, Content CMS, MediaAsset technical truth, Pricing/Quote, Booking, Payment, SEO IndexPolicy, Search, or Identity/Party. |
| **P17-R2** | VisaDefinition = stable visa-type identity. VisaRequirementSet = context-dependent requirement facts. **VisaDefinition != VisaRequirementSet**. |
| **P17-R3** | Each VisaRequirementSet has exactly one VisaApplicability context. Logical destination/jurisdiction + optional nationality/residence + optional ApplicantCategory. **Applicability != Rules Engine**. |
| **P17-R4** | RequiredDocument and EligibilityRequirement are structured children. **RequiredDocument != EligibilityRequirement**. **RequiredDocument != ApplicantSubmittedDocument**. No applicant uploads/OCR/MediaAsset persistence. |
| **P17-R5** | ProcessingTime != VisaValidity != AllowedStay. EntryPolicy is a fourth independent fact. No generic Duration field. Effective-period readiness only. |
| **P17-R6** | OfficialVisaFee != CommercialPrice != Quote != PaymentAmount. Visa stores official/regulatory fee facts with platform Money. Pricing remains Price/Quote owner. No FX. |
| **P17-R7** | Visa owns structured facts + public read contracts. PublicExperience composes VisaDetailPage. Content remains editorial. SEO owns IndexPolicy. **Public Visa Page != Automatically SEO Indexed**. No application workflow. |
| **P17-R8** | Visa policy/information capability is complete in P17. Applicant-specific VisaApplication/case workflow is explicitly deferred outside P17. **Visa != VisaApplication**. **VisaApplication != Booking**. **VisaApplication != Payment**. |

## 4. Boundary / ownership matrix

| Concern | Owner | P17 posture |
|---------|-------|-------------|
| VisaDefinition / RequirementSet / Applicability / documents / eligibility / processing / validity / stay / entry / official fee | **Visa** | Structured visa policy facts |
| Editorial Article/Guide/FAQ | **Content** | **Structured Visa Fact != Editorial Guidance** |
| Media bytes / variants / StorageKey | **Media** | Logical MediaAssetId only when needed later |
| Destination / Country geography | **Destination / ReferenceData** | Opaque logical ids only |
| Commercial Price / Quote / FX | **Pricing** | **OfficialVisaFee != CommercialPrice** |
| Public page composition | **PublicExperience** | **Visa != PublicExperience** |
| Retrieval / discovery projection | **Search** | **Visa != Search authority**; no P17 Search engine |
| IndexPolicy / canonical / redirects / sitemap | **Seo** | **Public Visa Page != Automatically SEO Indexed** |
| Applicant case / upload / appointment | **Future VisaApplication (deferred)** | **Visa != VisaApplication** |
| Booking / Payment | **Out of P17** | Modules do not exist |

## 5. Invariant evidence (T001–T008)

### 5.1 Visa != peer SoT

- Independent module + schema `visa`.
- No peer-schema FK. No shared DbContext with Destination/Content/Pricing/Search/Seo.
- No Country/Destination/Nationality master clone inside Visa.

### 5.2 VisaDefinition != VisaRequirementSet

- Separate aggregates. Definition holds stable type identity/meaning.
- RequirementSet holds context-dependent facts and children.
- No applicability/docs/fees improperly collapsed into VisaDefinition.

### 5.3 Applicability != Rules Engine

- Exactly one VisaApplicability per RequirementSet.
- Logical destination/jurisdiction id + optional opaque nationality/residence + optional ApplicantCategory.
- No Expression/Predicate/Rules/DSL fields.

### 5.4 RequiredDocument != EligibilityRequirement != ApplicantSubmittedDocument

- Row/code-based documents with RequirementLevel.
- Structured eligibility Code/Kind/Value/Unit facts.
- No applicant uploads, OCR, MediaAsset technical persistence, or generic rules engine.

### 5.5 Processing / validity / stay / entry

- Distinct VisaProcessingTime, VisaValidity, VisaAllowedStay, VisaEntryPolicy.
- No generic Duration field on RequirementSet or processing entities.
- Optional EffectiveFrom/EffectiveTo readiness on RequirementSet only.

### 5.6 OfficialVisaFee != Pricing / Payment

- VisaOfficialFee uses platform Money/CurrencyCode in source currency.
- No Quote/Discount/Commission/Markup/ExchangeRate fields.
- Visa does not own commercial pricing or payment authority.

### 5.7 Public / Content / SEO / Search

- Public read via `/api/visa/public/definitions/{code}`.
- Frontend `VisaDetailPage` composes structured facts + Content enrichment + composed SEO metadata.
- Visa does not own IndexPolicy, canonical, redirect, or sitemap policy.
- Public Visa route existence != automatic indexability.
- No Search engine/FTS/Elasticsearch in P17 Visa module.

### 5.8 Application boundary (deferred)

- `VisaApplicationBoundary` documents future case capability only.
- No VisaApplication aggregate, `visa_applications` table, applicant PII, document upload, OCR, appointment, embassy integration, or generic workflow engine.
- Public Visa UI has no Apply Now / Upload Documents / Pay Visa Fee / Book Appointment transactional CTAs.

## 6. Guardrail / test surfaces

| Area | Evidence |
|------|----------|
| Unit | `TravelCore.Modules.Visa.UnitTests` — ownership, applicability, requirements, issuance semantics, official fee, public read, application boundary |
| Architecture | `VisaBoundaryGuardrailTests` + `VisaPhaseBoundaryGuardrailTests` — peer refs, R1–R8, engines, evidence pack |
| Persistence | `VisaDbContext` schema `visa`; no peer FK; no application/applicant tables |
| Host | `VisaPublicQueryHostTests` — public informational read only |
| Frontend | `visa-detail` Server Component composes facts; no transactional CTAs |

## 7. Validation commands (this task)

```text
dotnet build TravelCore.sln
dotnet test tests/Unit/TravelCore.Modules.Visa.UnitTests
dotnet test tests/Architecture/TravelCore.ArchitectureTests
dotnet test tests/Integration/TravelCore.Persistence.IntegrationTests
dotnet test tests/Integration/TravelCore.Host.IntegrationTests
npx tsc --noEmit --project src/frontend/web
git diff --check
```

## 8. Carry-forward invariants into GATE

- Visa != Destination · Visa != ReferenceData · Visa != Content · Visa != Pricing · Visa != Booking · Visa != Payment · Visa != SEO authority · Visa != Search authority · VisaDefinition != VisaRequirementSet · Applicability != Rules Engine · RequiredDocument != EligibilityRequirement · RequiredDocument != ApplicantSubmittedDocument · EligibilityRequirement != Rules Engine · ProcessingTime != VisaValidity · ProcessingTime != AllowedStay · VisaValidity != AllowedStay · OfficialVisaFee != CommercialPrice · OfficialVisaFee != Quote · OfficialVisaFee != PaymentAmount · Public Visa Page != Automatically SEO Indexed · Structured Visa Fact != Editorial Guidance · Visa != VisaApplication · VisaApplication != Booking · VisaApplication != Payment · Visa policy data != Applicant PII.

T009 does **not** close `TC-P17-GATE`.
