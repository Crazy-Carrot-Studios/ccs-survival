// =============================================================================
// SCRIPT: CCS_SurvivalStatType
// CATEGORY: Survival / Runtime / SurvivalCore / Stats
// PURPOSE: Identifiers for survival core stat channels.
// PLACEMENT: Used by stat state, profiles, service, and events.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: No UI or gameplay system dependencies.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    public enum CCS_SurvivalStatType
    {
        Health = 0,
        Stamina = 1,
        Hunger = 2,
        Thirst = 3,
        Temperature = 4,
        Fatigue = 5
    }
}
