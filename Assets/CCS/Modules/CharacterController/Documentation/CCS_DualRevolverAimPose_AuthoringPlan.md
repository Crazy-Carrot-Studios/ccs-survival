# CCS Dual Revolver Aim Pose Authoring Plan

**Author:** James Schilz  
**Created:** 2026-07-03  
**Target milestone:** v0.7.14 — Dual Revolver Aim Pose Authoring

## Why v0.7.13 removed runtime procedural convergence

v0.7.13 ships as a clean stripped animation baseline. Runtime Animation Rigging experiments did not produce a production-worthy dual-gun aim pose.

| Approach | Result |
|---|---|
| **MultiAimConstraint** | Upper arms raised or rolled instead of cleanly angling inward. Bone local aim axes did not match the desired inward pose, and no stable Aim Axis / Up Axis combination was found. |
| **MultiRotationConstraint** | Copied pose-handle rotations only. It did not solve a correct dual-gun aim pose toward one center target. |
| **TwoBoneIK** | Hand targets twisted wrists and made revolvers point upward because the solver moves the limb tip. |

Exact dual-muzzle convergence toward one center reticle likely needs an **authored animation pose/clip**, with optional tiny final polish later.

## v0.7.13 active baseline

v0.7.13 keeps:

- Locomotion on Base Layer
- Right-hand revolver draw / hold / holster on `SingleRevolverUpperBody`
- Experimental mirrored left-hand preview on `SingleRevolverLeftUpperBody` if still acceptable
- Aim camera screenshot values
- Passive shared visual reticle and shared aim point
- Dual revolver visual preview
- No shooting, damage, ammo, or pickup
- No active procedural arm convergence on the production prefab

Passive aim reference hierarchy:

```
Model
└── Aiming
    └── CCS_DualRevolverVisualAimReference
        └── CCS_SharedDualAimPoint
```

The reticle is visual reference only. It does not drive arms, weapons, or shooting.

## Future v0.7.14 — Dual Revolver Aim Pose Authoring

### Goal

Create an authored dual-revolver aim hold pose/clip where both arms naturally angle toward one center aim target.

### Method

1. Start from the current right-hand aim hold animation.
2. Duplicate or create a CCS-owned dual-revolver aim hold clip.
3. Use Animation Rigging or manual animation editing only as an **authoring aid**, not as runtime gameplay solving.
4. Pose both clavicles, upper arms, forearms, and hands naturally.
5. Keep wrists natural.
6. Keep revolvers attached to hand sockets.
7. Align both muzzle directions visually toward the center reticle.
8. Save the pose as an authored animation clip.
9. Drive it through the Animator layer at runtime.
10. Do not use runtime arm IK/convergence as the primary solution.

### Future runtime policy

- Authored aim pose first
- Camera aim and passive reticle visual second
- No wrist IK unless explicitly approved later
- Muzzle convergence / shooting ray later and separate from animation pose

### Suggested authoring workflow

1. Open validation scene with Equip Weapon true.
2. Enter aim pose using current right-hand hold as the starting reference.
3. Author a dual-revolver hold clip that brings both arms inward toward the passive center reticle.
4. Validate wrists, socket attachment, and revolver visuals in Play Mode without any procedural rig layers.
5. Wire the authored clip into the Animator aim layer for v0.7.14.
6. Keep `CCS_SharedDualAimPoint` and `CCS_SharedDualAimReticle` passive.

### Out of scope for v0.7.14 unless explicitly approved

- Runtime MultiAim / MultiRotation / TwoBoneIK convergence on the production player prefab
- Hand or wrist IK for muzzle alignment
- Shooting, damage, ammo, pickup, or aim target resolver restoration

## References

- Baseline report: `Logs/CharacterController/StripBaseline/CCS_StrippedCharacterControllerBaseline_v0.7.13.md`
- Equipment Fit Studio: `Assets/CCS/Modules/CharacterController/Documentation/CCS_Equipment_Fit_Studio.md`
- Right-hand equipped fit profile: `Assets/CCS/Modules/CharacterController/Profiles/EquipmentFitting/RevolverM1879/CCS_RevolverM1879_RightHandEquipped_Fit.asset`
