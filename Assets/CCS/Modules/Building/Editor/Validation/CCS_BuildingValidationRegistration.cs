using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_BuildingValidationRegistration
// CATEGORY: Modules / Building / Editor / Validation
// PURPOSE: Registers building validator with the survival validation pipeline.
// PLACEMENT: Auto-loaded at editor startup via InitializeOnLoad.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: No manual scene scanning. Pipeline deduplicates by ValidatorId.
// =============================================================================

namespace CCS.Modules.Building.Editor
{
    [InitializeOnLoad]
    public static class CCS_BuildingValidationRegistration
    {
        static CCS_BuildingValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_BuildingValidationValidator());
        }
    }
}
