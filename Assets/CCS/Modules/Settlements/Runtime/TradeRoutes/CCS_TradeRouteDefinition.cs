using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TradeRouteDefinition
// CATEGORY: Modules / Settlements / Runtime / TradeRoutes
// PURPOSE: Metadata-only trade route between frontier settlements.
// PLACEMENT: Assets/CCS/Survival/Content/TradeRoutes/
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.4.0 — metadata + discovery/active defaults; no transport simulation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [CreateAssetMenu(
        fileName = "CCS_TradeRouteDefinition",
        menuName = "CCS/Survival/Settlements/Trade Route Definition")]
    public sealed class CCS_TradeRouteDefinition : ScriptableObject
    {
        [SerializeField] private string routeId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private string originSettlementId = string.Empty;
        [SerializeField] private string destinationSettlementId = string.Empty;
        [SerializeField] private string[] preferredGoods = new string[0];
        [SerializeField] private float distance = 1f;

        [Tooltip("Placeholder route difficulty band for future encounter tuning.")]
        [SerializeField] private CCS_TradeRouteDifficulty routeDifficulty = CCS_TradeRouteDifficulty.Moderate;

        [Tooltip("Whether this route starts discovered when no save state exists.")]
        [SerializeField] private bool startsDiscovered;

        [Tooltip("Whether this route starts active when discovered and no save state exists.")]
        [SerializeField] private bool startsActive = true;

        public string RouteId => routeId ?? string.Empty;
        public string DisplayName => displayName ?? string.Empty;
        public string OriginSettlementId => originSettlementId ?? string.Empty;
        public string DestinationSettlementId => destinationSettlementId ?? string.Empty;
        public string[] PreferredGoods => preferredGoods ?? new string[0];
        public float Distance => distance < 0f ? 0f : distance;

        public CCS_TradeRouteDifficulty RouteDifficulty => routeDifficulty;

        public bool StartsDiscovered => startsDiscovered;

        public bool StartsActive => startsActive;
    }
}
