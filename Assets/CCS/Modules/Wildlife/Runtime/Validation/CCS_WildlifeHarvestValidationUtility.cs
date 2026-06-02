using CCS.Modules.Resources;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_WildlifeHarvestValidationUtility
// CATEGORY: Modules / Wildlife / Runtime / Validation
// PURPOSE: Validates wildlife harvest definitions, profiles, and catalog rules.
// PLACEMENT: Used by editor validators and harvest service initialization.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Milestone 1.3.2 frontier hunting foundation.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public static class CCS_WildlifeHarvestValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateHarvestProfile(CCS_WildlifeHarvestProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Wildlife harvest profile is null.");
            }

            if (profile.HarvestDefinitions == null || profile.HarvestDefinitions.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Wildlife harvest profile has no definitions.");
            }

            for (int index = 0; index < profile.HarvestDefinitions.Count; index++)
            {
                CCS_WildlifeHarvestDefinition definition = profile.HarvestDefinitions[index];
                CCS_SurvivalValidationResult entryResult = ValidateHarvestDefinition(definition);
                if (!entryResult.IsSuccess)
                {
                    return entryResult;
                }
            }

            return CCS_SurvivalValidationResult.Pass("Wildlife harvest profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateHarvestDefinition(CCS_WildlifeHarvestDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Wildlife harvest definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.HarvestDefinitionId))
            {
                return CCS_SurvivalValidationResult.Fail("Wildlife harvest definition is missing harvestDefinitionId.");
            }

            if (definition.WildlifeDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Harvest definition '{definition.HarvestDefinitionId}' is missing wildlifeDefinition.");
            }

            if (definition.ResourceSourceType != CCS_ResourceSourceType.Wildlife)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Harvest definition '{definition.HarvestDefinitionId}' must use ResourceSourceType.Wildlife.");
            }

            if (!HasAnyDrops(definition))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Harvest definition '{definition.HarvestDefinitionId}' has no skin or butcher drops.");
            }

            CCS_SurvivalValidationResult dropResult = ValidateDropList(
                definition.HarvestDefinitionId,
                "skin",
                definition.SkinDrops);
            if (!dropResult.IsSuccess)
            {
                return dropResult;
            }

            dropResult = ValidateDropList(
                definition.HarvestDefinitionId,
                "butcher",
                definition.ButcherDrops);
            if (!dropResult.IsSuccess)
            {
                return dropResult;
            }

            return CCS_SurvivalValidationResult.Pass(
                $"Wildlife harvest definition '{definition.HarvestDefinitionId}' validated.");
        }

        private static bool HasAnyDrops(CCS_WildlifeHarvestDefinition definition)
        {
            return HasValidDrop(definition.SkinDrops) || HasValidDrop(definition.ButcherDrops);
        }

        private static bool HasValidDrop(System.Collections.Generic.IReadOnlyList<CCS_WildlifeHarvestDropDefinition> drops)
        {
            if (drops == null)
            {
                return false;
            }

            for (int index = 0; index < drops.Count; index++)
            {
                if (drops[index]?.ItemDefinition != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static CCS_SurvivalValidationResult ValidateDropList(
            string harvestDefinitionId,
            string tableName,
            System.Collections.Generic.IReadOnlyList<CCS_WildlifeHarvestDropDefinition> drops)
        {
            if (drops == null)
            {
                return CCS_SurvivalValidationResult.Pass($"{tableName} drops omitted.");
            }

            for (int index = 0; index < drops.Count; index++)
            {
                CCS_WildlifeHarvestDropDefinition drop = drops[index];
                if (drop?.ItemDefinition == null)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Harvest '{harvestDefinitionId}' {tableName} drop at index {index} is missing item.");
                }

                if (drop.HarvestMethodType != CCS_HarvestMethodType.Skin
                    && drop.HarvestMethodType != CCS_HarvestMethodType.Butcher)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Harvest '{harvestDefinitionId}' drop '{drop.ItemDefinition.ItemId}' must use Skin or Butcher.");
                }

                if (drop.MinQuantity < 0 || drop.MaxQuantity < 0)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Harvest '{harvestDefinitionId}' drop '{drop.ItemDefinition.ItemId}' has negative quantities.");
                }
            }

            return CCS_SurvivalValidationResult.Pass($"{tableName} drops validated.");
        }
    }
}
