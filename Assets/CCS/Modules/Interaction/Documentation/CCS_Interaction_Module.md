# CCS Interaction Module

Version: **0.4.0**

## Purpose

The Interaction module provides a **reusable interaction foundation** for survival gameplay. v0.4.0 ships a local-owner scanner, server validation path, and one networked test interactable. Future systems can build on the same contract without coupling to CharacterController.

Interaction lives in `Assets/CCS/Modules/Interaction/` and is **not** part of CharacterController.

## Folder ownership

```text
Assets/CCS/Modules/Interaction/
├── Runtime/
│   ├── Contracts/        CCS_IInteractable
│   ├── Components/       CCS_NetworkInteractionScanner, CCS_TestToggleInteractable
│   ├── Data/             CCS_InteractionRequest, CCS_InteractionResult
│   ├── Events/           CCS_InteractionCompletedEvent
│   ├── Profiles/         CCS_InteractionScannerProfile
│   └── Validation/       CCS_InteractionValidationUtility
├── Editor/               Asset builder, prefab builder, validator, menu
├── Tests/
│   ├── Prefabs/          PF_CCS_TestInteractable_ToggleCube.prefab
│   └── Profiles/         CCS_InteractionScannerProfile_Default.asset
└── Documentation/
```

## Owner scanner flow

```text
Local owner presses E (Keyboard.current)
        │
        ▼
CCS_NetworkInteractionScanner raycasts from camera or player forward
        │
        ▼
Find CCS_IInteractable on hit collider / parent
        │
        ├─ Solo / no Netcode session → local Interact
        │
        └─ Multiplayer
              ├─ Host owner → server validates and applies directly
              └─ Client owner → SubmitInteractionServerRpc
                        │
                        ▼
                  Server validates sender == OwnerClientId
                        │
                        ▼
                  Server validates range from scanner origin / hit point
                        │
                        ▼
                  CCS_IInteractable.Interact on server
                        │
                        ▼
                  Interactable state replicates (NetworkVariable, etc.)
```

## Server validation flow

The scanner builds a `CCS_InteractionRequest` with:

- `RequesterClientId`
- `TargetNetworkObjectId`
- `OriginPosition` and `HitPoint`
- `MaxRange` from `CCS_InteractionScannerProfile`

On the server, `CCS_NetworkInteractionScanner`:

1. Confirms the RPC sender matches the scanner owner.
2. Resolves the target `NetworkObject` by ID.
3. Calls `CanInteract` on the target.
4. Validates hit distance and actor distance against profile range.
5. Calls `Interact` only on the server for networked targets.

## Solo behavior

When `NetworkManager` is not listening:

- `CCS_NetworkInteractionScanner` treats the player as the local owner.
- Raycast hits resolve interactables without `NetworkObjectId`.
- `CCS_TestToggleInteractable` toggles local visual state immediately.
- No Netcode session is required.

## Scanner profile

`CCS_InteractionScannerProfile_Default.asset`:

| Field | Default |
|-------|---------|
| Profile ID | `ccs.survival.profile.interaction.scanner.default` |
| `interactionRange` | `3` meters |
| `interactionLayerMask` | All layers |
| `interactionCooldownSeconds` | `0.25` |
| `useCameraForward` | `true` |

## Test interactable

`PF_CCS_TestInteractable_ToggleCube.prefab`:

- Networked `CCS_TestToggleInteractable` on a cube visual.
- Toggles height and color between closed (red, low) and open (green, raised).
- Placed once in `SCN_CCS_CharacterController_MasterTest` near `TP_Spawn_Host`.
- Reachable by walking from spawn and pressing **E**.

## Test player integration

Canonical prefab:

`Assets/CCS/Modules/CharacterController/Tests/Prefabs/PF_CCS_CharacterController_TestPlayer_Networked.prefab`

Wired by `CCS_InteractionTestPlayerPrefabBuilder`:

| Component | Purpose |
|-----------|---------|
| `CCS_NetworkInteractionScanner` | Local-owner raycast scanner; E key input |
| `CCS_InteractionScannerProfile_Default` | Range, layer mask, cooldown, camera forward |

The scanner does **not** modify movement, camera, or `CCS_CharacterController_InputActions` bindings.

## Future uses

The `CCS_IInteractable` contract is intended for:

- Doors
- Pickups
- Crafting stations
- Storage containers
- Vendors
- Resource nodes

v0.4.0 intentionally excludes inventory, dialogue, crafting, vendors, and pickups.

## Validation

Editor menu: **CCS → Interaction → Validate Interaction Module**

Checks:

- Module folders and asmdefs exist
- Scanner profile asset exists and is valid
- Canonical player prefab has scanner + profile
- Test interactable prefab exists
- Master Test scene contains the toggle cube instance
- Scanner source gates to local owner
- Solo path does not require `NetworkManager`
- Multiplayer path is server-authoritative

## Playtest checklist

### Solo Master Test

- [ ] Player spawns
- [ ] Health HUD still works
- [ ] **K** still damages health
- [ ] Walk to toggle cube
- [ ] Press **E** — cube toggles
- [ ] No join notification UI
- [ ] Nameplate remains hidden on self

### Host

- [ ] Host spawns
- [ ] Host can press **E** on cube — cube toggles
- [ ] Health HUD still works
- [ ] Join notification still appears

### Client

- [ ] Client spawns
- [ ] Client controls only self
- [ ] Client presses **E** on cube — server validates
- [ ] Cube toggles for both host and client
- [ ] Remote movement/nameplates still work
- [ ] No duplicate camera, HUD, or control
