// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthMarkerType
// CATEGORY: Modules / Settlements / Runtime / VisualGrowth
// PURPOSE: Primitive placeholder marker archetypes for settlement growth visuals.
// PLACEMENT: Used by growth anchors and bootstrap scene setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 — dev-readable placeholders only.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_SettlementVisualGrowthMarkerType
    {
        Unknown = 0,
        CampMarker = 1,
        SupplyCrates = 2,
        SettlementSign = 3,
        TradingSign = 4,
        TradeCrates = 5,
        ServiceHub = 6,
        HitchingRail = 7,
        RoadMarker = 8,
        TownCenter = 9,
        OutpostBoundary = 10,
        FrontierTownPlaceholder = 11,
        EstablishedTownPlaceholder = 12
    }
}
