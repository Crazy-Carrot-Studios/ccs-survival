# CCS Interaction Module

**Version:** 0.5.4 — Pickup and WalkThroughDoor

## Purpose

Reusable interaction foundation for survival gameplay: owner-side detection, server-authoritative requests, prompt presentation, and animation routing. Lives in `Assets/CCS/Modules/Interaction/` and is **not** part of CharacterController.

## Supported interactable kinds (v0.5.4)

| Kind | Animation | Behavior |
|------|-----------|----------|
| **Pickup** | `PickUp_RH` | Collect / destroy test pickup |
| **WalkThroughDoor** | `WalkThroughDoor_RH` | Open hinged door |

Not in scope: inventory, door close/toggle, left-hand variants, crafting stations.

## Readiness rules

Prompt and **E** accept only when **all** pass:

1. **Awareness** — overlap sphere at `InteractionScanOrigin` (Interactable layer)
2. **Forward volume** — target bounds center in player-local box (`z > 0`, within half-width/height and strict range)
3. **Line of sight** — sphere cast from scan origin to collider **closest point**; player colliders skipped

Pickup strict range: **1.5 m**. Door strict range: **1.75 m**.

## Owner flow

```text
Local owner Update
        │
        ▼
Overlap candidates → pick closest in forward volume + LOS
        │
        ▼
Prompt HUD when ready (CCS_InteractionPromptPresenter)
        │
        ▼
E pressed → volume + LOS re-check → BeginInteractionLock
        │
        ▼
CCS_InteractableExecutor → CCS_IInteractable.Interact
        │
        ├─ Solo / offline → local apply
        └─ Netcode → ServerRpc → server validate → Interact
        │
        ▼
InteractionCompleted → animator trigger (PickUp_RH / WalkThroughDoor_RH)
        │
        ▼
Movement unlock after lock duration
```

## Folder ownership

```text
Assets/CCS/Modules/Interaction/
├── Runtime/
│   ├── Contracts/     CCS_IInteractable, lock/busy/target contracts
│   ├── Components/    Scanner, executor, door, label target, test helpers
│   ├── Data/          Request, result, definition, animation keys
│   ├── Events/        InteractionCompletedEvent
│   ├── Profiles/      CCS_InteractionScannerProfile
│   ├── UI/            CCS_InteractionPromptPresenter
│   └── Validation/
├── Editor/            Builders, validators, batch entries
├── Tests/
│   ├── Prefabs/       PF_CCS_TestInteractable_PickupItem.prefab
│   └── Profiles/      CCS_InteractionScannerProfile_Default.asset
└── Documentation/
```

## Master Test targets

| Object | Scene placement | Kind |
|--------|-----------------|------|
| `CCS_TestDetectionCube` | Near spawn | Pickup |
| Building door interactable | Test building (~30, 30) | WalkThroughDoor |

Scene: `Assets/CCS/Modules/CharacterController/Scenes/Validation/SCN_CCS_CharacterController_Validation.unity`

## Test player integration

Canonical prefab: `Assets/CCS/Modules/CharacterController/Prefabs/Player/PF_CCS_CharacterController_Player_Networked.prefab`

Wired by `CCS_InteractionTestPlayerPrefabBuilder`:

| Component | Role |
|-----------|------|
| `CCS_NetworkInteractionScanner` | Owner detection, E input, server path |
| `CCS_InteractionPromptPresenter` | Press [E] HUD |
| `CCS_PlayerInteractionAnimator` | Lock + animator triggers (CharacterController) |
| `InteractionScanOrigin` | Chest-height scan origin (local Y = 1) |

### Module coupling (intentional)

`CCS.Modules.CharacterController.Runtime` references `Interaction.Runtime` and `Attributes.Runtime` for the **canonical test player** integration (scanner, prompt, motor lock, health HUD). This is accepted for the current milestone.

**Future:** If coupling grows, extract a `CharacterController.InteractionBridge` (or similar) assembly rather than expanding direct runtime references.

## Scanner profile

`Tests/Profiles/CCS_InteractionScannerProfile_Default.asset`:

| Field | Default |
|-------|---------|
| Profile ID | `ccs.survival.profile.interaction.scanner.default` |
| Broad detection | 3 m overlap at scan origin |
| Cooldown | 0.25 s |
| Enable debug logs | Off (use profile flag for verbose scanner logging) |

## Validation

**CCS → Interaction → Validate Interaction Module** (report-first; menu may repair known test assets before validate)

Checks module folders, scanner profile, test player wiring, pickup prefab, Master Test cube and door, owner gating, solo and server paths.

Batch: `CCS.Modules.Interaction.Editor.CCS_InteractionValidationBatchEntry.RunFromBatchMode`

## Playtest checklist — Solo Master Test

- [ ] Player spawns at `TP_Spawn_Host`
- [ ] Health HUD works; **K** damages health
- [ ] Face pickup cube → **Press [E]** prompt appears
- [ ] **E** collects cube; `PickUp_RH` fires; movement locks then unlocks
- [ ] Walk to building door → prompt appears
- [ ] **E** opens door; `WalkThroughDoor_RH` fires
- [ ] Clean console (no errors)

## Retired (pre–0.5.4)

- `CCS_TestToggleInteractable` and toggle cube prefab — removed; do not reintroduce without a new design pass.
