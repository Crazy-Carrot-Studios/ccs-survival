# CCS Banking Module

Milestone **2.6.0** — generic stored-currency foundation plus simple frontier loans.

## Purpose

Provides bank account open, deposit, withdraw, balance queries, loan borrow/repay, transaction history placeholders, save/load, and land office debug integration. Western-specific bank, land office, and loan naming lives in Survival content assets under `Assets/CCS/Survival/Content/Banking/`.

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

## Frontier Loan Loop (2.6.0)

Need Capital  
↓  
Borrow From Bank  
↓  
Buy Supplies / Expand Claim  
↓  
Earn Income  
↓  
Repay Loan  

## Runtime Types

| Type | Role |
|------|------|
| `CCS_BankAccountDefinition` | ScriptableObject account template (currency id, display name) |
| `CCS_BankAccountProfile` | Profile catalog registered on bootstrap host; references `CCS_LoanProfile` |
| `CCS_LoanDefinition` | ScriptableObject loan product (principal, repayment, max active loans) |
| `CCS_LoanProfile` | Loan catalog referenced from bank account profile |
| `CCS_LoanState` | Loan lifecycle enum (None, Active, Due, Paid, Defaulted placeholder, Disabled) |
| `CCS_LoanSnapshot` | Serializable loan state for save/load |
| `CCS_LoanTransaction` | Loan transaction history placeholder record |
| `CCS_LoanTransactionResult` | Operation result for borrow/repay |
| `CCS_BankAccountState` | Account lifecycle enum (Closed, Open, Suspended) |
| `CCS_BankTransaction` | Transaction history placeholder record |
| `CCS_BankTransactionResult` | Operation result for open/deposit/withdraw |
| `CCS_BankingService` | Account ownership, wallet sync, loans, save/restore |
| `CCS_BankingRuntimeBridge` | Service resolution for routes and debug HUD |
| `CCS_BankingValidationUtility` | Shared profile/content validation |
| `CCS_BankingDebugHud` | Temporary dev panel (wallet, bank balance, loan summary, land office summary) |

## Rules (2.6.0)

- Cannot withdraw more than bank balance.
- Cannot deposit more than wallet balance.
- Deposits remove currency from `CCS_CurrencyService` before increasing bank balance.
- Withdrawals restore wallet through `CCS_CurrencyService` with rollback on failure.
- **2.5.0:** `TryDebitForUpkeep` / `CanDebitForUpkeep` debit bank for upkeep only (no wallet credit).
- **2.6.0 loans:** `TryOpenLoan` credits wallet principal; rolls back if wallet add fails.
- Cannot borrow when max active loans reached, loan disabled, or amounts invalid.
- `TryRepayLoan` debits bank first (when enabled), then wallet; never creates negative balances.
- No compound interest, foreclosure, credit score, final bank UI, or NPC bankers yet.
- `Defaulted` exists as a placeholder state without punishment in 2.6.0.

## Settlement Integration

- `CCS_SettlementServicePointType.Bank` routes to `CCS_BankingDebugHud` (bank mode).
- `CCS_SettlementServicePointType.LandOffice` routes to the same HUD (land office mode + claim summary).
- `CCS_SettlementServicePoint.OffersLoanServices` metadata is true for Bank service points.
- Bootstrap trading post includes Bank and Land Office service cubes.

## Save / Load

`CCS_SaveBankingWorldData` persists bank accounts and loan snapshots (loan id, owner id, principal, repayment amount, balance, state, transaction summary placeholder).

## Bootstrap

Run **`CCS_BankingFoundationBootstrapSetup.ExecuteBatch`** for account content and banking playtest steps.

Run **`CCS_BankingLoansFoundationBootstrapSetup.ExecuteBatch`** for Frontier Small Loan content, loan profile wiring, and loan playtest steps.

Also run **`CCS_FrontierSettlementBootstrapSetup.ExecuteBatch`** to ensure bank/land office service cubes exist in the bootstrap scene.

## Playtest

Banking playtest group (`CCS_PlaytestStepGroup.Banking`) covers bank/land office interaction, deposit/withdraw balance checks, loan borrow/repay, land office claim visibility, save, and load verification.

Shortcuts:

- **Ctrl+Shift+B** — bank funds, open account, sample deposit
- **Ctrl+Shift+O** — loan borrow, partial deposit, repay sample

Bank HUD dev hotkeys: **Shift+L** borrow, **Shift+P** repay (via `CCS_DevHotkeyUtility`).

## Reputation hooks (2.7.0)

When enabled on the reputation profile, successful **loan repay** events (`LoanTransactionCompleted` with repaid message) apply conservative `LoanRepaid` settlement trust via `CCS_ReputationService.TryApplyLoanRepaid`. No credit score or loan restrictions in 2.7.0.

## Deferred

- Compound interest and scheduled due enforcement
- Foreclosure and collateral enforcement
- Credit score / reputation tiers affecting loan terms
- Final bank UI and deed registry UI
- Multiplayer authority
