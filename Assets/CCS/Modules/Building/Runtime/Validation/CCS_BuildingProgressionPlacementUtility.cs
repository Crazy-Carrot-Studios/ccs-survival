using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingProgressionPlacementUtility
// CATEGORY: Modules / Building / Runtime / Validation
// PURPOSE: Validates primitive building placement rules against placed instances.
// PLACEMENT: Used by CCS_BuildingRecipeService before resource consumption.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Walls, doorways, and floors require foundation. Roofs require wall/doorway snap.
// =============================================================================

namespace CCS.Modules.Building
{
    public static class CCS_BuildingProgressionPlacementUtility
    {
        #region Public Methods

        public static bool TryValidatePlacementRules(
            CCS_BuildingService buildingService,
            CCS_BuildingPieceDefinition definition,
            CCS_BuildingRecipe recipe,
            CCS_BuildingPlacementState placementState,
            out string failureMessage)
        {
            failureMessage = string.Empty;

            if (definition == null || recipe == null)
            {
                failureMessage = "Building definition or recipe is missing.";
                return false;
            }

            CCS_BuildingRecipePlacementRules rules = recipe.PlacementRules;
            if (rules == null)
            {
                failureMessage = "Recipe placement rules are missing.";
                return false;
            }

            if (placementState == null || !placementState.IsPlacementValid)
            {
                failureMessage = "Placement preview is not valid.";
                return false;
            }

            if (rules.RequiresSnapPoint && !placementState.ActiveSnapMatch.HasMatch)
            {
                failureMessage = "Required snap point is missing.";
                return false;
            }

            if (rules.RequiresFoundationNearby
                && !HasFoundationNearby(buildingService, placementState.PreviewPosition, rules.FoundationSearchRadius))
            {
                failureMessage = "Placement requires a nearby foundation.";
                return false;
            }

            if (rules.RequiresWallOrDoorwaySupport
                && !HasWallOrDoorwaySnapSupport(buildingService, placementState.ActiveSnapMatch))
            {
                failureMessage = "Roof placement requires a supporting wall or doorway.";
                return false;
            }

            if (!rules.AllowsFreePlacement && !rules.RequiresSnapPoint && !rules.RequiresFoundationNearby)
            {
                failureMessage = "Placement rules rejected the current preview.";
                return false;
            }

            return true;
        }

        public static int CountPlacedByCategory(
            CCS_BuildingService buildingService,
            CCS_BuildingPieceCategory category)
        {
            if (buildingService == null || !buildingService.IsInitialized)
            {
                return 0;
            }

            int count = 0;
            IReadOnlyList<CCS_BuildingInstance> instances = buildingService.GetPlacedInstances();
            for (int index = 0; index < instances.Count; index++)
            {
                CCS_BuildingInstance instance = instances[index];
                if (instance == null)
                {
                    continue;
                }

                if (!buildingService.TryGetDefinition(instance.PieceId, out CCS_BuildingPieceDefinition definition))
                {
                    continue;
                }

                if (definition.PieceCategory == category)
                {
                    count++;
                }
            }

            return count;
        }

        public static bool MeetsMinimumShelter(
            CCS_BuildingService buildingService,
            CCS_BuildingProgressionProfile progressionProfile)
        {
            if (buildingService == null
                || !buildingService.IsInitialized
                || progressionProfile == null)
            {
                return false;
            }

            int foundationCount = CountPlacedByCategory(buildingService, CCS_BuildingPieceCategory.Foundation);
            int wallCount = CountPlacedByCategory(buildingService, CCS_BuildingPieceCategory.Wall);
            int roofCount = CountPlacedByCategory(buildingService, CCS_BuildingPieceCategory.Roof);

            return foundationCount >= progressionProfile.MinimumFoundationCount
                && wallCount >= progressionProfile.MinimumWallCount
                && roofCount >= progressionProfile.MinimumRoofCount;
        }

        #endregion

        #region Private Methods

        private static bool HasFoundationNearby(
            CCS_BuildingService buildingService,
            Vector3 position,
            float searchRadius)
        {
            if (buildingService == null)
            {
                return false;
            }

            float radiusSquared = searchRadius * searchRadius;
            IReadOnlyList<CCS_BuildingInstance> instances = buildingService.GetPlacedInstances();
            for (int index = 0; index < instances.Count; index++)
            {
                CCS_BuildingInstance instance = instances[index];
                if (instance == null)
                {
                    continue;
                }

                if (!buildingService.TryGetDefinition(instance.PieceId, out CCS_BuildingPieceDefinition definition))
                {
                    continue;
                }

                if (definition.PieceCategory != CCS_BuildingPieceCategory.Foundation)
                {
                    continue;
                }

                if ((instance.Position - position).sqrMagnitude <= radiusSquared)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasWallOrDoorwaySnapSupport(
            CCS_BuildingService buildingService,
            CCS_BuildingSnapMatch snapMatch)
        {
            if (buildingService == null || !snapMatch.HasMatch)
            {
                return false;
            }

            IReadOnlyList<CCS_BuildingInstance> instances = buildingService.GetPlacedInstances();
            for (int index = 0; index < instances.Count; index++)
            {
                CCS_BuildingInstance instance = instances[index];
                if (instance == null || instance.InstanceId != snapMatch.TargetInstanceId)
                {
                    continue;
                }

                if (!buildingService.TryGetDefinition(instance.PieceId, out CCS_BuildingPieceDefinition definition))
                {
                    return false;
                }

                return definition.PieceCategory == CCS_BuildingPieceCategory.Wall
                    || definition.PieceCategory == CCS_BuildingPieceCategory.Doorway;
            }

            return false;
        }

        #endregion
    }
}
