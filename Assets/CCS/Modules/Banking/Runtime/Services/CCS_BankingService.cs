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
// NOTES: Milestone 2.4.0 — no loans, taxes, interest, or debt yet.
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

        private readonly Dictionary<string, BankAccountInstance> accountsById =
            new Dictionary<string, BankAccountInstance>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, CCS_BankAccountDefinition> accountDefinitionLookup =
            new Dictionary<string, CCS_BankAccountDefinition>(StringComparer.OrdinalIgnoreCase);

        private readonly List<CCS_BankTransaction> transactionHistory = new List<CCS_BankTransaction>();

        private CCS_BankAccountProfile activeProfile;
        private CCS_CurrencyService currencyService;
        private CCS_LandClaimService landClaimService;
        private bool isInitialized;

        public event Action<CCS_BankTransactionResult> BankTransactionCompleted;

        public bool IsInitialized => isInitialized;

        public CCS_BankAccountProfile ActiveProfile => activeProfile;

        public IReadOnlyList<CCS_BankTransaction> TransactionHistory => transactionHistory;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_BankAccountProfile profile)
        {
            activeProfile = profile;
            accountDefinitionLookup.Clear();

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

        private void NotifyTransactionCompleted(CCS_BankTransactionResult result)
        {
            BankTransactionCompleted?.Invoke(result);
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
    }
}
