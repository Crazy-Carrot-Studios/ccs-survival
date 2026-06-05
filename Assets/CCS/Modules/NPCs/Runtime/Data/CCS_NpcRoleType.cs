// =============================================================================
// SCRIPT: CCS_NpcRoleType
// CATEGORY: Modules / NPCs / Runtime / Data
// PURPOSE: Workforce and service role identifiers for placeholder NPC identity.
// PLACEMENT: Serialized on identity state and role assignment mappings.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 — Doctor/Sheriff are placeholders only (not workforce-assigned).
// =============================================================================

namespace CCS.Modules.NPCs
{
    public enum CCS_NpcRoleType
    {
        Unknown = 0,
        Merchant = 1,
        Banker = 2,
        StableHand = 3,
        Gunsmith = 4,
        Blacksmith = 5,
        Farmer = 6,
        Rancher = 7,
        Miner = 8,
        LumberWorker = 9,
        Laborer = 10,
        Clerk = 11,
        DoctorPlaceholder = 12,
        SheriffPlaceholder = 13
    }
}
