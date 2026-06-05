using CCS.Survival.Editor.Development;

// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthValidationRegistration
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Registers settlement visual growth foundation validator on editor load.
// PLACEMENT: Auto-registered with CCS_SurvivalValidationPipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 settlement visual growth foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public static class CCS_SettlementVisualGrowthValidationRegistration
    {
        static CCS_SettlementVisualGrowthValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_SettlementVisualGrowthFoundationValidationValidator());
        }
    }
}
