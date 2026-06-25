# CCS Survival

**Version 0.6.16** · Crazy Carrot Studios

Modular Unity 6 survival framework project — URP, Input System, Netcode for GameObjects, Cinemachine 3.

## Active modules

| Module | Milestone |
|--------|-----------|
| **Framework** | Core platform (gameplay-free) |
| **Project** | Bootstrap, composition, validation standards |
| **CharacterController** | Movement, camera, Master Test harness (v0.6.16 simplified third-person revolver aim) |
| **Attributes** | Health model, replication, test HUD |
| **Interaction** | Pickup and walk-through-door flow (v0.5.4) |
| **Weapons** | Revolver M1879 world pickup, hitscan, fit profile pack (v0.6.16 reticle-aligned shots) |

## Current milestone

**0.6.16** — **Simplified third-person revolver aim cleanup:** third-person **Aim Over Shoulder** only (`CinemachineCamera_Aim`). Simplified `RevolverUpperBody` layer with upper-body/right-arm mask excluding left arm. Aim flow: `NoAim → IdleToAim → FullDraw → Return`. Red reticle (`#D32222`) visible only during **FullDraw**. Player shots resolve from **camera-center reticle aim** through `CCS_WeaponShotResolver`; projectile/tracer travels muzzle → resolved aim point. **Animation Fit Studio** edits controller-used `CCS_WW_Revolver_AimIdle_FullDraw.anim` via Humanoid muscle curves. Legacy first-person aim routing, AimPitch blend, FitTest active workflow, arm-to-reticle IK, and visual aim convergence active paths removed.

**0.6.15** — Local self headless first-person body fallback; Equipment Fit Studio weapon-space rotation; Master Test recording ambience.

Profiles live at `Assets/CCS/Modules/CharacterController/Profiles/EquipmentFitting/RevolverM1879/`.

## Validation and playtest

| Action | Entry point |
|--------|-------------|
| Project audit (docs, asmdefs, legacy leftovers) | **CCS → Project → Run Project Audit** |
| Master Test (primary) | `Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_MasterTest.unity` |
| Interaction module | **CCS → Interaction → Validate Interaction Module** |
| Weapons module | **CCS → Weapons → Validate Weapons Module** |
| Character Controller Master Test | **CCS → Character Controller → Scene → Setup And Validate Master Test Scene** |
| Equipment Fit Studio | **CCS → Character Controller → Equipment → Equipment Fit Studio** |
| Animation Fit Studio | **CCS → Character Controller → Animations → Animation Fit Studio** |
| Attributes module | **CCS → Attributes → Validate Attributes Module** |
| Bootstrap smoke | `Assets/CCS/Scenes/Bootstrap/SCN_CCS_Survival_Bootstrap.unity` |

Legacy ground-only preview (retained, not primary): `Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_Test.unity`

## Requirements

Unity 6 · URP · Input System · Netcode for GameObjects · Cinemachine 3.1
