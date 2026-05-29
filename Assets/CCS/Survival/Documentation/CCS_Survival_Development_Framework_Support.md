# CCS Survival — Development Framework Support

**Milestone:** 0.3.6 — Development Framework Support Foundation  
**Status:** Complete (review refinements applied)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-28  
**Predecessor:** [Milestone 0.3.5](Milestones/Milestone_0.3.5_Survival_Framework_Quality_Gate.md) (`v0.3.5a`)

**Goal:** Reusable development infrastructure before gameplay modules. Gameplay begins at **0.3.7 Survival Core**.

---

## Version sequence (after framework rollback)

| Version | Milestone |
|---------|-----------|
| 0.3.5 / 0.3.5a | Survival framework quality gate |
| **0.3.6** | Development Framework Support (**this milestone**) |
| 0.3.7 | Survival Core |
| 0.3.8 | Character Controller |
| 0.3.9 | Interaction |
| 0.4.0 | Inventory |

---

## Purpose by area

| Area | Purpose | Runtime / Editor |
|------|---------|------------------|
| **Diagnostics** | Runtime messages with **Info / Warning / Error** severity; lifecycle state tracking | Runtime |
| **Validation** | **Registrable validators** + central report pipeline; menus do not own checks | Editor |
| **Testing** | Dev toggles + reserved folders for Traversal, Simulation, Inventory, SaveLoad | Runtime + Editor |
| **Settings** | Placeholder profile/service for future player preferences | Runtime |
| **Scene Bootstrap** | Profile lists: **Required Services**, **Required Scene Objects**, **Optional Scene Objects** | Runtime + Editor |

---

## Folder structure

```text
Assets/CCS/Survival/
  Runtime/Development/
    Diagnostics/
    Testing/
      Traversal/      (reserved)
      Simulation/     (reserved)
      Inventory/      (reserved)
      SaveLoad/       (reserved)
    Settings/
    Bootstrap/
  Editor/Development/
    Validation/
    Testing/
    Bootstrap/
  Documentation/
```

---

## Diagnostics

| Script | Role |
|--------|------|
| `CCS_SurvivalDiagnosticsSeverity` | **Info**, **Warning**, **Error** |
| `CCS_SurvivalDiagnosticsState` | Lifecycle: Unknown / Initializing / Ready / Warning / Error |
| `CCS_SurvivalDiagnosticsMessage` | Payload: source id, message, severity, state, timestamp |
| `CCS_SurvivalDiagnosticsService` | Instance hub; `MessageAdded` / `SystemStateChanged` events |

Future modules call `ReportMessage(sourceId, detail, CCS_SurvivalDiagnosticsSeverity.*)` without coupling to each other.

---

## Validation (central pipeline)

| Script | Role |
|--------|------|
| `CCS_ISurvivalValidationValidator` | Contract: `ValidatorId` + `Validate(report)` |
| `CCS_SurvivalValidationPipeline` | Registers validators; `RunAll()` builds one report |
| `CCS_SurvivalFoundationValidationValidator` | 0.3.6 folder/version checks |
| `CCS_SurvivalValidationUtility` | Facade — **do not add checks here** |
| `CCS_SurvivalValidationMenu` | **CCS → Survival → Validation → Run Survival Validation** |

**Adding a module validator (future Combat, Building, AI):**

```csharp
CCS_SurvivalValidationPipeline.RegisterValidator(new MyFeatureValidationValidator());
```

Do **not** extend menu classes with hard-coded rules.

---

## Testing

| Script / folder | Role |
|-----------------|------|
| `CCS_SurvivalTestToggleProfile` | Toggles: diagnostics, traversal, simulation, inventory, **save/load** |
| `CCS_SurvivalTestRuntimeFlags` | Static mirror for runtime checks |
| `CCS_SurvivalTestingMenu` | Reset flags / apply selected profile |
| `Testing/Traversal/` | Reserved automated traversal tests |
| `Testing/Simulation/` | Reserved survival simulation tests |
| `Testing/Inventory/` | Reserved inventory smoke tests |
| `Testing/SaveLoad/` | Reserved save/load tests |

No gameplay automation in 0.3.6.

---

## Settings

| Script | Role |
|--------|------|
| `CCS_SurvivalSettingsProfile` | Placeholder preference ScriptableObject |
| `CCS_SurvivalSettingsService` | Safe defaults when profile is null |

Player-facing settings UI is **Settings finalization** (late roadmap), not 0.3.6.

---

## Scene Bootstrap

| Script | Role |
|--------|------|
| `CCS_SurvivalSceneBootstrapServiceRequirement` | Required service contract entry |
| `CCS_SurvivalSceneBootstrapRequirementEntry` | Required/optional scene object entry |
| `CCS_SurvivalSceneBootstrapProfile` | **Required Services**, **Required Scene Objects**, **Optional Scene Objects** |
| `CCS_SurvivalSceneBootstrapper` | Optional MonoBehaviour; validates on Awake |
| `CCS_SurvivalSceneBootstrapValidationUtility` | Active scene + profile validation |
| `CCS_SurvivalSceneBootstrapValidationMenu` | **CCS → Survival → Bootstrap → Validate Active Scene Bootstrap** |

Lists may be **empty** in 0.3.6. Modules append requirements later without changing bootstrap architecture.

---

## Definition of done (0.3.6)

| Criterion | Expected |
|-----------|----------|
| Diagnostics foundation complete | **Yes** — severity + service |
| Validation framework complete | **Yes** — pipeline + registrable validators |
| Testing framework complete | **Yes** — profile + four reserved folders |
| Settings foundation complete | **Yes** |
| Scene bootstrap foundation complete | **Yes** — services + scene object lists |
| Documentation updated | **Yes** — this doc + module roadmap |
| Version **0.3.6** | **Yes** — `bundleVersion` + READMEs |
| Git committed and pushed | **Yes** |
| Unity compiles, zero errors | **Verify** |
| Working tree clean | **Verify** |

---

## Next milestone

**0.3.7 — Survival Core Module Foundation**

- Health, Stamina, Hunger, Thirst, Fatigue, Temperature architecture
- First gameplay module after development support

---

## Related

- [Module Roadmap](CCS_Survival_Module_Roadmap.md)
- [Framework Architecture Guide](Framework_Architecture_Guide.md)
- [Future Gameplay Module Guidelines](Future_Gameplay_Module_Guidelines.md)
