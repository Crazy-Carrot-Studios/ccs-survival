using System;

// =============================================================================
// SCRIPT: CCS_SettlementSimulationState
// CATEGORY: Modules / WorldSimulation / Runtime / Data
// PURPOSE: Runtime settlement supply, demand, production, and prosperity state.
// PLACEMENT: Stored by CCS_WorldSimulationService and save payloads.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.0.0 frontier world simulation foundation.
// =============================================================================

namespace CCS.Modules.WorldSimulation
{
    [Serializable]
    public sealed class CCS_SettlementSimulationState
    {
        public string settlementId = string.Empty;
        public int population;
        public float prosperity;
        public bool isDiscovered;
        public CCS_SettlementSupplyEntry[] supplies = Array.Empty<CCS_SettlementSupplyEntry>();
        public CCS_SettlementDemandEntry[] demands = Array.Empty<CCS_SettlementDemandEntry>();
        public CCS_SettlementProductionEntry[] productions = Array.Empty<CCS_SettlementProductionEntry>();
    }
}
