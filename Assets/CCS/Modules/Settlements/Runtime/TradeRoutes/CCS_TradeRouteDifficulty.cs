// =============================================================================
// SCRIPT: CCS_TradeRouteDifficulty
// CATEGORY: Modules / Settlements / Runtime / TradeRoutes
// PURPOSE: Placeholder difficulty band for trade route metadata and freight tuning.
// PLACEMENT: Referenced by CCS_TradeRouteDefinition and snapshots.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.4.0 — no encounter simulation yet.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_TradeRouteDifficulty
    {
        Unknown = 0,
        Easy = 1,
        Moderate = 2,
        Hard = 3
    }
}
