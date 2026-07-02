# CCS Character Controller Module

**Version:** 0.7.6 — living module overview  
**Author:** James Schilz  
**Last updated:** 2026-06-25

## Purpose

Profile-driven third-person movement, Cinemachine camera control, equipment socket fitting, and animation architecture contracts for `ccs-survival`. The module ships a unified network-capable player for solo validation and multiplayer hosting flows.

**Non-goals:** AI combat logic, weapons hitscan resolver, inventory database (sibling modules).

## Current safe baseline

| Milestone | Scope |
|-----------|-------|
| **v0.7.1a** | Gameplay baseline — locomotion, revolver aim/fire, AI combat, weapons pickup, hosting |
| **v0.7.1a** | AI health bar fill direction hotfix (signed off) |
| **v0.7.1c** | Editor/documentation cleanup — Animation Fit Studio removed; no gameplay behavior changes |
| **v0.7.1d** | Testing Manager foundation + editor menu reduction; no gameplay behavior changes |
| **v0.7.1e** | Player prefab component audit + test-only separation readiness; no prefab rewrite |
| **v0.7.11** | Mouse-driven revolver aim body/arm architecture plan — aim target resolver, body presenter, arm IK presenter, muzzle LOS resolver, reticle convergence profiles documented; **no implementation**; gameplay unchanged |
| **v0.7.10f** | Reticle reveal animation event — `CCS_OnRevolverAimHoldStarted` on `Fulldraw_Idle`; v0.7.10e smoothing retained; draw normalized reveal no longer primary; barrel LOS deferred; no gameplay changes |
| **v0.7.10e** | Reticle reveal timing and pitch stability — `CCS_RevolverReticlePresentationProfile`; late-draw reveal window; screen smoothing/clamp; barrel LOS deferred; fit profile unchanged; no gameplay changes |
| **v0.7.10d** | Reticle aim readiness gate — `CCS_IRevolverAimPresentationReadinessSource`; reticle hidden until `Revolver_Aim_Hold`; hand socket preview never shows reticle; barrel line-of-sight plan only; fit profile unchanged; no gameplay changes |
| **v0.7.10c** | Revolver right-hand fit offset tuning — updates `CCS_RevolverM1879_RightHandEquipped_Fit` from manual alignment; fit profile only; no gameplay changes |
| **v0.7.10b** | Revolver right-hand fit profile refinement — `CCS_RightHandRevolverAttachmentOffset`; fit profile source of truth; Equipment Fit Studio tuning; no gameplay changes |
| **v0.7.10a** | Revolver hand socket preview hotfix — fixes diagnostics preview attach to `CCS_HandSocket_Right`; socket vs IK audit; player equipment visual lookup fix; no gameplay changes |
| **v0.7.10** | Revolver hand socket preview toggle — diagnostics Force Revolver Hand Socket Preview (visual-only right-hand socket); Force Revolver Aim Setup Pose remains separate; no gameplay ownership/ammo/damage changes; no new animation layers |
| **v0.7.9** | Validation cleanup — weapon damage target moved to Prototyping; legacy TestDetectionCube removed; diagnostics Force Revolver Aim Setup Pose (animation + right-hand visual preview); CapsuleVisual/VisualGlasses removed from production player prefab |
| **v0.7.8** | Single revolver aim upper-body layer — `SingleRevolverUpperBody` masked draw/hold/holster; Wild West clips; presentation-only `CCS_SingleRevolverAimAnimator`; gameplay aim/fire unchanged |
| **v0.7.7** | EnemyAI default AI bandit visual — `Model` root + `PF_CCS_AI_Bandit_Model_EnemyAI`; legacy `PF_CCS_Player_Visual` deleted when unreferenced; Unity 6 CS0618 editor warning cleanup |
| **v0.7.6** | Kevin default player visual — `Model` root + `PF_CCS_Player_Model_Kevin`; legacy `PF_CCS_Player_Visual` deferred if bandit still references |
| **v0.7.5** | Player prefab hierarchy architecture (Phase 3D) — target hierarchy, root budgets, subsystem ownership, composition interface; no prefab changes |
| **v0.7.4** | Animation rebuild architecture (Phase 3C) — documented future layers, parameter IDs, weapon mode enum, presenter interface; locomotion-only Animator preserved; no import |
| **v0.7.3** | Locomotion-only Animator reset (Phase 3B) — Base Layer locomotion only; aim/revolver/interaction animation layers removed from player controller; gameplay aiming/shooting/interaction retained |
| **v0.7.2** | Productionize architecture — Tests folder removed; player prefab + validation scene production paths; Prototyping folder; no Animator reset |
| **v0.7.1f** | Safe test-only component separation; Master Test manager migration; two root test components moved to scene |

Working systems that must remain stable unless a dedicated, batch-validated milestone approves changes:

- Master Test spawn, movement, camera, revolver aim/fire
- AI bandit combat and health bar
- Weapons pickup and validation
- Multiplayer hosting scene
- Locomotion animation clips on Base Layer (idle/walk/sprint/jump/in-air)
- Revolver/interaction/aim animation clips remain on disk for future rebuild (not wired on player controller in v0.7.3+)

## v0.7.6 — Kevin default player visual

- **Production Kevin prefab:** `Characters/Player/Prefabs/PF_CCS_Player_Model_Kevin.prefab`
- **Networked player:** single `Model` root with nested Kevin visual (no `VisualRoot` / `PF_CCS_Player_Visual` nesting)
- **Validation:** `CCS_PlayerVisualModelSwapValidationUtility` + batch `CCS_PlayerVisualModelSwapBatchEntry`
- **Imports:** Kevin wired; EnemyAI and Camila imported under `Assets/Reallusion/DataLink_Imports/` but **not wired**
- **Preserved:** locomotion-only Animator Controller; no weapon/interaction animation layers; no clip edits
- **Equipment Fit Studio:** socket rebuild for Kevin humanoid rig
- **Legacy:** `PF_CCS_Player_Visual` retained while AI bandit or other prefabs still reference it

## v0.7.5 Phase 3D — Player prefab hierarchy architecture (planning only)

- **Doc:** `Documentation/CCS_PlayerPrefab_Hierarchy_Architecture.md`
- **Contract:** `CCS_IPlayerCompositionRoot` (interface-only composition hub)
- **Reports:** `Logs/CharacterController/PrefabAudit/CCS_PlayerPrefab_HierarchyAudit_v0.7.5.md` and `CCS_PlayerPrefab_HierarchyArchitecture_v0.7.5.md` (generated by Phase 3D batch)
- Root component budget policy defined (Target A ideal ~6, Target B Netcode-safe)
- Single `Model` root target defined (replaces `VisualRoot` + nested `PF_CCS_Player_Visual`)
- Owner-only UI separation planned (`LocalOnly` — v0.7.9)
- Netcode-safe root rules documented (do not move `NetworkBehaviour` without validated `NetworkObject` strategy)
- **Not in v0.7.5:** prefab hierarchy changes, `PF_CCS_Player_Visual` changes, animation import, CC4 import

## v0.7.4 Phase 3C — Animation rebuild architecture (planning only)

- **Doc:** `Documentation/CCS_CharacterController_Animation_Rebuild_Architecture.md`
- **Contracts:** `CCS_CharacterAnimationParameterIds`, `CCS_CharacterWeaponAnimationMode`, `CCS_ICharacterAnimationPresenter`
- `CCS_PlayerLocomotionAnimator` uses centralized active parameter hashes only.
- **Preserved:** v0.7.3 locomotion-only Animator Controller (Base Layer + four parameters).
- **Not in v0.7.4:** animation import, CC4 import, Animator layer rebuild, clip edits, `PF_CCS_Player_Visual` changes.
- **Report:** `Logs/CharacterController/AnimationRebuild/CCS_AnimationRebuildArchitecture_v0.7.4.md` (generated by Phase 3C batch).

## v0.7.3 Phase 3B — Locomotion-only Animator reset

- Player Animator Controller (`AC_CCS_Player_Locomotion_StarterAssets.controller`) keeps **Base Layer locomotion only**.
- Removed from controller: `RevolverUpperBody`, `Interaction`, preview/aim/revolver/interaction states and parameters.
- Removed obsolete `CCS_RevolverUpperBodyAnimator` animation bridge from production prefabs (player, AI bandit, validation NPC).
- **Retained gameplay:** `CCS_CharacterAimLocomotionController`, `CCS_RevolverController`, interaction scanner/locks, camera aim, IK/reticle gameplay paths.
- `CCS_PlayerInteractionAnimator` retains interaction busy/control lock; no Animator triggers.
- `CCS_PlayerLocomotionAnimator` drives locomotion parameters only.
- Reports: `Logs/CharacterController/AnimatorReset/CCS_AnimatorController_Before_v0.7.3.md` and `_After_v0.7.3.md`.
- **Future:** rebuild single-gun / two-gun animation layers per v0.7.4 architecture doc (Phase 3C). No import in v0.7.3.

## Runtime / editor / validation boundaries (v0.7.2)

| Layer | Location | Rules |
|-------|----------|-------|
| **Runtime** | `Runtime/` | Production gameplay + diagnostics + local player + validation scene helpers. No Editor dependencies. |
| **Netcode** | `Runtime/Netcode/`, `Runtime/Netcode/Hosting/` | Multiplayer hosting runtime and network player controller. |
| **Editor** | `Editor/`, `Editor/Netcode/` | Builders, validators, batch entries, Equipment Fit Studio. |
| **Validation** | `Scenes/Validation/` | Primary validation scene and scene-local wiring |
| **Prototyping** | `Prototyping/` | Reusable blockout environment prefabs and materials for validation scenes |
| **Editor** | `Editor/`, `Editor/Netcode/` | Builders, validators, batch entries, Equipment Fit Studio |

**Editor tools are not gameplay dependencies.** Runtime must compile and run without Editor-only tooling.

`CCS_CharacterControllerDiagnosticsManager` is production-ready development tooling (default toggles off), not a test harness script.

## Canonical player prefab (v0.7.2)

| Asset | Path |
|-------|------|
| Network-capable player | `Prefabs/Player/PF_CCS_CharacterController_Player_Networked.prefab` |
| Validation scene | `Scenes/Validation/SCN_CCS_CharacterController_Validation.unity` |
| Player visual (Kevin production) | `Characters/Player/Prefabs/PF_CCS_Player_Model_Kevin.prefab` |
| Player visual (legacy CC3) | `Characters/Player/Prefabs/PF_CCS_Player_Visual.prefab` (deferred deletion if referenced) |
| Locomotion controller | `Characters/Player/Animations/Controllers/AC_CCS_Player_Locomotion_StarterAssets.controller` |

### Solo validation scene

- `CCS_ValidationSpawnController` instantiates the networked prefab when no Netcode session is active.
- `CCS_LocalPlayerOfflineBootstrapper` + `CCS_LocalPlayerSessionConfigurator` enable local input/motor/camera.
- Validation scene has **no scene-placed player** and **no scene NetworkManager**.
- Scene object `CCS_DiagnosticsManager` hosts `CCS_CharacterControllerDiagnosticsManager` (exactly one).

### Multiplayer

- `PF_CCS_NetworkManager` in `SCN_CCS_MultiplayerHosting` registers the same prefab as `NetworkConfig.PlayerPrefab`.
- `CCS_NetworkPlayerController` enforces owner-only input/camera.

## Target folder structure

Incremental migration target (do not big-bang move assets):

```text
Assets/CCS/Modules/CharacterController/
├── Content/              # Input, animations, controllers
├── Documentation/        # Permanent living docs only (this file + Equipment Fit Studio)
├── Prefabs/              # Camera, player, network manager
├── Profiles/             # Movement, camera, equipment fit
├── Prototyping/          # Blockout environment prefabs, materials, textures for validation scenes
├── Runtime/              # Core, input, movement, camera, diagnostics, local, netcode, validation helpers
├── Scenes/Validation/    # Primary Character Controller validation scene
├── Editor/               # Builders, validation, Equipment Fit Studio, Netcode/hosting batch entries
```

The `Tests/` folder was removed in v0.7.2. Prototyping assets retain `Test` prefixes where they denote blockout props (for example `PF_CCS_TestGround_OneMeterGrid`).

## Batch-first validation policy

All module integrity checks run via Unity `-batchmode -executeMethod`:

| Batch | Entry |
|-------|-------|
| Master Test (+ project audit) | `CCS.Project.Editor.CCS_ProjectMasterTestBatchEntry.RunFromBatchMode` |
| Player prefab audit | `CCS.Modules.CharacterController.Editor.CCS_CharacterControllerPlayerPrefabAuditBatchEntry.RunFromBatchMode` |
| AI | `CCS.Modules.AI.Editor.CCS_AIBanditBatchEntry.RunFromBatchMode` |
| Weapons | `CCS.Modules.Weapons.Editor.CCS_WeaponsValidationBatchEntry.RunFromBatchMode` |
| Hosting | `CCS.Modules.CharacterController.Netcode.Editor.CCS_MultiplayerHostingSceneBatchEntry.RunFromBatchMode` |
| Phase 3C animation architecture | `CCS.Modules.CharacterController.Editor.CCS_CharacterControllerPhase3CBatchEntry.RunFromBatchMode` |
| Phase 3D hierarchy architecture | `CCS.Modules.CharacterController.Editor.CCS_CharacterControllerPhase3DBatchEntry.RunFromBatchMode` |
| Player visual Kevin swap | `CCS.Modules.CharacterController.Editor.CCS_PlayerVisualModelSwapBatchEntry.RunFromBatchMode` |
| Revolver right-hand fit profile (v0.7.10b) | `CCS.Modules.CharacterController.Editor.CCS_RevolverRightHandFitProfileBatchEntry.RunFromBatchMode` |
| Revolver hand socket preview (v0.7.10) | `CCS.Modules.CharacterController.Editor.CCS_RevolverHandSocketPreviewBatchEntry.RunFromBatchMode` |
| Validation cleanup / aim debug toggle | `CCS.Modules.CharacterController.Editor.CCS_ValidationCleanupAimDebugToggleBatchEntry.RunFromBatchMode` |
| Single revolver aim layer | `CCS.Modules.CharacterController.Editor.CCS_SingleRevolverAimLayerBatchEntry.RunFromBatchMode` |
| Reticle reveal animation event (v0.7.10f) | `CCS.Modules.CharacterController.Editor.CCS_ReticleRevealAnimationEventBatchEntry.RunFromBatchMode` |

See `Documentation/CCS_MouseDriven_RevolverAim_BodyArm_Architecture.md` and `Documentation/CCS_MouseDriven_RevolverAim_ValidationPlan.md` for the v0.7.11 mouse-driven aim plan (planning + interface contracts only).

Editor menus are optional convenience wrappers. CI and Cursor workflows must not depend on manual menu clicks.

## Editor menu policy (v0.7.1d)

| Classification | Action |
|----------------|--------|
| **KeepProductionMenu** | Equipment Fit Studio only |
| **Batch-only (menus removed v0.7.1d)** | Master Test setup (`CCS_ProjectMasterTestBatchEntry`), hosting setup (`CCS_MultiplayerHostingSceneBatchEntry`), camera preset utilities (`CCS_CharacterAimCameraProfilePresetUtility`, `CCS_CharacterFirstPersonCameraDefaultsUtility`) |
| **Removed (v0.7.1c)** | Animation Fit Studio window, Apply Default Revolver FullDraw Nudge |
| **Removed (v0.7.1d)** | `CCS_CharacterControllerMasterTestMenus`, hosting scene MenuItem wrapper, camera preset MenuItem wrappers |

Do not add new editor menus without classifying them and documenting the batch equivalent.

## Diagnostics Manager policy (v0.7.2)

**Central switchboard:** `Runtime/Diagnostics/CCS_CharacterControllerDiagnosticsManager` on validation scene object `CCS_DiagnosticsManager`.

**Compatibility:** Master Test scene uses `CCS_CharacterControllerDiagnosticsManager` directly (v0.7.1f). The `CCS_CharacterControllerDiagnosticsManager` wrapper was removed after serialized migration.

**Scene-level test replacements (v0.7.1f):**

| Removed from player root | Scene replacement |
|--------------------------|-------------------|
| `CCS_LocalPlayerOfflineBootstrap` | `CCS_LocalPlayerOfflineBootstrapper` on `CCS_DiagnosticsManager` |
| `CCS_TestPlayerAttributeDebugInput` | `CCS_PlayerDiagnosticsInputRouter` on `CCS_DiagnosticsManager` (gated by `EnableDamageDiagnostics`) |

Solo Master Test still configures offline players through `CCS_ValidationSpawnController` plus the scene bootstrapper safety net. Hosting/network spawn does not depend on prefab-root offline bootstrap.

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
| `forceRevolverAimSetupPose` | off (validation scene only; presentation-only setup pose + right-hand visual preview) |
| `forceRevolverHandSocketPreview` | off (validation scene only; visual-only right-hand socket preview on `CCS_HandSocket_Right`; v0.7.10a hotfix) |
| `enableReticleClamp` | on |
| `enableThirdPersonAimPitchBlend` | on |

**Debug policy:**

- Console logs and Markdown reports in `Logs/CharacterController/TestingReports/` are preferred.
- Runtime production scripts must not own `OnGUI` overlays.
- Diagnostics reporters live under `Runtime/Diagnostics/` and are gated by the Diagnostics Manager.
- Production player prefab must **not** require the manager.

**Future work (not v0.7.1e):** player prefab script reduction, Runtime folder moves, animation pack import, Master Test manager wrapper removal.

## Player prefab component audit policy (v0.7.1e)

**Purpose:** inventory and classify every component on the active test player prefab without a big-bang prefab rewrite.

**Audit utility:** `Editor/Validation/CCS_CharacterControllerPlayerPrefabAuditUtility.cs`  
**Batch entry:** `CCS_CharacterControllerPlayerPrefabAuditBatchEntry.RunFromBatchMode`

**Generated report (not committed):** `Logs/CharacterController/PlayerPrefabAudit/CCS_PlayerPrefab_ComponentAudit_v0.7.2.md`

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
3. Prefer moving debug toggles to `CCS_CharacterControllerDiagnosticsManager` before moving components off the prefab.
4. Future production-style root target (~6 MonoBehaviours + Transform + CharacterController) is documented in the audit report; **not enforced in v0.7.1e**.
5. `CCS_CharacterControllerDiagnosticsManager` compatibility wrapper removed in v0.7.1f after scene migration.

### v0.7.1f scope (Phase 2D)

- Master Test scene migrated to `CCS_CharacterControllerDiagnosticsManager` directly.
- Removed prefab-root `CCS_LocalPlayerOfflineBootstrap` and `CCS_TestPlayerAttributeDebugInput` after scene replacements validated.
- Test damage input is gated by `EnableDamageDiagnostics` on the Testing Manager.
- Player prefab reduction remains incremental; no big-bang rewrite.
- Animation import remains future work.

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
- `PF_CCS_CharacterController_Player_Networked.prefab`
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
