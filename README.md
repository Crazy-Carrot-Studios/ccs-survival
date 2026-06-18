# CCS Survival

**Version:** 0.2.1 — Character Controller Test Ground

A modular Unity survival project built on the Crazy Carrot Studios framework.

Unity 6 · URP · Input System · Controlled Rebuild

## Overview

CCS Survival is the survival gameplay project for Crazy Carrot Studios. It is built on the CCS Framework and uses isolated gameplay modules under `Assets/CCS/Modules/` that communicate through framework services and events.

The active project scope is intentionally small: framework, project bootstrap, and the Character Controller module with its test ground scene.

## Current State

- Framework baseline integrated
- Project bootstrap/composition in place
- Character Controller module (`0.2.1`) with test player prefab, test ground prefab/scene, and validation
- No other gameplay modules yet — create them when needed
- No production gameplay loop yet

## Repository Layout

```text
Assets/CCS/
├── Framework/              # Core platform
├── Modules/
│   └── CharacterController/   # Only active gameplay module
├── Project/                # Bootstrap, composition, scenes, docs
└── FOLDER_STRUCTURE.md     # Folder reference
```

Cross-module shared assets and project-wide test harnesses are **not** kept as empty placeholders. Add `Shared/` or `Tests/` only when something actively uses them.

## Architecture Rules

- Framework contains reusable platform code only.
- Gameplay systems live in Modules.
- Project owns bootstrap, composition, scenes, and project documentation.
- Do not keep placeholder module folders for features not being built yet.
- Each new module must include runtime, test asset, validation, and docs before moving on.

## Requirements

- Unity 6
- Universal Render Pipeline
- Unity Input System
- Cinemachine 3.1

Open `Assets/CCS/Project/Scenes/SCN_CCS_Survival_Bootstrap.unity` and confirm a clean console before development.

Test ground scene: `Assets/CCS/Modules/CharacterController/Tests/Scenes/SCN_CCS_CharacterController_Test.unity`

## Documentation

**Project (bootstrap / framework gate):** [`Assets/CCS/Project/Documentation/`](Assets/CCS/Project/Documentation/README.md)

**Repo-level planning:** [`Documentation/`](Documentation/README.md)

- [Folder Structure](Assets/CCS/FOLDER_STRUCTURE.md)
- [Versioning Policy](Assets/CCS/Project/Documentation/CCS_Versioning_Policy.md)
- [Framework Architecture Gate](Assets/CCS/Project/Documentation/Survival_Framework_Architecture_Gate.md)
- [Module Guidelines](Documentation/Planning/Future_Gameplay_Module_Guidelines.md)
- [Character Controller Module](Assets/CCS/Modules/CharacterController/Documentation/CCS_CharacterController_Module.md)

## Versioning

| | |
|---|---|
| `0.x.x` | Internal rebuild / beta / prototype |
| `1.0.0` | First alpha-ready release |

**Current working milestone:** `0.2.1` (not tagged yet)

## Ownership

Copyright © Crazy Carrot Studios. All rights reserved unless a license file states otherwise.
