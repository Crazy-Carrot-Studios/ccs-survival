# CCS Player Animation Clips

CCS-owned runtime animation clips for the canonical test player Animator Controller.

## Policy

- Third-party animation packs (Starter Assets, Movement Animset Pro, external shooter reference, etc.) are **source libraries only**.
- Production Animator Controllers must reference **CCS-owned `.anim` copies** under this folder.
- Do **not** edit vendor clips directly.
- Do **not** reference vendor FBX sub-assets directly from production Animator Controllers.
- External reference assets live in the separate **CCS_Assets** project and must not appear in `ccs-survival` runtime prefabs, scenes, or Animator Controllers.
- External vendor scripts, prefabs, controllers, UI, inventory, and weapon systems are **not** part of CCS runtime.

## Folder layout

| Folder | Purpose |
|--------|---------|
| `Locomotion/` | Idle, walk, run/sprint, jump, in-air clips |
| `Interaction/` | Pickup, door, and other interact animation clips |
| `Combat/AimStrafe/` | MAP-isolated aim strafe/backpedal locomotion clips |
| `Combat/Revolver/` | Archive legacy two-handed revolver upper-body clips |
| `Pistol/` | v0.7.14+ CCS-owned pistol aim clips and test controllers |
| `Pistol/TwoHanded/` | Two-handed pistol aim hold and related clips |

## Aim Strafe Animation Policy

- MAP strafe clips are isolated as CCS-owned `.anim` assets under `Combat/AimStrafe/`.
- Animator Controller must reference CCS-owned assets only.
- Vendor FBX sub-assets are source-only and must not be referenced by runtime controllers.

## Revolver Upper-Body Animation Policy

- Legacy revolver upper-body clips under `Combat/Revolver/` are archive-only.
- Active Wild West one-handed aim uses `Revolver/WildWest/`.
- New two-handed pistol aim reference work uses `Pistol/TwoHanded/`.

## Adding new clips

1. Identify the vendor source clip (FBX sub-asset or standalone `.anim`).
2. Duplicate/extract into the correct folder using naming:
   - `CCS_Locomotion_<OriginalClipName>.anim`
   - `CCS_Pistol_<Purpose>.anim`
3. Confirm license allows duplication into CCS-owned paths.
4. Wire only CCS-owned `.anim` files into production Animator Controllers.
