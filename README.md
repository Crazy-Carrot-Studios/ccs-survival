# CCS Survival

**Version:** 0.1.0 — Architecture Normalization

A modular Unity survival project built on the Crazy Carrot Studios framework.

Unity 6 · URP · Input System · Controlled Rebuild

## Overview

CCS Survival is the survival gameplay project for Crazy Carrot Studios. It is built on the CCS Framework and uses isolated gameplay modules under `Assets/CCS/Modules/` that communicate through framework services and events.

The project architecture is normalized and ready for controlled module rebuild. This is not a playable survival game yet.

## Current State

- Framework baseline integrated
- Project architecture normalized (`Framework`, `Modules`, `Shared`, `Project`, `Tests`)
- Module placeholder structure established
- Gameplay modules pending controlled rebuild
- No production gameplay loop yet

## Repository Layout

```text
Assets/CCS/
├── Framework/   # Core platform
├── Modules/     # Gameplay modules
├── Shared/      # Cross-module shared assets/contracts
├── Project/     # Bootstrap, composition, scenes, docs
└── Tests/       # Validation and test assets
```

## Architecture Rules

- Framework contains reusable platform code only.
- Gameplay systems live in Modules.
- Shared is for cross-module assets and contracts only.
- Project owns bootstrap, composition, scenes, and project documentation.
- Each rebuilt module must include validation and a working test prefab or scene object.

## Requirements

- Unity 6
- Universal Render Pipeline
- Unity Input System

Open `Assets/CCS/Project/Scenes/SCN_CCS_Survival_Bootstrap.unity` and confirm a clean console before development.

## Documentation

[`Assets/CCS/Project/Documentation/`](Assets/CCS/Project/Documentation/README.md)

- [Versioning Policy](Assets/CCS/Project/Documentation/CCS_Versioning_Policy.md)
- [Framework Architecture](Assets/CCS/Project/Documentation/Survival_Framework_Architecture_Gate.md)
- [Module Guidelines](Assets/CCS/Project/Documentation/Future_Gameplay_Module_Guidelines.md)
- [Scene Bootstrap Standards](Assets/CCS/Project/Documentation/Survival_Scene_Bootstrap_Standards.md)

## Versioning

| | |
|---|---|
| `0.x.x` | Internal rebuild / beta / prototype |
| `1.0.0` | First alpha-ready release |

**Current tag:** `v0.1.0`

## Ownership

Copyright © Crazy Carrot Studios. All rights reserved unless a license file states otherwise.
