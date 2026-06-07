// =============================================================================
// SCRIPT: CCS_NpcAffiliationContentIds
// CATEGORY: Modules / NPCs / Runtime / Affiliations
// PURPOSE: Stable ids for NPC affiliation bootstrap, validation, and playtest.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest service.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.8.0 NPC settlement affiliation foundation.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcAffiliationContentIds
    {
        public const string AffiliationProfilesRoot = "Assets/CCS/Survival/Profiles/NPCs/Affiliations";

        public const string DefaultAffiliationProfilePath =
            AffiliationProfilesRoot + "/CCS_DefaultNpcAffiliationProfile.asset";

        public const string DefaultAffiliationProfileId = "ccs.survival.profile.npcaffiliation.default";
    }
}
