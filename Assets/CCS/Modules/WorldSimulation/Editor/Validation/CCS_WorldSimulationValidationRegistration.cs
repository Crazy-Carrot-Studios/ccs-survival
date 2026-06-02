using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_WorldSimulationValidationRegistration
// CATEGORY: Modules / WorldSimulation / Editor / Validation
// PURPOSE: Registers world simulation validator on the central survival validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.WorldSimulation.Editor
{
    [InitializeOnLoad]
    public static class CCS_WorldSimulationValidationRegistration
    {
        static CCS_WorldSimulationValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_WorldSimulationValidationValidator());
        }
    }
}
