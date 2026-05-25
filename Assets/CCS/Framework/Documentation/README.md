# CCS Framework — Documentation

Authoritative documentation for the CCS framework.

## Established standards

- **[CCS Script Standards](CCS_Script_Standards.md)** — Required headers, regions, naming, logging, runtime/editor separation, and architecture conventions.

## Core runtime contracts

- **Core runtime interfaces established** — Foundational service and system contracts in `Core/Runtime/Services/Interfaces/` and `Core/Runtime/Systems/Interfaces/`
- **Framework contract layer introduced** — `CCS_IService`, `CCS_ISystem`, and update interfaces (`CCS_IUpdatable`, `CCS_IFixedUpdatable`, `CCS_ILateUpdatable`) define the architectural contract layer before implementations

## Core runtime utility types

- **Core result/message utility types established** — `CCS_Result`, `CCS_Result<T>`, `CCS_Message`, and `CCS_MessageType` in `Core/Runtime/Data/Results/` and `Core/Runtime/Data/Messages/`

## Core runtime utilities

- **Core logging helper established** — `CCS_Logger` in `Core/Runtime/Utilities/Logging/` with `[CCS {category}]` formatting, optional normal log toggles, and always-visible warnings/errors
- **Core validation utilities established** — `CCS_Validation` in `Core/Runtime/Utilities/Validation/` for reusable null, string, and collection checks with `CCS_Result` patterns

## Core service registry

- **Core service registry foundation established** — `CCS_IServiceRegistry` and `CCS_ServiceRegistry` in `Core/Runtime/Services/` for interface-driven registration and resolution
- Services can be registered and resolved by interface type without static global singletons or gameplay coupling

## Core event dispatcher

- **Core event dispatcher foundation established** — `CCS_IEvent`, `CCS_IEventDispatcher`, and `CCS_EventDispatcher` in `Core/Runtime/Systems/Events/` for lightweight decoupled communication

## Core runtime update loop

- **Core runtime update loop foundation established** — `CCS_RuntimeUpdateLoop` in `Core/Runtime/Systems/UpdateLoop/` coordinates `CCS_IUpdatable`, `CCS_IFixedUpdatable`, and `CCS_ILateUpdatable` systems

## Core runtime host

- **Runtime host bridge established** — `CCS_RuntimeHost` in `Core/Runtime/Systems/RuntimeHost/` connects Unity lifecycle callbacks to CCS runtime architecture
- Unity `Update` / `FixedUpdate` / `LateUpdate` now drive the pure C# update loop, service registry, and event dispatcher

## Core bootstrap composition

- **Bootstrap installer foundation established** — `CCS_IBootstrapInstaller` and `CCS_BootstrapRunner` in `Core/Runtime/Systems/Bootstrap/` for modular runtime composition into `CCS_RuntimeHost`

## Core runtime prefabs

- **Runtime host prefab established** — `PF_CCS_RuntimeHost` in `Core/Runtime/Prefabs/` is the official scene runtime entry prefab

## Core bootstrap scene

- **Bootstrap scene established** — `SCN_CCS_Bootstrap` in `Core/Runtime/Scenes/` is the official minimal framework startup scene (Build Settings index 0)

## Runtime smoke test

Authoritative path: `Assets/CCS/Framework/Core/Runtime/SmokeTests/`

- **Runtime smoke test established** — `CCS_RuntimeSmokeTestSystem`, `CCS_RuntimeSmokeTestInstaller`, and `CCS_RuntimeSmokeTestBridge`
- Bootstrap scene uses `PF_CCS_RuntimeHost` + optional `CCS_RuntimeSmokeTestBridge` for Play Mode validation
- **Smoke tests isolated from production runtime host** — `CCS_RuntimeHost` is validation-agnostic

## Runtime diagnostics

- **Runtime diagnostics toggle established** — `CCS_RuntimeHost.EnableRuntimeDiagnostics` gates validation behavior
- **Smoke test bridge respects runtime diagnostics** — `CCS_RuntimeSmokeTestBridge` runs only when diagnostics are enabled on the host

## Stability checkpoint (0.2.0)

- **0.2.0 stability checkpoint established** — First stable CCS Core runtime architecture checkpoint
- **[CCS Core Stability Checkpoint 0.2.0](CCS_Core_Stability_Checkpoint_0_2_0.md)** — Verified pipeline, production boundaries, and foundation inventory

## Module contracts (0.3.0)

Authoritative path: `Assets/CCS/Framework/Core/Runtime/Modules/`

- **Module contract foundation established** — `CCS_IModule`, `CCS_IModuleInstaller`, `CCS_IModuleDependencyProvider`, `CCS_ModuleMetadata`, and `CCS_ModuleState`
- **Future modules** will implement `CCS_IModule` and optionally `CCS_IModuleDependencyProvider` for declared dependency IDs

## Module base class (0.3.1)

- **Module base class foundation established** — `CCS_ModuleBase` in `Core/Runtime/Modules/Base/` centralizes lifecycle state transitions and overridable hooks

## Module installer base (0.3.2)

- **Module installer base foundation established** — `CCS_ModuleInstallerBase` in `Core/Runtime/Modules/Base/` plugs modules into `CCS_BootstrapRunner` with shared validation and install hooks

## Module registry (0.3.3)

- **Module registry foundation established** — `CCS_IModuleRegistry` and `CCS_ModuleRegistry` in `Core/Runtime/Modules/Registry/` for manual module registration and lookup

## Module host integration (0.3.4)

- **Module host registry integration established** — `CCS_ModuleHost` owns module registry; `CCS_RuntimeHost` exposes `ModuleHost`; installers register modules after successful install

## Installer smoke test alignment (0.3.5)

- **Module installer smoke test established** — `CCS_SmokeTestModule` and `CCS_SmokeTestModuleInstaller` validate installer hooks, module host registration, and duplicate module ID blocking

## Module lifecycle state (0.3.6)

- **Module lifecycle state tracking established** — `CCS_ModuleLifecycleState` on modules; installer/host coordinate install outcomes without replacing `CCS_Result`

## Module uninstall (0.3.7)

- **Module uninstall foundation established** — `UninstallModule`, `UnregisterInstalledModule`, and `CCS_ModuleBase` uninstall flow with lifecycle `Uninstalling` / `Uninstalled` / `Failed`

## Core platform baseline (0.3.8)

- **Core platform architecture documented** — [CCS Core Platform Architecture](../Core/Documentation/CCS_Core_Platform_Architecture.md) (authoritative 0.3.8 baseline)

## Template readiness (0.3.9)

- **Core template readiness checklist** — [CCS Core Template Readiness Checklist](../Core/Documentation/CCS_Core_Template_Readiness_Checklist.md) for GitHub template and game project branching

## Final validation (0.3.10)

- **Core platform final validation pass** — [CCS Core Final Validation 0.3.10](../Core/Documentation/CCS_Core_Final_Validation_0_3_10.md); version/meta/folder/runtime/git checks before 0.4.x

## Purpose

This folder is the **source of truth** for framework standards, architecture notes, and setup guides. Implementation in `Core/` and `Modules/` must align with documents published here.

## Adding documentation

New standards or architecture documents should be added as markdown in this folder and referenced from the root `README.md` when they reach milestone status.
