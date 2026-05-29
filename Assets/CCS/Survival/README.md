# CCS Survival — Project Shell

**Milestone:** 0.3.6 — Development Framework Support Foundation  
**Author:** James Schilz  
**Date:** 2026-05-28

## Development Framework Support (0.3.6)

Pre-gameplay development infrastructure added at **v0.3.6**:

| Area | Path |
|------|------|
| Runtime development | `Runtime/Development/` |
| Editor development | `Editor/Development/` |
| Module roadmap | [Documentation/CCS_Survival_Module_Roadmap.md](Documentation/CCS_Survival_Module_Roadmap.md) |
| Support guide | [Documentation/CCS_Survival_Development_Framework_Support.md](Documentation/CCS_Survival_Development_Framework_Support.md) |

**Editor menus:**

- **CCS → Survival → Validation → Run Survival Validation**
- **CCS → Survival → Bootstrap → Validate Active Scene Bootstrap**
- **CCS → Survival → Testing →** reset/apply test toggle profiles

## Framework Quality Gate Completed (0.3.5)

Pre-gameplay foundation audit completed at **v0.3.5**:

- Architecture boundaries verified (Survival → Core only)
- Identity philosophy documented and aligned (authority, avatar, profile, scene, runtime)
- Validation deduplicated and bootstrap-time only
- Diagnostics ownership confirmed (survival-owned)
- Contributor guides and anti-patterns published
- `CCS_SurvivalFrameworkFutureMarkers` added for planned integrations

**Authoritative guides:**

- [Framework Architecture Guide](Documentation/Framework_Architecture_Guide.md)
- [Future Gameplay Module Guidelines](Documentation/Future_Gameplay_Module_Guidelines.md)
- [Scene Bootstrap Standards](Documentation/Scene_Bootstrap_Standards.md)

## Scene bootstrap standards (0.3.4)

| Rule | Detail |
|------|--------|
| Composition root | One `CCS_RuntimeHost` + one `CCS_SurvivalBootstrap` |
| Context | `CCS_SurvivalRuntimeContext` |
| Profile slots | Optional; default empty |

## Authority vs Avatar (0.3.3)

| Layer | Contract |
|-------|----------|
| Authority | `CCS_ISurvivalAuthority` — ownership, stable IDs |
| Avatar | `CCS_ISurvivalAvatar` — scene representation |

## Foundation layer

| Type | Path |
|------|------|
| Constants / FUTURE markers | `Runtime/Foundation/Diagnostics/` |
| Scene rules | `Runtime/Foundation/Scene/` |
| Profile slot | `Runtime/Foundation/Bootstrap/` |
| Validation | `Runtime/Foundation/Validation/` |
| Profiles | `Runtime/Foundation/Profiles/` |
| Development support | `Runtime/Development/`, `Editor/Development/` |

## Skeleton diagnostics expectations

| Check | Expected |
|-------|----------|
| Modules | 1 |
| Services | 0 |
| Update systems | 0 |
| Bootstrap installers | 1 |

## What it does not own yet

- Gameplay mechanics, movement, input, controller, networking, save implementation
- Inventory, attributes, combat, AI
- Required profile slots or gameplay profile assets

## Runtime assemblies

| Assembly | References |
|----------|------------|
| `CCS.Survival.Runtime` | `CCS.Core.Runtime` |
| `CCS.Survival.Editor` | `CCS.Core.Runtime`, `CCS.Survival.Runtime` |

## Related documentation

- [In-Unity doc index](Documentation/README.md)
- [Module roadmap](Documentation/CCS_Survival_Module_Roadmap.md)
- [Milestone 0.3.5](Documentation/Milestones/Milestone_0.3.5_Survival_Framework_Quality_Gate.md)
