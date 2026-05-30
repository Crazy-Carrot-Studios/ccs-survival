# CCS Survival — Module Roadmap

**Milestone baseline:** 0.3.8a — Batch validation runner hotfix  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-28  
**Status:** Batch validation infrastructure **complete** at 0.3.8a. **0.3.9 Interaction** is next.

---

## Version sequence (foundation grouping)

| Version | Milestone |
|---------|-----------|
| **0.3.5 / 0.3.5a** | Survival framework quality gate (pre-gameplay) |
| **0.3.6** | Development / Framework Support Foundation |
| **0.3.7** | Survival Core (Health, Stamina, Hunger, Thirst, Fatigue, Temperature architecture) |
| **0.3.7a** | Module folder structure cleanup (`Assets/CCS/Modules/`) |
| **0.3.7b** | Foundation cleanup — commit default profile, remove temporary editor tooling |
| **0.3.8** | Character Controller (`Assets/CCS/Modules/CharacterController/`) |
| **0.3.8a** | Batch validation runner + Survival Core editor asmdef fix |
| **0.3.9** | Interaction |
| **0.4.0** | Inventory |

Later milestones continue from **0.4.1+** (Equipment, UI, Crafting, world systems, combat, AI, building, quests, audio, settings finalization).

---

## Folder policy

| Path | Purpose |
|------|---------|
| `Assets/CCS/Modules/` | Gameplay modules — one folder per feature |
| `Assets/CCS/Survival/` | Project shell — bootstrap, scenes, profiles, composition, roadmap docs |
| `Assets/CCS/Framework/` | Reusable Core Platform |

Do **not** add gameplay modules under `Assets/CCS/Survival/Runtime/<ModuleName>/`.

## Roadmap policy

- Manual module registration through `CCS_SurvivalInstaller` (explicit install order).
- Module IDs use reverse-DNS prefix `ccs.survival.*`.
- Editor validation uses **registrable validators** via `CCS_SurvivalValidationPipeline` — modules add validators; menus do not hard-code checks.
- Scene bootstrap profiles declare **Required Services**, **Required Scene Objects**, and **Optional Scene Objects** — modules append entries without changing bootstrap architecture.
- No gameplay mechanics in Core; survival gameplay stays in this repository.

---

## Recommended gameplay module order (after 0.3.6)

| Order | Version (target) | Module area | Module ID (planned) |
|------:|------------------|-------------|---------------------|
| — | **0.3.6** | **Development / Framework Support** | `ccs.survival.development` (support layer) — **Complete** |
| — | 0.3.0 | Character skeleton | `ccs.survival.character` — **Installed** |
| 1 | **0.3.7** | **Survival Core** | `ccs.survival.core` — **Complete** (foundation) |
| 2 | **0.3.8** | **Character Controller** | `ccs.survival.movement` — **Complete** (foundation) |
| 3 | **0.3.9** | **Interaction** | `ccs.survival.interaction` |
| 4 | **0.4.0** | **Inventory** | `ccs.survival.inventory` |
| 5 | 0.4.x | Equipment | `ccs.survival.equipment` |
| 6 | 0.4.x | UI / HUD | `ccs.survival.ui` — **after Inventory** to visually validate each new system |
| 7 | 0.4.x | Crafting | `ccs.survival.crafting` |
| 8 | 0.4.x | World Resources | `ccs.survival.world.resources` |
| 9 | 0.4.x | Save / Load | `ccs.survival.save` |
| 10 | 0.4.x | Time of Day | `ccs.survival.time` |
| 11 | 0.4.x | Weather | `ccs.survival.weather` |
| 12 | 0.4.x | Loot / Spawn | `ccs.survival.loot` |
| 13 | 0.5.x | Combat | `ccs.survival.combat` |
| 14 | 0.5.x | AI / Wildlife | `ccs.survival.ai` |
| 15 | 0.5.x | Building | `ccs.survival.building` |
| 16 | 0.5.x | Quests / Objectives | `ccs.survival.quests` |
| 17 | 0.5.x | Audio | `ccs.survival.audio` |
| 18 | 0.5.x | Settings finalization | `ccs.survival.settings` (player-facing preferences UI) |

**Rationale:** UI immediately after Inventory lets every subsequent system be validated on-screen as it is built.

---

## Testing workflows (not gameplay modules)

| Folder | Purpose |
|--------|---------|
| `Runtime/Development/Testing/Traversal/` | Automated traversal route tests |
| `Runtime/Development/Testing/Simulation/` | Survival simulation / vitals smoke |
| `Runtime/Development/Testing/Inventory/` | Inventory smoke tests |
| `Runtime/Development/Testing/SaveLoad/` | Save/load round-trip tests |

Controlled by `CCS_SurvivalTestToggleProfile` and `CCS_SurvivalTestRuntimeFlags`. No automation in **0.3.6**.

---

## 0.3.6 definition of done

| Criterion | Status |
|-----------|--------|
| Diagnostics foundation (Info / Warning / Error severity) | **Complete** |
| Validation framework (registrable validators + central pipeline) | **Complete** |
| Testing framework (folders + toggle profile reserved categories) | **Complete** |
| Settings foundation | **Complete** |
| Scene bootstrap foundation (required/optional services + scene objects) | **Complete** |
| Documentation updated | **Complete** |
| Version **0.3.6** | **Complete** |
| Git committed and pushed | **Verify** |
| Unity compiles with zero errors | **Verify in Editor** |
| Working tree clean | **Verify** |

---

## Next milestone

**0.3.9 — Interaction Module Foundation**

---

## 0.3.8a definition of done

| Criterion | Status |
|-----------|--------|
| `CCS_SurvivalValidationBatchUtility` + `HasWarnings()` | **Complete** |
| Batchmode exit 0 pass / 1 on warnings or errors | **Complete** |
| Validation menus skip dialogs in batchmode | **Complete** |
| Survival Core stat constructor + Editor asmdef GUID fix | **Complete** |
| Version **0.3.8a** | **Complete** |

---

## 0.3.8 definition of done

| Criterion | Status |
|-----------|--------|
| `Assets/CCS/Modules/CharacterController/` layout | **Complete** |
| Unity `CharacterController` motor (no Rigidbody) | **Complete** |
| Input abstraction + camera-look foundation | **Complete** |
| Stamina drain **request** event (no Survival Core calls) | **Complete** |
| Default profile under `Assets/CCS/Survival/Profiles/CharacterController/` | **Complete** |
| Validation registered on pipeline | **Complete** |
| Version **0.3.8** | **Complete** |
| Bootstrap installer / scene prefab wiring | **Deferred** |

---

## 0.3.7a definition of done

| Criterion | Status |
|-----------|--------|
| `Assets/CCS/Modules/SurvivalCore/` layout | **Complete** |
| Module asmdefs (`CCS.Modules.SurvivalCore.*`) | **Complete** |
| Namespaces `CCS.Modules.SurvivalCore` | **Complete** |
| `SurvivalDiagnosticsLogCategory` compile fix | **Complete** |
| Documentation + version **0.3.7a** | **Complete** |

---

## 0.3.7b definition of done

| Criterion | Status |
|-----------|--------|
| Default profile committed under `Assets/CCS/Survival/Profiles/SurvivalCore/` | **Complete** |
| Profile creation menu removed | **Complete** |
| Development testing convenience menus removed | **Complete** |
| Validation pipeline + menus retained | **Complete** |
| Version **0.3.7b** | **Complete** |

---

## 0.3.7 definition of done

| Criterion | Status |
|-----------|--------|
| Stat architecture (6 stats) | **Complete** |
| Profiles + default asset tool | **Complete** |
| `CCS_SurvivalCoreService` + events | **Complete** |
| Validation registered on pipeline | **Complete** |
| Documentation | **Complete** |
| Version **0.3.7** | **Complete** |
| Bootstrap installer wiring | **Deferred** |

---

## Related

- [Survival Core Module](../../Modules/SurvivalCore/Documentation/CCS_Survival_Core_Module.md)
- [Character Controller Module](../../Modules/CharacterController/Documentation/CCS_CharacterController_Module.md)
- [Gameplay modules index](../../Modules/README.md)
- [Development Framework Support](CCS_Survival_Development_Framework_Support.md)
- [Future Gameplay Module Guidelines](Future_Gameplay_Module_Guidelines.md)
- [Framework Architecture Guide](Framework_Architecture_Guide.md)
