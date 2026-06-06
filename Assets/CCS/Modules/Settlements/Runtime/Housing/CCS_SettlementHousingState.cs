using System;

// =============================================================================
// SCRIPT: CCS_SettlementHousingState
// CATEGORY: Modules / Settlements / Runtime / Housing
// PURPOSE: Persisted settlement housing activation and capacity state.
// PLACEMENT: Stored on CCS_SettlementSimulationState.housingStates.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 — save/load via world simulation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [Serializable]
    public sealed class CCS_SettlementHousingState
    {
        public string housingId = string.Empty;

        public string settlementId = string.Empty;

        public string displayName = string.Empty;

        public int housingType;

        public int capacityContribution;

        public int requiredGrowthStage;

        public int workforceAffinity;

        public bool isActive;
    }
}
