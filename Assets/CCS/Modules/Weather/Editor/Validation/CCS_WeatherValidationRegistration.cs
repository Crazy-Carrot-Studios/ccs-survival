using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_WeatherValidationRegistration
// CATEGORY: Modules / Weather / Editor / Validation
// PURPOSE: Registers weather validator with the survival validation pipeline.
// PLACEMENT: Auto-loaded at editor startup via InitializeOnLoad.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: No manual scene scanning. Pipeline deduplicates by ValidatorId.
// =============================================================================

namespace CCS.Modules.Weather.Editor
{
    [InitializeOnLoad]
    public static class CCS_WeatherValidationRegistration
    {
        static CCS_WeatherValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_WeatherValidationValidator());
        }
    }
}
