using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_GatheringValidationRegistration
// CATEGORY: Modules / Gathering / Editor / Validation
// PURPOSE: Registers gathering validator on the central survival validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: No manual registration required outside this file.
// =============================================================================

namespace CCS.Modules.Gathering.Editor
{
    [InitializeOnLoad]
    public static class CCS_GatheringValidationRegistration
    {
        #region Unity Callbacks

        static CCS_GatheringValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_GatheringValidationValidator());
        }

        #endregion
    }
}
