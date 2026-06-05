# CCS NPC Module

**Milestone 4.1.0** — NPC identity and role foundation for population placeholder actors.

## Purpose

Generic NPC identity framework for future workers, merchants, lawmen, doctors, bankers, ranchers, miners, and quest givers. Assigns stable names, roles, and settlement affiliation to `CCS_PopulationPlaceholderActor` instances without AI, dialogue, schedules, pathfinding, or combat.

## NPC Identity Loop

```text
Population Grows
↓
Placeholder Workers Appear
↓
Workers Receive Names + Roles
↓
Settlements Feel More Human
```

## Runtime Types

| Type | Role |
|------|------|
| `CCS_NpcIdentityProfile` | Name pools, role display names, workforce/business mappings |
| `CCS_NpcIdentityDefinition` | Per-settlement first/last name pools |
| `CCS_NpcRoleAssignment` | Maps workforce + optional business id → `CCS_NpcRoleType` |
| `CCS_NpcIdentityState` | Persisted identity on `CCS_SettlementSimulationState` |
| `CCS_NpcIdentitySnapshot` | Runtime identity applied to placeholders |
| `CCS_NpcIdentityService` | Resolve/create/persist identities |
| `CCS_NpcRuntimeBridge` | Bridge to population placeholder actors |
| `CCS_NpcIdentityValidationUtility` | Profile, persistence, and role validation |

## Active Roles (4.1.0)

Merchant, Banker, StableHand, Gunsmith, Blacksmith, Farmer, Rancher, Miner, LumberWorker, Laborer, Clerk.

**Placeholders only:** Doctor, Sheriff (defined but not workforce-assigned).

## Integration

- **Population presence:** `CCS_PopulationPresenceAnchor` assigns identity per anchor slot after spawning placeholders.
- **Labels:** `CCS_PopulationPlaceholderActor` shows `Name — Role` and workforce category (e.g. `Elias Carter — Miner`).
- **Save/load:** `CCS_SettlementSimulationState.npcIdentityStates` via world simulation capture/restore.
- **Business mapping:** Merchants at General Store, Stable Hand at Stable, Farmers at Farm Supply, Miners at Mining Supplier, Lumber Workers at Lumber Yard.

## Wiring

- Profile: `Assets/CCS/Survival/Profiles/NPCs/Identity/CCS_DefaultNpcIdentityProfile.asset`
- World simulation: `CCS_WorldSimulationProfile.settlementNpcIdentityProfile`
- Services: `CCS_SurvivalGameplayServiceRegistration` → `CreateNpcIdentityService` / `WireNpcIdentity`

## Playtest

- Group: **NPC Identity**
- Shortcut: **Ctrl+Shift+E** (Ctrl+Shift+I is reserved for playtest HUD)
- Bootstrap: `CCS.Modules.NPCs.Editor.CCS_NpcIdentityFoundationBootstrapSetup.ExecuteBatch`

## Validation

Registered via `CCS_NpcValidationRegistration` → `CCS_NpcIdentityFoundationValidationValidator`.
