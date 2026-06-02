using CCS.Survival.Editor.Development;

// =============================================================================
// SCRIPT: CCS_FrontierStarterProgressionValidationRegistration
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Registers frontier starter progression validator on the survival validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    [UnityEditor.InitializeOnLoad]
    public static class CCS_FrontierStarterProgressionValidationRegistration
    {
        static CCS_FrontierStarterProgressionValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_FrontierStarterProgressionValidationValidator());
        }
    }
}
