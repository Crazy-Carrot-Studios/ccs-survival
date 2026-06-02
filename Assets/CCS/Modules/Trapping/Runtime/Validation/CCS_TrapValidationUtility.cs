using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Survival;

namespace CCS.Modules.Trapping
{
    public static class CCS_TrapValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_TrapProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Trap profile is null.");
            }

            if (!profile.EnableTrapping)
            {
                return CCS_SurvivalValidationResult.Pass("Trapping disabled by profile (valid).");
            }

            if (profile.TrapDefinitions == null || profile.TrapDefinitions.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Trap profile has no trap definitions.");
            }

            HashSet<string> trapIds = new HashSet<string>();
            for (int index = 0; index < profile.TrapDefinitions.Count; index++)
            {
                CCS_TrapDefinition definition = profile.TrapDefinitions[index];
                CCS_SurvivalValidationResult definitionResult = ValidateTrapDefinition(definition);
                if (!definitionResult.IsSuccess)
                {
                    return definitionResult;
                }

                if (!trapIds.Add(definition.TrapDefinitionId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate trap definition id '{definition.TrapDefinitionId}'.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Trap profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateTrapDefinition(CCS_TrapDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Trap definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.TrapDefinitionId))
            {
                return CCS_SurvivalValidationResult.Fail("Trap definition id is required.");
            }

            if (definition.PlaceableItem == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Trap '{definition.TrapDefinitionId}' is missing placeable item.");
            }

            if (definition.CapturedWildlifeDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Trap '{definition.TrapDefinitionId}' is missing captured wildlife definition.");
            }

            bool hasHarvestData = definition.CapturedWildlifeDefinition.HarvestDefinition != null
                || (definition.CapturedWildlifeDefinition.HarvestDrops != null
                    && definition.CapturedWildlifeDefinition.HarvestDrops.Count > 0);
            if (!hasHarvestData)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Trap '{definition.TrapDefinitionId}' wildlife has no harvest drops configured.");
            }

            if (!CCS_ItemGameplayUtility.IsPlaceableTrapItem(definition.PlaceableItem))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Trap item '{definition.PlaceableItem.ItemId}' is not classified as placeable.");
            }

            return CCS_SurvivalValidationResult.Pass($"Trap definition '{definition.TrapDefinitionId}' validated.");
        }
    }
}
