# CCS Weather Module

**Module ID:** `ccs.survival.weather`  
**Milestone:** 0.7.1 — Weather Foundation  
**Location:** `Assets/CCS/Modules/Weather/`

---

## Purpose

The Weather module is the authoritative source for current weather state and weather-related gameplay modifiers. It provides data for future environment effects, survival stats, wildlife behavior, and presentation systems without coupling visual or audio polish into the foundation milestone.

---

## Weather Types

| Type | Role |
|------|------|
| **Clear** | Default baseline weather |
| **Cloudy** | Mild temperature reduction |
| **Rain** | Wetness and temperature modifiers |
| **Storm** | Strongest temperature and wetness modifiers |
| **Fog** | Visibility and exposure foundation hook |

---

## Weather State and Snapshot

- **`CCS_WeatherState`** — mutable runtime state owned by `CCS_WeatherService`
- **`CCS_WeatherSnapshot`** — read-only snapshot for HUD, debug, and future systems

Snapshot fields include current/previous/target weather, transition progress, remaining duration, temperature modifier, wetness modifier, pause state, and transition state.

---

## Weather Transition Flow

1. Service initializes from `CCS_WeatherProfile`.
2. Weather runs for a random duration between profile min/max seconds.
3. When duration expires (if change enabled), service selects the next weather type.
4. Transitions use **Timed** mode by default (`transitionDurationSeconds`).
5. `SetWeather(type, mode)` supports **Instant** or **Timed** manual changes.
6. Events fire on change, transition start/complete, pause, and resume.

No particles, fog volumes, lighting, or audio in 0.7.1.

---

## Time Of Day Dependency Direction

```
Weather → Time Of Day   (allowed)
Time Of Day → Weather   (forbidden)
```

`CCS_WeatherService` may optionally bind `CCS_TimeOfDayService` and read snapshots to prepare future weighting hooks.

**Future examples (not implemented in 0.7.1):**

- Fog more likely at dawn
- Storms can affect exposure
- Rain can increase wetness over time

---

## Save / Load Behavior

- Saveable ID: `ccs.survival.saveable.weather.global`
- Payload: `CCS_WeatherSaveData` (`saveDataVersion = 1`)
- Restore order: inventory → equipment → time of day → **weather**

Persisted fields: current/previous/target weather, transition progress, remaining duration, pause state.

---

## HUD / Debug Display

Bootstrap scene includes `WeatherHudArea` near the time-of-day panel (upper-right).

Display format:

- `Weather: Clear`
- `Weather: Rain (45%)` while transitioning

Plain text only. No icons or final art.

---

## Future Environment Effects (0.7.2+)

Temperature, wetness, and exposure modifiers from weather profiles will feed Survival Core stats once the Environment Effects foundation milestone lands. Weather remains data-only until then.

---

## Deferred Systems

- Rain/snow/storm particles
- Fog volumes and sky rendering
- Lighting response
- Audio ambience
- Biome-specific weather tables
- AI schedule coupling

---

## Related Documentation

- [Time Of Day Module](../../TimeOfDay/Documentation/CCS_Time_Of_Day_Module.md)
- [Save Load Module](../../SaveLoad/Documentation/CCS_Save_Load_Module.md)
- [Survival Module Roadmap](../../../Survival/Documentation/CCS_Survival_Module_Roadmap.md)
