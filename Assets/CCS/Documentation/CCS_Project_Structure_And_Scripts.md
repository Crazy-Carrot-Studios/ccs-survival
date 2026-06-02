# CCS Survival — Project Folder Structure & Script Reference

**Repository:** [ccs-survival](https://github.com/Crazy-Carrot-Studios/ccs-survival)  |  **Version:** 1.1.3  |  **Author:** James Schilz  |  **Generated:** 2026-05-28

Complete inventory of **600** C# scripts under `Assets/CCS/`, with folder layout and one-line purpose from each script's CCS file header.

## Table of contents

1. [Repository & Unity layout](#repository--unity-layout)
2. [Folder tree (summary)](#folder-tree-summary)
3. [Module folder convention](#module-folder-convention)
4. [Scripts by zone](#scripts-by-zone)
   - [Framework](#zone-framework)
   - [Modules (gameplay)](#zone-modules)
   - [Survival (project shell)](#zone-survival)
   - (Shared / Database / Tests have no `.cs` scripts today — assets and placeholders only)

---

## Repository & Unity layout

| Path | What lives here |
|------|-----------------|
| `Assets/CCS/` | All CCS survival code, content, profiles, and scenes |
| `Assets/Settings/` | URP / render pipeline ScriptableObjects |
| `Packages/` | UPM dependencies (`manifest.json`) |
| `ProjectSettings/` | Unity player settings, `bundleVersion`, input, graphics |
| `Builds/` | Verified Windows builds (`CCS_Survival_1.1.x_Windows/`) — gitignored |
| `Documentation/` | Repo-level docs (milestones, etc.) |
| `.cursor/rules/` | Cursor AI workspace rules |

### `Assets/CCS/` zones

| Zone | Path | Role |
|------|------|------|
| **Framework** | `Assets/CCS/Framework/` | Reusable Core Platform: `CCS_RuntimeHost`, service registry, modules, bootstrap, diagnostics |
| **Modules** | `Assets/CCS/Modules/` | Gameplay systems (inventory, crafting, sleep, building, …) — one folder per feature |
| **Survival** | `Assets/CCS/Survival/` | Game project shell: bootstrap scene, default profiles, service composition, editor validation/build |
| **Shared** | `Assets/CCS/Shared/` | Shared art, scenes, shaders (not tied to one module) |
| **Database** | `Assets/CCS/Database/` | Authoring placeholders for items, recipes, loot (future pipeline) |
| **Documentation** | `Assets/CCS/Documentation/` | Architecture, standards, this file |
| **Tests** | `Assets/CCS/Tests/` | Unity Test Framework assemblies |

---

## Folder tree (summary)

```text
ccs-survival/
├── Assets/
│   ├── CCS/
│   │   ├── Database/          # Item/recipe/loot authoring placeholders
│   │   ├── Documentation/     # Cross-cutting docs
│   │   ├── Framework/
│   │   │   ├── Core/          # Runtime + Editor: bootstrap, services, modules
│   │   │   ├── Documentation/
│   │   │   ├── Modules/       # Framework-level modules (if any)
│   │   │   ├── Shared/        # Framework shared assets
│   │   │   └── Tests/
│   │   ├── Modules/           # 24 gameplay modules (see below)
│   │   ├── Shared/            # Game-shared scenes/art
│   │   ├── Survival/
│   │   │   ├── Content/       # Items, prefabs, primitive definitions
│   │   │   ├── Documentation/
│   │   │   ├── Editor/        # Validation, build runner, bootstrap setup
│   │   │   ├── Input/         # Input Actions asset
│   │   │   ├── Prefabs/       # Player, bootstrap root
│   │   │   ├── Profiles/      # Default ScriptableObject tuning per module
│   │   │   ├── Runtime/       # Composition, player, diagnostics
│   │   │   └── Scenes/        # SCN_CCS_Survival_Bootstrap
│   │   └── Tests/
│   └── Settings/              # URP assets
├── Packages/
├── ProjectSettings/
└── Builds/                    # gitignored
```

### Gameplay modules under `Assets/CCS/Modules/`

| Module | Milestone focus |
|--------|-----------------|
| `Building/` | Primitive shelter placement, snap, save restore |
| `CharacterController/` | Movement, camera, stamina integration |
| `Combat/` | Melee hunting, wildlife damage |
| `Cooking/` | Campfire stations, consumable food |
| `Crafting/` | Hand / campfire / workbench recipes |
| `EnvironmentEffects/` | Temperature, exposure, equipment modifiers |
| `Equipment/` | Slots, durability, environmental gear |
| `Gathering/` | World nodes (tree, rock, bush) |
| `Hotbar/` | Hotbar selection (foundation) |
| `Interaction/` | Raycast interactables |
| `Inventory/` | Slots, stacks, item definitions |
| `PlayerDeath/` | Starvation/dehydration death, respawn |
| `Playtesting/` | Manual checklist HUD (F-keys) |
| `SaveLoad/` | Per-system save hooks (inventory, equipment, …) |
| `SaveSystem/` | Unified JSON save (`CCS_SaveData`) |
| `Shelter/` | Sheltered state for sleep/environment |
| `Sleep/` | Bedroll spots, sleep service, respawn assign |
| `Storage/` | World storage crates |
| `SurvivalCore/` | Hunger, thirst, stamina, fatigue stats |
| `TimeOfDay/` | Day/night cycle |
| `UI/` | HUD presenters and layout |
| `Weather/` | Weather state and save |
| `Wildlife/` | AI, harvest carcass |
| `WorldResources/` | Harvestable resources (legacy/alternate gather path) |

---

## Module folder convention

Most modules follow:

```text
Modules/<Name>/
├── Documentation/       # CCS_<Name>_Module.md
├── Editor/
L 
├── Runtime/
L 
│   ├── Data/            # DTOs, save state, enums
│   ├── Definitions/     # ScriptableObject definitions
│   ├── Events/          # Event args + delegates
│   ├── Interactables/   # CCS_IInteractable implementations
│   ├── Profiles/        # Module profile SO (tuning lives in Survival/Profiles often)
│   ├── Services/        # CCS_*Service + runtime bridges
│   ├── Validation/      # Runtime validation utilities
│   └── Testing/         # Dev harnesses
├── CCS.Modules.<Name>.Runtime.asmdef
└── CCS.Modules.<Name>.Editor.asmdef
```

Project tuning assets: `Assets/CCS/Survival/Profiles/<Module>/CCS_Default*.asset`
World content prefabs: `Assets/CCS/Survival/Content/...`

---

## Scripts by zone

---

## Zone: Framework {#zone-framework}

**Script count:** 49

### `Assets/CCS/Framework/Core/Runtime/Data/Messages`

| Script | Purpose |
|--------|---------|
| `CCS_Message` | Serializable framework message for future UI/debug/logging abstraction. |
| `CCS_MessageType` | Classifies framework messages for UI, debug, and logging layers. |

### `Assets/CCS/Framework/Core/Runtime/Data/Results`

| Script | Purpose |
|--------|---------|
| `CCS_CoreErrorCode` | Stable error classification codes for CCS Core operation results. |
| `CCS_Result` | Lightweight immutable non-generic result for CCS operations. |
| `CCS_Result<T>` | Lightweight immutable generic result wrapper for CCS operations. |

### `Assets/CCS/Framework/Core/Runtime/Diagnostics`

| Script | Purpose |
|--------|---------|
| `CCS_CoreDiagnosticsReport` | Aggregated read-only diagnostics snapshot for CCS Core runtime systems. |
| `CCS_ModuleDiagnosticsInfo` | Read-only snapshot of a registered module for diagnostics reports. |
| `CCS_ServiceDiagnosticsInfo` | Read-only snapshot of service registry state for diagnostics reports. |
| `CCS_UpdateLoopDiagnosticsInfo` | Read-only snapshot of update loop registration counts. |

### `Assets/CCS/Framework/Core/Runtime/Modules/Base`

| Script | Purpose |
|--------|---------|
| `CCS_ModuleBase` | Abstract base class for future CCS modules with shared lifecycle behavior. |
| `CCS_ModuleInstallerBase` | Abstract base for module installers that plug into CCS_BootstrapRunner. |

### `Assets/CCS/Framework/Core/Runtime/Modules/Data`

| Script | Purpose |
|--------|---------|
| `CCS_ModuleDependency` | Declares a module or service dependency without resolution logic. |
| `CCS_ModuleDependencyType` | Classifies declared module or service dependency metadata. |
| `CCS_ModuleLifecycleState` | Tracks install-focused module lifecycle state for registry integration. |
| `CCS_ModuleMetadata` | Standard lightweight identity information for future framework modules. |
| `CCS_ModuleState` | Tracks basic module lifecycle state for future framework modules. |

### `Assets/CCS/Framework/Core/Runtime/Modules/Host`

| Script | Purpose |
|--------|---------|
| `CCS_ModuleHost` | Owns module registry and exposes safe module lookup for the runtime host. |

### `Assets/CCS/Framework/Core/Runtime/Modules/Install`

| Script | Purpose |
|--------|---------|
| `CCS_ModuleDependencyValidation` | Validates declared module dependencies against current runtime host state. |
| `CCS_ModuleInstallPlan` | Executes explicit ordered module installs with validation and stop-on-failure. |
| `CCS_ModuleInstallPlanEntry` | Single ordered module installer entry for manual install plans. |

### `Assets/CCS/Framework/Core/Runtime/Modules/Interfaces`

| Script | Purpose |
|--------|---------|
| `CCS_IModule` | Base contract for future CCS framework modules. |
| `CCS_IModuleDependencyProvider` | Allows modules to declare dependency module IDs without resolution logic. |
| `CCS_IModuleInstaller` | Allows modules to install through the existing bootstrap pipeline. |

### `Assets/CCS/Framework/Core/Runtime/Modules/Registry`

| Script | Purpose |
|--------|---------|
| `CCS_IModuleRegistry` | Contract for manual CCS module registration and lookup by module ID. |
| `CCS_ModuleRegistry` | Lightweight runtime registry for manual CCS module registration and lookup. |

### `Assets/CCS/Framework/Core/Runtime/Services/Interfaces`

| Script | Purpose |
|--------|---------|
| `CCS_IService` | Base contract for future CCS framework services. |
| `CCS_IServiceRegistry` | Contract for registering and resolving CCS services by interface type. |

### `Assets/CCS/Framework/Core/Runtime/Services/Registry`

| Script | Purpose |
|--------|---------|
| `CCS_ServiceRegistry` | Lightweight runtime registry for CCS service registration and lookup. |

### `Assets/CCS/Framework/Core/Runtime/SmokeTests/Installers`

| Script | Purpose |
|--------|---------|
| `CCS_RuntimeSmokeTestInstaller` | Installs runtime smoke test system into CCS_RuntimeHost for validation. |
| `CCS_SmokeTestDependentModuleInstaller` | Installer for dependency validation smoke module. |
| `CCS_SmokeTestModuleInstaller` | Validates CCS_ModuleInstallerBase and module host registry integration. |

### `Assets/CCS/Framework/Core/Runtime/SmokeTests/Modules`

| Script | Purpose |
|--------|---------|
| `CCS_SmokeTestDependentModule` | Test-only module declaring a required dependency on the smoke test module. |
| `CCS_SmokeTestModule` | Minimal test-only module validating CCS_ModuleBase lifecycle hooks. |

### `Assets/CCS/Framework/Core/Runtime/SmokeTests/RuntimeBridge`

| Script | Purpose |
|--------|---------|
| `CCS_RuntimeSmokeTestBridge` | Validation-only bridge that runs smoke test installers on CCS_RuntimeHost. |

### `Assets/CCS/Framework/Core/Runtime/SmokeTests/Systems`

| Script | Purpose |
|--------|---------|
| `CCS_RuntimeSmokeTestSystem` | Validates CCS runtime system and update loop execution in Play Mode. |

### `Assets/CCS/Framework/Core/Runtime/Systems/Bootstrap`

| Script | Purpose |
|--------|---------|
| `CCS_BootstrapRunner` | Orchestrates bootstrap installers into the CCS runtime host. |

### `Assets/CCS/Framework/Core/Runtime/Systems/Bootstrap/Interfaces`

| Script | Purpose |
|--------|---------|
| `CCS_IBootstrapInstaller` | Defines how services and systems install into the CCS runtime host. |

### `Assets/CCS/Framework/Core/Runtime/Systems/Events`

| Script | Purpose |
|--------|---------|
| `CCS_EventDispatcher` | Lightweight runtime dispatcher for decoupled CCS event communication. |

### `Assets/CCS/Framework/Core/Runtime/Systems/Events/Interfaces`

| Script | Purpose |
|--------|---------|
| `CCS_IEvent` | Base contract for all CCS runtime events. |
| `CCS_IEventDispatcher` | Contract for subscribing to and dispatching CCS runtime events. |

### `Assets/CCS/Framework/Core/Runtime/Systems/Interfaces`

| Script | Purpose |
|--------|---------|
| `CCS_IFixedUpdatable` | Defines fixed-timestep update contracts for physics-aligned systems. |
| `CCS_ILateUpdatable` | Defines late-update contracts for systems that run after standard updates. |
| `CCS_ISystem` | Base contract for future CCS framework systems. |
| `CCS_IUpdatable` | Defines update-driven systems for per-frame Tick execution. |

### `Assets/CCS/Framework/Core/Runtime/Systems/RuntimeHost`

| Script | Purpose |
|--------|---------|
| `CCS_RuntimeHost` | Thin Unity MonoBehaviour bridge into CCS runtime architecture. |

### `Assets/CCS/Framework/Core/Runtime/Systems/UpdateLoop`

| Script | Purpose |
|--------|---------|
| `CCS_RuntimeUpdateLoop` | Coordinates Tick, FixedTick, and LateTick for registered CCS systems. |

### `Assets/CCS/Framework/Core/Runtime/Utilities/Logging`

| Script | Purpose |
|--------|---------|
| `CCS_Logger` | Centralized CCS log formatting and output helpers. |

### `Assets/CCS/Framework/Core/Runtime/Utilities/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_CoreValidation` | Centralized validation helpers for CCS Core runtime systems. |
| `CCS_Validation` | Backward-compatible validation helpers delegating to CCS_CoreValidation. |

---

## Zone: Modules {#zone-modules}

**Script count:** 480

### Module: `Building`

#### `Assets/CCS/Modules/Building/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_BuildingBootstrapSetup` | Creates default profile, test definitions, and bootstrap gameplay wiring. |
| `CCS_BuildingProgressionBootstrapSetup` | Creates tier-1 primitive building definitions, recipes, and bootstrap wiring. |
| `CCS_BuildingValidationMenu` | Menu entry for building validation through the central pipeline. |
| `CCS_BuildingValidationRegistration` | Registers building validator with the survival validation pipeline. |
| `CCS_BuildingValidationValidator` | Validates building module folders, asmdefs, profile, definitions, and wiring. |

#### `Assets/CCS/Modules/Building/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_BuildingInstance` | Represents a placed building structure instance in the world. |
| `CCS_BuildingInstanceSaveRecord` | Serializable placed instance record for building persistence restore. |
| `CCS_BuildingPieceCategory` | Tier-1 primitive building categories for survival progression. |
| `CCS_BuildingPieceSnapshot` | Read-only building piece snapshot with placement placeholders. |
| `CCS_BuildingPlacementSnapshot` | Read-only placement mode snapshot for HUD and debug tooling. |
| `CCS_BuildingPlacementState` | Mutable runtime placement mode state owned by CCS_BuildingPlacementService. |
| `CCS_BuildingRecipe` | Serializable primitive building recipe for progression placement. |
| `CCS_BuildingRecipePlacementRules` | Placement restrictions for a building recipe category. |
| `CCS_BuildingRecipeRequiredItem` | Serializable inventory cost entry for a building recipe. |
| `CCS_BuildingSaveData` | Versioned save payload for global building catalog and placed instances. |
| `CCS_BuildingShelterContribution` | Runtime shelter protection contribution from a placed building instance. |
| `CCS_BuildingState` | Mutable runtime building catalog state owned by CCS_BuildingService. |

#### `Assets/CCS/Modules/Building/Runtime/Definitions`

| Script | Purpose |
|--------|---------|
| `CCS_BuildingCostEntry` | Serializable build cost entry referencing inventory item definitions. |
| `CCS_BuildingPieceDefinition` | ScriptableObject identity and metadata for a buildable structure piece. |
| `CCS_BuildingPieceType` | Enumerates building piece categories for structure definitions. |

#### `Assets/CCS/Modules/Building/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_BuildingEventArgs` | Event payload for building definition registration and state changes. |
| `CCS_BuildingEvents` | Event delegate definitions for building catalog and placement lifecycle. |
| `CCS_BuildingPlacementEventArgs` | Event payload for building placement lifecycle. |
| `CCS_BuildingPlacementFailedEventArgs` | Event payload for failed building placement attempts. |
| `CCS_BuildingProgressionEventArgs` | Event payload for building progression recipe and placement notifications. |
| `CCS_BuildingProgressionEvents` | Delegate contracts for building progression recipe events. |
| `CCS_BuildingShelterContributionsChangedEventArgs` | Event payload when building shelter contributions are recalculated. |

#### `Assets/CCS/Modules/Building/Runtime/Placement`

| Script | Purpose |
|--------|---------|
| `CCS_BuildingInstanceVisualFactory` | Spawns primitive visuals for placed and restored building instances. |
| `CCS_BuildingPlacementPreview` | Development-only primitive preview for active building placement. |

#### `Assets/CCS/Modules/Building/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_BuildingProfile` | Tuning profile for building feature flags and startup definition registration. |
| `CCS_BuildingProgressionProfile` | Tier-1 primitive building progression recipes and enabled piece catalog. |

#### `Assets/CCS/Modules/Building/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_BuildingDefinitionLookup` | Resolves building piece definition IDs for restore and validation. |
| `CCS_BuildingPlacementService` | Build mode orchestration with preview updates and piece placement. |
| `CCS_BuildingRecipeService` | Recipe lookup, placement authorization, and inventory cost handling for building. |
| `CCS_BuildingRuntimeBridge` | Resolves building service from the runtime registry for HUD and tooling. |
| `CCS_BuildingService` | Authoritative building definition catalog and placed instance tracking. |
| `CCS_BuildingShelterRuntimeBridge` | Resolves building service for shelter and environment integrations. |

#### `Assets/CCS/Modules/Building/Runtime/Snap`

| Script | Purpose |
|--------|---------|
| `CCS_BuildingRuntimeSnapPoint` | World-space snap point state for a placed building instance. |
| `CCS_BuildingSnapCompatibilityUtility` | Explicit snap point compatibility rules for placement matching. |
| `CCS_BuildingSnapMatch` | Result payload for snap matching during placement preview. |
| `CCS_BuildingSnapPoint` | Serializable snap point authored on building piece definitions. |
| `CCS_BuildingSnapPointType` | Categories for building snap point compatibility rules. |

#### `Assets/CCS/Modules/Building/Runtime/Testing`

| Script | Purpose |
|--------|---------|
| `CCS_BuildingPersistenceTestHarness` | Development-only save/load verification for placed building instances. |
| `CCS_BuildingPlacementTestHarness` | Development-only harness that places foundation, wall, and roof with snapping. |

#### `Assets/CCS/Modules/Building/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_BuildingPlacementValidationResult` | Result payload for building placement validation and TryPlaceCurrentPiece. |
| `CCS_BuildingPlacementValidationUtility` | Placement validation and inventory cost consumption helpers. |
| `CCS_BuildingProgressionPlacementUtility` | Validates primitive building placement rules against placed instances. |
| `CCS_BuildingValidationUtility` | Profile, definition, and HUD formatting helpers for runtime and editor checks. |

### Module: `CharacterController`

#### `Assets/CCS/Modules/CharacterController/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_CharacterControllerValidationMenu` | Menu entry for character controller validation through the central pipeline. |
| `CCS_CharacterControllerValidationRegistration` | Registers character controller validator with the central validation pipeline. |
| `CCS_CharacterControllerValidationValidator` | Validates module folders, asmdefs, profile asset, and tuning rules. |

#### `Assets/CCS/Modules/CharacterController/Runtime/Camera`

| Script | Purpose |
|--------|---------|
| `CCS_CharacterCameraController` | Applies look input to yaw/pitch and provides follow/look hooks for a camera transform. |
| `CCS_CharacterLookState` | Stores yaw/pitch look orientation for movement facing and camera follow. |

#### `Assets/CCS/Modules/CharacterController/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_CharacterControllerEvents` | Event name constants and delegate contracts for character controller systems. |
| `CCS_CharacterMovementEventArgs` | Event payload for movement, grounding, jump, sprint, crouch, and stamina hooks. |

#### `Assets/CCS/Modules/CharacterController/Runtime/Input`

| Script | Purpose |
|--------|---------|
| `CCS_CharacterInputActionProvider` | Reads CCS_Survival_InputActions Gameplay map and produces input snapshots. |
| `CCS_CharacterInputRuntimeBridge` | Manual/test input provider; gameplay uses CCS_CharacterInputActionProvider. |
| `CCS_CharacterInputSnapshot` | Frame input sample for movement and look (move, look, jump, sprint, crouch). |
| `CCS_ICharacterInputProvider` | Abstracts locomotion and look input for future New Input System wiring. |

#### `Assets/CCS/Modules/CharacterController/Runtime/Movement`

| Script | Purpose |
|--------|---------|
| `CCS_CharacterControllerMotor` | Unity CharacterController locomotion (walk/run/crouch/jump/gravity/slopes). |
| `CCS_CharacterGroundingState` | Ground contact classification for CharacterController movement. |
| `CCS_CharacterMovementInput` | Processed locomotion input consumed by CCS_CharacterControllerMotor. |
| `CCS_CharacterMovementService` | Runtime owner for CharacterController locomotion, look, snapshots, and events. |
| `CCS_CharacterMovementSnapshot` | Read-only movement state snapshot for consumers and diagnostics. |
| `CCS_CharacterMovementState` | High-level locomotion states for character controller movement. |

#### `Assets/CCS/Modules/CharacterController/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_CharacterCameraProfile` | Look sensitivity and pitch clamp tuning for character camera foundation. |
| `CCS_CharacterControllerProfile` | Project tuning profile for character movement, capsule, camera, and stamina hooks. |
| `CCS_CharacterMovementProfile` | Movement tuning and Unity CharacterController capsule settings. |

#### `Assets/CCS/Modules/CharacterController/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_CharacterMovementRuntimeBridge` | Resolves character movement service from the runtime registry. |

#### `Assets/CCS/Modules/CharacterController/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_CharacterControllerValidationUtility` | Runtime-safe validation for character controller profiles and tuning. |

### Module: `Combat`

#### `Assets/CCS/Modules/Combat/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_CombatBootstrapSetup` | Creates combat profile, weapon melee stats, spear equipment, and bootstrap wiring. |
| `CCS_CombatValidationMenu` | Menu entry for combat validation through the central pipeline. |
| `CCS_CombatValidationRegistration` | Registers combat validator on the central survival validation pipeline. |
| `CCS_CombatValidationValidator` | Validates combat module layout, assets, bootstrap wiring, and HUD integration. |

#### `Assets/CCS/Modules/Combat/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_CombatDamageType` | Damage categories used by primitive melee combat resolution. |
| `CCS_CombatHitResult` | Result payload for a primitive melee attack attempt. |
| `CCS_CombatRangeType` | Engagement range categories for primitive melee combat. |

#### `Assets/CCS/Modules/Combat/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_CombatEventArgs` | Event payload for combat hits and wildlife kills. |
| `CCS_CombatEvents` | Delegate types for primitive combat service events. |

#### `Assets/CCS/Modules/Combat/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_CombatProfile` | Tuning profile for primitive melee combat and wildlife hunting rules. |

#### `Assets/CCS/Modules/Combat/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_CombatRuntimeBridge` | Resolves combat and equipment services from the runtime registry. |
| `CCS_CombatService` | Resolves equipped weapons and performs primitive melee wildlife attacks. |

#### `Assets/CCS/Modules/Combat/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_CombatValidationUtility` | Runtime-safe validation for combat profiles and weapon tuning rules. |

### Module: `Cooking`

#### `Assets/CCS/Modules/Cooking/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_CookingBootstrapSetup` | Creates default profile, food items, campfire content, and bootstrap scene wiring. |
| `CCS_CookingValidationMenu` | Menu entry for cooking validation through the central pipeline. |
| `CCS_CookingValidationRegistration` | Registers cooking validator on the central survival validation pipeline. |
| `CCS_CookingValidationValidator` | Validates cooking module folders, asmdefs, profile assets, and bootstrap content. |

#### `Assets/CCS/Modules/Cooking/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_CampfireInstanceState` | Mutable runtime state for one campfire instance tracked by CCS_CampfireService. |
| `CCS_CampfireSnapshot` | Immutable snapshot of campfire instance state for diagnostics. |
| `CCS_CampfireState` | Runtime state for campfire interactable placeholders. |
| `CCS_ConsumableFoodResult` | Represents the outcome of a food consumption attempt. |
| `CCS_CookingFuelType` | Logical fuel categories accepted by primitive campfire cooking recipes. |
| `CCS_CookingRecipe` | Serializable campfire recipe mapping raw items, fuel, and cooked outputs. |
| `CCS_CookingRequest` | Request payload for campfire cooking attempts. |
| `CCS_CookingResult` | Represents the outcome of a cooking attempt. |
| `CCS_CookingStationSaveState` | Serializable campfire station state for unified save persistence. |
| `CCS_CookingStationType` | Identifies world cooking station archetypes for profile and validation rules. |

#### `Assets/CCS/Modules/Cooking/Runtime/Definitions`

| Script | Purpose |
|--------|---------|
| `CCS_CampfireDefinition` | ScriptableObject rules for campfire interactables and cooking timing. |
| `CCS_ConsumableFoodDefinition` | Maps inventory food items to hunger restoration and consume pacing rules. |

#### `Assets/CCS/Modules/Cooking/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_CookingEventArgs` | Event payload for cooking station lifecycle and consumable food notifications. |
| `CCS_CookingEvents` | Event name constants and delegate contracts for cooking systems. |

#### `Assets/CCS/Modules/Cooking/Runtime/Interactables`

| Script | Purpose |
|--------|---------|
| `CCS_CampfireInteractable` | Interactable MonoBehaviour wrapper for campfire light and cook actions. |

#### `Assets/CCS/Modules/Cooking/Runtime/Interaction`

| Script | Purpose |
|--------|---------|
| `CCS_CookingInteractable` | Interaction entry for campfire cooking using CCS_CookingService recipes. |

#### `Assets/CCS/Modules/Cooking/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_CookingProfile` | Tuning profile for campfire cooking recipes, fuel rules, and consumable food. |

#### `Assets/CCS/Modules/Cooking/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_CampfireService` | Tracks campfire state, placement from kits, and cooking orchestration hooks. |
| `CCS_ConsumableFoodService` | Consumes inventory food items and restores hunger through Survival Core. |
| `CCS_CookingRuntimeBridge` | Resolves gameplay services from the runtime registry for cooking systems. |
| `CCS_CookingService` | Registers cooking stations, validates fuel and ingredients, and grants outputs. |

#### `Assets/CCS/Modules/Cooking/Runtime/Stations`

| Script | Purpose |
|--------|---------|
| `CCS_CookingStation` | World cooking station state for campfire cooking without player input logic. |

#### `Assets/CCS/Modules/Cooking/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_CookingValidationUtility` | Runtime-safe validation for cooking profiles, recipes, and campfire content. |

### Module: `Crafting`

#### `Assets/CCS/Modules/Crafting/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_CraftingBootstrapSetup` | Creates test recipes, items, bootstrap harness, and gameplay service wiring. |
| `CCS_CraftingProgressionBootstrapSetup` | Creates primitive progression items, recipes, workbench, and profile wiring. |
| `CCS_CraftingValidationMenu` | Menu entry for crafting validation through the central pipeline. |
| `CCS_CraftingValidationRegistration` | Registers crafting validator with the central validation pipeline. |
| `CCS_CraftingValidationValidator` | Validates crafting module folders, asmdefs, profile asset, and scripts. |

#### `Assets/CCS/Modules/Crafting/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_CraftingProgressionRecipeEntry` | Links a crafting recipe asset to progression unlock tier metadata. |
| `CCS_CraftingQueueEntry` | Represents a queued crafting job for timed crafting support. |
| `CCS_CraftingRequest` | Represents a single crafting attempt request. |
| `CCS_CraftingResult` | Represents the outcome of a crafting attempt. |
| `CCS_CraftingSnapshot` | Read-only crafting state snapshot for queries and future save hooks. |

#### `Assets/CCS/Modules/Crafting/Runtime/Definitions`

| Script | Purpose |
|--------|---------|
| `CCS_CraftingIngredientDefinition` | Serializable ingredient entry referencing inventory item definitions. |
| `CCS_CraftingRecipeDefinition` | ScriptableObject recipe identity, requirements, and outputs. |
| `CCS_CraftingResultDefinition` | Serializable result entry referencing inventory item definitions. |
| `CCS_CraftingStationType` | Station categories required by recipes and runtime station context. |

#### `Assets/CCS/Modules/Crafting/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_CraftingEventArgs` | Event payload for crafting request, progress, completion, and unlock notifications. |
| `CCS_CraftingEvents` | Event name constants and delegate contracts for crafting systems. |
| `CCS_CraftingProgressionEventArgs` | Event payload for crafting progression recipe lifecycle notifications. |
| `CCS_CraftingProgressionEvents` | Delegate contracts for crafting progression recipe lifecycle events. |

#### `Assets/CCS/Modules/Crafting/Runtime/Interactables`

| Script | Purpose |
|--------|---------|
| `CCS_CraftingStationInteractable` | Sets active crafting station context when the player interacts. |

#### `Assets/CCS/Modules/Crafting/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_CraftingProfile` | Tuning profile for hand crafting, queueing, and craft timing rules. |
| `CCS_CraftingProgressionProfile` | Tiered primitive crafting progression recipes grouped by station context. |

#### `Assets/CCS/Modules/Crafting/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_CraftingRecipeService` | Progression recipe lookup, station filtering, and crafting orchestration. |
| `CCS_CraftingRuntimeBridge` | Resolves gameplay services from the runtime registry for crafting. |
| `CCS_CraftingService` | Validates and executes crafting against inventory and station context. |

#### `Assets/CCS/Modules/Crafting/Runtime/Stations`

| Script | Purpose |
|--------|---------|
| `CCS_CraftingStationContext` | Runtime-safe description of the station available for crafting. |

#### `Assets/CCS/Modules/Crafting/Runtime/Testing`

| Script | Purpose |
|--------|---------|
| `CCS_CraftingTestHarness` | Development-only harness that attempts test and primitive recipes when inventory is ready. |

#### `Assets/CCS/Modules/Crafting/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_CraftingValidationUtility` | Runtime-safe validation for crafting profiles, recipes, and station types. |

### Module: `EnvironmentEffects`

#### `Assets/CCS/Modules/EnvironmentEffects/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_EnvironmentEffectsBootstrapSetup` | Creates default profile, bootstrap wiring, and HUD environment display. |
| `CCS_EnvironmentEffectsValidationMenu` | Menu entry for environment effects validation through the central pipeline. |
| `CCS_EnvironmentEffectsValidationRegistration` | Registers environment effects validator with the survival validation pipeline. |
| `CCS_EnvironmentEffectsValidationValidator` | Validates environment effects module folders, asmdefs, profile, and scripts. |

#### `Assets/CCS/Modules/EnvironmentEffects/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_EnvironmentEffectiveValueUtility` | Applies shelter and equipment protection to raw environment values. |
| `CCS_EnvironmentSaveData` | Versioned save payload for global environment simulation state. |
| `CCS_EnvironmentSnapshot` | Read-only environment snapshot with raw, shelter-adjusted, and effective values. |
| `CCS_EnvironmentState` | Mutable environment simulation state owned by CCS_EnvironmentEffectsService. |

#### `Assets/CCS/Modules/EnvironmentEffects/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_EnvironmentEffectsEventArgs` | Event payload carrying environment snapshots and diagnostic messages. |
| `CCS_EnvironmentEffectsEvents` | Event delegate definitions for environment simulation lifecycle. |

#### `Assets/CCS/Modules/EnvironmentEffects/Runtime/Presentation`

| Script | Purpose |
|--------|---------|
| `CCS_EnvironmentEffectsHudPresenter` | Read-only HUD display for ambient temperature, wetness, and exposure. |

#### `Assets/CCS/Modules/EnvironmentEffects/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_EnvironmentEffectsProfile` | Tuning profile for ambient temperature, wetness, and exposure simulation. |

#### `Assets/CCS/Modules/EnvironmentEffects/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_EnvironmentEffectsRuntimeBridge` | Resolves environment service from the runtime registry for HUD and debug tools. |
| `CCS_EnvironmentEffectsService` | Authoritative ambient temperature, wetness, and exposure simulation layer. |

#### `Assets/CCS/Modules/EnvironmentEffects/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_EnvironmentEffectsValidationUtility` | Profile validation helpers for runtime and editor checks. |

### Module: `Equipment`

#### `Assets/CCS/Modules/Equipment/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_EquipmentEnvironmentalBootstrapSetup` | Creates test equipment assets with environmental survival modifiers. |
| `CCS_EquipmentValidationMenu` | Menu entry for equipment validation through the central pipeline. |
| `CCS_EquipmentValidationRegistration` | Registers equipment validator with the central validation pipeline. |
| `CCS_EquipmentValidationValidator` | Validates equipment module folders, asmdefs, profile asset, and scripts. |

#### `Assets/CCS/Modules/Equipment/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_DurabilityState` | Runtime durability tracking for equipped items. |
| `CCS_EquipmentCapacityModifierUtility` | Aggregate inventory slot and carry weight modifiers from equipped items. |
| `CCS_EquipmentEnvironmentalModifierSnapshot` | Aggregated environmental survival modifiers from equipped items. |
| `CCS_EquipmentEnvironmentalModifierUtility` | Aggregate environmental survival modifiers from equipped items. |
| `CCS_EquipmentSaveData` | Root equipment save payload with version and capacity modifier fields. |
| `CCS_EquipmentSaveSlotEntry` | Serializable equipped slot payload for save/load restore. |
| `CCS_EquipmentSnapshot` | Read-only equipment state snapshot for queries and future save hooks. |
| `CCS_EquippedItem` | Runtime equipped item instance referencing inventory item identity. |

#### `Assets/CCS/Modules/Equipment/Runtime/Definitions`

| Script | Purpose |
|--------|---------|
| `CCS_EquipmentItemDefinition` | Equipment-specific extension data referencing inventory item definitions. |
| `CCS_EquipmentItemDefinitionLookup` | Resolves item IDs to equipment definitions for save restore. |
| `CCS_EquipmentSlotType` | Supported equipment slot identifiers for player loadout architecture. |

#### `Assets/CCS/Modules/Equipment/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_EquipmentEventArgs` | Event payload for equip, unequip, change, durability, and failure notifications. |
| `CCS_EquipmentEvents` | Event name constants and delegate contracts for equipment systems. |

#### `Assets/CCS/Modules/Equipment/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_EquipmentProfile` | Tuning profile for future equipment rules and slot policy. |

#### `Assets/CCS/Modules/Equipment/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_EquipmentEnvironmentRuntimeBridge` | Resolves equipment service from the runtime registry for environment systems. |
| `CCS_EquipmentRuntimeBridge` | Resolves gameplay services from the runtime registry for equipment systems. |
| `CCS_PlayerEquipmentService` | Runtime owner of player equipment slots, compatibility, and equipment events. |

#### `Assets/CCS/Modules/Equipment/Runtime/Slots`

| Script | Purpose |
|--------|---------|
| `CCS_EquipmentSlot` | Single equipment slot with compatibility validation and occupied state. |

#### `Assets/CCS/Modules/Equipment/Runtime/Testing`

| Script | Purpose |
|--------|---------|
| `CCS_InventoryEquipmentPersistenceTestHarness` | Development-only harvest/craft/equip/save/load persistence verification harness. |
| `CCS_PrimitiveToolEquipTestHarness` | Verifies primitive tool equip, HUD refresh, capacity, and save/load persistence. |

#### `Assets/CCS/Modules/Equipment/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_EquipmentValidationUtility` | Runtime-safe validation for equipment profiles and definitions. |

### Module: `Gathering`

#### `Assets/CCS/Modules/Gathering/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_GatheringBootstrapSetup` | Creates gathering profile, resource items, and bootstrap test nodes. |
| `CCS_GatheringValidationMenu` | Menu entry for gathering validation through the central pipeline. |
| `CCS_GatheringValidationRegistration` | Registers gathering validator on the central survival validation pipeline. |
| `CCS_GatheringValidationValidator` | Validates gathering module layout, assets, bootstrap wiring, and rewards. |

#### `Assets/CCS/Modules/Gathering/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_GatheringNodeSaveState` | Serializable gathering node state for unified save persistence. |
| `CCS_GatheringNodeType` | Identifies bootstrap gathering node archetypes for reward lookup. |
| `CCS_GatheringResourceType` | Identifies primitive resource categories granted by gathering nodes. |
| `CCS_GatheringResult` | Outcome payload for CCS_GatheringService.TryGatherNode attempts. |
| `CCS_GatheringReward` | Serializable reward entry mapping resource type to inventory item grants. |

#### `Assets/CCS/Modules/Gathering/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_GatheringEventArgs` | Event payload for gathering node gathered, depleted, and respawned notifications. |
| `CCS_GatheringEvents` | Delegate types for gathering node lifecycle events. |

#### `Assets/CCS/Modules/Gathering/Runtime/Interaction`

| Script | Purpose |
|--------|---------|
| `CCS_GatheringInteractable` | Interaction entry point that gathers through CCS_GatheringService. |

#### `Assets/CCS/Modules/Gathering/Runtime/Nodes`

| Script | Purpose |
|--------|---------|
| `CCS_GatheringNode` | Primitive gathering node state for availability, depletion, and respawn. |

#### `Assets/CCS/Modules/Gathering/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_GatheringProfile` | Tuning profile for primitive gathering nodes, rewards, and respawn rules. |

#### `Assets/CCS/Modules/Gathering/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_GatheringRuntimeBridge` | Resolves gathering and inventory services from the runtime registry. |
| `CCS_GatheringService` | Registers gathering nodes, resolves rewards, and grants inventory items. |

#### `Assets/CCS/Modules/Gathering/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_GatheringValidationUtility` | Runtime-safe validation for gathering profiles and reward configuration. |

### Module: `Interaction`

#### `Assets/CCS/Modules/Interaction/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_InteractionValidationMenu` | Menu entry for interaction validation through the central pipeline. |
| `CCS_InteractionValidationRegistration` | Registers interaction validator with the central validation pipeline. |
| `CCS_InteractionValidationValidator` | Validates interaction module folders, asmdefs, profile asset, and scripts. |

#### `Assets/CCS/Modules/Interaction/Runtime/Detection`

| Script | Purpose |
|--------|---------|
| `CCS_InteractionDetectionResult` | Forward-raycast detection output for the interaction scanner. |
| `CCS_InteractionScanner` | Profile-driven forward raycast to find CCS_IInteractable targets. |

#### `Assets/CCS/Modules/Interaction/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_InteractionEventArgs` | Event payload for interaction detection and request outcomes. |
| `CCS_InteractionEvents` | Event name constants and delegate contracts for interaction systems. |

#### `Assets/CCS/Modules/Interaction/Runtime/Interaction`

| Script | Purpose |
|--------|---------|
| `CCS_InteractableBase` | Reusable MonoBehaviour base for future interactable world objects. |
| `CCS_InteractionService` | Runtime owner of interaction scanning, current target, and request flow. |

#### `Assets/CCS/Modules/Interaction/Runtime/Interfaces`

| Script | Purpose |
|--------|---------|
| `CCS_IInteractable` | Contract for world objects the player can detect and interact with. |
| `CCS_IInteractableResultProvider` | Optional interactable contract that reports interaction success or failure. |

#### `Assets/CCS/Modules/Interaction/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_InteractionProfile` | Tuning profile for interaction scan distance and physics layers. |

#### `Assets/CCS/Modules/Interaction/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_InteractionRuntimeBridge` | Resolves interaction service from the runtime registry for player drivers. |

#### `Assets/CCS/Modules/Interaction/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_InteractionValidationUtility` | Runtime-safe validation for interaction profiles and tuning values. |

### Module: `Inventory`

#### `Assets/CCS/Modules/Inventory/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_InventoryValidationMenu` | Menu entry for inventory validation through the central pipeline. |
| `CCS_InventoryValidationRegistration` | Registers inventory validator with the central validation pipeline. |
| `CCS_InventoryValidationValidator` | Validates inventory module folders, asmdefs, profile asset, and scripts. |

#### `Assets/CCS/Modules/Inventory/Runtime/Containers`

| Script | Purpose |
|--------|---------|
| `CCS_IInventoryContainer` | Contract for slot-based inventory storage with stack merge and split support. |
| `CCS_InventoryContainer` | Variable-slot inventory storage with stack merging and partial removal. |

#### `Assets/CCS/Modules/Inventory/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_InventoryCapacityModifierSnapshot` | Lightweight capacity modifier data for future bootstrap composition. |
| `CCS_InventorySaveData` | Root inventory save payload with version and capacity modifier fields. |
| `CCS_InventorySaveSlotEntry` | Serializable inventory slot payload for save/load restore. |
| `CCS_InventorySlot` | Single inventory slot with stack validation and capacity helpers. |
| `CCS_InventorySnapshot` | Read-only inventory state snapshot for queries and future save hooks. |
| `CCS_ItemStack` | Quantity of a single item definition held in an inventory slot. |

#### `Assets/CCS/Modules/Inventory/Runtime/Definitions`

| Script | Purpose |
|--------|---------|
| `CCS_DamageType` | Damage category placeholder for future combat systems. |
| `CCS_ItemCategory` | High-level item classification for inventory data architecture. |
| `CCS_ItemDefinition` | ScriptableObject identity and stacking rules for inventory items. |
| `CCS_ItemDefinitionLookup` | Resolves stable item IDs to CCS_ItemDefinition assets for save restore. |
| `CCS_ItemGameplayKind` | Distinguishes generic items from tools and weapons for progression systems. |
| `CCS_ItemToolType` | Lightweight tool identity for harvest requirement matching. |
| `CCS_RangeType` | Engagement range placeholder for future combat systems. |
| `CCS_ToolArchetype` | Stable tool archetype identity for harvesting and crafting progression. |
| `CCS_ToolTier` | Technology tier for tool effectiveness in future harvesting rules. |
| `CCS_WeaponArchetype` | Stable weapon archetype identity for future combat systems. |
| `CCS_WeaponType` | High-level weapon behavior placeholder for future combat systems. |

#### `Assets/CCS/Modules/Inventory/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_InventoryEventArgs` | Event payload for inventory add, remove, change, and full notifications. |
| `CCS_InventoryEvents` | Event name constants and delegate contracts for inventory systems. |

#### `Assets/CCS/Modules/Inventory/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_InventoryProfile` | Tuning profile for player inventory slot count and future weight limits. |

#### `Assets/CCS/Modules/Inventory/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_PlayerInventoryService` | Runtime owner of the player inventory container and inventory events. |

#### `Assets/CCS/Modules/Inventory/Runtime/Utilities`

| Script | Purpose |
|--------|---------|
| `CCS_InventoryToolUtility` | Resolves whether inventory or equipped items satisfy harvest tool requirements. |
| `CCS_ItemGameplayUtility` | Shared helpers for item, tool, and weapon gameplay classification checks. |

#### `Assets/CCS/Modules/Inventory/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_InventoryValidationUtility` | Runtime-safe validation for inventory profiles and tuning values. |

### Module: `PlayerDeath`

#### `Assets/CCS/Modules/PlayerDeath/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_PlayerDeathValidationRegistration` | Registers player death validator on the central survival validation pipeline. |
| `CCS_PlayerDeathValidationValidator` | Validates player death module layout, assets, and composition wiring. |

#### `Assets/CCS/Modules/PlayerDeath/Runtime/Components`

| Script | Purpose |
|--------|---------|
| `CCS_PlayerRespawnPoint` | World spawn transform used by CCS_PlayerDeathService on respawn. |

#### `Assets/CCS/Modules/PlayerDeath/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_PlayerDeathEventArgs` | Event payload for player death and respawn notifications. |
| `CCS_PlayerDeathEvents` | Delegate contracts for player death and respawn events. |

#### `Assets/CCS/Modules/PlayerDeath/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_PlayerDeathProfile` | Tuning profile for starvation/dehydration death and respawn recovery values. |

#### `Assets/CCS/Modules/PlayerDeath/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_PlayerDeathRuntimeBridge` | Resolves CCS_PlayerDeathService from the active runtime host registry. |
| `CCS_PlayerDeathService` | Monitors hunger/thirst depletion and handles respawn at bootstrap spawn points. |

### Module: `Playtesting`

#### `Assets/CCS/Modules/Playtesting/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_PlaytestBootstrapSetup` | Creates default playtest profile, composition wiring, and bootstrap HUD. |
| `CCS_PlaytestValidationRegistration` | Registers playtesting module validator with the survival validation pipeline. |
| `CCS_PlaytestValidationValidator` | Validates playtesting module layout, assets, composition, and bootstrap HUD. |

#### `Assets/CCS/Modules/Playtesting/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_PlaytestStepDefinition` | Serializable manual playtest checklist step configuration. |
| `CCS_PlaytestStepState` | Runtime checklist state for a single playtest step definition. |
| `CCS_PlaytestStepStatus` | Checklist status values for manual playtest harness steps. |
| `CCS_PlaytestStepType` | Step archetypes for the bootstrap manual playtest checklist. |

#### `Assets/CCS/Modules/Playtesting/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_PlaytestEventArgs` | Event payload for playtest checklist lifecycle notifications. |
| `CCS_PlaytestEvents` | Delegate contracts for manual playtest harness events. |

#### `Assets/CCS/Modules/Playtesting/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_PlaytestProfile` | Bootstrap-only manual playtest harness tuning and default checklist. |

#### `Assets/CCS/Modules/Playtesting/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_PlaytestRuntimeBridge` | Resolves CCS_PlaytestService from the active runtime host registry. |
| `CCS_PlaytestService` | Manual bootstrap playtest checklist state and event-driven step completion. |

#### `Assets/CCS/Modules/Playtesting/Runtime/UI`

| Script | Purpose |
|--------|---------|
| `CCS_PlaytestHud` | Developer-only on-screen manual playtest checklist for bootstrap scenes. |

### Module: `SaveLoad`

#### `Assets/CCS/Modules/SaveLoad/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_SaveLoadBootstrapSetup` | Creates default profile, bootstrap wiring, and development test saveable. |
| `CCS_SaveLoadValidationMenu` | Menu entry for save/load validation through the central pipeline. |
| `CCS_SaveLoadValidationRegistration` | Registers save/load validator with the central validation pipeline. |
| `CCS_SaveLoadValidationValidator` | Validates save/load module folders, asmdefs, profile asset, and scripts. |

#### `Assets/CCS/Modules/SaveLoad/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_SaveGameData` | Root JSON save document for a single save slot. |
| `CCS_SaveLoadResult` | Represents the outcome of a save/load operation. |
| `CCS_SaveLoadSaveableIds` | Stable saveable identifiers and restore ordering for module payloads. |
| `CCS_SaveMetadata` | Lightweight metadata describing a save slot without full module payloads. |
| `CCS_SaveModuleDataEntry` | Serializable key/value entry for module-owned save payloads. |
| `CCS_SaveSlotData` | Describes one on-disk save slot for listing and selection. |

#### `Assets/CCS/Modules/SaveLoad/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_SaveLoadEventArgs` | Event payload for save/load lifecycle notifications. |
| `CCS_SaveLoadEvents` | Delegate contracts for save/load service events. |

#### `Assets/CCS/Modules/SaveLoad/Runtime/Interfaces`

| Script | Purpose |
|--------|---------|
| `CCS_ISaveable` | Contract for modules and systems that participate in save/load. |

#### `Assets/CCS/Modules/SaveLoad/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_SaveLoadProfile` | Tuning profile for auto-save rules and save slot limits. |

#### `Assets/CCS/Modules/SaveLoad/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_SaveableRegistry` | Tracks registered saveable systems for capture and restore passes. |
| `CCS_SaveLoadRuntimeBridge` | Resolves save/load service from the runtime registry for dev components. |
| `CCS_SaveLoadService` | Creates, loads, enumerates, and deletes JSON save slots via registered saveables. |
| `CCS_SavePathUtility` | Resolves persistent save directory and file paths. |

#### `Assets/CCS/Modules/SaveLoad/Runtime/Testing`

| Script | Purpose |
|--------|---------|
| `CCS_SaveLoadDebugController` | Developer-facing manual save/load/delete hooks for bootstrap verification. |
| `CCS_SaveLoadDebugPanelPresenter` | Minimal developer save/load panel bound to CCS_SaveLoadDebugController. |
| `CCS_SaveLoadDebugState` | Read-only snapshot for save/load debug panel display. |
| `CCS_TestSaveableComponent` | Development-only saveable that stores string, integer, and timestamp fields. |
| `CCS_TestSaveableState` | Serializable payload for CCS_TestSaveableComponent persistence checks. |

#### `Assets/CCS/Modules/SaveLoad/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_SaveLoadValidationUtility` | Runtime-safe validation for save/load profiles and slot identifiers. |

### Module: `SaveSystem`

#### `Assets/CCS/Modules/SaveSystem/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_SaveBootstrapSetup` | Creates save/death profiles, bootstrap wiring, respawn point, and save node ids. |
| `CCS_SaveValidationRegistration` | Registers save system validator on the central survival validation pipeline. |
| `CCS_SaveValidationValidator` | Validates save system module layout, assets, composition, and smoke tests. |

#### `Assets/CCS/Modules/SaveSystem/Runtime/Bootstrap`

| Script | Purpose |
|--------|---------|
| `CCS_SaveStartupLoader` | Binds player transform and applies unified save or starter loadout on play start. |

#### `Assets/CCS/Modules/SaveSystem/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_SaveData` | Root serializable save payload for unified survival persistence. |

#### `Assets/CCS/Modules/SaveSystem/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_SaveEventArgs` | Event payload for save and load lifecycle notifications. |
| `CCS_SaveEvents` | Delegate contracts for CCS_SaveService lifecycle events. |

#### `Assets/CCS/Modules/SaveSystem/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_SaveProfile` | Tuning profile for unified survival save file rules and auto-save. |

#### `Assets/CCS/Modules/SaveSystem/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_SaveRuntimeBridge` | Resolves CCS_SaveService from the active runtime host registry. |
| `CCS_SaveService` | Unified JSON save/load for player, needs, inventory, and world state. |

#### `Assets/CCS/Modules/SaveSystem/Runtime/Testing`

| Script | Purpose |
|--------|---------|
| `CCS_SaveDebugController` | Development hotkeys for manual unified save and load verification. |

#### `Assets/CCS/Modules/SaveSystem/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_SaveValidationUtility` | Runtime-safe validation for save profiles and save file paths. |

### Module: `Shelter`

#### `Assets/CCS/Modules/Shelter/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_ShelterBootstrapSetup` | Creates default profile, bootstrap wiring, HUD shelter display, and test volume. |
| `CCS_ShelterValidationMenu` | Menu entry for shelter validation through the central pipeline. |
| `CCS_ShelterValidationRegistration` | Registers shelter validator with the survival validation pipeline. |
| `CCS_ShelterValidationValidator` | Validates shelter module folders, asmdefs, profile asset, and scripts. |

#### `Assets/CCS/Modules/Shelter/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_ShelterModifierSnapshot` | Read-only shelter protection values consumed by Environment Effects. |
| `CCS_ShelterSaveData` | Versioned save payload for global shelter state persistence. |
| `CCS_ShelterSnapshot` | Read-only shelter state snapshot for HUD and environment integration. |
| `CCS_ShelterState` | Mutable runtime shelter state owned by CCS_ShelterService. |

#### `Assets/CCS/Modules/Shelter/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_ShelterEventArgs` | Event payload for shelter lifecycle notifications. |
| `CCS_ShelterEvents` | Event delegate definitions for shelter lifecycle. |

#### `Assets/CCS/Modules/Shelter/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_ShelterProfile` | Tuning profile for default shelter protection and volume policy. |

#### `Assets/CCS/Modules/Shelter/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_ShelterRuntimeBridge` | Resolves shelter service from the runtime registry for volumes and harnesses. |
| `CCS_ShelterService` | Authoritative local shelter protection state with events and save/load. |

#### `Assets/CCS/Modules/Shelter/Runtime/Testing`

| Script | Purpose |
|--------|---------|
| `CCS_BuildingShelterIntegrationTestHarness` | Development-only verification of building shelter contribution integration. |
| `CCS_ShelterTestHarness` | Development-only harness that toggles shelter state for bootstrap verification. |

#### `Assets/CCS/Modules/Shelter/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_ShelterValidationUtility` | Profile and protection validation helpers for runtime and editor checks. |

#### `Assets/CCS/Modules/Shelter/Runtime/Volumes`

| Script | Purpose |
|--------|---------|
| `CCS_ShelterVolume` | Trigger volume that applies local shelter protection on entry and exit. |

### Module: `Sleep`

#### `Assets/CCS/Modules/Sleep/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_SleepBedrollFoundationBootstrapSetup` | Creates primitive bedroll prefab, spot definition, profile wiring, and test object. |
| `CCS_SleepBootstrapSetup` | Creates sleep profile, bedroll content, recipe, and bootstrap test area. |
| `CCS_SleepValidationMenu` | Menu entry for sleep validation through the central pipeline. |
| `CCS_SleepValidationRegistration` | Registers sleep validator on the central survival validation pipeline. |
| `CCS_SleepValidationValidator` | Validates sleep module folders, 1.1.3 bedroll foundation, save, and respawn wiring. |

#### `Assets/CCS/Modules/Sleep/Runtime/Components`

| Script | Purpose |
|--------|---------|
| `CCS_SleepSpot` | World placeable bedroll sleep spot with save snapshot and respawn linkage. |

#### `Assets/CCS/Modules/Sleep/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_SleepFailureReason` | Discrete failure reasons for sleep request validation. |
| `CCS_SleepRequest` | Input payload for sleep attempts through CCS_SleepService. |
| `CCS_SleepResult` | Outcome payload for sleep attempts. |
| `CCS_SleepSnapshot` | Read-only sleep readiness and last-result snapshot for HUD and tests. |
| `CCS_SleepSpotSaveState` | Serializable per-instance sleep spot world snapshot for unified save. |

#### `Assets/CCS/Modules/Sleep/Runtime/Definitions`

| Script | Purpose |
|--------|---------|
| `CCS_SleepSpotDefinition` | ScriptableObject definition for primitive placeable bedroll sleep spots. |

#### `Assets/CCS/Modules/Sleep/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_SleepEventArgs` | Event payload for sleep spot and legacy request notifications. |
| `CCS_SleepEvents` | Event handler delegates for sleep spot lifecycle notifications. |

#### `Assets/CCS/Modules/Sleep/Runtime/Interactables`

| Script | Purpose |
|--------|---------|
| `CCS_SleepSpotInteractable` | Interaction handoff that starts sleep at a placed bedroll through the service. |

#### `Assets/CCS/Modules/Sleep/Runtime/Interaction`

| Script | Purpose |
|--------|---------|
| `CCS_BedrollSleepInteractable` | Simple rest point that requests sleep through CCS_SleepService on interact. |

#### `Assets/CCS/Modules/Sleep/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_SleepProfile` | Tuning profile for sleep duration, fatigue restore, and bedroll rules. |

#### `Assets/CCS/Modules/Sleep/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_SleepRuntimeBridge` | Resolves gameplay services from the runtime registry for sleep systems. |
| `CCS_SleepService` | Validates and executes sleep requests, advancing time and restoring fatigue. |

#### `Assets/CCS/Modules/Sleep/Runtime/Testing`

| Script | Purpose |
|--------|---------|
| `CCS_SleepTestHarness` | Development-only harness for bedroll seeding, fatigue setup, and sleep verification. |

#### `Assets/CCS/Modules/Sleep/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_SleepValidationUtility` | Runtime-safe validation for sleep profiles and sleep hour rules. |

### Module: `Storage`

#### `Assets/CCS/Modules/Storage/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_StorageBootstrapSetup` | Creates primitive storage crate content, profile wiring, and bootstrap test object. |
| `CCS_StorageValidationRegistration` | Registers storage validator with the central validation pipeline. |
| `CCS_StorageValidationValidator` | Validates storage module layout, assets, composition wiring, and save integration. |

#### `Assets/CCS/Modules/Storage/Runtime/Components`

| Script | Purpose |
|--------|---------|
| `CCS_StorageContainer` | World storage container with slot inventory, open state, and save snapshots. |

#### `Assets/CCS/Modules/Storage/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_StorageContainerSaveState` | Serializable per-instance storage container world and inventory snapshot. |

#### `Assets/CCS/Modules/Storage/Runtime/Definitions`

| Script | Purpose |
|--------|---------|
| `CCS_StorageContainerDefinition` | ScriptableObject definition for primitive storage container placement and capacity. |

#### `Assets/CCS/Modules/Storage/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_StorageEventArgs` | Event payload for storage container open, transfer, and restore notifications. |
| `CCS_StorageEvents` | Event delegate declarations for storage container lifecycle notifications. |

#### `Assets/CCS/Modules/Storage/Runtime/Interactables`

| Script | Purpose |
|--------|---------|
| `CCS_StorageContainerInteractable` | Interaction handoff that opens or closes a storage container through the service. |

#### `Assets/CCS/Modules/Storage/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_StorageProfile` | Tuning profile for storage service startup and default container definitions. |

#### `Assets/CCS/Modules/Storage/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_StorageRuntimeBridge` | Resolves storage and inventory services from the runtime service registry. |
| `CCS_StorageService` | Registers world storage containers, tracks active container, transfers, and save state. |

#### `Assets/CCS/Modules/Storage/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_StorageValidationUtility` | Profile validation helpers for storage module startup configuration. |

### Module: `SurvivalCore`

#### `Assets/CCS/Modules/SurvivalCore/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalCoreEnvironmentBootstrapSetup` | Updates default survival core profile and bootstrap influence HUD panel. |
| `CCS_SurvivalCoreHungerBootstrapSetup` | Updates default survival core profile with 0.9.5 hunger usage tuning. |
| `CCS_SurvivalCoreValidationMenu` | Unity menu entry for survival core validation through the central pipeline. |
| `CCS_SurvivalCoreValidationRegistration` | Registers survival core validator with the central validation pipeline at editor load. |
| `CCS_SurvivalCoreValidationValidator` | Editor validator for survival core folders, scripts, and profile rules. |

#### `Assets/CCS/Modules/SurvivalCore/Runtime/Environment`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalEnvironmentInfluence` | Read-only environment influence snapshot for stat pressure and HUD debug. |
| `CCS_SurvivalEnvironmentInfluenceUtility` | Calculates conservative environment pressure rates for Survival Core stats. |

#### `Assets/CCS/Modules/SurvivalCore/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalCoreEvents` | Event name constants and delegate types for survival core service notifications. |
| `CCS_SurvivalEnvironmentEventArgs` | Payload for environment influence change notifications. |
| `CCS_SurvivalStatChangedEventArgs` | Event payload when a survival core stat value changes. |

#### `Assets/CCS/Modules/SurvivalCore/Runtime/Presentation`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalEnvironmentInfluenceHudPresenter` | Read-only HUD display for environment influence rates on survival stats. |

#### `Assets/CCS/Modules/SurvivalCore/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalCoreProfile` | ScriptableObject tuning profile for survival core stats and decay rates. |
| `CCS_SurvivalStatDecayDefinition` | Per-second change rates for survival stat decay, recovery, and exposure drift. |
| `CCS_SurvivalStatDefinition` | ScriptableObject-serialized min, max, and starting values for one survival stat. |

#### `Assets/CCS/Modules/SurvivalCore/Runtime/Runtime`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalCoreService` | Runtime owner of survival core stat states, decay tick, snapshots, and events. |

#### `Assets/CCS/Modules/SurvivalCore/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalCoreRuntimeBridge` | Resolves survival core service from the runtime registry for HUD and debug tools. |

#### `Assets/CCS/Modules/SurvivalCore/Runtime/Stats`

| Script | Purpose |
|--------|---------|
| `CCS_HungerState` | Discrete hunger warning states derived from current hunger value. |
| `CCS_HungerStateUtility` | Resolves hunger warning states and fullness checks from snapshots and profile thresholds. |
| `CCS_SurvivalStatModifier` | Additive or multiplicative modifier applied to a survival stat value. |
| `CCS_SurvivalStatSnapshot` | Read-only stat value snapshot for queries and event payloads. |
| `CCS_SurvivalStatState` | Mutable runtime stat state with clamped current, min, and max values. |
| `CCS_SurvivalStatType` | Identifiers for survival core stat channels. |
| `CCS_SurvivalStatUtility` | Shared clamp, normalization, and modifier helpers for survival stats. |

#### `Assets/CCS/Modules/SurvivalCore/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalCoreValidationUtility` | Runtime-safe validation helpers for survival core profiles and stat definitions. |

### Module: `TimeOfDay`

#### `Assets/CCS/Modules/TimeOfDay/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_TimeOfDayBootstrapSetup` | Creates default profile, bootstrap wiring, and HUD time display. |
| `CCS_TimeOfDayValidationMenu` | Menu entry for time-of-day validation through the central pipeline. |
| `CCS_TimeOfDayValidationRegistration` | Registers time-of-day validator with the central validation pipeline. |
| `CCS_TimeOfDayValidationValidator` | Validates time-of-day module folders, asmdefs, profile asset, and scripts. |

#### `Assets/CCS/Modules/TimeOfDay/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_GameClockState` | Mutable runtime clock state owned by CCS_TimeOfDayService. |
| `CCS_GameTimeSnapshot` | Read-only game clock snapshot for HUD and debug display. |
| `CCS_TimeOfDayPhase` | High-level day cycle phases for the global game clock. |
| `CCS_TimeOfDaySaveData` | Serializable save payload for the global game clock. |

#### `Assets/CCS/Modules/TimeOfDay/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_TimeOfDayEventArgs` | Event payload for time-of-day lifecycle notifications. |
| `CCS_TimeOfDayEvents` | Event delegate definitions for global game clock lifecycle. |

#### `Assets/CCS/Modules/TimeOfDay/Runtime/Presentation`

| Script | Purpose |
|--------|---------|
| `CCS_TimeOfDayHudPresenter` | Read-only HUD display for day, clock time, and current phase. |

#### `Assets/CCS/Modules/TimeOfDay/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_TimeOfDayProfile` | Tuning profile for global game clock start time, scale, and phase boundaries. |

#### `Assets/CCS/Modules/TimeOfDay/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_TimeOfDayRuntimeBridge` | Resolves time-of-day service from the runtime registry for HUD and debug tools. |
| `CCS_TimeOfDayService` | Global game clock with phase tracking, events, and save/load integration. |

#### `Assets/CCS/Modules/TimeOfDay/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_TimeOfDayValidationUtility` | Runtime-safe validation for time-of-day profiles and phase boundaries. |

### Module: `UI`

#### `Assets/CCS/Modules/UI/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_UIHudBootstrapSetup` | Creates default HUD profile, HUD prefab, and bootstrap scene integration. |
| `CCS_UIHudLayoutSetup` | Applies 0.4.2a HUD readability layout pass to profile and prefab assets. |
| `CCS_UIValidationMenu` | Menu entry for UI validation through the central pipeline. |
| `CCS_UIValidationRegistration` | Registers UI validator with the central validation pipeline. |
| `CCS_UIValidationValidator` | Validates UI module folders, asmdefs, profile asset, prefab, and scripts. |

#### `Assets/CCS/Modules/UI/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_HudEventArgs` | Payload for HUD presentation events and cached display data. |
| `CCS_HudEvents` | Event name constants and delegate contracts for HUD presentation. |

#### `Assets/CCS/Modules/UI/Runtime/Presentation`

| Script | Purpose |
|--------|---------|
| `CCS_EquipmentSummaryPresenter` | Displays compact equipment summary data from HUD presentation snapshots. |
| `CCS_HudLayoutApplicator` | Applies HUD profile layout settings to anchored presentation areas. |
| `CCS_HudRootPresenter` | HUD composition root that owns presentation service and child presenters. |
| `CCS_InteractionPromptPresenter` | Displays the current interaction prompt from HUD presentation service. |
| `CCS_InventorySummaryPresenter` | Displays compact inventory summary data from HUD presentation snapshots. |
| `CCS_NotificationPresenter` | Displays one transient HUD notification line. |
| `CCS_NotificationQueue` | Manages transient HUD notification messages with lifetime dismissal. |
| `CCS_SurvivalBarPresenter` | Displays survival core stat bars from HUD presentation snapshots. |
| `CCS_WildlifeAiDebugPresenter` | Optional upper-right debug readout for passive wildlife AI states. |

#### `Assets/CCS/Modules/UI/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_HudLayoutSettings` | Serializable HUD layout and readability tuning for presentation layer. |
| `CCS_HudProfile` | HUD visibility and layout tuning profile for presentation layer. |
| `CCS_NotificationProfile` | Serializable notification queue tuning for HUD presentation. |

#### `Assets/CCS/Modules/UI/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_HudGameplayServiceWiring` | Resolves gameplay services from the runtime registry and binds HUD presentation. |
| `CCS_HudPresentationService` | Read-only bridge between gameplay module services and HUD presenters. |

#### `Assets/CCS/Modules/UI/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_UIValidationUtility` | Runtime-safe validation for HUD profiles and module structure checks. |

### Module: `Weather`

#### `Assets/CCS/Modules/Weather/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_WeatherBootstrapSetup` | Creates default profile, bootstrap wiring, and HUD weather display. |
| `CCS_WeatherValidationMenu` | Menu entry for weather validation through the central pipeline. |
| `CCS_WeatherValidationRegistration` | Registers weather validator with the survival validation pipeline. |
| `CCS_WeatherValidationValidator` | Validates weather module folders, asmdefs, profile asset, and scripts. |

#### `Assets/CCS/Modules/Weather/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_WeatherSaveData` | Versioned save payload for global weather state persistence. |
| `CCS_WeatherSnapshot` | Read-only weather snapshot for HUD, debug, and future environment systems. |
| `CCS_WeatherState` | Mutable runtime weather state owned by CCS_WeatherService. |

#### `Assets/CCS/Modules/Weather/Runtime/Definitions`

| Script | Purpose |
|--------|---------|
| `CCS_WeatherTransitionMode` | Defines how weather changes are applied at runtime. |
| `CCS_WeatherType` | Authoritative weather type identifiers for global weather state. |

#### `Assets/CCS/Modules/Weather/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_WeatherEventArgs` | Event payload carrying weather snapshots and diagnostic messages. |
| `CCS_WeatherEvents` | Event delegate definitions for global weather lifecycle. |

#### `Assets/CCS/Modules/Weather/Runtime/Presentation`

| Script | Purpose |
|--------|---------|
| `CCS_WeatherHudPresenter` | Read-only HUD display for current weather and transition progress. |

#### `Assets/CCS/Modules/Weather/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_WeatherProfile` | Tuning profile for weather types, durations, transitions, and modifiers. |

#### `Assets/CCS/Modules/Weather/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_WeatherRuntimeBridge` | Resolves weather service from the runtime registry for HUD and debug tools. |
| `CCS_WeatherService` | Authoritative global weather state with transitions, events, and save/load. |

#### `Assets/CCS/Modules/Weather/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_WeatherValidationUtility` | Profile and weather type validation helpers for runtime and editor checks. |

### Module: `Wildlife`

#### `Assets/CCS/Modules/Wildlife/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_WildlifeAiBootstrapSetup` | Creates wildlife AI profile, living bootstrap wildlife, and HUD debug wiring. |
| `CCS_WildlifeAiValidationMenu` | Menu entry for wildlife AI validation through the central pipeline. |
| `CCS_WildlifeAiValidationRegistration` | Registers wildlife AI validator on the central survival validation pipeline. |
| `CCS_WildlifeAiValidationValidator` | Validates passive wildlife AI scripts, profile assets, and bootstrap content. |
| `CCS_WildlifeBootstrapSetup` | Creates default profile, test definitions, items, and bootstrap scene carcass placeholders. |
| `CCS_WildlifeValidationMenu` | Menu entry for wildlife validation through the central pipeline. |
| `CCS_WildlifeValidationRegistration` | Registers wildlife validator with the central validation pipeline. |
| `CCS_WildlifeValidationValidator` | Validates wildlife module folders, asmdefs, profile asset, and bootstrap content. |

#### `Assets/CCS/Modules/Wildlife/Runtime/AI`

| Script | Purpose |
|--------|---------|
| `CCS_WildlifeAgent` | Passive living wildlife agent with wander, idle, alert, and flee behavior. |
| `CCS_WildlifeAiSnapshot` | Read-only wildlife AI state snapshot for HUD debug and diagnostics. |
| `CCS_WildlifeAiSpecies` | Species classification for passive wildlife AI tuning. |
| `CCS_WildlifeAiState` | Passive wildlife behavior states for 0.9.7 foundation. |

#### `Assets/CCS/Modules/Wildlife/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_WildlifeHarvestedItemDrop` | Runtime drop payload generated by wildlife harvest service. |
| `CCS_WildlifeHarvestRequest` | Request payload for wildlife harvest validation and execution. |
| `CCS_WildlifeHarvestResult` | Represents the outcome of a wildlife harvest attempt. |
| `CCS_WildlifeSnapshot` | Immutable snapshot of wildlife instance state for diagnostics and future save. |
| `CCS_WildlifeState` | Mutable runtime state for a harvestable wildlife instance. |

#### `Assets/CCS/Modules/Wildlife/Runtime/Definitions`

| Script | Purpose |
|--------|---------|
| `CCS_WildlifeDefinition` | ScriptableObject identity and harvest rules for wildlife resource placeholders. |
| `CCS_WildlifeHarvestDropDefinition` | Serializable drop entry for wildlife harvest definitions. |
| `CCS_WildlifeType` | High-level wildlife classification for resource foundation placeholders. |

#### `Assets/CCS/Modules/Wildlife/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_WildlifeEventArgs` | Event payload for wildlife harvest notifications. |
| `CCS_WildlifeEvents` | Event name constants and delegate contracts for wildlife harvest systems. |

#### `Assets/CCS/Modules/Wildlife/Runtime/Harvesting`

| Script | Purpose |
|--------|---------|
| `CCS_HarvestableWildlife` | Interactable MonoBehaviour wrapper for wildlife carcass placeholders. |

#### `Assets/CCS/Modules/Wildlife/Runtime/Health`

| Script | Purpose |
|--------|---------|
| `CCS_WildlifeCarcassSpawnUtility` | Spawns harvestable carcass placeholders when living wildlife dies. |
| `CCS_WildlifeDamageable` | Living wildlife health for primitive melee combat damage application. |
| `CCS_WildlifeHealthState` | Runtime health state for living wildlife damageable targets. |

#### `Assets/CCS/Modules/Wildlife/Runtime/Movement`

| Script | Purpose |
|--------|---------|
| `CCS_WildlifeMovementController` | Simple transform-based wildlife movement without NavMesh or Rigidbody. |

#### `Assets/CCS/Modules/Wildlife/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_WildlifeAiProfile` | Tuning profile for passive rabbit and deer AI movement and flee behavior. |
| `CCS_WildlifeProfile` | Tuning profile for wildlife carcass harvesting foundation rules. |

#### `Assets/CCS/Modules/Wildlife/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_WildlifeAiService` | Registers passive wildlife agents and exposes debug snapshots for HUD wiring. |
| `CCS_WildlifeHarvestService` | Validates wildlife harvest attempts, generates drops, and raises harvest events. |
| `CCS_WildlifeRuntimeBridge` | Resolves gameplay services from the runtime registry for wildlife harvesting. |

#### `Assets/CCS/Modules/Wildlife/Runtime/States`

| Script | Purpose |
|--------|---------|
| `CCS_WildlifeStateMachine` | Passive wildlife state machine for idle, wander, alert, and flee. |

#### `Assets/CCS/Modules/Wildlife/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_WildlifeAiValidationUtility` | Runtime-safe validation for passive wildlife AI profiles and species tuning. |
| `CCS_WildlifeValidationUtility` | Runtime-safe validation for wildlife profiles and definitions. |

### Module: `WorldResources`

#### `Assets/CCS/Modules/WorldResources/Editor/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_WorldResourceBootstrapSetup` | Creates default profile, test definitions, and bootstrap scene test nodes. |
| `CCS_WorldResourceValidationMenu` | Menu entry for world resource validation through the central pipeline. |
| `CCS_WorldResourceValidationRegistration` | Registers world resource validator with the central validation pipeline. |
| `CCS_WorldResourceValidationValidator` | Validates world resource module folders, asmdefs, profile asset, and scripts. |

#### `Assets/CCS/Modules/WorldResources/Runtime/Data`

| Script | Purpose |
|--------|---------|
| `CCS_HarvestedItemDrop` | Represents a single harvested item quantity result. |
| `CCS_HarvestRequest` | Represents a harvest attempt against a resource node. |
| `CCS_HarvestResult` | Represents the outcome of a harvest attempt. |
| `CCS_ResourceNodeState` | Mutable runtime state for a harvestable resource node instance. |
| `CCS_ResourceSnapshot` | Read-only resource node snapshot for queries and future save hooks. |

#### `Assets/CCS/Modules/WorldResources/Runtime/Definitions`

| Script | Purpose |
|--------|---------|
| `CCS_RequiredToolType` | Tool categories required to harvest specific resource definitions. |
| `CCS_ResourceDefinition` | ScriptableObject identity and harvest rules for world resource nodes. |
| `CCS_ResourceDropDefinition` | Serializable drop entry referencing inventory item definitions. |
| `CCS_ResourceNodeType` | High-level classification for harvestable world resource nodes. |

#### `Assets/CCS/Modules/WorldResources/Runtime/Events`

| Script | Purpose |
|--------|---------|
| `CCS_ResourceEventArgs` | Event payload for harvest, depletion, and respawn notifications. |
| `CCS_ResourceEvents` | Event name constants and delegate contracts for world resource systems. |

#### `Assets/CCS/Modules/WorldResources/Runtime/Harvesting`

| Script | Purpose |
|--------|---------|
| `CCS_HarvestableResource` | Interactable MonoBehaviour wrapper for harvestable world resource nodes. |
| `CCS_ResourceHarvestService` | Validates harvest attempts, generates drops, and raises harvest events. |

#### `Assets/CCS/Modules/WorldResources/Runtime/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_WorldResourceProfile` | Tuning profile for respawn rules and future world resource policy. |

#### `Assets/CCS/Modules/WorldResources/Runtime/Respawn`

| Script | Purpose |
|--------|---------|
| `CCS_ResourceRespawnService` | Tracks depleted nodes and restores node state when timers complete. |
| `CCS_ResourceRespawnState` | Tracks respawn timer state for a depleted resource node. |

#### `Assets/CCS/Modules/WorldResources/Runtime/Services`

| Script | Purpose |
|--------|---------|
| `CCS_WorldResourceRuntimeBridge` | Resolves gameplay services from the runtime registry for resource harvesting. |

#### `Assets/CCS/Modules/WorldResources/Runtime/Testing`

| Script | Purpose |
|--------|---------|
| `CCS_ResourceHarvestingTestHarness` | Development-only harness that drives interaction scan and harvest attempts. |

#### `Assets/CCS/Modules/WorldResources/Runtime/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_WorldResourceValidationUtility` | Runtime-safe validation for world resource profiles and definitions. |

---

## Zone: Survival {#zone-survival}

**Script count:** 71

### `Assets/CCS/Survival/Editor/Development/Bootstrap`

| Script | Purpose |
|--------|---------|
| `CCS_BootstrapGroundColliderBootstrapSetup` | Ensures bootstrap test ground has a solid collider and player spawns above it. |
| `CCS_PlayerBootstrapSetup` | Creates player prefab, wires bootstrap scene, and assigns controller profile. |
| `CCS_PrimitiveToolWeaponBootstrapSetup` | Creates tool/weapon classifications, bone resources, recipes, and equipment wiring. |
| `CCS_StarterLoadoutBootstrapSetup` | Creates starter items, primitive recipes, loadout profile, and bootstrap wiring. |
| `CCS_SurvivalGameplayServiceBootstrapSetup` | Assigns default gameplay service profiles to the survival bootstrap prefab host. |
| `CCS_SurvivalSceneBootstrapValidationMenu` | Editor menu for validating active scene bootstrap against development profile expectations. |

### `Assets/CCS/Survival/Editor/Development/BuildVerification`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalBuildVerificationBuildRunner` | Runs Windows development build verification for bootstrap prototype scene. |
| `CCS_SurvivalBuildVerificationSceneSetup` | Ensures bootstrap scene has one Main Camera and build verification ground reference. |

### `Assets/CCS/Survival/Editor/Development/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_BootstrapSceneValidationUtility` | Validates bootstrap scene playable ground collider and player spawn clearance. |
| `CCS_ISurvivalValidationValidator` | Contract for registrable survival editor validation checks. |
| `CCS_PrimitiveToolWeaponValidationRegistration` | Registers primitive tool/weapon validator with the central validation pipeline. |
| `CCS_PrimitiveToolWeaponValidationValidator` | Validates tool/weapon classifications, bone resources, recipes, and equipment wiring. |
| `CCS_StarterLoadoutValidationRegistration` | Registers starter loadout validator with the central validation pipeline. |
| `CCS_StarterLoadoutValidationValidator` | Validates starter loadout profile, items, recipes, and composition wiring. |
| `CCS_SurvivalFoundationValidationValidator` | Foundation milestone validator for folder structure and project version checks. |
| `CCS_SurvivalValidationBatchUtility` | Shared batchmode-safe validation runner with strict error/warning policy. |
| `CCS_SurvivalValidationIssue` | Single validation issue entry for survival editor validation reports. |
| `CCS_SurvivalValidationIssueSeverity` | Severity levels for editor-side survival validation reports. |
| `CCS_SurvivalValidationMenu` | Unity menu entry for running survival development validation checks. |
| `CCS_SurvivalValidationPipeline` | Central validation report pipeline with registrable validators. |
| `CCS_SurvivalValidationReport` | Aggregated editor validation report for survival project structure checks. |
| `CCS_SurvivalValidationUtility` | Facade for survival editor validation through the central pipeline. |

### `Assets/CCS/Survival/Runtime/Bootstrap`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalBootstrap` | Survival layer startup pipeline for install sequencing and diagnostics on CCS_RuntimeHost. |

### `Assets/CCS/Survival/Runtime/Character/Authority`

| Script | Purpose |
|--------|---------|
| `CCS_ISurvivalAuthority` | Contract for future ownership authority of a survival character (identity, control, save, network signals). |

### `Assets/CCS/Survival/Runtime/Character/Avatar`

| Script | Purpose |
|--------|---------|
| `CCS_ISurvivalAvatar` | Contract for the physical scene representation of a survival character (body root, spawn, possession). |
| `CCS_SurvivalAuthorityAvatarBinding` | Readonly relationship between a survival authority ID and avatar ID for future binding/spawn planning. |
| `CCS_SurvivalAuthorityAvatarValidationUtility` | Static validation for authority, avatar, and authority-avatar binding contracts. |

### `Assets/CCS/Survival/Runtime/Character/Diagnostics`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalCharacterDiagnostics` | Character-layer diagnostic aliases for survival foundation constants. |

### `Assets/CCS/Survival/Runtime/Character/Identity`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalIdentityUtility` | Static validation for save-stable authority, avatar, profile, and binding identity strings. |

### `Assets/CCS/Survival/Runtime/Character/Modules`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalCharacterModule` | Survival-owned character-layer module identity using survival foundation module base. |
| `CCS_SurvivalCharacterModuleInstaller` | Survival-owned installer for the character module using survival foundation installer base. |

### `Assets/CCS/Survival/Runtime/Composition`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalGameplayServiceHost` | (no header — infer from name) |
| `CCS_SurvivalGameplayServiceRegistration` | (no header — infer from name) |

### `Assets/CCS/Survival/Runtime/Context`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalRuntimeContext` | Instance-owned survival layer state bound to a single CCS_RuntimeHost. |

### `Assets/CCS/Survival/Runtime/Development/Bootstrap`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalSceneBootstrapper` | Minimal optional scene bootstrap helper for future required prefab/service validation wiring. |
| `CCS_SurvivalSceneBootstrapProfile` | ScriptableObject profile for required/optional scene startup services and objects. |
| `CCS_SurvivalSceneBootstrapRequirementEntry` | Serializable scene object requirement for scene bootstrap profiles. |
| `CCS_SurvivalSceneBootstrapServiceRequirement` | Serializable required service contract entry for scene bootstrap profiles. |
| `CCS_SurvivalSceneBootstrapValidationUtility` | Development-layer scene bootstrap validation for profile service/object requirements. |

### `Assets/CCS/Survival/Runtime/Development/Diagnostics`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalDiagnosticsMessage` | Immutable diagnostic message payload for survival development diagnostics. |
| `CCS_SurvivalDiagnosticsService` | Lightweight runtime diagnostics hub for module/system status without direct coupling. |
| `CCS_SurvivalDiagnosticsSeverity` | Severity classification for survival development diagnostic messages. |
| `CCS_SurvivalDiagnosticsState` | Lightweight lifecycle state values for survival development diagnostics reporting. |

### `Assets/CCS/Survival/Runtime/Development/Settings`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalSettingsProfile` | ScriptableObject placeholder for future graphics/audio/input/accessibility preferences. |
| `CCS_SurvivalSettingsService` | Placeholder settings service for future preference modules without requiring a profile asset. |

### `Assets/CCS/Survival/Runtime/Development/Testing`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalTestRuntimeFlags` | Dev-only runtime flags mirrored from CCS_SurvivalTestToggleProfile for lightweight checks. |
| `CCS_SurvivalTestToggleProfile` | ScriptableObject dev-only test toggles for future automated and manual validation runs. |

### `Assets/CCS/Survival/Runtime/Diagnostics`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalDiagnostics` | Survival-owned diagnostics that verify Core health and survival validation rules. |

### `Assets/CCS/Survival/Runtime/Foundation/Bootstrap`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalBootstrapProfileSlot` | Serializable placeholder for future profile-driven bootstrap setup slots on survival scenes. |

### `Assets/CCS/Survival/Runtime/Foundation/Diagnostics`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalFrameworkFutureMarkers` | Descriptive FUTURE integration markers for contributors (no placeholder systems). |
| `CCS_SurvivalRuntimeConstants` | Central survival-owned constants for module IDs, log categories, and diagnostic expectations. |

### `Assets/CCS/Survival/Runtime/Foundation/Modules`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalModuleBase` | Abstract base for survival-owned modules with shared log and metadata conventions. |
| `CCS_SurvivalModuleInstallerBase` | Abstract installer base for survival-owned modules with standardized log category behavior. |

### `Assets/CCS/Survival/Runtime/Foundation/Profiles`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalProfileBase` | Abstract ScriptableObject base for future survival setup profiles (configuration only). |
| `CCS_SurvivalProfileValidationUtility` | Static validation for survival setup profile ScriptableObject assets. |

### `Assets/CCS/Survival/Runtime/Foundation/Scene`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalSceneBootstrapRules` | Survival-owned scene bootstrap standards and rule constants for AAA-safe composition roots. |
| `CCS_SurvivalSceneBootstrapValidationUtility` | Static validation for survival scene bootstrap composition during bootstrap/diagnostics only. |
| `CCS_SurvivalSceneQueryUtility` | Unity-version-safe scene object queries for validation utilities. |

### `Assets/CCS/Survival/Runtime/Foundation/Services`

| Script | Purpose |
|--------|---------|
| `CCS_ISurvivalService` | Marker contract for future survival-owned services registered on CCS_ServiceRegistry. |

### `Assets/CCS/Survival/Runtime/Foundation/Validation`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalModuleValidationUtility` | Static survival module and skeleton-phase host validation helpers. |
| `CCS_SurvivalValidationResult` | Lightweight survival validation outcome with optional warning state. |

### `Assets/CCS/Survival/Runtime/Installers`

| Script | Purpose |
|--------|---------|
| `CCS_SurvivalInstaller` | Survival-layer composition root for explicit survival module install sequencing. |

### `Assets/CCS/Survival/Runtime/Player`

| Script | Purpose |
|--------|---------|
| `CCS_CampfireBuildingPlayerDriver` | Lets the player place a campfire from a kit using the building framework. |
| `CCS_ConsumableFoodPlayerDriver` | Lets the player consume configured food items and restore hunger. |
| `CCS_InteractionPlayerDriver` | Drives interaction scan and requests from the player camera forward ray. |
| `CCS_PlayerCombatDriver` | Drives primary melee attacks through CCS_CombatService from the player camera. |
| `CCS_PlayerGameplayController` | Composition glue wiring input, movement, camera, stamina, and cursor lock. |

### `Assets/CCS/Survival/Runtime/Player/Loadout`

| Script | Purpose |
|--------|---------|
| `CCS_StarterLoadoutEntry` | Serializable starter inventory grant entry for loadout profiles. |
| `CCS_StarterLoadoutEvents` | Event contracts for starter loadout application. |
| `CCS_StarterLoadoutProfile` | Defines starter inventory grants and primitive recipe registration for new games. |
| `CCS_StarterLoadoutService` | Applies starter inventory grants once on fresh runtime starts. |

