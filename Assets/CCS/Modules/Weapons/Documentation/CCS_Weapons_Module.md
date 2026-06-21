# CCS Weapons Module

**Version:** 0.6.0  
**Scope:** Test revolver hitscan foundation for Master Test solo play.

## Purpose

Provides the first weapon mechanic for CCS Survival: a test revolver with aim, fire, reload, camera-center hitscan, and a simple damage target. This module does not include inventory, equipment, hotbar, crafting, or weapon swapping.

## Runtime Components

| Component | Role |
|-----------|------|
| `CCS_RevolverController` | Local-owner revolver state, fire/reload, hitscan dispatch |
| `CCS_HitscanWeaponRaycaster` | Camera-center ray with spread |
| `CCS_TestDamageTarget` | Master Test damage receiver |
| `CCS_RevolverHudPresenter` | Test-only ammo/aim/reload HUD |

## Profiles

| Asset | Path |
|-------|------|
| Test revolver definition | `Tests/Profiles/CCS_RevolverDefinition_Test.asset` |

## Input

Weapon input actions live on the CharacterController Input Actions asset:

- **Aim** — mouse right button / gamepad left trigger
- **Fire** — mouse left button / gamepad right trigger
- **Reload** — keyboard R / gamepad West (Square/X)

`CCS_CharacterInputActionProvider` exposes read-only `AimHeld`, `FirePressed`, and `ReloadPressed`.

## Master Test

- Test player prefab receives `CCS_RevolverController`, `MuzzlePoint`, and weapon HUD wiring via editor builders.
- Master Test scene receives `CCS_TestWeaponDamageTarget` at a safe forward/lateral offset from `TP_Spawn_Host`.

## Networking

v0.6.0 supports solo/local-owner firing only. Future multiplayer must use server-authoritative validation for owner, cooldown, ammo, origin, range, and hit target before applying damage.

## Validation

Menu: **CCS → Weapons → Validate Weapons Module**

Batch: `CCS.Modules.Weapons.Editor.CCS_WeaponsValidationBatchEntry.RunFromBatchMode`

## Dependencies

- `CCS.Core.Runtime`
- `CCS.Project.Runtime`
- `CCS.Modules.CharacterController.Runtime` (input + camera context)

Weapon logic stays in this module; Framework is not modified.
