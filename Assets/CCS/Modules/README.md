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

## Modules

| Module | Status |
|--------|--------|
| CharacterController | v0.2.0 foundation — movement, camera, test prefab, validation |
| Interaction | Placeholder |
| Inventory | Placeholder |
| Equipment | Placeholder |
| Crafting | Placeholder |
| Hotbar | Placeholder |
| SaveSystem | Placeholder |
| UI | Placeholder |