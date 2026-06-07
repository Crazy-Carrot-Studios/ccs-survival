// =============================================================================
// SCRIPT: CCS_DynamicContractKind
// CATEGORY: Modules / Contracts / Runtime / Dynamic
// PURPOSE: Local supply vs freight delivery archetypes for generated contracts.
// PLACEMENT: Serialized on CCS_DynamicContractRule.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 dynamic contract generation foundation.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public enum CCS_DynamicContractKind
    {
        LocalSupply = 0,
        FreightDelivery = 1
    }
}
