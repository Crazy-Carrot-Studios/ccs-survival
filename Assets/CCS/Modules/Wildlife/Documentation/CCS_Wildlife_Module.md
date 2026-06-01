# CCS Wildlife Module

**Milestone:** 0.9.3 — Wildlife Resource Foundation  
**Author:** James Schilz  
**Date:** 2026-06-01  
**Module ID:** `ccs.survival.wildlife`

---

## Purpose

The Wildlife module provides a **resource-provider foundation** for animal carcass placeholders. Wildlife can be harvested for Bone, Hide, Sinew, and Raw Meat through the existing interaction and inventory systems.

This milestone does **not** include:

- Live animal AI
- Combat or kill validation
- Predator behavior
- Patrol, chase, or flee logic
- Animations or final art
- Spawning systems

---

## Architecture

| Layer | Responsibility |
|-------|----------------|
| `CCS_WildlifeDefinition` | ScriptableObject harvest rules, tool requirement, and drop table |
| `CCS_HarvestableWildlife` | Scene MonoBehaviour implementing `CCS_IInteractableResultProvider` |
| `CCS_WildlifeHarvestService` | Validates harvests, generates drops, adds items to inventory, raises events |
| `CCS_WildlifeProfile` | Project-shell tuning profile under `Assets/CCS/Survival/Profiles/Wildlife/` |

---

## Harvestable carcass model

Bootstrap and test scenes use **primitive carcass placeholders** (sphere/capsule/cube). Each placeholder:

- Has a visible mesh and solid collider
- References a `CCS_WildlifeDefinition`
- Tracks remaining harvest count through `CCS_WildlifeState`
- Becomes non-interactable when depleted

No combat kill state is required at 0.9.3. Carcasses represent already-dead harvest sources.

---

## Drop model

Drops are defined on `CCS_WildlifeDefinition` through `CCS_WildlifeHarvestDropDefinition` entries referencing `CCS_ItemDefinition` assets.

Example test definitions:

| Definition | Type | Tool | Drops |
|------------|------|------|-------|
| `CCS_TestRabbit` | SmallGame | Knife | Raw Meat x1, Hide x1, Bone x1 |
| `CCS_TestDeerCarcass` | Deer | Knife | Raw Meat x3, Hide x2, Bone x2, Sinew x1 |

---

## Tool requirement

Harvest validation uses `CCS_RequiredToolType` from World Resources (for example Knife). Tool resolution checks:

- Player inventory tool items
- Equipped MainHand and Tool slots

Failures return safe `CCS_WildlifeHarvestResult` messages instead of throwing exceptions.

---

## Inventory integration

`CCS_WildlifeHarvestService.TryHarvest`:

1. Validates tool, remaining harvest count, and definition
2. Pre-checks inventory capacity with `CanAdd`
3. Adds each drop with `AddItem`
4. Fails safely when inventory is full

The service is registered on `CCS_RuntimeHost.ServiceRegistry` through `CCS_SurvivalGameplayServiceRegistration`.

---

## Events

`CCS_WildlifeHarvestService` raises:

- `WildlifeHarvestStarted`
- `WildlifeHarvestCompleted`
- `WildlifeHarvestFailed`
- `WildlifeDepleted`

`CCS_HudPresentationService` listens for completed, failed, and depleted events and queues notifications through the existing HUD notification queue.

---

## Validation

Registered on `CCS_SurvivalValidationPipeline` via `CCS_WildlifeValidationRegistration`.

Menu: **CCS → Survival → Wildlife → Validate Wildlife**

Batch setup: `CCS.Modules.Wildlife.Editor.CCS_WildlifeBootstrapSetup.ExecuteBatch`

---

## Deferred systems

Future milestones may add:

- Live wildlife AI and movement
- Combat and kill-state validation
- Predator aggression behavior
- Respawn and population spawning
- Save/load persistence for wildlife instances
- Cooking and nutrition for Raw Meat

None of these are implemented at **0.9.3**.
