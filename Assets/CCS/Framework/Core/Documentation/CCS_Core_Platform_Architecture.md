# CCS Core Platform Architecture

**Version:** 0.4.0  
**Status:** Phase One Core Platform baseline (frozen)  
**Author:** James Schilz  
**Date:** 2026-05-24

This document describes the **CCS Core Platform** architecture as of **0.3.8**. It is the reusable baseline reference before additional gameplay modules, editor tooling, or package extraction work continues.

Related documents:

- [CCS Script Standards](../../Documentation/CCS_Script_Standards.md)
- [CCS Core Stability Checkpoint 0.2.0](../../Documentation/CCS_Core_Stability_Checkpoint_0_2_0.md)
- [Framework Documentation README](../../Documentation/README.md)

---

## Platform overview

The CCS Core Platform is a **production-safe, manually composed** runtime foundation compiled in `CCS.Core.Runtime`. It connects Unity lifecycle (`CCS_RuntimeHost`) to pure C# subsystems: bootstrap composition, update loop, events, services, and module install/registry lifecycle.

```text
Unity Scene (SCN_CCS_Bootstrap)
  └── PF_CCS_RuntimeHost
        ├── CCS_RuntimeHost (MonoBehaviour bridge)
        ├── CCS_RuntimeSmokeTestBridge (diagnostics-only, optional)
        └── Subsystems (instance-owned, non-singleton):
              ├── CCS_RuntimeUpdateLoop
              ├── CCS_ServiceRegistry
              ├── CCS_EventDispatcher
              ├── CCS_BootstrapRunner
              └── CCS_ModuleHost
                    └── CCS_ModuleRegistry
```

---

## Core policies

| Policy | Rule |
|--------|------|
| **No singleton** | Subsystems are instance-owned by `CCS_RuntimeHost` or installers. No static global managers. |
| **No auto-discovery** | No reflection scanning, no scene-wide module discovery, no automatic installer registration. |
| **Manual registration** | Installers, services, events, and modules are registered explicitly by bootstrap/game code. |
| **CCS_Result for outcomes** | Mutating operations return `CCS_Result`. Lifecycle enums describe state; they do not replace results. |
| **Diagnostics gating** | Validation smoke tests run only when `CCS_RuntimeHost.EnableRuntimeDiagnostics` is true. |

---

## RuntimeHost

**Type:** `CCS_RuntimeHost` (`Core/Runtime/Systems/RuntimeHost/`)  
**Role:** Thin Unity MonoBehaviour bridge into CCS runtime architecture.

### Responsibilities

- Create and own runtime subsystems in `Awake`
- Drive `CCS_RuntimeUpdateLoop` from Unity `Update` / `FixedUpdate` / `LateUpdate`
- Expose subsystems as properties
- Clear subsystems in `OnDestroy`
- Expose `EnableRuntimeDiagnostics` (permission flag only — no validation logic inside host)

### Does not

- Run smoke tests or module logic directly
- Use `DontDestroyOnLoad` or singleton patterns
- Auto-discover installers or modules

### Owned subsystems

| Property | Type |
|----------|------|
| `RuntimeUpdateLoop` | `CCS_RuntimeUpdateLoop` |
| `ServiceRegistry` | `CCS_ServiceRegistry` |
| `EventDispatcher` | `CCS_EventDispatcher` |
| `BootstrapRunner` | `CCS_BootstrapRunner` |
| `ModuleHost` | `CCS_ModuleHost` |

---

## BootstrapRunner

**Type:** `CCS_BootstrapRunner` (`Core/Runtime/Systems/Bootstrap/`)  
**Contract:** `CCS_IBootstrapInstaller`

### Responsibilities

- Collect installers via `RegisterInstaller`
- Execute installers in registration order via `Run(CCS_RuntimeHost)`
- Clear installer list on shutdown

### Usage pattern

```text
runtimeHost.BootstrapRunner.RegisterInstaller(installer);
runtimeHost.BootstrapRunner.Run(runtimeHost);
```

Installers are **plain C# types** — not MonoBehaviours.

---

## RuntimeUpdateLoop

**Type:** `CCS_RuntimeUpdateLoop` (`Core/Runtime/Systems/UpdateLoop/`)

### Responsibilities

- Register `CCS_IUpdatable`, `CCS_IFixedUpdatable`, `CCS_ILateUpdatable`
- Tick registered systems from host Unity callbacks
- Clear registrations on shutdown

Gameplay and module systems integrate by registering updatables — not by inheriting `MonoBehaviour` update methods in Core.

---

## EventDispatcher

**Type:** `CCS_EventDispatcher` (`Core/Runtime/Systems/Events/`)  
**Contract:** `CCS_IEvent`, `CCS_IEventDispatcher`

### Responsibilities

- Lightweight publish/subscribe for `CCS_IEvent` types
- Instance-owned per runtime host
- Cleared on shutdown

Future modules should communicate through events (and services/update contracts) rather than static global buses.

---

## ServiceRegistry

**Type:** `CCS_ServiceRegistry` (`Core/Runtime/Services/Registry/`)  
**Contract:** `CCS_IService`, `CCS_IServiceRegistry`

### Responsibilities

- Register and resolve services by **interface type**
- Instance-owned per runtime host
- `bool` return helpers for register/unregister (existing service API)

Module install flow does **not** auto-register services — modules or installers do that explicitly when needed.

---

## Module contracts

**Path:** `Core/Runtime/Modules/`

| Type | Purpose |
|------|---------|
| `CCS_IModule` | Module contract extending `CCS_ISystem` |
| `CCS_IModuleInstaller` | Bootstrap installer contract with `Module` property |
| `CCS_IModuleDependencyProvider` | Declares required module IDs (no resolution yet) |
| `CCS_ModuleMetadata` | Immutable module identity |
| `CCS_ModuleState` | System lifecycle (`Uninitialized` → `Initialized` → `Installed` → `Shutdown`) |
| `CCS_ModuleLifecycleState` | Install lifecycle (`Uninstalled`, `Installing`, `Installed`, `Failed`, `Uninstalling`) |

---

## ModuleBase

**Type:** `CCS_ModuleBase` (`Core/Runtime/Modules/Base/`)

### Responsibilities

- Default `Initialize` / `Install` / `Uninstall` / `Shutdown` orchestration
- `CCS_ModuleState` transitions for system lifecycle
- `CCS_ModuleLifecycleState` exposed via `LifecycleState`
- Protected virtual hooks: `OnInitialize`, `OnInstall`, `OnUninstall`, `OnShutdown`

### Design notes

- Non-MonoBehaviour
- No automatic service or update registration
- `CCS_Result` returned from `Install` / `Uninstall`

---

## ModuleInstallerBase

**Type:** `CCS_ModuleInstallerBase` (`Core/Runtime/Modules/Base/`)

### Responsibilities

- Thin bootstrap adapter implementing `CCS_IModuleInstaller`
- Orchestrates module install pipeline into `CCS_RuntimeHost`
- Calls `ModuleHost.RegisterInstalledModule` after successful install

### Install pipeline order

1. Validate `runtimeHost` and `module`
2. **Duplicate preflight** (`ModuleHost.TryPreflightInstall`) — **before any hooks**
3. Set lifecycle `Installing`
4. `OnBeforeInstall`
5. `module.Install(runtimeHost)`
6. `OnAfterInstall`
7. `ModuleHost.RegisterInstalledModule`
8. Set lifecycle `Installed`

On failure: lifecycle `Failed`, warnings via `CCS_Logger`, no exceptions for expected validation failures.

---

## ModuleRegistry

**Type:** `CCS_ModuleRegistry` (`Core/Runtime/Modules/Registry/`)  
**Contract:** `CCS_IModuleRegistry`

### Responsibilities

- Manual registration by `CCS_ModuleMetadata.ModuleId`
- `CCS_Result` for `RegisterModule` / `UnregisterModule`
- TryGet by module ID or module type
- Duplicate module IDs rejected
- Missing unregister returns failure with warning

Registry is **ID/type lookup only** — not a lifecycle manager.

---

## ModuleHost

**Type:** `CCS_ModuleHost` (`Core/Runtime/Modules/Host/`)

### Responsibilities

- Own private `CCS_ModuleRegistry` instance
- Safe query APIs for installed modules
- Orchestrate install registration and uninstall flows

### Key APIs

| API | Purpose |
|-----|---------|
| `RegisterInstalledModule` | Register after successful install |
| `UnregisterInstalledModule` | Registry removal |
| `TryPreflightInstall` | Block duplicate IDs before install pipeline |
| `UninstallModule(runtimeHost, moduleId)` | `Uninstall` + unregister on success |
| `TryGetModule` / `TryGetModule<T>` | Lookup |
| `IsModuleRegistered` / `IsModuleInstalled` | State queries |
| `TryGetModuleLifecycleState` | Lifecycle query by ID |

---

## ModuleLifecycleState

**Type:** `CCS_ModuleLifecycleState` (`Core/Runtime/Modules/Data/`)

| State | Meaning |
|-------|---------|
| `Uninstalled` | Not installed / removed from registry flow |
| `Installing` | Install pipeline in progress |
| `Installed` | Successfully installed and registered |
| `Failed` | Install/uninstall attempt failed or duplicate blocked |
| `Uninstalling` | Uninstall pipeline in progress |

`CCS_ModuleState` remains the **system** lifecycle (`CCS_ISystem`). Both can be queried; they serve different concerns.

---

## Install flow (end-to-end)

```text
BootstrapRunner.Run(host)
  └── CCS_ModuleInstallerBase.Install(host)
        ├── TryPreflightInstall(moduleId)     ← duplicate blocked here
        ├── Lifecycle: Installing
        ├── OnBeforeInstall
        ├── CCS_ModuleBase.Install(host)
        │     ├── Initialize (if needed)
        │     └── OnInstall
        ├── OnAfterInstall
        ├── ModuleHost.RegisterInstalledModule
        └── Lifecycle: Installed
```

---

## Duplicate install preflight

**Location:** `CCS_ModuleHost.TryPreflightInstall` called at the top of `CCS_ModuleInstallerBase.Install` (after host/module validation, **before** `OnBeforeInstall` and `module.Install`).

### Expected behavior

- Registered module remains `Installed`
- Duplicate installer instance gets warning + lifecycle `Failed`
- **No** second pass through before/after hooks or `module.Install`

Validated in Play Mode (0.3.6+): duplicate path shows only preflight warning and `Failed` on the duplicate instance.

---

## Uninstall flow

```text
ModuleHost.UninstallModule(host, moduleId)
  ├── TryGetModule(moduleId)
  ├── module.Uninstall(host)
  │     ├── Lifecycle: Uninstalling
  │     ├── OnUninstall
  │     └── Lifecycle: Uninstalled (or Failed)
  └── UnregisterInstalledModule(moduleId)   ← only after successful uninstall
```

### Graceful failures

| Case | Outcome |
|------|---------|
| Missing module ID | `CCS_Result.Failure` + warning |
| Duplicate uninstall | `CCS_Result.Failure` + warning (not in registry) |
| Uninstall hook failure | Lifecycle `Failed`, module remains registered |

---

## Core diagnostics (0.3.11+)

**Path:** `Core/Runtime/Diagnostics/`

Read-only diagnostics are built **on demand** via `CCS_RuntimeHost.BuildDiagnosticsReport()`. No per-frame allocation.

| Type | Purpose |
|------|---------|
| `CCS_CoreDiagnosticsReport` | Aggregated host snapshot |
| `CCS_ModuleDiagnosticsInfo` | Module ID + lifecycle state |
| `CCS_ServiceDiagnosticsInfo` | Service count + type names |
| `CCS_UpdateLoopDiagnosticsInfo` | Updatable / fixed / late counts |

Exposed from: `CCS_RuntimeHost`, `CCS_ModuleHost`, `CCS_ServiceRegistry`, `CCS_RuntimeUpdateLoop`, `CCS_EventDispatcher`, `CCS_BootstrapRunner`.

---

## Core validation (0.3.12+)

**Type:** `CCS_CoreValidation` (`Core/Runtime/Utilities/Validation/`)

Centralized validation helpers returning `CCS_Result` (no throws for expected failures). `CCS_Validation` delegates to `CCS_CoreValidation` for backward compatibility.

---

## Core error codes (0.3.12+)

**Type:** `CCS_CoreErrorCode` + `CCS_Result.ErrorCode`

Stable failure classification (`DuplicateModuleId`, `ModuleNotRegistered`, `MissingRequiredModuleDependency`, etc.). Existing string messages preserved; new failures should use `CCS_Result.Failure(CCS_CoreErrorCode, message)`.

---

## Module dependencies (0.3.13+)

**Types:** `CCS_ModuleDependency`, `CCS_ModuleDependencyType`

Modules declare dependencies via `CCS_IModule.Dependencies` (default empty on `CCS_ModuleBase`). **Metadata only** — no auto-install or discovery.

`CCS_ModuleInstallerBase` validates required module/service dependencies before duplicate preflight. Optional dependencies never fail install.

---

## Manual module install plans (0.3.14+)

**Types:** `CCS_ModuleInstallPlan`, `CCS_ModuleInstallPlanEntry`

Explicit ordered installer list. Validates duplicate module IDs and dependencies before execution. **Stop on first failure.** Manual order is authoritative (no automatic sorting).

---

## Diagnostics and smoke tests

**Path:** `Core/Runtime/SmokeTests/`  
**Entry:** `CCS_RuntimeSmokeTestBridge` on `PF_CCS_RuntimeHost` (diagnostics only)

### Gate

Smoke tests run only when `enableRuntimeDiagnostics` is **true** on `CCS_RuntimeHost`.

### Validated scenarios (0.4.0 baseline)

Play `SCN_CCS_Bootstrap` with diagnostics enabled and confirm:

| Scenario | Expected |
|----------|----------|
| Runtime host init | `[CCS Runtime Host] Runtime host initialized.` |
| Runtime smoke test | Initialized, installer completed, update tick confirmed |
| Module install | Installing → hooks → Installed → registry registered |
| Duplicate install | Preflight warning only; no second hook pass; duplicate `Failed`; `ModuleAlreadyInstalled` error code |
| Missing dependency install | Dependent module `Failed`; no auto-install of parent |
| Dependency present install | Dependent module `Installed` when parent installed |
| Install plan duplicate | `DuplicateInstallPlanEntry` before partial install |
| Install plan bad order | `MissingRequiredModuleDependency` |
| Install plan success | Smoke module installed via plan |
| Diagnostics report | Generated without exceptions; zero modules after uninstall |
| Module uninstall | Uninstalling → uninstalled → registry cleared → `Uninstalled` |
| Missing uninstall | `ModuleNotRegistered` + warning |
| Second uninstall | Warning + graceful failure |
| Clean shutdown | All subsystems cleared |

---

## Future multiplayer / MMO-safe design notes

The 0.3.8 baseline is intentionally compatible with future large-scale multiplayer goals:

1. **Instance-owned subsystems** — Each runtime host/context can own its own registries and loops without global static state.
2. **Explicit composition** — Bootstrap installers compose features per mode (client, server, headless) without scene scanning.
3. **Module ID registry** — Modules are identified by stable string IDs suitable for manifest-driven server builds later.
4. **Result-driven failures** — Validation and install/uninstall failures are observable without exception-driven control flow.
5. **Separation of concerns** — Unity bridge (`CCS_RuntimeHost`) stays thin; simulation-facing logic remains in plain C# systems and modules.
6. **Diagnostics isolation** — Play Mode validation does not change production host behavior when diagnostics are disabled.

Future game repos may add: networked service boundaries, editor module manifests, and package-based module distribution — without rewriting this baseline.

---

## Assembly and paths

| Item | Location |
|------|----------|
| Runtime assembly | `CCS.Core.Runtime` (`Core/Runtime/Assembly/`) |
| Bootstrap scene | `Core/Runtime/Scenes/SCN_CCS_Bootstrap.unity` |
| Runtime host prefab | `Core/Runtime/Prefabs/PF_CCS_RuntimeHost.prefab` |
| Module contracts | `Core/Runtime/Modules/` |
| Smoke tests | `Core/Runtime/SmokeTests/` |

---

## Version history (Core Platform)

| Version | Milestone |
|---------|-----------|
| 0.2.0 | Core runtime stability checkpoint |
| 0.3.0–0.3.2 | Module contracts, base, installer base |
| 0.3.3–0.3.4 | Module registry and host integration |
| 0.3.5–0.3.6 | Installer smoke alignment, lifecycle state, duplicate preflight |
| 0.3.7 | Module uninstall foundation |
| 0.3.8 | Core Platform baseline documented |
| 0.3.9–0.3.10 | Template readiness + final validation pass |
| 0.3.11–0.3.14 | Diagnostics, validation, error codes, dependencies, install plans |
| **0.4.0** | **Phase One Core Platform freeze (this file)** |
