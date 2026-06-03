using CCS.Modules.Inventory;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_LandClaimValidationUtility
// CATEGORY: Modules / Land / Runtime / Validation
// PURPOSE: Shared profile/content validation for land bootstrap and runtime.
// PLACEMENT: Used by CCS_LandClaimService and editor validators.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.3.0 land ownership foundation.
// =============================================================================

namespace CCS.Modules.Land
{
    public static class CCS_LandClaimValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_LandClaimProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Land claim profile is null.");
            }

            CCS_LandClaimDefinition[] claims = profile.ClaimDefinitions;
            if (claims == null || claims.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Land claim profile has no claim definitions.");
            }

            for (int index = 0; index < claims.Length; index++)
            {
                CCS_LandClaimDefinition claim = claims[index];
                if (claim == null)
                {
                    return CCS_SurvivalValidationResult.Fail($"Land claim definition at index {index} is null.");
                }

                if (string.IsNullOrWhiteSpace(claim.ClaimDefinitionId))
                {
                    return CCS_SurvivalValidationResult.Fail($"Land claim at index {index} is missing claimDefinitionId.");
                }

                if (claim.ClaimDeedItem == null)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Land claim '{claim.ClaimDefinitionId}' is missing claim deed item.");
                }

                if (claim.ClaimRadius <= 0f)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Land claim '{claim.ClaimDefinitionId}' has invalid claim radius.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Land claim profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateItem(CCS_ItemDefinition item, string expectedItemId)
        {
            if (item == null)
            {
                return CCS_SurvivalValidationResult.Fail($"Missing item definition for '{expectedItemId}'.");
            }

            if (string.IsNullOrWhiteSpace(item.ItemId)
                || !string.Equals(item.ItemId, expectedItemId, System.StringComparison.OrdinalIgnoreCase))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Item '{item.name}' expected id '{expectedItemId}' but has '{item.ItemId}'.");
            }

            return CCS_SurvivalValidationResult.Pass($"Item '{expectedItemId}' validated.");
        }
    }
}
