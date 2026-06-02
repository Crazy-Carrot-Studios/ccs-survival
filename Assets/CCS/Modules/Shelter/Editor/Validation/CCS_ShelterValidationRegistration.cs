using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_ShelterValidationRegistration
// CATEGORY: Modules / Shelter / Editor / Validation
// PURPOSE: Registers shelter validator with the survival validation pipeline.
// PLACEMENT: Auto-loaded at editor startup via InitializeOnLoad.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: No manual scene scanning. Pipeline deduplicates by ValidatorId.
// =============================================================================

namespace CCS.Modules.Shelter.Editor
{
    [InitializeOnLoad]
    public static class CCS_ShelterValidationRegistration
    {
        static CCS_ShelterValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_ShelterValidationValidator());
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_FrontierShelterValidationValidator());
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_FrontierHomesteadValidationValidator());
        }
    }
}
