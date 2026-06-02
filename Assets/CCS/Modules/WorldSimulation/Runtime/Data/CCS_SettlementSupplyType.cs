// =============================================================================
// SCRIPT: CCS_SettlementSupplyType
// CATEGORY: Modules / WorldSimulation / Runtime / Data
// PURPOSE: Supply category identifiers for settlement simulation state.
// PLACEMENT: Referenced by settlement supply, demand, and production entries.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.0.0 frontier world simulation foundation.
// =============================================================================

namespace CCS.Modules.WorldSimulation
{
    public enum CCS_SettlementSupplyType
    {
        Food = 0,
        Water = 1,
        Fuel = 2,
        BuildingMaterials = 3,
        IndustrialMaterials = 4,
        Tools = 5,
        TradeGoods = 6
    }
}
