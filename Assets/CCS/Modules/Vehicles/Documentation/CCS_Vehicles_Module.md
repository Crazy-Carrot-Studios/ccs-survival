# CCS Vehicles Module

Generic vehicle framework for hand carts, wagons, stagecoaches, and future mine carts.

## Milestone 3.5.0 — Route risk freight

Wagon cargo remains the delivery path for risk-adjusted freight rewards. Trade routes store `preferredWagonRequirementPlaceholder` (e.g. frontier wagon id) for future enforcement — not validated in 3.5.0.

## Milestone 3.4.0 — Freight cargo

`CCS_VehicleService.ActiveCargoInstanceId` resolves the summoned wagon storage instance for `CCS_ContractFreightUtility` (prefer wagon cargo; remove goods on freight delivery). No loading UI in this milestone.

## Milestone 1.7.0 — Mining haul

Dense ore/coal item weights encourage wagon cargo for bulk mining hauls (placeholder logistics; slot-based cargo unchanged).

## Milestone 1.5.2 — Wagon Foundation

- `CCS_VehicleDefinition` / `CCS_VehicleProfile` — vehicle stats and catalog
- `CCS_VehicleService` — ownership, summon, park, hitch/unhitch, follow tick
- `CCS_VehicleState` — Idle, Hitched, Moving, Parked, Stored
- `CCS_WagonCargoContainer` — wagon cargo via `CCS_StorageService` (24 slots, instance id)
- Frontier Stable vendor sells **Frontier Wagon Deed** (General Store does not sell wagons)
- Save section `CCS_SaveVehiclesWorldData`
- Horse `WagonHitchPoint` transform on `PF_CCS_Horse`

## Hitching

Simple transform-follow behind the horse hitch point. No wheel physics or towing simulation.

## Bootstrap

```text
CCS.Modules.Vehicles.Editor.CCS_WagonFoundationBootstrapSetup.ExecuteBatch
```

## Frontier Logistics Loop

```text
Earn Wealth
  ↓
Buy Horse
  ↓
Buy Wagon
  ↓
Carry More
  ↓
Move Supplies
  ↓
Expand Homestead Reach
```
