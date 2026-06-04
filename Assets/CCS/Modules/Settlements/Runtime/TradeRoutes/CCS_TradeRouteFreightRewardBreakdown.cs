// =============================================================================
// SCRIPT: CCS_TradeRouteFreightRewardBreakdown
// CATEGORY: Modules / Settlements / Runtime / TradeRoutes
// PURPOSE: Computed freight reward modifiers for contract completion and debug HUD.
// PLACEMENT: Produced by CCS_TradeRouteRewardModifierUtility.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.5.0 route risk and freight bonus foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_TradeRouteFreightRewardBreakdown
    {
        public static readonly CCS_TradeRouteFreightRewardBreakdown Empty = new CCS_TradeRouteFreightRewardBreakdown();

        public string LinkedRouteId { get; set; } = string.Empty;

        public CCS_TradeRouteRiskLevel RiskLevel { get; set; } = CCS_TradeRouteRiskLevel.Unknown;

        public int BaseTradeDollars { get; set; }

        public float RouteMultiplier { get; set; } = 1f;

        public float RiskMultiplier { get; set; } = 1f;

        public int FinalTradeDollars { get; set; }

        public int BonusReputation { get; set; }

        public bool UsedRouteModifiers { get; set; }

        public bool IsValid => BaseTradeDollars >= 0 && FinalTradeDollars >= 0;
    }
}
