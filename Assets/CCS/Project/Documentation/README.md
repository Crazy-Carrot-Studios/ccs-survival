# CCS Project Documentation

**Version:** 0.5.5 — Project audit baseline

## Overview

Project-specific documentation for the CCS Survival composition layer: bootstrap rules, runtime foundation, validation standards, and architecture gate.

Broader planning and contributor guides live in the repo [`Documentation/`](../../../../Documentation/README.md) folder.

## Project documentation

| Document | Topic |
|----------|-------|
| [Survival Framework Architecture Gate](Survival_Framework_Architecture_Gate.md) | Ownership boundaries, identity, profiles, scenes, runtime principles |
| [Survival Runtime Foundation](Survival_Runtime_Foundation.md) | Module base classes, installer hierarchy, service marker, constants |
| [Survival Validation Standards](Survival_Validation_Standards.md) | Module ID rules, profile identity, save-safe IDs, diagnostics |
| [Survival Scene Bootstrap Standards](Survival_Scene_Bootstrap_Standards.md) | Composition root, host requirements, profile slots, scene validation |
| [Versioning Policy](CCS_Versioning_Policy.md) | Rebuild version map and tag rules |

## Bootstrap assets

| Asset | Path |
|-------|------|
| Project bootstrap scene | `Assets/CCS/Scenes/Bootstrap/SCN_CCS_Survival_Bootstrap.unity` |
| Project bootstrap prefab | `Assets/CCS/Project/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab` |
| Core validation scene | `Assets/CCS/Framework/Core/Runtime/Scenes/SCN_CCS_Bootstrap.unity` |

## Primary test scenes

| Scene | Role |
|-------|------|
| `Assets/CCS/Modules/CharacterController/Scenes/Validation/SCN_CCS_CharacterController_Validation.unity` | **Source of truth** — controller, attributes HUD, interaction |
| `Assets/CCS/Scenes/Network/SCN_CCS_MultiplayerHosting.unity` | Host/join Netcode harness |
| `Assets/CCS/Modules/CharacterController/Tests/Scenes/SCN_CCS_CharacterController_Test.unity` | Legacy ground-only preview (retained) |

## Runtime assembly

`Assets/CCS/Project/Runtime/CCS.Project.Runtime.asmdef` → `CCS.Core.Runtime` only.  
Namespace: `CCS.Project`.

## Project audit

**CCS → Project → Run Project Audit** — report-first checks for version/doc consistency, active modules, asmdefs, and legacy interaction leftovers. Does not regenerate scenes.

## Modules

Module conventions and active module docs: [`Assets/CCS/Modules/`](../Modules/README.md)

- [Character Controller](../Modules/CharacterController/Documentation/CCS_CharacterController_Module.md)
- [Attributes](../Modules/Attributes/Documentation/CCS_Attributes_Module.md)
- [Interaction](../Modules/Interaction/Documentation/CCS_Interaction_Module.md)
