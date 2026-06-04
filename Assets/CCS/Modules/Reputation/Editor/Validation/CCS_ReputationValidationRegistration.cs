using CCS.Survival.Editor.Development;

// =============================================================================
// SCRIPT: CCS_ReputationValidationRegistration
// CATEGORY: Modules / Reputation / Editor / Validation
// PURPOSE: Registers reputation foundation validator on editor load.
// PLACEMENT: Auto-loaded by Unity editor assembly.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 reputation and service trust foundation.
// =============================================================================

namespace CCS.Modules.Reputation.Editor
{
    public static class CCS_ReputationValidationRegistration
    {
        static CCS_ReputationValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_ReputationFoundationValidationValidator());
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_ServiceAccessFoundationValidationValidator());
        }
    }
}
