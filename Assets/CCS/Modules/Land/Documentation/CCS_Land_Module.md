# CCS Land Module

**Milestone:** 2.3.0 — Land Ownership Foundation

**Author:** James Schilz  
**Date:** 2026-06-02

## Purpose

Generic land ownership framework for future homesteads, ranches, farms, towns, forts, mining claims, and rail camps. Players buy a claim deed, preview claim radius, confirm placement, and associate nearby placed structures with legal frontier presence.

Not included in 2.3.0: full legal/tax systems, final deeds UI, final map UI, or multiplayer authority. **2.4.0** adds Land Office service point with owned-claim summary. **2.5.0** registers Frontier Homestead Claim Tax upkeep on claim placement and restores entries on load via `CCS_UpkeepService`.

## Land claim loop

```text
Earn Money
      ↓
Buy Homestead Claim Deed
      ↓
Claim Land
      ↓
Build Inside Claim
      ↓
Establish Legal Frontier Presence
```

## Architecture

| Component | Role |
|-----------|------|
| `CCS_LandClaimProfile` | Catalog of `CCS_LandClaimDefinition` entries |
| `CCS_LandClaimDefinition` | Claim radius, allowed structure kinds, deed item, registration cost |
| `CCS_LandClaimService` | Deed placement, structure association, save/restore |
| `CCS_LandClaimInstance` / `CCS_LandClaimSnapshot` | Runtime and persisted claim state |
| `CCS_LandClaimRuntimeBridge` | Null-safe service resolution |
| `CCS_LandClaimValidationUtility` | Profile and content validation |

## Claim states

- Unclaimed
- Claimed
- Abandoned
- Invalid

## Frontier content (2.3.0)

| Asset | Id |
|-------|-----|
| Profile | `Assets/CCS/Survival/Profiles/Land/CCS_DefaultLandClaimProfile.asset` |
| Frontier Homestead Claim | `ccs.survival.land.claim.frontierhomestead` |
| Homestead Claim Deed | `ccs.survival.land.item.homesteadclaimdeed` |

## Structure association

Structures inside claim radius register via composition hooks (no per-system rewrites):

- Shelter, campfire, bedroll, storage, workbench, industry stations, ranch structures, farm plots

## Integration

| System | Hook |
|--------|------|
| Economy | General Store sells Homestead Claim Deed |
| Camp | `CCS_CampService` records `landClaimId` when camp center is inside a claim |
| Save | `CCS_SaveData.land.claims[]` via `CCS_LandClaimSnapshot` |
| Active Item | `BindFrontierLandClaimPlacementHandler` routes deed preview/confirm |
| Playtest | Steps 154–159; Ctrl+Shift+L shortcut |

## Bootstrap

Run **`CCS_LandOwnershipFoundationBootstrapSetup.ExecuteBatch`** to create content, vendor row, profile assignment, and playtest steps.

## Future

- Land Office service point shows owned claim count and nearby claim id (**2.4.0** debug HUD)
- Deeds UI, taxes, mortgage filings
- Farmstead homestead tier placeholder (no tier change required in 2.3.0)
