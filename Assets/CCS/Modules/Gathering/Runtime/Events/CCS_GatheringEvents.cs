// =============================================================================
// SCRIPT: CCS_GatheringEvents
// CATEGORY: Modules / Gathering / Runtime / Events
// PURPOSE: Delegate types for gathering node lifecycle events.
// PLACEMENT: Used by CCS_GatheringService event surface.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: GatheringNodeGathered, GatheringNodeDepleted, GatheringNodeRespawned in 0.9.9.
// =============================================================================

namespace CCS.Modules.Gathering
{
    public delegate void GatheringNodeGatheredHandler(CCS_GatheringEventArgs eventArgs);

    public delegate void GatheringNodeDepletedHandler(CCS_GatheringEventArgs eventArgs);

    public delegate void GatheringNodeRespawnedHandler(CCS_GatheringEventArgs eventArgs);
}
