# CCS Survival

**Version:** 0.5.4 — Interaction Pickup and Door Flow

A modular Unity survival project built on the Crazy Carrot Studios framework.

Unity 6 · URP · Input System · Controlled Rebuild

## Overview

CCS Survival is the survival gameplay project for Crazy Carrot Studios. It is built on the CCS Framework and uses isolated gameplay modules under `Assets/CCS/Modules/` that communicate through framework services and events.

The active project scope includes framework bootstrap, Character Controller, Attributes, and the Interaction module with Master Test validation.

## Current State

- Framework baseline integrated
- Project bootstrap/composition in place
- Character Controller module with test player prefab, Master Test scene, and validation
- Attributes module with Health HUD and server-authoritative replication
- Interaction module (`0.5.4`) with pickup and walk-through-door flow
- No production gameplay loop yet

## Interaction (v0.5.4)

The Interaction module now supports **Pickup** and **WalkThroughDoor** interactable kinds.

- **Prompt** shows only when the target is interaction-ready (forward volume + line of sight).
- **Forward volume detection** uses a player-local box in front of the scan origin (chest height).
- **Line of sight** casts from `InteractionScanOrigin` to the target collider **closest point**, skipping player colliders.
- **Movement locks** during interaction animations via `CCS_IInteractionLockController` and motor hard-stop.
- **Animation routing** triggers `PickUp_RH` and `WalkThroughDoor_RH` on the player animator.
- **Master Test** includes a pickup cube near spawn and a building door interaction target.

Test scene: `Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_MasterTest.unity`

Editor menu: **CCS → Interaction → Validate Interaction Module**

## Repository Layout

```text
Assets/CCS/
├── Framework/              # Core platform
├── Modules/
│   ├── CharacterController/
│   ├── Attributes/
│   └── Interaction/
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
- [Interaction Module](Assets/CCS/Modules/Interaction/Documentation/CCS_Interaction_Module.md)

## Versioning

| | |
|---|---|
| `0.x.x` | Internal rebuild / beta / prototype |
| `1.0.0` | First alpha-ready release |

**Current working milestone:** `0.5.4` — tagged `v0.5.4`

## Ownership

Copyright © Crazy Carrot Studios. All rights reserved unless a license file states otherwise.
