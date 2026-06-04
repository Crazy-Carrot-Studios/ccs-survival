// =============================================================================
// SCRIPT: CCS_TradeRouteSnapshot
// CATEGORY: Modules / Settlements / Runtime / TradeRoutes
// PURPOSE: Serializable trade route metadata for save payloads and queries.
// PLACEMENT: Used by save system and future trade route services.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.4.0 — includes discovery, active, and usage runtime fields.
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

        public bool IsValid => !string.IsNullOrWhiteSpace(RouteId);
    }
}
