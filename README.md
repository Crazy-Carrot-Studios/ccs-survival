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

**0.4.2a** — HUD Readability & Anchor Pass

**Unity:** Unity 6

### Status

Foundation phase complete. UI/HUD readability and anchor pass delivered at **0.4.2a**.

### Implemented Modules
- Character Controller
- Interaction
- Inventory
- Equipment
- UI / HUD (presentation foundation)

### Validated

- Batch compilation
- Module validation pipeline
- Bootstrap scene validation
- UI validation
- Windows build verification (0.4.2a)

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
| Equipment | Complete |
| UI / HUD | Foundation + readability (0.4.2a) |
| Crafting | Planned |
| Building | Planned |
| Wildlife | Planned |
| Combat | Planned |
| Save System | Planned |

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

**Latest verified build:** 0.4.2a
**Build output:** `Builds/CCS_Survival_0.4.2a_Windows/` (gitignored)

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

**0.4.3+** — Inventory/equipment UI expansion and gameplay service wiring

---

## Git Hygiene

Do not commit local Unity churn (`Library/`, `UserSettings/`, `Builds/`, or incidental `ProjectSettings` edits unless intentional). See `.gitignore`.

**Author:** James Schilz
