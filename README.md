# CCS Survival

**Version 0.6.15** · Crazy Carrot Studios

Modular Unity 6 survival framework project — URP, Input System, Netcode for GameObjects, Cinemachine 3.

## Active modules

| Module | Milestone |
|--------|-----------|
| **Framework** | Core platform (gameplay-free) |
| **Project** | Bootstrap, composition, validation standards |
| **CharacterController** | Movement, camera, Master Test harness (v0.6.15 local self headless first-person body fallback) |
| **Attributes** | Health model, replication, test HUD |
| **Interaction** | Pickup and walk-through-door flow (v0.5.4) |
| **Weapons** | Revolver M1879 world pickup, hitscan, fit profile pack (v0.6.8) |

## Current milestone

**0.6.15** — **Local self headless first-person body fallback:** combined CC3 `CC_Game_Body` uses a CCS-owned headless mesh substitute on `CCS_LocalFirstPersonBody` for local BodyAware first-person aim only; full body/head remains for third-person and remote viewers. Separate head renderers still layer-mask via `CCS_LocalSelfHeadHidden`. `CinemachineCamera_FP_BodyAware` is the only first-person aim camera.

**0.6.11** — **Hard replace revolver two-handed aim with Wild West one-handed:** `RevolverUpperBody` layer only; preview layer removed; instant layer weight + forced CrossFade into `Revolver_AimIdle` on RMB; F9 Master Test force-play debug; legacy two-handed clips archive-only.

**0.6.10** — **Wild West one-handed revolver aim default:** one-handed Wild West aim/fire clips drive the existing `RevolverUpperBody` animator layer.

**0.6.9** — **Fixed first-person aim camera + Wild West revolver preview foundation:** exploration uses `ThirdPersonSurvival`. RMB firearm aim switches to `FirstPersonAim` via carry-state camera source. **FirstPersonAim** uses fixed `FirstPersonAimCameraAnchor` at `(0, 0.28, 0.36)` — not head-tracked. FOV **66**. Wild West right-hand revolver clips isolated under `Content/Animations/Revolver/WildWest/`.

**0.6.8** — Equipment Fit Studio, reticle-aligned aim, cosmetic revolver fire visuals (readable bullet tracer trail, reload-only shell extraction).

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
| Player animation isolation | **CCS → Character Controller → Animations → Validate Player Animation Isolation** |
| Attributes module | **CCS → Attributes → Validate Attributes Module** |
| Bootstrap smoke | `Assets/CCS/Scenes/Bootstrap/SCN_CCS_Survival_Bootstrap.unity` |

Legacy ground-only preview (retained, not primary): `Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_Test.unity`

## Requirements

Unity 6 · URP · Input System · Netcode for GameObjects · Cinemachine 3.1

## Layout

```text
Assets/CCS/
├── Framework/     Reusable core platform
├── Modules/       Gameplay modules (CharacterController, Attributes, Interaction, Weapons)
├── Scenes/        Bootstrap, Master Test, multiplayer hosting
└── Project/       Composition shell and project documentation
```

## Documentation

- [Folder structure](Assets/CCS/FOLDER_STRUCTURE.md)
- [Versioning policy](Assets/CCS/Project/Documentation/CCS_Versioning_Policy.md)
- [Project documentation index](Assets/CCS/Project/Documentation/README.md)
- [Module index](Assets/CCS/Modules/README.md)

## Versioning

`0.x.x` — internal rebuild milestones · `1.0.0` — first alpha-ready release (future)

Tagged release: **v0.6.9**

---

Copyright © Crazy Carrot Studios. All rights reserved.
