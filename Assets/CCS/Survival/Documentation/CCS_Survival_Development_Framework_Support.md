# CCS Survival — Development Framework Support

**Milestone:** 0.3.6 — Development Framework Support Foundation  
**Status:** Complete  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-28  
**Predecessor:** [Milestone 0.3.5](Milestones/Milestone_0.3.5_Survival_Framework_Quality_Gate.md) (`v0.3.5a`)

**Goal:** Provide reusable development infrastructure (diagnostics, validation, testing toggles, settings placeholders, scene bootstrap profiles) before gameplay modules begin.

---

## Purpose by area

| Area | Purpose | Runtime / Editor |
|------|---------|------------------|
| **Diagnostics** | Lightweight runtime messages and system state tracking without module coupling | Runtime |
| **Validation** | Reusable editor validation report structure and menu-driven project checks | Editor |
| **Testing** | Dev-only test toggle profiles and runtime flags for future automation | Runtime + Editor |
| **Settings** | Placeholder settings profile/service for future graphics/audio/input/accessibility | Runtime |
| **Scene Bootstrap** | Optional scene bootstrap profile, bootstrapper, and validation helpers | Runtime + Editor |

---

## Folder structure

```text
Assets/CCS/Survival/
  Runtime/Development/
    Diagnostics/
    Testing/
    Settings/
    Bootstrap/
  Editor/Development/
    Diagnostics/
    Validation/
    Testing/
    Bootstrap/
  Documentation/
```

---

## Diagnostics

| Script | Role |
|--------|------|
| `CCS_SurvivalDiagnosticsState` | Unknown / Initializing / Ready / Warning / Error |
| `CCS_SurvivalDiagnosticsMessage` | Immutable diagnostic payload (source id, message, state, timestamp) |
| `CCS_SurvivalDiagnosticsService` | Instance-owned hub; `MessageAdded` and `SystemStateChanged` events |

Future gameplay modules report status through stable system ids (for example `ccs.survival.development.validation`) without referencing each other directly.

---

## Validation

| Script | Role |
|--------|------|
| `CCS_SurvivalValidationIssueSeverity` | Info / Warning / Error |
| `CCS_SurvivalValidationIssue` | Single report line |
| `CCS_SurvivalValidationReport` | Aggregated issues + summary |
| `CCS_SurvivalValidationUtility` | 0.3.6 folder/config checks |
| `CCS_SurvivalValidationMenu` | **CCS → Survival → Validation → Run Survival Validation** |

Future modules append checks to `CCS_SurvivalValidationUtility` or dedicated validators registered by feature area.

---

## Testing

| Script | Role |
|--------|------|
| `CCS_SurvivalTestToggleProfile` | ScriptableObject dev toggles (traversal, simulation, inventory smoke — reserved) |
| `CCS_SurvivalTestRuntimeFlags` | Static runtime mirror of profile toggles |
| `CCS_SurvivalTestingMenu` | Reset flags / apply selected profile |

No gameplay automation in 0.3.6.

---

## Settings

| Script | Role |
|--------|------|
| `CCS_SurvivalSettingsProfile` | Placeholder preference ScriptableObject |
| `CCS_SurvivalSettingsService` | Safe defaults when profile is null |

No settings UI in 0.3.6.

---

## Scene Bootstrap

| Script | Role |
|--------|------|
| `CCS_SurvivalSceneBootstrapProfile` | Required host/bootstrap/service expectation profile |
| `CCS_SurvivalSceneBootstrapper` | Optional MonoBehaviour; validates profile on Awake |
| `CCS_SurvivalSceneBootstrapValidationUtility` | Development-layer active scene checks |
| `CCS_SurvivalSceneBootstrapValidationMenu` | **CCS → Survival → Bootstrap → Validate Active Scene Bootstrap** |

Complements foundation `Runtime/Foundation/Scene/CCS_SurvivalSceneBootstrapValidationUtility.cs` — does not replace it.

---

## Editor assembly

`Assets/CCS/Survival/Editor/CCS.Survival.Editor.asmdef` references `CCS.Survival.Runtime` and `CCS.Core.Runtime`. Editor scripts live under `Editor/` only.

---

## Done criteria (0.3.6)

| Check | Status |
|-------|--------|
| Development folder structure created | **Done** |
| Diagnostics runtime foundation | **Done** |
| Validation editor foundation + menu | **Done** |
| Testing toggle foundation + menu | **Done** |
| Settings placeholder service/profile | **Done** |
| Scene bootstrap profile/bootstrapper/validation | **Done** |
| Module roadmap + this document | **Done** |
| `bundleVersion` = **0.3.6** | **Done** |
| Scripts compile; no Editor in runtime asmdef | **Verify in Unity** |
| Gameplay modules not started | **Done** |

---

## Next milestone

Gameplay modules begin **after 0.3.6**. See [CCS_Survival_Module_Roadmap.md](CCS_Survival_Module_Roadmap.md) for recommended order.

---

## Related

- [Framework Architecture Guide](Framework_Architecture_Guide.md)
- [Future Gameplay Module Guidelines](Future_Gameplay_Module_Guidelines.md)
- [Module Roadmap](CCS_Survival_Module_Roadmap.md)
