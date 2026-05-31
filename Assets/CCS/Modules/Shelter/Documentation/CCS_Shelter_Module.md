# CCS Survival — Shelter Module

**Milestone:** 0.7.5 — Shelter & Environmental Protection Foundation  
**Module ID:** `ccs.survival.shelter`  
**Namespace:** `CCS.Modules.Shelter` (editor: `CCS.Modules.Shelter.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-31  
**Status:** Foundation complete (trigger volumes and service only; no building placement)

---

## Purpose

Provide the **runtime shelter protection architecture** that sits between Weather / Environment Effects and future Building systems.

Shelter is the authoritative source for local environmental protection while the player is inside a registered shelter volume:

| Protection | Role |
|------------|------|
| Wetness protection | Reduces effective wetness after weather simulation |
| Exposure protection | Reduces effective exposure after weather simulation |
| Temperature protection | Placeholder warmth mitigation (no complex heating yet) |
| Protection multiplier | Scales volume/profile protection values |

No building placement, snapping, structure durability, or final shelter art in 0.7.5.

---

## Architecture flow

```text
Weather + Time Of Day
        ↓ raw environment values
CCS_ShelterService (local protection state)
        ↓ shelter modifiers
CCS_EnvironmentEffectsService
        ↓ equipment resistances
Effective environment snapshot
        ↓
Survival Core pressure (temperature, fatigue, thirst)
```

**Critical rule:** Shelter exposes protection only. It does **not** mutate weather, survival stats, or building state.

---

## Folder layout

```text
Assets/CCS/Modules/Shelter/
  Runtime/
    Data/           → snapshots, state, save payloads
    Volumes/        → CCS_ShelterVolume trigger volumes
    Services/       → CCS_ShelterService and runtime bridge
    Profiles/       → CCS_ShelterProfile
    Events/         → shelter lifecycle events
    Validation/     → runtime validation helpers
    Testing/        → development harnesses
  Editor/
    Validation/     → pipeline validator, menu, bootstrap setup
  Documentation/    → this file

Assets/CCS/Survival/Profiles/Shelter/
  CCS_DefaultShelterProfile.asset
```

---

## Protection model

Order applied by `CCS_EnvironmentEffectsService`:

1. **Raw** ambient temperature, wetness, exposure from Time Of Day + Weather
2. **Shelter** protection when `IsSheltered` is true
3. **Equipment** resistances from equipped items
4. **Effective** values exposed on `CCS_EnvironmentSnapshot`

Shelter rules:

- Wetness and exposure clamp at **0** minimum after protection
- Temperature receives additive protection placeholder
- Safe when no shelter service or no active shelter exists

---

## Trigger volume flow

`CCS_ShelterVolume`:

- Requires a trigger collider (auto-enabled in `Awake`)
- Registers with `CCS_ShelterService` on enable
- Calls `EnterShelter` / `ExitShelter` on trigger enter/exit
- Accept-any-subject mode is optional for development volumes

Bootstrap test object: `CCS_TestShelterVolume` near the resource test area.

Development harness: `CCS_ShelterTestHarness` toggles sheltered state when no final player trigger exists.

---

## Save / load behavior

| Type | Role |
|------|------|
| `CCS_ShelterSaveData` | Versioned payload with shelter ID, sheltered flag, and active modifiers |
| Saveable ID | `ccs.survival.saveable.shelter.global` |

Restore order:

```text
Inventory → Equipment → Time Of Day → Weather → Shelter → Environment
```

Environment restores **after** shelter so effective values rebuild with saved shelter state.

---

## Environment Effects dependency

`CCS_EnvironmentEffectsService.BindShelterService()` subscribes to shelter events and rebuilds snapshots when protection changes.

Environment snapshot exposes:

- `IsSheltered`
- `ShelterModifierSnapshot`

HUD environment panel shows sheltered state and shelter protection values for debug verification.

---

## Future Building integration

Future Building milestones will replace placeholder volumes with authored structures, doors, and interior zones while keeping:

- `CCS_ShelterService` as protection authority
- Explicit volume registration (no scene scanning)
- Environment Effects as the consumer of shelter modifiers

Deferred: placement, snapping, foundations, walls, doors, durability, final art, biome systems.

---

## Related Documentation

- [Environment Effects Module](../../EnvironmentEffects/Documentation/CCS_Environment_Effects_Module.md)
- [Save Load Module](../../SaveLoad/Documentation/CCS_Save_Load_Module.md)
- [Survival Module Roadmap](../../../Survival/Documentation/CCS_Survival_Module_Roadmap.md)
