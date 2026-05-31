# CCS Survival — Survival Core Module

**Milestone:** 0.7.4 — Clothing & Equipment Environmental Modifiers  
**Module ID:** `ccs.survival.core`  
**Namespace:** `CCS.Modules.SurvivalCore` (editor: `CCS.Modules.SurvivalCore.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-31  
**Status:** Foundation complete at **0.3.7b**. Environment integration complete at **0.7.3**. Equipment effective environment values consumed at **0.7.4**.

---

## Purpose

Provide a **data-driven survival stat foundation** for Health, Stamina, Hunger, Thirst, Temperature/Exposure, and Fatigue/Sleep before character controller, interaction, or inventory systems consume it.

---

## Included stats

| Stat | Role (0.3.7) |
|------|----------------|
| **Health** | Vitality pool; passive heal/damage placeholders on profile |
| **Stamina** | Exertion pool; recovery/drain placeholders via service flag |
| **Hunger** | Pressure stat with gentle decay placeholder |
| **Thirst** | Pressure stat with gentle decay placeholder |
| **Temperature** | Exposure comfort drift toward profile target (default 50/100) |
| **Fatigue** | Sleep pressure; gains over time from zero |

---

## Architecture

```text
Assets/CCS/Modules/SurvivalCore/
  Runtime/
    Stats/          → types, state, modifier, snapshot, utility
    Profiles/       → ScriptableObject tuning
    Runtime/        → CCS_SurvivalCoreService
    Events/         → change/depleted/restored/initialized contracts
    Validation/     → profile/stat/decay validation (runtime-safe)
  Editor/
    Validation/     → pipeline validator + menu
    Tools/          → default profile creation menu
  Documentation/    → this file

Assets/CCS/Survival/Profiles/SurvivalCore/   → default profile assets (project shell)
```

**Assemblies:**

| Assembly | References |
|----------|------------|
| `CCS.Modules.SurvivalCore.Runtime` | `CCS.Core.Runtime`, `CCS.Survival.Runtime`, `CCS.Modules.EnvironmentEffects.Runtime` |
| `CCS.Modules.SurvivalCore.Editor` | Core, Survival runtime/editor, SurvivalCore runtime |

**No:** UI, CharacterController, inventory, equipment, scene objects.

---

## Runtime service flow

1. Construct `CCS_SurvivalCoreService` (optional `CCS_SurvivalDiagnosticsService`).
2. `InitializeFromProfile(CCS_SurvivalCoreProfile)` — builds `CCS_SurvivalStatState` per definition.
3. Composition binds `CCS_EnvironmentEffectsService` via `BindEnvironmentEffectsService`.
4. `CCS_IUpdatable.Tick` calls `TickSurvival(deltaTime)` each frame through the runtime update loop.
5. Consumers read `TryGetSnapshot(statType)` or `GetAllSnapshots()`.
6. Consumers subscribe to `StatChanged`, `StatDepleted`, `StatRestored`, `SurvivalCoreInitialized`, `EnvironmentInfluenceChanged`.
7. Optional: `TryApplyModifier` for additive/multiplicative changes.

Diagnostics reports use `ccs.survival.core` when a diagnostics service is present; **safe when null**.

---

## Environment → Survival flow (0.7.3 / 0.7.4)

```text
CCS_EnvironmentEffectsService (raw ambient from Time + Weather)
        ↓ equipment resistances applied
CCS_EnvironmentSnapshot (raw + effective + equipment modifiers)
        ↓ effective values
CCS_SurvivalEnvironmentInfluenceUtility.Calculate
        ↓ per-second rates
CCS_SurvivalCoreService.ApplyEnvironmentInfluence
        ↓
Temperature / Fatigue / Thirst stat states (clamped)
```

| Environment input | Survival stat | Rule |
|-------------------|---------------|------|
| **Effective** ambient temperature | **Temperature** | Positive effective × recovery rate; negative effective × decay rate; clamped by profile min/max |
| **Effective** exposure | **Fatigue** | Effective exposure × exposure fatigue multiplier |
| **Effective** wetness | **Thirst** | Effective wetness × wetness thirst multiplier (additional drain) |

**Health is not modified.** No hypothermia, heat stroke, damage, or death systems in 0.7.4.

`CCS_SurvivalEnvironmentInfluence` exposes ambient inputs and calculated per-second deltas for HUD/debug.

---

## Events

| Event | When |
|-------|------|
| `StatChanged` | Current value changes beyond epsilon |
| `StatDepleted` | Stat crosses to at-or-below min |
| `StatRestored` | Stat leaves depleted state |
| `SurvivalCoreInitialized` | Profile applied successfully |
| `EnvironmentInfluenceChanged` | Environment influence rates change meaningfully |

Event name constants: `CCS_SurvivalCoreEvents`.

---

## Profiles

| Type | Role |
|------|------|
| `CCS_SurvivalCoreProfile` | Root tuning asset |
| `CCS_SurvivalStatDefinition` | Min / max / starting per stat |
| `CCS_SurvivalStatDecayDefinition` | Per-second drain/gain/exposure drift |

**Environment tuning (0.7.3) on `CCS_SurvivalCoreProfile`:**

| Field | Role |
|-------|------|
| `temperatureRecoveryRate` | Scale when ambient temperature is above neutral |
| `temperatureDecayRate` | Scale when ambient temperature is below neutral |
| `exposureFatigueMultiplier` | Fatigue gain per exposure unit |
| `wetnessThirstMultiplier` | Thirst drain per wetness unit |
| `minimumTemperatureClamp` / `maximumTemperatureClamp` | Clamp applied during environment temperature influence |

**Default asset path:** `Assets/CCS/Survival/Profiles/SurvivalCore/CCS_DefaultSurvivalCoreProfile.asset` (committed project configuration; do not move into Modules)

Default starting values:

| Stat | Start / Max |
|------|-------------|
| Health | 100 / 100 |
| Stamina | 100 / 100 |
| Hunger | 100 / 100 |
| Thirst | 100 / 100 |
| Temperature | 50 / 100 |
| Fatigue | 0 / 100 |

---

## Validation

| Component | Role |
|-----------|------|
| `CCS_SurvivalCoreValidationUtility` | Runtime-safe profile/stat/decay rules |
| `CCS_SurvivalCoreValidationValidator` | Registered on `CCS_SurvivalValidationPipeline` |
| Menu | **CCS → Survival → Survival Core → Validate Survival Core** |

Checks: folders, scripts, profile completeness, min/max/start validity, non-negative decay rates.

---

## Intentionally deferred (post-0.3.7)

| Item | Target |
|------|--------|
| Bootstrap module installer + service registry | 0.3.7+ wiring or 0.3.8 integration |
| Character controller stamina drain hook | 0.3.8 |
| Interaction / consumable effects | 0.3.9 |
| Inventory item modifiers | 0.4.0 |
| UI / HUD readouts | After inventory |
| Save/load persistence | Later |
| Environmental zone modifiers | 0.7.4 equipment resistances via Environment Effects effective values |
| Clothing insulation | 0.7.4+ |
| Death / respawn rules | Later |

---

## Related

- [Module Roadmap](CCS_Survival_Module_Roadmap.md)
- [Development Framework Support](CCS_Survival_Development_Framework_Support.md)
