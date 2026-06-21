# CCS Survival

**Version 0.6.0** · Crazy Carrot Studios

Modular Unity 6 survival framework project — URP, Input System, Netcode for GameObjects, Cinemachine 3.

## Active modules

| Module | Milestone |
|--------|-----------|
| **Framework** | Core platform (gameplay-free) |
| **Project** | Bootstrap, composition, validation standards |
| **CharacterController** | Movement, camera, Master Test harness |
| **Attributes** | Health model, replication, test HUD |
| **Interaction** | Pickup and walk-through-door flow (v0.5.4) |
| **Weapons** | Test revolver hitscan foundation (v0.6.0) |

## Current milestone

**0.6.0** — Revolver shooting foundation: aim/fire/reload input, camera-center hitscan, test damage target, and Master Test wiring.

## Validation and playtest

| Action | Entry point |
|--------|-------------|
| Project audit (docs, asmdefs, legacy leftovers) | **CCS → Project → Run Project Audit** |
| Master Test (primary) | `Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_MasterTest.unity` |
| Interaction module | **CCS → Interaction → Validate Interaction Module** |
| Weapons module | **CCS → Weapons → Validate Weapons Module** |
| Character Controller Master Test | **CCS → Character Controller → Scene → Setup And Validate Master Test Scene** |
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

Tagged release: **v0.6.0**

---

Copyright © Crazy Carrot Studios. All rights reserved.
