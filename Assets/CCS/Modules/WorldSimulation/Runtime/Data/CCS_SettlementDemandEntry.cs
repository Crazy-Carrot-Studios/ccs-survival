using System;

// =============================================================================
// SCRIPT: CCS_SettlementDemandEntry
// CATEGORY: Modules / WorldSimulation / Runtime / Data
// PURPOSE: Demand tracking for a settlement supply category.
// PLACEMENT: Used by settlement simulation state and save payloads.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.0.0 frontier world simulation foundation.
// =============================================================================

namespace CCS.Modules.WorldSimulation
{
    [Serializable]
    public sealed class CCS_SettlementDemandEntry
    {
        public int supplyType;
        public float currentDemand;

        public CCS_SettlementSupplyType SupplyType =>
            Enum.IsDefined(typeof(CCS_SettlementSupplyType), supplyType)
                ? (CCS_SettlementSupplyType)supplyType
                : CCS_SettlementSupplyType.TradeGoods;
    }
}
