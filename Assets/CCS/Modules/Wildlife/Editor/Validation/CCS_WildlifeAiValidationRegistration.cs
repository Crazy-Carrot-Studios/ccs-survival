using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_WildlifeAiValidationRegistration
// CATEGORY: Modules / Wildlife / Editor / Validation
// PURPOSE: Registers wildlife AI validator on the central survival validation pipeline.
// PLACEMENT: Auto-loaded at editor startup via InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No manual registration required outside this file.
// =============================================================================

namespace CCS.Modules.Wildlife.Editor
{
    [InitializeOnLoad]
    public static class CCS_WildlifeAiValidationRegistration
    {
        static CCS_WildlifeAiValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_WildlifeAiValidationValidator());
        }
    }
}
