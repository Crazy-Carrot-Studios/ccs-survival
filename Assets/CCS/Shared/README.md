# CCS Shared — Cross-Module Assets

**Location:** `Assets/CCS/Shared/`  
**Author:** James Schilz  
**Date:** 2026-06-07

## Purpose

Shared holds **only** assets or code used by **multiple gameplay modules**. It is not a dumping ground for module-specific content.

## Allowed

- Shared cross-module runtime contracts (when referenced by 2+ modules)
- Shared debug prefabs used across module test harnesses
- Common materials, fonts, or UI chrome used by multiple modules
- Gameplay tags or ID registries **only** when consumed by multiple modules

## Not allowed

- Module-owned data (items, recipes, loot tables, characters) — keep inside the owning module
- Bootstrap or composition roots — belong in `Assets/CCS/Project/`
- Core platform code — belongs in `Assets/CCS/Framework/`
- Global `Database/` folders — module data lives under `Assets/CCS/Modules/<Feature>/`

## Folder guide

| Folder | Intended use |
|--------|----------------|
| `Scenes/` | Cross-module test or sandbox scenes (not project bootstrap) |
| `Art/`, `Audio/`, `Materials/`, `Textures/` | Reusable presentation assets |
| `Prefabs/` | Shared debug or utility prefabs |
| `ScriptableObjects/` | Cross-module registries only |
| `UI/`, `VFX/`, `Shaders/` | Shared presentation systems |

## Dependency rule

Shared may reference `CCS.Core.Runtime`. Shared must **not** reference `CCS.Project.Runtime` or individual module assemblies unless a proven cross-cutting contract requires it (document the exception).
