# CCS Survival

Modular survival gameplay framework for Unity, built by Crazy Carrot Studios.

**Repository:** https://github.com/Crazy-Carrot-Studios/ccs-survival  
**Upstream Core:** [ccs-framework](https://github.com/Crazy-Carrot-Studios/ccs-framework) (vendored under `Assets/CCS/Framework/`)

---

## Unity Package Manager Install

Use Unity Package Manager:

**Window → Package Manager → + → Add package from git URL**

Copy/paste URL:

```text
https://github.com/Crazy-Carrot-Studios/ccs-survival.git
```

---

## Current Version

**0.7.5** — Shelter & Environmental Protection Foundation

**Unity:** Unity 6

### Status

HUD runtime wiring to gameplay services delivered at **0.4.3**. Crafting module foundation delivered at **0.5.0**. World resource module foundation delivered at **0.5.1**. Resource harvesting integration delivered at **0.5.2**. Crafting gameplay integration delivered at **0.5.3**. Save/load foundation delivered at **0.6.0**. Save/load debug manual controls delivered at **0.6.1**. Inventory and equipment persistence delivered at **0.6.2**. Time of day foundation delivered at **0.7.0**. Weather foundation delivered at **0.7.1**. Environment effects foundation delivered at **0.7.2**. Survival Core environment integration delivered at **0.7.3**. Equipment environmental modifiers delivered at **0.7.4**. Shelter environmental protection foundation delivered at **0.7.5**.

### Implemented Modules
- Character Controller
- Interaction
- Inventory
- Equipment
- Crafting
- World Resources
- Save / Load (persistence foundation)
- Time Of Day (global game clock foundation)
- Weather (global weather state foundation)
- Environment Effects (ambient temperature, wetness, exposure simulation)
- UI / HUD (presentation foundation)

### Validated

- Batch compilation
- Module validation pipeline
- Bootstrap scene validation
- UI validation
- Windows build verification (0.6.1)

---

## Overview

**CCS Survival** is a modular, AAA-quality survival gameplay framework built on top of **CCS Core**. It provides reusable survival systems, explicit module composition, validation tooling, and a bootstrap scene for integration testing — without tying the repository to a single game product.

Gameplay modules live under `Assets/CCS/Modules/`. Project composition, scenes, profiles, and bootstrap configuration live under `Assets/CCS/Survival/`. The reusable platform lives under `Assets/CCS/Framework/Core/`.

---

## Architecture

| Layer | Role |
|-------|------|
| **Framework/Core** | Reusable CCS platform systems |
| **Modules** | Gameplay systems reusable across survival projects |
| **Survival** | Project-specific composition, profiles, scenes, validation, and bootstrap configuration |

Dependency direction:

```text
Survival → Modules → Core
```

Core must never reference survival or gameplay modules.

---

## Folder Layout

```text
Assets/
└── CCS/
    ├── Framework/
    │   └── Core/
    │
    ├── Modules/
    │   ├── SurvivalCore/
    │   ├── CharacterController/
    │   ├── Interaction/
    │   ├── Inventory/
    │   ├── Equipment/
    │   └── UI/
    │
    └── Survival/
        ├── Scenes/
        ├── Prefabs/
        ├── Profiles/
        ├── Runtime/
        └── Documentation/
```

---

## Modules

| Module | Status |
|--------|--------|
| Survival Core | Complete |
| Character Controller | Complete |
| Interaction | Complete |
| Inventory | Complete |
| Equipment | Environmental modifiers — temperature, wetness, exposure resistance (0.7.4) |
| UI / HUD | Runtime wiring (0.4.3) |
| Crafting | Gameplay integration (0.5.3) |
| World Resources | Harvest integration (0.5.2) |
| Save / Load | Foundation + inventory/equipment persistence (0.6.2) |
| Time Of Day | Global game clock foundation (0.7.0) |
| Weather | Global weather state foundation (0.7.1) |
| Environment Effects | Raw/effective simulation with shelter and equipment modifiers (0.7.5) |
| Shelter | Trigger-volume environmental protection foundation (0.7.5) |
| Survival Core | Effective environment pressure — temperature, fatigue, thirst (0.7.3+) |
| Building | Planned |
| Wildlife | Planned |
| Combat | Planned |

Module IDs use reverse-DNS style (for example `ccs.survival.inventory`). Modules register manually through the survival bootstrap install pipeline — no auto-discovery or scene scanning.

---

## Validation

Every milestone must pass:

- Batch compile
- Survival validation
- Survival Core validation
- Character Controller validation
- Interaction validation
- Inventory validation
- Equipment validation
- UI validation
- Crafting validation
- World Resources validation
- Save/load validation
- Time Of Day validation
- Weather validation
- Environment Effects validation
- Bootstrap scene validation

**Policy:** zero warnings, zero errors.

Batch entry points and log paths are documented in [Assets/CCS/Modules/README.md](Assets/CCS/Modules/README.md).

---

## Build Verification

**Bootstrap scene:** `Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity`

**Verified at 0.4.1b:**

- Main Camera
- Bootstrap root (`PF_CCS_Survival_BootstrapRoot`)
- Validation pipeline
- Windows development build

**Latest verified build:** 0.7.5
**Build output:** `Builds/CCS_Survival_0.7.5_Windows/` (gitignored)

Details: [Assets/CCS/Survival/Documentation/CCS_Survival_Build_Verification.md](Assets/CCS/Survival/Documentation/CCS_Survival_Build_Verification.md)

---

## Documentation

| Document | Purpose |
|----------|---------|
| [Survival documentation index](Assets/CCS/Survival/Documentation/README.md) | In-project survival docs |
| [Module roadmap](Assets/CCS/Survival/Documentation/CCS_Survival_Module_Roadmap.md) | Milestone sequence and module order |
| [Build verification](Assets/CCS/Survival/Documentation/CCS_Survival_Build_Verification.md) | Scene, camera, and build requirements |
| [Modules README](Assets/CCS/Modules/README.md) | Module layout and batch validation commands |
| [Core Platform Architecture](Assets/CCS/Framework/Core/Documentation/CCS_Core_Platform_Architecture.md) | Upstream Core reference |

---

## What Belongs Here

- Survival gameplay modules and installers
- Bootstrap scenes, prefabs, and profiles
- Validation and build verification tooling
- Survival architecture and milestone documentation

## What Belongs Upstream

Changes that benefit every CCS game belong in **ccs-framework**:

- Runtime host, bootstrap runner, service registry, event dispatcher
- Module registry contracts, lifecycle, diagnostics
- Core smoke tests and framework bootstrap scene

See [CCS Upstream Workflow](Assets/CCS/Framework/Core/Documentation/CCS_Upstream_Workflow.md).

---

## Next Milestone

**0.7.2+** — Environment Effects Foundation (temperature, wetness, exposure driven by Time + Weather)

---

## Git Hygiene

Do not commit local Unity churn (`Library/`, `UserSettings/`, `Builds/`, or incidental `ProjectSettings` edits unless intentional). See `.gitignore`.

**Author:** James Schilz
