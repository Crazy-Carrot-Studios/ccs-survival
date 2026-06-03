# CCS Upkeep Module

Milestone **2.5.0** — generic recurring-cost foundation for land taxes, property upkeep, stable fees, storage fees, licenses, and future rentals.

## Purpose

Tracks upkeep entries per owned target (starting with land claims), supports manual due forcing, pays from bank then wallet without overdraft, persists through save/load, and surfaces status on the Land Office debug HUD. No debt, loans, foreclosure, faction law, or final UI in 2.5.0.

## Frontier Upkeep Loop

Claim Land  
↓  
Earn Money  
↓  
Deposit Savings  
↓  
Pay Claim Tax / Upkeep  
↓  
Maintain Legal Frontier Presence  

## Runtime Types

| Type | Role |
|------|------|
| `CCS_UpkeepDefinition` | ScriptableObject recurring cost (currency, amount, interval/grace placeholders, auto-pay flags) |
| `CCS_UpkeepProfile` | Profile catalog registered on bootstrap host |
| `CCS_UpkeepEntry` | Serializable entry (owner, target, amount due, days, status, summary) |
| `CCS_UpkeepState` | Current, Due, Paid, Failed, Disabled |
| `CCS_UpkeepTransaction` / `CCS_UpkeepTransactionResult` | History and operation results with `CCS_UpkeepPaymentSource` (None, Bank, Wallet) |
| `CCS_UpkeepService` | Registration, due forcing, payment, save/restore, land claim reconciliation |
| `CCS_UpkeepRuntimeBridge` | Service resolution for HUD and playtest |
| `CCS_UpkeepValidationUtility` | Shared profile/content validation |

## First content asset

**Frontier Homestead Claim Tax** (`ccs.survival.upkeep.land.frontierhomesteadtax`) — applies to land claim instances. Default amount 25 Trade Dollars; interval/grace are day placeholders only (manual due in 2.5.0).

## Payment order

1. Bank account when `autoPayFromBank` and `CCS_BankingService.CanDebitForUpkeep` / `TryDebitForUpkeep`
2. Wallet when `autoPayFromWallet` and sufficient `CCS_CurrencyService` balance
3. Fail safely — no overdraft, no negative balances, failed payment does not remove land claims

## Integration

- **Land:** `LandClaimPlaced` registers upkeep; load reconciles missing entries via `ReconcileLandClaimEntries`
- **Banking:** upkeep debits bank only (does not credit wallet)
- **Save:** `CCS_SaveUpkeepWorldData.entries` captured in `CCS_SaveService`
- **Land Office HUD:** owned claim count, nearby claim id, upkeep status, amount due, Shift+T pay hotkey
- **Playtest:** steps 169–177; Ctrl+Shift+U foundation shortcut

## Editor

- `CCS_UpkeepFoundationBootstrapSetup.ExecuteBatch` — content, profile, host wiring, playtest steps, version bump
- `CCS_UpkeepFoundationValidationValidator` — module, dependency, save, HUD, playtest validation
