# CCS Sleep Module

**Milestone:** 1.1.3 — Sleep + Bedroll Foundation

## Purpose

Turn the crafted **Bedroll** item into a primitive placeable sleep object that supports sleeping, need recovery, time skip foundation, unified save/load, and respawn point assignment.

Primitives only — no final art, multiplayer, or item database editor.

## Gameplay flow

1. Gather resources and craft **Bedroll** at the workbench.
2. Place a primitive bedroll in the world (`PF_CCS_PrimitiveBedroll`).
3. Interact to sleep — needs recover per profile defaults.
4. Save/load restores placed bedrolls and assigned respawn.
5. On death, respawn at the assigned bedroll when set; otherwise `CCS_PlayerRespawnPoint_Bootstrap`.

## Runtime types

| Type | Role |
|------|------|
| `CCS_SleepProfile` | Sleep duration, hunger/thirst/stamina recovery, respawn assignment, bedroll content refs |
| `CCS_SleepSpotDefinition` | Primitive bedroll definition + prefab |
| `CCS_SleepSpot` | World instance, `CanSleep`, `Sleep`, `CaptureState`, `RestoreState` |
| `CCS_SleepSpotInteractable` | Interaction handoff to `CCS_SleepService.TrySleep` |
| `CCS_SleepService` | Registry, sleep execution, save capture/restore, respawn assignment |
| `CCS_SleepRuntimeBridge` | Resolve service from `CCS_RuntimeHost` |
| `CCS_SleepEventArgs` | Spot id, instance id, display name, position, success, message |

## Events

- `SleepStarted`
- `SleepCompleted`
- `SleepFailed`
- `SleepRespawnPointAssigned`
- `SleepStateRestored`

## Dev hotkeys (playtest harness)

| Key | Action |
|-----|--------|
| **Shift+F2** | Place primitive bedroll near player, or sleep at nearest bedroll |
| **F2** | Storage crate (unchanged) |
| **F8** | Delete save (save debug — do not use for bedroll) |

## Save data shape

`CCS_SaveData.sleep`:

- `assignedRespawnSpotId` — active bedroll respawn spawn id
- `sleepSpots[]` — per placed bedroll: definition id, instance id, display name, position, rotation, assigned respawn id, `isAssignedRespawn`

## Bootstrap batch setup

```
CCS.Modules.Sleep.Editor.CCS_SleepBedrollFoundationBootstrapSetup.ExecuteBatch
```

Creates primitive prefab, spot definition, profile 1.1.3 fields, and `CCS_TestBedroll` in `SCN_CCS_Survival_Bootstrap`.

## Item id

`ccs.survival.item.starter.bedroll`
