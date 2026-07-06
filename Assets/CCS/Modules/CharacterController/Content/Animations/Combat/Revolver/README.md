# Combat / Revolver Upper-Body Animations (Archive)

CCS-owned legacy two-handed revolver upper-body clips isolated from external shooter source FBXs (v0.6.4).

**v0.6.11:** these clips are **archive-only**. Active runtime RMB aim uses Wild West one-handed clips under `Content/Animations/Revolver/WildWest/`.

## Policy

- Revolver upper-body clips were isolated from external reference FBXs into CCS-owned `.anim` files in this folder.
- Runtime Animator Controllers must reference only CCS-owned `.anim` clips and `CCS_Revolver_UpperBody.mask`.
- External reference assets remain in the separate CCS_Assets project only.
- External vendor scripts, prefabs, controllers, and UI are not part of CCS runtime.

## Required clips

| Asset | Source | Loop |
|-------|--------|------|
| `CCS_Revolver_AimIdle_UpperBody.anim` | Reference two-handed aim hold pose | Yes |
| `CCS_Revolver_IdlePistol_UpperBody.anim` | Reference pistol idle pose | Yes |
| `CCS_Revolver_Fire_UpperBody.anim` | Reference pistol fire pose | No |
| `CCS_Revolver_Reload_UpperBody.anim` | Reference pistol reload | No |

## Mask

- `CCS_Revolver_UpperBody.mask` — upper body only (spine, chest, arms, head).
- Excludes hips translation, legs, feet, and lower-body locomotion.

## Animator layer

- Layer name: `RevolverUpperBody` (Override, script-controlled weight).
- States (v0.6.11): `Revolver_Empty`, `Revolver_IdleToAim`, `Revolver_AimIdle_FullDraw`, `Revolver_AimToIdle`, `Revolver_WalkToAimWalk`, `Revolver_AimWalk`, `Revolver_AimWalkToWalk`, `Revolver_Fire`, `Revolver_Reload`.
- Driven at runtime by `CCS_RevolverUpperBodyAnimator` on the player `VisualRoot`.

## v0.7.14 note

New two-handed pistol aim work uses `Content/Animations/Pistol/TwoHanded/CCS_Pistol_TwoHand_AimHold.anim` after license-approved duplicate from external reference inspected in CCS_Assets.

Do not reference external FBX sub-assets directly from production Animator Controllers.
