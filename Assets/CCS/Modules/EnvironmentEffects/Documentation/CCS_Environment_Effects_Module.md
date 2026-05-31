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
- Restore order: inventory → equipment → time of day → weather → **environment**

Persisted fields: ambient temperature, wetness, exposure.

---

## HUD / Debug Display

Bootstrap scene includes `EnvironmentHudArea` beneath time and weather panels (upper-right):

```
Env Temp: 2
Wetness: 0
Exposure: 0
```

Plain text only. No icons or final art.

---

## Survival Core Integration (0.7.3)

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

Clothing and equipment will provide insulation modifiers applied **after** environment simulation. Deferred until equipment/environment coupling milestone.

---

## Future Biome Integration

Biome profiles may add regional temperature offsets and weather weighting. Environment Effects profile remains the local modifier layer.

---

## Deferred Systems

- Damage / health effects from exposure
- Hypothermia / heat stroke systems
- Clothing insulation
- Biome regional offsets
- Wetness accumulation over time (beyond snapshot modifiers)

---

## Related Documentation

- [Time Of Day Module](../../TimeOfDay/Documentation/CCS_Time_Of_Day_Module.md)
- [Weather Module](../../Weather/Documentation/CCS_Weather_Module.md)
- [Save Load Module](../../SaveLoad/Documentation/CCS_Save_Load_Module.md)
- [Survival Core Module](../../SurvivalCore/Documentation/CCS_Survival_Core_Module.md)
- [Survival Module Roadmap](../../../Survival/Documentation/CCS_Survival_Module_Roadmap.md)
