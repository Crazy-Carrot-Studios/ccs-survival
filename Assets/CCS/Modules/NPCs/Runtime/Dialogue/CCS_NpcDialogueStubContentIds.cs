// =============================================================================
// SCRIPT: CCS_NpcDialogueStubContentIds
// CATEGORY: Modules / NPCs / Runtime / Dialogue
// PURPOSE: Stable ids for NPC dialogue stub bootstrap, validation, and playtest.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest service.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.9.0 NPC dialogue stub foundation.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcDialogueStubContentIds
    {
        public const string DialogueProfilesRoot = "Assets/CCS/Survival/Profiles/NPCs/Dialogue";

        public const string DefaultDialogueStubProfilePath =
            DialogueProfilesRoot + "/CCS_DefaultNpcDialogueStubProfile.asset";

        public const string DefaultDialogueStubProfileId = "ccs.survival.profile.npcdialoguestub.default";
    }
}
