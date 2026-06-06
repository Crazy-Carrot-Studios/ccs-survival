// =============================================================================
// SCRIPT: CCS_NpcServiceRepresentativeContentIds
// CATEGORY: Modules / NPCs / Runtime / Data
// PURPOSE: Stable ids for NPC service representative bootstrap and validation.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-05
// NOTES: Milestone 4.3.0 NPC service representatives foundation.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcServiceRepresentativeContentIds
    {
        public const string RepresentativeProfilesRoot =
            "Assets/CCS/Survival/Profiles/NPCs/ServiceRepresentatives";
        public const string DefaultRepresentativeProfilePath =
            RepresentativeProfilesRoot + "/CCS_DefaultNpcServiceRepresentativeProfile.asset";
        public const string DefaultRepresentativeProfileId =
            "ccs.survival.profile.npcservicerepresentative.default";
        public const string RepresentativeIdPrefix = "ccs.survival.npc.representative";
        public const string RepresentativeAnchorPrefix = "ccs.survival.npc.representative.anchor";
    }
}
