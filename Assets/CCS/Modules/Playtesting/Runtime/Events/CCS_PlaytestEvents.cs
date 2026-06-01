// =============================================================================
// SCRIPT: CCS_PlaytestEvents
// CATEGORY: Modules / Playtesting / Runtime / Events
// PURPOSE: Delegate contracts for manual playtest harness events.
// PLACEMENT: Referenced by dev HUD and diagnostics listeners.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Event-driven only; no global singleton shortcuts.
// =============================================================================

namespace CCS.Modules.Playtesting
{
    public delegate void PlaytestStepChangedHandler(CCS_PlaytestEventArgs eventArgs);

    public delegate void PlaytestStepPassedHandler(CCS_PlaytestEventArgs eventArgs);

    public delegate void PlaytestStepFailedHandler(CCS_PlaytestEventArgs eventArgs);

    public delegate void PlaytestResetHandler();
}
