// =============================================================================
// SCRIPT: CCS_CookingStationType
// CATEGORY: Modules / Cooking / Runtime / Data
// PURPOSE: Identifies world cooking station archetypes for profile and validation rules.
// PLACEMENT: Serialized on CCS_CookingStation and CCS_CookingProfile campfire settings.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Campfire-only foundation for 1.0.0 milestone.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public enum CCS_CookingStationType
    {
        None = 0,
        Campfire = 1
    }
}
