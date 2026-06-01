// =============================================================================
// SCRIPT: CCS_PlayerDeathEvents
// CATEGORY: Modules / PlayerDeath / Runtime / Events
// PURPOSE: Delegate contracts for player death and respawn events.
// PLACEMENT: Referenced by HUD and diagnostics subscribers.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Event-driven only; no singleton shortcuts.
// =============================================================================

namespace CCS.Modules.PlayerDeath
{
    public delegate void PlayerDiedHandler(CCS_PlayerDeathEventArgs eventArgs);

    public delegate void PlayerRespawnedHandler(CCS_PlayerDeathEventArgs eventArgs);
}
