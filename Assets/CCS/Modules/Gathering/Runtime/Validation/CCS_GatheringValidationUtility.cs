using CCS.Modules.Resources;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_GatheringValidationUtility
// CATEGORY: Modules / Gathering / Runtime / Validation
// PURPOSE: Runtime-safe validation for gathering profiles and reward configuration.
// PLACEMENT: Used by editor validators and CCS_GatheringService preflight checks.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Runtime-only; no editor APIs.
// =============================================================================

namespace CCS.Modules.Gathering
{
    public static class CCS_GatheringValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_GatheringProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Gathering profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.NodeInteractionDistance <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Node interaction distance must be greater than zero.");
            }

            if (profile.GatherDurationSeconds < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Gather duration must be non-negative.");
            }

            if (profile.RespawnEnabled && profile.RespawnDelaySeconds <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Respawn delay must be greater than zero when respawn is enabled.");
            }

            profile.BuildRewardLookup();
            return ValidateAllNodeRewardSettings(profile);
        }

        #endregion

        #region Private Methods

        private static CCS_SurvivalValidationResult ValidateAllNodeRewardSettings(CCS_GatheringProfile profile)
        {
            if (profile.NodeRewardSettings == null || profile.NodeRewardSettings.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Gathering profile has no node reward settings.");
            }

            System.Collections.Generic.List<string> nodeTypeIds =
                new System.Collections.Generic.List<string>();

            for (int index = 0; index < profile.NodeRewardSettings.Count; index++)
            {
                CCS_GatheringNodeRewardSettings settings = profile.NodeRewardSettings[index];
                string label = settings.nodeType.ToString();
                nodeTypeIds.Add(label);

                CCS_SurvivalValidationResult metadataValidation = ValidateNodeMetadata(settings, label);
                if (!metadataValidation.IsSuccess)
                {
                    return metadataValidation;
                }

                CCS_SurvivalValidationResult rewardsValidation = ValidateNodeRewards(profile, settings, label);
                if (!rewardsValidation.IsSuccess)
                {
                    return rewardsValidation;
                }
            }

            CCS_SurvivalValidationResult legacySmallTree =
                ValidateNodeRewards(profile, CCS_GatheringNodeType.SmallTree, "SmallTree");
            if (!legacySmallTree.IsSuccess)
            {
                return legacySmallTree;
            }

            CCS_SurvivalValidationResult legacyRock =
                ValidateNodeRewards(profile, CCS_GatheringNodeType.Rock, "Rock");
            if (!legacyRock.IsSuccess)
            {
                return legacyRock;
            }

            CCS_SurvivalValidationResult legacyBush =
                ValidateNodeRewards(profile, CCS_GatheringNodeType.Bush, "Bush");
            if (!legacyBush.IsSuccess)
            {
                return legacyBush;
            }

            return CCS_SurvivalValidationResult.Pass("Gathering profile validated.");
        }

        private static CCS_SurvivalValidationResult ValidateNodeMetadata(
            CCS_GatheringNodeRewardSettings settings,
            string label)
        {
            CCS_SurvivalValidationResult sourceValidation =
                CCS_ResourceFrameworkValidationUtility.ValidateSourceTypeAssigned(settings.resourceSourceType);
            if (!sourceValidation.IsSuccess)
            {
                return CCS_SurvivalValidationResult.Fail($"{label}: {sourceValidation.Message}");
            }

            CCS_SurvivalValidationResult methodValidation =
                CCS_ResourceFrameworkValidationUtility.ValidateHarvestMethodAssigned(settings.harvestMethod);
            if (!methodValidation.IsSuccess)
            {
                return CCS_SurvivalValidationResult.Fail($"{label}: {methodValidation.Message}");
            }

            CCS_SurvivalValidationResult toolRulesValidation =
                CCS_ResourceFrameworkValidationUtility.ValidateHarvestMethodToolRules(
                    settings.harvestMethod,
                    settings.requiredToolType);
            if (!toolRulesValidation.IsSuccess)
            {
                return CCS_SurvivalValidationResult.Fail($"{label}: {toolRulesValidation.Message}");
            }

            return CCS_SurvivalValidationResult.Pass($"{label} metadata validated.");
        }

        private static CCS_SurvivalValidationResult ValidateNodeRewards(
            CCS_GatheringProfile profile,
            CCS_GatheringNodeType nodeType,
            string label)
        {
            if (!profile.TryGetNodeRewardSettings(nodeType, out CCS_GatheringNodeRewardSettings settings))
            {
                return CCS_SurvivalValidationResult.Fail($"{label} gathering rewards are not configured.");
            }

            return ValidateNodeRewards(profile, settings, label);
        }

        private static CCS_SurvivalValidationResult ValidateNodeRewards(
            CCS_GatheringProfile profile,
            CCS_GatheringNodeRewardSettings settings,
            string label)
        {
            CCS_GatheringReward[] rewards = settings.rewards;
            if (rewards == null || rewards.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail($"{label} requires at least one reward entry.");
            }

            for (int index = 0; index < rewards.Length; index++)
            {
                CCS_GatheringReward reward = rewards[index];
                if (reward.amount <= 0)
                {
                    return CCS_SurvivalValidationResult.Fail($"{label} reward amount must be greater than zero.");
                }

                if (string.IsNullOrWhiteSpace(reward.itemDefinitionId))
                {
                    return CCS_SurvivalValidationResult.Fail($"{label} reward itemDefinitionId is required.");
                }

                if (!profile.TryResolveItemDefinition(reward.itemDefinitionId, out _))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"{label} reward item '{reward.itemDefinitionId}' is missing from the reward catalog.");
                }
            }

            return CCS_SurvivalValidationResult.Pass($"{label} gathering rewards validated.");
        }

        #endregion
    }
}
