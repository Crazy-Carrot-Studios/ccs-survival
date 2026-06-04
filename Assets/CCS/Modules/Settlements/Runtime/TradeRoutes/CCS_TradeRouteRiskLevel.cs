// =============================================================================
// SCRIPT: CCS_TradeRouteRiskLevel
// CATEGORY: Modules / Settlements / Runtime / TradeRoutes
// PURPOSE: Risk band for trade route freight reward scaling and future encounter tuning.
// PLACEMENT: Referenced by CCS_TradeRouteDefinition and reward modifier utility.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.5.0 — Dangerous/Severe reserved for future encounter milestones.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_TradeRouteRiskLevel
    {
        Unknown = 0,
        Safe = 1,
        Low = 2,
        Moderate = 3,
        Dangerous = 4,
        Severe = 5
    }
}
