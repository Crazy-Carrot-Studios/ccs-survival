using System;

// =============================================================================
// SCRIPT: CCS_SettlementSupplyEntry
// CATEGORY: Modules / WorldSimulation / Runtime / Data
// PURPOSE: Current and desired supply amounts for a settlement category.
// PLACEMENT: Used by settlement simulation state and save payloads.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Profile-driven desired amounts; runtime tracks current amounts.
// =============================================================================

namespace CCS.Modules.WorldSimulation
{
    [Serializable]
    public sealed class CCS_SettlementSupplyEntry
    {
        public int supplyType;
        public float currentAmount;
        public float desiredAmount;

        public CCS_SettlementSupplyType SupplyType =>
            Enum.IsDefined(typeof(CCS_SettlementSupplyType), supplyType)
                ? (CCS_SettlementSupplyType)supplyType
                : CCS_SettlementSupplyType.TradeGoods;

        public float FillRatio
        {
            get
            {
                if (desiredAmount <= 0f)
                {
                    return currentAmount > 0f ? 1f : 0f;
                }

                return currentAmount / desiredAmount;
            }
        }
    }
}
