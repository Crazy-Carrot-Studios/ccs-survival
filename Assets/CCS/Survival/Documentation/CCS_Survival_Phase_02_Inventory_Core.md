# CCS Survival — Phase 2 Inventory Core

**Document Type:** Phase Engineering Plan  
**Project:** CCS Survival  
**Phase:** 2 — Inventory Core  
**Author:** James Schilz  
**Date:** 2026-05-28  
**Status:** Phase 2B — Inventory core implemented (0.7.0)

---

## Version policy

| Item | Value |
|------|--------|
| **Current project version** | **0.7.0 — Inventory Prototype Foundation** |
| **Prior version** | **0.6.1 — Phase One Cleanup Patch** |
| **Future build naming** | `Builds/Windows/CCS-Survival-0.7.0-*`, log prefix `[CCS 0.7.0-*]` |

---

## Phase 2A — Inventory Foundation Readiness + Architecture Planning

### Purpose

Prepare the survival prototype for Phase Two inventory work: remove stale test leftovers, confirm dev validation isolation, validate default scene cleanliness, and document inventory architecture before **`ccs.survival.inventory`** implementation.

### Pre-check

| Check | Result |
|-------|--------|
| Branch `main` up to date with `origin/main` | **Pass** |
| `bundleVersion` **0.6.1** | **Pass** |
| Build scene index **0** = `SCN_CCS_Survival_Bootstrap.unity` | **Pass** |
| Unity compile (no pending script errors in repo) | **Pass** (static review; Editor compile assumed on open) |

### Cleanup audit (2A)

**Removed (local stale, not tracked in git):**

| Item | Action |
|------|--------|
| `Assets/CCS/Survival/Editor/Temp/` | **Deleted** — stale standalone build scripts (0.4.0-F, 0.5.0-A, traversal smoke) |
| `Assets/CCS/Survival/Editor/Temp.meta` | **Deleted** |

**Kept (reusable):**

| Item | Role |
|------|------|
| Traversal test runtime + route/agent | Under **`CCS_DevValidationRoot`** (opt-in) |
| Hazard / vitals zone systems + editor setup menus | Dev validation |
| `CCS_DevValidationRootSceneSetup_Editor` | Enable/disable dev root |
| `CCS_InteractionPickupSceneSetup_Editor` | Prototype pickup wiring |
| Interaction scanner/input/pickup foundation | Phase One gameplay |
| Debug overlay | Phase One diagnostics |
| Phase One validation documentation | Historical record |

**Not staged / not committed:** `Builds/`, `Logs/`, `ProjectSettings/SceneTemplateSettings.json`, untracked `Editor.meta`.

### Scene cleanliness (committed scene)

**Default gameplay roots (active):**

| Root | Role |
|------|------|
| `PF_CCS_Survival_BootstrapRoot` (prefab instance) | Composition root, vitals module, debug overlay |
| `CCS_GameplayPlayAreaRoot` | **`CCS_PrototypeGround`** collider for default play |
| `CCS_PlayerRoot` | Movement, interaction scanner/input |
| `Main Camera` / `CM_PrototypeFollow` | Rendering + follow |
| `CCS_PrototypePickupsRoot` | `PU_FoodTin`, `PU_WaterCanteen`, `PU_Kindling` |
| `Directional Light` | Scene lighting |

**Dev validation (inactive by default):**

| Root | `m_IsActive` | Children (preserved) |
|------|:------------:|----------------------|
| `CCS_DevValidationRoot` | **0** | Environment/course, traversal route, traversal agent, hazards, vitals zones |

| Check | Result |
|-------|--------|
| `enableTraversalTest` | **0** |
| Pickups active and near spawn (not in hazard cluster) | **Pass** |
| Player spawn not inside active hazard/vitals zones | **Pass** (dev root off) |

### Play Mode sanity (2A)

| Check | Status |
|-------|--------|
| WASD / camera / overlay / pickup **E** / no dev zone telemetry | **Pending** — not run from automation (Phase 1J.3 manual pickup **Pass** on 0.6.1) |

Re-run a short Editor pass after 2A if desired; no scene YAML changes in 2A beyond cleanup outside repo.

### Final status (2A)

**Phase One remains validated at 0.6.1.** Project is ready for Phase 2B inventory implementation planning execution. Stale **Editor/Temp** removed locally.

---

## A. Inventory goals

| Goal | Detail |
|------|--------|
| **Store pickups** | Turn world **`CCS_SurvivalPickupCollectedEvent`** into durable inventory stacks |
| **Stackable resources** | Food, water, kindling, future crafting materials with stack limits |
| **Future consumption** | Hook food/water items to vitals without hard-wiring pickups to hunger/thirst |
| **Future crafting** | Item definitions + categories/tags as inputs to recipe systems |
| **Future equipment** | Separate container or slot rules later; not Phase 2B scope |
| **Save-stable IDs** | Lowercase dotted ids (`survival.item.food_tin`) — never Unity paths or instance IDs |
| **Multiplayer-conscious** | Clear authority ownership of inventory mutations per player/session host |

---

## B. Core inventory concepts

| Concept | Responsibility |
|---------|----------------|
| **Item Definition** | Immutable authoring data: id, display name, icon ref, category/tags, max stack, flags (consumable, equippable later) |
| **Item Stack** | Runtime pair: `itemId` + `quantity` (+ optional instance metadata later) |
| **Inventory Slot** | Holds zero or one stack; fixed index in a container |
| **Inventory Container** | Fixed-size slot array; add/remove/query; optional capacity rules later |
| **Inventory Service / Module** | `ccs.survival.inventory` — registers `CCS_ISurvivalInventoryService` (name TBD), owns containers |
| **Pickup-to-inventory handoff** | Listener on **`CCS_SurvivalPickupCollectedEvent`** maps `pickupId` → `itemId`, calls add |
| **Item tags / categories** | `food`, `water`, `fuel`, `material` for filtering, crafting, UI grouping |
| **Stack limits** | Per definition `maxStack`; container rejects overflow |
| **Weight / capacity** | Deferred — optional placeholder field on definition only |
| **Durability** | Deferred |
| **Rarity / value** | Deferred unless needed for economy prototype |

---

## C. Architecture direction

### Platform alignment

- Register through **`CCS_RuntimeHost`** / **`CCS_ServiceRegistry`** — no static inventory singleton.
- Implement as **`CCS_IModule`** + installer (`CCS_SurvivalInventoryModule` / `CCS_SurvivalInventoryModuleInstaller`) with explicit install order after survival core.
- Use **`CCS_EventDispatcher`** for **item added / removed / changed** events — UI and vitals subscribe later.
- Return **`CCS_Result`** for mutating operations; log via **`CCS_Logger`** with a dedicated category.

### Coupling rules

| Rule | Detail |
|------|--------|
| **Pickups stay dumb** | `CCS_SurvivalPickupInteractable` dispatches **`CCS_SurvivalPickupCollectedEvent`** only — no direct inventory calls |
| **Scanner stays generic** | Interaction layer does not reference inventory types |
| **Save-stable IDs** | `pickupId` / `itemId` use lowercase dotted strings; map table or shared id on definition |
| **Authoring** | **`ScriptableObject`** item definitions under `Assets/CCS/Survival/` content paths |
| **Runtime data** | Plain structs/classes for stacks and slots — no SO mutation at runtime |
| **No UI in core** | Inventory module has zero uGUI/UI Toolkit dependency |
| **No character lock-in** | Service API works with interactor id / authority id placeholder for future multiplayer |
| **Authority** | Mutations go through service on owning context; no hidden global player inventory |

### Suggested layout

```text
Assets/CCS/Survival/Runtime/Inventory/
  Definitions/       → CCS_SurvivalItemDefinition (ScriptableObject)
  Data/              → CCS_SurvivalItemStack, CCS_SurvivalInventorySlot
  Containers/        → CCS_SurvivalInventoryContainer
  Services/          → CCS_ISurvivalInventoryService
  Modules/           → CCS_SurvivalInventoryModule + Installer
  Events/            → CCS_SurvivalInventoryItemAddedEvent, etc.
  Integration/       → CCS_SurvivalPickupInventoryBridge (subscribes to pickup collected)
```

### Pickup event bridge (Phase 2C)

```text
CCS_SurvivalPickupInteractable.Interact
  → CCS_SurvivalPickupCollectedEvent (pickupId, displayName, amount)
  → CCS_SurvivalPickupInventoryBridge (module listener)
  → CCS_ISurvivalInventoryService.TryAddItem(itemId, amount)
  → CCS_SurvivalInventoryItemAddedEvent
```

Prototype pickup ids today (`survival.pickup.food_tin`, etc.) become or map to **`survival.item.*`** definition ids.

---

## D. Early inventory scope

### Recommended Phase 2B (first implementation)

| Deliverable | Detail |
|-------------|--------|
| Item definition asset | SO with id, display name, max stack, category/tags |
| Item stack + slot + fixed container | In-memory, deterministic |
| `CCS_ISurvivalInventoryService` | `TryAddItem`, `TryRemoveItem`, `HasItem`, `GetStackCount` |
| Module + installer | Manual registration in survival bootstrap install plan |
| Events | Item added / removed / changed |
| Debug logging | Categorized, not per-frame |
| Pickup bridge | Optional in 2B or **2C** — add item then hide pickup (existing hide behavior retained) |

### Deferred (post-2B)

| Feature | Notes |
|---------|--------|
| Full inventory UI | Grid, drag/drop |
| Equipment slots | Separate container type |
| Crafting | Recipe module consumes inventory service |
| Save / load | Persistence module later |
| Multiplayer replication | Authority + net layer later |
| Durability | Per-stack metadata |
| Weight limits | Unless simple max-slot placeholder suffices |
| Hotbar | Input/UI milestone |

---

## E. Integration plan

| Phase | Milestone | Version bump |
|-------|-----------|--------------|
| **2A** | Readiness + architecture (this doc) | Stay **0.6.1** |
| **2B** | Core inventory service + item definitions | → **0.7.0** when started |
| **2C** | Pickup-to-inventory integration | 0.7.x |
| **2D** | Inventory debug overlay / simple list | 0.7.x |
| **2E** | Basic consume-item hook (food/water → vitals API) | 0.7.x |
| **2F** | Standalone validation build | **0.7.0-A** smoke |

### Module ID

- **`ccs.survival.inventory`** — gameplay module in game repo only (not `ccs-framework`).

### Dependencies

| Depends on | Reason |
|------------|--------|
| `ccs.survival` core bootstrap | Runtime host, event dispatcher |
| Interaction events | Pickup collected handoff |
| (Future) vitals service | Consume hooks in 2E |

### Testing expectations (future)

- Play Mode: collect three prototype pickups → inventory counts increase → overlay/list shows stacks.
- Standalone: zero exceptions; pickup + inventory logs once per collect.
- Dev validation: unchanged — inventory tests do not require dev root enabled.

---

## References

| Document | Path |
|----------|------|
| Phase One complete | [CCS_Survival_Phase_01_Survival_Core.md](CCS_Survival_Phase_01_Survival_Core.md) |
| Module guidelines | [Future_Gameplay_Module_Guidelines.md](Future_Gameplay_Module_Guidelines.md) |
| Prototype roadmap | [CCS_Survival_Prototype_Roadmap.md](CCS_Survival_Prototype_Roadmap.md) |
| Pickup event | `Assets/CCS/Survival/Runtime/Interaction/CCS_SurvivalInteractionEvents.cs` |

---

## Phase 2B — Core Inventory Service + Item Definitions

### Purpose

Implement runtime inventory foundation: item definitions, stack/slot/container data, inventory service/module, events, prototype item assets, debug overlay readout, and bootstrap validation updates. **Pickup-to-inventory bridge deferred to Phase 2C.**

### Implemented (2B)

| Area | Deliverable |
|------|-------------|
| **Definitions** | `CCS_SurvivalItemDefinition` (ScriptableObject), `CCS_SurvivalItemCategory`, validation utility |
| **Data** | `CCS_SurvivalItemStack`, `CCS_SurvivalInventorySlot`, `CCS_SurvivalInventorySlotSnapshot` |
| **Container** | `CCS_SurvivalInventoryContainer` — merge stacks, fill slots, overflow `remainingAmount` |
| **Service** | `CCS_ISurvivalInventoryService` — add/remove/query, local events, dispatcher payloads |
| **Module** | `CCS_SurvivalInventoryModule` — module ID **`ccs.survival.inventory`**, default **16** slots |
| **Installer** | `CCS_SurvivalInventoryModuleInstaller` via `CCS_SurvivalInstaller` |
| **Events** | `CCS_SurvivalInventoryChangedEvent`, `CCS_SurvivalItemAddedEvent`, `CCS_SurvivalItemRemovedEvent` |
| **Assets** | `Assets/CCS/Survival/Data/Items/` — `ITM_FoodTin`, `ITM_WaterCanteen`, `ITM_Kindling` |
| **Overlay** | `Inventory occupied/total` + compact `Items` summary line on `CCS_SurvivalDebugOverlay` |
| **Diagnostics** | `ExpectedSkeletonModuleCount = 2`, `SkeletonExpectedServicesCount = 3`, inventory module/service checks |
| **Bootstrap self-test** | `CCS_SurvivalInventoryBootstrapSelfTest` when inventory debug logs enabled at install |

### Prototype item assets

| Asset | itemId | Category | maxStack |
|-------|--------|----------|---------:|
| `ITM_FoodTin` | `survival.item.food_tin` | Food | 12 |
| `ITM_WaterCanteen` | `survival.item.water_canteen` | Water | 6 |
| `ITM_Kindling` | `survival.item.kindling` | Material | 24 |

### Service registration

| Check | Result |
|-------|--------|
| Module installed via `CCS_SurvivalInstaller` | **Yes** — after character module |
| `CCS_ISurvivalInventoryService` on `CCS_ServiceRegistry` | **Yes** — registered in `OnInstall` |
| Inventory starts empty at bootstrap | **Yes** |
| Pickups still collect/hide (no inventory mutation yet) | **Yes** — Phase 2C |

### Validation checklist (2B)

| Check | Status |
|-------|--------|
| Unity compiles | **Pass** — batch compile (6000.3.10f1) |
| Core health OK | **Expected** — service count now **3** |
| Survival validation rules passed | **Expected** — inventory module installed |
| Overlay `Inventory 0/16` at start | **Expected** (Play Mode) |
| Overlay `Items None` at start | **Expected** (Play Mode) |
| Pickup UX unchanged | **Expected** (Play Mode) |
| `CCS_DevValidationRoot` inactive | **Unchanged** |

### Deferred (post-2B)

| Item | Phase |
|------|-------|
| Pickup → inventory (`CCS_SurvivalPickupCollectedEvent` bridge) | **2C** |
| Full inventory UI / drag-drop | Later |
| Consume hooks | **2E** |
| Save/load, crafting, equipment | Later |

### Final status (2B)

**0.7.0 inventory core foundation implemented.** Ready for **Phase 2C — Pickup-to-Inventory Integration**.

---

## Next step

**Phase 2C — Pickup-to-Inventory Integration** (map `survival.pickup.*` → `survival.item.*`, add on collect).
