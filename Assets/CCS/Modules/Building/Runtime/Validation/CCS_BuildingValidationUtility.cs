using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_BuildingValidationUtility
// CATEGORY: Modules / Building / Runtime / Validation
// PURPOSE: Profile, definition, and HUD formatting helpers for runtime and editor checks.
// PLACEMENT: Used by building services initialization and editor validation pipeline.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Returns CCS_SurvivalValidationResult. No UnityEditor references.
// =============================================================================

namespace CCS.Modules.Building
{
    public static class CCS_BuildingValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_BuildingProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Building profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (!profile.AllowPlacement)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Building profile must enable placement for 0.8.1 placement foundation.");
            }

            if (profile.AllowDemolition || profile.AllowUpgrades)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Building profile demolition and upgrade flags must remain disabled for 0.8.1.");
            }

            return CCS_SurvivalValidationResult.Pass("Building profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateDefinition(CCS_BuildingPieceDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Building piece definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.PieceId))
            {
                return CCS_SurvivalValidationResult.Fail("Building piece definition requires a piece ID.");
            }

            if (string.IsNullOrWhiteSpace(definition.DisplayName))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Building piece '{definition.PieceId}' requires a display name.");
            }

            return CCS_SurvivalValidationResult.Pass($"Building piece '{definition.PieceId}' validated.");
        }

        public static string FormatBuildingDefinitionCountLine(int registeredDefinitionCount)
        {
            int count = registeredDefinitionCount < 0 ? 0 : registeredDefinitionCount;
            return $"Building Definitions: {count}";
        }

        public static string FormatPlacementHudLines(
            CCS_BuildingPlacementSnapshot placementSnapshot,
            int placedInstanceCount)
        {
            CCS_BuildingPlacementSnapshot snapshot = placementSnapshot.IsPlacementModeActive
                ? placementSnapshot
                : CCS_BuildingPlacementSnapshot.Empty;

            string placementLabel = snapshot.IsPlacementModeActive ? "Yes" : "No";
            string selectedPiece = snapshot.IsPlacementModeActive
                ? FormatPieceTypeLabel(snapshot.ActivePieceType)
                : "None";
            int placedCount = placedInstanceCount < 0 ? 0 : placedInstanceCount;

            return
                $"Placement Active: {placementLabel}\n" +
                $"Selected Piece: {selectedPiece}\n" +
                $"Placed Count: {placedCount}";
        }

        public static string FormatPieceTypeLabel(CCS_BuildingPieceType pieceType)
        {
            return pieceType.ToString();
        }

        #endregion
    }
}
