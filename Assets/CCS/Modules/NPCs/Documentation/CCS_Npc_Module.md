# CCS NPC Module

**Milestone 4.6.0** — Profile-driven NPC schedule blocks for placeholder workers and service representatives.

**Milestone 4.5.0** — Transform-based NPC movement driven by schedule blocks (work/home/leisure targets).

**Milestone 4.4.0** — Optional `homeHousingId` placeholder on `CCS_NpcIdentityState` / `CCS_NpcIdentitySnapshot` for future home assignment (debug label on placeholders when set).

**Milestone 4.3.0** — NPC service representatives foundation for settlement businesses.

**Milestone 4.1.0** — NPC identity and role foundation for population placeholder actors.

## Purpose

Generic NPC identity, schedule, movement, and service representative framework for merchants, bankers, clerks, and workforce roles. Assigns stable names, roles, daily schedule blocks, and business-facing titles to population placeholders without AI, dialogue, pathfinding, or combat.

## NPC Schedule Loop (4.6.0)

```text
NPC Has Role
↓
Role Selects Schedule
↓
Schedule Selects Destination
↓
Movement Sends NPC There
↓
Future Routines Ready
```

Default schedules:

| Schedule | Blocks |
|----------|--------|
| Worker | Home 20:00–06:00, Work 06:00–18:00, Leisure 18:00–20:00 |
| Service Representative | Home 20:00–07:00, Service 07:00–18:00, Leisure 18:00–20:00 |

Role mappings: Banker/Merchant representatives → Service Representative schedule; Miner/Farmer/LumberWorker → Worker schedule.

Persisted on `CCS_SettlementSimulationState.npcScheduleStates` (`activeScheduleId`, `currentBlockType`, `currentTargetKind`, `currentTargetId`, `lastEvaluatedHour`). Movement falls back to profile work/home hours when schedule service is unavailable.

Playtest: **NPC Schedule** — **Ctrl+Alt+S**

Bootstrap: `CCS_NpcScheduleFoundationBootstrapSetup.ExecuteBatch`

## Service Representative Loop

```text
Business Activates
↓
Representative Assigned
↓
Player Talks To Named NPC
↓
Existing Service Opens
↓
Town Feels Human
```

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
| `CCS_NpcIdentityService` | Resolve/create/persist identities |
| `CCS_NpcServiceRepresentativeProfile` | Business → service point → representative role/title mappings |
| `CCS_NpcServiceRepresentativeDefinition` | Per-business representative mapping row |
| `CCS_NpcServiceRepresentativeState` | Persisted assignment on settlement simulation state |
| `CCS_NpcServiceRepresentativeSnapshot` | Runtime representative resolved for interaction/debug |
| `CCS_NpcServiceRepresentativeAssignment` | Transient assignment payload during business sync |
| `CCS_NpcServiceRepresentativeService` | Business activate/deactivate sync, actor wiring |
| `CCS_NpcServiceRepresentativeRuntimeBridge` | Spawned rep roots, display names, playtest bridge |
| `CCS_NpcServiceRepresentativeInteractable` | Routes interaction through `CCS_SettlementServiceRouteResolver` |
| `CCS_NpcServiceRepresentativeDebugHud` | Dev HUD for route/fallback status |
| `CCS_NpcServiceRepresentativeUtility` | Id building, role→route mapping, state helpers |
| `CCS_NpcServiceRepresentativeValidationUtility` | Profile, persistence, routing validation |
| `CCS_NpcScheduleProfile` | Schedule definitions and role-to-schedule mappings |
| `CCS_NpcScheduleService` | Daily block evaluation and persisted schedule state |
| `CCS_NpcScheduleRuntimeBridge` | Snapshot/refresh/force-evaluate bridge for playtest |
| `CCS_NpcScheduleValidationUtility` | Block overlap/coverage validation and target resolution |
| `CCS_NpcMovementService` | Transform movement toward schedule-selected targets |

## Active Roles

Merchant, Banker, StableHand, Gunsmith, Blacksmith, Farmer, Rancher, Miner, LumberWorker, Laborer, Clerk.

**Placeholders only:** Doctor, Sheriff (defined but not workforce-assigned).

## Representative Routing

| Role | Route |
|------|-------|
| Merchant, StableHand, Gunsmith, Farmer, Miner, LumberWorker | Vendor |
| Banker | Bank |
| Blacksmith | Industry |
| Clerk | ContractBoard |

Representatives call the same `CCS_SettlementServiceRouteResolver.TryActivate` path as `CCS_SettlementServicePoint`. Service cubes remain valid fallback when a representative is missing or inactive.

## Labels

- **Workers:** `Elias Carter — Miner` + optional schedule debug line (`Work | ccs.survival.schedule.worker | Workplace`)
- **Representatives:** `Samuel Reed` + title line `Frontier Banker` (name + title)

## Integration

- **Business activation:** `CCS_BusinessService` events → `CCS_NpcServiceRepresentativeService.HandleBusinessActivated/Deactivated`
- **Population placeholders:** Prefer existing anchor actor by `businessId`; else spawn/sync near service point
- **Save/load:** `CCS_SettlementSimulationState.npcServiceRepresentativeStates` and `npcIdentityStates` via world simulation
- **Business mapping:** General Store → Merchant, Bank → Banker, Stable → Stable Hand, Gunsmith → Gunsmith, Blacksmith → Blacksmith, Contract Office → Clerk

## Wiring

- Identity profile: `Assets/CCS/Survival/Profiles/NPCs/Identity/CCS_DefaultNpcIdentityProfile.asset`
- Representative profile: `Assets/CCS/Survival/Profiles/NPCs/ServiceRepresentatives/CCS_DefaultNpcServiceRepresentativeProfile.asset`
- World simulation: `settlementNpcIdentityProfile`, `settlementNpcServiceRepresentativeProfile`
- Services: `CCS_SurvivalGameplayServiceRegistration` → identity + representative create/wire

## Playtest

| Group | Shortcut | Bootstrap |
|-------|----------|-----------|
| NPC Identity | Ctrl+Shift+E | `CCS_NpcIdentityFoundationBootstrapSetup.ExecuteBatch` |
| NPC Service Representatives | Ctrl+Alt+R | `CCS_NpcServiceRepresentativeFoundationBootstrapSetup.ExecuteBatch` |

## Validation

Registered via `CCS_NpcValidationRegistration`:

- `CCS_NpcIdentityFoundationValidationValidator` (4.1.0)
- `CCS_NpcServiceRepresentativeFoundationValidationValidator` (4.3.0)
