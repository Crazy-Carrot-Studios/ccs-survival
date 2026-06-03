# CCS Banking Module

Milestone **2.4.0** — generic stored-currency foundation for frontier finance services.

## Purpose

Provides bank account open, deposit, withdraw, balance queries, transaction history placeholders, save/load, and land office debug integration. Western-specific bank and land office naming lives in Survival content assets under `Assets/CCS/Survival/Content/Banking/`.

## Frontier Finance Loop

Earn Trade Dollars  
↓  
Deposit Savings  
↓  
Claim Land  
↓  
Register Frontier Presence  
↓  
Prepare For Taxes / Loans / Expansion  

## Runtime Types

| Type | Role |
|------|------|
| `CCS_BankAccountDefinition` | ScriptableObject account template (currency id, display name) |
| `CCS_BankAccountProfile` | Profile catalog registered on bootstrap host |
| `CCS_BankAccountState` | Account lifecycle enum (Closed, Open, Suspended) |
| `CCS_BankTransaction` | Transaction history placeholder record |
| `CCS_BankTransactionResult` | Operation result for open/deposit/withdraw |
| `CCS_BankingService` | Account ownership, wallet sync, save/restore |
| `CCS_BankingRuntimeBridge` | Service resolution for routes and debug HUD |
| `CCS_BankingValidationUtility` | Shared profile/content validation |
| `CCS_BankingDebugHud` | Temporary dev panel (wallet, bank balance, land office summary) |

## Rules (2.4.0)

- Cannot withdraw more than bank balance.
- Cannot deposit more than wallet balance.
- Deposits remove currency from `CCS_CurrencyService` before increasing bank balance.
- Withdrawals restore wallet through `CCS_CurrencyService` with rollback on failure.
- No loans, taxes, interest, debt, or final bank UI in this milestone.

## Settlement Integration

- `CCS_SettlementServicePointType.Bank` routes to `CCS_BankingDebugHud` (bank mode).
- `CCS_SettlementServicePointType.LandOffice` routes to the same HUD (land office mode + claim summary).
- Bootstrap trading post includes Bank and Land Office service cubes.

## Save / Load

`CCS_SaveBankingWorldData` persists account id, owner id, currency id, balance, account state, and transaction summary placeholder.

## Bootstrap

Run **`CCS_BankingFoundationBootstrapSetup.ExecuteBatch`** to create account content, profile assignment, and playtest steps.

Also run **`CCS_FrontierSettlementBootstrapSetup.ExecuteBatch`** to ensure bank/land office service cubes exist in the bootstrap scene.

## Playtest

Banking playtest group (`CCS_PlaytestStepGroup.Banking`) covers bank/land office interaction, deposit/withdraw balance checks, land office claim visibility, save, and load verification.

Shortcut: **Ctrl+Shift+B** (bank funds, open account, sample deposit).

## Deferred

- Loans and mortgages
- Tax ledger and fees
- Final bank UI and deed registry UI
- Multiplayer authority
