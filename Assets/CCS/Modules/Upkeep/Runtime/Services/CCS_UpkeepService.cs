using System;
using System.Collections.Generic;
using CCS.Modules.Banking;
using CCS.Modules.Economy;
using CCS.Modules.Land;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_UpkeepService
// CATEGORY: Modules / Upkeep / Runtime / Services
// PURPOSE: Owns recurring upkeep entries, due state, payments, and save/restore.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.5.0 — no debt, loans, foreclosure, or faction law yet.
// =============================================================================

namespace CCS.Modules.Upkeep
{
    public sealed class CCS_UpkeepService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_UpkeepService]";
        private const int MaxTransactionHistoryEntries = 32;

        private readonly Dictionary<string, CCS_UpkeepEntry> entriesByTargetId =
            new Dictionary<string, CCS_UpkeepEntry>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, CCS_UpkeepDefinition> definitionLookup =
            new Dictionary<string, CCS_UpkeepDefinition>(StringComparer.OrdinalIgnoreCase);

        private readonly List<CCS_UpkeepTransaction> transactionHistory = new List<CCS_UpkeepTransaction>();

        private CCS_UpkeepProfile activeProfile;
        private CCS_CurrencyService currencyService;
        private CCS_BankingService bankingService;
        private Func<int> currentDayProvider;
        private int manualDayCounter = 1;
        private bool isInitialized;

        public event Action<CCS_UpkeepTransactionResult> UpkeepTransactionCompleted;

        public bool IsInitialized => isInitialized;

        public CCS_UpkeepProfile ActiveProfile => activeProfile;

        public IReadOnlyList<CCS_UpkeepTransaction> TransactionHistory => transactionHistory;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_UpkeepProfile profile)
        {
            activeProfile = profile;
            definitionLookup.Clear();

            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_UpkeepValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            CCS_UpkeepDefinition[] definitions = profile.UpkeepDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_UpkeepDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.UpkeepDefinitionId))
                {
                    continue;
                }

                definitionLookup[definition.UpkeepDefinitionId] = definition;
            }

            isInitialized = validation.IsSuccess || definitionLookup.Count > 0;
        }

        public void BindCurrencyService(CCS_CurrencyService currency)
        {
            currencyService = currency;
        }

        public void BindBankingService(CCS_BankingService banking)
        {
            bankingService = banking;
        }

        public void BindCurrentDayProvider(Func<int> provider)
        {
            currentDayProvider = provider;
        }

        public CCS_UpkeepTransactionResult TryRegisterLandClaimUpkeep(CCS_LandClaimInstance claim)
        {
            if (!isInitialized || claim == null || string.IsNullOrWhiteSpace(claim.InstanceId))
            {
                return Failure(
                    CCS_UpkeepTransactionResultType.InvalidTarget,
                    string.Empty,
                    claim?.InstanceId,
                    string.Empty,
                    CCS_UpkeepState.Disabled,
                    "Land claim is invalid for upkeep registration.");
            }

            if (entriesByTargetId.ContainsKey(claim.InstanceId))
            {
                CCS_UpkeepEntry existing = entriesByTargetId[claim.InstanceId];
                return Success(
                    existing.entryId,
                    existing.targetId,
                    existing.upkeepDefinitionId,
                    0,
                    CCS_UpkeepPaymentSource.None,
                    (CCS_UpkeepState)existing.status,
                    "Upkeep entry already registered for land claim.");
            }

            if (activeProfile == null || !activeProfile.TryGetDefaultLandClaimUpkeep(out CCS_UpkeepDefinition definition))
            {
                return Failure(
                    CCS_UpkeepTransactionResultType.InvalidDefinition,
                    string.Empty,
                    claim.InstanceId,
                    string.Empty,
                    CCS_UpkeepState.Disabled,
                    "Default land claim upkeep definition was not found.");
            }

            if (!definition.Enabled)
            {
                return Failure(
                    CCS_UpkeepTransactionResultType.Disabled,
                    string.Empty,
                    claim.InstanceId,
                    definition.UpkeepDefinitionId,
                    CCS_UpkeepState.Disabled,
                    "Land claim upkeep definition is disabled.");
            }

            int currentDay = GetCurrentDay();
            CCS_UpkeepEntry entry = new CCS_UpkeepEntry
            {
                entryId = BuildEntryId(claim.InstanceId, definition.UpkeepDefinitionId),
                ownerId = string.IsNullOrWhiteSpace(claim.OwnerId)
                    ? CCS_UpkeepContentIds.DefaultPlayerOwnerId
                    : claim.OwnerId,
                targetId = claim.InstanceId,
                targetType = (int)CCS_UpkeepTargetType.LandClaim,
                upkeepDefinitionId = definition.UpkeepDefinitionId,
                amountDue = 0,
                lastPaidDay = 0,
                nextDueDay = currentDay + Math.Max(1, definition.IntervalDaysPlaceholder),
                status = (int)CCS_UpkeepState.Current,
                lastTransactionSummary = "Upkeep registered."
            };
            entriesByTargetId[claim.InstanceId] = entry;

            CCS_UpkeepTransactionResult result = Success(
                entry.entryId,
                entry.targetId,
                entry.upkeepDefinitionId,
                0,
                CCS_UpkeepPaymentSource.None,
                CCS_UpkeepState.Current,
                $"Registered upkeep for land claim '{claim.InstanceId}'.");
            NotifyTransactionCompleted(result);
            return result;
        }

        public bool TryGetEntryForTarget(string targetId, out CCS_UpkeepEntry entry)
        {
            entry = null;
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            return entriesByTargetId.TryGetValue(targetId, out entry) && entry != null;
        }

        public CCS_UpkeepTransactionResult TryForceDue(string targetId)
        {
            if (!TryGetEntryForTarget(targetId, out CCS_UpkeepEntry entry)
                || !TryResolveDefinition(entry.upkeepDefinitionId, out CCS_UpkeepDefinition definition))
            {
                return Failure(
                    CCS_UpkeepTransactionResultType.InvalidTarget,
                    string.Empty,
                    targetId,
                    string.Empty,
                    CCS_UpkeepState.Disabled,
                    "Upkeep entry was not found.");
            }

            if (!definition.Enabled)
            {
                return Failure(
                    CCS_UpkeepTransactionResultType.Disabled,
                    entry.entryId,
                    entry.targetId,
                    entry.upkeepDefinitionId,
                    CCS_UpkeepState.Disabled,
                    "Upkeep definition is disabled.");
            }

            entry.amountDue = definition.Amount;
            entry.status = (int)CCS_UpkeepState.Due;
            entry.lastTransactionSummary = $"Upkeep due: {definition.Amount} {definition.CurrencyId}.";

            CCS_UpkeepTransactionResult result = Success(
                entry.entryId,
                entry.targetId,
                entry.upkeepDefinitionId,
                entry.amountDue,
                CCS_UpkeepPaymentSource.None,
                CCS_UpkeepState.Due,
                "Upkeep marked due.");
            NotifyTransactionCompleted(result);
            return result;
        }

        public CCS_UpkeepTransactionResult TryPayUpkeep(string targetId)
        {
            if (!TryGetEntryForTarget(targetId, out CCS_UpkeepEntry entry)
                || !TryResolveDefinition(entry.upkeepDefinitionId, out CCS_UpkeepDefinition definition))
            {
                return Failure(
                    CCS_UpkeepTransactionResultType.InvalidTarget,
                    string.Empty,
                    targetId,
                    string.Empty,
                    CCS_UpkeepState.Disabled,
                    "Upkeep entry was not found.");
            }

            CCS_UpkeepState currentState = (CCS_UpkeepState)entry.status;
            if (currentState != CCS_UpkeepState.Due || entry.amountDue <= 0)
            {
                return Failure(
                    CCS_UpkeepTransactionResultType.NotDue,
                    entry.entryId,
                    entry.targetId,
                    entry.upkeepDefinitionId,
                    currentState,
                    "Upkeep is not due for payment.");
            }

            if (!definition.Enabled)
            {
                return Failure(
                    CCS_UpkeepTransactionResultType.Disabled,
                    entry.entryId,
                    entry.targetId,
                    entry.upkeepDefinitionId,
                    CCS_UpkeepState.Disabled,
                    "Upkeep definition is disabled.");
            }

            int amount = entry.amountDue;
            string currencyId = definition.CurrencyId;
            string ownerId = entry.ownerId;

            if (definition.AutoPayFromBank
                && bankingService != null
                && bankingService.IsInitialized
                && bankingService.ActiveProfile != null
                && bankingService.CanDebitForUpkeep(
                    ownerId,
                    bankingService.ActiveProfile.DefaultAccountDefinitionId,
                    amount))
            {
                CCS_BankTransactionResult bankResult = bankingService.TryDebitForUpkeep(
                    ownerId,
                    bankingService.ActiveProfile.DefaultAccountDefinitionId,
                    amount,
                    $"Upkeep payment: {definition.DisplayName}");
                if (bankResult.IsSuccess)
                {
                    return CompleteSuccessfulPayment(
                        entry,
                        definition,
                        amount,
                        currencyId,
                        CCS_UpkeepPaymentSource.Bank,
                        bankResult.Message);
                }
            }

            if (definition.AutoPayFromWallet
                && currencyService != null
                && currencyService.IsInitialized
                && currencyService.CanAfford(currencyId, amount))
            {
                CCS_CurrencyTransactionResult walletResult = currencyService.RemoveCurrency(
                    currencyId,
                    amount,
                    $"Upkeep payment: {definition.DisplayName}");
                if (walletResult.IsSuccess)
                {
                    return CompleteSuccessfulPayment(
                        entry,
                        definition,
                        amount,
                        currencyId,
                        CCS_UpkeepPaymentSource.Wallet,
                        walletResult.Message);
                }
            }

            entry.status = (int)CCS_UpkeepState.Failed;
            entry.lastTransactionSummary = "Upkeep payment failed: insufficient bank and wallet funds.";
            CCS_UpkeepTransactionResult failedResult = Failure(
                CCS_UpkeepTransactionResultType.InsufficientFunds,
                entry.entryId,
                entry.targetId,
                entry.upkeepDefinitionId,
                CCS_UpkeepState.Failed,
                entry.lastTransactionSummary);
            RecordTransaction(entry, amount, CCS_UpkeepPaymentSource.None, CCS_UpkeepState.Failed, failedResult.Message);
            NotifyTransactionCompleted(failedResult);
            return failedResult;
        }

        public void ReconcileLandClaimEntries(CCS_LandClaimService landClaimService)
        {
            if (!isInitialized || landClaimService == null || !landClaimService.IsInitialized)
            {
                return;
            }

            CCS_LandClaimSnapshot[] snapshots = landClaimService.CaptureClaimState();
            for (int index = 0; index < snapshots.Length; index++)
            {
                CCS_LandClaimSnapshot snapshot = snapshots[index];
                if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.instanceId))
                {
                    continue;
                }

                if (entriesByTargetId.ContainsKey(snapshot.instanceId))
                {
                    continue;
                }

                if (!landClaimService.TryGetClaim(snapshot.instanceId, out CCS_LandClaimInstance claim))
                {
                    continue;
                }

                TryRegisterLandClaimUpkeep(claim);
            }
        }

        public CCS_UpkeepEntry[] CaptureUpkeepState()
        {
            if (entriesByTargetId.Count == 0)
            {
                return Array.Empty<CCS_UpkeepEntry>();
            }

            CCS_UpkeepEntry[] snapshots = new CCS_UpkeepEntry[entriesByTargetId.Count];
            int index = 0;
            foreach (KeyValuePair<string, CCS_UpkeepEntry> pair in entriesByTargetId)
            {
                CCS_UpkeepEntry source = pair.Value;
                if (source == null)
                {
                    continue;
                }

                snapshots[index++] = CloneEntry(source);
            }

            if (index < snapshots.Length)
            {
                Array.Resize(ref snapshots, index);
            }

            return snapshots;
        }

        public void RestoreState(CCS_UpkeepEntry[] entries)
        {
            entriesByTargetId.Clear();
            transactionHistory.Clear();

            if (entries == null || entries.Length == 0)
            {
                return;
            }

            for (int index = 0; index < entries.Length; index++)
            {
                CCS_UpkeepEntry entry = entries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.targetId))
                {
                    continue;
                }

                entriesByTargetId[entry.targetId] = CloneEntry(entry);
            }
        }

        private CCS_UpkeepTransactionResult CompleteSuccessfulPayment(
            CCS_UpkeepEntry entry,
            CCS_UpkeepDefinition definition,
            int amount,
            string currencyId,
            CCS_UpkeepPaymentSource paymentSource,
            string paymentMessage)
        {
            int currentDay = GetCurrentDay();
            entry.lastPaidDay = currentDay;
            entry.nextDueDay = currentDay + Math.Max(1, definition.IntervalDaysPlaceholder);
            entry.amountDue = 0;
            entry.status = (int)CCS_UpkeepState.Paid;
            entry.lastTransactionSummary =
                $"Paid {amount} {currencyId} from {paymentSource}. {paymentMessage}";

            RecordTransaction(entry, amount, paymentSource, CCS_UpkeepState.Paid, entry.lastTransactionSummary);

            entry.status = (int)CCS_UpkeepState.Current;
            CCS_UpkeepTransactionResult result = Success(
                entry.entryId,
                entry.targetId,
                entry.upkeepDefinitionId,
                amount,
                paymentSource,
                CCS_UpkeepState.Current,
                entry.lastTransactionSummary);
            NotifyTransactionCompleted(result);
            return result;
        }

        private void RecordTransaction(
            CCS_UpkeepEntry entry,
            int amount,
            CCS_UpkeepPaymentSource paymentSource,
            CCS_UpkeepState resultState,
            string reason)
        {
            if (entry == null)
            {
                return;
            }

            string currencyIdForTransaction = string.Empty;
            if (TryResolveDefinition(entry.upkeepDefinitionId, out CCS_UpkeepDefinition definitionForTransaction))
            {
                currencyIdForTransaction = definitionForTransaction.CurrencyId;
            }

            CCS_UpkeepTransaction transaction = new CCS_UpkeepTransaction(
                entry.entryId,
                entry.targetId,
                entry.upkeepDefinitionId,
                currencyIdForTransaction,
                amount,
                paymentSource,
                resultState,
                reason,
                DateTime.UtcNow.ToString("o"),
                entry.lastTransactionSummary);
            transactionHistory.Add(transaction);
            if (transactionHistory.Count > MaxTransactionHistoryEntries)
            {
                transactionHistory.RemoveAt(0);
            }
        }

        private bool TryResolveDefinition(string upkeepDefinitionId, out CCS_UpkeepDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(upkeepDefinitionId))
            {
                return false;
            }

            return definitionLookup.TryGetValue(upkeepDefinitionId, out definition) && definition != null;
        }

        private static CCS_UpkeepEntry CloneEntry(CCS_UpkeepEntry source)
        {
            return new CCS_UpkeepEntry
            {
                entryId = source.entryId,
                ownerId = source.ownerId,
                targetId = source.targetId,
                targetType = source.targetType,
                upkeepDefinitionId = source.upkeepDefinitionId,
                amountDue = source.amountDue,
                lastPaidDay = source.lastPaidDay,
                nextDueDay = source.nextDueDay,
                status = source.status,
                lastTransactionSummary = source.lastTransactionSummary
            };
        }

        private static string BuildEntryId(string targetId, string upkeepDefinitionId)
        {
            return $"{targetId}:{upkeepDefinitionId}";
        }

        private int GetCurrentDay()
        {
            if (currentDayProvider != null)
            {
                int day = currentDayProvider.Invoke();
                if (day > 0)
                {
                    manualDayCounter = day;
                    return day;
                }
            }

            return manualDayCounter;
        }

        public void AdvanceManualDayCounter()
        {
            manualDayCounter++;
        }

        private void NotifyTransactionCompleted(CCS_UpkeepTransactionResult result)
        {
            UpkeepTransactionCompleted?.Invoke(result);
        }

        private static CCS_UpkeepTransactionResult Success(
            string entryId,
            string targetId,
            string upkeepDefinitionId,
            int amount,
            CCS_UpkeepPaymentSource paymentSource,
            CCS_UpkeepState entryState,
            string message)
        {
            return CCS_UpkeepTransactionResult.Success(
                entryId,
                targetId,
                upkeepDefinitionId,
                amount,
                paymentSource,
                entryState,
                message);
        }

        private static CCS_UpkeepTransactionResult Failure(
            CCS_UpkeepTransactionResultType resultType,
            string entryId,
            string targetId,
            string upkeepDefinitionId,
            CCS_UpkeepState entryState,
            string message)
        {
            return CCS_UpkeepTransactionResult.Failure(
                resultType,
                entryId,
                targetId,
                upkeepDefinitionId,
                entryState,
                message);
        }
    }
}
