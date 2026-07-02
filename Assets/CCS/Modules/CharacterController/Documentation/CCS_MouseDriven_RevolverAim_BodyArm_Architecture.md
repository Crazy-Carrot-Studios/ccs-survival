# CCS Mouse-Driven Revolver Aim — Body/Arm Architecture

**Version:** 0.7.12a — v0.7.12a reticle consumes aim target resolver  
**Author:** James Schilz  
**Created:** 2026-06-25  
**Status:** v0.7.12a binds reticle to resolver; body/arm/muzzle/convergence remain planned

## Purpose

Define how Kevin's body, upper body, right arm, revolver muzzle, and reticle should all follow the **same mouse/camera aim target** so aim feels coherent when looking up/down or crossing the horizon.

This milestone documents data flow, component contracts, profile specs, validation rules, and implementation order. **No runtime solver, no Animator changes, and no gameplay fire/damage changes in v0.7.11.**

## 1. Current issue

- Camera/mouse aim, reticle screen position, upper-body animation, and muzzle direction are **not fully unified**.
- Player can aim with the mouse, but arm/gun presentation can feel disconnected from reticle and camera line.
- Reticle reveal is correctly gated by `CCS_OnRevolverAimHoldStarted` on `Fulldraw_Idle` (v0.7.10f).
- Releasing RMB must **immediately hide** the reticle when holster starts (preserved contract).
- Shooting can still feel shoulder-origin because reticle follows camera/hybrid drift while arm follows animation + partial IK.

## 2. Target data flow

```
Mouse / input look delta
  → CCS_CharacterCameraController
  → camera aim ray (viewport center or configured origin)
  → CCS_RevolverAimTargetResolver          [v0.7.12 prototype — diagnostics/future only]
  → CCS_RevolverAimPresentationState         [planned aggregate/read model]
  → CCS_RevolverBodyAimPresenter             [planned; may evolve CCS_RevolverBodyAimFollowController]
  → CCS_RevolverArmAimIKPresenter            [planned; may evolve CCS_RevolverArmReticleIK]
  → muzzle direction (MuzzlePoint on equipped visual)
  → CCS_RevolverMuzzleLineOfSightResolver  [planned]
  → CCS_RevolverMuzzleLineOfSightResolver  [planned]
  → CCS_RevolverReticleConvergencePresenter [planned; evolves CCS_MuzzleDrivenReticleController]
  → optional future fire validation             [separate approval required]
```

## 3. Single revolver aim behavior (planned)

- One camera aim intent target from `CCS_IRevolverAimTargetSource`.
- One muzzle (`MuzzlePoint`), one convergence point, one reticle (mode from profile).
- Body + upper body bias toward target within profile limits; full-body yaw when threshold exceeded.
- Right arm IK moves `CCS_RightHandIKTarget` toward target during aim hold (after animation event readiness).
- Weapon remains on `CCS_HandSocket_Right` → `CCS_RightHandRevolverAttachmentOffset`.
- Reticle displays resolved convergence screen point — **not** the IK driver.
- Hidden during draw; visible after `Fulldraw_Idle` event; **hidden immediately on RMB release**.

## 4. Future dual revolver behavior (planned)

- Right and left muzzle resolvers with shoulder-width separation.
- Each arm/gun may solve toward its own target line or shared intent with lateral offset.
- Reticle mode (`CCS_RevolverReticleMode`) selects presentation:
  - `SingleCameraIntent` — center camera intent only
  - `SingleMuzzleConvergence` — one muzzle-converged reticle
  - `HybridIntentAndMuzzle` — center intent + muzzle marker
  - `DualMuzzleReticles` — left/right muzzle indicators
- Dual modes **must not** activate for single revolver without explicit profile.
- Dual weapon animation layers remain a later milestone (not v0.7.12–v0.7.16).

## 5. Camera aim intent

Mouse look input drives `CCS_CharacterCameraController`. The active camera defines aim intent via viewport-center ray (or configured aim origin). **No presentation system may invent a competing world target.**

**Core principle:** Reticle must not drive the arm. Mouse/camera creates the world aim target; body, arm, muzzle, and reticle all resolve from that same target. Gameplay firing may continue on the current path until a later approved milestone.

---

## Implemented component: `CCS_RevolverAimTargetResolver` (v0.7.12)

**Path:** `Assets/CCS/Modules/CharacterController/Runtime/Aiming/CCS_RevolverAimTargetResolver.cs`  
**Contract:** `CCS_IRevolverAimTargetSource`  
**Placement:** `PF_CCS_CharacterController_Player_Networked` → `Model` → `Aiming` (non-root)

### v0.7.12a scope

- `CCS_MuzzleDrivenReticleController` consumes `CCS_IRevolverAimTargetSource.AimWorldPoint` as primary screen target.
- Reticle no longer performs independent hit/no-hit camera raycast when resolver is wired.
- Horizon pitch snap mitigated by shared resolver smoothing/clamp/hold-last-valid.
- Reticle reveal timing unchanged (Fulldraw_Idle Animation Event).

### v0.7.12 scope

- Resolves stable world-space aim target from local owner camera viewport-center ray.
- Profile-driven tuning via `CCS_RevolverAimTargetProfile`.
- Diagnostics/future-system only — does **not** drive body aim, arm IK, muzzle LOS, reticle convergence, or gameplay fire/damage.
- Local-owner gating via `NetworkObject.IsOwner` (solo/offline treats local player as owner).
- Optional debug rays when Aim Diagnostics / Visual Debug Helpers are enabled.

### Responsibilities

- Read local owner camera aim from active camera (`aimCamera` → `CCS_CharacterMovementCameraContext` → `Camera.main`).
- Produce a stable **world-space aim target** from camera center ray.
- Raycast with configurable layer masks.
- If no hit, use fallback distance from profile.
- Hold last valid target briefly during invalid projection.
- Clamp sudden target jumps in world space.
- Expose read-only presentation state:

| Property | Meaning |
|----------|---------|
| `AimWorldPoint` | Current resolved world target |
| `AimDirection` | Normalized direction from aim origin to target |
| `AimDistance` | Distance to target |
| `HasValidAimTarget` | Target is usable this frame |
| `IsObstructed` | Diagnostic obstruction between camera and target |

Profile path: `Assets/CCS/Modules/CharacterController/Profiles/Aiming/CCS_RevolverAimTargetProfile.asset`

### Rules

- Camera/mouse owns aim intent.
- Muzzle/barrel validates or visually converges to this target; it does not replace intent.
- Do not let upper-body animation invent an independent aim point.
- Do not make reticle UI the source of world aim.

---

## Planned component: `CCS_RevolverAimTargetResolver` (superseded by v0.7.12 implementation above)

**Planned path:** `Assets/CCS/Modules/CharacterController/Runtime/Aiming/CCS_RevolverAimTargetResolver.cs`  
**Contract (v0.7.11):** `CCS_IRevolverAimTargetSource` — interface-only; no runtime behavior in v0.7.11.

**Category:** presentation aim (local owner)  
**Placement:** player Model root or WeaponHudRoot sibling under local owner branch

### Responsibilities (reference — implemented in v0.7.12)

- Read local owner camera aim from `CCS_CharacterCameraController` / active camera.
- Produce a stable **world-space aim target** from camera center ray (or configured aim origin).
- Raycast with configurable layer masks.
- If no hit, use fallback distance from profile.
- Hold last valid target briefly during invalid projection (reuse v0.7.10e stability patterns).
- Clamp sudden target jumps in world space.
- Expose read-only presentation state:

| Property | Meaning |
|----------|---------|
| `AimWorldPoint` | Current resolved world target |
| `AimDirection` | Normalized direction from aim origin to target |
| `AimDistance` | Distance to target |
| `HasValidAimTarget` | Target is usable this frame |
| `IsObstructed` | Muzzle or camera path blocked (future cooperation with muzzle resolver) |

Planned profile path: `Assets/CCS/Modules/CharacterController/Profiles/Aiming/CCS_RevolverAimTargetProfile.asset`

### Rules

- Camera/mouse owns aim intent.
- Muzzle/barrel validates or visually converges to this target; it does not replace intent.
- Do not let upper-body animation invent an independent aim point.
- Do not make reticle UI the source of world aim.

---

## 6. Body / upper-body response — `CCS_RevolverBodyAimPresenter`

**Planned path:** `Assets/CCS/Modules/CharacterController/Runtime/Aiming/CCS_RevolverBodyAimPresenter.cs`  
**May evolve:** `CCS_RevolverBodyAimFollowController`

### Responsibilities

- Rotate or bias upper body toward `CCS_RevolverAimTargetResolver.AimWorldPoint`.
- Keep full-body yaw aligned enough that the right arm does not over-twist.
- Support pitch up/down with profile limits.
- Apply smoothing to avoid snapping at horizon crossings.
- When yaw offset exceeds threshold, blend character root/body yaw turn (presentation-only; must not fight gameplay motor authority without explicit design).

### Profile: `CCS_RevolverBodyAimProfile`

**Planned path:** `Assets/CCS/Modules/CharacterController/Profiles/Aiming/CCS_RevolverBodyAimProfile.asset`

| Field | Purpose |
|-------|---------|
| `maxUpperBodyYawDegrees` | Max additive upper-body yaw toward target |
| `maxUpperBodyPitchUpDegrees` | Max pitch up bias |
| `maxUpperBodyPitchDownDegrees` | Max pitch down bias |
| `fullBodyTurnThresholdDegrees` | Yaw beyond this triggers body turn blend |
| `fullBodyTurnBlendSpeed` | Speed of full-body yaw correction |
| `upperBodyYawBlendSpeed` | Smoothing for upper-body yaw |
| `upperBodyPitchBlendSpeed` | Smoothing for upper-body pitch |
| `shoulderTwistWeight` | Shoulder contribution cap |
| `spineTwistWeight` | Spine contribution cap |
| `pelvisCounterTwistWeight` | Counter-rotation to avoid unnatural pelvis |
| `aimPoseLayerWeight` | Blend weight vs base hold animation |
| `minAimDistance` | Close-range correction start |
| `closeAimCorrectionStrength` | Extra upper-body correction when target is close |

### Rules

- Small mouse/camera movements adjust upper body and right arm presentation.
- Larger yaw offsets rotate whole character body (within motor/presentation contract).
- Do not let the right shoulder become the apparent bullet source.
- Do not overdrive spine/shoulder bones beyond profile limits.

---

## 7. Right arm / hand IK response — `CCS_RevolverArmAimIKPresenter`

**Planned path:** `Assets/CCS/Modules/CharacterController/Runtime/Aiming/CCS_RevolverArmAimIKPresenter.cs`  
**May evolve:** `CCS_RevolverArmReticleIK`

### Responsibilities

- Move `CCS_RightHandIKTarget` based on resolved aim world point.
- Keep hand/grip relationship via equipment socket hierarchy.
- Keep `CCS_RightElbowHint` stable.
- Maintain wrist orientation so barrel points toward target.
- Blend IK in only during aim hold (after readiness / animation event gate).
- Blend IK out during holster immediately when aim intent ends.
- Avoid fighting base draw/hold Wild West animation.

### Hierarchy definitions (must not change ownership)

| Transform | Role |
|-----------|------|
| `CCS_HandSocket_Right` | Equipment attachment socket |
| `CCS_RightHandRevolverAttachmentOffset` | Fit profile offset parent (`CCS_RevolverM1879_RightHandEquipped_Fit`) |
| `CCS_RightHandIKTarget` | IK control target — **not** weapon parent |
| `CCS_RightElbowHint` | Elbow hint only |
| `MuzzlePoint` | Muzzle/barrel reference on equipped revolver visual |
| Reticle UI | Presentation output — **not** arm movement source |

### Rules

- Weapon visual remains parented to hand socket / fit offset chain.
- IK target moves arm/hand pose; gun is never parented to IK targets.
- IK follows **aim resolver world target**, not reticle screen position directly.
- Reticle displays convergence result after muzzle/aim solve.

### Profile: `CCS_RevolverArmIKProfile`

**Planned path:** `Assets/CCS/Modules/CharacterController/Profiles/Aiming/CCS_RevolverArmIKProfile.asset`

| Field | Purpose |
|-------|---------|
| `rightHandIKWeight` | Hand IK weight during aim hold |
| `rightElbowHintWeight` | Elbow hint influence |
| `handPositionBlendSpeed` | Position blend toward IK goal |
| `handRotationBlendSpeed` | Rotation blend toward barrel alignment |
| `wristRollOffset` | Fine wrist roll tuning |
| `closeRangeAimOffset` | Offset when target is very close |
| `maxHandReachOffset` | Clamp hand IK reach |
| `maxPitchCompensation` | Extra pitch correction cap |
| `aimBlendInSeconds` | IK blend in after hold readiness |
| `aimBlendOutSeconds` | IK blend out on holster start |
| `holsterIKDisableDelay` | Optional delay before IK fully off |

---

## 8. Muzzle line-of-sight relationship — `CCS_RevolverMuzzleLineOfSightResolver`

**Planned path:** `Assets/CCS/Modules/CharacterController/Runtime/Aiming/CCS_RevolverMuzzleLineOfSightResolver.cs`  
**Contract (v0.7.11):** `CCS_IRevolverMuzzleAimSource`

### Responsibilities

- Read `MuzzlePoint` from equipped revolver visual (`CCS_PlayerEquipmentVisualController`).
- Compute muzzle-to-aim-target direction.
- Check clear line from muzzle to aim target (obstruction layers).
- Detect near-wall obstruction cases.
- Provide visual convergence result for reticle (screen-space point + validity).
- **Do not** change gameplay damage until separately approved.

### Single revolver behavior

- One muzzle, one reticle, one camera aim intent target.
- Muzzle visually converges toward resolver target.
- Reticle shows stable convergence point.

### Future dual revolver behavior

- Right and left muzzle each have independent lines.
- Each arm solves toward its side's target line (or shared intent with lateral offset).
- Reticle modes (profile-selected):
  1. Single center intent reticle + subtle left/right muzzle indicators
  2. Dual left/right reticles
  3. Hybrid center reticle with spread/convergence dots

Dual mode must not activate for single revolver without explicit profile.

Dual mode must not activate for single revolver without explicit profile.

### Profile: `CCS_RevolverMuzzleLineOfSightProfile`

**Planned path:** `Assets/CCS/Modules/CharacterController/Profiles/Aiming/CCS_RevolverMuzzleLineOfSightProfile.asset`

| Field | Purpose |
|-------|---------|
| `muzzleRayDistance` | Max muzzle ray length |
| `obstructionLayerMask` | Obstruction test layers |
| `nearWallCorrectionDistance` | Near-wall handling distance |
| `minValidMuzzleDistance` | Minimum valid muzzle-to-target distance |
| `maxMuzzleTargetAngleDegrees` | Max angle off bore before fallback |
| `closeRangeConvergenceStrength` | Close-range muzzle influence |
| `farRangeConvergenceStrength` | Far-range muzzle influence |
| `lastValidMuzzleTargetHoldSeconds` | Hold last valid convergence |

---

## 9. Reticle convergence relationship — `CCS_RevolverReticleConvergencePresenter`

**Planned path:** `Assets/CCS/Modules/CharacterController/Runtime/Visuals/CCS_RevolverReticleConvergencePresenter.cs`  
**May evolve:** `CCS_MuzzleDrivenReticleController`

### Responsibilities

- Display reticle at resolved aim/convergence screen point.
- Smooth screen movement and clamp snap (reuse v0.7.10e stability patterns where appropriate).
- Keep v0.7.10f Animation Event reveal timing via `CCS_RevolverReticlePresentationProfile`.
- Local-owner-only; block hand socket preview.
- Support future muzzle convergence and dual reticle modes via `CCS_RevolverReticleMode`.

### Profile: `CCS_RevolverReticleConvergenceProfile`

**Planned path:** `Assets/CCS/Modules/CharacterController/Profiles/Reticle/CCS_RevolverReticleConvergenceProfile.asset`

| Field | Purpose |
|-------|---------|
| `reticleScreenSmoothTime` | Screen smoothing |
| `maxScreenSnapPixelsPerFrame` | Per-frame snap clamp |
| `maxReticleDriftPixels` | Max drift from intent |
| `convergenceBlendSpeed` | Blend toward muzzle convergence |
| `muzzleInfluenceAtCloseRange` | Close-range muzzle weight |
| `muzzleInfluenceAtFarRange` | Far-range muzzle weight |
| `fallbackToCameraCenterWhenInvalid` | Safe fallback |
| `dualReticleSeparationScale` | Dual mode spacing |
| `dualReticleMode` | `CCS_RevolverReticleMode` |

---

## Planned aggregate: `CCS_RevolverAimPresentationState`

**Contract (v0.7.11):** `CCS_IRevolverAimPresentationStateSource`

---

## Existing baseline components (v0.7.10f)

| Component | Current role | Future relationship |
|-----------|--------------|---------------------|
| `CCS_SingleRevolverAimAnimator` | Draw/hold/holster + reticle readiness event | Unchanged animation ownership; feeds readiness only |
| `CCS_RevolverReticleAnimationEventReceiver` | Forwards `Fulldraw_Idle` event | Unchanged |
| `CCS_MuzzleDrivenReticleController` | Screen reticle + v0.7.10e stability | Becomes convergence presenter consumer |
| `CCS_RevolverBodyAimFollowController` | Additive spine/chest toward camera | Evolve into `CCS_RevolverBodyAimPresenter` |
| `CCS_RevolverArmReticleIK` | Arm IK toward reticle | Evolve into arm-to-aim-target IK |
| `CCS_RevolverM1879_RightHandEquipped_Fit` | Hand socket offset | Unchanged source of truth |

Fit profile path: `Assets/CCS/Modules/CharacterController/Profiles/EquipmentFitting/RevolverM1879/CCS_RevolverM1879_RightHandEquipped_Fit.asset`  
Offset parent: `CCS_HandSocket_Right/CCS_RightHandRevolverAttachmentOffset`

---

## 10. ScriptableObject profile contracts (summary)

| Profile | Planned asset path |
|---------|-------------------|
| `CCS_RevolverAimTargetProfile` | `Profiles/Aiming/CCS_RevolverAimTargetProfile.asset` |
| `CCS_RevolverBodyAimProfile` | `Profiles/Aiming/CCS_RevolverBodyAimProfile.asset` |
| `CCS_RevolverArmIKProfile` | `Profiles/Aiming/CCS_RevolverArmIKProfile.asset` |
| `CCS_RevolverMuzzleLineOfSightProfile` | `Profiles/Aiming/CCS_RevolverMuzzleLineOfSightProfile.asset` |
| `CCS_RevolverReticleConvergenceProfile` | `Profiles/Reticle/CCS_RevolverReticleConvergenceProfile.asset` |

Existing (unchanged): `CCS_RevolverReticlePresentationProfile` — reveal timing, fade, pitch stability (v0.7.10e/f).

## 11. Runtime implementation risks

| Risk | Mitigation |
|------|------------|
| IK fights Wild West draw/hold animation | Blend weights; holster disable; event readiness gate |
| Shoulder-origin feel returns | Body yaw threshold + muzzle convergence profile |
| Reticle/arm mismatch at pitch extremes | Shared aim resolver; separate convergence profile |
| Near-wall reticle jump | Muzzle LOS + last-valid hold + snap clamp |
| Non-owner presentation leak | Local owner gates on all presenters |
| Gameplay authority drift | Explicit milestone approval for fire ray changes |
| Profile duplication | Keep reveal (`CCS_RevolverReticlePresentationProfile`) separate from convergence |
| Root MonoBehaviour / hierarchy creep | No prefab migration in v0.7.12–v0.7.16 without audit |

## 12. Validation rules

See `CCS_MouseDriven_RevolverAim_ValidationPlan.md`.

## 13. Staged implementation roadmap

| Milestone | Scope |
|-----------|--------|
| **v0.7.11** | Architecture + interface contracts only (this document) |
| **v0.7.12** | Aim Target Resolver prototype — camera world target; reticle unchanged or debug viz only |
| **v0.7.13** | Body Aim Presenter prototype — upper-body/body yaw/pitch; no fire change |
| **v0.7.14** | Right Arm IK / Muzzle Alignment prototype — IK follows aim target; weapon on hand socket |
| **v0.7.15** | Muzzle Reticle Convergence prototype — reticle uses convergence; no damage ray change |
| **v0.7.16** | Near-wall / obstruction handling — visual only unless damage approved separately |
| **Later** | Dual revolver lines, dual reticle modes, dual weapon animation layers |

### Interface contracts (v0.7.11)

| File | Role |
|------|------|
| `Runtime/Aiming/CCS_IRevolverAimTargetSource.cs` | Camera aim target read model |
| `Runtime/Aiming/CCS_IRevolverMuzzleAimSource.cs` | Muzzle LOS / convergence read model |
| `Runtime/Aiming/CCS_IRevolverAimPresentationStateSource.cs` | Aggregate presentation state |
| `Runtime/Visuals/CCS_RevolverReticleMode.cs` | Future reticle mode enum |

---

## Future validation rules (implementation milestone)

Validation must **fail** if:

- Weapon attaches to IK target instead of `CCS_HandSocket_Right`
- Arm IK target or elbow hint missing on player prefab
- Muzzle point missing on equipped revolver visual
- Aim target resolver missing on local owner presentation branch
- Reticle has no readiness source
- Reticle visible when not aiming
- Reticle remains visible after holster starts / RMB release
- Muzzle ray source is behind camera or invalid
- Full-body aim yaw exceeds limit without body rotation
- Arm IK updates for non-owner players
- Dual reticle mode enabled for single revolver without explicit profile
- Gameplay damage ray changes without explicit milestone approval
- New Animator layers/states or animation clip edits introduced without approval

Validation should **warn** if:

- Body/IK profiles use hardcoded runtime constants instead of ScriptableObjects
- Convergence profile duplicates v0.7.10e timing fields (should stay separated)

---

## Related documentation

- `CCS_MouseDriven_RevolverAim_ValidationPlan.md` — future validation rules
- `CCS_Revolver_Reticle_Barrel_LineOfSight_Plan.md` — muzzle convergence and reticle modes
- `CCS_CharacterController_Animation_Rebuild_Architecture.md` — animation layer baseline
- `CCS_PlayerPrefab_Hierarchy_Architecture.md` — socket vs IK hierarchy rules
- `CCS_Equipment_Fit_Studio.md` — fit profile tuning workflow

---

## Explicit non-goals until user confirms implementation

- Runtime aim solver code in v0.7.11
- Animator Controller / clip changes
- Gameplay damage, ammo, pickup, inventory, fire cadence, ownership changes
- Dual revolver wiring
- Camila wiring
- Kevin / EnemyAI visual rewrites
- Prefab hierarchy migration in this planning pass

---

## Explicit non-goals until user confirms v0.7.12+
