using System;

// =============================================================================
// SCRIPT: CCS_DynamicContractRequirementEntry
// CATEGORY: Modules / Contracts / Runtime / Dynamic
// PURPOSE: JsonUtility-compatible requirement payload for persisted generated contracts.
// PLACEMENT: Embedded in CCS_DynamicContractState.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 save/load support.
// =============================================================================

namespace CCS.Modules.Contracts
{
    [Serializable]
    public sealed class CCS_DynamicContractRequirementEntry
    {
        public string itemId = string.Empty;

        public int quantity = 1;
    }
}
