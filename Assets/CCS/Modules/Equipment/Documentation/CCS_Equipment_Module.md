# CCS Survival — Equipment Module

**Milestone:** 0.4.1 — Equipment Module Foundation  
**Module ID:** `ccs.survival.equipment`  
**Namespace:** `CCS.Modules.Equipment` (editor: `CCS.Modules.Equipment.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-28  
**Status:** Foundation complete (runtime architecture only; not wired to bootstrap installer)

---

## Purpose

Provide the **runtime equipment architecture** that sits on top of Inventory and will later support clothing, armor, weapons, tools, stat bonuses, durability, and character visuals — without implementing UI, combat, weapon functionality, or visuals in 0.4.1.

The module answers:

| Question | Owner |
|----------|--------|
| What slot can an item use? | `CCS_EquipmentItemDefinition.AllowedSlot` |
| What is equipped where? | `CCS_EquipmentSlot` + `CCS_EquippedItem` |
| Who owns player equipment? | `CCS_PlayerEquipmentService` |
| How is durability tracked? | `CCS_DurabilityState` on equipped items |
| What changed? | Equipment events on the service |

---

## Architecture flow

```text
CCS_ItemDefinition (Inventory identity)
        ↓
CCS_EquipmentItemDefinition (slot + durability + stat placeholder)
        ↓
CCS_EquippedItem (runtime equipped instance)
        ↓
CCS_EquipmentSlot (per-slot validation)
        ↓
CCS_PlayerEquipmentService (player loadout owner + events)
```

**Critical rule:** Equipment references inventory item definitions but does **not** duplicate inventory ownership. Inventory containers remain the source of item stacks; equipment tracks what is worn/wielded by reference.

---

## Folder layout

```text
Assets/CCS/Modules/Equipment/
  Runtime/
    Definitions/    → slot types + equipment item definitions
    Data/           → equipped items, snapshots, durability state
    Slots/          → per-slot validation
    Services/       → player equipment service
    Events/         → event args + contracts
    Profiles/       → CCS_EquipmentProfile
    Validation/     → runtime profile/definition validation
  Editor/
    Validation/     → pipeline validator + menu
  Documentation/    → this file

Assets/CCS/Survival/Profiles/Equipment/
  CCS_DefaultEquipmentProfile.asset
```

---

## Slot architecture

Supported `CCS_EquipmentSlotType` values:

| Slot | Typical future use |
|------|-------------------|
| Head | Hats, helmets |
| Chest | Jackets, armor vests |
| Legs | Pants, chaps |
| Feet | Boots |
| Hands | Gloves |
| Back | Backpacks, capes |
| Neck | Necklaces, bandanas |
| Accessory | Rings, trinkets |
| MainHand | Primary weapon/tool |
| OffHand | Shield, lantern |
| Sidearm | Holstered sidearm |
| Tool | Belt tool slot |

`CCS_EquipmentSlot` validates:

- Slot compatibility with definition `AllowedSlot`
- Empty vs occupied state
- Equip/unequip without mutating inventory stacks

---

## Equipment service

`CCS_PlayerEquipmentService` implements `CCS_ISurvivalService`:

| Method | Behavior |
|--------|----------|
| `EquipItem` | Validates definition + slot compatibility; occupies slot |
| `UnequipItem` | Clears slot and returns equipped instance |
| `GetEquippedItem` | Query equipped item in slot |
| `IsSlotEmpty` / `IsSlotOccupied` | Slot state helpers |
| `ValidateSlotCompatibility` / `CanEquip` | Pre-flight checks |
| `DamageEquippedDurability` / `RepairEquippedDurability` | Durability foundation API |
| `CreateSnapshot` | Read-only equipment state |

No inventory UI references. No automatic inventory removal in 0.4.1 — future integration will coordinate stack removal on equip.

---

## Durability system (foundation)

`CCS_DurabilityState`:

| API | Purpose |
|-----|---------|
| `CurrentDurability` / `MaxDurability` | Runtime values |
| `DamageDurability` | Reduce durability (no gameplay loss yet) |
| `RepairDurability` | Restore durability |
| `IsBroken` | True when durability reaches zero |

Enabled per item via `CCS_EquipmentItemDefinition.DurabilityEnabled`.

---

## Inventory integration

| Layer | Responsibility |
|-------|----------------|
| **Inventory** | Owns stacks, add/remove, container capacity |
| **Equipment** | References `CCS_ItemDefinition` through `CCS_EquipmentItemDefinition` |

Equipment definitions **require** an inventory item reference. Future milestones will:

- Remove one stack from inventory on equip
- Return stack to inventory on unequip
- Validate player owns item before equipping

---

## Events

| Event | When |
|-------|------|
| `ItemEquipped` | Item successfully placed in slot |
| `ItemUnequipped` | Item removed from slot |
| `EquipmentChanged` | Any equipment mutation |
| `DurabilityChanged` | Durability damaged or repaired |
| `EquipmentFailed` | Invalid slot, occupied slot, or bad definition |

Event name constants: `CCS_EquipmentEvents`.

---

## Validation

| Menu | Path |
|------|------|
| Validate Equipment | **CCS → Survival → Equipment → Validate Equipment** |

Batch entry: `CCS.Modules.Equipment.Editor.CCS_EquipmentValidationMenu.ValidateEquipment`

---

## Assemblies

| Assembly | References |
|----------|------------|
| `CCS.Modules.Equipment.Runtime` | `CCS.Core.Runtime`, `CCS.Survival.Runtime`, `CCS.Modules.Inventory.Runtime` |
| `CCS.Modules.Equipment.Editor` | Equipment runtime, `CCS.Survival.Editor` |

---

## Future combat integration (post-0.4.1)

| Feature | Notes |
|---------|--------|
| Weapon stats | Extend equipment definitions; consume MainHand/OffHand/Sidearm |
| Armor mitigation | Stat modifiers from placeholder field |
| Durability loss on use | Call `DamageEquippedDurability` from combat systems |
| Character visuals | Subscribe to equip/unequip events; attach meshes |
| Inventory equip flow | Inventory service removes stack; equipment service equips reference |
| Save / load | Serialize `CCS_EquipmentSnapshot` + durability values |

---

## Default profile (0.4.1)

| Setting | Value |
|---------|-------|
| Allow dual wield | false (placeholder) |
| Require durability for equip | false (placeholder) |

Asset: `Assets/CCS/Survival/Profiles/Equipment/CCS_DefaultEquipmentProfile.asset`
