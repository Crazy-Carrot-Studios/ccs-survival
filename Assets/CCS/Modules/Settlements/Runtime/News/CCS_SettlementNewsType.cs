// =============================================================================
// SCRIPT: CCS_SettlementNewsType
// CATEGORY: Modules / Settlements / Runtime / News
// PURPOSE: News categories generated from settlement events and future placeholders.
// PLACEMENT: Serialized on news definitions and persisted news state.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.2.0 — only active types participate in event-driven news.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_SettlementNewsType
    {
        Unknown = 0,
        MarketDay = 1,
        SupplyShipment = 2,
        HarvestFestival = 3,
        MiningShipment = 4,
        TimberDelivery = 5,
        RailroadArrival = 100,
        Election = 101,
        Fire = 102,
        Disease = 103,
        Raid = 104
    }
}
