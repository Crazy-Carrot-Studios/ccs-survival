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
            return ValidateRequiredNodeRewards(profile);
        }

        #endregion

        #region Private Methods

        private static CCS_SurvivalValidationResult ValidateRequiredNodeRewards(CCS_GatheringProfile profile)
        {
            CCS_SurvivalValidationResult smallTreeValidation =
                ValidateNodeRewards(profile, CCS_GatheringNodeType.SmallTree, "SmallTree");
            if (!smallTreeValidation.IsSuccess)
            {
                return smallTreeValidation;
            }

            CCS_SurvivalValidationResult rockValidation =
                ValidateNodeRewards(profile, CCS_GatheringNodeType.Rock, "Rock");
            if (!rockValidation.IsSuccess)
            {
                return rockValidation;
            }

            CCS_SurvivalValidationResult bushValidation =
                ValidateNodeRewards(profile, CCS_GatheringNodeType.Bush, "Bush");
            if (!bushValidation.IsSuccess)
            {
                return bushValidation;
            }

            return CCS_SurvivalValidationResult.Pass("Gathering profile validated.");
        }

        private static CCS_SurvivalValidationResult ValidateNodeRewards(
            CCS_GatheringProfile profile,
            CCS_GatheringNodeType nodeType,
            string label)
        {
            if (!profile.TryGetRewards(nodeType, out CCS_GatheringReward[] rewards))
            {
                return CCS_SurvivalValidationResult.Fail($"{label} gathering rewards are not configured.");
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
