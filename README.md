# CCS Survival

Survival-focused gameplay repository for Crazy Carrot Studios.

**Repository:** https://github.com/Crazy-Carrot-Studios/ccs-survival  
**Upstream Core:** [ccs-framework](https://github.com/Crazy-Carrot-Studios/ccs-framework) (vendored under `Assets/CCS/Framework/`)

## Current Project Version

0.4.1a — Equipment Carry Capacity Expansion

## Repository Purpose

**ccs-survival** is a **gameplay and content** repository. It owns survival mechanics, genre modules, scenes, narrative product themes (including western MMO / *Reckoning* lore), and game-specific UI — while keeping the reusable **CCS Core Platform** protected under `Assets/CCS/Framework/Core/`.

| Layer | Location | Policy |
|-------|----------|--------|
| **Core Platform** | `Assets/CCS/Framework/Core/` | Upstream-aligned; no gameplay logic; changes upstream when reusable |
| **Gameplay modules** | `Assets/CCS/Modules/` | Feature assemblies (`ccs.survival.*` module IDs) |
| **Survival identity & plans** | `Assets/CCS/Survival/`, `Documentation/` | Game repo direction, milestones, architecture notes |

## What Belongs Here

- Survival loops, crafting, inventory, equipment, character control (as modules)
- Factions, quests, weapons, biomes, and narrative content
- Game scenes, prefabs, ScriptableObjects, and game-specific UI
- Module installers and manual bootstrap install plans for this game
- Survival architecture and milestone documentation

## What Does NOT Belong Here (use upstream)

Changes that benefit **every** CCS game belong in **ccs-framework**, not duplicated as one-off forks:

- Runtime host, bootstrap runner, service registry, event dispatcher
- Module registry contracts, lifecycle, diagnostics, validation
- Core smoke tests and `SCN_CCS_Bootstrap` validation scene

See [CCS Upstream Workflow](Assets/CCS/Framework/Core/Documentation/CCS_Upstream_Workflow.md).

## Studio Repository Structure

| Repository | Role |
|------------|------|
| **ccs-framework** | Permanent reusable Core Platform upstream |
| **ccs-survival** | Survival-focused gameplay and game content (this repo) |

Genre and product themes (western, post-apocalyptic, extraction, co-op, MMO survival) live **inside** this project — not in framework naming or upstream module ID examples.

## Documentation (start here)

| Document | Purpose |
|----------|---------|
| [Survival Gameplay Architecture](Documentation/Architecture/Survival_Gameplay_Architecture.md) | High-level gameplay structure and module layering |
| [Survival Module Boundaries](Documentation/Architecture/Survival_Module_Boundaries.md) | Core vs Modules vs Survival folder rules |
| [Survival Networking Authority](Documentation/Architecture/Survival_Networking_Authority.md) | Multiplayer authority direction (contracts first) |
| [Survival Persistence Direction](Documentation/Architecture/Survival_Persistence_Direction.md) | Save/load direction (no implementation in 0.1.0) |
| [Milestone 0.1.0](Documentation/Milestones/Milestone_0.1.0_Survival_Project_Identity_Setup.md) | Identity setup scope and checklist |
| [Milestone 0.2.0](Documentation/Milestones/Milestone_0.2.0_Survival_Bootstrap_Scene_Empty_Install_Pipeline.md) | Bootstrap scene + empty install pipeline |
| [Survival (Unity) docs](Assets/CCS/Survival/Documentation/README.md) | In-project survival documentation index |

**Core reference (read-only for gameplay authors):**

- [CCS Core Platform Architecture](Assets/CCS/Framework/Core/Documentation/CCS_Core_Platform_Architecture.md)
- [CCS Script Standards](Assets/CCS/Framework/Documentation/CCS_Script_Standards.md)

## Module ID Convention

Use reverse-DNS style IDs scoped to survival gameplay:

- `ccs.survival.inventory`
- `ccs.survival.crafting`
- `ccs.survival.character`

Do **not** use `ccs.reckoning.*` in shared upstream docs; *Reckoning* is product/lore inside this game repo.

## Architecture Policies (inherited from Core)

- No singleton managers or static global service locators
- No auto-discovery or scene scanning for installers/modules
- Manual bootstrap install plans and explicit module registration
- Instance-owned subsystems via `CCS_RuntimeHost` per runtime context
- Diagnostics-gated smoke tests remain under `Framework/Core/Runtime/SmokeTests/`

Gameplay code uses Core **contracts** (`CCS_IModule`, `CCS_ModuleInstallerBase`, `CCS_Result`, `CCS_EventDispatcher`) — it does not modify Core behavior for one-off game needs.

## Folder Layout (authoritative)

```text
ccs-survival/
├── Documentation/
│   ├── Architecture/          # Survival architecture direction
│   └── Milestones/            # Game repo milestones
├── Assets/CCS/
│   ├── Framework/             # Vendored Core Platform (protect from gameplay)
│   ├── Modules/               # Gameplay feature modules (future implementation)
│   └── Survival/              # Survival bootstrap, docs, game-specific shell
│       ├── Scenes/            # SCN_CCS_Survival_Bootstrap
│       ├── Prefabs/           # PF_CCS_Survival_BootstrapRoot
│       ├── Documentation/
│       └── Runtime/             # CCS.Survival.Runtime (bootstrap + character skeleton)
└── README.md
```

## Cursor Workspace Rules

- [.cursor/rules/ccs-core-platform-rules.mdc](.cursor/rules/ccs-core-platform-rules.mdc) — `alwaysApply: true`
- Enforces Core vs survival separation, script standards, naming, and multiplayer-safe direction

## Dependency rule (studio-critical)

Gameplay code depends **downward only:**

```text
Survival → Modules → Core
```

Core must never reference survival or gameplay modules.

## Milestone 0.4.1a Scope (current)

**Equipment carry capacity expansion:** equipped gear (Back, Satchel, Bedroll, etc.) exposes inventory slot and carry weight modifiers via `CCS_PlayerEquipmentService`. Inventory receives a placeholder `CCS_InventoryCapacityModifierSnapshot` hook only — bootstrap composition will connect systems before UI.

**Next:** 0.4.x UI milestone.

See [Survival README](Assets/CCS/Survival/README.md).

## Unity Version

Unity 6

## Git Hygiene

Do not commit local Unity churn (`Library/`, `UserSettings/`, incidental `ProjectSettings` edits unless intentional). See `.gitignore` and Core template checklist.

## Versioning

Game repo milestones use **0.x.y** during foundation (e.g. `0.1.0` identity setup). Align `ProjectSettings` → Player → **Version** with milestone releases when you tag gameplay milestones.

**Author:** James Schilz
