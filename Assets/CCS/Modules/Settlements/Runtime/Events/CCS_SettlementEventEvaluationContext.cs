// =============================================================================
// SCRIPT: CCS_SettlementEventEvaluationContext
// CATEGORY: Modules / Settlements / Runtime / Events
// PURPOSE: Lightweight evaluation inputs for settlement event generation.
// PLACEMENT: Populated by composition wiring from world simulation services.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Keeps Settlements runtime free of WorldSimulation assembly references.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public struct CCS_SettlementEventTimeSnapshot
    {
        public int DayNumber;
        public int Hour;

        public static CCS_SettlementEventTimeSnapshot Default => new CCS_SettlementEventTimeSnapshot
        {
            DayNumber = 1,
            Hour = 0
        };
    }

    public struct CCS_SettlementEventSimulationContext
    {
        public bool IsDiscovered;
        public int Population;
        public float Prosperity;
        public int ActiveBusinessCount;
        public int TradeRouteUsageCount;

        public static CCS_SettlementEventSimulationContext Empty => default;
    }
}
