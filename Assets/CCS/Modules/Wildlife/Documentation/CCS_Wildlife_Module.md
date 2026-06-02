# CCS Wildlife Module

**Milestone:** 1.3.2 — Frontier Hunting Foundation  
**Author:** James Schilz  
**Date:** 2026-06-01  
**Module ID:** `ccs.survival.wildlife`

---

## Purpose

The Wildlife module provides harvestable animal resources for the frontier survival loop: passive living wildlife, primitive death/carcass state, knife harvesting, and economy trade goods.

This milestone does **not** include:

- Final archery systems (draw, aim IK, arrow physics)
- Advanced animal AI (pack, stalk, flee trees)
- Final animations or IK
- Firearms
- Ragdolls or complex corpse physics

---

## Frontier hunting loop

```text
Bow (active item raycast)
  ↓
Kill wildlife (CCS_WildlifeDamageable)
  ↓
Carcass spawns / enters dead state
  ↓
Equip knife → use on carcass (Active Item routing)
  ↓
Skin + butcher drops → inventory
  ↓
Sell at General Store → Trade Dollars
```

---

## Architecture (1.3.2)

| Type | Responsibility |
|------|----------------|
| `CCS_WildlifeHarvestDefinition` | Frontier drop tables (skin/butcher) with `CCS_ResourceSourceType.Wildlife` |
| `CCS_WildlifeHarvestProfile` | Catalog + lookup by wildlife / harvest definition id |
| `CCS_WildlifeHarvestResult` | Typed outcomes (`Success`, `WildlifeNotDead`, `WrongTool`, etc.) |
| `CCS_WildlifeHarvestService` | Validates dead carcass, knife tool, merges drops, inventory add |
| `CCS_WildlifeHarvestValidationUtility` | Profile/definition/drop validation for pipeline |
| `CCS_HarvestableWildlife` | Scene carcass interactable + depleted visual marker |
| `CCS_WildlifeDamageable` | Minimal health/death for bow hunting foundation |

Content assets live under `Assets/CCS/Survival/Content/Wildlife/` (frontier-specific).

---

## Harvest rules

| Rule | Behavior |
|------|----------|
| Living wildlife | Cannot harvest (`WildlifeNotDead`) |
| Dead carcass | Harvest allowed |
| Tool | Knife required (`WrongTool` if missing) |
| Depleted | `WildlifeAlreadyHarvested` / `TargetUnavailable` |
| Methods | `CCS_HarvestMethodType.Skin` and `Butcher` drop tables |

---

## Frontier species drops (bootstrap)

| Species | Skin / Butcher outputs |
|---------|------------------------|
| Rabbit | Raw meat, hide, bone |
| Turkey | Raw meat, feather, bone |
| Deer | Raw meat, hide, bone, animal fat |

---

## Active item integration

- **Bow** → `CCS_ActiveItemBehaviorType.Bow` → `CCS_CombatService.TryRangedAttack` (raycast)
- **Knife** → `CCS_ActiveItemTargetKind.HarvestableWildlife` → `CCS_WildlifeHarvestService`

Enable routing on `CCS_ActiveItemProfile.enableWildlifeHarvestRouting`.

---

## Events

`CCS_WildlifeHarvestService` raises harvest started/completed/failed/depleted events consumed by HUD and playtest harness.

---

## Validation

| Validator | Registration |
|-----------|--------------|
| `CCS_WildlifeValidationValidator` | `CCS_WildlifeValidationRegistration` |
| `CCS_FrontierHuntingValidationValidator` | `CCS_FrontierHuntingValidationRegistration` |

Batch setup: `CCS.Modules.Wildlife.Editor.CCS_FrontierHuntingBootstrapSetup.ExecuteBatch`

---

## Passive wildlife AI (0.9.7+)

Living wildlife placeholders use transform movement and a small state machine (Idle / Wander). Species: Rabbit, Deer, Turkey (1.3.2).
