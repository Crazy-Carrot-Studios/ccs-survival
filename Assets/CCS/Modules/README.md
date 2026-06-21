# CCS Modules

Gameplay and game systems live under `Assets/CCS/Modules/`. Each feature is an isolated module with its own runtime assembly, content, prefabs, profiles, tests, and documentation.

## Active modules

| Module | Status |
|--------|--------|
| CharacterController | v0.2.4 — movement, camera, Master Test scene, netcode harness |
| Attributes | v0.3.0 — attribute model, Health, replication, test HUD |
| Interaction | v0.5.4 — pickup/door flow, forward volume, closest-point LOS, prompt HUD |
| Weapons | v0.6.0 — test revolver hitscan, aim/fire/reload, damage target |

## Test-player integration (intentional)

The canonical test player prefab integrates multiple modules:

- **CharacterController.Runtime** references **Attributes.Runtime** (health HUD, stamina) and **Interaction.Runtime** (scanner, lock contracts).
- **Interaction** does not reference CharacterController at runtime; integration is wired on the test player prefab and via CharacterController types (`CCS_PlayerInteractionAnimator`).

**Future cleanup:** If module coupling grows, introduce a dedicated bridge assembly (e.g. `CharacterController.InteractionBridge`) instead of expanding direct runtime references.

## Module structure

When a module is created, use this layout:

```text
Assets/CCS/Modules/<Feature>/
+-- Runtime/
+-- Editor/
+-- Content/
+-- Prefabs/
+-- Profiles/
+-- Documentation/
+-- Tests/
+-- UI/              (only when the module needs UI assets)
```

## Rules

- One module per feature folder.
- Module-owned data stays inside the module.
- Each module must include validation and a working test prefab or test scene asset before moving on.
- Module validation menus belong under `CCS/Modules/<Module Name>/`, not under `CCS/Project/`.
- Register modules through the Project installer pipeline in explicit order.
- Do not scaffold unused module folders ahead of implementation.
