# CCS Survival

**Version 0.5.5** · Crazy Carrot Studios

Modular Unity 6 survival framework project — URP, Input System, Netcode for GameObjects, Cinemachine 3.

## Active modules

| Module | Milestone |
|--------|-----------|
| **Framework** | Core platform (gameplay-free) |
| **Project** | Bootstrap, composition, validation standards |
| **CharacterController** | Movement, camera, Master Test harness |
| **Attributes** | Health model, replication, test HUD |
| **Interaction** | Pickup and walk-through-door flow (v0.5.4) |

## Current milestone

**0.5.5** — Project audit and interaction cleanup after the v0.5.4 pickup/door release.

Interaction supports forward-volume detection, closest-point line of sight, Press [E] prompts when ready, movement lock during animations, and `PickUp_RH` / `WalkThroughDoor_RH` routing. Details: [Interaction module doc](Assets/CCS/Modules/Interaction/Documentation/CCS_Interaction_Module.md).

No production survival loop yet.

## Validation and playtest

| Action | Entry point |
|--------|-------------|
| Project audit (docs, asmdefs, legacy leftovers) | **CCS → Project → Run Project Audit** |
| Master Test (primary) | `Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_MasterTest.unity` |
| Interaction module | **CCS → Interaction → Validate Interaction Module** |
| Character Controller Master Test | **CCS → Character Controller → Scene → Setup And Validate Master Test Scene** |
| Attributes module | **CCS → Attributes → Validate Attributes Module** |
| Bootstrap smoke | `Assets/CCS/Scenes/Bootstrap/SCN_CCS_Survival_Bootstrap.unity` |

Legacy ground-only preview (retained, not primary): `Assets/CCS/Modules/CharacterController/Tests/Scenes/SCN_CCS_CharacterController_Test.unity`

## Requirements

Unity 6 · URP · Input System · Netcode for GameObjects · Cinemachine 3.1

## Layout

```text
Assets/CCS/
├── Framework/     Reusable core platform
├── Modules/       Gameplay modules (CharacterController, Attributes, Interaction)
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

Tagged release: **v0.5.5**

---

Copyright © Crazy Carrot Studios. All rights reserved.
