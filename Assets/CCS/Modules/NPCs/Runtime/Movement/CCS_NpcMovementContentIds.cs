// =============================================================================
// SCRIPT: CCS_NpcMovementContentIds
// CATEGORY: Modules / NPCs / Runtime / Movement
// PURPOSE: Stable ids for NPC movement bootstrap, validation, and playtest.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest service.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.5.0 NPC movement foundation.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcMovementContentIds
    {
        public const string MovementProfilesRoot = "Assets/CCS/Survival/Profiles/NPCs/Movement";

        public const string DefaultMovementProfilePath =
            MovementProfilesRoot + "/CCS_DefaultNpcMovementProfile.asset";

        public const string DefaultMovementProfileId = "ccs.survival.profile.npcmovement.default";
    }
}
