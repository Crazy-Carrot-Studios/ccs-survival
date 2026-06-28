# CCS Character Controller Phase 01 Cleanup Plan

**Status:** Temporary planning document (Phase 1 only)  
**Baseline:** `v0.7.1a`  
**Author:** James Schilz  
**Created:** 2026-06-28  
**Remove or replace:** After Phase 2 cleanup execution is complete and validated

> **Phase 1 scope:** Planning and documentation only. No script deletion, no prefab edits, no animator/clip changes, no gameplay behavior changes.

---

## 1. Current Baseline

| Item | State |
|------|-------|
| **Version** | `v0.7.1a` (signed off) |
| **Gameplay** | Stable on `main` |
| **AI health bar** | Left-to-right drain hotfix signed off |
| **This phase** | Audit + Markdown plan only |
| **Git expectation** | Documentation-only milestone (`v0.7.1b`) |

Working systems that must remain untouched until Phase 2 audit proves safe moves:

- Master Test spawn, movement, camera, revolver aim/fire
- AI bandit combat and health bar
- Weapons pickup and validation
- Multiplayer hosting scene

---

## 2. Cleanup Principles

1. **No big-bang prefab rewrite** — avoid v0.8-style production prefab architecture experiments.
2. **No behavior changes during audit** — Phase 1 is read/plan; Phase 2 changes are incremental and batch-validated.
3. **Preserve working gameplay first** — delete or move only after replacement path exists.
4. **Batch-first validation** — Cursor/CI runs builders and validators via `-batchmode -executeMethod`; menus are optional convenience, not requirements.
5. **Test code stays in Tests** — runtime production assemblies must not depend on test harness types.
6. **Production Runtime stays clean** — no scattered debug toggles on player, camera, weapons, AI, or interactables.
7. **Debug UI removed or gated** — prefer Console logs and Markdown reports; any OnGUI overlay must be test-only and controlled by a Testing Manager.
8. **Editor tools are not gameplay dependencies** — Runtime must compile and run without Editor-only tooling.

### User decisions captured

| Decision | Phase 1 action | Phase 2 direction |
|----------|--------------|-------------------|
| Remove Animation Fit Studio long-term | Document all candidates | Delete editor folder + dependent menus/batch hooks |
| Limit editor menus | Audit and classify | Keep production asset workflows only |
| Centralize testing controls | Document manager plan | Evolve Master Test switchboard under `Tests/` |
| Debug logs over debug UI | Inventory OnGUI usage | Gate or remove runtime overlays |
| Module becomes definitive | Target structure + script classification | Incremental folder/asmdef cleanup |

---

## 3. Target Module Identity

The Character Controller module should be treated as a **standalone AAA-quality controller package** embedded in the CCS Survival repo. Responsibilities:

| Domain | Purpose |
|--------|---------|
| **Input** | Action maps, providers, mode routing |
| **Movement** | Motor, profiles, aim-locomotion coupling |
| **Camera** | Rig, profiles, first/third-person modes |
| **Animation** | Locomotion + upper-body/revolver/interaction drivers |
| **Presentation** | Visual adapters, local head visibility, billboards (non-AI) |
| **Equipment / visual adapters** | Socket registry, fit profile application |
| **Network / ownership adapters** | Test harness only in `Tests/Netcode`; production adapters stay minimal |
| **Validation** | Runtime + editor validators, batch entries |
| **Tests** | Master Test scene, spawners, diagnostics switchboard |

**Non-goals for this module:** AI combat logic, weapons hitscan resolver, inventory database (those live in sibling modules).

---

## 4. Proposed Final Folder Structure

Target layout (Phase 2+ migration; **do not move files in Phase 1**):

```text
Assets/CCS/Modules/CharacterController/
├── Content/
│   ├── Animations/
│   ├── Controllers/
│   ├── Input/
│   ├── Materials/
│   └── Textures/
├── Documentation/
├── Prefabs/
│   ├── Player/
│   ├── Camera/
│   └── TestOnly/
├── Profiles/
│   ├── Camera/
│   ├── Movement/
│   └── Animation/
├── Runtime/
│   ├── Core/
│   ├── Input/
│   ├── Movement/
│   ├── Camera/
│   ├── Animation/
│   ├── Presentation/
│   ├── Equipment/
│   ├── Networking/
│   ├── Integrations/
│   ├── Diagnostics/
│   └── Validation/
├── Editor/
│   ├── Builders/
│   ├── Validation/
│   ├── Migration/
│   └── Tools/
└── Tests/
    ├── Runtime/
    ├── Netcode/
    ├── Prefabs/
    ├── Scenes/
    └── Managers/
```

### Current → target mapping (audit notes)

| Current (v0.7.1a) | Target | Notes |
|-------------------|--------|-------|
| `Characters/Player/Animations/` | `Content/Animations/` + `Content/Controllers/` | **Do not move controllers/clips in Phase 2 until guardrails cleared** |
| `Content/` (partial) | `Content/` | Already started; consolidate vendor/CCS clips here |
| `Materials/` (module root) | `Content/Materials/` | Low-risk Phase 2 move |
| `Prefabs/Player`, `Prefabs/Camera`, `Prefabs/Environment` | `Prefabs/Player`, `Prefabs/Camera`, `Prefabs/TestOnly` | Environment test props → `TestOnly` |
| `Profiles/` | `Profiles/` (subfolders) | Split camera/movement/animation profiles |
| `Runtime/Components`, `Data`, `Visuals`, … | `Runtime/*` subdomains | Incremental re-home with asmdef updates |
| `Editor/AnimationFitStudio/` | **REMOVE** | See Section 5 |
| `Editor/EquipmentFitStudio/` | `Editor/Tools/` (keep) | Production equipment fit workflow — **not** part of Animation Fit Studio removal |
| `Editor/Validation/` | `Editor/Validation/` + `Editor/Builders/` | Split builders from validators in Phase 2 |
| `Tests/Runtime/CCS_MasterTestSceneTestingManager.cs` | `Tests/Runtime/Managers/` | Rename/evolve in Phase 2 |

---

## 5. Phase 2 Removal Candidates — Animation Fit Studio

**Action in Phase 2:** `REMOVE IN PHASE 2` (entire tooling stack below).  
**Phase 1:** No deletions.

### 5.1 Editor folder — remove entire directory

Remove folder (including `.meta` files):

`Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio/`

| File | Role |
|------|------|
| `CCS_AnimationFitStudioAimPoseScoreUtility.cs` | Aim pose scoring |
| `CCS_AnimationFitStudioBodyPartCatalog.cs` | Body part catalog |
| `CCS_AnimationFitStudioCleanupUtility.cs` | Scene/preview cleanup |
| `CCS_AnimationFitStudioClipAuditionUtility.cs` | Clip audition previews |
| `CCS_AnimationFitStudioClipCurveMode.cs` | Curve mode enum |
| `CCS_AnimationFitStudioClipCurveModeUtility.cs` | Curve mode helpers |
| `CCS_AnimationFitStudioClipDiagnostics.cs` | Clip diagnostics |
| `CCS_AnimationFitStudioClipResolver.cs` | Clip resolver |
| `CCS_AnimationFitStudioConstants.cs` | Fit studio constants |
| `CCS_AnimationFitStudioCurveHashUtility.cs` | Curve hashing |
| `CCS_AnimationFitStudioEditPartCatalog.cs` | Edit part catalog |
| `CCS_AnimationFitStudioFingerDiscoveryUtility.cs` | Finger bone discovery |
| `CCS_AnimationFitStudioFingerManipulationUtility.cs` | Finger manipulation |
| `CCS_AnimationFitStudioHumanoidControlState.cs` | Humanoid control state |
| `CCS_AnimationFitStudioHumanoidControlUtility.cs` | Humanoid control |
| `CCS_AnimationFitStudioHumanoidMuscleMapping.cs` | Muscle mapping |
| `CCS_AnimationFitStudioHumanoidMuscleWriteUtility.cs` | Muscle writeback |
| `CCS_AnimationFitStudioPlayablePreviewSampler.cs` | Playable preview sampling |
| `CCS_AnimationFitStudioPoseEditData.cs` | Pose edit data |
| `CCS_AnimationFitStudioPoseSourceCatalog.cs` | Pose source catalog |
| `CCS_AnimationFitStudioPoseTargetCatalog.cs` | Pose target catalog |
| `CCS_AnimationFitStudioPoseUtility.cs` | Pose utility |
| `CCS_AnimationFitStudioPreviewState.cs` | Preview state |
| `CCS_AnimationFitStudioPreviewUtility.cs` | Preview utility |
| `CCS_AnimationFitStudioRuntimeControllerClipUtility.cs` | Controller clip utility |
| `CCS_AnimationFitStudioRuntimePolicy.cs` | Runtime policy |
| `CCS_AnimationFitStudioSaveUtility.cs` | Save/write clips |
| `CCS_AnimationFitStudioValidationUtility.cs` | Fit studio validation |
| `CCS_AnimationFitStudioWindow.cs` | Editor window + menu |
| `CCS_AnimationFitStudioWindow.Layout.cs` | Window layout |
| `CCS_RevolverFullDrawHumanoidPoseNudgeUtility.cs` | FullDraw nudge menu + fit studio helper |

### 5.2 Related editor scripts (remove or decouple in Phase 2)

| File | Classification | Phase 2 action |
|------|----------------|----------------|
| `Editor/CCS_RevolverFullDrawNudgeBatchEntry.cs` | Fit Studio batch hook | **Remove** with Fit Studio (menu action already has batch twin) |
| `Editor/Common/CCS_RevolverAimPreviewPoseUtility.cs` | Shared preview helper | **Keep** — also used by Equipment Fit Studio |
| `Editor/AnimationInventory/CCS_AnimationInventoryReporter.cs` | Markdown/CSV reports | **Keep** — batch-only inventory, not Fit Studio UI |

### 5.3 Runtime constants to trim (Phase 2)

In `Runtime/CCS_CharacterControllerConstants.cs`, remove or rename **`AnimationFitStudio*`** constants after Fit Studio deletion. Keep production clip path constants (`RevolverAimPitch*`, `RevolverAimIdleFullDraw*`, etc.) even if they retain `_FitTest` in the filename until a dedicated rename pass.

### 5.4 Validation references to update (Phase 2)

| File | Notes |
|------|-------|
| `Editor/Validation/CCS_CharacterControllerAnimationValidationUtility.cs` | Remove Fit Studio source checks; keep production animation contracts |
| `Editor/Validation/CCS_CharacterControllerMasterTestValidator.cs` | Remove Fit Studio window/folder requirements |
| `Editor/Validation/CCS_RevolverAimSimplificationBuilder.cs` | Audit `_FitTest` clip delete list — **do not delete clips wired to AimPitch blend without replacement** |

### 5.5 Generated / Fit Studio artifact audit (Phase 2 — careful)

These paths are **audit candidates**, not blind deletes:

| Asset / report | Path | Notes |
|----------------|------|-------|
| FitTest clips (Wild West edited) | `Content/Animations/Revolver/WildWest/Edited/*_FitTest.anim` | **Production AimPitch blend uses Down/Center/Up FitTest clips today** — rename/replace only after animator-safe plan |
| Fit studio menu | `CCS/Character Controller/Animations/Animation Fit Studio` | Remove with window |
| Fit studio nudge menu | `CCS/Character Controller/Animations/Apply Default Revolver FullDraw Nudge` | Remove; batch entry remains until Fit Studio removal |
| Animation reports | `Documentation/AnimationReports/CCS_WildWestAnimationInventory.*` | **Keep** — general inventory, not Fit Studio UI |

### 5.6 Explicitly NOT in Animation Fit Studio removal scope

- **Equipment Fit Studio** (`Editor/EquipmentFitStudio/`) — separate production workflow for weapon attachment fit profiles
- **Production animation clips** referenced by `AC_CCS_Player_Locomotion_StarterAssets.controller`
- **`CCS_AnimationInventoryReporter`** — batch Markdown/CSV reporting

---

## 6. Phase 2 Editor Menu Reduction Plan

Audit date: v0.7.1a. Classifications:

| Script / menu | Menu path | Classification | Phase 2 action |
|---------------|-----------|----------------|----------------|
| `CCS_AnimationFitStudioWindow.cs` | `CCS/Character Controller/Animations/Animation Fit Studio` | **RemoveWithFitStudio** | Delete with Section 5 |
| `CCS_RevolverFullDrawHumanoidPoseNudgeUtility.cs` | `CCS/Character Controller/Animations/Apply Default Revolver FullDraw Nudge` | **RemoveWithFitStudio** | Delete menu; remove batch entry when nudge no longer needed |
| `CCS_EquipmentFitStudioWindow.cs` | Equipment Fit Studio menu (`CCS_EquipmentConstants.EquipmentFitStudioMenuPath`) | **KeepProductionMenu** | Keep — real equipment fit profile workflow |
| `CCS_CharacterAimCameraProfilePresetUtility.cs` | `CCS/Character Controller/Camera/Aim Profile Presets/*` | **ConvertToBatchOnly** | Move to builder/batch; menu optional |
| `CCS_CharacterFirstPersonCameraDefaultsUtility.cs` | `CCS/Character Controller/Camera/Apply First Person Body Aware Defaults` | **ConvertToBatchOnly** | Already mirrored by headless body batch builder |
| `CCS_CharacterControllerMasterTestMenus.cs` | `CCS/Character Controller/Scene/Setup And Validate Master Test Scene` | **ConvertToBatchOnly** | Prefer `CCS_ProjectMasterTestBatchEntry`; keep menu only if artists need one-click |
| `CCS_CharacterControllerTestHarnessMenus.cs` (Netcode tests) | `CCS/Character Controller/Scene/Setup And Validate Multiplayer Hosting Scene` | **ConvertToBatchOnly** | Prefer `CCS_MultiplayerHostingSceneBatchEntry` |
| `CCS_FirstPersonHeadlessBodyMeshBatchEntry.cs` | *(no menu — batch only)* | **KeepProductionMenu** | ✅ Already batch-first |
| `CCS_EquipmentFitStudioBatchEntry.cs` | *(no menu — batch only)* | **KeepProductionMenu** | ✅ Batch entry for equipment profiles |
| `CCS_CharacterControllerAnimationIsolationBatchEntry.cs` | *(no menu — batch only)* | **KeepProductionMenu** | ✅ Production validation |
| `CCS_AnimationInventoryReporter.cs` | *(no menu — batch only)* | **KeepProductionMenu** | ✅ Markdown report generator |

### Menu reduction rules (Phase 2)

1. **KeepProductionMenu** — inventory DB, validated asset generation, equipment fit, one-click validation if it saves real production time.
2. **ConvertToBatchOnly** — setup/test/repair flows invoked by Cursor, CI, or Project master batch.
3. **RemoveWithFitStudio** — anything that exists only for Animation Fit Studio or obsolete animation experiments.
4. **TestOnlyMenu** — none currently flagged; hosting/master test menus behave as batch wrappers.
5. **Unknown** — none at audit time.

---

## 7. Phase 2 Testing Manager Plan

### Proposed manager

**Name:** `CCS_CharacterControllerTestingManager`  
**Location:** `Assets/CCS/Modules/CharacterController/Tests/Runtime/Managers/`  
**Evolution path:** Extend/rename existing `CCS_MasterTestSceneTestingManager` (already on Master Test scene as `CCS_TestingManager`) rather than adding a second switchboard.

### Responsibilities

| Feature toggle | Current owner (v0.7.1a) | Target |
|----------------|-------------------------|--------|
| Recording ambience | `CCS_MasterTestSceneTestingManager` | Manager |
| Arm-to-reticle IK | Manager | Manager |
| Visual aim convergence | Manager | Manager |
| Reticle mode / clamp | Manager | Manager |
| Third-person aim pitch blend | Manager | Manager |
| Aim debug rays | Manager | Manager |
| Test damage routes | Scattered / module tests | Manager or Master Test spawner only |
| Debug logs | Per-component `[SerializeField] debug*` | Manager global log level flags |
| Animation diagnostics overlay | `CCS_RevolverUpperBodyAnimator.OnGUI` | **Gate via manager; remove from production runtime** |
| Camera diagnostics | Various | Manager flag → Console/Markdown report |
| Interaction diagnostics | Interaction module (sibling) | Master Test scene wires cross-module flags through manager API |
| Visual debug helpers | Test prefabs / gizmos | Manager enables `Tests/` helpers only |
| One-shot state reports | Batch validators | Manager button → log + optional Markdown dump in `Logs/` |

### Rules

1. **Production player prefab must not require this manager.**
2. **Test features query the manager** (or are wired once in Master Test scene bootstrap).
3. **No debug helper components on every runtime object** — consolidate toggles.
4. **Logs → Console and/or Markdown** — not on-screen overlays in production.
5. **Manager lives under `Tests/`** — not referenced by production Runtime asmdef.

### Known Phase 2 cleanup: runtime OnGUI

| Script | Issue | Phase 2 action |
|--------|-------|----------------|
| `Runtime/Animation/CCS_RevolverUpperBodyAnimator.cs` | Active `OnGUI()` debug overlay | Gate behind Testing Manager or move overlay to Tests-only diagnostic component |

---

## 8. Player Prefab Cleanup Direction

**Do not implement in Phase 1.**

Future goals (Phase 2+):

- Reduce root script clutter on `PF_CCS_CharacterController_TestPlayer_Networked`
- Move test-only scripts off production player prefab paths
- Remove debug components from objects that ship in production scenes
- Keep the **current working prefab** until audit identifies safe, batch-verified moves
- **No v0.8-style big-bang restructure** (no wholesale Presentation/CapsuleVisual re-parenting)

Audit checklist before any prefab component removal:

1. Component is not required in hosting or Master Test production paths
2. Replacement exists in Tests or manager-driven wiring
3. All four module batches pass
4. Manual Play Mode sign-off

---

## 9. Runtime Script Classification Plan

**Do not move files in Phase 1.** Classification guides Phase 2 folder/asmdef work.

| Script | Classification | Notes |
|--------|----------------|-------|
| `CCS_CharacterControllerConstants.cs` | **Core** | Trim Fit Studio constants in Phase 2 |
| `CCS_CharacterControllerService.cs` | **Core** | Module service locator |
| `CCS_CharacterControllerState.cs` | **Core** | Shared state enum/flags |
| `CCS_CharacterControllerSnapshot.cs` | **Core** | Snapshot DTO |
| `CCS_CharacterInputActionProvider.cs` | **Input** | |
| `CCS_CharacterMotor.cs` | **Movement** | |
| `CCS_CharacterMovementProfile.cs` | **Movement** | Profile asset |
| `CCS_CharacterMovementMode.cs` | **Movement** | |
| `CCS_CharacterAimLocomotionController.cs` | **Movement** | Aim-strafe locomotion |
| `CCS_ICharacterAimLocomotionState.cs` | **Movement** | Contract |
| `CCS_CharacterCameraController.cs` | **Camera** | |
| `CCS_CharacterCameraFollowAnchor.cs` | **Camera** | |
| `CCS_FirstPersonBodyCameraAnchor.cs` | **Camera** | |
| `CCS_CharacterCameraProfile.cs` | **Camera** | |
| `CCS_CharacterCameraProfileSet.cs` | **Camera** | |
| `CCS_CharacterCameraMode.cs` | **Camera** | |
| `CCS_CharacterCameraDefaultYawMode.cs` | **Camera** | |
| `CCS_CharacterCameraLayerUtility.cs` | **Camera** | |
| `CCS_CharacterCameraRelativeDirectionUtility.cs` | **Camera** | |
| `CCS_CharacterMovementCameraContext.cs` | **Camera** | |
| `CCS_IWeaponCarryStateCameraSource.cs` | **Integration** | Weapons module bridge |
| `CCS_PlayerLocomotionAnimator.cs` | **Animation** | |
| `CCS_RevolverUpperBodyAnimator.cs` | **Animation** | OnGUI overlay → **Diagnostics** candidate |
| `CCS_RevolverAimPhase.cs` | **Animation** | |
| `CCS_IRevolverAnimationState.cs` | **Animation** | |
| `CCS_IWeaponAimGate.cs` | **Integration** | |
| `CCS_PlayerInteractionAnimator.cs` | **Animation** | |
| `CCS_InteractionAnimationStateExitBehaviour.cs` | **Animation** | |
| `CCS_LocalFirstPersonHeadVisibility.cs` | **Presentation** | |
| `CCS_FirstPersonHeadlessMeshStats.cs` | **Presentation** | |
| `CCS_LocalFirstPersonHeadMaskMode.cs` | **Presentation** | |
| `CCS_EquipmentConstants.cs` | **Equipment** | |
| `CCS_EquipmentSocketRegistry.cs` | **Equipment** | |
| `CCS_EquipmentSocketAnchor.cs` | **Equipment** | |
| `CCS_EquipmentSocketDefinition.cs` | **Equipment** | |
| `CCS_EquipmentSocketProfile.cs` | **Equipment** | |
| `CCS_EquipmentSocketValidationUtility.cs` | **Equipment** | |
| `CCS_WeaponAttachmentFitProfile.cs` | **Equipment** | |
| `CCS_WeaponAttachmentFitProfileApplicator.cs` | **Equipment** | |
| `CCS_WeaponIKPoseProfile.cs` | **Equipment** | |
| `CCS_HandPoseDefinition.cs` | **Equipment** | |
| `CCS_RevolverFitProfilePaths.cs` | **Equipment** | |
| `CCS_EquipmentFitStudioSettings.cs` | **Equipment** | Runtime settings for fit profiles (keep; not Animation Fit Studio) |
| `CCS_ICharacterControlLockSource.cs` | **Integration** | Interaction lock contract |
| `CCS_CharacterJumpAuditHookRegistrar.cs` | **Diagnostics** | Gate in Phase 2 |
| `CCS_CharacterJumpDebugLog.cs` | **Diagnostics** | |
| `CCS_CharacterJumpAuditHook.cs` | **Diagnostics** | |
| `CCS_CharacterMotorAuditHook.cs` | **Diagnostics** | |
| `CCS_SingleAudioListenerUtility.cs` | **Core** | |
| `CCS_CharacterControllerValidationUtility.cs` | **Validation** | |

**TestOnly (already under `Tests/`, not Runtime):** spawners, NPC runner, join feed, nameplate billboard, door markers, session configurators, offline bootstrap, display profiles, **`CCS_MasterTestSceneTestingManager`**.

**DeprecatedCandidate (Phase 2 review only):** none confirmed at v0.7.1a audit — assign during Fit Studio removal if orphaned references appear.

---

## 10. Phase 2 Execution Checklist

Execute in order; stop if any batch fails.

- [ ] **Remove Animation Fit Studio** — delete `Editor/AnimationFitStudio/` and dependent menu/batch hooks
- [ ] **Remove Fit Studio menus** — Animation Fit Studio window + FullDraw nudge menu
- [ ] **Trim constants/validators** — remove Fit Studio references from `CCS_CharacterControllerConstants` and animation validators
- [ ] **Convert menus to batch** — master test, hosting, camera preset menus → batch-first
- [ ] **Testing Manager foundation** — move/rename to `Tests/Runtime/Managers/`; centralize toggles; gate `OnGUI` overlays
- [ ] **Move obvious test-only scripts** — confirm nothing in Runtime references them
- [ ] **Update asmdefs** — if folder moves occur
- [ ] **Audit `_FitTest` clips** — distinguish production AimPitch clips vs obsolete studio outputs
- [ ] **Run all batches** — Master Test, AI, Weapons, Hosting, Project Audit
- [ ] **Manual Play Mode** — locomotion, revolver, AI, death/restart, hosting
- [ ] **Commit/tag** — only after gameplay validation
- [ ] **Remove or replace this document** — fold lasting rules into `CCS_CharacterController_Module.md`

---

## 11. Guardrails

### Do not alter during Phase 1 (this document only)

- `AC_CCS_Player_Locomotion_StarterAssets.controller`
- `PF_CCS_CharacterController_TestPlayer_Networked.prefab`
- `PF_CCS_Player_Visual.prefab` (if present on disk)
- Player animation clips wired to production controller
- AI combat systems
- Weapons combat systems
- AI health bar fix (`v0.7.1a`)
- Hosting scene behavior

### Do not alter in Phase 2 without explicit follow-up plan

Same guardrail list applies to Phase 2 until a dedicated animation/prefab milestone is approved. Fit Studio removal must not rewrite the animator controller or production clip set.

### v0.8.x backup branch

Do not merge or reference `backup/v0.8-animation-prefab-repair-attempt` during Character Controller cleanup unless explicitly requested.

---

## 12. Validation for Phase 1

Phase 1 adds this Markdown file only. Re-run standard batches to prove no gameplay regression:

| Batch | ExecuteMethod | Log |
|-------|---------------|-----|
| Master Test | `CCS.Project.Editor.CCS_ProjectMasterTestBatchEntry.RunFromBatchMode` | `Logs/master-test-v0.7.1b-phase01-character-controller-plan-batch.log` |
| AI | `CCS.Modules.AI.Editor.CCS_AIBanditBatchEntry.RunFromBatchMode` | `Logs/ai-bandit-v0.7.1b-phase01-character-controller-plan-batch.log` |
| Weapons | `CCS.Modules.Weapons.Editor.CCS_WeaponsValidationBatchEntry.RunFromBatchMode` | `Logs/weapons-v0.7.1b-phase01-character-controller-plan-batch.log` |
| Hosting | `CCS.Modules.CharacterController.Tests.Netcode.Editor.CCS_MultiplayerHostingSceneBatchEntry.RunFromBatchMode` | `Logs/hosting-scene-v0.7.1b-phase01-character-controller-plan-batch.log` |

Expected: all pass with **documentation-only** git diff (+ version docs if tagged `v0.7.1b`).

---

## Appendix A — Editor script inventory (non–Fit Studio)

Retained for Phase 2 menu/folder planning:

- **Validation:** `CCS_CharacterControllerMasterTestBuilder`, `Validator`, animation isolation/simplification builders, revolver aim/mask utilities, ambient audio builder, join notification UI builder, environment prefab builder
- **Equipment Fit Studio:** full `Editor/EquipmentFitStudio/` stack (keep)
- **Builders:** player prefab, camera rig, movement assets, equipment sockets, revolver IK, first-person headless mesh
- **Batch entries:** first-person headless mesh, revolver full draw nudge (remove with Fit Studio), animation isolation, equipment fit studio

---

*End of Phase 01 Cleanup Plan — delete or supersede after Phase 2 execution.*
