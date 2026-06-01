# CCS Survival — Character Controller Module

**Milestone:** 0.9.0 — Character Controller Gameplay Integration  
**Module ID:** `ccs.survival.movement`  
**Namespace:** `CCS.Modules.CharacterController` (editor: `CCS.Modules.CharacterController.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-31  
**Status:** Playable player integration complete at **0.9.0** (input actions, prefab, movement/camera/stamina wiring)

---

## 0.9.0 player integration

| Asset / script | Role |
|----------------|------|
| `Assets/CCS/Survival/Input/CCS_Survival_InputActions.inputactions` | New Input System **Gameplay** + **UI** maps |
| `Assets/CCS/Survival/Prefabs/Player/PF_CCS_Player.prefab` | CharacterController + camera pivot + gameplay camera |
| `CCS_CharacterInputActionProvider` | Reads input actions → `CCS_CharacterInputSnapshot` |
| `CCS_PlayerGameplayController` | Composition glue: movement tick, cursor lock, stamina sprint gate |
| `CCS_SurvivalGameplayServiceRegistration` | Registers `CCS_CharacterMovementService`, stamina bind, updatable tick |

**Flow:** Input Actions → provider → `CCS_CharacterMovementService` → motor/camera. Sprint/jump raise Survival Core stamina hooks through composition (no direct module coupling).

**Deferred:** final character art, animation controller, input glyphs/rebinding UI.

---

## Purpose

Provide **Unity CharacterController-based locomotion** with walk/run/crouch/jump/gravity, look/camera hooks, and **decoupled stamina requests** for future Survival Core composition.

---

## Folder layout

```text
Assets/CCS/Modules/CharacterController/
  Runtime/
    Input/         → CCS_ICharacterInputProvider, snapshots, runtime bridge
    Movement/      → motor, service, states, snapshots
    Camera/        → look state, camera controller
    Profiles/      → movement/camera tuning + CCS_CharacterControllerProfile
    Events/        → movement and stamina hook contracts
    Validation/    → runtime-safe profile validation
  Editor/
    Validation/    → pipeline validator + menu
  Documentation/   → this file

Assets/CCS/Survival/Profiles/CharacterController/
  CCS_DefaultCharacterControllerProfile.asset   ← project configuration (not in Modules)
```

---

## CharacterController requirement

- Locomotion uses **`UnityEngine.CharacterController`** via `CCS_CharacterControllerMotor`.
- **No Rigidbody** movement in this module.
- Slopes and stairs use CharacterController **slope limit** and **step offset** from the profile.

---

## Animator root motion OFF

**Root motion must remain disabled** on the Animator. This motor owns world displacement through `CharacterController.Move`. Enabling root motion will fight the motor and break deterministic movement.

---

## Movement states

| State | Meaning |
|-------|---------|
| Idle | Grounded, no planar input |
| Walking | Grounded, walk speed |
| Running | Grounded, sprint/run speed |
| Crouching | Crouch held |
| Jumping | Jump impulse applied this frame |
| Falling | Airborne without fresh jump |

Grounding: `Grounded` / `Airborne` from `CharacterController.isGrounded`.

---

## Input abstraction

| Type | Role |
|------|------|
| `CCS_ICharacterInputProvider` | Supplies `CCS_CharacterInputSnapshot` each tick |
| `CCS_CharacterInputRuntimeBridge` | Serialized/test bridge until New Input System is wired |

**Deferred:** Input Actions asset, player map, and device rebinding.

---

## Camera — third-person (1.1.4)

| Type | Role |
|------|------|
| `CCS_CharacterLookState` | Yaw (movement facing) and pitch |
| `CCS_CharacterCameraController` | Applies mouse/gamepad look to yaw pivot + look target |
| `CCS_CharacterCameraProfile` | Sensitivity, pitch clamp, Cinemachine follow tuning |
| `CCS_PlayerCinemachineCameraDriver` | Survival composition — wires Cinemachine 3.1 ThirdPersonFollow |

**Default feel:** AAA survival/MMO-style third-person prototype.

- **Cinemachine 3.1** `CinemachineThirdPersonFollow` on `CM_GameplayCamera`
- **CameraPivot** (yaw) + **CameraLookTarget** (pitch) on `PF_CCS_Player`
- **Main Camera** at player root with `CinemachineBrain` (not inside the capsule)
- **Mouse look** uses reduced `mouseSensitivityX/Y` (calm, readable)
- **Gamepad look** uses `gamepadSensitivityX/Y` with delta time
- **Pitch** clamped (`minPitch` / `maxPitch`) to prevent flip/over-rotation
- **Interaction / combat / placement** raycasts use the gameplay `Camera` forward

Prefab batch setup:

```text
CCS.Survival.Editor.Development.CCS_PlayerThirdPersonCameraBootstrapSetup.ExecuteBatch
```

**Deferred:** camera collision / obstacle avoidance polish.

---

## Survival Core stamina integration plan

- While sprinting, the service raises **`StaminaDrainRequested`** with a **placeholder drain rate** from the profile.
- **Does not** call `CCS_SurvivalCoreService` or modify stamina in 0.3.8.
- Future bootstrap composition: subscribe to `StaminaDrainRequested` and apply modifiers through Survival Core APIs.

---

## Runtime service flow

1. Scene provides `CharacterController` + optional camera/follow transforms.
2. `CCS_CharacterMovementService.InitializeFromScene(controller, profile, …)`.
3. Optional `SetInputProvider` (defaults to `CCS_CharacterInputRuntimeBridge`).
4. Each frame: `TickMovement(deltaTime)` — look, then motor move.
5. Consumers read `CurrentSnapshot` or subscribe to events.

---

## Validation

| Menu | Path |
|------|------|
| Validate Character Controller | **CCS → Survival → Character Controller → Validate Character Controller** |

Registered on `CCS_SurvivalValidationPipeline` at editor load.

---

## Assemblies

| Assembly | References |
|----------|------------|
| `CCS.Modules.CharacterController.Runtime` | `CCS.Core.Runtime`, `CCS.Survival.Runtime` |
| `CCS.Modules.CharacterController.Editor` | Core, Survival runtime/editor, CharacterController runtime |

**No** reference to `CCS.Modules.SurvivalCore.Runtime` (stamina via events only).

---

## Deferred (post-0.3.8)

| Feature | Notes |
|---------|--------|
| Bootstrap installer + registry | Manual install plan |
| New Input System actions | Replace runtime bridge |
| Scene player prefab | PF_CCS_Player + bootstrap wiring |
| Camera collision / obstacle avoidance | Polish milestone |
| Combat, interaction, inventory, UI | Separate modules |

---

## Default profile defaults (0.3.8)

| Setting | Value |
|---------|-------|
| Walk speed | 4 |
| Run speed | 7 |
| Crouch speed | 2 |
| Jump height | 1.2 |
| Gravity | -20 |
| Grounded stick | -2 |
| Controller height | 1.8 |
| Radius | 0.35 |
| Step offset | 0.35 |
| Slope limit | 45° |
| Sprint stamina drain (placeholder) | 4 / sec |
