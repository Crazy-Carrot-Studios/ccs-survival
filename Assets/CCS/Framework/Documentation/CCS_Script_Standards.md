# CCS Script Standards

**Version:** 0.0.5  
**Status:** Authoritative — all CCS framework and module scripts must follow these standards before implementation begins.

---

## 1. Purpose

The CCS framework enforces strict scripting standards so that code remains scalable, readable, and maintainable across projects and teams.

**Goals:**

- **Scalability** — Systems can grow without becoming tangled or fragile.
- **Readability** — Any developer can open a script and understand its role quickly.
- **AAA maintainability** — Long-lived codebases stay consistent across years of development.
- **Modularity** — Runtime, Editor, and module boundaries stay clear and compile-safe.

Standards are defined here **before** runtime systems are implemented so every future script starts from the same foundation.

---

## 2. Required CCS Header Format

Every CCS script must include a structured header block **immediately below `using` statements**.

**Do not use XML documentation summaries** (`/// <summary>`). Use the CCS header format only.

### Header fields

| Field | Description |
|-------|-------------|
| **SCRIPT** | Script or class name |
| **CATEGORY** | Framework area (e.g. Core, Module, Editor) |
| **PURPOSE** | What the script does in one or two sentences |
| **PLACEMENT** | Where the script lives and where it is attached in Unity |
| **AUTHOR** | Author name |
| **CREATED** | Creation date (YYYY-MM-DD) |
| **NOTES** | Optional constraints, dependencies, or warnings |

### Example

```csharp
using UnityEngine;

// =============================================================================
// SCRIPT: CCSCharacterController
// CATEGORY: Core / Runtime
// PURPOSE: Handles player movement and grounding for CCS character systems.
// PLACEMENT: Attach to the player root GameObject in gameplay scenes.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Depends on CCS.Core.Runtime. No Editor references.
// =============================================================================

public class CCSCharacterController : MonoBehaviour
{
}
```

### Rules

- Header appears **below** all `using` statements.
- Use the `// =============================================================================` delimiter style for visual consistency.
- **No XML summaries** — CCS headers are the single source of script-level documentation at the file top.

---

## 3. Region Organization Order

Scripts must organize code with `#region` blocks in this **fixed order**:

```csharp
#region Variables
#region Unity Callbacks
#region Public Methods
#region Private Methods
#region Properties
```

### Why this order

- **Consistent navigation** — Collapse regions in any CCS script and find members in the same place.
- **Predictable architecture** — Callbacks stay separate from business logic; public API is easy to scan.

Omit empty regions only when a region truly has no members (do not add placeholder code).

---

## 4. Serialized Field Standards

Inspector-facing fields must be clear for designers and other developers.

### Requirements

- Use `[Header("")]` to group related fields in the Inspector.
- Use `[Tooltip("")]` on every `[SerializeField]` to explain behavior.
- Use `//` comments above private serialized fields when behavior is non-obvious.

### Example

```csharp
[Header("Movement")]
[Tooltip("Maximum horizontal movement speed in meters per second.")]
[SerializeField] private float maximumMovementSpeed = 5f;

// Ground check distance used when resolving isGrounded.
[SerializeField] private float groundCheckDistance = 0.2f;
```

### Goals

- **Inspector readability** — Designers understand fields without reading source.
- **Designer usability** — Tooltips and headers reduce misconfiguration in production scenes.

---

## 5. Naming Standards

Names must be explicit and self-documenting.

### Rules

| Rule | Requirement |
|------|-------------|
| Abbreviations | Avoid unless industry-standard (e.g. `UI`, `ID`) |
| Single-letter variables | Not allowed (except loop indices `i`, `j` in tight loops) |
| Methods | Verb-led, descriptive (`InitializeInventory`, `TryEquipItem`) |
| Events | Clear subject + action (`OnInventoryChanged`, `OnPlayerDied`) |
| Booleans | Prefix with `is`, `has`, `can`, `should` (`isGrounded`, `hasSaveData`) |

### Examples

```csharp
private bool isGrounded;
private float currentMovementSpeed;

public event Action OnInventoryChanged;

private void TryApplyMovementInput() { }
```

---

## 6. Logging Standards

Logging must be consistent, searchable, and controllable.

### Rules

- Use **`CCS_Logger`** for all CCS framework and module logging (`Assets/CCS/Framework/Core/Runtime/Utilities/Logging/CCS_Logger.cs`).
- Pass a **short, clear category** (e.g. `"Core"`, `"Inventory"`, `"Character Controller"`).
- **Normal logs** use `CCS_Logger.Log` with an optional debug boolean (`isEnabled`) so verbose output can be toggled off.
- **Warnings and errors** use `CCS_Logger.LogWarning` / `CCS_Logger.LogError` and **always output** (not gated by debug flags).
- Do not call raw `Debug.Log` directly in CCS framework code unless there is a documented exception.

### Format

All CCS logs use centralized formatting:

```text
[CCS {category}] {message}
```

Examples:

```text
[CCS Core] Initialized
[CCS Inventory] Item added
```

### Example

```csharp
CCS_Logger.Log("Character Controller", "Initialized", enableDebugLogs);

CCS_Logger.LogWarning("Character Controller", "Missing required Rigidbody.");
CCS_Logger.LogError("Character Controller", "Configuration invalid. Disabling component.");
```

---

## 7. Validation Standards

Validation must be consistent and centralized.

### Rules

- Use **`CCS_Validation`** for reusable validation logic (`Assets/CCS/Framework/Core/Runtime/Utilities/Validation/CCS_Validation.cs`).
- Avoid duplicated null/empty/collection checks across framework and module code.
- Prefer centralized helpers (`IsObjectValid`, `IsStringValid`, `IsCollectionValid`) and `CCS_Result` returns (`ValidateObject`, `ValidateString`) over ad-hoc inline validation.
- Do not throw exceptions for expected validation failures; return `CCS_Result.Failure` with a clear message.

### Example

```csharp
CCS_Result objectResult = CCS_Validation.ValidateObject(inventoryService, "Inventory Service");
if (!objectResult.IsSuccess)
{
    return objectResult;
}

if (!CCS_Validation.IsCollectionValid(items))
{
    return CCS_Result.Failure("Items collection is empty.");
}
```

---

## 8. Runtime vs Editor Separation

CCS enforces compile-time separation between player/build code and Editor-only tooling.

### Rules

- **Runtime assemblies** (`CCS.Core.Runtime`, module Runtime asmdefs) must **never** reference `UnityEditor`.
- **Editor assemblies** (`CCS.Core.Editor`, module Editor asmdefs) may reference Runtime and `UnityEditor`.
- Editor utilities, custom inspectors, and menu items live only under `Editor/` folders with Editor asmdefs.

Violations break builds and must be caught in code review and CI.

---

## 9. Event-Driven Architecture

Prefer loose coupling over direct hard references between unrelated systems.

### Guidelines

- Avoid tight coupling where System A directly calls private methods on System B across module boundaries.
- Favor **interfaces**, **events**, and **services** for cross-system communication.
- Core provides extension points; modules subscribe and publish without owning each other's internals.

### Example patterns

- `OnInventoryChanged` events for UI and crafting listeners
- Service interfaces registered at bootstrap (`IInventoryService`, `ISaveService`)
- ScriptableObject or data-driven configuration instead of scene-hardcoded references

---

## 10. Service Architecture Standards

Service communication must stay interface-driven and lightweight.

### Rules

- Prefer **interfaces** over concrete hard references between systems and modules.
- Services should implement **`CCS_IService`**.
- Systems and modules should request dependencies through **`CCS_IServiceRegistry`** where possible instead of direct singletons or scene lookups.
- The service registry is a **registration and lookup utility** — not a gameplay god object, lifecycle manager, or global MonoBehaviour.
- Do not add a static global `CCS_ServiceRegistry` instance in Core; bootstrap code owns registry lifetime.
- Do not call `Initialize()` or `Shutdown()` automatically from the registry in this foundation phase.

### Example

```csharp
CCS_IServiceRegistry serviceRegistry = new CCS_ServiceRegistry(enableDebugLogs: true);

if (serviceRegistry.TryGetService<IInventoryService>(out IInventoryService inventoryService))
{
    inventoryService.Initialize();
}
```

---

## 11. Event Architecture Standards

Event communication must stay decoupled and lightweight.

### Rules

- Favor **events** over direct hard references when systems should not know each other's concrete types.
- Systems should communicate through **`CCS_IEventDispatcher`** and event contracts implementing **`CCS_IEvent`** where appropriate.
- Avoid **circular dependencies** between publishers and subscribers; prefer one-way event flows.
- Events should remain **lightweight data contracts** (timestamp + payload fields), not gameplay managers or MonoBehaviours.
- Do not add a static global `CCS_EventDispatcher` singleton in Core; bootstrap code owns dispatcher lifetime.
- No async, threading, event buffering, or sticky events in the foundation phase.

### Example

```csharp
CCS_IEventDispatcher eventDispatcher = new CCS_EventDispatcher(enableDebugLogs: true);
eventDispatcher.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
eventDispatcher.Dispatch(new InventoryChangedEvent(DateTime.UtcNow, itemId));
```

---

## 12. Runtime Update Loop Standards

Frame scheduling must stay interface-driven and free of unnecessary MonoBehaviours.

### Rules

- Systems that need per-frame updates should implement **`CCS_IUpdatable`** and register with **`CCS_RuntimeUpdateLoop`**.
- Physics-timed systems should implement **`CCS_IFixedUpdatable`**.
- Camera, follow-up, or post-update systems should implement **`CCS_ILateUpdatable`**.
- Avoid unnecessary **MonoBehaviours** for pure framework logic; use **`CCS_RuntimeHost`** as the thin bridge to Unity's player loop.
- Do not add a static global `CCS_RuntimeUpdateLoop` singleton in Core; bootstrap code owns loop lifetime.
- No priority ordering, exception handling wrappers, or list mutation during tick iteration in the foundation phase.

### Example

```csharp
CCS_RuntimeUpdateLoop updateLoop = new CCS_RuntimeUpdateLoop(enableDebugLogs: true);
updateLoop.RegisterUpdatable(movementSystem);
updateLoop.Tick(Time.deltaTime);
```

---

## 13. Runtime Host Standards

The runtime host is the **only intentional MonoBehaviour orchestration layer** in Core at this phase.

### Rules

- **MonoBehaviours should remain thin bridges** — delegate to pure C# systems, services, and dispatchers.
- **Framework logic should stay in pure C#** (`CCS_RuntimeUpdateLoop`, registries, utilities) wherever possible.
- Avoid **scene-driven architecture** for core systems; scenes host the bridge, not business logic.
- **`CCS_RuntimeHost`** is orchestration only — no gameplay logic, singletons, `DontDestroyOnLoad`, or automatic module loading.
- External bootstrap code registers services, events, and updatable systems through host properties after `Awake`.

### Example

```csharp
CCS_RuntimeHost runtimeHost = GetComponent<CCS_RuntimeHost>();
runtimeHost.ServiceRegistry.RegisterService(myService);
runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(mySystem);
```

---

## 14. Bootstrap Composition Standards

Runtime composition must stay modular and explicitly controlled.

### Rules

- Systems and modules should install through **`CCS_IBootstrapInstaller`** implementations registered with **`CCS_BootstrapRunner`**.
- Avoid **hardcoded scene wiring** for core framework registration; use installers and host properties.
- Bootstrap installers should remain **lightweight orchestration** — register services, events, and updatable systems only.
- **`CCS_RuntimeHost`** does not auto-run installers in the foundation phase; bootstrap code calls `BootstrapRunner.Run` when ready.
- No reflection scanning, scene searching, ScriptableObject installers, or dependency ordering in this phase.

### Example

```csharp
runtimeHost.BootstrapRunner.RegisterInstaller(new CoreBootstrapInstaller());
runtimeHost.BootstrapRunner.Run(runtimeHost);
```

---

## 15. Prefab Standards

Framework prefabs must stay minimal and reusable.

### Rules

- Framework prefabs use the **`PF_CCS_`** prefix (e.g. `PF_CCS_RuntimeHost`).
- **`PF_CCS_RuntimeHost`** is the standard runtime entry prefab for bootstrap and gameplay scenes.
- Prefabs should remain **minimal and modular** — only required components, no gameplay logic bundles.
- Do not embed installers, `DontDestroyOnLoad`, or module implementations in foundation prefabs unless explicitly documented.

### Example

```text
PF_CCS_RuntimeHost
  └── Transform
  └── CCS_RuntimeHost (enableDebugLogs: false)
```

---

## 16. Scene Standards

Framework scenes must stay minimal and purpose-specific.

### Rules

- Framework scenes use the **`SCN_CCS_`** prefix (e.g. `SCN_CCS_Bootstrap`).
- **Bootstrap scenes** should remain minimal — runtime host prefab only at this phase.
- **Gameplay content** must not be added to the core framework bootstrap scene.
- Project/game scenes live under `Assets/CCS/Shared/Scenes/` or game-specific paths, not in `Framework/Core/Runtime/Scenes/`.

### Example

```text
SCN_CCS_Bootstrap
  └── PF_CCS_RuntimeHost (prefab instance)
```

---

## 17. Testing Standards

Framework tests must validate architecture without gameplay coupling.

### Rules

- **Smoke tests** validate framework architecture only (bootstrap, update loop, installers).
- Smoke tests should remain **lightweight and isolated** — no services, events, or scene loading required.
- Runtime validation must avoid **gameplay coupling**; use `CCS.Core.Tests` namespace and dedicated smoke test types.
- Smoke test sources compile inside **`CCS.Core.Runtime`** (`Core/Runtime/SmokeTests/`) so installers can reference `CCS_RuntimeHost` without circular asmdef references.
- **Production runtime classes** must not contain validation-only orchestration (no smoke test hooks in `CCS_RuntimeHost`).
- **Smoke tests** stay isolated behind **`CCS_RuntimeSmokeTestBridge`** — temporary validation infrastructure on the same GameObject as the host.
- **Runtime validation bridges** are optional MonoBehaviour layers, not part of the production host contract.

### Example

```text
Play SCN_CCS_Bootstrap → expect:
[CCS SmokeTest] Runtime smoke test initialized
[CCS SmokeTest] Smoke test installer completed
[CCS SmokeTest] Runtime update loop tick confirmed
```

---

## 18. Runtime Diagnostics Standards

Diagnostics must be explicit and never silently enabled in production runtime paths.

### Rules

- **Diagnostics must be explicitly gated** via `CCS_RuntimeHost.EnableRuntimeDiagnostics`.
- **Production runtime** must not silently run validation systems; diagnostic bridges check the host flag before executing.
- **Diagnostic bridges** (e.g. `CCS_RuntimeSmokeTestBridge`) must respect runtime host diagnostics settings and exit early when disabled.
- `CCS_RuntimeHost` exposes diagnostics permission only — it does not run validation logic itself.

### Example

```csharp
if (!runtimeHost.EnableRuntimeDiagnostics)
{
    return;
}
```

---

## 19. Module Contract Standards

Future framework modules must align with Core module contracts in `Core/Runtime/Modules/`.

### Rules

- **Modules must use `CCS_IModule`** — Extends `CCS_ISystem` with metadata, lifecycle state, and host-scoped install/uninstall.
- **Modules should expose metadata** via `CCS_ModuleMetadata` (module ID, display name, version, description).
- **Modules should install through bootstrap installers** implementing `CCS_IModuleInstaller` (extends `CCS_IBootstrapInstaller`).
- **Modules must avoid hard dependencies when possible** — Use `CCS_IModuleDependencyProvider` to declare required module IDs only; resolution is a future milestone.
- **Modules should communicate** through existing contracts: services (`CCS_IService`), events (`CCS_IEvent`), and update interfaces (`CCS_IUpdatable`, etc.).

### Do not (module foundation phase)

- No gameplay module implementations in Core
- No module manager singletons, ScriptableObject module assets, or automatic discovery/reflection scanning
- No dependency injection frameworks in Core

### Example

```csharp
public interface CCS_IModule : CCS_ISystem
{
    CCS_ModuleMetadata Metadata { get; }
    CCS_ModuleState ModuleState { get; }
    CCS_Result Install(CCS_RuntimeHost runtimeHost);
    CCS_Result Uninstall(CCS_RuntimeHost runtimeHost);
}
```

### Module base class

- **Future modules may inherit `CCS_ModuleBase`** instead of implementing `CCS_IModule` directly.
- **Module-specific behavior** belongs in protected override methods (`OnInitialize`, `OnInstall`, `OnUninstall`, `OnShutdown`).
- **Base class owns common lifecycle state transitions** (`CCS_ModuleState`); do not duplicate state logic in subclasses.
- **Modules must not bypass lifecycle methods** — use `Initialize`, `Install`, `Uninstall`, and `Shutdown` on the base class.

### Module installer base class

- **Module installers should inherit `CCS_ModuleInstallerBase`** where appropriate instead of implementing `CCS_IModuleInstaller` directly.
- **Installers should remain thin bootstrap adapters** — validate host/module, delegate to `CCS_IModule.Install`, optional before/after hooks only.
- **Module-specific setup** belongs in the module (`CCS_ModuleBase` overrides) or installer hooks (`OnBeforeInstall`, `OnAfterInstall`), not in installer managers.
- **Installers must not become gameplay managers** — no singletons, discovery, or gameplay orchestration in installer types.

### Module registry

- **Register modules manually** via `CCS_ModuleRegistry.RegisterModule` using `CCS_ModuleMetadata.ModuleId`.
- **Use `CCS_Result`** for register/unregister; use TryGet lookups — no exceptions for validation failures.
- **Registry instances are not singletons** — bootstrap or host wiring owns the instance; no auto-discovery.

### Module uninstall

- **Uninstall through `CCS_ModuleHost.UninstallModule(runtimeHost, moduleId)`** — calls `CCS_IModule.Uninstall`, then `UnregisterInstalledModule` on success.
- **Lifecycle** — `Uninstalling` during uninstall; `Uninstalled` on success; `Failed` on failure.
- **Missing or duplicate uninstall** — return `CCS_Result` failures with `CCS_Logger` warnings; no exceptions.

---

## 20. Assembly Definition Philosophy

CCS is modular by design. Assembly definitions define compile boundaries.

### Principles

- **Modular compile boundaries** — Each Core area and module compiles independently.
- **Future package scalability** — Modules can become UPM packages without rewriting Core.
- **Separation of systems** — Runtime never pulls Editor; modules depend on Core, not the reverse.

Current foundation:

- `CCS.Core.Runtime` — Player and build-safe Core code
- `CCS.Core.Editor` — Editor-only Core tooling (references Runtime)

Module asmdefs will follow the same Runtime / Editor split when implemented.

---

## 21. Future Standards Expansion

This document is the first authoritative standards milestone. Future sections will cover:

- **Unit testing** — Edit Mode and Play Mode test conventions
- **Networking** — Authority, replication, and RPC naming
- **Save systems** — Serialization, versioning, migration
- **Performance profiling** — Budgets, markers, and hot-path rules
- **Multiplayer readiness** — Determinism, session lifecycle, and server/client boundaries

Updates will be versioned with framework milestones and recorded in `Assets/CCS/Framework/Documentation/`.

---

*Crazy Carrot Studios — CCS Framework*
