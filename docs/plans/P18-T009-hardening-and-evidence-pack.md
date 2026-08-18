# TC-P18-T009 — Trip Planner / Lead Experience hardening and evidence pack

**Task:** TC-P18-T009 — Hardening + evidence  
**Product HEAD at T009 start:** `d302ad4` (`TC-P18-T008` **ACCEPTED** + `TC-FIX-TOUR-ROUTE-AMBIGUITY` **ACCEPTED**)  
**Date:** 2026-08-18  
**Scope:** Tests, architecture guardrails, documentation, SoT sync — **no new product capability**.  
**Forbidden in this task:** new planner aggregate · new preference dimension · new lifecycle status · Agency routing · Notification provider · CRM · Booking · Payment · Pricing/Quote · Search engine · AI/RAG · P19.  
**Not this task:** `TC-P18-GATE` (evidence pack only; Gate is next).

## 1. Mission checklist

| # | Verify | Result |
|---|--------|--------|
| 1 | Independent TripPlanner module/schema owns intent/lead facts (P18-R1) | **PASS** — T001 |
| 2 | TripIntent != Lead; Lead != Booking (P18-R2) | **PASS** — T002 |
| 3 | Anonymous-first; LeadContactSnapshot != Party (P18-R3) | **PASS** — T003 |
| 4 | Structured preferences; BudgetPreference != Price (P18-R4) | **PASS** — T004 |
| 5 | LeadStatus Submitted/Contacted/Closed/Cancelled; != CRM (P18-R5) | **PASS** — T005 |
| 6 | Agency Routing = DEFERRED (P18-R6) | **PASS** — T006 |
| 7 | LeadConsentSnapshot; ContactPermission != MarketingConsent (P18-R7) | **PASS** — T007 |
| 8 | PublicExperience composes `/plan`; honest follow-up CTA (P18-R8) | **PASS** — T008 |
| 9 | Canonical Tour route `/[locale]/tours/[slug]`; legacy `[productKey]` removed | **PASS** — TC-FIX-TOUR-ROUTE-AMBIGUITY `d302ad4` |
| 10 | No new product capability in this task | **PASS** — evidence/docs + strengthened guardrails only |

## 2. Accepted product commits (P18)

| Task | Commit | Essence |
|------|--------|---------|
| PLAN | `1826013` | Authoritative P18 plan |
| T001 | `d29ab8e` | TripPlanner module scaffolding (`trip_planner` schema) — P18-R1 |
| T002 | `1163e47` | TripIntent vs Lead aggregate boundary — P18-R2 |
| T003 | `3ccabd2` | Anonymous-first identity/contact — P18-R3 |
| T004 | `bdace2e` | Structured travel preference model — P18-R4 |
| T005 | `6a5b4ed` | Lead lifecycle baseline — P18-R5 |
| T006 | `c79c07d` | Agency routing boundary **DEFERRED** — P18-R6 |
| T007 | `b2e3173` | Lead consent/privacy boundary — P18-R7 |
| T008 | `9e1b1e0` | Public Trip Planner experience — P18-R8 |
| FIX | `d302ad4` | Remove legacy `/tours/[productKey]` (blocks T008 final ACCEPT) |

Architect acceptance of T001–T008 and the baseline repair is as issued. T009 prepares gate evidence; it does **not** execute `TC-P18-GATE`.

## 3. Locked decisions (all RESOLVED)

| ID | Essence |
|----|---------|
| **P18-R1** | Independent TripPlanner module. Schema `trip_planner`. Owns trip-intent/lead facts. **TripPlanner != Booking**. **TripPlanner != Payment**. **TripPlanner != Pricing**. **TripPlanner != CRM**. **TripPlanner != Search**. **TripPlanner != AgencyMarketplace**. **TripPlanner != Notification Provider**. **TripPlanner != Party Identity**. No peer-schema FK. No shared DbContext. |
| **P18-R2** | **TripIntent != Lead**. TripIntent = mutable planning intent. Lead = submitted request/snapshot. Later TripIntent mutation does not mutate existing Lead. |
| **P18-R3** | Anonymous TripIntent does not require Account. **PlannerActorReference != Identity Account entity**. **LeadContactSnapshot != Party**. **LeadContactSnapshot != Customer Master**. **TripPlanner != Identity Authority**. **TripPlanner != Party Authority**. Draft token is opaque access, not identity. |
| **P18-R4** | **DestinationPreference != Destination Source of Truth**. **PlannerTravelerComposition != BookingPassenger**. **BudgetPreference != Price**. **BudgetPreference != Quote**. **InterestPreference != Search Facet Authority**. **AccommodationPreference != Hotel Inventory**. **TransportPreference != Flight Inventory**. Timing: ExactDates · FlexibleRange · ApproximatePeriod · Undecided. |
| **P18-R5** | LeadStatus: Submitted · Contacted · Closed · Cancelled. **LeadStatus != CRM Pipeline Stage**. **Lead != CRM Opportunity**. **Contacted != Qualification**. **Closed != Booking conversion**. **LeadStatus != BookingStatus**. **LeadStatus != QuoteStatus**. |
| **P18-R6** | **P18 Agency Routing = DEFERRED**. **Lead != AgencyAssignment**. **LeadStatus != AgencyAssignmentStatus**. **Lead Routing != Search Ranking**. **Lead Routing != Recommendation**. |
| **P18-R7** | **ContactPermission != MarketingConsent**. **Consent != NotificationDelivery**. **LeadContactSnapshot != LeadConsentSnapshot**. **FollowUpContactAllowed != AgencyDataSharingPermission**. Notification provider DEFERRED. No hardcoded retention. |
| **P18-R8** | **PublicExperience != TripPlanner Source of Truth**. **Planner Submission != Booking**. **Planner Submission != Quote**. **Planner Submission != Payment**. **Planner Discovery != Search Ownership**. **Public Planner != Agency Routing**. **Notification Intent != Notification Delivery**. Honest CTA only. |

## 4. Boundary / ownership matrix

| Concern | Owner | P18 posture |
|---------|-------|-------------|
| TripIntent / Lead / preferences / consent / lifecycle | **TripPlanner** | Facts + submission snapshot |
| Public `/plan` composition | **PublicExperience** | Presentation only; **PublicExperience != TripPlanner Source of Truth** |
| Destination / Tour / Place catalogs | **Destination / Tour / Place** | Logical preference refs only |
| Retrieval / ranking / faceting | **Search** | **Planner Discovery != Search Ownership** |
| Commercial Price / Quote / FX | **Pricing** | **BudgetPreference != Price** |
| Agency commercial relationship | **AgencyMarketplace** | Routing **DEFERRED** |
| Notification delivery | **Notification (future)** | Provider **DEFERRED** |
| Identity / Party master | **Identity / Party** | Opaque actor ref + contact snapshot only |
| Booking / Payment / CRM | **Out of P18** | Modules Booking/Payment/CRM do not exist |

## 5. Invariant evidence (T001–T008)

### 5.1 R1 ownership
- Independent module + schema `trip_planner`.
- No peer-schema FK. No shared DbContext.
- No Destination/Tour/Place/Agency clone types in TripPlanner.Domain.

### 5.2 R2 TripIntent vs Lead
- Distinct aggregates and tables (`trip_intents`, `leads`).
- Submission copies `LeadSubmissionSnapshot`; Lead is not a live alias of TripIntent.

### 5.3 R3 identity/contact
- Anonymous-first `TripIntentDraftAccessToken`.
- Optional `PlannerActorReference` (opaque Guid).
- `LeadContactSnapshot` is historical follow-up context, not Party/Customer master.

### 5.4 R4 preferences
- Timing modes ExactDates / FlexibleRange / ApproximatePeriod / Undecided.
- Traveler counts only (no passenger PII).
- Budget is total-trip preference (CurrencyCode + min/max), not Price/Quote/FX.
- Destination refs are logical ids or undecided.

### 5.5 R5 lifecycle
- Closed set Submitted / Contacted / Closed / Cancelled.
- Allowed: Submitted→Contacted|Closed|Cancelled; Contacted→Closed|Cancelled.
- Closed/Cancelled terminal. No Qualified/Won/Lost/Converted.

### 5.6 R6 agency routing DEFERRED
- No AgencyAssignment / LeadAssignment / AssignedAgencyId.
- No routing tables, ranking, allocation, or agency inbox.

### 5.7 R7 consent
- `LeadConsentSnapshot` at submission: FollowUpContactAllowed, MarketingAllowed optional, PrivacyNoticeVersion, PreferredContactChannel, CapturedAt.
- No SMTP/SMS/push/WhatsApp provider, no marketing subscriber table, no hardcoded retention.

### 5.8 R8 public composition
- Backend: `/api/trip-planner/public/intents*` + header `X-TripPlanner-Draft-Token`.
- Frontend: `/[locale]/plan` Server Component + client workflow island; FA/EN/AR copy.
- Honest CTA: request follow-up only. No Book Now / Checkout / Pay.
- Repeat submit → `alreadySubmitted=true`. No public Lead listing. No public lifecycle-control endpoint.

### 5.9 Anonymous draft security
- Token is opaque random 32-byte value stored on TripIntent.
- GET/PATCH/submit require valid header; missing token → 400; mismatch → 404.
- Token is not Party/User identity and does not create an anonymous-user platform.

### 5.10 Privacy minimization
- Public APIs do not list Leads or expose arbitrary consent/contact collections.
- Frontend does not collect passport, national ID, visa documents, card, bank, or health data.
- Lead contact/consent remain submission-time snapshots.

### 5.11 Baseline Tour route repair
- Canonical: `/[locale]/tours/[slug]`.
- Removed: `/[locale]/tours/[productKey]`.
- Preserved: `/[locale]/tours/[slug]/[intent]` (nested, not same-level).
- Production `next build` succeeds; `/[locale]/plan` remains.

## 6. Guardrail / test surfaces

| Area | Evidence |
|------|----------|
| Unit | `TravelCore.Modules.TripPlanner.UnitTests` — scaffolding, identity, preferences, lifecycle, consent, public mapper/boundaries |
| Architecture | `TripPlannerBoundaryGuardrailTests` — peer refs, R1–R8, engines, T009 evidence pack, canonical routes |
| Persistence | `TripPlannerDbContext` schema `trip_planner`; 6 migrations; `trip_intents` + `leads` only; consent columns; no agency/notification/booking tables |
| Host | `TripPlannerPublicHostTests` — anonymous create/update/submit; draft token required; idempotent resubmit; no Booking/Checkout/Payment tokens |
| Frontend | `/[locale]/plan` Server Component + workflow island; FA/EN/AR; LtrValue for codes/dates/email/phone |

## 7. Validation commands (this task)

```text
dotnet build TravelCore.sln
dotnet test tests/Unit/TravelCore.Modules.TripPlanner.UnitTests
dotnet test tests/Architecture/TravelCore.ArchitectureTests
dotnet test tests/Integration/TravelCore.Persistence.IntegrationTests
dotnet test tests/Integration/TravelCore.Host.IntegrationTests
npm run typecheck   (src/frontend/web)
npm run lint
npm run build
git diff --check
```

## 8. Carry-forward invariants into GATE

- TripPlanner != Booking · TripPlanner != Payment · TripPlanner != Pricing · TripPlanner != CRM · TripPlanner != Search · TripPlanner != AgencyMarketplace · TripPlanner != Notification Provider · TripPlanner != Party Identity · TripIntent != Lead · PlannerActorReference != Identity Account entity · LeadContactSnapshot != Party · DestinationPreference != Destination Source of Truth · PlannerTravelerComposition != BookingPassenger · BudgetPreference != Price · BudgetPreference != Quote · InterestPreference != Search Facet Authority · LeadStatus != CRM Pipeline Stage · Lead != CRM Opportunity · Contacted != Qualification · Closed != Booking conversion · Lead != AgencyAssignment · LeadStatus != AgencyAssignmentStatus · ContactPermission != MarketingConsent · Consent != NotificationDelivery · LeadContactSnapshot != LeadConsentSnapshot · FollowUpContactAllowed != AgencyDataSharingPermission · PublicExperience != TripPlanner Source of Truth · Planner Submission != Booking · Planner Submission != Quote · Planner Submission != Payment · Planner Discovery != Search Ownership · P18 Agency Routing = DEFERRED.

T009 does **not** close `TC-P18-GATE`.
