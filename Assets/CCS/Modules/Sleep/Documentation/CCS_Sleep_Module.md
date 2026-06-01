# CCS Sleep Module

**Milestone:** 0.9.6 — Sleep & Bedroll Foundation

## Purpose

Adds the foundation for sleeping and resting using bedrolls and shelter-aware fatigue recovery.

## Scope (0.9.6)

| Included | Excluded |
|---|---|
| Bedroll item, equipment, and primitive hand recipe | Dreams |
| `CCS_SleepService` time advance and fatigue restore | Death |
| Shelter-aware fatigue multiplier | Enemy interruption |
| Bootstrap rest point interactable | Final sleep UI |
| HUD sleep notifications | Full day summary screens |
| Optional Sleep Ready debug label | Final bed art |

## Sleep Flow

1. Player interacts with a rest point (`CCS_BedrollSleepInteractable`) or harness calls `TrySleep`.
2. `CCS_SleepService` validates bedroll availability, fatigue need, and optional shelter requirement.
3. On success:
   - `CCS_TimeOfDayService.AdvanceTimeByHours` advances the game clock.
   - Fatigue is reduced through Survival Core modifiers.
   - Hunger and thirst drain are simulated for slept hours using existing Survival Core tuning.
4. On failure, `SleepFailed` raises a safe reason (missing bedroll, unsafe conditions, already rested, etc.).

## Profile Defaults (`CCS_DefaultSleepProfile`)

| Field | Default |
|---|---|
| `defaultSleepHours` | 6 |
| `minimumSleepHours` | 1 |
| `maximumSleepHours` | 10 |
| `fatigueRestorePerHour` | 12 |
| `requireBedroll` | true |
| `requireShelter` | false |
| `unshelteredFatigueRestoreMultiplier` | 0.5 |

## Bedroll Content

| Asset | Role |
|---|---|
| `CCS_Item_Bedroll` | Inventory bedroll item |
| `CCS_Equipment_Bedroll` | Bedroll equipment slot definition |
| `CCS_Recipe_Bedroll` | Hand recipe: Hide x2 + Fiber x4 → Bedroll x1 |

Bedroll is **not** granted in the default starter loadout. The sleep test harness may seed one bedroll for verification only.

## Validation

Menu: **CCS → Survival → Sleep → Validate Sleep**

Registered on `CCS_SurvivalValidationPipeline` via `CCS_SleepValidationRegistration`.

## Deferred

- Enemy interruption during sleep
- Starvation or death while sleeping
- Dreams and narrative sleep events
- Full sleep UI and day summary screens
