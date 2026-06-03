using System;
using System.Collections.Generic;
using CCS.Modules.Economy;
using CCS.Modules.Land;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BankingService
// CATEGORY: Modules / Banking / Runtime / Services
// PURPOSE: Owns stored-currency bank accounts, deposits, withdrawals, and save/restore.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.6.0 — stored currency, upkeep debits, and simple loans.
// =============================================================================

namespace CCS.Modules.Banking
{
    public sealed class CCS_BankingService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_BankingService]";
        private const int MaxTransactionHistoryEntries = 32;

        private sealed class BankAccountInstance
        {
            public string AccountId = string.Empty;
            public string OwnerId = string.Empty;
            public string AccountDefinitionId = string.Empty;
            public string CurrencyId = string.Empty;
            public int Balance;
            public CCS_BankAccountState State = CCS_BankAccountState.Closed;
        }

        private sealed class LoanInstance
        {
            public string LoanId = string.Empty;
            public string OwnerId = string.Empty;
            public string LoanDefinitionId = string.Empty;
            public string CurrencyId = string.Empty;
            public int PrincipalAmount;
            public int RepaymentAmount;
            public int Balance;
            public CCS_LoanState State = CCS_LoanState.None;
        }

        private readonly Dictionary<string, BankAccountInstance> accountsById =
            new Dictionary<string, BankAccountInstance>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, CCS_BankAccountDefinition> accountDefinitionLookup =
            new Dictionary<string, CCS_BankAccountDefinition>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, CCS_LoanDefinition> loanDefinitionLookup =
            new Dictionary<string, CCS_LoanDefinition>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, LoanInstance> loansById =
            new Dictionary<string, LoanInstance>(StringComparer.OrdinalIgnoreCase);

        private readonly List<CCS_LoanTransaction> loanTransactionHistory = new List<CCS_LoanTransaction>();

        private readonly List<CCS_BankTransaction> transactionHistory = new List<CCS_BankTransaction>();

        private CCS_BankAccountProfile activeProfile;
        private CCS_LoanProfile activeLoanProfile;
        private CCS_CurrencyService currencyService;
        private CCS_LandClaimService landClaimService;
        private bool isInitialized;

        public event Action<CCS_BankTransactionResult> BankTransactionCompleted;

        public event Action<CCS_LoanTransactionResult> LoanTransactionCompleted;

        public bool IsInitialized => isInitialized;

        public CCS_BankAccountProfile ActiveProfile => activeProfile;

        public CCS_LoanProfile ActiveLoanProfile => activeLoanProfile;

        public IReadOnlyList<CCS_BankTransaction> TransactionHistory => transactionHistory;

        public IReadOnlyList<CCS_LoanTransaction> LoanTransactionHistory => loanTransactionHistory;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_BankAccountProfile profile)
        {
            activeProfile = profile;
            activeLoanProfile = profile?.LoanProfile;
            accountDefinitionLookup.Clear();
            loanDefinitionLookup.Clear();

            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_BankingValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            if (activeLoanProfile != null)
            {
                CCS_SurvivalValidationResult loanValidation =
                    CCS_BankingValidationUtility.ValidateLoanProfile(activeLoanProfile);
                if (!loanValidation.IsSuccess)
                {
                    Debug.LogWarning($"{LogPrefix} Loan profile validation warning: {loanValidation.Message}");
                }
            }

            CCS_BankAccountDefinition[] definitions = profile.AccountDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_BankAccountDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.AccountDefinitionId))
                {
                    continue;
                }

                accountDefinitionLookup[definition.AccountDefinitionId] = definition;
            }

            if (activeLoanProfile != null)
            {
                CCS_LoanDefinition[] loanDefinitions = activeLoanProfile.LoanDefinitions;
                for (int index = 0; index < loanDefinitions.Length; index++)
                {
                    CCS_LoanDefinition loanDefinition = loanDefinitions[index];
                    if (loanDefinition == null || string.IsNullOrWhiteSpace(loanDefinition.LoanDefinitionId))
                    {
                        continue;
                    }

                    loanDefinitionLookup[loanDefinition.LoanDefinitionId] = loanDefinition;
                }
            }

            isInitialized = validation.IsSuccess || accountDefinitionLookup.Count > 0;
        }

        public void BindCurrencyService(CCS_CurrencyService currency)
        {
            currencyService = currency;
        }

        public void BindLandClaimService(CCS_LandClaimService landClaim)
        {
            landClaimService = landClaim;
        }

        public CCS_BankTransactionResult TryOpenAccount(string ownerId, string accountDefinitionId)
        {
            if (!isInitialized)
            {
                return Failure(
                    CCS_BankTransactionResultType.ServiceNotReady,
                    string.Empty,
                    ownerId,
                    string.Empty,
                    "Banking service is not initialized.");
            }

            if (!TryResolveAccountDefinition(accountDefinitionId, out CCS_BankAccountDefinition definition))
            {
                return Failure(
                    CCS_BankTransactionResultType.InvalidAccount,
                    string.Empty,
                    ownerId,
                    string.Empty,
                    "Bank account definition was not found.");
            }

            string resolvedOwnerId = ResolveOwnerId(ownerId);
            string accountId = BuildAccountId(resolvedOwnerId, definition.AccountDefinitionId);
            if (accountsById.TryGetValue(accountId, out BankAccountInstance existing)
                && existing.State == CCS_BankAccountState.Open)
            {
                int walletBalance = GetWalletBalance(definition.CurrencyId);
                return CCS_BankTransactionResult.Success(
                    accountId,
                    resolvedOwnerId,
                    definition.CurrencyId,
                    0,
                    walletBalance,
                    existing.Balance,
                    "Bank account is already open.");
            }

            BankAccountInstance account = existing ?? new BankAccountInstance();
            account.AccountId = accountId;
            account.OwnerId = resolvedOwnerId;
            account.AccountDefinitionId = definition.AccountDefinitionId;
            account.CurrencyId = definition.CurrencyId;
            account.Balance = existing?.Balance ?? 0;
            account.State = CCS_BankAccountState.Open;
            accountsById[accountId] = account;

            RecordTransaction(
                account,
                0,
                "Open",
                "Bank account opened",
                "Open account placeholder summary");

            CCS_BankTransactionResult result = CCS_BankTransactionResult.Success(
                accountId,
                resolvedOwnerId,
                definition.CurrencyId,
                0,
                GetWalletBalance(definition.CurrencyId),
                account.Balance,
                $"Opened bank account '{definition.DisplayName}'.");
            NotifyTransactionCompleted(result);
            return result;
        }

        public CCS_BankTransactionResult TryDeposit(string ownerId, string accountDefinitionId, int amount)
        {
            CCS_BankTransactionResult openResult = EnsureOpenAccount(ownerId, accountDefinitionId, out BankAccountInstance account);
            if (!openResult.IsSuccess)
            {
                return openResult;
            }

            if (amount <= 0)
            {
                return Failure(
                    CCS_BankTransactionResultType.InvalidAmount,
                    account.AccountId,
                    account.OwnerId,
                    account.CurrencyId,
                    "Deposit amount must be greater than zero.");
            }

            if (currencyService == null || !currencyService.IsInitialized)
            {
                return Failure(
                    CCS_BankTransactionResultType.ServiceNotReady,
                    account.AccountId,
                    account.OwnerId,
                    account.CurrencyId,
                    "Currency service is not ready.");
            }

            if (!currencyService.CanAfford(account.CurrencyId, amount))
            {
                return Failure(
                    CCS_BankTransactionResultType.InsufficientWalletFunds,
                    account.AccountId,
                    account.OwnerId,
                    account.CurrencyId,
                    "Wallet balance is too low for this deposit.");
            }

            CCS_CurrencyTransactionResult removeResult = currencyService.RemoveCurrency(
                account.CurrencyId,
                amount,
                "Bank deposit");
            if (!removeResult.IsSuccess)
            {
                return Failure(
                    CCS_BankTransactionResultType.UnknownFailure,
                    account.AccountId,
                    account.OwnerId,
                    account.CurrencyId,
                    removeResult.Message);
            }

            account.Balance += amount;
            RecordTransaction(
                account,
                amount,
                "Deposit",
                removeResult.Message,
                $"Deposited {amount} {account.CurrencyId}");

            CCS_BankTransactionResult result = CCS_BankTransactionResult.Success(
                account.AccountId,
                account.OwnerId,
                account.CurrencyId,
                amount,
                removeResult.BalanceAfter,
                account.Balance,
                $"Deposited {amount} to bank account.");
            NotifyTransactionCompleted(result);
            return result;
        }

        public CCS_BankTransactionResult TryWithdraw(string ownerId, string accountDefinitionId, int amount)
        {
            CCS_BankTransactionResult openResult = EnsureOpenAccount(ownerId, accountDefinitionId, out BankAccountInstance account);
            if (!openResult.IsSuccess)
            {
                return openResult;
            }

            if (amount <= 0)
            {
                return Failure(
                    CCS_BankTransactionResultType.InvalidAmount,
                    account.AccountId,
                    account.OwnerId,
                    account.CurrencyId,
                    "Withdraw amount must be greater than zero.");
            }

            if (account.Balance < amount)
            {
                return Failure(
                    CCS_BankTransactionResultType.InsufficientBankFunds,
                    account.AccountId,
                    account.OwnerId,
                    account.CurrencyId,
                    "Bank balance is too low for this withdrawal.");
            }

            if (currencyService == null || !currencyService.IsInitialized)
            {
                return Failure(
                    CCS_BankTransactionResultType.ServiceNotReady,
                    account.AccountId,
                    account.OwnerId,
                    account.CurrencyId,
                    "Currency service is not ready.");
            }

            int previousBankBalance = account.Balance;
            account.Balance -= amount;

            CCS_CurrencyTransactionResult addResult = currencyService.AddCurrency(
                account.CurrencyId,
                amount,
                "Bank withdrawal");
            if (!addResult.IsSuccess)
            {
                account.Balance = previousBankBalance;
                return Failure(
                    CCS_BankTransactionResultType.UnknownFailure,
                    account.AccountId,
                    account.OwnerId,
                    account.CurrencyId,
                    addResult.Message);
            }

            RecordTransaction(
                account,
                -amount,
                "Withdraw",
                addResult.Message,
                $"Withdrew {amount} {account.CurrencyId}");

            CCS_BankTransactionResult result = CCS_BankTransactionResult.Success(
                account.AccountId,
                account.OwnerId,
                account.CurrencyId,
                amount,
                addResult.BalanceAfter,
                account.Balance,
                $"Withdrew {amount} from bank account.");
            NotifyTransactionCompleted(result);
            return result;
        }

        public int GetBalance(string ownerId, string accountDefinitionId)
        {
            string resolvedOwnerId = ResolveOwnerId(ownerId);
            if (!TryResolveAccountDefinition(accountDefinitionId, out CCS_BankAccountDefinition definition))
            {
                return 0;
            }

            string accountId = BuildAccountId(resolvedOwnerId, definition.AccountDefinitionId);
            return accountsById.TryGetValue(accountId, out BankAccountInstance account)
                && account.State == CCS_BankAccountState.Open
                ? account.Balance
                : 0;
        }

        public int GetDefaultAccountBalance(string ownerId)
        {
            if (activeProfile == null)
            {
                return 0;
            }

            return GetBalance(ownerId, activeProfile.DefaultAccountDefinitionId);
        }

        public bool CanDebitForUpkeep(string ownerId, string accountDefinitionId, int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            string resolvedOwnerId = ResolveOwnerId(ownerId);
            if (!TryResolveAccountDefinition(accountDefinitionId, out CCS_BankAccountDefinition definition))
            {
                return false;
            }

            string accountId = BuildAccountId(resolvedOwnerId, definition.AccountDefinitionId);
            return accountsById.TryGetValue(accountId, out BankAccountInstance account)
                && account.State == CCS_BankAccountState.Open
                && account.Balance >= amount;
        }

        public CCS_BankTransactionResult TryDebitForUpkeep(
            string ownerId,
            string accountDefinitionId,
            int amount,
            string reason)
        {
            CCS_BankTransactionResult openResult = EnsureOpenAccount(ownerId, accountDefinitionId, out BankAccountInstance account);
            if (!openResult.IsSuccess)
            {
                return openResult;
            }

            if (amount <= 0)
            {
                return Failure(
                    CCS_BankTransactionResultType.InvalidAmount,
                    account.AccountId,
                    account.OwnerId,
                    account.CurrencyId,
                    "Upkeep debit amount must be greater than zero.");
            }

            if (account.Balance < amount)
            {
                return Failure(
                    CCS_BankTransactionResultType.InsufficientBankFunds,
                    account.AccountId,
                    account.OwnerId,
                    account.CurrencyId,
                    "Bank balance is too low for upkeep payment.");
            }

            account.Balance -= amount;
            RecordTransaction(
                account,
                -amount,
                "UpkeepDebit",
                reason ?? "Upkeep payment",
                $"Upkeep debit {amount} {account.CurrencyId}");

            int walletBalance = GetWalletBalance(account.CurrencyId);
            CCS_BankTransactionResult result = CCS_BankTransactionResult.Success(
                account.AccountId,
                account.OwnerId,
                account.CurrencyId,
                amount,
                walletBalance,
                account.Balance,
                $"Debited {amount} from bank for upkeep.");
            NotifyTransactionCompleted(result);
            return result;
        }

        public int GetOwnedLandClaimCount()
        {
            return landClaimService != null && landClaimService.IsInitialized
                ? landClaimService.GetClaimCount()
                : 0;
        }

        public string TryResolveNearbyLandClaimId(Vector3 worldPosition)
        {
            if (landClaimService == null || !landClaimService.IsInitialized)
            {
                return string.Empty;
            }

            return landClaimService.TryResolveClaimIdContainingPosition(worldPosition) ?? string.Empty;
        }

        public bool CanBorrow(string ownerId, string loanDefinitionId)
        {
            if (!isInitialized)
            {
                return false;
            }

            if (!TryResolveLoanDefinition(loanDefinitionId, out CCS_LoanDefinition definition))
            {
                return false;
            }

            if (!definition.Enabled)
            {
                return false;
            }

            if (definition.PrincipalAmount <= 0 || definition.RepaymentAmount <= 0)
            {
                return false;
            }

            string resolvedOwnerId = ResolveOwnerId(ownerId);
            return CountActiveLoans(resolvedOwnerId, definition.LoanDefinitionId) < definition.MaxActiveLoans;
        }

        public CCS_LoanTransactionResult TryOpenLoan(string ownerId, string loanDefinitionId)
        {
            if (!isInitialized)
            {
                return LoanFailure(
                    CCS_LoanTransactionResultType.ServiceNotReady,
                    string.Empty,
                    ownerId,
                    string.Empty,
                    "Banking service is not initialized.");
            }

            if (!TryResolveLoanDefinition(loanDefinitionId, out CCS_LoanDefinition definition))
            {
                return LoanFailure(
                    CCS_LoanTransactionResultType.InvalidLoan,
                    string.Empty,
                    ownerId,
                    string.Empty,
                    "Loan definition was not found.");
            }

            if (!definition.Enabled)
            {
                return LoanFailure(
                    CCS_LoanTransactionResultType.LoanDisabled,
                    string.Empty,
                    ownerId,
                    definition.CurrencyId,
                    "Loan definition is disabled.");
            }

            if (definition.PrincipalAmount <= 0 || definition.RepaymentAmount <= 0)
            {
                return LoanFailure(
                    CCS_LoanTransactionResultType.InvalidAmount,
                    string.Empty,
                    ownerId,
                    definition.CurrencyId,
                    "Loan principal and repayment amounts must be greater than zero.");
            }

            string resolvedOwnerId = ResolveOwnerId(ownerId);
            if (CountActiveLoans(resolvedOwnerId, definition.LoanDefinitionId) >= definition.MaxActiveLoans)
            {
                return LoanFailure(
                    CCS_LoanTransactionResultType.MaxLoansReached,
                    string.Empty,
                    resolvedOwnerId,
                    definition.CurrencyId,
                    "Maximum active loans reached for this loan product.");
            }

            if (currencyService == null || !currencyService.IsInitialized)
            {
                return LoanFailure(
                    CCS_LoanTransactionResultType.ServiceNotReady,
                    string.Empty,
                    resolvedOwnerId,
                    definition.CurrencyId,
                    "Currency service is not ready.");
            }

            CCS_CurrencyTransactionResult addResult = currencyService.AddCurrency(
                definition.CurrencyId,
                definition.PrincipalAmount,
                $"Loan principal ({definition.DisplayName})");
            if (!addResult.IsSuccess)
            {
                return LoanFailure(
                    CCS_LoanTransactionResultType.UnknownFailure,
                    string.Empty,
                    resolvedOwnerId,
                    definition.CurrencyId,
                    addResult.Message);
            }

            string loanId = BuildLoanId(resolvedOwnerId, definition.LoanDefinitionId);
            LoanInstance loan = new LoanInstance
            {
                LoanId = loanId,
                OwnerId = resolvedOwnerId,
                LoanDefinitionId = definition.LoanDefinitionId,
                CurrencyId = definition.CurrencyId,
                PrincipalAmount = definition.PrincipalAmount,
                RepaymentAmount = definition.RepaymentAmount,
                Balance = definition.RepaymentAmount,
                State = CCS_LoanState.Active
            };
            loansById[loanId] = loan;

            RecordLoanTransaction(
                loan,
                definition.PrincipalAmount,
                "Borrow",
                addResult.Message,
                $"Borrowed {definition.PrincipalAmount} {definition.CurrencyId}");

            int bankBalance = GetDefaultAccountBalance(resolvedOwnerId);
            CCS_LoanTransactionResult result = CCS_LoanTransactionResult.Success(
                loanId,
                resolvedOwnerId,
                definition.CurrencyId,
                definition.PrincipalAmount,
                addResult.BalanceAfter,
                bankBalance,
                loan.Balance,
                loan.State,
                $"Opened loan '{definition.DisplayName}'.");
            NotifyLoanTransactionCompleted(result);
            return result;
        }

        public CCS_LoanSnapshot GetActiveLoan(string ownerId, string loanDefinitionId)
        {
            string resolvedOwnerId = ResolveOwnerId(ownerId);
            if (!TryResolveLoanDefinition(loanDefinitionId, out CCS_LoanDefinition definition))
            {
                return null;
            }

            string loanId = BuildLoanId(resolvedOwnerId, definition.LoanDefinitionId);
            if (!loansById.TryGetValue(loanId, out LoanInstance loan)
                || !IsRepayableLoanState(loan.State))
            {
                return null;
            }

            return BuildLoanSnapshot(loan);
        }

        public int GetLoanBalance(string ownerId, string loanDefinitionId)
        {
            CCS_LoanSnapshot snapshot = GetActiveLoan(ownerId, loanDefinitionId);
            return snapshot?.balance ?? 0;
        }

        public CCS_LoanTransactionResult TryRepayLoan(string ownerId, string loanDefinitionId)
        {
            if (!isInitialized)
            {
                return LoanFailure(
                    CCS_LoanTransactionResultType.ServiceNotReady,
                    string.Empty,
                    ownerId,
                    string.Empty,
                    "Banking service is not initialized.");
            }

            if (!TryResolveLoanDefinition(loanDefinitionId, out CCS_LoanDefinition definition))
            {
                return LoanFailure(
                    CCS_LoanTransactionResultType.InvalidLoan,
                    string.Empty,
                    ownerId,
                    string.Empty,
                    "Loan definition was not found.");
            }

            string resolvedOwnerId = ResolveOwnerId(ownerId);
            string loanId = BuildLoanId(resolvedOwnerId, definition.LoanDefinitionId);
            if (!loansById.TryGetValue(loanId, out LoanInstance loan)
                || !IsRepayableLoanState(loan.State))
            {
                return LoanFailure(
                    CCS_LoanTransactionResultType.InvalidLoan,
                    loanId,
                    resolvedOwnerId,
                    definition.CurrencyId,
                    "No active loan is available for repayment.");
            }

            int amountDue = loan.Balance;
            if (amountDue <= 0)
            {
                return LoanFailure(
                    CCS_LoanTransactionResultType.InvalidAmount,
                    loanId,
                    resolvedOwnerId,
                    loan.CurrencyId,
                    "Loan balance is already cleared.");
            }

            if (currencyService == null || !currencyService.IsInitialized)
            {
                return LoanFailure(
                    CCS_LoanTransactionResultType.ServiceNotReady,
                    loanId,
                    resolvedOwnerId,
                    loan.CurrencyId,
                    "Currency service is not ready.");
            }

            int remaining = amountDue;
            int walletBalance = GetWalletBalance(loan.CurrencyId);
            int bankBalance = GetDefaultAccountBalance(resolvedOwnerId);

            if (definition.AutoRepayFromBank && remaining > 0 && activeProfile != null)
            {
                int bankPayment = Mathf.Min(remaining, bankBalance);
                if (bankPayment > 0)
                {
                    CCS_BankTransactionResult debitResult = TryDebitForUpkeep(
                        resolvedOwnerId,
                        activeProfile.DefaultAccountDefinitionId,
                        bankPayment,
                        $"Loan repayment ({definition.DisplayName})");
                    if (!debitResult.IsSuccess)
                    {
                        return LoanFailure(
                            CCS_LoanTransactionResultType.InsufficientFunds,
                            loanId,
                            resolvedOwnerId,
                            loan.CurrencyId,
                            debitResult.Message);
                    }

                    remaining -= bankPayment;
                    bankBalance = debitResult.BankBalanceAfter;
                }
            }

            if (remaining > 0 && definition.AutoRepayFromWallet)
            {
                if (!currencyService.CanAfford(loan.CurrencyId, remaining))
                {
                    return LoanFailure(
                        CCS_LoanTransactionResultType.InsufficientFunds,
                        loanId,
                        resolvedOwnerId,
                        loan.CurrencyId,
                        "Insufficient bank and wallet funds for loan repayment.");
                }

                CCS_CurrencyTransactionResult removeResult = currencyService.RemoveCurrency(
                    loan.CurrencyId,
                    remaining,
                    $"Loan repayment ({definition.DisplayName})");
                if (!removeResult.IsSuccess)
                {
                    return LoanFailure(
                        CCS_LoanTransactionResultType.UnknownFailure,
                        loanId,
                        resolvedOwnerId,
                        loan.CurrencyId,
                        removeResult.Message);
                }

                walletBalance = removeResult.BalanceAfter;
                remaining = 0;
            }

            if (remaining > 0)
            {
                return LoanFailure(
                    CCS_LoanTransactionResultType.InsufficientFunds,
                    loanId,
                    resolvedOwnerId,
                    loan.CurrencyId,
                    "Insufficient funds for loan repayment.");
            }

            loan.Balance = 0;
            loan.State = CCS_LoanState.Paid;
            RecordLoanTransaction(
                loan,
                -amountDue,
                "Repay",
                $"Repaid loan '{definition.DisplayName}'",
                $"Repaid {amountDue} {loan.CurrencyId}");

            CCS_LoanTransactionResult result = CCS_LoanTransactionResult.Success(
                loanId,
                resolvedOwnerId,
                loan.CurrencyId,
                amountDue,
                walletBalance,
                bankBalance,
                loan.Balance,
                loan.State,
                $"Repaid loan '{definition.DisplayName}'.");
            NotifyLoanTransactionCompleted(result);
            return result;
        }

        public CCS_BankAccountSnapshot[] CaptureBankingState()
        {
            if (accountsById.Count == 0)
            {
                return Array.Empty<CCS_BankAccountSnapshot>();
            }

            CCS_BankAccountSnapshot[] snapshots = new CCS_BankAccountSnapshot[accountsById.Count];
            int index = 0;
            foreach (KeyValuePair<string, BankAccountInstance> entry in accountsById)
            {
                BankAccountInstance account = entry.Value;
                if (account == null)
                {
                    continue;
                }

                snapshots[index++] = new CCS_BankAccountSnapshot
                {
                    accountId = account.AccountId,
                    ownerId = account.OwnerId,
                    accountDefinitionId = account.AccountDefinitionId,
                    currencyId = account.CurrencyId,
                    balance = account.Balance,
                    accountState = (int)account.State,
                    transactionSummaryPlaceholder = BuildTransactionSummaryPlaceholder(account)
                };
            }

            if (index < snapshots.Length)
            {
                Array.Resize(ref snapshots, index);
            }

            return snapshots;
        }

        public CCS_LoanSnapshot[] CaptureLoanState()
        {
            if (loansById.Count == 0)
            {
                return Array.Empty<CCS_LoanSnapshot>();
            }

            CCS_LoanSnapshot[] snapshots = new CCS_LoanSnapshot[loansById.Count];
            int index = 0;
            foreach (KeyValuePair<string, LoanInstance> entry in loansById)
            {
                LoanInstance loan = entry.Value;
                if (loan == null)
                {
                    continue;
                }

                snapshots[index++] = BuildLoanSnapshot(loan);
            }

            if (index < snapshots.Length)
            {
                Array.Resize(ref snapshots, index);
            }

            return snapshots;
        }

        public void RestoreState(CCS_BankAccountSnapshot[] snapshots)
        {
            accountsById.Clear();
            transactionHistory.Clear();

            if (snapshots == null || snapshots.Length == 0)
            {
                return;
            }

            for (int index = 0; index < snapshots.Length; index++)
            {
                CCS_BankAccountSnapshot snapshot = snapshots[index];
                if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.accountId))
                {
                    continue;
                }

                BankAccountInstance account = new BankAccountInstance
                {
                    AccountId = snapshot.accountId,
                    OwnerId = snapshot.ownerId ?? string.Empty,
                    AccountDefinitionId = snapshot.accountDefinitionId ?? string.Empty,
                    CurrencyId = snapshot.currencyId ?? string.Empty,
                    Balance = Mathf.Max(0, snapshot.balance),
                    State = Enum.IsDefined(typeof(CCS_BankAccountState), snapshot.accountState)
                        ? (CCS_BankAccountState)snapshot.accountState
                        : CCS_BankAccountState.Open
                };
                accountsById[account.AccountId] = account;
            }
        }

        public void RestoreLoanState(CCS_LoanSnapshot[] snapshots)
        {
            loansById.Clear();
            loanTransactionHistory.Clear();

            if (snapshots == null || snapshots.Length == 0)
            {
                return;
            }

            for (int index = 0; index < snapshots.Length; index++)
            {
                CCS_LoanSnapshot snapshot = snapshots[index];
                if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.loanId))
                {
                    continue;
                }

                LoanInstance loan = new LoanInstance
                {
                    LoanId = snapshot.loanId,
                    OwnerId = snapshot.ownerId ?? string.Empty,
                    LoanDefinitionId = snapshot.loanDefinitionId ?? string.Empty,
                    CurrencyId = snapshot.currencyId ?? string.Empty,
                    PrincipalAmount = Mathf.Max(0, snapshot.principalAmount),
                    RepaymentAmount = Mathf.Max(0, snapshot.repaymentAmount),
                    Balance = Mathf.Max(0, snapshot.balance),
                    State = Enum.IsDefined(typeof(CCS_LoanState), snapshot.loanState)
                        ? (CCS_LoanState)snapshot.loanState
                        : CCS_LoanState.None
                };
                loansById[loan.LoanId] = loan;
            }
        }

        private CCS_BankTransactionResult EnsureOpenAccount(
            string ownerId,
            string accountDefinitionId,
            out BankAccountInstance account)
        {
            account = null;
            CCS_BankTransactionResult openResult = TryOpenAccount(ownerId, accountDefinitionId);
            if (!openResult.IsSuccess)
            {
                return openResult;
            }

            string resolvedOwnerId = ResolveOwnerId(ownerId);
            if (!TryResolveAccountDefinition(accountDefinitionId, out CCS_BankAccountDefinition definition))
            {
                return Failure(
                    CCS_BankTransactionResultType.InvalidAccount,
                    openResult.AccountId,
                    resolvedOwnerId,
                    openResult.CurrencyId,
                    "Bank account definition was not found.");
            }

            string accountId = BuildAccountId(resolvedOwnerId, definition.AccountDefinitionId);
            if (!accountsById.TryGetValue(accountId, out account) || account.State != CCS_BankAccountState.Open)
            {
                return Failure(
                    CCS_BankTransactionResultType.AccountClosed,
                    accountId,
                    resolvedOwnerId,
                    definition.CurrencyId,
                    "Bank account is not open.");
            }

            return openResult;
        }

        private bool TryResolveAccountDefinition(string accountDefinitionId, out CCS_BankAccountDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(accountDefinitionId))
            {
                if (activeProfile != null
                    && activeProfile.TryGetAccountById(activeProfile.DefaultAccountDefinitionId, out definition))
                {
                    return true;
                }

                return false;
            }

            return accountDefinitionLookup.TryGetValue(accountDefinitionId, out definition);
        }

        private bool TryResolveLoanDefinition(string loanDefinitionId, out CCS_LoanDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(loanDefinitionId))
            {
                if (activeLoanProfile != null
                    && activeLoanProfile.TryGetDefaultLoan(out definition))
                {
                    return true;
                }

                return false;
            }

            return loanDefinitionLookup.TryGetValue(loanDefinitionId, out definition);
        }

        private int CountActiveLoans(string ownerId, string loanDefinitionId)
        {
            string loanId = BuildLoanId(ownerId, loanDefinitionId);
            if (!loansById.TryGetValue(loanId, out LoanInstance loan))
            {
                return 0;
            }

            return IsRepayableLoanState(loan.State) ? 1 : 0;
        }

        private static bool IsRepayableLoanState(CCS_LoanState state)
        {
            return state == CCS_LoanState.Active || state == CCS_LoanState.Due;
        }

        private static string BuildLoanId(string ownerId, string loanDefinitionId)
        {
            return $"{ownerId}:{loanDefinitionId}";
        }

        private static CCS_LoanSnapshot BuildLoanSnapshot(LoanInstance loan)
        {
            return new CCS_LoanSnapshot
            {
                loanId = loan.LoanId,
                ownerId = loan.OwnerId,
                loanDefinitionId = loan.LoanDefinitionId,
                currencyId = loan.CurrencyId,
                principalAmount = loan.PrincipalAmount,
                repaymentAmount = loan.RepaymentAmount,
                balance = loan.Balance,
                loanState = (int)loan.State,
                transactionSummaryPlaceholder = BuildLoanTransactionSummaryPlaceholder(loan)
            };
        }

        private static string ResolveOwnerId(string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId)
                ? CCS_BankingContentIds.DefaultPlayerOwnerId
                : ownerId;
        }

        private static string BuildAccountId(string ownerId, string accountDefinitionId)
        {
            return $"{ownerId}:{accountDefinitionId}";
        }

        private int GetWalletBalance(string currencyId)
        {
            return currencyService != null && currencyService.IsInitialized
                ? currencyService.GetBalance(currencyId)
                : 0;
        }

        private void RecordTransaction(
            BankAccountInstance account,
            int deltaAmount,
            string transactionKind,
            string reason,
            string summaryPlaceholder)
        {
            CCS_BankTransaction transaction = new CCS_BankTransaction(
                account.AccountId,
                account.OwnerId,
                account.CurrencyId,
                deltaAmount,
                account.Balance,
                transactionKind,
                reason,
                DateTime.UtcNow.ToString("o"),
                summaryPlaceholder);
            transactionHistory.Add(transaction);
            if (transactionHistory.Count > MaxTransactionHistoryEntries)
            {
                transactionHistory.RemoveAt(0);
            }
        }

        private static string BuildTransactionSummaryPlaceholder(BankAccountInstance account)
        {
            return account == null
                ? string.Empty
                : $"Account {account.AccountId} balance {account.Balance} ({account.CurrencyId})";
        }

        private void RecordLoanTransaction(
            LoanInstance loan,
            int deltaAmount,
            string transactionKind,
            string reason,
            string summaryPlaceholder)
        {
            CCS_LoanTransaction transaction = new CCS_LoanTransaction(
                loan.LoanId,
                loan.OwnerId,
                loan.CurrencyId,
                deltaAmount,
                loan.Balance,
                transactionKind,
                reason,
                DateTime.UtcNow.ToString("o"),
                summaryPlaceholder);
            loanTransactionHistory.Add(transaction);
            if (loanTransactionHistory.Count > MaxTransactionHistoryEntries)
            {
                loanTransactionHistory.RemoveAt(0);
            }
        }

        private static string BuildLoanTransactionSummaryPlaceholder(LoanInstance loan)
        {
            return loan == null
                ? string.Empty
                : $"Loan {loan.LoanId} balance {loan.Balance} ({loan.CurrencyId}) state {loan.State}";
        }

        private void NotifyTransactionCompleted(CCS_BankTransactionResult result)
        {
            BankTransactionCompleted?.Invoke(result);
        }

        private void NotifyLoanTransactionCompleted(CCS_LoanTransactionResult result)
        {
            LoanTransactionCompleted?.Invoke(result);
        }

        private static CCS_BankTransactionResult Failure(
            CCS_BankTransactionResultType resultType,
            string accountId,
            string ownerId,
            string currencyId,
            string message)
        {
            return CCS_BankTransactionResult.Failure(resultType, accountId, ownerId, currencyId, message);
        }

        private static CCS_LoanTransactionResult LoanFailure(
            CCS_LoanTransactionResultType resultType,
            string loanId,
            string ownerId,
            string currencyId,
            string message)
        {
            return CCS_LoanTransactionResult.Failure(resultType, loanId, ownerId, currencyId, message);
        }
    }
}
