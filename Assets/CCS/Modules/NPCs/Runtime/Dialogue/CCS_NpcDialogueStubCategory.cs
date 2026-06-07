// =============================================================================
// SCRIPT: CCS_NpcDialogueStubCategory
// CATEGORY: Modules / NPCs / Runtime / Dialogue
// PURPOSE: Simple non-branching dialogue stub categories for placeholder NPCs.
// PLACEMENT: Serialized on CCS_NpcDialogueStubLine entries.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.9.0 — no player response choices.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public enum CCS_NpcDialogueStubCategory
    {
        Unknown = 0,
        Greeting = 1,
        RoleIntroduction = 2,
        SettlementIntroduction = 3,
        BusinessIntroduction = 4,
        ServiceHint = 5,
        GenericFallback = 6
    }
}
