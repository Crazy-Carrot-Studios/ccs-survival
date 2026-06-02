using CCS.Modules.Gathering;
using CCS.Modules.Inventory;
using CCS.Modules.Resources;
using CCS.Modules.WorldResources;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_ProspectingValidationUtility
// CATEGORY: Modules / Prospecting / Runtime / Validation
// PURPOSE: Validates mining nodes, tool tiers, and drop metadata.
// PLACEMENT: Used by editor validation and bootstrap sanity checks.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Prospecting
{
    public static class CCS_ProspectingValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateResourceDefinition(CCS_ResourceDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Resource definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.ResourceId))
            {
                return CCS_SurvivalValidationResult.Fail("Resource definition is missing resourceId.");
            }

            if (definition.ResourceId == CCS_ProspectingContentIds.AbandonedMineEntranceResourceId)
            {
                return CCS_SurvivalValidationResult.Pass(
                    $"Placeholder entrance '{definition.ResourceId}' validated.");
            }

            if (definition.DropDefinitions == null || definition.DropDefinitions.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Resource '{definition.ResourceId}' has no drop definitions.");
            }

            if (definition.HarvestMethod == CCS_HarvestMethodType.Mine
                && definition.RequiredToolType == CCS_RequiredToolType.None)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Mining resource '{definition.ResourceId}' must require a pickaxe.");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"Resource '{definition.ResourceId}' validated for prospecting.");
        }

        public static CCS_SurvivalValidationResult ValidateGatheringNodeSettings(
            CCS_GatheringProfile profile,
            CCS_GatheringNodeType nodeType,
            CCS_ResourceSourceType expectedSource,
            CCS_HarvestMethodType expectedMethod,
            CCS_ItemToolType expectedTool,
            CCS_ToolTier expectedMinimumTier,
            string requiredRewardItemId)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Gathering profile is null.");
            }

            if (!profile.TryGetNodeRewardSettings(nodeType, out CCS_GatheringNodeRewardSettings settings))
            {
                return CCS_SurvivalValidationResult.Fail($"Gathering profile missing node type {nodeType}.");
            }

            if (settings.resourceSourceType != expectedSource
                || settings.harvestMethod != expectedMethod
                || settings.requiredToolType != expectedTool)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Gathering node {nodeType} has unexpected harvest metadata.");
            }

            if (settings.minimumToolTier != expectedMinimumTier)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Gathering node {nodeType} minimum tool tier expected {expectedMinimumTier}.");
            }

            bool hasReward = false;
            if (settings.rewards != null)
            {
                for (int index = 0; index < settings.rewards.Length; index++)
                {
                    if (settings.rewards[index].itemDefinitionId == requiredRewardItemId)
                    {
                        hasReward = true;
                        break;
                    }
                }
            }

            if (!hasReward)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Gathering node {nodeType} missing reward item '{requiredRewardItemId}'.");
            }

            return CCS_SurvivalValidationResult.Pass($"Gathering node {nodeType} validated.");
        }
    }
}
