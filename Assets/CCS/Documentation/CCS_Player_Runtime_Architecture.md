# CCS Player Runtime Architecture (v0.8.0)

**Author:** James Schilz  
**Date:** 2026-06-25  
**Baseline:** v0.7.2 animator layer cleanup preserved; v0.7.3 runtime layer-weight fixes retained.

## Current Problem

`PF_CCS_CharacterController_TestPlayer_Networked` mixes runtime gameplay, network authority, animation/presentation, local HUD, and Master Test debug helpers on one root object. That was acceptable for prototype work but not for production structure.

## Goals (v0.8.0)

- Introduce a **production** networked player prefab with a clean hierarchy.
- Keep the **legacy Master Test prefab** as the transitional test harness.
- Add a **runtime facade** for typed references (no deep `GetComponent` chains).
- Add **classification + validation** for production vs test-only components.
- **Do not** delete systems, rewrite gameplay, or move aim/interaction animation back to Base Layer.

## Prefab Paths

| Role | Path |
|------|------|
| Production runtime | `Assets/CCS/Prefabs/Player/PF_CCS_Player_Networked_Runtime.prefab` |
| Test harness copy | `Assets/CCS/Prefabs/Player/PF_CCS_Player_Networked_TestHarness.prefab` |
| Legacy Master Test harness | `Assets/CCS/Modules/CharacterController/Tests/Prefabs/PF_CCS_CharacterController_TestPlayer_Networked.prefab` |

Master Test and hosting continue to spawn the **legacy test harness** until production validation is complete in daily use.

## Hierarchy

```
PF_CCS_Player_Networked_Runtime
├── RuntimeSystems          (gameplay + network gameplay behaviours)
├── Presentation            (VisualRoot, animation, IK, world nameplate)
├── PlayerLocalUI           (owner-only transitional HUD)
├── CameraFollowAnchor      (runtime camera anchor; may move under RuntimeSystems later)
├── InteractionScanOrigin   (scanner origin)
└── MuzzlePoint             (weapon anchor; presentation-adjacent)
```

### Root (minimal, network-safe)

**Allowed:**

- `Transform`
- `CharacterController` (required by `CCS_CharacterMotor` on root for NetworkTransform sync)
- `CCS_CharacterMotor` (must move the same transform synced by `NetworkTransform`)
- `NetworkObject`
- `CCS_ClientOwnerNetworkTransform`
- `CCS_PlayerRuntimeFacade`

**Target:** fewer than 8 direct components (transitional allowance: 12).

**Avoid:**

- Debug/test damage helpers
- HUD presenters
- Raw animation helpers (live on Presentation / VisualRoot)
- Master Test offline bootstrap
- Scene setup / editor validators

### RuntimeSystems

Pure gameplay runtime controllers:

- Input provider
- Camera controller + controller service/bridge
- Interaction scanner
- Attribute container / service / network replicator
- Stamina + health regen
- Network health
- Revolver + loadout + carry state + equipment sockets/visual controller
- Aim locomotion controller
- Network ownership runtime (`CCS_PlayerNetworkRuntimeBehaviour` on production; legacy test subclass on harness)

### Presentation

Visuals and animation only (no gameplay authority):

- `VisualRoot` → nested player visual prefab
- `Animator` (single live runtime Animator)
- Locomotion / interaction / revolver upper-body animators
- IK + body aim follow
- First-person headless adapter
- Equipment socket children near bones
- Capsule/glasses test visuals (harness) or production visual prefab
- World-space nameplate anchor (replicated presentation)

### PlayerLocalUI (transitional)

Owner-only local HUD until scene/UI-system ownership:

- `AttributeHudRoot`
- `WeaponHudRoot`
- `InteractionPromptHudRoot`
- Death/restart presenter
- `CCS_PlayerLocalOwnerUiBootstrap` disables UI for non-owners

Preferred AAA direction: move HUD to scene-owned local UI and remove `PlayerLocalUI` from the network prefab.

### Test-Only (harness only)

- `CCS_TestPlayerOfflineBootstrap`
- `CCS_TestPlayerAttributeDebugInput`
- Master Test display profile wiring
- Debug flags enabled on production prefab (must be off on production)

## Animator Layer Contract (v0.7.2+)

| Layer | Purpose |
|-------|---------|
| **Base Layer** | Locomotion only (Idle, Walk, Sprint, Jump, InAir) |
| **RevolverUpperBody** | Aim / aim strafe (`AM_CCS_Revolver_UpperBodyRightArm_Aim` mask) |
| **Interaction** | NoInteraction, Interact_PickUp_RH, Interact_WalkThroughDoor_RH |

Rules:

- Presentation owns the live `Animator`.
- `CCS_PlayerInteractionAnimator` resolves the runtime Animator under Presentation.
- Revolver upper-body layer weight is driven by revolver runtime while equipped.
- Interaction layer weight stays at 1 on the runtime Animator.
- No duplicate Animator receiving parameter updates.

## Runtime Facade

`CCS_PlayerRuntimeFacade` on the root:

- Typed reference hub only
- Validates required references in `Awake` / `OnValidate`
- Exposes `IsLocalOwner` via `NetworkObject`
- No per-frame gameplay logic

## Validation

`CCS_PlayerPrefabArchitectureValidationUtility` classifies components:

- ProductionRequired / ProductionAllowed / PresentationAllowed / LocalOwnerUIOnly / TestOnly / Deprecated / Unknown

Production prefab validation is **strict**. Test harness allows TestOnly components but labels them clearly.

Batch entry: `CCS_PlayerPrefabArchitectureBatchEntry.RunFromBatchMode`  
Log: `Logs/player-prefab-architecture-v0.8.0-batch.log`

## Migration Notes

- Motor + `CharacterController` remain on root so `NetworkTransform` sync stays correct.
- Other runtime systems move under `RuntimeSystems`.
- Production prefab strips test-only components; harness keeps them.
- `CCS_ControllerTestNetworkPlayerBehaviour` remains on harness; production uses the same ownership path until renamed in a later pass.
