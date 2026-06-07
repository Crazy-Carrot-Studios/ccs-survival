using CCS.Modules.Regions;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;

// =============================================================================
// SCRIPT: CCS_DynamicContractGenerationRequest
// CATEGORY: Modules / Contracts / Runtime / Dynamic
// PURPOSE: Input payload for evaluating a single dynamic contract generation attempt.
// PLACEMENT: Built by CCS_DynamicContractService and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 deterministic generation foundation.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public sealed class CCS_DynamicContractGenerationRequest
    {
        public string SettlementId = string.Empty;

        public CCS_DynamicContractGenerationSource GenerationSource = CCS_DynamicContractGenerationSource.Unknown;

        public CCS_SettlementSupplyType SupplyType = CCS_SettlementSupplyType.Food;

        public float SupplyFillPercent;

        public CCS_SettlementEventType EventType = CCS_SettlementEventType.Unknown;

        public string LinkedEventId = string.Empty;

        public CCS_RegionSpecializationType RegionSpecialization = CCS_RegionSpecializationType.Unknown;

        public int CurrentDayNumber = 1;

        public string NewsHeadlineReference = string.Empty;

        public bool ForceGenerationForPlaytest;
    }
}
