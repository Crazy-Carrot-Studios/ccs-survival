# CCS Revolver Reticle — Barrel Line-of-Sight Plan

**Version:** planning document — barrel LOS still deferred after v0.7.12a  
**Author:** James Schilz  
**Created:** 2026-06-30  
**Status:** Plan only — not implemented

v0.7.12a binds reticle screen position to `CCS_RevolverAimTargetResolver.AimWorldPoint`. Muzzle/barrel line-of-sight and reticle convergence remain deferred.

## Purpose

Define the future presentation goal: the reticle should visually align with the end of the revolver barrel / muzzle direction instead of feeling like it originates from the shoulder or generic camera center.

v0.7.10d gated reticle visibility on aim presentation readiness. v0.7.10e added screen/current-mode pitch stabilization. v0.7.10f moved primary reveal timing to the `Fulldraw_Idle` animation event. This document covers the **next** milestone for barrel/muzzle visual convergence once visible.

## Smoke video observations (v0.7.10e inputs)

- Reticle timing needed to appear roughly half a second earlier than full-hold-only readiness.
- Reticle snapped when camera pitch crossed the horizon.
- v0.7.10e addresses timing via `CCS_RevolverReticlePresentationProfile` reveal window and addresses snap via screen smoothing/clamp — **not** full barrel line-of-sight.
- Future barrel/muzzle convergence must use a dedicated convergence profile and must not blindly replace the v0.7.10e timing profile.

## Current baseline (v0.7.10f)

- Reticle hidden by default and during draw.
- Reticle appears when `Fulldraw_Idle` fires `CCS_OnRevolverAimHoldStarted` (Animation Event).
- Reticle uses `CCS_MuzzleDrivenReticleController` with profile-driven smoothing/clamp on camera/current-mode target.
- Hybrid muzzle drift offset may still apply, but muzzle/barrel is not authoritative yet.
- Gameplay aim/fire still uses existing camera/player aim rules.
- Firing damage, ammo, ownership, and hitscan authority are unchanged.

## Design principles

1. **Gameplay first:** camera/player aim rules remain authoritative unless a later milestone explicitly changes combat authority.
2. **Presentation convergence:** the visible reticle should use the muzzle/barrel endpoint as a visual convergence guide.
3. **Avoid shoulder shooting feel:** reticle should not appear anchored to torso/camera center when the barrel clearly points elsewhere.
4. **Stability:** avoid wild reticle jumps when the muzzle crosses the body or near geometry.
5. **Local owner only:** player reticle is local presentation; AI must not use the player reticle.
6. **Tunable:** future behavior should be profile-driven, not hardcoded in runtime scripts.

## Candidate approaches

| Approach | Summary | Pros | Risks |
|----------|---------|------|-------|
| Camera-center gameplay ray + muzzle visual convergence | Keep hitscan/aim from camera center; move reticle toward muzzle projection with clamp | Minimal gameplay risk | Reticle may still diverge from barrel at extremes |
| Muzzle-origin raycast reticle | Reticle follows muzzle ray hit point | Strong barrel alignment | Near-wall jumps; obstruction complexity |
| Hybrid camera aim target + muzzle alignment | Camera chooses target; reticle lerps between center and muzzle projection | Balanced feel | Requires careful tuning and fallback rules |

**Recommended direction:** hybrid camera aim target + muzzle alignment, with clamped drift and obstruction fallback.

## Obstruction handling requirements

Future implementation must consider:

- Camera ray target vs muzzle ray clear line
- Near-wall correction when muzzle is blocked but camera sees open space
- Clamp max reticle drift in pixels
- Fallback to center reticle when muzzle data is invalid or unstable
- Layer mask / ignore-self rules for player geometry

## Proposed future architecture

| Component | Role |
|-----------|------|
| `CCS_RevolverMuzzleAimSource` | Resolves muzzle origin, forward, and optional obstruction hit for presentation |
| `CCS_ReticleConvergenceProfile` | ScriptableObject tuning: max drift pixels, convergence speed, ray length, layers, fallback mode |
| `CCS_MuzzleLineOfSightReticlePresenter` | Applies profile to reticle screen position after readiness gate passes |
| Diagnostics debug rays | Optional editor/runtime debug lines behind existing diagnostics toggles |

## Profile fields (draft)

- `maxReticleDriftPixels`
- `convergenceSpeed`
- `muzzleRayLength`
- `obstructionLayers`
- `fallbackToCenterReticle`
- `nearWallCorrectionDistance`
- `maxReticleJumpPixelsPerFrame`

## Explicit non-goals for the future milestone until approved

- Changing fire damage or hitscan authority
- Adding fire/reload/interaction/dual-revolver animation states
- Rewriting Kevin/EnemyAI visuals
- Attaching weapons to IK targets
- Hardcoding fit offsets outside equipment fit profiles

## Dependencies

- v0.7.10c right-hand fit profile remains source of truth for equipped revolver offset.
- v0.7.10d readiness gate and v0.7.10e timing/stability profiles must remain in place before line-of-sight convergence is layered on top.
- Equipment Fit Studio may later capture convergence preview values into `CCS_ReticleConvergenceProfile` (separate from `CCS_RevolverReticlePresentationProfile`).

## Validation expectations (future)

- Reticle still hidden during draw/holster.
- Hand socket preview still never shows reticle.
- Local non-owner clients never see owner reticle.
- Near-wall cases stay within configured drift/jump limits.
- Missing muzzle or invalid camera falls back safely without breaking fire.

## Next step

Wait for user confirmation after v0.7.10f manual smoke before implementing barrel line-of-sight reticle behavior. v0.7.11 documents the broader mouse-driven aim architecture in `CCS_MouseDriven_RevolverAim_BodyArm_Architecture.md`. Do not replace `CCS_RevolverReticlePresentationProfile` timing values with convergence tuning in the same pass.

## Reticle must not drive the arm (v0.7.11)

The reticle must **not** directly drive arm IK. Target chain:

1. **Aim resolver** produces world target from camera/mouse intent.
2. **Body presenter** and **arm IK presenter** solve toward that world target.
3. **Muzzle LOS resolver** validates barrel convergence.
4. **Reticle presenter** displays the final resolved/converged screen position.

Releasing RMB must hide the reticle immediately when holster starts; arm IK and body bias blend out on the same aim-intent edge.

## Future reticle modes

| Mode | Description |
|------|-------------|
| **Camera Intent Reticle** | Camera center / camera ray screen result |
| **Muzzle Convergence Reticle** | Screen-space point from muzzle-to-target convergence |
| **Hybrid Reticle** | Center intent dot plus muzzle convergence marker |
| **Future Dual Reticle** | Left/right muzzle-specific indicators |

Profile owner: planned `CCS_RevolverReticleConvergenceProfile` (separate from `CCS_RevolverReticlePresentationProfile` reveal/stability fields).

### Dual revolver reticle policy (v0.7.11)

- Future dual revolvers use shoulder-width left/right muzzle lines.
- Dual reticles must **not** be forced into one center point unless profile selects `SingleCameraIntent` or hybrid mode.
- Each muzzle may have its own convergence indicator in `DualMuzzleReticles` mode.
- Gameplay firing remains unchanged until explicitly approved.
