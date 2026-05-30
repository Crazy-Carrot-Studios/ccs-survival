# CCS UI / HUD Module

**Module ID (planned):** `ccs.survival.ui`  
**Milestone:** 0.4.2 — UI/HUD Foundation  
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
| **CCS_HudProfile** | Toggle HUD areas and notification tuning |

Rules:

- No inventory add/remove from UI
- No stat damage/heal from UI
- No equip/unequip from UI
- Safe when gameplay services are missing

---

## Presenter responsibilities

| Presenter | Display |
|-----------|---------|
| `CCS_HudRootPresenter` | Owns presentation service and binds child presenters |
| `CCS_SurvivalBarPresenter` | Health, stamina, hunger, thirst bars |
| `CCS_InteractionPromptPresenter` | Current interactable prompt text |
| `CCS_InventorySummaryPresenter` | Slot and quantity summary |
| `CCS_EquipmentSummaryPresenter` | Equipped slot summary |
| `CCS_NotificationQueue` | Transient notification stack |
| `CCS_NotificationPresenter` | Single notification line |

---

## HUD profile

Default asset:

`Assets/CCS/Survival/Profiles/UI/CCS_DefaultHudProfile.asset`

Settings:

- Show survival bars
- Show interaction prompt
- Show inventory summary
- Show equipment summary
- Show notifications
- Notification max visible count
- Notification lifetime seconds

---

## Prefab structure

`Assets/CCS/Modules/UI/Prefabs/PF_CCS_HUD_Root.prefab`

Contains:

- Canvas + Canvas Scaler + Graphic Raycaster
- Survival bar area (lower-left)
- Interaction prompt (lower-center)
- Inventory summary (upper-right)
- Equipment summary (upper-right)
- Notification queue (upper-right stack)

---

## Bootstrap scene integration

`Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity` includes one instance of `PF_CCS_HUD_Root`.

HUD may exist without final player wiring. Presenters show placeholder/unavailable text when services are not bound.

Batch setup entry:

`CCS.Modules.UI.Editor.CCS_UIHudBootstrapSetup.ExecuteBatch`

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
- Settings menu UI (player-facing preferences)
