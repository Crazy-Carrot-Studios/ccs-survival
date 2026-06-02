using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_EconomyValidationRegistration
// CATEGORY: Modules / Economy / Editor / Validation
// PURPOSE: Registers economy validator on the central survival validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No manual registration required outside this file.
// =============================================================================

namespace CCS.Modules.Economy.Editor
{
    [InitializeOnLoad]
    public static class CCS_EconomyValidationRegistration
    {
        static CCS_EconomyValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_EconomyValidationValidator());
        }
    }
}
