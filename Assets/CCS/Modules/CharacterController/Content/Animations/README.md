# CCS Player Animation Clips

CCS-owned runtime animation clips for the canonical test player Animator Controller.

## Policy

- Third-party animation packs (Starter Assets, Movement Animset Pro, Invector, etc.) are **source libraries only**.
- Production Animator Controllers must reference **CCS-owned `.anim` copies** under this folder.
- Do **not** edit vendor clips directly.
- Do **not** reference vendor FBX sub-assets directly from production Animator Controllers.
- Vendor assets live under `Assets/VendorSource/` and must not appear in runtime prefabs, scenes, or Animator Controllers.
- Invector scripts, prefabs, controllers, UI, inventory, and weapon systems are **not** part of CCS runtime.

## Folder layout

| Folder | Purpose |
|--------|---------|
| `Locomotion/` | Idle, walk, run/sprint, jump, in-air clips |
| `Interaction/` | Pickup, door, and other interact animation clips |
| `Combat/AimStrafe/` | MAP-isolated aim strafe/backpedal locomotion clips |
| `Combat/Revolver/` | Invector-isolated revolver upper-body aim/fire/reload clips and mask |

## Aim Strafe Animation Policy

- MAP strafe clips are isolated as CCS-owned `.anim` assets under `Combat/AimStrafe/`.
- Animator Controller must reference CCS-owned assets only.
- Vendor FBX sub-assets are source-only and must not be referenced by runtime controllers.

## Revolver Upper-Body Animation Policy

- Revolver upper-body clips are isolated from Invector source FBXs into CCS-owned `.anim` files under `Combat/Revolver/`.
- Runtime Animator Controllers must reference only CCS-owned `.anim` clips and CCS-owned masks.
- `RevolverUpperBody` is an override layer masked to upper body; locomotion remains on the base layer.
- Layer weight and animator parameters are driven by `CCS_RevolverUpperBodyAnimator` (CharacterController), not by Weapons gameplay code.

## Adding new clips

1. Identify the vendor source clip (FBX sub-asset or standalone `.anim`).
2. Duplicate/extract into the correct folder using naming:
   - `CCS_Locomotion_<OriginalClipName>.anim`
   - `CCS_Interaction_<OriginalClipName>.anim`
   - `CCS_Revolver_<Purpose>_UpperBody.anim` (revolver upper-body)
3. Rewire `AC_CCS_Player_Locomotion_StarterAssets.controller` to the CCS copy.
4. Run **CCS → Character Controller → Animations → Validate Player Animation Isolation**.

## Tooling

| Menu | Action |
|------|--------|
| **CCS → Character Controller → Animations → Isolate Player Animation Clips** | Duplicate vendor clips and rewire player AC |
| **CCS → Character Controller → Animations → Validate Player Animation Isolation** | Fail if any AC motion is outside this folder |

Batch entry: `CCS.Modules.CharacterController.Editor.CCS_CharacterControllerAnimationIsolationBatchEntry.RunFromBatchMode`
