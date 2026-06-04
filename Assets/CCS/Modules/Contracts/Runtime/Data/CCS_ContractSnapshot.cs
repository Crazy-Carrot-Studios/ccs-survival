using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ContractSnapshot
// CATEGORY: Modules / Contracts / Runtime / Data
// PURPOSE: Serializable contract instance state for save/load.
// PLACEMENT: Stored in CCS_SaveContractsWorldData.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.0.0 frontier contracts foundation.
// =============================================================================

namespace CCS.Modules.Contracts
{
    [Serializable]
    public sealed class CCS_ContractSnapshot
    {
        public string contractDefinitionId = string.Empty;

        public int contractState;

        public string acceptedSettlementId = string.Empty;
    }
}
