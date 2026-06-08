# Project Scene Bootstrap Standards

**Milestone:** 0.3.4  
**Author:** James Schilz  
**Date:** 2026-05-24

Project bootstrap scenes must start from a predictable, AAA-safe composition root before gameplay systems are added.

---

## Composition root (required)

Place on **one** GameObject (e.g. `PF_CCS_Survival_BootstrapRoot`):

| Component | Role |
|-----------|------|
| `CCS_RuntimeHost` | Core runtime host (single instance per scene) |
| `CCS_SurvivalBootstrap` | Survival composition root: context, installer pipeline, diagnostics |

**Rules:**

- Exactly **one** `CCS_RuntimeHost` per project bootstrap scene.
- Exactly **one** `CCS_SurvivalBootstrap` on the **same** GameObject as the host.
- Project scenes **own project diagnostics** — keep Core runtime diagnostics **disabled** unless testing Core in `SCN_CCS_Bootstrap`.

---

## Startup flow

```text
CCS_RuntimeHost.Awake (Core init)
    → CCS_SurvivalBootstrap.Awake
        → CCS_SurvivalRuntimeContext.Initialize()
        → CCS_SurvivalInstaller pipeline (modules)
        → CCS_SurvivalDiagnostics (when enabled)
```

Validation runs **once** during bootstrap/diagnostics — not per frame.

---

## Skeleton expectations

| Check | Expected |
|-------|----------|
| Modules | 1 (`ccs.survival.character`) |
| Services | 0 |
| Update systems | 0 |
| Bootstrap installers | 1 |

No project-registered services or updatables during skeleton phase unless a milestone explicitly allows them.

---

## Scene identity vs save identity

> **Scene names, hierarchy paths, and GameObject names are not save identity.**

Use stable IDs instead:

- Module IDs (`ccs.survival.*`)
- Authority / avatar IDs
- Profile IDs (`ccs.survival.profile.*`)
- Bootstrap profile slot IDs (`ccs.survival.bootstrap.slot.*`)

Never persist Unity asset paths or scene object references as authoritative keys.

---

## Profile slots (optional, setup only)

`CCS_SurvivalBootstrap` may serialize optional `CCS_SurvivalBootstrapProfileSlot` entries:

| Field | Purpose |
|-------|---------|
| `slotId` | Stable setup slot ID (`ccs.survival.bootstrap.slot.*`) |
| `profile` | Optional `CCS_SurvivalProfileBase` asset reference |
| `isRequired` | When true, profile must be assigned and valid |

**Rules:**

- Profiles configure **setup** only — not runtime simulation state.
- Leave slots **empty** during skeleton phase.
- No `Resources`, Addressables, or dynamic loading in foundation layer.
- Save systems must **not** depend on bootstrap slots directly.

---

## Reference implementation

| Type | Path |
|------|------|
| Scene rules | `Runtime/Foundation/Scene/CCS_SurvivalSceneBootstrapRules.cs` |
| Scene validation | `Runtime/Foundation/Scene/CCS_SurvivalSceneBootstrapValidationUtility.cs` |
| Profile slot | `Runtime/Foundation/Bootstrap/CCS_SurvivalBootstrapProfileSlot.cs` |
| Bootstrap scene | `Scenes/SCN_CCS_Survival_Bootstrap.unity` |

---

## Related

- [Milestone 0.3.4](Milestones/Milestone_0.3.4_Survival_Scene_Bootstrap_Standards.md)
- [Project README](../README.md)
