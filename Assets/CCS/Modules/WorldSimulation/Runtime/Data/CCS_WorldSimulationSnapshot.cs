using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_WorldSimulationSnapshot
// CATEGORY: Modules / WorldSimulation / Runtime / Data
// PURPOSE: Aggregate snapshot of world simulation state for queries and events.
// PLACEMENT: Returned by CCS_WorldSimulationService query methods.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.0.0 frontier world simulation foundation.
// =============================================================================

namespace CCS.Modules.WorldSimulation
{
    public sealed class CCS_WorldSimulationSnapshot
    {
        public IReadOnlyList<CCS_SettlementSimulationState> SettlementStates { get; set; } =
            new CCS_SettlementSimulationState[0];

        public IReadOnlyList<CCS_RegionSimulationState> RegionStates { get; set; } =
            new CCS_RegionSimulationState[0];
    }
}
