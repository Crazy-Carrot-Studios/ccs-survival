// =============================================================================
// SCRIPT: CCS_DynamicContractGenerationResult
// CATEGORY: Modules / Contracts / Runtime / Dynamic
// PURPOSE: Outcome payload for dynamic contract generation attempts.
// PLACEMENT: Returned by CCS_DynamicContractService.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 dynamic contract generation foundation.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public sealed class CCS_DynamicContractGenerationResult
    {
        public bool IsSuccess;

        public string Message = string.Empty;

        public string GeneratedContractId = string.Empty;

        public string SourceRuleId = string.Empty;

        public CCS_DynamicContractSnapshot Snapshot;

        public static CCS_DynamicContractGenerationResult Success(
            string generatedContractId,
            string sourceRuleId,
            CCS_DynamicContractSnapshot snapshot,
            string message)
        {
            return new CCS_DynamicContractGenerationResult
            {
                IsSuccess = true,
                GeneratedContractId = generatedContractId ?? string.Empty,
                SourceRuleId = sourceRuleId ?? string.Empty,
                Snapshot = snapshot,
                Message = message ?? string.Empty
            };
        }

        public static CCS_DynamicContractGenerationResult Failure(string message)
        {
            return new CCS_DynamicContractGenerationResult
            {
                IsSuccess = false,
                Message = message ?? string.Empty
            };
        }
    }
}
