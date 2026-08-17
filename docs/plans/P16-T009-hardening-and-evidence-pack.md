# TC-P16-T009 — UGC hardening tests & evidence pack

**Task:** TC-P16-T009 — UGC hardening and evidence pack  
**Product HEAD:** `62a1d7b` (`TC-P16-T008` **ACCEPTED**)  
**Date:** 2026-08-17  
**Scope:** Hardening + evidence **only** — no new product capability.  
**Forbidden in this task:** Like/Reaction · threaded comments · new target types · rating aggregation engine · automatic moderation policy · Search engine · SEO ownership changes · standalone UGC SEO pages · Content/Media ownership changes · Booking · Payment · AI infrastructure · P17.  
**Not this task:** `TC-P16-GATE` (evidence pack only; Gate is next).

## 1. Mission checklist

| # | Verify | Result |
|---|--------|--------|
| 1 | Independent UGC module/schema owns user-generated lifecycle (P16-R1) | **PASS** — T001 |
| 2 | Review owns OverallRating + DimensionRatings; Rating is not independent (P16-R2) | **PASS** — T002 |
| 3 | Review target is one controlled TargetType + TargetId; no peer FK (P16-R3) | **PASS** — T003 |
| 4 | Travelogue is UGC-owned; Travelogue != ContentItem (P16-R4) | **PASS** — T004 |
| 5 | UserPhoto is UGC relationship over logical MediaAssetId (P16-R5) | **PASS** — T005 |
| 6 | Comment is flat on Review/Travelogue; Like = DEFERRED (P16-R6) | **PASS** — T006 |
| 7 | ModerationStatus != PublicationStatus; PublicEligibility = Approved + Published (P16-R7) | **PASS** — T007 |
| 8 | UGC owns eligibility truth; PE composes; Search projects; SEO owns IndexPolicy (P16-R8) | **PASS** — T008 |
| 9 | P16-R1…R8 all RESOLVED | **PASS** — plan open-decisions table |
| 10 | No new product capability in this task | **PASS** — evidence/docs + strengthened guardrails only |

## 2. Accepted product commits (P16)

| Task | Commit | Essence |
|------|--------|---------|
| PLAN | `bac626b` | Authoritative P16 plan |
| T001 | `e5fa578` | UGC module scaffolding (`ugc` schema) — P16-R1 |
| T002 | `a5cccb2` | Review aggregate + structured dimension ratings — P16-R2 |
| T003 | `73f85f2` | Review logical target attachment — P16-R3 |
| T004 | `b35721c` | Travelogue UGC narrative — P16-R4 |
| T005 | `3d10913` | UserPhoto relationship over MediaAssetId — P16-R5 |
| T006 | `2d1dd59` | Flat Comment; Like = DEFERRED — P16-R6 |
| T007 | `30b3471` | Moderation, publication, UgcReport — P16-R7 |
| T008 | `62a1d7b` | Public composition / read contracts — P16-R8 **ACCEPTED** |

Architect acceptance of T001–T008 is as issued. T009 prepares gate evidence; it does **not** execute `TC-P16-GATE`.

## 3. Locked decisions (all RESOLVED)

| ID | Essence |
|----|---------|
| **P16-R1** | Independent UGC module. Schema `ugc`. Owns user-generated content lifecycle. Does not own Identity/Party, Content CMS, MediaAsset technical truth, Tour/Place/Destination facts, SEO IndexPolicy, Search, Booking, or Payment. |
| **P16-R2** | Review is the aggregate. OverallRating (1..5) is part of Review. Dimension ratings are children. Rating is not an independent aggregate. |
| **P16-R3** | Each Review owns exactly one logical target (`TargetType` + `TargetId`). Controlled: TourProduct · Place · Agency. No peer-schema FK. |
| **P16-R4** | Travelogue is an independent UGC aggregate. Travelogue != ContentItem. Article/Guide/LandingPage remain Content CMS. |
| **P16-R5** | UGC owns UserPhoto relationship (Actor + logical MediaAssetId). Media owns technical MediaAsset truth. UserPhoto != MediaAsset. |
| **P16-R6** | Comment = IN (flat, Review/Travelogue). Like = DEFERRED. No threading / ranking. |
| **P16-R7** | ModerationStatus != PublicationStatus. Approved != Published. PublicEligibility = Approved + Published. UgcReport is moderation input only. Report != Automatic Enforcement. |
| **P16-R8** | UGC owns public-eligibility truth. PublicExperience owns composition. Search owns retrieval/projection. SEO owns IndexPolicy. Publicly Eligible != SEO Indexed. Publicly Eligible != Automatically Search Indexed. |

## 4. Boundary / ownership matrix

| Concern | Owner | P16 posture |
|---------|-------|-------------|
| Review / rating facts / Travelogue / UserPhoto relationship / Comment / Report | **Ugc** | Fact + lifecycle + eligibility truth |
| Editorial Article/Guide/LandingPage | **Content** | Travelogue != ContentItem |
| Media bytes / variants / StorageKey | **Media** | UserPhoto != MediaAsset |
| Tour / Place / Agency catalog facts | **Tour / Place / AgencyMarketplace** | Logical target ids only |
| Public page composition | **PublicExperience** | PublicExperience != UGC Source of Truth |
| Retrieval / discovery projection | **Search** | Search != UGC Source of Truth; no P16 Search engine |
| IndexPolicy / canonical / redirects / sitemap | **Seo** | UGC != SEO Authority; Published != SEO Indexed |
| Identity / Party | **Identity / Party** | Opaque actor id only |
| Booking / Payment | **Out of P16** | Modules do not exist |

## 5. Invariant evidence (T001–T008)

### 5.1 UGC != Content

- Independent module + schema `ugc`.
- Travelogue has no `ContentItemId`. Content has no `IsUserGenerated` / `UgcType`.

### 5.2 UserPhoto != MediaAsset

- UserPhoto stores logical `MediaAssetId` only.
- No StorageKey / MimeType / FileSize / Width / Height / renditions / focal.

### 5.3 Rating != Independent Aggregate

- OverallRating is on Review. Dimension ratings are Review children.
- Public rating summary is a derived rebuildable read model, not a persisted aggregate.

### 5.4 Comment != Threaded Conversation System

- No `ParentCommentId`. Targets Review/Travelogue only.
- Like = DEFERRED. No LikeCount / Reaction.

### 5.5 Moderation / publication / report

- ModerationStatus != PublicationStatus.
- Approved != Published.
- PublicEligibility = Approved + Published.
- Rejected / Draft / Hidden / Archived are not public.
- Report != Automatic Enforcement.

### 5.6 Public composition / Search / SEO

- Public consumers use `/api/ugc/public/*` contracts.
- PublicExperience does not persist UGC facts or own lifecycle.
- Public contracts exclude Report details.
- Publicly Eligible != Automatically Search Indexed.
- Published != SEO Indexed.
- UGC != Search Ranking Authority.
- No embeddings / vector / RAG / LLM in P16.

## 6. Guardrail / test surfaces

| Area | Evidence |
|------|----------|
| Unit | `TravelCore.Modules.Ugc.UnitTests` — lifecycle, targets, public eligibility, ownership |
| Architecture | `UgcBoundaryGuardrailTests` + `UgcPhaseBoundaryGuardrailTests` — peer refs, R1–R8, engines, evidence pack |
| Persistence | `UgcMigrationLifecycleTests` — schema `ugc`; no peer FK; reports + publication columns |
| Host | Host composition registers UGC public endpoints without owning SEO/Search |
| Frontend | PE `ugc-composition-list` composes eligible facts only |

## 7. Validation commands (this task)

```text
dotnet build TravelCore.sln
dotnet test tests/Unit/TravelCore.Modules.Ugc.UnitTests
dotnet test tests/Architecture/TravelCore.ArchitectureTests
dotnet test tests/Integration/TravelCore.Persistence.IntegrationTests
dotnet test tests/Integration/TravelCore.Host.IntegrationTests
npm --prefix src/frontend/web run typecheck
git diff --check
```

## 8. Carry-forward invariants into GATE

- UGC != Content · Travelogue != ContentItem · UserPhoto != MediaAsset · Rating != Independent Aggregate · Comment != Threaded Conversation System · Like = DEFERRED · ModerationStatus != PublicationStatus · Approved != Published · PublicEligibility = Approved + Published · Published != SEO Indexed · Publicly Eligible != Automatically Search Indexed · Report != Automatic Enforcement · PublicExperience != UGC Source of Truth · Search != UGC Source of Truth · UGC != SEO Authority · UGC != Search Ranking Authority.

T009 does **not** close `TC-P16-GATE`.
