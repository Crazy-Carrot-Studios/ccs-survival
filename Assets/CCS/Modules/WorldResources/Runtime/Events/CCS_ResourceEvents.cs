// =============================================================================
// SCRIPT: CCS_ResourceEvents
// CATEGORY: Modules / WorldResources / Runtime / Events
// PURPOSE: Event name constants and delegate contracts for world resource systems.
// PLACEMENT: Instance events on harvest/respawn services document contracts here.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Subscribers react to harvest flow without UI or interaction visual coupling.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public static class CCS_ResourceEvents
    {
        public const string HarvestStartedEventName = "WorldResource.HarvestStarted";
        public const string HarvestCompletedEventName = "WorldResource.HarvestCompleted";
        public const string HarvestFailedEventName = "WorldResource.HarvestFailed";
        public const string ResourceDepletedEventName = "WorldResource.Depleted";
        public const string ResourceRespawnedEventName = "WorldResource.Respawned";
    }

    public delegate void HarvestStartedHandler(CCS_ResourceEventArgs eventArgs);

    public delegate void HarvestCompletedHandler(CCS_ResourceEventArgs eventArgs);

    public delegate void HarvestFailedHandler(CCS_ResourceEventArgs eventArgs);

    public delegate void ResourceDepletedHandler(CCS_ResourceEventArgs eventArgs);

    public delegate void ResourceRespawnedHandler(CCS_ResourceEventArgs eventArgs);
}
