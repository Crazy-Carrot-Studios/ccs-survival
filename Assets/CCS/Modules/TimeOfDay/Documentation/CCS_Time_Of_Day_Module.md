# CCS Survival — Time Of Day Module

**Milestone:** 0.7.0 — Time Of Day Foundation  
**Module ID:** `ccs.survival.timeofday`  
**Namespace:** `CCS.Modules.TimeOfDay` (editor: `CCS.Modules.TimeOfDay.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-31  
**Status:** Global game clock foundation complete at **0.7.0**. Weather module (0.7.1) reads time snapshots; Time Of Day does not depend on Weather. Lighting art, sleep, and AI schedules deferred.

---

## Purpose

Provide a **global game clock** with configurable day length, phase boundaries, lifecycle events, save/load persistence, and read-only HUD visibility. **0.7.0** delivers the runtime architecture without weather, final lighting, sleep gameplay, farming schedules, or AI timetables.

---

## Game clock architecture

```text
CCS_TimeOfDayProfile (start time, day length, phase hours)
        ↓
CCS_TimeOfDayService (tick, pause/resume, set time, events, save/load)
        ↓
CCS_GameClockState (mutable day/minute/phase state)
        ↓
CCS_GameTimeSnapshot (read-only HUD/debug snapshot)
        ↓
CCS_TimeOfDayHudPresenter + CCS_HudPresentationService binding
```

**Critical rule:** The clock does not reference lighting, weather, sleep, or AI systems directly. Future modules subscribe to time events.

---

## Folder layout

```text
Assets/CCS/Modules/TimeOfDay/
  Runtime/
    Data/           → phases, snapshots, clock state, save data
    Services/       → CCS_TimeOfDayService, runtime bridge
    Profiles/       → CCS_TimeOfDayProfile
    Events/         → time lifecycle events
    Validation/     → runtime profile validation
    Presentation/   → CCS_TimeOfDayHudPresenter
  Editor/
    Validation/     → pipeline validator, bootstrap setup, menu
  Documentation/    → this file

Assets/CCS/Survival/Profiles/TimeOfDay/
  CCS_DefaultTimeOfDayProfile.asset
```

---

## Phase rules

| Phase | Default hour range |
|-------|-------------------|
| Dawn | 05:00 – 06:59 |
| Day | 07:00 – 17:59 |
| Dusk | 18:00 – 19:59 |
| Night | 20:00 – 04:59 |

Phase boundaries are profile-driven. Validation enforces `Dawn < Day < Dusk < Night`.

---

## Default profile (0.7.0)

| Setting | Default |
|---------|---------|
| Start day | 1 |
| Start time | 07:00 |
| Real seconds per game day | 1800 (30 minutes) |
| Pause on start | false |
| Dawn start | 5 |
| Day start | 7 |
| Dusk start | 18 |
| Night start | 20 |

Asset: `Assets/CCS/Survival/Profiles/TimeOfDay/CCS_DefaultTimeOfDayProfile.asset`

---

## Event flow

| Event | When |
|-------|------|
| `TimeChanged` | Any clock mutation or tick advance |
| `HourChanged` | In-game hour rolls over |
| `DayChanged` | Day number increments |
| `PhaseChanged` | Dawn/Day/Dusk/Night boundary crossed |
| `TimePaused` | `PauseTime()` called |
| `TimeResumed` | `ResumeTime()` called |

Payload: `CCS_TimeOfDayEventArgs` with `CCS_GameTimeSnapshot`.

---

## Service API

`CCS_TimeOfDayService` implements `CCS_ISurvivalService`, `CCS_IUpdatable`, and `CCS_ISaveable`:

| Method | Behavior |
|--------|----------|
| `InitializeFromProfile` | Applies start time, phase rules, pause-on-start |
| `Tick` | Advances clock using delta time and time scale |
| `PauseTime` / `ResumeTime` | Pause/resume ticking |
| `SetTime` | Set day/hour/minute manually |
| `AdvanceTimeByHours` | Instantly advance clock by slept/rest hours (0.9.6 sleep integration) |
| `SetTimeScale` | Multiply tick rate |
| `CreateSnapshot` | Read-only clock state |

SaveableId: `ccs.survival.saveable.timeofday.global`

---

## Save/load behavior

Payload: `CCS_TimeOfDaySaveData` with `saveDataVersion = 1`

| Field | Purpose |
|-------|---------|
| `dayNumber` | Current day |
| `totalMinutesIntoDay` | Minutes elapsed in current day (0–1440) |
| `timeScale` | Active time scale multiplier |
| `isPaused` | Pause state |

Registered with `CCS_SaveLoadService` during gameplay composition. Restores after profile initialization via ordered registry restore (after inventory and equipment).

---

## HUD / debug display

Bootstrap scene upper-right panel (`TimeOfDayHudArea`):

```text
Time
Day 1
07:00
Phase: Day
```

`CCS_HudPresentationService` also binds the service for read-only `GameTimeSnapshot` access.

---

## Deferred (future milestones)

| Area | Status |
|------|--------|
| Weather coupling | One-way only — Weather may read Time Of Day; not vice versa (see Weather module 0.7.1) |
| Lighting / sky art | Deferred |
| Sleep gameplay | Deferred |
| Farming growth schedules | Deferred |
| AI / NPC timetables | Deferred |
| Final player-facing clock UI art | Deferred |

Future weather, sleep, and AI modules should subscribe to `PhaseChanged`, `HourChanged`, and `DayChanged` — not poll raw Unity time.

---

## Validation

**Editor menu:** **CCS → Survival → Time Of Day → Validate Time Of Day**

Validator ID: `ccs.survival.validation.timeofday`

Bootstrap setup batch: `CCS.Modules.TimeOfDay.Editor.CCS_TimeOfDayBootstrapSetup.ExecuteBatch`

---

## Related docs

- [Survival Module Roadmap](../../Survival/Documentation/CCS_Survival_Module_Roadmap.md)
- [Save / Load Module](../SaveLoad/Documentation/CCS_Save_Load_Module.md)
- [UI / HUD Module](../UI/Documentation/CCS_UI_HUD_Module.md)
