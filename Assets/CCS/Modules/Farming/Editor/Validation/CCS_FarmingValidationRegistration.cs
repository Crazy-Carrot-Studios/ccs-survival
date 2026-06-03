using CCS.Survival.Editor.Development;

// =============================================================================
// SCRIPT: CCS_FarmingValidationRegistration
// CATEGORY: Modules / Farming / Editor / Validation
// PURPOSE: Registers farming foundation validator on editor load.
// PLACEMENT: Auto-loaded by Unity editor assembly.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.2.0 farming foundation.
// =============================================================================

namespace CCS.Modules.Farming.Editor
{
    public static class CCS_FarmingValidationRegistration
    {
        static CCS_FarmingValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_FarmingFoundationValidationValidator());
        }
    }
}
