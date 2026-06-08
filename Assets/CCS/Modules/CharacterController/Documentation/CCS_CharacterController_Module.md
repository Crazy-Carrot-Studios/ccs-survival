# CCS Character Controller Module

**Version:** 0.2.0 — Character Controller Foundation

## Purpose

First rebuilt gameplay module for `ccs-survival`. Provides profile-driven third-person movement and Cinemachine camera control with module-owned input, validation, test prefab, and debug HUD.

## Folder ownership

```text
Assets/CCS/Modules/CharacterController/
├── Runtime/          # Movement, camera, input, service, validation
├── Editor/           # Validation menu and authoring checks
├── Content/Input/    # Module-owned Input Actions
├── Profiles/         # Movement and camera ScriptableObject profiles
├── Prefabs/          # Test player prefab
├── Documentation/    # This document
├── Tests/            # Reserved for module tests
└── UI/               # Reserved for future module UI assets
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
| Jump | Button | Placeholder; ignored unless profile enables jump |
| ToggleCursor | Button | Escape + Start |
| CameraZoom | Axis | Scroll Y + D-Pad up/down placeholder |

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

Drop into an empty scene with a ground plane and directional light. **No test scene is included in v0.2.0** — create the test scene manually in the next step.

## Debug HUD

`CCS_CharacterControllerDebugHud` (OnGUI, dev/test only) displays:

- Version 0.2.0
- Movement and camera modes
- Grounded, speeds, sprinting
- Input device and input vectors
- Yaw/pitch
- Active camera profile and sensitivity

## Validation

Menu: `CCS/Project/Validation/Validate Character Controller`

Checks asmdefs, input actions, profiles, prefab wiring, CinemachineBrain, ThirdPersonFollow, default camera mode, jump disabled, and no legacy `UnityEngine.Input` usage in module runtime code.

## Out of scope (v0.2.0)

- Test scene
- Inventory, interaction, crafting, stats, combat, save/load, multiplayer
- Final character art, animation controller, IK, equipment sockets
- Production HUD

## Related

- [Future Gameplay Module Guidelines](../../../Project/Documentation/Future_Gameplay_Module_Guidelines.md)
- [Modules README](../../README.md)
