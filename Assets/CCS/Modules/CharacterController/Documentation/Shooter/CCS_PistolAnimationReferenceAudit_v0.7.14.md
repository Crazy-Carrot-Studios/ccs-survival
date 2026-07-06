# CCS Pistol Animation Reference Audit v0.7.14

## Key finding

The useful external reference for CCS pistol aim is a **two-handed pistol aim hold pose** — a single-frame/snapshot upper-body pose with both hands on the weapon grip.

External reference also uses:

- Separate aim-strafe locomotion clips (`AimedIdle`, `AimedWalk`, etc.) on a masked upper-body layer.
- Runtime hand IK refinement in the vendor stack — **CCS will not replicate this**; CCS uses authored pose only per v0.7.13 baseline.

## CCS target clip (not copied yet)

| Property | Value |
|----------|-------|
| CCS path | `Assets/CCS/Modules/CharacterController/Content/Animations/Pistol/TwoHanded/CCS_Pistol_TwoHand_AimHold.anim` |
| Type | Humanoid upper-body pose |
| Hands | Two-handed authored support |
| Status | **Pending license confirmation** |

## Supporting reference clips (optional later)

| Use | Notes |
|-----|-------|
| Reference idle pistol pose | Upper-body holstered/ready pose |
| Aim strafe locomotion | Defer — CCS may keep own locomotion + upper-body hold |
| Fire / reload | Deferred gameplay milestone |

## Test controller

`Assets/CCS/Modules/CharacterController/Content/Animations/Pistol/AC_CCS_PistolAim_Test.controller`

| Layer | Purpose |
|-------|---------|
| Base Locomotion | Existing CCS locomotion (unchanged in test) |
| PistolUpperBodyTest | Reference two-handed aim hold when `IsAiming` |

| Parameter | Type |
|-----------|------|
| `IsAiming` | Bool |
| `PistolDrawTrigger` | Trigger |
| `PistolHolsterTrigger` | Trigger |

## Mask

- Prefer CCS-owned upper-body mask tuned for Kevin.
- Existing candidate: `AM_CCS_Revolver_UpperBodyLeftArm_Aim.mask` (experimental baseline).

## Kevin compatibility

- Reference pose was authored on a different humanoid skeleton.
- **Retarget validation required** on Kevin before production use.

## Copy policy

When approved:

1. Duplicate/extract pose from external reference FBX in **CCS_Assets** project only.
2. Save as `CCS_Pistol_TwoHand_AimHold.anim` under CCS-owned path above.
3. Do not edit source FBX.
4. Do not copy vendor scripts, controllers, or prefabs.
