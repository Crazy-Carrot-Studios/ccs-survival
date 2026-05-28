# CCS Survival — Project Shell

**Milestone:** 0.3.5 — Survival Framework Quality Gate + Documentation Pass  
**Author:** James Schilz  
**Date:** 2026-05-24

## Framework Quality Gate Completed

Pre-gameplay foundation audit completed at **v0.3.5**:

- Architecture boundaries verified (Survival → Core only)
- Identity philosophy documented and aligned (authority, avatar, profile, scene, runtime)
- Validation deduplicated and bootstrap-time only
- Diagnostics ownership confirmed (survival-owned)
- Contributor guides and anti-patterns published
- `CCS_SurvivalFrameworkFutureMarkers` added for planned integrations

**Authoritative guides:**

- [Gameplay Constitution](../Documentation/Gameplay/CCS_Survival_Gameplay_Constitution.md) — foundational design direction (what the game is)
- [Gameplay Systems Breakdown](../Documentation/Gameplay/CCS_Survival_Gameplay_Systems_Breakdown.md) — modules, dependencies, save boundaries (how systems split)
- [Gameplay Loop Specification](../Documentation/Gameplay/CCS_Survival_Gameplay_Loop_Specification.md) — player flow, pacing, onboarding, vertical slice
- [Reputation & Law Design Spec](../Documentation/Gameplay/CCS_Survival_Reputation_And_Law_Design_Spec.md) — reputation scale, crime, bounties, war, raids
- [Settlement & Territory Design Spec](../Documentation/Gameplay/CCS_Survival_Settlement_And_Territory_Design_Spec.md) — claims, town growth, influence, permissions, economy, decay
- [Economy & Logistics Design Spec](../Documentation/Gameplay/CCS_Survival_Economy_And_Logistics_Design_Spec.md) — player economy, regional pricing, quality, transport, and scarcity dynamics
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

## Runtime assembly

`Assets/CCS/Survival/Runtime/CCS.Survival.Runtime.asmdef` references **`CCS.Core.Runtime` only**.

## Related documentation

- [In-Unity doc index](Documentation/README.md)
- [Milestone 0.3.5](Documentation/Milestones/Milestone_0.3.5_Survival_Framework_Quality_Gate.md)
