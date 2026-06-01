using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FishingService
// CATEGORY: Modules / Fishing / Runtime / Services
// PURPOSE: Registers fishing spots, rolls catch tables, and grants inventory rewards.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration from fishing profile.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Foundation only. No minigame, casting animation, or line simulation.
// =============================================================================

namespace CCS.Modules.Fishing
{
    public sealed class CCS_FishingService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_FishingService]";

        #region Variables

        private readonly HashSet<CCS_FishingSpot> registeredSpots = new HashSet<CCS_FishingSpot>();
        private readonly HashSet<string> registeredSpotIds = new HashSet<string>();
        private CCS_FishingProfile activeProfile;
        private CCS_PlayerInventoryService inventoryService;
        private bool isInitialized;
        private string lastResultMessage = string.Empty;

        #endregion

        #region Events

        public event FishingAttemptedHandler FishingAttempted;
        public event FishingCatchGrantedHandler FishingCatchGranted;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_FishingProfile ActiveProfile => activeProfile;

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

        public void InitializeFromProfile(CCS_FishingProfile profile)
        {
            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_FishingValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            profile.BuildItemLookup();
            activeProfile = profile;
            isInitialized = true;
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            inventoryService = service;
        }

        public void RegisterSpot(CCS_FishingSpot fishingSpot)
        {
            if (fishingSpot == null || string.IsNullOrWhiteSpace(fishingSpot.SpotId))
            {
                return;
            }

            if (!registeredSpotIds.Add(fishingSpot.SpotId))
            {
                Debug.LogWarning($"{LogPrefix} Duplicate fishing spot id ignored: {fishingSpot.SpotId}");
                return;
            }

            registeredSpots.Add(fishingSpot);
        }

        public void UnregisterSpot(CCS_FishingSpot fishingSpot)
        {
            if (fishingSpot == null)
            {
                return;
            }

            registeredSpots.Remove(fishingSpot);
            if (!string.IsNullOrWhiteSpace(fishingSpot.SpotId))
            {
                registeredSpotIds.Remove(fishingSpot.SpotId);
            }
        }

        public CCS_FishingResult TryFish(CCS_FishingRequest request)
        {
            if (!isInitialized)
            {
                return Failure(CCS_FishingResultType.ServiceUnavailable, "Fishing service is unavailable.");
            }

            if (request == null || request.FishingSpot == null)
            {
                return Failure(CCS_FishingResultType.TargetUnavailable, "Fishing spot is unavailable.");
            }

            CCS_FishingSpot fishingSpot = request.FishingSpot;
            CCS_FishingSpotDefinition spotDefinition = fishingSpot.SpotDefinition;
            if (spotDefinition == null || !spotDefinition.SupportsFishing)
            {
                return Failure(CCS_FishingResultType.NoWater, "Target is not a fishable water source.");
            }

            if (!fishingSpot.CanFish())
            {
                return Failure(CCS_FishingResultType.TargetUnavailable, "Fishing spot is unavailable.");
            }

            if (!CCS_FishingValidationUtility.IsFishingPoleItemDefinition(request.FishingPoleItem))
            {
                return Failure(CCS_FishingResultType.Failed, "A fishing pole is required.");
            }

            CCS_FishingBaitRequirement baitRequirement = spotDefinition.BaitRequirement;
            if (baitRequirement != null && baitRequirement.requireBait)
            {
                if (!HasRequiredBait(baitRequirement))
                {
                    CCS_FishingResult noBait = Failure(CCS_FishingResultType.NoBait, "Bait is required to fish here.");
                    RaiseAttempt(fishingSpot.SpotId, noBait);
                    return noBait;
                }

                if (baitRequirement.consumeBaitOnAttempt)
                {
                    ConsumeBait(baitRequirement);
                }
            }

            CCS_FishingCatchDefinition selectedEntry = RollCatch(spotDefinition);
            if (selectedEntry == null)
            {
                CCS_FishingResult failedRoll = Failure(CCS_FishingResultType.Failed, "Fishing attempt failed.");
                RaiseAttempt(fishingSpot.SpotId, failedRoll);
                return failedRoll;
            }

            if (selectedEntry.catchKind == CCS_FishingCatchKind.Nothing)
            {
                CCS_FishingResult nothingResult = new CCS_FishingResult(
                    CCS_FishingResultType.NothingCaught,
                    "Nothing caught this time.",
                    false);
                lastResultMessage = nothingResult.Message;
                RaiseAttempt(fishingSpot.SpotId, nothingResult);
                return nothingResult;
            }

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                return Failure(CCS_FishingResultType.ServiceUnavailable, "Inventory service is unavailable.");
            }

            if (activeProfile == null
                || !activeProfile.TryResolveItem(selectedEntry.itemDefinitionId, out CCS_ItemDefinition itemDefinition)
                || itemDefinition == null)
            {
                return Failure(
                    CCS_FishingResultType.Failed,
                    $"Catch item {selectedEntry.itemDefinitionId} could not be resolved.");
            }

            int quantity = Mathf.Max(1, selectedEntry.quantity);
            int added = inventoryService.AddItem(itemDefinition, quantity);
            if (added <= 0)
            {
                CCS_FishingResult inventoryFailure = Failure(
                    CCS_FishingResultType.Failed,
                    "Inventory could not accept the catch.");
                RaiseAttempt(fishingSpot.SpotId, inventoryFailure);
                return inventoryFailure;
            }

            CCS_FishingResultType successType = MapCatchKindToResultType(selectedEntry.catchKind);
            string successMessage = BuildSuccessMessage(selectedEntry.catchKind, itemDefinition.DisplayName, added);
            CCS_FishingResult successResult = new CCS_FishingResult(
                successType,
                successMessage,
                true,
                itemDefinition.ItemId,
                added);
            lastResultMessage = successMessage;
            RaiseAttempt(fishingSpot.SpotId, successResult);
            FishingCatchGranted?.Invoke(
                new CCS_FishingEventArgs(
                    fishingSpot.SpotId,
                    successType,
                    successMessage,
                    itemDefinition.ItemId,
                    added));
            return successResult;
        }

        public CCS_FishingSnapshot CreateSnapshot()
        {
            return new CCS_FishingSnapshot(isInitialized, registeredSpots.Count, lastResultMessage);
        }

        #endregion

        #region Private Methods

        private static CCS_FishingResultType MapCatchKindToResultType(CCS_FishingCatchKind catchKind)
        {
            switch (catchKind)
            {
                case CCS_FishingCatchKind.SmallFish:
                    return CCS_FishingResultType.SmallFishCaught;
                case CCS_FishingCatchKind.Junk:
                    return CCS_FishingResultType.JunkCaught;
                case CCS_FishingCatchKind.Fish:
                default:
                    return CCS_FishingResultType.FishCaught;
            }
        }

        private static string BuildSuccessMessage(CCS_FishingCatchKind catchKind, string displayName, int quantity)
        {
            string label = string.IsNullOrWhiteSpace(displayName) ? "item" : displayName;
            switch (catchKind)
            {
                case CCS_FishingCatchKind.Junk:
                    return $"Caught junk: {label} x{quantity}.";
                case CCS_FishingCatchKind.SmallFish:
                    return $"Caught small fish: {label} x{quantity}.";
                default:
                    return $"Caught fish: {label} x{quantity}.";
            }
        }

        private CCS_FishingCatchDefinition RollCatch(CCS_FishingSpotDefinition spotDefinition)
        {
            CCS_FishingCatchDefinition[] catchTable = spotDefinition.CatchTable;
            if (catchTable == null || catchTable.Length == 0)
            {
                catchTable = activeProfile != null ? activeProfile.DefaultCatchTable : null;
            }

            if (catchTable == null || catchTable.Length == 0)
            {
                return null;
            }

            int totalWeight = 0;
            for (int index = 0; index < catchTable.Length; index++)
            {
                CCS_FishingCatchDefinition entry = catchTable[index];
                if (entry != null && entry.weight > 0)
                {
                    totalWeight += entry.weight;
                }
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            int roll = Random.Range(0, totalWeight);
            int cumulative = 0;
            for (int index = 0; index < catchTable.Length; index++)
            {
                CCS_FishingCatchDefinition entry = catchTable[index];
                if (entry == null || entry.weight <= 0)
                {
                    continue;
                }

                cumulative += entry.weight;
                if (roll < cumulative)
                {
                    return entry;
                }
            }

            return catchTable[catchTable.Length - 1];
        }

        private bool HasRequiredBait(CCS_FishingBaitRequirement baitRequirement)
        {
            if (inventoryService == null || !inventoryService.IsInitialized || baitRequirement == null)
            {
                return false;
            }

            if (activeProfile == null
                || !activeProfile.TryResolveItem(baitRequirement.baitItemDefinitionId, out CCS_ItemDefinition baitItem)
                || baitItem == null)
            {
                return false;
            }

            return inventoryService.HasItem(baitItem, Mathf.Max(1, baitRequirement.requiredQuantity));
        }

        private void ConsumeBait(CCS_FishingBaitRequirement baitRequirement)
        {
            if (inventoryService == null
                || !inventoryService.IsInitialized
                || activeProfile == null
                || !activeProfile.TryResolveItem(baitRequirement.baitItemDefinitionId, out CCS_ItemDefinition baitItem))
            {
                return;
            }

            inventoryService.RemoveItem(baitItem, Mathf.Max(1, baitRequirement.requiredQuantity));
        }

        private void RaiseAttempt(string spotId, CCS_FishingResult result)
        {
            FishingAttempted?.Invoke(
                new CCS_FishingEventArgs(
                    spotId,
                    result.ResultType,
                    result.Message,
                    result.GrantedItemId,
                    result.GrantedQuantity));
        }

        private static CCS_FishingResult Failure(CCS_FishingResultType resultType, string message)
        {
            return new CCS_FishingResult(resultType, message, false);
        }

        #endregion
    }
}
