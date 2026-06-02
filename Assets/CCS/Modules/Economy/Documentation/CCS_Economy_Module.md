# CCS Economy Module

**Author:** James Schilz  
**Date:** 2026-06-01  
**Latest:** 2.1.0 — General Store and Stable sell livestock, feed, and ranch structure kits; General Store buys eggs and milk for world simulation Food supply.

**1.7.0** — General Store and Gunsmith buy frontier mining goods (ore, coal, clay, scrap, nails)

**1.6.0** — Frontier Gunsmith sells firearms and ammo (Stable and General Store do not sell firearms)

## Purpose

Generic economy framework for any currency type and vendor buy/sell loop. Western frontier content (Trade Dollars, General Store) lives in Survival profiles under `Assets/CCS/Survival/Profiles/Economy/` and `Assets/CCS/Survival/Content/Vendors/`.

No final vendor UI, NPC AI, reputation, or dynamic pricing in this milestone. **Settlement service routing (1.8.0+):** `CCS_SettlementServicePoint` activates existing vendor definitions — economy logic stays in `CCS_VendorService`. **Blacksmith (1.8.1):** routes to industry summary panel, not a duplicate vendor.

## Frontier progression philosophy

```text
Traveler
  ↓
Gather
  ↓
Fish
  ↓
Trade
  ↓
Acquire Better Tools
  ↓
Build Prosperity
```

## Runtime services

| Service | Role |
|---------|------|
| `CCS_CurrencyService` | Wallet balances, add/remove/afford, optional inventory backing sync |
| `CCS_VendorService` | Buy/sell transactions against vendor catalogs |
| `CCS_EconomyRuntimeBridge` | Resolves services from `CCS_RuntimeHost` registry |

## Currency

`CCS_CurrencyDefinition` supports any future currency (dollars, gold, credits, tokens). Default frontier currency:

| ID | Display | Backing item |
|----|---------|--------------|
| `ccs.survival.currency.tradedollars` | Trade Dollars | `ccs.survival.item.starter.dollars` |

Wallet balances persist in unified save (`CCS_SaveData.economy`). Inventory backing keeps coin stacks aligned after transactions.

## Vendors

`CCS_VendorDefinition` + `CCS_VendorItemEntry` provide generic buy/sell. `CCS_VendorInteractable` uses the interaction module and opens `CCS_VendorDebugHud` (temporary panel).

Bootstrap test object: `CCS_TestGeneralStore` in `SCN_CCS_Survival_Bootstrap`.

### Mining trade (1.7.0)

General Store buys iron ore, coal, clay, scrap iron, nails, stone, and flint. Gunsmith buys refined iron and raw ore/coal for metalworking surplus.

```text
Industry → Mine → Haul → Refine → Sell
```

### Frontier Gunsmith (1.6.0)

| ID | Role |
|----|------|
| `ccs.survival.vendor.frontier.gunsmith` | Frontier Gunsmith vendor |
| `ccs.survival.item.firearm.revolver.frontier` | Frontier Revolver |
| `ccs.survival.item.firearm.rifle.frontier` | Frontier Rifle |
| `ccs.survival.item.firearm.shotgun.frontier` | Frontier Shotgun |
| `ccs.survival.item.ammo.*` | Cartridges and shells |

Bootstrap test object: `CCS_TestFrontierGunsmith` in `SCN_CCS_Survival_Bootstrap`. Firearms are **not** sold at the Stable or General Store.

```text
Industry → Ammunition → Purchase Firearm → Hunt → Harvest → Trade
```

### Frontier Stable (1.5.2)

| ID | Role |
|----|------|
| `ccs.survival.vendor.frontier.stable` | Frontier Stable vendor |
| `ccs.survival.item.mount.frontierhorse` | Horse deed (buy **2500** Trade Dollars) |
| `ccs.survival.item.vehicle.frontierwagon` | Wagon deed (buy **1800** Trade Dollars) |

Bootstrap test object: `CCS_TestFrontierStable` in `SCN_CCS_Survival_Bootstrap`. Horses and wagons are **not** listed on the General Store.

```text
Earn Wealth → Buy Horse → Buy Wagon → Carry More → Move Supplies → Expand Homestead Reach
```

### General Store catalog (1.3.1)

**Sells (early progression):** bone hatchet, fishing pole, cordage, crude hook, fishing line, hardtack, tinderbox, arrows, simple trap (optional).

**Buys from player:** raw/small fish, hide, raw meat, bone, scrap iron, feathers, animal fat, junk.

Hunting and fishing both produce trade goods for the same General Store loop.

**Intentional progression:** the bone hatchet (`ccs.survival.item.tool.hatchet.bone`, buy **18** Trade Dollars placeholder) is **not** in the knife-only starter loadout. Players earn currency (fish/salvage), sell goods, then purchase the hatchet as the first meaningful tool upgrade.

Fish remain **sell-only** at the store (purchase disabled).

## Item values

Optional fields on `CCS_ItemDefinition`:

- `hasEconomyValues`
- `buyValue` / `sellValue`
- `vendorCategory` (`CCS_ItemVendorCategory`)

Legacy items without economy fields continue to work (zero prices unless vendor overrides).

## Playtest

After fishing: obtain fish → interact store → sell fish → verify currency → **H** buy hatchet → equip (Shift+F6) → use hatchet on `CCS_TestGatheringSmallTree` for wood.

Hotkeys (playtest HUD): **Shift+V** sell fish or hide (step-dependent), **V** buy cordage, **H** buy hatchet, **Ctrl+V** grant test fish, **Ctrl+B** grant test bow.

Hunting playtest: bow → kill rabbit → knife harvest → sell hide → verify Trade Dollars.

## Debug vendor UI (temporary)

`CCS_VendorDebugHud` shows vendor name, Trade Dollars balance, last transaction (item id, qty, currency delta, result), and buttons: Sell Raw Fish, Buy Cordage, Buy Bone Hatchet, Close. **Esc** closes. Not final shop UI.

## Bootstrap batch

```text
CCS.Modules.Economy.Editor.CCS_EconomyBootstrapSetup.ExecuteBatch
```
