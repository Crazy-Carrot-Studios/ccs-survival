using CCS.Survival.Editor.Development;

// =============================================================================
// SCRIPT: CCS_PlayerDeathValidationRegistration
// CATEGORY: Modules / PlayerDeath / Editor / Validation
// PURPOSE: Registers player death validator on the central survival validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: No manual registration required outside this file.
// =============================================================================

namespace CCS.Modules.PlayerDeath.Editor
{
    public static class CCS_PlayerDeathValidationRegistration
    {
        #region Unity Callbacks

        static CCS_PlayerDeathValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_PlayerDeathValidationValidator());
        }

        #endregion
    }
}
