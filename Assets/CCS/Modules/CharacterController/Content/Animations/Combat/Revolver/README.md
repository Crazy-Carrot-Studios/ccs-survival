# Combat / Revolver Upper-Body Animations

CCS-owned revolver upper-body clips isolated from Invector shooter source FBXs (v0.6.4).

## Policy

- Revolver upper-body clips are isolated from Invector source FBXs into CCS-owned `.anim` files in this folder.
- Runtime Animator Controllers must reference only CCS-owned `.anim` clips and `CCS_Revolver_UpperBody.mask`.
- Vendor assets under `Assets/VendorSource/Invector/` are source-only.
- Invector scripts, prefabs, controllers, and UI are not part of CCS runtime.

## Required clips

| Asset | Source | Loop |
|-------|--------|------|
| `CCS_Revolver_AimIdle_UpperBody.anim` | Invector `Aiming@Pistol` | Yes |
| `CCS_Revolver_IdlePistol_UpperBody.anim` | Invector `Idle@Pistol` | Yes |
| `CCS_Revolver_Fire_UpperBody.anim` | Invector `Shot_Pistol` | No |
| `CCS_Revolver_Reload_UpperBody.anim` | Invector `Reload_Pistol` | No |

## Mask

- `CCS_Revolver_UpperBody.mask` — upper body only (spine, chest, arms, head).
- Excludes hips translation, legs, feet, and lower-body locomotion.

## Animator layer

- Layer name: `RevolverUpperBody` (Override, script-controlled weight).
- States: `Revolver_Empty`, `Revolver_AimIdle`, `Revolver_Fire`, `Revolver_Reload`.
- Driven at runtime by `CCS_RevolverUpperBodyAnimator` on the player `VisualRoot`.

## Source FBXs (vendor-only)

```
Assets/VendorSource/Invector/Shooter/Animations/
  Shooter_UpperBodyPoses.fbx
  Shooter_Shot&Reload.fbx
```

Do not reference these FBX sub-assets directly from production Animator Controllers.
