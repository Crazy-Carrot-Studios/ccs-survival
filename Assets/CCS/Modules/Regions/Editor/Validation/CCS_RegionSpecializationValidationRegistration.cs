using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_RegionSpecializationValidationRegistration
// CATEGORY: Modules / Regions / Editor / Validation
// PURPOSE: Registers regional specialization validator on the central validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.1.0 regional specialization foundation.
// =============================================================================

namespace CCS.Modules.Regions.Editor
{
    [InitializeOnLoad]
    public static class CCS_RegionSpecializationValidationRegistration
    {
        static CCS_RegionSpecializationValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_RegionSpecializationFoundationValidationValidator());
        }
    }
}
