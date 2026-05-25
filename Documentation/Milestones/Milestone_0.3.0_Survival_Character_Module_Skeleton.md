# Milestone 0.3.0 — Survival Character Module Skeleton

**Version:** 0.3.0  
**Status:** Implementation milestone  
**Author:** James Schilz  
**Date:** 2026-05-24  
**Predecessor:** [Milestone 0.2.0](Milestone_0.2.0_Survival_Bootstrap_Scene_Empty_Install_Pipeline.md)

**Goal:** First survival-owned gameplay module boundary (`ccs.survival.character`) without character mechanics.

---

## Scope

### In scope

- [x] `Runtime/Character/` module skeleton (module, installer, diagnostics constants)
- [x] `CCS.Survival.Runtime` asmdef at `Runtime/` root (references `CCS.Core.Runtime` only)
- [x] Survival runtime restructure under `Assets/CCS/Survival/Runtime/`
- [x] Character module install via `CCS_SurvivalInstaller` composition root
- [x] Survival diagnostics expect one registered module at 0.3.0

### Out of scope

- [ ] Movement, camera, player controller
- [ ] Attributes, hunger, thirst
- [ ] Inventory, crafting, equipment, hotbar
- [ ] Combat, AI, save, networking packages
- [ ] Core modifications

---

## Module identity

| Field | Value |
|-------|--------|
| **Module ID** | `ccs.survival.character` |
| **Type** | `CCS_SurvivalCharacterModule` : `CCS_ModuleBase` |
| **Installer** | `CCS_SurvivalCharacterModuleInstaller` : `CCS_ModuleInstallerBase` |

---

## Related documents

- [Survival README](../../Assets/CCS/Survival/README.md)
- [Survival gameplay architecture](../Architecture/Survival_Gameplay_Architecture.md)
