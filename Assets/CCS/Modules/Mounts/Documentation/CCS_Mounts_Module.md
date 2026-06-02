# CCS Mounts Module

Generic mount framework for horses, mules, donkeys, and future rideable animals.

## Milestone 1.5.1 — Horse Foundation

- `CCS_MountDefinition` / `CCS_MountProfile` — species stats and catalog
- `CCS_MountService` — ownership, mount/dismount, call, wait, riding tick
- `CCS_MountState` — Idle, Following, Mounted, Waiting, Returning
- `CCS_HorseSaddlebagContainer` — saddlebag storage via `CCS_StorageService`
- `WagonHitchPoint` on horse prefab for wagon towing (1.5.2)
- Frontier Stable vendor (horses not sold at General Store)
- **1.8.0:** Stable service point at `CCS_TestTradingPost` routes to `CCS_Vendor_FrontierStable` via Settlements module
- Save section `CCS_SaveMountsWorldData`

## Bootstrap

```text
CCS.Modules.Mounts.Editor.CCS_HorseFoundationBootstrapSetup.ExecuteBatch
```

## Frontier Transportation Loop

```text
Earn Wealth → Buy Horse → Travel Faster → Carry More → Expand Frontier Reach
```

See **Vehicles** module for wagon logistics (1.5.2).
