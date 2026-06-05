# CCS Businesses Module

**Milestone 4.1.0** — NPC identity role mappings use business ids (e.g. General Store → Merchant, Stable → Stable Hand).

**Milestone 4.0.0** — Population placeholder actors may anchor near business zones (visual only; business activation unchanged).

**Milestone 3.7.0 — Frontier Businesses Foundation**

Simulation-level business activation connects population, prosperity, settlement growth, and optional reputation to frontier settlement services.

## Loop

```text
Population
    ↓
Businesses Open
    ↓
Services Expand
    ↓
Prosperity Improves
    ↓
Settlement Grows
```

## Core types

| Type | Role |
|------|------|
| `CCS_BusinessType` | Business archetype enum |
| `CCS_BusinessDefinition` | Per-type activation thresholds |
| `CCS_BusinessProfile` | Definitions + per-settlement catalogs |
| `CCS_BusinessState` | Persisted active flag per business |
| `CCS_BusinessInstance` | Runtime query view |
| `CCS_BusinessSnapshot` | Active / inactive / available lists |
| `CCS_BusinessService` | Profile host, snapshots, activation events |
| `CCS_BusinessRuntimeBridge` | Service point activation gating |
| `CCS_BusinessValidationUtility` | Init, evaluation, validation (world simulation) |

## Settlement integration

Each settlement simulation state stores `businessStates[]` with activation flags. Catalog entries define which businesses can exist at a settlement.

**Trading Post:** General Store, Stable, Gunsmith, Bank, Contract Office  
**Broken Creek:** Farm Supply  
**Iron Ridge:** Mining Supplier  
**Pine Ridge:** Lumber Yard

## Activation rules

- Minimum population  
- Minimum prosperity  
- Minimum growth stage  
- Optional minimum reputation tier  

`CCS_WorldSimulationService` re-evaluates businesses when population, prosperity, or growth stage changes. `CCS_BusinessService` raises `BusinessActivated` and `BusinessDeactivated`.

## Save / load

Business flags persist on `CCS_SettlementSimulationState` inside world simulation save payloads.

## Playtest

- Group: **Businesses**  
- Shortcut: **Ctrl+Shift+J**  
- Bootstrap steps: `ccs.survival.playtest.business.*`

## Bootstrap

Run `CCS_FrontierBusinessesFoundationBootstrapSetup.ExecuteBatch` to create `CCS_DefaultBusinessProfile.asset` and wire the world simulation profile.

## Visible presence (3.8.0)

| Type | Role |
|------|------|
| `CCS_BusinessPresenceProfile` | Anchor catalog for validation and bootstrap |
| `CCS_BusinessPresenceAnchor` | World anchor linking settlement + business to marker/label |
| `CCS_BusinessPresenceMarker` | Primitive active/inactive/locked visual |
| `CCS_BusinessPresenceLabel` | TextMesh name + status |
| `CCS_BusinessPresenceService` | Refreshes markers from business snapshots |
| `CCS_BusinessPresenceValidationUtility` | Status resolution and profile validation |

Run `CCS_BusinessPresenceFoundationBootstrapSetup.ExecuteBatch` for scene anchors and playtest steps. Shortcut: **Ctrl+Shift+V**.

Business presence markers are independent from settlement visual growth zone markers (3.9.0) but may share settlement root transforms in the bootstrap scene.

**Business Presence Loop:**

```text
Population Grows → Business Activates → World Marker Updates → Settlement Feels More Developed
```
