using CCS.Modules.Settlements;

// =============================================================================
// SCRIPT: CCS_ContractCompletionResult
// CATEGORY: Modules / Contracts / Runtime / Data
// PURPOSE: Result payload for contract accept and complete operations.
// PLACEMENT: Returned by CCS_ContractService.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.5.0 — optional freight route reward modifier fields for debug HUD.
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
            : this(
                isSuccess,
                contractId,
                message,
                tradeDollarsGranted,
                reputationGainApplied,
                prosperityGainApplied,
                supplyAmountApplied,
                tradeDollarsGranted,
                1f,
                1f,
                string.Empty,
                CCS_TradeRouteRiskLevel.Unknown)
        {
        }

        public CCS_ContractCompletionResult(
            bool isSuccess,
            string contractId,
            string message,
            int tradeDollarsGranted,
            int reputationGainApplied,
            float prosperityGainApplied,
            float supplyAmountApplied,
            int baseTradeDollarsReward,
            float routeRewardMultiplier,
            float riskRewardMultiplier,
            string linkedTradeRouteId,
            CCS_TradeRouteRiskLevel routeRiskLevel)
        {
            IsSuccess = isSuccess;
            ContractId = contractId ?? string.Empty;
            Message = message ?? string.Empty;
            TradeDollarsGranted = tradeDollarsGranted;
            ReputationGainApplied = reputationGainApplied;
            ProsperityGainApplied = prosperityGainApplied;
            SupplyAmountApplied = supplyAmountApplied;
            BaseTradeDollarsReward = baseTradeDollarsReward < 0 ? 0 : baseTradeDollarsReward;
            RouteRewardMultiplier = routeRewardMultiplier < 0f ? 0f : routeRewardMultiplier;
            RiskRewardMultiplier = riskRewardMultiplier < 0f ? 0f : riskRewardMultiplier;
            LinkedTradeRouteId = linkedTradeRouteId ?? string.Empty;
            RouteRiskLevel = routeRiskLevel;
        }

        public bool IsSuccess { get; }

        public string ContractId { get; }

        public string Message { get; }

        public int TradeDollarsGranted { get; }

        public int ReputationGainApplied { get; }

        public float ProsperityGainApplied { get; }

        public float SupplyAmountApplied { get; }

        public int BaseTradeDollarsReward { get; }

        public float RouteRewardMultiplier { get; }

        public float RiskRewardMultiplier { get; }

        public string LinkedTradeRouteId { get; }

        public CCS_TradeRouteRiskLevel RouteRiskLevel { get; }

        public bool HasFreightRewardBreakdown =>
            !string.IsNullOrWhiteSpace(LinkedTradeRouteId)
            || BaseTradeDollarsReward != TradeDollarsGranted
            || RouteRewardMultiplier > 1.001f
            || RiskRewardMultiplier > 1.001f;
    }
}
