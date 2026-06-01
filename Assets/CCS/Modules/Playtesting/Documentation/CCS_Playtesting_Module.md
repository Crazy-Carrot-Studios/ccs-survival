# CCS Playtesting Module

**Module ID:** `ccs.survival.playtesting`  
**Milestone:** 1.0.2 — Manual Playtest Harness  
**Author:** James Schilz (Developer)

## Purpose

Development-only bootstrap checklist and on-screen HUD for manually verifying the core survival loop:

spawn → gather → equip → hunt → harvest → cook → eat → build → save → load → death → respawn.

This is not production UI and does not automate gameplay.

## Runtime types

| Type | Role |
|------|------|
| `CCS_PlaytestStepStatus` | Step state enum |
| `CCS_PlaytestStepType` | Step archetype for auto-completion |
| `CCS_PlaytestStepDefinition` | Serializable checklist entry |
| `CCS_PlaytestProfile` | Harness tuning and default steps |
| `CCS_PlaytestService` | Checklist state and module event subscriptions |
| `CCS_PlaytestRuntimeBridge` | Resolves service from `CCS_RuntimeHost` |
| `CCS_PlaytestHud` | On-screen dev checklist (OnGUI) |
| `CCS_PlaytestEventArgs` | Step change payloads |

## Default profile

`Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset`

## Hotkeys

| Key | Action |
|-----|--------|
| **F7** | Force hunger/thirst to zero (test death). Active only when harness is enabled. |
| **F10** | Toggle playtest HUD visibility |
| **F11** | Advance / pass the active checklist step |
| **F12** | Reset the full checklist |
| **F5** | Save game (`CCS_SaveDebugController`) |
| **F9** | Load game (`CCS_SaveDebugController`) |

## Bootstrap wiring

- `CCS_PlaytestService` registers through `CCS_SurvivalGameplayServiceRegistration`.
- `CCS_SurvivalGameplayServiceHost.playtestProfile` references the default profile.
- `CCS_PlaytestHud` lives under `PlaytestHarness` on `PF_CCS_Survival_BootstrapRoot`.

## Batch setup

```
CCS.Modules.Playtesting.Editor.CCS_PlaytestBootstrapSetup.ExecuteBatch
```

## Event subscriptions

When the harness is enabled, `CCS_PlaytestService` listens for:

- Gathering gathered
- Wildlife killed
- Wildlife harvest completed
- Cooking completed
- Food consumed
- Building placed
- Save completed / load completed
- Player died / player respawned
- Item equipped

## Validation

Registered validator: `ccs.survival.validation.playtesting`
