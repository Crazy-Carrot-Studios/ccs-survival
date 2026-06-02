# CCS Trapping Module

**Milestone:** 1.3.3 — Frontier Trapping Foundation  
**Author:** James Schilz  
**Date:** 2026-06-01  
**Module ID:** `ccs.survival.trapping`

## Frontier trapping loop

```text
Craft Trap
  ↓
Place Trap (active item, preview + confirm)
  ↓
Timer + capture roll (no collision simulation)
  ↓
Harvest with knife (CCS_WildlifeHarvestService)
  ↓
Trade goods at General Store
  ↓
Cook or preserve meat/fish at campfire (1.3.4)
```

## Architecture

| Type | Role |
|------|------|
| `CCS_TrapDefinition` | Trap tuning, species filter, harvest wildlife link |
| `CCS_TrapProfile` | Catalog of trap definitions |
| `CCS_TrapInstance` | Scene trap state + interactable harvest |
| `CCS_TrapService` | Placement, timer capture, save restore |
| `CCS_TrapPlacementPreview` | Primitive placement preview |

## Trap states

`Unarmed` → `Armed` → `Triggered` → `Harvested` (optional `Broken`)

## Capture rules

- Timer-based (`triggerDelaySeconds`) then `captureChance` / `breakChance` rolls
- Finds nearest `CCS_WildlifeAgent` (Rabbit/Turkey; not Deer)
- No wildlife collision simulation required
- Living agent disabled on successful capture

## Harvest

Triggered traps use **knife** interaction → `CCS_WildlifeHarvestService` (no duplicate drop logic).

## Save / load

`CCS_SaveData.trapping` stores trap instances (state, position, timer, capture data).

## Bootstrap

`CCS.Modules.Trapping.Editor.CCS_FrontierTrappingBootstrapSetup.ExecuteBatch`
