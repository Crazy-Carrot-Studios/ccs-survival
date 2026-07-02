# CCS Mouse-Driven Revolver Aim — Validation Plan

**Version:** 0.7.12 — v0.7.12 aim target resolver validation enforced  
**Author:** James Schilz  
**Created:** 2026-06-25  
**Status:** `CCS_RevolverAimTargetResolverValidationUtility` enforced in v0.7.12; body/arm/muzzle/convergence validations remain future

## Purpose

Define batch and editor validation rules for mouse-driven revolver aim presentation once runtime components are implemented (v0.7.12+).

## Future validation must fail if

| Rule | Rationale |
|------|-----------|
| Aim target resolver missing when implementation milestone begins | No unified world target |
| Body aim presenter updates non-owner players incorrectly | Local presentation only |
| Weapon attached to IK target instead of `CCS_HandSocket_Right` | Hierarchy contract |
| `CCS_RightHandIKTarget` missing on player prefab | Arm IK cannot solve |
| Right elbow hint (`CCS_RightElbowHint`) missing | Unstable elbow pole |
| `MuzzlePoint` missing on equipped revolver visual | No muzzle convergence |
| Reticle has no local-owner gate | Remote players must not see owner reticle |
| Reticle visible when not aiming | Presentation leak |
| Reticle remains visible after holster starts / RMB release | v0.7.10f contract broken |
| Muzzle line invalid and no fallback exists | Snap/flicker risk |
| Full-body yaw exceeds threshold without body turn logic | Over-twist / shoulder-origin feel |
| Arm IK fights draw/holster animation | Animation ownership violated |
| Gameplay damage ray changes without explicit milestone approval | Combat authority guard |
| Dual reticle mode enabled for single revolver without profile | Mode guard |
| New Animator layers/states or clip edits without approval | Animation guard |
| `CCS_RevolverReticleConvergenceProfile` duplicates v0.7.10e/f reveal timing fields | Profile separation |

## Future validation should warn if

- Body/IK/convergence values hardcoded in runtime instead of ScriptableObject profiles
- Aim resolver and reticle use different layer masks without documented reason
- Equipment Fit Studio or Animation Fit Studio policy violated

## Baseline validations (unchanged in v0.7.11)

Continue running until implementation milestones add new utilities:

- Player prefab audit
- Single revolver aim layer validation
- Reticle reveal animation event validation
- Reticle timing/stability validation
- Revolver right-hand fit profile validation
- Weapons validation
- Master test + project audit
- AI bandit validation
- Hosting scene validation
- Missing script scan

## Planned validation utilities

| Utility | Milestone | Status |
|---------|-----------|--------|
| `CCS_RevolverAimTargetResolverValidationUtility` | v0.7.12 | **Implemented** |
| `CCS_RevolverBodyAimPresenterValidationUtility` | v0.7.13 | Planned |
| `CCS_RevolverArmAimIKValidationUtility` | v0.7.14 | Planned |
| `CCS_RevolverMuzzleLineOfSightValidationUtility` | v0.7.15 | Planned |
| `CCS_RevolverReticleConvergenceValidationUtility` | v0.7.15 | Planned |

Batch entry: `CCS.Modules.CharacterController.Editor.CCS_RevolverAimTargetResolverBatchEntry.RunFromBatchMode`

## Related documentation

- `CCS_MouseDriven_RevolverAim_BodyArm_Architecture.md`
- `CCS_Revolver_Reticle_Barrel_LineOfSight_Plan.md`
- `CCS_PlayerPrefab_Hierarchy_Architecture.md`
