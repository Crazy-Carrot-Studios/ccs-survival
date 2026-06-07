// =============================================================================
// SCRIPT: CCS_NpcAffiliationType
// CATEGORY: Modules / NPCs / Runtime / Affiliations
// PURPOSE: Categories of persistent NPC community affiliation metadata.
// PLACEMENT: Serialized on affiliation state and profile validation rules.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.8.0 — identity metadata only; no gameplay effects yet.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public enum CCS_NpcAffiliationType
    {
        None = 0,
        Settlement = 1,
        Business = 2,
        Workforce = 3,
        Region = 4
    }
}
