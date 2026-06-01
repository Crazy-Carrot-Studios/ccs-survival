using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_FishingValidationRegistration
// CATEGORY: Modules / Fishing / Editor / Validation
// PURPOSE: Registers fishing validator on the central survival validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No manual registration required outside this file.
// =============================================================================

namespace CCS.Modules.Fishing.Editor
{
    [InitializeOnLoad]
    public static class CCS_FishingValidationRegistration
    {
        static CCS_FishingValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_FishingValidationValidator());
        }
    }
}
