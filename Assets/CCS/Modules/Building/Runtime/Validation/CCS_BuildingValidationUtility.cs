using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_BuildingValidationUtility
// CATEGORY: Modules / Building / Runtime / Validation
// PURPOSE: Profile and definition validation helpers for runtime and editor checks.
// PLACEMENT: Used by building service initialization and editor validation pipeline.
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

            if (profile.AllowPlacement || profile.AllowDemolition || profile.AllowUpgrades)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Building profile feature flags must remain disabled for 0.8.0 architecture milestone.");
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

        #endregion
    }
}
