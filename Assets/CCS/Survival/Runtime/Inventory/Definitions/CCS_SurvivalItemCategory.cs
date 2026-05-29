// =============================================================================
// SCRIPT: CCS_SurvivalItemCategory
// CATEGORY: Survival / Runtime / Inventory / Definitions
// PURPOSE: High-level category tags for survival item definitions and filtering.
// PLACEMENT: Runtime enum. Used by CCS_SurvivalItemDefinition assets.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Save-stable item IDs remain authoritative; category is authoring metadata only.
// =============================================================================

namespace CCS.Survival.Inventory
{
    public enum CCS_SurvivalItemCategory
    {
        Resource = 0,
        Food = 1,
        Water = 2,
        Tool = 3,
        Material = 4,
        Quest = 5,
        Misc = 6
    }
}
