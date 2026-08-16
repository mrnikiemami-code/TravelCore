# TC-P06-T010 — Consumer reference proof (contract-only)

**Task:** `TC-P06-T010`  
**Baseline:** `HEAD=origin/main=71b2886`  
**Architect lock:** **P06-R5 RESOLVED → CONTRACT-ONLY + ArchitectureTests**

## Decision

| Item | Lock |
|------|------|
| Destination.MediaAssetId | **NO** |
| Destination Hero/Cover/… role | **NO** |
| Cross-schema FK | **NO** |
| Media generic link table | **NO** |
| Proof shape | Media.Contracts `MediaAssetReference` + ArchitectureTests guards |

## Accepted future pattern

```text
Consumer domain owns business relationship semantics
        |
        v
MediaAssetId logical reference (MediaAssetReference)
        |
        v
Media.Contracts (+ presentation/app-proxy when rendering)
```

Not:

```text
Consumer → Media.Infrastructure / MediaDbContext / media tables / StorageKey
```

## Artifacts

| Artifact | Role |
|----------|------|
| `MediaConsumerReferenceContracts.cs` | Stable `MediaAssetReference` surface |
| `MediaConsumerReferenceGuardrailTests.cs` | Peer-module + Destination + generic-link guards |

## Non-Goals (explicit)

- No Destination / Media migration
- No public Destination page / Admin Media picker (T011)
- No R8 delete semantics
- No R9 consumer alt override
- No Place / P07
