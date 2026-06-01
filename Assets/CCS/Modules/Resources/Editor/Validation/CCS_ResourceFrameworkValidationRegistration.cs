using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_ResourceFrameworkValidationRegistration
// CATEGORY: Modules / Resources / Editor / Validation
// PURPOSE: Registers frontier resource framework validator on the survival validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Resources.Editor
{
    [InitializeOnLoad]
    public static class CCS_ResourceFrameworkValidationRegistration
    {
        static CCS_ResourceFrameworkValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_ResourceFrameworkValidationValidator());
        }
    }
}
