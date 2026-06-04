// =============================================================================
// SCRIPT: CCS_ContractCompletionResult
// CATEGORY: Modules / Contracts / Runtime / Data
// PURPOSE: Result payload for contract accept and complete operations.
// PLACEMENT: Returned by CCS_ContractService.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.0.0 frontier contracts foundation.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public sealed class CCS_ContractCompletionResult
    {
        public CCS_ContractCompletionResult(
            bool isSuccess,
            string contractId,
            string message,
            int tradeDollarsGranted = 0,
            int reputationGainApplied = 0,
            float prosperityGainApplied = 0f,
            float supplyAmountApplied = 0f)
        {
            IsSuccess = isSuccess;
            ContractId = contractId ?? string.Empty;
            Message = message ?? string.Empty;
            TradeDollarsGranted = tradeDollarsGranted;
            ReputationGainApplied = reputationGainApplied;
            ProsperityGainApplied = prosperityGainApplied;
            SupplyAmountApplied = supplyAmountApplied;
        }

        public bool IsSuccess { get; }

        public string ContractId { get; }

        public string Message { get; }

        public int TradeDollarsGranted { get; }

        public int ReputationGainApplied { get; }

        public float ProsperityGainApplied { get; }

        public float SupplyAmountApplied { get; }
    }
}
