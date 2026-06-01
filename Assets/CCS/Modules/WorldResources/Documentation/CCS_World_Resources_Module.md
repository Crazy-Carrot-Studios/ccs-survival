# CCS Survival — World Resources Module

**Milestone:** 0.5.1 — World Resource Module Foundation  
**Module ID:** `ccs.survival.world.resources`  
**Namespace:** `CCS.Modules.WorldResources` (editor: `CCS.Modules.WorldResources.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-28  
**Status:** Foundation complete at **0.5.1**. Harvest integration complete at **0.5.2**.

---

## Purpose

Provide the **runtime world resource harvest architecture** used by future Interaction harvesting, Crafting inputs, Save/Load, Loot, and UI systems — without implementing those features in 0.5.1.

The module answers:

| Question | Owner |
|----------|--------|
| What is a harvestable resource? | `CCS_ResourceDefinition` |
| What tool is required? | `CCS_RequiredToolType` |
| What items drop? | `CCS_ResourceDropDefinition` referencing `CCS_ItemDefinition` |
| Who validates and executes harvests? | `CCS_ResourceHarvestService` |
| Who tracks respawn? | `CCS_ResourceRespawnService` |
| What exists in the world? | `CCS_HarvestableResource` MonoBehaviour wrapper |

---

## Resource framework metadata (1.2.4)

`CCS_ResourceDefinition` now includes:

| Field | Purpose |
|-------|---------|
| `resourceSourceType` | `CCS_ResourceSourceType` |
| `harvestMethod` | `CCS_HarvestMethodType` |
| `dropDefinitions` | **Multi-drop** list with min/max quantities per item |

**Design rule:** CCS Survival uses practical resource sources (trees, outcrops, deadfall, fiber plants, water, ore/coal veins, salvage sites) rather than cluttering terrain with random pickup rocks and sticks.

Frontier prepared definitions live under `Assets/CCS/Survival/Profiles/WorldResources/Frontier/`.

`CCS_HarvestMethodType.Fish` is defined for future systems and is **not** implemented in 1.2.4 active use routing.

---

## Architecture flow

```text
CCS_ResourceDefinition (ScriptableObject identity + drops + tool rules)
        ↓
CCS_HarvestableResource (world node wrapper + runtime node state)
        ↓
CCS_HarvestRequest (definition + node state + equipped tool)
        ↓
CCS_ResourceHarvestService (validate → generate drops → optional inventory add)
        ↓
CCS_ResourceRespawnService (track depleted nodes → restore on timer)
```

**Critical rule:** World Resources never references resource UI, final world art, terrain systems, save/load, or interaction visuals in 0.5.1.

---

## Folder layout

```text
Assets/CCS/Modules/WorldResources/
  Runtime/
    Definitions/    → node type, tool type, resource + drop definitions
    Data/             → request, result, node state, snapshot
    Harvesting/       → harvest service + harvestable component
    Respawn/          → respawn state + respawn service
    Events/           → event args + contracts
    Profiles/         → CCS_WorldResourceProfile
    Validation/       → runtime profile and definition validation
  Editor/
    Validation/       → pipeline validator, menu, bootstrap setup
  Documentation/      → this file

Assets/CCS/Survival/Profiles/WorldResources/
  CCS_DefaultWorldResourceProfile.asset
  TestItems/
  TestResources/
```

---

## Resource node types

| Type | Purpose |
|------|---------|
| Tree | Wood and canopy harvest nodes |
| Rock | Stone and ore harvest nodes |
| Plant | Fiber and forage harvest nodes |
| Gatherable | Generic pickup-style nodes |
| Custom | Project-specific extensions |

---

## Tool requirements

| Tool | Purpose |
|------|---------|
| None | Hand gathering |
| Axe | Tree harvesting (heavier wood — later progression) |
| Pickaxe | Rock harvesting |
| Knife | Early tree branch harvesting and plant harvesting |
| Shovel | Buried resource harvesting |

Tool gameplay is foundation-only in 0.5.1. Harvest validation compares equipped tool type against the resource definition. At **0.9.1**, the bootstrap test tree uses **Knife → Branch** for the early-game loop. Axe/hatchet harvesting for heavier wood outputs is deferred.

---

## Early-game harvest loop (0.9.1)

```text
Starter loadout grants Knife
        ↓
Player interacts with test tree (Knife required)
        ↓
CCS_InventoryToolUtility confirms Knife in inventory
        ↓
Tree drops Branch x2–4
        ↓
Branches feed primitive hand crafting recipes
```

Bootstrap scene harvestables use `assumeRequiredToolEquipped = false` so the player path validates inventory tools.

---

## Rock resource planning (deferred)

Stone is **not** sourced from random loose rocks scattered across terrain as the primary loop.

| Decision | Notes |
|----------|-------|
| Primary stone source | Rock formations, outcrops, and quarries (future milestone) |
| Loose stones | Rare optional pickups later — not the core resource loop |
| Current bootstrap rock node | Retains pickaxe test path; not the intended early-game progression |

---

## Harvest flow

1. `CCS_HarvestableResource.CanHarvest(equippedToolType)` builds a `CCS_HarvestRequest`.
2. `CCS_ResourceHarvestService` validates definition, node state, and tool requirement.
3. Drops are generated from `CCS_ResourceDropDefinition` entries.
4. Optional inventory integration adds items through `CCS_PlayerInventoryService`.
5. Node state consumes one harvest; depletion raises `OnResourceDepleted`.
6. `CCS_ResourceRespawnService` tracks depleted nodes when respawn is enabled.

Failures return `CCS_HarvestResult.Failure` — no exceptions for expected validation failures.

---

## Respawn flow

1. Depleted node registers with `CCS_ResourceRespawnService`.
2. Timer duration = `resourceDefinition.RespawnTimeSeconds * profile.GlobalRespawnMultiplier`.
3. `TickNode` advances the timer each frame from `CCS_HarvestableResource.Update`.
4. When complete, node state resets and `OnResourceRespawned` fires.

Respawn can be disabled through `CCS_WorldResourceProfile.EnableRespawn`.

---

## Inventory integration plan

Harvest results return `CCS_HarvestedItemDrop` entries cleanly from the service layer.

| Operation | API |
|-----------|-----|
| Pre-check capacity | `CCS_PlayerInventoryService.CanAdd` |
| Grant harvested items | `CCS_PlayerInventoryService.AddItem` |

Inventory is optional at harvest time. Callers may apply drops later without UI coupling.

---

## Bootstrap verification scene

`SCN_CCS_Survival_Bootstrap` includes primitive placeholder nodes:

| Object | Node Type | Tool |
|--------|-----------|------|
| `CCS_TestTree` | Tree | Knife (Branch drops at 0.9.1) |
| `CCS_TestRock` | Rock | Pickaxe |
| `CCS_TestPlant` | Plant | Knife |

Each object uses `CCS_HarvestableResource` with test definitions under `Assets/CCS/Survival/Profiles/WorldResources/TestResources/`.

---

## Events

| Event | When |
|-------|------|
| OnHarvestStarted | Harvest validation passed and execution begins |
| OnHarvestCompleted | Drops generated successfully |
| OnHarvestFailed | Validation or execution failure |
| OnResourceDepleted | Remaining harvest count reached zero |
| OnResourceRespawned | Node state restored after respawn timer |

---

## Resource interaction flow (0.5.2)

```text
CCS_InteractionService.TickScan(camera origin/forward)
        ↓
CCS_HarvestableResource (CCS_IInteractableResultProvider)
        ↓
CCS_InteractionService.RequestInteraction()
        ↓
CCS_ResourceHarvestService.TryHarvest(request, inventoryService)
        ↓
CCS_PlayerInventoryService.AddItem(...)
        ↓
HUD inventory summary + harvest notifications
```

Harvestable resources expose display names from `CCS_ResourceDefinition`, return `CanInteract() == false` when depleted, and report harvest failures through harvest service events.

---

## Harvest → inventory flow (0.5.2)

1. Interaction triggers `TryInteract()` on the focused harvestable.
2. Harvest service validates tool, node state, and inventory capacity.
3. Drops are added through `CCS_PlayerInventoryService.AddItem`.
4. Partial inventory failures return a safe failure result without consuming the harvest.

---

## HUD notification flow (0.5.2)

| Event | Notification |
|-------|----------------|
| Harvest completed | `Harvested {Item} x{N}` |
| Harvest failed (inventory full) | `Harvest failed: Inventory Full` |
| Resource depleted | `Resource depleted` |
| Resource respawned | `Resource respawned` |

`CCS_HudPresentationService` binds resource harvest and respawn services through the runtime registry.

---

## Deferred features

| System | Status |
|--------|--------|
| Final world art | Not in 0.5.1 |
| Terrain systems | Not in 0.5.1 |
| Resource UI | Not in 0.5.1 |
| Interaction visuals | Not in 0.5.1 |
| Save/load node persistence | Not in 0.5.1 |
| World streaming | Not in 0.5.1 |
| Bootstrap gameplay service registration | Future composition pass |

---

## Validation

**Editor menu:** **CCS → Survival → World Resources → Validate World Resources**

Validator ID: `ccs.survival.validation.worldresources`

Registered via `CCS_WorldResourceValidationRegistration` on the central `CCS_SurvivalValidationPipeline`.

---

## Related docs

- [Inventory Module](../Inventory/Documentation/CCS_Inventory_Module.md)
- [Wildlife Module](../Wildlife/Documentation/CCS_Wildlife_Module.md)
- [Crafting Module](../Crafting/Documentation/CCS_Crafting_Module.md)
- [Survival Module Roadmap](../../Survival/Documentation/CCS_Survival_Module_Roadmap.md)
