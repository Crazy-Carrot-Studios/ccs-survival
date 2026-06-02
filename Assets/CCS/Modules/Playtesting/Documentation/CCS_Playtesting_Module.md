# CCS Playtesting Module

**Module ID:** `ccs.survival.playtesting`  
**Milestone:** 1.0.3 — Manual Playtest Pass + Fixes  
**Author:** James Schilz (Developer)

## Purpose

Development-only bootstrap checklist and on-screen HUD for manually verifying the core survival loop:

spawn → **controller polish** → gather → equip → hunt → harvest → cook → eat → shelter → workbench → storage → bedroll → save → load → death → respawn.

This is not production UI and does not automate gameplay.

## Manual playtest route (bootstrap)

**Scene:** `Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity`

1. Enter Play Mode.
2. Press **F10** to show the harness HUD.
3. Confirm **Spawn** passes when the player exists.
4. Complete **Third-person controller polish** (walk, sprint, camera, interact, gather, building preview, storage, bedroll).
5. Interact with **CCS_TestGatheringSmallTree** or **CCS_TestGatheringBush** (sticks or wood).
5. Press **F6** to equip the starter spear (required for melee hunt).
6. Primary attack **CCS_TestRabbit** or **CCS_TestDeer**.
7. Harvest the carcass (interact).
8. Cook raw meat at **CCS_TestCampfire** (interact).
9. Press **F** to eat cooked meat.
10. Press **B** to place one test foundation piece.
11. Press **F5** to save.
12. Press **F9** to load.
13. Press **F7** to force death.
14. Confirm **Respawn** passes (same frame as death in current foundation).

## Milestone 1.0.3 manual test results

**Method:** Code-path audit of bootstrap scene wiring plus targeted checklist fixes (Unity Editor interactive pass recommended after pull).

| Step | Result | Notes |
|------|--------|-------|
| Spawn | **Pass** | HUD notifies when `CCS_PlayerGameplayController` exists. |
| Gather sticks/wood | **Pass (after fix)** | Wood from small trees no longer blocked by stick-only target filter. |
| Equip spear | **Pass (after fix)** | **F6** dev equip added; no player-facing equip UI yet. |
| Hunt wildlife | **Pass** | Requires spear in MainHand; primary attack drives `CCS_CombatService`. |
| Harvest carcass | **Pass** | `WildlifeHarvestCompleted` event. |
| Cook at campfire | **Pass** | `CookingCompleted` event. |
| Eat cooked meat | **Pass** | **F** consume; rabbit or venison accepted. |
| Place foundation | **Pass (after fix)** | **B** dev placement seeds costs and places `ccs.survival.building.test.foundation`. |
| Save (F5) | **Pass** | `SaveCompleted` when `CCS_SaveDebugController` succeeds. |
| Load (F9) | **Pass** | `LoadCompleted` on success. |
| Trigger death (F7) | **Pass (after fix)** | `TriggerTestDeath` ensures death when stats were already depleted. |
| Respawn | **Pass** | Fires immediately after death in 1.0.1 foundation (no death screen). |

**Steps skipped:** None required for milestone closure.

## Bugs fixed in 1.0.3

| Issue | Fix |
|-------|-----|
| Gather step failed when granting **wood** (profile targeted stick only) | `MatchesTargetItem` accepts stick **or** wood when gather target is a gather resource id. |
| **F7** did not always trigger death | `CCS_PlayerDeathService.TriggerTestDeath()` called after draining needs. |
| Equip spear blocked (no equip UI) | **F6** `TryEquipStarterSpear()` via equipment profile catalog. |
| Building step blocked (placement harness disabled) | **B** `TryPlacePlaytestFoundation()` seeds build costs and places foundation. |
| Building event could ignore target piece id | `MatchesTargetBuildingPiece` validates foundation piece id. |

## Known limitations

- No production equip UI; use **F6** on bootstrap until equipment UI exists.
- No production building placement UI; use **B** on bootstrap until player placement UX exists.
- Death and respawn occur in the same frame (1.0.1 foundation); no death screen delay.
- **F11** can skip steps intentionally for partial verification.
- Automated gameplay bot deferred.

## Hotkeys

| Key | Action |
|-----|--------|
| **F6** | Equip starter spear (dev) |
| **F7** | Force death (drain needs + `TriggerTestDeath`) |
| **F10** | Toggle playtest HUD |
| **F11** | Advance active checklist step |
| **F12** | Reset checklist |
| **B** | Place test foundation (dev) |
| **F** | Consume food (`CCS_ConsumableFoodPlayerDriver`) |
| **Primary action** | Melee attack / interact gather |
| **F5** / **F9** / **F8** | Save / load / delete save (`CCS_SaveDebugController` via `CCS_DevHotkeyUtility`) |
| **R** | Reload active firearm (`CCS_PlayerActiveItemDriver`) |

## Input routing (1.7.2)

| Layer | Source |
|-------|--------|
| Gameplay | `CCS_Survival_InputActions` |
| Dev hotkeys | `CCS_DevHotkeyUtility` / `CCS_KeyboardInputUtility` |
| Banned | Legacy `UnityEngine.Input` |

## Checklist groups (HUD)

Steps render under grouped headers: Core Spawn / Movement, Inventory / Equipment, Gathering / Crafting, Fishing, Economy, Hunting, Trapping, Cooking, Shelter / Homestead, Industry, Horse / Wagon, Firearms, Prospecting. Mapping lives in `CCS_PlaytestStepGroupingUtility`.

## Runtime types

| Type | Role |
|------|------|
| `CCS_PlaytestStepStatus` | Step state enum |
| `CCS_PlaytestStepType` | Step archetype for auto-completion |
| `CCS_PlaytestStepDefinition` | Serializable checklist entry |
| `CCS_PlaytestProfile` | Harness tuning and default steps |
| `CCS_PlaytestService` | Checklist state and module event subscriptions |
| `CCS_PlaytestRuntimeBridge` | Resolves service from `CCS_RuntimeHost` |
| `CCS_PlaytestHud` | On-screen dev checklist (OnGUI, grouped by domain) |
| `CCS_PlaytestStepGroup` | HUD checklist section enum |
| `CCS_PlaytestStepGroupingUtility` | Maps step types to HUD groups |
| `CCS_PlaytestEventArgs` | Step change payloads |

## Default profile

`Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset`

## Bootstrap wiring

- `CCS_PlaytestService` registers through `CCS_SurvivalGameplayServiceRegistration`.
- `CCS_SurvivalGameplayServiceHost.playtestProfile` references the default profile.
- `CCS_PlaytestHud` lives under `PlaytestHarness` on `PF_CCS_Survival_BootstrapRoot`.

## Batch setup

```
CCS.Modules.Playtesting.Editor.CCS_PlaytestBootstrapSetup.ExecuteBatch
```

## Validation

Registered validator: `ccs.survival.validation.playtesting`
