# CCS Character Controller Module

**Version:** 0.7.1e — living module overview  
**Author:** James Schilz  
**Last updated:** 2026-06-25

## Purpose

Profile-driven third-person movement, Cinemachine camera control, revolver upper-body animation, and equipment socket fitting for `ccs-survival`. The module ships a unified network-capable test player for solo Master Test and multiplayer hosting flows.

**Non-goals:** AI combat logic, weapons hitscan resolver, inventory database (sibling modules).

## Current safe baseline

| Milestone | Scope |
|-----------|-------|
| **v0.7.1a** | Gameplay baseline — locomotion, revolver aim/fire, AI combat, weapons pickup, hosting |
| **v0.7.1a** | AI health bar fill direction hotfix (signed off) |
| **v0.7.1c** | Editor/documentation cleanup — Animation Fit Studio removed; no gameplay behavior changes |
| **v0.7.1d** | Testing Manager foundation + editor menu reduction; no gameplay behavior changes |
| **v0.7.1e** | Player prefab component audit + test-only separation readiness; no prefab rewrite |

Working systems that must remain stable unless a dedicated, batch-validated milestone approves changes:

- Master Test spawn, movement, camera, revolver aim/fire
- AI bandit combat and health bar
- Weapons pickup and validation
- Multiplayer hosting scene
- `AC_CCS_Player_Locomotion_StarterAssets.controller` wiring
- Production animation clips (including `_FitTest` AimPitch blend clips)

## Runtime / editor / test boundaries

| Layer | Location | Rules |
|-------|----------|-------|
| **Runtime** | `Runtime/` | Production gameplay assemblies. No Editor dependencies. No test harness types. |
| **Editor** | `Editor/` | Builders, validators, batch entries, Equipment Fit Studio. Not required at runtime. |
| **Tests** | `Tests/` | Master Test scene harness, hosting batch entries, spawners, diagnostics switchboard. Not referenced by production Runtime asmdef. |

**Editor tools are not gameplay dependencies.** Runtime must compile and run without Editor-only tooling.

## Canonical test player

| Asset | Path |
|-------|------|
| Network-capable test player | `Tests/Prefabs/PF_CCS_CharacterController_TestPlayer_Networked.prefab` |
| Player visual | `Characters/Player/Prefabs/PF_CCS_Player_Visual.prefab` |
| Locomotion controller | `Characters/Player/Animations/Controllers/AC_CCS_Player_Locomotion_StarterAssets.controller` |

### Solo Master Test

- `CCS_MasterTestSpawnController` instantiates the networked prefab when no Netcode session is active.
- `CCS_TestPlayerOfflineBootstrap` + `CCS_TestPlayerLocalSessionConfigurator` enable local input/motor/camera.
- Master Test has **no scene-placed player** and **no scene NetworkManager**.

### Multiplayer

- `PF_CCS_TestNetworkManager` in `SCN_CCS_MultiplayerHosting` registers the same prefab as `NetworkConfig.PlayerPrefab`.
- `CCS_ControllerTestNetworkPlayerBehaviour` enforces owner-only input/camera.

## Target folder structure

Incremental migration target (do not big-bang move assets):

```text
Assets/CCS/Modules/CharacterController/
├── Content/              # Input, animations, controllers, materials
├── Documentation/        # Permanent living docs only (this file + Equipment Fit Studio)
├── Prefabs/              # Camera, player, test-only environment
├── Profiles/             # Movement, camera, equipment fit
├── Runtime/              # Core, input, movement, camera, animation, equipment, validation
├── Editor/               # Builders, validation, Equipment Fit Studio, batch entries
└── Tests/                # Runtime, Netcode, prefabs, scenes, managers
```

Low-risk future moves: test environment props → `Prefabs/TestOnly/`; runtime script subdomains under `Runtime/*`; Testing Manager → `Tests/Runtime/Managers/`.

## Batch-first validation policy

All module integrity checks run via Unity `-batchmode -executeMethod`:

| Batch | Entry |
|-------|-------|
| Master Test (+ project audit) | `CCS.Project.Editor.CCS_ProjectMasterTestBatchEntry.RunFromBatchMode` |
| Player prefab audit | `CCS.Modules.CharacterController.Editor.CCS_CharacterControllerPlayerPrefabAuditBatchEntry.RunFromBatchMode` |
| AI | `CCS.Modules.AI.Editor.CCS_AIBanditBatchEntry.RunFromBatchMode` |
| Weapons | `CCS.Modules.Weapons.Editor.CCS_WeaponsValidationBatchEntry.RunFromBatchMode` |
| Hosting | `CCS.Modules.CharacterController.Tests.Netcode.Editor.CCS_MultiplayerHostingSceneBatchEntry.RunFromBatchMode` |

Editor menus are optional convenience wrappers. CI and Cursor workflows must not depend on manual menu clicks.

## Editor menu policy (v0.7.1d)

| Classification | Action |
|----------------|--------|
| **KeepProductionMenu** | Equipment Fit Studio only |
| **Batch-only (menus removed v0.7.1d)** | Master Test setup (`CCS_ProjectMasterTestBatchEntry`), hosting setup (`CCS_MultiplayerHostingSceneBatchEntry`), camera preset utilities (`CCS_CharacterAimCameraProfilePresetUtility`, `CCS_CharacterFirstPersonCameraDefaultsUtility`) |
| **Removed (v0.7.1c)** | Animation Fit Studio window, Apply Default Revolver FullDraw Nudge |
| **Removed (v0.7.1d)** | `CCS_CharacterControllerMasterTestMenus`, hosting scene MenuItem wrapper, camera preset MenuItem wrappers |

Do not add new editor menus without classifying them and documenting the batch equivalent.

## Testing Manager policy (v0.7.1d)

**Central switchboard:** `Tests/Runtime/Managers/CCS_CharacterControllerTestingManager` on Master Test scene object `CCS_TestingManager`.

**Compatibility:** `CCS_MasterTestSceneTestingManager` remains as a thin inherited wrapper so existing scene references stay valid. Remove the wrapper in a later milestone.

**Default toggles (all off unless noted):**

| Toggle | Default |
|--------|---------|
| `enableRecordingAmbience` | off |
| `enableArmToReticleIK` | off |
| `enableVisualAimConvergence` | off |
| `enableAimDebugRays` | off |
| `enableVerboseLogs` | off |
| `enableCameraDiagnostics` | off |
| `enableAimDiagnostics` | off |
| `enableAnimationDiagnostics` | off |
| `enableInteractionDiagnostics` | off |
| `enableTestDamage` | off |
| `enableVisualDebugHelpers` | off |
| `enableReticleClamp` | on |
| `enableThirdPersonAimPitchBlend` | on |

**Debug policy:**

- Console logs and Markdown reports in `Logs/CharacterController/TestingReports/` are preferred.
- Runtime production scripts must not own `OnGUI` overlays.
- Test-only diagnostics live under `Tests/Runtime/Diagnostics/` and are gated by the Testing Manager.
- Production player prefab must **not** require the manager.

**Future work (not v0.7.1e):** player prefab script reduction, Runtime folder moves, animation pack import, Master Test manager wrapper removal.

## Player prefab component audit policy (v0.7.1e)

**Purpose:** inventory and classify every component on the active test player prefab without a big-bang prefab rewrite.

**Audit utility:** `Editor/Validation/CCS_CharacterControllerPlayerPrefabAuditUtility.cs`  
**Batch entry:** `CCS_CharacterControllerPlayerPrefabAuditBatchEntry.RunFromBatchMode`

**Generated report (not committed):** `Logs/CharacterController/PlayerPrefabAudit/CCS_PlayerPrefab_ComponentAudit_v0.7.1e.md`

### Classification categories

| Category | Meaning |
|----------|---------|
| `RequiredRoot` | Must stay on prefab root for now (motor, input, network core) |
| `RequiredRuntime` / `RequiredLocalOnly` | Production Character Controller runtime behaviour |
| `RequiredNetwork` | Netcode spawn/ownership/nameplate requirements |
| `RequiredCamera` / `RequiredAnimation` / `RequiredInteraction` | Domain-specific runtime bridges |
| `RequiredWeaponsBridge` / `RequiredEquipmentVisual` / `RequiredAttributes` / `RequiredUIBridge` | External module bridges wired on the test player |
| `TestOnly` / `DebugOnly` / `DiagnosticsOnly` | Test harness or debug behaviour — move only after replacement exists |
| `TestingManagerCandidate` / `SceneManagerCandidate` | Should eventually move to Testing Manager or scene-level managers |
| `DeprecatedCandidate` / `UnknownReview` | Needs explicit review before any removal |

### Future component reduction process

1. Run the player prefab audit batch and read the Markdown report.
2. Do **not** delete components until Master Test + Hosting batches and manual smoke test prove a replacement path.
3. Prefer moving debug toggles to `CCS_CharacterControllerTestingManager` before moving components off the prefab.
4. Future production-style root target (~6 MonoBehaviours + Transform + CharacterController) is documented in the audit report; **not enforced in v0.7.1e**.
5. `CCS_MasterTestSceneTestingManager` compatibility wrapper remains until Phase 2D scene migration is validated.

### v0.7.1e scope guardrails

- No player prefab hierarchy rewrite.
- No animator controller, production clip, or `PF_CCS_Player_Visual` changes.
- Test-only components are classified only — not removed in this milestone.
- Equipment Fit Studio remains the production equipment fit workflow.

## Animation Fit Studio removal (v0.7.1c)

**Removed:** entire `Editor/AnimationFitStudio/` stack (pose audition, Humanoid muscle writeback, Fit Studio window, FullDraw nudge menu/batch).

**Kept:**

- **Equipment Fit Studio** — `Editor/EquipmentFitStudio/` (weapon socket / IK profile tuning). See `CCS_Equipment_Fit_Studio.md`.
- **`CCS_AnimationInventoryReporter`** — batch Wild West clip inventory; writes to `Logs/CharacterController/AnimationInventory/`.
- **`CCS_RevolverAimPreviewPoseUtility`** — shared preview helper for Equipment Fit Studio.
- Production clips and controller wiring (including `_FitTest` AimPitch Down/Center/Up clips on `Revolver_AimPitch_Blend`).

## Guardrails for future animation / prefab work

Do **not** alter without an explicit, batch-validated milestone:

- `AC_CCS_Player_Locomotion_StarterAssets.controller`
- `PF_CCS_CharacterController_TestPlayer_Networked.prefab`
- `PF_CCS_Player_Visual.prefab`
- Production animation clips wired to the active controller
- AI combat, weapons combat, health bar behavior, hosting scene behavior

**Principles:**

1. No big-bang prefab rewrite (avoid v0.8-style architecture experiments).
2. Preserve working gameplay first — delete or move only after a replacement path exists.
3. Do not merge `backup/v0.8-animation-prefab-repair-attempt` unless explicitly requested.
4. `_FitTest` in a filename does not imply obsolete — AimPitch blend clips are production until a dedicated rename pass.

## Documentation policy

1. **Temporary plans do not remain** after a milestone is complete — fold lasting rules into this file, then delete phase checklists.
2. **Generated reports go to `Logs/`** unless intentionally promoted to permanent module documentation.
3. **Module docs are living docs** — update in place; avoid phase clutter and duplicate overviews.
4. **Remove or consolidate** duplicate/outdated docs when content overlaps validators or batch logs.

### Permanent documentation in this module

| Document | Purpose |
|----------|---------|
| `CCS_CharacterController_Module.md` | This overview |
| `CCS_Equipment_Fit_Studio.md` | Equipment Fit Studio operator guide |

### Generated output (not committed)

| Output | Path |
|--------|------|
| Wild West animation inventory (MD/CSV) | `Logs/CharacterController/AnimationInventory/` |
| Player prefab component audit | `Logs/CharacterController/PlayerPrefabAudit/` |
| Batch validation logs | `Logs/*-batch.log` |

## Revolver animation summary

Wild West one-handed revolver aim uses CCS-owned clips under `Content/Animations/Revolver/WildWest/` on the `RevolverUpperBody` layer. Held aim loop uses `Revolver_AimPitch_Blend` driven by `RevolverAimPitch` with Down/Center/Up `_FitTest` clips. `RevolverAimHeld` follows RMB; `RevolverIsMoving` follows locomotion speed. Legacy Invector-derived aim/fire clips under `Content/Animations/Combat/Revolver/` must not be assigned to active Animator states.
