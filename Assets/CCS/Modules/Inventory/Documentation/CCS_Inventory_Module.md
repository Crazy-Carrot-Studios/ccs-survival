# CCS Survival — Inventory Module

**Milestone:** 0.4.0 — Inventory Module Foundation  
**Module ID:** `ccs.survival.inventory`  
**Namespace:** `CCS.Modules.Inventory` (editor: `CCS.Modules.Inventory.Editor`)  
**Author:** James Schilz (Developer)  
**Date:** 2026-05-28  
**Status:** Foundation complete (data architecture only; not wired to bootstrap installer)

---

## Purpose

Provide the **runtime inventory data architecture** used by future Equipment, Crafting, Storage, Loot, Save, and UI systems — without implementing any of those features in 0.4.0.

The module answers:

| Question | Owner |
|----------|--------|
| What is an item? | `CCS_ItemDefinition` + `CCS_ItemCategory` |
| How much of an item is held? | `CCS_ItemStack` |
| Where is it stored? | `CCS_InventorySlot` inside `CCS_InventoryContainer` |
| Who owns the player inventory? | `CCS_PlayerInventoryService` |
| What changed? | Inventory events on the service |

---

## Architecture flow

```text
CCS_ItemDefinition (ScriptableObject identity)
        ↓
CCS_ItemStack (definition + quantity)
        ↓
CCS_InventorySlot (per-slot validation + merge/split)
        ↓
CCS_InventoryContainer (variable slot count)
        ↓
CCS_PlayerInventoryService (player owner + events)
```

**Critical rule:** Inventory never references Interaction, Equipment, Crafting, Save, Storage world objects, or UI in 0.4.0.

---

## Folder layout

```text
Assets/CCS/Modules/Inventory/
  Runtime/
    Definitions/    → item category + item definition
    Data/             → stacks, slots, snapshots
    Containers/       → container interface + implementation
    Services/         → player inventory service
    Events/           → event args + contracts
    Profiles/         → CCS_InventoryProfile
    Validation/       → runtime profile validation
  Editor/
    Validation/       → pipeline validator + menu
  Documentation/      → this file

Assets/CCS/Survival/Profiles/Inventory/
  CCS_DefaultInventoryProfile.asset
```

---

## Item definitions

| Field | Type | Notes |
|-------|------|-------|
| Item Id | string | Stable reverse-DNS identity |
| Display Name | string | Player-facing label |
| Description | string | Future UI / tooltips |
| Category | `CCS_ItemCategory` | Generic, Resource, Consumable, etc. |
| Max Stack Size | int | Per-slot cap when stackable |
| Weight | float | Placeholder for future encumbrance |
| Icon | Sprite | Placeholder reference for future UI |
| Is Stackable | bool | When false, max stack size is treated as 1 |

No gameplay stats or equipment data in 0.4.0.

---

## Stacks and slots

| Type | Role |
|------|------|
| `CCS_ItemStack` | Immutable-friendly value type: definition + quantity |
| `CCS_InventorySlot` | Single slot with empty check, accept validation, add/remove |
| `CCS_InventorySnapshot` | Read-only container state for queries and future save hooks |

---

## Container

`CCS_IInventoryContainer` / `CCS_InventoryContainer`:

| Method | Behavior |
|--------|----------|
| `AddItem` | Merge into matching stacks first, then empty slots |
| `RemoveItem` | Partial removal across stacks (stack splitting) |
| `CanAdd` | Capacity check without mutation |
| `HasItem` | Quantity threshold check |
| `GetQuantity` | Total count for one definition |
| `Clear` | Empty all slots |
| `CreateSnapshot` | Immutable read model |

Supports **variable slot count**, **stack merging**, and **partial stack removal**.

---

## Player inventory service

`CCS_PlayerInventoryService` implements `CCS_ISurvivalService`:

1. `InitializeFromProfile(CCS_InventoryProfile)` — creates container with profile slot count
2. `AddItem` / `RemoveItem` — delegates to container, raises events
3. `CanAdd` / `HasItem` / `GetQuantity` — query helpers
4. `ClearInventory` / `CreateSnapshot` — bulk operations and read model

No interaction references. No save system references.

---

## Events

| Event | When |
|-------|------|
| `ItemAdded` | One or more units successfully added |
| `ItemRemoved` | One or more units successfully removed |
| `InventoryChanged` | Any inventory mutation (add, remove, clear) |
| `InventoryFull` | Add could not accept all requested quantity |

Event name constants: `CCS_InventoryEvents`.

---

## Profile

`CCS_InventoryProfile` extends `CCS_SurvivalProfileBase`:

| Setting | Default (0.4.0) |
|---------|-----------------|
| Inventory slot count | **40** |
| Enable weight limit | false (placeholder) |
| Max carry weight | 100 (placeholder) |

Default asset: `Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset`

---

## Validation

| Menu | Path |
|------|------|
| Validate Inventory | **CCS → Survival → Inventory → Validate Inventory** |

Batch entry: `CCS.Modules.Inventory.Editor.CCS_InventoryValidationMenu.ValidateInventory`

---

## Assemblies

| Assembly | References |
|----------|------------|
| `CCS.Modules.Inventory.Runtime` | `CCS.Core.Runtime`, `CCS.Survival.Runtime` |
| `CCS.Modules.Inventory.Editor` | Inventory runtime, `CCS.Survival.Editor` |

---

## Capacity expansion hook (0.4.1a)

`CCS_InventoryCapacityModifierSnapshot` is a lightweight placeholder struct:

| Field | Purpose |
|-------|---------|
| `AdditionalInventorySlots` | Bonus slots from equipped gear (future composition input) |
| `AdditionalCarryWeight` | Bonus carry weight from equipped gear (future composition input) |

Inventory does **not** reference the Equipment module. Bootstrap/composition will:

1. Read aggregate modifiers from `CCS_PlayerEquipmentService`
2. Build `CCS_InventoryCapacityModifierSnapshot`
3. Apply resolved capacity to player inventory before UI reads final values

---

## Future integrations (post-0.4.0)

| Feature | Notes |
|---------|--------|
| Equipment | Equip slots consume item stacks from inventory; capacity modifiers via composition (0.4.1a hook) |
| Crafting | Recipe inputs/outputs via container API |
| Storage / chests | Additional `CCS_InventoryContainer` instances |
| Loot | Spawn items into player container via service |
| Save / load | Serialize `CCS_InventorySnapshot` or slot arrays |
| UI / HUD | Subscribe to inventory events; render snapshots |
| Weight enforcement | Use profile weight fields + definition weight |
| Interaction pickup | Interaction module calls `AddItem` — no direct coupling in 0.4.0 |

---

## Default profile (0.4.0)

| Setting | Value |
|---------|-------|
| Inventory slots | 40 |
| Weight limit | Disabled |
| Max carry weight | 100 (placeholder) |
