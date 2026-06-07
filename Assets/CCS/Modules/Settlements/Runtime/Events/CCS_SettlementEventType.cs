// =============================================================================
// SCRIPT: CCS_SettlementEventType
// CATEGORY: Modules / Settlements / Runtime / Events
// PURPOSE: Dynamic settlement event categories for simulation-driven presentation.
// PLACEMENT: Serialized on definitions and persisted event state.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 — only active types participate in generation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_SettlementEventType
    {
        Unknown = 0,
        MarketDay = 1,
        SupplyShipment = 2,
        HarvestFestival = 3,
        MiningShipment = 4,
        TimberDelivery = 5,
        Election = 100,
        Fire = 101,
        Disease = 102,
        Raid = 103,
        RailroadArrival = 104
    }
}
