using System;

// =============================================================================
// SCRIPT: CCS_RegionSimulationState
// CATEGORY: Modules / WorldSimulation / Runtime / Data
// PURPOSE: Metadata-only region simulation state for frontier world organization.
// PLACEMENT: Stored by CCS_WorldSimulationService and save payloads.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Does not generate resources; discovery and potential metadata only.
// =============================================================================

namespace CCS.Modules.WorldSimulation
{
    [Serializable]
    public sealed class CCS_RegionSimulationState
    {
        public string regionId = string.Empty;
        public bool isDiscovered;
        public float foodPotential;
        public float wildlifePotential;
        public float miningPotential;
        public float industryPotential;
    }
}
