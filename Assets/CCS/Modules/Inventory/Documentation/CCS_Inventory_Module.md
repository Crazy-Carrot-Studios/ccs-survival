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

## Tool identity (0.9.1)

`CCS_ItemDefinition` exposes optional harvest tool identity:

| Field | Type | Notes |
|-------|------|-------|
| Has Tool Identity | bool | When true, item satisfies matching harvest tool checks |
| Tool Type | `CCS_ItemToolType` | Aligns with `CCS_RequiredToolType` values |

`CCS_InventoryToolUtility` scans the player inventory for a matching tool item. No durability, equipped-slot logic, or weapon damage in 0.9.1.

---

## Starter loadout items (0.9.1)

Starter content lives under `Assets/CCS/Survival/Content/Items/Starter/`:

| Asset | Display Name | Role |
|-------|--------------|------|
| `CCS_Item_Knife` | Knife | Starter tool (`CCS_ItemToolType.Knife`) |
| `CCS_Item_BasicFood` | Basic Food | Starter consumable placeholder |
| `CCS_Item_Coin` | Coin | Currency placeholder (no economy yet) |
| `CCS_Item_Branch` | Branch | Early harvest material |
| `CCS_Item_Spear` | Spear | Primitive craft output placeholder |
| `CCS_Item_BowStave` | Bow Stave | Bow component placeholder |
| `CCS_Item_ArrowShaft` | Arrow Shaft | Arrow component placeholder |
| `CCS_Item_CampfireKit` | Campfire Kit | Campfire kit placeholder |

`CCS_StarterLoadoutService` grants Knife x1, Basic Food x2, and Coin x10 on fresh runtime when inventory is empty. Save restore skips duplicate grants.

---

## Tool and weapon classifications (0.9.2)

| Field | Type | Notes |
|-------|------|-------|
| Gameplay Kind | `CCS_ItemGameplayKind` | Generic, Tool, Weapon, ToolAndWeapon |
| Tool Archetype | `CCS_ToolArchetype` | Knife, Hatchet, Pick, Shovel |
| Tool Tier | `CCS_ToolTier` | Primitive, Bone, Stone, Iron, Steel |
| Weapon Archetype | `CCS_WeaponArchetype` | Knife, Spear, Bow, Club |
| Weapon Type | `CCS_WeaponType` | Melee, Ranged, Thrown placeholder |
| Damage Type | `CCS_DamageType` | Slash, Pierce, Blunt placeholder |
| Range Type | `CCS_RangeType` | Melee, ShortRanged, LongRanged placeholder |

Resource placeholders: Bone, Sinew, Hide under `Content/Items/Resources/Primitive/`. Bone tools under `Content/Items/Tools/Bone/`. `CCS_ItemGameplayUtility` maps archetypes to harvest tool types. Equipped tools in MainHand/Tool slots also satisfy harvest checks at 0.9.2.

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

`CCS_PlayerInventoryService` implements `CCS_ISurvivalService` and `CCS_ISaveable` at **0.6.2**:

1. `InitializeFromProfile(CCS_InventoryProfile)` — creates container with profile slot count and save-restore item catalog
2. `AddItem` / `RemoveItem` — delegates to container, raises events
3. `CanAdd` / `HasItem` / `GetQuantity` — query helpers
4. `ClearInventory` / `CreateSnapshot` — bulk operations and read model
5. `CaptureState` / `RestoreState` — JSON persistence via `CCS_InventorySaveData`

SaveableId: `ccs.survival.saveable.inventory.player`

---

## Save/load persistence (0.6.2)

| Type | Role |
|------|------|
| `CCS_InventorySaveData` | Root payload with `saveDataVersion`, slot count, capacity modifier fields, slot entries |
| `CCS_InventorySaveSlotEntry` | Per-slot `itemId` + `quantity` (empty slots use blank id / zero quantity) |
| `CCS_ItemDefinitionLookup` | Resolves saved item IDs using profile `SaveRestoreItemDefinitions` catalog |

Restore skips unknown item definitions safely without corrupting other slots.

Inventory restores **before** equipment during load (see Save/Load module docs).

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
| Save restore item catalog | `SaveRestoreItemDefinitions` — resolves saved item IDs at load time |

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

## Resource harvest integration (0.5.2)

World resource harvesting adds items through the existing container API:

| Operation | API |
|-----------|-----|
| Pre-check capacity | `CanAdd(CCS_ItemDefinition, int quantity)` |
| Grant harvested drops | `AddItem(CCS_ItemDefinition, int quantity)` |

Harvest failures return safe results when inventory is full. Inventory events refresh the HUD summary without silent item loss.

---

## Crafting integration (0.5.3)

Crafting consumes and produces items through the same container API:

| Operation | API |
|-----------|-----|
| Pre-check ingredients | `HasItem(CCS_ItemDefinition, int quantity)` |
| Pre-check output capacity | `CanAdd(CCS_ItemDefinition, int quantity)` |
| Consume recipe inputs | `RemoveItem(CCS_ItemDefinition, int quantity)` |
| Grant recipe outputs | `AddItem(CCS_ItemDefinition, int quantity)` |

`CCS_CraftingService` validates output capacity before removing ingredients and restores ingredients if result grant fails. Crafting events drive HUD notifications; inventory summary refreshes on successful craft.

Test craft outputs (`Campfire Kit`, `Bandage`) fit the default 40-slot inventory profile.

---

## Future integrations (post-0.4.0)

| Feature | Notes |
|---------|--------|
| Equipment | Equip slots consume item stacks from inventory; capacity modifiers via composition (0.4.1a hook) |
| Crafting | **Wired at 0.5.3** via `CCS_CraftingService` |
| World resource harvest | **Wired at 0.5.2** via `CCS_ResourceHarvestService` |
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
