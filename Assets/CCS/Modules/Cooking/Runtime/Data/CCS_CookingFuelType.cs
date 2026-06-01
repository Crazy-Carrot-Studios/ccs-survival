// =============================================================================
// SCRIPT: CCS_CookingFuelType
// CATEGORY: Modules / Cooking / Runtime / Data
// PURPOSE: Logical fuel categories accepted by primitive campfire cooking recipes.
// PLACEMENT: Used for validation messaging and future fuel rule expansion.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Recipe fuel resolution uses inventory item IDs (stick and wood).
// =============================================================================

namespace CCS.Modules.Cooking
{
    public enum CCS_CookingFuelType
    {
        None = 0,
        Stick = 1,
        Wood = 2
    }
}
