# CCS Character Controller Module

**Version:** 0.2.1 — Character Controller Test Ground

## Purpose

First rebuilt gameplay module for `ccs-survival`. Provides profile-driven third-person movement and Cinemachine camera control with module-owned input, validation, test player prefab, and a reusable test ground scene.

## Folder ownership

```text
Assets/CCS/Modules/CharacterController/
├── Runtime/          # Movement, camera, input, service, validation
├── Editor/           # Long-term validation only
├── Content/Input/    # Module-owned Input Actions
├── Profiles/         # Movement and camera ScriptableObject profiles
├── Prefabs/          # Test player prefab
├── Documentation/    # This document
└── Tests/            # Test scene, ground prefab, materials
```

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
| Jump | Button | Ignored unless profile enables jump |
| ToggleCursor | Button | Escape + Start |
| CameraZoom | Axis | Scroll Y + D-Pad up/down (not implemented yet) |

## Movement profile

| Asset | Path |
|-------|------|
| Default movement profile | `Profiles/Movement/CCS_CharacterMovementProfile_Default.asset` |

Profile type: `CCS_CharacterMovementProfile` (inherits `CCS_SurvivalProfileBase`).

Jump is **disabled** by default (`jumpEnabled: false`).

## Camera profiles

| Asset | Path |
|-------|------|
| Third-person profile | `Profiles/Camera/CCS_CharacterCameraProfile_ThirdPersonSurvival.asset` |
| Profile set | `Profiles/Camera/CCS_DefaultCharacterCameraProfileSet.asset` |

Profile types: `CCS_CharacterCameraProfile`, `CCS_CharacterCameraProfileSet`.

Default active mode: `ThirdPersonSurvival`.

## Cinemachine setup

- Package: Cinemachine 3.1
- `Main Camera` uses `CinemachineBrain`
- `CM_ThirdPersonSurvival` uses `CinemachineCamera` + `CinemachineThirdPersonFollow`
- Follow target: `CameraPivot`
- Look target: `CameraLookTarget`
- Yaw applied on `CameraPivot`, pitch on `CameraLookTarget`

## Test prefab

| Asset | Path |
|-------|------|
| Test player | `Prefabs/PF_CCS_CharacterController_TestPlayer.prefab` |

Drop the prefab into a lit scene to test movement. It is not placed in the module test scene yet.

## Test scene (ground only)

| Asset | Path |
|-------|------|
| Test scene | `Tests/Scenes/SCN_CCS_CharacterController_Test.unity` |
| Ground prefab | `Tests/Prefabs/PF_CCS_TestGround_OneMeterGrid.prefab` |
| Ground grid material | `Tests/Materials/M_CCS_TestGround_1mGrid.mat` |
| Ground grid texture | `Tests/Materials/T_CCS_TestGround_1mGrid.png` |

**Scene instance:** `CCS_TestGround_OneMeterGrid` — prefab instance of `PF_CCS_TestGround_OneMeterGrid`.

**Ground size:** Unity Plane at scale `(20, 1, 20)` = **200m × 200m**.

**Grid rule:** 1 texture repeat = 10m × 10m with ten 1m cells per axis. Material tiling is `20×20` on the 200m plane.

**Scene contents only:**
- Reusable 1m grid ground prefab instance
- Directional Light
- Preview `Main Camera`
- Small scene label

**Not in scene yet:** test player, gameplay objects, bootstrap roots.

## Debug HUD

`CCS_CharacterControllerDebugHud` (OnGUI, dev/test only) displays movement/camera state, input vectors, and active profile data when the test player prefab is used.

## Validation

Menu: `CCS/Project/Validation/Validate Character Controller`

Checks asmdefs, input actions, profiles, test player prefab wiring, test ground prefab, test scene ground setup, Cinemachine wiring, and no legacy `UnityEngine.Input` usage in module runtime code.

## Out of scope (current milestone)

- Test player in test scene
- Inventory, interaction, crafting, stats, combat, save/load, multiplayer
- Final character art, animation controller, IK, equipment sockets
- Production HUD

## Related

- [Future Gameplay Module Guidelines](../../../Project/Documentation/Future_Gameplay_Module_Guidelines.md)
- [Modules README](../../README.md)
- [Folder Structure](../../../FOLDER_STRUCTURE.md)
