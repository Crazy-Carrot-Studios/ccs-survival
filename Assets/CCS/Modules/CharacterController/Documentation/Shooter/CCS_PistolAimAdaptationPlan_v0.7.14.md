# CCS Pistol Aim Adaptation Plan v0.7.14

## External Source (internal traceability)

The separate **CCS_Assets** Unity project contains an external third-person shooter reference package used for inspection only. That package is not imported into `ccs-survival` and must not become a production dependency.

## What we inspected

- Reference over-shoulder aim camera framing (distance, shoulder offset, height, FOV).
- Reference two-handed pistol aim pose/hold animation structure.
- Reference upper-body animator layering and aim locomotion patterns.

## CCS policy

- Do **not** copy external vendor scripts into `ccs-survival`.
- Do **not** wire vendor gameplay controllers, camera controllers, weapon managers, or UI into Kevin.
- Recreate only the behavior CCS needs in **CCS-owned code and assets**.
- Duplicate animation clips only after James confirms license/permission.
- Keep v0.7.13 authored-animation baseline: no runtime procedural arm convergence, no shooting/damage/ammo/pickup in this milestone.

## CCS-owned implementation targets

| Area | Future CCS component / asset |
|------|------------------------------|
| Aim layer driving | `CCS_PistolAimLayerAnimator` (or evolve `CCS_RevolverAimLayerAnimator`) |
| Presentation profile | `CCS_PistolAimPresentationProfile` |
| Camera tuning | `CCS_PistolAimCameraProfile` / existing `CCS_CharacterCameraProfile_AimOverShoulder` |
| Visual equip/aim | `CCS_PistolAimVisualPresenter` |
| Passive aim point | `CCS_SharedAimPointPresenter` (existing shared dual aim point baseline) |
| Passive reticle | `CCS_SharedAimReticlePresenter` (existing shared reticle baseline) |

## Animation target

- Primary test clip (after license approval):
  `Assets/CCS/Modules/CharacterController/Content/Animations/Pistol/TwoHanded/CCS_Pistol_TwoHand_AimHold.anim`
- Source: reference two-handed pistol aim hold pose from external shooter package (inspected in CCS_Assets only).
- CCS uses **authored animation only** — no runtime IK convergence for left-hand support in production.

## Animator test scaffold

- Test controller: `Assets/CCS/Modules/CharacterController/Content/Animations/Pistol/AC_CCS_PistolAim_Test.controller`
- Layers: `Base Locomotion`, `PistolUpperBodyTest`
- Params: `IsAiming`, `PistolDrawTrigger`, `PistolHolsterTrigger`

## Keep from v0.7.13 baseline

- Diagnostics Manager: Enable Enemy, Enable Aim Pose, Equip Weapon only.
- Locked aim camera screenshot values as starting point before reference-inspired tuning.
- Passive shared reticle / shared aim point.
- CCS locomotion and CCS camera architecture.

## Deferred

- Fire, reload, ammo, damage, pickup, weapon inventory, vendor IK solvers.

## Next steps (awaiting James)

1. Confirm license OK to duplicate reference two-handed pistol aim hold clip.
2. Extract to `CCS_Pistol_TwoHand_AimHold.anim`.
3. Retarget on Kevin; test with `AC_CCS_PistolAim_Test.controller`.
4. Compare against Wild West one-handed aim layer.
5. Tune CCS aim camera profile using reference-inspired test values (see camera reference doc).
6. Do **not** wire production Kevin until pose + camera smoke passes.
