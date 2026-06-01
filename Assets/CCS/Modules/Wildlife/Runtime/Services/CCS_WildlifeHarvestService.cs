using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Modules.WorldResources;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeHarvestService
// CATEGORY: Modules / Wildlife / Runtime / Services
// PURPOSE: Validates wildlife harvest attempts, generates drops, and raises harvest events.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration from wildlife profile.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No AI, combat, or spawning in 0.9.3 foundation.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public sealed class CCS_WildlifeHarvestService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_WildlifeHarvestService]";

        #region Variables

        private CCS_WildlifeProfile activeProfile;
        private bool isInitialized;

        #endregion

        #region Events

        public event WildlifeHarvestStartedHandler WildlifeHarvestStarted;
        public event WildlifeHarvestCompletedHandler WildlifeHarvestCompleted;
        public event WildlifeHarvestFailedHandler WildlifeHarvestFailed;
        public event WildlifeDepletedHandler WildlifeDepleted;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_WildlifeProfile ActiveProfile => activeProfile;

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

        public void InitializeFromProfile(CCS_WildlifeProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_WildlifeValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            isInitialized = true;
        }

        public bool CanHarvest(CCS_WildlifeHarvestRequest request)
        {
            return ValidateHarvestRequest(request).IsSuccess;
        }

        public CCS_WildlifeHarvestResult TryHarvest(
            CCS_WildlifeHarvestRequest request,
            CCS_PlayerInventoryService inventoryService = null)
        {
            if (!EnsureInitialized())
            {
                return FailHarvest(request, "Wildlife harvest service is not initialized.");
            }

            if (activeProfile != null && !activeProfile.EnableCarcassHarvesting)
            {
                return FailHarvest(request, "Wildlife carcass harvesting is disabled.");
            }

            CCS_SurvivalValidationResult validation = ValidateHarvestRequest(request);
            if (!validation.IsSuccess)
            {
                return FailHarvest(request, validation.Message);
            }

            CCS_WildlifeDefinition wildlifeDefinition = request.WildlifeDefinition;
            CCS_WildlifeState wildlifeState = request.WildlifeState;

            RaiseWildlifeHarvestStarted(wildlifeDefinition, wildlifeState, request.InstanceKey);

            List<CCS_WildlifeHarvestedItemDrop> drops = GenerateDrops(wildlifeDefinition);
            if (drops.Count == 0)
            {
                return FailHarvest(request, "Wildlife definition produced no valid drops.");
            }

            int itemsAddedToInventory = 0;
            if (inventoryService != null && inventoryService.IsInitialized)
            {
                for (int index = 0; index < drops.Count; index++)
                {
                    CCS_WildlifeHarvestedItemDrop drop = drops[index];
                    if (drop?.ItemDefinition == null || drop.Quantity <= 0)
                    {
                        continue;
                    }

                    if (!inventoryService.CanAdd(drop.ItemDefinition, drop.Quantity))
                    {
                        return FailHarvest(request, "Inventory cannot hold harvested items.");
                    }
                }

                for (int index = 0; index < drops.Count; index++)
                {
                    CCS_WildlifeHarvestedItemDrop drop = drops[index];
                    if (drop?.ItemDefinition == null || drop.Quantity <= 0)
                    {
                        continue;
                    }

                    int added = inventoryService.AddItem(drop.ItemDefinition, drop.Quantity);
                    if (added < drop.Quantity)
                    {
                        return FailHarvest(request, "Inventory cannot hold harvested items.");
                    }

                    itemsAddedToInventory += added;
                }
            }
            else if (inventoryService != null)
            {
                return FailHarvest(request, "Inventory service is not initialized.");
            }

            wildlifeState.ConsumeHarvest("Wildlife harvest completed.");

            CCS_WildlifeHarvestResult success = CCS_WildlifeHarvestResult.Success(
                drops,
                itemsAddedToInventory,
                "Wildlife harvest completed.");

            RaiseWildlifeHarvestCompleted(wildlifeDefinition, wildlifeState, request.InstanceKey, drops);

            if (wildlifeState.IsDepleted)
            {
                RaiseWildlifeDepleted(wildlifeDefinition, wildlifeState, request.InstanceKey);
            }

            return success;
        }

        #endregion

        #region Private Methods

        private bool EnsureInitialized()
        {
            return isInitialized;
        }

        private CCS_SurvivalValidationResult ValidateHarvestRequest(CCS_WildlifeHarvestRequest request)
        {
            if (request == null)
            {
                return CCS_SurvivalValidationResult.Fail("Wildlife harvest request is null.");
            }

            CCS_WildlifeDefinition wildlifeDefinition = request.WildlifeDefinition;
            CCS_WildlifeState wildlifeState = request.WildlifeState;

            CCS_SurvivalValidationResult definitionValidation =
                CCS_WildlifeValidationUtility.ValidateWildlifeDefinition(wildlifeDefinition);

            if (!definitionValidation.IsSuccess)
            {
                return definitionValidation;
            }

            if (wildlifeState == null)
            {
                return CCS_SurvivalValidationResult.Fail("Wildlife state is null.");
            }

            if (wildlifeState.IsDepleted)
            {
                return CCS_SurvivalValidationResult.Fail("Wildlife carcass is depleted.");
            }

            if (!ValidateToolRequirement(
                    wildlifeDefinition.HarvestToolRequirement,
                    request.EquippedToolType))
            {
                return CCS_SurvivalValidationResult.Fail("Required tool is not equipped.");
            }

            return CCS_SurvivalValidationResult.Pass("Wildlife harvest request validated.");
        }

        private static bool ValidateToolRequirement(
            CCS_RequiredToolType requiredToolType,
            CCS_RequiredToolType equippedToolType)
        {
            if (requiredToolType == CCS_RequiredToolType.None)
            {
                return true;
            }

            return requiredToolType == equippedToolType;
        }

        private static List<CCS_WildlifeHarvestedItemDrop> GenerateDrops(CCS_WildlifeDefinition wildlifeDefinition)
        {
            List<CCS_WildlifeHarvestedItemDrop> drops = new List<CCS_WildlifeHarvestedItemDrop>();
            IReadOnlyList<CCS_WildlifeHarvestDropDefinition> dropDefinitions = wildlifeDefinition.HarvestDrops;

            for (int index = 0; index < dropDefinitions.Count; index++)
            {
                CCS_WildlifeHarvestDropDefinition dropDefinition = dropDefinitions[index];
                if (dropDefinition?.ItemDefinition == null)
                {
                    continue;
                }

                int minQuantity = dropDefinition.MinQuantity;
                int maxQuantity = dropDefinition.MaxQuantity;
                if (minQuantity <= 0 || maxQuantity <= 0)
                {
                    continue;
                }

                if (maxQuantity < minQuantity)
                {
                    int swap = minQuantity;
                    minQuantity = maxQuantity;
                    maxQuantity = swap;
                }

                int quantity = minQuantity == maxQuantity
                    ? minQuantity
                    : Random.Range(minQuantity, maxQuantity + 1);

                drops.Add(new CCS_WildlifeHarvestedItemDrop(dropDefinition.ItemDefinition, quantity));
            }

            return drops;
        }

        private CCS_WildlifeHarvestResult FailHarvest(CCS_WildlifeHarvestRequest request, string message)
        {
            CCS_WildlifeHarvestResult failure = CCS_WildlifeHarvestResult.Failure(message);
            if (request != null)
            {
                RaiseWildlifeHarvestFailed(
                    request.WildlifeDefinition,
                    request.WildlifeState,
                    request.InstanceKey,
                    message);
            }

            return failure;
        }

        private void RaiseWildlifeHarvestStarted(
            CCS_WildlifeDefinition wildlifeDefinition,
            CCS_WildlifeState wildlifeState,
            string instanceKey)
        {
            WildlifeHarvestStarted?.Invoke(
                new CCS_WildlifeEventArgs(wildlifeDefinition, wildlifeState, instanceKey, message: "Wildlife harvest started."));
        }

        private void RaiseWildlifeHarvestCompleted(
            CCS_WildlifeDefinition wildlifeDefinition,
            CCS_WildlifeState wildlifeState,
            string instanceKey,
            IReadOnlyList<CCS_WildlifeHarvestedItemDrop> drops)
        {
            WildlifeHarvestCompleted?.Invoke(
                new CCS_WildlifeEventArgs(wildlifeDefinition, wildlifeState, instanceKey, drops, "Wildlife harvest completed."));
        }

        private void RaiseWildlifeHarvestFailed(
            CCS_WildlifeDefinition wildlifeDefinition,
            CCS_WildlifeState wildlifeState,
            string instanceKey,
            string message)
        {
            WildlifeHarvestFailed?.Invoke(
                new CCS_WildlifeEventArgs(wildlifeDefinition, wildlifeState, instanceKey, message: message));
        }

        private void RaiseWildlifeDepleted(
            CCS_WildlifeDefinition wildlifeDefinition,
            CCS_WildlifeState wildlifeState,
            string instanceKey)
        {
            WildlifeDepleted?.Invoke(
                new CCS_WildlifeEventArgs(wildlifeDefinition, wildlifeState, instanceKey, message: "Wildlife depleted."));
        }

        #endregion
    }
}
