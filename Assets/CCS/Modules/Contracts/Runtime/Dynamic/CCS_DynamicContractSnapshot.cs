using System;

// =============================================================================
// SCRIPT: CCS_DynamicContractSnapshot
// CATEGORY: Modules / Contracts / Runtime / Dynamic
// PURPOSE: Read-only presentation snapshot for debug boards and validation.
// PLACEMENT: Built by CCS_DynamicContractService.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 — optional news headline reference for debug display.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public sealed class CCS_DynamicContractSnapshot
    {
        public string GeneratedContractId { get; set; } = string.Empty;

        public string SourceRuleId { get; set; } = string.Empty;

        public string SettlementId { get; set; } = string.Empty;

        public int GenerationDay { get; set; }

        public int ExpirationDay { get; set; }

        public CCS_ContractState ContractState { get; set; } = CCS_ContractState.Available;

        public string LinkedEventId { get; set; } = string.Empty;

        public string NewsHeadlineReference { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(GeneratedContractId)
            && !string.IsNullOrWhiteSpace(SourceRuleId)
            && !string.IsNullOrWhiteSpace(SettlementId);
    }
}
