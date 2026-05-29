# CCS Gameplay Modules

**Version:** 0.3.7a  
**Author:** James Schilz  
**Date:** 2026-05-28

## Folder rules

| Path | Purpose |
|------|---------|
| `Assets/CCS/Modules/` | Gameplay feature modules (`ccs.survival.*` module IDs) |
| `Assets/CCS/Survival/` | Survival project shell — bootstrap, scenes, profiles, composition, project roadmap docs |
| `Assets/CCS/Framework/` | Reusable Core Platform (upstream-aligned) |

Do **not** place gameplay modules under `Assets/CCS/Survival/Runtime/<ModuleName>/`.

## Standard module layout

```text
Assets/CCS/Modules/<ModuleName>/
  Runtime/           # Runtime scripts + CCS.Modules.<Name>.Runtime.asmdef
  Editor/            # Editor scripts + CCS.Modules.<Name>.Editor.asmdef
  Documentation/     # Module-specific docs (optional)
  Tests/             # Feature tests (optional)
  Prefabs/           # Feature prefabs (optional)
  Settings/          # ScriptableObject config (optional)
  UI/                # Feature UI (optional)
```

## Namespaces

- Runtime: `CCS.Modules.<ModuleName>`
- Editor: `CCS.Modules.<ModuleName>.Editor`
- Survival shell/bootstrap only: `CCS.Survival.*`

## Planned modules (roadmap)

| Folder | Milestone |
|--------|-----------|
| `SurvivalCore/` | 0.3.7 / 0.3.7a |
| `CharacterController/` | 0.3.8 |
| `Interaction/` | 0.3.9 |
| `Inventory/` | 0.4.0 |

See [Survival Module Roadmap](../Survival/Documentation/CCS_Survival_Module_Roadmap.md).
