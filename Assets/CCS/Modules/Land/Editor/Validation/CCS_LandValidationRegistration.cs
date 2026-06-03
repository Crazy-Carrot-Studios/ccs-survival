using CCS.Survival.Editor.Development;

// =============================================================================
// SCRIPT: CCS_LandValidationRegistration
// CATEGORY: Modules / Land / Editor / Validation
// PURPOSE: Registers land ownership foundation validator on editor load.
// PLACEMENT: Auto-loaded by Unity editor assembly.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.3.0 land ownership foundation.
// =============================================================================

namespace CCS.Modules.Land.Editor
{
    public static class CCS_LandValidationRegistration
    {
        static CCS_LandValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_LandOwnershipFoundationValidationValidator());
        }
    }
}
