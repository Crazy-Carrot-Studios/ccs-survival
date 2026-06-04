using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TradeRouteProfile
// CATEGORY: Modules / Settlements / Runtime / Profiles
// PURPOSE: Catalog of metadata-only trade routes between frontier settlements.
// PLACEMENT: Assets/CCS/Survival/Profiles/Settlements/
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.3.0 — no transport simulation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [CreateAssetMenu(
        fileName = "CCS_TradeRouteProfile",
        menuName = "CCS/Survival/Settlements/Trade Route Profile")]
    public sealed class CCS_TradeRouteProfile : CCS_SurvivalProfileBase
    {
        [SerializeField] private CCS_TradeRouteDefinition[] tradeRouteDefinitions = new CCS_TradeRouteDefinition[0];

        public CCS_TradeRouteDefinition[] TradeRouteDefinitions =>
            tradeRouteDefinitions ?? new CCS_TradeRouteDefinition[0];
    }
}
