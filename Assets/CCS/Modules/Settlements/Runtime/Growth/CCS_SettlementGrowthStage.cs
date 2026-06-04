// =============================================================================
// SCRIPT: CCS_SettlementGrowthStage
// CATEGORY: Modules / Settlements / Runtime / Growth
// PURPOSE: Ordered settlement growth archetypes for frontier world simulation.
// PLACEMENT: Used by growth profiles, world simulation, and settlement services.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.2.0 — FrontierTown and EstablishedTown are placeholders in 3.2.0.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_SettlementGrowthStage
    {
        Unknown = 0,
        Outpost = 1,
        TradingPost = 2,
        FrontierTown = 3,
        EstablishedTown = 4
    }
}
