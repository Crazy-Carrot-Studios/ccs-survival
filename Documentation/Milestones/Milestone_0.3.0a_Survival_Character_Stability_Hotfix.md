# Milestone 0.3.0a — Survival Character Stability Hotfix

**Version:** 0.3.0a  
**Status:** Stability hotfix  
**Author:** James Schilz  
**Date:** 2026-05-24  
**Predecessor:** [Milestone 0.3.0](Milestone_0.3.0_Survival_Character_Module_Skeleton.md) (`v0.3.0`)

**Goal:** Make `v0.3.0` reproducibly compilable on a fresh clone without architecture or gameplay changes.

---

## Fixes (no new features)

| Issue | Fix |
|-------|-----|
| Invalid 33-character Unity GUIDs on Character script `.meta` files | Corrected to valid 32-character GUIDs |
| `namespace CCS.Survival.Character` resolution failures | Normalized to `namespace CCS.Survival` in one assembly |
| `using CCS.Survival.Character` in bootstrap/installer | Removed (same assembly) |
| Partial asmdef move to `Survival/` root | Reverted — **`Runtime/CCS.Survival.Runtime.asmdef` remains canonical** |

---

## Unchanged

- Module ID: `ccs.survival.character`
- Bootstrap flow and survival composition root
- Assembly references: `CCS.Survival.Runtime` → `CCS.Core.Runtime` only
- No gameplay mechanics added

---

## Related documents

- [Survival README](../../Assets/CCS/Survival/README.md)
- [Milestone 0.3.0](Milestone_0.3.0_Survival_Character_Module_Skeleton.md)
