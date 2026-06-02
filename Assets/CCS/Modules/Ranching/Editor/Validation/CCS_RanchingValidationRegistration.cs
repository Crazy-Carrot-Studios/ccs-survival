using CCS.Survival.Editor.Development;

// =============================================================================
// SCRIPT: CCS_RanchingValidationRegistration
// CATEGORY: Modules / Ranching / Editor / Validation
// PURPOSE: Registers ranching foundation validator on editor load.
// PLACEMENT: Auto-loaded by Unity editor assembly.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.1.0 ranching foundation.
// =============================================================================

namespace CCS.Modules.Ranching.Editor
{
    public static class CCS_RanchingValidationRegistration
    {
        static CCS_RanchingValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_RanchingFoundationValidationValidator());
        }
    }
}
