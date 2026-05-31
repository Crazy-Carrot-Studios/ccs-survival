# CCS UI / HUD Module

**Module ID (planned):** `ccs.survival.ui`  
**Milestone:** 0.4.3 — HUD Runtime Wiring Pass  
**Author:** James Schilz  
**Date:** 2026-05-30

---

## Purpose

Provide a read-only HUD presentation layer that visualizes data from existing gameplay modules without owning gameplay logic.

The UI module binds to:

- Survival Core stat snapshots and events
- Interaction prompt state
- Inventory summary snapshots
- Equipment summary snapshots
- Local notification queue presentation

---

## Read-only architecture

| Layer | Responsibility |
|-------|----------------|
| **Gameplay modules** | Own data, services, events, and mutation |
| **CCS_HudPresentationService** | Cache snapshots and raise presentation events |
| **Presenters** | Render placeholder Unity UI from cached data |
| **CCS_HudProfile** | Toggle HUD areas, layout tuning, and notification settings |

Rules:

- No inventory add/remove from UI
- No stat damage/heal from UI
- No equip/unequip from UI
- Safe when gameplay services are missing

---

## Runtime wiring

Gameplay services register on `CCS_RuntimeHost.ServiceRegistry` through `CCS_SurvivalGameplayServiceRegistration` during `CCS_SurvivalBootstrap` startup.

`CCS_HudRootPresenter` resolves services in `Start()` via `CCS_HudGameplayServiceWiring` and binds them to `CCS_HudPresentationService`.

| Gameplay service | HUD data |
|------------------|----------|
| `CCS_SurvivalCoreService` | Health, stamina, hunger, thirst, fatigue, temperature snapshots |
| `CCS_InteractionService` | Plain-text interaction prompt |
| `CCS_PlayerInventoryService` | Used/total slot summary (includes equipment slot bonuses) |
| `CCS_PlayerEquipmentService` | Equipped count and bonus inventory slots |

`CCS_HudPresentationService` subscribes to gameplay events and raises read-only HUD notifications:

| Source event | Notification |
|--------------|--------------|
| Inventory item added | Item Added |
| Inventory item removed | Item Removed |
| Equipment item equipped | Item Equipped |
| Equipment item unequipped | Item Unequipped |
| Interaction failed | Interaction Failed |

`CCS_NotificationQueue` displays notifications from `CCS_HudPresentationService.NotificationQueued` using profile lifetime and max visible count.

---

## HUD anchoring rules

| Element | Anchor | Notes |
|---------|--------|-------|
| Survival bars | Lower-left | Clear of center gameplay view |
| Interaction prompt | Lower-center | Only center-ish element; kept low to avoid blocking aim/view |
| Inventory summary | Lower-right | Compact readable line, not a full menu |
| Equipment summary | Lower-right | Stacked near inventory summary |
| Notifications | Top-right | Vertical stack; profile-driven max count |

General rules:

- Avoid screen center except the low interaction prompt
- Use safe margins from profile layout settings
- Avoid tiny debug text
- Keep the gameplay scene visible

`CCS_HudLayoutApplicator` applies profile layout at runtime from `CCS_HudRootPresenter`.

---

## Readability standards

Default profile targets readable text at **1080p** and **1440p**:

- Survival bar width: 320–420 px (default 400)
- Survival bar row height: 28–40 px (default 34)
- Survival bar font size: 17
- Summary font size: 16
- Interaction prompt font size: 22
- Notification row height: 40
- Notification font size: 16
- Safe margin: 28 px

Missing gameplay data shows safe placeholders such as `Health: --`, `Inventory: --`, and `Equipment: --`. The interaction prompt stays hidden until a valid target prompt exists.

---

## Presenter responsibilities

| Presenter | Display |
|-----------|---------|
| `CCS_HudRootPresenter` | Owns presentation service, layout areas, and binds child presenters |
| `CCS_SurvivalBarPresenter` | Health, stamina, hunger, thirst bars |
| `CCS_InteractionPromptPresenter` | Plain-text interact prompt |
| `CCS_InventorySummaryPresenter` | Slot and quantity summary |
| `CCS_EquipmentSummaryPresenter` | Equipped slot summary |
| `CCS_NotificationQueue` | Transient notification stack |
| `CCS_NotificationPresenter` | Single notification line |

---

## HUD profile

Default asset:

`Assets/CCS/Survival/Profiles/UI/CCS_DefaultHudProfile.asset`

Settings:

- Visibility toggles for each HUD area
- Layout settings (`CCS_HudLayoutSettings`): scale, safe margin, bar dimensions, font sizes, prompt offset
- Notification settings (`CCS_NotificationProfile`): max visible count, lifetime, width, row height, font size

---

## Input glyphs (deferred)

Glyph assets are deferred until Input Actions / Input System integration.

Current HUD uses **plain text prompts only**, such as:

- `Interact`
- `Interact: {TargetName}`

Future glyph needs may include:

- Keyboard: E, I, Tab, Esc, Shift, Ctrl, R
- Mouse: LMB, RMB, MMB
- Gamepad: A/Cross, B/Circle, X/Square, Y/Triangle, LB/RB, LT/RT, Start/Menu, D-pad

---

## Prefab structure

`Assets/CCS/Modules/UI/Prefabs/PF_CCS_HUD_Root.prefab`

Contains:

- Canvas + Canvas Scaler + Graphic Raycaster
- Survival bar area (lower-left)
- Interaction prompt (lower-center)
- Inventory summary (lower-right)
- Equipment summary (lower-right)
- Notification queue (top-right stack)

---

## Bootstrap scene integration

`Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity` includes one instance of `PF_CCS_HUD_Root`.

HUD may exist without final player wiring. Presenters show placeholder text when services are not bound.

Batch setup entries:

- `CCS.Modules.UI.Editor.CCS_UIHudBootstrapSetup.ExecuteBatch`
- `CCS.Modules.UI.Editor.CCS_UIHudLayoutSetup.ExecuteBatch`

---

## Validation

Menu: **CCS → Survival → UI → Validate UI**

Batch entry:

`CCS.Modules.UI.Editor.CCS_UIValidationMenu.ValidateUI`

---

## Deferred features

- Full inventory menu
- Drag/drop
- Crafting UI
- Equipment paper-doll UI
- Final art and animation
- Input Actions wiring
- Input glyph assets
- Settings menu UI (player-facing preferences)
