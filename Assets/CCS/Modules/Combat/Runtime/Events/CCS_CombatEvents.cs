// =============================================================================
// SCRIPT: CCS_CombatEvents
// CATEGORY: Modules / Combat / Runtime / Events
// PURPOSE: Delegate types for primitive combat service events.
// PLACEMENT: Used by CCS_CombatService and HUD presentation bindings.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: WildlifeDamaged and WildlifeKilled only in 0.9.8 foundation.
// =============================================================================

namespace CCS.Modules.Combat
{
    public delegate void WildlifeDamagedHandler(CCS_CombatEventArgs eventArgs);

    public delegate void WildlifeKilledHandler(CCS_CombatEventArgs eventArgs);
}
