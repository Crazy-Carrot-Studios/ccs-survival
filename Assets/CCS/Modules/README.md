# CCS Modules

Gameplay and game systems live under `Assets/CCS/Modules/`. Each feature is an isolated module with its own runtime assembly, content, prefabs, profiles, tests, and documentation.

## Module structure

```text
Assets/CCS/Modules/<Feature>/
+-- Runtime/
+-- Editor/
+-- Content/
+-- Prefabs/
+-- Profiles/
+-- Documentation/
+-- Tests/
```

## Rules

- One module per feature folder.
- Module-owned data stays inside the module.
- Each rebuilt module must include validation and a working test prefab or scene object.
- Register modules through the Project installer pipeline in explicit order.

## Placeholder modules

CharacterController, Interaction, Inventory, Equipment, Crafting, Hotbar, SaveSystem, UI