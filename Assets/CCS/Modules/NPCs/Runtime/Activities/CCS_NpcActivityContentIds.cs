// =============================================================================
// SCRIPT: CCS_NpcActivityContentIds
// CATEGORY: Modules / NPCs / Runtime / Activities
// PURPOSE: Stable ids for NPC activity bootstrap, validation, and playtest.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest service.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.7.0 NPC activity state foundation.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcActivityContentIds
    {
        public const string ActivityProfilesRoot = "Assets/CCS/Survival/Profiles/NPCs/Activities";

        public const string DefaultActivityProfilePath =
            ActivityProfilesRoot + "/CCS_DefaultNpcActivityProfile.asset";

        public const string DefaultActivityProfileId = "ccs.survival.profile.npcactivity.default";
    }
}
