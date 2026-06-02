# CCS Economy Module (1.3.0 Foundation)

**Author:** James Schilz  
**Date:** 2026-06-01  
**Milestone:** 1.3.0 — Frontier Economy Foundation

## Purpose

Generic economy framework for any currency type and vendor buy/sell loop. Western frontier content (Trade Dollars, General Store) lives in Survival profiles under `Assets/CCS/Survival/Profiles/Economy/` and `Assets/CCS/Survival/Content/Vendors/`.

No final vendor UI, NPC AI, settlements, reputation, or dynamic pricing in this milestone.

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

## Item values

Optional fields on `CCS_ItemDefinition`:

- `hasEconomyValues`
- `buyValue` / `sellValue`
- `vendorCategory` (`CCS_ItemVendorCategory`)

Legacy items without economy fields continue to work (zero prices unless vendor overrides).

## Playtest

After fishing steps: obtain fish → interact store → Shift+V sell → verify currency → V buy cordage → verify inventory.

Hotkeys: **V** buy cordage, **Shift+V** sell raw fish, **Ctrl+V** grant test fish.

## Bootstrap batch

```text
CCS.Modules.Economy.Editor.CCS_EconomyBootstrapSetup.ExecuteBatch
```
