// =============================================================================
// SCRIPT: CCS_ContractType
// CATEGORY: Modules / Contracts / Runtime / Data
// PURPOSE: Contract archetype for settlement service board filtering.
// PLACEMENT: Used by CCS_ContractDefinition and settlement contract boards.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.0.0 frontier contracts foundation.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public enum CCS_ContractType
    {
        GeneralStoreSupply = 0,
        GunsmithSupply = 1,
        StableSupply = 2,
        TradingPostSupply = 3,
        LandOfficeSupply = 4,
        FreightDelivery = 5
    }
}
