using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_ResourceFrameworkValidationUtility
// CATEGORY: Modules / Resources / Runtime / Validation
// PURPOSE: Shared validation for resource source and harvest method metadata.
// PLACEMENT: Used by module validators and frontier bootstrap audits.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Runtime-safe. No UnityEditor references.
// =============================================================================

namespace CCS.Modules.Resources
{
    public static class CCS_ResourceFrameworkValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateSourceTypeAssigned(CCS_ResourceSourceType sourceType)
        {
            if (sourceType == CCS_ResourceSourceType.None)
            {
                return CCS_SurvivalValidationResult.Fail("Resource source type must be assigned.");
            }

            return CCS_SurvivalValidationResult.Pass("Resource source type is assigned.");
        }

        public static CCS_SurvivalValidationResult ValidateHarvestMethodAssigned(CCS_HarvestMethodType harvestMethod)
        {
            if (harvestMethod == CCS_HarvestMethodType.None)
            {
                return CCS_SurvivalValidationResult.Fail("Harvest method must be assigned.");
            }

            return CCS_SurvivalValidationResult.Pass("Harvest method is assigned.");
        }

        public static CCS_SurvivalValidationResult ValidateHarvestMethodToolRules(
            CCS_HarvestMethodType harvestMethod,
            CCS_ItemToolType explicitRequiredTool)
        {
            if (harvestMethod == CCS_HarvestMethodType.Fish)
            {
                return CCS_SurvivalValidationResult.Pass(
                    "Fish harvest method is reserved for future implementation.");
            }

            if (CCS_HarvestMethodToolRulesUtility.HarvestMethodRequiresMiningTool(harvestMethod))
            {
                CCS_ItemToolType effectiveTool = explicitRequiredTool != CCS_ItemToolType.None
                    ? explicitRequiredTool
                    : CCS_HarvestMethodToolRulesUtility.GetDefaultRequiredTool(harvestMethod);

                if (effectiveTool != CCS_ItemToolType.Pickaxe)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        "Mine harvest method requires a pickaxe-compatible tool.");
                }
            }

            if (CCS_HarvestMethodToolRulesUtility.HarvestMethodRequiresAxeTool(harvestMethod))
            {
                CCS_ItemToolType effectiveTool = explicitRequiredTool != CCS_ItemToolType.None
                    ? explicitRequiredTool
                    : CCS_HarvestMethodToolRulesUtility.GetDefaultRequiredTool(harvestMethod);

                if (effectiveTool != CCS_ItemToolType.Axe)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        "Chop harvest method requires an axe-compatible tool.");
                }
            }

            if (harvestMethod == CCS_HarvestMethodType.Dig)
            {
                CCS_ItemToolType effectiveTool = explicitRequiredTool != CCS_ItemToolType.None
                    ? explicitRequiredTool
                    : CCS_HarvestMethodToolRulesUtility.GetDefaultRequiredTool(harvestMethod);

                if (effectiveTool != CCS_ItemToolType.Shovel)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        "Dig harvest method requires a shovel-compatible tool.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Harvest method tool rules validated.");
        }

        public static CCS_SurvivalValidationResult ValidateUniqueIds(
            IReadOnlyList<string> resourceIds,
            string contextLabel)
        {
            if (resourceIds == null || resourceIds.Count == 0)
            {
                return CCS_SurvivalValidationResult.Pass($"{contextLabel}: no ids to validate.");
            }

            HashSet<string> seenIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < resourceIds.Count; index++)
            {
                string resourceId = resourceIds[index];
                if (string.IsNullOrWhiteSpace(resourceId))
                {
                    continue;
                }

                if (!seenIds.Add(resourceId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"{contextLabel} contains duplicate resource id: {resourceId}");
                }
            }

            return CCS_SurvivalValidationResult.Pass($"{contextLabel} resource ids are unique.");
        }
    }
}
