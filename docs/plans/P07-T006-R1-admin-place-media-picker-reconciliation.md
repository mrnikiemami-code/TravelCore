# TC-P07-T006-R1 — Admin Place media-selection reconciliation

**Task:** `TC-P07-T006-R1`  
**Product under review:** `TC-P07-T006` (`74e8540`)

## Pre-remediation answers (T006 shipped UX)

| # | Question | Answer |
|---|----------|--------|
| 1 | Does operator manually type/paste MediaAssetId? | **Yes** — Cover and Gallery forms used free-text GUID inputs. |
| 2 | Visible selector/list of Ready Media assets? | **No** |
| 3 | Selector preview / human-usable visual? | **No** |
| 4 | Select Cover without copying an ID? | **No** |
| 5 | Add Gallery without copying IDs? | **No** |

## Classification

**CASE B** — raw-ID was the primary Cover/Gallery attach workflow.

## Remediation (smallest usable picker)

- Load Ready Media via existing `listMediaAssetsAction` → `GET /api/media/assets/?status=Ready` (P06 Access `media.assets.write`).
- Grid cards show app-proxy thumbnail (fallback to original content), content-type, dimensions.
- **Use as Cover** / **Add to Gallery** submit `MediaAssetId` internally only.
- Current Cover badge + border; Gallery already-attached disabled; Cover asset not offered as Gallery duplicate (UNIQUE link).
- Cover replace + remove Cover preserved; Gallery remove preserved.
- No DAM, no StorageKey, no Hero role, no R3/R4/R5 surfaces, no new Place domain capabilities.
