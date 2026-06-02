# CCS Mounts Module

Generic mount framework for horses, mules, donkeys, and future rideable animals.

## Milestone 1.5.1 — Horse Foundation

- `CCS_MountDefinition` / `CCS_MountProfile` — species stats and catalog
- `CCS_MountService` — ownership, mount/dismount, call, wait, riding tick
- `CCS_MountState` — Idle, Following, Mounted, Waiting, Returning
- `CCS_HorseSaddlebagContainer` — saddlebag storage via `CCS_StorageService`
- Frontier Stable vendor (horses not sold at General Store)
- Save section `CCS_SaveMountsWorldData`

## Bootstrap

```text
CCS.Modules.Mounts.Editor.CCS_HorseFoundationBootstrapSetup.ExecuteBatch
```

## Frontier Transportation Loop

```text
Earn Wealth → Buy Horse → Travel Faster → Carry More → Expand Frontier Reach
```
