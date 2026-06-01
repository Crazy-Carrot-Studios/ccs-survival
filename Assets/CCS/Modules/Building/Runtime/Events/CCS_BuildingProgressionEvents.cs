// =============================================================================
// SCRIPT: CCS_BuildingProgressionEvents
// CATEGORY: Modules / Building / Runtime / Events
// PURPOSE: Delegate contracts for building progression recipe events.
// PLACEMENT: Referenced by CCS_BuildingRecipeService and HUD listeners.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.0 building progression foundation.
// =============================================================================

namespace CCS.Modules.Building
{
    public delegate void BuildingRecipeValidatedHandler(CCS_BuildingProgressionEventArgs eventArgs);

    public delegate void BuildingRecipeFailedHandler(CCS_BuildingProgressionEventArgs eventArgs);

    public delegate void BuildingPiecePlacedHandler(CCS_BuildingProgressionEventArgs eventArgs);

    public delegate void BuildingResourcesConsumedHandler(CCS_BuildingProgressionEventArgs eventArgs);
}
