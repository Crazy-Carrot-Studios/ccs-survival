// =============================================================================
// SCRIPT: CCS_SettlementType
// CATEGORY: Modules / Settlements / Runtime / Data
// PURPOSE: Generic settlement archetypes for frontier service locations.
// PLACEMENT: Used by CCS_SettlementDefinition and discovery snapshots.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 1.8.0 frontier settlement foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_SettlementType
    {
        TradingPost = 0,
        Town = 1,
        MiningCamp = 2,
        RailCamp = 3,
        Ranch = 4,
        Fort = 5,
        Homestead = 6,
        Other = 7
    }
}
