# CCS NPC Module

**Milestone 4.9.0** — Profile-driven dialogue stub lines for affiliated NPCs and service representatives.

**Milestone 4.8.0** — Persistent settlement, business, workforce, and region affiliation metadata for placeholder NPCs.

**Milestone 4.7.0** — Lightweight visible activity states derived from schedule blocks and movement status.

**Milestone 4.6.0** — Profile-driven NPC schedule blocks for placeholder workers and service representatives.

**Milestone 4.5.0** — Transform-based NPC movement driven by schedule blocks (work/home/leisure targets).

**Milestone 4.4.0** — Optional `homeHousingId` placeholder on `CCS_NpcIdentityState` / `CCS_NpcIdentitySnapshot` for future home assignment (debug label on placeholders when set).

**Milestone 4.3.0** — NPC service representatives foundation for settlement businesses.

**Milestone 4.1.0** — NPC identity and role foundation for population placeholder actors.

## Purpose

Generic NPC identity, affiliation, dialogue stubs, schedule, activity, movement, and service representative framework for merchants, bankers, clerks, and workforce roles. Assigns stable names, roles, community affiliations, placeholder dialogue lines, daily schedule blocks, visible activities, and business-facing titles to population placeholders without AI, branching dialogue, pathfinding, or combat.

## NPC Dialogue Stub Loop (4.9.0)

```text
Player Interacts
↓
NPC Identity Resolves
↓
Affiliation + Role Resolve
↓
Stub Line Displays
↓
Future Dialogue System Ready
```

Profile-driven stub categories: **Greeting**, **RoleIntroduction**, **SettlementIntroduction**, **BusinessIntroduction**, **ServiceHint**, **GenericFallback**. Lines filter by role, optional settlement/business/affiliation, and service route. No branching, player choices, quests, voice, or final UI.

| Component | Purpose |
|-----------|---------|
| `CCS_NpcDialogueStubProfile` | Role definitions, global lines, fallback |
| `CCS_NpcDialogueStubService` | Resolves lines from identity + affiliation |
| `CCS_NpcDialogueStubRuntimeBridge` | Runtime callbacks and last result for playtest |
| `CCS_NpcDialogueStubDebugHud` | Dev panel: name, role, settlement, business, lines |
| `CCS_NpcDialogueStubInteractable` | Workforce interaction path |
| `CCS_NpcServiceRepresentativeInteractable` | Dialogue before existing service routing |

Representatives show dialogue stub lines then route through `CCS_SettlementServiceRouteResolver` (unchanged). Static profile data — no dialogue persistence; identity/affiliation from 4.1/4.8 enable post-load resolution.

Playtest: **NPC Dialogue** — **Ctrl+Alt+D**

Bootstrap: `CCS_NpcDialogueFoundationBootstrapSetup.ExecuteBatch`

## NPC Affiliation Loop (4.8.0)

```text
Population Creates NPC
↓
NPC Assigned Settlement
↓
NPC Assigned Business/Workforce
↓
NPC Becomes Part Of Community
```

Persisted on `CCS_SettlementSimulationState.npcAffiliationStates`:

| Field | Purpose |
|-------|---------|
| `npcIdentityId` | Stable NPC key |
| `settlementId` | Settlement ownership |
| `regionId` | Region affiliation from world simulation |
| `businessId` | Business link for representatives |
| `workforceCategory` | Workforce link for workers |
| `isServiceRepresentative` | Representative flag |
| `loyaltyValue` | 0–100 metadata (default 50; no gameplay effects yet) |

Auto-assignment uses settlement, business, population, and representative services when identities are created or refreshed. Labels show settlement name on line three; debug shows `Affiliation: Settlement / Business` and detail HUD lines.

Playtest: **NPC Affiliations** — **Ctrl+Alt+F**

Bootstrap: `CCS_NpcAffiliationFoundationBootstrapSetup.ExecuteBatch`

## NPC Activity Loop (4.7.0)

```text
Schedule Selects Block
↓
Movement Selects Destination
↓
Activity Reflects Current Behavior
↓
Settlement Feels More Alive
```

Schedule block → activity mapping:

| Block | Activity |
|-------|----------|
| Sleep | Sleeping |
| Home | Resting |
| Work | Working |
| Service | Serving |
| Break | Resting |
| Leisure | Leisure |
| Idle | Idle |

Movement override: `TravelingToWork` / `TravelingHome` → **Traveling**.

Persisted on `CCS_SettlementSimulationState.npcActivityStates` (`currentActivityType`, `lastEvaluatedHour`). Labels show activity line; optional primitive cube indicator above actor.

Playtest: **NPC Activity** — **Ctrl+Alt+A**

Bootstrap: `CCS_NpcActivityFoundationBootstrapSetup.ExecuteBatch`

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
| `CCS_NpcMovementService` | Transform movement toward schedule-selected targets; notifies activity service on movement updates |
| `CCS_NpcActivityProfile` | Schedule block to activity mappings |
| `CCS_NpcActivityService` | Derives visible activity from schedule + movement |
| `CCS_NpcAffiliationProfile` | Default loyalty and affiliation assignment policy |
| `CCS_NpcAffiliationService` | Assigns/persists settlement, business, workforce, region affiliations |
| `CCS_NpcAffiliationRuntimeBridge` | Snapshot/refresh bridge for playtest and labels |
| `CCS_NpcAffiliationValidationUtility` | Profile/state validation and label debug helpers |
| `CCS_NpcActivityRuntimeBridge` | Snapshot/refresh bridge for playtest and labels |
| `CCS_NpcActivityValidationUtility` | Mapping validation and fallback resolution |

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

- **Representatives:** `Samuel Reed` → `Frontier Banker` → `Frontier Trading Post`
- **Workers:** `Elias Carter` → `Miner` → `Iron Ridge Mining Camp`
- **Debug:** `Affiliation: {Settlement} / {Business or Workforce}` plus detail HUD line with loyalty

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
