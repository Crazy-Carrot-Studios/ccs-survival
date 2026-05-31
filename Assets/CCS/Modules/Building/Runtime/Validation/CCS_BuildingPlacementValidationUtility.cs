using System.Collections.Generic;
using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingPlacementValidationUtility
// CATEGORY: Modules / Building / Runtime / Validation
// PURPOSE: Placement validation and inventory cost consumption helpers.
// PLACEMENT: Used by CCS_BuildingPlacementService.TryPlaceCurrentPiece().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Rolls back consumed items if consumption fails midway.
// =============================================================================

namespace CCS.Modules.Building
{
    public static class CCS_BuildingPlacementValidationUtility
    {
        #region Public Methods

        public static CCS_BuildingPlacementValidationResult ValidatePlacementAttempt(
            CCS_BuildingPlacementService placementService,
            CCS_BuildingService buildingService,
            CCS_PlayerInventoryService inventoryService,
            CCS_BuildingPlacementState placementState)
        {
            if (placementService == null || !placementService.IsInitialized)
            {
                return CCS_BuildingPlacementValidationResult.Failed("Placement service is not initialized.");
            }

            if (buildingService == null || !buildingService.IsInitialized)
            {
                return CCS_BuildingPlacementValidationResult.Failed("Building service is not initialized.");
            }

            if (placementState == null || !placementState.IsPlacementModeActive)
            {
                return CCS_BuildingPlacementValidationResult.Failed("Placement mode is not active.");
            }

            if (string.IsNullOrWhiteSpace(placementState.ActivePieceId))
            {
                return CCS_BuildingPlacementValidationResult.Failed("No active building definition selected.");
            }

            if (!buildingService.TryGetDefinition(
                    placementState.ActivePieceId,
                    out CCS_BuildingPieceDefinition definition))
            {
                return CCS_BuildingPlacementValidationResult.Failed("Building definition was not found.");
            }

            if (!ValidatePreviewPosition(placementState.PreviewPosition))
            {
                return CCS_BuildingPlacementValidationResult.Failed("Placement position is invalid.");
            }

            if (!placementState.IsPlacementValid)
            {
                return CCS_BuildingPlacementValidationResult.Failed("Placement preview is not valid.");
            }

            if (definition.RequiresSnapPoint && !placementState.ActiveSnapMatch.HasMatch)
            {
                return CCS_BuildingPlacementValidationResult.Failed("Required snap point is missing.");
            }

            if (placementState.ActiveSnapMatch.HasMatch
                && !ValidateSnapMatchAvailable(buildingService, placementState.ActiveSnapMatch))
            {
                return CCS_BuildingPlacementValidationResult.Failed("Target snap point is occupied or unavailable.");
            }

            return ValidateInventoryCosts(inventoryService, definition);
        }

        private static bool ValidateSnapMatchAvailable(
            CCS_BuildingService buildingService,
            CCS_BuildingSnapMatch snapMatch)
        {
            if (buildingService == null || !snapMatch.HasMatch)
            {
                return false;
            }

            IReadOnlyList<CCS_BuildingInstance> placedInstances = buildingService.GetPlacedInstances();
            for (int index = 0; index < placedInstances.Count; index++)
            {
                CCS_BuildingInstance instance = placedInstances[index];
                if (instance.InstanceId != snapMatch.TargetInstanceId)
                {
                    continue;
                }

                if (!instance.TryGetRuntimeSnapPoint(snapMatch.TargetSnapPointId, out CCS_BuildingRuntimeSnapPoint targetSnapPoint))
                {
                    return false;
                }

                return !targetSnapPoint.IsOccupied;
            }

            return false;
        }

        public static CCS_BuildingPlacementValidationResult ValidateInventoryCosts(
            CCS_PlayerInventoryService inventoryService,
            CCS_BuildingPieceDefinition definition)
        {
            if (definition == null)
            {
                return CCS_BuildingPlacementValidationResult.Failed("Building definition is null.");
            }

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                return CCS_BuildingPlacementValidationResult.Failed("Inventory service is unavailable.");
            }

            IReadOnlyList<CCS_BuildingCostEntry> costEntries = definition.BuildCostEntries;
            if (costEntries == null || costEntries.Count == 0)
            {
                return CCS_BuildingPlacementValidationResult.Passed;
            }

            for (int index = 0; index < costEntries.Count; index++)
            {
                CCS_BuildingCostEntry costEntry = costEntries[index];
                if (costEntry == null || costEntry.ItemDefinition == null || costEntry.Quantity <= 0)
                {
                    continue;
                }

                if (inventoryService.HasItem(costEntry.ItemDefinition, costEntry.Quantity))
                {
                    continue;
                }

                string itemName = ResolveItemDisplayName(costEntry.ItemDefinition);
                return CCS_BuildingPlacementValidationResult.Failed(
                    $"Missing required item '{itemName}'.",
                    itemName);
            }

            return CCS_BuildingPlacementValidationResult.Passed;
        }

        public static bool TryConsumeBuildCosts(
            CCS_PlayerInventoryService inventoryService,
            CCS_BuildingPieceDefinition definition,
            out CCS_BuildingPlacementValidationResult result)
        {
            result = CCS_BuildingPlacementValidationResult.Passed;

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                result = CCS_BuildingPlacementValidationResult.Failed("Inventory service is unavailable.");
                return false;
            }

            if (definition == null)
            {
                result = CCS_BuildingPlacementValidationResult.Failed("Building definition is null.");
                return false;
            }

            IReadOnlyList<CCS_BuildingCostEntry> costEntries = definition.BuildCostEntries;
            if (costEntries == null || costEntries.Count == 0)
            {
                return true;
            }

            List<(CCS_ItemDefinition itemDefinition, int quantity)> consumedEntries =
                new List<(CCS_ItemDefinition, int)>();

            for (int index = 0; index < costEntries.Count; index++)
            {
                CCS_BuildingCostEntry costEntry = costEntries[index];
                if (costEntry == null || costEntry.ItemDefinition == null || costEntry.Quantity <= 0)
                {
                    continue;
                }

                int removed = inventoryService.RemoveItem(costEntry.ItemDefinition, costEntry.Quantity);
                if (removed < costEntry.Quantity)
                {
                    RestoreConsumedEntries(inventoryService, consumedEntries);
                    string itemName = ResolveItemDisplayName(costEntry.ItemDefinition);
                    result = CCS_BuildingPlacementValidationResult.Failed(
                        $"Failed to consume '{itemName}'.",
                        itemName);
                    return false;
                }

                consumedEntries.Add((costEntry.ItemDefinition, removed));
            }

            return true;
        }

        public static void RestoreBuildCosts(
            CCS_PlayerInventoryService inventoryService,
            CCS_BuildingPieceDefinition definition)
        {
            if (inventoryService == null
                || !inventoryService.IsInitialized
                || definition == null)
            {
                return;
            }

            IReadOnlyList<CCS_BuildingCostEntry> costEntries = definition.BuildCostEntries;
            if (costEntries == null || costEntries.Count == 0)
            {
                return;
            }

            for (int index = 0; index < costEntries.Count; index++)
            {
                CCS_BuildingCostEntry costEntry = costEntries[index];
                if (costEntry == null || costEntry.ItemDefinition == null || costEntry.Quantity <= 0)
                {
                    continue;
                }

                inventoryService.AddItem(costEntry.ItemDefinition, costEntry.Quantity);
            }
        }

        public static string BuildMissingItemNotification(CCS_BuildingPlacementValidationResult result)
        {
            if (result.Success)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(result.MissingItemDisplayName))
            {
                return $"Missing {result.MissingItemDisplayName}";
            }

            return result.FailureReason;
        }

        public static string BuildPlacedPieceNotification(CCS_BuildingPieceDefinition definition)
        {
            if (definition == null)
            {
                return "Placed Structure";
            }

            return $"Placed {CCS_BuildingValidationUtility.FormatPieceTypeLabel(definition.BuildingPieceType)}";
        }

        #endregion

        #region Private Methods

        private static bool ValidatePreviewPosition(Vector3 position)
        {
            return !float.IsNaN(position.x)
                && !float.IsNaN(position.y)
                && !float.IsNaN(position.z);
        }

        private static string ResolveItemDisplayName(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                return "Item";
            }

            if (!string.IsNullOrWhiteSpace(itemDefinition.DisplayName))
            {
                string displayName = itemDefinition.DisplayName;
                if (displayName.StartsWith("Test ", System.StringComparison.Ordinal))
                {
                    return displayName.Substring(5);
                }

                return displayName;
            }

            return "Item";
        }

        private static void RestoreConsumedEntries(
            CCS_PlayerInventoryService inventoryService,
            List<(CCS_ItemDefinition itemDefinition, int quantity)> consumedEntries)
        {
            for (int index = 0; index < consumedEntries.Count; index++)
            {
                (CCS_ItemDefinition itemDefinition, int quantity) entry = consumedEntries[index];
                inventoryService.AddItem(entry.itemDefinition, entry.quantity);
            }
        }

        #endregion
    }
}
