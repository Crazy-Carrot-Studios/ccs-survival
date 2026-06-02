# CCS Survival

[![Unity 6](https://img.shields.io/badge/Unity-6-blue)](https://unity.com/)
[![Version](https://img.shields.io/badge/Version-1.3.3-green)](https://github.com/Crazy-Carrot-Studios/ccs-survival/releases)
[![License](https://img.shields.io/badge/License-Proprietary-lightgrey)](#)

Modular survival gameplay framework for Unity 6 — built by **Crazy Carrot Studios** for reusable AAA-style survival prototypes.

**Repository:** https://github.com/Crazy-Carrot-Studios/ccs-survival  
**Upstream Core:** [ccs-framework](https://github.com/Crazy-Carrot-Studios/ccs-framework) (vendored under `Assets/CCS/Framework/`)

---

## Description

**CCS Survival** is a production-oriented, module-driven survival framework. It ships playable bootstrap integration, validation tooling, persistence, and a manual playtest harness — without locking the codebase to a single commercial title.

Install via Unity Package Manager (**Add package from git URL**):

```text
https://github.com/Crazy-Carrot-Studios/ccs-survival.git
```

---

## Current Version

**1.3.3** — Frontier Trapping Foundation

Placeable simple trap (preview + confirm), timer capture rolls for rabbit/turkey, knife harvest via wildlife harvest service, trap save/load persistence, and playtest trapping loop (craft → place → trigger → harvest → sell).

**1.3.2** — Frontier Hunting Foundation

Bow raycast hunting, knife wildlife harvest (skin/butcher drop tables), passive wildlife death/carcass state, General Store buys hunting trade goods (hide, meat, feather, fat, bone), and playtest hunting loop (bow → kill → harvest → sell).

**1.3.1** — Vendor Trading Polish + Tool Acquisition

Expanded General Store catalog, polished debug vendor HUD, safer buy/sell transactions, and hatchet trade progression (fish/salvage → dollars → hatchet → wood harvest). Knife-only starter unchanged.

**1.3.0** — Frontier Economy Foundation — currency/vendor framework, Trade Dollars, save-backed wallet, initial trade loop.

**1.2.6** — Frontier Starter Progression Rework — knife, camp, water, bow, fishing, traps, salvage, shelter, tools, and trade placeholders.

---

## Project Status

Playable modular survival prototype foundation.

**Current focus:** prototype gameplay systems, validation, persistence, and playtest harness.

---

## Core Features

| System | Status |
|--------|--------|
| Character Controller | Third-person Cinemachine 3.1 + CharacterController locomotion |
| Interaction | Forward-ray interactables |
| Inventory | Stack-based inventory foundation |
| Equipment | Wearable modifiers + primitive equipped visuals (socket rig) |
| Active Item | Service-driven select/use; weapons → combat, tools → gathering/harvest, fishing pole → fishing |
| Fishing | Service-driven water spots, catch tables, inventory rewards (no minigame yet) |
| Crafting | Recipes + workstation progression |
| World Resources / Gathering | Practical source types, harvest methods, multi-drop yields |
| Building | Placement, snapping, persistence |
| Cooking / Campfire | Campfire cooking foundation |
| Combat | Primitive combat foundation |
| Passive Wildlife | Passive wildlife AI foundation |
| Sleep / Bedroll | Sleep spots + bedroll flow |
| Storage | Container storage foundation |
| Save / Load | World + player persistence |
| Time of Day | Global game clock |
| Weather | Global weather state |
| Environment Effects | Temperature, wetness, exposure |
| Shelter | Environmental protection volumes |
| HUD / Playtest Harness | Runtime HUD + guided playtest steps |

---

## Latest Milestones

| Version | Milestone |
|---------|-----------|
| **1.3.3** | Frontier Trapping Foundation |
| **1.3.2** | Frontier Hunting Foundation |
| **1.3.1** | Vendor Trading Polish + Tool Acquisition |
| **1.3.0** | Frontier Economy Foundation |
| **1.2.6** | Frontier Starter Progression Rework |
| **1.2.5** | Fishing Foundation |
| **1.2.4** | Frontier Resource Framework Audit |
| **1.2.3** | Primitive Tool Use Routing Foundation |
| **1.2.2** | Active Item Slot + Use Flow Foundation |
| **1.2.1** | Held Item Pose + Socket Cleanup |
| **1.2.0** | Primitive Equipment Visual Foundation |
| **1.1.5** | AAA Third-Person Controller Feel Polish |
| **1.1.4** | Third-Person Controller Feel + README Polish |
| **1.1.3** | Sleep + Bedroll Foundation |
| **1.1.2** | Storage Container Foundation |
| **1.1.1** | Crafting Progression + Workstation Foundation |
| **1.1.0** | Building Progression Foundation |
| **1.0.3** | Manual Playtest Pass + Fixes |

---

## Architecture

```text
Survival → Modules → Core
```

| Layer | Role |
|-------|------|
| **Framework/Core** | Reusable CCS platform (runtime host, services, events) |
| **Modules** | Gameplay systems (`ccs.survival.*` module IDs) |
| **Survival** | Project composition — scenes, prefabs, profiles, validation |

**Rule:** Core must never reference survival or gameplay modules.

---

## Repository Layout

```text
Assets/CCS/
├── Framework/Core/          # Upstream-aligned platform
├── Modules/                 # Gameplay feature modules
│   ├── CharacterController/
│   ├── Interaction/
│   ├── Inventory/
│   ├── Crafting/
│   ├── Building/
│   ├── Combat/
│   ├── Wildlife/
│   ├── Sleep/
│   ├── Storage/
│   └── …
└── Survival/                # Project shell
    ├── Scenes/              # SCN_CCS_Survival_Bootstrap.unity
    ├── Prefabs/Player/      # PF_CCS_Player
    ├── Profiles/
    ├── Runtime/
    └── Documentation/
```

---

## Validation & Quality Gate

**Policy:** zero warnings, zero errors.

| Check | Entry |
|-------|--------|
| Batch compile | Unity opens project without compile errors |
| Survival validation | `CCS.Survival.Editor.Development.CCS_SurvivalValidationMenu.RunSurvivalValidation` |
| Module validators | Per-module `Validate*` batch methods |
| Bootstrap scene | Scene bootstrap validation utility |
| Windows build | `CCS_SurvivalBuildVerificationBuildRunner.ExecuteBatch` |

Batch commands and log paths: [Assets/CCS/Modules/README.md](Assets/CCS/Modules/README.md)

Third-person camera prefab setup (1.1.4+):

```text
CCS.Survival.Editor.Development.CCS_PlayerThirdPersonCameraBootstrapSetup.ExecuteBatch
```

---

## Latest Verified Build

| Item | Value |
|------|--------|
| Version | **1.1.5** |
| Output | `Builds/CCS_Survival_1.1.5_Windows/` (gitignored) |
| Scene | `Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity` |

Details: [Build verification](Assets/CCS/Survival/Documentation/CCS_Survival_Build_Verification.md)

---

## Development Notes

| Topic | Detail |
|-------|--------|
| Unity | Unity 6 |
| Camera | **Cinemachine 3.1** third-person follow (default) |
| Locomotion | `CharacterController` (animator root motion **OFF**) |
| Look | Reduced mouse sensitivity; safe pitch clamp |
| Architecture | Modular service-driven composition via bootstrap host |

---

## Git Hygiene

Do not commit local Unity churn:

- `Library/`, `Temp/`, `Logs/`, `UserSettings/`
- `Builds/` (development build output)
- Incidental `ProjectSettings` edits unless intentional

See `.gitignore`.

---

## Author / Studio

**James Schilz** — Crazy Carrot Studios
