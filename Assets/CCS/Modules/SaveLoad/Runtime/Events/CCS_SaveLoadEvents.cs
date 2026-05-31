// =============================================================================
// SCRIPT: CCS_SaveLoadEvents
// CATEGORY: Modules / SaveLoad / Runtime / Events
// PURPOSE: Delegate contracts for save/load service events.
// PLACEMENT: Referenced by CCS_SaveLoadService and future HUD listeners.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Event-driven integration without direct UI coupling.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    public delegate void SaveStartedHandler(CCS_SaveLoadEventArgs eventArgs);

    public delegate void SaveCompletedHandler(CCS_SaveLoadEventArgs eventArgs);

    public delegate void LoadStartedHandler(CCS_SaveLoadEventArgs eventArgs);

    public delegate void LoadCompletedHandler(CCS_SaveLoadEventArgs eventArgs);

    public delegate void SaveFailedHandler(CCS_SaveLoadEventArgs eventArgs);

    public delegate void LoadFailedHandler(CCS_SaveLoadEventArgs eventArgs);
}
