using CCS.Survival.Editor.Development;

// =============================================================================
// SCRIPT: CCS_UpkeepValidationRegistration
// CATEGORY: Modules / Upkeep / Editor / Validation
// PURPOSE: Registers upkeep foundation validator on editor load.
// PLACEMENT: Auto-loaded by Unity editor assembly.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.5.0 tax and upkeep foundation.
// =============================================================================

namespace CCS.Modules.Upkeep.Editor
{
    public static class CCS_UpkeepValidationRegistration
    {
        static CCS_UpkeepValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_UpkeepFoundationValidationValidator());
        }
    }
}
