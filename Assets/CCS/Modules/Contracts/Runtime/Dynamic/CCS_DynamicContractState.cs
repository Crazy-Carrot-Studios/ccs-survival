using System;

// =============================================================================
// SCRIPT: CCS_DynamicContractState
// CATEGORY: Modules / Contracts / Runtime / Dynamic
// PURPOSE: Persisted generated contract instance with embedded definition payload.
// PLACEMENT: Stored in CCS_SaveContractsWorldData.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 — restores runtime definitions on load.
// =============================================================================

namespace CCS.Modules.Contracts
{
    [Serializable]
    public sealed class CCS_DynamicContractState
    {
        public string generatedContractId = string.Empty;

        public string sourceRuleId = string.Empty;

        public string settlementId = string.Empty;

        public int generationDay;

        public int expirationDay;

        public int contractState;

        public string acceptedSettlementId = string.Empty;

        public string linkedEventId = string.Empty;

        public string newsHeadlineReference = string.Empty;

        public bool isActive = true;

        public string displayName = string.Empty;

        public int contractType;

        public int contractKind;

        public int regionSpecialization;

        public string freightSourceSettlementId = string.Empty;

        public string freightDestinationSettlementId = string.Empty;

        public CCS_DynamicContractRequirementEntry[] requirements = Array.Empty<CCS_DynamicContractRequirementEntry>();

        public CCS_DynamicContractRewardEntry reward = new CCS_DynamicContractRewardEntry();
    }
}
