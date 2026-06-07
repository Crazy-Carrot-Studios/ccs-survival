// =============================================================================
// SCRIPT: CCS_NpcSocialContentIds
// CATEGORY: Modules / NPCs / Runtime / Social
// PURPOSE: Stable ids for NPC social bootstrap, validation, and playtest.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest service.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.0.0 NPC social presence foundation.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcSocialContentIds
    {
        public const string SocialProfilesRoot = "Assets/CCS/Survival/Profiles/NPCs/Social";

        public const string DefaultSocialProfilePath = SocialProfilesRoot + "/CCS_DefaultNpcSocialProfile.asset";

        public const string DefaultSocialProfileId = "ccs.survival.profile.npcsocial.default";

        public const string TradingPostCampfireAnchorId = "ccs.survival.social.tradingpost.campfire";

        public const string TradingPostHitchingRailAnchorId = "ccs.survival.social.tradingpost.hitchingrail";

        public const string BrokenCreekCommunityFireAnchorId = "ccs.survival.social.brokencreek.communityfire";

        public const string IronRidgeMineFireAnchorId = "ccs.survival.social.ironridge.minefire";

        public const string PineRidgeLumberCampFireAnchorId = "ccs.survival.social.pineridge.lumbercampfire";
    }
}
