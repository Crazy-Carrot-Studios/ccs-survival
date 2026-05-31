using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ResourceHarvestService
// CATEGORY: Modules / WorldResources / Runtime / Harvesting
// PURPOSE: Validates harvest attempts, generates drops, and raises harvest events.
// PLACEMENT: Used by CCS_HarvestableResource and future interaction wiring.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Inventory integration is optional. No UI or interaction visual references.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public sealed class CCS_ResourceHarvestService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_ResourceHarvestService]";

        #region Variables

        private CCS_WorldResourceProfile activeProfile;
        private bool isInitialized;

        #endregion

        #region Events

        public event HarvestStartedHandler HarvestStarted;
        public event HarvestCompletedHandler HarvestCompleted;
        public event HarvestFailedHandler HarvestFailed;
        public event ResourceDepletedHandler ResourceDepleted;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_WorldResourceProfile ActiveProfile => activeProfile;

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

        public void InitializeFromProfile(CCS_WorldResourceProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_WorldResourceValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            isInitialized = true;
        }

        public bool CanHarvest(CCS_HarvestRequest request)
        {
            return ValidateHarvestRequest(request).IsSuccess;
        }

        public CCS_HarvestResult TryHarvest(
            CCS_HarvestRequest request,
            CCS_PlayerInventoryService inventoryService = null)
        {
            if (!EnsureInitialized())
            {
                return FailHarvest(request, "Resource harvest service is not initialized.");
            }

            CCS_SurvivalValidationResult validation = ValidateHarvestRequest(request);
            if (!validation.IsSuccess)
            {
                return FailHarvest(request, validation.Message);
            }

            CCS_ResourceDefinition resourceDefinition = request.ResourceDefinition;
            CCS_ResourceNodeState nodeState = request.NodeState;

            RaiseHarvestStarted(resourceDefinition, nodeState, request.NodeKey);

            List<CCS_HarvestedItemDrop> drops = GenerateDrops(resourceDefinition);
            if (drops.Count == 0)
            {
                return FailHarvest(request, "Resource definition produced no valid drops.");
            }

            int itemsAddedToInventory = 0;
            if (inventoryService != null && inventoryService.IsInitialized)
            {
                for (int i = 0; i < drops.Count; i++)
                {
                    CCS_HarvestedItemDrop drop = drops[i];
                    if (drop?.ItemDefinition == null || drop.Quantity <= 0)
                    {
                        continue;
                    }

                    if (!inventoryService.CanAdd(drop.ItemDefinition, drop.Quantity))
                    {
                        return FailHarvest(request, "Inventory cannot hold harvested items.");
                    }
                }

                for (int i = 0; i < drops.Count; i++)
                {
                    CCS_HarvestedItemDrop drop = drops[i];
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

            nodeState.ConsumeHarvest();

            CCS_HarvestResult success = CCS_HarvestResult.Success(
                drops,
                itemsAddedToInventory,
                "Harvest completed.");

            RaiseHarvestCompleted(resourceDefinition, nodeState, request.NodeKey, drops);

            if (nodeState.IsDepleted)
            {
                RaiseResourceDepleted(resourceDefinition, nodeState, request.NodeKey);
            }

            return success;
        }

        #endregion

        #region Private Methods

        private bool EnsureInitialized()
        {
            return isInitialized;
        }

        private CCS_SurvivalValidationResult ValidateHarvestRequest(CCS_HarvestRequest request)
        {
            if (request == null)
            {
                return CCS_SurvivalValidationResult.Fail("Harvest request is null.");
            }

            CCS_ResourceDefinition resourceDefinition = request.ResourceDefinition;
            CCS_ResourceNodeState nodeState = request.NodeState;

            CCS_SurvivalValidationResult definitionValidation =
                CCS_WorldResourceValidationUtility.ValidateResourceDefinition(resourceDefinition);

            if (!definitionValidation.IsSuccess)
            {
                return definitionValidation;
            }

            if (nodeState == null)
            {
                return CCS_SurvivalValidationResult.Fail("Resource node state is null.");
            }

            if (nodeState.IsDepleted)
            {
                return CCS_SurvivalValidationResult.Fail("Resource node is depleted.");
            }

            if (!ValidateToolRequirement(resourceDefinition.RequiredToolType, request.EquippedToolType))
            {
                return CCS_SurvivalValidationResult.Fail("Required tool is not equipped.");
            }

            return CCS_SurvivalValidationResult.Pass("Harvest request validated.");
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

        private static List<CCS_HarvestedItemDrop> GenerateDrops(CCS_ResourceDefinition resourceDefinition)
        {
            List<CCS_HarvestedItemDrop> drops = new List<CCS_HarvestedItemDrop>();
            IReadOnlyList<CCS_ResourceDropDefinition> dropDefinitions = resourceDefinition.DropDefinitions;

            for (int i = 0; i < dropDefinitions.Count; i++)
            {
                CCS_ResourceDropDefinition dropDefinition = dropDefinitions[i];
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

                drops.Add(new CCS_HarvestedItemDrop(dropDefinition.ItemDefinition, quantity));
            }

            return drops;
        }

        private CCS_HarvestResult FailHarvest(CCS_HarvestRequest request, string message)
        {
            CCS_HarvestResult failure = CCS_HarvestResult.Failure(message);
            if (request != null)
            {
                RaiseHarvestFailed(request.ResourceDefinition, request.NodeState, request.NodeKey, message);
            }

            return failure;
        }

        private void RaiseHarvestStarted(
            CCS_ResourceDefinition resourceDefinition,
            CCS_ResourceNodeState nodeState,
            string nodeKey)
        {
            HarvestStarted?.Invoke(
                new CCS_ResourceEventArgs(resourceDefinition, nodeState, nodeKey, message: "Harvest started."));
        }

        private void RaiseHarvestCompleted(
            CCS_ResourceDefinition resourceDefinition,
            CCS_ResourceNodeState nodeState,
            string nodeKey,
            IReadOnlyList<CCS_HarvestedItemDrop> drops)
        {
            HarvestCompleted?.Invoke(
                new CCS_ResourceEventArgs(resourceDefinition, nodeState, nodeKey, drops, "Harvest completed."));
        }

        private void RaiseHarvestFailed(
            CCS_ResourceDefinition resourceDefinition,
            CCS_ResourceNodeState nodeState,
            string nodeKey,
            string message)
        {
            HarvestFailed?.Invoke(
                new CCS_ResourceEventArgs(resourceDefinition, nodeState, nodeKey, message: message));
        }

        private void RaiseResourceDepleted(
            CCS_ResourceDefinition resourceDefinition,
            CCS_ResourceNodeState nodeState,
            string nodeKey)
        {
            ResourceDepleted?.Invoke(
                new CCS_ResourceEventArgs(resourceDefinition, nodeState, nodeKey, message: "Resource depleted."));
        }

        #endregion
    }
}
