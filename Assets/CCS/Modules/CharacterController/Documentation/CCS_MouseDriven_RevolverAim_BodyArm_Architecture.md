# CCS Mouse-Driven Revolver Aim — Body/Arm Architecture

**Version:** 0.7.11 — planning document only  
**Author:** James Schilz  
**Created:** 2026-06-25  
**Status:** Architecture and contracts — **not implemented**

## Purpose

Define how Kevin's body, upper body, right arm, revolver muzzle, and reticle should all follow the **same mouse/camera aim target** so aim feels coherent when looking up/down or crossing the horizon.

This milestone documents data flow, component contracts, profile specs, validation rules, and implementation order. **No runtime solver, no Animator changes, and no gameplay fire/damage changes in v0.7.11.**

## Problem observed (v0.7.10f baseline)

- `SingleRevolverUpperBody` draw/hold/holster gives a right-arm aim pose, but the arm/gun visual does not fully track mouse/camera line of sight.
- Reticle and arm can feel disconnected, especially at pitch extremes and near the horizon.
- Reticle reveal is correctly gated by `CCS_OnRevolverAimHoldStarted` on `Fulldraw_Idle`, but reticle position still follows camera/hybrid drift rather than a unified aim target chain.
- Releasing RMB (stopping aim) must **immediately hide** the reticle when holster presentation starts. v0.7.10f clears readiness on holster start; future convergence work must preserve instant hide on aim intent release.

## Desired feel

| Input | Result |
|-------|--------|
| Mouse moves camera | Camera defines aim intent |
| Camera aim ray | Stable world-space aim target |
| Body / upper body | Reacts to aim direction within limits |
| Right arm + hand IK | Follows aim target, not independent reticle UI |
| Revolver muzzle | Points toward same target |
| Reticle | Presents converged screen result of aim + muzzle |
| RMB release | Reticle hides immediately; IK/body bias blends out |

**Non-goals for this architecture milestone:** dual revolvers, Camila wiring, new animation clips/layers/states, gameplay hitscan authority changes.

---

## Target data flow

```
Mouse / input look delta
  → CCS_CharacterCameraController
  → camera aim ray (viewport center or configured origin)
  → CCS_RevolverAimTargetResolver          [planned]
  → CCS_RevolverAimPresentationState         [planned aggregate/read model]
  → CCS_RevolverBodyAimPresenter             [planned; may evolve CCS_RevolverBodyAimFollowController]
  → CCS_RevolverArmAimIKPresenter            [planned; may evolve CCS_RevolverArmReticleIK]
  → muzzle direction (MuzzlePoint on equipped visual)
  → CCS_RevolverMuzzleLineOfSightResolver  [planned]
  → reticle convergence presenter            [planned; evolves CCS_MuzzleDrivenReticleController]
  → optional fire ray validation             [future; separate approval required]
```

### Core principle

**Camera/mouse owns aim intent.** The arm animation must not invent its own target. The reticle must not drive the arm directly. The reticle displays the **result** of aim/muzzle convergence.

Gameplay firing may continue using the current safe gameplay path until a later milestone explicitly changes combat authority.

---

## Planned component: `CCS_RevolverAimTargetResolver`

**Category:** presentation aim (local owner)  
**Placement:** player Model root or WeaponHudRoot sibling under local owner branch

### Responsibilities

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
| `CurrentFallbackMode` | Enum: RaycastHit / FallbackDistance / LastValidHold / Invalid |

### Rules

- Camera/mouse owns aim intent.
- Muzzle/barrel validates or visually converges to this target; it does not replace intent.
- Do not let upper-body animation invent an independent aim point.
- Do not make reticle UI the source of world aim.

---

## Planned component: `CCS_RevolverBodyAimPresenter`

**Category:** upper-body / full-body aim presentation  
**May evolve:** existing `CCS_RevolverBodyAimFollowController` (additive spine/chest bias today)

### Responsibilities

- Rotate or bias upper body toward `CCS_RevolverAimTargetResolver.AimWorldPoint`.
- Keep full-body yaw aligned enough that the right arm does not over-twist.
- Support pitch up/down with profile limits.
- Apply smoothing to avoid snapping at horizon crossings.
- When yaw offset exceeds threshold, blend character root/body yaw turn (presentation-only; must not fight gameplay motor authority without explicit design).

### Profile: `CCS_RevolverBodyAimProfile` (planned ScriptableObject)

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

## Planned component: `CCS_RevolverArmAimIKPresenter`

**Category:** right-arm IK presentation  
**May evolve:** existing `CCS_RevolverArmReticleIK` (currently arm-to-reticle; should become arm-to-aim-target)

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

### Profile: `CCS_RevolverArmIKProfile` (planned ScriptableObject)

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

---

## Planned component: `CCS_RevolverMuzzleLineOfSightResolver`

**Category:** muzzle/barrel presentation validation  
**Related doc:** `CCS_Revolver_Reticle_Barrel_LineOfSight_Plan.md`

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

---

## Planned aggregate: `CCS_RevolverAimPresentationState`

Read-only snapshot consumed by body presenter, arm IK, muzzle resolver, and reticle. Updated once per frame for local owner during aim.

Suggested fields:

- Resolver output (world point, direction, distance, validity)
- Body bias angles applied this frame
- IK weights active
- Muzzle convergence screen point
- `IsAimIntentActive` (gameplay RMB or approved debug setup pose)
- `IsReticleVisible` (readiness + intent + not hand-socket preview)
- `ShouldHideReticleImmediately` (true on holster start / RMB release)

---

## Reticle relationship (summary)

The reticle must **not** drive the arm.

```
Aim resolver → world target
     ├→ Body presenter → upper-body bias
     ├→ Arm IK presenter → hand/barrel alignment
     ├→ Muzzle LOS resolver → convergence validity
     └→ Reticle presenter → final screen position
```

### Future reticle modes (profile-driven)

| Mode | Description |
|------|-------------|
| **Camera Intent Reticle** | Camera center / camera ray screen result |
| **Muzzle Convergence Reticle** | Screen point from muzzle-to-target convergence |
| **Hybrid Reticle** | Center intent dot + muzzle convergence marker |
| **Future Dual Reticle** | Left/right muzzle-specific indicators |

Existing `CCS_RevolverReticlePresentationProfile` (v0.7.10e/f) retains **reveal timing, fade, and pitch stability**. Future `CCS_RevolverReticleConvergenceProfile` owns convergence/drift/dual-mode tuning.

### Aim lifecycle (including RMB release)

| Phase | Reticle | IK / body |
|-------|---------|-----------|
| Play start | Hidden | Off |
| Draw | Hidden | Off or minimal |
| Hold (`Fulldraw_Idle` event) | Visible | Blend in |
| Aim adjust | Visible | Track resolver target |
| RMB release / holster start | **Hidden immediately** | Blend out |
| Holster complete | Hidden | Off |

---

## Profile contracts (documentation-only specs)

### 1. `CCS_RevolverAimTargetProfile`

| Field | Purpose |
|-------|---------|
| `cameraRayDistance` | Max camera ray distance |
| `fallbackDistance` | No-hit fallback |
| `aimLayerMask` | Raycast layers for aim target |
| `obstructionLayerMask` | Layers for obstruction tests |
| `targetSmoothingTime` | World target smoothing |
| `maxTargetSnapDistance` | Max world snap per frame |
| `lastValidTargetHoldSeconds` | Hold on invalid projection |
| `nearWallCorrectionDistance` | Near geometry correction |

### 2. `CCS_RevolverBodyAimProfile`

See body presenter section above.

### 3. `CCS_RevolverArmIKProfile`

See arm IK section above.

### 4. `CCS_RevolverReticleConvergenceProfile`

| Field | Purpose |
|-------|---------|
| `reticleScreenSmoothTime` | Screen smoothing (may mirror v0.7.10e values initially) |
| `maxScreenSnapPixelsPerFrame` | Per-frame snap clamp |
| `maxReticleDriftPixels` | Max drift from center/intent |
| `convergenceBlendSpeed` | Blend toward muzzle convergence |
| `muzzleInfluenceAtCloseRange` | Close target muzzle weight |
| `muzzleInfluenceAtFarRange` | Far target muzzle weight |
| `dualReticleSeparationScale` | Dual mode spacing |
| `fallbackToCenterWhenInvalid` | Safe fallback |

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

## Recommended implementation order

1. **`CCS_RevolverAimTargetResolver` + `CCS_RevolverAimTargetProfile`** — single world target from camera; local owner only.
2. **`CCS_RevolverAimPresentationState`** — aggregate read model; wire diagnostics.
3. **`CCS_RevolverBodyAimPresenter`** — migrate/evolve body follow; profile-driven limits.
4. **`CCS_RevolverArmAimIKPresenter`** — arm/hand to world target; holster blend-out; preserve socket hierarchy.
5. **`CCS_RevolverMuzzleLineOfSightResolver`** — obstruction + convergence validity.
6. **Reticle convergence presenter update** — consume convergence profile; keep v0.7.10f reveal event gate.
7. **Validation utilities + batch entries** — hierarchy, non-owner, dual-mode guards.
8. **Manual smoke + optional gameplay authority review** — separate milestone for fire ray changes.

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

## Related documentation

- `CCS_Revolver_Reticle_Barrel_LineOfSight_Plan.md` — muzzle convergence and reticle modes
- `CCS_CharacterController_Animation_Rebuild_Architecture.md` — animation layer baseline
- `CCS_PlayerPrefab_Hierarchy_Architecture.md` — socket vs IK hierarchy rules
- `CCS_Equipment_Fit_Studio.md` — fit profile tuning workflow
