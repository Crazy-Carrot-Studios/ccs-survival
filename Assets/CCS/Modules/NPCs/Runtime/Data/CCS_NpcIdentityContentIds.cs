// =============================================================================
// SCRIPT: CCS_NpcIdentityContentIds
// CATEGORY: Modules / NPCs / Runtime / Data
// PURPOSE: Stable ids for NPC identity bootstrap, validation, and playtest harness.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest service.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 NPC identity and role foundation.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcIdentityContentIds
    {
        public const string IdentityProfilesRoot = "Assets/CCS/Survival/Profiles/NPCs/Identity";
        public const string DefaultIdentityProfilePath =
            IdentityProfilesRoot + "/CCS_DefaultNpcIdentityProfile.asset";
        public const string DefaultIdentityProfileId = "ccs.survival.profile.npcidentity.default";

        public const string IdentityIdPrefix = "ccs.survival.npc";
    }
}
