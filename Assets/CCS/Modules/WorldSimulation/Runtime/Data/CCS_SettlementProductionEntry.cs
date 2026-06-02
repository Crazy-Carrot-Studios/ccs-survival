using System;

// =============================================================================
// SCRIPT: CCS_SettlementProductionEntry
// CATEGORY: Modules / WorldSimulation / Runtime / Data
// PURPOSE: Production tracking for a settlement supply category.
// PLACEMENT: Used by settlement simulation state and save payloads.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.0.0 frontier world simulation foundation.
// =============================================================================

namespace CCS.Modules.WorldSimulation
{
    [Serializable]
    public sealed class CCS_SettlementProductionEntry
    {
        public int supplyType;
        public float currentProduction;

        public CCS_SettlementSupplyType SupplyType =>
            Enum.IsDefined(typeof(CCS_SettlementSupplyType), supplyType)
                ? (CCS_SettlementSupplyType)supplyType
                : CCS_SettlementSupplyType.TradeGoods;
    }
}
