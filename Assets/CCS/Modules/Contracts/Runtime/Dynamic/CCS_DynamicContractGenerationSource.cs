// =============================================================================
// SCRIPT: CCS_DynamicContractGenerationSource
// CATEGORY: Modules / Contracts / Runtime / Dynamic
// PURPOSE: Identifies which simulation signal triggered dynamic contract generation.
// PLACEMENT: Serialized on CCS_DynamicContractRule.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 — workforce/business/trade route sources reserved as placeholders.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public enum CCS_DynamicContractGenerationSource
    {
        Unknown = 0,
        LowSettlementSupply = 1,
        ActiveSettlementEvent = 2,
        RegionalSpecialization = 3,
        WorkforceDemand = 4,
        BusinessDemand = 5,
        TradeRouteDemand = 6
    }
}
