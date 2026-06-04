// =============================================================================
// SCRIPT: CCS_ContractState
// CATEGORY: Modules / Contracts / Runtime / Data
// PURPOSE: Runtime lifecycle state for player contract instances.
// PLACEMENT: Used by CCS_ContractSnapshot and CCS_ContractService.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.0.0 frontier contracts foundation.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public enum CCS_ContractState
    {
        Available = 0,
        Accepted = 1,
        Completed = 2
    }
}
