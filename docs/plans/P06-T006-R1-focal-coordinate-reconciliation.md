# TC-P06-T006-R1 — Focal Coordinate Policy Reconciliation

**Task:** `TC-P06-T006-R1`  
**Baseline:** `HEAD=origin/main=bdf5e88`  
**Product under review:** `TC-P06-T006` (`166e9db`)

## 1. T006 plan wording (pre-implementation)

From [`P06-implementation-plan.md`](P06-implementation-plan.md) § TC-P06-T006:

- Purpose: Persist focal point for crop/responsive framing.
- Allowed: domain fields + Admin set/get.
- Forbidden: undocumented frontend-only crop as SoR.
- Done-when: focal point stored and returned on reads.

**Coordinate representation (X/Y units, origin, nullability rules):** **not specified** in the plan entry, constitution, or other accepted Media docs prior to T006.

Module-boundaries and ROADMAP mention “focal point” as ownership/capability only — no normalized vs pixel model.

## 2. Case classification

| Question | Finding |
|----------|---------|
| Did repository truth already define normalized [0,1] top-left focal coordinates? | **NO** |
| Classification of Cursor T006 coordinate choice | **ARCHITECT_DECISION_INVENTED** (chose via industry convention instead of STOP `BLOCKED_FOCAL_COORDINATE_POLICY`) |

## 3. Architect decision (now)

**P06 FOCAL COORDINATE POLICY = RESOLVED**

- Model: normalized resolution-independent coordinates
- `X ∈ [0.0, 1.0]`, `Y ∈ [0.0, 1.0]`
- Origin: top-left (`X=0` left, `X=1` right, `Y=0` top, `Y=1` bottom)
- Both values together; both null clears; one-null invalid; outside [0,1] invalid
- Persistence: floating representation on MediaAsset SoR
- No pixel-coordinate SoR; no per-variant focal duplication
- T005 no-crop variant behavior unchanged

## 4. Implementation verification (`166e9db` / HEAD `bdf5e88`)

| Check | Result |
|-------|--------|
| FocalX/FocalY on MediaAsset | PASS |
| Both nullable; both-null clears | PASS (`SetFocalPoint`) |
| Partial-null rejected | PASS |
| Values &lt;0 or &gt;1 rejected | PASS (`NormalizeFocalCoordinate`) |
| Origin documented top-left | PASS (domain + contracts comments) |
| No pixel SoR duplicate | PASS |
| No per-variant focal ownership | PASS |
| No Destination/Place/Tour focal table | PASS |
| No focal crop in T005 processor | PASS |
| No FE-only SoR | PASS |
| Schema media-only | PASS (`focal_x`/`focal_y` on `media.media_assets`) |
| Access mutation authority | PASS (`Access.Media.Assets.Write`) |

**Mismatch vs architect policy:** NONE → product fix **not** required.

## 5. Governance

Cursor **should have** stopped with `BLOCKED_FOCAL_COORDINATE_POLICY` before inventing the coordinate model.

**Classification:** `NON_BLOCKING_GOVERNANCE_DEVIATION`  
(implementation matches the now-approved architect policy exactly)

Incident recorded; history not rewritten.

## 6. Recommendation

Accept `TC-P06-T006` as COMPLETE after this reconciliation artifact, with focal coordinate policy locked as above.

## 7. Open decisions (unchanged)

R1 UNRESOLVED · R2/R3/R6 RESOLVED · R4/R5/R8 UNRESOLVED · R7/R9 DEFERRED
