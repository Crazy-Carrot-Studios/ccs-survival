// =============================================================================
// SCRIPT: CCS_ReputationContentIds
// CATEGORY: Modules / Reputation / Runtime / Data
// PURPOSE: Stable content ids for reputation bootstrap, validation, and playtest harness.
// PLACEMENT: Shared by bootstrap setup, validators, and debug HUD.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 frontier trading post trust foundation.
// =============================================================================

namespace CCS.Modules.Reputation
{
    public static class CCS_ReputationContentIds
    {
        public const string FrontierTradingPostReputationDefinitionId =
            "ccs.survival.reputation.settlement.frontier.testtradingpost";
        public const string DefaultReputationProfilePath =
            "Assets/CCS/Survival/Profiles/Reputation/CCS_DefaultReputationProfile.asset";
        public const string FrontierTradingPostReputationDefinitionPath =
            "Assets/CCS/Survival/Content/Reputation/CCS_Reputation_FrontierTradingPost.asset";
        public const string DefaultTradingPostSettlementId =
            "ccs.survival.settlement.frontier.testtradingpost";
    }
}
