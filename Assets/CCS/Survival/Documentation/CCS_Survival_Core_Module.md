# CCS Survival — Survival Core Module

**Milestone:** 0.3.7 — Survival Core Module Foundation  
**Module ID:** `ccs.survival.core`  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-28  
**Status:** Foundation complete (not yet wired to bootstrap installer)

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
Assets/CCS/Survival/Runtime/SurvivalCore/
  Stats/          → types, state, modifier, snapshot, utility
  Profiles/       → ScriptableObject tuning
  Runtime/        → CCS_SurvivalCoreService
  Events/         → change/depleted/restored/initialized contracts
  Validation/     → profile/stat/decay validation (runtime-safe)

Assets/CCS/Survival/Editor/SurvivalCore/
  Validation/     → pipeline validator + menu
  Tools/          → default profile creation menu
```

**Dependencies:** `CCS.Survival.Runtime` → `CCS.Core.Runtime` only.  
**No:** UI, CharacterController, inventory, equipment, scene objects.

---

## Runtime service flow

1. Construct `CCS_SurvivalCoreService` (optional `CCS_SurvivalDiagnosticsService`).
2. `InitializeFromProfile(CCS_SurvivalCoreProfile)` — builds `CCS_SurvivalStatState` per definition.
3. External systems call `TickSurvival(deltaTime)` each frame or fixed update.
4. Consumers read `TryGetSnapshot(statType)` or `GetAllSnapshots()`.
5. Consumers subscribe to `StatChanged`, `StatDepleted`, `StatRestored`, `SurvivalCoreInitialized`.
6. Optional: `TryApplyModifier` for additive/multiplicative changes.

Diagnostics reports use `ccs.survival.core` when a diagnostics service is present; **safe when null**.

---

## Events

| Event | When |
|-------|------|
| `StatChanged` | Current value changes beyond epsilon |
| `StatDepleted` | Stat crosses to at-or-below min |
| `StatRestored` | Stat leaves depleted state |
| `SurvivalCoreInitialized` | Profile applied successfully |

Event name constants: `CCS_SurvivalCoreEvents`.

---

## Profiles

| Type | Role |
|------|------|
| `CCS_SurvivalCoreProfile` | Root tuning asset |
| `CCS_SurvivalStatDefinition` | Min / max / starting per stat |
| `CCS_SurvivalStatDecayDefinition` | Per-second drain/gain/exposure drift |

**Default asset path:** `Assets/CCS/Survival/Profiles/SurvivalCore/CCS_DefaultSurvivalCoreProfile.asset`  
**Create via menu:** **CCS → Survival → Survival Core → Create Default Survival Core Profile**

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
| Environmental zone modifiers | Later |
| Death / respawn rules | Later |

---

## Related

- [Module Roadmap](CCS_Survival_Module_Roadmap.md)
- [Development Framework Support](CCS_Survival_Development_Framework_Support.md)
