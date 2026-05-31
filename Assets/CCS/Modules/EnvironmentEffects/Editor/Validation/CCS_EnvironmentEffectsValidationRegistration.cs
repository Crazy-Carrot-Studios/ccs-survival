using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_EnvironmentEffectsValidationRegistration
// CATEGORY: Modules / EnvironmentEffects / Editor / Validation
// PURPOSE: Registers environment effects validator with the survival validation pipeline.
// PLACEMENT: Auto-loaded at editor startup via InitializeOnLoad.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: No manual scene scanning. Pipeline deduplicates by ValidatorId.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects.Editor
{
    [InitializeOnLoad]
    public static class CCS_EnvironmentEffectsValidationRegistration
    {
        static CCS_EnvironmentEffectsValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_EnvironmentEffectsValidationValidator());
        }
    }
}
