using System;
using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CurrencyService
// CATEGORY: Modules / Economy / Runtime / Services
// PURPOSE: Generic wallet service for any currency type with optional inventory backing.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Trade Dollars may sync with stackable inventory item via backing definition.
// =============================================================================

namespace CCS.Modules.Economy
{
    public sealed class CCS_CurrencyService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_CurrencyService]";
        private const int MaxTransactionHistoryEntries = 32;

        #region Variables

        private readonly Dictionary<string, int> balances = new Dictionary<string, int>();
        private readonly Dictionary<string, CCS_CurrencyDefinition> currencyLookup =
            new Dictionary<string, CCS_CurrencyDefinition>();
        private readonly List<CCS_CurrencyTransaction> transactionHistory = new List<CCS_CurrencyTransaction>();

        private CCS_EconomyProfile activeProfile;
        private CCS_PlayerInventoryService inventoryService;
        private bool isInitialized;

        #endregion

        #region Events

        public event CurrencyBalanceChangedHandler CurrencyBalanceChanged;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_EconomyProfile ActiveProfile => activeProfile;

        public IReadOnlyList<CCS_CurrencyTransaction> TransactionHistory => transactionHistory;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_EconomyProfile profile)
        {
            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_EconomyValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            currencyLookup.Clear();
            balances.Clear();
            transactionHistory.Clear();

            CCS_CurrencyDefinition[] definitions = profile.CurrencyDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                RegisterCurrencyDefinition(definitions[index]);
            }

            activeProfile = profile;
            isInitialized = true;
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            inventoryService = service;
            SyncAllInventoryBackedBalances();
        }

        public void RegisterCurrencyDefinition(CCS_CurrencyDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.CurrencyId))
            {
                return;
            }

            currencyLookup[definition.CurrencyId] = definition;
            if (!balances.ContainsKey(definition.CurrencyId))
            {
                balances[definition.CurrencyId] = 0;
            }
        }

        public bool TryGetCurrencyDefinition(string currencyId, out CCS_CurrencyDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(currencyId))
            {
                return false;
            }

            return currencyLookup.TryGetValue(currencyId, out definition);
        }

        public int GetBalance(string currencyId)
        {
            if (string.IsNullOrWhiteSpace(currencyId))
            {
                return 0;
            }

            return balances.TryGetValue(currencyId, out int balance) ? balance : 0;
        }

        public bool CanAfford(string currencyId, int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            return GetBalance(currencyId) >= amount;
        }

        public CCS_CurrencyTransactionResult AddCurrency(string currencyId, int amount, string reason)
        {
            if (!isInitialized)
            {
                return CCS_CurrencyTransactionResult.Failure(
                    CCS_CurrencyTransactionResultType.UnknownFailure,
                    currencyId,
                    "Currency service is not initialized.");
            }

            if (!TryGetCurrencyDefinition(currencyId, out _))
            {
                return CCS_CurrencyTransactionResult.Failure(
                    CCS_CurrencyTransactionResultType.InvalidCurrency,
                    currencyId,
                    "Unknown currency.");
            }

            if (amount <= 0)
            {
                return CCS_CurrencyTransactionResult.Failure(
                    CCS_CurrencyTransactionResultType.InvalidAmount,
                    currencyId,
                    "Amount must be positive.");
            }

            int previous = GetBalance(currencyId);
            int updated = previous + amount;
            balances[currencyId] = updated;
            RecordTransaction(currencyId, amount, updated, reason);
            SyncInventoryBacking(currencyId);
            CurrencyBalanceChanged?.Invoke(currencyId, previous, updated, reason);
            LogDebug($"Added {amount} {currencyId} ({reason}). Balance={updated}.");
            return CCS_CurrencyTransactionResult.Success(currencyId, amount, updated, reason);
        }

        public CCS_CurrencyTransactionResult RemoveCurrency(string currencyId, int amount, string reason)
        {
            if (!isInitialized)
            {
                return CCS_CurrencyTransactionResult.Failure(
                    CCS_CurrencyTransactionResultType.UnknownFailure,
                    currencyId,
                    "Currency service is not initialized.");
            }

            if (!TryGetCurrencyDefinition(currencyId, out _))
            {
                return CCS_CurrencyTransactionResult.Failure(
                    CCS_CurrencyTransactionResultType.InvalidCurrency,
                    currencyId,
                    "Unknown currency.");
            }

            if (amount <= 0)
            {
                return CCS_CurrencyTransactionResult.Failure(
                    CCS_CurrencyTransactionResultType.InvalidAmount,
                    currencyId,
                    "Amount must be positive.");
            }

            int previous = GetBalance(currencyId);
            if (previous < amount)
            {
                return CCS_CurrencyTransactionResult.Failure(
                    CCS_CurrencyTransactionResultType.InsufficientFunds,
                    currencyId,
                    "Insufficient funds.");
            }

            int updated = previous - amount;
            balances[currencyId] = updated;
            RecordTransaction(currencyId, -amount, updated, reason);
            SyncInventoryBacking(currencyId);
            CurrencyBalanceChanged?.Invoke(currencyId, previous, updated, reason);
            LogDebug($"Removed {amount} {currencyId} ({reason}). Balance={updated}.");
            return CCS_CurrencyTransactionResult.Success(currencyId, amount, updated, reason);
        }

        public void SetBalance(string currencyId, int amount, string reason, bool syncInventoryBacking)
        {
            if (!TryGetCurrencyDefinition(currencyId, out _))
            {
                return;
            }

            int previous = GetBalance(currencyId);
            int clamped = amount < 0 ? 0 : amount;
            balances[currencyId] = clamped;
            RecordTransaction(currencyId, clamped - previous, clamped, reason);
            if (syncInventoryBacking)
            {
                SyncInventoryBacking(currencyId);
            }

            CurrencyBalanceChanged?.Invoke(currencyId, previous, clamped, reason);
        }

        public void SyncAllInventoryBackedBalances()
        {
            foreach (KeyValuePair<string, CCS_CurrencyDefinition> pair in currencyLookup)
            {
                if (pair.Value != null && pair.Value.HasInventoryBacking)
                {
                    SyncInventoryBacking(pair.Key);
                }
            }
        }

        public void ImportBalancesFromInventoryBacking()
        {
            foreach (KeyValuePair<string, CCS_CurrencyDefinition> pair in currencyLookup)
            {
                CCS_CurrencyDefinition definition = pair.Value;
                if (definition == null || !definition.HasInventoryBacking || inventoryService == null)
                {
                    continue;
                }

                int inventoryAmount = inventoryService.GetQuantity(definition.InventoryBackingItem);
                if (inventoryAmount > GetBalance(pair.Key))
                {
                    SetBalance(pair.Key, inventoryAmount, "Import from inventory backing.", false);
                }
            }
        }

        public CCS_CurrencyBalance[] CaptureBalances()
        {
            CCS_CurrencyBalance[] snapshot = new CCS_CurrencyBalance[balances.Count];
            int index = 0;
            foreach (KeyValuePair<string, int> pair in balances)
            {
                snapshot[index] = new CCS_CurrencyBalance(pair.Key, pair.Value);
                index++;
            }

            return snapshot;
        }

        public void RestoreBalances(CCS_CurrencyBalance[] savedBalances, bool syncInventoryBacking)
        {
            if (savedBalances == null)
            {
                return;
            }

            for (int index = 0; index < savedBalances.Length; index++)
            {
                CCS_CurrencyBalance balance = savedBalances[index];
                if (balance == null || string.IsNullOrWhiteSpace(balance.currencyId))
                {
                    continue;
                }

                SetBalance(balance.currencyId, balance.amount, "Restore from save.", false);
            }

            if (syncInventoryBacking)
            {
                SyncAllInventoryBackedBalances();
            }
        }

        #endregion

        #region Private Methods

        private void SyncInventoryBacking(string currencyId)
        {
            if (inventoryService == null
                || !inventoryService.IsInitialized
                || !TryGetCurrencyDefinition(currencyId, out CCS_CurrencyDefinition definition)
                || !definition.HasInventoryBacking)
            {
                return;
            }

            int walletBalance = GetBalance(currencyId);
            int inventoryQuantity = inventoryService.GetQuantity(definition.InventoryBackingItem);
            if (inventoryQuantity == walletBalance)
            {
                return;
            }

            if (inventoryQuantity > walletBalance)
            {
                inventoryService.RemoveItem(definition.InventoryBackingItem, inventoryQuantity - walletBalance);
            }
            else
            {
                inventoryService.AddItem(definition.InventoryBackingItem, walletBalance - inventoryQuantity);
            }
        }

        private void RecordTransaction(string currencyId, int deltaAmount, int balanceAfter, string reason)
        {
            transactionHistory.Add(
                new CCS_CurrencyTransaction(
                    currencyId,
                    deltaAmount,
                    balanceAfter,
                    reason,
                    DateTime.UtcNow.ToString("o")));

            while (transactionHistory.Count > MaxTransactionHistoryEntries)
            {
                transactionHistory.RemoveAt(0);
            }
        }

        private void LogDebug(string message)
        {
            if (activeProfile != null && activeProfile.EnableDebugLogging)
            {
                Debug.Log($"{LogPrefix} {message}");
            }
        }

        #endregion
    }
}
