# CCS Industry Module

## Frontier Industry Loop

```
Gather → Process Resources → Produce Materials → Forge Better Tools → Expand Homestead
```

## Milestone 1.7.0 — Mining outputs

- Iron ore from ore veins feeds existing **Iron Ore → Refined Iron** forge process.
- Coal, clay, stone, flint, scrap iron, and nails remain crafting/trade materials.
- Coal industry placeholder process reserved for future fuel chemistry.

## Milestone 1.6.0 — Ammunition

- Primitive forge blacksmith recipes craft revolver cartridges, rifle cartridges, and shotgun shells.
- Inputs: **Refined Iron** + **Charcoal** (sulfur/saltpeter placeholders reserved for later).
- Category: `CCS_BlacksmithRecipeCategory.Ammunition`.

## Milestone 1.5.0

- Generic `CCS_IndustryService` for resource processing jobs at placed workstations.
- Workstation roles: Saw Table, Charcoal Kiln, Primitive Forge (plus Frontier Workbench for general crafting).
- Blacksmith foundation via `CCS_BlacksmithRecipeDefinition` and forge crafting recipes.
- Camp tier **IndustrialHomestead** requires Primitive Forge after Frontier Homestead.

## Workstation Roles

| Role | Processing |
|------|------------|
| Saw Table | Wood → Lumber, Sapling → Poles |
| Charcoal Kiln | Wood → Charcoal |
| Primitive Forge | Iron Ore → Refined Iron; forge tool parts |
| Frontier Workbench | General crafting (existing crafting module) |

## Save

Industry active jobs persist in `saveData.industry.activeJobs`.
