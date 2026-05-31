# CCS Environment Effects Module

**Module ID:** `ccs.survival.environment`  
**Milestone:** 0.7.2 — Environment Effects Foundation  
**Location:** `Assets/CCS/Modules/EnvironmentEffects/`

---

## Purpose

The Environment Effects module is the authoritative simulation layer for **ambient temperature**, **wetness**, and **exposure**. It combines Time Of Day and Weather data into read-only environment snapshots for HUD, debug, and future Survival Core integration.

No damage, no Survival Core stat mutation, and no clothing insulation in 0.7.2.

---

## Simulation Layer

```
Time Of Day ──┐
              ├──► CCS_EnvironmentEffectsService ──► CCS_EnvironmentSnapshot
Weather ──────┘
```

| Output | Source |
|--------|--------|
| Ambient Temperature | Day/Night phase bonus + weather temperature modifier |
| Wetness | Weather wetness modifier |
| Exposure | Weather exposure modifier |

Snapshot also includes current **Weather Type** and **Time Phase** for downstream systems.

---

## Time Of Day Dependency

- Reads `CCS_GameTimeSnapshot` from `CCS_TimeOfDayService`
- **Day** phase applies profile day temperature bonus (+2 default)
- **Night** phase applies profile night temperature penalty (-3 default)
- Dawn/Dusk apply no phase temperature modifier in 0.7.2

Time Of Day does **not** depend on Environment Effects.

---

## Weather Dependency

- Reads `CCS_WeatherSnapshot` from `CCS_WeatherService`
- Uses effective weather type (transition target while transitioning)
- Applies profile weather/temperature, wetness, and exposure modifiers

Weather does **not** depend on Environment Effects.

---

## Save / Load Behavior

- Saveable ID: `ccs.survival.saveable.environment.global`
- Payload: `CCS_EnvironmentSaveData` (`saveDataVersion = 1`)
- Restore order: inventory → equipment → time of day → weather → **shelter** → **environment**

Persisted fields: ambient temperature, wetness, exposure.

---

## HUD / Debug Display

Bootstrap scene includes `EnvironmentHudArea` beneath time and weather panels (upper-right):

```text
Env Temp: 2
Wetness: 0
Exposure: 0
Sheltered: No
Shelter Wet: 0
Shelter Exp: 0
Shelter Temp: 0
Temp Res: 0
Wet Res: 0
Exp Res: 0
Eff Temp: 2
Eff Wet: 0
Eff Exp: 0
```

Raw values come from Time Of Day and Weather. Shelter protection applies before equipment resistances to produce effective values.

Plain text only. No icons or final art.

---

## Shelter Environmental Protection (0.7.5)

`CCS_EnvironmentEffectsService` binds `CCS_ShelterService` and applies shelter protection **before** equipment modifiers.

| Stage | Rule |
|-------|------|
| **Raw** | Simulated ambient temperature, wetness, exposure from Time + Weather |
| **Sheltered wetness** | `max(0, raw wetness − wetness protection × multiplier)` |
| **Sheltered exposure** | `max(0, raw exposure − exposure protection × multiplier)` |
| **Sheltered temperature** | Raw temperature + temperature protection × multiplier |
| **Effective values** | Equipment resistances applied after shelter protection |

`CCS_EnvironmentSnapshot` exposes `IsSheltered` and `ShelterModifierSnapshot`.

Persisted environment save data remains **raw** simulation; shelter state restores through `CCS_ShelterSaveData`.

---

## Equipment Environmental Modifiers (0.7.4)

`CCS_EnvironmentEffectsService` binds `CCS_PlayerEquipmentService` and applies aggregated equipment modifiers when building snapshots.

| Value | Rule |
|-------|------|
| **Raw** | Simulated ambient temperature, wetness, exposure from Time + Weather |
| **Effective temperature** | Raw temperature + temperature resistance |
| **Effective wetness** | `max(0, raw wetness − wetness resistance)` |
| **Effective exposure** | `max(0, raw exposure − exposure resistance)` |

`CCS_EnvironmentSnapshot` exposes raw values, effective values, and `EquipmentModifierSnapshot`.

Survival Core reads **effective** values for temperature, fatigue, and thirst pressure.

Persisted save data remains **raw** environment simulation; equipment modifiers are derived at runtime from equipped items.

---

## Survival Core Integration (0.7.3 / 0.7.4)

Environment Effects remains authoritative for ambient temperature, wetness, and exposure.

`CCS_SurvivalCoreService` reads environment snapshots and applies conservative pressure to:

- **Temperature** — ambient temperature influence with profile recovery/decay rates and clamps
- **Fatigue** — exposure pressure
- **Thirst** — wetness pressure

No Health modification. No damage or death systems.

Bootstrap HUD includes an influence debug panel beneath the environment panel:

```text
Temp Δ: X
Fatigue Δ: X
Thirst Δ: X
```

---

## Future Clothing Insulation Integration

Tiered clothing sets, slot-specific gear progression, and crafting sources will expand the 0.7.4 resistance foundation. Biome and regional modifiers remain deferred.

---

## Future Biome Integration

Biome profiles may add regional temperature offsets and weather weighting. Environment Effects profile remains the local modifier layer.

---

## Deferred Systems

- Damage / health effects from exposure
- Hypothermia / heat stroke systems
- Biome regional offsets
- Wetness accumulation over time (beyond snapshot modifiers)

---

## Related Documentation

- [Time Of Day Module](../../TimeOfDay/Documentation/CCS_Time_Of_Day_Module.md)
- [Weather Module](../../Weather/Documentation/CCS_Weather_Module.md)
- [Save Load Module](../../SaveLoad/Documentation/CCS_Save_Load_Module.md)
- [Survival Core Module](../../SurvivalCore/Documentation/CCS_Survival_Core_Module.md)
- [Survival Module Roadmap](../../../Survival/Documentation/CCS_Survival_Module_Roadmap.md)
