// =============================================================================
// SCRIPT: CCS_StorageEvents
// CATEGORY: Modules / Storage / Runtime / Events
// PURPOSE: Event delegate declarations for storage container lifecycle notifications.
// PLACEMENT: Used by CCS_StorageService and playtest harness listeners.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.2 storage container foundation.
// =============================================================================

namespace CCS.Modules.Storage
{
    public delegate void StorageContainerOpenedHandler(CCS_StorageEventArgs eventArgs);

    public delegate void StorageContainerClosedHandler(CCS_StorageEventArgs eventArgs);

    public delegate void StorageItemAddedHandler(CCS_StorageEventArgs eventArgs);

    public delegate void StorageItemRemovedHandler(CCS_StorageEventArgs eventArgs);

    public delegate void StorageStateRestoredHandler(CCS_StorageEventArgs eventArgs);
}
