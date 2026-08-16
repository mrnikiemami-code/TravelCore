# TC-P06-T008 — Image Optimization Contract + P06-R1 DEFER

**Task:** `TC-P06-T008`  
**Baseline:** `HEAD=origin/main=c79c507`  
**Architect lock:** **P06-R1 RESOLVED → DEFER** (WebP/AVIF conversion / generation pipeline out of P06)

## 1. Purpose

Document and lock the P06 **image optimization contract** for consumers and implementers:

- Accepted input/output format posture for derived variants
- Explicit **DEFER** of cross-format WebP/AVIF optimization with architect sign-off
- Clear Non-Goals for P06 so T008 is a contract + defer evidence pack — **not** a half-built conversion feature

## 2. Accepted optimization contract (P06)

### 2.1 Same-format derived variants (IN)

Derived profiles (`Large` / `Medium` / `Thumbnail`) keep the **source ContentType**:

| Source upload | Derived variant output |
|---------------|------------------------|
| `image/jpeg` | `image/jpeg` |
| `image/png` | `image/png` |
| `image/webp` | `image/webp` |

Rules (aligned with **P06-R3** / T005):

- Fit-within longest-edge sizing: large=1600 / medium=960 / thumbnail=320
- No crop · no upscale · original blob not duplicated as a variant row
- Synchronous Media-owned processor (`ImageSharpMediaVariantProcessor`)
- GIF: fail-closed (variant policy unresolved)
- SVG: denied at upload (**P06-R6**)

Quality/encoder defaults in Infrastructure (JPEG/WebP quality 85) are implementation detail under the same-format contract; they are **not** a license to invent alternate output MIME types.

### 2.2 Content-Type / naming posture

- Variant `ContentType` equals the owning MediaAsset’s normalized MIME for Ready variants
- Object keys retain extension mapping for that MIME (`.jpg` / `.png` / `.webp`) — no silent remapping to another format
- Public delivery URL strategy remains **R4 OPEN** (owned by T009) — this contract does **not** invent public URLs, content negotiation, or Accept-based format switching

### 2.3 Consumer expectation

Consumers (Admin, future public presentation) may assume:

- At most one Ready blob per derived profile per asset
- MIME of a Ready variant matches the original asset MIME
- No guaranteed WebP or AVIF alternate for a JPEG/PNG original in P06

## 3. P06-R1 decision — DEFER

| Field | Value |
|-------|--------|
| Decision ID | **P06-R1** |
| Question | Whether a WebP/AVIF **generation / conversion** pipeline ships in P06 |
| Classification | **RESOLVED — DEFER** (out of P06) |
| Sign-off | Architect lock for T008 (same-format posture only) |

### Rationale

- ROADMAP scoped WebP/AVIF as «در صورت تأیید» — confirmation is **not** granted for P06 ship
- T005 already delivers usable same-format variants; cross-format conversion is a separate product/ops capability (CDN, Accept negotiation, dual-blob storage, cache keys)
- Shipping a partial JPEG→WebP path without AVIF policy, negotiation, and migration for P02 `MediaImage` would be an unapproved half-feature
- Defer keeps the gate criterion honest: contract accepted **and** conversion pipeline explicitly deferred with evidence

## 4. What is NOT in P06 (explicit Non-Goals)

| Item | Status |
|------|--------|
| JPEG/PNG → WebP conversion | **NOT in P06** |
| Any format → AVIF conversion | **NOT in P06** |
| Automatic WebP generation alongside same-format variants | **NOT in P06** |
| HTTP content negotiation (`Accept: image/avif,image/webp,…`) | **NOT in P06** |
| Dual-MIME variant rows / “alternate format” profiles | **NOT in P06** |
| Inventing public URL / `remotePatterns` strategy (R4) | **NOT in T008** — T009 |
| Breaking P02 `MediaImage` / `MediaImagePresentation` without a migration path | **Forbidden** |

Native **WebP uploads** that stay WebP under T005 remain allowed; that is **not** a conversion pipeline.

## 5. Relationship to T005 / T009

| Task | Role vs this contract |
|------|------------------------|
| **T005** | Ships the **implementation** of same-format derived variants + dimensions (R3 lock). T008 **documents** that posture as the accepted P06 optimization contract and forbids treating it as WebP/AVIF conversion. |
| **T008** | Contract + R1 defer evidence. No new conversion pipeline. |
| **T009** | Public presentation URL + frontend `remotePatterns` (depends on R4). Must consume same-format variants; must **not** invent R4 or assume WebP/AVIF alternates. |

## 6. Implementation evidence (no conversion policy types)

Repo checks at baseline `c79c507` (and retained by this task):

| Check | Evidence |
|-------|----------|
| Same-format encode switch | `ImageSharpMediaVariantProcessor.EncodeFitWithinAsync` — JPEG→JPEG, PNG→PNG, WebP→WebP only |
| Supported output formats | `EnsureSupportedOutputFormat` allows only `image/jpeg` \| `image/png` \| `image/webp`; GIF denied |
| No AVIF MIME in upload allowlist | `MediaUploadContentRules.AllowedContentTypes` — no `image/avif` |
| No production conversion policy types in Contracts | Media Contracts have no `Avif*` / `ConvertToWebP` / target-format / content-negotiation policy types |
| Unit coverage of same-format | `MediaVariantProcessingTests` asserts PNG derived variants keep `image/png` |

T008 adds **no** half-feature conversion code. Docs + state lock are sufficient for the defer gate.

## 7. Gate criterion mapping

P06 Acceptance Strategy item 7:

> Optimization contract accepted; WebP/AVIF shipped **or** explicitly deferred with evidence.

**Satisfied by:** this artifact + plan/state/roadmap R1 = **RESOLVED DEFER**.

## 8. Open decisions (unchanged by T008)

| ID | Status |
|----|--------|
| R1 | **RESOLVED — DEFER** |
| R2 / R3 / R6 | RESOLVED (prior) |
| R4 | **OPEN** — public URL strategy; decide by T009 (**do not invent here**) |
| R5 / R8 | OPEN |
| R7 / R9 | DEFERRED |
