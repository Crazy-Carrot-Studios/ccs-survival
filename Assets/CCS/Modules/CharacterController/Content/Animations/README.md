# CCS Player Animation Clips

CCS-owned runtime animation clips for the canonical test player Animator Controller.

## Policy

- Third-party animation packs (Starter Assets, Movement Animset Pro, Invector, etc.) are **source libraries only**.
- Production Animator Controllers must reference **CCS-owned `.anim` copies** under this folder.
- Do **not** edit vendor clips directly.
- Do **not** reference vendor FBX sub-assets directly from production Animator Controllers.

## Folder layout

| Folder | Purpose |
|--------|---------|
| `Locomotion/` | Idle, walk, run/sprint, jump, in-air clips |
| `Interaction/` | Pickup, door, and other interact animation clips |
| `Combat/Revolver/` | Reserved for future revolver/combat clips |

## Adding new clips

1. Identify the vendor source clip (FBX sub-asset or standalone `.anim`).
2. Duplicate/extract into the correct folder using naming:
   - `CCS_Locomotion_<OriginalClipName>.anim`
   - `CCS_Interaction_<OriginalClipName>.anim`
   - `CCS_Combat_Revolver_<OriginalClipName>.anim` (future combat)
3. Rewire `AC_CCS_Player_Locomotion_StarterAssets.controller` to the CCS copy.
4. Run **CCS → Character Controller → Animations → Validate Player Animation Isolation**.

## Tooling

| Menu | Action |
|------|--------|
| **CCS → Character Controller → Animations → Isolate Player Animation Clips** | Duplicate vendor clips and rewire player AC |
| **CCS → Character Controller → Animations → Validate Player Animation Isolation** | Fail if any AC motion is outside this folder |

Batch entry: `CCS.Modules.CharacterController.Editor.CCS_CharacterControllerAnimationIsolationBatchEntry.RunFromBatchMode`
