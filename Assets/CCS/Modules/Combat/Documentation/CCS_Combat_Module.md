# CCS Combat Module

**Milestone:** 0.9.8 — Primitive Combat Foundation

## Purpose

Adds simple melee combat sufficient for hunting passive wildlife in the bootstrap scene. Players equip a knife or spear, attack with **Gameplay/PrimaryAction**, and kill rabbits or deer to spawn harvestable carcasses.

## Scope (0.9.8)

| Included | Excluded |
|---|---|
| `CCS_CombatService` melee attacks via camera-forward SphereCast | Humanoid enemies |
| Weapon damage/range on `CCS_ItemDefinition` (knife, spear) | Predators and advanced AI |
| `CCS_WildlifeHealthState` / `CCS_WildlifeDamageable` on living wildlife | Animation-driven combat |
| Carcass spawn on kill (`CCS_TestRabbitCarcass`, `CCS_TestDeerCarcass`) | Multiplayer combat |
| HUD notifications (`Hit Rabbit (10)`, `Rabbit Killed`) | Crosshair and world damage numbers |
| `CCS_PlayerCombatDriver` on player prefab | Physics knockback / rigidbody damage |

## Attack Flow

1. Player presses **PrimaryAction** (mouse left / gamepad right trigger).
2. `CCS_PlayerCombatDriver` calls `CCS_CombatService.TryMeleeAttack` from the player camera origin and forward.
3. Service resolves equipped **MainHand** weapon (`MeleeDamage`, `MeleeRange`).
4. `Physics.SphereCastAll` finds the nearest `CCS_WildlifeDamageable` within range.
5. `ApplyDamage` updates health; on death the service spawns a carcass and destroys the living agent.
6. `WildlifeDamaged` / `WildlifeKilled` events drive HUD notifications.

## Profile Defaults (`CCS_DefaultCombatProfile`)

| Species | Max Health | Carcass |
|---|---|---|
| Rabbit | 20 | `CCS_TestRabbit` wildlife definition |
| Deer | 50 | `CCS_TestDeerCarcass` wildlife definition |

| Weapon | Damage | Range |
|---|---|---|
| Knife | 10 | 2 m |
| Spear | 20 | 3 m |

## Bootstrap Verification Path

Default starter loadout includes a **pocket knife** for bootstrap hunting (1.2.6). The **spear** remains as optional regression content (`ccs.survival.item.starter.spear`) for playtest and legacy validation — not granted on fresh spawn. Living `CCS_TestRabbit` and `CCS_TestDeer` agents retain passive flee AI; the player must close distance and attack until health reaches zero. Harvesting the spawned carcass uses the existing wildlife harvest flow.

## Validation

Menu: **CCS → Survival → Combat → Validate Combat**

Batch: `CCS.Modules.Combat.Editor.CCS_CombatValidationMenu.ValidateCombat`

Bootstrap batch: `CCS.Modules.Combat.Editor.CCS_CombatBootstrapSetup.ExecuteBatch`

Registered on `CCS_SurvivalValidationPipeline` via `CCS_CombatValidationRegistration`.

## Deferred

- Ranged weapons and blocking
- Humanoid and predator combat
- Combat animations and hit reactions
- Multiplayer authority and replication
- World-space damage numbers and targeting reticle
