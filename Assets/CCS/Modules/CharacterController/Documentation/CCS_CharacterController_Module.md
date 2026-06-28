# CCS Character Controller Module

**Version:** 0.7.1c — living module overview  
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
| **Tests** | `Tests/` | Master Test scene harness, hosting menu, spawners, diagnostics switchboard. Not referenced by production Runtime asmdef. |

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
| AI | `CCS.Modules.AI.Editor.CCS_AIBanditBatchEntry.RunFromBatchMode` |
| Weapons | `CCS.Modules.Weapons.Editor.CCS_WeaponsValidationBatchEntry.RunFromBatchMode` |
| Hosting | `CCS.Modules.CharacterController.Tests.Netcode.Editor.CCS_MultiplayerHostingSceneBatchEntry.RunFromBatchMode` |

Editor menus are optional convenience wrappers. CI and Cursor workflows must not depend on manual menu clicks.

## Editor menu policy

| Classification | Action |
|----------------|--------|
| **KeepProductionMenu** | Equipment Fit Studio, validated asset workflows that save production profiles |
| **ConvertToBatchOnly** | Master Test setup, hosting setup, camera preset utilities (Phase 2B+) |
| **Removed (v0.7.1c)** | Animation Fit Studio window, Apply Default Revolver FullDraw Nudge |

Do not add new editor menus without classifying them and documenting the batch equivalent.

## Testing Manager policy (future — not implemented in v0.7.1c)

Evolve existing `CCS_MasterTestSceneTestingManager` on Master Test scene (`CCS_TestingManager`) into `Tests/Runtime/Managers/CCS_CharacterControllerTestingManager`:

- Centralize recording ambience, aim debug, IK toggles, and test-only diagnostics
- Production player prefab must **not** require the manager
- Prefer Console logs and Markdown reports in `Logs/` over runtime OnGUI overlays
- Gate or remove production `OnGUI` debug (e.g. `CCS_RevolverUpperBodyAnimator`) via manager flags

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
| Batch validation logs | `Logs/*-batch.log` |

## Revolver animation summary

Wild West one-handed revolver aim uses CCS-owned clips under `Content/Animations/Revolver/WildWest/` on the `RevolverUpperBody` layer. Held aim loop uses `Revolver_AimPitch_Blend` driven by `RevolverAimPitch` with Down/Center/Up `_FitTest` clips. `RevolverAimHeld` follows RMB; `RevolverIsMoving` follows locomotion speed. Legacy Invector-derived aim/fire clips under `Content/Animations/Combat/Revolver/` must not be assigned to active Animator states.
