# CCS Survival — Module Roadmap

**Milestone baseline:** 0.3.6 — Development Framework Support Foundation  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-28  
**Status:** Development / Framework Support **complete** at 0.3.6. Gameplay modules start next.

---

## Roadmap policy

- Manual module registration through `CCS_SurvivalInstaller` (explicit install order).
- Module IDs use reverse-DNS prefix `ccs.survival.*`.
- Each module validates at bootstrap; extend development validation as modules land.
- No gameplay mechanics in Core; survival gameplay stays in this repository.

---

## Full module list (planned)

| Order | Module area | Module ID (planned) | Phase | Notes |
|------:|-------------|---------------------|-------|-------|
| — | **Development / Framework Support** | `ccs.survival.development` (support layer) | **0.3.6** | **Complete** — diagnostics, validation, testing, settings, scene bootstrap dev tools |
| 1 | Character skeleton | `ccs.survival.character` | 0.3.0 | **Installed** — authority/avatar contracts + skeleton module |
| 2 | Survival core / vitals | `ccs.survival.core` | 1.x | Needs, pressure, vitals service |
| 3 | Character controller / movement | `ccs.survival.movement` | 1.x | CharacterController, sprint/stamina integration |
| 4 | Camera | `ccs.survival.camera` | 1.x | Cinemachine follow / aim |
| 5 | Interaction | `ccs.survival.interaction` | 1.x | Scanner, interactables, input |
| 6 | Pickups | `ccs.survival.pickups` | 1.x | World collectibles; bridge to inventory |
| 7 | Inventory | `ccs.survival.inventory` | 2.x | Container, items, service |
| 8 | Environment hazards | `ccs.survival.environment.hazards` | 1.x | Heat, cold, damage zones |
| 9 | Vitals modifier zones | `ccs.survival.environment.vitals` | 1.x | Rest, water, food zones |
| 10 | UI / HUD | `ccs.survival.ui` | 2.x | Vitals, inventory, prompts |
| 11 | Crafting | `ccs.survival.crafting` | 3.x | Recipes, stations |
| 12 | Equipment | `ccs.survival.equipment` | 3.x | Wearables, tools |
| 13 | Save / persistence | `ccs.survival.save` | 3.x | Authority-owned save hooks |
| 14 | Combat | `ccs.survival.combat` | 4.x | Weapons, damage |
| 15 | AI / creatures | `ccs.survival.ai` | 4.x | Hostile/neutral agents |
| 16 | Quests / narrative | `ccs.survival.quests` | 4.x | Objectives, factions (product/lore) |

Shared upstream module placeholders under `Assets/CCS/Modules/` (Inventory, Crafting, Equipment, Save, UI) remain **empty shells** until survival-specific installers wire game implementations.

---

## Recommended creation order (gameplay)

After **0.3.6**:

1. **Survival core / vitals** — first playable pressure loop  
2. **Movement + camera** — player presence in bootstrap scene  
3. **Interaction + pickups** — world engagement  
4. **Inventory** — collect and store items  
5. **Environment zones** — hazards and vitals modifiers  
6. **UI / HUD** — readouts and prompts  
7. **Crafting / equipment** — crafting loop expansion  
8. **Save** — persistence when schemas stabilize  
9. **Combat / AI / quests** — content-heavy systems last  

Traversal automation, standalone smoke builds, and dev validation roots are **testing workflows** — use `Runtime/Development/Testing` toggles; not standalone gameplay modules.

---

## 0.3.6 completion summary

| Deliverable | Status |
|-------------|--------|
| `Runtime/Development/*` foundation scripts | **Complete** |
| `Editor/Development/*` validation/testing/bootstrap menus | **Complete** |
| `CCS.Survival.Editor.asmdef` | **Complete** |
| Documentation (this roadmap + Development Framework Support) | **Complete** |
| Gameplay module code | **Not started** (by design) |

---

## Related

- [Development Framework Support](CCS_Survival_Development_Framework_Support.md)
- [Future Gameplay Module Guidelines](Future_Gameplay_Module_Guidelines.md)
- [Framework Architecture Guide](Framework_Architecture_Guide.md)
