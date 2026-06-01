// =============================================================================
// SCRIPT: CCS_SaveEvents
// CATEGORY: Modules / SaveSystem / Runtime / Events
// PURPOSE: Delegate contracts for CCS_SaveService lifecycle events.
// PLACEMENT: Referenced by subscribers outside the save module UI layer.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Event-driven persistence notifications only.
// =============================================================================

namespace CCS.Modules.SaveSystem
{
    public delegate void SaveStartedHandler(CCS_SaveEventArgs eventArgs);

    public delegate void SaveCompletedHandler(CCS_SaveEventArgs eventArgs);

    public delegate void LoadStartedHandler(CCS_SaveEventArgs eventArgs);

    public delegate void LoadCompletedHandler(CCS_SaveEventArgs eventArgs);
}
