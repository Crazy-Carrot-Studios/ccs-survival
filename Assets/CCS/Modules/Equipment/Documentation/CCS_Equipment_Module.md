# CCS Survival — Equipment Module

**Milestone:** 1.2.1 — Held Item Pose + Socket Cleanup  
**Module ID:** `ccs.survival.equipment`  
**Namespace:** `CCS.Modules.Equipment` (editor: `CCS.Modules.Equipment.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-28  
**Status:** Foundation complete (runtime architecture only; not wired to bootstrap installer)

---

## Purpose

Provide the **runtime equipment architecture** that sits on top of Inventory and supports clothing, armor, weapons, tools, stat bonuses, durability, and **placeholder equipped visuals** driven by the equipment service.

**1.2.0+ visual foundation notes:**

- Equipped visuals are **primitive Unity placeholder meshes** (cylinders/cubes/capsules), not final art.
- Attachment uses **transform sockets** on `PF_CCS_Player` (`EquipmentRig` / `Socket_*`). Humanoid bone sockets and IK are planned later.
- **Equipment state remains service-driven** via `CCS_PlayerEquipmentService`; visuals mirror equip/unequip/save-restore only.
- No full combat animation, aim poses, or IK in this milestone (future flags exist on visual definitions).

**1.2.1 pose cleanup:**

- Socket local positions/rotations on the capsule rig were tuned for third-person readability (hands, back, hips, backpack).
- `CCS_EquipmentVisualDefinition` local offsets/scales were tuned so held spear, knife, hatchet, pick, torch, and bedroll attach deliberately.
- Runtime visual controller duplicate/stale-instance guards were hardened; save/load resync remains duplicate-safe.
- Batch: `CCS_EquipmentVisualPoseCleanupBootstrapSetup.ExecuteBatch`

**1.2.2 active item sync:**

- `CCS_ActiveItemService` does **not** spawn separate active visuals; it tracks the active item ID that should match existing equipment visuals on hand sockets.
- Clearing equipment clears active item state on bulk restore/clear events.

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
    Visuals/        → socket rig, visual definitions, visual controller
  Editor/
    Validation/     → pipeline validator + menu
  Documentation/    → this file

Assets/CCS/Survival/Profiles/Equipment/
  CCS_DefaultEquipmentProfile.asset
  CCS_DefaultEquipmentVisualProfile.asset

Assets/CCS/Survival/Prefabs/Equipment/Visuals/
  PF_CCS_Visual_* placeholder prefabs

Assets/CCS/Survival/Content/Equipment/Visuals/
  CCS_EquipmentVisual_* definition assets
```

---

## Visual equipment flow (1.2.0)

```text
CCS_PlayerEquipmentService (equip / unequip / restore)
        ↓ events + bulk restore resync
CCS_PlayerEquipmentVisualBinder (PF_CCS_Player)
        ↓
CCS_EquipmentVisualController
        ↓ lookup by item ID
CCS_EquipmentVisualProfile / CCS_EquipmentVisualDefinition
        ↓ spawn prefab at socket
CCS_EquipmentAttachmentRig → CCS_EquipmentAttachmentSocket
```

Bootstrap batches:

```text
CCS.Survival.Editor.Development.CCS_EquipmentVisualFoundationBootstrapSetup.ExecuteBatch
CCS.Survival.Editor.Development.CCS_EquipmentVisualPoseCleanupBootstrapSetup.ExecuteBatch
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
| Back | Backpacks, capes, pack frames |
| Satchel | Belt satchels, small carry pouches |
| Bedroll | Bedroll / bedroll strap carry expansion |
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
| `CreateSnapshot` | Read-only equipment state including aggregate capacity modifiers |
| `GetAdditionalInventorySlots` | Sum of equipped inventory slot bonuses |
| `GetAdditionalCarryWeight` | Sum of equipped carry weight bonuses |
| `CaptureState` / `RestoreState` | JSON persistence via `CCS_EquipmentSaveData` at **0.6.2** |

SaveableId: `ccs.survival.saveable.equipment.player`

No inventory UI references. No automatic inventory removal in 0.4.1 — future integration will coordinate stack removal on equip.

---

## Save/load persistence (0.6.2)

| Type | Role |
|------|------|
| `CCS_EquipmentSaveData` | Root payload with `saveDataVersion`, capacity modifiers, equipped slot entries |
| `CCS_EquipmentSaveSlotEntry` | Slot type, item ID, durability fields |
| `CCS_EquipmentItemDefinitionLookup` | Resolves saved item IDs using profile `SaveRestoreEquipmentDefinitions` catalog |

Equipment restores **after** inventory during load so item identities exist before equip state is applied.

Missing definitions or invalid slot mappings fail safely (slot skipped, warning logged).

---

## Carry capacity modifiers (0.4.1a)

`CCS_EquipmentItemDefinition` exposes modifier fields (defaults: flags **false**, values **0**):

| Field | Purpose |
|-------|---------|
| `ModifiesInventoryCapacity` | When true, `AdditionalInventorySlots` contributes while equipped |
| `AdditionalInventorySlots` | Extra player inventory slots (non-negative) |
| `ModifiesCarryWeight` | When true, `AdditionalCarryWeight` contributes while equipped |
| `AdditionalCarryWeight` | Extra carry weight capacity (non-negative) |

Examples: satchel, backpack (`Back`), bedroll (`Bedroll`), future pack frame / saddle bag.

`CCS_EquipmentCapacityModifierUtility` aggregates equipped modifiers.  
`CCS_EquipmentSnapshot` includes `TotalAdditionalInventorySlots` and `TotalAdditionalCarryWeight`.

`EquipmentChanged` fires with a capacity-specific message when capacity-affecting gear is equipped or unequipped.

**Rules:**

- Equipment exposes modifiers only — it does **not** resize inventory containers.
- Inventory module does **not** reference Equipment in 0.4.1a.
- Future bootstrap/composition will map equipment modifiers into inventory resolved capacity.
- UI should read **final resolved capacity** from composition, not raw profile slot count alone.

---

## Environmental survival modifiers (0.7.4)

Equipment is the **authoritative source** for environmental resistance while items are equipped.

| Field | Purpose |
|-------|---------|
| `TemperatureResistance` | Adds warmth mitigation to cold ambient pressure (default **0**) |
| `WetnessResistance` | Reduces effective wetness (default **0**) |
| `ExposureResistance` | Reduces effective exposure (default **0**) |

Survival modifiers only — **not** armor, combat stats, damage reduction, or visuals.

`CCS_EquipmentEnvironmentalModifierSnapshot` aggregates equipped resistances.  
`CCS_PlayerEquipmentService.GetEnvironmentalModifiers()` returns the aggregate without per-call list allocation.

`CCS_EquipmentEnvironmentRuntimeBridge` resolves the equipment service safely from the runtime registry.

Test validation assets under `Assets/CCS/Survival/Profiles/Equipment/TestItems/`:

| Asset | Modifiers |
|-------|-----------|
| Warm Hat | Temperature +1 |
| Heavy Coat | Temperature +2, Exposure +0.3 |
| Waterproof Boots | Wetness +0.4 |

Future clothing progression will expand slot coverage, tiered gear, and crafting sources.

---

## Primitive tool equipment (0.9.2)

Equipment definitions for primitive tools live under `Assets/CCS/Survival/Content/Equipment/Primitive/`:

| Equipment | Slot | Item |
|-----------|------|------|
| `CCS_Equipment_Knife` | MainHand | Starter Knife |
| `CCS_Equipment_BoneHatchet` | Tool | Bone Hatchet |
| `CCS_Equipment_BonePick` | Tool | Bone Pick |
| `CCS_Equipment_BoneKnife` | Tool | Bone Knife |
| `CCS_Equipment_BoneShovel` | Tool | Bone Shovel |

`CCS_PrimitiveToolEquipTestHarness` verifies equip paths, HUD refresh, inventory capacity integrity, and knife save/load persistence. Disabled by default.

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
| **Composition (future)** | Applies `CCS_InventoryCapacityModifierSnapshot` to resolved player capacity |

Equipment definitions **require** an inventory item reference. Future milestones will:

- Remove one stack from inventory on equip
- Return stack to inventory on unequip
- Validate player owns item before equipping
- Connect equipment aggregate modifiers to inventory slot/weight resolution via bootstrap

Inventory placeholder: `CCS_InventoryCapacityModifierSnapshot` (no Equipment assembly reference).

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
