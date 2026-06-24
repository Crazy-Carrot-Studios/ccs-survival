# CCS Character Controller Module

**Version:** 0.2.4 — Unified Test Player + Master Test Harness

## Purpose

Profile-driven third-person movement and Cinemachine camera control for `ccs-survival`, with a unified network-capable test player used for solo Master Test and multiplayer hosting flows.

## Folder ownership

```text
Assets/CCS/Modules/CharacterController/
├── Runtime/              # Movement, camera, input, service, validation
├── Editor/               # Master test builder, validator, prefab builder
├── Content/
│   ├── Input/            # Module-owned Input Actions
│   └── Animations/       # CCS-owned duplicated runtime animation clips (v0.5.6+)
├── Profiles/             # Movement, camera, and test display ScriptableObject profiles
├── Prefabs/              # Camera rig, environment, NPC, network manager assets
├── Documentation/        # This document
└── Tests/
    ├── Runtime/          # Solo spawn, offline bootstrap, session events, join feed UI
    ├── Netcode/          # Hosting menu, network player behaviour, join announcer
    └── Prefabs/          # Canonical network-capable test player prefab
```

## Editor menus (v0.2.4)

**CCS → Character Controller → Scene →**

| Menu item | Action |
|-----------|--------|
| Setup And Validate Master Test Scene | Builds/repairs master test scene, then validates |
| Setup And Validate Multiplayer Hosting Scene | Builds/repairs hosting scene + UI, then validates |

## Canonical test player prefab

Solo and multiplayer both use one prefab:

| Asset | Path |
|-------|------|
| Network-capable test player | `Tests/Prefabs/PF_CCS_CharacterController_TestPlayer_Networked.prefab` |

### Solo Master Test flow

- `CCS_MasterTestSpawnController` instantiates the networked prefab when no Netcode session is active.
- `CCS_TestPlayerOfflineBootstrap` + `CCS_TestPlayerLocalSessionConfigurator` enable local input/motor/camera and disable `NetworkTransform`.
- Master Test has **no scene-placed player** and **no scene NetworkManager**.

### Multiplayer flow

- `PF_CCS_TestNetworkManager` in `SCN_CCS_MultiplayerHosting` registers the same prefab as `NetworkConfig.PlayerPrefab`.
- `CCS_ControllerTestNetworkPlayerBehaviour` enforces owner-only input/camera.
- Remote players remain visual-only with owner-authoritative `CCS_ClientOwnerNetworkTransform`.

## Display profile

Visual layout and tuning references come from:

| Asset | Path |
|-------|------|
| Test player display profile | `Profiles/TestPlayer/CCS_TestPlayerDisplayProfile_Default.asset` |

Applied by `CCS_TestPlayerDisplayProfileApplicator` to:

- Nameplate position
- Capsule body scale/position
- Glasses capsule visual
- `CameraFollowAnchor` height (from linked camera profile)

## Input actions

| Asset | Path |
|-------|------|
| Input Actions | `Content/Input/CCS_CharacterController_InputActions.inputactions` |

**Action map:** `Gameplay`

| Action | Type | Notes |
|--------|------|-------|
| Move | Vector2 | WASD + left stick |
| Look | Vector2 | Mouse delta + right stick |
| Sprint | Button | Left Shift + left stick press |
| Jump | Button | Controlled by movement profile |
| ToggleCursor | Button | Escape + Start |
| CameraZoom | Axis | Scroll Y + D-Pad up/down (not implemented yet) |

## Animation Asset Policy

v0.5.6 isolates production player animation clips under `Content/Animations/`.

| Folder | Purpose |
|--------|---------|
| `Content/Animations/Locomotion/` | Idle, walk, run/sprint, jump, in-air clips |
| `Content/Animations/Interaction/` | Pickup, door, and other interact clips |
| `Content/Animations/Combat/Revolver/` | Reserved for future revolver/combat clips |

Rules:

- Third-party packs (Starter Assets, Movement Animset Pro, Invector, etc.) are **source libraries only**.
- `AC_CCS_Player_Locomotion_StarterAssets.controller` must reference **CCS-owned `.anim` copies** only.
- Do not edit vendor clips directly or wire vendor FBX sub-assets into production Animator Controllers.
- When adding clips from a vendor pack, duplicate/extract into `Content/Animations/` first.

Tooling:

| Menu | Action |
|------|--------|
| **CCS → Character Controller → Animations → Isolate Player Animation Clips** | Duplicate vendor clips and rewire player AC |
| **CCS → Character Controller → Animations → Validate Player Animation Isolation** | Fail if any AC motion is outside `Content/Animations/` |

See also: [Content/Animations/README.md](../Content/Animations/README.md)

## Movement profile

| Asset | Path |
|-------|------|
| Default movement profile | `Profiles/Movement/CCS_CharacterMovementProfile_Default.asset` |

Profile type: `CCS_CharacterMovementProfile` (inherits `CCS_SurvivalProfileBase`).

Jump is **enabled** on the default test profile (`jumpEnabled: true`).

## Camera profiles

| Asset | Path |
|-------|------|
| **Third-person survival (default v0.6.9)** | `Profiles/Camera/CCS_CharacterCameraProfile_ThirdPersonSurvival.asset` |
| First-person body-aware (future scroll mode) | `Profiles/Camera/CCS_CharacterCameraProfile_FirstPersonBodyAware.asset` |
| First-person firearm aim | `Profiles/Camera/CCS_CharacterCameraProfile_FirstPersonAim.asset` |
| Aim-over-shoulder profile | `Profiles/Camera/CCS_CharacterCameraProfile_AimOverShoulder.asset` (retained for future mode switching) |
| Profile set | `Profiles/Camera/CCS_DefaultCharacterCameraProfileSet.asset` |

Profile types: `CCS_CharacterCameraProfile`, `CCS_CharacterCameraProfileSet`.

**v0.6.9 third-person default + first-person weapon aim:** exploration uses `ThirdPersonSurvival`. RMB firearm aim (local owner only) blends to `FirstPersonAim` (~**0.15s** in / **0.25s** out). `FirstPersonAim` uses fixed `FirstPersonAimCameraAnchor` under `CameraPitchTarget` at local offset `(0, 0.28, 0.36)` — **not** head-tracked. Yaw follows body / `CameraFollowAnchor`; pitch on `CameraPitchTarget`; roll **0**; damping **0**. `FirstPersonBodyAware` may keep head tracking for future full first-person exploration but is not the default. Look-down pitch clamp: **-50°** aim (max **75°**). **Body yaw follows camera/reticle yaw** while aiming. Optional `CCS_FirstPersonBodyVisibilityController` hides head/face renderers for the **local owner only** during aim.

**Weapon carry state:** `CCS_WeaponCarryState` (`None`, `Holstered`, `EquippedInHands`, `Aiming`) drives combat strafe locomotion and holster/equipped visuals. Holstered/no weapon uses normal third-person locomotion. Weapon in hands or aiming uses combat/strafe locomotion.

**Local camera switching (v0.6.9 fix):** `CCS_WeaponCarryStateController` implements `CCS_IWeaponCarryStateCameraSource`. The scene `CCS_CharacterCameraController` binds to the local player carry source in `BindFollowTargets` and switches `ThirdPersonSurvival` ↔ `FirstPersonAim` when carry state changes. Solo/offline counts as local. Remote players never drive the local camera. Optional `debugCameraModeTransitions` on the scene camera controller logs transitions when enabled.

**FirstPersonAim sightline (v0.6.12 BodyAware only):** RMB firearm aim routes to `CinemachineCamera_FP_BodyAware` with head-tracked `FirstPersonCameraAnchor`. Legacy `CinemachineCamera_FP_Aim` and `FirstPersonAimCameraAnchor` are removed from runtime routing. Look-down pitch clamp on BodyAware profile is **-53°** (was -58°). Gameplay aim uses `CCS_WeaponAimResolver` via the active output camera (unchanged). Enable `enableRuntimeCameraDebug` on the scene camera rig to inspect active camera, pitch clamp, and BodyAware routing.

**Local self head visibility (v0.6.14–0.6.15):** `CCS_LocalFirstPersonHeadVisibility` hides the local owner's head in first-person BodyAware aim without globally disabling renderers or syncing network state. Separated head renderers (eyes, teeth, eyelashes, glasses) move to layer `CCS_LocalSelfHeadHidden`, culled from the BodyAware output camera. Combined CC3 `CC_Game_Body` uses `CombinedBodyHeadlessFallback`: the full body moves to `CCS_LocalSelfHeadHidden` while `VisualRoot/CCS_FirstPersonHeadlessBody` renders baked mesh `Content/Meshes/FirstPerson/CCS_CC3_FirstPerson_HeadlessBody.asset` on layer `CCS_LocalFirstPersonBody`. Third-person and remote viewers still see the full head/body. Debug toggles (`enableRuntimeCameraDebug`, etc.) default **off**.

**Wild West one-handed revolver aim (v0.6.11 proper aim set):** isolated CCS-owned clips under `Content/Animations/Revolver/WildWest/` drive the existing `RevolverUpperBody` animator layer with enter/loop/exit states for idle and moving aim. States: `Revolver_IdleToAim`, `Revolver_AimIdle_FullDraw`, `Revolver_AimToIdle`, `Revolver_WalkToAimWalk`, `Revolver_AimWalk`, `Revolver_AimWalkToWalk`, `Revolver_Fire`. `RevolverAimHeld` follows RMB; `RevolverIsMoving` follows locomotion speed. Legacy two-handed Invector-derived aim/fire clips remain archived under `Content/Animations/Combat/Revolver/` and must not be assigned to any active Animator state. `RevolverRightHandPreview` layer is removed from the active controller. `CCS_RevolverUpperBodyAnimator` keeps layer weight active during aim enter/exit transitions and exposes Play Mode debug (enabled on Master Test player). See `Documentation/AnimationReports/CCS_WildWestAnimationInventory.md` for source→CCS clip mapping. Vendor Wild West FBX assets under `Assets/YashMakesGames/` remain source-only.

Reticle/hitscan/tracer use the active gameplay camera output via `CCS_WeaponAimResolver` (unchanged from v0.6.8).

## Equipment Sockets + IK Foundation (v0.6.6)

v0.6.6 adds future-ready equipment socket metadata and zero-weight Animation Rigging IK targets. Weapon visuals are **not** attached yet — world revolver pickup remains the only gun visual.

### Standards

- Sockets are **direct children of animated humanoid skeleton bones** (or approved test fallback anchors when bones are missing).
- Weapon/equipment prefabs attach at local zero under sockets; fit tuning lives on the socket definition transform.
- IK targets live under `VisualRoot/CCS_WeaponIKTargets`.
- `Rig_WeaponIK` and all IK constraint weights default to **0** until weapon visuals return.

### Profile assets

| Asset | Path |
|-------|------|
| Default socket profile | `Profiles/EquipmentSockets/CCS_DefaultEquipmentSocketProfile.asset` |
| Socket definitions | `Profiles/EquipmentSockets/Sockets/*.asset` |

Types: `CCS_EquipmentSocketDefinition`, `CCS_EquipmentSocketProfile`.

### Runtime components

| Component | Role |
|-----------|------|
| `CCS_EquipmentSocketAnchor` | Metadata marker on each socket transform |
| `CCS_EquipmentSocketRegistry` | Lookup service: `TryGetSocket(socketId, out Transform)` |

### Default socket table

| Socket ID | Parent bone | Allowed item types |
|-----------|-------------|-------------------|
| `CCS_HolsterSocket_RightHip` | Hips | `weapon.revolver`, `weapon.pistol` |
| `CCS_HolsterSocket_LeftHip` | Hips | `weapon.pistol`, `tool.knife`, `tool.hand` |
| `CCS_HandSocket_Right` | RightHand | `weapon.revolver`, `weapon.pistol`, `weapon.rifle`, `weapon.shotgun`, `weapon.bow`, `tool.knife`, `tool.hand` |
| `CCS_HandSocket_Left` | LeftHand | `tool.lantern`, `tool.offhand`, `weapon.rifle`, `weapon.shotgun`, `weapon.bow` |
| `CCS_BackSocket_LongGun_A` | Chest (fallback Spine) | `weapon.rifle`, `weapon.shotgun`, `weapon.bow` — blocks Back B |
| `CCS_BackSocket_LongGun_B` | Chest (fallback Spine) | `weapon.rifle`, `weapon.shotgun`, `weapon.bow` — blocks Back A |

Test-only fallback anchors (`CCS_TestBoneSocketFallbacks`) are created only when required humanoid bones are unavailable. Real CC4/humanoid characters must not use fallback anchors.

## Equipment Fit Studio (v0.6.7)

Editor-only tuning tool for socket and IK target authoring. See [CCS_Equipment_Fit_Studio.md](CCS_Equipment_Fit_Studio.md).

| Menu | Action |
|------|--------|
| **CCS → Character Controller → Equipment → Equipment Fit Studio** | Open socket/IK tuning window with live preview |

Saved values go to ScriptableObject profiles. Builders reapply profiles. Validators fail if editor preview objects remain in scenes or prefabs.

## v0.6.8 Revolver Fit Profile Tuning

Profile pack under `Profiles/EquipmentFitting/RevolverM1879/` stores holster, equipped-hand, aim IK, and grip pose foundation data for `ccs.weapon.revolver.m1879` on `ccs.character.testplayer.cc3_base_plus`.

Use Equipment Fit Studio to preview and save fit values. v0.6.8 final layout is Editor Mode only with Fit Target first. Runtime holster/equipped visuals, **reticle-aligned aim** (`CCS_WeaponAimResolver`), and **right-shoulder aim camera alignment** are separate concerns — Fit Studio tunes profile offsets only. Visual gun convergence is experimental/OFF by default (rotating the weapon breaks hand fit).

**Fire visual readability (v0.6.8):** cosmetic bullet tracers spawn from equipped `FitGuides/MuzzlePoint` toward the reticle aim point with a short readable trail; spent shells extract only on reload. Gameplay damage still follows `CCS_WeaponAimResolver` hitscan — fire visuals do not apply damage.

## Cinemachine setup (v0.2.4)

- Package: Cinemachine 3.1
- Scene rig: `Prefabs/Camera/PF_CCS_CharacterCameraRig.prefab`
- `CinemachineCamera_TP` uses **Orbital Follow** + **Rotation Composer**
- Player follow target: `CameraFollowAnchor` (world-stable anchor, not body-yaw pivot)
- Look target: child of `CameraFollowAnchor`
- Profile-driven orbital radius, shoulder offset, damping, and vertical orbit limits

## Test scenes

| Scene | Path | Purpose |
|-------|------|---------|
| Master Test | `Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_MasterTest.unity` | Primary traversal + solo/multiplayer test arena |
| Ground preview | `Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_Test.unity` | Legacy grid-only preview scene |
| Multiplayer Hosting | `Assets/CCS/Scenes/Network/SCN_CCS_MultiplayerHosting.unity` | Host/join UI; contains `PF_CCS_TestNetworkManager` |

## Validation

Validation utilities live in `Editor/Validation/`.

Checks include asmdefs, input actions, profiles, canonical test player prefab wiring, master test scene layout, Cinemachine Orbital Follow wiring, network player contracts, equipment socket/IK foundation (v0.6.6), Animation Rigging package presence, and no legacy `UnityEngine.Input` usage in module runtime code.

## Out of scope (current milestone)

- Interaction, inventory, crafting, stats, combat, save/load
- Final character art and production animation polish
- Attached weapon visuals, holster/equipped gun meshes, and enabled weapon IK
- Production HUD

## Related

- [Future Gameplay Module Guidelines](../../../../Documentation/Planning/Future_Gameplay_Module_Guidelines.md)
- [Modules README](../../README.md)
- [Folder Structure](../../../FOLDER_STRUCTURE.md)
