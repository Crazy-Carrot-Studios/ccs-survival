# CCS Survival

A modular survival game project built on the Crazy Carrot Studios framework.

CCS Survival is the survival gameplay repository for Crazy Carrot Studios. It is a Unity-based project that uses a strict module architecture: gameplay systems are isolated under `Assets/CCS/Modules/` and communicate through framework services and events.

The project is built on the vendored CCS Core Platform (`Assets/CCS/Framework/`). Core provides runtime hosting, module lifecycle, services, and diagnostics. Gameplay logic does not live in Core.

The repository is at a **controlled rebuild baseline**. Previous gameplay work was archived; modules will be rebuilt incrementally with validation and test prefabs at each step.

## Current Status

**Version:** 0.0.3 — Controlled Rebuild Baseline

| | |
|---|---|
| Framework | Integrated |
| Project architecture | Established |
| Gameplay modules | Not yet rebuilt |
| Next step | Controlled module-by-module rebuild |

## Project Structure

```text
Assets/CCS/
├── Framework/     # CCS Core platform
├── Modules/       # Gameplay systems (per-feature modules)
├── Shared/        # Cross-module assets and contracts
├── Project/       # Bootstrap, scenes, composition, project docs
└── Tests/         # Project-wide validation and test assets
```

**Framework** — Reusable CCS Core platform only. No game-specific gameplay code.

**Modules** — Gameplay and game systems. Each module owns its runtime, editor tools, prefabs, settings, tests, and documentation. Examples: Character Controller, Interaction, Inventory, Equipment, Crafting, Save System.

**Shared** — Assets used by multiple modules (art, audio, materials, shared UI, common ScriptableObjects). No module-specific gameplay logic.

**Project** — Game bootstrap, composition, scenes, and project-level documentation.

**Tests** — Cross-cutting edit/play mode harnesses and validation assets.

## Architecture Rules

- No gameplay code in `Framework/`.
- Gameplay systems must live in `Modules/`.
- `Shared/` is only for cross-module assets and contracts.
- `Project/` owns bootstrap and composition only.
- Modules must be testable in isolation.
- Every rebuilt module must include a working test prefab or scene object before the next module starts.
- No singleton managers, scene scanning, or auto-discovery for composition.
- Dependency direction: `Modules → Project → Core`.

## Getting Started

**Requirements**

- Unity 6
- Universal Render Pipeline (configured in project)
- Input System (configured in project)

**Open the project**

1. Clone the repository and open the project folder in Unity 6.
2. Open the bootstrap scene: `Assets/CCS/Project/Scenes/SCN_CCS_Survival_Bootstrap.unity`
3. Confirm the Unity Console has no compile errors before development.

Core platform validation scene (separate from game bootstrap): `Assets/CCS/Framework/Core/Runtime/Scenes/SCN_CCS_Bootstrap.unity`

## Documentation

Additional documentation lives in [`Assets/CCS/Project/Documentation/`](Assets/CCS/Project/Documentation/README.md):

- [Versioning policy](Assets/CCS/Project/Documentation/CCS_Versioning_Policy.md)
- [Architecture gate](Assets/CCS/Project/Documentation/Survival_Framework_Architecture_Gate.md)
- [Runtime foundation](Assets/CCS/Project/Documentation/Survival_Runtime_Foundation.md)
- [Validation standards](Assets/CCS/Project/Documentation/Survival_Validation_Standards.md)
- [Authority and avatar architecture](Assets/CCS/Project/Documentation/Survival_Authority_And_Avatar_Architecture.md)
- [Scene bootstrap standards](Assets/CCS/Project/Documentation/Survival_Scene_Bootstrap_Standards.md)
- [Module guidelines](Assets/CCS/Project/Documentation/Future_Gameplay_Module_Guidelines.md)

Repo-level architecture notes: [`Documentation/Architecture/`](Documentation/Architecture/Survival_Gameplay_Architecture.md)

Framework reference (read-only for gameplay authors):

- [CCS Core Platform Architecture](Assets/CCS/Framework/Core/Documentation/CCS_Core_Platform_Architecture.md)
- [CCS Script Standards](Assets/CCS/Framework/Documentation/CCS_Script_Standards.md)

## Version

**Current version:** 0.0.3

| | |
|---|---|
| `0.x.x` | Internal rebuild / beta / prototype |
| `1.0.0` | First alpha-ready release |

Tags use `v<major>.<minor>.<patch>` (e.g. `v0.0.3`). Full policy: [CCS_Versioning_Policy.md](Assets/CCS/Project/Documentation/CCS_Versioning_Policy.md)

## License

Copyright © Crazy Carrot Studios. All rights reserved unless a separate license file states otherwise.
