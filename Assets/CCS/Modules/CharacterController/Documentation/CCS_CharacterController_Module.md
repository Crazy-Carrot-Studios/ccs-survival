# CCS Survival — Character Controller Module

**Milestone:** 0.3.8 — Character Controller Module Foundation  
**Module ID:** `ccs.survival.movement`  
**Namespace:** `CCS.Modules.CharacterController` (editor: `CCS.Modules.CharacterController.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-28  
**Status:** Foundation complete (not wired to bootstrap installer or scene prefab)

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

## Camera-look foundation

| Type | Role |
|------|------|
| `CCS_CharacterLookState` | Yaw (facing) and pitch |
| `CCS_CharacterCameraController` | Applies look input; follow/look hooks |
| `CCS_CharacterCameraProfile` | Sensitivity and pitch clamp |

**Deferred:** Camera collision, Cinemachine, final polish.

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
| Camera collision / Cinemachine | Polish milestone |
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
