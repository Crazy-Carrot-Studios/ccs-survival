# CCS Character Controller — Animation Rebuild Architecture

**Version:** 0.7.4 (Phase 3C — planning only)  
**Author:** James Schilz  
**Last updated:** 2026-06-29  
**Baseline:** v0.7.7 Kevin/EnemyAI visuals + locomotion-only Base Layer (`b980f09`)

## v0.7.8 — Single revolver aim upper-body layer (implemented)

- Added masked `SingleRevolverUpperBody` layer on `AC_CCS_Player_Locomotion_StarterAssets.controller`
- Presentation parameters: `IsAiming`, `RevolverDrawTrigger`, `RevolverHolsterTrigger`
- Clips (read-only Wild West FBX sub-assets): `Idle_Fulldraw_Revolver`, `Fulldraw_Idle`, `Idle_Full_Holster_Revolver`
- Mask: `AM_CCS_Revolver_UpperBodyRightArm_Aim`
- Presentation driver: `CCS_SingleRevolverAimAnimator` on player `Model` root
- Readiness contract: `CCS_IRevolverAimPresentationReadinessSource` gates reticle visibility until `Revolver_Aim_Hold` / `Fulldraw_Idle` (v0.7.10d)
- Gameplay aim/fire remains owned by `CCS_RevolverController`
- Not in v0.7.8: fire, reload, interaction, dual revolver layers; remote player aim presentation

## v0.7.9 — Validation cleanup and aim setup pose toggle (implemented)

- Moved `PF_CCS_TestWeaponDamageTarget` to `CharacterController/Prototyping/Prefabs/Targets/`
- Removed legacy `CCS_TestDetectionCube` validation path and bootstrap scripts
- Added diagnostics **Force Revolver Aim Setup Pose** on `CCS_DiagnosticsManager` (presentation-only)
- `CCS_SingleRevolverAimAnimator` + `CCS_PlayerEquipmentVisualController` honor setup pose via `CCS_IRevolverAimSetupPoseDebugSource` / `CCS_RevolverAimSetupPoseDebugRegistry`
- Removed `CapsuleVisual` and `VisualGlasses` from production player prefab
- Animator Controller unchanged from v0.7.8 (still Base Layer + `SingleRevolverUpperBody`)

## v0.7.11 — Mouse-driven revolver aim body/arm architecture (planning only)

- Added `CCS_MouseDriven_RevolverAim_BodyArm_Architecture.md` — full aim data flow from mouse/camera to body, arm IK, muzzle, and reticle
- Planned components: `CCS_RevolverAimTargetResolver`, `CCS_RevolverBodyAimPresenter`, `CCS_RevolverArmAimIKPresenter`, `CCS_RevolverMuzzleLineOfSightResolver`
- Planned profiles: `CCS_RevolverAimTargetProfile`, `CCS_RevolverBodyAimProfile`, `CCS_RevolverArmIKProfile`, `CCS_RevolverReticleConvergenceProfile`
- Reticle must not drive arm IK; aim resolver owns world target; reticle presents convergence result
- RMB release must hide reticle immediately on holster start (preserved contract from v0.7.10f)
- Added interface contracts: `CCS_IRevolverAimTargetSource`, `CCS_IRevolverMuzzleAimSource`, `CCS_IRevolverAimPresentationStateSource`, `CCS_RevolverReticleMode`
- Added `CCS_MouseDriven_RevolverAim_ValidationPlan.md` and staged roadmap v0.7.12–v0.7.16

## v0.7.10f — Reticle reveal animation event (implemented)

- Added `CCS_OnRevolverAimHoldStarted` Animation Event at the start of `Fulldraw_Idle` (FBX importer clip metadata only)
- Added `CCS_RevolverReticleAnimationEventReceiver` on Kevin's Animator GameObject; forwards to `CCS_SingleRevolverAimAnimator.NotifyRevolverAimHoldAnimationEvent()`
- `CCS_RevolverReticlePresentationProfile.ReticleRevealSource` defaults to `AnimationEvent`; draw normalized-time reveal disabled
- Reticle hidden at Play start and during draw; revealed by hold animation event; hidden immediately on holster
- v0.7.10e screen smoothing/clamp and pitch stability retained; barrel/muzzle line-of-sight still deferred

## v0.7.10e — Reticle reveal timing and pitch stability (implemented)

- Added `CCS_RevolverReticlePresentationProfile` for late-draw reveal timing and screen smoothing/clamp tuning
- Extended `CCS_IRevolverAimPresentationReadinessSource` with `IsAimPresentationInReticleRevealWindow`
- Reticle appears during late `Revolver_Draw` before full `Revolver_Aim_Hold`; still hidden at Play start and during early draw
- `CCS_MuzzleDrivenReticleController` stabilizes camera/current-mode target (no-hit fallback, last-valid hold, pitch dead zone, SmoothDamp, snap clamp)
- Barrel/muzzle line-of-sight remains deferred; no gameplay fire/damage changes

## v0.7.10d — Reticle aim readiness gate (implemented)

- Added `CCS_IRevolverAimPresentationReadinessSource` implemented by `CCS_SingleRevolverAimAnimator`
- `CCS_MuzzleDrivenReticleController` hides reticle by default and shows it only after `Revolver_Aim_Hold` / `Fulldraw_Idle`
- Force Revolver Hand Socket Preview never shows reticle; setup pose shows reticle only after hold readiness
- Right-hand fit profile unchanged; barrel line-of-sight reticle plan documented only (`CCS_Revolver_Reticle_Barrel_LineOfSight_Plan.md`)
- No gameplay ownership/ammo/damage/fire changes; no new Animator layers/states or clip edits

## Purpose

Document the production-ready target for a future Character Controller animation system. This milestone defines layers, parameter contracts, presentation boundaries, and implementation order. **No animation import, no CC4 import, and no Animator Controller rebuild occurs in v0.7.4.**

## Design principles

1. **Gameplay owns state.** Aiming, shooting, damage, pickup eligibility, and interaction locks live in gameplay systems (`CCS_RevolverController`, interaction scanner, AI brain, etc.).
2. **Animator presents visuals only.** Layers and parameters reflect presentation; they do not author gameplay outcomes.
3. **Graceful degradation.** If an upper-body or interaction layer is missing, gameplay must continue without Animator errors or missing-parameter warnings.
4. **Centralized contracts.** Parameter names and hashes live in `CCS_CharacterAnimationParameterIds`. Weapon presentation modes live in `CCS_CharacterWeaponAnimationMode`. Future bridges implement `CCS_ICharacterAnimationPresenter`.
5. **Cosmetic events only.** Animation events (when added later) may trigger sound, VFX, shell ejection, or footstep audio — never damage, pickup success, or interaction completion.

## Current active state (v0.7.3 preserved)

| Item | Value |
|------|-------|
| **Controller** | `AC_CCS_Player_Locomotion_StarterAssets.controller` |
| **Layers** | `Base Layer` only |
| **Parameters** | `SpeedNormalized`, `IsGrounded`, `IsSprinting`, `JumpTrigger` |
| **States** | `Idle`, `Walk`, `Sprint`, `Jump`, `InAir` |
| **Active writer** | `CCS_PlayerLocomotionAnimator` (via `CCS_CharacterAnimationParameterIds`) |

---

## 1. Base Locomotion Layer (active)

**Status:** Implemented and wired on player and AI bandit.

### Responsibilities

- Idle
- Walk
- Run / sprint
- Jump
- Fall / in-air
- Land (optional future state when clips and transitions are imported)

### Active parameters

| Parameter | Type | Owner |
|-----------|------|-------|
| `SpeedNormalized` | Float | `CCS_PlayerLocomotionAnimator` |
| `IsGrounded` | Bool | `CCS_PlayerLocomotionAnimator` |
| `IsSprinting` | Bool | `CCS_PlayerLocomotionAnimator` |
| `JumpTrigger` | Trigger | `CCS_PlayerLocomotionAnimator` |

### Rules

- No aim-strafe parameters on Base Layer.
- No weapon or interaction triggers on Base Layer.
- AI locomotion may share the same controller asset; AI must not write removed revolver parameters to Base Layer.

---

## 2. Future Upper Body Weapon Layer (not implemented)

**Status:** Design only. Do not add to Animator Controller until clip import and retargeting milestones are approved.

### Target weapon modes (`CCS_CharacterWeaponAnimationMode`)

| Mode | Description |
|------|-------------|
| `None` | Unarmed / no upper-body weapon presentation |
| `SingleRevolver` | One-handed revolver aim, fire, reload |
| `DualRevolver` | Dual revolver idle, aim, alternating fire, reload/equip/unequip (later) |

### Responsibilities

- One-handed revolver aim pose
- Single revolver fire presentation
- Single revolver reload presentation
- Dual revolver idle / aim
- Dual revolver alternating fire
- Dual revolver reload, equip, unequip (later phases)

### Planned parameters (design only — not active in v0.7.4)

See `CCS_CharacterAnimationParameterIds.FutureDesignOnly` and the parameter table in `CCS_CharacterAnimationParameterIds.cs`.

### Gameplay boundary

- `CCS_RevolverController` and `CCS_AIWeaponController` continue to own fire cadence, ammo, and damage.
- A future `CCS_CharacterWeaponAnimationPresenter` (implements `CCS_ICharacterAnimationPresenter`) will mirror gameplay state for visuals only.

---

## 3. Future Interaction Layer (not implemented)

**Status:** Design only.

### Responsibilities

- Pickup right hand
- Open / close door
- Use workbench / container
- Contextual one-shot actions

### Planned parameters (design only)

- `InteractionTrigger`
- `InteractionType` (int or enum hash)

### Gameplay boundary

- `CCS_PlayerInteractionAnimator` retains interaction busy / control lock without Animator triggers in v0.7.3+.
- Future presentation bridge may play one-shot interaction clips when clips exist; interaction success remains gameplay-driven.

---

## 4. Future Additive Aim / Pose Layer (not implemented)

**Status:** Design only.

### Responsibilities

- Aim pitch presentation
- Subtle torso / arm offsets
- Non-gameplay cosmetic posing only

### Planned parameters (design only)

- `AimPitch`, `AimYaw` (or blend tree drivers)
- Optional additive layer weight driven by presentation bridge

### Gameplay boundary

- Camera aim, reticle, and IK gameplay paths remain independent of this layer.
- Layer may be disabled entirely without affecting aim/fire gameplay.

---

## Script-driven routing (future)

```
Gameplay systems (motor, revolver, interaction, AI brain)
        │
        ▼ read-only state
CCS_ICharacterAnimationPresenter (presentation bridge)
        │
        ▼ hashes from CCS_CharacterAnimationParameterIds
Animator Controller layers (locomotion → weapon → interaction → additive)
```

No gameplay system should call `Animator.SetTrigger` for weapon or interaction directly after rebuild; routes go through the presenter interface.

---

## CC4 retargeting (future)

- CC4 humanoid import and retargeting are **out of scope** until a dedicated milestone after architecture sign-off.
- Locomotion clips currently on disk remain the Base Layer source until replaced.
- `PF_CCS_Player_Visual` is not modified during architecture or v0.7.4 planning.

---

## Recommended implementation phases (after user sign-off)

| Phase | Scope |
|-------|--------|
| **3C (v0.7.4)** | Architecture docs, parameter IDs, mode enum, presenter interface, validation — **current** |
| **3D** | Import / isolate locomotion refresh if needed; verify Base Layer only |
| **3E** | Single revolver upper-body layer + presenter implementation |
| **3F** | Interaction one-shots + gameplay lock sync |
| **3G** | Dual revolver mode + additive aim |
| **3H** | CC4 character retargeting pass |

---

## Explicit non-goals (v0.7.4)

- No Insane Gunner or MoCap clip import
- No Animator Controller layer/state rebuild with real weapon clips
- No animation clip edits
- No `PF_CCS_Player_Visual` changes
- No Animation Fit Studio reintroduction

## Related assets

| Asset | Role |
|-------|------|
| `CCS_CharacterAnimationParameterIds.cs` | Active + future parameter name contract |
| `CCS_CharacterWeaponAnimationMode.cs` | Weapon presentation mode enum |
| `CCS_ICharacterAnimationPresenter.cs` | Presentation boundary interface |
| `CCS_CharacterControllerPhase3CValidationUtility.cs` | v0.7.4 architecture validation |
| `Logs/CharacterController/AnimationRebuild/CCS_AnimationRebuildArchitecture_v0.7.4.md` | Generated architecture report |
