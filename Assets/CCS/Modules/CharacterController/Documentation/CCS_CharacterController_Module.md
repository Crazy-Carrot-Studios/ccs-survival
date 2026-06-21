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
| Third-person profile | `Profiles/Camera/CCS_CharacterCameraProfile_ThirdPersonSurvival.asset` |
| Profile set | `Profiles/Camera/CCS_DefaultCharacterCameraProfileSet.asset` |

Profile types: `CCS_CharacterCameraProfile`, `CCS_CharacterCameraProfileSet`.

Default active mode: `ThirdPersonSurvival`.

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

Checks include asmdefs, input actions, profiles, canonical test player prefab wiring, master test scene layout, Cinemachine Orbital Follow wiring, network player contracts, and no legacy `UnityEngine.Input` usage in module runtime code.

## Out of scope (current milestone)

- Interaction, inventory, crafting, stats, combat, save/load
- Final character art, animation controller, IK, equipment sockets
- Production HUD

## Related

- [Future Gameplay Module Guidelines](../../../../Documentation/Planning/Future_Gameplay_Module_Guidelines.md)
- [Modules README](../../README.md)
- [Folder Structure](../../../FOLDER_STRUCTURE.md)
