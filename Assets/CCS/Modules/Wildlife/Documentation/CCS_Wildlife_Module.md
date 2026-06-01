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

None of these are implemented at **0.9.3**.

---

## Passive wildlife AI (0.9.7)

Living wildlife placeholders use transform movement and a small state machine:

| State | Behavior |
|-------|----------|
| Idle | Pause at current location |
| Wander | Move to random point within wander radius |
| Alert | Brief detection pause when player enters flee radius |
| Flee | Move away from player until distance exceeds flee radius × 2 |

| Component | Responsibility |
|-----------|----------------|
| `CCS_WildlifeAgent` | Scene MonoBehaviour on living rabbit/deer placeholders |
| `CCS_WildlifeStateMachine` | Idle / wander / alert / flee transitions |
| `CCS_WildlifeMovementController` | Transform movement without NavMesh or Rigidbody |
| `CCS_WildlifeAiService` | Agent registry and HUD debug snapshots |
| `CCS_WildlifeAiProfile` | Rabbit and deer wander/flee/speed tuning |

Default profile: `Assets/CCS/Survival/Profiles/Wildlife/CCS_DefaultWildlifeAiProfile.asset`

| Species | Wander radius | Flee radius | Move speed |
|---------|---------------|-------------|------------|
| Rabbit | 10 | 8 | 4 |
| Deer | 20 | 15 | 6 |

Bootstrap living placeholders:

- `CCS_TestRabbit` — sphere primitive with `CCS_WildlifeAgent`
- `CCS_TestDeer` — capsule primitive with `CCS_WildlifeAgent`

Carcass harvest placeholders (`CCS_TestRabbitCarcass`, `CCS_TestDeerCarcass`) remain available for the existing harvest loop.

HUD optional debug (upper-right): `Wildlife:` lines such as `Rabbit Idle`, `Deer Wander`.

Validation menu: **CCS → Survival → Wildlife → Validate Wildlife AI**

Batch setup: `CCS.Modules.Wildlife.Editor.CCS_WildlifeAiBootstrapSetup.ExecuteBatch`

---

## Deferred systems (post-0.9.7)

Future milestones may add:

- Primitive combat and kill-state validation
- Carcass spawn on death (0.9.8 hunting loop)
- Predator aggression behavior
- Respawn and population spawning
- Save/load persistence for wildlife instances
- Animations and final art

None of these are implemented at **0.9.7**.
