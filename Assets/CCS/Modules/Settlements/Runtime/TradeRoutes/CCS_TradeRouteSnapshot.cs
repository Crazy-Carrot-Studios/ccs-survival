// =============================================================================
// SCRIPT: CCS_TradeRouteSnapshot
// CATEGORY: Modules / Settlements / Runtime / TradeRoutes
// PURPOSE: Serializable trade route metadata for save payloads and queries.
// PLACEMENT: Used by save system and future trade route services.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.5.0 — includes risk metadata for debug queries; config lives on definitions.
// =============================================================================

using System;

namespace CCS.Modules.Settlements
{
    [Serializable]
    public sealed class CCS_TradeRouteSnapshot
    {
        public string routeId = string.Empty;
        public string displayName = string.Empty;
        public string originSettlementId = string.Empty;
        public string destinationSettlementId = string.Empty;
        public string[] preferredGoods = Array.Empty<string>();
        public float distance;
        public int routeDifficulty;
        public bool isDiscovered;
        public bool isActive;
        public int usageCount;
        public int riskRating;
        public float baseFreightMultiplier = 1f;
        public float distanceMultiplier = 1f;

        public string RouteId => routeId ?? string.Empty;
        public string DisplayName => displayName ?? string.Empty;
        public string OriginSettlementId => originSettlementId ?? string.Empty;
        public string DestinationSettlementId => destinationSettlementId ?? string.Empty;
        public string[] PreferredGoods => preferredGoods ?? Array.Empty<string>();
        public float Distance => distance;

        public CCS_TradeRouteDifficulty RouteDifficulty =>
            System.Enum.IsDefined(typeof(CCS_TradeRouteDifficulty), routeDifficulty)
                ? (CCS_TradeRouteDifficulty)routeDifficulty
                : CCS_TradeRouteDifficulty.Unknown;

        public bool IsDiscovered => isDiscovered;

        public bool IsActive => isActive;

        public int UsageCount => usageCount < 0 ? 0 : usageCount;

        public CCS_TradeRouteRiskLevel RiskRating =>
            System.Enum.IsDefined(typeof(CCS_TradeRouteRiskLevel), riskRating)
                ? (CCS_TradeRouteRiskLevel)riskRating
                : CCS_TradeRouteRiskLevel.Unknown;

        public float BaseFreightMultiplier => baseFreightMultiplier < 0f ? 0f : baseFreightMultiplier;

        public float DistanceMultiplier => distanceMultiplier < 0f ? 0f : distanceMultiplier;

        public bool IsValid => !string.IsNullOrWhiteSpace(RouteId);
    }
}
