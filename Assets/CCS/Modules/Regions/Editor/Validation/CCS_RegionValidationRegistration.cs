using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_RegionValidationRegistration
// CATEGORY: Modules / Regions / Editor / Validation
// PURPOSE: Registers region validator on the central survival validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Regions.Editor
{
    [InitializeOnLoad]
    public static class CCS_RegionValidationRegistration
    {
        static CCS_RegionValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_FrontierRegionValidationValidator());
        }
    }
}
