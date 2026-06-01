# CCS Gathering Module

**Milestone:** 0.9.9 — Resource Gathering Foundation

## Purpose

Adds primitive world gathering nodes for sticks, stone, wood, and plant fiber without tool requirements. Players interact with bootstrap placeholders to collect resources into inventory.

## Scope (0.9.9)

| Included | Excluded |
|---|---|
| `CCS_GatheringService` node registration and reward grants | Tool-gated harvesting (see World Resources) |
| `CCS_GatheringNode` availability, depletion, and respawn timer | Animation-driven gather actions |
| SmallTree, Rock, and Bush node types | Procedural world spawning |
| Reward tables keyed by node type | Multiplayer replication |
| Interaction via `CCS_GatheringInteractable` | Dedicated gather UI |

## Gather Flow

1. Player focuses a gathering node through `CCS_InteractionService`.
2. `CCS_GatheringInteractable.TryInteract()` calls `CCS_GatheringNode.Gather()`.
3. `CCS_GatheringService.TryGatherNode()` resolves profile rewards and grants inventory items.
4. Node depletes; optional respawn timer restores availability.
5. Events `GatheringNodeGathered`, `GatheringNodeDepleted`, and `GatheringNodeRespawned` fire for future HUD wiring.

## Default Rewards

| Node | Rewards |
|---|---|
| SmallTree | Stick x2, Wood x1 |
| Rock | Stone x2 |
| Bush | PlantFiber x2, Stick x1 |

## Profile Defaults (`CCS_DefaultGatheringProfile`)

| Field | Default |
|---|---|
| `nodeInteractionDistance` | 3 m |
| `gatherDurationSeconds` | 0 (instant) |
| `respawnEnabled` | true |
| `respawnDelaySeconds` | 30 |

## Bootstrap Verification

`CCS_GatheringTestArea` in `SCN_CCS_Survival_Bootstrap` contains:

- `CCS_TestGatheringSmallTree`
- `CCS_TestGatheringRock`
- `CCS_TestGatheringBush`

## Resource framework metadata (1.2.4)

Each `CCS_GatheringNodeRewardSettings` entry includes:

| Field | Purpose |
|-------|---------|
| `resourceSourceType` | `CCS_ResourceSourceType` (Natural, Salvage, Mining, Water, …) |
| `harvestMethod` | `CCS_HarvestMethodType` (Gather, Chop, Mine, Salvage, …) |
| `requiredToolType` | `CCS_ItemToolType` override (None uses method defaults) |
| `rewards[]` | **Multi-drop** fixed yields per gather |

**Design rule:** Practical sources (trees, outcrops, fiber, water, veins, salvage) replace clutter pickups of random rocks/sticks for progression.

## Active item routing (1.2.3)

When the player uses an active tool from third-person:

1. `CCS_ActiveItemTargetResolver` reads `CCS_InteractionService.CurrentTarget`.
2. `CCS_ActiveItemGatheringToolUtility` maps node type → required tool (tree/bush → axe, rock → pickaxe).
3. On match, `CCS_GatheringService.TryGatherNode()` grants rewards through existing inventory integration.

Interact (F) and active tool use (primary) both reach gathering through their respective entry points; active use adds tool validation before gather.

## Validation

Menu: **CCS → Survival → Gathering → Validate Gathering**

Batch: `CCS.Modules.Gathering.Editor.CCS_GatheringValidationMenu.ValidateGathering`

Bootstrap batch: `CCS.Modules.Gathering.Editor.CCS_GatheringBootstrapSetup.ExecuteBatch`

Registered on `CCS_SurvivalValidationPipeline` via `CCS_GatheringValidationRegistration`.

## Deferred

- Gather duration UI and progress channel
- Tool requirements and skill modifiers
- Node clustering and biome distribution
- Multiplayer authority for node depletion state
