# CCS Survival — Build Verification

**Milestone:** 0.4.1b — Prototype Scene Build Verification  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-28  
**Status:** Bootstrap scene + Windows development build verification

---

## Purpose

Verify that the survival bootstrap prototype scene compiles, validates, and produces a runnable Windows development build **before** UI/HUD work begins.

This milestone intentionally excludes gameplay UI, final player prefab wiring, Cinemachine, and gameplay camera controllers.

---

## Scene used

| Item | Path |
|------|------|
| Bootstrap scene | `Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity` |
| Bootstrap prefab instance | `Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab` |

---

## Camera requirements

The bootstrap scene must contain **exactly one** active Main Camera:

| Requirement | Value |
|-------------|-------|
| GameObject name | `Main Camera` |
| Tag | `MainCamera` |
| Audio Listener | Present on Main Camera only |
| Duplicate MainCamera tags | None |
| Gameplay camera controller | **Not included** |
| Cinemachine | **Not included** |

Default verification camera transform:

- Position: `(0, 4, -8)`
- Rotation: `(20, 0, 0)` — forward/down view of scene root content

---

## Build verification reference object

Non-gameplay visual reference only:

| Object | Purpose |
|--------|---------|
| `CCS_BuildVerificationScene` | Scene organization root |
| `CCS_BuildVerificationGround` | Simple plane primitive (no collider) for render verification |

Not final environment art.

---

## Build settings

| Setting | Policy |
|---------|--------|
| Survival bootstrap in Build Settings | **Required**, listed first |
| Framework bootstrap scene | Enabled (secondary) |
| Startup scene for 0.4.1b Windows build | `SCN_CCS_Survival_Bootstrap` |

---

## Build output path

Windows development build output (gitignored):

```text
Builds/CCS_Survival_0.4.1b_Windows/
  CCS_Survival.exe
```

Do **not** commit `Builds/` output.

---

## Validation requirements (before tag)

Run Unity batch compile, then all validation menus (policy: **0 warnings, 0 errors**):

| Validation | Batch entry |
|------------|-------------|
| Survival | `CCS.Survival.Editor.Development.CCS_SurvivalValidationMenu.RunSurvivalValidation` |
| Survival Core | `CCS.Modules.SurvivalCore.Editor.CCS_SurvivalCoreValidationMenu.ValidateSurvivalCore` |
| Character Controller | `CCS.Modules.CharacterController.Editor.CCS_CharacterControllerValidationMenu.ValidateCharacterController` |
| Interaction | `CCS.Modules.Interaction.Editor.CCS_InteractionValidationMenu.ValidateInteraction` |
| Inventory | `CCS.Modules.Inventory.Editor.CCS_InventoryValidationMenu.ValidateInventory` |
| Equipment | `CCS.Modules.Equipment.Editor.CCS_EquipmentValidationMenu.ValidateEquipment` |

Expected `bundleVersion`: **0.4.1b** (`ProjectSettings` → Player → Version).

---

## Batch tooling

| Step | Execute method |
|------|----------------|
| Scene setup | `CCS.Survival.Editor.Development.CCS_SurvivalBuildVerificationSceneSetup.ExecuteBatch` |
| Windows dev build | `CCS.Survival.Editor.Development.CCS_SurvivalBuildVerificationBuildRunner.ExecuteBatch` |

Example:

```powershell
$unity = "C:\Program Files\Unity\Hub\Editor\6000.4.1f1\Editor\Unity.exe"
$project = "<PROJECT_PATH>"

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Survival.Editor.Development.CCS_SurvivalBuildVerificationSceneSetup.ExecuteBatch `
  -logFile Logs/CCS_BuildVerificationSceneSetup.log

& $unity -batchmode -quit -projectPath $project `
  -executeMethod CCS.Survival.Editor.Development.CCS_SurvivalBuildVerificationBuildRunner.ExecuteBatch `
  -logFile Logs/CCS_BuildVerificationBuild.log
```

---

## Build verification checklist

| Check | Expected |
|-------|----------|
| Compile | 0 errors |
| Batch validations | 0 warnings, 0 errors |
| Windows development build | Succeeds |
| Build warnings | 0 (policy) |
| Launch | Window opens, scene loads, Main Camera renders skybox + ground reference |
| Version visible in UI | **Not required** — confirm via `PlayerSettings.bundleVersion` and build log |

---

## Intentionally not included (0.4.1b)

| Feature | Status |
|---------|--------|
| UI / HUD | Deferred to next milestone |
| Gameplay camera controller | Deferred |
| Cinemachine | Deferred |
| Final player prefab wiring | Deferred |
| Save/load | Deferred |
| Crafting / storage world objects | Deferred |

---

## Next milestone

**0.4.x — UI / HUD** (after build verification passes)
