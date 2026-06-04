using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_FrontierBusinessesValidationRegistration
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Registers frontier businesses foundation validator on editor load.
// PLACEMENT: Auto-loaded by Unity Editor.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.7.0 frontier businesses foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    [InitializeOnLoad]
    public static class CCS_FrontierBusinessesValidationRegistration
    {
        static CCS_FrontierBusinessesValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_FrontierBusinessesFoundationValidationValidator());
        }
    }
}
