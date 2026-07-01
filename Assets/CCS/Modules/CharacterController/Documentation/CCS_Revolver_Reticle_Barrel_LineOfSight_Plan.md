# CCS Revolver Reticle — Barrel Line-of-Sight Plan

**Version:** planning document — barrel LOS still deferred after v0.7.10e  
**Author:** James Schilz  
**Created:** 2026-06-30  
**Status:** Plan only — not implemented

## Purpose

Define the future presentation goal: the reticle should visually align with the end of the revolver barrel / muzzle direction instead of feeling like it originates from the shoulder or generic camera center.

v0.7.10d gated reticle visibility on aim presentation readiness. v0.7.10e added late-draw reveal timing and camera/current-mode pitch stabilization. This document covers the **next** milestone for barrel/muzzle visual convergence once visible.

## Smoke video observations (v0.7.10e inputs)

- Reticle timing needed to appear roughly half a second earlier than full-hold-only readiness.
- Reticle snapped when camera pitch crossed the horizon.
- v0.7.10e addresses timing via `CCS_RevolverReticlePresentationProfile` reveal window and addresses snap via screen smoothing/clamp — **not** full barrel line-of-sight.
- Future barrel/muzzle convergence must use a dedicated convergence profile and must not blindly replace the v0.7.10e timing profile.

## Current baseline (v0.7.10e)

- Reticle hidden by default and during early draw.
- Reticle appears during late draw (`IsAimPresentationInReticleRevealWindow`) and remains visible in hold.
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

Wait for user confirmation after v0.7.10e manual smoke before implementing barrel line-of-sight reticle behavior. Do not replace `CCS_RevolverReticlePresentationProfile` timing values with convergence tuning in the same pass.
