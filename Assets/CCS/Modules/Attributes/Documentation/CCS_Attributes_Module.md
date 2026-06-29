# CCS Attributes Module

Version: **0.3.0**

## Purpose

The Attributes module provides a **generic attribute model** for survival gameplay stats. v0.3.0 ships **Health only**, but the container, definition assets, and replication path are designed for future attributes:

- Stamina
- Hunger
- Thirst
- Temperature
- Exposure
- Damage / healing modifiers
- Status effects

Attributes live in `Assets/CCS/Modules/Attributes/` and are **not** part of CharacterController.

## Folder ownership

```text
Assets/CCS/Modules/Attributes/
├── Runtime/
│   ├── Components/       CCS_AttributeContainer, CCS_NetworkAttributeReplicator
│   ├── Data/             CCS_AttributeValue, CCS_DamageRequest, CCS_HealRequest
│   ├── Events/           Changed, damage, death placeholder events
│   ├── Profiles/         CCS_AttributeDefinition
│   ├── Services/         CCS_AttributeService
│   ├── UI/               CCS_PlayerAttributeHud
│   └── Validation/       CCS_AttributesValidationUtility
├── Editor/               Validator, prefab builder, menu
├── Tests/
│   ├── Runtime/          CCS_TestPlayerAttributeDebugInput
│   └── Profiles/         CCS_AttributeDefinition_Health.asset
└── Documentation/
```

## Generic attribute model

| Type | Role |
|------|------|
| `CCS_AttributeDefinition` | ScriptableObject profile per attribute type (inherits `CCS_SurvivalProfileBase`) |
| `CCS_AttributeValue` | Readonly runtime snapshot (`current`, `min`, `max`) |
| `CCS_AttributeContainer` | Holds all attribute values on an actor; clamps and raises events |
| `CCS_AttributeService` | Thin read/query wrapper over the container |
| `CCS_NetworkAttributeReplicator` | Server-authoritative Netcode replication for Health |

Health is defined by `CCS_AttributeDefinition_Health.asset`:

- Profile ID: `ccs.survival.profile.attributes.health`
- Default / max: `100`
- Min: `0`

## Server-authoritative damage flow

```text
Local owner test input (K)
        │
        ▼
CCS_NetworkAttributeReplicator.RequestSelfDamage(10)
        │
        ├─ Solo / no Netcode session → CCS_AttributeContainer.ApplyDamage locally
        │
        └─ Multiplayer
              ├─ Host owner → server applies damage directly
              └─ Client owner → RequestSelfDamageServerRpc
                        │
                        ▼
                  Server validates sender == OwnerClientId
                        │
                        ▼
                  CCS_AttributeContainer.ApplyDamage
                        │
                        ▼
                  NetworkVariable<float> health replicates
                        │
                        ▼
                  Owner HUD updates via AttributeChanged
```

## Solo / offline behavior

When `NetworkManager` is not listening:

- `CCS_AttributeContainer` initializes Health to `100` in `Awake`
- `CCS_NetworkAttributeReplicator.RequestSelfDamage` applies damage locally
- `CCS_PlayerAttributeHud` treats non-spawned players as local owner
- No Netcode session is required

## Test player integration

Canonical prefab:

`Assets/CCS/Modules/CharacterController/Prefabs/Player/PF_CCS_CharacterController_Player_Networked.prefab`

Wired by `CCS_AttributesTestPlayerPrefabBuilder`:

| Component | Purpose |
|-----------|---------|
| `CCS_AttributeContainer` | Runtime Health storage |
| `CCS_AttributeService` | Read/query surface |
| `CCS_NetworkAttributeReplicator` | Server-authoritative Health replication |
| `CCS_PlayerAttributeHud` | Local-owner Health HUD (`100 / 100`) |
| `CCS_TestPlayerAttributeDebugInput` | Press **K** for 10 self-damage (test only) |

CharacterController movement, camera binding, nameplates, join notifications, offline bootstrap, and owner network transform are unchanged.

## Editor menu

| Menu | Action |
|------|--------|
| `CCS/Attributes/Validate Attributes Module` | Repairs test player attribute wiring, then validates module |

Master test setup also calls the Attributes prefab builder automatically.

## Playtest checklist

### Solo Master Test

- [ ] Player spawns
- [ ] Health HUD shows `100 / 100`
- [ ] Press **K** → Health becomes `90 / 100`
- [ ] No join notification UI
- [ ] Movement and camera still work

### Host

- [ ] Host spawns with Health `100 / 100`
- [ ] Host HUD visible only to host
- [ ] Host test damage reduces host health
- [ ] Join notification still appears
- [ ] Movement and camera still work

### Client joins

- [ ] Client spawns with Health `100 / 100`
- [ ] Client HUD visible only to client
- [ ] Host HUD still shows host health only
- [ ] Client test damage reduces client health on server and replicates back
- [ ] Host sees client moving normally
- [ ] No duplicate HUDs or duplicate control
- [ ] Nameplates still work

## Future work (not in v0.3.0)

- Interaction module
- Dedicated Networking module
- Combat, weapons, hitboxes
- Respawn on `CCS_PlayerDeathEvent`
- Additional attribute definitions and replicated stat sets
- Status effects and regeneration systems
