# CCS Framework

Reusable AAA-ready modular game platform for Crazy Carrot Studios.

**Repository:** https://github.com/Crazy-Carrot-Studios/ccs-framework

## Current Framework Version

0.4.4

## Repository Purpose

This repository is the **permanent reusable CCS Core Platform upstream**. It is the engine/platform layer for all future Crazy Carrot Studios games — not a game project.

## What Belongs Here

- Runtime systems
- Bootstrap systems
- Module architecture
- Service registry
- Diagnostics
- Validation
- Save foundations (shared contracts only, when added)
- Networking-safe architecture
- Reusable engine/platform systems
- Core documentation, smoke tests, and bootstrap validation scene

## What Does NOT Belong Here

- Game-specific gameplay
- Survival mechanics
- Western systems
- MMO content
- Quests
- Factions
- Weapons
- Crafting content
- UI specific to a game
- Scenes/assets for a game

Gameplay belongs in **forked or templated game repositories** under `Assets/CCS/Modules/`.

## Recommended Workflow

1. Keep **ccs-framework** as the reusable upstream
2. Fork, branch, or **Use this template** into game repos
3. Build gameplay systems in game repos
4. Upstream reusable improvements back into **ccs-framework** when appropriate

See [CCS Upstream Workflow](Assets/CCS/Framework/Core/Documentation/CCS_Upstream_Workflow.md) and [CCS GitHub Template Setup](Assets/CCS/Framework/Core/Documentation/CCS_GitHub_Template_Setup.md).

## Studio repository structure

| Repository | Role |
|------------|------|
| **ccs-framework** | Reusable upstream Core Platform (this repo) |
| **ccs-survival** | Survival-focused gameplay and game content |

Genre-specific titles (western, post-apocalyptic, extraction, co-op, MMO survival, and similar) belong **inside** a game project such as **ccs-survival** — not in framework or repository names.

## Suggested future repositories

- **ccs-survival** — survival gameplay (western, post-apocalyptic, extraction, co-op, MMO survival, crafting, and related genres)
- **ccs-kids-learning**
- **ccs-topdown-prototype**

## Current Baseline

| Item | Value |
|------|--------|
| **Version** | 0.4.4 |
| **Phase One baseline tag** | `v0.4.0-core-platform-baseline` |
| **Core Platform** | Complete and validated (0.4.0+) |

**Key documents:**

- [CCS Core Platform Architecture](Assets/CCS/Framework/Core/Documentation/CCS_Core_Platform_Architecture.md)
- [CCS Core Platform 0.4.0 Release Notes](Assets/CCS/Framework/Core/Documentation/Releases/CCS_Core_Platform_0.4.0_Release_Notes.md)
- [CCS Phase One Completion Checklist](Assets/CCS/Framework/Core/Documentation/CCS_Phase_One_Core_Platform_Completion_Checklist.md)
- [CCS Core Template Readiness Checklist](Assets/CCS/Framework/Core/Documentation/CCS_Core_Template_Readiness_Checklist.md)

**Platform rules:** no singleton managers, no auto-discovery, no gameplay code in Core, manual registration only, diagnostics-gated smoke tests under `Core/Runtime/SmokeTests/`.

## Cursor workspace rules

Repository-level Cursor rules enforce CCS architectural policies during AI-assisted development:

- **Auto-apply (Cursor):** [.cursor/rules/ccs-core-platform-rules.mdc](.cursor/rules/ccs-core-platform-rules.mdc) — `alwaysApply: true` in every session
- **Reference copy:** [.cursor/rules/ccs-core-platform-rules.md](.cursor/rules/ccs-core-platform-rules.md)
- **Covers:** `ccs-framework` vs `ccs-survival` separation, Core constraints, script standards, naming, module policy, multiplayer-safe direction
- **Governance only** — no runtime behavior change; policies align with Core Platform documentation

**Do not commit** local Unity churn (`Library/`, `UserSettings/`, incidental scene/RPM edits) — see `.gitignore` and the template checklist.

## Architecture
- Framework
- Modules
- Shared
- Database
- Documentation
- Tests

## Versioning
Framework version uses semantic-style **Major.Minor.Patch** (e.g. `0.0.1`, `0.1.0`, `1.0.0`).

| Bump | When to use |
|------|-------------|
| **Patch** | Small fixes, cleanup, non-breaking adjustments |
| **Minor** | New framework systems or modules |
| **Major** | Large architectural milestones |

**Sync rule:** On every major framework milestone commit, update **both**:
1. `Current Framework Version` in this README
2. **Unity Player → Version** (`ProjectSettings/ProjectSettings.asset` → `bundleVersion`)

Keep Git commits, framework milestones, Unity project version, and internal documentation aligned.

## Current Status
Foundation setup and architecture established.

## Framework Bootstrap Structure Established

Authoritative folder layout under `Assets/CCS/Framework/`:

- **Core** — Runtime and Editor foundation (systems, services, utilities, data, assembly definitions)
- **Modules** — Pluggable feature assemblies (future)
- **Shared** — Framework-level shared assets
- **Tests** — Runtime and Editor test roots
- **Documentation** — Framework documentation

## Git Hygiene Established

Repository ignore rules keep generated and local-only content out of Git:

- Unity cache and build folders are ignored (`Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/`, `MemoryCaptures/`, `Build/`, `Builds/`)
- Local IDE files are ignored (`.vs/`, `.vscode/`, `*.csproj`, `*.sln`, and related generated project files)

**Still tracked:** `Assets/**/*.meta`, `ProjectSettings/`, `Packages/`, `README.md`, and all `.asmdef` / `.asmdef.meta` files.

## CCS Scripting Standards Established

Authoritative scripting standards are defined before runtime implementation:

- Document: `Assets/CCS/Framework/Documentation/CCS_Script_Standards.md`
- Covers CCS headers, region order, naming, logging, runtime/editor separation, and event-driven architecture

## CCS Core Interfaces Foundation Established

Foundational runtime contracts in `CCS.Core.Runtime`:

- `CCS_IService` — Base service contract (`IsInitialized`, `Initialize`)
- `CCS_ISystem` — Base system contract (`IsInitialized`, `Initialize`, `Shutdown`)
- `CCS_IUpdatable`, `CCS_IFixedUpdatable`, `CCS_ILateUpdatable` — Update loop contracts

Interfaces only; no implementations, singletons, or service locators in this milestone.

## CCS Core Result and Message Types Established

Foundational runtime utility types in `CCS.Core.Runtime`:

- `CCS_Result` — Immutable non-generic operation result
- `CCS_Result<T>` — Immutable generic result wrapper
- `CCS_Message` / `CCS_MessageType` — Classified message data for future UI and logging layers

Structs only; no managers, service locators, or `Debug.Log` integration in this milestone.

## CCS Core Logging Foundation Established

Centralized runtime logging in `CCS.Core.Runtime`:

- `CCS_Logger` — Static helper with `Log`, `LogWarning`, `LogError`, and `FormatMessage`
- Format: `[CCS {category}] {message}`
- Normal logs respect `isEnabled`; warnings and errors always output

No managers, file logging, or editor tooling in this milestone.

## CCS Core Validation Foundation Established

Centralized runtime validation in `CCS.Core.Runtime`:

- `CCS_Validation` — Static helpers for objects, strings, and collections
- `ValidateObject` / `ValidateString` — Return `CCS_Result` for consistent failure handling

No exceptions, logging, managers, or editor tooling in this milestone.

## CCS Core Service Registry Foundation Established

First minor-version architecture milestone — interface-driven service registration in `CCS.Core.Runtime`:

- `CCS_IServiceRegistry` — Register, unregister, resolve, and query services by interface type
- `CCS_ServiceRegistry` — Lightweight sealed implementation using `Dictionary<Type, CCS_IService>`

No static global singleton, MonoBehaviours, bootstrap scenes, or automatic service lifecycle in this milestone.

## CCS Core Event Foundation Established

Lightweight decoupled event architecture in `CCS.Core.Runtime`:

- `CCS_IEvent` — Base event contract with `Timestamp`
- `CCS_IEventDispatcher` / `CCS_EventDispatcher` — Subscribe, unsubscribe, dispatch, and clear

No static global dispatcher, MonoBehaviours, async/threading, or gameplay logic in this milestone.

## CCS Core Runtime Update Loop Foundation Established

Interface-driven update scheduling in `CCS.Core.Runtime`:

- `CCS_RuntimeUpdateLoop` — Registers and drives `CCS_IUpdatable`, `CCS_IFixedUpdatable`, and `CCS_ILateUpdatable` systems
- `Tick` / `FixedTick` / `LateTick` — Called in registration order without per-frame list allocation

No MonoBehaviour bridge, static singleton, or gameplay systems in this milestone.

## CCS Core Runtime Host Foundation Established

First controlled Unity bridge in `CCS.Core.Runtime`:

- `CCS_RuntimeHost` — Thin `MonoBehaviour` connecting Unity lifecycle to CCS architecture
- Owns `CCS_RuntimeUpdateLoop`, `CCS_ServiceRegistry`, and `CCS_EventDispatcher` instances
- `Update` / `FixedUpdate` / `LateUpdate` drive registered systems; `OnDestroy` clears registries

No singleton, `DontDestroyOnLoad`, gameplay logic, or automatic module loading in this milestone.

## CCS Bootstrap Installer Foundation Established

Modular runtime composition in `CCS.Core.Runtime`:

- `CCS_IBootstrapInstaller` — Contract for installing into `CCS_RuntimeHost`
- `CCS_BootstrapRunner` — Registers and runs installers in order
- `CCS_RuntimeHost.BootstrapRunner` — Host-owned runner (not auto-run yet)

No gameplay modules, scene auto-loading, or ScriptableObject installers in this milestone.

## CCS Runtime Host Prefab Foundation Established

Official runtime entry prefab in `Assets/CCS/Framework/Core/Runtime/Prefabs/`:

- **`PF_CCS_RuntimeHost`** — Root GameObject with `CCS_RuntimeHost` (`enableDebugLogs: false` by default)
- Drop into any scene as the standard CCS runtime entry point

Prefab only; no sample scenes or gameplay wiring in this milestone.

## CCS Bootstrap Scene Foundation Established

Minimal framework startup scene:

- **`SCN_CCS_Bootstrap`** — `Assets/CCS/Framework/Core/Runtime/Scenes/SCN_CCS_Bootstrap.unity`
- Contains single `PF_CCS_RuntimeHost` prefab instance
- Registered as **Build Settings index 0**

No camera, lights, gameplay objects, or scene loading in this milestone.

## CCS Runtime Smoke Test Foundation Established

Play Mode architecture validation in `CCS.Core.Runtime`:

- `CCS_RuntimeSmokeTestSystem` — `CCS_ISystem` + `CCS_IUpdatable` smoke validation
- `CCS_RuntimeSmokeTestInstaller` — Registers smoke test via bootstrap runner
- `CCS_RuntimeSmokeTestBridge` — Isolated validation MonoBehaviour (on `PF_CCS_RuntimeHost` for bootstrap validation)

Expected logs when bridge `enableDebugLogs` is enabled: smoke test initialized, installer completed, update tick confirmed.

## CCS Runtime Smoke Test Isolation Established

Production/runtime boundary cleanup:

- Removed smoke test coupling from `CCS_RuntimeHost`
- Added `CCS_RuntimeSmokeTestBridge` for validation-only bootstrap orchestration
- `CCS_RuntimeHost` is validation-agnostic; smoke testing is an optional bridge layer

## CCS Runtime Diagnostics Toggle Foundation Established

Explicit diagnostics gating on `CCS_RuntimeHost`:

- `enableRuntimeDiagnostics` — Allows diagnostic bridges to run validation (default **true** on `PF_CCS_RuntimeHost` during foundation development)
- `CCS_RuntimeSmokeTestBridge` — Runs smoke tests only when `EnableRuntimeDiagnostics` is true

With diagnostics **disabled**: no smoke test logs, no errors. With diagnostics **enabled**: standard three smoke test log lines.

## CCS Core Framework Stability Checkpoint Established

First stable architecture checkpoint for CCS Core runtime foundation (**0.2.0**):

- Documented in `Assets/CCS/Framework/Documentation/CCS_Core_Stability_Checkpoint_0_2_0.md`
- Verified pipeline: RuntimeHost → BootstrapRunner → SmokeTestInstaller → RuntimeUpdateLoop → Tick → Clean Shutdown
- Production boundaries: diagnostics-gated validation, production-safe host, bootstrap/service/event/update contracts for future systems

## CCS Module Contract Foundation Established

Module architecture contracts in `CCS.Core.Runtime` (`Core/Runtime/Modules/`):

- `CCS_ModuleState` — Module lifecycle enum
- `CCS_ModuleMetadata` — Immutable module identity struct
- `CCS_IModule` — Module contract extending `CCS_ISystem`
- `CCS_IModuleDependencyProvider` — Declared module dependency IDs
- `CCS_IModuleInstaller` — Bootstrap installer contract for modules

No gameplay modules, managers, discovery, or editor tooling in this milestone. Core **0.2.0** runtime checkpoint remains stable.

## CCS Module Base Class Foundation Established

`CCS_ModuleBase` abstract lifecycle class in `Core/Runtime/Modules/Base/`:

- Implements `CCS_IModule` with shared `Initialize`, `Install`, `Uninstall`, and `Shutdown` behavior
- Protected virtual hooks: `OnInitialize`, `OnInstall`, `OnUninstall`, `OnShutdown`
- Non-MonoBehaviour; no automatic service/update registration or singleton behavior
- Corrected invalid Unity `.meta` GUIDs for module contract assets (32-character hex)

## CCS Module Installer Base Foundation Established

`CCS_ModuleInstallerBase` bootstrap adapter in `Core/Runtime/Modules/Base/`:

- Implements `CCS_IModuleInstaller` with shared host/module validation
- Delegates install to `CCS_IModule.Install`
- Protected hooks: `OnBeforeInstall`, `OnAfterInstall`, `GetLogCategory`
- Non-MonoBehaviour; no discovery, singletons, or gameplay managers

## CCS Module Registry Foundation Established

`CCS_IModuleRegistry` and `CCS_ModuleRegistry` in `Core/Runtime/Modules/Registry/`:

- Manual registration by `CCS_ModuleMetadata.ModuleId`
- `CCS_Result` for register/unregister; TryGet by ID or type; duplicate ID prevention
- Non-singleton instance registry; no auto-discovery

## CCS Module Host Registry Integration Established

`CCS_ModuleHost` on `CCS_RuntimeHost`:

- Owns private `CCS_ModuleRegistry` (no static/singleton registry)
- Registers modules after successful install via `CCS_ModuleInstallerBase`
- Safe queries: `TryGetModule`, `IsModuleRegistered`, `GetRegisteredModules`
- Service registry behavior on runtime host unchanged

## CCS Installer Smoke Test Alignment Established

Diagnostics smoke tests now validate the module installer pipeline:

- `CCS_SmokeTestModule` / `CCS_SmokeTestModuleInstaller` via `CCS_ModuleInstallerBase`
- Confirms before/after install hooks, module install, `ModuleHost` registration, and duplicate ID blocking

## CCS Module Lifecycle State Tracking Established

`CCS_ModuleLifecycleState` (Uninstalled → Installing → Installed / Failed):

- Tracked on `CCS_ModuleBase`; orchestrated by `CCS_ModuleInstallerBase`
- Duplicate install blocked before pipeline runs; registered module stays `Installed`
- `CCS_ModuleHost` exposes `IsModuleInstalled` and `TryGetModuleLifecycleState`
- `CCS_ModuleState` remains system lifecycle; `CCS_Result` remains operation outcome

## CCS Module Uninstall Foundation Established

Manual module uninstall via `CCS_ModuleHost`:

- `CCS_IModule.Uninstall` / `CCS_ModuleBase.OnUninstall` — lifecycle `Uninstalling` → `Uninstalled` or `Failed`
- `UnregisterInstalledModule` / `UninstallModule` — orchestrated uninstall and registry removal
- Smoke test validates uninstall, registry cleanup, and graceful missing/duplicate uninstall failures

## CCS Core Platform Baseline Documentation Established

Authoritative architecture reference at **0.3.8**:

- [Assets/CCS/Framework/Core/Documentation/CCS_Core_Platform_Architecture.md](Assets/CCS/Framework/Core/Documentation/CCS_Core_Platform_Architecture.md)
- Covers RuntimeHost, bootstrap, update loop, events, services, module contracts, install/uninstall flows, smoke test expectations, and platform policies (no singleton, no auto-discovery, manual registration)

**0.3.7 Play Mode validation confirmed:** install, duplicate preflight (no hook re-run), uninstall, registry clear, graceful missing/duplicate uninstall failures.

## CCS Core Template Readiness Pass Established

Template-readiness review for GitHub template / game project branching:

- Folder structure and Core-only `.cs` scope verified
- Smoke tests isolated under `Core/Runtime/SmokeTests/`
- Documentation points to Core Platform baseline + template checklist
- Local-only Unity files excluded via `.gitignore` (do not commit incidental scene/settings churn)

## CCS Core Platform Final Validation Pass Established

Milestone **0.3.10** confirms the foundation is clean and consistent: no new architecture; validation note + meta fix only. Ready for **0.4.x** core baseline / GitHub template phase after Play Mode reconfirmation.

## Unity Version
Unity 6

## Notes
This framework is designed to support multiple future game genres including survival, simulation, learning games, and large-scale multiplayer projects.
