# CCS Firearms Module

Generic firearm framework for revolvers, rifles, shotguns, and future repeaters.

## Milestone 1.6.0 — Firearm Foundation

- `CCS_FirearmDefinition` / `CCS_FirearmProfile` — weapon stats and catalog
- `CCS_AmmoDefinition` — ammunition linked to inventory items
- `CCS_FirearmService` — loaded rounds, reload, fire routing, save snapshots
- `CCS_ActiveItemBehaviorType.Firearm` — primary use consumes ammo and raycast hunts wildlife
- Frontier **Gunsmith** vendor (stable and general store do not sell firearms)
- **1.8.0:** Gunsmith service point at `CCS_TestTradingPost` routes to `CCS_Vendor_FrontierGunsmith` via Settlements module
- Primitive forge crafts ammunition from refined iron and charcoal
- Save section `CCS_SaveFirearmsWorldData`

## Bootstrap

```text
CCS.Modules.Firearms.Editor.CCS_FirearmFoundationBootstrapSetup.ExecuteBatch
```

## Frontier Firearm Loop

```text
Industry
  ↓
Ammunition
  ↓
Purchase Firearm
  ↓
Hunt
  ↓
Harvest
  ↓
Trade
```

## Controls

- **Primary action** — fire equipped firearm (when active item is firearm)
- **R** — reload equipped firearm

## Playtest

**Ctrl+Shift+G** — grant currency, buy revolver and ammo, equip, reload
