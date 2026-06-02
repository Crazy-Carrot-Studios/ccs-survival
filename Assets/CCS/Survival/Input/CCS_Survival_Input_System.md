# CCS Survival — Input System (New Input System)

**Milestone:** 2.1.1 — Input Asset Verification + Fishing Runtime Safety  
**Author:** James Schilz (Developer)  
**Date:** 2026-06-02

---

## Asset location

| Item | Path |
|------|------|
| Input Actions asset | `Assets/CCS/Survival/Input/CCS_Survival_InputActions.inputactions` |
| Generated C# (if enabled) | Unity generates beside the `.inputactions` asset when **Generate C# Class** is on |

**Policy:** New Input System only. Legacy `UnityEngine.Input` is banned in runtime code (enforced by `CCS_SurvivalInputValidationUtility`).

---

## Runtime binding owner

| Script | Role |
|--------|------|
| **`CCS_CharacterInputActionProvider`** | Owns the `InputActionAsset` reference, resolves the **Gameplay** map, enables actions in `OnEnable`, reads snapshots in `Update` / `GetInputSnapshot()` |
| **`PF_CCS_Player.prefab`** | Assigns `CCS_Survival_InputActions` to the provider’s `inputActions` field (wired by `CCS_PlayerBootstrapSetup`) |
| **`CCS_PlayerGameplayController`** | Requires `CCS_CharacterInputActionProvider` on the player; drives movement via `CCS_CharacterMovementService` |
| **Player drivers** | `CCS_InteractionPlayerDriver`, `CCS_PlayerActiveItemDriver`, `CCS_PlayerCombatDriver`, etc. read the same provider |

**Flow:**

```text
CCS_Survival_InputActions (asset)
    → CCS_CharacterInputActionProvider (PF_CCS_Player)
        → CCS_CharacterInputSnapshot
            → CCS_CharacterMovementService / player drivers
```

---

## Gameplay map actions

| Action | Default binding (KB+M) | Consumed by |
|--------|------------------------|-------------|
| **Move** | WASD | Movement motor |
| **Look** | Mouse delta | Camera |
| **Jump** | Space | Movement |
| **Sprint** | Left Shift | Movement + stamina gate |
| **Crouch** | Left Ctrl | Movement |
| **Interact** | E | `CCS_InteractionPlayerDriver` |
| **PrimaryAction** | Mouse LMB | Active item use, combat (`CCS_PlayerActiveItemDriver`, `CCS_PlayerCombatDriver`) |
| **SecondaryAction** | Mouse RMB | Secondary item actions (module-specific) |
| **Consume** | F | Food consume driver |
| **BuildMode** | B | Building placement |
| **Inventory** | I | Inventory UI (when wired) |
| **Pause** | Escape | Pause / cursor unlock |

**Reload (firearms):** routed through **`CCS_DevHotkeyUtility.WasReloadActiveFirearmPressed()`** (dev chord registry), not a separate Input Actions entry in 2.1.1.

---

## Dev / menu hotkeys (not in Input Actions asset)

Playtest, save debug, vendor HUD, and similar chords use **`CCS_DevHotkeyUtility`** + **`CCS_KeyboardInputUtility`** (`Keyboard.current` / `Mouse.current` from the Input System package). Examples: F1–F12 playtest steps, F5/F9 save/load, Ctrl+Alt+M/T menu toggles.

Registry: `Assets/CCS/Modules/CharacterController/Runtime/Input/CCS_DevHotkeyUtility.cs`

---

## UI map

| Action | Purpose |
|--------|---------|
| Navigate | UI focus navigation |
| Submit / Cancel | Confirm / back |
| TabLeft / TabRight | Tab switching |

Used when UI action map consumers are enabled (foundation present on asset).

---

## Verification checklist (2.1.1)

- [x] `CCS_Survival_InputActions.inputactions` exists under `Assets/CCS/Survival/Input/`
- [x] `PF_CCS_Player` assigns asset to `CCS_CharacterInputActionProvider`
- [x] Character controller validator checks asset + prefab wiring
- [x] No runtime `UnityEngine.Input` usage (foundation input scan)
- [x] Movement / look / sprint / interact / primary / consume read through provider

---

## Related docs

- [CCS_CharacterController_Module.md](../../Modules/CharacterController/Documentation/CCS_CharacterController_Module.md)
- [CCS_SurvivalInputValidationUtility.cs](../Editor/Development/Validation/CCS_SurvivalInputValidationUtility.cs)
