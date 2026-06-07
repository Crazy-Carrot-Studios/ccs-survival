// =============================================================================
// SCRIPT: CCS_NpcDialogueStubResultType
// CATEGORY: Modules / NPCs / Runtime / Dialogue
// PURPOSE: Result codes for profile-driven NPC dialogue stub resolution.
// PLACEMENT: Returned by CCS_NpcDialogueStubService and runtime bridge.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.9.0 — no branching dialogue or quests.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public enum CCS_NpcDialogueStubResultType
    {
        Success = 0,
        NoIdentity = 1,
        NoAffiliation = 2,
        NoRole = 3,
        NoMatchingStub = 4,
        InvalidTarget = 5,
        Failed = 6
    }
}
